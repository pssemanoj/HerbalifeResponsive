using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_CL : PaymentGatewayControl
    {
        #region Public Methods and Operators

        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            //To do : uncomment the next line  once servipag works.
            string payCode = ddlCards.SelectedValue;

            if (!string.IsNullOrEmpty(payCode))
            {
                payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
            }

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
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

        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //To do : enable once servipag works.
                // pnlPECards.Visible = false;

                // To hide or show WebPay payment option          
                if (!HLConfigManager.Configurations.PaymentsConfiguration.HideWebPay)
                {
                    ddlCards.Items.Add(new ListItem("Visa", "VI"));
                    ddlCards.Items.Add(new ListItem("MasterCard", "MC"));
                    ddlCards.Items.Add(new ListItem("Amex", "AX"));
                    ddlCards.Items.Add(new ListItem("Debito", "DB"));
                }

                // To hide or show ServiPag payment option
                if (!HLConfigManager.Configurations.PaymentsConfiguration.HideCL_ServiPag)
                {
                    ddlCards.Items.Add(new ListItem("ServiPag", "IO"));
                }
            }
        }

        public override HL.PGH.Api.PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;
                string payCode = ddlCards.SelectedValue;
                request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.CreditCard;
                IssuerAssociationType card = CreditCard.GetCardType(payCode);
                if (card == IssuerAssociationType.PaymentGateway)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.Servipag;
                }
                return request;
            }
        }

        #endregion
    }
}