using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using LSOmni.Common.Util;
using LSOmni.DataAccess.BOConnection.PreCommon.Mapping;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using System.ComponentModel;

namespace LSOmni.DataAccess.BOConnection.PreCommon
{
    public partial class PreCommonBase
    {

        public OmniWrapper2.OmniWrapper2 centralWS2 = null;
        public virtual string MyCustomFunction(string data)
        {
            // TODO: Here you put the code to access BC or NAV WS
            // Data Mapping is done under Mapping folder
            return "My return data + Incoming data: " + data;
        }

        #region Altria Phase II
        public List<PublishedOffer> PublishedOffersGet2(string cardId, string itemId, string storeId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            centralWS2 = new OmniWrapper2.OmniWrapper2();
            string url = config.SettingsGetByKey(ConfigKey.BOUrl);
            centralWS2.Url = url.Replace("RetailWebServices", "OmniWrapper2");
            centralWS2.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralWS2.PreAuthenticate = true;
            centralWS2.AllowAutoRedirect = true;
            centralWS2.Credentials = new System.Net.NetworkCredential(
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUser.ToString()).Value,
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOPassword.ToString()).Value);

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);
            OmniWrapper2.RootGetDirectMarketingInfo root = new OmniWrapper2.RootGetDirectMarketingInfo();

            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo2 - CardId: {0}, ItemId: {1}", cardId, itemId);
            centralWS2.GetDirectMarketingInfo(ref respCode, ref errorText, XMLHelper.GetString(cardId), XMLHelper.GetString(itemId), XMLHelper.GetString(storeId), ref root);
            HandleWS2ResponseCode("GetDirectMarketingInfo", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "GetDirectMarketingInfo2 Response - " + Serialization.ToXml(root, true));
            List<PublishedOffer> data = map.MapFromRootToPublishedOffers(root);
            logger.StatisticEndSub(ref stat, index);
            return data;
        }
        #endregion Altria Phase II

        #region AgeChecker
        public async Task<List<string>> AgeVerifyAsync(
            Statistics stat,
            string IDNameFirst,
            string IDNameLast,
            string IDAddress,
            string IDCity,
            string IDState,
            string IDZip,
            string IDCountry,
            string OptionsCustIP,
            string UUID,
            int IDDOBDay,
            int IDDOBMonth,
            int IDDOBYear,
            int OptionsMinAge = 18)
        {
            HttpClient request = new HttpClient();
            string APIKey = "lCr0Q4OXQZtrf4hDXs10osYPfPqkkl0g",
                APISecret = "w8zMYJXeDOz4ky8Q",
                APISite = "https://api.agechecker.net/v1/create",
                ResultsErrorCode = string.Empty,
                ResultsErrorMsg = string.Empty,
                ResultsStatus = string.Empty,
                ResultsUploadType = string.Empty,
                ResultsUUID = string.Empty;
            Boolean ResultsBlocked = false;

            logger.StatisticStartSub(true, ref stat, out int index);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): begin");
            var obj1 = new
            {
                first_name = IDNameFirst,
                last_name = IDNameLast,
                address = IDAddress,
                city = IDCity,
                state = IDState,
                zip = IDZip,
                country = IDCountry,
                dob_day = IDDOBDay,
                dob_month = IDDOBMonth,
                dob_year = IDDOBYear
            };
            var obj2 = new
            {
                min_age = OptionsMinAge,
                customer_ip = OptionsCustIP
            };
            var obj3 = new
            {
                key = APIKey,
                secret = APISecret,
                data = obj1,
                options = obj2
            };
            JsonContent content = JsonContent.Create(obj3);
            logger.Debug(config.LSKey.Key, "request.PostAsync(APISite, content)");
            HttpResponseMessage response = await request.PostAsync(APISite, content);
            logger.Debug(config.LSKey.Key, "response.Content.ReadAsStringAsync()");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): jsonResponse: {0}", jsonResponse);
            AgeVerifyGetValues(
                jsonResponse,
                ref ResultsErrorCode,
                ref ResultsErrorMsg,
                ref ResultsStatus,
                ref ResultsUploadType,
                ref ResultsUUID,
                ref ResultsBlocked);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsErrorCode: {0}", ResultsErrorCode);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsErrorMsg: {0}", ResultsErrorMsg);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsStatus: {0}", ResultsStatus);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsUploadType: {0}", ResultsUploadType);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsUUID: {0}", ResultsUUID);
            logger.Debug(config.LSKey.Key, "AgeVerifyAsync(): ResultsBlocked: {0}", ResultsBlocked);
            List <string> data = new List<string> { ResultsUUID, response.StatusCode.ToString(), ResultsStatus, ResultsErrorCode, ResultsErrorMsg };
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public async Task<List<string>> AgeVerifyCheckResultAsync(Statistics stat, string UUID)
        {
            string ResultsErrorCode = string.Empty,
                ResultsErrorMsg = string.Empty,
                ResultsStatus = string.Empty,
                ResultsUploadType = string.Empty,
                ResultsUUID = string.Empty;
            Boolean ResultsBlocked = false;

            if (UUID == string.Empty)
            {
                throw new Exception("UUID is required");
            }
            logger.StatisticStartSub(true, ref stat, out int index);
            logger.Debug(config.LSKey.Key, "AgeVerifyCheckResultAsync(): begin");
            logger.Debug(config.LSKey.Key, "https://api.agechecker.net/v1/status/{0}", UUID);
            HttpClient request = new HttpClient();
            var response = await request.GetAsync("https://api.agechecker.net/v1/status/" + UUID);
            string responseText = await response.Content.ReadAsStringAsync();
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: resultText: {0}", responseText);
            AgeVerifyGetValues(
                responseText,
                ref ResultsErrorCode,
                ref ResultsErrorMsg,
                ref ResultsStatus,
                ref ResultsUploadType,
                ref ResultsUUID,
                ref ResultsBlocked);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsErrorCode: {0}", ResultsErrorCode);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsErrorMsg: {0}", ResultsErrorMsg);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsStatus: {0}", ResultsStatus);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsUploadType: {0}", ResultsUploadType);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsUUID: {0}", ResultsUUID);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: ResultsBlocked: {0}", ResultsBlocked);
            logger.Debug(config.LSKey.Key, "AgeVerifyGetValues: result.StatusCode.ToString(): {0}", response.StatusCode.ToString());
            logger.Debug(config.LSKey.Key, "AgeVerifyCheckResultAsync(): end");
            List<string> data = new List<string> { ResultsUUID, response.StatusCode.ToString(), ResultsStatus, ResultsErrorCode, ResultsErrorMsg };
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public void AgeVerifyGetValues(
            string JsonResponseText,
            ref string ResultsErrorCode,
            ref string ResultsErrorMsg,
            ref string ResultsStatus,
            ref string ResultsUploadType,
            ref string ResultsUUID,
            ref Boolean ResultsBlocked)
        {
            if (JsonResponseText == string.Empty)
            {
                ResultsErrorMsg = "Json message must not be empty";
            }
            try
            {
                JsonNode AgeCheckerResponse = JsonNode.Parse(JsonResponseText);
                try
                {
                    ResultsBlocked = (Boolean)AgeCheckerResponse["blocked"];
                }
                catch
                {
                    ResultsBlocked = false;
                }
                try
                {
                    ResultsErrorCode = (string)AgeCheckerResponse["error"]["code"];
                }
                catch
                {
                    ResultsErrorCode = string.Empty;
                }
                try
                {
                    ResultsErrorMsg = (string)AgeCheckerResponse["error"]["message"];
                }
                catch
                {
                    ResultsErrorMsg = string.Empty;
                }
                try
                {
                    ResultsStatus = (string)AgeCheckerResponse["status"];
                }
                catch
                {
                    ResultsStatus = string.Empty;
                }
                try
                {
                    ResultsUploadType = (string)AgeCheckerResponse["upload_type"];
                }
                catch
                {
                    ResultsUploadType = string.Empty;
                }
                try
                {
                    ResultsUUID = (string)AgeCheckerResponse["uuid"];
                }
                catch
                {
                    ResultsUUID = string.Empty;
                }
            }
            catch (Exception e)
            {
                ResultsErrorMsg = "Unable to parse Json response: " + e.Message;
            }
            return;
        }
        #endregion AgeChecker

    }
}
