using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_PY : PaymentGatewayControl
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
                // To hide or show Brancard payment option          
                if (!HLConfigManager.Configurations.PaymentsConfiguration.HideBancard)
                {
                    ddlCards.Items.Add(new ListItem("Visa", "VI"));
                    ddlCards.Items.Add(new ListItem("MasterCard", "MC"));
                    ddlCards.Items.Add(new ListItem("Amex", "AX"));
                    ddlCards.Items.Add(new ListItem("Cabal", "CC"));
                    ddlCards.Items.Add(new ListItem("Panal", "CP"));
                }
            }
        }

        #endregion
    }
}