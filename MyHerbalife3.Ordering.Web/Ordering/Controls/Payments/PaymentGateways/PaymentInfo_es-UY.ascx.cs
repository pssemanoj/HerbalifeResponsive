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
    public partial class PaymentInfo_es_UY : PaymentGatewayControl
    {
        #region Public Methods and Operators
       

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

        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //To do : enable once servipag works.
                // pnlPECards.Visible = false;

                // To hide or show Vpayment payment option          
                if (!HLConfigManager.Configurations.PaymentsConfiguration.HideVpayment)
                {
                    ddlCards.Items.Add(new ListItem("Visa", "VI"));
                }

                // To hide or show Oca payment option
                if (!HLConfigManager.Configurations.PaymentsConfiguration.HideOca)
                {
                    ddlCards.Items.Add(new ListItem("Oca", "OC"));
                }
            }
        }

        public override HL.PGH.Api.PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;
                string payCode = ddlCards.SelectedValue;
                IssuerAssociationType ass = CreditCard.GetCardType(payCode);

                if (ass == IssuerAssociationType.Visa)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.VisaCard;
                }
                else if (ass == IssuerAssociationType.Oca)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.Oca;
                }

                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var user = member.Value;
                MyHLShoppingCart myCart;
                            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(user.Id, TheBase.Locale);
                            myCart = sessionInfoMyCart.ShoppingCart;
                            if (myCart == null)
                                myCart = ShoppingCartProvider.GetShoppingCart(user.Id, TheBase.Locale);

                OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
                var hasDiscount = OrderProvider.HasVATDiscount(totals);
                request.TaxableAmountTotal = totals.TaxableAmountTotal;
                request.TaxAmount = totals.TaxAmount;
                request.HasDiscount = hasDiscount;
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
            var options = new PaymentOptions_V01();
            options.NumberOfInstallments = Int32.Parse(ddlInstallments.SelectedValue);
            payment.PaymentOptions = options;
            //payment.Card.IssuingBankID = _cards.Card.Find(c => c.CardId == ddlCards.SelectedValue).Id.ToString();

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }

        #endregion
    }
}