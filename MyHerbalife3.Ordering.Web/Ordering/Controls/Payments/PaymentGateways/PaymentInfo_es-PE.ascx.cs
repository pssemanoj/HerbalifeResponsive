using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Payments;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_PE : PaymentGatewayControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            var tabs = TabControl as RadioButtonList;
            if (null != tabs && tabs.SelectedValue != "2")
            {
                return true;
            }


            return true;
        }

        public override HL.PGH.Api.PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;
                string payCode = ddlCards.SelectedValue;
                IssuerAssociationType ass = CreditCard.GetCardType(payCode);

                if (ass == IssuerAssociationType.MasterCard)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.MasterCard;
                }
                else if (ass == IssuerAssociationType.Visa)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.VisaCard;
                }
                return request;
            }
        }

        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            string payCode = ddlCards.SelectedValue;
            if (!string.IsNullOrEmpty(payCode))
            {
                payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
            }
            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }
    }
}