﻿using LSRetail.Omni.Domain.DataModel.Base.Retail;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Replication
{
    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountResponse : IDisposable
    {
        public ReplDiscountResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Discounts = new List<ReplDiscount>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Discounts != null)
                    Discounts.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscount> Discounts { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplMixMatchResponse : IDisposable
    {
        public ReplMixMatchResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Discounts = new List<ReplDiscount>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Discounts != null)
                    Discounts.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscount> Discounts { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscount : IDisposable
    {
        public ReplDiscount(bool isJson)
        {
            IsDeleted = false;
            PriorityNo = 0;
            ItemId = string.Empty;
            VariantId = string.Empty;
            CustomerDiscountGroup = string.Empty;
            LoyaltySchemeCode = string.Empty;
            FromDate = new DateTime((isJson) ? 1970 : 1900, 1, 1);
            ToDate = new DateTime((isJson) ? 1970 : 1900, 1, 1);
            ModifyDate = DateTime.Now.ToUniversalTime();
            UnitOfMeasureId = string.Empty;
            MinimumQuantity = 0M;
            CurrencyCode = string.Empty;
            DiscountValue = 0M;
            OfferNo = string.Empty;
            StoreId = string.Empty;
            Description = string.Empty;
            Details = string.Empty;
            ValidationPeriodId = string.Empty;
            Type = ReplDiscountType.Unknown; //Disc. Offer, Multibuy
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string StoreId { get; set; }
        [DataMember]
        public int PriorityNo { get; set; }
        [DataMember]
        public string ItemId { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public string CustomerDiscountGroup { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime FromDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ToDate { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public decimal MinimumQuantity { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public DiscountValueType DiscountValueType { get; set; }
        [DataMember]
        public decimal DiscountValue { get; set; }
        [DataMember]
        public string OfferNo { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ModifyDate { get; set; }
        [DataMember]
        public ReplDiscountType Type { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public string ValidationPeriodId { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountSetupResponse : IDisposable
    {
        public ReplDiscountSetupResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            Discounts = new List<ReplDiscountSetup>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Discounts != null)
                    Discounts.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscountSetup> Discounts { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountSetup : IDisposable
    {
        public ReplDiscountSetup(bool isJson)
        {
            IsDeleted = false;
            PriorityNo = 0;
            DiscountValue = 0M;
            OfferNo = string.Empty;
            Description = string.Empty;
            Details = string.Empty;
            PriceGroup = string.Empty;
            CouponCode = string.Empty;
            CustomerDiscountGroup = string.Empty;
            LoyaltySchemeCode = string.Empty;
            MemberAttribute = string.Empty;
            TenderTypeCode = string.Empty;
            TenderTypeValue = string.Empty;
            Type = ReplDiscountType.Unknown; //Disc. Offer, Multibuy
            Number = string.Empty;
            VariantId = string.Empty;
            UnitOfMeasureId = string.Empty;
            CurrencyCode = string.Empty;
            LinePriceGroup = string.Empty;
            ProductItemCategory = string.Empty;
            ValidFromBeforeExpDate = string.Empty;
            ValidToBeforeExpDate = string.Empty;
            ValidationPeriodId = string.Empty;
            LineGroup = string.Empty;
            StoreGroupCodes = string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string OfferNo { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public ReplDiscountType Type { get; set; }
        [DataMember]
        public int PriorityNo { get; set; }
        [DataMember]
        public string ValidationPeriodId { get; set; }
        [DataMember]
        public DiscountValueType DiscountValueType { get; set; }
        [DataMember]
        public decimal DealPriceValue { get; set; }
        [DataMember]
        public decimal DiscountValue { get; set; }
        [DataMember]
        public decimal DiscountAmountValue { get; set; }
        [DataMember]
        public string CustomerDiscountGroup { get; set; }
        [DataMember]
        public string StoreGroupCodes { get; set; }
        [DataMember]
        public decimal AmountToTrigger { get; set; }
        [DataMember]
        public string LoyaltySchemeCode { get; set; }
        [DataMember]
        public string CouponCode { get; set; }
        [DataMember]
        public decimal CouponQtyNeeded { get; set; }
        [DataMember]
        public string PriceGroup { get; set; }
        [DataMember]
        public ReplDiscMemberType MemberType { get; set; }
        [DataMember]
        public string MemberAttribute { get; set; }
        [DataMember]
        public decimal MaxDiscountAmount { get; set; }
        [DataMember]
        public string TenderTypeCode { get; set; }
        [DataMember]
        public string TenderTypeValue { get; set; }
        [DataMember]
        public bool PromptForAction { get; set; }
        [DataMember]
        public decimal TenderOffer { get; set; }
        [DataMember]
        public decimal TenderOfferAmount { get; set; }
        [DataMember]
        public decimal MemberPoints { get; set; }

        [DataMember]
        public int LineNumber { get; set; }
        [DataMember]
        public ReplDiscountLineType LineType { get; set; }
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public string VariantId { get; set; }
        [DataMember]
        public decimal StandardPriceInclVAT { get; set; }
        [DataMember]
        public decimal StandardPrice { get; set; }
        [DataMember]
        public decimal SplitDealPriceDiscount { get; set; }
        [DataMember]
        public decimal DealPriceDiscount { get; set; }
        [DataMember]
        public string LinePriceGroup { get; set; }
        [DataMember]
        public string CurrencyCode { get; set; }
        [DataMember]
        public string UnitOfMeasureId { get; set; }
        [DataMember]
        public string ProductItemCategory { get; set; }
        [DataMember]
        public string ValidFromBeforeExpDate { get; set; }
        [DataMember]
        public string ValidToBeforeExpDate { get; set; }
        [DataMember]
        public string LineGroup { get; set; }
        [DataMember]
        public int NumberOfItemNeeded { get; set; }
        [DataMember]
        public bool IsPercentage { get; set; }
        [DataMember]
        public decimal LineDiscountAmount { get; set; }
        [DataMember]
        public decimal LineDiscountAmountInclVAT { get; set; }
        [DataMember]
        public decimal OfferPrice { get; set; }
        [DataMember]
        public decimal OfferPriceInclVAT { get; set; }
        [DataMember]
        public bool TriggerPopUp { get; set; }
        [DataMember]
        public int VariantType { get; set; }
        [DataMember]
        public bool Exclude { get; set; }
        [DataMember]
        public decimal LineMemberPoints { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountValidationResponse : IDisposable
    {
        public ReplDiscountValidationResponse()
        {
            LastKey = string.Empty;
            MaxKey = string.Empty;
            RecordsRemaining = 0;
            DiscountValidations = new List<ReplDiscountValidation>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DiscountValidations != null)
                    DiscountValidations.Clear();
            }
        }

        [DataMember]
        public string LastKey { get; set; }
        [DataMember]
        public string MaxKey { get; set; }
        [DataMember]
        public int RecordsRemaining { get; set; }
        [DataMember]
        public List<ReplDiscountValidation> DiscountValidations { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public class ReplDiscountValidation : IDisposable
    {
        public ReplDiscountValidation(bool isJson)
        {
            int year = (isJson) ? 1970 : 1900;

            Id = string.Empty;
            Description = string.Empty;
            StartDate = new DateTime(year, 1, 1);
            EndDate = new DateTime(year, 1, 1);
            StartTime = new DateTime(year, 1, 1);
            EndTime = new DateTime(year, 1, 1);
            MondayStart = new DateTime(year, 1, 1);
            MondayEnd = new DateTime(year, 1, 1);
            TuesdayStart = new DateTime(year, 1, 1);
            TuesdayEnd = new DateTime(year, 1, 1);
            WednesdayStart = new DateTime(year, 1, 1);
            WednesdayEnd = new DateTime(year, 1, 1);
            ThursdayStart = new DateTime(year, 1, 1);
            ThursdayEnd = new DateTime(year, 1, 1);
            FridayStart = new DateTime(year, 1, 1);
            FridayEnd = new DateTime(year, 1, 1);
            SaturdayStart = new DateTime(year, 1, 1);
            SaturdayEnd = new DateTime(year, 1, 1);
            SundayStart = new DateTime(year, 1, 1);
            SundayEnd = new DateTime(year, 1, 1);
            TimeWithinBounds = false;
            EndAfterMidnight = false;
            MondayWithinBounds = false;
            MondayEndAfterMidnight = false;
            TuesdayWithinBounds = false;
            TuesdayEndAfterMidnight = false;
            WednesdayWithinBounds = false;
            WednesdayEndAfterMidnight = false;
            ThursdayWithinBounds = false;
            ThursdayEndAfterMidnight = false;
            FridayWithinBounds = false;
            FridayEndAfterMidnight = false;
            SaturdayWithinBounds = false;
            SaturdayEndAfterMidnight = false;
            SundayWithinBounds = false;
            SundayEndAfterMidnight = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndDate { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime StartTime { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime EndTime { get; set; }
        [DataMember]
        public bool TimeWithinBounds { get; set; }
        [DataMember]
        public bool EndAfterMidnight { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MondayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime MondayEnd { get; set; }
        [DataMember]
        public bool MondayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TuesdayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime TuesdayEnd { get; set; }
        [DataMember]
        public bool TuesdayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime WednesdayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime WednesdayEnd { get; set; }
        [DataMember]
        public bool WednesdayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ThursdayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime ThursdayEnd { get; set; }
        [DataMember]
        public bool ThursdayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime FridayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime FridayEnd { get; set; }
        [DataMember]
        public bool FridayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SaturdayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SaturdayEnd { get; set; }
        [DataMember]
        public bool SaturdayWithinBounds { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SundayStart { get; set; }
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime SundayEnd { get; set; }
        [DataMember]
        public bool SundayWithinBounds { get; set; }
        [DataMember]
        public bool MondayEndAfterMidnight { get; set; }
        [DataMember]
        public bool TuesdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool WednesdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool ThursdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool FridayEndAfterMidnight { get; set; }
        [DataMember]
        public bool SaturdayEndAfterMidnight { get; set; }
        [DataMember]
        public bool SundayEndAfterMidnight { get; set; }
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ReplDiscountType
    {
        [EnumMember]
        Multibuy = 0,
        [EnumMember]
        MixAndMatch = 1,
        [EnumMember]
        DiscOffer = 2,
        [EnumMember]
        TotalDiscount = 3,
        [EnumMember]
        TenderType = 4,
        [EnumMember]
        ItemPoint = 5,
        [EnumMember]
        LineDiscount = 6,
        [EnumMember]
        Unknown = 99
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum DiscountValueType
    {
        [EnumMember]
        DealPrice = 0,
        [EnumMember]
        Percent = 1,
        [EnumMember]
        Amount = 2,
        [EnumMember]
        LeastExpensive = 3,
        [EnumMember]
        LineSpec = 4
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ReplDiscMemberType
    {
        [EnumMember]
        Scheme = 0,
        [EnumMember]
        Club = 1
    }

    [DataContract(Namespace = "http://lsretail.com/LSOmniService/Base/2017")]
    public enum ReplDiscountLineType
    {
        [EnumMember]
        Item = 0,
        [EnumMember]
        ProductGroup = 1,
        [EnumMember]
        ItemCategory = 2,
        [EnumMember]
        All = 3,
        [EnumMember]
        SpecialGroup = 4
    }
}
