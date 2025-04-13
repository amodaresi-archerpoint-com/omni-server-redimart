﻿using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;

using LSOmni.BLL;
using LSOmni.Common.Util;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Base.Setup;
using LSRetail.Omni.Domain.DataModel.Base.Utils;
using LSRetail.Omni.Domain.DataModel.Base.Menu;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Items;
using LSRetail.Omni.Domain.DataModel.Loyalty.Baskets;
using LSRetail.Omni.Domain.DataModel.Loyalty.Setup;

namespace LSOmni.Service
{
    /// <summary>
    /// Base class for private methods shared 
    /// </summary>
    public partial class LSOmniBase
    {
        private string GetImageStreamUrl(ImageView imgView)
        {
            if (imgView == null)
            {
                logger.Warn(config.LSKey.Key, "imgView is null");
                return "";
            }
            // return http://localhost/LSOmniService/ucjson.svc/ImageStreamGetById?id=66&width=255&height=455
            return string.Format("{0}/ImageStreamGetById?id={1}&width={2}&height={3}", BaseUrlReturnedToClient(), imgView.Id, imgView.ImgSize.Width, imgView.ImgSize.Height);
        }

        private string BaseUrlReturnedToClient()
        {
            // returns the base URL

            //dont fully trust baseUri - hence this method...

            //have to be carefull of the url reurned to client since Host unde IIS is tricky
            //returns the correct URL without the URL Request
            //It returns  http://macbookjij.lsretail.local/LSOmniService/json.svc 
            //  or http://192.22.11.12/LSOmniService.json.svc  

            //System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath "/LSOmniService" 

            //build the URL based on defaults
            // serverUri = http://macbookjij.lsretail.local/LSOmniService/Json.svc/ImageGetById
            //  baseUriOrignalString =  http://macbookjij.lsretail.local/LSOmniService/Json.svc   

            //find the index of .svc just in case some crap is added to baseOriginalString
            string svc = "ucservice.svc"; //http://macbookjij.lsretail.local/LSOmniService/Json.svc/ 
            string srvUrl = baseUriOrignalString.ToLower();  //note orgianlString also has the port number = good

            int idx = srvUrl.IndexOf(svc);
            if (idx > 0)
            {
                srvUrl = srvUrl.Substring(0, idx); // => http://macbookjij.lsretail.local/LSOmniService/json.svc
                srvUrl += "ucjson.svc";
            }
            return srvUrl;
        }

        #region protected members

        protected string LogJson(object objIn)
        {
            string sOut = "";
            if (logger.IsDebugEnabled || logger.IsErrorEnabled)
            {
                if (objIn == null)
                    return "";
                try
                {
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(objIn.GetType());
                        ser.WriteObject(memstream, objIn);
                        if (memstream.Length > 2097152)
                            return "> Json Object too big (size > 2MB) <";

                        sOut = Encoding.Default.GetString(memstream.ToArray());
                        //remove the password
                        if (sOut.Contains("\"Password\":"))
                        {
                            int idx = sOut.IndexOf("\"Password\":");
                            if (idx > 0)
                            {
                                int nextIdx = sOut.IndexOf(",", idx + 1); //find  the next comma and remove
                                sOut = sOut.Remove(idx + "\"Password\":".Length, nextIdx - idx - "\"Password\":".Length);
                            }
                        }
                        sOut = string.Format("\r\nFormatted json for logging. Size(bytes):{0}\r\n{1}\r\n", sOut.Length.ToString(), sOut);
                    }
                }
                catch (Exception)
                {
                    logger.Error(config.LSKey.Key, Serialization.ToXml(objIn, true));
                }
            }
            return sOut; //FormatOutput(sOut);
        }

        protected virtual void HandleExceptions(Exception ex, string errMsg, params object[] args)
        {
            HandleExceptions(ex, string.Format(errMsg, args));
        }

        protected virtual void HandleExceptions(Exception ex, string errMsg)
        {
            logger.Error(config.LSKey.Key, ex, errMsg);

            if (ex.GetType() == typeof(LSOmniServiceException))
            {
                LSOmniServiceException lEx = (LSOmniServiceException)ex;
                throw new WebFaultException<LSOmniException>(new LSOmniException(lEx.StatusCode, "ERROR:" + errMsg + " - " + lEx.Message), exStatusCode);
            }
            if (ex.GetType() == typeof(LSOmniException))
            {
                throw new WebFaultException<LSOmniException>((LSOmniException)ex, exStatusCode);
            }

            throw new WebFaultException<LSOmniException>(new LSOmniException(StatusCode.Error, errMsg + " - " + ex.Message), exStatusCode);
        }

        #endregion protected members

        #region private members

        public virtual string Version()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }

        private BOConfiguration GetConfig(BOConfiguration config)
        {
            ConfigBLL bll = new ConfigBLL();
            string token = config.SecurityToken;
            bool json = config.IsJson;
            if (string.IsNullOrEmpty(serverUri))
            {
                return null;
            }

            //Check default LSNAV Appsettings values (single tenant)
            if (ConfigSetting.KeyExists("BOConnection.Nav.UserName"))
            {
                //Get default configuration
                config = bll.ConfigGet("");
                config.IsJson = json;
                config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUser.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.UserName");

                if (ConfigSetting.KeyExists("BOConnection.Nav.Password"))
                {
                    string pwd = ConfigSetting.GetString("BOConnection.Nav.Password");
                
                    //check if the password has been encrypted by our LSOmniPasswordGenerator.exe
                    if (DecryptConfigValue.IsEncryptedPwd(pwd))
                    {
                        pwd = DecryptConfigValue.DecryptString(pwd, ConfigSetting.GetEncrCode());
                    }
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOPassword.ToString()).Value = pwd;
                }

                if (ConfigSetting.KeyExists("BOConnection.Nav.Url"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOUrl.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.Url");
                if (ConfigSetting.KeyExists("BOConnection.Nav.QryUrl"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOQryUrl.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.QryUrl");
                if (ConfigSetting.KeyExists("BOConnection.Nav.ODataUrl"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOODataUrl.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.ODataUrl");

                if (ConfigSetting.KeyExists("BOConnection.Nav.Protocol"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOProtocol.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.Protocol");
                if (ConfigSetting.KeyExists("BOConnection.Nav.Tenant"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOTenant.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.Tenant");

                if (ConfigSetting.KeyExists("SqlConnectionString.Nav"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOSql.ToString()).Value = ConfigSetting.GetString("SqlConnectionString.Nav");

                //TimeoutInSeconds from client can overwrite BOConnection.NavSQL.Timeout
                if (ConfigSetting.KeyExists("BOConnection.Nav.Timeout"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOTimeout.ToString()).Value = (ConfigSetting.GetInt("BOConnection.Nav.Timeout")).ToString();  //millisecs,  60 seconds
                if (ConfigSetting.KeyExists("BOConnection.Nav.Encoding"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.BOEncode.ToString()).Value = ConfigSetting.GetString("BOConnection.Nav.Encoding");  //millisecs,  60 seconds

                if (ConfigSetting.KeyExists("ECom.Url"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.EComUrl.ToString()).Value = ConfigSetting.GetString("ECom.Url");

                if (ConfigSetting.KeyExists("Security.EncrCode"))
                    config.Settings.FirstOrDefault(x => x.Key == ConfigKey.EncrCode.ToString()).Value = ConfigSetting.GetEncrCode();
                if (ConfigSetting.KeyExists("Security.Validatetoken"))
                    config.SecurityCheck = ConfigSetting.GetBoolean("Security.Validatetoken");
            }

            //check in db
            else if (bll.ConfigKeyExists(ConfigKey.LSKey, config.LSKey.Key))
            {
                config = bll.ConfigGet(config.LSKey.Key);
            }
            else if (serverUri.Contains("PortalService.svc") || serverUri.Contains("PortalJson.svc"))
            {
                config = bll.ConfigGet("");
            }
            else
            {
                throw new LSOmniServiceException(StatusCode.LSKeyInvalid, " Invalid LSRETAIL-KEY");
            }

            //Validate securitytoken if not ecommerce
            //TODO: add settings in db for sec token validation
            //config.SecurityCheck = serverUri.ToUpper().Contains("UCSERVICE") == false;
            config.SecurityToken = token;
            return config;
        }

        private void MenuSetLocation(MobileMenu mobileMenu)
        {
            if (mobileMenu == null)
                return;

            for (int k = 0; k < (mobileMenu.Items != null ? mobileMenu.Items.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Items[k].Images != null && mobileMenu.Items[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Items[k].Images.Count; m++)
                    {
                        if (mobileMenu.Items[k].Images[m] != null)
                            mobileMenu.Items[k].Images[m].StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.Items[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.Products != null ? mobileMenu.Products.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Products[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Products[k].Images.Count; m++)
                    {
                        if (mobileMenu.Products[k].Images[m] != null)
                            mobileMenu.Products[k].Images[m].StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.Products[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.Recipes != null ? mobileMenu.Recipes.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Recipes[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Recipes[k].Images.Count; m++)
                    {
                        if (mobileMenu.Recipes[k].Images[m] != null)
                            mobileMenu.Recipes[k].Images[m].StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.Recipes[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.Deals != null ? mobileMenu.Deals.Count : 0); k++)
            {
                //get all the iviews Location
                if (mobileMenu.Deals[k].Images.Count > 0)
                {
                    for (int m = 0; m < mobileMenu.Deals[k].Images.Count; m++)
                    {
                        if (mobileMenu.Deals[k].Images[m] != null)
                            mobileMenu.Deals[k].Images[m].StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.Deals[k].Images[m].Id));
                    }
                }
            }

            for (int k = 0; k < (mobileMenu.MenuNodes != null ? mobileMenu.MenuNodes.Count : 0); k++)
            {
                if (mobileMenu.MenuNodes[k].Image != null)
                    mobileMenu.MenuNodes[k].Image.StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.MenuNodes[k].Image.Id));

                if (mobileMenu.MenuNodes[k].MenuNodes != null)
                {
                    for (int m = 0; m < mobileMenu.MenuNodes[k].MenuNodes.Count; m++)
                    {
                        if (mobileMenu.MenuNodes[k].MenuNodes[m] != null)
                            mobileMenu.MenuNodes[k].MenuNodes[m].Image.StreamURL = GetImageStreamUrl(new ImageView(mobileMenu.MenuNodes[k].MenuNodes[m].Image.Id));
                    }
                }
            }
        }

        #endregion private members

        #region set location

        private void SearchSetLocation(SearchRs rs)
        {
            if (rs == null)
                return;

            foreach (ItemCategory ic in rs.ItemCategories)
            {
                ItemCategorySetLocation(ic);
            }
            foreach (LoyItem item in rs.Items)
            {
                ItemSetLocation(item);
            }

            foreach (Notification noty in rs.Notifications)
            {
                NotificationSetLocation(noty);
            }
            foreach (ProductGroup p in rs.ProductGroups)
            {
                ProductGroupSetLocation(p);
            }

            foreach (OneList s in rs.OneLists)
            {
                OneListSetLocation(s);
            }
            foreach (Store s in rs.Stores)
            {
                StoreSetLocation(s);
            }
        }

        private void ItemCategorySetLocation(ItemCategory itemCategory)
        {
            if (itemCategory == null)
                return;

            foreach (ImageView iv in itemCategory.Images)
            {
                iv.StreamURL = GetImageStreamUrl(iv);
            }
            foreach (ProductGroup pg in itemCategory.ProductGroups)
            {
                foreach (ImageView iv in pg.Images)
                {
                    iv.StreamURL = GetImageStreamUrl(iv);
                }
                foreach (LoyItem it in pg.Items)
                {
                    foreach (ImageView iv in it.Images)
                    {
                        iv.StreamURL = GetImageStreamUrl(iv);
                    }
                }
            }
        }

        private void ProductGroupSetLocation(ProductGroup productGroup)
        {
            if (productGroup == null)
                return;

            foreach (ImageView iv in productGroup.Images)
            {
                iv.StreamURL = GetImageStreamUrl(iv);
            }
            foreach (LoyItem it in productGroup.Items)
            {
                foreach (ImageView iv in it.Images)
                {
                    iv.StreamURL = GetImageStreamUrl(iv);
                }
            }
        }

        private void StoreSetLocation(Store store)
        {
            if (store == null || store.Images == null)
                return;

            foreach (ImageView iv in store.Images)
            {
                iv.StreamURL = GetImageStreamUrl(iv);
            }
        }

        private void NotificationSetLocation(Notification notification)
        {
            if (notification == null)
                return;
            foreach (ImageView iv in notification.Images)
            {
                iv.StreamURL = GetImageStreamUrl(iv);
            }
        }

        private void ItemSetLocation(LoyItem item)
        {
            if (item == null)
                return;

            foreach (ImageView iv in item.Images)
            {
                iv.StreamURL = GetImageStreamUrl(iv);
            }
            foreach (VariantRegistration variant in item.VariantsRegistration)
            {
                foreach (ImageView iv2 in variant.Images)
                {
                    iv2.StreamURL = GetImageStreamUrl(iv2);
                }
            }
            foreach (VariantRegistration variant in item.VariantsRegistration)
            {
                foreach (ImageView iv2 in variant.Images)
                {
                    iv2.StreamURL = GetImageStreamUrl(iv2);
                }
            }
        }

        private void OneListSetLocation(List<OneList> lists)
        {
            if (lists.Count == 0)
                return;
            foreach (OneList list in lists)
            {
                OneListSetLocation(list);
            }
        }

        private void OneListSetLocation(OneList list)
        {
            if (list == null || list.Items == null)
                return;

            foreach (OneListItem line in list.Items)
            {
                if (line.Image != null)
                    line.Image.StreamURL = GetImageStreamUrl(line.Image);
            }
        }

        private void ContactSetLocation(MemberContact contact)
        {
            foreach (Notification notification in contact.Notifications)
            {
                NotificationSetLocation(notification);
            }

            this.OneListSetLocation(contact.OneLists);

            foreach (PublishedOffer list in contact.PublishedOffers)
            {
                foreach (ImageView iv in list.Images)
                {
                    iv.StreamURL = GetImageStreamUrl(iv);
                }
                foreach (OfferDetails od in list.OfferDetails)
                {
                    if (od.Image != null)
                        od.Image.StreamURL = GetImageStreamUrl(od.Image);
                }
            }
        }

        private void AdvertisementSetLocation(Advertisement advertisement)
        {
            if (advertisement != null && advertisement.ImageView != null)
            {
                advertisement.ImageView.StreamURL = GetImageStreamUrl(advertisement.ImageView);
            }
        }

        #endregion setlocation
    }
}
