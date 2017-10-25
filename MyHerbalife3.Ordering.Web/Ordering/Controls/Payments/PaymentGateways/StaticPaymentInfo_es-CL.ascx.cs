using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using System.Collections.Specialized;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class StaticPaymentInfo_CL : StaticPaymentInfo
    {
        protected override void fillPaymentInfo()
        {
            base.fillPaymentInfo();

            string paymentGatewayLogs = string.Empty;
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
                
                if (payment.Card.IssuerAssociation.ToString() == IssuerAssociationType.GenericDebitCard.ToString())
                {
                    lblCardNumber.Text = "N/A";
                }
                else
                {
                    lblCardNumber.Text = payment.Card.AccountNumber.Trim();
                }

                if ((this.Page as ProductsBase).CountryCode == "RS")
                {
                    lblCardNumber.Text = "--";
                    lblUrl.Text = "rs.myherbalife.com";
                    ResponseContext _responseContext ;
                    paymentGatewayLogs = OrderProvider.GetPaymentGatewayLog((this.Page as ProductsBase).OrderNumber, PaymentGatewayLogEntryType.Response).Where(l => l.Contains("result=CAPTURED") || l.Contains("result:=CAPTURED")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(paymentGatewayLogs))
                    {
                        _responseContext = new ResponseContext(paymentGatewayLogs);
                        if (_responseContext.PostedValues.AllKeys.Contains("result"))
                        {
                            if (!string.IsNullOrEmpty(_responseContext.PostedValues["auth"]))
                            {
                                lblAuthorizationCode.Text = _responseContext.PostedValues["auth"];
                            }
                        }
                    }
                }
                else
                {
                    lblAuthorizationCode.Text = payment.AuthorizationCode; 
                }
                lblAmount.Text = payment.Amount.ToString();
                lblTransactionDate.Text = System.DateTime.Today.ToShortDateString();                             
                lblPayOptions.Text = (payment.PaymentOptions as PaymentOptions_V01).NumberOfInstallments.ToString();
                lblInstallmentType.Text = payment.AuthorizationMerchantAccount; // Installment Type; borrow the Auth Merchant field to place the data
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
                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }
    }
}
