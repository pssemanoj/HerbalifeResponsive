using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using System.Web;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_zh_CN : PaymentGatewayControl
    {
        /// <summary>
        ///     Access global context
        /// </summary>
        public IGlobalContext GlobalContext
        {
            get { return HttpContext.Current.ApplicationInstance as IGlobalContext; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var banks = BankInfoProvider.GetAvailableBanks(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.BankUsage.UsedByGateway);

                if (HLConfigManager.Configurations.DOConfiguration.IsResponsive)
                {
                    ddlBanks.RepeatColumns = 3;
                }

                if (banks != null)
                {
                    foreach (var b in banks)
                    {
                        ddlBanks.Items.Add(new ListItem(
                                               "<img src='" +
                                               ResolveClientUrl("~/Ordering/Images/China/Banks/" + b.ImageName) +
                                               "' title='" + b.ImageName + "'>", b.BankCode));
                    }
                }
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

            return true;
        }

        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            if (payment != null)
            {
                var payCode = ddlBanks.SelectedValue;
                if (!string.IsNullOrEmpty(payCode))
                {
                    payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
                }
                //payment.TransactionType = ddlBanks.SelectedValue;
                payment.AuthorizationMerchantAccount = ddlBanks.SelectedValue;
                payment.TransactionType = "10";

                payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                payment.Address = new Address_V01();

                Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);
            }
            return payment;
        }

    }
}