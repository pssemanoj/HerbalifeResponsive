using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class StaticPaymentInfo_MY : StaticPaymentInfo
    {
        protected override void fillPaymentInfo()
        {
            base.fillPaymentInfo();

            if ((this.Page as ProductsBase) != null
                && ((ProductsBase) this.Page).CountryCode == "MY"
                && CurrentPaymentInfo is CreditPayment_V01
                && ((CreditPayment_V01)CurrentPaymentInfo).AuthorizationMethod == AuthorizationMethodType.PaymentGateway)
            {
                pnlFPXData.Visible = true;
                var sessionData =  SessionInfo.GetSessionInfo(((ProductsBase) this.Page).DistributorID, ((ProductsBase) this.Page).Locale);
                List<string> paymentGatewayLog = OrderProvider.GetPaymentGatewayLog(sessionData.OrderNumber, PaymentGatewayLogEntryType.Response);
                if (null != paymentGatewayLog)
                {
                    string theOne = paymentGatewayLog.Find(i => i.Contains("QueryString: Agency:=FpxPaymentGateway"));
                    if (!string.IsNullOrEmpty(theOne))
                    {
                        var dataLog = paymentGatewayLog.LastOrDefault();
                        if (null != dataLog)
                        {
                            NameValueCollection response = GetRequestVariables(theOne);
                            var dictionary = response.AllKeys.ToDictionary(k => k, k => response[k]);
                            lblMerchantName.Text = FormatTextData(dictionary, "", HLConfigManager.Configurations.PaymentsConfiguration.FPXMerchantName);
                            lblFPXTransactionId.Text = FormatTextData(dictionary, "fpx_fpxTxnId:");
                            lblProductDescription.Text = FormatTextData(dictionary, "fpx_sellerOrderNo:", "Herbalife order - ");
                            var dateTimeFormat = HLConfigManager.Configurations.DOConfiguration.DateTimeFormat;
                            lblDateAndTime.Text = FormatTextData(dictionary, "fpx_fpxTxnTime:", "", dateTimeFormat);
                            lblBuyerBank.Text = FormatTextData(dictionary, "fpx_buyerBankBranch:");
                            lblBankReferenceNumber.Text = FormatTextData(dictionary, "fpx_debitAuthNo:");
                            string pghPaymentStatus =String.Empty;
                            if (Session[PaymentGatewayResponse.PGH_FPX_PaymentStatus] != null)
                            {
                                pghPaymentStatus = (string)Session[PaymentGatewayResponse.PGH_FPX_PaymentStatus];
                                if (!string.IsNullOrEmpty(pghPaymentStatus))
                                {
                                    lblTransactionStatus.Text = FormatTextData(dictionary, "", pghPaymentStatus);
                                }
                            }
                            if (pghPaymentStatus == "Declined")
                            {
                                lblProductDescription.Visible = false;
                                lblProductDescriptionText.Visible = false;
                                lblReference.Visible = false;
                                lblReferenceText.Visible = false;
                            }
                        }
                    }
                }
            }
            else
            {
                pnlFPXData.Visible = false;
            }
        }

        /// <summary>
        /// Formats the text to display.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key of the dictionary.</param>
        /// <param name="value">The value defined.</param>
        /// <param name="format">The format to apply.</param>
        private string FormatTextData(IDictionary<string,string> dictionary, string key, string value = "", string format = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(format) && !string.IsNullOrEmpty(key))
                {
                    string dictValue;
                    string customFormat = "yyyyMMddHHmmss";

                    
                    if (dictionary.TryGetValue(key, out dictValue))
                    {
                        var dateValue = DateTime.ParseExact(dictValue, customFormat, CultureInfo.CurrentCulture);
                        return string.Format("{0}", dateValue.ToString(format, CultureInfo.CurrentCulture));
                    }                  
                }

                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(key))
                {
                    return value;
                }

                if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
                {
                    if (dictionary.TryGetValue(key, out value))
                    {
                        return value;
                    }                    
                }

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    string dictValue;
                    if (dictionary.TryGetValue(key, out dictValue))
                    {
                        return string.Format("{0} {1}", value, dictValue);
                    }                    
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }            

            return string.Empty;
        }

        /// <summary>
        /// Gets the request variables.
        /// </summary>
        /// <param name="requestData">The request data.</param>
        private NameValueCollection GetRequestVariables(string requestData)
        {
            NameValueCollection result = new NameValueCollection();
            List<string> items = new List<string>(requestData.Split(new char[] { ';' }));
            foreach (string item in items)
            {
                string[] elements = item.Split(new char[] { '=' });
                if (elements.Length == 2)
                {
                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }
    }
}