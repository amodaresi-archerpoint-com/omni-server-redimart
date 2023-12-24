using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.DiscountEngine.DataModels;
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
            string eaivValue = "1";
            if (ret[1].ToUpper().Equals(Constants.STATUS_OK))
            {
                if (ret[2].ToUpper().Equals(Constants.REPLY_ACCEPTED)) eaivValue = "2";
                if (ret[2].ToUpper().Equals(Constants.REPLY_DENIED)) eaivValue = "3";
                BOCustom.SetMemberAttributes(cardId, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue, eaivValue, stat);
            }
            return ret;
        }

        public virtual List<string> GetAgeCheckerStatus(string cardId, string UUID, Statistics stat)
        {
            List<string> ret = BOCustom.GetAgeCheckerStatus(stat, UUID);
            string eaivValue = "1";
            if (ret[1].ToUpper().Equals(Constants.STATUS_OK))
            {
                if (ret[2].ToUpper().Equals(Constants.REPLY_ACCEPTED)) eaivValue = "2";
                if (ret[2].ToUpper().Equals(Constants.REPLY_DENIED)) eaivValue = "3";
                BOCustom.SetMemberAttributes(cardId, null, null, null, null, null, null, eaivValue, stat);
            }
            return ret;
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
            if (config.SettingsBoolGetByKey(ConfigKey.Allow_Dublicate_Email) == false && BOLoyConnection.ContactGet(ContactSearchType.Email, contact.Email, stat) != null)
                throw new LSOmniServiceException(StatusCode.EmailExists, "Email already exists: " + contact.UserName);

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
            return ("WEB-" + userName);
        }

        public new void SecurityCheck()
        {
            base.SecurityCheck();
        }
        #endregion

        #region Altria Phase II - Altria Offer Retrieval
        public void RetrievePersonalizedOfferForCardId(string cardId, Statistics stat)
        {
            BOCustom.RetrievePersonalizedOfferForCardId(cardId, stat);
        }
        #endregion
    }
}


