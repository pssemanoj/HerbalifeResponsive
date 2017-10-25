using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using System.Web.Security;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using PaymentGatewayRecordStatusType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayRecordStatusType;
using CreditPayment_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01;
using System.Security.Cryptography.X509Certificates;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CN_99BillPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        public const string ProductName = "康宝莱产品订单";
        public const string ProductDesc = "商品描述";
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        private readonly PaymentGatewayInvoker _theInvoker;

        #region Constructors and Destructors

        private CN_99BillPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("CN_99BillPaymentGateway", paymentMethod, amount)
        {
            var payment =
               HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;

            if (payment != null && payment.TransactionType == "QP")
            {
                string invokerType = "CN_99BillQuickPayInvoker";

                var args = new object[] { paymentMethod, amount };
                var type = Type.GetType(string.Concat(RootNameSpace, invokerType), true, true);
                this._theInvoker =
                    Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as
                    PaymentGatewayInvoker;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override void Submit()
        {
            if (this._theInvoker != null)
            {
                this._theInvoker.Submit();
                return;
            }

            var redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
            var orderNumber = _orderNumber;
            var returnURL = _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved");
            var amount = _orderAmount <= 0 ? "0.00" : (Convert.ToDouble(_orderAmount.ToString("0.00")) * 100).ToString();
            var returnUrl = string.Concat(RootUrl, returnURL);
            var paymentGatewayReturnBgUrl = _configHelper.GetConfigEntry("paymentGatewayReturnBgUrl");
            var bgrUrl = string.Concat(RootUrl, paymentGatewayReturnBgUrl + "?rtnOrderNumber=" + orderNumber);
            var merchantId = _configHelper.GetConfigEntry("paymentGatewayMerchantdId");

            var disId = _distributorId;
            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var name = string.Empty;
            if (membershipUser != null)
            {
                name = membershipUser.Value.DistributorName();
                disId = membershipUser.Value.Id;
            }
            var sessionInfoMyCart = SessionInfo.GetSessionInfo(disId, _locale);
            sessionInfoMyCart.OrderStatus = SubmitOrderStatus.Unknown;
            var myCart = sessionInfoMyCart.ShoppingCart ??
                         ShoppingCartProvider.GetShoppingCart(disId, _locale);
            var email = string.Empty;
            var payerName = string.Empty;
            if (myCart != null)
            {
                email = (!string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress : string.Empty;
                payerName = (myCart.DeliveryInfo != null && myCart.DeliveryInfo.Address != null)
                                ? myCart.DeliveryInfo.Address.Recipient ?? string.Empty
                                : string.Empty;
            }
            var payment =
                HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;

            if (payment != null && payment.TransactionType == "CC")
            {
                PostCNP(myCart, payment, sessionInfoMyCart, disId, name);
                return;
            }

            if (payment != null)
            {
                if (payment.TransactionType == "10")
                    bgrUrl = bgrUrl + "&BankId=" + payment.AuthorizationMerchantAccount;
                if (payment.TransactionType == "12") bgrUrl = bgrUrl + "&BankId=Fastmoney";
            }
            string orderTime = DateTime.Now.ToString(DateTimeFormat);
            var signMsgVal = "";
            signMsgVal = AppendParam(signMsgVal, "inputCharset", "1");
            signMsgVal = AppendParam(signMsgVal, "pageUrl", returnUrl);
            signMsgVal = AppendParam(signMsgVal, "bgUrl", bgrUrl);
            signMsgVal = AppendParam(signMsgVal, "version", "v2.0");
            signMsgVal = AppendParam(signMsgVal, "language", "1");
            signMsgVal = AppendParam(signMsgVal, "signType", "4"); //4 = PKI signate
            signMsgVal = AppendParam(signMsgVal, "merchantAcctId", merchantId);
            signMsgVal = AppendParam(signMsgVal, "payerName", payerName); //?????
            signMsgVal = AppendParam(signMsgVal, "payerContactType", "1");
            signMsgVal = AppendParam(signMsgVal, "payerContact", email);
            signMsgVal = AppendParam(signMsgVal, "orderId", orderNumber);
            signMsgVal = AppendParam(signMsgVal, "orderAmount", amount);
            signMsgVal = AppendParam(signMsgVal, "orderTime", orderTime);
            signMsgVal = AppendParam(signMsgVal, "productName", ProductName);
            signMsgVal = AppendParam(signMsgVal, "productNum", "1");
            signMsgVal = AppendParam(signMsgVal, "productId", "");
            signMsgVal = AppendParam(signMsgVal, "productDesc", ProductDesc);
            signMsgVal = AppendParam(signMsgVal, "ext1", "");
            signMsgVal = AppendParam(signMsgVal, "ext2", "");
            signMsgVal = AppendParam(signMsgVal, "payType", payment != null ? payment.TransactionType : "10");
            if (payment.TransactionType == "10") // eBanking
            {
                signMsgVal = AppendParam(signMsgVal, "bankId",
                                         payment != null ? payment.AuthorizationMerchantAccount : string.Empty);
            }
            signMsgVal = AppendParam(signMsgVal, "redoFlag", "0");
            signMsgVal = AppendParam(signMsgVal, "pid", "");

            var signMsg = GenerateHash(signMsgVal);
            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat(BuildHtml("inputCharset", "1"));
            sb.AppendFormat(BuildHtml("pageUrl", returnUrl));
            sb.AppendFormat(BuildHtml("bgUrl", bgrUrl));
            sb.AppendFormat(BuildHtml("version", "v2.0"));
            sb.AppendFormat(BuildHtml("language", "1"));
            sb.AppendFormat(BuildHtml("signType", "4"));
            sb.AppendFormat(BuildHtml("merchantAcctId", merchantId));
            sb.AppendFormat(BuildHtml("payerName", payerName));
            sb.AppendFormat(BuildHtml("payerContactType", "1"));
            sb.AppendFormat(BuildHtml("payerContact", email));
            sb.AppendFormat(BuildHtml("orderId", orderNumber));
            sb.AppendFormat(BuildHtml("orderAmount", amount));
            sb.AppendFormat(BuildHtml("orderTime", orderTime));
            sb.AppendFormat(BuildHtml("productName", ProductName));
            sb.AppendFormat(BuildHtml("productNum", "1"));
            sb.AppendFormat(BuildHtml("productId", ""));
            sb.AppendFormat(BuildHtml("productDesc", ProductDesc));
            sb.AppendFormat(BuildHtml("ext1", ""));
            sb.AppendFormat(BuildHtml("ext2", ""));
            sb.AppendFormat(BuildHtml("payType", payment != null ? payment.TransactionType : "10"));

            if (payment.TransactionType == "10") // eBanking
            {
                sb.AppendFormat(BuildHtml("bankId", payment != null ? payment.AuthorizationMerchantAccount : string.Empty));
            }

            sb.AppendFormat(BuildHtml("redoFlag", "0"));
            sb.AppendFormat(BuildHtml("pid", ""));
            sb.AppendFormat(BuildHtml("signMsg", signMsg));
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            var response = sb.ToString();
            LogMessageWithInfo(PaymentGatewayLogEntryType.Request, OrderNumber, _distributorId, _gatewayName,
                       PaymentGatewayRecordStatusType.Unknown, response);

            bool isLockedeach = true;
            bool isLocked = true;
            string lockfailed = string.Empty;
            if (myCart != null && myCart.pcLearningPointOffSet > 0M && !(myCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
            {
                isLockedeach = OrderProvider.LockPCLearningPoint(_distributorId, orderNumber, new OrderMonth(myCart.CountryCode).OrderMonthShortString,
                                                  Convert.ToInt32(Math.Truncate(myCart.pcLearningPointOffSet)), HLConfigManager.Platform);
                if (!isLockedeach)
                {
                    lockfailed = "PC Learning Point";
                    isLocked = false;
                }

            }
            else if (myCart != null && myCart.pcLearningPointOffSet > 0M)
            {
                isLockedeach = OrderProvider.LockETOLearningPoint(
                        myCart.CartItems.Select(s => s.SKU),
                        _distributorId, 
                        orderNumber, 
                        new OrderMonth(myCart.CountryCode).OrderMonthShortString,
                        Convert.ToInt32(Math.Truncate(myCart.pcLearningPointOffSet)), 
                        HLConfigManager.Platform);

                if (!isLockedeach)
                {
                    lockfailed = " Learning Point";
                    isLocked = false;
                }
            }
            if (myCart.HastakenSrPromotion)
            {
                isLockedeach = ChinaPromotionProvider.LockSRPromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", SR Promotion";
                    isLocked = false;
                }
            }
            if (myCart.HastakenSrPromotionGrowing)
            {
                isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", SR Query Growing";
                    isLocked = false;
                }
            }
            if (myCart.HastakenSrPromotionExcelnt)
            {
                isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", SR Query Excellent";
                    isLocked = false;
                }
            }
            if (myCart.HastakenBadgePromotion)
            {
                isLockedeach = ChinaPromotionProvider.LockBadgePromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", Badge promo";
                    isLocked = false;
                }
            }
            if (myCart.HastakenNewSrpromotion)
            {
                isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", NewSrPromotion";
                    isLocked = false;
                }
            }
            if ( myCart.HasBrochurePromotion)
            {
                isLockedeach = ChinaPromotionProvider.LockBrochurePromotion(myCart, OrderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", Brochure Promotion";
                    isLocked = false;
                }
            }
            if (isLocked)
            {
                HttpContext.Current.Response.Write(response);
                HttpContext.Current.Response.End();
            }
            else
            {
                LogMessageWithInfo(PaymentGatewayLogEntryType.Response, OrderNumber, _distributorId, _gatewayName,
                        PaymentGatewayRecordStatusType.Declined, lockfailed.TrimStart(',') + " locking fails.");
            }
        }


        public static string BuildHtml(string inputName, string inputValue)
        {
            if (!string.IsNullOrEmpty(inputName) && !string.IsNullOrEmpty(inputValue))
            {
                return string.Format("<input type='hidden' name='{0}' value='{1}'/>", inputName, inputValue);
            }
            return string.Empty;
        }
        /// <summary>
        /// AppendParam
        /// </summary>
        /// <param name="returnStr"></param>
        /// <param name="paramId"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public static string AppendParam(string returnStr, string paramId, string paramValue)
        {
            if (returnStr != "")
            {
                if (paramValue != "")
                {
                    returnStr += "&" + paramId + "=" + paramValue;
                }
            }
            else
            {
                if (paramValue != "")
                {
                    returnStr = paramId + "=" + paramValue;
                }
            }
            return returnStr;
        }

        public string GenerateHash(string message)
        {
            string result = "";
            var privateCertName = _configHelper.GetConfigEntry("gatewayPrivateCertName");

            var storex = new X509Store(StoreLocation.LocalMachine);
            storex.Open(OpenFlags.ReadOnly);
            var certificates = storex.Certificates.Find(X509FindType.FindBySubjectName, privateCertName, false);
            if (certificates != null && certificates.Count > 0 && certificates[0] != null)
            {
                var cert = certificates[0];

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

                RSACryptoServiceProvider rsapri = (RSACryptoServiceProvider)cert.PrivateKey;
                RSAPKCS1SignatureFormatter f = new RSAPKCS1SignatureFormatter(rsapri);
                byte[] resultInBytesArray;
                f.SetHashAlgorithm("SHA1");
                SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
                resultInBytesArray = sha.ComputeHash(bytes);
                result = System.Convert.ToBase64String(f.CreateSignature(resultInBytesArray)).ToString();
            }

            return result;
        }

        public static PaymentGatewayResponse GetOrderStatus(string orderNumber)
        {
            var _distributorId = HttpContext.Current.User.Identity.Name;
            var currentSession = SessionInfo.GetSessionInfo(_distributorId, HLConfigManager.Configurations.Locale);
            if (currentSession.OrderQueryStatus99BillInprocess) return null;
            //LogMessage(PaymentGatewayLogEntryType.Request,
            //              orderNumber,
            //              string.Empty,
            //              "CN_99BillPaymentGateway",
            //              PaymentGatewayRecordStatusType.Unknown, "GatewayOrderQueryRequest for OrderNumber: " + orderNumber);
            var requestString = "GatewayOrderQueryRequest for OrderNumber: " + orderNumber;
            var orderStatusResponse = China.OrderProvider.Get99BillOrderStatus(orderNumber);

            if (orderStatusResponse == null) return null;
            if (orderStatusResponse.IsCreditCardOrder && !string.IsNullOrEmpty(orderStatusResponse.CreditCardResponse))
            {
                return CNPQueryResponse(orderStatusResponse.CreditCardResponse, orderNumber);
            }
            if (orderStatusResponse.IsWechatOrder)
            {
                PaymentGatewayResponse response = new CN_99BillPaymentGatewayResponse();
                response.OrderNumber = orderNumber;
                response.CanSubmitIfApproved = true;
                response.ReloadShoppingCart = true;
                if (!string.IsNullOrEmpty(orderStatusResponse.WechatResponse) &&
                    orderStatusResponse.WechatResponse == "SUCCESS")
                {
                    response.IsApproved = true;
                }
                else
                {
                    response.IsApproved = false;
                }
                return response;
            }


            if (orderStatusResponse.GatewayResponse == null) return null;
            if (orderStatusResponse.GatewayResponse.OrderDetail != null &&
                orderStatusResponse.GatewayResponse.OrderDetail.Any())
            {
                if (orderStatusResponse.GatewayResponse.OrderDetail.FirstOrDefault().PayResult != "10")
                {
                    LogMessageWithInfo(
                        PaymentGatewayLogEntryType.Response,
                        orderNumber,
                        string.Empty,
                        orderStatusResponse.GetType().Name.Replace("Response", string.Empty),
                        PaymentGatewayRecordStatusType.Declined, requestString + ToXml(orderStatusResponse.GatewayResponse)
                        );
                    return null;
                }
                currentSession.OrderQueryStatus99BillInprocess = true;
                LogMessageWithInfo(PaymentGatewayLogEntryType.Response,
                    orderNumber,
                    string.Empty,
                    orderStatusResponse.GetType().Name.Replace("Response", string.Empty),
                    PaymentGatewayRecordStatusType.Approved, requestString + ToXml(orderStatusResponse.GatewayResponse)
                    );
                PaymentGatewayResponse response = new CN_99BillPaymentGatewayResponse();

                response.IsApproved = true;
                response.OrderNumber = orderNumber;
                response.CanSubmitIfApproved = true;
                response.ReloadShoppingCart = true;
                response.SpecialResponse = string.Format("{0},{1}",
                    orderStatusResponse.GatewayResponse.OrderDetail.FirstOrDefault()
                        .DealId,
                    orderStatusResponse.GatewayResponse.OrderDetail.FirstOrDefault()
                        .DealId);
                return response;
            }
            var strDecline = Settings.GetRequiredAppSetting("EbankingResponseCodeForDecline", "31003,31005,31006");
            var responseCodeForDecline = new List<string>(strDecline.Split(new char[] { ',' }));
            if (responseCodeForDecline.Contains(orderStatusResponse.GatewayResponse.ErrorCode))
            {
                currentSession.OrderQueryStatus99BillInprocess = false;
                LogMessageWithInfo(
                    PaymentGatewayLogEntryType.Response,
                    orderNumber,
                    string.Empty,
                    orderStatusResponse.GetType().Name.Replace("Response", string.Empty),
                    PaymentGatewayRecordStatusType.Declined, requestString + ToXml(orderStatusResponse.GatewayResponse)
                    );
            }
            return null;
        }

        private static string ToXml(Gateway99BillOrderQueryResponse request)
        {
            try
            {
                var ser = new XmlSerializer(typeof(Gateway99BillOrderQueryResponse));
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                ser.Serialize(writer, request);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "ToXML failed serializing in CN_99BillPaymentGatewayInvoker: GetOrderStatus");
                return null;
            }
        }

        protected override void GetOrderNumber()
        {
            if (this._theInvoker != null)
            {
                var type = this._theInvoker.GetType();
                var methodInfo = type.GetMethod(
                    "GetOrderNumber", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                methodInfo.Invoke(this._theInvoker, new object[] { });
                return;
            }

            var orderData = _context.Session[PaymentGateWayOrder] as string;
            _context.Session.Remove(PaymentGateWayOrder);
            var currentSession = SessionInfo.GetSessionInfo(_distributorId, _locale);
            if (!string.IsNullOrEmpty(currentSession.PendingOrderId))
            {
                _orderNumber = currentSession.PendingOrderId;
                currentSession.PendingOrderId = string.Empty;
                SessionInfo.SetSessionInfo(_distributorId, _locale, currentSession);
                OrderProvider.UpdatePaymentGatewayRecord(_orderNumber, orderData, PaymentGatewayLogEntryType.Request,
                                                         PaymentGatewayRecordStatusType.Unknown);
            }
            else
            {
                var request = new MyHerbalife3.Ordering.ServiceProvider.OrderSvc.GenerateOrderNumberRequest_V01
                {
                    Amount = _orderAmount,
                    Country = _country,
                    DistributorID = _distributorId
                };
                var response = OrderProvider.GenerateOrderNumber(request);
                if (null != response)
                {
                    _orderNumber = response.OrderID;
                }
                OrderProvider.InsertPaymentGatewayRecord(_orderNumber, _distributorId, _gatewayName, orderData, _locale);
            }
        }
    }
}