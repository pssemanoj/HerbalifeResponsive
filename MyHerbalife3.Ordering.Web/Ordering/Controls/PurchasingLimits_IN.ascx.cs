using System;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits_IN : UserControlBase, IPurchasingLimits
    {
        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            DisplayRemaining();
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            DisplayRemaining();
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    trRemainingVal.Visible = false;
                }
                else
                {
                    DisplayRemaining();
                }
            }
            else if (string.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
            {
                ShoppingCart.SelectedDSSubType = string.IsNullOrEmpty(DistributorOrderingProfile.OrderSubType) ? "NA" : DistributorOrderingProfile.OrderSubType;
            }
        }

        private void DisplayRemaining()
        {
            string DSType = DistributorOrderingProfile.OrderSubType == null ? string.Empty : DistributorOrderingProfile.OrderSubType;
            PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, DSType);
            ShoppingCart.SelectedDSSubType = string.IsNullOrEmpty(DSType) ? "NA" : DSType;
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;
            decimal cartVolume = ShoppingCart.VolumeInCart;
            if (null != limits)
            {
                decimal remaining = decimal.Zero;
                if (FOPEnabled)
                {
                    cartVolume = (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits) ? ShoppingCart.ProductPromoVolumeInCart : ShoppingCart.VolumeInCart;
                }
                remaining = (limits.RemainingVolume - cartVolume);

                if (remaining < 0)
                {
                    remaining = 0;
                }

                lblRemainingVal.Text = remaining.ToString("N2");
            }

            trRemainingVal.Visible = ((null != limits && limits.PurchaseLimitType == PurchaseLimitType.Volume)
                                     ||
                                     (null != limits &&
                                      PurchasingLimitManager(DistributorID).PurchasingLimitsRestriction ==
                                      PurchasingLimitRestrictionType.MarketingPlan));
            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
        }

        #region IPurchasingLimits Members

        public bool HideEmptyListItem { get; set; }

        public bool DisplayStatic { get; set; }

        #endregion
    }
}