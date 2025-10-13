using System;
using System.Linq;
using System.Collections.Generic;

namespace LSRetail.Omni.Domain.DataModel.Base
{
    public enum PageExtension
    {
        Empty = 0,
        Default,
        DefaultAspx,
        Phone,
        PhoneAspx,
        Tablet,
        TabletAspx,
    }

    public enum ErrorCode
    {
        MissingInternetProtocol = 1,
        MissingComputerName,
        MissingInstanceName,
        InvalidUrlFormat,
        MissingSaasTenant,
    }

    public class UrlParser
    {
        private string internetProtocol = string.Empty;
        private string computerName = string.Empty;
        private string port = string.Empty;
        private string instanceName = string.Empty;
        private string company = string.Empty;
        private string page = string.Empty;
        private string tenant = string.Empty;
        private string otherSettings = string.Empty;

        public List<string> Errors { get; } = new List<string>();
        public List<ErrorCode> ErrorCodes { get; } = new List<ErrorCode>();

        public bool Valid { get; private set; }
        public bool Empty { get; private set; }
        public bool IsSaas { get; set; } = false;

        public PageExtension PageType { get; set; } = PageExtension.Empty;

        public string InternetProtocol
        {
            get => internetProtocol;
            private set => internetProtocol = value ?? string.Empty;
        }

        public string ComputerName
        {
            get => computerName;
            private set => computerName = value ?? string.Empty;
        }

        public string Port
        {
            get => port;
            private set => port = value ?? string.Empty;
        }

        public string InstanceName
        {
            get => instanceName;
            private set => instanceName = value ?? string.Empty;
        }

        public string Company
        {
            get => company;
            private set => company = value ?? string.Empty;
        }

        public string Page
        {
            get => page;
            private set => page = value ?? string.Empty;
        }

        public string Tenant
        {
            get => tenant;
            private set => tenant = value ?? string.Empty;
        }

        public string OtherSettings
        {
            get => otherSettings;
            private set => otherSettings = value ?? string.Empty;
        }

        public UrlParser(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Validate(url);
                return;
            }

            url = url.Trim().TrimEnd('/', '?', '&');

            ParseUrl(url);
        }

        public UrlParser(string selectedSecurityStandard, string computerName, string webServiceInstance, string port = "", string company = "", string page = "", string tenant = "", string otherSettings = "")
        {
            InternetProtocol = selectedSecurityStandard == null ? "" : selectedSecurityStandard.ToLower();
            ComputerName = computerName;
            InstanceName = webServiceInstance;
            Port = port;
            Company = company;
            Page = page;
            Tenant = tenant;
            OtherSettings = otherSettings;
        }

        public string GetUrl(bool isSaas = false, bool inTabletMode = false, string username = "", string password = "")
        {
            var protocol = InternetProtocol == "https" ? "https://" : "http://";
            var baseUrl = $"{protocol}{ComputerName}{(string.IsNullOrEmpty(Port) ? "" : $":{Port}")}";

            var path = string.Empty;
            if (isSaas && !string.IsNullOrEmpty(Tenant))
            {
                path = $"/{Tenant}/{InstanceName}";
            }
            else
            {
                path = $"/{InstanceName}";
            }

            if (inTabletMode)
            {
                path += "/tablet.aspx";
            }

            var queryParameters = new List<string>();

            if (!string.IsNullOrEmpty(Company))
            {
                queryParameters.Add($"company={Company}");
            }

            if (!string.IsNullOrEmpty(Page))
            {
                queryParameters.Add($"page={Page}");
            }

            if (!isSaas && !string.IsNullOrEmpty(Tenant))
            {
                queryParameters.Add($"tenant={Tenant}");
            }

            if (!string.IsNullOrEmpty(OtherSettings))
            {
                queryParameters.Add(OtherSettings);
            }
            queryParameters.Add("redirect=0");
            
            var queryString = string.Join("&", queryParameters);
            var userNameWithDomain = GetUserNameWithDomain(username);

            if (!string.IsNullOrEmpty(userNameWithDomain) && !string.IsNullOrEmpty(password))
            {
                baseUrl = baseUrl.Replace(protocol, $"{protocol}{userNameWithDomain}:{password}@");
            }

            var url = $"{baseUrl}{path}{(queryParameters.Any() ? "?" + queryString : "")}";

            Validate(url, isSaas);
            return url;
        }

        private string GetUserNameWithDomain(string username)
        {
            if (username.Contains("\\"))
            {
                var splitName = username.Split('\\');
                return $"{splitName[1]}@{splitName[0]}";
            }
            return username;
        }

        public static bool DetectSaasUrl(string url, PageExtension pageType)
        {
            var partsToDetect = pageType == PageExtension.Empty ? 1 : 2;
            try
            {
                var uri = new Uri(url);
                var l = uri.LocalPath;
                var parts = l.Split('/');
                var partCount = 0;
                foreach (string s in parts)
                {
                    if (s.Trim().Length > 0)
                    {
                        partCount++;
                    }
                }

                return partCount > partsToDetect;
            }
            catch (UriFormatException)
            {
            }
            return false;
        }

        public void DetectPageExtension(string url)
        {
            var parts = url.Split('?')[0].Split('/');
            if (parts.Length > 0)
            {
                switch (parts.Last().ToLower())
                {
                    case "default":
                        PageType = PageExtension.Default;
                        break;
                    case "default.aspx":
                        PageType = PageExtension.DefaultAspx;
                        break;
                    case "phone":
                        PageType = PageExtension.Phone;
                        break;
                    case "phone.aspx":
                        PageType = PageExtension.PhoneAspx;
                        break;
                    case "tablet":
                        PageType = PageExtension.Tablet;
                        break;
                    case "tablet.aspx":
                        PageType = PageExtension.TabletAspx;
                        break;
                    default:
                        PageType = PageExtension.Empty;
                        break;
                }
            }

            IsSaas = DetectSaasUrl(url, PageType);
        }

        public void ParseUrl(string url)
        {
            var orgUrl = url;
            if (string.IsNullOrEmpty(url))
            {
                Validate(url);
                return;
            }

            SetSecurityStandard(ref url); //removes http:// or https://
            var tempParams = url.Split('?');
            var tempUrlparams = "";
            if (tempParams.Length > 1)
            {
                url = tempParams[0];
                if (url.EndsWith("/"))
                {
                    url = url.TrimEnd('/');
                }

                tempUrlparams = tempParams[1];
            }
            else
            {
                tempUrlparams = string.Empty;
            }

            DetectPageExtension(url);

            var splitUrl = url.Split('/');

            if (splitUrl.Length > 1)
            {
                if (PageType != PageExtension.Empty)
                {
                    splitUrl = splitUrl.Take(splitUrl.Count() - 1).ToArray();
                }
            }

            if (splitUrl.Length > 0)
            {
                SetComputerNameAndPort(splitUrl[0]);
            }

            if (splitUrl.Length == 2)
            {
                InstanceName = splitUrl[1];
            }

            if (splitUrl.Length == 3)
            {
                //IsSaas = true;
                Tenant = splitUrl[1];
                InstanceName = splitUrl[2];
            }

            if (splitUrl.Length == 4)
            {
                //IsSaas = true;
                Tenant = splitUrl[1];
                InstanceName = splitUrl[2];
            }

            ParseSettings(tempUrlparams);
            Valid = Validate(orgUrl);
        }

        private void SetSecurityStandard(ref string url)
        {
            if (url.ToLower().StartsWith("http://"))
            {
                InternetProtocol = "http";
                url = url.Substring(7);
            }
            else if (url.ToLower().StartsWith("https://"))
            {
                InternetProtocol = "https";
                url = url.Substring(8);
            }
            else
            {
                InternetProtocol = "http"; // default to http
            }
        }

        private void SetComputerNameAndPort(string computerAndPort)
        {
            var parts = computerAndPort.Split(':');
            ComputerName = parts[0];
            if (parts.Length == 2)
            {
                Port = parts[1];
            }
        }

        private void ParseSettings(string otherSettingsParam)
        {
            var settings = otherSettingsParam.Split('&');
            foreach (var setting in settings)
            {
                var splitSettings = setting.Split('=');
                if (splitSettings.Length == 2)
                {
                    switch (splitSettings[0].ToLower())
                    {
                        case "company":
                            Company = splitSettings[1];
                            break;
                        case "page":
                            Page = splitSettings[1];
                            break;
                        case "tenant":
                            Tenant = splitSettings[1];
                            break;
                        case "redirect":
                            break;
                        default:
                            if (!string.IsNullOrEmpty(OtherSettings))
                            {
                                OtherSettings += "&";
                            }
                            OtherSettings += setting;
                            break;
                    }
                }
            }
        }

        public bool Validate(string url, bool isSaas = false)
        {
            Errors.Clear();
            ErrorCodes.Clear();
            if (string.IsNullOrEmpty(InternetProtocol))
            {
                if (!ErrorCodes.Contains(ErrorCode.MissingInternetProtocol))
                {
                    ErrorCodes.Add(ErrorCode.MissingInternetProtocol);
                }
            }
            if (string.IsNullOrEmpty(ComputerName))
            {
                if (!ErrorCodes.Contains(ErrorCode.MissingComputerName))
                {
                    ErrorCodes.Add(ErrorCode.MissingComputerName);
                }
            }
            if (string.IsNullOrEmpty(InstanceName))
            {
                if (!ErrorCodes.Contains(ErrorCode.MissingInstanceName))
                {
                    ErrorCodes.Add(ErrorCode.MissingInstanceName);
                }
            }

            if (isSaas)
            {
                if (!ErrorCodes.Contains(ErrorCode.MissingSaasTenant))
                {
                    ErrorCodes.Add(ErrorCode.MissingSaasTenant);
                }
            }

            if (ErrorCodes.Count == 0)
            {
                try
                {
                    new Uri(url);
                }
                catch (UriFormatException ex)
                {
                    if (!ErrorCodes.Contains(ErrorCode.InvalidUrlFormat))
                    {
                        ErrorCodes.Add(ErrorCode.InvalidUrlFormat);
                    }
                    Errors.Add(ex.Message);
                }
            }

            Valid = ErrorCodes.Count == 0;
            Empty = string.IsNullOrEmpty(url);
            return Valid;
        }
    }
}