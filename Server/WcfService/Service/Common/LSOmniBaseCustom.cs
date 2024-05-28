using System;
using System.Collections.Generic;
using System.Net;
using LSOmni.BLL;
using LSOmni.BLL.Loyalty;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

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
                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                List<string> list = customLoyBll.GetAgeCheckerReply(cardId, firstName, lastName, dobDT, phoneNo, address, city, state, zip, email, tobaccoValue, cigValue, cigarValue, dipValue, onpValue, snusValue, stat);
                logger.Debug(config.LSKey.Key, "GetAgeCheckerReply received results {0} {1} {2} {3} {4}", list[0], list[1], list[2], list[3], list[4]);
                logger.Debug(config.LSKey.Key, "GetAgeCheckerReply is saving contact data");
                MemberContact buyerContact = customLoyBll.ContactGetByCardId(cardId, stat);
                string origBuyerEmail = buyerContact.Email;
                buyerContact.AlternateId = list[0];
                buyerContact.Profiles.Clear(); //we don't want to update the member attributes any further
                if ((!string.IsNullOrEmpty(list[2]) && ((string)list[2]).ToUpper().Equals(Constants.REPLY_ACCEPTED)))
                {
                    buyerContact.FirstName = firstName;
                    buyerContact.LastName = lastName;
                    Address addressObject = new Address
                    {
                        Id = "home",
                        Type = AddressType.Residential,
                        Address1 = address,
                        City = city,
                        PostCode = zip,
                        StateProvinceRegion = state,
                        Country = "US"
                    };
                    if (!string.IsNullOrEmpty(phoneNo))
                    {
                        addressObject.PhoneNumber = phoneNo;
                    }
                    buyerContact.Addresses = new List<Address>();
                    buyerContact.Addresses.Add(addressObject);
                    buyerContact.BirthDay = dobDT;
                    if (Validation.IsValidEmail(email))
                    {
                        buyerContact.Email = email;
                    }
                } 
                try
                {
                    logger.Debug(config.LSKey.Key, "GetAgeCheckerReply is saving contact data");
                    contactBLL.ContactUpdate(buyerContact, false, stat);
                }
                catch (LSOmniServiceException lsose)
                {
                    logger.Debug(config.LSKey.Key, "GetAgeCheckerReply encountered error while saving contact: {0}", lsose.Message);
                    buyerContact.Email = origBuyerEmail;
                    logger.Debug(config.LSKey.Key, "GetAgeCheckerReply is trying again to save contact data");
                    contactBLL.ContactUpdate(buyerContact, false, stat);
                }
                customLoyBll.RetrievePersonalizedOfferForCardId(cardId, stat);
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

        public virtual List<string> GetAgeCheckerStatus(string cardId, string UUID)
        {
            if (cardId == null)
                cardId = string.Empty;
            if (UUID == null)
                UUID = string.Empty;
            Statistics stat = logger.StatisticStartMain(config, serverUri);
            try
            {
                logger.Debug(config.LSKey.Key, "GetAgeCheckerStatus was called");
                logger.Debug(config.LSKey.Key, "cardId:{0} UUID:{1}", cardId, UUID);
                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                List<string> list = customLoyBll.GetAgeCheckerStatus(cardId, UUID, stat);
                ContactBLL contactBLL = new ContactBLL(config, clientTimeOutInSeconds);
                logger.Debug(config.LSKey.Key, "GetAgeCheckerStatus received results {0} {1} {2} {3} {4} {5}", list[0], list[1], list[2], list[3], list[4], list[5]);

                if (!string.IsNullOrEmpty(list[5]))
                {
                    logger.Debug(config.LSKey.Key, "GetAgeCheckerStatus is saving contact data");
                    MemberContact buyerContact = customLoyBll.ContactGetByCardId(cardId, stat);
                    string origBuyerEmail = buyerContact.Email;
                    string[] buyerFields = list[5].Split(';');
                    buyerContact.FirstName = buyerFields[0];
                    buyerContact.LastName = buyerFields[1];
                    Address addressObject = new Address
                    {
                        Id = "home",
                        Type = AddressType.Residential,
                        Address1 = buyerFields[2],
                        City = buyerFields[3],
                        PostCode = buyerFields[4],
                        StateProvinceRegion = buyerFields[5],
                        Country = buyerFields[6]
                    };
                    if (!string.IsNullOrEmpty(buyerFields[11]))
                    {
                        addressObject.PhoneNumber = buyerFields[11];
                    }
                    buyerContact.Addresses = new List<Address>();
                    buyerContact.Addresses.Add(addressObject);
                    int.TryParse(buyerFields[7], out int dayOfBirth);
                    int.TryParse(buyerFields[8], out int monthOfBirth);
                    int.TryParse(buyerFields[9], out int yearOfBirth);
                    buyerContact.BirthDay = new DateTime(yearOfBirth, monthOfBirth, dayOfBirth);
                    if (!string.IsNullOrEmpty(buyerFields[10])) {
                        buyerContact.Email = buyerFields[10];
                    }
                    buyerContact.Profiles.Clear(); //we don't want to update the member attributes any further
                    try
                    {
                        contactBLL.ContactUpdate(buyerContact, false, stat);
                    }
                    catch (LSOmniServiceException lsose)
                    {
                        logger.Debug(config.LSKey.Key, "GetAgeCheckerStatus encountered error while saving contact: {0} {1}", lsose.Message);
                        buyerContact.Email = origBuyerEmail;
                        contactBLL.ContactUpdate(buyerContact, false, stat);
                    }
                    customLoyBll.RetrievePersonalizedOfferForCardId(cardId, stat);
                }
                else
                    logger.Debug(config.LSKey.Key, "GetAgeCheckerReply did NOT save contact date");
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0} UUID:{1}", cardId, UUID);
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

        #region Altria Phase III - Remember Attributes
        public virtual List<Profile> ProfilesByCardIdGet(string cardId)
        {
            if (cardId == null)
                cardId = string.Empty;

            Statistics stat = logger.StatisticStartMain(config, serverUri);

            try
            {
                logger.Debug(config.LSKey.Key, "ProfilesByCardIdGet was called");
                logger.Debug(config.LSKey.Key, "cardId:{0} ", cardId);

                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                List<Profile> list = customLoyBll.ProfilesByCardIdGet(cardId, stat);
                return list;
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0}", cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }
        #endregion

        #region Altria Phase III - save contact info
        public virtual MemberContact ContactGetByCardId2(string cardId)
        {
            if (cardId == null)
                cardId = string.Empty;

            Statistics stat = logger.StatisticStartMain(config, serverUri);
            try
            {
                logger.Debug(config.LSKey.Key, "LSOmniBaseCustom ContactGetByCardId was called");
                logger.Debug(config.LSKey.Key, "cardId:{0} ", cardId);

                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                return customLoyBll.ContactGetByCardId(cardId, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "cardId:{0}", cardId);
                return null; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
            
        }
        #endregion

        #region Altria Phase III - Log terms acceptance 
        public virtual bool LogTermsPolicyAcceptance(string loginID, string deviceID, string termsCondVersion, string privacyPolicyVersion)
        {
            if (loginID == null)
                loginID = string.Empty;
            if (deviceID == null)
                deviceID = string.Empty;
            if (termsCondVersion == null)
                termsCondVersion = string.Empty;
            if (privacyPolicyVersion == null)
                privacyPolicyVersion = string.Empty;
            Statistics stat = logger.StatisticStartMain(config, serverUri);
            try
            {
                logger.Debug(config.LSKey.Key, "LogTermsPolicyAcceptance was called");
                logger.Debug(config.LSKey.Key, "loginID:{0} deviceID:{1} termsCondVersion:{2} privacyPolicyVersion:{3}", loginID, deviceID, termsCondVersion, privacyPolicyVersion);
                CustomLoyBLL customLoyBll = new CustomLoyBLL(config, clientTimeOutInSeconds);
                return customLoyBll.LogTermsPolicyAcceptance(loginID, deviceID, termsCondVersion, privacyPolicyVersion, stat);
            }
            catch (Exception ex)
            {
                HandleExceptions(ex, "loginID:{0} deviceID:{1} termsCondVersion:{2} privacyPolicyVersion:{3}",
                    loginID, deviceID, termsCondVersion, privacyPolicyVersion);
                return false; //never gets here
            }
            finally
            {
                logger.StatisticEndMain(stat);
            }
        }
        #endregion
    }
}
