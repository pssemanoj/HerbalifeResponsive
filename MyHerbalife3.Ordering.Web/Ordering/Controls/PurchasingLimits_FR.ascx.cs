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

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits_FR : UserControlBase, IPurchasingLimits
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
            string DSType = DistributorOrderingProfile.OrderSubType == null ? string.Empty : DistributorOrderingProfile.OrderSubType;
            if (string.IsNullOrEmpty(DSType) || DSType.ToUpper() == "N")
            {
                DSType = "NA";
            }

            if (!IsPostBack)
            {
                ShoppingCart.SelectedDSSubType = string.IsNullOrEmpty(ShoppingCart.SelectedDSSubType) ? DSType.Equals("F") ? "PC" : "NA" : ShoppingCart.SelectedDSSubType; // initial value
                if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    trOrderType.Visible = false;
                    trRemainingVal.Visible = false;
                    //ShoppingCart.SelectedDSSubType = String.Empty;
                    ShoppingCart.SelectedDSSubType = DSType.Equals("F") ? "PC" : "NA";
                    PurchasingLimitProvider.GetPurchasingLimits(DistributorID, "ETO");
                    return;
                }

                bool noConsignmentOrdersAllowed = false;
                if (DSType.Equals("F"))
                {
                    if (APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                    {
                        noConsignmentOrdersAllowed = true;
                    }
                }

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
                    case "F":
                        {
                            ddl_DSSubType.Items.Clear();
                            if (!HideEmptyListItem)
                            {
                                ddl_DSSubType.Items.Add(string.Empty);
                            }
                            if (!noConsignmentOrdersAllowed)
                            {
                                ddl_DSSubType.Items.Add(new ListItem("Revente", "RE")); //TODOD: Resx these
                            }
                            ddl_DSSubType.Items.Add(new ListItem("Consommation Personnelle", "PC"));
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
                            }
                            break;
                        }
                    default:
                        {
                            ShoppingCart.SelectedDSSubType = DSType;
                            lblOrderType.Visible = false;
                            lblOrderTypeVal.Text = @"Consommation Personnelle";
                            ddl_DSSubType.Visible = false;
                            ddl_DSSubType.Items.Clear();
                            ddl_DSSubType.Items.Add(new ListItem(DSType, DSType));
                            ddl_DSSubType.SelectedIndex = 0;
                            break;
                        }
                }

                if (ddl_DSSubType.Items.Count > 0 && null != ddl_DSSubType.SelectedItem && ddl_DSSubType.Visible)
                {
                    ShoppingCart.SelectedDSSubType = ddl_DSSubType.SelectedItem.Value;
                }
                else
                {
                    ShoppingCart.SelectedDSSubType = "NA";
                }

                if (DisplayStatic)
                {
                    if (DSType.Equals("F"))
                    {
                        ddl_DSSubType.Visible = false;
                        lblOrderTypeVal.Visible = true;
                    }
                    else
                    {
                        DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                    }
                }
                else
                {
                    if (DSType.Equals("F"))
                    {
                        ddl_DSSubType.Visible = true;
                        lblOrderTypeVal.Visible = false;
                    }
                    else
                    {
                        DisplayRemainingValues(ShoppingCart.SelectedDSSubType);
                    }
                }
            }
        }

        protected void ddl_DSSubType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ddl_DSSubType.SelectedItem.Value))
            {
                bool refresh = false;
                PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID,
                                                                                          ddl_DSSubType.SelectedItem
                                                                                                       .Value);
                ShoppingCart.SelectedDSSubType = ddl_DSSubType.SelectedItem.Value;
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
        private void DisplayRemainingValuesFOP(string DSSubType, bool Refresh, PurchasingLimits_V01 limits)
        {
            trRemainingVal.Visible = true;
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;

            decimal cartVolume = ShoppingCart.VolumeInCart;

            if (null != limits)
            {
                decimal remaining = (limits.RemainingVolume - cartVolume);

                if (remaining < 0)
                {
                    remaining = 0;
                }
                lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingVolume").ToString();
                lblRemainingVal.Text = ProductsBase.GetVolumePointsFormat(remaining);
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
        }
        private void DisplayRemainingValues(string DSSubType, bool Refresh)
        {
            var limits = new PurchasingLimits_V01();
            trRemainingVal.Visible = true;
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
                if (limits != null && limits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                {
                    DisplayRemainingValuesFOP(DistributorID, Refresh, limits);
                    return;
                }
            }

            if (DSSubType == "RE")
            {
                trRemainingVal.Visible = false;
                OnOrderSubTypeChanged(this,
                                      new OrderSubTypeEventArgs(ddl_DSSubType.SelectedItem.Value, true, false, Refresh));
            }
            else if (DSSubType == "PC")
            {
                lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingVolume").ToString();
                if (ShoppingCart != null)
                {
                    decimal discountedRetail = (null != ShoppingCart.Totals)
                                                   ? ShoppingCart.ProductDiscountedRetailInCart
                                                   : 0.0M;
                    decimal remaining = limits.RemainingVolume - discountedRetail;
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
            else
            {
                trRemainingVal.Visible = false;
                OnOrderSubTypeChanged(this, new OrderSubTypeEventArgs("NA", true, false, Refresh));
            }
            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }
        }

        //[SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        //public void OnShoppingCartChanged(object sender, EventArgs e)
        //{
        //    trRemainingVal.Visible = true;
        //    PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(Global.CurrentDistributor.Value.ID, ddl_DSSubType.SelectedItem.Value);
        //    //If A1 or B1 - Display earnings else display volume
        //    //TODO - look up values from resource
        //    if (ddl_DSSubType.SelectedItem.Value == "A1" || ddl_DSSubType.SelectedItem.Value == "B1")
        //    {
        //        lblRemainingValDisplay.Text = "Remaining Earnings";
        //        lblRemainingVal.Text = limits.RemainingEarnings.ToString();
        //    }
        //    else
        //    {
        //        lblRemainingValDisplay.Text = "Remaining Volume";
        //        lblRemainingVal.Text = limits.RemainingVolume.ToString();
        //    }
        //}
    }
}