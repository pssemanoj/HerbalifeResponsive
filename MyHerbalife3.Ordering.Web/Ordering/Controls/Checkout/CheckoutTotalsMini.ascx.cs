
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using HL.Common.EventHandling;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using System.Linq;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class CheckoutTotalsMini : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && HLConfigManager.Configurations.PaymentsConfiguration.ShowBigGrandTotal)
            {
                lblGrandTotal.Attributes.Add("class", "Title");
                lblDisplayGrandTotal.Attributes.Add("class", "Title");
            }
            if (IsChina)
            {
                blErrors.Items.Clear();
            }

            MyHLShoppingCart shoppingCart = (Page as ProductsBase).ShoppingCart;
            OrderMonth.RegisterOrderMonthControls(pnlOrderMonth, lblOrderMonthChanged, ProductsBase.IsEventTicketMode,
                                                  pnlOrderMonthLabel);

            var control = loadPurchasingLimitsControl(false);
            if (control != null)
            {
                pnlPurchaseLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(control);
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges)
            {
                shoppingCart.Calculate();
            }
            BindTotals();
            if (trYourLevel != null) trYourLevel.Visible = false; // Hide row by default
            if (shoppingCart.OrderCategory == OrderCategoryType.ETO ||
                !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase)
            {
                if (shoppingCart.OrderCategory == OrderCategoryType.ETO ||
                    !HLConfigManager.Configurations.CheckoutConfiguration.HasSummaryEarnBase)
                    trEarnBase.Visible = false;
            }
            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasSubTotal)
            {
                divDistributorSubtotal.Visible = false;
            }
            if (HLConfigManager.Configurations.CheckoutConfiguration.HidePHShippingForETO)
            {
                if (SessionInfo.IsEventTicketMode)
                {
                    trPackingHandling.Visible = false;
                    if (HLConfigManager.Configurations.CheckoutConfiguration.ShowDistributorSubTotalForETO)
                    {
                        divDistributorSubtotal.Visible = true;
                    }
                    else
                    {
                        divDistributorSubtotal.Visible = false;
                    }
                    if (HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePoinsForETO)
                    {
                        trVolumePoints.Visible = true;
                    }
                    else
                    {
                        trVolumePoints.Visible = false;
                    }
                }
            }
            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasOrderMonthVolumePoints)
            {
                trOrderMonthVP.Visible = false;
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

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
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

                if (HLConfigManager.Configurations.CheckoutConfiguration.HideFreightCharges && ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
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

                if (trPackingHandling != null)
                {
                    trPackingHandling.Visible = false;
                }

                if (HLConfigManager.Configurations.CheckoutConfiguration.HasLogisticCharges)
                {
                    if (trLogisticCharge != null)
                        trLogisticCharge.Visible = true;
                }

                if (ShoppingCart.DeliveryInfo != null)
                {
                    if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
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

                if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                {
                    if ((ShoppingCart.PcLearningDeliveryoption == ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown &&
                        shoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping) || ShoppingCart.PcLearningDeliveryoption == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping)
                    {
                        trPCLearningPoint.Visible = true;
                        trPCLearningPointLimit.Visible = true;
                        GetEligibleUsePoint();
                    }
                    else
                    {
                        trPCLearningPoint.Visible = false;
                        trPCLearningPointLimit.Visible = false;
                    }
                }
                else if (ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder)
                {
                    trPCLearningPoint.Visible = true;
                    trPCLearningPointLimit.Visible = true;
                    GetEligibleUseETOPoints();
                }
                else
                {
                    trPCLearningPoint.Visible = false;
                    trPCLearningPointLimit.Visible = false;
                }

              

            }
            else
            {
                if (trDiscountTotal != null)
                {
                    trDiscountTotal.Visible = HLConfigManager.Configurations.CheckoutConfiguration.ShowDisocuntTotal;
                }
            }

            if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
            {
                trVolumePoints.Visible = false;
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

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (!HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPrice)
                {
                    if (trRetailPrice != null)
                        trRetailPrice.Visible = false;
                }
                else if (SessionInfo.IsEventTicketMode && trRetailPrice != null)
                {
                    trRetailPrice.Visible = HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPriceForETO;
                }
                trWeight.Visible = false;
            }
            else
            {
                trGrandTotal.Visible = false;
                trPackingHandling.Visible = false;
                lblDisplayPackageHandling.Visible = false;
                lblPackageHandling.Visible = false;
                trOtherCharges.Visible = false;
                trLogisticCharge.Visible = false;
                trShippingCharge.Visible = !string.IsNullOrEmpty(lblShippingCharges.Text) && HLConfigManager.Configurations.DOConfiguration.ShowFreightChrageonCOP1 
                    && shoppingCart.DeliveryInfo !=null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping;
                trTaxVat.Visible = false;
                trLocalTax.Visible = false;
                trPCLearningPoint.Visible = false;
                trPCLearningPointLimit.Visible = false;
            }

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                trOrderMonth.Visible = false;
                trOrderMonthVP.Visible = false;
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

        private void GetEligibleUsePoint()
        {
            string msg = (string) GetLocalResourceObject("EligiblePCLearningPoint.Text");
            decimal eligibleAmt=0 ;
            OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;

            Charge_V01 freightCharge =
                        totals.ChargeList.Find(
                            delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                        Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);

            decimal PCLearningPoint = ShoppingCart.PCLearningPoint;
            if (PCLearningPoint == 0)
                eligibleAmt = 0;
            else if(freightCharge.Amount >= PCLearningPoint )
                eligibleAmt = Math.Truncate(PCLearningPoint);
            else if (PCLearningPoint > freightCharge.Amount)
                eligibleAmt = Math.Truncate(freightCharge.Amount);

            lblEligiblePCLearningPoint.Text = string.Format(msg,PCLearningPoint.ToString(), eligibleAmt.ToString());
        }
       

        [Publishes(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        public event EventHandler OnCheckOutOptionsNotPopulated;

        [Publishes(MyHLEventTypes.QuoteError)]
        public event EventHandler OnQuoteError;

        #region SubscriptionEvents

        [SubscribesTo(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public void OnCartItemRemovedDueToSKULimitationRules(object sender, EventArgs e)
        {
            BindTotals();
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
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
            MyHLShoppingCart shoppingCart = (Page as ProductsBase).ShoppingCart;
            if (null != shoppingCart && null != shoppingCart.CartItems && shoppingCart.CartItems.Count > 0)
            {
                shoppingCart.Calculate();
            }
            BindTotals();
        }
        [SubscribesTo(MyHLEventTypes.ProceedingToCheckout)]
        public void OnProceedingToCheckout(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && DistributorOrderingProfile !=null && DistributorOrderingProfile.IsPC || (ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder))
            {
                try
                {

                    //if (!DistributorOrderingProfile.IsPC) return;
                    blErrors.Items.Clear();
                    blErrors.DataSource = null;
                    blErrors.DataBind();
                    var errors = blErrors.DataSource as List<string> ?? new List<string>();

                    OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;
                    Charge_V01 freightCharge =
                            totals.ChargeList.Find(
                                delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                            Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);

                    decimal inputLearnPoint;
                    bool rsl = decimal.TryParse(txtPCLearningPoint.Text, out inputLearnPoint);

                    decimal PCLearningPoint = ShoppingCart.OrderCategory == OrderCategoryType.ETO ? ETOLearningPoint : ShoppingCart.PCLearningPoint;
                    if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        if (inputLearnPoint != 0 && inputLearnPoint > PCLearningPoint)
                            errors.Add((string)GetLocalResourceObject("ETOLearningInputExceedAccumulated"));
                        else if (totals != null && inputLearnPoint != 0 &&  inputLearnPoint >= totals.AmountDue)
                            errors.Add((string)GetLocalResourceObject("ETOLearningInputExceedOrdertotal"));
                    }
                    else
                    {
                        if (inputLearnPoint != 0 && inputLearnPoint > PCLearningPoint)
                            errors.Add((string)GetLocalResourceObject("PCLearningInputExceedAccumulated.Text"));
                        //errors.Add("Offset PC Learning point exceeding accumulated learning point");
                        else if (totals != null && inputLearnPoint != 0 && (inputLearnPoint > freightCharge.Amount || inputLearnPoint > totals.AmountDue))
                            errors.Add((string)GetLocalResourceObject("PCLearningInputExceedFreight.Text"));
                        //errors.Add("PC Learning point exceeding freightcharge");
                    }

                    if (errors.Count > 0)
                    {
                        blErrors.DataSource = errors;
                        blErrors.DataBind();
                        OnCheckOutOptionsNotPopulated(this, null);
                        hasValidationErrors.Value = "true";
                    }
                    else
                    {
                        //totals.AmountDue -= inputLearnPoint;
                        this.ShoppingCart.pcLearningPointOffSet = inputLearnPoint;
                        hasValidationErrors.Value = "false";
                    }

                }
                catch (Exception ex)
                {
                            HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: onProceedingToCheckout error : " + ex);
                }
            }

        }
        #endregion

       
        #region HelperMethods

        private void BindTotals()
        {
            //init etosku
            InitEtoPanel();
            try
            {
                if (ShoppingCart != null)
                {
                    var lstShoppingCartItems = ShoppingCart.ShoppingCartItems;
                    if (lstShoppingCartItems.Count > 0 && ShoppingCart.Totals != null)
                    {
                        MyHLShoppingCart shoppingCart = ProductsBase.ShoppingCart;
                        OrderTotals_V01 totals = shoppingCart.Totals as OrderTotals_V01;
                        lblDiscountRate.Text =
                            HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePointRange ?
                            HLRulesManager.Manager.PerformDiscountRangeRules(shoppingCart, Locale, ProductsBase.DistributorDiscount) : (totals.DiscountPercentage).ToString() + "%";
                        trDiscountRate.Visible = !string.IsNullOrEmpty(lblDiscountRate.Text);

                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            if(ShoppingCart.OrderCategory == OrderCategoryType.ETO && SessionInfo != null && !SessionInfo.IsReplacedPcOrder)
                                GetEligibleUseETOPoints();
                            else
                                GetEligibleUsePoint();
                            if (shoppingCart != null && shoppingCart.pcLearningPointOffSet > 0)
                                txtPCLearningPoint.Text = shoppingCart.pcLearningPointOffSet.ToString();
                            else
                                txtPCLearningPoint.Text = "0";
                            // added for China DO
                            if (HLConfigManager.Configurations.CheckoutConfiguration.HasDiscountAmount)
                            {
                                OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                                if (totals_V02 != null)
                                {
                                    HLRulesManager.Manager.PerformDiscountRules(ShoppingCart, null, Locale,
                                                                                ShoppingCartRuleReason.CartCalculated);
                                    trDiscountTotal.Visible = totals_V02.DiscountAmount > 0;
                                    lblDiscountTotal.Text = getAmountString(totals_V02.DiscountAmount);
                                    trDiscountRate.Visible = false;
                                }
                            }

                            if ((HLConfigManager.Configurations.DOConfiguration.CalculateWithoutItems &&
                            ShoppingCart.Totals != null &&
                            (ShoppingCart.Totals as OrderTotals_V01).AmountDue != decimal.Zero) || (ShoppingCart.CartItems.Count > 0 && ShoppingCart.Totals != null))
                            {
                                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                                {
                                    decimal amountDue = OrderProvider.GetConvertedAmount(totals.AmountDue, CountryCode);
                                    if (HLConfigManager.Configurations.PaymentsConfiguration.RoundAmountDue == "Standard")
                                    {
                                        amountDue = Math.Round(amountDue);
                                    }
                                    if (DistributorOrderingProfile.IsPC)
                                    {
                                        if (HttpContext.Current.Request.UrlReferrer != null &&
                                            HttpContext.Current.Request.UrlReferrer.ToString().Contains("Checkout.aspx"))
                                        {
                                            totals.AmountDue = ShoppingCart.AmountduepriorpcLearningoffset;
                                            lblGrandTotal.Text =
                                                getAmountString(shoppingCart.AmountduepriorpcLearningoffset, true);
                                        }
                                        else
                                        {
                                            lblGrandTotal.Text = getAmountString(amountDue, true);
                                        }
                                    }
                                    else
                                    {
                                    lblGrandTotal.Text = getAmountString(amountDue, true);
                                }
                                   
                                }
                                else
                                {
                                    if (HLConfigManager.Configurations.PaymentsConfiguration.RoundAmountDue == "Standard")
                                    {
                                        totals.AmountDue = Math.Round(totals.AmountDue);
                                    }
                                    if (DistributorOrderingProfile.IsPC)
                                    {
                                        if (HttpContext.Current.Request.UrlReferrer != null &&
                                           HttpContext.Current.Request.UrlReferrer.ToString().Contains("Checkout.aspx"))
                                        {
                                            totals.AmountDue = ShoppingCart.AmountduepriorpcLearningoffset;
                                            lblGrandTotal.Text =   
                                             getAmountString(ShoppingCart.AmountduepriorpcLearningoffset );
                                        }
                                        else
                                        {
                                            lblGrandTotal.Text =
                                      getAmountString(ShoppingCart.Totals != null ? totals.AmountDue : (decimal)0.00);
                                        }
                                    }
                                    else
                                    {
                                    lblGrandTotal.Text =
                                        getAmountString(ShoppingCart.Totals != null ? totals.AmountDue : (decimal)0.00);
                                }
                                   
                                }
                            }
                            if (IsReturnFromCheckout && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                            {
                                lblGrandTotal.Text = getAmountString(totals != null ? totals.ItemsTotal : 0M);
                                lblDisplayPCLearningPoint.ForeColor = System.Drawing.Color.Green;
                                lblDisplayPCLearningPoint.Font.Bold = true;
                            }
                        }
                        else
                        {
                            if (lblDiscountTotal != null)
                            {
                                lblDiscountTotal.Text =
                                    getAmountString(CheckoutTotalsDetailed.GetDiscountTotal(lstShoppingCartItems));
                            }
                        }

                        lblEarnBase.Text = getAmountString(CheckoutTotalsDetailed.GetTotalEarnBase(lstShoppingCartItems));

                        decimal currentMonthVolume = 0.00M;
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

                        lblRetailPrice.Text = getAmountString(totals.ItemsTotal);
                        lblSubtotal.Text = getAmountString(totals.DiscountedItemsTotal);
                        lblVolumePoints.Text = ProductsBase.GetVolumePointsFormat(totals.VolumePoints);

                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            if (totals.ChargeList!=null)
                            {
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

                                //added for HR
                                if (HLConfigManager.Configurations.CheckoutConfiguration.MergePHAndShippingCharges)
                                {
                                    decimal phShippingCharges = pHCharge.Amount + freightCharge.Amount;
                                    lblPackageHandling.Text = getAmountString(phShippingCharges);
                                }
                            }
                            lblTaxVAT.Text = getAmountString(totals.TaxAmount);
                        }
                        else
                        {
                            lblRetailPrice.Text = getAmountString((ShoppingCart.Totals as OrderTotals_V01).ItemsTotal);
                            lblSubtotal.Text = getAmountString(OrderProvider.GetDistributorSubTotal(ShoppingCart.Totals as OrderTotals_V01));
                        }
                        if (HLConfigManager.Configurations.DOConfiguration.ShowFreightChrageonCOP1)
                        {
                            if (totals.ChargeList != null)
                            {
                                Charge_V01 freightCharge =
                               totals.ChargeList.Find(
                                   delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                               Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                                lblShippingCharges.Text = getAmountString(freightCharge.Amount);
                                trShippingCharge.Visible = !string.IsNullOrEmpty(lblShippingCharges.Text) && HLConfigManager.Configurations.DOConfiguration.ShowFreightChrageonCOP1
                                                    && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping;
                            }
                        }
                        if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayWeight && !SessionInfo.IsEventTicketMode && lblWeight != null)
                        {
                            lblWeight.Text = ShoppingCartProvider.GetWeight(ShoppingCart);
                        }
                    }
                    else
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            if ((ShoppingCart.Totals == null || (ShoppingCart.Totals as OrderTotals_V01).AmountDue == 0.0M) &&
                            !(ShoppingCart.OrderCategory == OrderCategoryType.ETO && HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket) &&
                            (ShoppingCart.ShoppingCartItems != null && ShoppingCart.ShoppingCartItems.Any()))
                            {
                                OnQuoteError(null, null);
                            }

                            if (trDonationTotal != null)
                            {
                                trDonationTotal.Visible = true;
                                OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                                if (totals_V02 != null)
                                {
                                    lblDonationTotal.Text = getAmountString(totals_V02.Donation);
                                }
                            }
                            //trDiscountRate.Visible = false;
                            lblGrandTotal.Text = getAmountString(ShoppingCart.Totals != null ? (ShoppingCart.Totals as OrderTotals_V01).AmountDue : (decimal)0.00);
                        }

                        DisplayEmptyTotals();
                    }
                    if (ShoppingCart.Totals != null && HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                    {
                        if (trDonationTotal != null)
                        {
                            trDonationTotal.Visible = true;
                            OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                            if (totals_V02 != null)
                            {
                                lblDonationTotal.Text = getAmountString(totals_V02.Donation);
                            }
                        }
                    }
                }
                else
                {
                    if (trDonationTotal != null)
                    {
                        trDonationTotal.Visible = true;
                        OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                        if (totals_V02 != null)
                        {
                            lblDonationTotal.Text = getAmountString(totals_V02.Donation);
                        }
                    }
                    lblGrandTotal.Text = getAmountString(ShoppingCart.Totals != null ? (ShoppingCart.Totals as OrderTotals_V01).AmountDue : (decimal)0.00);
                    DisplayEmptyTotals();
                }
            }
            catch (Exception ex)
            {
                //Log Exception
                LoggerHelper.Error(string.Format("Exception while displaying totals. Message: {0}, StackTrace: {1}, TargetSite:{2}, DistributorID: {3}", ex.Message, ex.StackTrace, ex.TargetSite, DistributorID));

                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    if (trDonationTotal != null)
                    {
                        trDonationTotal.Visible = true;
                        OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                        if (totals_V02 != null)
                        {
                            lblDonationTotal.Text = getAmountString(totals_V02.Donation);
                        }
                    }
                }

                DisplayEmptyTotals();
            }
        }

        private void DisplayEmptyTotals()
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
                if (totals_V02 != null)
                {
                    trDiscountTotal.Visible = totals_V02.DiscountAmount > 0;
                }
            }

            lblOrderMonthVolume.Text = ProductsBase.CurrentMonthVolume;
            lblDiscountRate.Text = ProductsBase.DistributorDiscount.ToString() + "%";
            lblVolumePoints.Text = GetVolumeString(0.00M);
            lblEarnBase.Text = getAmountString((decimal)0.00);
            lblRetailPrice.Text = getAmountString((decimal)0.00);
            lblSubtotal.Text = getAmountString((decimal)0.00);
            lblOtherCharges.Text = getAmountString((decimal)0.00);
            lblLocalTax.Text = getAmountString((decimal)0.00);
            lblPackageHandling.Text = getAmountString((decimal)0.00);
            lblShippingCharges.Text = getAmountString((decimal)0.00);
            lblTaxVAT.Text = getAmountString((decimal)0.00);

        }
        [SubscribesTo(MyHLEventTypes.OnStandAloneDonationClear)]
        public void OnStandAloneDonationClear(object sender, EventArgs e)
        {
            var cart = (Page as ProductsBase).ShoppingCart;
            if (null != cart)
            {
                if (null != cart.Totals)
                {
                    lblGrandTotal.Text = DisplayAsCurrency((cart.Totals as OrderTotals_V01).AmountDue, false);
                    lblDonationTotal.Text = DisplayAsCurrency((ShoppingCart.Totals as OrderTotals_V02).Donation, false);
                }
            }

        }
        protected string DisplayAsCurrency(decimal amount, bool showCurrencySymbol)
        {
            if (showCurrencySymbol)
            {
                return getAmountString(amount, true);
            }
            else
            {
                return string.Format("{0:F2}", amount);
            }
        }
        #endregion


        #region eto for user story 383200

        private bool IsReturnFromCheckout
        {
            get
            {
                if (this.Request.UrlReferrer != null &&
                    this.Request.UrlReferrer.AbsoluteUri.ToLower().IndexOf("checkout.aspx") > -1)
                {
                    return true;
                }
                return false;
            }
        }

        private void InitEtoPanel()
        {
            trPCLearningPointLimit.Visible = false;
            trPCLearningPoint.Visible = false;
        }

        private void GetEligibleUseETOPoints()
        {
            lblDisplayPCLearningPoint.Text = (string)GetLocalResourceObject("lblETOLearningPointTotal");
            if (ETOLearningPoint > 0)
            {
                var msg = GetLocalResourceObject("EligibleETOLearningPoint").ToString();
                var maxAmount = GetOrderMaxAmount();
                var convertibleFee = maxAmount < ETOLearningPoint ? maxAmount : ETOLearningPoint;

                lblEligiblePCLearningPoint.Text = string.Format(msg, ETOLearningPoint.ToString("0"), convertibleFee.ToString("0"));

                if (convertibleFee > 0)
                {
                    txtPCLearningPoint.Enabled = true;
                }
                trPCLearningPointLimit.Visible = true;
                trPCLearningPoint.Visible = true;

            }
            else
            {
                trPCLearningPointLimit.Visible = false;
                trPCLearningPoint.Visible = true;
                txtPCLearningPoint.Enabled = false;
            }
        }
        private string s_etoSku;

        public string EtoSku
        {
            get
            {
                if (String.IsNullOrEmpty(s_etoSku))
                {
                    if (ShoppingCart.OrderCategory == OrderCategoryType.ETO &&
                        ShoppingCart.CartItems != null &&
                        ShoppingCart.CartItems.Any())
                    {
                        try
                        {
                            s_etoSku = ShoppingCart.CartItems.First().SKU;
                        }
                        catch
                        {
                            s_etoSku = String.Empty;
                        }
                    }
                }

                return s_etoSku;
            }
        }

        private Nullable<decimal> s_etoLearningPoint = null;
        private decimal ETOLearningPoint
        {
            get
            {
                if (!String.IsNullOrEmpty(EtoSku))
                {
                    if (!s_etoLearningPoint.HasValue)
                    {
                        s_etoLearningPoint = Convert.ToDecimal(GetDistributorConvertibleFee(EtoSku, DistributorID));
                    }

                    return s_etoLearningPoint.Value;
                }

                return 0;
            }
        }

        //private decimal s_orderMaxAmount;
        private decimal GetOrderMaxAmount()
        {

            OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;
            var maxAmount = totals.ItemsTotal - 1;

            return maxAmount <= 0 ? 0 : maxAmount;

        }

        private double GetDistributorConvertibleFee(string sku, string distributorId)
        {
            return String.IsNullOrEmpty(sku) ? 0 : OrderProvider.GetDueConvertibleFee(sku, distributorId);
        }
        #endregion
    }
}
