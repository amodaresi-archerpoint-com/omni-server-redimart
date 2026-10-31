using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Http;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Loyalty.Notifications;

namespace LSOmni.DataAccess.Firebase
{
	public class FirebaseCustom
	{
        private FirebaseApp app;
        private FirebaseMessaging messaging;
        private static LSLogger logger = new LSLogger();
        // Own named Firebase app (instead of FirebaseApp.DefaultInstance), so our options - including the
        // HTTP handler below - are always used, even if LS code (SpgNotificationBLL) created the default app first.
        private const string RediMartAppName = "RediMartFirebaseCustom";
        private static readonly object appLock = new object();
        public FirebaseCustom()
		{
            lock (appLock)
            {
                app = FirebaseApp.GetInstance(RediMartAppName);
                if (app == null)
                {
                    app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                                     .CreateScoped("https://www.googleapis.com/auth/firebase.messaging"),
                        ServiceAccountId = "firebase-adminsdk-zq9fs@redi-mart-spg-app.iam.gserviceaccount.com",
                        ProjectId = "redi-mart-spg-app",
                        HttpClientFactory = new FirebaseHttpClientFactory(),
                    }, RediMartAppName);
                }
            }
            messaging = FirebaseMessaging.GetMessaging(app);
            logger.Debug(string.Empty, "ServiceAccountId:{0} ProjectId:{1}", app.Options.ServiceAccountId, app.Options.ProjectId);
        }

        public string SendPushNotificationToTopic(string topic, string title, string message, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            string messageID = "";

            var messageObject = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = message,
                },
                Topic = topic,
            };
            messageID = messaging.SendAsync(messageObject).Result;
            logger.StatisticEndSub(ref stat, index);
            return messageID;
        }

        public virtual string SendPushNotificationToToken(string token, string title, string message, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            string messageID = "";
            //use topic is reg token for now
            var registrationToken = token;
            var messageObject = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = message,
                },
                Token = registrationToken,
            };
            messageID = messaging.SendAsync(messageObject).Result;
            logger.StatisticEndSub(ref stat, index);
            return messageID;
        }

        public bool SubscribeTokenToTopic(string token, string topic, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            var tokens = new List<string>() { token };
            var response = messaging.SubscribeToTopicAsync(tokens, topic).Result;
            logger.StatisticEndSub(ref stat, index);
            return (response.SuccessCount == 1) ? true : false;
        }

        public TopicSubscriptionResult SubscribeTokensToTopic(List<string> tokens, string topic, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            var response = messaging.SubscribeToTopicAsync(tokens, topic).Result;
            logger.StatisticEndSub(ref stat, index);
            return BuildTopicSubscriptionResult(tokens, response);
        }

        public TopicSubscriptionResult UnsubscribeTokensFromTopic(List<string> tokens, string topic, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            var response = messaging.UnsubscribeFromTopicAsync(tokens, topic).Result;
            logger.StatisticEndSub(ref stat, index);
            return BuildTopicSubscriptionResult(tokens, response);
        }

        private TopicSubscriptionResult BuildTopicSubscriptionResult(List<string> tokens, TopicManagementResponse response)
        {
            var errors = new List<TopicSubscriptionError>();
            foreach (var error in response.Errors)
            {
                errors.Add(new TopicSubscriptionError
                {
                    Index = error.Index,
                    Token = tokens[error.Index],
                    Reason = error.Reason,
                });
            }
            return new TopicSubscriptionResult
            {
                SuccessCount = response.SuccessCount,
                FailureCount = response.FailureCount,
                Errors = errors,
            };
        }

    }

    /// <summary>
    /// Redi-Mart: HTTP client factory for the Firebase Admin SDK. Wraps the default handler with
    /// <see cref="FirebaseHttpLoggingHandler"/> so every call to Google is logged and sent as HTTP/1.1.
    /// </summary>
    internal sealed class FirebaseHttpClientFactory : HttpClientFactory
    {
        protected override HttpMessageHandler CreateHandler(CreateHttpClientArgs args)
        {
            return new FirebaseHttpLoggingHandler(base.CreateHandler(args));
        }
    }

    /// <summary>
    /// Redi-Mart: logs each Firebase HTTP call (target, requested HTTP version, status, error body) and
    /// any exception thrown before a response arrives - FirebaseAdmin reports those only as "unknown-error".
    /// Forces HTTP/1.1: the .NET Framework HTTP stack (HttpWebRequest) does not support HTTP/2.
    /// </summary>
    internal sealed class FirebaseHttpLoggingHandler : DelegatingHandler
    {
        private static LSLogger logger = new LSLogger();
        private static readonly Regex tokenInPath = new Regex(@"(/registrations/)([^/]{8})[^/]*", RegexOptions.Compiled);

        public FirebaseHttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestedVersion = request.Version == null ? "?" : request.Version.ToString();
            request.Version = HttpVersion.Version11;
            string target = request.Method + " " + MaskToken(request.RequestUri);

            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                logger.Debug(string.Empty, "Firebase HTTP {0} (requested HTTP/{1}, sent HTTP/1.1) -> {2} {3}",
                    target, requestedVersion, (int)response.StatusCode, response.ReasonPhrase);
                if (!response.IsSuccessStatusCode && response.Content != null)
                {
                    // Buffer first, so the SDK can still read the content after us.
                    await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                    string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    logger.Warn(string.Empty, "Firebase HTTP {0} -> {1} {2}, body: {3}",
                        target, (int)response.StatusCode, response.ReasonPhrase, body);
                }
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(string.Empty, ex, "Firebase HTTP {0} (requested HTTP/{1}) failed before a response was received", target, requestedVersion);
                throw;
            }
        }

        private static string MaskToken(Uri uri)
        {
            if (uri == null)
                return string.Empty;
            // Registration tokens are part of the path for topic management - log only their first 8 characters.
            return tokenInPath.Replace(uri.GetLeftPart(UriPartial.Path), "$1$2...") + uri.Query;
        }
    }
}
