using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using LSOmni.Common.Util;

namespace LSOmni.DataAccess.Firebase
{
	public class FirebaseCustom
	{
        private FirebaseApp app;
        private FirebaseMessaging messaging;
        private static LSLogger logger = new LSLogger();
        public FirebaseCustom()
		{
            app = FirebaseApp.DefaultInstance;
            if (app == null)
            {
                app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.GetApplicationDefault()
                                 .CreateScoped("https://www.googleapis.com/auth/firebase.messaging"),
                    ServiceAccountId = "firebase-adminsdk-drcwr@spg-pn1.iam.gserviceaccount.com",
                    ProjectId = "spg-pn1",
                });
            }
            messaging = FirebaseMessaging.GetMessaging(app);
            logger.Debug(string.Empty, "ServiceAccountId:{0} ProjectId:{1}", app.Options.ServiceAccountId, app.Options.ProjectId);
        }

        public string SendPushNotificationToTopic(string topic, string title, string message, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);
            string messageID = "";
            //use topic is reg token for now
            var registrationToken = topic;
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

    }
}
