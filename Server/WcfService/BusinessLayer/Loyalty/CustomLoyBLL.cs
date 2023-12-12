using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.BOConnection;
using LSRetail.Omni.DiscountEngine.DataModels;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;

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
            if (ret[2].ToUpper().Equals(Constants.REPLY_ACCEPTED)) eaivValue = "2";
            if (ret[2].ToUpper().Equals(Constants.REPLY_DENIED)) eaivValue = "3";
            BOCustom.SetMemberAttributes(cardId, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue, eaivValue, stat);
            return ret;
        }
        #endregion

    }
}


