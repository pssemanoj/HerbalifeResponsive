using HL.Common.Utilities;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using System.Collections.Specialized;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class StaticPaymentInfo_RS : StaticPaymentInfo
    {
        protected override void fillPaymentInfo()
        {
            base.fillPaymentInfo();

            CreditPayment_V01 payment = CurrentPaymentInfo as CreditPayment_V01;
            if (null != payment)
            {
                pnlAlternatePayment.Visible = false;
                pnlCardPayments.Visible = true;
                trCardHolderName.Visible = false;
                trCardType.Visible = false;
                trCardExpDate.Visible = false;
                trAmountText.Visible = true;
                trPaymentOptions.Visible = true;
                trAddress.Visible = false;


                lblCardNumber.Text = "--";
                lblUrl.Text = "rs.myherbalife.com";
                List<string> PaymentGatewayLog = OrderProvider.GetPaymentGatewayLog((this.Page as ProductsBase).OrderNumber, PaymentGatewayLogEntryType.Response);
                string theOne = PaymentGatewayLog.Find(i => i.Contains("result=CAPTURED") || i.Contains("result:=CAPTURED"));
                if (!string.IsNullOrEmpty(theOne))
                {
                    trNestPayOrderId.Visible = trNestPayAuthCode.Visible = 
                        trNestPayPaymentCode.Visible = trNestPayTransStatus.Visible = trNestPayTransCode.Visible = trNestPayTransDate.Visible = tr3DTransStatus.Visible = false;

                    NameValueCollection theResponse = GetRequestVariables(theOne);

                    // Authorization Code
                    if (!string.IsNullOrEmpty(theResponse["auth"]))
                    {
                        lblAuthorizationCode.Text = theResponse["auth"];
                    }

                    // Authorization Code
                    if (!string.IsNullOrEmpty(theResponse["paymentid"]))
                    {
                        lblPaymentCode.Text = theResponse["paymentid"];
                    }

                    // Authorization Code
                    if (!string.IsNullOrEmpty(theResponse["tranid"]))
                    {
                        lblTransactionCode.Text = theResponse["tranid"];
                    }

                    lblAmount.Text = string.Concat(payment.Amount.ToString(), HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol);
                    lblTransactionDate.Text = DateUtils.GetCurrentLocalTime(CountryCode).ToString();
                    lblPayOptions.Text = (payment.PaymentOptions as PaymentOptions_V01).NumberOfInstallments.ToString();
                    lblInstallmentType.Text = payment.AuthorizationMerchantAccount; // Installment Type; borrow the Auth Merchant field to place the data
                }
                else
                {
                    trAmountText.Visible = trPaymentOptions.Visible =
                        trTrasanctionDate.Visible = trAuthorizationCode.Visible = trPaymentCode.Visible = trTransactionCode.Visible = false;

                    theOne = PaymentGatewayLog.Find(i => i.Contains("QueryString: Agency:=NestPay"));
                    if (!string.IsNullOrEmpty(theOne))
                    {
                        NameValueCollection theResponse = GetRequestVariables(theOne);

                        // order ID
                        if (!string.IsNullOrEmpty(theResponse["oid"]))
                        {
                            lblOrderId.Text = theResponse["oid"];
                        }

                        // Authorization Code
                        if (!string.IsNullOrEmpty(theResponse["AuthCode"]))
                        {
                            lblAuthorizationCode2.Text = theResponse["AuthCode"];
                        }

                        // Payment Status
                        if (!string.IsNullOrEmpty(theResponse["Response"]))
                        {
                            lblPaymentCode2.Text = theResponse["Response"];
                        }

                        // Transaction Status Code
                        if (!string.IsNullOrEmpty(theResponse["ProcReturnCode"]))
                        {
                            lblTransactionStatus.Text = theResponse["ProcReturnCode"];
                        }

                        // Transaction ID
                        if (!string.IsNullOrEmpty(theResponse["TransId"]))
                        {
                            lblTransactionCode2.Text = theResponse["TransId"];
                        }

                        // Transaction Date
                        if (!string.IsNullOrEmpty(theResponse["EXTRA.TRXDATE"]))
                        {
                            lblTransactionDate2.Text = theResponse["EXTRA.TRXDATE"];
                        }

                        // Status code for the 3D transaction
                        if (!string.IsNullOrEmpty(theResponse["mdStatus"]))
                        {
                            lbl3DTransactionStatus.Text = theResponse["mdStatus"];
                        }
                    }
                }
            }
        }

        private NameValueCollection GetRequestVariables(string requestData)
        {
            NameValueCollection result = new NameValueCollection();
            List<string> items = new List<string>(requestData.Split(new char[] { ';' }));
            foreach (string item in items)
            {
                string[] elements = item.Split(new char[] { '=' });
                if (elements.Length == 2)
                {
                    // Remove the last char if is a colon symbol.
                    if (elements[0].EndsWith(":"))
                    {
                        elements[0] = elements[0].Substring(0, elements[0].Length - 1);
                    }

                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }
    }
}
