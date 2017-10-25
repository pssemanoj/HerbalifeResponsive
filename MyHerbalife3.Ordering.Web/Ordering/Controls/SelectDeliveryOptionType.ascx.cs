using System;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class SelectDeliveryOptionType : UserControlBase
    {
        [Publishes(MyHLEventTypes.DeliveryOptionTypeSelected)]
        public event EventHandler OnDeliveryOptionTypeSelected;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DeliveryType.SelectedIndex = 0;
            }
        }

        public void ResetSelection()
        {
            DeliveryType.SelectedIndex = 0;
        }

        public DeliveryOptionType DeliveryOption()
        {
            if (DeliveryType.SelectedIndex == 1)
                return DeliveryOptionType.Shipping;
            else if (DeliveryType.SelectedIndex == 2)
                return DeliveryOptionType.Pickup;
            return DeliveryOptionType.Unknown;
        }

        protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
            ViewState["DeliveryType"] = DeliveryOption();
            var ddl = sender as DropDownList;
            OnDeliveryOptionTypeSelected(this, new DeliveryOptionTypeEventArgs(DeliveryOption()));
            popup_SelectDeliveryOptionType.Hide();
        }
    }
}