using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Resources;
using System.Web.UI;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Web.Ordering.Helpers;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    /// <summary>
    ///     The product sku.
    /// </summary>
    public partial class ProductSKU : ProductsBase
    {

        /// <summary>
        ///     The numitems.
        /// </summary>
        private const int NUMITEMS = 20;

        /// <summary>
        ///     The numitems.
        /// </summary>
      // private const string[]ETOSKU = Settings.GetRequiredAppSetting("ETOSKU").Split('|');

        /// <summary>
        ///     The _errors.
        /// </summary>
        private readonly List<string> friendlyMessages = new List<string>();

        /// <summary>
        ///     skuList
        /// </summary>
        private readonly List<SkuQty> skuList = new List<SkuQty>();

        /// <summary>
        ///     The err sku.
        /// </summary>
        private List<string> errSKU = new List<string>();

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
            if (DisplayCCMessage && HLConfigManager.Configurations.DOConfiguration.AllowToDispalyCCMessage)
            {
                alertNoCC.Visible = true;
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
            setupClientSideEventForQuantityAndSku();

            CheckoutButton1.Text = CheckoutButton1Disabled.Text = CheckoutButton2Disabled.Text =
                                                                  CheckoutButton2.Text =
                                                                  GetLocalResourceObject("CheckoutButtonResource1.Text")
                                                                  as string;

            //HLRulesManager.Manager.ProcessCatalogItemsForInventory(this.Locale, ShoppingCart);
            // this is to trigger qty/avail check.
            CatalogProvider.GetProductInfoCatalog(Locale, CurrentWarehouse);
            if (!IsPostBack)
            {
                if (Session["showedAPFPopup"] == null) Session["showedAPFPopup"] = false;
                if (HL.Common.Configuration.Settings.GetRequiredAppSetting("IsChina", false))
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
                    //if (orders != null && orders.Any())
                    //{
                    //    var orderNumber = orders.FirstOrDefault().OrderId; //10000761
                    //    ViewState["pendingOrderNumber"] = orderNumber;
                    //    dupeOrderPopupExtender.Show();
                    //}

                }
                string title = GetLocalResourceObject("PageResource1.Title") as string;
                TopTab.Text = title;
                (Master as OrderingMaster).SetPageHeader(title);
                if (ShoppingCart != null && ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0)
                {
                    findCrossSell(ShoppingCart.CartItems);
                }
                else
                {
                    NoCrossSellFound(this, null);
                }
                DisplayAPFMessage();
                SetNqsMessage();
            }
            else
            {
                CheckoutButton1.Enabled = true;
                CheckoutButton2.Enabled = true;
            }

            if (ShoppingCart != null && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                Page.Title = GetLocalResourceObject("EventTickets.Title") as string;
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("EventTickets.Title") as string);
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina && !IsEventTicketMode)
            {
                this.CnChkout24h.AddToCartControlList = new List<System.Web.UI.Control>
                {
                    this.CheckoutButton1,
                    this.CheckoutButton2,
                };
            }
            DisplayELearningMessage();
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
            //For User Story 391426
            GetTWSKUQuantitytorestrict();
            if (SessionInfo != null && !SessionInfo.IsAPFOrderFromPopUp)
                SessionInfo.IsAPFOrderFromPopUp = false;
            var allowedCountries = Settings.GetRequiredAppSetting("AllowAPFPopupForStandAloneContries", "CH");
            if (!(bool)Session["showedAPFPopup"] && allowedCountries.Contains(Locale.Substring(3)) && (APFDueProvider.IsAPFDueWithinOneYear(DistributorID, Locale.Substring(3)) || APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale)))
            {
                APFDuermndrPopUp.ShowPopUp();
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
            {
                List<DeliveryOption> shippingAddresses =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetShippingAddresses((Page as ProductsBase).DistributorID,
                                                                (Page as ProductsBase).Locale)
                                          .Where(s => s.HasAddressRestriction == true)
                                          .ToList();
                if (shippingAddresses.Count == 0)
                {
                    AddressResPopUP1.ShowAddressRestrictionPopUp();
                }
            }
        }

        /// <summary>
        ///     The find cross sell.
        /// </summary>
        private void findCrossSell()
        {
            var cs = Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] as CrossSellInfo;
            if (cs != null)
            {
                var category = CatalogHelper.FindCategory(ProductInfoCatalog, cs.CategoryID);
                if (category != null)
                {
                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(cs));
                    return;
                }
            }

            NoCrossSellFound(this, null);
        }

        public void findCrossSell(List<ShoppingCartItem_V01> products)
        {
            if (products.Count == 0)
            {
                NoCrossSellFound(this, null);
                return;
            }
            var crossSellList = new List<CrossSellInfo>();
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
                    candidate = ProductDetail.SelectCrossSellFromPreviousDisplayOrCrssSellList(ShoppingCart,
                                                                                               crossSellList,
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

        // setup quantity validation on client
        /// <summary>
        ///     The setup client side event for quantity and sku.
        /// </summary>
        private void setupClientSideEventForQuantityAndSku()
        {
            for (int i = 1; i < NUMITEMS + 1; i++)
            {
                var ctrlSKU = tblSKU.FindControl("SKUBox" + i) as TextBox;
                if (ctrlSKU != null)
                {
                    ctrlSKU.Attributes["onkeypress"] = "checkSKUChar(event,this)";
                    ctrlSKU.Attributes["onblur"] = string.Format("checkSKU(event,this,'{0}')",
                                                                 MyHL_ErrorMessage.DuplicateSKU);                    
                }
                var ctrlQty = tblSKU.FindControl("QuantityBox" + i) as TextBox;
                if (ctrlQty != null)
                {
                    ctrlQty.MaxLength = HLConfigManager.Configurations.ShoppingCartConfiguration.QuantityBoxSize;
                }
            }
        }

        /// <summary>
        ///     The get all input.
        /// </summary>
        /// <returns>
        /// </returns>
        private List<ShoppingCartItem_V01> getAllInput(List<string> friendlyMessages)
        {
            labelBackOrderMessage.Text = string.Empty;
            var allSKUs = CatalogProvider.GetAllSKU(Locale, base.CurrentWarehouse);

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
            bool bNotETOSKU = true;
            for (int i = 1; i < NUMITEMS + 1; i++)
            {
                string controlID = "SKUBox" + i;
                var ctrlSKU = tblSKU.FindControl(controlID) as TextBox;
                var ctrlQty = tblSKU.FindControl("QuantityBox" + i) as TextBox;
                var ctrlError = tblSKU.FindControl("imgError" + i) as Image;
                if (ctrlError != null)
                {
                    ctrlError.Visible = false;
                }
                string strSKU = ctrlSKU.Text.Trim();
                if (IsChina && bNotETOSKU && !ValidateETOSKU(strSKU))
                {
                    bNotETOSKU = false;
                }
                string strQty = ctrlQty.Text;
                int qty;
                int.TryParse(strQty, out qty);

                if (!string.IsNullOrEmpty(strSKU) && qty != 0)
                {
                    strSKU = strSKU.ToUpper();
                    AllSKUS.TryGetValue(strSKU, out skuV01);
                    if (skuV01 == null)
                    {
                        // if not valid setup error
                        setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, true),
                                         string.Format((GetLocalResourceObject("NoSKUFound") as string), strSKU), errSKU);
                    }
                    else
                    {
                        if (CheckMaxQuantity(ShoppingCart.CartItems, qty, skuV01, errSKU))
                        {
                            if (skuList.Any(s => s.SKU == strSKU))
                            {
                                var skuQty = new SkuQty(controlID, strSKU, qty, ctrlError, true, true);
                                skuList.Add(skuQty);
                                setErrorDelegate(skuQty,
                                                 string.Format((GetLocalResourceObject("DuplicateSKU") as string),
                                                               strSKU), errSKU);
                                var skuToFind = skuList.Find(s => s.SKU == strSKU);
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
                            ctrlError.Visible = true;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(strSKU) && qty <= 0)
                {
                    setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, false),
                                     String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "QuantityIncorrect"), strSKU), errSKU);
                }
                else
                {
                    if (strSKU.Length + strQty.Length != 0)
                    {
                        setErrorDelegate(new SkuQty(controlID, strSKU, qty, ctrlError, true, false),
                                         GetLocalResourceObject("SKUOrQtyMissing") as string, errSKU);
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
                    int availQty;
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
                            HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, ShoppingCart,
                                                                                   new List<SKU_V01> {skuV01});
                            //CatalogProvider.GetProductAvailability(skuV01, CurrentWarehouse, ShoppingCart.DeliveryInfo != null
                            //                                                                   ? ShoppingCart.DeliveryInfo.Option
                            //                                                                   : DeliveryOptionType.Unknown);

                            // check isBlocked first
                            //if (IsBlocked(skuV01, this.CurrentWarehouse))
                            //{
                            //    setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), s.SKU), errSKU);
                            //}
                            if (!skuV01.IsPurchasable)
                            {
                                setErrorDelegate(s,
                                                 string.Format(GetLocalResourceObject("SKUCantBePurchased") as string,
                                                               s.SKU), errSKU);
                            }
                            else if (CatalogProvider.GetProductAvailability(skuV01, CurrentWarehouse,
                                           ShoppingCart.DeliveryInfo != null
                                               ? (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                               : ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown) == ProductAvailabilityType.Unavailable)
                            {
                                setErrorDelegate(s, string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "SKUNotAvailable"), s.SKU), errSKU);
                                if (HLConfigManager.Configurations.DOConfiguration.DisplayBackOrderEnhancements)
                                {
                                    //show the link for back order enhancement
                                    labelBackOrderMessage.Text =
                                            PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                      "SKUNotAvailableBOLink");
                                }
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
                                    int allSkuQuantity = GetAllQuantities(ShoppingCart.CartItems, s.Qty, s.SKU);
                                    // out of stock
                                    if (
                                        (availQty =
                                         ShoppingCartProvider.CheckInventory(skuV01.CatalogItem, allSkuQuantity,
                                                                             CurrentWarehouse)) == 0)
                                    {
                                        setErrorDelegate(s, string.Format(MyHL_ErrorMessage.OutOfInventory, s.SKU),
                                                         errSKU);
                                    }
                                    else if (availQty < GetAllQuantities(ShoppingCart.CartItems, s.Qty, s.SKU))
                                    {
                                        setErrorDelegate(s,
                                                         string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "LessInventory"), s.SKU, availQty),
                                                         errSKU);
                                        HLRulesManager.Manager.PerformBackorderRules(ShoppingCart, skuV01.CatalogItem);
                                        var ruleResultMessages =
                                            from r in ShoppingCart.RuleResults
                                            where r.Result == RulesResult.Failure && r.RuleName == "Back Order"
                                            select r.Messages[0];
                                        if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                                        {
                                            errSKU.Add(ruleResultMessages.First());
                                            ShoppingCart.RuleResults.Clear();
                                        }
                                        if (availQty != 0)
                                        {
                                            if (availQty == allSkuQuantity)
                                            {
                                                products.AddRange((from c in skuList
                                                                   where c.HasError == true
                                                                    && c.SKU.Trim() == s.SKU.Trim()
                                                                   select new ShoppingCartItem_V01(0, c.SKU, availQty, DateTime.Now)).ToList());
                                            }
                                            else
                                            {
                                                int qtyInSkuCart = allSkuQuantity - s.Qty;
                                                if (availQty - qtyInSkuCart > 0)                                                
                                                    products.AddRange((from c in skuList
                                                                       where c.HasError == true
                                                                       && c.SKU.Trim()== s.SKU.Trim()
                                                                       select new ShoppingCartItem_V01(0, c.SKU, availQty - qtyInSkuCart, DateTime.Now)).ToList());
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }

                    products.AddRange((from c in skuList
                                           where c.HasError == false
                                           select new ShoppingCartItem_V01(0, c.SKU, c.Qty, DateTime.Now)).ToList());

                    if(HLConfigManager.Configurations.DOConfiguration.IsChina && !bNotETOSKU)
                    { products.Clear(); }
                    
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("getAllInput error:" + ex));
                }
            }

            return products;
        }

        public static bool IsBlocked(SKU_V01 sku, string warehouse)
        {
            var isBlocked = false;
            var wareHouse = warehouse;

            try
            {
                var inventory = sku.CatalogItem.InventoryList[wareHouse] as WarehouseInventory_V01;
                if (inventory != null)
                {
                    isBlocked = inventory.IsBlocked;
                }
            }
            catch
            { }

            return isBlocked;
        }

        /// <summary>
        ///     The add to cart.
        /// </summary>
        /// <param name="Source">
        ///     The source.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void AddToCart(object Source, EventArgs e)
        {
            
           

            if (CanAddProduct(DistributorID, ref errSKU))
            {

                //List<string> friendlyMessages = new List<string>();
                var products = getAllInput(friendlyMessages);
                //CheckForOrderThresholds(products);
               
               
                uxErrores.DataSource = errSKU;
                uxErrores.DataBind();

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
                                errSKU.Add(
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
                                errSKU.Add(
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
                                    errSKU.Add(
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
                                    errSKU.Add(
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
                        AddItemsToCart(products);
                        findCrossSell(products);
                    }
                    //errSKU.AddRange(friendlyMessages);
                    var CurrentSession = SessionInfo.GetSessionInfo(ShoppingCart.DistributorID, Locale);
                    if (null != CurrentSession && CurrentSession.IsReplacedPcOrder && CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                    {
                        CurrentSession = SessionInfo.GetSessionInfo(CurrentSession.ReplacedPcDistributorOrderingProfile.Id, Locale);
                        DistributorOrderingProfile = CurrentSession.ReplacedPcDistributorOrderingProfile;
                    }
                    if (ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback))
                    {
                        // Checking for specific message to display
                        var feedbackResult = ShoppingCart.RuleResults.FirstOrDefault(r => r.Result == RulesResult.Feedback && r.Messages !=null && r.Messages.Any(m => m != null && m.StartsWith("HTML|")));
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
                            string feedback = (ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Feedback).First().Messages !=null && 
                                ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Feedback).First().Messages.Count > 0 
                                ? ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Feedback).First().Messages[0] : string.Empty);

                            if (!string.IsNullOrEmpty(feedback))
                            {
                                string title = PlatformResources.GetGlobalResourceString("GlobalResources", "FeedbackNotification");
                                if (IsChina) title = GetGlobalResourceString("FeedbackNotification");
                                (Master as OrderingMaster).ShowMessage(title, feedback);
                            }
                        }
                        if (IsChina && (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC || (SessionInfo != null && SessionInfo.ReplacedPcDistributorOrderingProfile != null && SessionInfo.ReplacedPcDistributorOrderingProfile.IsPC)))
                        {
                            if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                            {
                                if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                                {
                                    var ruleResultMsgs = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                            .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                                    errSKU.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (IsChina)
                        {
                            if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                            {
                                if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                                {
                                    var ruleResultMsgs = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                                .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                                    errSKU.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                                }
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
                                      rs.RuleName == "PurchaseRestriction Rules" && rs.Result == RulesResult.Failure))
                        {
                            if (ShoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchaseRestriction Rules" && rs.Result == RulesResult.Failure && rs.Messages != null && rs.Messages.Count() > 0))
                            {
                                var ruleResultMsgs =
                            ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchaseRestriction Rules")
                                        .Select(r => r.Messages);
                                errSKU.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchaseRestriction Rules");
                            }
                        }
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            if (
                                  ShoppingCart.RuleResults.Any(
                                      rs => rs.Result == RulesResult.Failure ))
                            {
                                var ruleResultMessages =
                                    ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.Messages != null && r.Messages.Count > 0 && (r.RuleName == "Promotional Rules" || r.RuleName == "ETO Rules"));
                                if (ruleResultMessages != null)
                                {
                                    foreach (ShoppingCartRuleResult message in ruleResultMessages)
                                    {
                                        errSKU.AddRange(message.Messages.Select(x => x.ToString()));

                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            var ruleResultMessages = from r in ShoppingCart.RuleResults
                                                     where r.Result == RulesResult.Failure && r.Messages !=null &&r.Messages.Any()
                                                     select r.Messages[0];

                            if (null != ruleResultMessages && ruleResultMessages.Any())
                            {
                                errSKU.AddRange(ruleResultMessages.Distinct().ToList());
                            }
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

        private Boolean ValidateETOSKU(string sku)
        {
            var ETOSKU = Settings.GetRequiredAppSetting("ETOsku").Split('|');
            string ETO = Request.QueryString["ETO"];
            if (ETO != null && ETO.ToUpper() == "FALSE" && sku != "" && ETOSKU.Contains(sku))
            {
                errSKU.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage","NotEligibleForETO")));
                return false;
            }
            if (ETO !=null && ETO.ToUpper() == "TRUE")
            {
                string[] words = null;
                string eligibleSKu = string.Empty;
                var rsp = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetEventEligibility(DistributorID);
                if (rsp != null && rsp.IsEligible)
                {
                    words = rsp.Remark.Split('|');
                    eligibleSKu = words[words.Length - 2];
                    SKU_V01 etoSku;
                    AllSKUS.TryGetValue(eligibleSKu, out etoSku);
                    if (etoSku != null)
                    {
                        var skuETO = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased
                        {
                            SKU = etoSku.CatalogItem.StockingSKU.Trim(),
                            Category = "ETO",
                        };
                        var skuList = new List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased>();
                        skuList.Add(skuETO);
                        var totalETOCount =
                            Providers.China.OrderProvider.GetSkuOrderedAndPurchased(ShoppingCart.CountryCode, null, null,
                                                                                    null, skuList);
                        var skulimit = Settings.GetRequiredAppSetting("ETOskulimit",string.Empty).Split('|');

                        if (skulimit.Length >1 && eligibleSKu.Trim().Equals(skulimit[0]) && totalETOCount.Any(x => x.QuantityPurchased >= Convert.ToInt16(skulimit[1]) && x.SKU == etoSku.CatalogItem.StockingSKU.Trim()))
                        {
                            etoSku.CatalogItem.InventoryList.ToList().ForEach(
                                x =>
                                {
                                    var val = x.Value as WarehouseInventory_V01;
                                    if (val != null)
                                    {
                                        val.QuantityOnHand = 0;
                                        val.QuantityAvailable = 0;
                                        val.IsBlocked = true;
                                    }

                                });

                        }
                    }
                    if (sku != "" && eligibleSKu != sku)
                    {
                        errSKU.Add(String.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "NotEligibleForETO")));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///     The clear items on page.
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
        protected void HideSlowMovingMsg(object sender, EventArgs e)
        {
            SlowmovingSkuPopupExtender.Hide();
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
            catch
            {
            }
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
                    AddToCart(this, null);
                    ClearItemsOnPage();
                    upProductSku.Update();
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void ClearMessages(object sender, EventArgs e)
        {
            uxErrores.DataSource = string.Empty;
            uxErrores.DataBind();
            upProductSku.Update();
            labelBackOrderMessage.Text = string.Empty;
        }

        #region Nested type: SkuQty

        /// <summary>
        ///     The sku qty.
        /// </summary>
        private class SkuQty
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="SkuQty" /> class.
            /// </summary>
            /// <param name="sku">
            ///     The sku.
            /// </param>
            /// <param name="qty">
            ///     The qty.
            /// </param>
            /// <param name="image">
            ///     The image.
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
            ///     Gets or sets Control ID.
            /// </summary>
            public string ControlID { get; set; }

            /// <summary>
            ///     Gets or sets SKU.
            /// </summary>
            public string SKU { get; set; }

            /// <summary>
            ///     Gets or sets Qty.
            /// </summary>
            public int Qty { get; set; }

            /// <summary>
            ///     Gets or sets Image.
            /// </summary>
            public Image Image { get; set; }

            /// <summary>
            ///     Gets or sets HasError.
            /// </summary>
            public bool HasError { get; set; }

            /// <summary>
            ///     Gets or sets DupeSKU.
            /// </summary>
            public bool DupeSKU { get; set; }
        }

        #endregion

        #region Nested type: setError

        /// <summary>
        ///     The set error.
        /// </summary>
        /// <param name="img">
        ///     The img.
        /// </param>
        /// <param name="error">
        ///     The error.
        /// </param>
        /// <param name="errSKU">
        ///     The err sku.
        /// </param>
        private delegate void setError(SkuQty sku, string error, List<string> errSKU);

        #endregion

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
