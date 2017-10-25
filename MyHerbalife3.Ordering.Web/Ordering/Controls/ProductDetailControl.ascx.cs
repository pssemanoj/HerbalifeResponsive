using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Web.Ordering.Helpers;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    /// <summary>
    ///     The product detail control base.
    /// </summary>
    public class ProductDetailControlBase : UserControlBase
    {
        /// <summary>
        ///     The backorderMessages.
        /// </summary>
        protected List<string> _backorderMessages = new List<string>();

        /// <summary>
        ///     The _errors.
        /// </summary>
        protected List<string> _errors = new List<string>();

        /// <summary>
        ///     The friendlyMessages.
        /// </summary>
        protected List<string> _friendlyMessages = new List<string>();

        /// <summary>
        ///     Gets or sets a value indicating whether FromPopup.
        /// </summary>
        private bool _fromPopup;

        /// <summary>
        ///     The _show all inventry.
        /// </summary>
        protected bool _showAllInventry = true;

        public bool FromPopup
        {
            get { return _fromPopup; }
            set
            {
                _fromPopup = value;
                SetPrintThisPageVisiblity(value);
            }
        }

        /// <summary>
        ///     Gets or sets CategoryID.
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        ///     Gets or sets ProductID.
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        ///     ProductTableColumns
        /// </summary>
        protected int ProductTableColumns { get; set; }

        public bool UpdateCalled { get; set; }

        public virtual void SetPrintThisPageVisiblity(bool fromPopup)
        {
        }

        public string imgFavorON
        {
            get { return "/Ordering/Images/icons/icn_fav_on.png"; }
        }

        public string imgFavorOFF
        {
            get { return "/Ordering/Images/icons/icn_fav_off.png"; }
        }

        #region AdobeTarget_Salesforce
        public string AT_id = string.Empty;
        public string AT_item = string.Empty;
        public string AT_categories = string.Empty;
        public decimal AT_value = 0.0M;
        public int AT_inventory = 0;
        #endregion

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
        ///     The get current category.
        /// </summary>
        /// <param name="categoryID">
        ///     The category id.
        /// </param>
        /// <returns>
        /// </returns>
        protected Category_V02 getCurrentCategory(int categoryID)
        {
            foreach (Category_V02 cat in ProductInfoCatalog.RootCategories)
            {
                var categoryToFind = findCategoryByID(categoryID, cat);
                if (categoryToFind != null)
                    return categoryToFind;
            }

            return null;
        }

        protected Category_V02 findCategoryByProductID(int productID, out ProductInfo_V02 product)
        {
            product = null;
            foreach (Category_V02 cat in ProductInfoCatalog.RootCategories)
            {
                var categoryToFind = findCategoryByProductID(cat, productID, out product);
                if (categoryToFind != null)
                    return categoryToFind;
            }

            return null;
        }

        protected Category_V02 findCategoryByProductID(Category_V02 thisCategory,
                                                       int productID,
                                                       out ProductInfo_V02 product)
        {
            product = null;
            ProductInfo_V02 productFound = null;
            if ((productFound = getCurrentProduct(thisCategory, productID)) != null)
            {
                product = productFound;
                return thisCategory;
            }
            if (thisCategory.SubCategories != null)
            {
                foreach (Category_V02 cat in thisCategory.SubCategories)
                {
                    var categoryToFind = findCategoryByProductID(cat, productID, out product);
                    if (categoryToFind != null)
                        return categoryToFind;
                }
            }

            return null;
        }

        protected Category_V02 findCategoryByID(int categoryID, Category_V02 thisCategory)
        {
            if (thisCategory.ID == categoryID)
                return thisCategory;
            if (thisCategory.SubCategories != null)
            {
                foreach (Category_V02 c in thisCategory.SubCategories)
                {
                    var catFound = findCategoryByID(categoryID, c);
                    if (catFound != null)
                    {
                        return catFound;
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     The get current product.
        /// </summary>
        /// <param name="currCategory">
        ///     The curr category.
        /// </param>
        /// <param name="productID">
        ///     The product id.
        /// </param>
        /// <returns>
        /// </returns>
        protected ProductInfo_V02 getCurrentProduct(Category_V02 currCategory, int productID)
        {
            ProductInfo_V02 info = null;

            if (currCategory == null || currCategory.Products == null)
            {
                return info;
            }
                var varProd = from c in currCategory.Products
                              where c.ID == productID
                              select c;

            if (varProd != null && varProd.Count() > 0)
            {
                info = varProd.First();
            }
            else
            {
                //bug on the parameter  it should be 1 not 2
                LoggerHelper.Warn(string.Format("Product Not Found for ProductId {0}, currentCategory Display Name:{1}", productID, currCategory.DisplayName));
            }

            return info;
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
            return availType != ProductAvailabilityType.Unavailable;
        }

        protected string IsDisplay()
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                return "";
            else
                return "display:none";
        }

        protected bool getVisible(ProductAvailabilityType availType)
        {
            if (HLConfigManager.Configurations.DOConfiguration.DisplayBackOrderEnhancements)
            {
                if (!getEnabled(availType))
                {
                    return false;
                }
            }
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

        protected decimal getDiscountedPrice(CatalogItem catalogItem)
        {
            decimal retailPrice = catalogItem.ListPrice;
            if (catalogItem.ProductType != ServiceProvider.CatalogSvc.ProductType.Product)
            {
                return retailPrice;
            }
            decimal discount;
            var total = ShoppingCart.Totals;

            if (total == null)
            {
                discount = decimal.Parse(ProductsBase.DistributorDiscount.ToString());
            }
            else
            {
                discount = (total as OrderTotals_V01).DiscountPercentage;
            }

            if (discount == 0)
            {
                return Math.Round(retailPrice, 0, 0); //formatting as per OrderTotals.
            }
            decimal result = Math.Round(retailPrice*(100 - discount)/100.0M, 0, 0);
            return result; //formatting as per OrderTotals.
        }

        /// <summary>
        ///     The get default sku image path.
        /// </summary>
        /// <param name="product">
        ///     The product.
        /// </param>
        /// <returns>
        ///     The get default sku image path.
        /// </returns>
        protected string getDefaultSKUImagePath(ProductInfo_V02 product)
        {
            if (product.DefaultSKU != null)
            {
                return product.DefaultSKU.ImagePath;
            }

            return string.Empty;
        }

        /// <summary>
        ///     The display cross sell product.
        /// </summary>
        /// <param name="currCategory">
        ///     The curr category.
        /// </param>
        /// <param name="currProduct">
        ///     The curr product.
        /// </param>
        protected void displayCrossSellProduct(Category_V02 currCategory, ProductInfo_V02 currProduct)
        {
            // display cross sell if only
            CrossSellInfo cs;
            if ((cs = findCrossSellProduct(currCategory, currProduct)) != null)
            {
                var category = CatalogHelper.FindCategory(ProductInfoCatalog, cs.CategoryID);
                if (category != null)
                {
                    Session[ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY] = cs;
                    Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] = cs;

                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(cs));
                    return;
                }
            }

            NoCrossSellFound(this, null);
        }

        /// <summary>
        ///     The find cross sell product.
        /// </summary>
        /// <param name="currCategory">
        ///     The curr category.
        /// </param>
        /// <param name="currProduct">
        ///     The curr product.
        /// </param>
        /// <returns>
        /// </returns>
        protected CrossSellInfo findCrossSellProduct(Category_V02 currCategory, ProductInfo_V02 currProduct)
        {
            CrossSellInfo candidate = null;
            if (null != currCategory && null != currProduct)
            {
                if (currProduct.CrossSellProducts != null && currProduct.CrossSellProducts.Count > 0)
                {
                    //if (currProduct.CrossSellProducts.Count > 1)
                    {
                        foreach (int catalogID in currProduct.CrossSellProducts.Keys)
                        {
                            foreach (ProductInfo_V02 c in currProduct.CrossSellProducts[catalogID])
                            {
                                // don't pick if it is the same as current prodcut
                                if (c.ID != currProduct.ID)
                                {
                                    if (ProductDetail.ShouldSelectThisCrossSell(c, ShoppingCart,
                                                                                Session[
                                                                                    ProductDetail.
                                                                                        LAST_SEEN_PRODUCT_SESSION_EKY]
                                                                                as
                                                                                CrossSellInfo,
                                                                                Session[
                                                                                    ProductDetail.
                                                                                        LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                                                                                    ] as CrossSellInfo))
                                    {
                                        if (!ProductDetail.IfOutOfStock(c, AllSKUS))
                                        {
                                            candidate = new CrossSellInfo(catalogID, c);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // if nothing found
                        if (candidate == null)
                        {
                            var crossSellList = new List<CrossSellInfo>();
                            ProductDetail.CollectAllCrossSellProducts(currCategory, crossSellList);
                            candidate = ProductDetail.SelectCrossSellFromPreviousDisplayOrCrssSellList(ShoppingCart,
                                                                                                       crossSellList,
                                                                                                       Session[
                                                                                                           ProductDetail
                                                                                                               .
                                                                                                               LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                                                                                                           ] as
                                                                                                       CrossSellInfo,
                                                                                                       AllSKUS);
                        }
                    }
                    //else
                    //{
                    // only one
                    //    candidate = new CrossSellInfo(currCategory.ID, currProduct.CrossSellProducts.First().Value.First());
                    //}

                    return candidate;
                }
                else
                {
                    // find all products within the same category except the product clicked
                    // get a list of all cross sell product ids within these products
                    var crossSellsOfproducts = new List<CrossSellInfo>();
                    ProductDetail.CollectAllCrossSellProducts(currCategory, crossSellsOfproducts);

                    bool crossSellFound = false;
                    var randomList = new List<CrossSellInfo>();
                    if (crossSellsOfproducts.Count > 0)
                    {
                        if (crossSellsOfproducts.Count == 1)
                        {
                            candidate = crossSellsOfproducts[0];
                            return candidate;
                        }

                        while (crossSellFound == false)
                        {
                            var r = new Random();
                            int numProd = r.Next(0, crossSellsOfproducts.Count);
                            var csToEvaluate = crossSellsOfproducts[numProd];
                            randomList.Add(crossSellsOfproducts[numProd]);
                            crossSellsOfproducts.RemoveAt(numProd);
                            if (ProductDetail.ShouldSelectThisCrossSell(csToEvaluate.Product, ShoppingCart,
                                                                        Session[
                                                                            ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY]
                                                                        as CrossSellInfo,
                                                                        Session[
                                                                            ProductDetail.
                                                                                LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                                                                            ] as
                                                                        CrossSellInfo))
                            {
                                if (!ProductDetail.IfOutOfStock(csToEvaluate.Product, AllSKUS))
                                {
                                    candidate = csToEvaluate;
                                    crossSellFound = true;
                                    break;
                                }
                            }

                            if (crossSellsOfproducts.Count == 0)
                            {
                                break;
                            }
                        }

                        return candidate;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     The get id from query string.
        /// </summary>
        protected void getIDFromQueryString()
        {
            string strProductID = HttpContext.Current.Request.QueryString["ProdInfoID"] ?? string.Empty;
            int productID = 0, categoryID = 0;

            if (!string.IsNullOrEmpty(strProductID))
            {
                if(int.TryParse(strProductID, out productID))
                {
                ProductID = productID;
            }
            }

            string strCategoryID = HttpContext.Current.Request.QueryString["CategoryID"] ?? string.Empty;
            if (!string.IsNullOrEmpty(strCategoryID))
            {
                if (int.TryParse(strCategoryID, out categoryID))
                {
                CategoryID = categoryID;
            }
        }
        }

        /// <summary>
        ///     The load product.
        /// </summary>
        /// <param name="categoryID">
        ///     The category id.
        /// </param>
        /// <param name="productID">
        ///     The product id.
        /// </param>
        public virtual void LoadProduct(int categoryID, int productID, bool loadCrossSell)
        {
        }

        public virtual void ResetInventory()
        {
        }

        public virtual void ReloadProduct()
        {
        }

        protected virtual void dataBind(Category_V02 currCategory, ProductInfo_V02 currProduct)
        {
        }

        public void Hide()
        {
            if (PopupExtender != null)
                PopupExtender.ClosePopup();
        }
    }

    /// <summary>
    ///     The product detail control.
    /// </summary>
    public partial class ProductDetailControl : ProductDetailControlBase
    {
        // static string SKUGridID = "Products";
        private static string SKUControlID = "ProductID";
        private static string QtyControlID = "Quantity";
        private List<FavouriteSKU> favourSKUs;
        private FavouriteSkuLoader favourLoader;

        /// <summary>
        ///     The show back order msgs.
        /// </summary>
        [Publishes(MyHLEventTypes.ShowBackorderMessages)]
        public event EventHandler OnShowBackorderMessages;

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
            _showAllInventry = ProductsBase.SessionInfo.ShowAllInventory;
            ProductTableColumns = 8;
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase ||
                SessionInfo.IsEventTicketMode)
            {
                ProductTableColumns--;
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                favourLoader = new FavouriteSkuLoader();
                favourSKUs = favourLoader.GetDistributorFavouriteSKU(DistributorID, Thread.CurrentThread.CurrentCulture.Name);

                if (lbVolumePoint != null)
                {
                    lbVolumePoint.Text = "0.00";
                }
            }

            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.HasDiscountedPriceForEventTicket &&
                SessionInfo.IsEventTicketMode)
            {
                ProductTableColumns--;
            }

            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayVolumePointsForEventTicket &&
                SessionInfo.IsEventTicketMode)
            {
                ProductTableColumns--;
            }

            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount)
            {
                ProductTableColumns--;
            }
            getIDFromQueryString();

            // from search
            if (CategoryID == 0 && ProductID != 0)
            {
                ProductInfo_V02 prodInfo = null;
                var category = findCategoryByProductID(ProductID, out prodInfo);
                if (category != null)
                {
                    CategoryID = category.ID;
                }
            }
            if (CategoryID == 0 || ProductID == 0)
            {
                return;
            }

            if (!IsPostBack)
            {
                if (FromPopup)
                {
                    divProductDetail.Style.Add("max-height", "650px");
                    divProductDetail.Style.Add("overflow", "auto");
                }
                else
                {
                    if (CancelButton != null)
                    {
                        CancelButton.Visible = false;
                    }
                    if (popupClose != null)
                    {
                        popupClose.Visible = false;
                    }
                }
                LoadProduct(CategoryID, ProductID, !FromPopup);
                if (lbVolumePoint != null)
                    lbVolumePoint.Text = "0.00";
            }
            else
            {
                var prodInfo = Session["CurrentProduct"] as ProductInfo_V02;
                if (prodInfo != null)
                {
                    ProductInfoFooter1.ProdDetails = new ProductDetailFooter(prodInfo);
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("jQuery"))
                    this.Page.ClientScript.RegisterClientScriptInclude("jQuery", Page.ResolveUrl("~/Ordering/Scripts/ProductFavourite.js"));

                if (SessionInfo != null && !SessionInfo.IsEventTicketMode)
                {
                    this.CnChkout24h.AddToCartControlList = new List<System.Web.UI.Control>
                        {
                            this.DynamicButton2,
                            this.CheckoutButton,
                        };
                }
                DynamicButton3.Visible = recalcVPSubmit.Visible = false; // Hide Recalculate VP buttons
                TotalAmountText.Visible = lbTotalAmount.Visible = true; // Show Total Ammount

                if (DistributorOrderingProfile.IsPC)
                {
                    VolumePointText.Visible = lbVolumePoint.Visible = false;
                }
            }

            if (!HLConfigManager.Configurations.DOConfiguration.IsChina
                && HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                && SessionInfo.IsEventTicketMode
                && Locale.Substring(3).Equals("MX"))
            {
                VolumePointText.Visible = lbVolumePoint.Visible = false;
            }

            if (Session["CurrentProduct"] != null)
            {
                var product = Session["CurrentProduct"] as ProductInfo_V02;

                if (product != null && product.DisplaySizeChart == true)
                {
                    divProductDetail.Attributes.Add("class", string.Format("{0} apparel-product", divProductDetail.Attributes["class"]));
                    recalcVPSubmit.Visible = DynamicButton1.Visible = DynamicButton2.Visible = false;
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
            lbFastFacts.Visible = HLConfigManager.Configurations.DOConfiguration.IsChina ? false : true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            CheckoutButton.Text =
                CheckoutButton1.Text = GetLocalResourceObject("CheckoutButtonResource1.Text") as string;
            if (UpdateCalled)
            {
                upProductDetail.Update();
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                DynamicButton3.Visible = recalcVPSubmit.Visible = false; // Hide Recalculate VP buttons
                TotalAmountText.Visible = lbTotalAmount.Visible = true; // Show Total Ammount

                if (DistributorOrderingProfile.IsPC)
                {
                    VolumePointText.Visible = lbVolumePoint.Visible = false;
                }
            }
        }

        public override void SetPrintThisPageVisiblity(bool fromPopup)
        {
            ProdImage.EnlargeImage(fromPopup);
            if (fromPopup)
            {
                ProdImage.Visible = false;
            }
            }

        /// <summary>
        ///     The load product.
        /// </summary>
        /// <param name="categoryID">
        ///     The category id.
        /// </param>
        /// <param name="productID">
        ///     The product id.
        /// </param>
        public override void LoadProduct(int categoryID, int productID, bool loadCrossSell)
        {
            // clear errors
            _errors.Clear();
            _backorderMessages.Clear();
            uxErrores.DataSource = _errors;
            uxErrores.DataBind();

            CategoryID = categoryID;
            ProductID = productID;

            try
            {
                CheckoutButton.Text = GetLocalResourceObject("CheckoutButtonResource1.Text") as string;
                if (ProductInfoCatalog != null)
                {
                    var currCategory = getCurrentCategory(CategoryID);
                    var currProduct = getCurrentProduct(currCategory, ProductID);

                    if (currProduct != null && currProduct.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket && !SessionInfo.IsEventTicketMode)
                    {
                        string url = string.Format("{0}&ETO=TRUE", Request.RawUrl);
                        Response.Redirect(url);
                        return;
                    }
                    else if (currProduct != null && currProduct.TypeOfProduct != ServiceProvider.CatalogSvc.ProductType.EventTicket && SessionInfo.IsEventTicketMode)
                    {
                        string url = string.Format("{0}&ETO=FALSE", Request.RawUrl);
                        Response.Redirect(url);
                        return;
                    }

                    if (currProduct != null)
                    {
                        Session["CurrentProduct"] = currProduct;

                        OmnitureHelper.RegisterOmnitureProductsScript(Parent.Page, currProduct.DisplayName, currProduct.SKUs, currCategory.ID.ToString());
                        populateProducts(currProduct);

                        var rootCategory = CatalogHelper.GetRootCategory(ProductInfoCatalog, currCategory.ID);

                        if (FromPopup == false)
                        {
                            ProductsPanel.PagerCategoryID = currCategory.ID;
                            ProductsPanel.PagerRootCategoryID = rootCategory.ID;
                            ProductsPanel.PopulateProducts(currCategory, rootCategory);
                        }
                        else
                        {
                            ProductsPanel.Visible = false;
                            //ProdImage.Enlarged = true;
                        }

                        dataBind(currCategory, currProduct);
                        ProductInfoFooter1.ProdDetails = new ProductDetailFooter(currProduct);

                        if(!this.IsPostBack && HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations)
                        {
                            // Get Inventory from all warehouses
                            AT_inventory = 0;
                            foreach(var WHInventory in currProduct.DefaultSKU.CatalogItem.InventoryList)
                            {
                                var warehouse = WHInventory.Value as WarehouseInventory_V01;
                                if(warehouse != null)
                                {
                                    AT_inventory += warehouse.QuantityAvailable;
                                }
                            }

                            // Get Categories for Default SKU
                            AT_categories = string.Format("{0}_{1}", Locale, currCategory.DisplayName);

                            AT_id = string.Format("{0}_{1}", Locale, currProduct.DefaultSKU.SKU);
                            AT_item = currProduct.DefaultSKU.Product.ID.ToString();
                            AT_value = currProduct.DefaultSKU.CatalogItem.ListPrice;
                        }

                        if (loadCrossSell)
                        {
                            displayCrossSellProduct(currCategory, currProduct);
                        }
                        ProductAvailability.ShowLabel = true;
                        hdCategoryID.Value = (null != currCategory) ? currCategory.ID.ToString() : "0";
                    }

                    if (_errors.Count > 0)
                    {
                        Products.Visible = false;
                        uxErrores.DataSource = _errors;
                        uxErrores.DataBind();
                    }
                    else
                    {
                        Products.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error((Page as ProductsBase).GetDistributorInfo() + "LoadProduct fails!\n" + ex);
            }
        }

        /// <summary>
        ///     The populate products.
        /// </summary>
        /// <param name="product">
        ///     The product.
        /// </param>
        protected void populateProducts(ProductInfo_V02 product)
        {
            if (product != null)
            {
                bool showAllInventory = ProductsBase.SessionInfo.ShowAllInventory;
                var listChildSKUs = retrieveChildSKUs(product.SKUs);

                var skus =
                    from s in product.SKUs
                    from a in AllSKUS.Keys
                    where
                        s.SKU == a && s.IsDisplayable &&
                        !listChildSKUs.Contains(s)
                        && (showAllInventory ||
                         (showAllInventory == false && s.ProductAvailability != ProductAvailabilityType.Unavailable))
                    select AllSKUS[a];

                if (skus.Count() == 0)
                {
                    _errors.Add(GetLocalResourceObject("OutOfStock") as string);
                }
                else
                {
                    HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart, skus.ToList());
                    CatalogProvider.GetProductAvailability(skus.ToList(), Locale, DistributorID,
                                                           ProductsBase.CurrentWarehouse,
                                                           ShoppingCart.DeliveryInfo != null
                                                               ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                               : DeliveryOptionType.Unknown);
                }

                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {

                    IEnumerable<SKU_V01> SKUFavour = from sk in skus select sk;
                    SKUFavour.ToList().ForEach(x => x.Version = "01");

                    foreach (var s in SKUFavour)
                    {
                        if (favourSKUs != null && favourSKUs.Any(x => x.ProductSKU.Contains(s.SKU)))
                            s.Version = "Favor";
                    }

                    Products.DataSource = SKUFavour;
                    Products.DataBind();
                }
                else
                {
                Products.DataSource = skus;
                Products.DataBind();
                }

                calcDistributorPrice();
            }
        }

        private static List<SKU_V01> retrieveChildSKUs(List<SKU_V01> listSKUs)
        {
            var listChildSKUs = new List<SKU_V01>();
            foreach (SKU_V01 sku in listSKUs)
            {
                if (sku.SubSKUs != null)
                {
                    listChildSKUs.AddRange((from s in sku.SubSKUs
                                            where !listChildSKUs.Contains(s)
                                            select s));
                }
            }
            return listChildSKUs;
        }

        /// <summary>
        ///     The products_ on item created.
        /// </summary>
        /// <param name="Sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void Products_OnItemCreated(object Sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.Header:
                    {
                        var th = e.Item.FindControl("thEarnBase") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                                         !SessionInfo.IsEventTicketMode;
                        }
                        th = e.Item.FindControl("thTitleDiscountedPrice") as HtmlTableCell;
                        if (th != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .HasDiscountedPriceForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                th.Visible = false;
                            }
                            else
                            {
                                th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount;
                            }
                        }
                        th = e.Item.FindControl("thRetailPrice") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice;
                        }
                        th = e.Item.FindControl("thVolumePoint") as HtmlTableCell;
                        if (th != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .DisplayVolumePointsForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                th.Visible = false;
                            }
                            else
                            {
                                th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.HasVolumePoints;
                            }
                            if(th.Visible && DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                                th.Visible = false;
                        }
                        th = e.Item.FindControl("thDoc") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn;
                        }
                        var lbDocRef = e.Item.FindControl("thDoc") as HtmlTableCell;
                        if (lbDocRef != null)
                        {
                            if (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress 
                                && Locale.Substring(3).Equals("MX") 
                                && SessionInfo.IsEventTicketMode)
                            {
                                lbDocRef.Visible = false;
                            }
                            else
                            {
                                lbDocRef.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn;
                            }
                        }
                    }
                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                    {
                        var lb = e.Item.FindControl("lbEarnBase") as Label;
                        if (lb != null)
                        {
                            lb.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                                         !SessionInfo.IsEventTicketMode;
                        }
                        var td = e.Item.FindControl("tdEarnBase") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase &&
                                         !SessionInfo.IsEventTicketMode;
                        }
                        td = e.Item.FindControl("tdDiscountedPrice") as HtmlTableCell;
                        if (td != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .HasDiscountedPriceForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                td.Visible = false;
                            }
                            else
                            {
                                td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount;
                            }
                        }
                        td = e.Item.FindControl("tdRetailPrice") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice;
                        }
                        td = e.Item.FindControl("tdVolumePoint") as HtmlTableCell;
                        if (td != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .DisplayVolumePointsForEventTicket && SessionInfo.IsEventTicketMode)
                            {
                                td.Visible = false;
                            }
                            else
                            {
                                td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.HasVolumePoints;
                            }
                            if (td.Visible && DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                                td.Visible = false;
                        }
                        td = e.Item.FindControl("tdPdf") as HtmlTableCell;
                        if (td != null)
                        {
                            if (Locale.Substring(3).Equals("MX") && SessionInfo.IsEventTicketMode)
                            {
                                td.Visible = false;
                            }
                            else
                            {
                                td.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn;
                            }
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        protected override void dataBind(Category_V02 currCategory, ProductInfo_V02 currProduct)
        {
            if (currProduct != null)
            {
                if (currProduct.Links != null && currProduct.Links.Count > 0)
                {
                    ProductLinks.Visible = true;
                    ProductLinks.DataBind(currProduct.Links);
                }
                else
                {
                    ProductLinks.Visible = false;
                }

                // check hot key
                if (!string.IsNullOrEmpty(currProduct.HotKeys))
                {
                    var si = new ScriptInjecter();
                    si.Text = currProduct.HotKeys;
                    divAddscript.Controls.Add(si);
                }

                uxProductName.InnerHtml = currProduct.DisplayName;

                tabone.Visible = !string.IsNullOrEmpty(currProduct.Overview) ||
                                 !string.IsNullOrEmpty(currProduct.Benefits);
                if (tabone.Visible)
                {
                    // key benefits
                    if (!string.IsNullOrEmpty(currProduct.Benefits))
                    {
                        uxOverview.InnerHtml = currProduct.Overview;
                        pBenefits.InnerHtml = currProduct.Benefits;
                        DivOverviewTab1.Visible = true;
                        DivOverviewTab2.Visible = false;
                    }
                    else
                    {
                        uxOverview2.InnerHtml = currProduct.Overview;
                        DivOverviewTab2.Visible = true;
                        DivOverviewTab1.Visible = false;
                    }
                }

                tabtwo.Visible = !string.IsNullOrEmpty(currProduct.Details) || !string.IsNullOrEmpty(currProduct.Usage);

                if (tabtwo.Visible)
                {
                    if (!string.IsNullOrEmpty(currProduct.Usage))
                    {
                        pDetails.InnerHtml = currProduct.Details;
                        pUsage.InnerHtml = currProduct.Usage;
                        DivUsageTab1.Visible = true;
                        DivUsageTab2.Visible = false;
                    }
                    else
                    {
                        pDetails2.InnerHtml = currProduct.Details;
                        DivUsageTab2.Visible = true;
                        DivUsageTab1.Visible = false;
                    }
                }

                tabthree.Visible = !string.IsNullOrEmpty(currProduct.FastFacts);
                if (tabthree.Visible)
                {
                    DivFastFactsTab2.Visible = true;
                    DivFastFactsTab1.Visible = false;
                    pQuickFacts.InnerHtml = pQuickFacts2.InnerHtml = currProduct.FastFacts;
                }

                ProdImage.ImageName = getDefaultSKUImagePath(currProduct);

                //ProductInfoFooter1.ProdDetails = new ProductDetailFooter(currProduct);
                                
                if (currProduct.DisplaySizeChart)                
                {
                    sizeChartTable.Visible = showSizeChart(currProduct.ID);                    
                }
                else
                {
                    sizeChartTable.Visible = false;
                }
            }
        }

        /// <summary>
        ///     The add items.
        /// </summary>
        /// <param name="dlg">
        ///     The dlg.
        /// </param>
        protected void addItems(AddItemsDelegate dlg)
        {
            bool bNoItemSelected = true;
            foreach (object s in Products.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var uxQuantity = item.FindControl("Quantity") as TextBox;
                    int quantity = 0;
                    if (uxQuantity.Text != string.Empty)
                    {
                        bNoItemSelected = false;
                        string productID = (item.FindControl("ProductoID") as HiddenField).Value;

                        if (!int.TryParse(uxQuantity.Text, out quantity) || quantity <= 0)
                        {
                            _errors.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "QuantityIncorrect"), productID));
                            //if (!FromPopup)
                                uxQuantity.Text = string.Empty;
                        }
                        else
                        {
                            SKU_V01 sku01;
                            if (AllSKUS.TryGetValue(productID, out sku01))
                            {
                                if (CatalogProvider.GetProductAvailability(sku01, ProductsBase.CurrentWarehouse,
                                                                           ShoppingCart.DeliveryInfo != null
                                                                               ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                               : DeliveryOptionType.Unknown) ==
                                    ProductAvailabilityType.Unavailable)
                                {
                                    _errors.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), productID));
                                    continue;
                                }

                                int availQuantity = 0;
                                int backorderCoverage = 0;
                                if (ProductsBase.CheckMaxQuantity(ShoppingCart.CartItems, quantity, sku01, _errors))
                                {
                                    backorderCoverage = ProductsBase.CheckBackorderCoverage(quantity, sku01,
                                                                                            _friendlyMessages);
                                    if (backorderCoverage == 0)
                                    {
                                        int allQuanties = ProductsBase.GetAllQuantities(ShoppingCart.CartItems, quantity,
                                                                                        sku01.SKU);
                                        availQuantity = ProductsBase.CheckInventory(allQuanties, sku01, _errors);
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
                            }
                            //if (!FromPopup)
                                uxQuantity.Text = string.Empty;
                        }
                    }
                }
            }

            if (bNoItemSelected)
            {
                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoItemsSelected"));
            }
        }

        protected void refreshIndicator()
        {
            foreach (object s in Products.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var pv = item.FindControl("ProductAvailibility") as ProductAvailability;
                    if (pv != null)
                    {
                        var skuFld = item.FindControl("ProductoID") as HiddenField;
                        if (skuFld != null)
                        {
                            SKU_V01 sku;
                            if (AllSKUS.TryGetValue(skuFld.Value, out sku))
                            {
                                HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart,
                                                                                       new List<SKU_V01> {sku});
                                CatalogProvider.GetProductAvailability(sku, ProductsBase.CurrentWarehouse,
                                                                       ShoppingCart.DeliveryInfo != null
                                                                           ? (DeliveryOptionType)Enum.Parse(typeof(DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                           : DeliveryOptionType.Unknown);
                                pv.Available = sku.ProductAvailability;
                            }
                        }
                    }

                    var qtyTextBox = item.FindControl("Quantity") as TextBox;
                    if (qtyTextBox != null)
                    {
                        var skuFld = item.FindControl("ProductoID") as HiddenField;
                        if (skuFld != null)
                        {
                            SKU_V01 sku;
                            if (AllSKUS.TryGetValue(skuFld.Value, out sku))
                            {
                                HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart,
                                                                                       new List<SKU_V01> {sku});
                                CatalogProvider.GetProductAvailability(sku, ProductsBase.CurrentWarehouse);

                                if (!getEnabled(sku.ProductAvailability) && HLConfigManager.Configurations.DOConfiguration.DisplayBackOrderEnhancements)
                                {
                                    var linkDetails = item.FindControl("LinkBackOrderDetails");
                                    if (linkDetails != null)
                                    {
                                        qtyTextBox.Visible = false;
                                        linkDetails.Visible = true;
                                    }
                                }
                                else
                                {
                                qtyTextBox.Enabled = getEnabled(sku.ProductAvailability);
                                qtyTextBox.BackColor = getEnabled(sku.ProductAvailability)
                                                           ? ColorTranslator.FromHtml("white")
                                                           : ColorTranslator.FromHtml("#CCCCCC");
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void clearQuantity()
        {
            foreach (object s in Products.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var qtyTextBox = item.FindControl("Quantity") as TextBox;
                    if (qtyTextBox != null)
                    {
                        qtyTextBox.Text = string.Empty;
                    }
                }
            }
        }

        protected void calcDistributorPrice()
        {
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayDiscount)
                return;
            var products = new List<tmpClass>();
            foreach (object s in Products.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    string productID = (item.FindControl("ProductoID") as HiddenField).Value;
                    {
                        var discountedPrice = item.FindControl("DiscountedPrice") as Label;
                        if (discountedPrice != null)
                        {
                            products.Add(new tmpClass
                                {
                                    ShoppingCartItem = new ShoppingCartItem_V01
                                        {
                                            SKU = productID,
                                            Quantity = 1,
                                            Updated = DateTime.Now,
                                        },
                                    LabelControl = discountedPrice
                                });

                            var volumePoints = (Label) item.FindControl("VolumePoints");
                            if (volumePoints != null)
                            {
                                products.Last().VolumeLabelControl = volumePoints;
                            }
                        }
                    }
                }
            }
            if (products.Count > 0)
            {
                var totals = ShoppingCart.Calculate(products.Select(p => p.ShoppingCartItem).ToList());
                if (totals != null)
                {
                    foreach (tmpClass t in products)
                    {
                        if (t.LabelControl.Text == string.Empty)
                            continue;
                        var lineItem =
                            (totals as OrderTotals_V01).ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == t.ShoppingCartItem.SKU) as
                            ItemTotal_V01;
                        if (lineItem != null)
                        {
                            decimal lineItemTotal =
                                HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges
                                    ? OrderProvider.getPriceWithAllCharges(totals as OrderTotals_V01, t.ShoppingCartItem.SKU, 1)
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
            }
        }

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
                }
            }
            else if (_errors.Count > 0)
            {
                lbVolumePoint.Text = "0.00";
            }

            uxErrores.DataSource = _errors;
            uxErrores.DataBind();
        }

        /// <summary>
        ///     handler for "Add to cart"
        /// </summary>
        /// <param name="Source">
        /// </param>
        /// <param name="e">
        /// </param>
        protected void AddToCart(object Source, EventArgs e)
        {
            _errors.Clear();
            _friendlyMessages.Clear();
            _backorderMessages.Clear();
            CheckoutButton.Text = GetLocalResourceObject("CheckoutButtonResource1.Text") as string;
            if (FromPopup)
            {
                if (ShoppingCart.DeliveryInfo == null)
                {
                    _errors.Add(GetLocalResourceObject("NoShippingInfo") as string);
                    uxErrores.DataSource = _errors;
                    uxErrores.DataBind();
                    return;
                }
            }
            if (ProductsBase.CanAddProductFromPopup(DistributorID, ref _errors))
            {
                var products = new List<ShoppingCartItem_V01>();
                addItems(
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
                    }
                    
                    if (products.Count > 0)
                    {
                        ProductsBase.AddItemsToCart(products);
                        if (ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback))
                        {
                            if (ShoppingCart.RuleResults.Any(r => r.Result == RulesResult.Feedback && r.Messages != null && r.Messages.Count > 0))
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
                                    string feedback =
                                        ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Feedback)
                                                    .First()
                                                    .Messages[0
                                            ];
                                    if (!string.IsNullOrEmpty(feedback))
                                    {
                                        string title = PlatformResources.GetGlobalResourceString("GlobalResources", "FeedbackNotification");
                                        if (IsChina) title = GetGlobalResourceString("FeedbackNotification");
                                        (Master).ShowMessage(title, feedback);
                                    }
                                }
                            }
                            if (IsChina && (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC || (SessionInfo != null && SessionInfo.ReplacedPcDistributorOrderingProfile != null && SessionInfo.ReplacedPcDistributorOrderingProfile.IsPC)))
                            {
                                if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                                {
                                    var ruleResultMsgs = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                                .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                                    _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                                }
                            }
                        }
                        else
                        {
                            if (IsChina)
                            {
                                if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                                {
                                    var ruleResultMsgs = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                                .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                                    _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                                }
                            }
                            else
                            {
                                if (
                                    ShoppingCart.RuleResults.Any(
                                        rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                                {
                                    PurchasingLimitProvider.SetPostErrorRemainingLimitsSummaryMessage(ShoppingCart);
                                }
                            }
                            if (ShoppingCart.RuleResults.Any(
                                       rs =>
                                       rs.RuleName == "PurchaseRestriction Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                            {
                                var ruleResultMsgs =
                                ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchaseRestriction Rules")
                                            .Select(r => r.Messages);
                                  _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchaseRestriction Rules");

                            }
                            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                            {
                                if (
                                   ShoppingCart.RuleResults.Any(
                                       rs => rs.Result == RulesResult.Failure))
                                {
                                    var ruleResultMessages =
                                        ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.Messages != null && r.Messages.Count > 0 && (r.RuleName == "Promotional Rules" || r.RuleName == "ETO Rules"));
                                    if (ruleResultMessages != null)
                                    {
                                        foreach (ShoppingCartRuleResult message in ruleResultMessages)
                                        {
                                            if (message.Messages != null && message.Messages.Any())
                                                _errors.AddRange(message.Messages.Select(x => x.ToString()));

                                        }
                                    }
                                }

                            }
                            else
                            {
                            var ruleResultMessages = (from r in ShoppingCart.RuleResults
                                                      where
                                                              r.Result == RulesResult.Failure && r.RuleName != "Back Order" && r.Messages !=null &&r.Messages.Any()
                                                      select r.Messages[0]);
                            if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                            {
                                _errors.AddRange(ruleResultMessages.Distinct().ToList());
                            }
                            }


                           var ruleResultMessage =
                                (from r in ShoppingCart.RuleResults
                                 where r.Result == RulesResult.Failure && r.RuleName == "Back Order" && r.Messages !=null&&r.Messages.Any()
                                 select r.Messages[0]);
                            if (null != ruleResultMessage && ruleResultMessage.Count() > 0)
                            {
                                _backorderMessages.AddRange(ruleResultMessage.Distinct().ToList());
                            }
                        }

                        if (FromPopup && _errors.Count == 0)
                        {
                            //Hide();
                            if (_friendlyMessages.Count > 0)
                            {
                                OnShowBackorderMessages(this, new ShowBackorderMessagesEventArgs(_friendlyMessages));
                            }
                        }

                        ShoppingCart.RuleResults.Clear();
                    }

                    OmnitureHelper.RegisterOmnitureAddCartScript(Page, ShoppingCart.ShoppingCartItems, products);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error((Page as ProductsBase).GetDistributorInfo() + "Can't add to cart!\n" + ex);
                }
            }
            if (!FromPopup)
            {
                _errors.AddRange(_backorderMessages);
                if (_friendlyMessages.Count > 0)
                {
                    ProductsBase.ShowBackorderMessage(_friendlyMessages);
                }

                if (ProductsBase.CantBuy)
                {
                    var message = PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder");
                    var listMessages = (Master as OrderingMaster).Status.GetMessages();
                    if (!listMessages.Any(x => x.MessageText == message.ToString()))
                    {
                        (Page.Master as OrderingMaster).Status.AddMessage(message);
                    }
                    return;
                }
            }          
            uxErrores.DataSource = _errors;
            uxErrores.DataBind();

         
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
                Promotion_MY_Control.ShowPromo();
                ShoppingCart.DisplayPromo = false;
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                int count = 0;
                bool display = false;
                var popUpMessage = Providers.China.CatalogProvider.GetSlowMovingPopUp(ShoppingCart, AllSKUS, out count, out display);
                if (display && count > 0)
                {
                    lblSlowMovingDescription.Text = popUpMessage;
                    SlowmovingSkuPopupExtender.Show();
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
            
        }

        protected void HidePromoMsg(object sender, EventArgs e)
        {
            promoSkuPopupExtender.Hide();
            Hide();
        }

        /// <summary>
        ///     The on cancel.
        /// </summary>
        /// <param name="Source">
        ///     The source.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnCancel(object Source, EventArgs e)
        {
            Hide();
        }

        private void AddItemsForCalc(bool bClearTextField, AddItemsDelegate dlg)
        {
            if (ShoppingCart != null)
            {
                bool bNoItemSelected = true;
                foreach (object s in Products.Items)
                {
                    var item = s as RepeaterItem;
                    if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                    {
                        var uxQuantity = item.FindControl(QtyControlID) as TextBox;
                        int quantity = 0;
                        if (uxQuantity.Text != string.Empty)
                        {
                            bNoItemSelected = false;
                            string productID = (item.FindControl(SKUControlID) as HiddenField).Value;
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
                // foreach
                if (bNoItemSelected)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoItemsSelected"));
                }
            }
        }

        public override void ResetInventory()
        {
            getIDFromQueryString();
            refreshIndicator();
            ProductAvailability.ShowLabel = true;
            //upProductDetail.Update();
            UpdateCalled = true;
        }

        public override void ReloadProduct()
        {
            getIDFromQueryString();
            LoadProduct(CategoryID, ProductID, true);
            UpdateCalled = true;
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

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            ReloadProduct();
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void ClearMessages(object sender, EventArgs e)
        {
            uxErrores.DataSource = string.Empty;
            uxErrores.DataBind();

            UpdateCalled = true;
            //upProductDetail.Update();
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressPopupClosed)]
        public void OnShippingAddressPopupClosed(object sender, EventArgs e)
        {
            bool bNoDeliveryOptionInfo = Session[ProductsBase.ViewStateNoDeliveryOptionInfo] == null ? false : true;
            if (bNoDeliveryOptionInfo)
            {
                Session[ProductsBase.ViewStateNoDeliveryOptionInfo] = null;
                if (!ProductsBase.NoDeliveryOptionInfo())
                {
                    AddToCart(this, null);
                    clearQuantity();
                }
            }
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
        protected delegate void AddItemsDelegate(string sku, int quantity, bool partialBackordered);

        #endregion

        internal class tmpClass
        {
            public ShoppingCartItem_V01 ShoppingCartItem { get; set; }
            public Label LabelControl { get; set; }
            public Label VolumeLabelControl { get; set; }
        };
        protected void HideSlowMovingMsg(object sender, EventArgs e)
        {
            SlowmovingSkuPopupExtender.Hide();
        }

        private bool showSizeChart(int productID)
        {
            bool showChart = false;
            
            if (tblSizeChart.Rows.Count > 0)   //means table was already populated and this is the 2nd call to this method
            {
                return true;
            }
            else
            {
                SizeChart_V01 sizeChart = CatalogProvider.GetSizeChartByProductID(Locale, productID);
                if (sizeChart != null)
                {
                    tblSizeChart.Rows.Clear();

                    if (sizeChart.MeasureSizeList != null && sizeChart.MeasureSizeList.Count() > 0)
                    {
                        showChart = true;

                        //Add header rows with Sizes    
                        TableHeaderRow headerRowSizeChart = new TableHeaderRow();

                        //Text comes from first retrieved row
                        string measureMsg = sizeChart.MeasureSizeList.Select(msg => msg.MeasureMessage).FirstOrDefault();

                        TableHeaderCell headerCellExtra = new TableHeaderCell();
                        headerCellExtra.Text = string.IsNullOrEmpty(measureMsg) ? "" : measureMsg;
                        headerCellExtra.CssClass = "measurement-head";
                        headerRowSizeChart.Cells.Add(headerCellExtra);

                        var sizes = sizeChart.MeasureSizeList.OrderBy(so => so.SizeOrder).Select(s => s.Size).Distinct();
                        if (sizes != null)
                        {
                            foreach (string size in sizes)
                            {
                                TableHeaderCell headerCell = new TableHeaderCell();
                                headerCell.Text = string.IsNullOrEmpty(size) ? "" : size;
                                headerCell.CssClass = "measurement-head";
                                headerRowSizeChart.Cells.Add(headerCell);
                            }
                        }
                        tblSizeChart.Rows.Add(headerRowSizeChart);

                        //Add rows with Measures                
                        var measures = sizeChart.MeasureSizeList.OrderBy(mo => mo.MeasureOrder).Select(m => m.Measure).Distinct();
                        if (measures != null)
                        {
                            foreach (string measure in measures)
                            {
                                TableRow measureRow = new TableRow();
                                TableCell measureCell = new TableCell();

                                measureCell.Text = string.IsNullOrEmpty(measure) ? "" : measure;
                                measureCell.CssClass = "left-size";

                                measureRow.Cells.Add(measureCell);
                                tblSizeChart.Rows.Add(measureRow);
                            }
                        }

                        //Add values for measure-size combination
                        foreach (MeasureSizeItem_V01 measureSizeItem in sizeChart.MeasureSizeList.OrderBy(m => m.MeasureOrder).ThenBy(s => s.SizeOrder))
                        {
                            TableCell cellValue = new TableCell();
                            cellValue.Text = string.IsNullOrEmpty(measureSizeItem.Value) ? "" : measureSizeItem.Value;

                            int rowIndex = measureSizeItem.MeasureOrder;
                            tblSizeChart.Rows[rowIndex].Cells.Add(cellValue);
                        }

                        sizeChartTable.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                    }
                }

                return showChart;
            }
        }
        
    }
}
