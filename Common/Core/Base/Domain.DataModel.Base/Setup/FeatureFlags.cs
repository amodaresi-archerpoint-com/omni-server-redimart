﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LSRetail.Omni.Domain.DataModel.Base.Setup
{
    public class FeatureFlags
    {
        [DataMember]
        public List<FeatureFlag> Flags { get; set; } = new List<FeatureFlag>();

        public void AddFlag(FeatureFlagName flagName, string flagValue)
        {
            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
            {
                Flags.Add(new FeatureFlag()
                {
                    Name = flagName,
                    Value = flagValue
                });
            }
            else
            {
                flag.Value = flagValue;
            }
        }

        public void AddFlag(FeatureFlagName flagName, int flagValue)
        {
            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
            {
                Flags.Add(new FeatureFlag()
                {
                    Name = flagName,
                    Value = flagValue.ToString()
                });
            }
            else
            {
                flag.Value = flagValue.ToString();
            }
        }

        public void AddFlag(string flagCode, string flagValue)
        {
            FeatureFlagName flagName = FeatureFlagName.None;

            switch (flagCode)
            {
                case "ALLOW AUTO LOGOFF":
                    flagName = FeatureFlagName.AllowAutoLogoff;
                    break;
                case "ALLOW LS CENTRAL LOGIN":
                    flagName = FeatureFlagName.AllowCentralLogin;
                    break;
                case "ALLOW OFFLINE":
                    flagName = FeatureFlagName.AllowOffline;
                    break;
                case "SEND RECEIPT IN EMAIL":
                    flagName = FeatureFlagName.SendReceiptInEmail;
                    break;
                case "USE LOYALITY SYSTEM":
                    flagName = FeatureFlagName.UseLoyaltySystem;
                    break;
                case "POS SHOW INVENTORY":
                    flagName = FeatureFlagName.PosShowInventory;
                    break;
                case "POS INVENTORY LOOKUP":
                    flagName = FeatureFlagName.PosInventoryLookup;
                    break;
                case "SETTINGS PASSWORD":
                    flagName = FeatureFlagName.SettingsPassword;
                    break;
                case "HIDE VOIDED TRANSACTION":
                    flagName = FeatureFlagName.HideVoidedTransaction;
                    break;
                //SPG
                case "ALLOW ANONYMOUS":
                    flagName = FeatureFlagName.AllowAnonymousUser;
                    break;
                case "ALLOW SHOP HOME":
                    flagName = FeatureFlagName.AllowShopHome;
                    break;
                case "DEVICE TYPE":
                    flagName = FeatureFlagName.DeviceType;
                    break;
                case "CATALOG TYPE":
                    flagName = FeatureFlagName.CatalogType;
                    break;
                case "DEFAULT WEB STORE":
                    flagName = FeatureFlagName.DefaultWebStore;
                    break;
                case "ALLOWED PAYMENT WITH POS":
                    flagName = FeatureFlagName.AllowedPaymentWithPOS;
                    break;
                case "ALLOWED PAYMENT WITH CARD":
                    flagName = FeatureFlagName.AllowedPaymentWithCard;
                    break;
                case "ALLOWED PAYMENT WITH LOYALTY":
                    flagName = FeatureFlagName.AllowedPaymentWithLoyalty;
                    break;
                case "ALLOWED PAYMENT CUSTOMER ACCOUNT":
                    flagName = FeatureFlagName.AllowedPaymentToCustomerAccount;
                    break;
                case "CHECK STATUS TIMER":
                    flagName = FeatureFlagName.CheckStatusTimer;
                    break;
                case "TERMS AND CONDITION URL":
                    flagName = FeatureFlagName.TermsAndConditionURL;
                    break;
                case "TERMS AND CONDITION VERSION":
                    flagName = FeatureFlagName.TermsAndConditionVersion;
                    break;
                case "PRIVACY POLICY URL":
                    flagName = FeatureFlagName.PrivacyPolicyURL;
                    break;
                case "PRIVACY POLICY VERSION":
                    flagName = FeatureFlagName.PrivacyPolicyVersion;
                    break;
                case "ENABLE PLATFORM PAYMENT":
                    flagName = FeatureFlagName.EnablePlatformPayment;
                    break;

                case "ADD CARD BEFORE SHOPPING":
                    flagName = FeatureFlagName.AddCardBeforeShopping;
                    break;

                case "SHOW CUSTOMER QR CODE":
                    flagName = FeatureFlagName.ShowCustomerQrCode;
                    break;
                case "SHOW POINT STATUS":
                    flagName = FeatureFlagName.ShowPointStatus;
                    break;
                case "USE SECURITY CHECK":
                    flagName = FeatureFlagName.UseSecurityCheck;
                    break;
                case "HIDE PRICE OF ITEM":
                    flagName = FeatureFlagName.HidePriceOfItem;
                    break;
                case "HIDE ADD CREDIT CARD":
                    flagName = FeatureFlagName.HideAddCreditCard;
                    break;
                case "HIDE SHOPPING SCREEN":
                    flagName = FeatureFlagName.HideShoppingScreen;
                    break;
                case "USE ONLINE SEARCH":
                    flagName = FeatureFlagName.UseOnlineSearch;
                    break;
                case "CURRENCY CODE":
                    flagName = FeatureFlagName.CurrencyCode;
                    break;
                case "ENABLE NOTIFICATIONS":
                    flagName = FeatureFlagName.EnableNotifications;
                    break;

                //AUDKENNI SPG
                case "AUDKENNI BASE URL":
                    flagName = FeatureFlagName.AudkenniBaseURL;
                    break;
                case "AUDKENNI CLIENT ID":
                    flagName = FeatureFlagName.AudkenniClientId;
                    break;
                case "AUDKENNI REDIRECT URL":
                    flagName = FeatureFlagName.AudkenniRedirectURL;
                    break;
                case "AUDKENNI SECRET":
                    flagName = FeatureFlagName.AudkenniSecret;
                    break;
                case "AUDKENNI MESSAGE TO USER":
                    flagName = FeatureFlagName.AudkenniMessageToUser;
                    break;
                case "AUDKENNI LOGIN ENABLED":
                    flagName = FeatureFlagName.AudkenniLoginEnabled;
                    break;
                case "AUDKENNI TEST USER ENABLED":
                    flagName = FeatureFlagName.AudkenniTestUserEnabled;
                    break;
                case "AUDKENNI TEST USER":
                    flagName = FeatureFlagName.AudkenniTestUser;
                    break;
                case "AUDKENNI TEST USER CARDID":
                    flagName = FeatureFlagName.AudkenniTestCardId;
                    break;
                case "AUDKENNI TEXT TO MAKE HASH":
                    flagName = FeatureFlagName.AudkenniTextToMakeAHash;
                    break;
                case "FACEBOOK LOGIN ENABLED":
                    flagName = FeatureFlagName.FacebookLoginEnabled;
                    break;
                case "GOOGLE LOGIN ENABLED":
                    flagName = FeatureFlagName.GoogleLoginEnabled;
                    break;
                case "GOOGLE IOS CLIENT ID":
                    flagName = FeatureFlagName.GoogleIosClientId;
                    break;
                case "APPLE LOGIN ENABLED":
                    flagName = FeatureFlagName.AppleLoginEnabled;
                    break;
                case "OPEN GATE":
                    flagName = FeatureFlagName.OpenGate;
                    break;
                case "CLOSE GATE":
                    flagName = FeatureFlagName.CloseGate;
                    break;
                case "SHOW CUSTOMER SURVEY":
                    flagName = FeatureFlagName.ShowCustomerSurvey;
                    break;

                //Card Payments
                case "CARD PAYMENT METHOD":
                    flagName = FeatureFlagName.CardPaymentMethod;
                    break;
                case "LS PAY SERVICE IP ADDRESS":
                    flagName = FeatureFlagName.LsPayServiceIpAddress;
                    break;
                case "LS PAY SERVICE PORT":
                    flagName = FeatureFlagName.LsPayServicePort;
                    break;
                case "LS PAY PLUGIN ID":
                    flagName = FeatureFlagName.LsPayPluginId;
                    break;
                case "LS PAY APPLEPAY PLUGIN ID":
                    flagName = FeatureFlagName.LsPayApplePluginId;
                    break;
                case "LS PAY GOOGLE PLUGIN ID":
                    flagName = FeatureFlagName.LsPayGooglePluginId;
                    break;
            }

            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
            {
                Flags.Add(new FeatureFlag()
                {
                    Name = flagName,
                    Value = flagValue
                });
            }
            else
            {
                flag.Value = flagValue;
            }
        }


        public bool GetFlagBool(FeatureFlagName flagName, bool defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
                return defaultValue;

            try
            {
                return Convert.ToInt16(flag.Value) == 1;
            }
            catch
            {
                try
                {
                    return Convert.ToBoolean(flag.Value);
                }
                catch
                {
                    try
                    {
                        return flag.Value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
            }
        }

        public int GetFlagInt(FeatureFlagName flagName, int defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(flag.Value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetFlagString(FeatureFlagName flagName, string defaultValue)
        {
            FeatureFlag flag = Flags.Find(f => f.Name == flagName);
            if (flag == null)
                return defaultValue;

            return (flag.Value == null) ? string.Empty : flag.Value;
        }
    }

    public class FeatureFlag
    {
        public FeatureFlagName Name { get; set; } = FeatureFlagName.None;
        public string Value { get; set; } = string.Empty;
    }

    public enum FeatureFlagName
    {
        None = 100,
        AllowAutoLogoff = 101,
        AutoLogOffAfterMin = 102,
        AllowOffline = 103,
        ExitAfterEachTransaction = 104,
        SendReceiptInEmail = 105,
        ShowNumberPad = 106,
        UseLoyaltySystem = 107,
        PosShowInventory = 108,
        PosInventoryLookup = 109,
        SettingsPassword = 110,
        HideVoidedTransaction = 111,
        AllowCentralLogin = 112,

        //ScanPayGo
        AllowAnonymousUser = 200,
        AllowShopHome = 201,
        DeviceType = 202,
        CatalogType = 203,
        DefaultWebStore = 204,
        AllowedPaymentWithPOS = 205,
        AllowedPaymentWithCard = 206,
        AllowedPaymentWithLoyalty = 207,
        CardPaymentType = 208,
        CheckStatusTimer = 209,
        TermsAndConditionURL = 210,
        TermsAndConditionVersion = 211,
        OpenGate = 212,
        CloseGate = 213,
        PrivacyPolicyURL = 214,
        PrivacyPolicyVersion = 215,
        ShowCustomerSurvey = 216,
        AddCardBeforeShopping = 217,
        ShowCustomerQrCode = 218,
        ShowPointStatus = 219,
        UseSecurityCheck = 220,
        HidePriceOfItem = 221,
        HideAddCreditCard = 222,
        HideShoppingScreen = 223,
        UseOnlineSearch = 224,
        CurrencyCode = 225,
        AllowedPaymentToCustomerAccount = 226,
        EnableNotifications = 227,

        //ScanPayGoPaymentFlags
        EnablePlatformPayment = 300,
        CardPaymentMethod = 307,
        LsPayServiceIpAddress = 308,
        LsPayServicePort = 309,
        LsPayPluginId = 310,
        LsPayApplePluginId = 311,
        LsPayGooglePluginId = 312,

        //Alternate Logins
        //AudkenniFlags
        AudkenniBaseURL = 400,
        AudkenniClientId = 401,
        AudkenniRedirectURL = 402,
        AudkenniSecret = 403,
        AudkenniMessageToUser = 404,
        AudkenniLoginEnabled = 405,

        AudkenniTestUserEnabled = 406,
        AudkenniTestUser = 407,
        AudkenniTestCardId = 408,
        AudkenniTextToMakeAHash= 409,

        //Google
        GoogleLoginEnabled = 410,
        GoogleIosClientId = 411,

        //Facebook
        FacebookLoginEnabled = 420,

        //Apple
        AppleLoginEnabled = 430,
    }
}
