using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public partial class CN_99BillPaymentGatewayInvoker
    {
        private void PostCNP(MyHLShoppingCart shoppingcart, CreditPayment_V01 payment, SessionInfo sessionInfo, string disId, string name)
        {
            if (shoppingcart == null || payment == null)
                return;

            var tr3Url = _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved");
            var merchantId = _configHelper.GetConfigEntry("CNPTerminalId");
            var terminalId = _configHelper.GetConfigEntry("terminalId");
            try
            {

                var phoneNumber = DistributorOrderingProfileProvider.GetPhoneNumberForCN(disId).Trim();
                var amount = _orderAmount <= 0 ? "0.00" : _orderAmount.ToString("0.00");
                var tins = DistributorOrderingProfileProvider.GetTinList(disId, true);
                var tin = tins.Find(t => t.ID == "CNID");
                //var phoneNum = string.Empty;
                if (string.IsNullOrEmpty(phoneNumber) && shoppingcart.DeliveryInfo != null &&
                    shoppingcart.DeliveryInfo.Address != null)
                {
                    phoneNumber = shoppingcart.DeliveryInfo.Address.Phone;
                }

                var sbXml = new StringBuilder();
                sbXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\">");
                sbXml.Append("<version>1.0</version><TxnMsgContent><txnType>PUR</txnType><interactiveStatus>TR1</interactiveStatus>");
                sbXml.AppendFormat("<cardNo>{0}</cardNo>", payment.Card.AccountNumber);
                sbXml.AppendFormat("<expiredDate>{0}</expiredDate>", payment.Card.Expiration.ToString("MM") + payment.Card.Expiration.ToString("yy"));
                sbXml.AppendFormat("<cvv2>{0}</cvv2>", payment.Card.CVV);
                sbXml.AppendFormat("<amount>{0}</amount>", amount);
                sbXml.AppendFormat("<merchantId>{0}</merchantId>", merchantId.Trim());
                sbXml.AppendFormat("<terminalId>{0}</terminalId>", terminalId.Trim());
                sbXml.AppendFormat("<cardHolderName>{0}</cardHolderName>", name);
                sbXml.AppendFormat("<cardHolderId>{0}</cardHolderId>", tin == null ? string.Empty : tin.IDType.Key.Trim());
                sbXml.Append("<idType>0</idType>");
                sbXml.AppendFormat("<entryTime>{0}</entryTime>", DateTime.Now.ToString("yyyyMMddHHmmss"));
                sbXml.AppendFormat("<externalRefNumber>{0}</externalRefNumber>", _orderNumber);
                sbXml.AppendFormat("<extMap><extDate><key>phone</key><value>{0}</value></extDate></extMap>", phoneNumber);
                sbXml.AppendFormat("<tr3Url>{0}</tr3Url>", tr3Url.Trim());
                sbXml.AppendFormat("<bankId>{0}</bankId>", payment.AuthorizationMerchantAccount);
                sbXml.AppendFormat("</TxnMsgContent></MasMessage>");

                var decyptedCardNum = CryptographicProvider.Decrypt(payment.Card.AccountNumber);
                var decyptedCVV = CryptographicProvider.Decrypt(payment.Card.CVV);

                var securedOrderData = sbXml.ToString().Replace(payment.Card.AccountNumber, PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa));

                if (!string.IsNullOrEmpty(decyptedCVV))
                    securedOrderData = securedOrderData.Replace(payment.Card.CVV, "123");

                LogMessageWithInfo(PaymentGatewayLogEntryType.Request, _orderNumber, _distributorId, _gatewayName,
                           PaymentGatewayRecordStatusType.Unknown, securedOrderData);

                bool isLockedeach = true;
                bool isLocked = true;
                string lockfailed = string.Empty;
                if (shoppingcart.pcLearningPointOffSet > 0M && !(shoppingcart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
                {
                    isLockedeach = OrderProvider.LockPCLearningPoint(_distributorId, _orderNumber,
                                                      new OrderMonth(shoppingcart.CountryCode).OrderMonthShortString,
                                                    Convert.ToInt32(Math.Truncate(shoppingcart.pcLearningPointOffSet)),
                                                      HLConfigManager.Platform);
                    if (!isLockedeach)
                    {
                        lockfailed = "PC Learning Point";
                        isLocked = false;
                    }
                }
                else if (shoppingcart.pcLearningPointOffSet > 0M)
                {
                    isLockedeach = OrderProvider.LockETOLearningPoint(
                            shoppingcart.CartItems.Select(s => s.SKU),
                            _distributorId,
                            _orderNumber,
                            new OrderMonth(shoppingcart.CountryCode).OrderMonthShortString,
                            Convert.ToInt32(Math.Truncate(shoppingcart.pcLearningPointOffSet)),
                            HLConfigManager.Platform);

                    if (!isLockedeach)
                    {
                        lockfailed = "ETO Learning Point";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenSrPromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRPromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Promotion";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenSrPromotionGrowing)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Growing";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenSrPromotionExcelnt)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Excellent";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenBadgePromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockBadgePromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", Badge promo";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenNewSrpromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", NewSrPromotion";
                        isLocked = false;
                    }
                }

                if (shoppingcart.HasBrochurePromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockBrochurePromotion(shoppingcart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", Brochure Promotion";
                        isLocked = false;
                    }
                }
                if (isLocked)
                {
                    var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                    var request = new ServiceProvider.OrderChinaSvc.GetCNPPaymentServiceRequest_V01()
                    {
                        Data = sbXml.ToString().Replace(payment.Card.AccountNumber, decyptedCardNum).Replace(payment.Card.CVV, decyptedCVV)
                    };
                    var response = proxy.GetCnpPaymentServiceDetail(new ServiceProvider.OrderChinaSvc.GetCnpPaymentServiceDetailRequest(request)).GetCnpPaymentServiceDetailResult as ServiceProvider.OrderChinaSvc.GetCNPPaymentServiceResponse_V01;

                    if (null != response)
                    {
                        if (response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success && response.Response.Length > 0)
                        {
                            var msgReturn = response.Response;
                            if (msgReturn.IndexOf("xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"") > 1)
                            {
                                msgReturn = msgReturn.Replace(" xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"", "");
                            }
                            var xmlDoc = new XmlDocument();
                            var encodedString = Encoding.UTF8.GetBytes(msgReturn);
                            var ms = new MemoryStream(encodedString);
                            ms.Flush();
                            ms.Position = 0;
                            // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                            xmlDoc.Load(ms);
                            var list = xmlDoc.SelectNodes("//TxnMsgContent");
                            var externalRefNumberback = string.Empty;
                            var refNumberback = string.Empty;
                            var gatewayAmount = string.Empty;
                            var approved = false;
                            string responseCode = "";
                            if (list != null && list.Count == 0)
                            {
                                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorMessage");
                                if (selectSingleNode != null)
                                {
                                    var errorMessage = selectSingleNode.InnerText;
                                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, msgReturn + errorMessage);
                                }

                            }
                            else
                            {

                                var authorizationCodeback = "";
                                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/responseCode");
                                if (selectSingleNode != null)
                                {
                                    responseCode = selectSingleNode.InnerText;
                                    approved = responseCode == "00";
                                }

                                var singleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/externalRefNumber");
                                externalRefNumberback = singleNode != null ? singleNode.InnerText : string.Empty;
                                var refNumber = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/refNumber");
                                refNumberback = refNumber != null ? refNumber.InnerText : string.Empty;
                                var authorizationCode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/authorizationCode");
                                authorizationCodeback = authorizationCode != null
                                                            ? authorizationCode.InnerText
                                                            : string.Empty;
                                var retAmount = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/amount");
                                gatewayAmount = retAmount != null ? retAmount.InnerText : string.Empty;

                                if (approved)
                                {
                                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Approved, msgReturn);
                                }
                                else
                                {
                                    var strCNPUnknown = Settings.GetRequiredAppSetting("CNPResponseCodeForUnknown", "C0,68");
                                    var cnpResponseCodeForUnknown = new List<string>(strCNPUnknown.Split(new char[] { ',' }));
                                    if (cnpResponseCodeForUnknown.Contains(responseCode.ToUpper()))
                                    {
                                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Unknown, msgReturn);
                                    }
                                    else
                                    {
                                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, msgReturn);
                                    }
                                }

                                sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;
                            }
                            payment.Card.IssuingBankID = refNumberback;
                            payment.AuthorizationMerchantAccount = externalRefNumberback;
                            var signMsgVal = string.Format("{0},{1},{2},{3},{4},{5}", _orderNumber, approved ? "1" : "0", DateTime.Now.ToString(DateTimeFormat), refNumberback, externalRefNumberback, gatewayAmount);
                            var verStr = Encrypt(signMsgVal, EncryptionKey);
                            var url = string.Format("{0}?VerStr={1}", _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"), verStr);

                            HttpContext.Current.Response.Redirect(url, false);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            var resp =
                                string.Format("CNP - PostCNP response fails. OrderNumber: {0} ; response: {1}; status: {2}",
                                    _orderNumber, response.Response, response.Status);
                            //LoggerHelper.Error(resp);
                            LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                        }
                    }
                    else
                    {
                        var resp = "CNP - PostCNP response fails. OrderNumber:" + _orderNumber;
                        //LoggerHelper.Error(resp);
                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                    }
                }
                else
                {
                    var resp = "CNP - " + lockfailed.TrimStart(',') + " locking fails. OrderNumber:" + _orderNumber;
                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                }
            }
            catch (Exception ex)
            {
                var resp = string.Format("CNP - PostCNP exception. OrderNumber: {0}. ex : {1} ", _orderNumber, ex.Message);
                LoggerHelper.Error(resp);
                LogMessage(PaymentGatewayLogEntryType.Response, _orderNumber, _distributorId, _gatewayName, PaymentGatewayRecordStatusType.Declined, resp);
            }
        }

        public static string EncryptionKey = "12345678";
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        public static string Decrypt(string stringToDecrypt, string sEncryptionKey)
        {
            Byte[] inputByteArray = new Byte[stringToDecrypt.Length];
            try
            {
                key = System.Text.Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(stringToDecrypt.Replace(' ', '+'));
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = Encoding.UTF8;

                return encoding.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                            string.Format("Decrypt ex : {0} ", ex.Message));
                return ex.Message;
            }
        }
        private static byte[] key = { };
        public static string Encrypt(string stringToEncrypt, string SEncryptionKey)
        {
            try
            {
                key = System.Text.Encoding.UTF8.GetBytes(EncryptionKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                            string.Format("Encrypt ex : {0} ", ex.Message));
                return ex.Message;
            }
        }


        private static CN_99BillPaymentGatewayResponse CNPQueryResponse(string response, string orderNumber)
        {
            if (response.IndexOf("xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"") > 1)
            {
                response = response.Replace(" xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"", "");
            }
            var xmlDoc = new XmlDocument();
            var encodedString = Encoding.UTF8.GetBytes(response);
            var ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;
            // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
            xmlDoc.Load(ms);
            var list = xmlDoc.SelectNodes("//TxnMsgContent");

            if (list != null && list.Count == 0)
            {
                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorMessage");
                if (selectSingleNode != null)
                {
                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, string.Empty, "CN_99BillPaymentGateway",
                        PaymentGatewayRecordStatusType.Declined, response + selectSingleNode.InnerText);
                }

            }
            else
            {
                var approved = false;
                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/responseCode");
                if (selectSingleNode != null)
                {
                    var responseCode = selectSingleNode.InnerText;
                    approved = responseCode == "00";
                    if (!approved)
                    {
                        var strCNPUnknown = Settings.GetRequiredAppSetting("CNPResponseCodeForUnknown", "C0,68");
                        var cnpResponseCodeForUnknown = new List<string>(strCNPUnknown.Split(new char[] { ',' }));
                        if (cnpResponseCodeForUnknown.Contains(responseCode.ToUpper()))
                        {
                            return null;
                        }
                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, string.Empty,
                                   "CN_99BillPaymentGateway",
                                   PaymentGatewayRecordStatusType.Declined, response + selectSingleNode.InnerText);
                        return null;
                    }
                }
                if (!approved) return null;
                var distributorId = HttpContext.Current.User.Identity.Name;
                var currentSession = SessionInfo.GetSessionInfo(distributorId, HLConfigManager.Configurations.Locale);
                currentSession.OrderQueryStatus99BillInprocess = true;
                var singleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/externalRefNumber");
                var externalRefNumberback = singleNode != null ? singleNode.InnerText : string.Empty;
                var refNumber = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/refNumber");
                var refNumberback = refNumber != null ? refNumber.InnerText : string.Empty;
                LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, string.Empty, "CN_99BillPaymentGateway",
                    PaymentGatewayRecordStatusType.Approved, response);

                var paymentGatewayResponse = new CN_99BillPaymentGatewayResponse
                {
                    IsApproved = true,
                    OrderNumber = orderNumber,
                    CanSubmitIfApproved = true,
                    ReloadShoppingCart = true,
                    SpecialResponse = string.Format("{0},{1}", externalRefNumberback, refNumberback)
                };
                return paymentGatewayResponse;
            }
            return null;
        }

        public static bool CNPQueryRespnse(string response, string orderNumber)
        {
            if (response.IndexOf("xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"") > 1)
            {
                response = response.Replace(" xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"", "");
            }
            var xmlDoc = new XmlDocument();
            var encodedString = Encoding.UTF8.GetBytes(response);
            var ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;
            // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
            xmlDoc.Load(ms);
            var list = xmlDoc.SelectNodes("//TxnMsgContent");

            if (list != null && list.Count == 0)
            {
                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorMessage");
                if (selectSingleNode != null)
                {
                    LoggerHelper.Info(string.Format(PaymentGatewayLogEntryType.Response.ToString(), orderNumber,
                                                         string.Empty, "CN_99BillPaymentGateway",
                                                         PaymentGatewayRecordStatusType.Declined,
                                                         response + selectSingleNode.InnerText));
                }

            }
            else
            {
                var approved = false;
                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/responseCode");
                if (selectSingleNode != null)
                {
                    var responseCode = selectSingleNode.InnerText;
                    approved = responseCode == "00";
                }

                return approved;
            }
            return false;
        }

        public static bool PostCNPForMobile(MyHLShoppingCart shoppingcart, CreditPayment_V01 payment, string disId, string name, decimal amoun, string orderNumber, string distributorId, string phone)
        {
            if (shoppingcart == null || payment == null)
                return false;

            ConfigHelper configHelper = new ConfigHelper("CN_99BillPaymentGateway");

            var tr3Url = configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved");
            var merchantId = configHelper.GetConfigEntry("CNPTerminalId");
            var terminalId = configHelper.GetConfigEntry("terminalId");
            try
            {
                var amount = amoun <= 0 ? "0.00" : amoun.ToString("0.00");
                var tins = DistributorOrderingProfileProvider.GetTinList(disId, true);
                var tin = tins.Find(t => t.ID == "CNID");


                var sbXml = new StringBuilder();
                sbXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\">");
                sbXml.Append("<version>1.0</version><TxnMsgContent><txnType>PUR</txnType><interactiveStatus>TR1</interactiveStatus>");
                sbXml.AppendFormat("<cardNo>{0}</cardNo>", payment.Card.AccountNumber);
                sbXml.AppendFormat("<expiredDate>{0}</expiredDate>", payment.Card.Expiration.ToString("MM") + payment.Card.Expiration.ToString("yy"));
                sbXml.AppendFormat("<cvv2>{0}</cvv2>", payment.Card.CVV);
                sbXml.AppendFormat("<amount>{0}</amount>", amount);
                sbXml.AppendFormat("<merchantId>{0}</merchantId>", merchantId.Trim());
                sbXml.AppendFormat("<terminalId>{0}</terminalId>", terminalId.Trim());
                sbXml.AppendFormat("<cardHolderName>{0}</cardHolderName>", name);
                sbXml.AppendFormat("<cardHolderId>{0}</cardHolderId>", tin == null ? string.Empty : tin.IDType.Key.Trim());
                sbXml.Append("<idType>0</idType>");
                sbXml.AppendFormat("<entryTime>{0}</entryTime>", DateTime.Now.ToString("yyyyMMddHHmmss"));
                sbXml.AppendFormat("<externalRefNumber>{0}</externalRefNumber>", orderNumber);
                sbXml.AppendFormat("<extMap><extDate><key>phone</key><value>{0}</value></extDate></extMap>", phone);
                sbXml.AppendFormat("<tr3Url>{0}</tr3Url>", tr3Url.Trim());
                sbXml.AppendFormat("<bankId>{0}</bankId>", payment.AuthorizationMerchantAccount);
                sbXml.AppendFormat("</TxnMsgContent></MasMessage>");
                var encyptedCardNum = CryptographicProvider.Encrypt(payment.Card.AccountNumber);
                var encryptedCvv = CryptographicProvider.Encrypt(payment.Card.CVV);

                var decyptedCardNum = CryptographicProvider.Decrypt(encyptedCardNum);
                var decryptedCvv = CryptographicProvider.Decrypt(encryptedCvv);

                var logData =
                    sbXml.ToString()
                        .Replace(payment.Card.AccountNumber, encyptedCardNum)
                        .Replace(payment.Card.CVV, encryptedCvv);

                LogMessageWithInfo(PaymentGatewayLogEntryType.Request, orderNumber, orderNumber, orderNumber,
                           PaymentGatewayRecordStatusType.Unknown, logData);

                bool isLockedeach = true;
                bool isLocked = true;
                string lockfailed = string.Empty;

                if (shoppingcart.pcLearningPointOffSet > 0M && !(shoppingcart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
                {
                    isLockedeach = OrderProvider.LockPCLearningPoint(distributorId, orderNumber,
                                                      new OrderMonth(shoppingcart.CountryCode).OrderMonthShortString,
                                                    Convert.ToInt32(Math.Truncate(shoppingcart.pcLearningPointOffSet)),
                                                      HLConfigManager.Platform);
                    if (!isLockedeach)
                    {
                        lockfailed = "PC Learning Point";
                        isLocked = false;
                    }
                }
                else if (shoppingcart.pcLearningPointOffSet > 0M)
                {
                   isLockedeach = OrderProvider.LockETOLearningPoint(
                            shoppingcart.CartItems.Select(s => s.SKU),
                            distributorId,
                            orderNumber,
                            new OrderMonth(shoppingcart.CountryCode).OrderMonthShortString,
                            Convert.ToInt32(Math.Truncate(shoppingcart.pcLearningPointOffSet)),
                            HLConfigManager.Platform);

                    if (!isLockedeach)
                    {
                        lockfailed = "ETO Learning Point";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenSrPromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRPromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Promotion";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenSrPromotionGrowing)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Growing";
                        isLocked = false;
                    }

                }
                if (shoppingcart.HastakenSrPromotionExcelnt)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Excellent";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenBadgePromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockBadgePromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", Badge promo";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HastakenNewSrpromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", NewSrPromotion";
                        isLocked = false;
                    }
                }
                if (shoppingcart.HasBrochurePromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockBrochurePromotion(shoppingcart, orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", Brochure Promotion";
                        isLocked = false;
                    }
                }
                if (isLocked)
                {
                    var proxy = ServiceClientProvider.GetChinaOrderServiceProxy();
                    var request = new ServiceProvider.OrderChinaSvc.GetCNPPaymentServiceRequest_V01()
                    {
                        Data = sbXml.ToString().Replace(payment.Card.AccountNumber, payment.Card.AccountNumber)
                    };
                    var response = proxy.GetCnpPaymentServiceDetail(new ServiceProvider.OrderChinaSvc.GetCnpPaymentServiceDetailRequest(request)).GetCnpPaymentServiceDetailResult as ServiceProvider.OrderChinaSvc.GetCNPPaymentServiceResponse_V01;

                    if (null != response)
                    {
                        if (response.Status == ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success && response.Response.Length > 0)
                        {
                            var msgReturn = response.Response;
                            if (msgReturn.IndexOf("xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"") > 1)
                            {
                                msgReturn = msgReturn.Replace(" xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\"", "");
                            }
                            var xmlDoc = new XmlDocument();
                            var encodedString = Encoding.UTF8.GetBytes(msgReturn);
                            var ms = new MemoryStream(encodedString);
                            ms.Flush();
                            ms.Position = 0;
                            // Build the XmlDocument from the MemorySteam of UTF-8 encoded bytes
                            xmlDoc.Load(ms);
                            var list = xmlDoc.SelectNodes("//TxnMsgContent");
                            var externalRefNumberback = string.Empty;
                            var refNumberback = string.Empty;
                            var gatewayAmount = string.Empty;
                            var approved = false;
                            if (list != null && list.Count == 0)
                            {
                                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorMessage");
                                if (selectSingleNode != null)
                                {
                                    var errorMessage = selectSingleNode.InnerText;
                                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker", PaymentGatewayRecordStatusType.Declined, msgReturn + errorMessage);

                                    return false;

                                }

                            }
                            else
                            {

                                var authorizationCodeback = "";
                                var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/responseCode");
                                if (selectSingleNode != null)
                                {
                                    var responseCode = selectSingleNode.InnerText;
                                    approved = responseCode == "00";
                                    if (!approved)
                                    {
                                        var strCNPUnknown = Settings.GetRequiredAppSetting("CNPResponseCodeForUnknown", "C0,68");
                                        var cnpResponseCodeForUnknown = new List<string>(strCNPUnknown.Split(new char[] { ',' }));
                                        if (cnpResponseCodeForUnknown.Contains(responseCode.ToUpper()))
                                        {
                                            return approved;
                                        }
                                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, string.Empty,
                                                   "CN_99BillPaymentGateway",
                                                   PaymentGatewayRecordStatusType.Declined, msgReturn);
                                        return approved;
                                    }
                                }

                                var singleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/externalRefNumber");
                                externalRefNumberback = singleNode != null ? singleNode.InnerText : string.Empty;
                                var refNumber = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/refNumber");
                                refNumberback = refNumber != null ? refNumber.InnerText : string.Empty;
                                var authorizationCode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/authorizationCode");
                                authorizationCodeback = authorizationCode != null
                                                            ? authorizationCode.InnerText
                                                            : string.Empty;
                                var retAmount = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/amount");
                                gatewayAmount = retAmount != null ? retAmount.InnerText : string.Empty;
                                LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker",
                                           PaymentGatewayRecordStatusType.Approved, msgReturn);
                                //sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;

                            }
                            payment.Card.IssuingBankID = refNumberback;
                            payment.AuthorizationMerchantAccount = externalRefNumberback;

                            return approved;

                        }
                        else
                        {
                            var resp =
                                string.Format(
                                    "Response failure. Unable to connect to 99Bill. OrderNumber: {0} ; response: {1}; status: {2}",
                                    orderNumber, response.Response, response.Status);
                            //LoggerHelper.Error(resp);
                            LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker", PaymentGatewayRecordStatusType.Declined, resp);
                            return false;
                        }
                    }
                    else
                    {
                        var resp = "Response null, Unable to connect to 99Bill. OrderNumber:" + orderNumber;
                        //LoggerHelper.Error(resp);
                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker", PaymentGatewayRecordStatusType.Declined, resp);
                        return false;
                    }
                }
                else
                {
                    var resp = "PostCNP - " + lockfailed.TrimStart(',') + " locking fails. OrderNumber:" + orderNumber;
                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker", PaymentGatewayRecordStatusType.Declined, resp);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var resp = string.Format("PostCNP Error. OrderNumber: {0}. ex : {1} ", orderNumber, ex.Message);
                LoggerHelper.Error(resp);
                LogMessage(PaymentGatewayLogEntryType.Response, orderNumber, distributorId, "CN_99BillPaymentGatewayInvoker", PaymentGatewayRecordStatusType.Declined, resp);
                return false;
            }
        }
    }
}
