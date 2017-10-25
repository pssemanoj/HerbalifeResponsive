using System.Web;
using System.Web.UI.WebControls;
using System.Web.Security;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using HL.PGH.Api;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_en_IN : PaymentGatewayControl
    { 
        #region Public Methods and Operators

        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
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

        public override HL.PGH.Api.PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;
                request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.CreditCard;
                var payments = HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as CreditPayment_V01;
                if (payments.Card.IssuerAssociation == IssuerAssociationType.PaymentGateway)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.NetBanking;
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

        #endregion
    }
}