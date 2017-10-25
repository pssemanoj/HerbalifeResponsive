using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.NPSConfiguration;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using DateUtils = HL.Common.Utilities.DateUtils;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ViewModel.Model;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsDetailed : UserControlBase
    {
        protected MyHLShoppingCart _shoppingCart = null;

        [Publishes(MyHLEventTypes.PageVisitRefused)]
        public event EventHandler OnPageVisitRefused;

        protected void Page_Load(object sender, EventArgs e)
        {
            DoPageLoad();
        }

        protected virtual void DoPageLoad()
        {
            if (IsChina && (DistributorOrderingProfile.IsPC || (ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder)))
            {
                trPCLearningPoint.Visible = true;
                if(ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder)
                    lblPCLearningPoint.Text = (string)GetLocalResourceObject("lblETOLearningPointResource");
            }
            else if(trPCLearningPoint !=null)
            {
                trPCLearningPoint.Visible = false;
            }
            if (HLConfigManager.Configurations.PaymentsConfiguration.ShowBigGrandTotal)
            {
                lblGrandTotal.Attributes.Add("class", "Title");
                lblDisplayGrandTotal.Attributes.Add("class", "Title");
            }

            _shoppingCart = (Page as ProductsBase).ShoppingCart;
            BindTotals();
            if (trYourLevel != null) trYourLevel.Visible = false; // Hide row by default

            if ((HLConfigManager.Configurations.DOConfiguration.CalculateWithoutItems &&
                        _shoppingCart.Totals != null &&
                        (_shoppingCart.Totals as OrderTotals_V01).AmountDue != decimal.Zero) || (_shoppingCart.CartItems.Count > 0 && ShoppingCart.Totals != null))

           // if (_shoppingCart.CartItems.Count > 0 && ShoppingCart.Totals != null)
            {
                OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;

                if (HLConfigManager.Configurations.DOConfiguration.IsChina && !IsPostBack && Page.Request.Path.Contains("Checkout.aspx"))
                {
                    ShoppingCart.AmountduepriorpcLearningoffset = totals.AmountDue;
                    if (DistributorOrderingProfile.IsPC)
                        totals.AmountDue -= _shoppingCart.pcLearningPointOffSet;
                    else if (_shoppingCart != null && _shoppingCart.pcLearningPointOffSet > 0M && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                        totals.AmountDue -= _shoppingCart.pcLearningPointOffSet;

                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                {
                    decimal amountDue = OrderProvider.GetConvertedAmount(totals.AmountDue, CountryCode);
                    if (amountDue == 0.0M)
                    {
                        LoggerHelper.Error("Exception while getting the currency conversion - ");
                        DisplayEmptyLabels();
                        var eventArgs = new PageVisitRefusedEventArgs(Request.Url.PathAndQuery, String.Empty,
                                                                      PageVisitRefusedReason.UnableToPrice);
                        OnPageVisitRefused(this, eventArgs);
                        return;
                    }                    
                    if (HLConfigManager.Configurations.PaymentsConfiguration.RoundAmountDue == "Standard")
                    {
                        amountDue = Math.Round(amountDue);
                    }
                    lblGrandTotal.Text = getAmountString(amountDue, true);
                }
                else
                {
                    if (HLConfigManager.Configurations.PaymentsConfiguration.RoundAmountDue == "Standard")
                    {
                        totals.AmountDue = Math.Round(totals.AmountDue);
                    }
                    lblGrandTotal.Text = 
                        getAmountString(_shoppingCart.Totals != null ? totals.AmountDue : (decimal)0.00);
                }

                
            }

            if (_shoppingCart.OrderCategory == OrderCategoryType.ETO ||
                !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase)
            {
                if (_shoppingCart.OrderCategory == OrderCategoryType.ETO ||
                    !HLConfigManager.Configurations.CheckoutConfiguration.HasSummaryEarnBase)
                    trEarnBase.Visible = false;
            }

            //if (_shoppingCart.OrderCategory == HL.Common.ValueObjects.OrderCategoryType.ETO || HLConfigManager.Configurations.CheckoutConfiguration.HidePHShippingForETO)
            //{
            //    trPackageHandling.Visible = trShippingCharges.Visible = false;
            //}

            var control = loadPurchasingLimitsControl(true);
            if (control != null)
            {
                trLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(control);
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HasOtherCharges)
            {
                trOtherCharges.Visible = true;
            }

            // Conditioning tax vat row visibility.
            if (trTaxVat != null)
            {
                trTaxVat.Visible = HLConfigManager.Configurations.CheckoutConfiguration.HasTaxVat;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HasLocalTax)
            {
                trLocalTax.Visible = true;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HasDiscountAmount)
            {
                trDiscountAmount.Visible = true;
            }
            if (IsChina)
            {
                trWeight.Visible = false;
            }

            if (trAdditionalDiscount != null)
                trAdditionalDiscount.Visible = HLConfigManager.Configurations.CheckoutConfiguration.MergePHAndShippingCharges;
            if (trSubtotal != null)
                trSubtotal.Visible = HLConfigManager.Configurations.CheckoutConfiguration.MergePHAndShippingCharges;

            if (HLConfigManager.Configurations.CheckoutConfiguration.HidePHShippingForETO)
            {
                if (SessionInfo.IsEventTicketMode)
                {
                    trPackingHandling.Visible = false;
                    trShippingCharge.Visible = false;
                    if (!HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePoinsForETO)
                    {
                        trVolumePoints.Visible = false;
                    }
                    if (HLConfigManager.Configurations.CheckoutConfiguration.HasSubTotalOnTotalsDetailed)
                    {
                        trDistributorSubtotal.Visible = false;
                    }
                    else
                    {
                        trDistributorSubtotal.Visible = true;
                    }
                    
                }
            }

            if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                trVolumePoints.Visible = trOrderMonthVolume.Visible = false;
            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasOrderMonthVolumePoints)
            {
                trOrderMonthVolume.Visible = false;
            }
            if (HLConfigManager.Configurations.CheckoutConfiguration.HideFreightCharges && _shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                trShippingCharge.Visible = false;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HideShippingCharges)
            {
                trShippingCharge.Visible = false;
            }

            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPrice)
            {
                if (trRetailPrice != null)
                    trRetailPrice.Visible = false;
            }
            else if (SessionInfo.IsEventTicketMode && trRetailPrice != null)
            {
                trRetailPrice.Visible = HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPriceForETO;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HasLogisticCharges)
            {
                if (trLogisticCharge != null)
                    trLogisticCharge.Visible = true;
            }

            if (_shoppingCart.DeliveryInfo != null)
            {
                if (_shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    if (HLConfigManager.Configurations.CheckoutConfiguration.HasPickupCharge)
                    {
                        if (lblDisplayShippingCharges != null)
                            lblDisplayShippingCharges.Text = (string)GetLocalResourceObject("lblDisplayPickupCharges");
                    }
                    else
                    {
                        trShippingCharge.Visible = false;
                    }
                }
            }

            //New Code to hide/display the ?

            string levelDSType = (Page as ProductsBase).Level;
            if (levelDSType == "DS")
            {
                imgOrderMonth.Visible = true;
            }
            else
            {
                imgOrderMonth.Visible = false;
            }

            // Check for NPS updates.
            if (imgEarnBase != null)
            {
                imgEarnBase.Visible = ShowEarnBaseHelpInfo();
            }

            if (trDiscountTotal != null)
            {
                trDiscountTotal.Visible = HLConfigManager.Configurations.CheckoutConfiguration.ShowDisocuntTotal;
            }

            RemovePHCharge();

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                trOrderMonth.Visible = false;
                trOrderMonthVolume.Visible = false;
                if (ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
                {
                    trEarnBase.Visible = trDiscountRate.Visible = false;
                    if (trYourLevel != null) trYourLevel.Visible = true;
                    lblYourLevel.Text = GetGlobalResourceString(string.Format("DisplayLevel_{0}", ProductsBase.LevelSubType), defaultValue: string.Empty);
                }
            }

            if (trWeight != null)
            {
                trWeight.Visible = HLConfigManager.Configurations.CheckoutConfiguration.DisplayWeight && !SessionInfo.IsEventTicketMode;
            }

            if (HLConfigManager.Configurations.DOConfiguration.DisplayBifurcationKeys && 
                ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                lblDisplayVolumePoints.Text = (string)GetLocalResourceObject("lblDisplayVolumePointsResource1MB.Text");
            }
        }

        protected virtual void BindTotals()
        {
            List<DistributorShoppingCartItem> lstShoppingCartItems =
                (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
            try
            {
                if ((HLConfigManager.Configurations.DOConfiguration.CalculateWithoutItems &&
                        _shoppingCart.Totals != null &&
                        (_shoppingCart.Totals as OrderTotals_V01).AmountDue != decimal.Zero) || (lstShoppingCartItems.Count > 0 && ShoppingCart.Totals != null))
                {
                    OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;
                  
                    lblDiscountRate.Text = _shoppingCart.Totals == null
                                               ? "0%"
                                               : ((_shoppingCart.Totals as OrderTotals_V01).DiscountPercentage).ToString() + "%";
                    _shoppingCart.EmailValues.DistributorSubTotal = OrderProvider.GetDistributorSubTotal(_shoppingCart.Totals as OrderTotals_V01);
                    _shoppingCart.EmailValues.DistributorSubTotalFormatted = getAmountString(_shoppingCart.EmailValues.DistributorSubTotal);
                    lblDistributorSubtotal.Text = _shoppingCart.EmailValues.DistributorSubTotalFormatted;
                    lblEarnBase.Text = getAmountString(GetTotalEarnBase(lstShoppingCartItems));
                    if (lblDiscountTotal != null)
                    {
                        lblDiscountTotal.Text =
                            getAmountString(CheckoutTotalsDetailed.GetDiscountTotal(lstShoppingCartItems));
                    }
                    if (IsChina && (DistributorOrderingProfile.IsPC || (ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder)))
                    {
                    lblPCLearningPointAmount.Text = getAmountString(_shoppingCart.pcLearningPointOffSet);
                    }
                    // added for China DO
                    if (HLConfigManager.Configurations.CheckoutConfiguration.HasDiscountAmount)
                    {
                        OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                        if (totals_V02 != null)
                        {
                            HLRulesManager.Manager.PerformDiscountRules(_shoppingCart, null, Locale,
                                                                        ShoppingCartRuleReason.CartCalculated);
                            lblDiscountAmount.Text = getAmountString(totals_V02.DiscountAmount);
                            trDiscountRate.Visible = false;
                        }
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                    {
                        if (trDonationAmount != null)
                        {
                            trDonationAmount.Visible = true;
                            OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                            if (totals_V02 != null)
                            {
                                lblDonationAmount.Text = getAmountString(totals_V02.Donation);
                            }
                        }
                    }
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
                    if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayFormatNeedsDecimal)
                    {
                        lblOrderMonthVolume.Text = decimal.TryParse(ProductsBase.CurrentMonthVolume, NumberStyles.Any, CultureInfo.InstalledUICulture, out currentMonthVolume)
                            ? ProductsBase.GetVolumePointsFormat(currentMonthVolume) :
                            (Page as ProductsBase).CurrentMonthVolume;
                    }
                    else
                    {
                        lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
                    }

                    Charge_V01 otherCharges =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.OTHER; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.OTHER, (decimal)0.0);
                    Charge_V01 pHCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.PH; }) as Charge_V01 ??
                        new Charge_V01(ChargeTypes.PH, (decimal)0.0);
                    Charge_V01 freightCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                    Charge_V01 localTaxCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOCALTAX; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.LOCALTAX, (decimal)0.0);
                    lblOtherCharges.Text = getAmountString(otherCharges.Amount);
                    lblLocalTax.Text = getAmountString(localTaxCharge.Amount);
                    Charge_V01 logisticCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOGISTICS_CHARGE; })
                        as Charge_V01 ?? new Charge_V01(ChargeTypes.LOGISTICS_CHARGE, (decimal)0.0);
                    lblLogisticCharges.Text = getAmountString(logisticCharge.Amount);
                    lblPackageHandling.Text = getAmountString(pHCharge.Amount);
                    lblShippingCharges.Text = getAmountString(freightCharge.Amount);
                    lblRetailPrice.Text = getAmountString(totals.ItemsTotal);
                    lblTaxVAT.Text = getAmountString(totals.TaxAmount);
                    if (lblSubtotal != null)
                        lblSubtotal.Text = getAmountString(logisticCharge.Amount);
                    if (lblAdditionalDiscount != null)
                        lblAdditionalDiscount.Text = getAmountString(otherCharges.Amount);
                    lblVolumePoints.Text = ProductsBase.GetVolumePointsFormat(totals.VolumePoints);

                    //added for HR
                    if (HLConfigManager.Configurations.CheckoutConfiguration.MergePHAndShippingCharges)
                    {
                        decimal phShippingCharges = pHCharge.Amount + freightCharge.Amount;
                        lblPackageHandling.Text = getAmountString(phShippingCharges);
                    }

                    if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayWeight && !SessionInfo.IsEventTicketMode && lblWeight != null)
                    {
                        lblWeight.Text = ShoppingCartProvider.GetWeight(ShoppingCart);
                    }
                }
                else
                {
                    if (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                    {
                        if (trDonationAmount != null)
                        {
                            trDonationAmount.Visible = true;
                            OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                            if (totals_V02 != null)
                            {
                                lblDonationAmount.Text = getAmountString(totals_V02.Donation);
                            }
                        }
                    }

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

        /// <summary>
        /// Method that invokes the GetOrderMonthString logic in OrderMonth.
        /// Method is not removed because it is referenced by all the sub classes.
        /// </summary>
        /// <returns></returns>
        protected string GetOrderMonthString()
        {
            return new OrderMonth(CountryCode).GetOrderMonthString(this.SessionInfo);
        }

        protected virtual void DisplayEmptyLabels()
        {
            lblDiscountRate.Text = "0.00";
            lblDistributorSubtotal.Text =
                lblEarnBase.Text =
                lblGrandTotal.Text = getAmountString((decimal)0.00);
            lblOrderMonth.Text = getOrderMonth();
            lblOrderMonthVolume.Text = GetVolumeString(0.00M);
            //lblOrderType.Text = getOrderType();
            lblOtherCharges.Text = getAmountString((decimal)0.00);
            lblLocalTax.Text = getAmountString((decimal)0.00);
            lblPackageHandling.Text = getAmountString((decimal)0.00);
            //lblRemainingVolume.Text = "0.00";
            lblRetailPrice.Text = getAmountString((decimal)0.00);
            lblShippingCharges.Text = getAmountString((decimal)0.00);
            lblTaxVAT.Text = getAmountString((decimal)0.00);
            lblVolumePoints.Text = GetVolumeString(0.00M);
        }

        //private string getOrderType()
        //{
        //    return string.Empty;
        //}

        private string getOrderMonth()
        {
            return string.Empty;
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void OnOrderSubTypeChanged(object sender, EventArgs e)
        {
            BindTotals();
        }

        /// <summary>
        /// Removes the PH charge.
        /// </summary>
        public void RemovePHCharge()
        {
            var config = (new NPSConfigurationProvider()).GetNPSConfigSection(this.CountryCode);
            if (config == null || config.HasPHCharge)
            {
                return;
            }

            var startDate = config.RemovePHChargeStartDate;
            if (string.IsNullOrEmpty(startDate))
            {
                return;
            }

            const string format = "yyyy-MM-dd";
            DateTime startDateTime;

            if (!DateTime.TryParseExact(startDate, format, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out startDateTime))
            {
                return;
            }

            if (DateUtils.GetCurrentLocalTime(CountryCode) <= startDateTime)
            {
                return;
            }

            if (trPackingHandling != null)
            {
                trPackingHandling.Visible = false;
            }
        }

        public static bool ShowEarnBaseHelpInfo()
        {
            var countryCode = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(3);
            var config = (new NPSConfigurationProvider()).GetNPSConfigSection(countryCode);

            if (config == null)
            {
                return false;
            }

            var startDate = config.ShowEarnbaseHelpDate;
            if (string.IsNullOrEmpty(startDate))
            {
                return false;
            }

            const string format = "yyyy-MM-dd";
            DateTime startDateTime;

            if (!DateTime.TryParseExact(startDate, format, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out startDateTime))
            {
                return false;
            }

            return DateUtils.GetCurrentLocalTime(countryCode) > startDateTime;
        }

        /// <summary>
        /// Gets the total earn base.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static decimal GetTotalEarnBase(List<DistributorShoppingCartItem> items)
        {
            // If total earn base should only consider product type P.
            var countryCode = CultureInfo.CurrentCulture.Name.Substring(3);
            var config = (new NPSConfigurationProvider()).GetNPSConfigSection(countryCode);

            if (config != null && config.TotalEarnBasePrdctTypeOnly)
            {
                var startDate = config.TotalEarnBasePrdctTypeOnlyDate;
                if (!string.IsNullOrEmpty(startDate))
                {
                    const string format = "yyyy-MM-dd";

                    DateTime startDateTime;

                    if (DateTime.TryParseExact(startDate, format, CultureInfo.InvariantCulture,
                                               DateTimeStyles.None, out startDateTime))
                    {
                        if (DateUtils.GetCurrentLocalTime(countryCode).Date >= startDateTime.Date)
                        {
                            return
                                items.Where(
                                    shoppingCartItem => shoppingCartItem.CatalogItem != null && shoppingCartItem.CatalogItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                                     .Sum(shoppingCartItem => shoppingCartItem.EarnBase);
                        }
                    }
                }
            }

            return items.Sum(shoppingCartItem => shoppingCartItem.EarnBase);
        }

        /// <summary>
        /// Gets the discount total.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static decimal GetDiscountTotal(List<DistributorShoppingCartItem> items)
        {
            if (items == null)
            {
                return 0;
            }

            return
                items.Where(
                    shoppingCartItem => shoppingCartItem != null)
                     .Sum(shoppingCartItem => shoppingCartItem.DiscountPrice);
        }
    }
}
