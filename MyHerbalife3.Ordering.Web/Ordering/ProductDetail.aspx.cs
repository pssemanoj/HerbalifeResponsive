using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    /// <summary>
    ///     The product detail.
    /// </summary>
    public partial class ProductDetail : ProductsBase
    {
        /// <summary>
        ///     The las t_ see n_ produc t_ sessio n_ eky.
        /// </summary>
        public static string LAST_SEEN_PRODUCT_SESSION_EKY = "LASTSEENPRODUCT";

        /// <summary>
        ///     The las t_ see n_ cros s_ sel l_ produc t_ sessio n_ eky.
        /// </summary>
        public static string LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY = "LASTSEENCROSSSELLPRODUCT";

        /// <summary>
        ///     The page_ load.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string controlName = HLConfigManager.Configurations.DOConfiguration.ProductDetailCntrl;
            if (!string.IsNullOrEmpty(controlName))
            {
                var cntrl = LoadControl(controlName) as ProductDetailControlBase;

                if (cntrl != null)
                {
                    (Page.Master as OrderingMaster).EventBus.RegisterObject(cntrl);
                    cntrl.FromPopup = false;
                    DivProductDetail.Controls.Add(cntrl);
                }
            }

            if (ShoppingCart != null && ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("EventTickets.Title") as string);
                Page.Title = GetLocalResourceObject("EventTickets.Title") as string;
            }
            else
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                Page.Title = GetLocalResourceObject("PageResource1.Title") as string;
            }
            DisplayELearningMessage();
            if(!IsPostBack)
            {
                //For User Story 391426
                GetTWSKUQuantitytorestrict();
            }
        }

        protected ProductDetailControlBase getProductDetailControl(Control parent)
        {
            return parent.Controls.Count > 1 ? parent.Controls[1] as ProductDetailControlBase : null;
        }

        /// <summary>
        ///     The collect all cross sell products.
        /// </summary>
        /// <param name="current">
        ///     The current.
        /// </param>
        /// <param name="itemList">
        ///     The item list.
        /// </param>
        public static void CollectAllCrossSellProducts(
            Category_V02 current,
            List<CrossSellInfo> itemList)
        {
            if (current != null)
            {
                if (current.SubCategories != null)
                {
                    foreach (Category_V02 c in current.SubCategories)
                    {
                        CollectAllCrossSellProducts(c, itemList);
                    }
                }

                if (current.Products != null)
                {
                    foreach (ProductInfo_V02 d in current.Products)
                    {
                        if (d.CrossSellProducts == null)
                            continue;
                        foreach (int cat in d.CrossSellProducts.Keys)
                        {
                            itemList.AddRange(from a in d.CrossSellProducts[cat]
                                              select new CrossSellInfo(cat, a));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     The if out of stock.
        /// </summary>
        /// <param name="product">
        ///     The product.
        /// </param>
        /// <param name="allSKUs">
        ///     The all sk us.
        /// </param>
        /// <returns>
        ///     The if out of stock.
        /// </returns>
        public static bool IfOutOfStock(
            ProductInfo_V02 product,
            // cross sell prod id
            Dictionary<string, SKU_V01> allSKUs)
        {
            if (product != null && product.SKUs != null)
            {
                var skuList = from s in product.SKUs
                              from a in allSKUs.Keys
                              where
                                  s.SKU == a &&
                                  allSKUs[a].ProductAvailability != ProductAvailabilityType.Unavailable
                              select s;
                return skuList.Count() == 0;
            }

            return false;
        }

        /// <summary>
        ///     to check if cross sell product is in shopping cart
        /// </summary>
        /// <param name="shoppingCart">
        ///     The shopping Cart.
        /// </param>
        /// <param name="cs">
        ///     The cs.
        /// </param>
        /// <returns>
        ///     The if in shopping cart.
        /// </returns>
        private static bool ifInShoppingCart(MyHLShoppingCart shoppingCart, ProductInfo_V02 cs)
        {
            if (shoppingCart != null && shoppingCart.ShoppingCartItems != null)
            {
                // if already in shopping cart, return true
                var prods =
                    from s in shoppingCart.ShoppingCartItems
                    where s != null && s.ProdInfo != null && s.ProdInfo.ID == cs.ID
                    select s;

                if (prods.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     The should select this cross sell.
        /// </summary>
        /// <param name="cs">
        ///     The cs.
        /// </param>
        /// <param name="shoppingCart">
        ///     The shopping cart.
        /// </param>
        /// <param name="lastSeenProd">
        ///     The last seen prod.
        /// </param>
        /// <param name="lastSeenCrossSell">
        ///     The last seen cross sell.
        /// </param>
        /// <returns>
        ///     The should select this cross sell.
        /// </returns>
        public static bool ShouldSelectThisCrossSell(
            ProductInfo_V02 cs,
            MyHLShoppingCart shoppingCart,
            CrossSellInfo lastSeenProd,
            CrossSellInfo lastSeenCrossSell
            )
        {
            // if it is in shopping cart, do not pick
            if (ifInShoppingCart(shoppingCart, cs))
            {
                return false;
            }

            if (lastSeenProd != null && cs.ID == lastSeenProd.Product.ID)
            {
                return false;
            }

            if (lastSeenCrossSell != null && cs.ID == lastSeenCrossSell.Product.ID)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     The select cross sell from previous display or crss sell list.
        /// </summary>
        /// <param name="crossSellList">
        ///     The cross sell list.
        /// </param>
        /// <param name="lastSeenCrossSell">
        ///     The last seen cross sell.
        /// </param>
        /// <returns>
        /// </returns>
        public static CrossSellInfo SelectCrossSellFromPreviousDisplayOrCrssSellList(MyHLShoppingCart shoppingCart,
                                                                                     List<CrossSellInfo> crossSellList,
                                                                                     CrossSellInfo lastSeenCrossSell,
                                                                                     Dictionary<string, SKU_V01> allSKUs)
        {
            CrossSellInfo candidate = null;
            if (lastSeenCrossSell != null && lastSeenCrossSell.Product != null)
            {
                if (!ifInShoppingCart(shoppingCart, lastSeenCrossSell.Product) &&
                    !IfOutOfStock(lastSeenCrossSell.Product, allSKUs))
                    return candidate = lastSeenCrossSell;
            }

            if (crossSellList != null && crossSellList.Count > 0)
            {
                //return candidate = crossSellList.First();
                foreach (CrossSellInfo cs in crossSellList)
                {
                    if (!ifInShoppingCart(shoppingCart, cs.Product) && !IfOutOfStock(cs.Product, allSKUs))
                        return candidate = cs;
                }
            }

            return candidate;
        }

        /// <summary>
        ///     The get category from product.
        /// </summary>
        /// <param name="product">
        ///     The product.
        /// </param>
        /// <param name="productInfoCatalog">
        ///     The product info catalog.
        /// </param>
        /// <returns>
        /// </returns>
        public static Category_V02 GetCategoryFromProduct(ProductInfo_V02 product,
                                                          ProductInfoCatalog_V01 productInfoCatalog)
        {
            var varProd = from a in productInfoCatalog.AllCategories.Values
                          from p in a.Products
                          where p != null && p.ID == product.ID
                          select a;
            return varProd.Count() > 0 ? varProd.First() : null;
        }

        private void resetInventory()
        {
            var cntrl = getProductDetailControl(DivProductDetail);
            if (cntrl != null)
            {
                cntrl.ResetInventory();
            }
        }

        [SubscribesTo(MyHLEventTypes.ShowAllInventory)]
        public void OnShowAllInventory(object sender, EventArgs e)
        {
            var cntrl = getProductDetailControl(DivProductDetail);
            if (cntrl != null)
            {
                cntrl.ReloadProduct();
            }
        }

        [SubscribesTo(MyHLEventTypes.ShowAvailableInventory)]
        public void OnShowAvailableInventory(object sender, EventArgs e)
        {
            var cntrl = getProductDetailControl(DivProductDetail);
            if (cntrl != null)
            {
                cntrl.ReloadProduct();
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            resetInventory();
        }

        [SubscribesTo(MyHLEventTypes.ProductAvailabilityTypeChanged)]
        public void OnProductAvailabilityTypeChanged(object sender, EventArgs e)
        {
            resetInventory();
        }

        /// <summary>
        ///     The on shopping cart changed.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            if (SessionInfo.ShowAllInventory == false)
            {
                var cntrl = getProductDetailControl(DivProductDetail);
                if (cntrl != null)
                {
                    cntrl.ResetInventory();
                }
            }
        }
    }
}