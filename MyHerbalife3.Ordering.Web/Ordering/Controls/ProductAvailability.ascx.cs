using System;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    /// <summary>
    ///     The product availability.
    /// </summary>
    public partial class ProductAvailability : UserControlBase
    {
        /// <summary>
        ///     Sets Available.
        /// </summary>
        public ProductAvailabilityType Available
        {
            set
            {
                divGreen.Visible = value == ProductAvailabilityType.Available;
                divRed.Visible = value == ProductAvailabilityType.Unavailable;
                divYellow.Visible = value == ProductAvailabilityType.AllowBackOrder;
                divBlue.Visible = value == ProductAvailabilityType.UnavailableInPrimaryWh;
            }
        }

        /// <summary>
        ///     Sets a value indicating whether ShowLabel.
        /// </summary>
        public bool ShowLabel
        {
            set
            {
                divGreen.Visible = divYellow.Visible = divRed.Visible = divBlue.Visible = true;
                uxGreenLabel.Visible = uxRedLabel.Visible = uxYellowLabel.Visible = uxBlueLabel.Visible = value;
                if (value)
                {
                    if (!HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder)
                    {
                        divYellow.Visible = uxYellowLabel.Visible = false;
                    }
                    else if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                             ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                             !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickup)
                    {
                        divYellow.Visible = uxYellowLabel.Visible = false;
                    }
                    else if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                             ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier &&
                             !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupFromCourier)
                    {
                        divYellow.Visible = uxYellowLabel.Visible = false;
                    }
                    if (ProductsBase.SessionInfo.ShowAllInventory == false)
                    {
                        divRed.Visible = uxRedLabel.Visible = false;
                    }
                    if (ProductsBase.SessionInfo.IsEventTicketMode)
                    {
                        divYellow.Visible = uxYellowLabel.Visible = false;
                    }
                    if (string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit))
                    {
                        divBlue.Visible = uxBlueLabel.Visible = false;
                    }
                    else if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                             ShoppingCart.DeliveryInfo.Option != DeliveryOptionType.Shipping)
                    {
                        divBlue.Visible = uxBlueLabel.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether hide control.
        /// </summary>
        public bool Hide
        {
            set
            {
                divGreen.Visible = divYellow.Visible = divRed.Visible = divBlue.Visible = !value;
                uxGreenLabel.Visible = uxRedLabel.Visible = uxYellowLabel.Visible = uxBlueLabel.Visible = !value;
            }
        }

        /// <summary>
        ///     The page_ load.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ShoppingCart != null && ShoppingCart.DeliveryInfo != null)
            {
                if (!HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder ||
                    ShoppingCart.OrderCategory == OrderCategoryType.ETO ||
                    (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup && !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickup) ||
                    (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier && !HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupFromCourier)
                    )
                {
                    divYellow.Visible = uxYellowLabel.Visible = false;
                }
            }

            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                uxRedLabel.Text = GetLocalResourceObject("EventUnavailableText").ToString();
            }

            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO ||
                string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.WhCodesForSplit) ||
                (ShoppingCart != null && ShoppingCart.DeliveryInfo != null &&
                     ShoppingCart.DeliveryInfo.Option != DeliveryOptionType.Shipping))
            {
                divBlue.Visible = uxBlueLabel.Visible = false;
            }
        }
    }
}