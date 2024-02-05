using System;
using LSOmni.Common.Util;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using LSOmni.DataAccess.BOConnection.CentralPre.Dal;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.Domain.DataModel.Base;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.BOConnection.CentralPre
{
    public class NavCustom : NavBase, ICustomLoyBO
    {
        public NavCustom(BOConfiguration config) : base(config)
        {
        }

        public virtual string MyCustomFunction(string data)
        {
            bool usedatabase = false;

            // using database lookup
            if (usedatabase)
            {
                MyCustomRepository rep = new MyCustomRepository(config, LSCVersion);
                return rep.GetMyData(data);
            }
            else
            {
                // using Web Service Lookup
                return LSCentralWSBase.MyCustomFunction(data);
            }
        }

        #region Altria Phase I - Offer Impression Logging
        private const string CreateAltriaLogEntryRequestId = "CREATE_ALTRIA_LOG_ENTRY";
        private int base64ConversionMinLength = 1024 * 100; //50KB 75KB  minimum length to base64 conversion
        private string wsEncoding = "utf-8"; //default to utf-8
        private static readonly object Locker = new object();

        enum ActivityTypeEnum : ushort
        {
            None = 1,
            Impression = 2
        }
        enum ChannelTypeEnum : ushort
        {
            None = 1,
            Email = 2,
            App = 3,
            Notification = 4
        }

        public string CreateAltriaLogEntryRequestXML(string storeId, string offerId, string cardId, int activityType, int channelType)
        {
            /*
              &lt;Request&gt; 
                &lt;Request_ID&gt;CREATE_ALTRIA_LOG_ENTRY&lt;/Request_ID&gt;
                &lt;Request_Body&gt;
                    &lt;Store_No&gt;123&lt;/Store_No&gt;
                    &lt;Offer_No&gt;RD000087&lt;/Offer_No&gt;
                    &lt;Member_Card_No&gt;000000233209&lt;/Member_Card_No&gt;
                    &lt;Activity_Type&gt;Impression&lt;/Activity_Type&gt;
                    &lt;Channel_Type&gt;App&lt;/Channel_Type&gt;
                &lt;/Request_Body&gt;
              &lt;/Request&gt;
            */

            ActivityTypeEnum activityTypeEnum = (ActivityTypeEnum)activityType;
            ChannelTypeEnum channelTypeEnum = (ChannelTypeEnum)channelType;

            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", wsEncoding, "no");
            document.AppendChild(declaration);

            XmlElement request = document.CreateElement("Request");
            document.AppendChild(request);

            XmlElement requestId = document.CreateElement("Request_ID");
            requestId.InnerText = CreateAltriaLogEntryRequestId;
            request.AppendChild(requestId);

            XmlElement requestBody = document.CreateElement("Request_Body");
            request.AppendChild(requestBody);

            XmlElement storeNoElement = document.CreateElement("Store_No");
            storeNoElement.InnerText = storeId;
            requestBody.AppendChild(storeNoElement);

            XmlElement offerNoElement = document.CreateElement("Offer_No");
            offerNoElement.InnerText = offerId;
            requestBody.AppendChild(offerNoElement);

            XmlElement memberCardNoElement = document.CreateElement("Member_Card_No");
            memberCardNoElement.InnerText = cardId;
            requestBody.AppendChild(memberCardNoElement);

            XmlElement activityTypeElement = document.CreateElement("Activity_Type");
            activityTypeElement.InnerText = activityTypeEnum.ToString();
            requestBody.AppendChild(activityTypeElement);

            XmlElement channelTypeElement = document.CreateElement("Channel_Type");
            channelTypeElement.InnerText = channelTypeEnum.ToString();
            requestBody.AppendChild(channelTypeElement);

            return document.OuterXml;
        }

        public void AltriaLogEntryCreate(string storeId, string offerId, string cardId, int activityType, int channelType)
        {
            //storeId will be optional since SPG does not require store to be chosen for offers
            //if (storeId.Length == 0)
            if (offerId.Length == 0)
                throw new ArgumentException("Mandatory field is empty", "offerId");
            if (cardId.Length == 0)
                throw new ArgumentException("Mandatory field is empty", "cardId");
            if (activityType == 0)
                throw new ArgumentException("Mandatory field is empty", "activityType");
            if (channelType == 0)
                throw new ArgumentException("Mandatory field is empty", "channelType");

            //if (navWS == null)
            //{
            //LSOmni.DataAccess.BOConnection.NavCommon. NavXml navXml = new NavXml();
            string xmlRequest = CreateAltriaLogEntryRequestXML(storeId, offerId, cardId, activityType, channelType);
            logger.Debug(config.LSKey.Key, "AltriaLogEntryCreate Request - " + xmlRequest);
            string xmlResponse = RunOperation(xmlRequest);
            logger.Debug(config.LSKey.Key, "AltriaLogEntryCreate Response - " + xmlResponse);
            HandleResponseCode(ref xmlResponse, new string[] { "1010", "1012" }); //1010 means no altria data. 1012 no altria offer id. it is fine to error here.
            //}
        }

        protected string HandleResponseCode(ref string xmlResponse, string[] codesToHandle = null, bool useMsgText = false)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string responseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            if (responseCode != "0000")
            {
                string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
                string navResponseText = ParseResponseText(doc.GetElementsByTagName("Response_Text"));

                StatusCode statusCode = MapResponseToStatusCode(navResponseId, responseCode);
                //string msg = string.Format("navResponseCode: {0}-{1}  [StatusCode: {2}]", responseCode, navResponseText, statusCode.ToString());
                string msg = string.Format("navResponseCode: {0}-{1}]", responseCode, navResponseText);
                logger.Error(config.LSKey.Key, msg);

                if (codesToHandle != null && codesToHandle.Length > 0)
                {
                    foreach (string code in codesToHandle)
                    {
                        if (useMsgText)
                        {
                            if (navResponseText.Contains(code))
                                return responseCode;

                            continue;
                        }

                        //expected return codes, so don't throw unexpected exception, rather return the known codes to client  
                        if (code.Equals(responseCode))
                            return code;
                    }
                }
                throw new LSOmniServiceException(statusCode, msg);
            }
            return string.Empty;
        }

        protected StatusCode MapResponseToStatusCode(string navResponseId, string navCode)
        {
            //mapping response code from NAV to LSOmni, but sometimes the same navCode is used for different navResponseId
            // so need to check them both - sometimes
            navResponseId = navResponseId.ToUpper().Trim();
            StatusCode statusCode = StatusCode.Error; //default to Error
            switch (navCode)
            {
                case "0000":
                    statusCode = StatusCode.OK;
                    break;

                default:
                    statusCode = StatusCode.GeneralErrorCode;
                    break;
            }
            return statusCode;
        }

        private string GetRequestID(ref string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
        }

        protected string ParseResponseCode(XmlNodeList responseCode)
        {
            XmlNode node = responseCode.Item(0);
            return node.InnerText;
        }
        protected string ParseResponseText(XmlNodeList responseText)
        {
            XmlNode node = responseText.Item(0);
            return node.InnerText;
        }

        private void Base64StringConvertion(ref string xmlRequest)
        {
            string base64String = Convert.ToBase64String(new UTF8Encoding().GetBytes(xmlRequest));
            //Don't want to load hundreds of KB in xdoc just to get the requestId
            //XDocument doc = XDocument.Parse(xmlRequest); //to get the requstId
            //string reqId = doc.Element("Request").Element("Request_ID").Value;
            int first = xmlRequest.IndexOf("Request_ID>") + "Request_ID>".Length;
            int last = xmlRequest.LastIndexOf("</Request_ID");
            string reqId = xmlRequest.Substring(first, last - first);

            XDocument doc64 = new XDocument(new XDeclaration("1.0", "utf-8", "no"));
            XElement root =
                            new XElement("Request", new XAttribute("Encoded", "Base64"),
                                new XElement("Request_ID", reqId),
                                new XElement("Encoded_Request", base64String)
                            );
            ;
            doc64.Add(root);
            xmlRequest = doc64.ToString();
            /*
            <Request Encoded="Base64">
                <Request_ID>TEST_CONNECTION</Request_ID>
                <Encoded_Request>xxxx yy
                </Encoded_Request>
            </Request>
            */

        }

        protected string GetResponseCode(ref string xmlResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string navResponseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            return navResponseCode;
        }

        protected void ExecuteWebRequest(ref string xmlRequest, ref string xmlResponse, bool useQuery)
        {
            int TimeoutSec = 0;
            string timeout = config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOTimeout.ToString()).Value;
            BOConnection.PreCommon.NavWebReference.RetailWebServices wsToUse = new BOConnection.PreCommon.NavWebReference.RetailWebServices();

            wsToUse.Url = config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUrl.ToString()).Value;
            wsToUse.Timeout = (timeout == null ? 20 : ConvertTo.SafeInt(timeout)) * 1000;  //millisecs,  60 seconds
            wsToUse.PreAuthenticate = true;
            wsToUse.AllowAutoRedirect = true;
            wsToUse.Credentials = new System.Net.NetworkCredential(
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUser.ToString()).Value,
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOPassword.ToString()).Value);
            try
            {
                //first time comes in at NavWsVersion.Unknown so defaults to NAV7
                if (TimeoutSec > 0)
                    wsToUse.Timeout = (TimeoutSec - 2) * 1000;//-2 to make sure server timeout before client

                wsToUse.WebRequest(ref xmlRequest, ref xmlResponse);
            }
            catch (Exception ex)
            {
                //note pxmlResponce  vs  pxmlResponse
                if (ex.Message.Contains("pxmlResponse in method WebRequest in service"))
                {
                    // Are you connecting to NAV 2013 instead 2009?   7 vs 6
                    lock (Locker)
                    {
                        wsToUse.WebRequest(ref xmlRequest, ref xmlResponse);
                    }
                }
                else
                    throw ex;
            }
        }

        protected string RunOperation(string xmlRequest, bool useQuery = false, bool logResponse = true)
        {
            bool doBase64 = false;
            //only larger requests should be converted to base64
            if (xmlRequest.Length >= base64ConversionMinLength && (xmlRequest.Contains("WEB_POS") || xmlRequest.Contains("IM_SEND_DOCUMENT") || xmlRequest.Contains("IM_SEND_INVENTORY_TRANSACTION")))
            {
                //add key Nav.SkipBase64Conversion  true to skip this basel64 trick
                if (config.SettingsBoolGetByKey(ConfigKey.SkipBase64Conversion) == false)
                {
                    doBase64 = true;
                    Base64StringConvertion(ref xmlRequest);
                    logger.Debug(config.LSKey.Key, "Base64 string sent to Nav as <Encoded_Request>");
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //WebService Request
            string xmlResponse = string.Empty;
            ExecuteWebRequest(ref xmlRequest, ref xmlResponse, useQuery);

            stopWatch.Stop();
            if (string.IsNullOrWhiteSpace(xmlResponse))
            {
                logger.Error(config.LSKey.Key, "xmlResponse from NAV is empty");
                logger.Debug(config.LSKey.Key, "xmlRequest: " + xmlRequest);
                throw new LSOmniServiceException(StatusCode.NavWSError, "xmlResponse from NAV is empty");
            }

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("NAV WS call. ElapsedTime (mi:sec.msec): {0:00}:{1:00}.{2:000}",
                ts.Minutes, ts.Seconds, ts.Milliseconds);

            if (logger.IsDebugEnabled)
            {
                string reqId = GetRequestID(ref xmlRequest);
                string resId = GetRequestID(ref xmlResponse);
                if (reqId != resId)
                {
                    logger.Debug(config.LSKey.Key, "WARNING (DEBUG ONLY) Request and Response not the same from NAV: requestId:{0}  ResponesId:{1}", reqId, resId);
                }
                if (doBase64)
                {
                    string responseCode = GetResponseCode(ref xmlResponse);
                    // 0020 = Request Node Request_Body not found in request
                    if (responseCode == "0020")
                    {
                        logger.Debug(config.LSKey.Key, "WARNING Base64 string was passed to Nav but Nav failed. Has Codeunit 99009510 been updated to support base64 and Encoded_Request? requestId:{0}  ResponesId:{1}",
                            reqId, resId);
                    }
                }
            }

            //LogXml(xmlRequest, xmlResponse, elapsedTime, logResponse);
            return xmlResponse;
        }
        #endregion Altria Phase I

        #region Altria Phase II - Filteration of Offers
        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            return LSCentralWSBase.PublishedOffersGet2(cardId, itemId, storeId, stat);
        }
        #endregion Altria Phase II

        #region Altria Phase II - AgeChecker
        public virtual List<string> GetAgeCheckerReply(string cardId, string firstName, string lastName, DateTime dobDT, string phoneNo, string address, string city, string state, string zip, string email, Statistics stat)
        {
            return LSCentralWSBase.AgeVerifyReg(stat, firstName, lastName, address, city, state, zip, "US", phoneNo, email, "", "", dobDT.Day, dobDT.Month, dobDT.Year);
        }

        public virtual List<string> GetAgeCheckerStatus(Statistics stat, string UUID)
        {
            return LSCentralWSBase.AgeVerifyCheck(stat, UUID);
        }
        #endregion AgeChecker

        #region Altria Phase II - Member Attributes
        public virtual void SetMemberAttributes(string cardId, string tobaccoValue, string cigValue, string cigarValue, string dipValue, string onpValue, string snusValue, string eaivValue, Statistics stat)
        {
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();
            if (tobaccoValue != null) { myDictionary.Add(Constants.CAT_TOBACCO, tobaccoValue); }
            if (cigValue != null) { myDictionary.Add(Constants.AD_CONSENT_CIGARETTE, cigValue); }
            if (cigarValue != null) { myDictionary.Add(Constants.AD_CONSENT_CIGAR, cigarValue); }
            if (dipValue != null) { myDictionary.Add(Constants.AD_CONSENT_DIP, dipValue); }
            if (onpValue != null) { myDictionary.Add(Constants.AD_CONSENT_ONP, onpValue); }
            if (snusValue != null) { myDictionary.Add(Constants.AD_CONSENT_SNUS, snusValue); }
            if (eaivValue != null) { myDictionary.Add(Constants.AGE_VERIFIED, eaivValue); }
            LSCentralWSBase.SetMemberAttributes(cardId, myDictionary, stat);
        }
        #endregion

        #region Altria Phase II - Login for existing customers
        public virtual MemberContact ContactCreate(MemberContact contact, Statistics stat)
        {
            return LSCentralWSBase.ContactCreate2(contact, stat);
        }
        #endregion

        #region Altria Phase II - Altria Offer Retrieval
        public void RetrievePersonalizedOfferForCardId(string cardId, Statistics stat)
        {
            LSCentralWSBase.RetrievePersonalizedOfferForCardId(cardId, stat);
        }
        #endregion

        #region Altria Phase III - Remember Attributes
        public virtual List<Profile> ProfilesByCardIdGet(string cardId, Statistics stat)
        {
            return LSCentralWSBase.GetMemberAttributesByCardId(cardId, stat);
        }
        #endregion
    }
}
