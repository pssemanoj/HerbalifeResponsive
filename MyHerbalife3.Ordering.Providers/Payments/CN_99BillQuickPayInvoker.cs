using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CN_99BillQuickPayInvoker : PaymentGatewayInvoker
    {
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        private CN_99BillQuickPayInvoker(string paymentMethod, decimal amount)
            : base("CN_99BillPaymentGateway", paymentMethod, amount)
        {
        }

        public override void Submit()
        {
            bool isLockedeach = true;
            bool isLocked = true;
            string lockfailed = string.Empty;
            var currentSession = SessionInfo.GetSessionInfo(HttpContext.Current.User.Identity.Name, _locale);
            if (currentSession != null)
            {
                if (currentSession.ShoppingCart != null && currentSession.ShoppingCart.pcLearningPointOffSet > 0M && !(currentSession.ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO))
                {
                    isLockedeach = OrderProvider.LockPCLearningPoint(_distributorId, OrderNumber, new OrderMonth(currentSession.ShoppingCart.CountryCode).OrderMonthShortString,
                                                   Convert.ToInt32(Math.Truncate(currentSession.ShoppingCart.pcLearningPointOffSet)), HLConfigManager.Platform);
                    if (!isLockedeach)
                    {
                        lockfailed = "PC Learning Point";
                        isLocked = false;
                    }
                }
                else if (currentSession.ShoppingCart != null && currentSession.ShoppingCart.pcLearningPointOffSet > 0M)
                {
                    isLockedeach = OrderProvider.LockETOLearningPoint(
                            currentSession.ShoppingCart.CartItems.Select(s => s.SKU),
                            _distributorId, 
                            OrderNumber, 
                            new OrderMonth(currentSession.ShoppingCart.CountryCode).OrderMonthShortString,
                            Convert.ToInt32(Math.Truncate(currentSession.ShoppingCart.pcLearningPointOffSet)), 
                            HLConfigManager.Platform);

                    if (!isLockedeach)
                    {
                        lockfailed = "ETO Learning Point";
                        isLocked = false;
                    }
                }
                if (currentSession.ShoppingCart.HastakenSrPromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRPromotion(currentSession.ShoppingCart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Promotion";
                        isLocked = false;
                    }
                }
                if (currentSession.ShoppingCart.HastakenSrPromotionGrowing)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQGrowingPromotion(currentSession.ShoppingCart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Growing";
                        isLocked = false;
                    }
                }
                if (currentSession.ShoppingCart.HastakenSrPromotionExcelnt)
                {
                    isLockedeach = ChinaPromotionProvider.LockSRQExcellentPromotion(currentSession.ShoppingCart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", SR Query Excellent";
                        isLocked = false;
                    }
                }
                if (currentSession.ShoppingCart.HastakenBadgePromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockBadgePromotion(currentSession.ShoppingCart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", Badge promo";
                        isLocked = false;
                    }
                }
                if (currentSession.ShoppingCart.HastakenNewSrpromotion)
                {
                    isLockedeach = ChinaPromotionProvider.LockNewSRPromotion(currentSession.ShoppingCart, _orderNumber);
                    if (!isLockedeach)
                    {
                        lockfailed = lockfailed + ", NewSrPromotion";
                        isLocked = false;
                    }
                }
            }

            if (currentSession.ShoppingCart.HasBrochurePromotion)
            {
                isLockedeach = ChinaPromotionProvider.LockBrochurePromotion(currentSession.ShoppingCart, _orderNumber);
                if (!isLockedeach)
                {
                    lockfailed = lockfailed + ", Brochure Promotion";
                    isLocked = false;
                }
            }
            if (isLocked)
            {
                CN_99BillQuickPayProvider provider = new CN_99BillQuickPayProvider();
                var resultString = provider.Submit(_orderNumber, _orderAmount);

                if (!string.IsNullOrEmpty(resultString))
                {
                    var verStr = Encrypt(resultString, EncryptionKey);
                    var url = string.Format("{0}?QPVerStr={1}", _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"), verStr);

                    HttpContext.Current.Response.Redirect(url, false);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {
                var resultString = string.Format("{0},{1},{2},{3},{4},{5}", OrderNumber, "0", DateTime.Now.ToString(DateTimeFormat), "", "", "");

                var verStr = Encrypt(resultString, EncryptionKey);
                var url = string.Format("{0}?QPVerStr={1}", _configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"), verStr);

                HttpContext.Current.Response.Redirect(url, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
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

        protected override void GetOrderNumber()
        {
            var currentSession = SessionInfo.GetSessionInfo(HttpContext.Current.User.Identity.Name, _locale);

            if (currentSession != null && !string.IsNullOrEmpty(currentSession.OrderNumber))
            {
                _orderNumber = currentSession.OrderNumber;

                string orderData = _context.Session[PaymentGateWayOrder] as string;
                _context.Session.Remove(PaymentGateWayOrder);
            }
            else
            {
                base.GetOrderNumber();
            }
        }
    }
}
