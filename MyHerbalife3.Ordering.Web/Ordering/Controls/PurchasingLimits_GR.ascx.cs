using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits_GR : UserControlBase, IPurchasingLimits
    {
        public bool HideEmptyListItem { get; set; }
        public bool DisplayStatic { get; set; }

        [Publishes(MyHLEventTypes.OrderSubTypeChanged)]
        public event EventHandler OnOrderSubTypeChanged;

        [Publishes(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public event EventHandler OnCartItemRemovedDueToSKULimitationRules;

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            if (ShoppingCart.OrderCategory != ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
            {
                DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            List<TaxIdentification> tinList = DistributorOrderingProfile.TinList;

            if (!IsPostBack)
            {
                if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    trOrderType.Visible = false;
                    trRemainingVal.Visible = false;
                    ShoppingCart.SelectedDSSubType = "NA";
                    PurchasingLimitProvider.GetPurchasingLimits(DistributorID, "ETO");
                    return;
                }

                if (tinList == null || tinList.Where(t => t.IDType.Key == "GRVT").Count() == 0)
                {
                    trOrderType.Visible = false;
                    trRemainingVal.Visible = true;
                    ShoppingCart.SelectedDSSubType = "PC";
                    DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                }
                else
                {
                    if (tinList.Where(t => t.IDType.Key == "GRVT" || t.IDType.Key == "GRTN" || t.IDType.Key == "GRSS" || t.IDType.Key == "GRBL").Count() == 4)
                    {
                        trRemainingVal.Visible = false;
                        trOrderType.Visible = true;
                    }
                    else
                    {
                        trRemainingVal.Visible = true;
                        trOrderType.Visible = true;
                    }

                    if (!String.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
                    {
                        ddl_DSSubType.ClearSelection();
                        ListItem item = ddl_DSSubType.Items.FindByValue(ShoppingCart.SelectedDSSubType);
                        if (null != item)
                        {
                            item.Selected = true;
                            lblOrderTypeVal.Text = item.Text;
                        }
                        else
                        {
                            if (ddl_DSSubType.Items.Count > 0)
                            {
                                item = ddl_DSSubType.Items[0];
                                item.Selected = true;
                                lblOrderTypeVal.Text = item.Text;
                                ShoppingCart.SelectedDSSubType = item.Value;
                            }
                        }
                        DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                    }
                    else
                    {
                        //User Story 102057: INC2676705: Default 'Προσωπικό όριο αγορών' (Personal limit) 
                        ListItem item = ddl_DSSubType.Items.FindByValue("PC");
                        item.Selected = true;
                        lblOrderTypeVal.Text = item.Text;
                        ShoppingCart.SelectedDSSubType = item.Value;                        
                        
                        ShoppingCart.Calculate();
                        OnOrderSubTypeChanged(this, new OrderSubTypeEventArgs(item.Value, true, false, true));
                    }

                    if (DisplayStatic)
                    {
                        ddl_DSSubType.Visible = false;
                        lblOrderTypeVal.Visible = true;
                    }
                    else
                    {
                        ddl_DSSubType.Visible = true;
                        lblOrderTypeVal.Visible = false;
                    }
                }
                lblMessageZeroPercent.Visible = false;
                if (ddl_DSSubType.Visible == true)
                {
                    if (ddl_DSSubType.SelectedValue != null && ddl_DSSubType.SelectedValue == "RE")
                    {
                        lblMessageZeroPercent.Visible = true;
                    }
                }
            }
        }

        protected void ddl_DSSubType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ddl_DSSubType.SelectedItem.Value))
            {
                bool refresh = false;
                PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, ddl_DSSubType.SelectedItem.Value);
                ShoppingCart.SelectedDSSubType = ddl_DSSubType.SelectedItem.Value;
                //If the order type is switched to Resale and DS has added APFs to cart, throw them out
                if (limits.PurchaseType == OrderPurchaseType.Consignment && APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems) && !APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                {
                    ShoppingCart.DeleteItemsFromCart(APFDueProvider.GetAPFSkuList());
                    refresh = true;
                }
                if (ShoppingCart.CartItems.Count > 0)
                {
                    List<ShoppingCartRuleResult> results = HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartCalculated);
                    if (results.Count > 0 && results.Any(r => r.Result == RulesResult.Failure))
                    {
                        foreach (ShoppingCartRuleResult result in results)
                        {
                            if (result.Result == RulesResult.Failure)
                            {
                                CartModifiedForSKULimitationsEventArgs args = new CartModifiedForSKULimitationsEventArgs(result.Messages[0]);
                                OnCartItemRemovedDueToSKULimitationRules(this, args);
                            }
                        }
                    }
                    ShoppingCart.Calculate();
                    OnOrderSubTypeChanged(this, new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, true, false, true));
                }
                DisplayRemainingValues(ddl_DSSubType.SelectedItem.Value, refresh);
                lblMessageZeroPercent.Visible = ShoppingCart.SelectedDSSubType == "RE";
            }
            else
            {
                ShoppingCart.SelectedDSSubType = String.Empty;
                trRemainingVal.Visible = false;
            }
        }
        private void DisplayRemainingValues(string DSSubType)
        {
            DisplayRemainingValues(DSSubType, false);
        }
        private void DisplayRemainingValues(string DSSubType, bool Refresh)
        {
            PurchasingLimits_V01 limits = new PurchasingLimits_V01();
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;
            
            if (string.IsNullOrEmpty(DSSubType))
            {
                limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
            }
            else
            {
                limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, DSSubType);
            }
            if (FOPEnabled)
            {
                trRemainingVal.Visible = limits.RemainingVolume > -1;
            }
            else
            {
                trRemainingVal.Visible = PurchasingLimitManager(ShoppingCart.DistributorID).PurchasingLimitsRestriction != PurchasingLimitRestrictionType.MarketingPlan;
            }

            if (DSSubType == "RE")
            {
                //trRemainingVal.Visible = false;
                OnOrderSubTypeChanged(this, new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, true, false, Refresh));
            }

            lblRemainingValDisplay.Text = this.GetLocalResourceObject("RemainingVolume").ToString();
            if (ShoppingCart != null && limits != null)
            {
                decimal totalDue = TotalsExcludeAPF(ShoppingCart, this.CountryCode);
                decimal remaining = decimal.Zero;
                if (FOPEnabled)
                {
                    decimal cartVolume = (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits) ? ShoppingCart.ProductPromoVolumeInCart : ShoppingCart.VolumeInCart;
                    remaining = (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits) ? limits.RemainingVolume - totalDue : limits.RemainingVolume - cartVolume;
                }
                else
                {
                    remaining = limits.RemainingVolume - totalDue;
                }
                if (remaining < 0)
                {
                    remaining = 0;
                }
                this.lblRemainingVal.Text = limits.PurchaseLimitType == PurchaseLimitType.Volume ? remaining.ToString("N2") : this.getAmountString(remaining);
                this.lblRemainingVal.Visible = limits.PurchaseLimitType != PurchaseLimitType.None;
            }
            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
            OnOrderSubTypeChanged(this, new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, false, true, Refresh));
        }

        public static decimal TotalsExcludeAPF(MyHLShoppingCart cart, string countryCode)
        {
            if (APFDueProvider.IsAPFSkuPresent(cart.CartItems))
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
                    var skuExcluded = (from c in allItems.Values
                                       where (c as CatalogItem_V01).ProductType != ServiceProvider.CatalogSvc.ProductType.Product
                                       select c.SKU);
                    calcTheseItems.RemoveAll(s => skuExcluded.Contains(s.SKU));
                }

                var totals = cart.Calculate(calcTheseItems);
                return totals !=null ? (totals as OrderTotals_V01).AmountDue : decimal.Zero;
            }
            return cart.Totals != null ? (cart.Totals as OrderTotals_V01).AmountDue : decimal.Zero;
        }
    }
}
