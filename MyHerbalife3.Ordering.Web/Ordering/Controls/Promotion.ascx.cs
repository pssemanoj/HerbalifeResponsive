using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using HL.Common.Configuration;
using HL.Common.Utilities;
using System.Globalization;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class Promotion : UserControlBase
    {
        [Publishes(MyHLEventTypes.ShoppingCartChanged)]
        public event EventHandler ShoppingCartChanged;

        private static string s_promotionItemDisplayFormat = "{0}    x  {1}";

        protected void Page_Load(object sender, EventArgs e)
        {


            if (!IsPostBack)
            {
                if (!SessionInfo.IsReplacedPcOrder)
                {
                    displaySRPromo();

                }
                else
                {
                    // RemoveSelectablePromoskus();
                    divPromo.Visible = false;
                }

                if (IsRuleTime())
                {
                    displayBadgePromo();
                }
            }

        }

        /// <summary>
        /// display Promo
        /// </summary>

        private void displayPromo()
        {
            divPromo.Visible = false;
            PromotionInformation promo;
            List<CatalogItem> PcPromoOnly = new List<CatalogItem>();
            Dictionary<string, SKU_V01> _AllSKUS = null;
            if (ShoppingCart != null && ShoppingCart.CartItems.Any())
            {
                if (ShoppingCart.DeliveryInfo == null || string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
                if (CatalogProvider.IsPreordering(ShoppingCart.CartItems, ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            var profile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, ShoppingCart.DeliveryInfo.Address.Address.Country);

            if ((promo =
                 ChinaPromotionProvider.GetPCPromotion(profile.CNCustomorProfileID.ToString(), ShoppingCart.DistributorID)) !=
                null)
            {
                var totals = ShoppingCart.Totals as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V02;
                decimal currentMonthTotalDue = 0;
                if (promo.MonthlyInfo != null)
                {
                    if (promo.MonthlyInfo.Count > 0)
                        currentMonthTotalDue = promo.MonthlyInfo[0].Amount;
                }
                if (totals != null && totals.OrderFreight != null && totals.AmountDue + currentMonthTotalDue - totals.OrderFreight.FreightCharge >= promo.promoelement.AmountMinInclude)
                {
                    if (promo.IsEligible)
                    {
                        if (promo.SKUList.Count > 0)
                        {
                            if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
                            {
                                if (rblFreeGiftlist.Items.Count > 0)
                                    rblFreeGiftlist.Items.Clear();
                                _AllSKUS = (ProductsBase).ProductInfoCatalog.AllSKUs;
                                SKU_V01 sku;
                                foreach (CatalogItem t in promo.SKUList)
                                {
                                    if (_AllSKUS.TryGetValue(t.SKU, out sku))
                                    {
                                        if (!ChinaPromotionProvider.GetPCPromoCode(t.SKU).Trim().Equals("PCPromo"))
                                        {

                                            continue;

                                        }
                                        if ((
                                        ShoppingCartProvider.CheckInventory(t as CatalogItem_V01, 1,
                                                                                ProductsBase.CurrentWarehouse) > 0 &&
                                            (CatalogProvider.GetProductAvailability(sku,
                                                                                    ProductsBase.CurrentWarehouse) == ProductAvailabilityType.Available)))
                                        {

                                            rblFreeGiftlist.Items.Add(new ListItem(t.Description,
                                                                                   t.SKU));

                                            divPromo.Visible = true;
                                            PcPromoOnly.Add(t);
                                        }
                                    }

                                }
                                if (rblFreeGiftlist.Items.Count > 0)
                                {
                                    rblFreeGiftlist.Items[0].Selected = true;
                                }
                                var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                                var promoInCart =
                                    myShoppingCart.CartItems.Select(c => c.SKU).Intersect(PcPromoOnly.Select(f => f.SKU));
                                if (promoInCart.Any())
                                {
                                    btnAddToCart.Enabled = false;
                                    divPromo.Visible = false;
                                }
                                else
                                {
                                    btnAddToCart.Enabled = true;
                                }
                                if (ShoppingCart.CartItems.Count == 1)
                                {
                                    if (promoInCart.Any())
                                    {
                                        ShoppingCart.DeleteItemsFromCart(promoInCart.ToList(), true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var itemsInBoth =
                                      ShoppingCart.CartItems.Where(x => x.IsPromo)
                                          .Select(c => c.SKU)
                                          .Intersect(promo.SKUList.Select(f => f.SKU));
                        if (itemsInBoth.Any())
                            ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                    }
                }
                else
                {
                    var itemsInBoth =
                                        ShoppingCart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(promo.SKUList.Select(f => f.SKU));
                    if (itemsInBoth.Any())
                        ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                }
            }
            promotionPanel.Update();
        }
        private void displaySRPromo()
        {
            divPromo.Visible = false;
            PromotionInformation promo;
            List<CatalogItem> SRPromoOnly = new List<CatalogItem>();
            Dictionary<string, SKU_V01> _AllSKUS = null;
            if (ShoppingCart != null && ShoppingCart.CartItems.Any())
            {
                if (ShoppingCart.DeliveryInfo == null || string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
                if (CatalogProvider.IsPreordering(ShoppingCart.CartItems, ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            var profile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, ShoppingCart.DeliveryInfo.Address.Address.Country);

            if (ChinaPromotionProvider.IsEligibleForSRPromotion(ShoppingCart, HLConfigManager.Platform))
            {
                var cacheKey = string.Format("GetSRPromoDetail_{0}", ShoppingCart.DistributorID);
                var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
                var skuList = results.Skus.Split(',').ToArray();
                if (results != null && !string.IsNullOrWhiteSpace(results.Skus))
                {
                    //if (promo.IsEligible)
                    //{

                    if (skuList.Count() > 0)
                    {
                        if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
                        {
                            if (rblFreeGiftlist.Items.Count > 0)
                                rblFreeGiftlist.Items.Clear();
                            _AllSKUS = (ProductsBase).ProductInfoCatalog.AllSKUs;
                            SKU_V01 sku;
                            foreach (var t in skuList)
                            {
                                if (_AllSKUS.TryGetValue(t, out sku))
                                {
                                    if ((
                                     ShoppingCartProvider.CheckInventory(sku.CatalogItem, 1,
                                                                             ProductsBase.CurrentWarehouse) > 0 &&
                                         (CatalogProvider.GetProductAvailability(sku,
                                                                                 ProductsBase.CurrentWarehouse) == ProductAvailabilityType.Available)))
                                    {

                                        rblFreeGiftlist.Items.Add(new ListItem(sku.Description,
                                                                               t));

                                        divPromo.Visible = true;
                                        SRPromoOnly.Add(sku.CatalogItem);
                                    }
                                }

                            }
                            if (rblFreeGiftlist.Items.Count > 0)
                            {
                                rblFreeGiftlist.Items[0].Selected = true;
                            }
                            var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                            var promoInCart =
                                myShoppingCart.CartItems.Select(c => c.SKU).Intersect(SRPromoOnly.Select(f => f.SKU));
                            if (promoInCart.Any())
                            {
                                btnAddToCart.Enabled = false;
                                divPromo.Visible = false;
                                // ShoppingCart.HastakenSrPromotion = true;
                            }
                            else
                            {
                                btnAddToCart.Enabled = true;
                                //  ShoppingCart.HastakenSrPromotion = false;
                                lblFreeGift.Text = GetLocalResourceObject("lblFreeGiftResource1.Text") as string;
                            }
                            if (ShoppingCart.CartItems.Count == 1)
                            {
                                if (promoInCart.Any())
                                {
                                    ShoppingCart.DeleteItemsFromCart(promoInCart.ToList(), true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var itemsInBoth =
                                  ShoppingCart.CartItems.Where(x => x.IsPromo)
                                      .Select(c => c.SKU)
                                      .Intersect(skuList, StringComparer.OrdinalIgnoreCase);
                    if (itemsInBoth.Any())
                        ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                }
                //}
                //else
                //{
                //    var itemsInBoth =
                //                        ShoppingCart.CartItems.Where(x => x.IsPromo)
                //                            .Select(c => c.SKU)
                //                            .Intersect(promo.SKUList.Select(f => f.SKU));
                //    if (itemsInBoth.Any())
                //        ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                //}
            }
            promotionPanel.Update();
        }
        private void displayBadgePromo()
        {
            divPromo.Visible = false;
            List<CatalogItem> badgePromoList = new List<CatalogItem>();
            Dictionary<string, SKU_V01> allsku = null;
            if (ShoppingCart != null && ShoppingCart.CartItems.Any())
            {
                if (ShoppingCart.DeliveryInfo == null || string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
                if (CatalogProvider.IsPreordering(ShoppingCart.CartItems, ShoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return;
                }
                if (!(ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.RSO))
                {
                    return;
                }
                if (APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
                {
                    return;
                }
            }
            else
            {
                divPromo.Visible = false;
                promotionPanel.Update();
                return;
            }

            //var profile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, ShoppingCart.DeliveryInfo.Address.Address.Country);
            var memberId = String.IsNullOrEmpty(ShoppingCart.SrPlacingForPcOriginalMemberId) ? 
                ShoppingCart.DistributorID : 
                ShoppingCart.SrPlacingForPcOriginalMemberId;

            if (ChinaPromotionProvider.IsEligibleForBadgePromotion(ShoppingCart, HLConfigManager.Platform, memberId))
            {
                var cacheKey = string.Format("GetBadgePromoDetail_{0}", memberId);
                var results = HttpRuntime.Cache[cacheKey] as GetBadgePinResponse_V01;

                if (results != null)
                {
                    var badgeList = results.BadgeDetails;
                    if (results != null && badgeList.Length > 0)
                    {

                        if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
                        {
                            if (cblFreeGiftlist.Items.Count > 0)
                                cblFreeGiftlist.Items.Clear();

                            allsku = ProductsBase.ProductInfoCatalog.AllSKUs;

                            foreach (var badge in badgeList)
                            {
                                if (allsku.ContainsKey(badge.BadgeCode))
                                {
                                    var sku = allsku[badge.BadgeCode];
                                    if (ShoppingCartProvider.CheckInventory(sku.CatalogItem, badge.Quantity, ProductsBase.CurrentWarehouse) > 0 &&
                                         CatalogProvider.GetProductAvailability(sku, ProductsBase.CurrentWarehouse) == ProductAvailabilityType.Available)
                                    {
                                        cblFreeGiftlist.Items.Add(
                                            new ListItem
                                            {
                                                Selected = true,
                                                Text = String.Format(s_promotionItemDisplayFormat, badge.BadegName, badge.Quantity),
                                                Value = sku.SKU
                                            });
                                        //divPromo.Visible = true;
                                        badgePromoList.Add(sku.CatalogItem);
                                    }
                                }

                            }

                            //var myShoppingCart = ProductsBase.ShoppingCart;

                            var promoInCart = ShoppingCart.CartItems.Select(c => c.SKU).Intersect(badgePromoList.Select(f => f.SKU));
                            if (promoInCart.Any())
                            {
                                foreach (var x in promoInCart)
                                {
                                    var foundItems = new List<ListItem>();
                                    foreach (ListItem item in cblFreeGiftlist.Items)
                                    {
                                        if (item.Value == x)
                                            foundItems.Add(item);
                                    }

                                    foundItems.ForEach(f => cblFreeGiftlist.Items.Remove(f));
                                }
                            }
                            else
                            {
                                btnAddToCart.Enabled = true;
                                //  ShoppingCart.HastakenSrPromotion = false;
                                lblFreeGift.Text = GetLocalResourceObject("lblFreeGiftResource1.Text") as string;
                            }
                            if (ShoppingCart.CartItems.Count >= 1)
                            {
                                var others = ShoppingCart.CartItems.Where(s => !s.IsPromo)
                                                            .Select(c => c.SKU)
                                                            .Except(APFDueProvider.GetAPFSkuList());
                                if (!others.Any())
                                {
                                    if (promoInCart.Any())
                                    {
                                        ShoppingCart.DeleteItemsFromCart(promoInCart.ToList(), true);
                                    }
                                }
                            }

                            divPromo.Visible = cblFreeGiftlist.Items.Count > 0;
                        }

                    }
                    else
                    {
                        var itemsInBoth = ShoppingCart.CartItems
                            .Where(x => x.IsPromo)
                            .Select(c => c.SKU)
                            .Intersect(badgeList.Select(b => b.BadgeCode), StringComparer.OrdinalIgnoreCase);

                        if (itemsInBoth.Any())
                            ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                    }
                }
            }
            promotionPanel.Update();
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            var memberId = String.IsNullOrEmpty(ShoppingCart.SrPlacingForPcOriginalMemberId) ?
                ShoppingCart.DistributorID :
                ShoppingCart.SrPlacingForPcOriginalMemberId;

            if (ChinaPromotionProvider.IsEligibleForBadgePromotion(ShoppingCart, HLConfigManager.Platform, memberId))
            {
                var cacheKey = string.Format("GetBadgePromoDetail_{0}", memberId);
                var results = HttpRuntime.Cache[cacheKey] as GetBadgePinResponse_V01;
                
                var selectItems = cblFreeGiftlist.Items.Cast<ListItem>().Where(x => x.Selected).ToList();

                if (!ShoppingCart.CartItems.Exists(x => selectItems.Exists(i => i.Value == x.SKU)))
                {
                    var itemsToAdd = new List<ShoppingCartItem_V01>();
                    foreach (var sitem in selectItems)
                    {
                        var findBadge = results.BadgeDetails.FirstOrDefault(s => s.BadgeCode == sitem.Value);
                        if (findBadge != null)
                        {
                            itemsToAdd.Add(new ShoppingCartItem_V01
                            {
                                Quantity = findBadge.Quantity,
                                SKU = findBadge.BadgeCode,
                                IsPromo = true,
                            });
                        }
                    }

                    ShoppingCart.AddItemsToCart(itemsToAdd, true);
                    ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
                }
                if (cblFreeGiftlist.Items.Count == 0)
                {
                    divPromo.Visible = false;
                    promotionPanel.Update();
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {

            if (!SessionInfo.IsReplacedPcOrder)
            {
                displaySRPromo();

            }
            else
            {
                // RemoveSelectablePromoskus();
                divPromo.Visible = false;
            }
            if (IsRuleTime())
            {
                displayBadgePromo();
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {

            if (!SessionInfo.IsReplacedPcOrder)
            {
                displaySRPromo();

            }
            else
            {
                //  RemoveSelectablePromoskus();
                divPromo.Visible = false;
            }
            if (IsRuleTime())
            {
                displayBadgePromo();
            }
        }
        private void RemoveSelectablePromoskus()
        {
            var SRPromoSku = Settings.GetRequiredAppSetting("ChinaSRPromo", string.Empty).Split('|');
            if (SRPromoSku.Count() > 0)
            {
                var itemsInBoth =
                                    ShoppingCart.CartItems.Where(x => x.IsPromo)
                                        .Select(c => c.SKU)
                                        .Intersect(SRPromoSku, StringComparer.OrdinalIgnoreCase);
                if (itemsInBoth.Any())
                    ShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
            }
        }
        private bool IsRuleTime()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate))
            {
                DateTime beginDate;
                DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate,
                                       "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out beginDate);

                if (DateUtils.GetCurrentLocalTime(ShoppingCart.CountryCode).Date < beginDate.Date)
                {
                    return false;
                }
            }
            else
            {

                return false;

            }

            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate))
            {
                DateTime endDate;
                DateTime.TryParseExact(HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate,
                                       "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate);

                if (DateUtils.GetCurrentLocalTime(ShoppingCart.CountryCode).Date > endDate.Date)
                {
                    return false;
                }
            }
            else
            {

                return false;

            }
            return true;
        }

    }
}