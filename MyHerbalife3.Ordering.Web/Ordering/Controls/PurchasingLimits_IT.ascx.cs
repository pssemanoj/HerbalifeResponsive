using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits_IT : UserControlBase, IPurchasingLimits
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
            if (ShoppingCart.OrderCategory != OrderCategoryType.ETO)
                DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           DistributorOrderingProfile dsProfile  = DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode);
           string DSType = dsProfile.OrderSubType;
           

            //lblProductType.Visible = false;
            if (!IsPostBack)
            {
                if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    trOrderType.Visible = false;
                    trRemainingVal.Visible = false;
                    ShoppingCart.SelectedDSSubType = "D2";
                    PurchasingLimitProvider.GetPurchasingLimits(DistributorID, "ETO");
                    return;
                }
                else
                {
                    if (ShoppingCart.SelectedDSSubType == "ETO")
                    {
                        ShoppingCart.SelectedDSSubType = String.Empty;
                    }
                }

                //lblPickupLocation.Visible = ddlPickup.Visible = false;
                bool noConsignmentOrdersAllowed = false;
                if (DSType.Equals("A") || DSType.Equals("B"))
                {
                    if (APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                    {
                        noConsignmentOrdersAllowed = true;
                    }
                }

                //Begin HD Ticket 406707
                if (ShoppingCart.OrderSubType == "A2" || ShoppingCart.OrderSubType == "B2")
                {
                    bool isAPFDue = APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale) ||
                                    APFDueProvider.IsAPFDueWithinOneYear(DistributorID,CountryCode) ||
                                    APFDueProvider.IsAPFDueGreaterThanOneYear(DistributorID, CountryCode);
                    if (!isAPFDue)
                    {
                        //delete APF SKU from cart
                        ShoppingCart.DeleteItemsFromCart(APFDueProvider.GetAPFSkuList());
                    }
                }
                //End HD Ticket 406707

                if (noConsignmentOrdersAllowed)
                {
                    lbltext.Text = GetLocalResourceObject("CannotPlaceConsignmentWhileAPFDue") as string;
                    lbltext.Visible = true;
                }

                ddl_DSSubType.Enabled = false;
                //ddlDelivery.Enabled = false;
                trRemainingVal.Visible = false;

                switch (DSType)
                {
                    case "A":
                        ddl_DSSubType.Items.Clear();
                        if (!HideEmptyListItem)
                        {
                            ddl_DSSubType.Items.Add(string.Empty);
                        }
                        if (!noConsignmentOrdersAllowed)
                        {
                            ddl_DSSubType.Items.Add(new ListItem("Vendita a Cliente ", "A1")); //TODOD: Resx these
                        }
                        ddl_DSSubType.Items.Add(new ListItem("Uso Personale", "A2"));
                        ddl_DSSubType.Enabled = true;
                        if (!String.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
                        {
                            ddl_DSSubType.ClearSelection();
                            ListItem item = ddl_DSSubType.Items.FindByValue(ShoppingCart.SelectedDSSubType);
                            if (null != item)
                            {
                                item.Selected = true;
                                lblOrderTypeVal.Text = item.Text;
                            }
                            //lblOrderTypeVal.Text = ddl_DSSubType.Items.FindByValue(ShoppingCart.SelectedDSSubType).Text;
                            DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                        }
                        break;
                    case "B":
                        ddl_DSSubType.Items.Clear();
                        if (!HideEmptyListItem)
                        {
                            ddl_DSSubType.Items.Add(string.Empty);
                        }
                        if (!noConsignmentOrdersAllowed)
                        {
                            ddl_DSSubType.Items.Add(new ListItem("Vendita a Cliente", "B1"));
                            DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                        }
                        ddl_DSSubType.Items.Add(new ListItem("Uso Personale", "B2"));
                        ddl_DSSubType.Enabled = true;
                        if (!String.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
                        {
                            ddl_DSSubType.ClearSelection();
                            ListItem item = ddl_DSSubType.Items.FindByValue(ShoppingCart.SelectedDSSubType);
                            if (null != item)
                            {
                                item.Selected = true;
                                lblOrderTypeVal.Text = item.Text;
                            }
                            DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                            trRemainingVal.Visible = true;
                        }
                        break;
                    case "C":
                    case "D":
                    case "D2":
                    case "E":
                        if (DSType == "D")
                        {
                            DSType = "D2";
                        }
                        lblOrderType.Visible = false;
                        lblOrderTypeVal.Text = "Uso Personale";
                        ddl_DSSubType.Visible = false;
                        ddl_DSSubType.Items.Clear();
                        ddl_DSSubType.Items.Add(new ListItem(DSType, DSType));
                        ddl_DSSubType.SelectedIndex = 0;
                        //ddl_DSSubType.Enabled = false;
                        //ddl_DSSubType.Items.Add(DSType);
                        //ddlDelivery.Enabled = true;
                        break;
                }

                if (ddl_DSSubType.SelectedItem != null)
                {
                    ShoppingCart.SelectedDSSubType = ddl_DSSubType.SelectedItem.Value;
                }

                if (DisplayStatic)
                {
                    if (DSType.Equals("A") || DSType.Equals("B"))
                    {
                        ddl_DSSubType.Visible = false;
                        lblOrderTypeVal.Visible = true;
                    }
                    else if (DSType.Equals("D") || DSType.Equals("D2") || DSType.Equals("E"))
                    {
                        DisplayRemainingValues(DSType);
                    }
                }
                else
                {
                    if (DSType.Equals("A") || DSType.Equals("B"))
                    {
                        ddl_DSSubType.Visible = true;
                        lblOrderTypeVal.Visible = false;
                    }
                    else if (DSType.Equals("D") || DSType.Equals("D2") || DSType.Equals("E"))
                    {
                        DisplayRemainingValues(DSType);
                    }
                }
                if (this.FOPEnabled)
                {
                    var limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, DSType);
                    if (limits != null && limits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                    {
                        DisplayRemainingValues(DSType);
                    }
                }
            }
        }

        protected void ddl_DSSubType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ddl_DSSubType.SelectedItem.Value))
            {
                bool refresh = false;

                ShoppingCart.SelectedDSSubType = ddl_DSSubType.SelectedItem.Value;
                PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID,
                                                                                          ddl_DSSubType.SelectedItem
                                                                                                       .Value);
                //If the order type is switched to Resale and DS has added APFs to cart, throw them out
                if (limits.PurchaseType == OrderPurchaseType.Consignment &&
                    APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems) &&
                    !APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                {
                    ShoppingCart.DeleteItemsFromCart(APFDueProvider.GetAPFSkuList());
                    refresh = true;
                }
                if (ShoppingCart.CartItems.Count > 0)
                {
                    List<ShoppingCartRuleResult> results = HLRulesManager.Manager.ProcessCart(ShoppingCart,
                                                                                              ShoppingCartRuleReason
                                                                                                  .CartCalculated);
                    if (results.Count > 0 && results.Any(r => r.Result == RulesResult.Failure))
                    {
                        foreach (ShoppingCartRuleResult result in results)
                        {
                            if (result.Result == RulesResult.Failure)
                            {
                                var args = new CartModifiedForSKULimitationsEventArgs(result.Messages[0]);
                                OnCartItemRemovedDueToSKULimitationRules(this, args);
                            }
                        }
                    }
                }
                DisplayRemainingValues(ddl_DSSubType.SelectedItem.Value, refresh);
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

        private void displayPurchasingLimits(string DSSubType, PurchasingLimits_V01 limits, bool Refresh)
        {
            //If A1 or B1 - Display earnings else display volume
            //TODO - look up values from resource
            decimal remaining = 0.0M;
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;
            if (DSSubType == "B1")
            {
                lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingEarnings") as string;
                if (ShoppingCart != null && limits != null)
                {
                    remaining = limits.RemainingEarnings - ShoppingCart.ProductEarningsInCart;
                    if (remaining < 0)
                    {
                        remaining = 0;
                    }

                    lblRemainingVal.Text = limits.PurchaseLimitType == PurchaseLimitType.Volume
                                               ? remaining.ToString("N2")
                                               : getAmountString(remaining);
                }
                OnOrderSubTypeChanged(this,
                                      new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, true, false, Refresh));
            }
            else if (DSSubType == "A1" || DSSubType == "C")
            {
                trRemainingVal.Visible = false;
                OnOrderSubTypeChanged(this,
                                      new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, true, false, Refresh));
            }
            else
            {
                lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingVolume") as string;
                if (ShoppingCart != null && limits != null)
                {
                    decimal cartVolume = ShoppingCart.VolumeInCart;

                    remaining = limits.RemainingVolume - cartVolume;
                    if (remaining < 0)
                    {
                        remaining = 0;
                    }
                    lblRemainingVal.Text = limits.PurchaseLimitType == PurchaseLimitType.Volume
                                               ? remaining.ToString("N2")
                                               : getAmountString(remaining);
                }
                OnOrderSubTypeChanged(this,
                                      new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, false, true, Refresh));
            }
            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
        }

        private void displayFOP(PurchasingLimits_V01 limits, bool Refresh)
        {
            decimal remaining = 0.0M;
            lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingVolume") as string;
            if (ShoppingCart != null && limits != null)
            {
                decimal cartVolume = ShoppingCart.VolumeInCart;

                remaining = limits.RemainingVolume - cartVolume;
                if (remaining < 0)
                {
                    remaining = 0;
                }
                lblRemainingVal.Text = limits.PurchaseLimitType == PurchaseLimitType.Volume
                                           ? remaining.ToString("N2")
                                           : getAmountString(remaining);
            }
            OnOrderSubTypeChanged(this,
                                  new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, false, true, Refresh));
            
            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
        }

        private void DisplayRemainingValues(string DSSubType, bool Refresh)
        {
            trRemainingVal.Visible = true;
            var limits = new PurchasingLimits_V01();
            if (string.IsNullOrEmpty(DSSubType))
            {
                trRemainingVal.Visible = false;
                return;
            }
            else
            {
                limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, DSSubType);
            }
            if (!this.FOPEnabled)
                displayPurchasingLimits(DSSubType, limits, Refresh);
            else
            {
                if (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits)
                {
                    displayPurchasingLimits(DSSubType, limits, Refresh);
                }
                else
                {
                    displayFOP(limits, Refresh);
                }
            }
        }

    }
}
