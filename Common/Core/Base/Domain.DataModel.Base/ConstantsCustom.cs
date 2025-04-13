﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public static class Constants
    {
        public const string CAT_TOBACCO = "TOBACCO";
        public const string AD_CONSENT_CIGARETTE = "TOB_CIG";
        public const string AD_CONSENT_CIGAR = "TOB_CIGAR";
        public const string AD_CONSENT_DIP = "TOB_DIP";
        public const string AD_CONSENT_ONP = "TOB_ONP";
        public const string AD_CONSENT_SNUS = "TOB_SNUS";
        public const string REPLY_ACCEPTED = "ACCEPTED";
        public const string REPLY_DENIED = "DENIED";
        public const string AGE_VERIFIED = "EAIV";
        public const string STATUS_OK = "OK";
        public const string VAR_UUID = "UUID";
        public const string REDI_PENDING = "1";
        public const string REDI_ACCEPTED = "2";
        public const string REDI_DENIED = "3";
        public const string REDI_REDOACCEPTED = "4";
        public const string PREF_DEVTOKEN_NO = "DeviceTokenNumber";
        public const string PREF_DEVTOKEN_DATE = "DeviceTokenDate";
        public const string FIREBASE_TOPIC_DEFAULT = "general";
        public const int HIST_MAX = 50;
        public const int MIN_REQ_BUILD = 43;
        public const int CS_BUILD = 43;
    }
}
