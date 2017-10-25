using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsMini_BR : UserControlBase
    {
        private MyHLShoppingCart _shoppingCart;

        protected void Page_Load(object sender, EventArgs e)
        {
            OrderMonth.RegisterOrderMonthControls(pnlOrderMonth, lblOrderMonthChanged, ProductsBase.IsEventTicketMode,
                                                  pnlOrderMonthLabel);

            string levelDSType = (Page as ProductsBase).Level;
            if (levelDSType == "DS")
            {
                imgOrderMonth.Visible = true;
            }
            else
            {
                imgOrderMonth.Visible = false;
            }
            var control = loadPurchasingLimitsControl(false);
            if (control != null)
            {
                pnlPurchaseLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(control);
            }
            if (!IsPostBack)
            {
                if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
                {
                    divDiscountRate.Visible = false;
                }

                BindTotals();
            }
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

        #endregion SubscriptionEvents

        private void BindTotals()
        {
            var lstShoppingCartItems = (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
            _shoppingCart = (Page as ProductsBase).ShoppingCart;
            try
            {
                if (lstShoppingCartItems.Count > 0 && _shoppingCart.Totals != null)
                {
                    lblDiscountRate.Text = HLRulesManager.Manager.PerformDiscountRangeRules(_shoppingCart, Locale,
                                                                                            ProductsBase
                                                                                                .DistributorDiscount);
                    divDiscountRate.Visible = !string.IsNullOrEmpty(lblDiscountRate.Text);
                    lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;

                    lblVolumePoints.Text = (_shoppingCart.Totals as OrderTotals_V01).VolumePoints.ToString("N2");
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

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01) p).ChargeType == type; }) as Charge_V01 ??
                   new Charge_V01(type, (decimal) 0.0);
        }

        private void DisplayEmptyTotals()
        {
            string empty = getAmountString((decimal) 0.00);
            lblDiscountRate.Text = HLRulesManager.Manager.PerformDiscountRangeRules(_shoppingCart, Locale,
                                                                                    ProductsBase.DistributorDiscount);
            lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
            lblVolumePoints.Text = "0.00";
        }
    }
}