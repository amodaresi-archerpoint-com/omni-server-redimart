using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;

namespace LSOmni.Service
{
    public partial class LSOmniBase
    {
        public virtual string MyCustomFunction(string data)
        {
            CustomBLL myBLL = new CustomBLL(config);
            return myBLL.MyCustomFunction(data);
        }

        #region Phase II - Filtration of offers
        public virtual List<PublishedOffer> PublishedOffersGetByCardId2(string cardId, string itemId, string storeId)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (itemId == null)
                itemId = string.Empty;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "PublishedOffersGetByCardId2 was called");
                logger.Debug(config.LSKey.Key, "itemId:{0} cardId:{1} storeId:{2}", itemId, cardId);

                OfferBLL bll = new OfferBLL(config, clientTimeOutInSeconds);
                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);

                List<PublishedOffer> list = customLoyBll.PublishedOffersGet(cardId, itemId, string.Empty, stat);
                foreach (PublishedOffer it in list)
                {
                    logger.Debug(config.LSKey.Key, "PublishedOffersGetByCardId2 about to call AltriaLogEntryCreate");
                    customLoyBll.AltriaLogEntryCreate(storeId, it.Id, cardId, 2, 3);
                    logger.Debug(config.LSKey.Key, "PublishedOffersGetByCardId2 returned from calling AltriaLogEntryCreate");
                    foreach (ImageView iv in it.Images)
                    {
                        iv.StreamURL = GetImageStreamUrl(iv);
                    }
                    foreach (OfferDetails od in it.OfferDetails)
                    {
                        od.Image.StreamURL = GetImageStreamUrl(od.Image);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "itemId:{0} cardId:{1}", itemId, cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }
        #endregion
        #region Phase II - Age checker
        public virtual List<string> GetAgeCheckerReply(string cardId, string firstName, string lastName, DateTime dobDT, string phoneNo, string address, string city, string state, string zip, string email, string tobaccoValue, string cigValue, string cigarValue,
                                                                    string dipValue, string onpValue, string snusValue)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (firstName == null)
                firstName = string.Empty;
            if (lastName == null)
                lastName = string.Empty;
            if (phoneNo == null)
                phoneNo = string.Empty;
            if (address == null)
                address = string.Empty;
            if (state == null)
                state = string.Empty;
            if (zip == null)
                zip = string.Empty;
            if (email == null)
                email = string.Empty;
            if (tobaccoValue == null)
                tobaccoValue = string.Empty;
            if (cigValue == null)
                cigValue = string.Empty;
            if (cigarValue == null)
                cigarValue = string.Empty;
            if (dipValue == null)
                dipValue = string.Empty;
            if (onpValue == null)
                onpValue = string.Empty;
            if (snusValue == null)
                snusValue = string.Empty;
            if (dobDT == null)
                dobDT = DateTime.MinValue;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "GetAgeCheckerReply was called");
                logger.Debug(config.LSKey.Key, "cardId:{0} firstName:{1} lastName:{2} dobDT:{3} phoneNo:{4} address:{5} city:{6} state:{7} zip:{8} email:{9} tobaccoValue:{10} cigValue:{11} cigarValue:{12} dipValue:{13} onpValue:{14} snusValue:{15}",
                    cardId, firstName, lastName, dobDT.ToString(), phoneNo, address, city, state, zip, email, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue);
                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                List<string> list = customLoyBll.GetAgeCheckerReply(cardId, firstName, lastName, dobDT, phoneNo, address, city, state, zip, email, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue, stat);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} firstName:{1} lastName:{2} dobDT:{3} phoneNo:{4} address:{5} city:{6} state:{7} zip:{8} email:{9} tobaccoValue:{10} cigValue:{11} cigarValue:{12} dipValue:{13} onpValue:{14} snusValue:{15}",
                    cardId, firstName, lastName, dobDT.ToString(), phoneNo, address, city, state, zip, email, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }

        }
        #endregion
        #region Phase II - Login for existing members
        public virtual MemberContact ContactCreate2(MemberContact contact, bool doLogin)
        {
            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, $"DoLogin:{doLogin} > " + LogJson(contact));

                if (contact.Cards == null)
                    contact.Cards = new List<Card>();

                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);//not using security token here, so no security checks
                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                MemberContact contactOut = customLoyBll.ContactCreate(contact, doLogin, stat);

                if (doLogin)
                {
                    if (string.IsNullOrWhiteSpace(contact.Authenticator))
                        contactOut = contactBLL.Login(contact.UserName, contact.Password, true, contact.LoggedOnToDevice.Id, stat);
                    else
                        contactOut = contactBLL.SocialLogon(contact.Authenticator, contact.AuthenticationId, contact.LoggedOnToDevice.Id, string.Empty, true, stat);
                }
                customLoyBll.SecurityCheck();

                contactOut.Environment.Version = this.Version();
                ContactSetLocation(contactOut);
                return contactOut;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "ContactCreate()");
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }

        #endregion
    }
}
