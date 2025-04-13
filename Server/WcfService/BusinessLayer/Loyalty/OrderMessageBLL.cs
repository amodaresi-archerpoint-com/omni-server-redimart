﻿using System;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Xml.Xsl;
using System.Drawing;
using System.Web.Script.Serialization;

using LSOmni.Common.Util;
using LSOmni.DataAccess.Interface.Repository.Loyalty;
using LSRetail.Omni.Domain.DataModel.Base;
using LSRetail.Omni.Domain.DataModel.Base.Retail;
using LSRetail.Omni.Domain.DataModel.Loyalty.Members;
using LSRetail.Omni.Domain.DataModel.Loyalty.Orders;

namespace LSOmni.BLL.Loyalty
{
    public class OrderMessageBLL : BaseLoyBLL
    {
        private readonly INotificationRepository iNotificationRepository;
        private readonly IPushNotificationRepository iPushRepository;

        public OrderMessageBLL(BOConfiguration config, string deviceId, int timeoutInSeconds)
            : base(config, deviceId, timeoutInSeconds)
        {
            iNotificationRepository = GetDbRepository<INotificationRepository>(config);
            iPushRepository = GetDbRepository<IPushNotificationRepository>(config);
        }

        public OrderMessageBLL(BOConfiguration config, int timeoutInSeconds)
            : this(config, "", timeoutInSeconds)
        {
        }

        public virtual void OrderMessageSave(string orderId, int status, string subject, string message, Statistics stat)
        {
            // Status: New = 0, InProcess = 1, Failed = 2, Processed = 3,
            CreateNotificationsFromOrderMessage(orderId, status, subject, message, stat);
            SendToEcom("orderstatus", new { document_id = orderId, status = status });
        }

        public virtual void OrderMessageStatusUpdate(OrderMessageStatus orderMessage, Statistics stat)
        {
            // Status: New = 0, InProcess = 1, Failed = 2, Processed = 3,
            CreateNotificationsFromOrderMessage(orderMessage, stat);
            string payloadJson = new JavaScriptSerializer().Serialize(orderMessage);
            SendToEcom("orderstatus", payloadJson);
        }

        public virtual string OrderMessageRequestPayment(string orderId, int status, decimal amount, string token, string authcode, string reference)
        {
            // Status: Unchanged = 0, Changed = 1, Canceled = 2
            string message = string.Empty;
            bool success = OrderMessageRequestPaymentEx(orderId, status, amount, string.Empty, token, authcode, reference, ref message);
            if (success)
                return "OK";
            return message;
        }

        public virtual bool OrderMessageRequestPaymentEx(string orderId, int status, decimal amount, string currencyCode, string token, string authcode, string reference, ref string message)
        {
            // Status: Unchanged = 0, Changed = 1, Canceled = 2
            string json = SendToEcom("orderpayment", new { document_id = orderId, status = status, token = token, amount = amount, currencyCode = currencyCode, authcode = authcode, reference = reference });
            OrderMessageResult result;
            if (json.Length > 0 && (json[0] == '{' || json[0] == '['))
            {
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);

                result = Serialization.Deserialize<OrderMessageResult>(json);
                message = result.message;
                return result.success;
            }

            char[] charsToTrim = { '"' };
            json = json.Trim(charsToTrim);

            message = json;
            return json.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual bool OrderMessagePayment(OrderMessagePayment orderPayment, ref string message)
        {
            // Status: Unchanged = 0, Changed = 1, Canceled = 2
            string payloadJson = new JavaScriptSerializer().Serialize(orderPayment);
            string json = SendToEcom("orderpayment", payloadJson);
            OrderMessageResult result;
            if (json.Length > 0 && (json[0] == '{' || json[0] == '['))
            {
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);

                result = Serialization.Deserialize<OrderMessageResult>(json);
                message = result.message;
                return result.success;
            }

            char[] charsToTrim = { '"' };
            json = json.Trim(charsToTrim);

            message = json;
            return json.Equals("OK", StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual OrderMessageShippingResult OrderMessageShipping(OrderMessageShipping orderShipping)
        {
            // Status: New = 0, InProcess = 1, Failed = 2, Processed = 3,
            string payloadJson = new JavaScriptSerializer().Serialize(orderShipping);
            string json = SendToEcom("ordershipping", payloadJson);

            try
            {
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);
                if (json[0] == '[') // remove [ ] 
                    json = json.Substring(1, json.Length - 2);

                return Serialization.Deserialize<OrderMessageShippingResult>(json);
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                return new OrderMessageShippingResult()
                {
                    success = false,
                    message = ex.Message
                };
            }
        }


        #region private

        private string NotificationHtmlFromOrderMessageXml(string message, MemberContact contact)
        {
            #region xml
            /*
             *this here http://www.w3schools.com/xsl/tryxslt.asp?xmlfile=cdcatalog&xsltfile=cdcatalog
        <OmniMessage>
            <OrderStatus>Ready</OrderStatus>
            <QRC>
                <CustomerOrder>
                    <DocStatus>1</DocStatus>
                    <DocID>CO00000287</DocID>
                </CustomerOrder>
            </QRC>
            <Order>
                <DocID>CO00000287</DocID>
                <DocDate>2014-10-30T16:05:52.54Z</DocDate>
                <MemberCardNo>10008</MemberCardNo>
                <StoreToCollect>S0009</StoreToCollect>
                <CollectTimeLimit>2014-11-08T17:00:00Z</CollectTimeLimit>
                <OrderLinesToCollect>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                    <Line>
                        <ItemDescription>Briefcase, Leather</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM></UOM>
                        <Quantity>1</Quantity>
                        <Amount>800</Amount>
                    </Line>
                </OrderLinesToCollect>
                <EstimatedTotalAmount>3200</EstimatedTotalAmount>
            </Order>
            <Disclaimer>TEST</Disclaimer>
        </OmniMessage>

             <OmniMessage>
            <OrderStatus>Canceled</OrderStatus>
            <QRC>
                <CustomerOrder>
                    <DocStatus>1</DocStatus>
                    <DocID>CO000008</DocID>
                </CustomerOrder>
            </QRC>
            <Order>
                <DocID>CO000008</DocID>
                <DocDate>2014-11-20T14:29:24.54Z</DocDate>
                <MemberCardNo>10021</MemberCardNo>
                <StoreToCollect>S0001</StoreToCollect>
                <CollectTimeLimit></CollectTimeLimit>
                <OrderLinesOutOfStock>
                    <Line>
                        <ItemDescription>Wireless Mouse</ItemDescription>
                        <VariantCode></VariantCode>
                        <UOM>PCS</UOM>
                        <Quantity>1</Quantity>
                        <Amount>8</Amount>
                    </Line>
                </OrderLinesOutOfStock>
                <EstimatedTotalAmount>0</EstimatedTotalAmount>
            </Order>
            <Disclaimer>TEST</Disclaimer>
        </OmniMessage> 
        */
            #endregion xml

            try
            {
                XDocument doc = XDocument.Parse(message);
                //cleanup data and add to it
                string status = doc.Element("OmniMessage").Element("OrderStatus").Value;
                if (status.ToUpper().StartsWith("REA"))
                    status = "Ready"; //Ready Canceled
                else if (status.ToUpper().StartsWith("CANC"))
                    status = "Canceled";
                doc.Element("OmniMessage").Element("Order").Add(new XElement("Status", status));
                doc.Element("OmniMessage").Element("Order").Add(new XElement("FirstName", contact.FirstName));
                doc.Element("OmniMessage").Element("Order").Add(new XElement("LastName", contact.LastName));

                //
                //string storeNo = doc.Element("OmniMessage").Element("Order").Element("StoreToCollect").Value;
                //StoreBLL storeBLL = new StoreBLL(timeoutInSeconds);
                //Store store = storeBLL.StoreGetById(storeNo);
                //doc.Element("OmniMessage").Element("Order").Add(new XElement("StoreName", store.Description));
                //string addr = store.Address.Address1 + ", " + store.Address.City;
                //doc.Element("OmniMessage").Element("Order").Add(new XElement("StoreAddress", addr));
                string timeLimit = doc.Element("OmniMessage").Element("Order").Element("CollectTimeLimit").Value;

                if (string.IsNullOrWhiteSpace(timeLimit) == false)
                {
                    DateTime collectTime = Convert.ToDateTime(timeLimit);
                    //friendly datetime
                    doc.Element("OmniMessage").Element("Order").Element("CollectTimeLimit").Value = collectTime.ToString("f");
                }

                string xslFile = string.Format(@"{0}\xsl\notification.xsl", AppDomain.CurrentDomain.BaseDirectory);
                string xslInput = string.Empty;
                xslInput = File.ReadAllText(xslFile);

                string xmlInput = doc.Element("OmniMessage").Element("Order").ToString(); //<Order>..
                xslInput = WebUtility.HtmlDecode(xslInput);
                string detailsAsHtml = String.Empty;

                // xslInput is a string that contains xsl
                using (StringReader srt = new StringReader(xslInput))
                // xmlInput is a string that contains xml
                using (StringReader sri = new StringReader(xmlInput))
                {
                    using (XmlReader xrt = XmlReader.Create(srt))
                    using (XmlReader xri = XmlReader.Create(sri))
                    {
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(xrt);
                        using (StringWriter sw = new StringWriter())
                        using (XmlWriter xwo = XmlWriter.Create(sw, xslt.OutputSettings)) // use OutputSettings of xsl, so it can be output as HTML
                        {
                            xslt.Transform(xri, xwo);
                            detailsAsHtml = sw.ToString();
                            detailsAsHtml = detailsAsHtml.Replace("<html>", "").Replace("</html>", "").Replace("\r", "").Replace("\n", "").Replace("<br />", "\n");
                            //TODO must be an easier way !
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                            detailsAsHtml = detailsAsHtml.Replace("<?xml version=\"1.0\"?>", "");
                        }
                    }
                }
                return detailsAsHtml;
            }
            catch (Exception ex)
            {
                logger.Error(config.LSKey.Key, ex);
                throw;
            }
        }

        private void CreateNotificationsFromOrderMessage(string orderId, int orderStatus, string subject, string message, Statistics stat)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;     // nothing to save here

            string cardId = string.Empty;
            string notificationId = GuidHelper.NewGuidString();

            try
            {
                ContactBLL contactBLL = new ContactBLL(config, timeoutInSeconds);
                XDocument doc = XDocument.Parse(message);
                XElement elCardNo = doc.Element("OmniMessage")?.Element("Order")?.Element("MemberCardNo");
                if (elCardNo == null)
                    throw new XmlException("MemberCardNo. node not found in message.Details XML");

                cardId = elCardNo.Value;
                if (string.IsNullOrEmpty(cardId))
                    return;

                MemberContact contact = contactBLL.ContactGet(ContactSearchType.CardId, cardId, stat);
                if (contact == null)
                {
                    logger.Error(config.LSKey.Key, "Failed to find contact for cardId:{0}", cardId);
                    return;
                }

                //parse XML from nav for the Notifications
                string detailsAsHtml = NotificationHtmlFromOrderMessageXml(message, contact);
                string qrText = string.Empty;
                //if status is ready 
                if (orderStatus == 1 || orderStatus == 3)
                {
                    qrText = doc.Element("OmniMessage").Element("QRC").Element("CustomerOrder").ToString();
                    qrText = qrText.Replace("\r", "").Replace("\n", "");
                }

                iNotificationRepository.OrderMessageNotificationSave(notificationId, orderId, cardId, subject, detailsAsHtml, qrText, stat);
                iPushRepository.SavePushNotification(contact.Id, notificationId, stat);

                //if status is ready 
                if (orderStatus == 1 || orderStatus == 3)
                {
                    //create qr image 
                    ImageView iv = new ImageView(notificationId)
                    {
                        DisplayOrder = 0,
                        LocationType = LocationType.Image,
                        ImgBytes = GenerateQRCode(qrText)
                    };

                    IImageRepository imgRepository = base.GetDbRepository<IImageRepository>(config);
                    imgRepository.SaveImageLink(iv, "Member Notification", "Member Notification: " + notificationId, notificationId, iv.Id, 0);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(config.LSKey.Key, ex, "OrderMessageSend failed for GUID:{0} CardId:{1}", notificationId, cardId);
            }
        }

        private void CreateNotificationsFromOrderMessage(OrderMessageStatus orderMsg, Statistics stat)
        {
            if (orderMsg == null || string.IsNullOrEmpty(orderMsg.MsgDetail))
                return;     // nothing to save here

            string notificationId = GuidHelper.NewGuidString();

            try
            {
                if (string.IsNullOrEmpty(orderMsg.CardId))
                    return;

                ContactBLL contactBLL = new ContactBLL(config, timeoutInSeconds);
                MemberContact contact = contactBLL.ContactGet(ContactSearchType.CardId, orderMsg.CardId, stat);
                if (contact == null)
                {
                    logger.Error(config.LSKey.Key, "Failed to find contact for cardId:{0}", orderMsg.CardId);
                    return;
                }

                iNotificationRepository.OrderMessageNotificationSave(notificationId, orderMsg.OrderId, orderMsg.CardId, orderMsg.MsgSubject, orderMsg.MsgDetail, orderMsg.OrderId, stat);
                iPushRepository.SavePushNotification(contact.Id, notificationId, stat);

                //if status is ready 
                if (orderMsg.HeaderStatus.StartsWith("REA"))
                {
                    //create qr image 
                    ImageView iv = new ImageView(notificationId)
                    {
                        DisplayOrder = 0,
                        LocationType = LocationType.Image,
                        ImgBytes = GenerateQRCode(orderMsg.OrderId)
                    };

                    IImageRepository imgRepository = base.GetDbRepository<IImageRepository>(config);
                    imgRepository.SaveImageLink(iv, "Member Notification", "Member Notification: " + notificationId, notificationId, iv.Id, 0);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(config.LSKey.Key, ex, "OrderMessageSend failed for GUID:{0} CardId:{1}", notificationId, orderMsg.CardId);
            }
        }

        #endregion private

        #region qrCode generation

        private byte[] GenerateQRCode(string qrCode)
        {
            int height = 500; //500 was 58 KB
            int width = 500;
            ZXing.BarcodeWriter writer = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.Q,
                    Height = height,
                    Width = width,
                    Margin = 0,
                }
            };

            Bitmap bitmap = writer.Write(qrCode);
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                return imageBytes;
            }
        }

        #endregion
    }
}
