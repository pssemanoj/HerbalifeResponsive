
namespace MyHerbalife3.Ordering.Providers.Payments
{
using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class HU_PayUPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private HU_PayUPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("HU_PayUPaymentGateway", paymentMethod, amount)
        {

        }

        #region Public Methods and Operators

        public override void Submit()
        {
            string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");            
            string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);
            string returnUrlApproved = (string.Format("{0}?Agency=PayU&OrderNumber={1}&Redirect=Yes", returnUrl, this.OrderNumber));
            string merchantId = _configHelper.GetConfigEntry("paymentGatewayMerchantdId");
            string key = _configHelper.GetConfigEntry("paymentGatewayKey");
            key = Prehash(key);
            string PayUMessage;
            PayUOrder Order = new PayUOrder();
            List<PayUItem> OrderItemList = new List<PayUItem>();

            GenerateRequest(Order, merchantId, key, returnUrlApproved);
            PayUMessage = GenerateMessage(Order);
            string hash = string.Empty;
            hash = checkHMAC(key, PayUMessage);
           
            
            // Post and redirect to Produbanco website
            HttpContext.Current.Response.Clear();

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""frmSolicitudPago""].submit()'>");
            sb.AppendFormat("<form name='frmSolicitudPago' action='{0}' method='post'>", redirectUrl);
            sb.AppendFormat("<input type='hidden' id='MERCHANT' name='MERCHANT' value='{0}'>", Order.Merchant);
            sb.AppendFormat("<input type='hidden' id='ORDER_REF' name='ORDER_REF' value='{0}'>", Order.Order_ref);
            sb.AppendFormat("<input type='hidden' id='ORDER_DATE' name='ORDER_DATE' value='{0}'>", Order.Order_date.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendFormat("<input type='hidden' id='ORDER_PNAME' name='ORDER_PNAME[]' value='{0}'>", Order.Order_items[0].Order_pname);
            sb.AppendFormat("<input type='hidden' id='ORDER_PCODE' name='ORDER_PCODE[]' value='{0}'>", Order.Order_items[0].Order_pcode);
            sb.AppendFormat("<input type='hidden' id='ORDER_PINFO' name='ORDER_PINFO[]' value='{0}'>", Order.Order_items[0].Order_pinfo);
            sb.AppendFormat("<input type='hidden' id='ORDER_PRICE' name='ORDER_PRICE[]' value='{0}'>", Order.Order_items[0].Order_price);
            sb.AppendFormat("<input type='hidden' id='ORDER_QTY' name='ORDER_QTY[]' value='{0}'>", Order.Order_items[0].Order_qty);
            sb.AppendFormat("<input type='hidden' id='ORDER_VAT' name='ORDER_VAT[]' value='{0}'>", Order.Order_items[0].Order_vat);
            sb.AppendFormat("<input type='hidden' id='ORDER_VER' name='ORDER_VER[]' value='{0}'>", Order.Order_items[0].Order_ver);
            sb.AppendFormat("<input type='hidden' id='ORDER_SHIPPING' name='ORDER_SHIPPING' value='{0}'>", Order.Order_shipping);
            sb.AppendFormat("<input type='hidden' id='PRICES_CURRENCY' name='PRICES_CURRENCY' value='{0}'>", Order.Prices_currency);
            sb.AppendFormat("<input type='hidden' id='DISCOUNT' name='DISCOUNT' value='{0}'>", Order.Discount);
            sb.AppendFormat("<input type='hidden' id='PAY_METHOD' name='PAY_METHOD' value='{0}'>", Order.Pay_method);
            sb.AppendFormat("<input type='hidden' id='ORDER_PGROUP' name='ORDER_PGROUP[]' value='{0}'>", Order.Order_items[0].Order_pgroup);
            sb.AppendFormat("<input type='hidden' id='LANGUAGE' name='LANGUAGE' value='{0}'>", Order.Language);
            sb.AppendFormat("<input type='hidden' id='AUTOMODE' name='AUTOMODE' value='{0}'>", Order.Automode);
            sb.AppendFormat("<input type='hidden' id='TESTORDER' name='TESTORDER' value='{0}'>", Order.Testorder);
            sb.AppendFormat("<input type='hidden' id='DEBUG' name='DEBUG' value='{0}'>", Order.Debug);
            sb.AppendFormat("<input type='hidden' id='ORDER_TIMEOUT' name='ORDER_TIMEOUT' value='{0}'>", Order.Order_timeout);
            sb.AppendFormat("<input type='hidden' id='TIMEOUT_URL' name='TIMEOUT_URL' value='{0}'>", Order.Timeout_url);
            sb.AppendFormat("<input type='hidden' id='BACK_REF' name='BACK_REF' value='{0}'>", Order.Back_ref);
            sb.AppendFormat("<input type='hidden' id='ORDER_HASH' name='ORDER_HASH' value='{0}'>", hash);  
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            string response = sb.ToString();
            LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

            HttpContext.Current.Response.Write(response);
            HttpContext.Current.Response.End();
        }



        #endregion

        #region  Private Methods      



        private string checkHMAC(string key, string message)
        {
            if (key == null) return string.Empty;
            if (message == null) return string.Empty;
            string hmac1 = string.Empty;

            try
            {
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] keyByte = encoding.GetBytes(key);
                HMACMD5 hmacmd5 = new HMACMD5(keyByte);
                byte[] messageBytes = encoding.GetBytes(message);
                byte[] hashmessage = hmacmd5.ComputeHash(messageBytes);
                hmac1 = ByteToString(hashmessage);
            }
            catch (System.Security.Cryptography.CryptographicException ext)
            {
                LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "Crypto Exc" +  ext.Message);
                LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "Crypto Exc"+ ext.StackTrace);
            }
            catch (Exception ex)
            {
                LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "Normal Exc" + ex.Message);
                LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, "Normal Exc" + ex.StackTrace);
            } 
            return hmac1;
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("x2"); // hex format
            }
            return (sbinary);
        }

        private void GenerateRequest(PayUOrder OrderPayU, string merchant, string key, string url)
        {
            double gmtHungary = 9;   
            var OrderItemList = new List<PayUItem>();
            OrderPayU.Merchant = merchant;
            OrderPayU.Order_ref = this.OrderNumber;
            OrderPayU.Order_date = System.DateTime.Now.AddHours(gmtHungary); 
            OrderPayU.Order_shipping = "0";
            OrderPayU.Prices_currency = "HUF";
            OrderPayU.Discount = "0";
            OrderPayU.Pay_method = "CCVISAMC";
            OrderPayU.Language = "HU";
            OrderPayU.Automode = "0";
            OrderPayU.Testorder = "FALSE";//Change to FALSE once we are in prod by webconfig
            OrderPayU.Debug = "0";
            OrderPayU.Order_timeout = "1200";
            OrderPayU.Timeout_url = string.Concat(url, "&Timeout=TimeoutPayU"); ;
            OrderPayU.Back_ref = url;
            OrderItemList.Add(new PayUItem("001", this.OrderNumber, "001", "001",  Convert.ToInt32(this._orderAmount).ToString(), "1", "0", "1"));
            OrderPayU.Order_items = OrderItemList; 
           
        }

        private string Prehash(string key)
        {
            string hashKey = key.Replace("@@@@", "&T_=");
            return hashKey;
        }

            

        private string GenerateMessage(PayUOrder OrderPayU)
        {
            StringBuilder sb = new StringBuilder();
            string itemsName = string.Empty;
            string itemsCode = string.Empty;
            string itemsGroup = string.Empty;
            string itemsInfo = string.Empty;
            string itemsPrice = string.Empty;
            string itemsQty = string.Empty;
            string itemsVat = string.Empty;
            string itemsVer = string.Empty;

            sb.Append(OrderPayU.Merchant.Length.ToString());
            sb.Append(OrderPayU.Merchant);
            sb.Append(OrderPayU.Order_ref.Length.ToString());
            sb.Append(OrderPayU.Order_ref);
            sb.Append(OrderPayU.Order_date.ToString("yyyy-MM-dd HH:mm:ss").Length.ToString());
            sb.Append(OrderPayU.Order_date.ToString("yyyy-MM-dd HH:mm:ss"));

            foreach (PayUItem item in OrderPayU.Order_items)
            {
                itemsName = itemsName + item.Order_pname.Length.ToString();
                itemsName = itemsName + item.Order_pname;
                itemsCode = itemsCode + item.Order_pcode.Length.ToString();
                itemsCode = itemsCode + item.Order_pcode;
                itemsGroup = itemsGroup + item.Order_pgroup.Length.ToString();
                itemsGroup = itemsGroup + item.Order_pgroup;
                itemsInfo = itemsInfo + item.Order_pinfo.Length.ToString();
                itemsInfo = itemsInfo + item.Order_pinfo;
                itemsPrice = itemsPrice + item.Order_price.Length.ToString();
                itemsPrice = itemsPrice + item.Order_price;
                itemsQty = itemsQty + item.Order_qty.Length.ToString();
                itemsQty = itemsQty + item.Order_qty;
                itemsVat = itemsVat + item.Order_vat.Length.ToString();
                itemsVat = itemsVat + item.Order_vat;
                itemsVer = itemsVer + item.Order_ver.Length.ToString();
                itemsVer = itemsVer + item.Order_ver;
            }

            sb.Append(itemsName);
            sb.Append(itemsCode);
            sb.Append(itemsInfo);
            sb.Append(itemsPrice);
            sb.Append(itemsQty);
            sb.Append(itemsVat);
            sb.Append(itemsVer);
            sb.Append(OrderPayU.Order_shipping.Length.ToString());
            sb.Append(OrderPayU.Order_shipping);
            sb.Append(OrderPayU.Prices_currency.Length.ToString());
            sb.Append(OrderPayU.Prices_currency);
            sb.Append(OrderPayU.Discount.Length.ToString());
            sb.Append(OrderPayU.Discount);
            sb.Append(OrderPayU.Pay_method.Length.ToString());
            sb.Append(OrderPayU.Pay_method);
            sb.Append(itemsGroup);
            return sb.ToString();
        }

        #endregion


    }   


 public class PayUItem
    {
        public PayUItem(string order_pname, string order_pcode, string order_pgroup, string order_pinfo, string order_price,
             string order_qty, string order_vat, string order_ver)
        {
            Order_pname = order_pname;
            Order_pcode = order_pcode;
            Order_pgroup = order_pgroup;
            Order_pinfo = order_pinfo;
            Order_price = order_price;
            Order_qty = order_qty;
            Order_vat = order_vat;
            Order_ver = order_ver;

        }

        public string Order_pname { get; set; }
        public string Order_pcode { get; set; }
        public string Order_pgroup { get; set; }
        public string Order_pinfo { get; set; }
        public string Order_price { get; set; }
        public string Order_qty { get; set; }
        public string Order_vat { get; set; }
        public string Order_ver { get; set; }

    }


    public class PayUOrder
    {
        public PayUOrder()
        { }

        public string Merchant { get; set; }
        public string Order_ref { get; set; }
        public DateTime Order_date { get; set; }
        public List<PayUItem> Order_items { get; set; }
        public string Order_shipping { get; set; }
        public string Prices_currency { get; set; }
        public string Discount { get; set; }
        public string Pay_method { get; set; }
        public string Language { get; set; }
        public string Automode { get; set; }
        public string Testorder { get; set; }
        public string Debug { get; set; }
        public string Order_timeout { get; set; }
        public string Timeout_url { get; set; }
        public string Back_ref { get; set; }
        public string Order_hash { get; set; }

    }

}
