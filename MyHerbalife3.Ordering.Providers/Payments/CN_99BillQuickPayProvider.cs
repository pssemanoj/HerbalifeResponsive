using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using PaymentGatewayLogEntryType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayLogEntryType;
using PaymentGatewayRecordStatusType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayRecordStatusType;
using IssuerAssociationType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.IssuerAssociationType;
using PaymentInformation = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentInformation;
using System.Linq;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CN_99BillQuickPayProvider
    {
        protected ConfigHelper _configHelper;
        protected string _distributorId;
        protected string _loginDistributorId;
        protected string _locale;
        protected string _country;
        private const string GatewayName = "CN_99BillPaymentGateway";
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        private MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.IChinaInterface _chinaOrderProxy;
        private IPaymentInfoProviderLoader _paymentInfoProviderLoader;
        private IOrderProviderLoader _orderProviderLoader;
        private IDistributorOrderingProfileProviderLoader _distributorOrderingProfileProviderLoader;
        private ICatalogProviderLoader _catalogProfileProviderLoader;
        private IEmailHelperLoader _emailHelperLoader;
        private IShoppingCartProviderLoader _shoppingCartProviderLoader;
        private IMembershipLoader _membershipLoader;

        private const string LogEntry = "<PaymentGatewayRecord RecordType=\"{0}\" OrderNumber=\"{1}\" Distributor=\"{2}\" GatewayName=\"{3}\"><Time>{4}</Time><Status>{5}</Status><Data><![CDATA[{6}]]></Data></PaymentGatewayRecord>";

        public string StorablePAN { get; set; }

        public string Token { get; set; }

        public string LastErrorMessage { get; set; }

        public bool IsUnknownPaymentStatus { get; set; }

        public CN_99BillQuickPayProvider()
        {
            _configHelper = new ConfigHelper(GatewayName);
            _locale = HLConfigManager.Configurations.Locale;
            _country = HLConfigManager.Configurations.Locale.Substring(3);

            _distributorId = HttpContext.Current.User.Identity.Name;
            _loginDistributorId = HttpContext.Current.User.Identity.Name;
            var currentSession = SessionInfo.GetSessionInfo(_loginDistributorId, _locale);
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && currentSession.IsReplacedPcOrder && currentSession.ReplacedPcDistributorOrderingProfile != null)
            {
                _distributorId = currentSession.ReplacedPcDistributorOrderingProfile.Id;
            }
        }

        public CN_99BillQuickPayProvider(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.IChinaInterface chinaOrderProxy,
                                      IPaymentInfoProviderLoader paymentInfoProviderLoader = null,
                                      IDistributorOrderingProfileProviderLoader distributorOrderingProfileProviderLoader = null,
                                      ICatalogProviderLoader catalogProviderLoader = null,
                                      IEmailHelperLoader emailHelperLoader = null,
                                      IOrderProviderLoader orderProviderLoader = null,
                                      IShoppingCartProviderLoader shoppingCartProviderLoader = null,
                                      IMembershipLoader membershipLoader = null)
        {
            _chinaOrderProxy = chinaOrderProxy;
            _paymentInfoProviderLoader = paymentInfoProviderLoader;
            _distributorOrderingProfileProviderLoader = distributorOrderingProfileProviderLoader;
            _catalogProfileProviderLoader = catalogProviderLoader;
            _emailHelperLoader = emailHelperLoader;
            _orderProviderLoader = orderProviderLoader;
            _shoppingCartProviderLoader = shoppingCartProviderLoader;
            _membershipLoader = membershipLoader;

            _configHelper = new ConfigHelper(GatewayName);
            _locale = HLConfigManager.Configurations.Locale;
            _country = HLConfigManager.Configurations.Locale.Substring(3);

            _distributorId = HttpContext.Current.User.Identity.Name;
            _loginDistributorId = HttpContext.Current.User.Identity.Name;
            var currentSession = SessionInfo.GetSessionInfo(_loginDistributorId, _locale);
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && currentSession.IsReplacedPcOrder && currentSession.ReplacedPcDistributorOrderingProfile != null)
            {
                _distributorId = currentSession.ReplacedPcDistributorOrderingProfile.Id;
            }
        }

        public bool CheckBindedCard(string bankId, string cardType, string phoneNumber, string distributorid = null)
        {
            bool result = false;
            LastErrorMessage = "";

            LoadStorableData(bankId, cardType, phoneNumber, distributorid);

            if (string.IsNullOrEmpty(StorablePAN))
                return result;

            var merchantId = _configHelper.GetConfigEntry("QPMerchantId");

            _loginDistributorId = !string.IsNullOrEmpty(_loginDistributorId) ? _loginDistributorId : distributorid;

            try
            {
                var storablePAN = CryptographicProvider.Encrypt(StorablePAN);
                var sbXml = new StringBuilder();
                sbXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\">");
                sbXml.Append("<version>1.0</version><PciQueryContent>");
                sbXml.AppendFormat("<merchantId>{0}</merchantId>", merchantId.Trim());
                sbXml.AppendFormat("<customerId>{0}</customerId>", _loginDistributorId.Trim());
                sbXml.AppendFormat("<storablePan>{0}</storablePan>", storablePAN);
                sbXml.AppendFormat("<bankId>{0}</bankId>", bankId);
                sbXml.AppendFormat("<cardType>{0}</cardType>", cardType);
                sbXml.AppendFormat("</PciQueryContent></MasMessage>");

                var decyptedCardNum = CryptographicProvider.Decrypt(storablePAN);

                if (_chinaOrderProxy == null)
                    _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                var request = new GetCnpPaymentServiceRequest_V02()
                {
                    Data = sbXml.ToString().Replace(storablePAN, decyptedCardNum),
                    RequestUrlKey = "QPPCIQueryUrl",
                };
                var response = _chinaOrderProxy.GetCnpPaymentServiceDetail(new GetCnpPaymentServiceDetailRequest(request)).GetCnpPaymentServiceDetailResult as GetCNPPaymentServiceResponse_V01;

                if (null != response)
                {
                    if (response.Status == ServiceResponseStatusType.Success && response.Response.Length > 0)
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
                        var list = xmlDoc.SelectNodes("//PciQueryContent");
                        if (list != null && list.Count == 0)
                        {
                            var errorCode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorCode");

                            if (errorCode != null && !string.IsNullOrEmpty(errorCode.InnerText))
                            {
                                LastErrorMessage = GetResponseCodeMessage(errorCode.InnerText.Trim());
                            }

                            if (string.IsNullOrEmpty(LastErrorMessage))
                            {
                                LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                            }
                        }
                        else
                        {
                            var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/PciQueryContent/responseCode");
                            if (selectSingleNode != null)
                            {
                                var responseCode = selectSingleNode.InnerText;
                                result = responseCode == "00";

                                if (!result && !string.IsNullOrEmpty(responseCode))
                                {
                                    LastErrorMessage = GetResponseCodeMessage(responseCode.Trim());
                                }
                            }

                            if (result)
                            {
                                result = false; //reset

                                var cardInfo = xmlDoc.SelectSingleNode("MasMessage/PciQueryContent/pciInfos");

                                if (cardInfo != null && !string.IsNullOrEmpty(cardInfo.InnerText))
                                {
                                    var storablePan = xmlDoc.SelectSingleNode("MasMessage/PciQueryContent/pciInfos/pciInfo/storablePan");

                                    if (storablePan != null)
                                    {
                                        StorablePAN = storablePan.InnerText;
                                        result = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var resp =
                            string.Format("Quick Pay - CheckBindedCard response fails. Distributor ID: {0} ; response: {1}; status: {2}",
                                _distributorId, response.Response, response.Status);
                        LoggerHelper.Error(resp);
                        LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                    }
                }
                else
                {
                    var resp = "Quick Pay - CheckBindedCard response fails. Distributor ID:" + _distributorId;
                    LoggerHelper.Error(resp);
                    LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                }
            }
            catch (Exception ex)
            {
                var resp = string.Format("Quick Pay - CheckBindedCard exception. Distributor ID: {0}. ex : {1} ", _distributorId, ex.Message);
                LoggerHelper.Error(resp);
                LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
            }

            return result;
        }

        //public List<PaymentInformation> LoadStorableDataQuick(string bankId, string cardType, string phoneNumber, string distributorid = null)
        public List<PaymentInformation> LoadStorableDataQuick(string phoneNumber, string distributorid = null)
        {

            if (_distributorOrderingProfileProviderLoader == null)
                _distributorOrderingProfileProviderLoader = new DistributorOrderingProfileProviderLoader();

            // var phoneNumber = _distributorOrderingProfileProviderLoader.GetPhoneNumberForCN(distributorid).Trim();

            List<PaymentInformation> result = null;

            if (_paymentInfoProviderLoader == null)
                _paymentInfoProviderLoader = new PaymentInfoProviderLoader();
            _loginDistributorId = !string.IsNullOrEmpty(_loginDistributorId) ? _loginDistributorId : distributorid;
            var paymentInfo = _paymentInfoProviderLoader.GetPaymentInfoForQuickPay(_loginDistributorId, _locale);

            if (paymentInfo != null && paymentInfo.Count > 0)
            {

                //result = paymentInfo.Where(t => t.CardType.Trim() == (cardType == "0001" ? "QC" : "QD") && t.Alias == bankId &&  t.BillingAddress.Line3 == phoneNumber).ToList();
                result = paymentInfo.Where(t => t.BillingAddress != null && t.BillingAddress.Line3 != null && t.BillingAddress.Line3.Trim() == phoneNumber).ToList();
            }

            return result;
        }

        public bool RequestMobilePinForPurchase(string orderNumber, ServiceProvider.OrderSvc.Order_V01 order, MyHLShoppingCart shoppingCart, string distributorid = null)
        {
            _distributorId = string.IsNullOrEmpty(_distributorId) && !string.IsNullOrEmpty(distributorid) ? distributorid : _distributorId;
            bool result = false;
            LastErrorMessage = "";

            if (order == null || order.Payments == null || order.Payments.Count == 0)
                return result;

            var payment = order.Payments[0] as ServiceProvider.OrderSvc.CreditPayment_V01;

            if (payment == null || payment.Card == null)
                return result;

            var merchantId = _configHelper.GetConfigEntry("QPMerchantId");
            var quickPayCard = payment.Card as ServiceProvider.OrderSvc.QuickPayPayment;
            var decyptedCardNum = CryptographicProvider.Decrypt(payment.Card.AccountNumber);

            string decyptedCVV = "";

            if (!string.IsNullOrEmpty(payment.Card.CVV))
                decyptedCVV = CryptographicProvider.Decrypt(payment.Card.CVV);

            if (quickPayCard == null)
                return result;

            _loginDistributorId = !string.IsNullOrEmpty(_loginDistributorId) ? _loginDistributorId : distributorid;

            try
            {
                var sbXml = new StringBuilder();
                sbXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\">");
                sbXml.Append("<version>1.0</version><GetDynNumContent>");
                sbXml.AppendFormat("<merchantId>{0}</merchantId>", merchantId.Trim());
                sbXml.AppendFormat("<customerId>{0}</customerId>", _loginDistributorId.Trim());
                sbXml.AppendFormat("<externalRefNumber>{0}</externalRefNumber>", orderNumber);

                if (decyptedCardNum.Length == 16 || decyptedCardNum.Length == 19)
                {
                    sbXml.AppendFormat("<cardHolderName>{0}</cardHolderName>", payment.Card.NameOnCard);
                    sbXml.AppendFormat("<pan>{0}</pan>", payment.Card.AccountNumber);
                    if (!quickPayCard.IsDebitCard)
                    {
                        sbXml.AppendFormat("<cvv2>{0}</cvv2>", payment.Card.CVV);
                        sbXml.AppendFormat("<expiredDate>{0}</expiredDate>", payment.Card.Expiration.ToString("MMyy"));
                    }
                    sbXml.AppendFormat("<cardHolderId>{0}</cardHolderId>", quickPayCard.CardHolderId);
                    sbXml.AppendFormat("<idType>{0}</idType>", quickPayCard.CardHolderType);
                    sbXml.AppendFormat("<phoneNO>{0}</phoneNO>", quickPayCard.MobilePhoneNumber);
                }
                else
                {
                    sbXml.AppendFormat("<storablePan>{0}</storablePan>", quickPayCard.AccountNumber);
                }

                sbXml.AppendFormat("<bankId>{0}</bankId>", payment.Card.IssuingBankID);
                sbXml.AppendFormat("<amount>{0}</amount>", payment.Amount.ToString("0.00"));
                sbXml.AppendFormat("</GetDynNumContent></MasMessage>");

                if (_orderProviderLoader == null)
                    _orderProviderLoader = new OrderProviderLoader(_distributorOrderingProfileProviderLoader, _catalogProfileProviderLoader, _emailHelperLoader);

                var theOrder = _orderProviderLoader.CreateOrder(order, shoppingCart, _country);

                string creditCardNumber = "";
                string cvv = "";

                creditCardNumber = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber;
                cvv = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV;

                ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = "123";
                ((order.Payments[0]) as ServiceProvider.OrderSvc.CreditPayment_V01).Card.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                ((order.Payments[0]) as ServiceProvider.OrderSvc.CreditPayment_V01).Card.CVV = "123";

                var orderData = _orderProviderLoader.SerializeOrder(theOrder, order, shoppingCart, new Guid());

                var isPaymentGatewayUpdated = InsertPaymentGatewayRecord(orderNumber, _distributorId, GatewayName, orderData, HLConfigManager.Configurations.Locale);

                ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = creditCardNumber;
                ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = cvv;
                ((order.Payments[0]) as ServiceProvider.OrderSvc.CreditPayment_V01).Card.AccountNumber = creditCardNumber;
                ((order.Payments[0]) as ServiceProvider.OrderSvc.CreditPayment_V01).Card.CVV = cvv;

                if (!isPaymentGatewayUpdated)
                {
                    return false;
                }

                var securedOrderData = sbXml.ToString().Replace(payment.Card.AccountNumber, PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa));

                if (!string.IsNullOrEmpty(decyptedCVV))
                    securedOrderData = securedOrderData.Replace(payment.Card.CVV, "123");

                LogMessageWithInfo(PaymentGatewayLogEntryType.Request, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Unknown, securedOrderData);

                if (_chinaOrderProxy == null)
                    _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                var request = new GetCnpPaymentServiceRequest_V02()
                {
                    Data = sbXml.ToString().Replace(payment.Card.AccountNumber, decyptedCardNum),
                    RequestUrlKey = "QPGetDynNumUrl",
                };

                if (!string.IsNullOrEmpty(decyptedCVV))
                    request.Data = request.Data.Replace(payment.Card.CVV, decyptedCVV);

                var response = _chinaOrderProxy.GetCnpPaymentServiceDetail(new GetCnpPaymentServiceDetailRequest(request)).GetCnpPaymentServiceDetailResult as GetCNPPaymentServiceResponse_V01;

                if (null != response)
                {
                    if (response.Status == ServiceResponseStatusType.Success && response.Response.Length > 0)
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
                        var list = xmlDoc.SelectNodes("//GetDynNumContent");
                        if (list != null && list.Count == 0)
                        {
                            var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorMessage");
                            if (selectSingleNode != null)
                            {
                                var errorCode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorCode");

                                if (errorCode != null && !string.IsNullOrEmpty(errorCode.InnerText))
                                {
                                    LastErrorMessage = GetResponseCodeMessage(errorCode.InnerText.Trim());
                                }

                                if (string.IsNullOrEmpty(LastErrorMessage))
                                {
                                    LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                                }

                                return result;
                            }
                        }
                        else
                        {
                            var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/GetDynNumContent/responseCode");
                            if (selectSingleNode != null)
                            {
                                var responseCode = selectSingleNode.InnerText;
                                result = responseCode == "00";

                                if (!result && !string.IsNullOrEmpty(responseCode))
                                {
                                    LastErrorMessage = GetResponseCodeMessage(responseCode.Trim());
                                }
                            }

                            if (result)
                            {
                                var storablePan = xmlDoc.SelectSingleNode("MasMessage/GetDynNumContent/storablePan");
                                var token = xmlDoc.SelectSingleNode("MasMessage/GetDynNumContent/token");

                                if (storablePan != null)
                                    StorablePAN = storablePan.InnerText;

                                if (token != null)
                                    Token = token.InnerText;

                                result = !string.IsNullOrEmpty(StorablePAN);
                            }
                        }

                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Unknown, msgReturn);
                    }
                    else
                    {
                        var resp =
                         string.Format("Quick Pay - RequestMobilePinForPurchase response fails. Distributor ID: {0} ; response: {1}; status: {2}",
                             _distributorId, response.Response, response.Status);
                        LoggerHelper.Error(resp);
                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);

                        LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                    }
                }
                else
                {
                    var resp = "Quick Pay - RequestMobilePinForPurchase response fails. Distributor ID:" + _distributorId;
                    LoggerHelper.Error(resp);
                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                    LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                }
            }
            catch (Exception ex)
            {
                var resp = string.Format("Quick Pay - RequestMobilePinForPurchase exception. Distributor ID: {0}. ex : {1} ", _distributorId, ex.Message);
                LoggerHelper.Error(resp);
                LogMessage(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>If the submission sucessful, a valid redirect url will be returned. Otherwise, empty string will be returned.</returns>
        public string Submit(string orderNumber, decimal orderAmount, string distributorid = null, ServiceProvider.OrderSvc.CreditPayment_V01 payments = null)
        {
            string result = "";
            LastErrorMessage = "";

            _loginDistributorId = string.IsNullOrEmpty(_loginDistributorId) && !string.IsNullOrEmpty(distributorid) ? distributorid : _loginDistributorId;

            var sessionInfo = SessionInfo.GetSessionInfo(_loginDistributorId, _locale);
            sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;

            if (_shoppingCartProviderLoader == null)
                _shoppingCartProviderLoader = new ShoppingCartProviderLoader();

            var shoppingcart = sessionInfo.ShoppingCart ?? _shoppingCartProviderLoader.GetShoppingCart(_loginDistributorId, _locale);

            var payment = HttpContext.Current.Session != null ? HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as ServiceProvider.OrderSvc.CreditPayment_V01 : payments != null ? payments : null;

            if (shoppingcart == null || payment == null)
            {
                result = string.Format("{0},{1},{2},{3},{4},{5}", orderNumber, "0", DateTime.Now.ToString(DateTimeFormat), "", "", "");
                LastErrorMessage = "Invalid Order Data";
                return result;
            }

            var tr3Url = _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved");
            var merchantId = _configHelper.GetConfigEntry("QPMerchantId");
            var terminalId = _configHelper.GetConfigEntry("QPterminalId");

            try
            {
                if (_distributorOrderingProfileProviderLoader == null)
                    _distributorOrderingProfileProviderLoader = new DistributorOrderingProfileProviderLoader();

                var phoneNumber = _distributorOrderingProfileProviderLoader.GetPhoneNumberForCN(_loginDistributorId).Trim();
                var amount = orderAmount <= 0 ? "0.00" : orderAmount.ToString("0.00");
                var tins = _distributorOrderingProfileProviderLoader.GetTinList(_loginDistributorId, true);
                var tin = tins.Find(t => t.ID == "CNID");
                string decyptedCVV = "";

                var decyptedCardNum = CryptographicProvider.Decrypt(payment.Card.AccountNumber);

                if (!string.IsNullOrEmpty(payment.Card.CVV))
                    decyptedCVV = CryptographicProvider.Decrypt(payment.Card.CVV);

                var sbXml = new StringBuilder();
                sbXml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><MasMessage xmlns=\"http://www.99bill.com/mas_cnp_merchant_interface\">");
                sbXml.Append("<version>1.0</version><TxnMsgContent><txnType>PUR</txnType><interactiveStatus>TR1</interactiveStatus>");
                sbXml.AppendFormat("<merchantId>{0}</merchantId>", merchantId.Trim());
                sbXml.AppendFormat("<terminalId>{0}</terminalId>", terminalId.Trim());
                sbXml.AppendFormat("<tr3Url>{0}</tr3Url>", tr3Url.Trim());
                sbXml.AppendFormat("<entryTime>{0}</entryTime>", DateTime.Now.ToString("yyyyMMddHHmmss"));

                var quickPayCard = payment.Card as ServiceProvider.OrderSvc.QuickPayPayment;

                if (quickPayCard != null && (decyptedCardNum.Length == 16 || decyptedCardNum.Length == 19))
                {
                    if (!quickPayCard.IsDebitCard)
                    {
                        sbXml.AppendFormat("<expiredDate>{0}</expiredDate>", quickPayCard.Expiration.ToString("MMyy"));
                        sbXml.AppendFormat("<cvv2>{0}</cvv2>", quickPayCard.CVV);
                    }
                    sbXml.AppendFormat("<cardNo>{0}</cardNo>", quickPayCard.AccountNumber);
                    sbXml.AppendFormat("<cardHolderName>{0}</cardHolderName>", payment.Card.NameOnCard);
                    sbXml.AppendFormat("<cardHolderId>{0}</cardHolderId>", tin == null ? string.Empty : tin.IDType.Key.Trim());
                    sbXml.Append("<idType>0</idType>");
                }
                else
                {
                    sbXml.AppendFormat("<storableCardNo>{0}</storableCardNo>", payment.Card.AccountNumber);
                    quickPayCard.BindCard = true;
                }

                sbXml.AppendFormat("<amount>{0}</amount>", amount);
                sbXml.AppendFormat("<externalRefNumber>{0}</externalRefNumber>", orderNumber);
                sbXml.AppendFormat("<customerId>{0}</customerId>", _loginDistributorId);
                sbXml.Append("<spFlag>QuickPay</spFlag>");

                if (quickPayCard != null && (decyptedCardNum.Length == 16 || decyptedCardNum.Length == 19))
                {
                    sbXml.AppendFormat("<extMap><extDate><key>phone</key><value>{0}</value></extDate>", phoneNumber);
                }
                else
                {
                    sbXml.AppendFormat("<extMap><extDate><key>phone</key><value>{0}</value></extDate>", "");
                }

                if (quickPayCard != null)
                    sbXml.AppendFormat("<extDate><key>validCode</key><value>{0}</value></extDate>", quickPayCard.MobilePin);

                sbXml.AppendFormat("<extDate><key>savePciFlag</key><value>{0}</value></extDate>", quickPayCard.BindCard ? "1" : "0");

                if (quickPayCard != null && !string.IsNullOrEmpty(quickPayCard.Token))
                    sbXml.AppendFormat("<extDate><key>token</key><value>{0}</value></extDate>", quickPayCard.Token);

                if (quickPayCard != null && (decyptedCardNum.Length == 16 || decyptedCardNum.Length == 19))
                {
                    sbXml.AppendFormat("<extDate><key>payBatch</key><value>{0}</value></extDate>", "1");
                }
                else
                {
                    sbXml.AppendFormat("<extDate><key>payBatch</key><value>{0}</value></extDate>", "2");
                }
                sbXml.AppendFormat("</extMap></TxnMsgContent></MasMessage>");

                var securedOrderData = sbXml.ToString().Replace(payment.Card.AccountNumber, PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa));

                if (!string.IsNullOrEmpty(decyptedCVV))
                    securedOrderData = securedOrderData.Replace(payment.Card.CVV, "123");

                LogMessageWithInfo(PaymentGatewayLogEntryType.Request, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Unknown, securedOrderData);

                if (_chinaOrderProxy == null)
                    _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                var request = new GetCnpPaymentServiceRequest_V02()
                {
                    Data = sbXml.ToString().Replace(payment.Card.AccountNumber, decyptedCardNum),
                    RequestUrlKey = "QPPurchaseUrl"
                };

                if (!string.IsNullOrEmpty(decyptedCVV))
                {
                    request.Data = request.Data.Replace(payment.Card.CVV, decyptedCVV);
                }

                var response = _chinaOrderProxy.GetCnpPaymentServiceDetail(new GetCnpPaymentServiceDetailRequest(request)).GetCnpPaymentServiceDetailResult as GetCNPPaymentServiceResponse_V01;

                if (null != response)
                {
                    if (response.Status == ServiceResponseStatusType.Success && response.Response.Length > 0)
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
                        var refNumberback = string.Empty;
                        var gatewayAmount = string.Empty;
                        var approved = false;
                        string responseCode = "";
                        if (list != null && list.Count == 0)
                        {
                            var errorCode = xmlDoc.SelectSingleNode("MasMessage/ErrorMsgContent/errorCode");

                            if (errorCode != null && !string.IsNullOrEmpty(errorCode.InnerText))
                            {
                                LastErrorMessage = GetResponseCodeMessage(errorCode.InnerText.Trim());
                            }

                            if (string.IsNullOrEmpty(LastErrorMessage))
                            {
                                LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                            }
                        }
                        else
                        {
                            var selectSingleNode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/responseCode");
                            if (selectSingleNode != null)
                            {
                                responseCode = selectSingleNode.InnerText;
                                approved = responseCode == "00";
                            }

                            var storablePAN = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/storableCardNo");

                            if (approved && storablePAN != null && !string.IsNullOrEmpty(storablePAN.InnerText) && quickPayCard.BindCard)
                            {
                                SaveStorableData(quickPayCard.IssuingBankID, quickPayCard.IsDebitCard ? "0002" : "0001", storablePAN.InnerText, phoneNumber, payment.Card.NameOnCard);
                            }

                            var refNumber = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/refNumber");
                            refNumberback = refNumber != null ? refNumber.InnerText : string.Empty;
                            var authorizationCode = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/authorizationCode");

                            var retAmount = xmlDoc.SelectSingleNode("MasMessage/TxnMsgContent/amount");
                            gatewayAmount = retAmount != null ? retAmount.InnerText : string.Empty;

                            if (approved)
                            {
                                LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Approved, msgReturn);
                            }
                            else
                            {
                                var strCNPUnknown = Settings.GetRequiredAppSetting("CNPResponseCodeForUnknown", "C0,68");
                                var cnpResponseCodeForUnknown = new List<string>(strCNPUnknown.Split(new char[] { ',' }));
                                if (cnpResponseCodeForUnknown.Contains(responseCode.ToUpper()))
                                {
                                    IsUnknownPaymentStatus = true;
                                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Unknown, msgReturn);
                                }
                                else
                                {
                                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, msgReturn);
                                    if (!string.IsNullOrEmpty(responseCode))
                                    {
                                        LastErrorMessage = GetResponseCodeMessage(responseCode.Trim());
                                    }
                                }
                            }

                            sessionInfo.OrderStatus = SubmitOrderStatus.Unknown;
                            payment.Card.IssuingBankID = refNumberback;
                            payment.AuthorizationMerchantAccount = quickPayCard.IssuingBankID;
                        }
                        var signMsgVal = string.Format("{0},{1},{2},{3},{4},{5}", orderNumber, approved ? "1" : "0", DateTime.Now.ToString(DateTimeFormat), refNumberback, quickPayCard.IssuingBankID, gatewayAmount);
                        result = signMsgVal;
                    }
                    else
                    {
                        var resp =
                            string.Format("Quick Pay - Submit response fails. OrderNumber: {0} ; response: {1}; status: {2}",
                                orderNumber, response.Response, response.Status);
                        LoggerHelper.Error(resp);
                        LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                        LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                    }
                }
                else
                {
                    var resp = "Quick Pay - Submit response fails. OrderNumber:" + orderNumber;
                    LoggerHelper.Error(resp);
                    LogMessageWithInfo(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                    LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
                }
            }
            catch (Exception ex)
            {
                var resp = string.Format("Quick Pay - Submit exception. OrderNumber: {0}. ex : {1} ", orderNumber, ex.Message);
                LoggerHelper.Error(resp);
                LogMessage(PaymentGatewayLogEntryType.Response, orderNumber, _distributorId, GatewayName, PaymentGatewayRecordStatusType.Declined, resp);
                LastErrorMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "QuickPayServiceError");
            }

            if (string.IsNullOrEmpty(result))
                result = string.Format("{0},{1},{2},{3},{4},{5}", orderNumber, "0", DateTime.Now.ToString(DateTimeFormat), "", "", "");

            return result;
        }

        private PaymentInformation LoadStorableData(string bankId, string cardType, string phoneNumber, string distributorid = null)
        {
            PaymentInformation result = null;

            if (_paymentInfoProviderLoader == null)
                _paymentInfoProviderLoader = new PaymentInfoProviderLoader();
            _loginDistributorId = !string.IsNullOrEmpty(_loginDistributorId) ? _loginDistributorId : distributorid;
            var paymentInfo = _paymentInfoProviderLoader.GetPaymentInfoForQuickPay(_loginDistributorId, _locale);

            if (paymentInfo != null)
            {
                foreach (var itm in paymentInfo)
                {
                    if (itm.Alias == bankId && !string.IsNullOrEmpty(itm.CardType) && itm.CardType.Trim() == (cardType == "0001" ? "QC" : "QD") && (itm.BillingAddress != null && (itm.BillingAddress.Line3 ?? "").Trim() == phoneNumber))
                    {
                        StorablePAN = itm.CardNumber;
                        result = itm;
                        break;
                    }
                }
            }

            return result;
        }

        public void SaveStorableData(string bankId, string cardType, string storablePAN, string phoneNumber, string cardHolderName)
        {
            PaymentInformation paymentInfo = new PaymentInformation();
            var result = LoadStorableData(bankId, cardType, phoneNumber);

            if (result != null)
            {
                if (((result.Alias ?? "").Trim() == bankId) && ((result.CardType ?? "").Trim() == (cardType == "0001" ? "QC" : "QD")) && ((result.CardNumber ?? "").Trim() == storablePAN))
                {
                    return;
                }
            }

            paymentInfo.CardHolder = new ServiceProvider.OrderSvc.Name_V01();
            paymentInfo.CardHolder.First = cardHolderName;
            paymentInfo.CardHolder.Last = "";
            paymentInfo.CardHolder.Middle = "";

            paymentInfo.BillingAddress = new ServiceProvider.OrderSvc.Address_V01();
            paymentInfo.BillingAddress.Line3 = phoneNumber;

            paymentInfo.CardNumber = storablePAN;
            paymentInfo.CardType = cardType == "0001" ? "QC" : "QD";
            paymentInfo.Created = DateTime.Now;
            paymentInfo.Alias = bankId;
            paymentInfo.Expiration = DateTime.Now.AddYears(5); //Purposely use this dummy datetime. As it is not required to keep for this purpose.

            if (_paymentInfoProviderLoader == null)
                _paymentInfoProviderLoader = new PaymentInfoProviderLoader();

            _paymentInfoProviderLoader.SavePaymentInfo(_loginDistributorId, _locale, paymentInfo);

            StorablePAN = storablePAN;
        }

        private bool InsertPaymentGatewayRecord(string orderNumber, string distributorId, string gatewayName, string orderData, string locale)
        {
            if (_orderProviderLoader == null)
                _orderProviderLoader = new OrderProviderLoader();

            int recordId = _orderProviderLoader.InsertPaymentGatewayRecord(orderNumber, distributorId, gatewayName, orderData, locale);

            if (recordId > 0)
                return true;
            else
                return false;
        }

        public void LogMessage(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status, string data)
        {
            LoggerHelper.Error(string.Format(LogEntry, entryType.ToString(), orderNumber, distributorId, paymentGatewayName, XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local), status, data));

            if (_orderProviderLoader == null)
                _orderProviderLoader = new OrderProviderLoader();

            _orderProviderLoader.UpdatePaymentGatewayRecord(orderNumber, data, entryType, status);
        }

        public void LogMessageWithInfo(PaymentGatewayLogEntryType entryType, string orderNumber, string distributorId, string paymentGatewayName, PaymentGatewayRecordStatusType status, string data)
        {
            LoggerHelper.Info(string.Format(LogEntry, entryType.ToString(), orderNumber, distributorId, paymentGatewayName, XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local), status, data));

            if (_orderProviderLoader == null)
                _orderProviderLoader = new OrderProviderLoader();

            _orderProviderLoader.UpdatePaymentGatewayRecord(orderNumber, data, entryType, status);
        }

        private string GetResponseCodeMessage(string messageCode)
        {
            switch (messageCode)
            {
                case "00":
                    return "交易成功";
                case "07":
                    return "特定条件下没收卡";
                case "34":
                    return "有作弊嫌疑";
                case "35":
                    return "请联系快钱公司";
                case "36":
                    return "受限制的卡";
                case "37":
                    return "风险卡，请联系快钱公司";
                case "41":
                    return "挂失卡";
                case "43":
                    return "被窃卡";
                case "67":
                    return "强行受理（要求在自动柜员机上没收此卡）";
                case "01":
                    return "请联系发卡行，或核对卡信息后重新输入";
                case "02":
                    return "请联系快钱公司";
                case "03":
                    return "无效商户";
                case "04":
                    return "无效终端";
                case "05":
                    return "不予承兑";
                case "12":
                    return "无效交易";
                case "13":
                    return "无效金额，交易金额不在许可的范围内，疑问请联系快钱公司。";
                case "14":
                    return "无效卡号";
                case "17":
                    return "客户取消";
                case "18":
                    return "商户保证金对应的可交易额度不足，请联系快钱公司。";
                case "20":
                    return "卡信息校验错";
                case "21":
                    return "不做任何处理";
                case "22":
                    return "故障怀疑";
                case "23":
                    return "不可接受的交易费";
                case "25":
                    return "找不到原始交易";
                case "30":
                    return "格式错误";
                case "31":
                    return "不支持该发卡银行";
                case "32":
                    return "商户不受理的卡";
                case "33":
                    return "过期的卡";
                case "38":
                    return "超过允许的试输入";
                case "39":
                    return "无贷记账户";
                case "40":
                    return "请求的功能尚不支持";
                case "42":
                    return "无此账户";
                case "44":
                    return "无此投资账户";
                case "51":
                    return "资金不足";
                case "52":
                    return "无此支票账户";
                case "55":
                    return "密码错误，此卡需要输入正确的密码";
                case "57":
                    return "不允许持卡人进行的交易";
                case "58":
                    return "不允许终端进行的交易";
                case "59":
                    return "有作弊嫌疑";
                case "60":
                    return "请联系快钱公司";
                case "61":
                    return "超出取款转账金额限制";
                case "62":
                    return "受限制的卡,拒绝交易";
                case "64":
                    return "原始金额错误";
                case "65":
                    return "超出取款次数限制";
                case "66":
                    return "请联系快钱公司";
                case "75":
                    return "允许输入密码次数超限";
                case "76":
                    return "无效账户";
                case "93":
                    return "交易违法、不能完成";
                case "06":
                    return "出错";
                case "09":
                    return "请求正在处理中";
                case "15":
                    return "无此发卡方 / 拒绝";
                case "19":
                    return "当前的有效退货额度不足，请结账后进行再退货或联系快钱公司进行线下退货";
                case "56":
                    return "无此卡记录";
                case "63":
                    return "侵犯安全";
                case "68":
                    return "无法在正常时间内获得交易应答，请稍后重试";
                case "90":
                    return "正在日终处理（系统终止一天的活动，开始第二天的活动，交易在几分钟后可再次发送）";
                case "91":
                    return "发卡方或交换中心不能操作";
                case "92":
                    return "网络暂时无法达到，请稍后重试";
                case "94":
                    return "重复交易";
                case "95":
                    return "核对差错";
                case "96":
                    return "系统异常、失效";
                case "97":
                    return "ATM / POS 终端号找不到";
                case "98":
                    return "交换中心收不到发卡方应答";
                case "99":
                    return "密码格式错";
                case "C0":
                    return "正在处理中";
                case "I0":
                    return "外部交易跟踪编号（如：商户订单号）发生重复";
                case "53":
                    return "无此储蓄卡账户";
                case "54":
                    return "过期的卡";
                case "J0":
                    return "订单不存在";
                case "I1":
                    return "请提供正确的持卡人姓名";
                case "I2":
                    return "请提供正确的验证码（CVV2），验证码在卡背面签名栏后的三位数字串";
                case "I3":
                    return "请提供正确的证件号码，必须与申请信用卡时的证件号码一致";
                case "I4":
                    return "请提供正确的卡有效期，卡有效期是在卡号下面的 4 位数字";
                case "I5":
                    return "超出持卡人设置的交易限额，请持卡人联系发卡银行调高限额";
                case "I6":
                    return "错误的证件类型";
                case "I7":
                    return "CVV2 或有效期错";
                case "I8":
                    return "金额超限或其他";
                case "24":
                    return "通讯校验错";
                case "45":
                    return "流水号错";
                case "Q2":
                    return "有效期错";
                case "I9":
                    return "银行结帐中，请重试交易";
                case "IA":
                    return "请提供正确的手机号";
                case "77":
                    return "请向网络中心签到";
                case "79":
                    return "POS 终端重传脱机数据";
                case "L0":
                    return "查发卡行的特殊条件";
                case "L1":
                    return "刷卡读取数据有误，请重新刷卡";
                case "L2":
                    return "发卡行未找到相关记录";
                case "L3":
                    return "脱机交易对帐不平";
                case "81":
                    return "MAC 校验错";
                case "D6":
                    return "手机号被列入黑名单";
                case "D7":
                    return "银行卡号被列入黑名单";
                case "D8":
                    return "未于服务器建立连接或与服务器的连接已断开";
                case "F1":
                    return "手机号不合法";
                case "FA":
                    return "密码错误 1 次";
                case "FB":
                    return "密码错误 2 次";
                case "FC":
                    return "密码错误 3 次";
                case "FF":
                    return "支付失败，持卡人未接听或拒绝支付";
                case "G0":
                    return "超出单笔金额上限";
                case "G3":
                    return "超出系统当日金额限制";
                case "G4":
                    return "超出系统当日该银行卡成功交易次数限制";
                case "G5":
                    return "超出商户当日金额限制";
                case "G6":
                    return "超出商户当日该银行卡成功交易次数限制";
                case "GB":
                    return "超出商户当月该银行卡成功交易次数限制";
                case "GD":
                    return "超出商户当日该身份证成功交易次数限制";
                case "GF":
                    return "超出商户当月该身份证成功交易次数限制";
                case "GK":
                    return "超出商户当月该手机号成功交易次数限制";
                case "GT":
                    return "超出商户当月该身份证与银行卡支付次数限制";
                case "GU":
                    return "超出商户当月该银行卡与身份证支付次数限制";
                case "GV":
                    return "超出商户当月该银行卡与手机号支付次数限制";
                case "GZ":
                    return "超出商户当月该手机号与银行卡支付次数限制";
                case "A1":
                    return "授权网络状态不正常";
                case "A2":
                    return "授权网络应答错误";
                case "A3":
                    return "授权网络已经被关闭";
                case "A4":
                    return "找不到授权终端";
                case "A5":
                    return "只支持 CNP 交易";
                case "A6":
                    return "不支持的交易类型";
                case "A7":
                    return "找不到原授权代理工作任务";
                case "A8":
                    return "不可撤销的交易（已经原交易已经批结算）";
                case "A9":
                    return "交易已经被冲正撤销";
                case "A0":
                    return "还有撤销交易等待处理";
                case "AA":
                    return "不正确的工作台";
                case "AB":
                    return "工作任务已经拣出";
                case "AC":
                    return "交易必须成功完成";
                case "AD":
                    return "授权终端找不到";
                case "AE":
                    return "授权终端无效";
                case "AF":
                    return "授权终端类型不匹配";
                case "AG":
                    return "授权终端没有登录";
                case "AH":
                    return "交易已经受理并处理成功，无法取消";
                case "AI":
                    return "授权终端找不到";
                case "AJ":
                    return "授权网络拒绝分期交易";
                case "AK":
                    return "授权网络拒绝该分期期数";
                case "AL":
                    return "授权网络找不到原授权交易";
                case "AM":
                    return "系统处于日终状态，拒绝交易";
                case "AN":
                    return "银行已日切，请提交退货处理";
                case "AO":
                    return "银行未结算，请提交撤销处理或系统日切后再次退货";
                case "L4":
                    return "BGW.网络连接失败";
                case "L5":
                    return "BIN.无法识别的主账号";
                case "L6":
                    return "BIN.无法识别的二磁道";
                case "L7":
                    return "BIN.无法识别的三磁道";
                case "L8":
                    return "该卡不被受理或者交易信息不全";
                case "L9":
                    return "BIN.无效卡号";
                case "O1":
                    return "EDC.不允许手工输入卡号";
                case "O2":
                    return "EDC.找不到原交易";
                case "O3":
                    return "EDC.原交易无法撤消";
                case "O4":
                    return "EDC.交易状态不正确";
                case "O5":
                    return "EDC.商户编号不匹配";
                case "O6":
                    return "EDC.终端编号不匹配";
                case "O7":
                    return "EDC.交易金额不匹配";
                case "O8":
                    return "EDC.交易无法冲正";
                case "O9":
                    return "EDC.交易已经冲正";
                case "OA":
                    return "EDC.交易类型不匹配";
                case "OB":
                    return "EDC.不接受的银行卡";
                case "OC":
                    return "EDC.退货金额大于原金额";
                case "OD":
                    return "EDC.预授权过期失效";
                case "OE":
                    return "EDC.确认金额超过预授权限额";
                case "OF":
                    return "EDC.交易积分不匹配";
                case "OG":
                    return "EDC.交易金额超过最大限制";
                case "OH":
                    return "EDC.错误商户类型";
                case "OI":
                    return "EDC.找不到父商户（集团）";
                case "OJ":
                    return "EDC.交易进行被冲正";
                case "OK":
                    return "EDC.交易已经完成";
                case "OL":
                    return "EDC.交易超时";
                case "OM":
                    return "EDC.退货额度已经使用完";
                case "ON":
                    return "EDC.退货日限额已经用完";
                case "OO":
                    return "EDC.退货金额超限";
                case "OP":
                    return "EDC.商户财务控制信息设置不正确";
                case "OQ":
                    return "EDC.销售日限额已经用完";
                case "OR":
                    return "EDC.外部跟踪编号重复";
                case "OS":
                    return "EDC.找不到对应的结算商户";
                case "OT":
                    return "EDC.交易金额太小";
                case "OU":
                    return "EDC.无效商户";
                case "OV":
                    return "EDC.无效的退货交易";
                case "OW":
                    return "EDC.建议优先撤销交易";
                case "OX":
                    return "EDC.查询汇率的参数无效";
                case "OY":
                    return "EDC.无效的撤销交易";
                case "OZ":
                    return "EDC.无效的撤销完成冲正交易";
                case "N2":
                    return "EDC.重复交易";
                case "N3":
                    return "EDC.决策表路由读取失败";
                case "N4":
                    return "分期期数为空";
                case "N5":
                    return "交易金额小于分期起分点";
                case "M1":
                    return "Merchant.错误的商户编号";
                case "M2":
                    return "Merchant.商户状态不匹配";
                case "M3":
                    return "Merchant.错误的终端编号";
                case "M4":
                    return "Merchant.终端被锁定";
                case "M5":
                    return "Merchant.终端状态不匹配";
                case "M6":
                    return "Merchant.终端不处于营业时间段";
                case "M7":
                    return "Merchant.终端不支持的交易类型";
                case "M8":
                    return "商户 DN 为空";
                case "M9":
                    return "无效的商户操作员";
                case "MA":
                    return "不匹配的商户 DN";
                case "MB":
                    return "不正确的商户操作员状态";
                case "MC":
                    return "不正确的商户用户名";
                case "MD":
                    return "不正确的商户操作员密码";
                case "ME":
                    return "重复的商户操作员编号";
                case "MF":
                    return "找不到的手续费合同";
                case "MG":
                    return "重复 DN 编号";
                case "MH":
                    return "重复的商户编号";
                case "MI":
                    return "重复的终端编号";
                case "MJ":
                    return "该商户不支持手动预授权完成";
                case "MK":
                    return "该商户不支持快速退货";
                case "ML":
                    return "该商户不支持撤销授权完成并级联撤销授权操作";
                case "MM":
                    return "商户清算中心配置错误";
                case "MN":
                    return "不支持 IVR 终端错误";
                case "MO":
                    return "错误的邮件地址";
                case "P1":
                    return "POS.商户的状态不正确";
                case "P2":
                    return "POS.商户号不正确";
                case "P3":
                    return "POS.终端的状态不正确";
                case "P4":
                    return "POS.终端号不正确";
                case "P5":
                    return "POS.终端 IP 不正确";
                case "P6":
                    return "POS.原交易不存在（用于撤销和退货） P7 POS.退货速率过快";
                case "P8":
                    return "POS.重复退货";
                case "R1":
                    return "RCS.错误的风险状态";
                case "R2":
                    return "RCS.交易已经结算";
                case "R3":
                    return "RCS.退货交易失败";
                case "R4":
                    return "Risk.交易拒绝";
                case "IJ":
                    return "ISO8583.位图配置找不到";
                case "IB":
                    return "ISO8583.解包错误";
                case "IC":
                    return "ISO8583.打包错误";
                case "ID":
                    return "ISO8583.域检查错误";
                case "IE":
                    return "ISO8583.配置错误";
                case "IF":
                    return "ISO8583.MAC 码计算时错误";
                case "IG":
                    return "ISO8583.MAC 验证错误";
                case "IH":
                    return "ISO8583.MAC 验证错误";
                case "II":
                    return "ISO8583.获取 MACKEY 出错";
                case "C1":
                    return "MAStxnCtrl 对象转换 APStxnCtrl 时出错";
                case "C2":
                    return "APStxnCtrl 对象转换 MAStxnCtrl 时出错";
                case "S0":
                    return "APS 系统异常";
                case "S1":
                    return "交易授权网络不一致";
                case "S2":
                    return "交易授权网络不存在";
                case "S3":
                    return "交易授权网络路由错误";
                case "S4":
                    return "交易对象已超出最大值";
                case "S5":
                    return "证书较验错误";
                case "AP":
                    return "不支持的证件类型/ 证件号";
                case "08":
                    return "请与银行联系";
                case "26":
                    return "重复交易";
                case "28":
                    return "交易无法处理";
                case "29":
                    return "FileUpdateDenied";
                case "78":
                    return "止付卡";
                case "84":
                    return "联网暂断,重做交易";
                case "87":
                    return "PIN 密钥同步错";
                case "88":
                    return "MAC 密钥同步错";
                case "KG":
                    return "卡状态、户口无效或不存在，拒绝交易对照";
                case "B.AUTH.0001":
                    return "授权.授权网络状态不正常";
                case "B.AUTH.0002":
                    return "授权.授权网络应答错误";
                case "B.AUTH.0003":
                    return "授权.授权网络已经被关闭";
                case "B.AUTH.0004":
                    return "授权.找不到授权终端";
                case "B.AUTH.0005":
                    return "授权.只支持 CNP 交易";
                case "B.AUTH.0006":
                    return "不支持的交易类型";
                case "B.AUTH.0007":
                    return "找不到原授权代理工作任务";
                case "B.AUTH.0008":
                    return "不可撤销的交易（已经原交易已经批结算）";
                case "B.AUTH.0009":
                    return "交易已经被冲正撤销";
                case "B.AUTH.0010":
                    return "还有撤销交易等待处理";
                case "B.AUTH.0011":
                    return "不正确的工作台";
                case "B.AUTH.0012":
                    return "工作任务已经拣出";
                case "B.AUTH.0013":
                    return "交易必须成功完成";
                case "B.AUTH.0014":
                    return "授权终端找不到";
                case "B.AUTH.0015":
                    return "授权终端无效";
                case "B.AUTH.0016":
                    return "授权终端类型不匹配";
                case "B.AUTH.0017":
                    return "授权终端没有登录";
                case "B.AUTH.0018":
                    return "交易已经受理并处理成功，无法取消";
                case "B.AUTH.0019":
                    return "授权终端找不到";
                case "B.AUTH.0020":
                    return "授权网络拒绝分期交易";
                case "B.AUTH.0021":
                    return "授权网络拒绝该分期期数";
                case "B.AUTH.0022":
                    return "授权网络找不到原授权交易";
                case "B.AUTH.0023":
                    return "授权.系统处于日终状态，拒绝交易";
                case "B.AUTH.0024":
                    return "授权.银行已日切，请提交退货处理";
                case "B.AUTH.0025":
                    return "授权.银行未结算，请提交撤销处理或系统日切后再次退货";
                case "B.AUTH.0026":
                    return "不支持的证件类型 / 证件号";
                case "B.BGW.0001":
                    return "BGW.网络连接失败";
                case "B.BIN.0001":
                    return "BIN.无法识别的主账号";
                case "B.BIN.0002":
                    return "BIN.无法识别的二磁道";
                case "B.BIN.0003":
                    return "BIN.无法识别的三磁道";
                case "B.BIN.0004":
                    return "BIN.找不到路由";
                case "B.BIN.0005":
                    return "BIN.无效卡号";
                case "B.EDC.0001":
                    return "EDC.不允许手工输入卡号";
                case "B.EDC.0002":
                    return "EDC.找不到原交易";
                case "B.EDC.0003":
                    return "EDC.原交易无法撤消";
                case "B.EDC.0004":
                    return "EDC.交易状态不正确";
                case "B.EDC.0005":
                    return "EDC.商户编号不匹配";
                case "B.EDC.0006":
                    return "EDC.终端编号不匹配";
                case "B.EDC.0007":
                    return "EDC.交易金额不匹配";
                case "B.EDC.0008":
                    return "EDC.交易无法冲正";
                case "B.EDC.0009":
                    return "EDC.交易已经冲正";
                case "B.EDC.0010":
                    return "EDC.交易类型不匹配";
                case "B.EDC.0011":
                    return "EDC.不接受的银行卡";
                case "B.EDC.0012":
                    return "EDC.退货金额大于原金额";
                case "B.EDC.0013":
                    return "EDC.预授权过期失效";
                case "B.EDC.0014":
                    return "EDC.确认金额超过预授权限额";
                case "B.EDC.0015":
                    return "EDC.交易积分不匹配";
                case "B.EDC.0016":
                    return "EDC.交易金额超过最大限制";
                case "B.EDC.0017":
                    return "EDC.错误商户类型";
                case "B.EDC.0018":
                    return "EDC.找不到父商户（集团）";
                case "B.EDC.0019":
                    return "EDC.交易进行被冲正";
                case "B.EDC.0020":
                    return "EDC.交易已经完成";
                case "B.EDC.0021":
                    return "EDC.交易超时";
                case "B.EDC.0022":
                    return "EDC.退货额度已经使用完";
                case "B.EDC.0023":
                    return "EDC.退货日限额已经用完";
                case "B.EDC.0024":
                    return "EDC.退货金额超限";
                case "B.EDC.0025":
                    return "EDC.商户财务控制信息设置不正确";
                case "B.EDC.0026":
                    return "EDC.销售日限额已经用完";
                case "B.EDC.0027":
                    return "EDC.外部跟踪编号重复";
                case "B.EDC.0028":
                    return "EDC.找不到对应的结算商户";
                case "B.EDC.0029":
                    return "EDC.交易金额太小";
                case "B.EDC.0030":
                    return "EDC.无效商户";
                case "B.EDC.0031":
                    return "EDC.无效的退货交易";
                case "B.EDC.0032":
                    return "EDC.建议优先撤销交易";
                case "B.EDC.0033":
                    return "EDC.查询汇率的参数无效";
                case "B.EDC.0034":
                    return "EDC.无效的撤销交易";
                case "B.EDC.0035":
                    return "EDC.无效的撤销完成冲正交易";
                case "B.EDC.0036":
                    return "EDC.重复交易";
                case "B.EDC.0037":
                    return "EDC.决策表路由读取失败";
                case "B.EDC.0038":
                    return "分期期数为空";
                case "B.EDC.0039":
                    return "交易金额小于分期起分点";
                case "B.MERCHANT.0001":
                    return "Merchant.错误的商户编号";
                case "B.MERCHANT.0002":
                    return "Merchant.商户状态不匹配";
                case "B.MERCHANT.0003":
                    return "Merchant.错误的终端编号";
                case "B.MERCHANT.0004":
                    return "Merchant.终端被锁定";
                case "B.MERCHANT.0005":
                    return "Merchant.终端状态不匹配";
                case "B.MERCHANT.0006":
                    return "Merchant.终端不处于营业时间段";
                case "B.MERCHANT.0007":
                    return "Merchant.终端不支持的交易类型";
                case "B.MERCHANT.0008":
                    return "商户DN 为空";
                case "B.MERCHANT.0009":
                    return "无效的商户操作员";
                case "B.MERCHANT.0010":
                    return "不匹配的商户 DN";
                case "B.MERCHANT.0011":
                    return "不正确的商户操作员状态";
                case "B.MERCHANT.0012":
                    return "不正确的商户用户名";
                case "B.MERCHANT.0013":
                    return "不正确的商户操作员密码";
                case "B.MERCHANT.0014":
                    return "重复的商户操作员编号";
                case "B.MERCHANT.0015":
                    return "找不到的手续费合同";
                case "B.MERCHANT.0016":
                    return "重复 DN 编号";
                case "B.MERCHANT.0017":
                    return "重复的商户编号";
                case "B.MERCHANT.0018":
                    return "重复的终端编号";
                case "B.MERCHANT.0019":
                    return "该商户不支持手动预授权完成";
                case "B.MERCHANT.0020":
                    return "该商户不支持快速退货";
                case "B.MERCHANT.0021":
                    return "该商户不支持撤销授权完成并级联撤销授权操作";
                case "B.MERCHANT.0022":
                    return "商户清算中心配置错误";
                case "B.MERCHANT.0023":
                    return "不支持 IVR 终端错误";
                case "B.MERCHANT.0024":
                    return "错误的邮件地址";
                case "B.POS.0001":
                    return "POS.商户的状态不正确";
                case "B.POS.0002":
                    return "POS.商户号不正确";
                case "B.POS.0003":
                    return "POS.终端的状态不正确";
                case "B.POS.0004":
                    return "POS.终端号不正确";
                case "B.POS.0005":
                    return "POS.终端 IP 不正确";
                case "B.POS.0006":
                    return "POS.原交易不存在（用于撤销和退货）";
                case "B.POS.0007":
                    return "POS.退货速率过快";
                case "B.POS.0008":
                    return "POS.重复退货";
                case "B.RCS.0001":
                    return "RCS.错误的风险状态";
                case "B.RCS.0002":
                    return "RCS.交易已经结算";
                case "B.RCS.0003":
                    return "RCS.退货交易失败";
                case "B.RSK.0001":
                    return "Risk.交易拒绝";
                case "R.ISO8583.0001":
                    return "ISO8583.位图配置找不到";
                case "R.ISO8583.0002":
                    return "ISO8583.解包错误";
                case "R.ISO8583.0003":
                    return "ISO8583.打包错误";
                case "R.ISO8583.0004":
                    return "ISO8583.域检查错误";
                case "R.ISO8583.0005":
                    return "ISO8583.配置错误";
                case "R.ISO8583.0006":
                    return "ISO8583.MAC 码计算时错误";
                case "R.ISO8583.0007":
                    return "ISO8583.MAC 验证错误";
                case "R.ISO8583.0008":
                    return "ISO8583.获取 MACKEY 出错";
                case "A.CONVERT.0001":
                    return "MAS txnCtrl 对象转换 APS txnCtrl 时出错";
                case "A.CONVERT.0002":
                    return "APS txnCtrl 对象转换 MAS txnCtrl 时出错";
                case "APS.SYS.0001":
                    return "APS 系统异常";
                case "APS.AUTH.001":
                    return "交易授权网络不一致";
                case "APS.AUTH.002":
                    return "交易授权网络不存在";
                case "APS.AUTH.003":
                    return "交易授权网络路由错误";
                case "APS.AUTH.004":
                    return "交易对象已超出最大值";
                case "S3.0001":
                    return "证书较验错误";
                case "S3.0002":
                    return "证书较验错误";
                //case "I0": return "外部跟踪编号重复";
                //case "M1": return "储值卡卡库不足";
                //case "M2": return "无法找到车保交易信息";
                //case "M3": return "保险公司与车辆平台金额不匹配";
                //case "M4": return "找不到车辆平台商户";
                //case "M5": return "车辆平台交易号重复";
                //case "M6": return "预授权完成与预授权持卡人姓名不符";
                //case "M7": return "预授权完成与预授权金额不符";
                //case "25": return "找不到原始交易";
                //case "00": return "成功";
                //case "96": return "系统异常";
                //case "51": return "资金不足";
                //case "55": return "密码错误";
                case "F9":
                    return "未知交易";
                case "FD":
                    return "交易信息不存在";
                //case "I9": return "银行结帐中，请重试交易";
                case "J2":
                    return "无效订单";
                case "J4":
                    return "订单已支付";
                case "T0":
                    return "查询条数超限";
                case "T1":
                    return "找不到交易历史";
                case "T2":
                    return "查询时间为空";
                case "T3":
                    return "不支持的银行卡";
                case "T4":
                    return "找不到银行映射信息";
                case "T5":
                    return "反接银行授权商户不能为空";
                case "T6":
                    return "密码加密错误";
                case "LA":
                    return "未配置二级商户号";
                case "0000":
                    return "未知错误";
                case "10001":
                    return "接口版本不正确";
                case "10002":
                    return "字符集不正确";
                case "10003":
                    return "签名类型不正确";
                case "10004":
                    return "商户会员编号格式不正确";
                case "10005":
                    return "服务代码不正确";
                case "10006":
                    return "业务参数不正确";
                case "10007":
                    return "申请编号格式不正确";
                case "10008":
                    return "申请时间格式不正确";
                case "10009":
                    return "银行代码不正确";
                case "10010":
                    return "银行账户名称格式不正确";
                case "10011":
                    return "银行账号格式不正确";
                case "10012":
                    return "身份证件类型格式不正确";
                case "10013":
                    return "身份证件号码格式不正确";
                case "10014":
                    return "扩展参数一不正确";
                case "10015":
                    return "扩展参数二不正确";
                case "10016":
                    return "签名字符串格式不正确";
                case "10017":
                    return "手机号码格式不正确";
                case "11001":
                    return "申请时间与快钱服务器时间相差超过 15 分钟";
                case "11002":
                    return "签名字符串不匹配";
                case "20001":
                    return "商户未开通此项服务";
                case "20002":
                    return "该银行账户号码交易超过限制次数";
                case "21001":
                    return "身份证件号码及银行账户名称与银行账户号码都不匹配";
                case "21002":
                    return "身份证号码与银行账户号码不匹配";
                case "21003":
                    return "银行账户名称与银行账户号码不匹配";
                case "21004":
                    return "银行代码与银行账户号码不匹配";
                case "21005":
                    return "该银行账户号码已被挂失";
                case "21006":
                    return "该银行账户号码已被冻结";
                case "21007":
                    return "该银行账户号码已被销户";
                case "21008":
                    return "该银行账户号码不存在";
                case "21009":
                    return "该银行账户状态异常";
                case "21010":
                    return "处理鉴权结果异常";
                case "21011":
                    return "手机号不正确";
                case "30001":
                    return "系统异常，请稍后重试";
                case "30002":
                    return "业务处理异常";
                case "B.MGW.0001":
                    return "MGW.错误的商户编号";
                case "B.MGW.0002":
                    return "MGW.DN 不匹配";
                case "B.MGW.0003":
                    return "MGW.商户状态不正确";
                case "B.MGW.0004":
                    return "MGW.错误的 LoginKey";
                case "B.MGW.0005":
                    return "MGW.错误的客户端 IP";
                case "B.MGW.0110":
                    return "MGW.商户提交的 Http Basic Auth 的信息非法";
                case "B.MGW.0120":
                    return "MGW.商户提交 / 回复的数据非法";
                case "B.MGW.0130":
                    return "MGW.不支持商户请求的交易类型";
                case "B.MGW.0140":
                    return "MGW.商户接受 TR3 的系统异常";
                case "B.MGW.0150":
                    return "MGW.商户网关对 TR3 请求进行签名时出现异常";
                case "B.MGW.0160":
                    return "MGW.发过来的系统参考号和我们期望的不一致";
                case "B.MGW.0170":
                    return "MGW.商户查询的交易流水不存在";
                case "B.MGW.0180":
                    return "MGW.商户返回通知消息版本 - POST 格式的数据有错，请求重发";
                case "B.MGW.0190":
                    return "MGW.商户卡号无效或者注册信息无效无法完成代扣交易";
                case "B.MGW.0200":
                    return "证书序列号无法验证通过";
                case "B.MGW.0999":
                    return "MGW.商户网关系统异常";
                case "B0":
                    return "持卡人撤销绑定错误";
                case "B1":
                    return "没收卡";
                case "B2":
                    return "签到失败，非法来源";
                case "B3":
                    return "订单信息不存在";
                case "B4":
                    return "获取结果超时，请查询交易结果";
                case "B5":
                    return "系统维护中，请稍后再试";
                case "B6":
                    return "持卡人身份不合法";
                case "B7":
                    return "交易服务器处理错误";
                case "B8":
                    return "获取持卡人帐户信息失败";
                case "B9":
                    return "持卡人绑定错误";
                case "BA":
                    return "卡信息错误次数超限，请联系发卡行";
                case "BB":
                    return "CVV 错误次数超限";
                case "BC":
                    return "无效卡";
                case "D0":
                    return "绑定失败，持卡人未应答或拒绝绑定";
                case "D1":
                    return "持卡人未接听或拒绝支付";
                case "D2":
                    return "撤销失败，持卡人未应答或拒绝撤销";
                case "D3":
                    return "持卡人未接听或拒绝修改绑定信息";
                case "D4":
                    return "持卡人修改绑定信息错误";
                case "D5":
                    return "支付成功，绑定失败";
                case "D9":
                    return "原交易日期错误";
                case "E0":
                    return "原交易时间错误";
                case "E1":
                    return "原交易流水号错误";
                case "E2":
                    return "绑定上限金额错误";
                case "E3":
                    return "银行卡密码为空或不足 6 位";
                case "E4":
                    return "金额格式错误";
                case "E5":
                    return "订单号不合法";
                case "E6":
                    return "银行卡号不合法";
                case "E7":
                    return "交易金额不合法";
                case "E8":
                    return "交易时间不合法";
                case "E9":
                    return "币种不合法";
                case "EA":
                    return "原交易已过期";
                case "EC":
                    return "交易有效期内，用户未支付成功";
                case "F0":
                    return "商品描述长度不符";
                case "F2":
                    return "支付号不合法";
                case "F3":
                    return "冲正说明性文字长度不符";
                case "F4":
                    return "会话已过期";
                case "F5":
                    return "业务类型有误";
                case "F6":
                    return "交易流水号不能为空";
                case "F7":
                    return "身份证号不合法";
                case "F8":
                    return "MAC 鉴别失败";
                case "FE":
                    return "该功能暂未开通";
                case "FG":
                    return "无商户对应信息或该接口方式未激活";
                case "FH":
                    return "商户信息有误";
                case "FI":
                    return "无法建立呼叫，用户忙或对方挂断";
                case "FJ":
                    return "无法建立呼叫, 用户关机(线路故障）";
                case "FK":
                    return "无法建立呼叫, 语音通道忙";
                case "FL":
                    return "无法建立呼叫, 未知原因";
                case "FM":
                    return "接听连接成功";
                case "FN":
                    return "无法建立呼叫, 不在服务区";
                case "FO":
                    return "无法建立呼叫, 用户停机";
                case "FP":
                    return "输入金额过小";
                case "FQ":
                    return "输入金额过大";
                case "FR":
                    return "超过输入次数";
                case "FS":
                    return "密码格式错误";
                case "FT":
                    return "选择菜单时输入超时";
                case "FU":
                    return "无法建立呼叫, 电话串线";
                case "FV":
                    return "呼叫转移";
                case "FW":
                    return "输入长度不足";
                case "FX":
                    return "取款密码错误";
                case "FY":
                    return "无法建立呼叫, 空号";
                case "FZ":
                    return "语音合成失败";
                case "G1":
                    return "系统风控参数载入错误";
                case "G2":
                    return "商户风控参数载入错误";
                case "G7":
                    return "超出系统当月金额限制";
                case "G8":
                    return "超出系统当月该银行卡成功交易次数限制";
                case "G9":
                    return "超出系统当月该银行卡密码错误次数限制";
                case "GA":
                    return "超出商户当月金额限制";
                case "GC":
                    return "超出系统当日该身份证成功交易次数限制";
                case "GE":
                    return "超出系统当月该身份证成功交易次数限制";
                case "GG":
                    return "超出系统当月该身份证密码错误次数限制";
                case "GH":
                    return "用户输入格式错误";
                case "GI":
                    return "用户无效输入";
                case "GJ":
                    return "用户输入成功";
                case "GL":
                    return "用户输入信息错误";
                case "GM":
                    return "用户两次输入不符";
                case "GN":
                    return "用户拒绝重新输入";
                case "GO":
                    return "用户拒绝输入";
                case "GP":
                    return "用户输入超时";
                case "GQ":
                    return "用户超过输入重复次数";
                case "GR":
                    return "用户输入过长";
                case "GS":
                    return "支付密码错误";
                case "GW":
                    return "超出系统当月该手机号更换银行卡支付次数限制";
                case "GX":
                    return "超出系统当月该手机号更换身份证支付次数限制";
                case "H0":
                    return "运营商系统忙";
                case "H1":
                    return "异地用户资料";
                case "H2":
                    return "非法用户资料";
                case "H3":
                    return "用户输入归属地州错";
                case "H4":
                    return "该号码已欠费停机";
                case "H5":
                    return "本地州尚未开通该业务";
                case "H6":
                    return "用户不存在或已销号";
                case "H7":
                    return "代收费不允许少缴费";
                case "H8":
                    return "无此定制帐号类型";
                case "H9":
                    return "该定制帐号尚未开通服务功能";
                case "HA":
                    return "该帐号无定制对应关系";
                case "HB":
                    return "定制已取消";
                case "HC":
                    return "该运营商尚未开通";
                case "HD":
                    return "交易成功，需打印单据，提示第二天补帐";
                case "HE":
                    return "该帐号已经绑定";
                case "HF":
                    return "定制已超过有效期";
                case "HI":
                    return "当天流水号重复";
                case "HU":
                    return "有效期不符";
                case "HV":
                    return "CVV2 不符";
                case "HW":
                    return "手机号码不符";
                case "HX":
                    return "姓名不符";
                case "HY":
                    return "证件类型不符";
                case "HZ":
                    return "证件号不符";
                case "IM":
                    return "无效的路由商户号";
                case "IN":
                    return "销售月限额已经用完";
                case "IY":
                    return "交易金额不允许";
                case "J1":
                    return "风控校验异常";
                case "J3":
                    return "只能对非即时订单进行支付";
                case "J5":
                    return "订单支付中";
                case "J6":
                    return "订单已过期";
                case "J7":
                    return "订单已作废";
                case "J8":
                    return "语音网关请求失败";
                case "J9":
                    return "手机号后六位错误";
                case "JA":
                    return "原手机号的绑定关系不存在";
                case "JB":
                    return "数据未加密";
                case "JC":
                    return "截止时间不能早于开始时间";
                case "JD":
                    return "当前页不能小于1";
                case "JE":
                    return "记录数不合法";
                case "JF":
                    return "银行卡号解密错误";
                case "JG":
                    return "无密钥序号或加密卡号";
                case "JH":
                    return "解析卡号异常";
                case "JI":
                    return "姓名长度有误";
                case "JJ":
                    return "证件地址长度有误";
                case "JL":
                    return "订单失效有误";
                case "JM":
                    return "订单有效期有误";
                case "JN":
                    return "机构编号有误";
                case "JO":
                    return "机构商户对用关系不存在";
                case "JP":
                    return "运营商余额信息不存在";
                case "JQ":
                    return "运营商类型有误";
                case "JS":
                    return "电力客户 ID 号有误";
                case "JT":
                    return "商户类型有误";
                case "JU":
                    return "电力欠费信息不存在";
                case "JV":
                    return "新卡密码错误1次";
                case "JW":
                    return "新卡密码错误2次";
                case "JX":
                    return "新卡密码错误3次";
                case "JY":
                    return "运营商帐号有误";
                case "JZ":
                    return "系统升级中，请稍后重试";
                case "K1":
                    return "查询企业状态信息异常";
                case "K2":
                    return "该企业处于未启用状态";
                case "K3":
                    return "该企业处于暂停状态";
                case "K4":
                    return "该企业处于关闭状态";
                case "K5":
                    return "该企业处于登出状态";
                case "K6":
                    return "该企业处于强制签退状态";
                case "K7":
                    return "该企业处于日终状态";
                case "K8":
                    return "基础参数校验错";
                case "K9":
                    return "卡标志无效";
                case "KA":
                    return "商户未上送(交易流水号/ 交易时间/ 订单号)";
                case "KB":
                    return "当日不能进行部分消费撤销";
                case "KC":
                    return "隔日不能进行消费撤销";
                case "KD":
                    return "超出一年退货有效期，不能退货";
                case "KE":
                    return "商户同一日不能进行隔日退票处理";
                case "KF":
                    return "特店不存在，拒绝交易";
                case "KH":
                    return "正在批处理";
                case "KI":
                    return "出生日期不正确";
                case "KJ":
                    return "交易失败";
                case "KK":
                    return "系统处于日终状态，拒绝交易";
                case "KL":
                    return "交易币种不支持";
                case "KM":
                    return "不支持的证件类型/ 证件号";
                case "KN":
                    return "地址校验拒绝";
                case "KO":
                    return "银行已日切，请提交退货处理";
                case "KP":
                    return "银行未结算，请提交撤销处理或系统日切后再次退货";
                case "KT":
                    return "身份证号被列入黑名单";
                case "LB":
                    return "退货超出银行有效期，请用快速退货";
                case "LG":
                    return "该银行卡未开通银联在线支付业务";
                case "MR":
                    return "商户不支持的卡类型";
                case "MT":
                    return "商户不支持的交易类型";
                case "N0":
                    return "不匹配的交易 / Unmatched Transaction";
                case "N1":
                    return "Valid Unmatched Transaction";
                case "NA":
                    return "卡数量超限";
                case "NB":
                    return "总金额不符";
                case "NC":
                    return "无此卡种";
                case "ND":
                    return "卡种类重复";
                case "NE":
                    return "购卡数为";
                case "NF":
                    return "库存不足";
                case "NG":
                    return "通联";
                case "NH":
                    return "卡已锁";
                case "NI":
                    return "与密管平台交互失败";
                case "R0":
                    return "交易不予承兑，请换卡重试";
                case "R5":
                    return "交易不予承兑，请换卡重试";
                case "RC":
                    return "中国银联拒绝交易码";
                case "RX":
                    return "乘机人与使用人不符，请确认后重新填写";
                case "T7":
                    return "未找到对应的安全级别";
                case "T8":
                    return "解绑失败，未绑定相关信息";
                case "T9":
                    return "绑定失败";
                case "TA":
                    return "鉴权失败";
                case "TB":
                    return "补全信息不存在或发生变化";
                case "TC":
                    return "商户权限不足";
                case "TD":
                    return "商户没有保存 PCI 的权限";
                case "TG":
                    return "卡有效期提供不一致";
                case "TU":
                    return "商户未开通卡信息验证";
                case "U1":
                    return "数据字段长度不符合定义";
                case "U2":
                    return "接入码错误";
                case "U3":
                    return "无结账交易";
                case "U4":
                    return "存在必填字段未上送";
                case "U5":
                    return "EDC 交易不能使用 DCC 编号";
                case "U6":
                    return "字段不符合要求";
                case "U7":
                    return "DCC 交易不能使用 EDC 编号";
                case "U8":
                    return "DCC 交易未能匹配 DCC 汇率查询";
                case "U9":
                    return "授权号已被使用，请求被拒绝";
                case "UA":
                    return "票据号不能全为0";
                case "UB":
                    return "非 DCC 交易禁止上送 DCC交易字段";
                case "UC":
                    return "不支持的报价币种";
                case "W0":
                    return "手机号与开户时登记的不一致";
                case "W1":
                    return "手机号与开户时登记的不一致，且超过支付限额";
                case "W2":
                    return "身份证号码与开户时登记的不一致";
                case "W3":
                    return "身份证号码与开户时登记的不一致，且超过支付限额";
                case "W4":
                    return "姓名与开户时登记的不一致";
                case "W5":
                    return "姓名与开户时登记的不一致，且超过支付限额";
                case "W6":
                    return "手机号、身份证号码、姓名与开户时登记的不一致";
                case "W7":
                    return "手机号、身份证号码、姓名与开户时登记的不一致，且超过支付限额";
                case "W8":
                    return "手机号、身份证号码与开户时登记的不一致";
                case "W9":
                    return "手机号、身份证号码与开户时登记的不一致，且超过支付限额";
                case "WA":
                    return "身份证号码、姓名与开户时登记的不一致";
                case "WB":
                    return "身份证号码、姓名与开户时登记的不一致，且超过支付限额";
                case "WC":
                    return "手机号、姓名与开户时登记的不一致";
                case "WD":
                    return "手机号、姓名与开户时登记的不一致，且超过支付限额";
                case "WE":
                    return "风控校验异常";
                case "WF":
                    return "您的卡号存在异常";
                case "WG":
                    return "您的手机号存在异常";
                case "WH":
                    return "您的身份证号存在异常";
                case "WI":
                    return "超出系统身份证当日金额限制";
                case "WJ":
                    return "超出商户身份证当日金额限制";
                case "WK":
                    return "超出系统身份证当月金额限制";
                case "WL":
                    return "超出商户身份证当月金额限制";
                case "WM":
                    return "超出系统手机号当日金额限制";
                case "WN":
                    return "超出商户手机号当日金额限制";
                case "WO":
                    return "超出系统手机号当月金额限制";
                case "WP":
                    return "超出商户手机号当月金额限制";
                case "WQ":
                    return "系统绑定风控参数载入错误";
                case "WR":
                    return "商户绑定风控参数载入错误";
                case "WS":
                    return "超出系统该卡绑定身份证数的限制";
                case "WT":
                    return "超出系统该卡绑定手机号数的限制";
                case "WU":
                    return "超出商户该卡绑定身份证数的限制";
                case "WV":
                    return "超出商户该卡绑定手机号数的限制";
                case "WW":
                    return "超出系统该身份证绑定银行卡数的限制";
                case "WX":
                    return "超出系统该身份证绑定手机号数的限制";
                case "WY":
                    return "超出商户该身份证绑定银行卡数的限制";
                case "WZ":
                    return "超出商户该身份证绑定手机号数的限制";
                case "X0":
                    return "超出系统该手机号绑定银行卡数的限制";
                case "X1":
                    return "超出系统该手机号绑定身份证数的限制";
                case "X2":
                    return "超出商户该手机号绑定银行卡数的限制";
                case "X3":
                    return "超出商户该身份证绑定手机号数的限制";
                case "X4":
                    return "超出当月手机号更改绑定关系次数限制";
                case "X5":
                    return "超出当月银行卡号更改绑定关系次数限制";
                case "Y1":
                    return "身份认证失败";
                case "Y2":
                    return "部分承兑";
                case "Y3":
                    return "重要人物批准";
                case "Y4":
                    return "无效的关联交易";
                case "Y5":
                    return "卡未初始化";
                case "Y6":
                    return "转账货币不一致";
                case "Y7":
                    return "有缺陷的成功";
                case "Y8":
                    return "安全处理失败";
                case "Y9":
                    return "重新提交交易请求";
                case "YA":
                    return "无效应答";
                case "YB":
                    return "无此帐单号码资料";
                case "YC":
                    return "费用已经缴纳";
                case "YD":
                    return "该帐单号码不能以该种方式缴费";
                case "YE":
                    return "该帐单号码未申请此项业务";
                case "YF":
                    return "定制号码类型有误";
                case "YG":
                    return "该定制号码已经取消";
                case "YH":
                    return "该定制号码尚未定制";
                case "YI":
                    return "该定制号码已超过有效期";
                case "YJ":
                    return "单笔代收没有定制成用户委托类型";
                case "YK":
                    return "消费交易中定制类型为非主动方式";
                case "YL":
                    return "交易终端为未定制过的非法终端(非法手机)";
                case "YM":
                    return "该帐单号码已经取消定制";
                case "YN":
                    return "该帐单号码已经定制";
                case "YO":
                    return "该帐单号码尚未定制";
                case "YP":
                    return "无此帐单号码标首";
                case "YQ":
                    return "该类帐单号码功能尚未开通";
                case "YR":
                    return "系统应用参数表中没有系统机构代码";
                case "YS":
                    return "调用交易类型转换出错";
                case "YT":
                    return "商户代号对应的商户 ID 与 33 域商户标识码不一致";
                case "YU":
                    return "帐单支付方式表中交易终端的定制状态非法";
                case "YV":
                    return "接入代理上送商户代号与帐单拆分出来的商户号不一致";
                case "YW":
                    return "该卡种功能尚未开通";
                case "YX":
                    return "该银行或机构未开通此功能（机构信息表中功能标志位对应的交易未开通）";
                case "YY":
                    return "委托关系不正确";
                case "YZ":
                    return "已超出系统设置的该银行卡能定制的帐单最大个数";
                case "Z1":
                    return "请先签到";
                case "Z2":
                    return "积分不够";
                case "Z3":
                    return "分期期数错";
                case "Z4":
                    return "分期计划错";
                case "Z5":
                    return "请联系快钱手工退货";
                case "Z6":
                    return "无效交易币种";
                case "Z7":
                    return "上批未结，请先结完上批";
                case "Z8":
                    return "不支持该卡种";
                case "Z9":
                    return "银联 EMV 卡完成交易请使用预授权完成通知交易重新上送";
                case "ZA":
                    return "根据银联规定，完成通知交易不能撤消";
                case "ZB":
                    return "请使用与预授权交易同一类型终端做完成交易";
                case "ZC":
                    return "请用刷卡方式进行交易";
                //case "X1": return "积分扣款失败";
                //case "X2": return "积分余额不足";
                //case "X3": return "积分退款失败";
                //case "X4": return "积分金额扣款失败";
                //case "X5": return "积分金额余额不足";
                //case "X6": return "积分金额退款失败";
                default:
                    return "";
            }
        }
    }
}
