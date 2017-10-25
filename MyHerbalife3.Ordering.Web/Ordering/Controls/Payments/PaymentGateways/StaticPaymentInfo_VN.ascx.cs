using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class StaticPaymentInfo_VN : StaticPaymentInfo
    {
        const string resPayment = "PaymentDisplay_";
        const string htmlNewLine = "<br />";

        protected override void fillPaymentInfo()
        {
            base.fillPaymentInfo();

            if ((this.Page as ProductsBase) != null
                && ((ProductsBase)this.Page).CountryCode == "VN"
                && CurrentPaymentInfo is CreditPayment_V01
                && ((CreditPayment_V01)CurrentPaymentInfo).AuthorizationMethod == AuthorizationMethodType.PaymentGateway)
            {
                trAlternatePaymentMethod.Visible = false;
                trPGHPaymentMethod.Visible = true;

                var sessionData = SessionInfo.GetSessionInfo(((ProductsBase)this.Page).DistributorID, ((ProductsBase)this.Page).Locale);
                List<string> paymentGatewayLog = OrderProvider.GetPaymentGatewayLog(sessionData.OrderNumber, PaymentGatewayLogEntryType.Response);
                if (null != paymentGatewayLog)
                {
                    string paymentData = string.Empty;

                    string theOne = paymentGatewayLog.Find(i => i.Contains("QueryString: Agency:=CyberSource"));
                    if (!string.IsNullOrEmpty(theOne))
                    {
                        string pghResponse = paymentGatewayLog.Find(i => i.Contains("SourceApplication=PGH;"));
                        if (!string.IsNullOrEmpty(pghResponse))
                        {
                            paymentData = htmlNewLine;
                            NameValueCollection responsePGH = GetRequestVariables(pghResponse);
                            var dictionatyResponsePGH = responsePGH.AllKeys.ToDictionary(k => k, k => responsePGH[k]);

                            if (dictionatyResponsePGH != null && dictionatyResponsePGH.Count > 0)
                            {
                                if (dictionatyResponsePGH.ContainsKey("CardType"))
                                {
                                    paymentData += GetLocalResourceObject(resPayment + dictionatyResponsePGH["CardType"]) as string;
                                }

                                if (dictionatyResponsePGH.ContainsKey("CardNumber"))
                                {
                                    string cardNumber = dictionatyResponsePGH["CardNumber"];
                                    string maskedCC = cardNumber;

                                    if (cardNumber.Length > 4)
                                    {
                                        maskedCC = string.Format("{0}{1}", new string('*', cardNumber.Length - 5), cardNumber.Substring(cardNumber.Length - 4));
                                    }

                                    paymentData += string.Format("{0}{1}", htmlNewLine, maskedCC);
                                }
                            }
                        }
                    }
                    else
                    {
                        theOne = paymentGatewayLog.Find(i => i.Contains("QueryString: Agency:=VNPay"));
                        if (!string.IsNullOrEmpty(theOne))
                        {
                            paymentData = htmlNewLine;
                            paymentData += GetLocalResourceObject(resPayment + "DM") as string;
                        }
                    }

                    lblPGHPaymentMethodData.Text = paymentData;
                }
            }
            else
            {
                trPGHPaymentMethod.Visible = false;
                trAlternatePaymentMethod.Visible = true;
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