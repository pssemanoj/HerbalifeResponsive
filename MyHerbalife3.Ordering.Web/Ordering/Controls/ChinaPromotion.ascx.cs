using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class ChinaPromotion : UserControlBase
    {
        [Publishes(MyHLEventTypes.ShoppingCartChanged)]
        public event EventHandler ShoppingCartChanged;

        private List<CatalogItem> ChinaPromoSkus;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                displayPromo();
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
            if ((promo = ChinaPromotionProvider.GetChinaPromotion(DistributorID)) != null)
            {
                var currestsession = SessionInfo.GetSessionInfo(DistributorID, "zh-CN");
                if (currestsession != null && currestsession.ChinaPromoSKUQuantity > 0)
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
                                    if (!ChinaPromotionProvider.GetPCPromoCode(t.SKU).Trim().Equals("DSChinaPromo")) // SKUs are same for both DSChinaPromo and PCChinaPromo
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
                            ChinaPromoSkus = PcPromoOnly;
                            if (rblFreeGiftlist.Items.Count > 0)
                            {
                                rblFreeGiftlist.Items[0].Selected = true;
                            }
                            var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                            var promoInCart =
                                myShoppingCart.CartItems.Select(c => c.SKU).Intersect(PcPromoOnly.Select(f => f.SKU));
                            var itemcount = promoInCart as string[] ?? promoInCart.ToArray();
                            var count = (from skucount in myShoppingCart.CartItems
                                         from cartitem in itemcount
                                         where cartitem.Trim().Equals(skucount.SKU.Trim())
                                         select skucount.Quantity).Sum();

                            if (count == currestsession.ChinaPromoSKUQuantity)
                            {
                                btnAddToCart.Enabled = false;
                                divPromo.Visible = false;
                            }
                            else if (count > currestsession.ChinaPromoSKUQuantity)
                            {
                                var itemsInBoth =
                                         myShoppingCart.CartItems.Where(x => x.IsPromo)
                                             .Select(c => c.SKU)
                                             .Intersect(ChinaPromoSkus.Select(f => f.SKU));
                                if (itemsInBoth.Any())
                                    myShoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                btnAddToCart.Enabled = true;
                                lblFreeGift.Text =
                                   string.Format(GetLocalResourceObject("lblFreeGiftResource1.Text") as string,
                                                 currestsession.ChinaPromoSKUQuantity);
                            }
                            else
                            {
                                btnAddToCart.Enabled = true;
                                lblFreeGift.Text =
                                   string.Format(GetLocalResourceObject("lblFreeGiftResource1.Text") as string,
                                                 currestsession.ChinaPromoSKUQuantity - count);
                            }
                           
                        }
                    }


                }
                promotionPanel.Update();
                }
                   
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            string SKU = rblFreeGiftlist.SelectedValue;
            ShoppingCart.AddItemsToCart(new List<ShoppingCartItem_V01>
                {
                    new ShoppingCartItem_V01 {SKU = SKU, Quantity = 1, IsPromo = true,}
                });
            ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
            var currestsession = SessionInfo.GetSessionInfo(DistributorID, "zh-CN");
            if (currestsession != null && currestsession.ChinaPromoSKUQuantity > 0)
            {
                var myShoppingCart = (Page as ProductsBase).ShoppingCart;
                var promoInCart =
                    myShoppingCart.CartItems.Select(c => c.SKU).Intersect(ChinaPromoSkus.Select(f => f.SKU));
                var count = (from skucount in myShoppingCart.CartItems
                             from cartitem in promoInCart
                             where cartitem.Trim().Equals(skucount.SKU.Trim())
                             select skucount.Quantity).Sum();
                if (promoInCart.Count() == currestsession.ChinaPromoSKUQuantity)
                {
                    btnAddToCart.Enabled = false;
                    divPromo.Visible = false;
                }
                else
                {
                    btnAddToCart.Enabled = true;
                }
            }

        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            displayPromo();
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            displayPromo();
        }
    }
}