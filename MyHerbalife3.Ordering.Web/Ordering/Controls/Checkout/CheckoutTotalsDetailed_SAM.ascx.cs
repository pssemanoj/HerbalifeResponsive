using System;
using System.Collections.Generic;
using System.Globalization;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsDetailed_SAM : CheckoutTotalsDetailed
    {
        [SubscribesTo(MyHLEventTypes.PaymentInfoChanged)]
        public void Update(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasLocalTax)
            {
                trLocalTax.Visible = true;
            }
            BindTotals();
            //this.pnlTotalsUpdate.Update();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);

            if (Label2 != null)
            {
                Label2.Text = GetLocalResourceObject("EarnBaseInfo.Text") as string;
            }
        }

        protected override void BindTotals()
        {
            decimal earnBase = 0.00M;
            List<DistributorShoppingCartItem> lstShoppingCartItems =
                (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
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
                    lblDistributorSubtotal.Text = getAmountString(totals.DiscountedItemsTotal);
                    lblEarnBase.Text = getAmountString(GetTotalEarnBase(lstShoppingCartItems));
                    if (ProductsBase.IsEventTicketMode)
                    {
                        var orderMonthShortString = string.Empty;
                        var ordermonth = OrderMonth.DualOrderMonthForEventTicket(true, out orderMonthShortString); // dual ordermonth should be desable for ETO
                        lblOrderMonth.Text = string.IsNullOrWhiteSpace(ordermonth) ? GetOrderMonthString() : ordermonth;
                        var currentSession = SessionInfo;

                        if (null != currentSession && !string.IsNullOrWhiteSpace(orderMonthShortString))
                        {
                            currentSession.OrderMonthString = ordermonth;
                            currentSession.OrderMonthShortString = orderMonthShortString;
                        }
                    }
                    else
                        lblOrderMonth.Text = GetOrderMonthString();

                    decimal currentMonthVolume = 0;
                    if (decimal.TryParse((Page as ProductsBase).CurrentMonthVolume, out currentMonthVolume))
                    {
                        lblOrderMonthVolume.Text =
                            HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                                ? currentMonthVolume.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                                : currentMonthVolume.ToString("N2");
                    }
                    else
                    {
                        lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
                    }

                    Charge_V01 otherCharges =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.OTHER; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.OTHER, (decimal) 0.0);
                    Charge_V01 pHCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.PH; }) as Charge_V01 ??
                        new Charge_V01(ChargeTypes.PH, (decimal) 0.0);
                    Charge_V01 freightCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.FREIGHT; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal) 0.0);
                    lblOtherCharges.Text = getAmountString(otherCharges.Amount);
                    Charge_V01 localTaxCharge = OrderProvider.GetLocalTax(_shoppingCart.Totals as OrderTotals_V01);
                    lblLocalTax.Text = getAmountString(localTaxCharge.Amount);
                    lblPackageHandling.Text = getAmountString(pHCharge.Amount);
                    Charge_V01 logisticCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.LOGISTICS_CHARGE; })
                        as Charge_V01 ?? new Charge_V01(ChargeTypes.LOGISTICS_CHARGE, (decimal) 0.0);
                    lblLogisticCharges.Text = getAmountString(logisticCharge.Amount);
                    var amount = GetSubTotalValue(totals, CountryCode);
                    lblSubtotal.Text = getAmountString(amount);
                    lblRetailPrice.Text = getAmountString(totals.ItemsTotal);
                    lblShippingCharges.Text = getAmountString(freightCharge.Amount);
                    lblTaxVAT.Text = getAmountString(OrderProvider.GetTaxAmount(_shoppingCart.Totals as OrderTotals_V01));

                    Charge_V01 taxedNet =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.TAXEDNET; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.TAXEDNET, (decimal) 0.0);
                    lblTaxedNet.Text = getAmountString(taxedNet.Amount);


                    lblVolumePoints.Text = HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                                               ? totals.VolumePoints.ToString("N",
                                                                                            CultureInfo.GetCultureInfo(
                                                                                                "en-US"))
                                               : totals.VolumePoints.ToString("N2");

                    lblGrandTotal.Text = getAmountString(totals.AmountDue);
                }
                else
                {
                    DisplayEmptyLabels();
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.HasTotalTaxable)
                {
                    trTaxedNet.Visible = true;
                    trTax.Visible = false;
                }

                if (_shoppingCart.Totals != null)
                {
                    OrderTotals_V01 totals = _shoppingCart.Totals as OrderTotals_V01;
                    if (HLConfigManager.Configurations.CheckoutConfiguration.HasTaxPercentage)
                    {
                        trTaxPercentage.Visible = true;
                        //lblPercentage.Text = HLConfigManager.Configurations.CheckoutConfiguration.TaxPercentage + "%";
                        lblPercentage.Text = _shoppingCart.Totals != null
                                                 ? getAmountString(OrderProvider.GetTaxAmount(_shoppingCart.Totals as OrderTotals_V01))
                                                 : string.Empty;
                    }

                    if (HLConfigManager.Configurations.CheckoutConfiguration.HasTotalDiscount)
                    {
                        trTotalDiscount.Visible = true;
                        lblTotalDiscount2.Text = _shoppingCart.Totals != null
                                                     ? getAmountString(totals.TotalItemDiscount)
                                                     : string.Empty;
                    }
                    // hide this row if there is no local tax return
                    trLocalTax.Visible = OrderProvider.HasLocalTax(totals);
                }
            }
            catch (Exception ex)
            {
                //Log Exception
                LoggerHelper.Error("Exception while displaying totals - " + ex);
                DisplayEmptyLabels();
            }
        }


        public decimal GetSubTotalValue(OrderTotals_V01 totals, string countryCode)
        {
            switch (countryCode)
            {
                case "EC":
                    {

                        var pHCharge =
                            totals.ChargeList.Find(
                                delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.PH; }) as
                            Charge_V01 ?? new Charge_V01(ChargeTypes.PH, (decimal) 0.0);
                        var freightCharge =
                            totals.ChargeList.Find(
                                delegate(Charge p) { return ((Charge_V01) p).ChargeType == ChargeTypes.FREIGHT; }) as
                            Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal) 0.0);
                        //calculate the taxable skus
                        decimal taxAmountTotal = 0;

                        foreach (var item in totals.ItemTotalsList as List<ItemTotal>)
                        {
                            taxAmountTotal += (item as ItemTotal_V01).DiscountedPrice;
                        }
                        return (taxAmountTotal + pHCharge.Amount + freightCharge.Amount);

                    }
                    break;
                default:
                    return totals.TaxableAmountTotal;
                    break;
            }
        }

    }
}