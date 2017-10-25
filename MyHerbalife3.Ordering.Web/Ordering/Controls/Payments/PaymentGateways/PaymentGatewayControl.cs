using System.Web.UI;
using System.Web.UI.WebControls;
using HL.PGH.Api;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public abstract class PaymentGatewayControlBase : UserControl
    {
        public abstract PaymentRequest PaymentRequest { get; }
        public abstract Payment GetPaymentInfo();
        public abstract bool Validate(out string errorMessage);
        public object TabControl { get; set; }

        protected Payment GetBasePaymentInfo()
        {
            return OrderProvider.GetBasePayment((null != TabControl as RadioButtonList)
                ? (TabControl as RadioButtonList).SelectedItem.Text
                : string.Empty);
        }
    }
}