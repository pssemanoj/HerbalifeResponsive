using System;
using System.Collections.Generic;
using HL.Common.EventHandling;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsDetailed_BR : CheckoutTotalsDetailed
    {
        [SubscribesTo(MyHLEventTypes.PaymentInfoChanged)]
        public void Update(object sender, EventArgs e)
        {
            BindTotals();
            //this.pnlTotalsUpdate.Update();
        }

        protected override void DoPageLoad()
        {
            string levelDSType = (this.Page as ProductsBase).Level;
            if (levelDSType == "DS")
            {
                imgOrderMonth.Visible = true;
            }
            else
            {
                imgOrderMonth.Visible = false;
            }
            var control = loadPurchasingLimitsControl(true);
            if (control != null)
            {
                trLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(control);
            }
            if (!IsPostBack)
            {
                if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
                {
                    divDiscountRate.Visible = false;
                    pnlForLiteratureRetail.Visible = true;
                    pnlForProductandPromoteRetail.Visible = false;
                }
                else
                {
                    pnlForLiteratureRetail.Visible = true;
                    pnlForProductandPromoteRetail.Visible = true;

                }
                BindTotals();
            }
            RemovePHCharge();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            (this.Page.Master as OrderingMaster).EventBus.RegisterObject(this);
        }

        protected override void BindTotals()
        {
            
            var lstShoppingCartItems = (this.Page as ProductsBase).ShoppingCart.ShoppingCartItems;
            _shoppingCart = (this.Page as ProductsBase).ShoppingCart;
            try
            {
                if (lstShoppingCartItems.Count > 0 && _shoppingCart.Totals != null)
                {
                    OrderTotals_V01 totals = _shoppingCart.Totals as OrderTotals_V01;
                    lblDiscountRate.Text = HLRulesManager.Manager.PerformDiscountRangeRules(_shoppingCart, Locale, ProductsBase.DistributorDiscount);
                    divDiscountRate.Visible = !string.IsNullOrEmpty(lblDiscountRate.Text);

                    if (ProductsBase.IsEventTicketMode)
                    {
                        var orderMonthShortString = string.Empty;
                        var ordermonth = OrderMonth.DualOrderMonthForEventTicket(true,out orderMonthShortString); // dual ordermonth should be desable for ETO
                        lblOrderMonth.Text =  string.IsNullOrWhiteSpace(ordermonth)?GetOrderMonthString():ordermonth;
                        var currentSession = SessionInfo;

                        if (null != currentSession && !string.IsNullOrWhiteSpace(orderMonthShortString))
                        {
                            currentSession.OrderMonthString = ordermonth;
                            currentSession.OrderMonthShortString = orderMonthShortString;
                        }
                    }
                    else
                    lblOrderMonth.Text = GetOrderMonthString();
                    lblOrderMonthVolume.Text = (this.Page as ProductsBase).CurrentMonthVolume;
                    lblVolumePoints.Text = totals.VolumePoints.ToString("N2");

                    Charge_V01 pHCharge = GetCharge(totals.ChargeList, ChargeTypes.PH);
                    lblPackageHandling.Text = getAmountString(pHCharge.Amount);
                    lblGrandTotal.Text = getAmountString(totals.AmountDue);

                    // Brasil exclusive labels.
                    lblDiscountedProductRetail.Text = getAmountString(totals.ProductRetailAmount);
                    lblDiscountedLiteratureRetail.Text = getAmountString(totals.LiteratureRetailAmount);
                    lblDiscountedPromoteRetail.Text = getAmountString(totals.PromotionRetailAmount);

                    lblRetailPrice.Text = getAmountString(totals.ItemsTotal);

                    Charge_V01 freightCharge = GetCharge(totals.ChargeList, ChargeTypes.FREIGHT);
                    lblFreightCharges.Text = getAmountString(freightCharge.Amount);
                    lblICMS.Text = getAmountString(totals.IcmsTax);
                    lblIPI.Text = getAmountString(totals.IpiTax);
                    //lbSubtotal.Text = getAmountString(freightCharge.Amount + pHCharge.Amount+_shoppingCart.Totals.ProductRetailAmount + _shoppingCart.Totals.LiteratureRetailAmount + _shoppingCart.Totals.PromotionRetailAmount);
                }
                else
                {
                    DisplayEmptyLabels();
                }
            }
            catch (Exception ex)
            {
                //Log Exception
                LoggerHelper.Error("Exception while displaying totals - " + ex.ToString());
                DisplayEmptyLabels();
            }
        }

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == type; }) as Charge_V01 ?? new Charge_V01(type, (decimal)0.0);
        }

        protected override void DisplayEmptyLabels()
        {
            string empty = getAmountString((decimal)0.00);
            lblDiscountRate.Text = "0.00";
            lblGrandTotal.Text = empty;
            lblOrderMonth.Text = string.Empty;
            lblOrderMonthVolume.Text = "0.00";
            lblPackageHandling.Text = empty;
            lblVolumePoints.Text = "0.00";
            lblDiscountedProductRetail.Text = empty;
            lblDiscountedLiteratureRetail.Text = empty;
            lblDiscountedPromoteRetail.Text = empty;

        }
    }
}