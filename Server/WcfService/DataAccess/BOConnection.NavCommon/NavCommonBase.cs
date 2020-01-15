﻿using System;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net.Security;
using System.Diagnostics;
using System.Collections.Generic;

using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.NavCommon.XmlMapping;
using LSRetail.Omni.Domain.DataModel.Base;
using System.Linq;

namespace LSOmni.DataAccess.BOConnection.NavCommon
{
    //Navision back office connection
    public partial class NavCommonBase
    {
        protected int TimeOutInSeconds { get; set; }

        protected static LSLogger logger = new LSLogger();

        private NavWebReference.RetailWebServices navWebReference = null;
        public NavWS.OmniWrapper navWS = null;
        private int base64ConversionMinLength = 1024 * 100; //50KB 75KB  minimum length to base64 conversion
        private static readonly object Locker = new object();

        public Version NAVVersion = null; //use this in code to check Nav version
        public bool NavDirect = true;

        protected BOConfiguration config = null;

        private string ecomAppId = string.Empty;
        private string ecomAppType = string.Empty;

        private string pgtablename = "Product Group";

        public NavCommonBase(BOConfiguration configuration, bool ping = false)
        {
            if (configuration == null && !ping)
            {
                throw new LSOmniServiceException(StatusCode.SecurityTokenInvalid, "SecurityToken invalid");
            }
            config = configuration;

            base64ConversionMinLength = config.SettingsIntGetByKey(ConfigKey.Base64MinXmlSizeInKB) * 1024; //in KB

            ecomAppId = config.SettingsGetByKey(ConfigKey.NavAppId);
            ecomAppType = config.SettingsGetByKey(ConfigKey.NavAppType);

            string domain = "";
            NetworkCredential credentials = null;

            //check if domain is part of the username
            string username = config.SettingsGetByKey(ConfigKey.BOUser);
            string password = config.SettingsGetByKey(ConfigKey.BOPassword);

            //check if domain is part of the config.UserName
            if (username.Contains("/") || username.Contains(@"\"))
            {
                username = username.Replace(@"/", @"\");
                string[] splitter = username.Split('\\');
                domain = splitter[0];
                username = splitter[1];
            }

            //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
            if (DecryptConfigValue.IsEncryptedPwd(password))
            {
                password = DecryptConfigValue.DecryptString(password);
            }

            if (string.IsNullOrWhiteSpace(username) == false && string.IsNullOrWhiteSpace(password) == false)
            {
                credentials = new NetworkCredential(username, password, domain);
            }

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

            //navWsVersion is Unknown for the first time 
            if (NAVVersion == null)
            {
                navWebReference = new NavWebReference.RetailWebServices();
                
                navWebReference.Url = config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUrl.ToString()).Value;
                //TimeoutInSeconds from client can overwrite BOConnection.NavSQL.Timeout
                string timeout = config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOTimeout.ToString()).Value;
                navWebReference.Timeout = (timeout == null ? 20 : ConvertTo.SafeInt(timeout)) * 1000;  //millisecs,  60 seconds

                //dont set the credentials unless we have them. Can use the app pool too.
                if (credentials != null)
                    navWebReference.Credentials = credentials;

                navWebReference.PreAuthenticate = true;
                navWebReference.AllowAutoRedirect = true;
                if (string.IsNullOrEmpty(config.SettingsGetByKey(ConfigKey.Proxy_Server)) == false)
                    navWebReference.Proxy = GetWebProxy();

                NavVersionToUse(true); //check the nav version
            }

            // Use NAV Web Service V2
            if ((NAVVersion == null || NAVVersion.Major >= 11))
            {
                navWS = new NavWS.OmniWrapper();

                navWS.Url = config.SettingsGetByKey(ConfigKey.BOUrl).Replace("RetailWebServices", "OmniWrapper");
                navWS.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds

                if (credentials != null)
                    navWS.Credentials = credentials;

                navWS.PreAuthenticate = true;
                navWS.AllowAutoRedirect = true;
            }

            if (NAVVersion > new Version("14.2"))
                pgtablename = "Retail Product Group";
        }

        public string TenderTypeMapping(string tenderMapping, string tenderType, bool toOmni)
        {
            try
            {
                int tenderTypeId = -1;
                if (string.IsNullOrWhiteSpace(tenderMapping))
                {
                    return null;
                }

                // first one is Omni TenderType, 2nd one is the NAV id
                //tenderMapping: "1=1,2=2,3=3,4=4,6=6,7=7,8=8,9=9,10=10,11=11,15=15,19=19"
                //or can be : "1  =  1  ,2=2,3= 3, 4=4,6 =6,7=7,8=8,9=9,10=10,11=11,15=15,19=19"

                string[] commaMapping = tenderMapping.Split(',');  //1=1 or 2=2  etc
                foreach (string s in commaMapping)
                {
                    string[] eqMapping = s.Split('='); //1 1
                    if (toOmni)
                    {
                        if (tenderType == eqMapping[1].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[0].Trim());
                            break;
                        }
                    }
                    else
                    {
                        if (tenderType == eqMapping[0].Trim())
                        {
                            tenderTypeId = Convert.ToInt32(eqMapping[1].Trim());
                            break;
                        }
                    }
                }
                return tenderTypeId.ToString();
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in TenderTypeMapping(tenderMapping:{0} tenderType:{1})", tenderMapping, tenderType.ToString());
                logger.Error(config.LSKey.Key, ex, msg + ex.Message);
                throw;
            }
        }


        public XMLTableData DoReplication(int tableid, string storeId, string appid, string apptype, int batchSize, ref string lastKey, out int totalrecs)
        {
            bool endoftable = true;
            totalrecs = 0;

            if (string.IsNullOrEmpty(appid) && string.IsNullOrEmpty(apptype))
            {
                appid = ecomAppId;
                apptype = ecomAppType;
            }

            int restorepoint = Convert.ToInt32(string.IsNullOrEmpty(lastKey) ? "0" : lastKey);
            int resPoint;
            NAVWebXml xml = new NAVWebXml(storeId, appid, apptype);
            List<XMLTableData> tablist = new List<XMLTableData>();

            if (restorepoint == -1)
            {
                // restore everything
                RestoreWebReplication(xml, restorepoint);
                restorepoint = 0;
            }

            if (restorepoint == 0)
            {
                // restart to beginning
                restorepoint = 1;
                RestoreWebReplication(xml, restorepoint);
                tablist = StartWebReplication(xml, batchSize, ref restorepoint);
            }
            else
            {
                NAVSyncCycleStatus status = GetTableStatus(xml, tableid, out resPoint);
                switch (status)
                {
                    case NAVSyncCycleStatus.InProgress:
                        tablist = new List<XMLTableData>();
                        XMLTableData t = new XMLTableData();
                        t.TableId = tableid;
                        t.SyncCycleStatus = NAVSyncCycleStatus.InProgress;
                        tablist.Add(t);
                        break;
                    case NAVSyncCycleStatus.New:
                        tablist = StartWebReplication(xml, batchSize, ref restorepoint);
                        break;
                    case NAVSyncCycleStatus.Finished:
                        if (restorepoint < resPoint)
                            tablist = RestoreWebReplication(xml, restorepoint);
                        else
                            tablist = StartWebReplication(xml, batchSize, ref restorepoint);
                        break;
                }
            }

            XMLTableData data = GetTableData(xml, tablist, tableid, out endoftable, out totalrecs, out resPoint);
            if (endoftable)
                totalrecs = 0;
            if (resPoint > 0)
                restorepoint = resPoint;

            lastKey = restorepoint.ToString();
            return data;
        }


        public string NavVersionToUse(bool forceCallToNav = true)
        {
            if (NAVVersion == null)
                NAVVersion = new Version("14.0");

            //this methods is called in PING and in constructor
            try
            {
                //To overwrite what comes from NAV (or if Nav doesn't implement TEST_CONNECTION
                //in AppSettings add this key.
                //  <add key="LSNAV.Version" value="8.0"/>    or "7.0"  "7.1"

                string navver = string.Empty;
                if (forceCallToNav)
                {
                    string appVersion = string.Empty;
                    string appBuild = string.Empty;
                    string retailCopyright = string.Empty;
                    string retailVersion = string.Empty;

                    if (navWS != null && NAVVersion.Major > 13)
                    {
                        string respCode = string.Empty;
                        string errorText = string.Empty;

                        navWS.TestConnection(ref respCode, ref errorText, ref appVersion, ref appBuild, ref retailVersion, ref retailCopyright);
                    }
                    else
                    {
                        XmlMapping.Loyalty.NavXml navXml = new XmlMapping.Loyalty.NavXml();
                        string xmlRequest = navXml.TestConnectionRequestXML();
                        string xmlResponse = RunOperation(xmlRequest);
                        logger.Info(config.LSKey.Key, "Nav Version: " + xmlResponse);
                        string rCode = GetResponseCode(ref xmlResponse);

                        //ignore unknown Request_ID 
                        //the 0004   Unknown Request_ID   TEST_CONNECTION
                        if (rCode != "0004")
                        {
                            HandleResponseCode(ref xmlResponse);
                            navXml.TestConnectionResponseXML(xmlResponse, ref appVersion, ref appBuild, ref retailVersion, ref retailCopyright);
                        }
                    }

                    int st = retailVersion.IndexOf('(');
                    int ed = retailVersion.IndexOf(')');
                    string vv1 = retailVersion.Substring(st + 1, ed - st - 1);
                    NAVVersion = new Version(vv1);

                    navver = string.Format("LS:{0} [{1}]", vv1, appBuild);
                    NAVVersion = new Version(vv1);

                    logger.Info(config.LSKey.Key, "appVer:{0} appBuild:{1} retailVer:{2} retailCopyright:{3} NavVersionToUse:{4}",
                        appVersion, appBuild, retailVersion, retailCopyright, (NAVVersion == null) ? "None" : NAVVersion.ToString());
                }

                //can overwrite what comes from NAV by adding key LSNAV.Version to the appConfig FILE not table.. LSNAV_Version
                string version = config.SettingsGetByKey(ConfigKey.LSNAV_Version);
                if (string.IsNullOrEmpty(version) == false)
                {
                    NAVVersion = new Version(version);

                    logger.Info(config.LSKey.Key, "Value {0} of key LSNAV.Version from TenantConfig file is being used : {1}", version, NAVVersion);
                }
                return navver;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Failed to determine NavVersion");
                return "ERROR " + ex.Message;
            }
        }

        #region private members

        private void Base64StringConvertion(ref string xmlRequest)
        {
            string base64String = Convert.ToBase64String(new UTF8Encoding().GetBytes(xmlRequest));
            //Dont want to load hundreds of KB in xdoc just to get the requestId
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

        //run the nav web service operation
        protected string RunOperation(string xmlRequest)
        {
            bool doBase64 = false;
            string originalxmlRequest = "";
            //only larger requests should be converted to base64
            if (xmlRequest.Length >= base64ConversionMinLength && (xmlRequest.Contains("WEB_POS") || xmlRequest.Contains("IM_SEND_DOCUMENT") || xmlRequest.Contains("IM_SEND_INVENTORY_TRANSACTION")))
            {
                //add key Nav.SkipBase64Conversion  true to skip this basel64 trick
                if (config.SettingsBoolGetByKey(ConfigKey.SkipBase64Conversion))
                {
                    originalxmlRequest = xmlRequest;//save it for logging
                    doBase64 = true;
                    Base64StringConvertion(ref xmlRequest);
                    logger.Debug(config.LSKey.Key, "Base64 string sent to Nav as <Encoded_Request>");
                }
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //WebService Request
            string xmlResponse = string.Empty;
            ExecuteWebRequest(ref xmlRequest, ref xmlResponse);

            stopWatch.Stop();
            if (string.IsNullOrWhiteSpace(xmlResponse))
            {
                logger.Error(config.LSKey.Key, "xmlResponse from NAV is empty");
                logger.Debug(config.LSKey.Key, "xmlRequest: " + xmlRequest);
                throw new LSOmniServiceException(StatusCode.Error, "xmlResponse from NAV is empty");
            }

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("NAV ws call. ElapsedTime (mi:sec.msec): {0:00}:{1:00}.{2:000}",
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

            LogXml(xmlRequest, xmlResponse, elapsedTime);
            return xmlResponse;
        }

        private List<XMLTableData> StartWebReplication(NAVWebXml xml, int batchSize, ref int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            // get tables to replicate and current status
            xmlRequest = xml.StartSyncRequestXML(batchSize);
            xmlResponse = RunOperation(xmlRequest);
            string ret = HandleResponseCode(ref xmlResponse, new string[] { "1921" });
            if (ret == "1921")
            {
                // App is not registered, so lets register it
                xmlRequest = xml.RegisterApplicationRequestXML(NAVVersion);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);

                // Now try again to start Sync Cycle
                xmlRequest = xml.StartSyncRequestXML(batchSize);
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
            }
            return xml.SyncResponseXML(xmlResponse, out restorePoint);
        }

        private List<XMLTableData> RestoreWebReplication(NAVWebXml xml, int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            xmlRequest = xml.RestoreSyncRequestXML(restorePoint);
            xmlResponse = RunOperation(xmlRequest);
            string ret = HandleResponseCode(ref xmlResponse, new string[] { "1921", "1923" });
            if (ret != null)
                return new List<XMLTableData>();

            return xml.SyncResponseXML(xmlResponse, out restorePoint);
        }

        private XMLTableData GetTableData(NAVWebXml xml, List<XMLTableData> tablist, int tableidtoget, out bool endoftable, out int totalrecs, out int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;
            endoftable = true;
            totalrecs = 0;
            restorePoint = 0;

            foreach (XMLTableData table in tablist)
            {
                if (table.TableId != tableidtoget || table.SyncCycleStatus == NAVSyncCycleStatus.Finished)
                    continue;

                try
                {
                    xmlRequest = xml.GetTableDataRequestXML(table.TableId);
                    xmlResponse = RunOperation(xmlRequest);
                    HandleResponseCode(ref xmlResponse);
                    XMLTableData ret = xml.GetTableDataResponseXML(xmlResponse, table, out endoftable, out restorePoint);
                    totalrecs = ret.NumberOfValues;
                    return ret;
                }
                catch (Exception ex)
                {
                    logger.Error(config.LSKey.Key, ex, "Fetching Table data for table" + table.TableName);
                }
            }
            return null;
        }

        private NAVSyncCycleStatus GetTableStatus(NAVWebXml xml, int tableNo, out int restorePoint)
        {
            string xmlRequest;
            string xmlResponse;

            try
            {
                xmlRequest = xml.GetSyncStatusRequestXML();
                xmlResponse = RunOperation(xmlRequest);
                HandleResponseCode(ref xmlResponse);
                return xml.GetSyncStatusResponseXML(xmlResponse, tableNo, out restorePoint);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex, "Fetching Table data for table" + tableNo);
                restorePoint = 0;
                return NAVSyncCycleStatus.New;
            }
        }

        private void LogXml(string xmlRequest, string xmlResponse, string elapsedTime)
        {
            string reqId = GetRequestID(ref xmlRequest);
            //too much data even for debug mode. Only write some requestId if trace is enabled
            if (!logger.IsTraceEnabled && (reqId == "WEB_GET_DINING_TABLE_LIST" || reqId == "GET_DYNAMIC_CONTENT"))
            {
                return;
            }
            //only log xml in debug mode, since passwords are logged
            else if (logger.IsDebugEnabled)
            {
                xmlRequest = RemoveNode("Password", xmlRequest);
                xmlRequest = RemoveNode("NewPassword", xmlRequest);
                xmlRequest = RemoveNode("OldPassword", xmlRequest);
                XDocument doc = XDocument.Parse(xmlResponse);
                xmlResponse = doc.ToString();//get better xml parsing
                XDocument docRq = XDocument.Parse(xmlRequest);
                xmlRequest = docRq.ToString();//get better xml parsing
                logger.Debug(config.LSKey.Key, "\r\nNLOG DEBUG MODE ONLY:  {0}\r\n{1}\r\n  \r\n{2}\r\n", elapsedTime, xmlRequest, xmlResponse);
            }
        }

        protected string RemoveNode(string nodeName, string xml)
        {
            try
            {
                //remove a node from xml,, nodeName=Password removes value between <Password>
                string regex = "<" + nodeName + ">.*?</" + nodeName + ">"; //strip out 
                return System.Text.RegularExpressions.Regex.Replace(xml, regex, "<" + nodeName + ">XXX</" + nodeName + ">");
            }
            catch
            {
                return xml;
            }
        }

        //first time executing this I check if this is nav 7 or 6 web service
        protected void ExecuteWebRequest(ref string xmlRequest, ref string xmlResponse)
        {
            try
            {
                //first time comes in at NavWsVersion.Unknown so defaults to NAV7
                if (TimeOutInSeconds > 0)
                    navWebReference.Timeout = (TimeOutInSeconds - 2) * 1000;//-2 to make sure server timeout before client
                navWebReference.WebRequest(ref xmlRequest, ref xmlResponse);
            }
            catch (Exception ex)
            {
                //note pxmlResponce  vs  pxmlResponse
                if (ex.Message.Contains("pxmlResponse in method WebRequest in service"))
                {
                    // Are you connecting to NAV 2013 instead 2009?   7 vs 6
                    lock (Locker)
                    {
                        navWebReference.WebRequest(ref xmlRequest, ref xmlResponse);
                    }
                }
                else
                    throw ex;
            }
        }

        private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        protected string GetResponseCode(ref string xmlResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string navResponseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            return navResponseCode;
        }

        protected StatusCode GetStatusCode(ref string xmlResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string navResponseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
            return MapResponseToStatusCode(navResponseId, navResponseCode);
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

        protected string HandleResponseCode(ref string xmlResponse, string[] codesToHandle = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);
            string responseCode = ParseResponseCode(doc.GetElementsByTagName("Response_Code"));
            if (responseCode != "0000")
            {
                string navResponseId = ParseResponseCode(doc.GetElementsByTagName("Request_ID"));
                string navResponseText = ParseResponseText(doc.GetElementsByTagName("Response_Text"));

                //NavResponseCode navResponseCode = (NavResponseCode)Convert.ToInt32(responseCode);
                StatusCode statusCode = MapResponseToStatusCode(navResponseId, responseCode);
                string msg = string.Format("navResponseCode: {0} - {1}  [statuscode: {2}]", responseCode, navResponseText, statusCode.ToString());
                logger.Error(config.LSKey.Key, msg);

                if (codesToHandle != null && codesToHandle.Length > 0)
                {
                    foreach (string code in codesToHandle)
                    {
                        if (code.Equals(responseCode))
                            return code;
                    }
                }
                //expected return codes, so dont throw unexpected exception, rather return the known codes to client  
                throw new LSOmniServiceException(statusCode, msg);
            }
            return null;
        }

        protected StatusCode MapResponseToStatusCode(string navResponseId, string navCode)
        {
            /*
                MissingStoreNumber = 1001,
                MissingTenderLines = 1002,
             * 
             * */
            //mapping response code from NAV to LSOmni, but sometimes the same navCode is used for different navResponseId
            // so need to check them both - sometimes
            navResponseId = navResponseId.ToUpper().Trim();
            StatusCode statusCode = StatusCode.Error; //default to Error
            switch (navCode)
            {
                case "0000":
                    statusCode = StatusCode.OK;
                    break;
                case "0004":
                    statusCode = StatusCode.NAVWebFunctionNotFound;
                    break;
                case "0030":
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.InvalidNode;
                    break;
                case "0131":
                    statusCode = StatusCode.PasswordInvalid;
                    break;
                case "1001":
                    statusCode = StatusCode.Error;
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.NoHMPMenuFound;
                    else if (navResponseId == "MM_LOGIN_CHANGE") // cannot for "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.UserNameInvalid;
                    else if (navResponseId == "IM_GET_CUSTOMER_CARD_01")
                        statusCode = StatusCode.CustomerNotFound;
                    else if (navResponseId == "IM_GET_ITEM_CARD_01")
                        statusCode = StatusCode.ItemNotFound;
                    else if (navResponseId == "IM_GET_VENDOR_CARD_01")
                        statusCode = StatusCode.VendorNotFound;
                    break;
                case "1002":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.PasswordInvalid;
                    else if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.UserNameNotFound;
                    break;
                case "1003":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.UserNameInvalid;
                    else if (navResponseId == "MM_CREATE_LOGIN_LINKS")
                        statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1004":
                    if (navResponseId == "MM_LOGIN_CHANGE")
                        statusCode = StatusCode.UserNameExists;
                    break;
                case "1010":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.UserNameExists;
                    break;
                case "1011":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.ItemNotFound;
                    break;
                case "1012":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.StoreNotExists;
                    break;
                case "1013":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNotEnabled;
                    break;
                case "1014":
                    if (navResponseId == "GET_DYN_CONT_HMP_MENUS" || navResponseId == "GET_DYNAMIC_CONTENT")
                        statusCode = StatusCode.HMPMenuNoDynamicContentFoundToday;
                    break;
                case "1020":
                    if (navResponseId == "LOAD_PUBOFFERS_AND_PERSCOUPONS")
                        statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1100":
                    statusCode = StatusCode.UserNameExists;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.AccountNotFound;
                    break;
                case "1101":
                    statusCode = StatusCode.UserNameNotFound;
                    break;
                case "1102":
                    statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1103":
                    statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1106":
                    statusCode = StatusCode.QtyMustBePositive;
                    break;
                case "1107":
                    statusCode = StatusCode.LineNoMission;
                    break;
                case "1110":
                    statusCode = StatusCode.UserNameInvalid;
                    break;
                case "1120":
                    statusCode = StatusCode.PasswordInvalid;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.AuthFailed;
                    break;
                case "1130":
                    statusCode = StatusCode.EmailInvalid;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.CardIdInvalid;
                    break;
                case "1135":
                    statusCode = StatusCode.EmailExists;
                    break;
                case "1140":
                    statusCode = StatusCode.MissingLastName;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.LoginIdNotMemberOfClub;
                    break;
                case "1150":
                    statusCode = StatusCode.MissingFirstName;
                    if (navResponseId == "MM_MOBILE_LOGON")
                        statusCode = StatusCode.DeviceIdNotFound;
                    break;
                case "1160":
                    statusCode = StatusCode.AccountNotFound;
                    break;
                case "1170":
                    statusCode = StatusCode.OneAccountInvalid;
                    break;
                case "1180":
                    statusCode = StatusCode.PrivateAccountInvalid;
                    break;
                case "1190":
                    statusCode = StatusCode.ClubInvalid;
                    break;
                case "1200":
                    statusCode = StatusCode.SchemeInvalid;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1210":
                    statusCode = StatusCode.ClubOrSchemeInvalid;
                    break;
                case "1212":
                    statusCode = StatusCode.SchemeClubInvalid;
                    break;
                case "1220":
                    statusCode = StatusCode.AccountContactIdInvalid;
                    break;
                case "1230":
                    statusCode = StatusCode.AccountExistsInOtherClub;
                    break;

                case "1300":
                    statusCode = StatusCode.PasswordOldInvalid;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1310":
                    statusCode = StatusCode.PasswordInvalid; //invalid new password
                    break;
                case "1400":
                    statusCode = StatusCode.MissingItemNumer;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.AccountNotFound;
                    break;
                case "1410":
                    statusCode = StatusCode.MissingStoreNumber;
                    break;
                case "1450":
                    statusCode = StatusCode.DeviceIdMissing;
                    break;
                case "1500":
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIdNotFound;
                    break;
                case "1600":
                    statusCode = StatusCode.PosNotExists;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1601":
                    statusCode = StatusCode.StoreNotExists;
                    break;
                case "1602":
                    statusCode = StatusCode.StaffNotExists;
                    break;
                case "1603":
                    statusCode = StatusCode.ItemNotExists;
                    break;
                case "1604":
                    statusCode = StatusCode.VATSetupMissing;
                    break;
                case "1605":
                    statusCode = StatusCode.InvalidUom;
                    break;
                case "1606":
                    statusCode = StatusCode.ItemBlocked;
                    break;
                case "1607":
                    statusCode = StatusCode.InvalidVariant;
                    break;
                case "1608":
                    statusCode = StatusCode.InvalidPriceChange;
                    break;
                case "1609":
                    statusCode = StatusCode.PriceChangeNotAllowed;
                    break;
                case "1610":
                    statusCode = StatusCode.PriceTooHigh;
                    break;
                case "1611":
                    statusCode = StatusCode.InvalidDiscPercent;
                    break;
                case "1612":
                    statusCode = StatusCode.IncExpNotFound;
                    break;
                case "1613":
                    statusCode = StatusCode.TenderTypeNotFound;
                    break;
                case "1614":
                    statusCode = StatusCode.InvalidTOTDiscount;
                    break;
                case "1615":
                    statusCode = StatusCode.NotMobilePos;
                    break;
                case "1619":
                    statusCode = StatusCode.InvalidPostingBalance;
                    break;
                case "1620":
                    statusCode = StatusCode.SuspendWithPayment;
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.PaymentPointsMissing; //Payment with member points, %1 missing.
                    break;
                case "1621":
                    statusCode = StatusCode.UnknownSuspError;
                    if (navResponseId == "WEB_POS")
                        statusCode = StatusCode.MemberCardNotFound; //Unable to load member information – ErrorText from LOAD_MEMBER _INFO displayed
                    break;
                case "1625":
                    statusCode = StatusCode.SuspKeyNotFound;
                    break;
                case "1626":
                    statusCode = StatusCode.TransServError;
                    break;
                case "1627":
                    statusCode = StatusCode.SuspTransNotFound;
                    break;
                case "1670":
                    statusCode = StatusCode.CustomerNotFound;
                    break;
                case "1673":
                    statusCode = StatusCode.NoEntriesFound;
                    break;
                case "1674":
                    statusCode = StatusCode.MemberAccountNotFound;
                    break;
                case "1675":
                    statusCode = StatusCode.MemberCardNotFound;
                    break;
                case "1676":
                    statusCode = StatusCode.NoEntriesFound2;
                    break;
                case "1677":
                    statusCode = StatusCode.NoEntriesFound3;
                    break;
                case "1678":
                    statusCode = StatusCode.NoEntriesFound4;
                    break;

                case "1698":
                    statusCode = StatusCode.InvalidPrinterId;
                    break;
                case "1700":
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.CardInvalidInUse;
                    break;
                case "1800":
                    statusCode = StatusCode.TerminalIdMissing;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ClubInvalid;
                    break;
                case "1801":
                    statusCode = StatusCode.TransacitionIdMissing;
                    break;
                case "1802":
                    statusCode = StatusCode.EmailMissing;
                    break;
                case "1900":
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.CardInvalidStatus;
                    break;
                case "2000":
                    statusCode = StatusCode.InvalidPrintMethod;
                    if (navResponseId == "MM_CARD_TO_CONTACT")
                        statusCode = StatusCode.ContactIsBlocked;
                    break;
                case "0403":
                    statusCode = StatusCode.ServerRefusingToRespond;
                    break;
                case "4003":
                    statusCode = StatusCode.DiningTableStatusNotAbleToChange;
                    break;
                case "4004":
                    statusCode = StatusCode.CannotChangeNumberOfCoverOnTableNotSeated;
                    break;
                case "4005":
                    statusCode = StatusCode.CannotChangeNumberOfCoverOnTableNoSetup;
                    break;
                case "4006":
                    statusCode = StatusCode.SeatingNotUsedInHospType;
                    break;
                case "4007":
                    statusCode = StatusCode.StatusOfTableAlredySeated;
                    break;
                case "4008":
                    statusCode = StatusCode.SeatingNotPossible;
                    break;
                case "4009":
                    statusCode = StatusCode.POSTransNotFoundForActiveOrder;
                    break;
                case "4010":
                    statusCode = StatusCode.NoKitchenStatusFound;
                    break;
                case "4011":
                    statusCode = StatusCode.OpenPOSNotALlowed;
                    break;
                case "4020":
                    statusCode = StatusCode.MainStatusNorCorrect;
                    break;
                case "4021":
                    statusCode = StatusCode.TableAlreadyLocked;
                    break;
                case "7750":  //Percassi only
                    statusCode = StatusCode.SuspendFailure;
                    break;

                default:
                    break;
            }
            return statusCode;
        }

        private WebProxy GetWebProxy()
        {
            //<!-- Keep Proxy values blank if not used -->
            //<add key="Proxy.Server" value=""/>
            //<add key="Proxy.Port" value=""/>
            //<add key="Proxy.User" value=""/>
            //<add key="Proxy.Password" value=""/>
            //<add key="Proxy.Domain" value=""/>

            string proxyServer = config.SettingsGetByKey(ConfigKey.Proxy_Server);
            proxyServer = proxyServer.Trim();
            int iProxyPort = 0;
            string proxyPort = config.SettingsGetByKey(ConfigKey.Proxy_Port);
            string proxyUser = config.SettingsGetByKey(ConfigKey.Proxy_User);
            string proxyPassword = config.SettingsGetByKey(ConfigKey.Proxy_Password);
            string proxyDomain = config.SettingsGetByKey(ConfigKey.Proxy_Domain);

            if (string.IsNullOrEmpty(proxyPort) == false)
                iProxyPort = Convert.ToInt32(proxyPort);

            WebProxy oWebProxy = new WebProxy(proxyServer, iProxyPort);
            if (string.IsNullOrEmpty(proxyUser) == false)
            {
                oWebProxy.Credentials = new NetworkCredential(proxyUser, proxyPassword, proxyDomain); //
            }

            return oWebProxy;
        }

        #endregion private members
    }
}