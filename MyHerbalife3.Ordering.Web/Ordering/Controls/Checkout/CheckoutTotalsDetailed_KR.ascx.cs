using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsDetailed_KR : CheckoutTotalsDetailed
    {
        protected override void DoPageLoad()
        {
            _shoppingCart = (Page as ProductsBase).ShoppingCart;
            var control = loadPurchasingLimitsControl(true);
            if (control != null)
            {
                trLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(control);
            }
            BindTotals();
            RemovePHCharge();
        }

        protected override void BindTotals()
        {
            List<DistributorShoppingCartItem> lstShoppingCartItems =
                (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
            try
            {
                if (lstShoppingCartItems.Count > 0 && ShoppingCart.Totals != null)
                {
                    OrderTotals_V01 totals = _shoppingCart.Totals as OrderTotals_V01;
                    lblDiscountRate.Text = _shoppingCart.Totals == null
                                               ? "0%"
                                               : (totals.DiscountPercentage).ToString() + "%";
                    lblGrandTotal.Text = getAmountString(totals.AmountDue);
                    lblOrderMonth.Text = GetOrderMonthString();
                    lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
                    lblRetailPrice.Text = getAmountString(GetTotalEarnBase(lstShoppingCartItems)); //getAmountString(totals.ItemsTotal);
                    lblVolumePoints.Text = totals.VolumePoints.ToString("N2");
                }
                else
                {
                    DisplayEmptyLabels();
                }
            }
            catch (Exception ex)
            {
                //Log Exception
                LoggerHelper.Error("Exception while displaying totals - " + ex);
                DisplayEmptyLabels();
            }
        }

        protected override void DisplayEmptyLabels()
        {
            lblDiscountRate.Text = "0";
            lblGrandTotal.Text = getAmountString((decimal) 0.00);
            lblOrderMonth.Text = getOrderMonth();
            lblOrderMonthVolume.Text = "0";
            //lblOrderType.Text = getOrderType();
            //lblRemainingVolume.Text = "0.00";
            lblRetailPrice.Text = getAmountString((decimal) 0.00);
            lblVolumePoints.Text = "0.00";
        }

        private string getOrderType()
        {
            return string.Empty;
        }

        private string getOrderMonth()
        {
            return string.Empty;
        }
    }
}