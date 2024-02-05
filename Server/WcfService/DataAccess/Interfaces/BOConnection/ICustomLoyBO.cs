﻿using System;
using System.Collections.Generic;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.DataAccess.Interface.BOConnection
{
    public interface ICustomLoyBO
    {
        #region Altria Phase I - Log ad impression
        void AltriaLogEntryCreate(string storeId, string offerId, string cardId, int activityType, int channelType);
        #endregion

        #region Altria Phase II - Filtration of Offers
        List<PublishedOffer> PublishedOffersGet(string cardId, string itemId, string storeId, Statistics stat);
        #endregion

        #region Altria Phase II - AgeChecker
        List<string> GetAgeCheckerReply(string cardId, string firstName, string lastName, DateTime dobDT, string phoneNo, string address, string city, string state, string zip, string email, Statistics stat);

        List<string> GetAgeCheckerStatus(Statistics stat, string UUID);
        #endregion

        #region Altria Phase II - Member Attributes
        void SetMemberAttributes(string cardId, string tobaccoValue, string cigValue, string cigarValue, string dipValue, string onpValue, string snusValue, string eaivValue, Statistics stat);
        #endregion

        #region Altria Phase II - Login for existing members
        MemberContact ContactCreate(MemberContact contact, Statistics stat);
        #endregion

        #region Altria Phase II - Altria Offer Retrieval
        void RetrievePersonalizedOfferForCardId(string cardId, Statistics stat);
        #endregion

        #region Altria Phase III - Remember Attributes
        List<Profile> ProfilesByCardIdGet(string cardId, Statistics stat);
        #endregion
    }
}
