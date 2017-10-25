using System;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits : UserControlBase, IPurchasingLimits
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
        string countries = HL.Common.Configuration.Settings.GetRequiredAppSetting("CountriesforOrderSubTypeEmptyInOrderRequest", "EC");


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    trRemainingVal.Visible = false;
                }
                else
                {
                    DisplayRemaining();
                }
            }
            else if (ShoppingCart != null && string.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
            {

                ShoppingCart.SelectedDSSubType = !countries.Trim().Contains(Locale.Substring(3)) ? ProductsBase.LevelSubType : "";
            }
        }

        public static decimal TotalsExcludeAPF(MyHLShoppingCart cart, string countryCode)
        {
            var calcTheseItems = new List<ShoppingCartItem_V01>();
            calcTheseItems.AddRange(from i in cart.CartItems
                                    where !APFDueProvider.IsAPFSku(i.SKU)
                                    select
                                        new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                 i.MinQuantity));

            // remove A and L type
            var allItems =
                CatalogProvider.GetCatalogItems(
                    (from s in calcTheseItems where s.SKU != null select s.SKU).ToList(), countryCode);
            if (null != allItems && allItems.Count > 0)
            {
                var totals = cart.Calculate(calcTheseItems);
                return totals != null ? (totals as OrderTotals_V01).AmountDue : decimal.Zero;
            }

            return decimal.Zero;
        }

        private void DisplayRemaining()
        {
            string DSType = ProductsBase.LevelSubType;
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;

            PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, DSType);            
            ShoppingCart.SelectedDSSubType = !countries.Trim().Contains(Locale.Substring(3)) ? DSType : "";
            //var purchasingLimitManager = PurchasingLimitManager(this.DistributorID);
            //decimal cartVolume = (purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan) ? ShoppingCart.VolumeInCart : ShoppingCart.ProductVolumeInCart;
            decimal cartVolume = 0M;

            if (null != limits)            
            {
                if (FOPEnabled)
                {
                    if (limits.PurchaseLimitType == PurchaseLimitType.TotalPaid)
                    {
                        cartVolume = TotalsExcludeAPF(ShoppingCart, this.CountryCode);
                        lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingAmount").ToString();
                    }
                    else
                    {
                        cartVolume = (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits) ? ShoppingCart.ProductPromoVolumeInCart : ShoppingCart.VolumeInCart;
                    }
                }
                else
                {
                    var purchasingLimitManager = PurchasingLimitManager(this.DistributorID);
                    cartVolume = (purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan) ? ShoppingCart.VolumeInCart : ShoppingCart.ProductVolumeInCart;
                }
                decimal remaining = (limits.RemainingVolume - cartVolume);

                if(remaining < 0)
                {
                    remaining = 0;
                }

                if (limits.PurchaseLimitType == PurchaseLimitType.Volume)
                {
                    lblRemainingVal.Text = ProductsBase.GetVolumePointsFormat(remaining);
                }
                else
                {
                    lblRemainingVal.Text = ProductsBase.getAmountString(remaining);
                }
            }

            if (PurchasingLimitProvider.DisplayLimits(DistributorID, CountryCode))
            {
                trRemainingVal.Visible = null != limits &&
                                         (limits.PurchaseLimitType == PurchaseLimitType.Earnings ||
                                          limits.PurchaseLimitType == PurchaseLimitType.Volume ||
                                          limits.PurchaseLimitType == PurchaseLimitType.TotalPaid);
            }
            else
            {
                trRemainingVal.Visible = false;
            }

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
