using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using Resources;
using System.Web.UI;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Web.Ordering.Helpers;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    /// <summary>
    ///     The price list.
    /// </summary>
    public partial class PriceListFavourite : ProductsBase
    {
        private static string SKUGridID = "Products";
        private static string SKUControlID = "ProductID";
        private static string QtyControlID = "QuantityBox";
        private const string CatalogVirtualPath = "~/Ordering/Catalog.aspx";

        //the cache keys for the product grid should be concatenated with the current warehouse and countrie
        public const string ResponsiveGridCaheKey = "ResponsiveGrid_";
        public const string GridCaheKey = "Grid_";
        public static int INVENTORY_CACHE_MINUTES = Settings.GetRequiredAppSetting("InventoryCacheExpireMinutes", 10);

        /// <summary>
        ///     The _allow decimal.
        /// </summary>
        private readonly bool _allowDecimal = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal;

        /// <summary>
        ///     The _errors.
        /// </summary>
        private readonly List<string> _friendlyMessages = new List<string>();

        /// <summary>
        ///     The _errors.
        /// </summary>
        private List<string> _errors = new List<string>();

        private int _productTableColumns = 8;

        /// <summary>
        ///     The _show all inventry.
        /// </summary>
        private bool _showAllInventry = true;

        public bool UpdateCalled { get; set; }

        /// <summary>
        ///     The cross sell found.
        /// </summary>
        [Publishes(MyHLEventTypes.CrossSellFound)]
        public event EventHandler CrossSellFound;

        /// <summary>
        ///     The no cross sell found.
        /// </summary>
        [Publishes(MyHLEventTypes.NoCrossSellFound)]
        public event EventHandler NoCrossSellFound;

        /// <summary>
        ///     launch product detail popup
        /// </summary>
        [Publishes(MyHLEventTypes.ProductDetailBeingLaunched)]
        public event EventHandler ProductDetailBeingLaunched;

        private List<FavouriteSKU> favourSKUs = new List<FavouriteSKU>();
        private FavouriteSkuLoader favorSKULoader = new FavouriteSkuLoader();
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
            favourSKUs = favorSKULoader.GetDistributorFavouriteSKU(ShoppingCart.DistributorID, "zh-CN");
            
            if (favourSKUs.Count < 1)
            {
                CheckoutButton.Enabled = false;
                AddToCart.Enabled = false;
            }


            if ((HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) || (HLConfigManager.Configurations.DOConfiguration.IsChina))
            {
                Subcategories.Visible = false;
                if (!string.IsNullOrEmpty(pricelistGrid.Value))
                    priceListHTMLGrid.Controls.Add(new Literal
                        {
                            Text = pricelistGrid.Value.Replace('[', '<').Replace(']', '>')
                        });
                dvSubcategories.Visible = false;

            }
            if (DisplayCCMessage && HLConfigManager.Configurations.DOConfiguration.AllowToDispalyCCMessage)
            {
                imgWarning.Visible = true;
                imgWarning.ImageUrl = "~/Ordering/Images/Icons/smallWarningIcon.png";
                lblCraditcard.Attributes["class"] = "cc-label";
                lblCraditcard.Text = Convert.ToString(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "DisplayCCMessage") ?? "You do not have a valid Credit Card on file.");
                lnkSavedCards.Text = Convert.ToString(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "DisplayCCLink") ?? " Click here to enter a new credit card or correct the existing ones");
            }
            CnpTable.Visible = false;
            if (HLConfigManager.Configurations.DOConfiguration.HasDetailLink &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.DetailLinkAddress))
            {
                CnpTable.Visible = true;
                CnpTable.NavigateUrl = HLConfigManager.Configurations.DOConfiguration.DetailLinkAddress;


            }
            if (!IsPostBack)
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var orders = OrdersProvider.GetOrdersInProcessing(DistributorID, Locale);
                    if (orders != null && orders.Any())
                    {
                        var orderNumber = orders.FirstOrDefault().OrderId;
                        ViewState["pendingOrderNumber"] = orderNumber;
                        var isOrderSubmitted = CheckPendingOrderStatus("CN_99BillPaymentGateway", orderNumber);
                        ViewState["isOrderSubmitted"] = isOrderSubmitted;
                        if (isOrderSubmitted)
                        {

                            lblDupeOrderMessage.Text =
                                string.Format(GetLocalResourceObject("PendingOrderSubmittedResource").ToString(),
                                              orderNumber);
                        }
                        else
                        {
                            lblDupeOrderMessage.Text = GetLocalResourceObject("PendingOrderResource.Text") as string;
                        }
                        dupeOrderPopupExtender.Show();
                    }
                    divChinaPCMessageBox.Visible = SessionInfo.IsReplacedPcOrder;
                }
            }
            var sessionInfo = SessionInfo;
            if (sessionInfo != null)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                _showAllInventry = SessionInfo.ShowAllInventory;
            }

            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                !sessionInfo.IsEventTicketMode)
            {
                thTitleEarnBase.Visible = true;
            }
            else
            {
                thTitleEarnBase.Visible = false;
                _productTableColumns--;
            }

            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.HasDiscountedPriceForEventTicket &&
                SessionInfo.IsEventTicketMode)
            {
                thTitleDiscountedPrice.Visible = false;
                _productTableColumns--;
            }
            else
            {
                thTitleDiscountedPrice.Visible =
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount;
                if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount == false)
                    _productTableColumns--;
            }

            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
            {
                thTitleRetailPrice.Visible = true;
            }
            else
            {
                thTitleRetailPrice.Visible = false;
                _productTableColumns--;
            }

            //decide
            bool showVolume = ShowVolume();
            thTitleVolumePoint.Visible = showVolume;
            if (!showVolume)
                _productTableColumns--;

            ProductAvailability.ShowLabel = true;
            if (IsChina)
            {
                ChinaProductAvailability.ShowLabel = true;  // !HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
            AddToCart.Text = CheckoutButton.Text =
                             AddToCartDisabled.Text =
                             CheckoutButtonDisabled.Text = GetLocalResourceObject("AddToCartResource1.Text") as string;

            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);

                CategoryDropdown.DataSource = from r in ProductInfoCatalog.RootCategories
                                              where shouldTake(r, sessionInfo.IsEventTicketMode) && CatalogProvider.IsDisplayable(r, Locale)
                                              select r;
                CategoryDropdown.DataBind();

                VolumePointText.Visible =
                    recalcVPSubmit.Visible = recalcVPSubmit2.Visible = SessionInfo.IsEventTicketMode == false;
                lbVolumePoint.Text = "0.00";
                VolumePointText.Visible = lbVolumePoint.Visible = HLConfigManager.Configurations.DOConfiguration.IsChina ? showVolume : !SessionInfo.IsEventTicketMode;
                //For China DO
                lbTotalAmount.Text = "0.00";
                lbTotalAmount.Visible = TotalAmountText.Visible = HLConfigManager.Configurations.DOConfiguration.IsChina ? showVolume : false;
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    lbTotalAmount.Visible = TotalAmountText.Visible = true;
                }
                DisplayAPFMessage();
            }

            if (ShoppingCart != null && ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
            {
                Page.Title = GetLocalResourceObject("EventTickets.Title") as string;
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("EventTickets.Title") as string);
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                this.CnChkout24h.AddToCartControlList = new List<System.Web.UI.Control>{
                    this.CheckoutButton,
                    this.AddToCart,
                };
                Response.Cache.SetCacheability(HttpCacheability.NoCache); // For FireFox 
                Response.Cache.SetAllowResponseInBrowserHistory(false); // For Chrome
                Response.Cache.SetNoStore(); // For IE
                recalcVPSubmit.Visible = false;
                recalcVPSubmit2.Visible = false;

                if (DistributorOrderingProfile.IsPC)
                {
                    lbVolumePoint.Visible = VolumePointText.Visible = false;

                }
            }
        }



        [SubscribesTo(MyHLEventTypes.ProductDetailBeingClosed)]
        public void ProductDetailBeingClosed(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["CatalogItemCacheKey"] != null)
            {
                string cacheKey = HttpContext.Current.Session["CatalogItemCacheKey"] as string;
                HttpRuntime.Cache.Remove(cacheKey);
            }

            favourSKUs = favorSKULoader.GetDistributorFavouriteSKU(ShoppingCart.DistributorID, Thread.CurrentThread.CurrentCulture.Name);

            var sessionInfo = this.SessionInfo;

            if (sessionInfo.ShowAllInventory)
            {
                OnShowAllInventory(this, null);
            }
            else
            {
                OnShowAvailableInventory(this, null);
            }
        }

        /// <summary>
        ///     The get category id.
        /// </summary>
        /// <param name="uiCategoryProduct">
        ///     The ui category product.
        /// </param>
        /// <returns>
        ///     The get category id.
        /// </returns>
        protected int GetCategoryID(UICategoryProduct uiCategoryProduct)
        {
            return uiCategoryProduct.Category.ID;
        }

        /// <summary>
        ///     The get product id.
        /// </summary>
        /// <param name="uiCategoryProduct">
        ///     The ui category product.
        /// </param>
        /// <returns>
        ///     The get product id.
        /// </returns>
        protected int GetProductID(UICategoryProduct uiCategoryProduct)
        {
            return uiCategoryProduct.Product.ID;
        }

        /// <summary>
        ///     The should take.
        /// </summary>
        /// <param name="prod">
        ///     The prod.
        /// </param>
        /// <param name="bEventTicket">
        ///     The b event ticket.
        /// </param>
        /// <returns>
        ///     The should take.
        /// </returns>
        private bool shouldTake(ProductInfo_V02 prod, bool bEventTicket)
        {
            bool take = bEventTicket
                            ? (prod.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                            : (prod.TypeOfProduct != ServiceProvider.CatalogSvc.ProductType.EventTicket);
            if (take)
            {
                take = prod.SKUs.Any(s => s.IsDisplayable);
            }

            if (IsChina && bEventTicket && take)
            {
                take = MyHerbalife3.Ordering.Providers.China.OrderProvider.IsEligibleForEvents(this.DistributorID);
            }

            return take;
        }

        /// <summary>
        ///     The should take.
        /// </summary>
        /// <param name="category">
        ///     The category.
        /// </param>
        /// <param name="bEventTicket">
        ///     The b event ticket.
        /// </param>
        /// <returns>
        ///     The should take.
        /// </returns>
        private bool shouldTake(Category_V02 category, bool bEventTicket)
        {
            if (category.Products != null)
            {
                foreach (ProductInfo_V02 prod in category.Products)
                {
                    if (shouldTake(prod, bEventTicket))
                        return true;
                }
            }

            if (category.SubCategories != null)
            {
                foreach (Category_V02 sub in category.SubCategories)
                {
                    if (shouldTake(sub, bEventTicket))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool ShowVolume()
        {
            bool showVolume = true;
            if (DistributorOrderingProfile.IsPC)
                showVolume = false;
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayVolumePointsForEventTicket &&
                SessionInfo.IsEventTicketMode)
            {
                //ETO
                showVolume = false;
            }

            return showVolume;
        }

        /// <summary>
        ///     The get bread crumb text.
        /// </summary>
        /// <param name="categoryProudct">
        ///     The category proudct.
        /// </param>
        /// <returns>
        ///     The get bread crumb text.
        /// </returns>
        protected string getBreadCrumbText(UICategoryProduct categoryProudct)
        {
            //return getCategoryText(categoryProudct.Category) + categoryProudct.Category.DisplayName + "&nbsp;&gt;&nbsp;" +
            //       categoryProudct.Product.DisplayName;
            return CatalogHelper.getBreadCrumbText(categoryProudct.Category, categoryProudct.RootCategory,
                                                   categoryProudct.Product);
        }

        /// <summary>
        ///     The find root category from id.
        /// </summary>
        /// <param name="categoryID">
        ///     The category id.
        /// </param>
        /// <returns>
        /// </returns>
        private Category_V02 findRootCategoryFromID(int categoryID)
        {
            return ProductInfoCatalog.RootCategories.Find(c => c.ID == categoryID);
        }

        /// <summary>
        ///     The get enabled.
        /// </summary>
        /// <param name="availType">
        ///     The avail type.
        /// </param>
        /// <returns>
        ///     The get enabled.
        /// </returns>
        protected bool getEnabled(ProductAvailabilityType availType)
        {
            return true;
        }

        protected int getMaxLength(string sku)
        {
            if (sku == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(sku)))
            {
                return HLConfigManager.Configurations.DOConfiguration.HFFSkuMaxQuantity.ToString().Length;
            }
            return HLConfigManager.Configurations.ShoppingCartConfiguration.QuantityBoxSize;
        }

        /// <summary>
        ///     The on category dropdown_ data bound.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnCategoryDropdown_DataBound(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            ddl.SelectedIndex = -1;
            var selected = new ListItem();
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (Request.QueryString["PreOrdering"] != null)
                {
                    bool ispreordering;
                    bool.TryParse(Request.QueryString["PreOrdering"], out ispreordering);
                    if (ispreordering)
                    {
                        selected = ddl.Items.Count > 0 ? ddl.Items[ddl.Items.Count - 2] : null;
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            //Only clear cart if only APF in the cart. Otherwise user can clear the cart by themself.
                            if (DistributorOrderingProfile.CNAPFStatus == 2 && ShoppingCart != null && ShoppingCart.CartItems.Count == 1 && APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems))
                            {
                                var Skus = APFDueProvider.GetAPFSkusFromCart(ShoppingCart.CartItems);
                                ShoppingCart.DeleteItemsFromCart(Skus);
                            }
                        }
                    }
                }
                else
                {
                    selected = ddl.Items.Count > 0 ? ddl.Items[0] : null;
                }
            }
            else
            {
                selected = ddl.Items.Count > 0 ? ddl.Items[0] : null;

            }
            if (selected != null)
            {
                selected.Selected = true;
                populateCategories(findRootCategoryFromID(int.Parse(selected.Value)));
            }
        }

        /// <summary>
        ///     The on category dropdown_ selected index changed.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnCategoryDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProductInfoCatalog.RootCategories != null)
            {
                var ddl = sender as DropDownList;
                populateCategories(ddl.SelectedItem != null
                                       ? findRootCategoryFromID(int.Parse(ddl.SelectedItem.Value))
                                       : ProductInfoCatalog.RootCategories[0]);

                // 24061
                _errors.Clear();
                // 27237
                lblSuccess.Visible = false;
                blstErrores.DataSource = _errors;
                blstErrores.DataBind();

                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    if (ddl.Items[ddl.Items.Count - 1].Value == ddl.SelectedItem.Value) //Is Preordering, use redirect, otherwise mini cart will not be updated.
                    {
                        //Only clear cart if only APF in the cart. Otherwise user can clear the cart by themself.
                        if (DistributorOrderingProfile.CNAPFStatus == 2 && ShoppingCart != null && ShoppingCart.CartItems.Count == 1 && APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems))
                        {
                            base.ClearCart();
                        }
                    }
                }
            }

            lbVolumePoint.Text = "0.00";
            lbTotalAmount.Text = "0.00";
        }

        /// <summary>
        ///     The get all products.
        /// </summary>
        /// <param name="category">
        ///     The category.
        /// </param>
        /// <param name="allProducts">
        ///     The all products.
        /// </param>
        /// <returns>
        /// </returns>
        private List<UICategoryProduct> getAllProducts(Category_V02 rooCategory,
                                                       Category_V02 category,
                                                       List<UICategoryProduct> allProducts)
        {
            if (category.SubCategories != null)
            {
                foreach (Category_V02 sub in category.SubCategories)
                {
                    allProducts = getAllProducts(rooCategory, sub, allProducts);
                }
            }

            if (category.Products != null)
            {
                var prdList = from p in category.Products
                              where
                                  p.SKUs != null && p.SKUs.Count > 0 &&
                                  (from s in p.SKUs where s.IsDisplayable select s).Count() > 0
                              select new UICategoryProduct
                              {
                                  Category = category,
                                  Product = p,
                                  RootCategory = rooCategory,
                              };

                if (IsChina && SessionInfo.IsEventTicketMode)
                {
                    prdList = prdList.Where(x => x.Product.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket);
                }

                allProducts.AddRange(prdList);
            }

            return allProducts;
        }

        private List<SKU_V01> createSKUList(List<UICategoryProduct> prods)
        {
            var itemList = new List<SKU_V01>();
            foreach (UICategoryProduct i in prods)
            {
                itemList.AddRange((from p in i.Product.SKUs
                                   where !itemList.Contains(p)
                                   select p));
            }
            return itemList;
        }

        /// <summary>
        ///     The on subcategories_ item data bound.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnSubcategories_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                if (e.Item.DataItem != null)
                {
                    Repeater rp;
                    if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
                    {
                        rp = e.Item.FindControl(SKUGridID) as Repeater;
                    }
                    else
                    {
                        rp = e.Item.FindControl(SKUGridID) as Repeater;
                    }

                    if (SessionInfo.ShowAllInventory)
                    {
                        rp.DataSource = from p in (e.Item.DataItem as UICategoryProduct).Product.SKUs
                                        from a in AllSKUS.Keys
                                        where a == p.SKU && p.IsDisplayable
                                        select AllSKUS[a];
                    }
                    else
                    {
                        rp.DataSource = from p in (e.Item.DataItem as UICategoryProduct).Product.SKUs
                                        from a in AllSKUS.Keys
                                        where
                                            a == p.SKU && p.IsDisplayable &&
                                            AllSKUS[a].ProductAvailability != ProductAvailabilityType.Unavailable
                                        select AllSKUS[a];
                    }
                    rp.DataBind();
                }
            }
        }

        /// <summary>
        ///     The get bread crumb text.
        /// </summary>
        /// <returns>
        ///     The get bread crumb text.
        /// </returns>
        private string getBreadCrumbText()
        {
            return null;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (UpdateCalled)
            {
                upPriceList.Update();
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina && DistributorOrderingProfile.IsPC)
            {
                if (HttpContext.Current.Session["AttainedSurvey"] != null &&
                    Convert.ToBoolean(HttpContext.Current.Session["AttainedSurvey"]))
                {
                    OnAddToCart(this, null);
                }
                else if (HttpContext.Current.Session["RecalledPromoRule"] != null &&
                    Convert.ToBoolean(HttpContext.Current.Session["RecalledPromoRule"]))
                {

                    OnAddToCart(this, null);
                }
            }


        }

        /// <summary>
        ///     The populate categories.
        /// </summary>
        /// <param name="category">
        ///     The category.
        /// </param>
        private void populateCategories(Category_V02 category)
        {
            var prodList = new List<UICategoryProduct>();
            prodList = getAllProducts(category, category, prodList);
            if (prodList != null)
            {
                List<SKU_V01> skuList;
                HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart,
                                                                       skuList = createSKUList(prodList));
                CatalogProvider.GetProductAvailability(skuList, Locale, DistributorID, CurrentWarehouse);

                if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
                {
                    //SubcategoriesResponsive.DataSource = prodList;
                    //SubcategoriesResponsive.DataBind();
                    SubcategoriesResponsive.Visible = false;
                    //add here code for responsive
                    SubcategoriesResponsiveDatabind(prodList);

                }
                else
                {
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        SubcategoriesDatabind(prodList);
                    }
                    else
                    {
                        Subcategories.DataSource = prodList;
                        Subcategories.DataBind();
                    }

                }
                calcDistributorPrice();
                findCrossSell(category);
                refreshIndicator();
                if (!_showAllInventry)
                {
                    refreshBreadcrumbs();
                }
            }
            else
            {
                LoggerHelper.Error(string.Format("No category found for {0} in {1} pricelist", category != null ? category.Description : string.Empty, this.Locale));
            }

            #region mimic Ordering/Catalog.aspx dataBind() method when IsEventTicketMode and no event-product
            if (IsChina && SessionInfo.IsEventTicketMode && !Helper.GeneralHelper.Instance.HasData(prodList))
            {
                (Master as OrderingMaster).ShowMessage(GetOtherResourceString("EventTicket", CatalogVirtualPath),
                                                       GetOtherResourceString("EventTicketOderingNotAvailable", CatalogVirtualPath));
                SessionInfo.HasEventTicket = false;
                SessionInfo.IsEventTicketMode = false;
            }
            #endregion
        }

        public void ClearCatalogCache()
        {
            if (CategoryDropdown.SelectedItem != null)
            {
                var currentCategory = findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value));
                string cacheKey = GridCaheKey + CurrentWarehouse + CountryCode + currentCategory.ID + _showAllInventry + "Favour";

                HttpRuntime.Cache.Remove(cacheKey);

            }
        }

        public void ClearCatalogCachePriceListPage()
        {
            if (CategoryDropdown.SelectedItem != null)
            {
                var currentCategory = findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value));
                string cacheKey = GridCaheKey + CurrentWarehouse + CountryCode + currentCategory.ID + _showAllInventry;

                HttpRuntime.Cache.Remove(cacheKey);

            }
        }

        private void SubcategoriesDatabind(List<UICategoryProduct> prodlist)
        {
            if (CategoryDropdown.SelectedItem != null)
            {
                var currentCategory = findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value));
                string cacheKey = GridCaheKey + CurrentWarehouse + CountryCode + currentCategory.ID + _showAllInventry + "Favour";
                Session["FavourCatalogItemCacheKey"] = cacheKey;

                var result = HttpRuntime.Cache[cacheKey] as string;
                pricelistGridInfo.Value = string.Empty;
                if (result == null)
                {
                    StringBuilder htmlTable = new StringBuilder();
                    htmlTable.Append("<table>");
                    if (ShowVolume())
                    {
                        htmlTable.Append(
                            "<tr class='freeze-header'>" +
                                "<th class='col-Avail'></th>" +
                                "<th class='col-SKU'>" + GetLocalResourceObject("titleSKUResource1.Text") + "</th>" +
                                "<th class='col-Product'>" + GetLocalResourceObject("titleProductResource1.Text") + "</th>" +
                                "<th class='col-QTY'>" + GetLocalResourceObject("titleQTYResource1.Text") + "</th>" +
                                "<th class='col-RetailPrice'>" + GetLocalResourceObject("titleRetailPriceResource1.Text") + "</th>" +
                                "<th class='col-VolumePoint'>" + GetLocalResourceObject("titleVolumePointResource1.Text") + "</th>" +
                                "<th class='col-Favourite'>" + GetLocalResourceObject("titleFavouriteResource.Text") + "</th>" +
                            "</tr>");
                    }
                    else
                    {
                        htmlTable.Append(
                            "<tr class='freeze-header'><th class='col-Avail'></th>" +
                                "<th class='col-SKU'>" + GetLocalResourceObject("titleSKUResource1.Text") + "</th>" +
                                "<th class='col-Product'>" + GetLocalResourceObject("titleProductResource1.Text") + "</th>" +
                                "<th class='col-QTY'>" + GetLocalResourceObject("titleQTYResource1.Text") + "</th>" +
                                "<th class='col-RetailPrice'>" + GetLocalResourceObject("titleRetailPriceResource1.Text") + "</th>" +
                                "<th class='col-Favourite'>" + GetLocalResourceObject("titleFavouriteResource.Text") + "</th>" +
                            "</tr>");

                    }
                    if (prodlist.Count > 0)
                    {
                        var DataSource = new List<SKU_V01>();
                        var favNum = 0;
                        foreach (var prod in prodlist)
                        {
                            if (SessionInfo.ShowAllInventory)
                            {
                                DataSource = (from p in prod.Product.SKUs
                                              from a in AllSKUS.Keys
                                              where a == p.SKU && p.IsDisplayable
                                              select AllSKUS[a]).ToList();
                            }
                            else
                            {
                                DataSource = (from p in prod.Product.SKUs
                                              from a in AllSKUS.Keys
                                              where
                                                  a == p.SKU && p.IsDisplayable &&
                                                  AllSKUS[a].ProductAvailability != ProductAvailabilityType.Unavailable
                                              select AllSKUS[a]).ToList();
                            }
                            if (DataSource.Any())
                            {
                                if (!(favourSKUs.Any(fvr => DataSource.Select(sk => sk.SKU.Trim()).Contains(fvr.ProductSKU.Trim())))) continue;
                                    htmlTable.Append("<tr><td colspan='9' class='gdo-pricelist-breadcrumb'>" + getBreadCrumbText(prod) + "</td></tr>");
                                
                                var i = 0;
                                foreach (var skuV01 in DataSource)
                                {
                                    if (!(favourSKUs.Any(x => x.ProductSKU.Trim() == skuV01.SKU.Trim()))) continue;

                                        favNum += 1;
                                    string favID = "fav" + favNum;
                                    htmlTable.Append("<tr class='gdo-pricelist-tbl-data pricelist-data " + ((++i % 2 != 0) ? "gdo-row-odd" : "gdo-row-even") + "' product-sku='" + skuV01.SKU.Trim() + "'>");

                                    if (skuV01.ProductAvailability == ProductAvailabilityType.Available)
                                    {
                                        htmlTable.Append("<td class='col-Avail'><img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_green.gif' alt='' style='height:auto;'></td>");
                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.Unavailable)
                                    {
                                        htmlTable.Append("<td class='col-Avail'><img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_red.gif' alt=''></td>");

                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.AllowBackOrder)
                                    {
                                        htmlTable.Append("<td class='col-Avail'><img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_orange.gif' alt=''></td>");
                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.UnavailableInPrimaryWh)
                                    {
                                        htmlTable.Append("<td class='col-Avail'><img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_blue.gif' alt=''></td>");
                                    }
                                    htmlTable.Append("<td class='col-SKU'>" + skuV01.SKU.Trim() + "</td>");
                                    htmlTable.Append("<td class='col-Product'><a onclick='PrdDetailPopUp(" + skuV01.Product.ID + "," + prod.Category.ID + ")'>" + skuV01.Product.DisplayName + skuV01.Description + "</a></td>");
                                    htmlTable.Append("<td class='col-QTY' align='center'><input class='onlyNumbers' type='Text' ID='txt_" + skuV01.SKU.Trim() + "' runat='server' size='5' maxlength='5' onblur='show(this);'></td>");
                                    htmlTable.Append("<td class='col-RetailPrice' list-price='" + (decimal)skuV01.CatalogItem.ListPrice + "' >" + getAmountString((decimal)skuV01.CatalogItem.ListPrice) + "</td>");
                                    if (ShowVolume())
                                        htmlTable.Append("<td class='col-VolumePoint' item-vp='" + GetVolumePointsFormat((decimal)skuV01.CatalogItem.VolumePoints) + "'>" + GetVolumePointsFormat((decimal)skuV01.CatalogItem.VolumePoints) + "</td>");

                                    int Del = 1;
                                    string imgFavor = "/Ordering/Images/icons/icn_fav_on.png";
                                    htmlTable.Append("<td class='col-Favourite' list-price='" + (decimal)skuV01.CatalogItem.ListPrice + "' ><img src = '" + imgFavor + "' id = '" + favID + "' onclick='AddRemoveFavouriteClick(\"" + DistributorID.Trim() + "\",\"" + favID.Trim() + "\",\"" + skuV01.SKU.Trim() + "\"," + skuV01.CatalogItem.StockingSKU + "," + prod.Category.ID + ", " + Del + ");' /></td>");


                                    htmlTable.Append("</tr>");
                                }

                            }

                        }
                        htmlTable.Append("</table>");

                        result = htmlTable.ToString();
                        HttpRuntime.Cache.Insert(cacheKey,
                                                 result,
                                                 null,
                                                 DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                                                 Cache.NoSlidingExpiration,
                                                 CacheItemPriority.Normal,
                                                 null);
                    }
                }

                if (priceListHTMLGrid.HasControls())
                {
                    priceListHTMLGrid.Controls.RemoveAt(0);
                }

                priceListHTMLGrid.Controls.Add(new Literal { Text = result });
                if (result != null)
                    pricelistGrid.Value = result.Replace('<', '[');
                pricelistGrid.Value = pricelistGrid.Value.Replace('>', ']');
            }
        }


        private void SubcategoriesResponsiveDatabind(List<UICategoryProduct> prodlist)
        {
            if (CategoryDropdown.SelectedItem != null)
            {
                var currentCategory = findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value));
                string cacheKey = ResponsiveGridCaheKey + CurrentWarehouse + CountryCode + currentCategory.ID + _showAllInventry;

                var result = HttpRuntime.Cache[cacheKey] as string;
                pricelistGridInfo.Value = string.Empty;
                if (result == null)
                {
                    StringBuilder htmlTable = new StringBuilder();
                    if (prodlist.Count > 0)
                    {
                        var DataSource = new List<SKU_V01>();
                        foreach (var prod in prodlist)
                        {
                            if (SessionInfo.ShowAllInventory)
                            {
                                DataSource = (from p in prod.Product.SKUs
                                              from a in AllSKUS.Keys
                                              where a == p.SKU && p.IsDisplayable
                                              select AllSKUS[a]).ToList();
                            }
                            else
                            {
                                DataSource = (from p in prod.Product.SKUs
                                              from a in AllSKUS.Keys
                                              where
                                                  a == p.SKU && p.IsDisplayable &&
                                                  AllSKUS[a].ProductAvailability != ProductAvailabilityType.Unavailable
                                              select AllSKUS[a]).ToList();
                            }
                            if (DataSource.Any())
                            {
                                htmlTable.Append("<div id=\"trBreadcrumb\">");
                                htmlTable.Append("<div id=\"Td1\" class=\"gdo-pricelist-breadcrumb\" colspan='<%# " + getProductTableColumns() + "%>' >");
                                htmlTable.Append("<asp:Label ID=\"lbBreadCrumb\" runat=\"server\" Text='<%#" + getBreadCrumbText(prod) + "%>'></asp:Label></font>");
                                htmlTable.Append(getBreadCrumbText(prod));
                                htmlTable.Append("</div>");
                                htmlTable.Append("</div>");
                                var i = 0;
                                foreach (var skuV01 in DataSource)
                                {
                                    htmlTable.Append("<div class=\"row product-info myclear " + ((++i % 2 != 0) ? "gdo-row-odd" : "gdo-row-even") + "\">");
                                    htmlTable.Append("<div class=\"col-xs-9 left-side\">");
                                    htmlTable.Append("<div class=\"col-xs-12 col-Product\">");
                                    htmlTable.Append("<a onclick='PrdDetailPopUp(" + skuV01.Product.ID + "," + prod.Category.ID + ")' href=\"javascript:;\" id=\"LinkProductDetail\">" + skuV01.Product.DisplayName + skuV01.Description + "</a>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("<div class=\"col-xs-6 bold\">");
                                    htmlTable.Append("<div><span>" + GetLocalResourceObject("titleSKUResource1.Text") + "</span></div>");
                                    htmlTable.Append("<div><span>" + GetLocalResourceObject("titleRetailPriceResource1.Text") + "</span></div>");
                                    if (ShowVolume())
                                        htmlTable.Append("<div><span>" + GetLocalResourceObject("titleVolumePointResource1.Text") + "</span></div>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("<div class=\"col-xs-4 align-right\">");
                                    htmlTable.Append("<div>" + skuV01.SKU.Trim() + "</div>");
                                    htmlTable.Append("<div><span>" + getAmountString((decimal)skuV01.CatalogItem.ListPrice) + "</span></div>");
                                    if (ShowVolume())
                                        htmlTable.Append("<div><span>" + GetVolumePointsFormat((decimal)skuV01.CatalogItem.VolumePoints) + "</span></div>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("<div class=\"col-xs-3 right-side\">");
                                    htmlTable.Append("<div id=\"divGreen\" class=\"align-right col-Avail\">");
                                    htmlTable.Append("<div id=\"divGreen\">");
                                    htmlTable.Append("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\">");
                                    htmlTable.Append("<tbody>");
                                    htmlTable.Append("<tr>");
                                    htmlTable.Append("<td nowrap=\"nowrap\">");
                                    //ADD IMAGE

                                    if (skuV01.ProductAvailability == ProductAvailabilityType.Available)
                                    {
                                        //htmlTable.Append("<img style=\"border-width:0px;\" src=\"/Content/Global/Products/Img/circle_green.gif\" text=\"可订购产品\" id=\"uxGreen\">");

                                        htmlTable.Append("<img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_green.gif' alt='' style='height:auto;'>");
                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.Unavailable)
                                    {
                                        htmlTable.Append("<img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_red.gif' alt=''>");

                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.AllowBackOrder)
                                    {
                                        htmlTable.Append("<img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_orange.gif' alt=''>");
                                    }
                                    else if (skuV01.ProductAvailability == ProductAvailabilityType.UnavailableInPrimaryWh)
                                    {
                                        htmlTable.Append("<img src='https://cn.myherbalife.cn/Content/Global/Products/Img/circle_blue.gif' alt=''>");
                                    }
                                    //htmlTable.Append("<img style=\"border-width:0px;\" src=\"/Content/Global/Products/Img/circle_green.gif\" text=\"可订购产品\" id=\"uxGreen\">");
                                    htmlTable.Append("</td>");
                                    htmlTable.Append("</tr>");
                                    htmlTable.Append("</tbody>");
                                    htmlTable.Append("</table>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("<div class=\"center col-QTY\">");
                                    htmlTable.Append("<span class=\"qty\" id=\"titleQTY\">" + GetLocalResourceObject("titleQTYResource1.Text") + "</span>");
                                    //htmlTable.Append("<input type=\"text\" style=\"background-color:White;\" size=\"5\" class=\"onlyNumbers\" id=\"QuantityBox\" maxlength=\"5\">");
                                    htmlTable.Append("<input type='Text' ID='txt_" + skuV01.SKU.Trim() + "' style='background-color:White;' class='onlyNumbers' onblur='show(this);' maxlength='5' runat='server'>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("</div>");
                                    htmlTable.Append("</div>");

                                    break;
                                }
                            }
                        }
                    }
                    result = htmlTable.ToString();
                    HttpRuntime.Cache.Insert(cacheKey,
                                             result,
                                             null,
                                             DateTime.Now.AddMinutes(INVENTORY_CACHE_MINUTES),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Normal,
                                             null);
                }

                if (priceListHTMLGrid.HasControls())
                {
                    priceListHTMLGrid.Controls.RemoveAt(0);
                }
                priceListHTMLGrid.Controls.Add(new Literal { Text = result });
                pricelistGrid.Value = result.Replace('<', '[');
                pricelistGrid.Value = pricelistGrid.Value.Replace('>', ']');
            }
        }

        protected void OnProducts_ItemDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (IsPostBack && HLConfigManager.Configurations.DOConfiguration.IsChina)
                return;
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPriceForLiterature)
                return;

            //KR NPS change displaying retail price in earn base for literature items  USER STORY 136570
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                //task 149618 display retail price only for literature items
                if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice
                    && HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPriceForLiterature)
                {
                    var txtRetailPrice = e.Item.FindControl("Retail") as Label;
                    if (txtRetailPrice != null)
                    {
                        var itemTodisplay = (SKU_V01)e.Item.DataItem;
                        //if is not a L type, hide the retail price with an empty string 
                        if (itemTodisplay != null && itemTodisplay.Product.TypeOfProduct != ServiceProvider.CatalogSvc.ProductType.Literature &&
                            (HLConfigManager.Configurations.ShoppingCartConfiguration
                                            .DisplayRetailPriceForLiterature))
                        {
                            txtRetailPrice.Text = string.Empty;

                        }
                        //if is a L type hide the your prce to display only the retail price
                        //else
                        //{
                        //var txtDiscountedPrice = e.Item.FindControl("DiscountedPrice") as Label;
                        //if (txtDiscountedPrice != null)
                        // {
                        //txtDiscountedPrice.Text = string.Empty;
                        //  }
                        //}


                    }
                }
            }
        }

        /// <summary>
        ///     The on products_ on item created.
        /// </summary>
        /// <param name="Sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnProducts_OnItemCreated(object Sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.Item:

                case ListItemType.AlternatingItem:
                    {

                        var tdEarnBase = e.Item.FindControl("tdEarnBase") as HtmlTableCell;
                        if (tdEarnBase != null)
                        {
                            tdEarnBase.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                                !SessionInfo.IsEventTicketMode;
                        }

                        var tdEarnBase2 = e.Item.FindControl("tdEarnBase2") as HtmlTableCell;
                        if (tdEarnBase2 != null)
                        {
                            tdEarnBase2.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                                !SessionInfo.IsEventTicketMode;
                        }

                        var tdDiscountedPrice = e.Item.FindControl("tdDiscountedPrice") as HtmlTableCell;
                        if (tdDiscountedPrice != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .HasDiscountedPriceForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                tdDiscountedPrice.Visible = false;
                            }
                            else
                            {
                                tdDiscountedPrice.Visible =
                                    HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount;
                            }
                        }

                        var tdRetailPrice = e.Item.FindControl("tdRetailPrice") as HtmlTableCell;
                        if (tdRetailPrice != null)
                        {


                            tdRetailPrice.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice;


                        }

                        var tdVolumePoint = e.Item.FindControl("tdVolumePoint") as HtmlTableCell;
                        if (tdVolumePoint != null)
                        {
                            //decide
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .DisplayVolumePointsForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                //ETO
                                tdVolumePoint.Visible = false;
                            }
                            else
                            {
                                //RSO
                                tdVolumePoint.Visible =
                                    HLConfigManager.Configurations.ShoppingCartConfiguration.HasVolumePoints;
                            }
                            if (DistributorOrderingProfile.IsPC)
                                tdVolumePoint.Visible = false;
                        }

                        var tdAddItem = e.Item.FindControl("tdAddItem") as HtmlTableCell;
                        if (tdAddItem != null)
                        {
                            tdAddItem.Visible = HLConfigManager.Configurations.DOConfiguration.IsResponsive && GlobalContext.CurrentExperience.ExperienceType == Shared.ViewModel.ValueObjects.ExperienceType.Green;
                        }
                        var tdAddItem2 = e.Item.FindControl("tdAddItem2") as HtmlTableCell;
                        if (tdAddItem2 != null)
                        {
                            tdAddItem2.Visible = HLConfigManager.Configurations.DOConfiguration.IsResponsive && GlobalContext.CurrentExperience.ExperienceType == Shared.ViewModel.ValueObjects.ExperienceType.Green;
                        }
                    }

                    break;
                default:
                    break;
            }
        }



        /// <summary>
        ///     The add items.
        /// </summary>
        /// <param name="bClearTextField">
        ///     The b clear text field.
        /// </param>
        /// <param name="bChkInventory">
        ///     The b chk inventory.
        /// </param>
        /// <param name="dlg">
        ///     The dlg.
        /// </param>
        private void AddItems(bool bClearTextField, AddItemsDelegate dlg)
        {
            if (ShoppingCart != null)
            {
                bool bNoItemSelected = true;

                if ((HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) || (HLConfigManager.Configurations.DOConfiguration.IsChina))
                {
                    var pricelistGridValue = pricelistGridInfo.Value;
                    if (!string.IsNullOrEmpty(pricelistGridValue))
                    {
                        string[] griditems = pricelistGridValue.Split(',');
                        var skus = new List<string>();
                        foreach (var griditem in griditems.Reverse())
                        {
                            if (!string.IsNullOrEmpty(griditem))
                            {
                                if (!string.IsNullOrEmpty(griditem.Split('|')[1]))
                                {
                                    bNoItemSelected = false;
                                    int quantity = 0;
                                    if (skus.Any(x => x.Trim().Contains(griditem.Split('|')[0].Split('_')[1])))
                                    {
                                        continue;
                                    }
                                    skus.Add(griditem.Split('|')[0].Split('_')[1]);

                                    if (!int.TryParse(griditem.Split('|')[1], out quantity) || quantity <= 0)
                                    {
                                        _errors.Add(
                                            String.Format(
                                                PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                          "QuantityIncorrect"),
                                            griditem.Split('|')[0].Split('_')[1]));

                                    }
                                    else
                                    {
                                        SKU_V01 skuV01;
                                        if (AllSKUS.TryGetValue(griditem.Split('|')[0].Split('_')[1], out skuV01))
                                        {
                                            // Check isBlocked or unavailable first.
                                            if (CatalogProvider.GetProductAvailability(skuV01, CurrentWarehouse,
                                                                                   ShoppingCart.DeliveryInfo !=
                                                                                   null
                                                                                        ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                                        : DeliveryOptionType.Unknown) ==
                                            ProductAvailabilityType.Unavailable)
                                            {
                                                _errors.Add(
                                                    String.Format(
                                                        PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                  "SKUNotAvailable"),
                                                    griditem.Split('|')[0].Split('_')[1]));
                                                continue;
                                            }
                                            int availQuantity = 0;
                                            int backorderCoverage = 0;
                                            if (CheckMaxQuantity(ShoppingCart.CartItems, quantity, skuV01, _errors))
                                            {
                                                backorderCoverage = CheckBackorderCoverage(quantity, skuV01,
                                                                                           _friendlyMessages);
                                                if (backorderCoverage == 0)
                                                {
                                                    int allQuanties = GetAllQuantities(ShoppingCart.CartItems,
                                                                                       quantity,
                                                                                       skuV01.SKU);
                                                    availQuantity = CheckInventory(allQuanties, skuV01, _errors);
                                                    if (availQuantity != 0)
                                                    {
                                                        if (availQuantity == allQuanties)
                                                        {
                                                            dlg(griditem.Split('|')[0].Split('_')[1], quantity,
                                                                false);
                                                        }
                                                        else
                                                        {
                                                            int qtyInCart = allQuanties - quantity;
                                                            if (availQuantity - qtyInCart > 0)
                                                                dlg(griditem.Split('|')[0].Split('_')[1],
                                                                    availQuantity - qtyInCart, false);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // partial backordered
                                                    dlg(griditem.Split('|')[0].Split('_')[1], quantity, true);
                                                }
                                            }
                                            if (bClearTextField)
                                            {
                                                // uxQuantity.Text = string.Empty;
                                            }
                                        }
                                        else
                                        {
                                            // error out, but not possible
                                        }
                                    }
                                }
                                else
                                {
                                    skus.Add(griditem.Split('|')[0].Split('_')[1]);
                                }
                            }
                        }

                    }

                }
                else
                {
                    foreach (object s in Subcategories.Items)
                    {
                        var item = s as RepeaterItem;
                        if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                        {
                            var uxProducts = item.FindControl(SKUGridID) as Repeater;
                            foreach (object p in uxProducts.Items)
                            {
                                var productItem = p as RepeaterItem;
                                if (productItem.ItemType == ListItemType.AlternatingItem ||
                                    productItem.ItemType == ListItemType.Item)
                                {
                                    var uxQuantity = productItem.FindControl(QtyControlID) as TextBox;
                                    int quantity = 0;
                                    if (uxQuantity.Text != string.Empty)
                                    {
                                        bNoItemSelected = false;
                                        string productID = (productItem.FindControl(SKUControlID) as HiddenField).Value;

                                        if (!int.TryParse(uxQuantity.Text, out quantity) || quantity <= 0)
                                        {
                                            _errors.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "QuantityIncorrect"), productID));
                                            uxQuantity.Text = string.Empty;
                                        }
                                        else
                                        {
                                            SKU_V01 skuV01;
                                            if (AllSKUS.TryGetValue(productID, out skuV01))
                                            {
                                                // Check isBlocked or unavailable first.
                                                if (CatalogProvider.GetProductAvailability(skuV01, CurrentWarehouse,
                                                                                           ShoppingCart.DeliveryInfo != null
                                                                                                ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                                                : DeliveryOptionType.Unknown) == ProductAvailabilityType.Unavailable)
                                                {
                                                    _errors.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), productID));
                                                    continue;
                                                }
                                                int availQuantity = 0;
                                                int backorderCoverage = 0;
                                                if (CheckMaxQuantity(ShoppingCart.CartItems, quantity, skuV01, _errors))
                                                {
                                                    backorderCoverage = CheckBackorderCoverage(quantity, skuV01,
                                                                                               _friendlyMessages);
                                                    if (backorderCoverage == 0)
                                                    {
                                                        int allQuanties = GetAllQuantities(ShoppingCart.CartItems, quantity,
                                                                                           skuV01.SKU);
                                                        availQuantity = CheckInventory(allQuanties, skuV01, _errors);
                                                        if (availQuantity != 0)
                                                        {
                                                            if (availQuantity == allQuanties)
                                                            {
                                                                dlg(productID, quantity, false);
                                                            }
                                                            else
                                                            {
                                                                int qtyInCart = allQuanties - quantity;
                                                                if (availQuantity - qtyInCart > 0)
                                                                    dlg(productID, availQuantity - qtyInCart, false);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // partial backordered
                                                        dlg(productID, quantity, true);
                                                    }
                                                }
                                                if (bClearTextField)
                                                {
                                                    uxQuantity.Text = string.Empty;
                                                }
                                            }
                                            else
                                            {
                                                // error out, but not possible
                                            }
                                        }
                                    }
                                }
                            }

                        }

                    }
                }

                if (bNoItemSelected)
                {
                    bool isPickupinfofilled = Convert.ToBoolean(Session["Pickupinfofilled"]);
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && isPickupinfofilled)
                    {
                        _errors.Clear();
                        Session["Pickupinfofilled"] = false;
                    }
                    else
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoItemsSelected"));
                    }
                }
            }
        }
        private void RetainGridValue()
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var lt = new Literal { Text = pricelistGrid.Value.Replace('[', '<').Replace(']', '>') };
                var pricelistGridValue = pricelistGridInfo.Value;
                if (!string.IsNullOrEmpty(pricelistGridValue))
                {
                    string[] griditems = pricelistGridValue.Split(',');
                    var skus = new List<string>();
                    foreach (var griditem in griditems.Reverse())
                    {
                        if (!string.IsNullOrEmpty(griditem))
                        {
                            if (!string.IsNullOrEmpty(griditem.Split('|')[1]))
                            {
                                int quantity = 0;
                                if (skus.Any(x => x.Trim().Contains(griditem.Split('|')[0].Split('_')[1])))
                                {
                                    continue;
                                }

                                skus.Add(griditem.Split('|')[0].Split('_')[1]);

                                if (!int.TryParse(griditem.Split('|')[1], out quantity) || quantity <= 0)
                                {
                                    continue;
                                }


                                lt.Text = lt.Text.Replace("'" + griditem.Split('|')[0] + "'",
                                                          "'" + griditem.Split('|')[0] + "'" + " value ='" + quantity +
                                                          "'");
                            }
                            else
                            {
                                skus.Add(griditem.Split('|')[0].Split('_')[1]);
                            }
                        }
                    }
                }
                if (priceListHTMLGrid.HasControls() && lt.Text.Length > 0)
                {
                    priceListHTMLGrid.Controls.RemoveAt(0);
                    priceListHTMLGrid.Controls.Add(lt);
                }

            }
        }


        /// <summary>
        ///     AddItemsForCalc
        /// </summary>
        /// <param name="bClearTextField">
        ///     The b clear text field.
        /// </param>
        /// <param name="dlg">
        ///     The dlg.
        /// </param>
        private void AddItemsForCalc(bool bClearTextField, AddItemsDelegate dlg)
        {
            if (ShoppingCart != null)
            {
                bool bNoItemSelected = true;
                if ((HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) || (HLConfigManager.Configurations.DOConfiguration.IsChina))
                {
                    var lt = new Literal { Text = pricelistGrid.Value.Replace('[', '<').Replace(']', '>') };
                    var pricelistGridValue = pricelistGridInfo.Value;
                    if (!string.IsNullOrEmpty(pricelistGridValue))
                    {
                        string[] griditems = pricelistGridValue.Split(',');
                        var skus = new List<string>();
                        foreach (var griditem in griditems.Reverse())
                        {
                            if (!string.IsNullOrEmpty(griditem))
                            {
                                if (!string.IsNullOrEmpty(griditem.Split('|')[1]))
                                {
                                    bNoItemSelected = false;
                                    int quantity = 0;
                                    if (skus.Any(x => x.Trim().Contains(griditem.Split('|')[0].Split('_')[1])))
                                    {
                                        continue;
                                    }

                                    skus.Add(griditem.Split('|')[0].Split('_')[1]);

                                    if (!int.TryParse(griditem.Split('|')[1], out quantity) || quantity <= 0)
                                    {
                                        _errors.Add(
                                            String.Format(
                                                PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                          "QuantityIncorrect"),
                                                griditem.Split('|')[0].Split('_')[1]));

                                    }

                                    else
                                    {
                                        lt.Text = lt.Text.Replace("'" + griditem.Split('|')[0] + "'", "'" + griditem.Split('|')[0] + "'" + " value ='" + quantity + "'");

                                        dlg(griditem.Split('|')[0].Split('_')[1], quantity, false);

                                    }
                                }
                                else
                                {
                                    skus.Add(griditem.Split('|')[0].Split('_')[1]);
                                }
                            }
                        }
                        if (priceListHTMLGrid.HasControls() && lt.Text.Length > 0)
                        {
                            priceListHTMLGrid.Controls.RemoveAt(0);
                            priceListHTMLGrid.Controls.Add(lt);
                        }
                        //pricelistGridInfo.Value = string.Empty;
                    }
                }
                foreach (object s in Subcategories.Items)
                {
                    var item = s as RepeaterItem;
                    if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                    {
                        var uxProducts = item.FindControl(SKUGridID) as Repeater;
                        foreach (object p in uxProducts.Items)
                        {
                            var productItem = p as RepeaterItem;
                            if (productItem.ItemType == ListItemType.AlternatingItem ||
                                productItem.ItemType == ListItemType.Item)
                            {
                                var uxQuantity = productItem.FindControl(QtyControlID) as TextBox;
                                int quantity = 0;
                                if (uxQuantity.Text != string.Empty)
                                {
                                    bNoItemSelected = false;
                                    string productID = (productItem.FindControl(SKUControlID) as HiddenField).Value;

                                    if (!int.TryParse(uxQuantity.Text, out quantity) || quantity <= 0)
                                    {
                                        _errors.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "QuantityIncorrect"), productID));
                                        uxQuantity.Text = string.Empty;
                                    }
                                    else
                                    {
                                        dlg(productID, quantity, false);
                                    }
                                }
                            }
                        }
                    }
                }
                // foreach
                if (bNoItemSelected)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoItemsSelectedToCalculateVolume"));
                }
            }
        }

        /// <summary>
        ///     OnRecalculate
        /// </summary>
        /// <param name="Source">
        /// </param>
        /// <param name="e">
        /// </param>
        protected void OnRecalculate(object Source, EventArgs e)
        {
            _errors.Clear();
            var products = new List<ShoppingCartItem_V01>();
            AddItemsForCalc(false,
                            (string sku, int quantity, bool bPartialBackordered) =>
                            {
                                products.Add(new ShoppingCartItem_V01
                                {
                                    SKU = sku,
                                    Quantity = quantity,
                                    Updated = DateTime.Now,
                                    PartialBackordered = bPartialBackordered,
                                });
                            });
            if (_errors.Count == 0)
            {
                var total = ShoppingCart.Calculate(products);
                if (total != null)
                {
                    lbVolumePoint.Text = (total as OrderTotals_V01).VolumePoints.ToString("N2");
                    lbTotalAmount.Text = (total as OrderTotals_V01).ItemsTotal.ToString("N2");
                    VolumePointText.Visible = lbVolumePoint.Visible = HLConfigManager.Configurations.DOConfiguration.IsChina ? ShowVolume() : true;
                    lbTotalAmount.Visible = TotalAmountText.Visible = HLConfigManager.Configurations.DOConfiguration.IsChina;
                }
            }
            else if (_errors.Count > 0)
            {
                lbVolumePoint.Text = "0.00";
                lbTotalAmount.Text = "0.00";
            }

            blstErrores.DataSource = _errors;
            blstErrores.DataBind();
        }

        /// <summary>
        ///     The on add to cart.
        /// </summary>
        /// <param name="Source">
        ///     The source.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnAddToCart(object Source, EventArgs e)
        {
            var arg = e;
            _errors.Clear();
            _friendlyMessages.Clear();

            if (CanAddProduct(DistributorID, ref _errors))
            {
                var products = new List<ShoppingCartItem_V01>();
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && DistributorOrderingProfile.IsPC)
                {
                    if ((HttpContext.Current.Session["AttainedSurvey"] != null &&
                          Convert.ToBoolean(HttpContext.Current.Session["AttainedSurvey"])) || (HttpContext.Current.Session["RecalledPromoRule"] != null &&
                          Convert.ToBoolean(HttpContext.Current.Session["RecalledPromoRule"])))
                    {
                        HttpContext.Current.Session["RecalledPromoRule"] = false;
                        foreach (var shoppingCartItemV01 in ShoppingCart.ShoppingCartItems)
                        {
                            if (!shoppingCartItemV01.IsPromo)
                            {
                                products.Add(new ShoppingCartItem_V01
                                {
                                    SKU = shoppingCartItemV01.SKU,
                                    Quantity = shoppingCartItemV01.Quantity,
                                    Updated = DateTime.Now,
                                    PartialBackordered = shoppingCartItemV01.PartialBackordered,
                                });
                            }

                        }
                        ShoppingCart.ClearCart();

                    }
                    else
                    {
                        AddItems(true,
                                 (string sku, int quantity, bool bPartialBackordered) =>
                                 {
                                     products.Add(new ShoppingCartItem_V01
                                     {
                                         SKU = sku,
                                         Quantity = quantity,
                                         Updated = DateTime.Now,
                                         PartialBackordered = bPartialBackordered,
                                     });
                                 });
                        pricelistGridInfo.Value = string.Empty;
                        if (IsPostBack)
                        {
                            RetainGridValue();
                        }
                    }
                }
                else
                {
                    AddItems(true,
                             (string sku, int quantity, bool bPartialBackordered) =>
                             {
                                 products.Add(new ShoppingCartItem_V01
                                 {
                                     SKU = sku,
                                     Quantity = quantity,
                                     Updated = DateTime.Now,
                                     PartialBackordered = bPartialBackordered,
                                 });
                             });
                    pricelistGridInfo.Value = string.Empty;
                    if (IsPostBack)
                    {
                        RetainGridValue();
                    }
                }

                try
                {
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        bool preordersku = false;
                        bool preorderskuwithsameID = false;
                        if (ShoppingCart.CartItems.Any())
                        {
                            var cartItemsHasPreordering = CatalogProvider.IsPreordering(ShoppingCart.CartItems, ShoppingCart.DeliveryInfo.WarehouseCode);
                            var currentCartItemsHasPreordering = CatalogProvider.IsPreordering(products, ShoppingCart.DeliveryInfo.WarehouseCode);

                            if (cartItemsHasPreordering != currentCartItemsHasPreordering)
                            {
                                _errors.Add(
                                    String.Format(
                                        PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                  "PreOrderingSku")));
                                products.Clear();
                            }

                            CatalogProvider.IsPreordering(ShoppingCart, products,
                                                          ShoppingCart.DeliveryInfo.WarehouseCode, out preordersku,
                                                          out preorderskuwithsameID);

                            if (preordersku && !preorderskuwithsameID)
                            {
                                _errors.Add(
                                 String.Format(
                                     PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                               "PreOrderingSku")));
                                products.Clear();
                            }
                        }
                        else
                        {
                            if (products.Count > 0)
                            {
                                var cartItemsHasPreordering = CatalogProvider.IsPreordering(products, ShoppingCart.DeliveryInfo.WarehouseCode);
                                var isAllPreOrderingSkus = CatalogProvider.IsAllPreorderingProducts(products, ShoppingCart.DeliveryInfo.WarehouseCode);

                                if (cartItemsHasPreordering != isAllPreOrderingSkus)
                                {
                                    _errors.Add(
                                        String.Format(
                                            PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                      "PreOrderingSku")));
                                    products.Clear();
                                }

                                CatalogProvider.IsPreordering(ShoppingCart, products,
                                                              ShoppingCart.DeliveryInfo.WarehouseCode, out preordersku,
                                                              out preorderskuwithsameID);

                                if (preordersku && !preorderskuwithsameID)
                                {
                                    _errors.Add(
                                     String.Format(
                                         PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                   "PreOrderingSku")));
                                    products.Clear();
                                }
                            }
                        }
                    }

                    if (products.Count > 0)
                    {
                        if (ShoppingCart != null)
                        {
                            // Looking for duplicate skus

                            var duplicateItems = products.GroupBy(x => x.SKU).Select(group => new ShoppingCartItem_V01
                            {
                                SKU = group.Key,
                                MinQuantity = group.Count(),
                                Quantity = group.Sum(item => item.Quantity)
                            }).Where(x => x.MinQuantity > 1);
                            foreach (var product in duplicateItems)
                            {
                                SKU_V01 skuV01;
                                if (AllSKUS.TryGetValue(product.SKU, out skuV01))
                                {
                                    if (!CheckMaxQuantity(ShoppingCart.CartItems, product.Quantity, skuV01, _errors))
                                    {
                                        products.RemoveAll(p => p.SKU.Equals(product.SKU));
                                    }
                                }
                            }

                            AddItemsToCart(products);
                            if (ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback))
                            {
                                if (ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback)
                                                .Messages.Any())
                                {
                                    // Checking for specific message to display
                                    var feedbackResult = ShoppingCart.RuleResults.FirstOrDefault(r => r.Result == RulesResult.Feedback && r.Messages.Any(m => m != null && m.StartsWith("HTML|")));
                                    if (feedbackResult != null)
                                    {
                                        var fragmentName = feedbackResult.Messages[0].Split('|');
                                        if (fragmentName.Count() == 2 && fragmentName[1].Length > 0)
                                        {
                                            (this.Master as OrderingMaster).DisplayHtml(fragmentName[1]);
                                        }
                                    }
                                    else
                                    {
                                        var feedback =
                                            ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback)
                                                        .Messages[0];
                                        if (!string.IsNullOrEmpty(feedback))
                                        {
                                            var orderingMaster = Master as OrderingMaster;
                                            if (orderingMaster != null)
                                            {
                                                string title = MyHL_GlobalResources.FeedbackNotification;
                                                if (IsChina) title = GetGlobalResourceString("FeedbackNotification");
                                                orderingMaster.ShowMessage(title,
                                                                           feedback);
                                            }
                                        }
                                    }
                                }
                            }
                            //else
                            {
                                if (
                                    ShoppingCart.RuleResults.Any(
                                        rs =>
                                        rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                                {
                                    PurchasingLimitProvider.SetPostErrorRemainingLimitsSummaryMessage(ShoppingCart);
                                }

                                if (
                                   ShoppingCart.RuleResults.Any(
                                       rs =>
                                       rs.RuleName == "PurchaseRestriction Rules" && rs.Result == RulesResult.Failure))
                                {
                                    var ruleResultMsgs =
                                    ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchaseRestriction Rules")
                                                .Select(r => r.Messages);
                                    _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchaseRestriction Rules");
                                }
                                if (
                                   ShoppingCart.RuleResults.Any(
                                       rs => rs.Result == RulesResult.Failure))
                                {
                                    var ruleResultMessages =
                                        ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.Messages.Count > 0);
                                    if (ruleResultMessages.Any())
                                    {
                                        _errors.AddRange(ruleResultMessages.Select(r => r.Messages[0]));
                                    }
                                }
                            }

                            ShoppingCart.RuleResults.Clear();

                            if (_errors.Count == 0)
                            {
                                lblSuccess.Visible = true;
                            }
                            if (_friendlyMessages.Count > 0)
                            {
                                ShowBackorderMessage(_friendlyMessages);
                            }
                            findCrossSell();
                        }
                    }

                    OmnitureHelper.RegisterOmnitureAddCartScript(Page, ShoppingCart.ShoppingCartItems, products);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(GetDistributorInfo() + "Can't add to cart!\n" + ex);
                }
            }

            if (lbVolumePoint.Text != string.Empty)
                lbVolumePoint.Text = "0.00";

            if (lbTotalAmount.Text != string.Empty)
                lbTotalAmount.Text = "0.00";

            blstErrores.DataSource = _errors;
            blstErrores.DataBind();

            if (_errors.Count > 0)
            {
                OnShoppingCartChanged(null, null);
            }

            if (_errors.Count > 0 && lblSuccess.Visible)
            {
                lblSuccess.Visible = false;
            }

            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
            if (currentSession != null)
            {
                if (currentSession.HasPromoSku)
                {
                    if (ShoppingCart.CartItems.Find(x => x.IsPromo) != null)
                    {
                        promoSkuPopupExtender.Show();
                        currentSession.HasPromoSku = false;
                    }
                }
            }

            // Display PopUp for MY Promotion
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayPromoPopUp && ShoppingCart.DisplayPromo)
            {
                Promotion_MY.ShowPromo();
                ShoppingCart.DisplayPromo = false;
            }

        }

        /// <summary>
        ///     The product detail clicked.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void ProductDetailClicked(object sender, EventArgs e)
        {
            int categoryID = 0, productID = 0;
            var lb = sender as LinkButton;
            if (lb != null)
            {
                string commandArgument = lb.CommandArgument;
                var commandParts = commandArgument.Split(' ');
                productID = int.Parse(commandParts[0]);
                categoryID = int.Parse(commandParts[1]);
                ProductDetailBeingLaunched(this, new ProductDetailEventArgs(categoryID, productID));
            }
            //CntrlProductDetail.LoadProduct(categoryID, productID);
        }

        protected void DummyProductDetailClicked(object sender, EventArgs e)
        {


            if (!string.IsNullOrEmpty(hiddnProductIDCatagoriID.Value))
            {
                RetainGridValue();
                int categoryID = 0, productID = 0;
                var IDs = hiddnProductIDCatagoriID.Value;
                productID = int.Parse(IDs.Split('|')[0]);
                categoryID = int.Parse(IDs.Split('|')[1]);
                ProductDetailBeingLaunched(this, new ProductDetailEventArgs(categoryID, productID));
            }

        }

        /// <summary>
        ///     find Cross sell
        /// </summary>
        /// <param name="currentCategory"></param>
        private void findCrossSell()
        {
            if (CategoryDropdown.SelectedItem != null)
            {
                var currentCategory = findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value));
                if (currentCategory != null)
                {
                    findCrossSell(currentCategory);
                }
            }
        }

        /// <summary>
        ///     The find cross sell.
        /// </summary>
        /// <param name="currentCategory">
        ///     The current category.
        /// </param>
        private void findCrossSell(Category_V02 currentCategory)
        {
            if (currentCategory != null)
            {
                var crossSellList = new List<CrossSellInfo>();
                ProductDetail.CollectAllCrossSellProducts(currentCategory, crossSellList);

                if (crossSellList.Count > 0)
                {
                    CrossSellInfo candidate = null;
                    foreach (CrossSellInfo c in crossSellList)
                    {
                        if (ProductDetail.ShouldSelectThisCrossSell(c.Product, ShoppingCart,
                                                                    Session[ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY]
                                                                    as CrossSellInfo,
                                                                    Session[
                                                                        ProductDetail.
                                                                            LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] as
                                                                    CrossSellInfo))
                        {
                            if (!ProductDetail.IfOutOfStock(c.Product, AllSKUS))
                            {
                                candidate = c;
                                break;
                            }
                        }
                    }

                    // if nothing found
                    if (candidate == null)
                    {
                        candidate = ProductDetail.SelectCrossSellFromPreviousDisplayOrCrssSellList(ShoppingCart,
                                                                                                   crossSellList,
                                                                                                   Session[
                                                                                                       ProductDetail.
                                                                                                           LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                                                                                                       ] as
                                                                                                   CrossSellInfo,
                                                                                                   AllSKUS);
                    }

                    if (candidate != null)
                    {
                        Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] = candidate;

                        // fire event
                        CrossSellFound(this, new CrossSellEventArgs(candidate));
                        return;
                    }
                }

                // fire event
                NoCrossSellFound(this, null);
            }
        }

        /// <summary>
        ///     The refresh breadcrumbs.
        /// </summary>
        private void refreshBreadcrumbs()
        {
            foreach (object s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var trCntrl = item.FindControl("trBreadcrumb") as HtmlTableRow;
                    if (trCntrl != null)
                    {
                        var repeaterProducts = item.FindControl(SKUGridID) as Repeater;
                        if (repeaterProducts != null)
                        {
                            trCntrl.Visible = repeaterProducts.Items.Count != 0;
                        }
                    }
                }
            }
        }

        private void refreshIndicator()
        {
            if (AllSKUS == null)
            {
                return;
            }
            foreach (var s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var uxProducts = item.FindControl(SKUGridID) as Repeater;
                    foreach (var p in uxProducts.Items)
                    {
                        var productItem = p as RepeaterItem;
                        if (productItem.ItemType == ListItemType.AlternatingItem ||
                            productItem.ItemType == ListItemType.Item)
                        {
                            var pv = productItem.FindControl("MyAvail1") as ProductAvailability;
                            var qtyBox = productItem.FindControl("QuantityBox");
                            if (pv != null)
                            {
                                var skuFld = productItem.FindControl(SKUControlID) as HiddenField;
                                if (skuFld != null)
                                {
                                    SKU_V01 sku;
                                    if (AllSKUS.TryGetValue(skuFld.Value, out sku))
                                    {
                                        HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart,
                                                                                               new List<SKU_V01> { sku });
                                        CatalogProvider.GetProductAvailability(sku, CurrentWarehouse,
                                                                               ShoppingCart.DeliveryInfo != null
                                                                                ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                                : DeliveryOptionType.Unknown);
                                        pv.Available = sku.ProductAvailability;

                                        if (sku.ProductAvailability == ProductAvailabilityType.Available)
                                        {
                                            if (ProductSKU.IsBlocked(sku, this.CurrentWarehouse))
                                            {
                                                pv.Available = ProductAvailabilityType.Unavailable;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void clearQuantity()
        {
            foreach (var s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var uxProducts = item.FindControl(SKUGridID) as Repeater;
                    foreach (var p in uxProducts.Items)
                    {
                        var productItem = p as RepeaterItem;
                        if (productItem.ItemType == ListItemType.AlternatingItem ||
                            productItem.ItemType == ListItemType.Item)
                        {
                            var qtyTextBox = productItem.FindControl("QuantityBox") as TextBox;
                            if (qtyTextBox != null)
                            {
                                qtyTextBox.Text = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        private void calcDistributorPrice()
        {
            if (AllSKUS == null)
                return;
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount)
                return;

            var products = new List<tmpClass>();
            foreach (var s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var uxProducts = item.FindControl(SKUGridID) as Repeater;
                    foreach (var p in uxProducts.Items)
                    {
                        var productItem = p as RepeaterItem;
                        if (productItem.ItemType == ListItemType.AlternatingItem ||
                            productItem.ItemType == ListItemType.Item)
                        {
                            var skuFld = productItem.FindControl(SKUControlID) as HiddenField;

                            var discountedPrice = productItem.FindControl("DiscountedPrice") as Label;
                            if (skuFld != null && discountedPrice != null)
                            {
                                products.Add(new tmpClass
                                {
                                    ShoppingCartItem = new ShoppingCartItem_V01
                                    {
                                        SKU = skuFld.Value,
                                        Quantity = 1,
                                        Updated = DateTime.Now,
                                    },
                                    LabelControl = discountedPrice
                                });

                                var volumePoints = (Label)productItem.FindControl("VolumePoints");
                                if (volumePoints != null)
                                {
                                    products.Last().VolumeLabelControl = volumePoints;
                                }
                            }
                        }
                    }
                }
            }
            if (products.Count > 0)
            {
                var totals = ShoppingCart.Calculate(products.Select(p => p.ShoppingCartItem).ToList()) as OrderTotals_V01;
                if (totals != null)
                {
                    foreach (tmpClass t in products)
                    {
                        if (t.LabelControl.Text == string.Empty)
                            continue;
                        var lineItem =
                            totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == t.ShoppingCartItem.SKU) as
                            ItemTotal_V01;
                        if (lineItem != null)
                        {
                            decimal lineItemTotal =
                                HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges
                                    ? OrderProvider.getPriceWithAllCharges(totals, t.ShoppingCartItem.SKU, 1)
                                    : lineItem.DiscountedPrice;
                            t.LabelControl.Text = getAmountString(lineItemTotal);
                            if (t.VolumeLabelControl != null)
                            {
                                t.VolumeLabelControl.Text =
                                    HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                                        ? lineItem.VolumePoints.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                                        : string.Format("{0:N2}", lineItem.VolumePoints);
                            }
                        }
                    }
                }
                //KR NPS change displaying retail price in earn base for literature items  USER STORY 136570
                if (HLConfigManager.Configurations.ShoppingCartConfiguration
                                   .DisplayRetailPriceForLiterature &&
                    !HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
                {
                    foreach (var s in Subcategories.Items)
                    {
                        var item = s as RepeaterItem;
                        var uxProducts = item.FindControl(SKUGridID) as Repeater;
                        foreach (var p in uxProducts.Items)
                        {
                            var productItem = p as RepeaterItem;
                            if (productItem.ItemType == ListItemType.AlternatingItem ||
                                productItem.ItemType == ListItemType.Item)
                            {
                                var EarnBase = productItem.FindControl("EarnBase") as Label;

                                var retailPrice = productItem.FindControl("Retail") as Label;

                                if (retailPrice != null && EarnBase != null &&
                                    !string.IsNullOrEmpty(retailPrice.Text) &&
                                    !string.IsNullOrEmpty(EarnBase.Text) &&
                                    HLConfigManager.Configurations.ShoppingCartConfiguration
                                                   .DisplayRetailPriceForLiterature)
                                {
                                    EarnBase.Text = retailPrice.Text;
                                    retailPrice.Text = string.Empty;
                                }
                            }

                        }
                    }
                }
            }
        }

        protected int getProductTableColumns()
        {
            return (HLConfigManager.Configurations.DOConfiguration.IsResponsive) ? _productTableColumns + 1 : _productTableColumns;
        }

        /// <summary>
        ///     The on show all inventory.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        [SubscribesTo(MyHLEventTypes.ShowAllInventory)]
        public void OnShowAllInventory(object sender, EventArgs e)
        {
            _showAllInventry = true;
            populateCategories(CategoryDropdown.SelectedItem != null
                                   ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                   : ProductInfoCatalog.RootCategories[0]);

            ProductAvailability.ShowLabel = true;
            if (ChinaProductAvailability.Visible)
                ChinaProductAvailability.ShowLabel = true;
            UpdateCalled = true;
        }

        /// <summary>
        ///     The on show available inventory.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        [SubscribesTo(MyHLEventTypes.ShowAvailableInventory)]
        public void OnShowAvailableInventory(object sender, EventArgs e)
        {
            _showAllInventry = false;
            populateCategories(CategoryDropdown.SelectedItem != null
                                   ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                   : ProductInfoCatalog.RootCategories[0]);
            ProductAvailability.ShowLabel = true;
            if (ChinaProductAvailability.Visible)
                ChinaProductAvailability.ShowLabel = true;
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.ProductAvailabilityTypeChanged)]
        public void OnProductAvailabilityTypeChanged(object sender, EventArgs e)
        {
            populateCategories(CategoryDropdown.SelectedItem != null
                                   ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                   : ProductInfoCatalog.RootCategories[0]);
            ProductAvailability.ShowLabel = true;
            if (ChinaProductAvailability.Visible)
                ChinaProductAvailability.ShowLabel = true;
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            RetainGridValue();
            var products = new List<ShoppingCartItem_V01>();
            AddItems(false,
                     (string sku, int quantity, bool bPartialBackordered) =>
                     {
                         products.Add(new ShoppingCartItem_V01
                         {
                             SKU = sku,
                             Quantity = quantity,
                             Updated = DateTime.Now,
                             PartialBackordered = bPartialBackordered,
                         });
                     });

            if (products.Count == 0)
            {
                populateCategories(CategoryDropdown.SelectedItem != null
                                       ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                       : ProductInfoCatalog.RootCategories[0]);
            }

            refreshIndicator();
            ProductAvailability.ShowLabel = true;
            if (ChinaProductAvailability.Visible)
                ChinaProductAvailability.ShowLabel = true;
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            findCrossSell();
            //To correct defect 33652: Verify the number of cart items to hide the lblSuccess if needed
            if (ShoppingCart == null || ShoppingCart.CartItems == null || ShoppingCart.CartItems.Count == 0)
            {
                lblSuccess.Visible = false;
                blstErrores.Items.Clear();
            }
            UpdateCalled = true;

            populateCategories(CategoryDropdown.SelectedItem != null
                                   ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                   : ProductInfoCatalog.RootCategories[0]);

        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartDeleted)]
        public void OnShoppingCartDeleted(object sender, EventArgs e)
        {
            try
            {
                ClearMessages(sender, e);
            }
            catch
            {
            }
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressPopupClosed)]
        public void OnShippingAddressPopupClosed(object sender, EventArgs e)
        {
            bool bNoDeliveryOptionInfo = Session[ViewStateNoDeliveryOptionInfo] == null ? false : true;
            if (bNoDeliveryOptionInfo)
            {
                Session[ViewStateNoDeliveryOptionInfo] = null;
                if (!NoDeliveryOptionInfo())
                {
                    OnAddToCart(this, null);
                    clearQuantity();
                }
            }
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void ClearMessages(object sender, EventArgs e)
        {
            blstErrores.DataSource = string.Empty;
            blstErrores.DataBind();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartDeleted)]
        public void ShoppingCartDeleted(object sender, EventArgs e)
        {
            ClearMessages(sender, e);
        }

        #region Nested type: AddItemsDelegate

        /// <summary>
        ///     The add items delegate.
        /// </summary>
        /// <param name="sku">
        ///     The sku.
        /// </param>
        /// <param name="quantity">
        ///     The quantity.
        /// </param>
        private delegate void AddItemsDelegate(string sku, int quantity, bool partialBackordered);

        #endregion Nested type: AddItemsDelegate

        internal class tmpClass
        {
            public ShoppingCartItem_V01 ShoppingCartItem { get; set; }

            public Label LabelControl { get; set; }

            public Label VolumeLabelControl { get; set; }
        };

        protected void OnDupeOrderOK(object sender, EventArgs e)
        {
            dupeOrderPopupExtender.Hide();
            var pendingOrderNumber = ViewState["pendingOrderNumber"];
            var checkOrderSubmitted = ViewState["isOrderSubmitted"];
            if (checkOrderSubmitted != null)
            {
                RedirectPendingOrder(pendingOrderNumber.ToString(), (bool)checkOrderSubmitted);
            }
        }

        protected void HidePromoMsg(object sender, EventArgs e)
        {
            promoSkuPopupExtender.Hide();
        }
    }
}
