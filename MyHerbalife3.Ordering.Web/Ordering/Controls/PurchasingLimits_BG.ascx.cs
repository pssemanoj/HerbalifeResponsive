using System;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using HL.Common.EventHandling;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.ViewModel;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PurchasingLimits_BG : UserControlBase, IPurchasingLimits
    {
        public enum OrderTypesBG
        {
            PC,
            CO
        }

        #region IPurchasingLimits Members

        public bool HideEmptyListItem { get; set; }

        public bool DisplayStatic { get; set; }

        #endregion

        [Publishes(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public event EventHandler OnCartItemRemovedDueToSKULimitationRules;

        [Publishes(MyHLEventTypes.OrderSubTypeChanged)]
        public event EventHandler OnOrderSubTypeChanged;

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            DisplayRemaining(ShoppingCart.SelectedDSSubType);
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            DisplayRemaining(ShoppingCart.SelectedDSSubType);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool apfIsDue = false;
                bool hasVPLimitations = true;

                //Company (Resale) not allowed when APF is due
                if (APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                {
                    apfIsDue = true;

                    //lbltext.Text = GetLocalResourceObject("CannotPlaceResaletWhileAPFDue") as string;
                    //lbltext.Visible = true;
                }
                else if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    trOrderType.Visible = false;
                    trRemainingVal.Visible = false;
                    ShoppingCart.SelectedDSSubType = OrderTypesBG.PC.ToString();
                    PurchasingLimitProvider.GetPurchasingLimits(DistributorID, "ETO");
                    return;
                }
                else
                {
                    hasVPLimitations = HasMemberVPLimitations();
                }

                //For PC Orders always Has Member VP Limitations
                ddl_OrderType.Items.Clear();
                ddl_OrderType.Enabled = false;

                ddl_OrderType.Items.Add(new ListItem(GetLocalResourceObject("TypeOrder_1") as string, OrderTypesBG.PC.ToString()));   //Personal Consumption

                if (!hasVPLimitations && !apfIsDue)
                {
                    ddl_OrderType.Items.Add(new ListItem(GetLocalResourceObject("TypeOrder_2") as string, OrderTypesBG.CO.ToString()));   //Company(Resale)
                    ddl_OrderType.Enabled = true;

                    //lbltext.Text = string.Empty;
                    //lbltext.Visible = false;
                }

                if (ShoppingCart != null)
                {
                    switch (ShoppingCart.SelectedDSSubType)
                    {
                        case "PC":
                        case "CO":
                            {
                                ddl_OrderType.SelectedIndex = ddl_OrderType.Items.IndexOf(ddl_OrderType.Items.FindByValue(ShoppingCart.SelectedDSSubType));
                                break;
                            }
                        default:
                            {
                                ShoppingCart.SelectedDSSubType = ddl_OrderType.SelectedItem.Value;
                                break;
                            }
                    }

                    lblOrderTypeVal.Text = ddl_OrderType.SelectedItem.ToString();
                }

                // Displaying remaining volume points
                //if (ShoppingCart.OrderCategory == OrderCategoryType.ETO || !hasVPLimitations)
                if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    trRemainingVal.Visible = false;
                }
                else
                {
                    DisplayRemaining(ddl_OrderType.SelectedItem.Value);
                }

                

                if (DisplayStatic)
                {
                    ddl_OrderType.Visible = false;
                    lblOrderTypeVal.Visible = true;
                }
            }
        }

        private void DisplayRemaining(string orderSubType)
        {
            //string DSType = ProductsBase.LevelSubType;
            ShoppingCart.EmailValues.RemainingVolume = string.Empty;
            //ShoppingCart.SelectedDSSubType = orderSubType;
                        
            PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, orderSubType);
            decimal cartVolume = 0M;

            if (null != limits)
            {
                if (FOPEnabled)
                {
                    if (limits.PurchaseLimitType == PurchaseLimitType.TotalPaid)
                    {
                        cartVolume = TotalsExcludeAPF(ShoppingCart, this.CountryCode);
                        lblRemainingValDisplay.Text = GetLocalResourceObject("RemainingAmount").ToString();
                    }
                    else
                    {
                        cartVolume = (limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits) ? ShoppingCart.ProductPromoVolumeInCart : ShoppingCart.VolumeInCart;
                    }
                }
                else
                {
                    var purchasingLimitManager = PurchasingLimitManager(this.DistributorID);
                    cartVolume = (purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan) ? ShoppingCart.VolumeInCart : ShoppingCart.ProductVolumeInCart;
                }
                decimal remaining = (limits.RemainingVolume - cartVolume);

                if (remaining < 0)
                {
                    remaining = 0;
                }

                if (limits.PurchaseLimitType == PurchaseLimitType.Volume)
                {
                    lblRemainingVal.Text = ProductsBase.GetVolumePointsFormat(remaining);
                }
                //else
                //{
                //    lblRemainingVal.Text = ProductsBase.getAmountString(remaining);
                //}
            }

            if (PurchasingLimitProvider.DisplayLimits(DistributorID, CountryCode))
            {
                if (HasMemberVPLimitations())
                {
                    trRemainingVal.Visible = null != limits ? true : false;
                    //trRemainingVal.Visible = null != limits &&
                    //                         (limits.PurchaseLimitType == PurchaseLimitType.Earnings ||
                    //                          limits.PurchaseLimitType == PurchaseLimitType.Volume ||
                    //                          limits.PurchaseLimitType == PurchaseLimitType.TotalPaid);
                    PurchaseLimitType limitType = PurchaseLimitType.Volume;
                    limits.PurchaseLimitType = limitType;
                }
                else
                {
                    if (limits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                    {
                        trRemainingVal.Visible = null != limits ? true : false; //Show remaining VP when FOP regardless of Limitations
                    }
                    else
                    {
                        trRemainingVal.Visible = false;
                    }
                }
            }
            else
            {
                trRemainingVal.Visible = false;
            }

            if (trRemainingVal.Visible)
            {
                ShoppingCart.EmailValues.RemainingVolume = lblRemainingVal.Text;
            }           
        }

        public static decimal TotalsExcludeAPF(MyHLShoppingCart cart, string countryCode)
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
                var totals = cart.Calculate(calcTheseItems);
                return totals != null ? (totals as OrderTotals_V01).AmountDue : decimal.Zero;
            }

            return decimal.Zero;
        }

        protected void ddl_OrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(ddl_OrderType.SelectedItem.Value))
            {
                OnOrderSubTypeChanged(sender,e);
                //bool refresh = false;
                PurchasingLimits_V01 limits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID, ddl_OrderType.SelectedItem.Value);

                ShoppingCart.SelectedDSSubType = ddl_OrderType.SelectedItem.Value;
                lblOrderTypeVal.Text = ddl_OrderType.SelectedItem.ToString();
                
                //If the order type is switched to Resale and DS has added APFs to cart, throw them out
                if (ddl_OrderType.SelectedItem.Value == OrderTypesBG.CO.ToString() &&
                    APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems) &&
                    !APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale))
                {
                    ShoppingCart.DeleteItemsFromCart(APFDueProvider.GetAPFSkuList());
                    // refresh = true;
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

                if (HasMemberVPLimitations())
                {
                    PurchaseLimitType limitType = PurchaseLimitType.Volume;
                    limits.PurchaseLimitType = limitType;
                    DisplayRemaining(ShoppingCart.SelectedDSSubType);
                }
                else
                {
                    PurchaseLimitType limitType = PurchaseLimitType.None;
                    limits.PurchaseLimitType = limitType;
                }
            }
            else
            {
                ShoppingCart.SelectedDSSubType = String.Empty;
                trRemainingVal.Visible = false;
                lblOrderTypeVal.Text = String.Empty;
            }
        }

        private bool HasMemberVPLimitations()
        {
            bool hasLimitations = true;

            var processingCountryCode = DistributorProfileModel.ProcessingCountryCode;
            if (processingCountryCode == "BG")
            {
                var now = DateUtils.GetCurrentLocalTime("BG");
                var tins = DistributorOrderingProfileProvider.GetTinList(DistributorProfileModel.Id, false);
                var bgTin = tins.Find(t => t.IDType.Key == "BGBL");

                if (bgTin != null && bgTin.IDType != null && bgTin.IDType.ExpirationDate > now && string.IsNullOrEmpty(ddl_OrderType.SelectedValue))
                {
                    hasLimitations = false;
                }
                else if (bgTin != null && bgTin.IDType != null && bgTin.IDType.ExpirationDate > now && ddl_OrderType.SelectedValue != "PC")
                {
                    hasLimitations = false;
                    trRemainingVal.Visible = false;
                }
            }

            return hasLimitations;
        }
    }
}