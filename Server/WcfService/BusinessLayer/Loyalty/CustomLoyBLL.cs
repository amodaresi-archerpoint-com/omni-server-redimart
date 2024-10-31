using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSOmni.DataAccess.Firebase;
//using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.BLL.Loyalty
{
    public class CustomLoyBLL : BaseLoyBLL
    {
        private ICustomLoyBO iBOCustomConnection = null;

        protected ICustomLoyBO BOCustom
        {
            get
            {
                if (iBOCustomConnection == null)
                    iBOCustomConnection = GetBORepository<ICustomLoyBO>(config.LSKey.Key, config.IsJson);
                return iBOCustomConnection;
            }
        }

        public CustomLoyBLL(BOConfiguration config, int timeoutInSeconds)
            : base(config, timeoutInSeconds)
        {
        }

        public CustomLoyBLL(BOConfiguration config) : base(config, 0)
        {
        }

        #region Altria Phase I - Log ad impression
        public void AltriaLogEntryCreate(string storeId, string offerId, string cardId, int activityType, int channelType)
        {
            //In Phase I we added ability to log impressions (seeing offer page) of altria api offers
            BOCustom.AltriaLogEntryCreate(storeId, offerId, cardId, activityType, channelType);
        }
        #endregion

        #region Altria Phase II - Filtration of Offers
        public virtual List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat)
        {
            // In Phase II we made our own published offer function, to filter the published altria offers in case user is not age verified yet
            return BOCustom.PublishedOffersGet(cardId, itemId, storeId, stat);
        }
        #endregion

        #region Altria Phase II - AgeChecker & Member Attributes
        public virtual List<string> GetAgeCheckerReply(string cardId, string firstName, string lastName, DateTime dobDT, string phoneNo, string address, string city, string state, string zip, string email, string tobaccoValue, string cigValue, string cigarValue,
                                                                    string dipValue, string onpValue, string snusValue, Statistics stat)
        {
            List<string> ret = BOCustom.GetAgeCheckerReply(cardId, firstName, lastName, dobDT, phoneNo, address, city, state, zip, email, stat);
            string eaivValue = Constants.REDI_PENDING;
            if (ret[1].ToUpper().Equals(Constants.STATUS_OK))
            {
                if (ret[2].ToUpper().Equals(Constants.REPLY_ACCEPTED)) eaivValue = Constants.REDI_ACCEPTED;
                if (ret[2].ToUpper().Equals(Constants.REPLY_DENIED)) eaivValue = Constants.REDI_DENIED;

                Dictionary<string, string> myDictionary = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(tobaccoValue)) { myDictionary.Add(Constants.CAT_TOBACCO, tobaccoValue); }
                if (!string.IsNullOrEmpty(cigValue)) { myDictionary.Add(Constants.AD_CONSENT_CIGARETTE, cigValue); }
                if (!string.IsNullOrEmpty(cigarValue)) { myDictionary.Add(Constants.AD_CONSENT_CIGAR, cigarValue); }
                if (!string.IsNullOrEmpty(dipValue)) { myDictionary.Add(Constants.AD_CONSENT_DIP, dipValue); }
                if (!string.IsNullOrEmpty(onpValue)) { myDictionary.Add(Constants.AD_CONSENT_ONP, onpValue); }
                if (!string.IsNullOrEmpty(snusValue)) { myDictionary.Add(Constants.AD_CONSENT_SNUS, snusValue); }
                if (!string.IsNullOrEmpty(eaivValue)) { myDictionary.Add(Constants.AGE_VERIFIED, eaivValue); }

                BOCustom.SetMemberAttributes(cardId, myDictionary, stat);
            }
            return ret;
        }

        public virtual List<string> GetAgeCheckerStatus(string cardId, string UUID, Statistics stat)
        {
            List<string> ret = BOCustom.GetAgeCheckerStatus(stat, UUID);
            string eaivValue = Constants.REDI_PENDING;
            if (ret[1].ToUpper().Equals(Constants.STATUS_OK))
            {
                if (ret[2].ToUpper().Equals(Constants.REPLY_ACCEPTED)) eaivValue = Constants.REDI_ACCEPTED;
                if (ret[2].ToUpper().Equals(Constants.REPLY_DENIED)) eaivValue = Constants.REDI_DENIED;

                Dictionary<string, string> myDictionary = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(eaivValue)) { myDictionary.Add(Constants.AGE_VERIFIED, eaivValue); }

                BOCustom.SetMemberAttributes(cardId, myDictionary, stat);
            }
            return ret;
        }

        public virtual void RetrievePersonalizedOfferForCardId(string cardId, Statistics stat)
        {
            BOCustom.RetrievePersonalizedOfferForCardId(cardId, stat);
        }
        #endregion

        #region Altria Phase II - Login for existing members
        public virtual MemberContact ContactCreate(MemberContact contact, bool doLogin, Statistics stat)
        {
            //minor validation before going further 
            if (contact == null)
                throw new LSOmniException(StatusCode.ObjectMissing, "Null Object");

            if (string.IsNullOrEmpty(contact.UserName) == false)
            {
                if (Validation.IsValidUserName(contact.UserName) == false)
                    throw new LSOmniException(StatusCode.UserNameInvalid, "Validation of user name failed");
                contact.UserName = contact.UserName.Trim();
                contact.Password = contact.Password.Trim();
            }

            if (string.IsNullOrEmpty(contact.Password) == false && Validation.IsValidPassword(contact.Password) == false)
                throw new LSOmniException(StatusCode.PasswordInvalid, "Validation of password failed");

            if (Validation.IsValidEmail(contact.Email) == false)
                throw new LSOmniException(StatusCode.EmailInvalid, "Validation of email failed");

            //check if user exist before calling NAV
            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false)
            {
                MemberContact contactFromWS = BOCustom.ContactGetByEmail(contact.Email, stat);
                if (contactFromWS != null)
                {
                    if (contactFromWS.Cards.Find(x => x.Id.ToUpper().Equals(contact.UserName.ToUpper())) == null)
                        throw new LSOmniServiceException(StatusCode.EmailExists, "Email " + contact.Email + " is already used by user: " + contactFromWS.Id + "/" + contactFromWS.UserName);

                }
            }

            if (string.IsNullOrEmpty(contact.AuthenticationId) == false)
            {
                contact.AuthenticationId = contact.AuthenticationId.Trim();
                contact.Authenticator = contact.Authenticator.Trim();
            }

            //Web pages do not need to fill in a Card
            // the deviceId will be set to the UserName.
            if (contact.Cards == null)
                contact.Cards = new List<Card>();

            if (contact.LoggedOnToDevice == null)
                contact.LoggedOnToDevice = new Device();
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.Id))
                contact.LoggedOnToDevice.Id = GetDefaultDeviceId(contact.UserName);
            if (string.IsNullOrWhiteSpace(contact.LoggedOnToDevice.DeviceFriendlyName))
                contact.LoggedOnToDevice.DeviceFriendlyName = "Web application";

            if (contact.Profiles == null)
                contact.Profiles = new List<Profile>();

            MemberContact newcontact = BOCustom.ContactCreate(contact, stat);

            //login moved to upstream

            return newcontact;
        }
        private string GetDefaultDeviceId(string userName)
        {
            return ("WEB-" + userName.ToUpper());  //anmo
        }

        public new void SecurityCheck()
        {
            base.SecurityCheck();
        }

        public virtual MemberContact ContactGetByEmail(string cardId, Statistics stat)
        {
            return BOCustom.ContactGetByEmail(cardId, stat);
        }
        #endregion

        #region Altria Phase III - Remember Attributes
        public virtual List<Profile> ProfilesByCardIdGet(string cardId, Statistics stat)
        {
            return BOCustom.ProfilesByCardIdGet(cardId, stat);
        }
        #endregion

        #region Altria Phase III - save contact info
        public virtual MemberContact ContactGetByCardId(string cardId, Statistics stat)
        {
            return BOCustom.ContactGetByCardId(cardId, stat);
        }
        #endregion

        #region Altria Phase III - Log terms acceptance 
        public virtual bool LogTermsPolicyAcceptance(string loginID, string deviceID, string termsCondVersion, string privacyPolicyVersion, Statistics stat)
        {
            return BOCustom.LogTermsPolicyAcceptance(loginID, deviceID, termsCondVersion, privacyPolicyVersion, stat);
        }
        #endregion

        #region Altria Phase IV - Reset consent
        public virtual int SaveConsent(string cardId, string tobaccoValue, string cigValue, string cigarValue,
                                                                    string dipValue, string onpValue, string snusValue, Statistics stat)
        {
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(tobaccoValue)) { myDictionary.Add(Constants.CAT_TOBACCO, tobaccoValue); }
            if (!string.IsNullOrEmpty(cigValue)) { myDictionary.Add(Constants.AD_CONSENT_CIGARETTE, cigValue); }
            if (!string.IsNullOrEmpty(cigarValue)) { myDictionary.Add(Constants.AD_CONSENT_CIGAR, cigarValue); }
            if (!string.IsNullOrEmpty(dipValue)) { myDictionary.Add(Constants.AD_CONSENT_DIP, dipValue); }
            if (!string.IsNullOrEmpty(onpValue)) { myDictionary.Add(Constants.AD_CONSENT_ONP, onpValue); }
            if (!string.IsNullOrEmpty(snusValue)) { myDictionary.Add(Constants.AD_CONSENT_SNUS, snusValue); }

            BOCustom.SetMemberAttributes(cardId, myDictionary, stat);
            return 0;
        }
        #endregion

        #region Phase V - Firebase
        public virtual string SendPushNotificationToTopic(string topic, string title, string message, Statistics stat)
        {
            FirebaseCustom firebaseCustom = new FirebaseCustom();
            return firebaseCustom.SendPushNotificationToTopic(topic, title, message, stat);
        }
        #endregion
    }
}


