using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckOutTotalsMini_IT : UserControlBase
    {
        MyHLShoppingCart _shoppingCart = null;
        OnlineDistributor user = null;

        [Publishes(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        public event EventHandler OnCheckOutOptionsNotPopulated;


        protected void Page_Load(object sender, EventArgs e)
        {
             var PLControl = loadPurchasingLimitsControl(false) as PurchasingLimits_IT;
             if (PLControl != null)
             {
                 if (ShoppingCart.IsSavedCart || ShoppingCart.IsFromCopy)
                 {
                     PLControl.HideEmptyListItem = false;
                 }
                 pnlPurchaseLimits.Controls.Add(PLControl);
             }
            _shoppingCart = (Page as ProductsBase).ShoppingCart;
            //purchasingLimits = HL.MyHerbalife.Providers.PurchasingLimitProvider.GetPurchasingLimits(DistributorID, user.Value.DSSubType);
            OrderMonth.RegisterOrderMonthControls(pnlOrderMonth, lblOrderMonthChanged, ProductsBase.IsEventTicketMode,
                                                  pnlOrderMonthLabel);
            BindTotals();
            if (_shoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO ||
                !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase)
            {
                trEarnBase.Visible = false;
            }
            string levelDSType = (Page as ProductsBase).Level;
            if (levelDSType == "DS")
            {
                imgOrderMonth.Visible = true;
            }
            else
            {
                imgOrderMonth.Visible = false;
            }

            if (imgEarnBase != null)
            {
                imgEarnBase.Visible = CheckoutTotalsDetailed.ShowEarnBaseHelpInfo();
            }
        }

        private void BindTotals()
        {
            decimal earnBase = 0.00M;
            var lstShoppingCartItems = (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
            try
            {
                if (lstShoppingCartItems.Count > 0 && _shoppingCart.Totals != null)
                {
                    OrderTotals_V01 totals = _shoppingCart.Totals as OrderTotals_V01;
                    foreach (DistributorShoppingCartItem shoppingCartItem in lstShoppingCartItems)
                    {
                        earnBase += shoppingCartItem.EarnBase;
                    }

                    lblDiscountRate.Text = _shoppingCart.Totals == null
                                               ? "0%"
                                               : (totals.DiscountPercentage).ToString() + "%";
                    lblEarnBase.Text = getAmountString(CheckoutTotalsDetailed.GetTotalEarnBase(lstShoppingCartItems));

                    lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
                    lblRetailPrice.Text = getAmountString(totals.ItemsTotal);
                    lblSubtotal.Text = getAmountString(totals.DiscountedItemsTotal);
                    lblVolumePoints.Text = totals.VolumePoints.ToString("N2");
                }
                else
                {
                    DisplayEmptyTotals();
                }
            }
            catch (Exception ex)
            {
                //Log Exception
                LoggerHelper.Error("Exception while displaying totals - " + ex);
                DisplayEmptyTotals();
            }
        }

        private void DisplayEmptyTotals()
        {
            lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
            lblDiscountRate.Text = (Page as ProductsBase).DistributorDiscount.ToString() + "%";
            lblVolumePoints.Text = getAmountString((decimal) 0.00);
            lblEarnBase.Text = getAmountString((decimal) 0.00);
            lblRetailPrice.Text = getAmountString((decimal) 0.00);
            lblSubtotal.Text = getAmountString((decimal) 0.00);
        }

        #region SubscriptionEvents

        [SubscribesTo(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public void OnCartItemRemovedDueToSKULimitationRules(object sender, EventArgs e)
        {
            BindTotals();
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
            BindTotals();
        }

        [SubscribesTo(MyHLEventTypes.QuoteRetrieved)]
        public void OnQuoteRetrieved(object sender, EventArgs e)
        {
            BindTotals();
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            BindTotals();
        }

        [SubscribesTo(MyHLEventTypes.ProceedingToCheckout)]
        public void OnProceedingToCheckout(object sender, EventArgs e)
        {
            this.blErrors.Items.Clear();
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.PurchasingLimitsControl))
            {
                //if (PurchasingLimitProvider.RequirePurchasingLimits(ShoppingCart.DistributorID, this.CountryCode))
                //{

                    //Check Value Selected in Drop Down
                    if (String.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
                    {
                        blErrors.Items.Add(GetLocalResourceObject("OrderTypeNotSelected").ToString());
                        OnCheckOutOptionsNotPopulated(this, null);
                    }
                //}
            }
        }

        #endregion
    }
}