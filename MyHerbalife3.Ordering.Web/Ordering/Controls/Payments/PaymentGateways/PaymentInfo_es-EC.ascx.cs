using System;
using System.Web.UI.WebControls;
using System.Web.Security;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.PGH.Api;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_EC : PaymentGatewayControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // To hide or show Payclub payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HidePayclub)
            {
                ShowPayclub(false);
            }

            // To hide or show Produbanco payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideProdubanco)
            {
                ShowProdubanco(false);
            }
        }

        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            var tabs = TabControl as RadioButtonList;
            if (null != tabs && tabs.SelectedValue != "2")
            {
                return true;
            }

            GetPaymentInfo();

            return true;
        }

        public void ShowPayclub(bool visible)
        {
            if (!visible)
            {
                ddlCards.Items.Remove(ddlCards.Items.FindByValue("DN"));
            }
        }

        public void ShowProdubanco(bool visible)
        {
            if (!visible)
            {
                ddlCards.Items.Remove(ddlCards.Items.FindByValue("VI"));
                ddlCards.Items.Remove(ddlCards.Items.FindByValue("MC"));
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
                if (card == IssuerAssociationType.Diners)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.Dinners;
                }                
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var user = member.Value;
                MyHLShoppingCart myCart;
                SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(user.Id, TheBase.Locale);
                myCart = sessionInfoMyCart.ShoppingCart;
                if (myCart == null)
                    myCart = ShoppingCartProvider.GetShoppingCart(user.Id, TheBase.Locale);

                OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
                request.TaxableAmountTotal = totals.TaxableAmountTotal;
                request.TaxAmount = totals.TaxAmount;
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
            payment.Address = new Address_V01();
            payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }
    }
}