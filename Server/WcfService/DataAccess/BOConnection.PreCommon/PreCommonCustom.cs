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
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Collections;

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

        #region Altria Phase II - Filtration of Offers
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

        #region Altria Phase II - AgeChecker


        public List<string> AgeVerifyReg(
            Statistics stat,
            string IDNameFirst,
            string IDNameLast,
            string IDAddress,
            string IDCity,
            string IDState,
            string IDZip,
            string IDCountry,
            string phoneNumber,
            string emailAddress,
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
                dob_year = IDDOBYear,
                phone = phoneNumber,
                email = emailAddress
            };
            var obj2 = new
            {
                min_age = OptionsMinAge,
                customer_ip = OptionsCustIP,
                contact_customer = true
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

            HttpResponseMessage response = request.PostAsync(APISite, content).Result;
            logger.Debug(config.LSKey.Key, "response.Content.ReadAsStringAsync()");
            string jsonResponse = response.Content.ReadAsStringAsync().Result;
            logger.Debug(config.LSKey.Key, "jsonResponse: {0}", jsonResponse);
            AgeVerifyGetValues(
                jsonResponse,
                ref ResultsErrorCode,
                ref ResultsErrorMsg,
                ref ResultsStatus,
                ref ResultsUploadType,
                ref ResultsUUID,
                ref ResultsBlocked);
            logger.Debug(config.LSKey.Key, "StatusCode: {0}", response.StatusCode.ToString());
            logger.Debug(config.LSKey.Key, "ResultsErrorCode: {0}", ResultsErrorCode);
            logger.Debug(config.LSKey.Key, "ResultsErrorMsg: {0}", ResultsErrorMsg);
            logger.Debug(config.LSKey.Key, "ResultsStatus: {0}", ResultsStatus);
            logger.Debug(config.LSKey.Key, "ResultsUploadType: {0}", ResultsUploadType);
            logger.Debug(config.LSKey.Key, "ResultsUUID: {0}", ResultsUUID);
            logger.Debug(config.LSKey.Key, "ResultsBlocked: {0}", ResultsBlocked);
            List <string> data = new List<string> { ResultsUUID, response.StatusCode.ToString(), ResultsStatus, ResultsErrorCode, ResultsErrorMsg };
            logger.StatisticEndSub(ref stat, index);
            return data;
        }

        public List<string> AgeVerifyCheck(Statistics stat, string UUID)
        {
            string ResultsErrorCode = string.Empty,
                ResultsErrorMsg = string.Empty,
                ResultsStatus = string.Empty,
                ResultsUploadType = string.Empty,
                ResultsUUID = string.Empty;
            Boolean ResultsBlocked = false;

            if (UUID == string.Empty)
            {
                throw new LSOmniException(StatusCode.ParameterInvalid, "UUID is required");
            }
            logger.StatisticStartSub(true, ref stat, out int index);
            logger.Debug(config.LSKey.Key, "AgeVerifyCheck(): begin");
            logger.Debug(config.LSKey.Key, "https://api.agechecker.net/v1/status/{0}", UUID);
            HttpClient request = new HttpClient();
            var response = request.GetAsync("https://api.agechecker.net/v1/status/" + UUID).Result;
            string responseText = response.Content.ReadAsStringAsync().Result;
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

        #region Altria Phase II - Member Attributes
        public virtual void SetMemberAttributes(string cardId, Dictionary<string, string> attributes, Statistics stat)
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

            OmniWrapper2.RootSetMemberAttributes root = new OmniWrapper2.RootSetMemberAttributes();
            root.MemberAttribute = new OmniWrapper2.MemberAttribute[attributes.Count];
            var keyList = new List<string>(attributes.Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                OmniWrapper2.MemberAttribute memberAttribute = new OmniWrapper2.MemberAttribute();
                memberAttribute.AttributeCode = keyList[i];
                memberAttribute.AttributeValue = attributes[keyList[i]];
                root.MemberAttribute[i] = memberAttribute;
            }
            logger.Debug(config.LSKey.Key, "SetMemberAttributes - CardId: {0}", cardId);
            logger.Debug(config.LSKey.Key, "SetMemberAttributes Request - " + Serialization.ToXml(root, true));
            centralWS2.UpdateMemberAttributes(ref respCode, ref errorText, XMLHelper.GetString(cardId), ref root);
            logger.Debug(config.LSKey.Key, "SetMemberAttributes Response - " + Serialization.ToXml(root, true));
            HandleWS2ResponseCode("SetMemberAttributes", respCode, errorText, ref stat, index);
            logger.StatisticEndSub(ref stat, index);
        }
        #endregion

        #region Altria Phase II - Login for existing members
        public MemberContact ContactCreate2(MemberContact contact, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;
            ContactMapping map = new ContactMapping(config.IsJson, LSCVersion);

            string clubId = string.Empty;
            string cardId = string.Empty;
            string contId = string.Empty;
            string acctId = string.Empty;
            string schmId = string.Empty;
            decimal point = 0;

            centralWS2 = new OmniWrapper2.OmniWrapper2();
            string url = config.SettingsGetByKey(ConfigKey.BOUrl);
            centralWS2.Url = url.Replace("RetailWebServices", "OmniWrapper2");
            centralWS2.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralWS2.PreAuthenticate = true;
            centralWS2.AllowAutoRedirect = true;
            centralWS2.Credentials = new System.Net.NetworkCredential(
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUser.ToString()).Value,
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOPassword.ToString()).Value);

            OmniWrapper2.RootMemberContactCreate root = map.MapToRoot2(contact);
            //LSCentral.RootMemberContactCreate root = map.MapToRoot(contact);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Request - " + Serialization.ToXml(root, true));

            //centralWS.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
            centralWS2.MemberContactCreate(ref respCode, ref errorText, ref clubId, ref schmId, ref acctId, ref contId, ref cardId, ref point, ref root);
            HandleWS2ResponseCode("MemberContactCreate", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Response - ClubId: {0}, SchemeId: {1}, AccountId: {2}, ContactId: {3}, CardId: {4}, PointsRemaining: {5}",
                clubId, schmId, acctId, contId, cardId, point);

            logger.StatisticEndSub(ref stat, index);
            MemberContact cont = new MemberContact(contId)
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                MiddleName = contact.MiddleName,
                Name = contact.Name
            };
            cont.Cards = new List<Card>();
            cont.Cards.Add(new Card(cardId)
            {
                ClubId = clubId,
            });
            cont.Account = new Account(acctId)
            {
                Scheme = new Scheme(schmId)
                {
                    Club = new Club(clubId)
                },
            };
            return cont;
        }
        #endregion

        #region Altria Phase II - Altria Offer Retrieval
        public void RetrievePersonalizedOfferForCardId(string cardId, Statistics stat)
        {
            logger.StatisticStartSub(true, ref stat, out int index);

            string respCode = string.Empty;
            string errorText = string.Empty;

            centralWS2 = new OmniWrapper2.OmniWrapper2();
            string url = config.SettingsGetByKey(ConfigKey.BOUrl);
            centralWS2.Url = url.Replace("RetailWebServices", "OmniWrapper2");
            centralWS2.Timeout = config.SettingsIntGetByKey(ConfigKey.BOTimeout) * 1000;  //millisecs,  60 seconds
            centralWS2.PreAuthenticate = true;
            centralWS2.AllowAutoRedirect = true;
            centralWS2.Credentials = new System.Net.NetworkCredential(
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUser.ToString()).Value,
                                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOPassword.ToString()).Value);

            logger.Debug(config.LSKey.Key, "RetrievePersonalizedOfferForCardId Request - cardId: " + cardId);
            centralWS2.RetrievePersonalizedOffer(ref respCode, ref errorText, cardId);
            HandleWS2ResponseCode("RetrievePersonalizedOfferForCardId", respCode, errorText, ref stat, index);
            logger.Debug(config.LSKey.Key, "MemberContactCreate Response received");
            logger.StatisticEndSub(ref stat, index);
        }
        #endregion
    }
}
