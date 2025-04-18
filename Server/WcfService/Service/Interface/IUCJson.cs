﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Replication;
using LSRetail.Omni.Domain.DataModel.Base.Requests;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.SalesEntries;
using LSRetail.Omni.Domain.DataModel.Base.Hierarchies;
using LSRetail.Omni.Domain.DataModel.Loyalty.Replication;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;
using LSRetail.Omni.Domain.DataModel.Loyalty.OrderHosp;
using LSRetail.Omni.Domain.DataModel.Activity.Activities;
using LSRetail.Omni.Domain.DataModel.Activity.Client;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Payment;
using LSRetail.Omni.Domain.DataModel.ScanPayGo.Setup;

namespace LSOmni.Service
{
    // Returns json
    [ServiceContract(Namespace = "http://lsretail.com/LSOmniService/EComm/2017/Json")]
    [ServiceKnownType(typeof(StatusCode))]
    public interface IUCJson
    {
        // COMMON ////////////////////////////////////////////////////////////

        #region Helpers Common

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string Ping();

        [OperationContract]
        [WebGet(UriTemplate = "/Ping", ResponseFormat = WebMessageFormat.Json)]
        string PingGet(); //REST http://localhost/CommerceService/ucjson.svc/ping

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OmniEnvironment Environment();

        #endregion

        #region Altria phase II - Filtration of Offers
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PublishedOffer> PublishedOffersGetByCardId2(string cardId, string itemId, string storeId);
        #endregion

        #region Altria Phase II - AgeChecker && Member attributes
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAgeCheckerReply(string cardId, string firstName, string lastName, DateTime dobDT, string phoneNo, string address, string city, string state, string zip, string email, string tobaccoValue, string cigValue, string cigarValue,
                                                                    string dipValue, string onpValue, string snusValue);
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAgeCheckerStatus(string cardId, string UUID);
        #endregion

        #region Altria Phase II - Login For Existing Cardholders
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactCreate2(MemberContact contact, bool doLogin);
        #endregion

        #region Altria Phase III - Remember Attributes
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesByCardIdGet(string cardId);
        #endregion

        #region Altria Phase III - save contact data
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetByCardId2(string cardId);
        #endregion

        #region Altria Phase III - Log terms acceptance 
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool LogTermsPolicyAcceptance(string loginID, string deviceID, string termsCondVersion, string privacyPolicyVersion);
        #endregion

        #region Altria Phase IV - Minimum Version
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        int GetMinimumRequiredBuildNr();
        #endregion

        #region Altria Phase IV - Reset Consent
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        int SaveConsent(string cardId, string tobaccoValue, string cigValue, string cigarValue,
                                                                    string dipValue, string onpValue, string snusValue);
        #endregion

        #region Phase V - Firebase
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string SendPushNotificationToTopic(string topic, string title, string message);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string SendPushNotificationToToken(string token, string title, string message);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SubscribeTokenToTopic(string token, string topic);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool RegisterDevice(string loginId, string deviceId, string firebaseToken);
        #endregion

        #region Discount, Offers and GiftCards

        /// <summary>
        /// Get Published Offers for Member Card Id
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo
        /// </remarks>
        /// <param name="cardId">Member Card Id to look for</param>
        /// <param name="itemId">Only show Offers for this item</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PublishedOffer> PublishedOffersGetByCardId(string cardId, string itemId);

        /// <summary>
        /// Get related items in a published offer
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_PUBLISHED_OFFER_ITEMS
        /// </remarks>
        /// <param name="pubOfferId">Published offer id</param>
        /// <param name="numberOfItems">Number of items to return</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsGetByPublishedOfferId(string pubOfferId, int numberOfItems);

        /// <summary>
        /// Get discounts for items. Send in empty string for loyaltySchemeCode if getting anonymously.
        /// </summary>
        /// <param name="storeId">Store Id</param>
        /// <param name="itemIds">List of item ids to check for discounts</param>
        /// <param name="loyaltySchemeCode">[OPTIONAL] Loyalty scheme code for a user</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ProactiveDiscount> DiscountsGet(string storeId, List<string> itemIds, string loyaltySchemeCode);

        /// <summary>
        /// Get balance of a gift card.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetDataEntryBalance
        /// </remarks>
        /// <param name="cardNo">Gift card number</param>
        /// <param name="pin">Gift card pin number</param>
        /// <param name="entryType">Gift card Entry type. If empty, GiftCard_DataEntryType from TenantConfig is used</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        GiftCard GiftCardGetBalance(string cardNo, int pin, string entryType);

        /// <summary>
        /// Get activity history for Gift Card
        /// </summary>
        /// <param name="cardNo">Gift card number</param>
        /// <param name="pin">Gift card pin number</param>
        /// <param name="entryType">Gift card Entry type. If empty, GiftCard_DataEntryType from TenantConfig is used</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<GiftCardEntry> GiftCardGetHistory(string cardNo, int pin, string entryType);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Advertisement> AdvertisementsGetById(string id, string contactId);

        #endregion

        #region Notification

        /// <summary>
        /// Get all Order Notification for a Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : LOAD_MEMBER_DIR_MARK_INFO<p/>
        /// LS Central WS2 : GetDirectMarketingInfo
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="numberOfNotifications">Number of notifications to return</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Notification> NotificationsGetByCardId(string cardId, int numberOfNotifications);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool NotificationsUpdateStatus(string cardId, List<string> notificationIds, NotificationStatus notificationStatus);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Notification NotificationGetById(string notificationId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PushNotificationSave(PushNotificationRequest pushNotificationRequest);

        #endregion

        #region OneList

        /// <summary>
        /// Delete Basket or Wish List By OneList Id
        /// </summary>
        /// <param name="oneListId"></param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListDeleteById(string oneListId);

        /// <summary>
        /// Get Basket or all Wish Lists by Member Card Id
        /// </summary>
        /// <param name="cardId">Contact Id</param>
        /// <param name="listType">0=Basket,1=Wish</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<OneList> OneListGetByCardId(string cardId, ListType listType, bool includeLines);

        /// <summary>
        /// Get Basket or Wish List by OneList Id
        /// </summary>
        /// <param name="id">List Id</param>
        /// <param name="includeLines">Include detail lines</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListGetById(string id, bool includeLines);

        /// <summary>
        /// Save Basket or Wish List
        /// </summary>
        /// <remarks>
        /// OneList can be saved, for both Member Contact and Anonymous Users.
        /// Member Contact can have one or more Member Cards and each Card can have one WishList and one Basket
        /// For Anonymous User, keep CardId empty and OneListSave will return OneList Id back that should be store with the session for the Anonymous user, 
        /// as Commerce Service for LS Central does not store any information for Anonymous Users.<p/>
        /// Used OneListGetById to get the OneList back.<p/>
        /// NOTE: If no Name is provided with OneList, system will look up contact to pull the name, this can slow the process.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "oneList": {
        ///         "CardId": "10021",
        ///         "Items": [{
        ///             "Image": {
        ///                "Id": "40020"
        ///             },
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        ///         "ListType": 0,
        ///         "Name": "Tom Tomson",
        ///         "StoreId": "S0001"
        ///     },
        ///     "calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">List Id</param>
        /// <param name="calculate">Perform Calculation on a Basket and save result with Basket</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListSave(OneList oneList, bool calculate);

        /// <summary>
        /// Calculates OneList Basket Object and returns Order Object
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : EcomCalculateBasket<p/><p/>
        /// This function can be used to send in Basket and convert it to Order.<p/>
        /// Basic Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation<p/>
        /// NOTE: Image Ids are added if not provided with the item or returned from Central, this will result in extra calls, so to speed up things, provide Image object with Image Id only (not including blob)
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Basket with 2 of same Items
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "oneList": {
        ///         "CardId": "10021",
        ///         "Items": [{
        ///             "Image": {
        ///                 "Id": "40020"
        ///             },
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        ///         "ListType": 0,
        ///         "StoreId": "S0001"
        ///     },
        ///     "calculate": true
        /// }
        /// ]]>
        /// </code>
        /// Basket with 2 of same Items and with XMAS Coupon that gives discount 
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "oneList": {
        ///         "CardId": "10021",
        ///         "Items": [{
        ///             "Image": {
        ///                 "Id": "40020"
        ///             },
        ///             "ItemDescription": "Skirt Linda Professional Wear",
        ///             "ItemId": "40020",
        ///             "Quantity": 2,
        ///             "VariantDescription": "YELLOW/38",
        ///             "VariantId": "002"
        ///         }],
        ///         "PublishedOffers": [{
        ///             "Id": "XMAS",
        ///             "Type": "9",
        ///         }],
        ///         "ListType": 0,
        ///         "StoreId": "S0001"
        ///     },
        ///     "calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">OneList Object</param>
        /// <returns>Order Object that can be used to Create Order</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Order OneListCalculate(OneList oneList);

        /// <summary>
        /// Calculates OneList Basket for Hospitality and returns Hospitality Order Object
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MobilePosCalculate<p/><p/>
        /// This function can be used to send in Basket and convert it to Hospitality Order.<p/>
        /// Basic Hospitality Order data is then set for finalize it by setting the Order setting,
        /// Contact Info, Payment and then it can be posted for Creation
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Basket for EasyBurger Restaurant, Cheese Burger Meal, with Jalapeno Popper and Reg Orange Soda
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///   "oneList": {
        ///     "CardId": "10021",
        ///     "ExternalType": 0,
        ///     "IsHospitality": true,
        ///     "Items": [{
        ///         "IsADeal": true,
        ///         "ItemId": "S10025",
        ///         "OnelistSubLines": [{
        ///             "DealLineId": 10000,
        ///             "DealModLineId": 0,
        ///             "Quantity": 1,
        ///             "Type": 1
        ///           }, {
        ///             "DealLineId": 20000,
        ///             "DealModLineId": 70000,
        ///             "Quantity": 1,
        ///             "Type": 1,
        ///             "Uom": "PORTION"
        ///           }, {
        ///             "DealLineId": 30000,
        ///             "DealModLineId": 70000,
        ///             "Quantity": 1,
        ///             "Type": 1,
        ///             "Uom": "REG"
        ///           }],
        ///         "Quantity": 1,
        ///         "UnitOfMeasureId": null
        ///       }],
        ///     "ListType": 0,
        ///     "SalesType": "TAKEAWAY",
        ///     "StoreId": "S0017"
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneList">OneList Object</param>
        /// <returns>Order Object that can be used to Create Order</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderHosp OneListHospCalculate(OneList oneList);

        /// <summary>
        /// Add or remove Item in OneList without sending whole list
        /// </summary>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "onelistId":"1117AC57-10BD-4F7C-974D-FA19B1B027FB",
        ///     "item": {
        ///         "ItemDescription": "T-shirt Linda Wear",
        ///         "ItemId": "40045",
        ///         "Quantity": 1,
        ///         "VariantDescription": "BLACK/40",
        ///         "VariantId": "015"
        ///     },
        ///     "remove": false,
        ///     "calculate": true
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="oneListId">OneList Id</param>
        /// <param name="item">OneList Item to add or remove</param>
        /// <param name="cardId">Card Id of the person making the changes</param>
        /// <param name="remove">true if remove item, else false</param>
        /// <param name="calculate">Recalculate OneList</param>
        /// <returns>Updated OneList</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OneList OneListItemModify(string oneListId, OneListItem item, string cardId, bool remove, bool calculate);

        /// <summary>
        /// Link or remove a Member to/from existing OneList
        /// </summary>
        /// <param name="oneListId">OneList Id to link</param>
        /// <param name="cardId">Card Id to link or remove</param>
        /// <param name="email">Email address to look up Card Id when requesting a Linking</param>
        /// <param name="phone">Phone number to look up Card Id when requesting a Linking</param>
        /// <param name="status">Link action</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OneListLinking(string oneListId, string cardId, string email, string phone, LinkStatus status);

        #endregion

        #region Order

        /// <summary>
        /// Check the quantity available of items in order for certain store, Use with LS Nav 11.0 and later
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : COQtyAvailabilityV2
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="shippingOrder">true if order is to be shipped, false if click and collect</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderAvailabilityResponse OrderCheckAvailability(OneList request, bool shippingOrder);

        /// <summary>
        /// Create Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCreateVx
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Order to be shipped to Customer
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "request": {
        ///         "CardId": "10021",
        ///         "OrderDiscountLines": [],
        ///         "OrderLines": [{
        ///             "Amount": "160.0",
        ///             "DiscountAmount": "0",
        ///             "DiscountPercent": "0",
        ///             "ItemId": "40020",
        ///             "LineNumber": "1",
        ///             "LineType": "0",
        ///             "NetAmount": "128.00",
        ///             "NetPrice": "64.0",
        ///             "Price": "80.0",
        ///             "Quantity": "2",
        ///             "TaxAmount": "32.0",
        ///             "VariantId": "002"
        ///         }],
        ///         "OrderPayments": [{
        ///             "Amount": "160.0",
        ///             "AuthorizationCode": "123456",
        ///             "CardNumber": "10xx xxxx xxxx 1475",
        ///             "CardType": "VISA",
        ///             "CurrencyCode": "GBP",
        ///             "CurrencyFactor": 1,
        ///             "ExternalReference": "MyRef123",
        ///             "LineNumber": 1,
        ///             "PaymentType": "2",
        ///             "PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        ///             "TenderType": "1",
        ///             "TokenNumber": "123456"
        ///         }],
        ///         "OrderType": "0",
        ///         "PaymentStatus": "10",
        ///         "ShipOrder": "1",
        ///         "ShipToAddress": {
        ///             "Address1": "600 Lue Via",
        ///             "Address2": "None",
        ///             "City": "North Viola",
        ///             "Country": "Belgium",
        ///             "PhoneNumber": "555-555-5555",
        ///             "PostCode": "88391-4289",
        ///             "StateProvinceRegion": "None",
        ///             "Type": "0"
        ///         },
        ///         "StoreId": "S0013",
        ///         "TotalAmount": "160.0",
        ///         "TotalDiscount": "0",
        ///         "TotalNetAmount": "128.0"
        ///     },
        ///     "returnOrderIdOnly": 0
        /// }
        /// ]]>
        /// </code>
        /// Order with Manual 10% Discount to be shipped to Customer
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "request": {
        ///         "CardId": "10021",
        ///         "OrderDiscountLines": [{
        ///             "DiscountAmount": "16.0",
        ///             "DiscountPercent": "10.0",
        ///             "DiscountType": "4",
        ///             "LineNumber": "1",
        ///             "No": "10000"
        ///         }],
        ///         "OrderLines": [{
        ///             "Amount": "144.0",
        ///             "DiscountAmount": "16.3",
        ///             "DiscountPercent": "10.0",
        ///             "ItemId": "40020",
        ///             "LineNumber": "1",
        ///             "LineType": "0",
        ///             "NetAmount": "115.20",
        ///             "NetPrice": "64.0",
        ///             "Price": "80.0",
        ///             "Quantity": "2",
        ///             "TaxAmount": "28.0",
        ///             "VariantId": "002"
        ///         }],
        ///         "OrderPayments": [{
        ///             "Amount": "160.0",
        ///             "AuthorizationCode": "123456",
        ///             "CardNumber": "10xx xxxx xxxx 1475",
        ///             "CardType": "VISA",
        ///             "CurrencyCode": "GBP",
        ///             "CurrencyFactor": 1,
        ///             "ExternalReference": "MyRef123",
        ///             "LineNumber": 1,
        ///             "PaymentType": "2",
        ///             "PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        ///             "TenderType": "1",
        ///             "TokenNumber": "123456"
        ///         }],
        ///         "OrderType": "0",
        ///         "PaymentStatus": "10",
        ///         "ShipOrder": "1",
        ///         "ShipToAddress": {
        ///             "Address1": "600 Lue Via",
        ///             "Address2": "None",
        ///             "City": "North Viola",
        ///             "Country": "Belgium",
        ///             "PhoneNumber": "555-555-5555",
        ///             "PostCode": "88391-4289",
        ///             "StateProvinceRegion": "None",
        ///             "Type": "0"
        ///         },
        ///         "StoreId": "S0013",
        ///         "TotalAmount": "144.0",
        ///         "TotalDiscount": "16.0",
        ///         "TotalNetAmount": "115.20"
        ///     },
        ///     "returnOrderIdOnly": 0
        /// }
        /// ]]>
        /// </code>
        /// Order 3 Different payments, Credit Card, Loyalty Points (Currency is LOY and CardNumber is MemberContact Card ID) and Gift Card (CardNumber is the Gift Card Id)
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "request": {
        ///         "CardId": "10021",
        ///         "OrderDiscountLines": [],
        ///         "OrderLines": [{
        ///             "Amount": "160.0",
        ///             "DiscountAmount": "0",
        ///             "DiscountPercent": "0",
        ///             "ItemId": "40020",
        ///             "LineNumber": "1",
        ///             "LineType": "0",
        ///             "NetAmount": "128.00",
        ///             "NetPrice": "64.0",
        ///             "Price": "80.0",
        ///             "Quantity": "2",
        ///             "TaxAmount": "32.0",
        ///             "VariantId": "002"
        ///         }],
        ///         "OrderPayments": [{
        ///             "Amount": "160.0",
        ///             "AuthorizationCode": "123456",
        ///             "CardNumber": "10xx xxxx xxxx 1475",
        ///             "CardType": "VISA",
        ///             "CurrencyCode": "GBP",
        ///             "CurrencyFactor": 1,
        ///             "ExternalReference": "MyRef123",
        ///             "LineNumber": 1,
        ///             "PaymentType": "2",
        ///             "PreApprovedValidDate": "\/Date(1892160000000+1000)\/",
        ///             "TenderType": "1",
        ///             "TokenNumber": "123456"
        ///         },
        ///         {
        ///             "Amount": "200.0",
        ///             "CardNumber": "10021",
        ///             "CurrencyCode": "LOY",
        ///             "CurrencyFactor": "0.1",
        ///             "LineNumber": 2,
        ///             "TenderType": "3"
        ///         },
        ///         {
        ///             "Amount": "20.0",
        ///             "AuthorizationCode": 9999,
        ///             "CardNumber": "1000",
        ///             "CurrencyCode": "GBP",
        ///             "CurrencyFactor": "1.0",
        ///             "LineNumber": 3,
        ///             "TenderType": "4"
        ///         }],
        ///         "OrderType": "0",
        ///         "PaymentStatus": "10",
        ///         "ShipOrder": "1",
        ///         "ShipToAddress": {
        ///             "Address1": "600 Lue Via",
        ///             "Address2": "None",
        ///             "City": "North Viola",
        ///             "Country": "Belgium",
        ///             "PhoneNumber": "555-555-5555",
        ///             "PostCode": "88391-4289",
        ///             "StateProvinceRegion": "None",
        ///             "Type": "0"
        ///         },
        ///         "StoreId": "S0013",
        ///         "TotalAmount": "160.0",
        ///         "TotalDiscount": "0",
        ///         "TotalNetAmount": "128.0"
        ///     },
        ///     "returnOrderIdOnly": 0
        /// }
        /// ]]>
        /// </code>
        /// Order to be collected at Store S0001
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "request": {
        ///         "CardId": "10021",
        ///         "CollectLocation": "S0001",
        ///         "OrderDiscountLines": [],
        ///         "OrderLines": [{
        ///             "Amount": "160.0",
        ///             "ClickAndCollectLine": "1",
        ///             "DiscountAmount": "0",
        ///             "DiscountPercent": "0",
        ///             "ItemId": "40020",
        ///             "LineNumber": "1",
        ///             "LineType": "0",
        ///             "NetAmount": "128.00",
        ///             "NetPrice": "64.0",
        ///             "Price": "80.0",
        ///             "Quantity": "2",
        ///             "StoreId": "S0001",
        ///             "TaxAmount": "32.0",
        ///             "VariantId": "002"
        ///         }],
        ///         "OrderPayments": [],
        ///         "OrderType": "1",
        ///         "PaymentStatus": "0",
        ///         "ShipOrder": "0",
        ///         "ShipToAddress": {},
        ///         "StoreId": "S0013",
        ///         "TotalAmount": "160.0",
        ///         "TotalDiscount": "0",
        ///         "TotalNetAmount": "128.0"
        ///     },
        ///     "returnOrderIdOnly": 0
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <param name="returnOrderIdOnly">Only return Order Id back, not full order object</param>
        /// <returns>SalesEntry object for order if order creation was successful</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry OrderCreate(Order request, bool returnOrderIdOnly);

        /// <summary>
        /// Edit Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderEdit
        /// </remarks>
        /// <param name="request">Updated Order object</param>
        /// <param name="orderId">Order Id to edit</param>
        /// <param name="editType">Type of Order Edit</param>
        /// <param name="returnOrderIdOnly"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry OrderEdit(Order request, string orderId, OrderEditType editType, bool returnOrderIdOnly);

        /// <summary>
        /// Update payments for Customer Order
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId"></param>
        /// <returns>webPreAuthNotAuthorize</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderUpdatePayment(string orderId, string storeId, OrderPayment payment);

        /// <summary>
        /// Create a Hospitality Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CreateHospOrder
        /// </remarks>
        /// <example>
        /// Sample requests including minimum data needed to be able to process the request in LS Commerce<p/>
        /// Sample Order for EasyBurger Restaurant, Cheese Burger Meal, with Jalapeno Popper and Reg Orange Soda.
        /// Based on OneListHospCalculate result.
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        ///{
        ///    "request": {
        ///        "Address": {
        ///            "Address1": "Somestreet 109",
        ///            "CellPhoneNumber": "555-1234",
        ///            "City": "Kopavogur",
        ///            "PhoneNumber": "555-3214",
        ///            "PostCode": "200",
        ///            "Type": 1
        ///        },
        ///        "BillToName": "Tom Tomsson",
        ///        "CardId": "10021",
        ///        "Comment": "Make it fast",
        ///        "DeliveryType": 0,
        ///        "Directions": "",
        ///        "Email": "tom@xyz.com",
        ///        "ExternalId": "MYID123",
        ///        "Name": "Tom Tomsson",
        ///        "OrderDiscountLines": [{
        ///              "Description": "Deal",
        ///              "DiscountAmount": 0.00,
        ///              "DiscountPercent": 0.00,
        ///              "DiscountType": 6,
        ///              "LineNumber": 9750,
        ///              "OfferNumber": "S10025",
        ///              "PeriodicDiscGroup": "",
        ///              "PeriodicDiscType": 0
        ///          }, {
        ///              "Description": "Deal",
        ///              "DiscountAmount": 0.00,
        ///              "DiscountPercent": 0.00,
        ///              "DiscountType": 6,
        ///              "LineNumber": 10000,
        ///              "OfferNumber": "S10025",
        ///              "PeriodicDiscGroup": "",
        ///              "PeriodicDiscType": 0
        ///          }, {
        ///              "Description": "Deal",
        ///              "DiscountAmount": 0.00,
        ///              "DiscountPercent": 0.00,
        ///              "DiscountType": 6,
        ///              "LineNumber": 20000,
        ///              "OfferNumber": "S10025",
        ///              "PeriodicDiscGroup": "",
        ///              "PeriodicDiscType": 0
        ///          }, {
        ///              "Description": "Deal",
        ///              "DiscountAmount": 0.00,
        ///              "DiscountPercent": 0.00,
        ///              "DiscountType": 6,
        ///              "LineNumber": 30000,
        ///              "OfferNumber": "S10025",
        ///              "PeriodicDiscGroup": "",
        ///              "PeriodicDiscType": 0
        ///          }],
        ///        "OrderLines": [{
        ///            "Amount": 7.50,
        ///            "DiscountAmount": 0.00,
        ///            "DiscountPercent": 0.00,
        ///            "IsADeal": true,
        ///            "ItemDescription": "Cheese Burger Meal",
        ///            "ItemId": "S10025",
        ///            "ItemImageId": null,
        ///            "LineNumber": 9750,
        ///            "LineType": 0,
        ///            "NetAmount": 6.82,
        ///            "NetPrice": 6.82,
        ///            "Price": 7.50,
        ///            "PriceModified": false,
        ///            "Quantity": 1.00,
        ///            "SubLines": [{
        ///                  "Amount": 5.32,
        ///                 "DealCode": "S10025",
        ///                 "DealLineId": 10000,
        ///                 "DealModifierLineId": 0,
        ///                 "Description": "Cheese Burger",
        ///                 "DiscountAmount": 0.00,
        ///                 "DiscountPercent": 0.00,
        ///                 "ItemId": "R0024",
        ///                 "LineNumber": 10000,
        ///                 "ManualDiscountAmount": 0.0,
        ///                 "ManualDiscountPercent": 0.0,
        ///                 "ModifierGroupCode": "",
        ///                 "ModifierSubCode": "",
        ///                 "NetAmount": 4.84,
        ///                 "NetPrice": 4.84,
        ///                 "ParentSubLineId": 0,
        ///                 "Price": 5.32,
        ///                 "Quantity": 1.00,
        ///                 "TAXAmount": 0.48,
        ///                 "Type": 1,
        ///                 "Uom": ""
        ///             }, {
        ///                 "Amount": 1.28,
        ///                 "DealCode": "S10025",
        ///                 "DealLineId": 20000,
        ///                 "DealModifierLineId": 70000,
        ///                 "Description": "Jalapeno Popper",
        ///                 "DiscountAmount": 0.00,
        ///                 "DiscountPercent": 0.00,
        ///                 "ItemId": "33430",
        ///                 "LineNumber": 20000,
        ///                 "ManualDiscountAmount": 0.0,
        ///                 "ManualDiscountPercent": 0.0,
        ///                 "ModifierGroupCode": "",
        ///                 "ModifierSubCode": "",
        ///                 "NetAmount": 1.16,
        ///                 "NetPrice": 1.16,
        ///                 "ParentSubLineId": 0,
        ///                 "Price": 1.28,
        ///                 "Quantity": 1.00,
        ///                 "TAXAmount": 0.12,
        ///                 "Type": 1,
        ///                 "Uom": ""
        ///             }, {
        ///                 "Amount": 0.90,
        ///                 "DealCode": "S10025",
        ///                 "DealLineId": 30000,
        ///                 "DealModifierLineId": 70000,
        ///                 "Description": "Orange Soda",
        ///                 "DiscountAmount": 0.00,
        ///                 "DiscountPercent": 0.00,
        ///                 "ItemId": "30520",
        ///                 "LineNumber": 30000,
        ///                 "ManualDiscountAmount": 0.0,
        ///                 "ManualDiscountPercent": 0.0,
        ///                 "ModifierGroupCode": "",
        ///                 "ModifierSubCode": "",
        ///                 "NetAmount": 0.82,
        ///                 "NetPrice": 0.82,
        ///                 "ParentSubLineId": 0,
        ///                 "Price": 0.90,
        ///                 "Quantity": 1.00,
        ///                 "TAXAmount": 0.08,
        ///                 "Type": 1,
        ///                 "Uom": ""
        ///            }]
        ///            "TaxAmount": 0.68,
        ///            "UomId": "",
        ///            "VariantDescription": "",
        ///            "VariantId": ""
        ///          }],
        ///        "OrderPayments": [{
        ///            "Amount": 7.50,
        ///            "AuthorizationCode": "123456",
        ///            "CardNumber": "10xx xxxx xxxx 1475",
        ///            "CardType": "VISA",
        ///            "CurrencyCode": "GBP",
        ///            "CurrencyFactor": 1,
        ///            "ExternalReference": "MyRef123",
        ///            "LineNumber": 1,
        ///            "PaymentType": "2",
        ///            "PreApprovedValidDate": "\/Date(1704111120000)\/",
        ///            "TenderType": "1",
        ///            "TokenNumber": "123456"
        ///         }],
        ///        "PickupTime": "\/Date(1704111120000)\/",
        ///        "RestaurantNo": "S0017",
        ///        "SalesType": "TAKEAWAY",
        ///        "StoreId": "S0017",
        ///        "TotalAmount": 7.50,
        ///        "TotalDiscount": 0.00,
        ///        "TotalNetAmount": 6.82
        ///    },
        ///    "returnOrderIdOnly": 0
        ///}
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <param name="returnOrderIdOnly">Only return Order Id back, not full order object</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry OrderHospCreate(OrderHosp request, bool returnOrderIdOnly);

        /// <summary>
        /// Cancel hospitality order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelHospOrder
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool HospOrderCancel(string storeId, string orderId);

        /// <summary>
        /// Get Order status for hospitality order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetHospOrderEstimatedTime, GetKotStatus
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderHospStatus HospOrderStatus(string storeId, string orderId);

        /// <summary>
        /// Check Status of a Customer Order
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderStatus
        /// </remarks>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        OrderStatusResponse OrderStatusCheck(string orderId);

        /// <summary>
        /// Cancel Customer Order with lineNo option to cancel individual lines
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCancel
        /// </remarks>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId">Web Store Id</param>
        /// <param name="userId">User who cancels the order, use Contact ID for logged in user</param>
        /// <param name="lineNo">List of Order Line numbers to cancel, if empty whole order will be canceled</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderCancel(string orderId, string storeId, string userId, List<int> lineNo);

        /// <summary>
        /// Cancel Customer Order with lineNo and quantity to cancel items from individual lines
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CustomerOrderCancel
        /// </remarks>
        /// <param name="orderId">Customer Order Id</param>
        /// <param name="storeId">Web Store Id</param>
        /// <param name="userId">User who cancels the order, use Contact ID for logged in user</param>
        /// <param name="lines">List of Order Lines to cancel from, if empty whole order will be canceled</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool OrderCancelEx(string orderId, string storeId, string userId, List<OrderCancelLine> lines);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by card Id
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetMemberContactSalesHistory
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="maxNumberOfEntries">max number of transactions returned</param>
        /// <returns>List of most recent Transactions for a contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntriesGetByCardId(string cardId, int maxNumberOfEntries);

        /// <summary>
        /// Get All Sales Entries (Transactions and Orders) by Card Id and optional filter by Store Id and Registration Date
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetMemberContactSalesHistory
        /// </remarks>
        /// <param name="cardId">Card Id (Required)</param>
        /// <param name="storeId">Filter by Store Id</param>
        /// <param name="date">Filter by Registration Date.  Set Date value to MinValue (0001-01-01) to skip Date Filtering</param>
        /// <param name="dateGreaterThan">Get Entries Greater (true) or Less (false) than Filter Date</param>
        /// <param name="maxNumberOfEntries">max number of transactions returned</param>
        /// <returns>List of most recent Transactions for a contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntriesGetByCardIdEx(string cardId, string storeId, DateTime date, bool dateGreaterThan, int maxNumberOfEntries);

        /// <summary>
        /// Get the Sale details (order/transaction)
        /// </summary>
        /// <remarks>
        /// LS Central OData: GetSelectedSalesDoc
        /// </remarks>
        /// <param name="entryId">Sales Entry ID</param>
        /// <param name="type">Document Id type of the Sale entry</param>
        /// <returns>SalesEntry with Lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntry SalesEntryGet(string entryId, DocumentIdType type);

        /// <summary>
        /// Get Return sales transactions based on orginal transaction with HasReturnSale = true
        /// </summary>
        /// <param name="receiptNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntryGetReturnSales(string receiptNo);

        /// <summary>
        /// Get Transaction and Sales Invoices for Customer order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SalesEntry> SalesEntryGetSalesByOrderId(string orderId);

        /// <summary>
        /// Get Transaction, Sales Invoices and Shipments for Customer order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SalesEntryList SalesEntryGetSalesExtByOrderId(string orderId);

        #endregion

        #region Contact

        /// <summary>
        /// Create new Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberContactCreate<p/><p/>
        /// Contact will get new Card that should be used when dealing with Orders.  Card Id is the unique identifier for Contacts in LS Central<p/>
        /// Contact will be assigned to a Member Account.
        /// Member Account has Club and Club has Scheme level.<p/>
        /// If No Account is provided, New Account will be created.
        /// If No Club level for the Account is set, then default Club and Scheme level will be set.<p/>
        /// Valid UserName, Password and Email address is determined by LS Central and can be found in CU 99009001.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "contact": {
        ///         "Id": "",
        ///         "Account": {
        ///             "Id": "",
        ///             "Scheme": {
        ///                 "Id": "",
        ///                 "Club": {
        ///                     "Id": "CRONUS"
        ///                 }
        ///             }
        ///         },
        ///         "Addresses": [{
        ///             "Address1": "Santa Monica",
        ///             "CellPhoneNumber": "555-5551",
        ///             "City": "Hollywood",
        ///             "Country": "US",
        ///             "PhoneNumber": "666-6661",
        ///             "PostCode": "1001",
        ///             "StateProvinceRegion": "",
        ///             "Type": "0"
        ///         }],
        ///         "Email": "Sarah@Hollywood.com",
        ///         "FirstName": "Sarah",
        ///         "Gender": "2",
        ///         "Initials": "Ms",
        ///         "LastName": "Parker",
        ///         "MaritalStatus": "0",
        ///         "MiddleName": "",
        ///         "Name": "Sarah Parker",
        ///         "Password": "SxxInTheCity",
        ///         "UserName": "sarah"
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <param name="doLogin">Perform Login after Create, this will return full Contact object with all information. If false, only contact, card, account ids are returned.</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNameInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNameExists</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingLastName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.MissingFirstName</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccountNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactCreate(MemberContact contact, bool doLogin);

        /// <summary>
        /// Update Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberContactUpdate<p/><p/>
        /// Contact Id, User name and EMail are required values for the update command to work.<p/>
        /// Any field left out or sent in empty will wipe out that information. Always fill out all 
        /// Name field, Address and phone number even if it has not changed so it will not be wiped out from LS Central
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///   "contact": {
        ///     "Id": "MO000012",
        ///     "Addresses": [{
        ///       "Address1": "Santa Monica",
        ///       "CellPhoneNumber": "555-5551",
        ///       "City": "Hollywood",
        ///       "Country": "US",
        ///       "PhoneNumber": "666-6661",
        ///       "PostCode": "1001",
        ///       "StateProvinceRegion": "",
        ///       "Type": "0"
        ///     }],
        ///     "Cards": [{
        ///       "Id": "10033"
        ///     }],
        ///     "Email": "Sarah@Hollywood.com",
        ///     "FirstName": "Sarah",
        ///     "Gender": "2",
        ///     "Initials": "Ms",
        ///     "LastName": "Parker",
        ///     "MaritalStatus": "0",
        ///     "MiddleName": "",
        ///     "Name": "Sarah Parker",
        ///     "Password": "SxxInTheCity",
        ///     "UserName": "sarah"
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="contact">contact</param>
        /// <param name="getContact">Return contact object filled out after Update</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ParameterInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.EmailInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactUpdate(MemberContact contact, bool getContact);

        /// <summary>
        /// Get Member Contact by card Id. This function returns all information about the Member contact, 
        /// including Profiles, Offers, Sales history, Onelist baskets and notifications.
        /// To get basic information, use ContactGet.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <param name="cardId">Card Id</param>
        /// <param name="numberOfTransReturned">Number of Sales History to return, 0 = all</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGetByCardId(string cardId, int numberOfTransReturned);

        /// <summary>
        /// Search for list of Member Contacts by different searchType methods, 
        /// will return any contact that will match the search value.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <example>
        /// Sample request to search by Name
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///    "searchType": 4,
        ///    "search": "Sarah",
        ///    "maxNumberOfRowsReturned": 10,
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="searchType">Field to search by</param>
        /// <param name="search">Search value</param>
        /// <param name="maxNumberOfRowsReturned">Max number of record, if set to 1 the exact search will be performed</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberContact> ContactSearch(ContactSearchType searchType, string search, int maxNumberOfRowsReturned);

        /// <summary>
        /// Search for Member Contact by different searchType methods.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberContact2<p/>
        /// LS Central WS4 : GetMemberContactInfo
        /// </remarks>
        /// <example>
        /// Sample request to get Contact by Name
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///    "searchType": 4,
        ///    "search": "Sarah Parker",
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="searchType">Field to search by</param>
        /// <param name="search">Search value</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact ContactGet(ContactSearchType searchType, string search);

        /// <summary>
        /// Add new card to existing Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberCardToContact
        /// </remarks>
        /// <param name="contactId"></param>
        /// <param name="cardId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        double ContactAddCard(string contactId, string cardId, string accountId);

        /// <summary>
        /// Block Member Contact and remove information from LS Central and LS Commerce
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ContactBlock(string accountId, string cardId);

        /// <summary>
        /// Get Point balance for Member Card
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        long CardGetPointBalance(string cardId);

        /// <summary>
        /// Get Point entries for Member Card
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="dateFrom"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<PointEntry> CardGetPointEntries(string cardId, DateTime dateFrom);

        /// <summary>
        /// Gets Rate value for points (f.ex. 1 point = 0.01 Kr)
        /// </summary>
        /// <param name="currency">Currency to convert Loy points to, if empty Base Currency will be used</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        decimal GetPointRate(string currency);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool DeviceSave(string deviceId, string deviceFriendlyName, string platform, string osVersion, string manufacturer, string model);

        /// <summary>
        /// Change password
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordChange
        /// </remarks>
        /// <param name="userName">user name (LS Central:LoginID)</param>
        /// <param name="newPassword">new password (LS Central:NewPassword)</param>
        /// <param name="oldPassword">old password (LS Central:OldPassword)</param>
        /// <returns>true/false</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AccessNotAllowed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ContactIdNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordOldInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ChangePassword(string userName, string newPassword, string oldPassword);

        /// <summary>
        /// Request a ResetCode to use in Email to send to Member Contact
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
        /// </summary>
        /// <remarks>
        /// Settings for this function are found in Commerce Service for LS Central Database - TenantConfig table
        /// <ul>
        /// <li>forgotpassword_code_encrypted: Reset Code is Encrypted</li>
        /// </ul>
        /// </remarks>
        /// <param name="userNameOrEmail">User Name or Email Address</param>
        /// <returns>ResetCode</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.UserNameNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ForgotPassword(string userNameOrEmail);

        /// <summary>
        /// Send in Reset Password request for Member Contact
        /// <p/>NOTE: Its recommended to use PasswordReset and PasswordChange instead
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordReset<p/><p/>
        /// If anything fails, simply ask the user to go through the ForgotPassword again..<p/>
        /// Error PasswordInvalid = ask user for better password<p/>
        /// Error ParameterInvalid = ask user for correct userName since it does not match resetCode<p/>
        /// All other errors should as the user to go through the forgotPassword flow again
        /// </remarks>
        /// <param name="userName"></param>
        /// <param name="resetCode">Reset Code returned from ForgotPassword</param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.UserNameNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeNotFound</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ResetPasswordCodeExpired</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.PasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.ParameterInvalid</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [Obsolete("Its recommended to use PasswordReset and PasswordChange instead")]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ResetPassword(string userName, string resetCode, string newPassword);

        /// <summary>
        /// Reset current password or request new password for new Member Contact.  
        /// Send either login or email depending on which function is required.
        /// If sendEmail is true, send only email address, login is not used in this mode
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordReset
        /// </remarks>
        /// <param name="userName">Provide Login Id (UserName) to reset existing password</param>
        /// <param name="email">Provide Email to create new login password for new Member Contact</param>
        /// <returns>Token to be included in Email to send to Member Contact.  Send the token with PasswordChange function</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string PasswordReset(string userName, string email);

        /// <summary>
        /// Change password for Member Contact.
        /// Call PasswordReset first if oldPassword is unknown or no login/password exist for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberPasswordChange<p/><p/>
        /// To change password for existing contact: Send userName, newPassword, oldPassword<p/>
        /// To reset password for existing contact: Send userName, token, newPassword<p/>
        /// To register new login/password for new contact: Send userName, token, newPassword<p/>
        /// </remarks>
        /// <param name="userName">Login Id or UserName</param>
        /// <param name="token">Token from PasswordReset</param>
        /// <param name="newPassword">New Password</param>
        /// <param name="oldPassword">Previous Password</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool PasswordChange(string userName, string token, string newPassword, string oldPassword);

        /// <summary>
        /// Change password in SPG App
        /// </summary>
        /// <param name="email">EMail to send token to</param>
        /// <param name="token">Token from PasswordReset</param>
        /// <param name="newPassword">New Password</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string SPGPassword(string email, string token, string newPassword);

        /// <summary>
        /// Change Login Id for Member Contact
        /// </summary>
        /// <remarks>
        /// LS Nav WS1 : MM_LOGIN_CHANGE
        /// </remarks>
        /// <param name="oldUserName">Current Login Id</param>
        /// <param name="newUserName">New Login Id</param>
        /// <param name="password">Current Password</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool LoginChange(string oldUserName, string newUserName, string password);

        /// <summary>
        /// Login user
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon
        /// </remarks>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <param name="deviceId">device Id. Should be empty for non device user (web apps)</param>
        /// <returns>Contact</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact Login(string userName, string password, string deviceId);

        /// <summary>
        /// Social authentication login
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberAuthenticatorLogin
        /// </remarks>
        /// <param name="authenticator"></param>
        /// <param name="authenticationId"></param>
        /// <param name="deviceId">Device Id. Should be empty for non device user (web apps)</param>
        /// <param name="deviceName">Device name or description</param>
        /// <param name="includeDetails">Include detailed Contact information, like  Publish offer and transactions</param>
        /// <returns>Contact</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact SocialLogon(string authenticator, string authenticationId, string deviceId, string deviceName, bool includeDetails);

        /// <summary>
        /// Login user from web page.  This function is light version of Login and returns less data.
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MemberLogon
        /// </remarks>
        /// <param name="userName">user name</param>
        /// <param name="password">password</param>
        /// <returns>Returns contact but only Contact and Card object have data</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNamePasswordInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.AuthFailed</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.InvalidPassword</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.LoginIdNotFound</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MemberContact LoginWeb(string userName, string password);

        /// <summary>
        /// Search for Customer
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="search"></param>
        /// <param name="maxNumberOfRowsReturned"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Customer> CustomerSearch(CustomerSearchType searchType, string search, int maxNumberOfRowsReturned);

        #endregion

        #region Item

        /// <summary>
        /// Get stock status of an item from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetItemInventory<p/><p/>
        /// If storeId is empty, only store that are marked in LS Central with check box Loyalty or Mobile checked (Omni Section) will be returned
        /// </remarks>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="itemId">Item to get status for</param>
        /// <param name="variantId">Item variant</param>
        /// <param name="arrivingInStockInDays">Item Date Filter</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStockGet(string storeId, string itemId, string variantId, int arrivingInStockInDays);

        /// <summary>
        /// Get stock status for list of items from one or all stores
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultiple<p/><p/>
        /// If storeId is empty, all store that have item available will be returned
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "items": [{
        ///         "ItemId": "40020",
        ///         "VariantId": "010"
        ///     }],
        ///     "storeId": "S0001"
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStoreGet(List<InventoryRequest> items, string storeId);

        /// <summary>
        /// Get stock status for list of items from Store and/or Sourcing Location
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetInventoryMultipleV2<p/><p/>
        /// If storeId is empty, all store that have item available will be returned.
        /// If locationId is set, only status for that location will be shown (with or without storeId)
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "items": [{
        ///         "ItemId": "40020",
        ///         "VariantId": "010"
        ///     }],
        ///     "locationId": "W0001",
        ///     "storeId": "S0001",
        ///     "useSourcingLocation": 0
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="items">Items to get status for</param>
        /// <param name="storeId">Store to get Stock status for, if empty get status for all stores</param>
        /// <param name="locationId">Sourcing Location to get status for</param>
        /// <param name="useSourcingLocation">Get Inventory status from all Sourcing Locations</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<InventoryResponse> ItemsInStoreGetEx(List<InventoryRequest> items, string storeId, string locationId, bool useSourcingLocation);

        /// <summary>
        /// Gets Hospitality Kitchen Current Availability
        /// </summary>
        /// <param name="request">List of items to get, if empty, get all items</param>
        /// <param name="storeId">Store to get, if empty get all stores</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<HospAvailabilityResponse> CheckAvailability(List<HospAvailabilityRequest> request, string storeId);

        /// <summary>
        /// Search Items by Description
        /// </summary>
        /// <param name="search">Description search</param>
        /// <param name="maxNumberOfItems"></param>
        /// <param name="includeDetails"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsSearch(string search, int maxNumberOfItems, bool includeDetails);

        /// <summary>
        /// Lookup Item
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetById(string itemId, string storeId);

        /// <summary>
        /// Look up Item by Barcode
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetItemWithBarcode
        /// </remarks>
        /// <param name="barcode"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        LoyItem ItemGetByBarcode(string barcode, string storeId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<LoyItem> ItemsPage(string storeId, int pageSize, int pageNumber, string itemCategoryId, string productGroupId, string search, bool includeDetails);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCategory> ItemCategoriesGetAll();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ItemCategory ItemCategoriesGetById(string itemCategoryId);

        /// <summary>
        /// Gets customer specific prices
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : EcomGetCustomerPrice
        /// </remarks>
        /// <param name="storeId"></param>
        /// <param name="cardId"></param>
        /// <param name="items">list of items to get prices for</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ItemCustomerPrice> ItemCustomerPricesGet(string storeId, string cardId, List<ItemCustomerPrice> items);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ProductGroup ProductGroupGetById(string productGroupId, bool includeDetails);

        /// <summary>
        /// Gets Hierarchy setup for a Store with all Leaves and Nodes
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetHierarchy, GetHierarchyNode<p/><p/>
        /// It is recommended for large hierarchies to use the hierarchy replication functions.
        /// It will give option on getting only changes, instead of always have to get the whole hierarchy like this function does.
        /// </remarks>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Hierarchy> HierarchyGet(string storeId);

        #endregion

        #region Menu

        /// <summary>
        /// Load Hospitality Menu
        /// </summary>
        /// <param name="storeId">Store to load, empty loads all</param>
        /// <param name="salesType">Sales type to load, empty loads all</param>
        /// <param name="loadDetails">Load Item Details and Image data</param>
        /// <param name="imageSize">Size of Image if loadDetails is set to true</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MobileMenu MenuGet(string storeId, string salesType, bool loadDetails, ImageSize imageSize);

        #endregion menu

        #region Profile

        /// <summary>
        /// Gets all Member Attributes that are available to assign to a Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : MobileGetProfiles<p/><p/>
        /// Only Member Attributes of type Boolean and Lookup Type None and are valid in Default Club will be selected
        /// </remarks>
        /// <returns>List of Member Attributes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetAll();

        /// <summary>
        /// Gets all Member Attributes for a Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetMemberCard<p/><p/>
        /// Member Attribute Value has to have Value Yes or No, even in other languages as Commerce Service for LS Central uses that Text to determent if the Attribute is selected or not for the Contact
        /// </remarks>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Profile> ProfilesGetByCardId(string cardId);

        #endregion

        #region Images

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ImageView ImageGetById(string id, ImageSize imageSize);

        [OperationContract]
        [WebGet(UriTemplate = "/ImageStreamGetById?id={id}&width={width}&height={height}", RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
        Stream ImageStreamGetById(string id, int width, int height);

        #endregion

        #region Account

        /// <summary>
        /// Get all schemes in system
        /// </summary>
        /// <returns>List of schemes</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Scheme> SchemesGetAll();

        #endregion

        #region Store Location

        /// <summary>
        /// Get store by Store Id
        /// </summary>
        /// <remarks>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <param name="storeId">store Id</param>
        /// <param name="includeImages">Include Image blobs</param>
        /// <returns>Store</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Store StoreGetById(string storeId, bool includeImages);

        /// <summary>
        /// Get List of stores
        /// </summary>
        /// <param name="storeType">Type of stores to get</param>
        /// <param name="includeDetails">Include detail data, like store hours</param>
        /// <param name="includeImages">Include Image Blobs for stores</param>
        /// <remarks>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <example>
        /// Sample request to get all Click and Collect Stores
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///    "storeType": 1,
        ///    "includeDetails": 1,
        ///    "includeImages": 0
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>List of stores</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGet(StoreGetType storeType, bool includeDetails, bool includeImages);

        /// <summary>
        /// Get all stores
        /// </summary>
        /// <remarks>
        /// Data for Store Hours needs to be generated in LS Central by running COMMERCE_XXXX Scheduler Jobs
        /// </remarks>
        /// <returns>List of stores</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetAll();

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude)
        /// </summary>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <returns>List of stores marked as ClickAndCollect within max distance of coordinates</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetByCoordinates(double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets all Click and Collect stores, within maxDistance from current location (latitude,longitude), that have the item available
        /// </summary>
        /// <param name="itemId">item Id</param>
        /// <param name="variantId">variant Id</param>
        /// <param name="latitude">current latitude</param>
        /// <param name="longitude">current longitude</param>
        /// <param name="maxDistance">max distance of stores from latitude and longitude in kilometers, 0 = no limit</param>
        /// <returns>List of stores marked as ClickAndCollect that have the item in stock</returns>
        /// <exception cref="LSOmniServiceException">StatusCodes returned:
        /// <list type="bullet">
        /// <item>
        /// <description>StatusCode.Error</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.SecurityTokenInvalid</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.UserNotLoggedIn</description>
        /// </item>
        /// <item>
        /// <description>StatusCode.DeviceIsBlocked</description>
        /// </item>
        /// </list>
        /// </exception>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Store> StoresGetbyItemInStock(string itemId, string variantId, double latitude, double longitude, double maxDistance);

        /// <summary>
        /// Gets Return Policy
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="storeGroupCode"></param>
        /// <param name="itemCategory"></param>
        /// <param name="productGroup"></param>
        /// <param name="itemId"></param>
        /// <param name="variantCode"></param>
        /// <param name="variantDim1"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ReturnPolicy> ReturnPolicyGet(string storeId, string storeGroupCode, string itemCategory, string productGroup, string itemId, string variantCode, string variantDim1);

        /// <summary>
        /// Get Currency by Code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        Currency CurrencyGet(string code);

        #endregion

        #region Replication

        /// <summary>
        /// Replicate Item Barcodes (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001451 - LSC Barcodes
        /// LS Central WS4 : GetBarcode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.  
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of barcodes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplBarcodeResponse ReplEcommBarcodes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 4 - Currency
        /// LS Central WS4 : GetCurrency
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currencies</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyResponse ReplEcommCurrency(ReplRequest replRequest);

        /// <summary>
        /// Replicate Currency Rate Setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 330 - Currency Exchange Rate
        /// LS Central WS4 : GetCurrencyExchRate
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of currency rates</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCurrencyExchRateResponse ReplEcommCurrencyRate(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Extended Variants Setup (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001413 - LSC Extd. Variant Values
        /// LS Central WS4 : GetExtdVariantValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variants</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplExtendedVariantValuesResponse ReplEcommExtendedVariants(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Image links
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009064 - LSC Retail Image Link
        /// LS Central WS4 : GetImageLink
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of image links</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageLinkResponse ReplEcommImageLinks(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Images
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009063 - LSC Retail Image
        /// LS Central WS4 : GetWIImageBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of images</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplImageResponse ReplEcommImages(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Categories (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5722 - Item Category
        /// LS Central WS4 : GetItemCategory
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item categories</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemCategoryResponse ReplEcommItemCategories(ReplRequest replRequest);

        /// <summary>
        /// Replicate Retail Items (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item
        /// LS Central WS4 : GetWIItemBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemResponse ReplEcommItems(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Unit of Measures (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5404 - Item Unit of Measure
        /// LS Central WS4 : GetItemUnitOfMeasure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of item unit of measures</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemUnitOfMeasureResponse ReplEcommItemUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Variant Registrations (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001414 - LSC Item Variant Registration
        /// LS Central WS4 : GetVariantRegWithStatus
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant registrations</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemVariantRegistrationResponse ReplEcommItemVariantRegistrations(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Variant (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 5401 - Item Variant
        /// LS Central WS4 : GetItemVariant
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of variant</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemVariantResponse ReplEcommItemVariants(ReplRequest replRequest);

        /// <summary>
        /// Replicate Best Prices for Items from WI Price table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012861 - LSC WI Price
        /// LS Central WS4 : GetWIPrice
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs.  
        /// This will generate the Best price for product based on date and offers available at the time.<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item and Sales Price tables are used to find deleted changes.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommPrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item Prices from Sales Price table (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 7002 - Sales Price
        /// LS Central WS4 : GetSalesPrice
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of prices</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplPriceResponse ReplEcommBasePrices(ReplRequest replRequest);

        /// <summary>
        /// Replicate Product groups (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000705 - LSC Retail Product Group
        /// LS Central WS4 : GetProductGroup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of product groups</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplProductGroupResponse ReplEcommProductGroups(ReplRequest replRequest);

        /// <summary>
        /// Replicate Store setups
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001470 - LSC Store
        /// LS Central WS4 : GetStoreBuffer
        /// <p/><p/>
        /// Only store with Loyalty or Mobile Checked will be replicated
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of stores</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreResponse ReplEcommStores(ReplRequest replRequest);

        /// <summary>
        /// Replicate Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 204 - Unit of Measure
        /// LS Central WS4 : GetUnitOfMeasure
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of unit of measures</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplUnitOfMeasureResponse ReplEcommUnitOfMeasures(ReplRequest replRequest);

        /// <summary>
        /// Replicate Collection for Unit of Measures
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001430 - LSC Collection Framework
        /// LS Central WS4 : GetCollection
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Collection</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCollectionResponse ReplEcommCollection(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendors
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 23 - Vendor
        /// LS Central WS4 : GetVendor
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendors</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplVendorResponse ReplEcommVendor(ReplRequest replRequest);

        /// <summary>
        /// Replicate Vendor Item Mapping (supports Item distribution)
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item (Lookup by [Vendor No_])
        /// LS Central WS4 : GetVendorItem
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of vendor item mappings</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplLoyVendorItemMappingResponse ReplEcommVendorItemMapping(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attributes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000784 - LSC Attribute
        /// LS Central WS4 : GetAttribute
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <example>
        /// Sample request can be used with all other Replication functions
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "replRequest": {
        ///         "BatchSize": 100,
        ///         "FullReplication": 1,
        ///         "LastKey": "",
        ///         "MaxKey": "",
        ///         "StoreId": "S0013",
        ///         "TerminalId": ""
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attributes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeResponse ReplEcommAttribute(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000786 - LSC Attribute Value
        /// LS Central WS4 : GetAttributeValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute values</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeValueResponse ReplEcommAttributeValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Attribute Option Values
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000785 - LSC Attribute Option Value
        /// LS Central WS4 : GetAttributeOptionValues
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of attribute option values</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplAttributeOptionValueResponse ReplEcommAttributeOptionValue(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000971 - LSC Data Translation
        /// LS Central WS4 : GetDataTranslation
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommDataTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text for Item HTML table
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001410 - LSC Item HTML ML
        /// LS Central WS4 : GetItemHTML
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommHtmlTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation text for Deal HTML table
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10001410 - LSC Deal HTML ML
        /// LS Central WS4 : GetDealHTML
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of texts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationResponse ReplEcommDealHtmlTranslation(ReplRequest replRequest);

        /// <summary>
        /// Replicate Translation Language Codes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000972 - LSC Data Translation Language
        /// <p/><p/>
        /// This will always replicate all Code
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Codes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDataTranslationLangCodeResponse ReplEcommDataTranslationLangCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Scheduling data for Hierarchy
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000955 - LSC Validation Schedule
        /// <p/><p/>
        /// This function only checks if there are any available pre-actions for any of the tables involved in the Schedule data 
        /// and if there is, the whole Validation Schedule will be replicated again.
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Validation Schedule objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplValidationScheduleResponse ReplEcommValidationSchedule(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy roots
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000920 - LSC Hierarchy
        /// LS Central WS4 : GetHierarchy
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Hierarchy objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyResponse ReplEcommHierarchy(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Nodes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000921 - LSC Hierar. Nodes
        /// LS Central WS4 : GetHierarchyNodes
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy nodes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyNodeResponse ReplEcommHierarchyNode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Node Leaves
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10000922 - LSC Hierar. Node Link
        /// LS Central WS4 : GetHierarchyLeaf
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy node leaves</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyLeafResponse ReplEcommHierarchyLeaf(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deals for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001503 - LSC Offer Line
        /// LS Central WS4 : GetHierarchyDeal
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deals</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyHospDealResponse ReplEcommHierarchyHospDeal(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Deal lines for Deal
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001651 - LSC Deal Modifier Item
        /// LS Central WS4 : GetHierarchyDealLine
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy deal lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplHierarchyHospDealLineResponse ReplEcommHierarchyHospDealLine(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Recipe lines for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 90 - BOM Component
        /// LS Central WS4 : GetWIItemRecipeBuffer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy recipe items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemRecipeResponse ReplEcommItemRecipe(ReplRequest replRequest);

        /// <summary>
        /// Replicate Hierarchy Hospitality Modifier lines for Node Leaf
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001483 - LSC Information Subcode
        /// LS Central WS4 : GetWIItemModifier
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of hierarchy modifier lines</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplItemModifierResponse ReplEcommItemModifier(ReplRequest replRequest);

        /// <summary>
        /// Replicate Item with full detailed data (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 27 - Item
        /// <p/><p/>
        /// FullItem replication includes all variants, unit of measures, attributes and prices for an item<p/>
        /// NOTE: It is recommended to replicate item data separately using<p/>
        /// ReplEcomm Item / Prices / ItemUnitOfMeasures / ItemVariantRegistrations / ExtendedVariants / Attribute / AttributeValue / AttributeOptionValue<p/>
        /// Price Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// For update, actions for Item, Item HTML, Sales Price, Item Variant, Item Unit of Measure, Variants and distribution tables are used to find changes,
        /// and it may return empty list of items while Records Remaining is still not 0.  Keep on calling the function till Records Remaining become 0.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Item objects</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplFullItemResponse ReplEcommFullItem(ReplRequest replRequest);

        /// <summary>
        /// Replicate Periodic Discounts and MultiBuy for Items from WI Discount table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012862 - LSC WI Discounts
        /// LS Central WS4 : GetWIDiscounts
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountResponse ReplEcommDiscounts(ReplRequest replRequest);

        /// <summary>
        /// Replicate Mix and Match Offers for Items from WI Mix and Match Offer table in LS Central (supports Item distribution)<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 10012863 - LSC WI Mix and Match Offer
        /// LS Central WS4 : GetWIMixMatch
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplMixMatchResponse ReplEcommMixAndMatch(ReplRequest replRequest);

        /// <summary>
        /// Replicate Discount Setup from Central<p/>
        /// Only Multibuy, Discount, Total and Tender discounts are replicated
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001453 - LSC Periodic Discount
        /// <p/><p/>
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Periodic Discounts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountSetupResponse ReplEcommDiscountSetup(ReplRequest replRequest);

        /// <summary>
        /// Replicate Validation Periods for Discounts<p/>
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001481 - LSC Validation Period
        /// LS Central WS4 : GetValidationPeriod
        /// <p/><p/>
        /// Data for this function needs to be generated in LS Central by running either COMMERCE_XXXX Scheduler Jobs<p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// Item distribution is based on StoreId, and pulls all record related to Item include for distribution to that store.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of discounts for items</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplDiscountValidationResponse ReplEcommDiscountValidations(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Shipping agents and services
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 291 - Shipping Agent
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of shipping agents</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplShippingAgentResponse ReplEcommShippingAgent(ReplRequest replRequest);

        /// <summary>
        /// Replicate Member contacts
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99009002 - LSC Member Contact (with valid Membership Card)
        /// LS Central WS4 : GetContact
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Member contacts</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCustomerResponse ReplEcommMember(ReplRequest replRequest);

        /// <summary>
        /// Replicate Customers
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 18 - Customer
        /// LS Central WS4 : GetCustomer
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Customers</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCustomerResponse ReplEcommCustomer(ReplRequest replRequest);

        /// <summary>
        /// Replicate all Country Codes
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 9 - Country/Region
        /// LS Central WS4 : GetCountryCode
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of Country codes</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplCountryCodeResponse ReplEcommCountryCode(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tender types for Store
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001466 - LSC Tender Type Setup
        /// LS Central WS4 : GetTenderType
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplStoreTenderTypeResponse ReplEcommStoreTenderTypes(ReplRequest replRequest);

        /// <summary>
        /// Replicate Tax setup
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 325 - VAT Posting Setup
        /// LS Central WS4 : GetVATPostingSetup
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// For full replication of all data, set FullReplication to true and LastKey and MaxKey to 0.
        /// For delta (or updated data) replication, set FullReplication to false and LastKey and MaxKey to the last value returned from previous call. 
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey and MaxKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central, both during full or delta replication.
        /// To reset replication and get all delta data again, set LastKey and MaxKey to 0 and perform a full replication.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplTaxSetupResponse ReplEcommTaxSetup(ReplRequest replRequest);

        /// <summary>
        /// Replicate Inventory Status
        /// </summary>
        /// <remarks>
        /// LS Central Main Table data: 99001608 - LSC Inventory Lookup Table
        /// LS Central WS4 : GetInventoryStatus
        /// <p/><p/>
        /// Net Inventory field in Inventory Lookup Table must be updated before the replication can be done.  
        /// In Retail Product Group card, set up which products to check status for by click on Update POS Inventory Lookup button. Set store to be Web Store.
        /// Run Scheduler job with CodeUnit 10012871 - WI Update Inventory which will update the Net Inventory field.
        /// <p/><p/>
        /// Most ReplEcommXX web methods work the same way.
        /// This function always performs full replication
        /// The BatchSize is how many records are to be returned in each batch.<p/><p/>
        /// NOTE: LastKey from each ReplEcommXX call needs to be stored between all calls to Commerce Service for LS Central.
        /// </remarks>
        /// <param name="replRequest">Replication request object</param>
        /// <returns>Replication result object with List of store tender types</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ReplInvStatusResponse ReplEcommInventoryStatus(ReplRequest replRequest);

        #endregion

        #region Search

        /// <summary>
        /// Search different data based on SearchType value
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="search"></param>
        /// <param name="searchTypes"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        SearchRs Search(string cardId, string search, SearchType searchTypes);

        #endregion search

        #region LS Recommends

        /// <summary>
        /// Checks if LS Recommend is active in Commerce Service for LS Central
        /// <p/>NOTE: Not supported anymore
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("Not supported anymore", true)]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool RecommendedActive();

        /// <summary>
        /// Get Recommended Items based of list of items
        /// <p/>NOTE: Not supported anymore
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("Not supported anymore", true)]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<RecommendedItem> RecommendedItemsGet(List<string> items);

        #endregion

        #region Activity

        /// <summary>
        /// Confirm Activity Booking
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ConfirmActivityVx<p/><p/>
        /// If property [Paid] is set, then returns details for the retail basket.<p/>
        /// [BookingRef] should be assigned to the OrderLine and passed in with Order so retrieved basket payment through Commerce Service for LS Central will update the Activities payment status and assign the sales order document as payment document.<p/> 
        /// If activity type does not require [contactNo] then it is sufficient to provide client name.<p/>
        /// If [ReservationNo] is blank the system will create new reservation and return the value to ReservationNo.  If ReservationNo is populated parameter then the system will try to add the activity to existing reservation if the reservation exists and is neither canceled or closed.<p/>
        /// [PromoCode] is validated and adjusts pricing accordingly.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        ///{
        ///   "request": {
        ///      "ActivityTime": "\/Date(1576011600000)\/",
        ///      "ContactAccount": "",
        ///      "ContactName": "Tom",
        ///      "ContactNo": "MO000008",
        ///      "Email": "tom@comp.com",
        ///      "Location": "CAMBRIDGE",
        ///      "NoOfPeople": "1",
        ///      "OptionalResource": "",
        ///      "OptionalComment": "",
        ///      "Paid": "false",
        ///      "ProductNo": "MASSAGE30",
        ///      "PromoCode": "",
        ///      "Quantity": "1",
        ///      "ReservationNo": "RES0045",
        ///      "Token": "",
        ///   }
        ///}        
        ///]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>Activity Number and Booking Reference</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityConfirm(ActivityRequest request);

        /// <summary>
        /// Cancel Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelActivity<p/><p/>
        /// If cancellation charges apply, then those vales will be returned and could be applied to a retail basket.
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityCancel(string activityNo);

        /// <summary>
        /// Returns list of available time-slots/prices for a specific location,product and date 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAvailabilityVx<p/><p/>
        /// Optional to include required resource (if only specific resource) and contactNo for accurate pricing.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "locationNo": "CAMBRIDGE",
        ///     "productNo": "MASSAGE30",
        ///     "activityDate": "\/Date(1580580398000)\/",
        ///     "contactNo": "MO000008",
        ///     "optionalResource": "",
        ///     "promoCode": ""
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="contactNo"></param>
        /// <param name="contactAccount"></param>
        /// <param name="optionalResource"></param>
        /// <param name="promoCode"></param>
        /// <param name="activityNo"></param>
        /// <param name="noOfPersons"></param>
        /// <param name="guestType"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityAvailabilityGet(string locationNo, string productNo, DateTime activityDate, string contactNo, string contactAccount, string optionalResource, string promoCode, string activityNo, int noOfPersons, string guestType);

        /// <summary>
        /// Returns list with the required or optional additional charges for the Activity as applied automatically according to the product
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAdditionalCharges
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdditionalCharge> ActivityAdditionalChargesGet(string activityNo);

        /// <summary>
        /// Returns list of charges for products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetProductChargesV2
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="dateOfBooking"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdditionalCharge> ActivityProductChargesGet(string locationNo, string productNo, DateTime dateOfBooking);

        /// <summary>
        /// Change or insert additional charges to Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAdditionalChargesVx
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///   "request": {
        ///     "ActivityNo": "ACT0035",
        ///     "DiscountPercentage": "0.0",
        ///     "InvoiceReference": "",
        ///     "ItemNo": "40020",
        ///     "LineNo": 1,
        ///     "Optional": "",
        ///     "OptionalComment", "",
        ///     "ParentLine": 0,
        ///     "Price": 110.0,
        ///     "ProductType": "0",
        ///     "Quantity": 1,
        ///     "TotalAmount": 110.0,
        ///     "UnitOfMeasure": "",
        ///     "VariantCode": ""
        ///   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityAdditionalChargesSet(AdditionalCharge request);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetReservationAdditionalCharges
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdditionalCharge> ActivityReservationAdditionalChargesGet(string reservationNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetGroupAdditionalCharges
        /// </remarks>
        /// <param name="reqest"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityGroupAdditionalChargesSet(AdditionalCharge reqest);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetGroupReservationAdditionalCharges
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdditionalCharge> ActivityGroupReservationAdditionalChargesGet(string reservationNo, int memberNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetGroupMembers
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="contact"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="guestType"></param>
        /// <param name="optionalComment"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityGroupMemberSet(string reservationNo, int memberSequence, string contact, string name, string email, string phone, string guestType, string optionalComment);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : AssignGroupMember
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityGroupMemberAssign(string reservationNo, int memberSequence, int lineNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : RemoveGroupMember
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="memberSequence"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityGroupMemberRemove(string reservationNo, int memberSequence, int lineNo);

        /// <summary>
        /// Returns list of Attributes which are assigned on a given Activity product, reservation or activity entry
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAttributes
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AttributeResponse> ActivityAttributesGet(AttributeType type, string linkNo);

        /// <summary>
        /// Action to set an attribute value on a given reservation or activity.  If attribute does not exist on the entry then its inserted otherwise updated
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetAttribute
        /// </remarks>
        /// <param name="type"></param>
        /// <param name="linkNo"></param>
        /// <param name="attributeCode"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        int ActivityAttributeSet(AttributeType type, string linkNo, string attributeCode, string attributeValue);

        /// <summary>
        /// Action to create a Reservation header into the LS Reservation table
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertReservationVx
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///     "request": {
        ///        "ClientName": "Tom",
        ///        "ContactNo": "MO000008",
        ///        "Description": "",
        ///        "Email": "tom@xxx.com",
        ///        "EventNo": "",
        ///        "Internalstatus": "0",
        ///        "Location": "CAMBRIDGE",
        ///        "ResDateFrom": "\/Date(1570737600000)\/",
        ///        "ResDateTo": "\/Date(1570741200000)\/",
        ///        "ResTimeFrom": "\/Date(1570737600000)\/",
        ///        "ResTimeTo": "\/Date(1570741200000)\/",
        ///        "ReservationType": "SPA",
        ///        "SalesPerson": "AH",
        ///        "Status": ""
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityReservationInsert(Reservation request);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservationStatus
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="setStatusCode"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityUpdateReservationStatus(string reservationNo, string setStatusCode);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateActivityStatus
        /// </remarks>
        /// <param name="activityNo"></param>
        /// <param name="setStatusCode"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityUpdateActivityStatus(string activityNo, string setStatusCode);

        /// <summary>
        /// Action to force update to a reservation header in the LS Reservation table.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateReservationVx
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityReservationUpdate(Reservation request);

        /// <summary>
        /// Sell Membership (membership type) to Member Contact
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SellMembership
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="membersShipType"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        MembershipResponse ActivityMembershipSell(string contactNo, string membersShipType);

        /// <summary>
        /// Cancels a specific membership and validates if cancellation is in order (i.e. compares to commitment period)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelMembership
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="memberShipNo"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityMembershipCancel(string contactNo, string memberShipNo, string comment);

        /// <summary>
        /// Get availability for specific resource, for a specific date and location (all required parameters)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetResourceAvailability
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="resourceNo"></param>
        /// <param name="intervalType">Use specific intervals setup in the system or leave blank for whole day</param>
        /// <param name="noOfDays">Set how many days to return availability, if set to 0 then use default setting (10 days normally)</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityResourceAvailabilityGet(string locationNo, DateTime activityDate, string resourceNo, string intervalType, int noOfDays);

        /// <summary>
        /// Get availability for all active resource in specific resource group, for a specific date and location (all required parameters)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetResourceGroupAvailability
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="activityDate"></param>
        /// <param name="groupNo"></param>
        /// <param name="intervalType">Use specific intervals setup in the system or leave blank for whole day</param>
        /// <param name="noOfDays">Set how many days to return availability, if set to 0 then use default setting (10 days normally)</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AvailabilityResponse> ActivityResourceGroupAvailabilityGet(string locationNo, DateTime activityDate, string groupNo, string intervalType, int noOfDays);

        /// <summary>
        /// Check if valid access for either membership or ticketing.  
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CheckAccess
        /// </remarks>
        /// <param name="searchReference">Either TicketBarcode, Member No. or Membership No. LocationNo</param>
        /// <param name="locationNo">Optional Activity Location</param>
        /// <param name="gateNo">Optional Gate number</param>
        /// <param name="registerAccessEntry">Set if to register admission</param>
        /// <param name="checkType">0 = All checks, 1 = CheckTicket only, 2 = CheckMembership only</param>
        /// <param name="messageString">Returned info</param>
        /// <returns>Returns true or false if access is valid</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityCheckAccess(string searchReference, string locationNo, string gateNo, bool registerAccessEntry, int checkType, out string messageString);

        /// <summary>
        /// Get Availability Token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetAvailabilityToken
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="activityTime"></param>
        /// <param name="optionalResource"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityGetAvailabilityToken(string locationNo, string productNo, DateTime activityTime, string optionalResource, int quantity);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ExtendToken
        /// </remarks>
        /// <param name="tokenId"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityExtendToken(string tokenId, int seconds);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : CancelToken
        /// </remarks>
        /// <param name="tokenId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityCancelToken(string tokenId);

        /// <summary>
        /// Create Group Reservation
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : InsertGroupReservationVx
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        /// {
        ///      "request": {
        ///        "ClientName": "Tom",
        ///        "ContactNo": "MO000008",
        ///        "Description": "",
        ///        "Email": "tom@xxx.com",
        ///        "EventNo": "",
        ///        "Internalstatus": "0",
        ///        "Location": "CAMBRIDGE",
        ///        "NoOfPerson": 2,
        ///        "ResDateFrom": "\/Date(1570737600000)\/",
        ///        "ResDateTo": "\/Date(1570741200000)\/",
        ///        "ResTimeFrom": "\/Date(1570737600000)\/",
        ///        "ResTimeTo": "\/Date(1570741200000)\/",
        ///        "ReservationType": "SPA",
        ///        "SalesPerson": "AH",
        ///        "Status": ""
        ///      }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityInsertGroupReservation(Reservation request);

        /// <summary>
        /// Update Group reservation header.  Blank fields will be ignored
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateGroupReservationVx
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityUpdateGroupReservation(Reservation request);

        /// <summary>
        /// Confirm Group Activity Booking
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : ConfirmGroupActivityVx<p/><p/>
        /// If property [Paid] is set, then returns details for the retail basket.<p/>
        /// [BookingRef] should be assigned to the OrderLine and passed in with Order so retrieved basket payment through Commerce Service for LS Central will update the Activities payment status and assign the sales order document as payment document.<p/> 
        /// If activity type does not require [contactNo] then it is sufficient to provide client name.<p/>
        /// If [ReservationNo] is blank the system will create new reservation and return the value to ReservationNo.  If ReservationNo is populated parameter then the system will try to add the activity to existing reservation if the reservation exists and is neither canceled or closed.<p/>
        /// [PromoCode] is validated and adjusts pricing accordingly.
        /// </remarks>
        /// <example>
        /// Sample request including minimum data needed to be able to process the request in LS Commerce
        /// <code language="xml" title="REST Sample Request">
        /// <![CDATA[
        ///{
        ///   "request": {
        ///      "ActivityTime": "\/Date(1576011600000)\/",
        ///      "ContactAccount": "",
        ///      "ContactName": "Tom",
        ///      "ContactNo": "MO000008",
        ///      "Email": "tom@comp.com",
        ///      "GroupNo": "",
        ///      "Location": "CAMBRIDGE",
        ///      "NoOfPeople": "1",
        ///      "OptionalResource": "",
        ///      "OptionalComment": "",
        ///      "Paid": "false",
        ///      "ProductNo": "MASSAGE30",
        ///      "PromoCode": "",
        ///      "Quantity": "1",
        ///      "ReservationNo": "RES0045",
        ///      "SetGroupReservation": "",
        ///      "Token": "",
        ///      "UnitPrice": 0.0
        ///   }
        ///}        
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="request"></param>
        /// <returns>Activity Number and Booking Reference</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityConfirmGroup(ActivityRequest request);

        /// <summary>
        /// Delete Group Activity
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : DeleteGroupActivity
        /// </remarks>
        /// <param name="groupNo"></param>
        /// <param name="lineNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool ActivityDeleteGroup(string groupNo, int lineNo);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UpdateGroupHeaderStatus
        /// </remarks>
        /// <param name="groupNo"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string ActivityUpdateGroupHeaderStatus(string groupNo, string statusCode);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : PreSellActivityProduct
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="productNo"></param>
        /// <param name="promoCode"></param>
        /// <param name="contactNo"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ActivityResponse ActivityPreSellProduct(string locationNo, string productNo, string promoCode, string contactNo, int quantity);

        #endregion

        #region Activity Data Get (Replication)

        /// <summary>
        /// Returns list of Activity Products
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityProducts
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityProduct> ActivityProductsGet();

        /// <summary>
        /// Returns list of Activity Types
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityTypes
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityType> ActivityTypesGet();

        /// <summary>
        /// Returns list of Activity Locations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityLocations
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityLocation> ActivityLocationsGet();

        /// <summary>
        /// Returns list of Reservations for Member Contact or list of Activities assigned to a Reservation
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : <p/>
        /// With [contactNo, activityType] UploadClientBookingsV2<p/>
        /// With [reservationNo] : UploadReservationActivities
        /// </remarks>
        /// <param name="reservationNo">Look up Activities for a Reservation</param>
        /// <param name="contactNo">Look up Reservations for a Contact</param>
        /// <param name="activityType">Activity type for Contact Lookup</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Booking> ActivityReservationsGet(string reservationNo, string contactNo, string activityType);

        /// <summary>
        /// Look up Reservation Headers
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetActReservations
        /// </remarks>
        /// <param name="reservationNo"></param>
        /// <param name="reservationType"></param>
        /// <param name="status"></param>
        /// <param name="locationNo"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ResHeader> ActivityReservationsHeaderGet(string reservationNo, string reservationType, string status, string locationNo, DateTime fromDate);

        /// <summary>
        /// Returns list of Active Promotions (for information purposes only)
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPromotions
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Promotion> ActivityPromotionsGet();

        /// <summary>
        /// Returns list of Member Contacts issued (sold) allowances
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadPurchasedAllowances
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Allowance> ActivityAllowancesGet(string contactNo);

        /// <summary>
        /// Returns list of all entries charged to the Member Contact customer account (A/R). The Account no. is based on the contact business relation settings
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadCustomerEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <param name="customerNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<CustomerEntry> ActivityCustomerEntriesGet(string contactNo, string customerNo);

        /// <summary>
        /// Returns list of Membership types (products) which are active and can be sold 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipProducts
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<MemberProduct> ActivityMembershipProductsGet();

        /// <summary>
        /// Returns list of all subscription charges posted towards their membership account. Draft unposted entries are not included
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipSubscriptionCharges
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<SubscriptionEntry> ActivitySubscriptionChargesGet(string contactNo);

        /// <summary>
        /// Returns list of Member Contact visit registrations
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadAdmissionEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<AdmissionEntry> ActivityAdmissionEntriesGet(string contactNo);

        /// <summary>
        /// Returns list of the Member Contact current active or on hold memberships
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadMembershipEntries
        /// </remarks>
        /// <param name="contactNo">Member Contact</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Membership> ActivityMembershipsGet(string contactNo);

        /// <summary>
        /// Get list of activities assigned to a resource, required parameters Resource code (number), Date from and to date
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadResourceActivities
        /// </remarks>
        /// <param name="locationNo"></param>
        /// <param name="resourceNo"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<Booking> ActivityGetByResource(string locationNo, string resourceNo, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get list of all resources 
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : UploadActivityResources
        /// </remarks>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ActivityResource> ActivityResourceGet();

        #endregion

        #region ScanPayGo

        /// <summary>
        /// Creates a client token for payment provider
        /// </summary>
        /// <param name="customerId">Customer id, used to show saved cards</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ClientToken PaymentClientTokenGet(string customerId);

        /// <summary>
        /// Gets Profile setup for SPG App
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SPGProfileGet
        /// </remarks>
        /// <param name="profileId"></param>
        /// <param name="storeNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ScanPayGoProfile ScanPayGoProfileGet(string profileId, string storeNo);

        /// <summary>
        /// Check security status of a profile
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SecurityCheckProfile
        /// </remarks>
        /// <param name="orderNo"></param>
        /// <param name="storeNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SecurityCheckProfile(string orderNo, string storeNo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        ScanPayGoSecurityLog SecurityCheckLog(string orderNo);

        /// <summary>
        /// Allow app to open Gate when exiting the store
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SPGOpenGate
        /// </remarks>
        /// <param name="qrCode"></param>
        /// <param name="storeNo"></param>
        /// <param name="devLocation"></param>
        /// <param name="memberAccount"></param>
        /// <param name="exitWithoutShopping"></param>
        /// <param name="isEntering"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string OpenGate(string qrCode, string storeNo, string devLocation, string memberAccount, bool exitWithoutShopping, bool isEntering);

        /// <summary>
        /// Add Payment token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : SetTokenEntry, SetMemberCardToken, DeleteMemberCardToken
        /// </remarks>
        /// <param name="token"></param>
        /// <param name="deleteToken">Delete token, Send token with token and cardId to delete</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool TokenEntrySet(ClientToken token, bool deleteToken);

        /// <summary>
        /// Get Payment token
        /// </summary>
        /// <remarks>
        /// LS Central WS2 : GetTokenEntry, GetMemberCardToken
        /// </remarks>
        /// <param name="accountNo"></param>
        /// <param name="hotelToken">Get token for LS Hotels</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        List<ClientToken> TokenEntryGet(string accountNo, bool hotelToken);

        /// <summary>
        /// Request to unlock Rod Device
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SpgUnlockRodDevice(string storeId, string cardId);

        /// <summary>
        /// Used by Rod Device to check if there is request to unlock a device
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string SpgUnlockRodDeviceCheck(string storeId);

        /// <summary>
        /// Register for Push Notifications when Wish list gets some updates, like new follower or items
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SpgRegisterNotification(string cardId, string token);

        /// <summary>
        /// Un-Register Push Notifications for Wish list updates
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        bool SpgUnRegisterNotification(string cardId);

        /// <summary>
        /// Get codes
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        string GetAuthCodes();
		
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
		string MyCustomFunction(string data);
		
        #endregion
    }
}