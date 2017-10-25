// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderBySKU.aspx.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to get the order bu sku page.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Ordering.SharedProviders.EventHandling;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Script.Services;
    using System.Web.Services;
    using System.Web.UI.WebControls;
    using HL.Common.Logging;
    using MyHerbalife3.Ordering.Providers;
    using MyHerbalife3.Ordering.Providers.CrossSell;
    using MyHerbalife3.Ordering.Providers.EventHandling;
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Web.MasterPages;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
    using Resources;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using Helpers;    /// <summary>
                      /// Use provide code behind of the order by sku page.
                      /// </summary>
    public partial class OrderBySKU : ProductsBase
    {
        /// <summary>
        /// The numitems.
        /// </summary>
        private const int NUMITEMS = 20;

        /// <summary>
        /// The err sku.
        /// </summary>
        private List<string> errSKU = new List<string>();

        /// <summary>
        /// The _errors.
        /// </summary>
        private List<string> friendlyMessages = new List<string>();

        /// <summary>
        /// skuList
        /// </summary>
        private List<SkuQty> skuList = new List<SkuQty>();

        /// <summary>
        /// Products list.
        /// </summary>
        private static IEnumerable<string> Products { get; set; }

        /// <summary>
        /// The cross sell found.
        /// </summary>
        [Publishes(MyHLEventTypes.CrossSellFound)]
        public event EventHandler CrossSellFound;

        /// <summary>
        /// The no cross sell found.
        /// </summary>
        [Publishes(MyHLEventTypes.NoCrossSellFound)]
        public event EventHandler NoCrossSellFound;

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            setupClientSideEventForQuantityAndSku();

            CheckoutButton1.Text = CheckoutButton1Disabled.Text = CheckoutButton2Disabled.Text =
            CheckoutButton2.Text = GetLocalResourceObject("CheckoutButtonResource1.Text") as string;

            //HLRulesManager.Manager.ProcessCatalogItemsForInventory(this.Locale, ShoppingCart);
            // this is to trigger qty/avail check.
            CatalogProvider.GetProductInfoCatalog(Locale, CurrentWarehouse);
            if (!IsPostBack)
            {
                string title = GetLocalResourceObject("PageResource1.Title") as string;
                TopTab.Text = title;
                (this.Master as OrderingMaster).SetPageHeader(title);
                if (ShoppingCart != null && ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0)
                {
                    findCrossSell(ShoppingCart.CartItems);
                }
                else
                {
                    NoCrossSellFound(this, null);
                }
                DisplayAPFMessage();
                //User Story 391426:- start
                GetTWSKUQuantitytorestrict();
            }
            else
            {
                CheckoutButton1.Enabled = true;
                CheckoutButton2.Enabled = true;
            }

            if (ShoppingCart != null && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                this.Page.Title = GetLocalResourceObject("EventTickets.Title") as string;
                (this.Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("EventTickets.Title") as string);
            }

            // Set static product list.
            if (this.AllSKUS.Any())
            {
                Products = this.AllSKUS.Where(sku => sku.Value.ProductAvailability == ProductAvailabilityType.Available).Select(
                    sku => string.Concat(sku.Key, " ", sku.Value.Product.DisplayName, " ", sku.Value.Description));
            }
            if (!IsPostBack)
            {
                if (IsChina)
                {
                    divChinaPCMessageBox.Visible = SessionInfo.IsReplacedPcOrder;
                }
            }
        }

        /// <summary>
        /// The find cross sell.
        /// </summary>
        private void findCrossSell()
        {
            var cs = Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] as CrossSellInfo;
            if (cs != null)
            {
                Category_V02 category = CatalogHelper.FindCategory(ProductInfoCatalog, cs.CategoryID);
                if (category != null)
                {
                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(cs));
                    return;
                }
            }

            NoCrossSellFound(this, null);
        }

        /// <summary>
        /// Gets the SKU list.
        /// </summary>
        /// <returns>List of products information.</returns>
        [WebMethod]
        [ScriptMethod]
        public static ProductsResultView GetAutoCompleteOptions()
        {
            return new ProductsResultView()
            {
                ProductsList = Products,
                SkuList = Products != null ? Products.Select(
                    product => product.Substring(0, product.IndexOf(' '))).ToList() : Products
            };
        }

        public void findCrossSell(List<ShoppingCartItem_V01> products)
        {
            if (products.Count == 0)
            {
                NoCrossSellFound(this, null);
                return;
            }
            List<CrossSellInfo> crossSellList = new List<CrossSellInfo>();
            foreach (ShoppingCartItem_V01 cartItem in products)
            {
                var item = ShoppingCart.ShoppingCartItems.Find(s => s.SKU == cartItem.SKU);
                if (item != null)
                {
                    if (item.ProdInfo == null || item.ParentCat == null || item.ProdInfo.CrossSellProducts == null)
                        continue;

                    foreach (int cat in item.ProdInfo.CrossSellProducts.Keys)
                    {
                        crossSellList.AddRange(from a in item.ProdInfo.CrossSellProducts[cat]
                                               select new CrossSellInfo(cat, a));
                    }
                }

            }
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
                    candidate = ProductDetail.SelectCrossSellFromPreviousDisplayOrCrssSellList(ShoppingCart, crossSellList,
                                                                                               Session[
                                                                                                   ProductDetail.
                                                                                                       LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                                                                                                   ] as
                                                                                               CrossSellInfo, AllSKUS);
                }

                if (candidate != null)
                {
                    Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] = candidate;

                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(candidate));
                    return;
                }
            }
            NoCrossSellFound(this, null);
            //findCrossSell();
        }

        /// <summary>
        /// The setup client side event for quantity and sku.
        /// setup quantity validation on client.
        /// </summary>
        private void setupClientSideEventForQuantityAndSku()
        {
            for (int i = 1; i < NUMITEMS + 1; i++)
            {
                var ctrlSKU = tblSKU.FindControl("SKUBox" + i) as TextBox;
                if (ctrlSKU != null)
                {
                    ctrlSKU.Attributes["onkeypress"] = "checkSKUChar(event,this)";
                    ctrlSKU.Attributes["onblur"] = string.Format("checkSKU(event,this,'{0}')", MyHL_ErrorMessage.DuplicateSKU);
                }
            }
        }

        /// <summary>
        /// The get all input.
        /// </summary>
        /// <returns>
        /// </returns>
        /// 
        private List<ShoppingCartItem_V01> getAllInput(List<string> friendlyMessages)
        {
            Dictionary<string, SKU_V01> allSKUs = CatalogProvider.GetAllSKU(Locale, base.CurrentWarehouse);
            SKU_V01 skuV01 = null;

            friendlyMessages.Clear();
            errSKU.Clear();

            var products = new List<ShoppingCartItem_V01>();
            setError setErrorDelegate = delegate(SkuQty sku, string error, List<string> errors)
            {
                sku.Image.Visible = true;
                sku.HasError = true;
                if (!errors.Contains(error))
                {
                    errors.Add(error);
                }
            };

            bool bNoItemSelected = true;
            for (int i = 1; i < NUMITEMS + 1; i++)
            {

                string controlID = "SKUBox" + i;
                var ctrlSKU = tblSKU.FindControl(controlID) as TextBox;
                var ctrlQty = tblSKU.FindControl("QuantityBox" + i) as TextBox;
                var ctrlError = tblSKU.FindControl("imgError" + i) as Image;

                string strSKU = ctrlSKU.Text.Trim();
                string strQty = ctrlQty.Text;
                int qty;
                int.TryParse(strQty, out qty);

                if (!string.IsNullOrEmpty(strSKU) && qty != 0)
                {
                    strSKU = strSKU.ToUpper();

                    // If the str has a product.
                    strSKU = strSKU.Split(new char[] { ' ' })[0];

                    AllSKUS.TryGetValue(strSKU, out skuV01);
                    if (skuV01 == null)
                    {
                        // if not valid setup error
                        setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, true), string.Format((GetLocalResourceObject("NoSKUFound") as string), strSKU), errSKU);
                    }
                    else
                    {
                        if (CheckMaxQuantity(ShoppingCart.CartItems, qty, skuV01, errSKU))
                        {
                            if (skuList.Any(s => s.SKU == strSKU))
                            {
                                var skuQty = new SkuQty(controlID, strSKU, qty, ctrlError, true, true);
                                skuList.Add(skuQty);
                                setErrorDelegate(skuQty, string.Format((GetLocalResourceObject("DuplicateSKU") as string), strSKU), errSKU);
                                SkuQty skuToFind = skuList.Find(s => s.SKU == strSKU);
                                if (skuToFind != null)
                                {
                                    // this is to prevent dupe one to NOT be added to cart
                                    skuToFind.HasError = true;
                                    skuToFind.Image.Visible = true;
                                }
                            }
                            else
                            {
                                skuList.Add(new SkuQty(controlID, strSKU, qty, ctrlError, false, false));
                            }
                        }
                        else
                        {
                            ctrlError.CssClass = ctrlError.CssClass.Replace("hide", string.Empty);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(strSKU) && qty <= 0)
                {
                    setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, false), String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "QuantityIncorrect"), strSKU), errSKU);
                }
                else
                {
                    if (strSKU.Length + strQty.Length != 0)
                    {
                        setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, false), GetLocalResourceObject("SKUOrQtyMissing") as string, errSKU);
                    }
                }

                if (!string.IsNullOrEmpty(strSKU) || !string.IsNullOrEmpty(strQty))
                {
                    bNoItemSelected = false;
                }
            }

            if (bNoItemSelected)
            {
                errSKU.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoItemsSelected"));
            }
            else
            {
                try
                {
                    foreach (SkuQty s in skuList)
                    {
                        // do not need to check at this point
                        if (APFDueProvider.IsAPFSku(s.SKU))
                        {
                            setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), s.SKU), errSKU);
                            continue;
                        }

                        AllSKUS.TryGetValue(s.SKU, out skuV01);
                        if (skuV01 != null)
                        {
                            HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, this.ShoppingCart, new List<SKU_V01> { skuV01 });
                            CatalogProvider.GetProductAvailability(skuV01, CurrentWarehouse);

                            int availQty;

                            // check isBlocked first
                            if (IsBlocked(skuV01))
                            {
                                setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), s.SKU), errSKU);
                            }
                            else if (!skuV01.IsPurchasable)
                            {
                                setErrorDelegate(s, string.Format(GetLocalResourceObject("SKUCantBePurchased") as string, s.SKU), errSKU);
                            }
                            else if (skuV01.ProductAvailability == ProductAvailabilityType.Unavailable)
                            {
                                setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), s.SKU), errSKU);
                            }
                            else if (HLConfigManager.Configurations.DOConfiguration.IsChina && ChinaPromotionProvider.GetPCPromoSkus(skuV01.SKU))
                            {
                                setErrorDelegate(s, string.Format(GetLocalResourceObject("SKUCantBePurchased") as string, s.SKU), errSKU);
                            }
                            else
                            {
                                int backorderCoverage = CheckBackorderCoverage(s.Qty, skuV01, friendlyMessages);
                                if (backorderCoverage == 0)
                                {
                                    // out of stock
                                    if ((availQty = ShoppingCartProvider.CheckInventory(skuV01.CatalogItem, GetAllQuantities(ShoppingCart.CartItems, s.Qty, s.SKU), CurrentWarehouse)) == 0)
                                    {
                                        setErrorDelegate(s, string.Format(MyHL_ErrorMessage.OutOfInventory, s.SKU), errSKU);
                                    }
                                    else if (availQty < GetAllQuantities(ShoppingCart.CartItems, s.Qty, s.SKU))
                                    {
                                        setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "LessInventory"), s.SKU, availQty), errSKU);
                                        HLRulesManager.Manager.PerformBackorderRules(ShoppingCart, skuV01.CatalogItem);
                                        IEnumerable<string> ruleResultMessages =
                                                            from r in ShoppingCart.RuleResults
                                                            where r.Result == RulesResult.Failure && r.RuleName == "Back Order"
                                                            select r.Messages[0];
                                        if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                                        {
                                            errSKU.Add(ruleResultMessages.First());
                                            ShoppingCart.RuleResults.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //if (errSKU.Count == 0)
                    {
                        products.AddRange((from c in skuList
                                           where c.HasError == false
                                           select new ShoppingCartItem_V01(0, c.SKU, c.Qty, DateTime.Now)).ToList());
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("getAllInput error:" + ex));
                }
            }

            return products;
        }

        private bool IsBlocked(SKU_V01 sku)
        {
            bool isBlocked = false;

            string wareHouse = CurrentWarehouse;
            try
            {
                WarehouseInventory_V01 inventory = sku.CatalogItem.InventoryList[wareHouse] as WarehouseInventory_V01;
                isBlocked = inventory.IsBlocked;
            }
            catch { }

            return isBlocked;
        }
        /// <summary>
        /// The add to cart.
        /// </summary>
        /// <param name="Source">
        /// The source.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void AddToCart(object Source, EventArgs e)
        {
            if (CanAddProduct(DistributorID, ref errSKU))
            {
                //List<string> friendlyMessages = new List<string>();
                List<ShoppingCartItem_V01> products = getAllInput(friendlyMessages);
                //CheckForOrderThresholds(products);

                uxErrores.DataSource = errSKU;
                uxErrores.DataBind();


                try
                {
                    if (products.Count > 0)
                    {
                        AddItemsToCart(products);
                        findCrossSell(products);
                    }
                    //errSKU.AddRange(friendlyMessages);

                    if (ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback))
                    {
                        string feedback = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Feedback).First().Messages[0];
                        if (!string.IsNullOrEmpty(feedback))
                        {
                            string title = PlatformResources.GetGlobalResourceString("GlobalResources", "FeedbackNotification");
                            if (IsChina) title = GetGlobalResourceString("FeedbackNotification");
                            (this.Master as OrderingMaster).ShowMessage(title, feedback);
                        }
                    }
                    else
                    {
                        if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                        {
                            PurchasingLimitProvider.SetPostErrorRemainingLimitsSummaryMessage(ShoppingCart);
                        }
                        if (ShoppingCart.RuleResults.Any(
                                      rs =>
                                      rs.RuleName == "PurchaseRestriction Rules" && rs.Result == RulesResult.Failure))
                        {
                            var ruleResultMsgs =
                            ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchaseRestriction Rules")
                                        .Select(r => r.Messages);
                            errSKU.AddRange(ruleResultMsgs.First().Distinct().ToList());
                            ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchaseRestriction Rules");
                        }
                        IEnumerable<string> ruleResultMessages = from r in ShoppingCart.RuleResults where r.Result == RulesResult.Failure select r.Messages[0];

                        if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                        {
                            errSKU.AddRange(ruleResultMessages.Distinct().ToList());
                        }
                    }

                    ShoppingCart.RuleResults.Clear();

                    if (errSKU.Count > 0)
                    {
                        uxErrores.DataSource = errSKU;
                        uxErrores.DataBind();
                    }
                    if (friendlyMessages.Count > 0)
                    {
                        ShowBackorderMessage(friendlyMessages);
                    }

                    OmnitureHelper.RegisterOmnitureAddCartScript(Page, ShoppingCart.ShoppingCartItems, products);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(GetDistributorInfo() + "Can't add to cart!\n" + ex);
                }

                ClearItemsOnPage();
            }
            else
            {
                uxErrores.DataSource = errSKU;
                uxErrores.DataBind();
            }

            // Initializing again the controls.
            System.Web.UI.ScriptManager.RegisterStartupScript(
                Page,
                Page.GetType(),
                Guid.NewGuid().ToString(),
                "InitializeControls();",
                true);            
        }

        /// <summary>
        /// The clear items on page.
        /// </summary>
        private void ClearItemsOnPage()
        {
            for (int i = 1; i < NUMITEMS + 1; i++)
            {
                string controlID = "SKUBox" + i;
                var ctrlSKU = tblSKU.FindControl(controlID) as TextBox;
                var ctrlQty = tblSKU.FindControl("QuantityBox" + i) as TextBox;
                var ctrlError = tblSKU.FindControl("imgError" + i) as Image;
                //if (ctrlError != null)
                //{
                //    ctrlError.Visible = false;
                //}
                // no error
                var sku = skuList.Find(s => s.ControlID == controlID);
                if (sku != null)
                {
                    // no error
                    if (!sku.HasError)
                    {
                        ctrlQty.Text = ctrlSKU.Text = string.Empty;
                        ctrlError.Visible = false;
                    }
                    else if (sku.DupeSKU)
                    {
                        ctrlSKU.Text = string.Empty;
                        ctrlError.Visible = false;
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartDeleted)]
        public void OnShoppingCartDeleted(object sender, EventArgs e)
        {
            try
            {
                if (ShoppingCart != null)
                {
                    NoCrossSellFound(null, null);
                    ClearMessages(sender, e);
                }
            }
            catch { }
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            try
            {
                if (ShoppingCart != null)
                {
                    findCrossSell(ShoppingCart.CartItems);
                }
            }
            catch { }
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
                    AddToCart(this, null);
                    ClearItemsOnPage();
                    upProductSku.Update();
                }
            }
        }

        #region Nested type: SkuQty

        /// <summary>
        /// The sku qty.
        /// </summary>
        private class SkuQty
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SkuQty"/> class.
            /// </summary>
            /// <param name="sku">
            /// The sku.
            /// </param>
            /// <param name="qty">
            /// The qty.
            /// </param>
            /// <param name="image">
            /// The image.
            /// </param>
            public SkuQty(string controlID, string sku, int qty, Image image, bool hasError, bool dupeSKU)
            {
                ControlID = controlID;
                SKU = sku;
                Qty = qty;
                Image = image;
                HasError = hasError;
                DupeSKU = dupeSKU;
            }

            /// <summary>
            /// Gets or sets Control ID.
            /// </summary>
            public string ControlID { get; set; }

            /// <summary>
            /// Gets or sets SKU.
            /// </summary>
            public string SKU { get; set; }

            /// <summary>
            /// Gets or sets Qty.
            /// </summary>
            public int Qty { get; set; }

            /// <summary>
            /// Gets or sets Image.
            /// </summary>
            public Image Image { get; set; }

            /// <summary>
            /// Gets or sets HasError.
            /// </summary>
            public bool HasError { get; set; }

            /// <summary>
            /// Gets or sets DupeSKU.
            /// </summary>
            public bool DupeSKU { get; set; }
        }

        #endregion

        #region Nested type: setError

        /// <summary>
        /// The set error.
        /// </summary>
        /// <param name="img">
        /// The img.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="errSKU">
        /// The err sku.
        /// </param>
        private delegate void setError(SkuQty sku, string error, List<string> errSKU);

        #endregion

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void ClearMessages(object sender, EventArgs e)
        {
            uxErrores.DataSource = string.Empty;
            uxErrores.DataBind();
            upProductSku.Update();
        }
    }
}
