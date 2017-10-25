using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using Resources;
using MyHerbalife3.Ordering.Web.MasterPages;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.Web.Ordering.Helpers;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    /// <summary>
    ///     The checkout order summary.
    /// </summary>
    public delegate void MessageHandler(string message);
    public partial class CheckoutOrderSummary : UserControlBase
    {
        public event MessageHandler SendMessage;
        #region Variables

        /// <summary>
        ///     The _errors.
        /// </summary>
        private readonly List<string> _backorderMessages = new List<string>();

        /// <summary>
        ///     The _errors.
        /// </summary>
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        ///     The msg for backorder
        /// </summary>
        private readonly List<string> _friendlyMessages = new List<string>();

        /// <summary>
        ///     The _prods deleted.
        /// </summary>
        private readonly List<string> _prodsDeleted = new List<string>();

        /// <summary>
        ///     The _allow decimal.
        /// </summary>
        private bool _allowDecimal;

        //private List<string> _apfSKUList;
        /// <summary>
        ///     The _apf sku list.
        /// </summary>
        /// <summary>
        ///     The _shopping cart.
        /// </summary>
        private MyHLShoppingCart _shoppingCart;

        /// <summary>
        ///     If the errors displayed is due to the min quantity ruke violation from customer order
        /// </summary>
        private bool customerOrderMinQuantityError;

        /// <summary>
        ///     The exceeds purchasing limit.
        /// </summary>
        private bool exceedsPurchasingLimit;

        /// <summary>
        ///     Gets or sets a value indicating whether DisplayReadOnlyGrid.
        /// </summary>
        public bool DisplayReadOnlyGrid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether FlattenProductDetails.
        /// </summary>
        public bool FlattenProductDetails { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether FlattenProductDetails.
        /// </summary>
        public string OrderSummaryText { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether FlattenProductDetails.
        /// </summary>
        public bool CheckOutOptionsHasErrors { get; set; }

        /// <summary>
        ///     Gets or sets all SKUS
        /// </summary>
        private Dictionary<string, SKU_V01> _AllSKUS { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating error occurs.
        /// </summary>
        public bool _HasErrors { get; set; }

        public bool IsHelpIconHidden
        {
            get { return !HLConfigManager.Configurations.CheckoutConfiguration.ShowDeleteItemsHelp; }
        }

        public bool _IsResponsiveControl {
            get
            {
                return (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile());
            }
        }

        public bool DisplayChildSKUs
        {
            get { return (this.ChildSKUs != null && this.ChildSKUs.Count > 0) ? true : false; }
        }

        private List<DistributorShoppingCartItem> ChildSKUs { get; set; }

        private string RecalculateChildSKUCacheKey { get; set; }

        private SaveCartCommand _saveCardCommand; 
        /// <summary>
        ///     Gets or sets Totals.
        /// </summary>
        private OrderTotals_V01 Totals { get; set; }

        private CustomerOrder_V01 CustomerOrder { get; set; }

        #region omniture

        public string OmnitureState { get; set; }

        #endregion omniture

        /// <summary>
        ///     Event
        /// </summary>
        [Publishes(MyHLEventTypes.ProceedingToCheckout)]
        public event EventHandler ProceedingToCheckout;

        [Publishes(MyHLEventTypes.NotifyTodaysMagazineRecalculate)]
        public event EventHandler NotifyTodaysMagazineRecalculate;

        [Publishes(MyHLEventTypes.NotifyTodaysMagazineCancelOrder)]
        public event EventHandler NotifyTodaysMagazineCancelOrder;

        [Publishes(MyHLEventTypes.CheckoutToShoppingCart)]
        public event EventHandler CheckoutToShoppingCart;

        [Publishes(MyHLEventTypes.ShippingMethodCheckVP)]
        public event EventHandler ShippingMethodCheckVP;
        /// <summary>
        ///     launch product detail popup
        /// </summary>
        [Publishes(MyHLEventTypes.ProductDetailBeingLaunched)]
        public event EventHandler ProductDetailBeingLaunched;

        [Publishes(MyHLEventTypes.OnStandAloneDonationClear)]
        public event EventHandler OnStandAloneDonationClear;

        [Publishes(MyHLEventTypes.OnSCancelDonationsVisible)]
        public event EventHandler OnSCancelDonationsVisible;

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

        #region Nested type: delDisableControls

        /// <summary>
        ///     The del disable controls.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <param name="isSP">
        ///     The is sp.
        /// </param>
        private delegate void delDisableControls(ListViewItem item, bool isSP);

        #endregion Nested type: delDisableControls

        #endregion Variables
        #region Consts

        private const string ROOT_PATH = "~/Ordering/";
        private const string PATH_PRICELIST = "PriceList.aspx";

        #endregion

        #region SubscriptionEvents

        [SubscribesTo(MyHLEventTypes.PickUpOptionNotPopulated)]
        public void OnPickUpOptionNotPopulated(object sender, EventArgs e)
        {
            //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
            _errors.Add(GetLocalResourceObject("PickUpOptionNotPopulated").ToString());
        }

        [SubscribesTo(MyHLEventTypes.CheckOutOptionsNotPopulated)]
        public void OnCheckOutOptionsNotPopulated(object sender, EventArgs e)
        {
            CheckOutOptionsHasErrors = true;
        }

        [SubscribesTo(MyHLEventTypes.ShippingOptionNotPopulated)]
        public void OnShippingOptionNotPopulated(object sender, EventArgs e)
        {
            //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
            _errors.Add(GetLocalResourceObject("ShippingOptionNotPopulated").ToString());
        }

        [SubscribesTo(MyHLEventTypes.DeliveryOptionChanged)]
        public void OnDeliveryOptionChanged(object sender, EventArgs e)
        {
            //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
            string pickupMsg = GetLocalResourceObject("PickUpOptionNotPopulated").ToString();
            string shippingMsg = GetLocalResourceObject("ShippingOptionNotPopulated").ToString();
            _errors.Remove(pickupMsg);
            _errors.Remove(shippingMsg);
            blErrors.DataSource = _errors;
            blErrors.DataBind();
        }

        //[SubscribesTo(MyHLEventTypes.PickUpNicknameNotSelected)]
        //public void OnPickUpNicknameNotSelected(object sender, EventArgs e)
        //{
        //    //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
        //    DeliveryOptionTypeEventArgs args = e as DeliveryOptionTypeEventArgs;
        //    string message = PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingNickNameNotPopulated");
        //    if (null != args)
        //    {
        //        if (args.DeliveryOption == DeliveryOptionType.Pickup)
        //        {
        //            message = PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpNickNameNotPopulated");
        //        }
        //    }
        //    _errors.Add(message);
        //}

        [SubscribesTo(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public void OnCartItemRemovedDueToSKULimitationRules(object sender, EventArgs e)
        {
            setupShoppingCartDataSource(Level);
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            //_shoppingCart = (e as ShoppingCartEventArgs).ShoppingCart;
            setupShoppingCartDataSource(Level);
        }

        [SubscribesTo(MyHLEventTypes.QuoteRetrieved)]
        public void OnQuoteRetrieved(object sender, EventArgs e)
        {
            setupShoppingCartDataSource(Level);
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            if (ShoppingCart != null)
            {
                var results = HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartOrderSubTypeChanged);
                if (results.Any(r => r.RuleName.Equals("Promotional Rules") && r.Result == RulesResult.Success))
                {
                    setupShoppingCartDataSource(Level);
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            setupShoppingCartDataSource(Level);
        }

        [SubscribesTo(MyHLEventTypes.PaymentInfoChanged)]
        public void Update(object sender, EventArgs e)
        {
            setupShoppingCartDataSource(Level);
        }

        /// <summary>
        ///     The on order sub type changed.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void OnOrderSubTypeChanged(object sender, EventArgs e)
        {
            var args = e as OrderSubTypeEventArgs;
            if (null != ShoppingCart && null != ShoppingCart.Totals)
            {
                if (args != null)
                {
                    //PurchasingLimits_V01 limits = HL.MyHerbalife.Providers.Ordering.PurchasingLimitProvider.GetPurchasingLimits(DistributorID, args.DSSubType);
                    bool limitsExceeded = !FOPEnabled ? HLRulesManager.Manager.PurchasingLimitsAreExceeded(DistributorID) : PurchaseRestrictionManager(DistributorID).PurchasingLimitsAreExceeded(DistributorID, ShoppingCart);

                    if (limitsExceeded)
                    {
                        _errors.Clear();
                        checkOutButton.Enabled = false;
                        checkOutButton.Disabled = !checkOutButton.Enabled;
                        if (args.IsEarnings)
                        {
                            _errors.Add(GetLocalResourceObject("MaxEarningsLimitsExceeded").ToString());
                        }
                        else if (args.IsVolume)
                        {
                            _errors.Add(GetLocalResourceObject("MaxEarningsVolExceeded").ToString());
                        }
                        else
                        {
                            _errors.Add(GetLocalResourceObject("ErrorGettingPurchasingLimits").ToString());
                            checkOutButton.Enabled = false;
                            checkOutButton.Disabled = !checkOutButton.Enabled;
                        }
                    }
                    else
                    {
                        checkOutButton.Enabled = true;
                        checkOutButton.Disabled = !checkOutButton.Enabled;
                    }
                }
            }
            else
            {
                _errors.Add(GetLocalResourceObject("ErrorGettingPurchasingLimits").ToString());
                checkOutButton.Enabled = false;
                checkOutButton.Disabled = !checkOutButton.Enabled;
            }

            var refreshCartDisplay = false;
            if (ShoppingCart != null)
            {
                var results = HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartOrderSubTypeChanged);
                if (results.Any(r => r.RuleName.Equals("Promotional Rules")))
                {
                    refreshCartDisplay = true;
                }
            }

            if (args != null && args.RefreshCartDisplay || refreshCartDisplay)
            {
                setupShoppingCartDataSource(Level);
            }
            //if (_errors.Count > 0)
            //{
            blErrors.DataSource = _errors;
            blErrors.DataBind();
            return;
            //}
        }

        [SubscribesTo(MyHLEventTypes.PaymentOptionsViewChanged)]
        public void OnPaymentOptionsViewChanged(object sender, EventArgs e)
        {
            var refreshCartDisplay = false;
            if (ShoppingCart != null)
            {
                var results = HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartPaymentOptionChanged);
                if (results.Any(r => r.Result == RulesResult.Success))
                {
                    refreshCartDisplay = true;
                }
            }

            if (refreshCartDisplay)
            {
                setupShoppingCartDataSource(Level);
            }
        }

        #endregion SubscriptionEvents

        [Publishes(MyHLEventTypes.ItemRemovedAPFLeft)]
        public event EventHandler OnItemRemovedAPFLeft;

        [Publishes(MyHLEventTypes.ShoppingCartRecalculated)]
        public event EventHandler OnCartRecalculated;

        [Publishes(MyHLEventTypes.CNShoppingCartRecalculated)]
        public event EventHandler RaiseCNCartRecalculated;

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

            var savedCartCommand = LoadControl("~/Ordering/Controls/SaveCartCommand.ascx");
            _saveCardCommand = (SaveCartCommand)savedCartCommand;

            _AllSKUS = (ProductsBase).ProductInfoCatalog.AllSKUs;
            _shoppingCart = ShoppingCart;

            CalculateChildSKUs();

            _allowDecimal = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal;
            if (!IsPostBack)
            {
                _HasErrors = false;
                var lstDistributorShoppingCartItem =
                    (Page as ProductsBase).ShoppingCart.ShoppingCartItems;
                bool bCancheckout = 
                    lstDistributorShoppingCartItem != null && lstDistributorShoppingCartItem.Count > 0;
                if (!bCancheckout)
                {
                    if (HLConfigManager.Configurations.DOConfiguration.CalculateWithoutItems &&
                        _shoppingCart.Totals != null &&
                        (_shoppingCart.Totals as OrderTotals_V01).AmountDue != decimal.Zero)
                        bCancheckout = true;
                }
                if (bCancheckout)
                {
                    setupShoppingCartDataSource(Level);
                    cartEmpty.Visible = false;
                    uxCancelOrder.Enabled = !(APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) && HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed);
                    uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                    uxRecalculate.Enabled = true;
                    uxRecalculate.Disabled = !uxRecalculate.Enabled;
                    checkOutButton.Enabled = true;
                    checkOutButton.Disabled = !checkOutButton.Enabled;
                    mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                   
                }
                else
                {
                    Totals = new OrderTotals_V01();
                    cartEmpty.Visible = true;
                    uxCancelOrder.Enabled = false;
                    uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                    uxRecalculate.Enabled = false;
                    uxRecalculate.Disabled = !uxRecalculate.Enabled;
                    checkOutButton.Enabled = false;
                    checkOutButton.Disabled = !checkOutButton.Enabled;                    
                    ContinueShopping.Text = GetLocalResourceObject("ContinueShopping").ToString();
                    ProductAvailability1.Visible = false;
                    mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                }

                if ((Page as ProductsBase).ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    divEventTicketMessage.Visible = true;
                }
                else
                {
                    divSummaryMessage.Visible = HLConfigManager.Configurations.CheckoutConfiguration.HasSummaryMessage;
                }

                if (ShoppingCart.CustomerOrderDetail != null)
                {
                    uxReturnToCustomerOrder.Visible = true;
                }
                //User Story 391426:- start
                ProductsBase.GetTWSKUQuantitytorestrict();
                SetSplitOrderMessage();
                SetCurrencyConvectorMessage();
                SetApparelMessage();

                if (HLConfigManager.Configurations.CheckoutConfiguration.HideStatementOnCOP1 &&
                    this.Page.Request.Url.AbsolutePath.Equals("/Ordering/ShoppingCart.aspx"))
                {
                    StatementReader.Visible = false;
                }
                //if (DisplayReadOnlyGrid)
                //{
                //    CntrlProductDetail.Visible = false;
                //}
            }

            if (DisplayReadOnlyGrid)
            {
                divLabelErrors.Visible = false;
                //divLabelOrderSummary.Visible = false;
                tblLegend.Visible = false;
            }

            if (!String.IsNullOrEmpty(OrderSummaryText))
            {
                lblHeaderSummary.Text = OrderSummaryText;
            }
            
           

            this.SendMessage += m => ((SaveCartCommand)savedCartCommand).ListnerMethod();
            // Displaying saved carts
            if (HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts &&
                ShoppingCart.OrderCategory == OrderCategoryType.RSO)
            {
               (savedCartCommand as SaveCartCommand).IsInMinicart = false;
               (savedCartCommand as SaveCartCommand).ListnerMethod();
                plSavedCartCommand.Controls.Add(savedCartCommand);
                divSaveCarts.Visible = true;

                // If is in saved cart hide the cancel button
                if (ShoppingCart.IsSavedCart)
                {
                    OrderButtons.Visible = false;
                }
            }

            // Notify to DS the promo sku is in cart
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.NotifyPromoFromService &&
                !this.btnObservationOk.CommandArgument.Equals("Shown"))
            {
                var promo = ShoppingCartProvider.GetEligibleForPromo(this.ShoppingCart.DistributorID, this.ShoppingCart.Locale);
                if (promo != null && this.ShoppingCart.CartItems.Any(i => i.SKU.Equals(promo.Sku)) && !ShoppingCart.IsPromoNotified)
                {
                    ShoppingCart.IsPromoNotified = true;
                    this.btnObservationOk.CommandArgument = "Shown";
                    this.lblObservationText.Text = string.Format(this.GetLocalResourceObject("PromoInCart").ToString(), promo.Sku);
                    this.pnlObservation.Update();
                    this.pnlContentObservation.CssClass = this.pnlContentObservation.CssClass.Replace("gdo-hide", string.Empty);
                    this.mdlObservation.Show();
                }
            }

            //validates donations in China since they're not linked to any SKU
            //when there are donations, checkout and cancel buttons should be enabled
            OrderTotals_V02 totals = (Page as ProductsBase).ShoppingCart.Totals as OrderTotals_V02;
            if (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU && totals != null &&
                totals.Donation > 0.00m)
            {
                uxCancelOrder.Enabled = true;
                uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                checkOutButton.Enabled = true;
                checkOutButton.Disabled = !checkOutButton.Enabled;
                mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                checkOutButton.OnClientClick = "$(this).removeClass('disabled');";
            }

            if (_IsResponsiveControl)
            {
                uxProducts.Visible = false;
                uxProductsResponsive.Visible = true;

                uxPromo.Visible = false;
                uxPromoResponsive.Visible = true;
            }

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                uxCancelOrder.Text = GetLocalResourceObject("CancelHapOrder.Text").ToString();
            }

            uxChildSKU.Visible = (this.DisplayChildSKUs && !_IsResponsiveControl);
            uxChildSKUResponsive.Visible = (this.DisplayChildSKUs && _IsResponsiveControl);

            if (IsChina && !string.IsNullOrEmpty(Request.QueryString["CartID"]) )
            {
                int cartId;
                int.TryParse(Request.QueryString["CartID"], out cartId);
                GetDiscontinueSku(cartId);
            }
            if (_errors.Count > 0 )
            {
                blErrors.DataSource = _errors.Distinct().ToList();
                blErrors.DataBind();
                checkOutButton.Enabled = false;
                checkOutButton.Disabled = !checkOutButton.Enabled;
            }
            if(HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (!SessionInfo.IsReplacedPcOrder)
                {
                    if (ChinaPromotionProvider.IsEligibleForSRPromotion(ShoppingCart, HLConfigManager.Platform))
                    {
                        var cacheKey = string.Format("GetSRPromoDetail_{0}", ShoppingCart.DistributorID);
                        var results = HttpRuntime.Cache[cacheKey] as ServiceProvider.OrderChinaSvc.GetSRPromotionResponse_V01;
                        var skuList = results.Skus.Split(',').ToArray();
                        List<CatalogItem> SRPromoOnly = new List<CatalogItem>();
                        if (results != null && !string.IsNullOrWhiteSpace(results.Skus))
                        {
                            bool promoItemAdded = false;
                            if (skuList.Count() > 0)
                            {
                                if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
                                {

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
                                                SRPromoOnly.Add(sku.CatalogItem);
                                            }
                                        }
                                    }
                                    if (SRPromoOnly.Any())
                                    {
                                        if (
                                            this.ShoppingCart.CartItems.Any(
                                                cart => SRPromoOnly.Find(p => p.SKU == cart.SKU) != null))
                                        {
                                            promoItemAdded = true;

                                        }
                                        if (!promoItemAdded)
                                        {
                                            checkOutButton.Disabled = true;
                                        }
                                        else
                                            checkOutButton.Enabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetDiscontinueSku(int cartId)
        {
            var specialSkulist = ShoppingCartProvider.GetSpecialSkulist();
            var originalUnfilteredCopyCart =
                                ShoppingCartProvider.GetUnfilteredShoppingCartForCopyFromServiceint(
                                    cartId, DistributorID,
                                    Thread.CurrentThread.CurrentCulture.ToString(), 0, ProductInfoCatalog.AllSKUs);

            StringBuilder completeMessage = new StringBuilder();
            var detail = new StringBuilder();
            var discontinuedSku = ShoppingCartProvider.GetDiscontinuededSku(originalUnfilteredCopyCart);
            // Get the product info
            CatalogItemList missingProductDetail=new CatalogItemList();
            var skutobeQueried = discontinuedSku.Select(x => x.SKU).ToList();
            var listofDiscProd = CatalogProvider.GetCatalogItems(skutobeQueried, CountryCode);

            if(skutobeQueried.Any() && (listofDiscProd==null || listofDiscProd.Count==0))
            {
                CatalogItemList finalList = new CatalogItemList();

                // means there is a sku that no longer active but we fail to retrieve the info because the isdeleted true, then fetch directly from Db
                missingProductDetail = CatalogProvider.GetDiscontinuedProductDetailFromCahce(skutobeQueried);
                foreach (var item in skutobeQueried)
                {
                    if (missingProductDetail.Values.Any(x => x.SKU.Trim() == item))
                    {
                        finalList.Add(item, missingProductDetail[item]);
                    }
                }
                missingProductDetail = finalList;
                
            }
           
            
            //getDiscontinuedItem exclude virtual sku and promo Item
            var discontinue = discontinuedSku.Where(c => specialSkulist.All(d => d != c.SKU)).Where(e=>e.IsPromo==false).Select(f=>f.SKU).ToList();
            var skulist = string.Join(",", discontinue);
            
            //getPromoItem item where is not virtual and promo = true
            var listofPromoSKu = discontinuedSku.Where(c => specialSkulist.All(e => e != c.SKU)).Where(f => f.IsPromo == true).Select(g => g.SKU).ToList();
            var discontinuePromoList = string.Join(",", listofPromoSKu);
            // get virtual item 
            var virtualSku = discontinuedSku.Where(x => specialSkulist.Any(y => y == x.SKU)).Select(z=>z.SKU).ToList();
            var virtualSkuList = string.Join(",", virtualSku);
            if (!string.IsNullOrEmpty(skulist))
            {
                completeMessage.Append(string.Format((string)GetLocalResourceObject("ErrorCopyOrderDiscontinueSkuExist"), skulist) + " ");
            }
            if (!string.IsNullOrEmpty(discontinuePromoList))
            {
                completeMessage.Append(string.Format((string)GetLocalResourceObject("ErrorCopyOrderPromoEnd"), discontinuePromoList) + " ");
            }
            if (!string.IsNullOrEmpty(virtualSkuList))
            {
                completeMessage.Append(string.Format((string)GetLocalResourceObject("ErrorCopyOrderVirtualSkuExist"), virtualSkuList) + " ");
            }

            if (discontinuedSku != null && listofDiscProd != null)
            {
                DiscSkuLabelListLinkButton.Text = completeMessage.ToString();
                foreach (var item in listofDiscProd)
                {
                    detail.Append("sku " + item.Value.SKU + " " + item.Value.Description + "\n");
                }
                foreach (var item in missingProductDetail)
                {
                    detail.Append("sku " + item.Value.SKU + " " + item.Value.Description + "\n");
                }
                DiscSkuLabelList.Text = detail.ToString();
                DiscSkuLabelListLinkButton.Visible = true;

            }
            else
            {
                LoggerHelper.Error(
                    string.Format(
                        "failed to get product name listofDiscProd is null, Distributor{0}, locale{1},shoppingcart isnull:{2}",
                        DistributorID, Locale, (ShoppingCart == null) ? "null" : ShoppingCart.CartName));
            }
        }

        /// <summary>
        /// Routine will update the list of child skus that are detected on the shopping cart.
        /// It includes logic to skip any calculation if it has not detected any changes since it last execution.
        /// </summary>        
        private void CalculateChildSKUs()
        {
            var totals = _shoppingCart.Totals as OrderTotals_V01;

            // verify that if the shopping cart has not had any changes, skip the recalculation
            var localCacheKey = string.Empty;
            for (int i = 0; i < _shoppingCart.ShoppingCartItems.Count; i++)
            {
                localCacheKey += _shoppingCart.ShoppingCartItems[i].SKU + "|" + _shoppingCart.ShoppingCartItems[i].Quantity;
            }

            if (string.Compare(localCacheKey, this.RecalculateChildSKUCacheKey) == 0)
                return;
                        
            var dictSKUs = new Dictionary<string, DistributorShoppingCartItem>();

            if (totals != null && totals.ItemTotalsList != null)
            {
                foreach (DistributorShoppingCartItem cartItem in _shoppingCart.ShoppingCartItems)
                {
                    SKU_V01 skuReference;
                    if (_AllSKUS.TryGetValue(cartItem.SKU, out skuReference) && skuReference.SubSKUs != null)
                    {
                        foreach (var filteredSubSKU in skuReference.SubSKUs.Where(subSKU => subSKU.IsDisplayable))
                        {
                            bool isAddedToCart = false;
                            ItemTotal_V01 itemTotalOrder = new ItemTotal_V01();

                            //validate that Child SKU is already priced (means that already passed Cart and Order rules)
                            foreach (ItemTotal_V01 itemTotal in totals.ItemTotalsList)
                            {
                                if (itemTotal.SKU == filteredSubSKU.SKU)
                                {
                                    itemTotalOrder = itemTotal;
                                    isAddedToCart = true;
                                    break;
                                }
                            }

                            if (isAddedToCart)
                            {
                                DistributorShoppingCartItem refChildSKU = null;
                                if (!dictSKUs.TryGetValue(filteredSubSKU.SKU, out refChildSKU))
                                {
                                    refChildSKU = new DistributorShoppingCartItem();
                                    refChildSKU.ProdInfo = new ProductInfo_V02();
                                    refChildSKU.SKU = filteredSubSKU.SKU;
                                    refChildSKU.ProdInfo.ID = filteredSubSKU.ID;
                                    refChildSKU.Description = filteredSubSKU.Description;
                                    refChildSKU.Quantity = 0;

                                    refChildSKU.RetailPrice = itemTotalOrder.RetailPrice;
                                    refChildSKU.VolumePoints = itemTotalOrder.VolumePoints;
                                    refChildSKU.EarnBase = itemTotalOrder.EarnBase;
                                    refChildSKU.DiscountPrice = itemTotalOrder.DiscountedPrice;

                                    //populate additional information of the item
                                    this.ShoppingCart.AdditionalInfoHelper(Locale, refChildSKU);

                                    dictSKUs.Add(refChildSKU.SKU, refChildSKU);
                                }

                                refChildSKU.Quantity += cartItem.Quantity;
                            }
                        }
                    }
                }
            }

            // save values to the corresponding properties in the controller
            this.RecalculateChildSKUCacheKey = localCacheKey;
            this.ChildSKUs = dictSKUs.Values.ToList();
        }
        protected void OnDisplayDiscontinueSkuClose(object sender, EventArgs e)
        {
            displaySkuPopupModal.Hide();
        }
        protected void OnSaveClicked(object sender, EventArgs e)
        {
            var unavailableItems = GetListToRemove();
            if (unavailableItems.Count > 0)
            {
                var prodIds = from s in unavailableItems select s.SKU;
                ProductsBase.DeleteItemsFromCart(prodIds.ToList());
                if (null != ShoppingCart.CartItems && ShoppingCart.CartItems.Count > 0)
                {
                    string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
                    string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
                    var catItems = _shoppingCart.ShoppingCartItems.Select(s => s.CatalogItem).ToList();
                    if (
                        !catItems.Any(
                            c =>
                            c.ProductType == ServiceProvider.CatalogSvc.ProductType.Product &&
                            catItems.Any(i => i.SKU.Trim() == primarySku || i.SKU.Trim() == secondarySku)))
                    {
                        if (catItems.Any(c => c.SKU.Trim() == primarySku))
                            ShoppingCart.DeleteTodayMagazine(primarySku);
                        else
                            ShoppingCart.DeleteTodayMagazine(secondarySku);

                        NotifyTodaysMagazineCancelOrder(this, null);
                    }
                }
                if (ShoppingCart.ShoppingCartItems.Count > 0)
                {
                    AddOrderMonthToSessionInfo();
                    Response.Redirect("~/Ordering/Checkout.aspx",false);
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    doCartEmpty();
                    setupShoppingCartDataSource(Level);
                    mdlConfirm.Hide();
                    NotifyTodaysMagazineCancelOrder(this, null);
                    if (IsChina && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        if (HttpContext.Current != null)
                        {
                            string rawURL = HttpContext.Current.Request.RawUrl;
                            if (rawURL.Contains("Checkout.aspx"))
                            {
                                Response.Redirect("~/Ordering/pricelist.aspx?ETO=TRUE", false);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                            }
                        }
                    }
                }
            }
        }

        private void AddOrderMonthToSessionInfo()
        {
            var currentSession = SessionInfo;
            if (null != currentSession)
            {
                var orderMonth = new OrderMonth(CountryCode);
                currentSession.OrderMonthString = orderMonth.OrderMonthString;
                currentSession.OrderMonthShortString = orderMonth.OrderMonthShortString;
            }
        }

        private void ClearOrderMonthFromSessionInfo()
        {
            var currentSession = SessionInfo;
            if (null != currentSession)
            {
                currentSession.OrderMonthString = string.Empty;
                currentSession.OrderMonthShortString = string.Empty;
            }
        }

        protected void btnYesCancel_Click(object sender, EventArgs e)
        {
            _errors.Clear();
            blErrors.DataSource = _errors;
            blErrors.DataBind();
            if (ShoppingCart.CustomerOrderDetail == null)
            {
                doCartEmpty();
            }
            else
            {
                ShoppingCartProvider.DeleteOldShoppingCartForCustomerOrder(DistributorID,
                                                                           ShoppingCart.CustomerOrderDetail.
                                                                                        CustomerOrderID);

                ClearCustomerSession();
                //Update Status of Order To In Progress In Azure
                var customerOrderV01 =
                    CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
                CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID, customerOrderV01.OrderStatus,
                                                                ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.Cancelled);

                //Redirect To Order Details Page
                Response.Redirect("~/dswebadmin/customerorderdetail.aspx?orderid=" + customerOrderV01.OrderID);
            }

            try
            {
                var cartWidgetSource = new CartWidgetSource();
                cartWidgetSource.ExpireShoppingCartCache(DistributorID, Locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                       string.Format("Error occurred ClearMyHL3ShoppingCartCache. Id is {0}-{1}.\r\n{2}", DistributorID, Locale,
                                     ex.Message));
            }
            // total.DiscountPercentage = this.DistributorDiscount
            // this will refresh shipping box
            setupShoppingCartDataSource(Level);
            NotifyTodaysMagazineCancelOrder(this, null);
            //Response.Redirect(HLConfigManager.Configurations.DOConfiguration.LandingPage);
            // PickupOrDelivery.ResetNavigation();
            // ClearPreviousPage(DistributorID);
            SendMessage("");

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                // Redirect to HAP Orders landing page
                Response.Redirect("~/Ordering/HAPOrders.aspx");
            }
            var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && OrderTotals.Donation > 0)
            {
                SessionInfo.ClearStandAloneDonation();
                OnStandAloneDonationClear(this, e);
                OnSCancelDonationsVisible(this, e);
            }
            
        }

        #region ControlEvents

        /// <summary>
        ///     The on recalculate.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnRecalculate(object sender, EventArgs e)
        { 
            blErrors.DataSource = _errors;
            blErrors.DataBind();
            var initialVP = ShoppingCart.VolumeInCart;
            recalculate();
            OnCartRecalculated(this, new EventArgs());
            mdlConfirmDelete.Enabled = false;
            if (ShoppingCart.ShoppingCartItems.Count > 0 && _errors.Count == 0)
            {
                mdlConfirmDelete.Enabled = true;
                if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems))
                {
                    OnItemRemovedAPFLeft(this, null);
                }
            }

            #region China special promotion : Lower Price Delivery
            // CN has a special promotion need to manipulate the express company dropdownlist
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                // when recalculated, need to recheck the special promotion
                HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartCalculated);

                // and then reflect the express company dropdownlist changes, through simulate delivery address nickname changed
                // and hide red explanation word if needed
                RaiseCNCartRecalculated(sender, e);



                //Refresh the grid again.
                uxProducts.DataSource =
                       _shoppingCart.ShoppingCartItems.Where(
                           i => i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                                i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku && !i.IsPromo);
                Totals = _shoppingCart.Totals as OrderTotals_V01;
                uxProducts.DataBind();
                getLineItemWithAllCharges();

                //APF delete checkbox disabling 
                delDisableControls recheckDisableButton = delegate(ListViewItem item, bool isSP)
                {
                    var uxQty = item.ClientID.Contains("uxProducts") ? item.FindControl("uxQuantity") as TextBox : item.FindControl("uxPromoQuantity") as TextBox;
                    var uxDelete = item.ClientID.Contains("uxProducts") ? item.FindControl("uxDelete") as CheckBox : item.FindControl("uxPromoDelete") as CheckBox;
                    var uxProductoID = item.ClientID.Contains("uxProducts") ? item.FindControl("uxProductoID") as HiddenField : item.FindControl("uxPromoID") as HiddenField;
                    if (uxDelete != null && uxProductoID != null)
                    {
                        bool isAPFsku = APFDueProvider.IsAPFSku(uxProductoID.Value);
                        if (isAPFsku)
                        {
                            uxDelete.Visible = shouldDeleteVisible(this.Level, isAPFsku, uxProductoID.Value);
                            uxQty.ReadOnly = true;
                        }
                    }
                };

                Array.ForEach(uxProducts.Items.ToArray(), a => recheckDisableButton(a, true));
            }
            #endregion

            // Display PopUp for MY Promotion
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayPromoPopUp && ShoppingCart.DisplayPromo)
            {
                Promotion_MY_Control.ShowPromo();
                ShoppingCart.DisplayPromo = false;
            }
            //Verify if Shipping Methods should be displayed
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit != 0 )
                if (initialVP > HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit || ShoppingCart.VolumeInCart > HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShippingMethodsVPLimit)
                    ShippingMethodCheckVP(this, null);
        }

        /// <summary>
        ///     The on checkout.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void ReturnToCustomerOrdersSearch(object sender, EventArgs e)
        {
            ClearCustomerSession();
            Response.Redirect("~/dswebadmin/customerorders.aspx");
        }

        private void ClearCustomerSession()
        {
            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
            currentSession.CustomerOrderNumber = null;
            currentSession.ShoppingCart = null;
            var customerAddress =
                currentSession.ShippingAddresses.Find(p => p.ID == currentSession.CustomerAddressID);
            if (customerAddress != null)
            {
                currentSession.ShippingAddresses.Remove(customerAddress);
            }
            currentSession.CustomerAddressID = 0;
            SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
        }

        /// <summary>
        ///     The on checkout.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnCheckout(object sender, EventArgs e)
        {
            try
            {
                recalculate();

                // Display PopUp for MY Promotion
                if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayPromoPopUp && ShoppingCart.DisplayPromo && ShoppingCart.DisplayPromo)
                {
                    Promotion_MY_Control.ShowPromo();
                    ShoppingCart.DisplayPromo = false;
                    return;
                }

                // Validate HAP Rules
                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && _shoppingCart.OrderCategory == OrderCategoryType.HSO)
                {
                    ServerRulesManager.Instance.ValidateHAPRules(_shoppingCart, this.Locale);

                    var ruleResultMessages =
                                _shoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "HAP Rules")
                                            .Select(r => r.Messages.Count > 0 ? r.Messages[0] : string.Empty)
                                            .ToList();

                    if (ruleResultMessages.Any())
                    {
                        _errors.AddRange(ruleResultMessages.Distinct().ToList());
                    }

                    bool isValidAddress;
                    ShippingAddress_V02 address = new ShippingAddress_V02(ShoppingCart.DeliveryInfo.Address);

                    switch (CountryCode)
                    {
                        case "US":
                            string errStr = string.Empty;
                            ServiceProvider.AddressValidationSvc.Address avsAddress;
                            isValidAddress = ProductsBase.GetShippingProvider().ValidateAddress(address, out errStr, out avsAddress);
                            if (!isValidAddress && !string.IsNullOrEmpty(errStr))
                                _errors.Add(errStr);
                            break;
                        default:
                            isValidAddress = ProductsBase.GetShippingProvider().ValidateAddress(address);
                            if (!isValidAddress)
                                _errors.Add("Generic Address error"); //TODO: Add generic error for address
                            break;
                    }

                    if (address == null || string.IsNullOrEmpty(address.Phone))
                    {
                        isValidAddress = false;
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone"));
                    }

                    if (_errors.Count > 0)
                    {
                        blErrors.DataSource = _errors.Distinct();
                        blErrors.DataBind();
                        return;
                }
                }

                //validates donations in China since they're not linked to any SKU
                //when there are donations only, proceed to checkout
                OrderTotals_V02 totals = (Page as ProductsBase).ShoppingCart.Totals as OrderTotals_V02;
                if ((_shoppingCart != null && _shoppingCart.ShoppingCartItems != null && _shoppingCart.ShoppingCartItems.Count > 0 && !exceedsPurchasingLimit)
                    || (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU && totals.Donation > 0.00m))
                {
                    // fire event to quick view
                    ProceedingToCheckout(this, null);

                    if (HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                    {
                        ServerRulesManager.Instance.ValidateAPF(_shoppingCart, this.Locale);

                        var ruleResultMessages =
                                _shoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "APF Rules")
                                            .Select(r => r.Messages.Count > 0 ? r.Messages[0] : string.Empty)
                                            .ToList();

                        if (ruleResultMessages.Any())
                        {
                            _errors.AddRange(ruleResultMessages.Distinct().ToList());
                        }
                    }
                    List<string> results;
                    if (!SKULimitationProvider.CheckSKULimitation(this.CountryCode, _shoppingCart, out results))
                    {
                        _errors.AddRange(results);
                    }

                    if (_errors.Count > 0)
                    {
                        blErrors.DataSource = _errors.Distinct();
                        blErrors.DataBind();
                        return;
                    }

                    if (!CheckOutOptionsHasErrors)
                    {
                        if (ProductsBase.GetShippingProvider().ValidateShipping(_shoppingCart))
                        {
                            //Check for DR fraud..
                            if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                            {
                                if (null != ShoppingCart && null != ShoppingCart.DeliveryInfo &&
                                    null != ShoppingCart.DeliveryInfo.Address &&
                                    null != ShoppingCart.DeliveryInfo.Address.Address)
                                {
                                    var distributorProfile = DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode);
                                    ShoppingCart.DSFraudValidationError =
                                        DistributorOrderingProfileProvider.CheckForDRFraud(distributorProfile,
                                                                                           ShoppingCart.DeliveryInfo
                                                                                                       .Address.Address.PostalCode);
                                    if (!string.IsNullOrEmpty(ShoppingCart.DSFraudValidationError))
                                    {
                                        _errors.Add(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                ShoppingCart.DSFraudValidationError) as string);
                                        blErrors.DataSource = _errors;
                                        blErrors.DataBind();
                                        return;
                                    }
                                }
                            }

                            var unavailableItems = GetListToRemove();
                            if (unavailableItems.Count > 0)
                            {
                                if (ShoppingCart.CustomerOrderDetail != null)
                                {
                                    _errors.Add(
                                        GetLocalResourceObject("CustomerOrderItemUnavailableCheckoutError").ToString());
                                    blErrors.DataSource = _errors;
                                    blErrors.DataBind();
                                    return;
                                }
                                uxProdToRemove.DataSource = unavailableItems;
                                uxProdToRemove.DataBind();

                                updConfirmUnavailable.Update();
                                mdlConfirm.Show();
                            }
                            else
                            {
                                AddOrderMonthToSessionInfo();
                                var sessionInfo =
                                    SessionInfo.GetSessionInfo(DistributorID, Locale);
                                if (!string.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                                {
                                    sessionInfo.DeliveryInfo = ShoppingCart.DeliveryInfo;
                                    SessionInfo.SetSessionInfo(DistributorID, Locale, sessionInfo);
                                }
                                var redirectUrl = string.Empty;
                                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                                {
                                    AddressRestrictionConfirmPopupExtender.Show();
                                    return;
                                }
                                else if((!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.autoSelectShippingaddres) && _shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                                {
                                    AutoSelectShippingaddres.InnerHtml =
                                              ProductsBase.GetShippingProvider()
                                                                .FormatShippingAddress(_shoppingCart.DeliveryInfo.Address , _shoppingCart.DeliveryInfo.Option,
                                                                 ShoppingCart != null && ShoppingCart.DeliveryInfo != null
                                                                 ? ShoppingCart.DeliveryInfo.Description
                                                                 : string.Empty, true);
                                    AutoSelectShippingaddresPopupExtender.Show();
                                    return;

                                }
                                else
                                {
                                     redirectUrl = (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && _shoppingCart.OrderCategory == OrderCategoryType.HSO) ? "~/Ordering/Checkout.aspx?HAP=True" : "~/Ordering/Checkout.aspx";
                                }
                                //if (HLConfigManager.Configurations.CheckoutConfiguration.CheckPaymentPendingOrder)
                                //{
                                //    var orders = OrdersProvider.GetOrdersInProcessing(DistributorID, Locale);
                                //    if (orders != null && orders.Any())
                                //    {
                                //        orders = orders.OrderByDescending(p => p.SubmittedDate).ToList();
                                //        if (orders.FirstOrDefault().OrderStatus == "Unknown")
                                //        {
                                //            redirectUrl = "~/Ordering/OrderListView.aspx";// "?PendingOrderID=" + orders.FirstOrDefault().OrderId;
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    OrderProvider.CreateOrder("", ShoppingCart, null);
                                //    redirectUrl = "~/Ordering/Checkout.aspx";
                                //}

                                try
                                {
                                    Response.Redirect(redirectUrl,false);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                                finally
                                {
                                    Response.Redirect(redirectUrl,false);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                }
                            }
                        }
                        else
                        {
                            if (ShoppingCart != null)
                            {
                                LoggerHelper.Error(ShoppingCart.DeliveryInfo != null
                                    ? string.Format("WareHouseCode{0},FreightCode{1},DistributorId{2},Locale{3}",
                                        ShoppingCart.DeliveryInfo.WarehouseCode, ShoppingCart.DeliveryInfo.FreightCode,
                                        DistributorID, Locale)
                                    : "OnCheckout ValidateShipping falis DeliveryInfo Null ");
                            }
                            else
                            {
                                LoggerHelper.Error("OnCheckout ValidateShipping falis shopping cart Null ");
                            }
                        }
                    }
                }
                else if (!exceedsPurchasingLimit)
                {
                    doCartEmpty();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("OnCheckout error : " + ex);
            }
        }

        /// <summary>
        ///     The on continue shopping.
        /// </summary>
        /// <param name="Source">
        ///     The source.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnContinueShopping(object Source, EventArgs e)
        {
            ClearOrderMonthFromSessionInfo();
            CheckoutToShoppingCart(this, null);
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                Response.Redirect(string.Concat(ROOT_PATH, PATH_PRICELIST), false);
            }
            else if(ProductsBase.IsHAPMode)
                Response.Redirect("~/Ordering/Catalog.aspx?HAP=True", false);
            else
                Response.Redirect("~/Ordering/Catalog.aspx", false);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        ///     The shopping cart on item data bound.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void ShoppingCartOnItemDataBound(object sender, ListViewItemEventArgs e)
        {

            if (_IsResponsiveControl)
            {
                if (e.Item.ItemType == ListViewItemType.DataItem)
                {

                    if (DisplayReadOnlyGrid)
                    {
                        var tc = e.Item.FindControl("TdDeleteCheckbox");
                        if (tc != null)
                        {
                            tc.Visible = false;
                        }
                        var txtQuantity = (TextBox)e.Item.FindControl("uxQuantity");
                        var lblQuantity = (Label)e.Item.FindControl("lblQuantity");
                        if (txtQuantity != null)
                        {
                            txtQuantity.Visible = false;
                        }
                        if (lblQuantity != null)
                        {
                            lblQuantity.Visible = true;
                        }
                    }
                    var lbSKU = (Label)e.Item.FindControl("idSKU");
                    var prodLink = (LinkButton)e.Item.FindControl("LinkProductDetail") ;
                    var lbProd = (Label)e.Item.FindControl("lbProductName") ;
                    var nonLinkedSkus = APFDueProvider.GetAPFSkuList();
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku);
                    }
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku);
                    }
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku);
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0 )  //new config for HFF
                    {
                        nonLinkedSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
                    }

                    if (prodLink != null && lbSKU != null)
                    {
                        prodLink.Visible = !DisplayReadOnlyGrid && !nonLinkedSkus.Contains(lbSKU.Text.Trim());
                    }

                    if (lbProd != null && lbSKU != null)
                    {
                        lbProd.Visible = DisplayReadOnlyGrid || nonLinkedSkus.Contains(lbSKU.Text.Trim());
                    }

                    if (!HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase ||
                        ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        var earnBaseCol = (HtmlTableCell)e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }
                    else if (HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase &&
                             !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBaseBySku)
                    {
                        var earnBaseCol = (HtmlTableCell)e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }
                    else if (ShoppingCart.OrderCategory == OrderCategoryType.HSO && ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
                    {
                        var earnBaseCol = (HtmlTableCell)e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }
                    
                    if (ShoppingCart.CustomerOrderDetail != null)
                    {
                        var chkDelete = (CheckBox)e.Item.FindControl("uxDelete");
                        foreach (DistributorShoppingCartItem item in ShoppingCart.ShoppingCartItems)
                        {
                            if (item.SKU.ToUpper().Trim() == lbSKU.Text.Trim().ToUpper())
                            {
                                if (item.MinQuantity > 0)
                                {
                                    chkDelete.Visible = false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (e.Item.ItemType == ListViewItemType.DataItem)
                {
                    if (DisplayReadOnlyGrid)
                    {
                        var tc = (HtmlTableCell) e.Item.FindControl("TdDeleteCheckbox");
                        if (tc != null)
                        {
                            tc.Visible = false;
                        }
                        var txtQuantity = e.Item.ClientID.Contains("uxProducts") ? (TextBox)e.Item.FindControl("uxQuantity") : (TextBox)e.Item.FindControl("uxPromoQuantity");
                        var lblQuantity = e.Item.ClientID.Contains("uxProducts") ? (Label)e.Item.FindControl("lblQuantity") : (Label)e.Item.FindControl("lblPromoQuantity");
                        if (txtQuantity != null)
                        {
                            txtQuantity.Visible = false;
                        }
                        if (lblQuantity != null)
                        {
                            lblQuantity.Visible = true;
                        }
                    }

                    var lbSKU = e.Item.ClientID.Contains("uxProducts") ? (Label)e.Item.FindControl("idSKU") : (Label)e.Item.FindControl("idPromoSKU");
                    var prodLink = e.Item.ClientID.Contains("uxProducts") ? (LinkButton)e.Item.FindControl("LinkProductDetail") : (LinkButton)e.Item.FindControl("LinkPromoDetail");
                    var lbProd = e.Item.ClientID.Contains("uxProducts") ? (Label)e.Item.FindControl("lbProductName") : (Label)e.Item.FindControl("lbPromoName");
                    var nonLinkedSkus = APFDueProvider.GetAPFSkuList();
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku);
                    }
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku);
                    }
                    if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku))
                    {
                        nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku);
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0)  //new config for HFF
                    {
                        nonLinkedSkus.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);
                    }

                    if (prodLink != null && lbSKU != null)
                    {
                        prodLink.Visible = !DisplayReadOnlyGrid && !nonLinkedSkus.Contains(lbSKU.Text.Trim());
                    }
                    if (ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled)
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.DsType == ServiceProvider.DistributorSvc.Scheme.Member && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                        {
                            if (lbProd != null && lbSKU != null)
                            {
                                prodLink.Enabled = false;
                            }
                        }
                    }
                    else if (lbProd != null && lbSKU != null)
                    {
                        lbProd.Visible = DisplayReadOnlyGrid || nonLinkedSkus.Contains(lbSKU.Text.Trim());
                    }

                    if (!HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase ||
                        ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        var earnBaseCol = (HtmlTableCell) e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }
                    else if (HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase &&
                             !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBaseBySku)
                    {
                        var earnBaseCol = (HtmlTableCell)e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }
                    else if (ShoppingCart.OrderCategory == OrderCategoryType.HSO && ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
                    {
                        var earnBaseCol = (HtmlTableCell)e.Item.FindControl("TdEarnBase");
                        if (earnBaseCol != null)
                        {
                            earnBaseCol.Visible = false;
                        }
                    }

                    if (!HLConfigManager.Configurations.CheckoutConfiguration.HasYourPrice)
                    {
                        var yourPriceCol = (HtmlTableCell) e.Item.FindControl("TdYourPrice");
                        if (yourPriceCol != null)
                        {
                            yourPriceCol.Visible = false;
                        }
                    }
                    else
                    {
                        if (HLConfigManager.Configurations.CheckoutConfiguration.HideYourPriceOnCOP1)
                        {
                            var yourPriceCol = (HtmlTableCell) e.Item.FindControl("TdYourPrice");
                            if (yourPriceCol != null)
                            {
                                yourPriceCol.Visible = DisplayReadOnlyGrid;
                            }
                        }
                    }

                    if (HLConfigManager.Configurations.CheckoutConfiguration.HideRetailPriceOnCOP1 ||
                        !HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPrice)
                    {
                        var retailPriceCol = (HtmlTableCell) e.Item.FindControl("TdRetailPrice");
                        if (retailPriceCol != null)
                        {
                            retailPriceCol.Visible = DisplayReadOnlyGrid &&
                                                     HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPrice;
                        }
                    }

                    if (!HLConfigManager.Configurations.CheckoutConfiguration.HasVolumePoints &&
                        ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        var volumePoints = (HtmlTableCell) e.Item.FindControl("TdVolumePoint");
                        if (volumePoints != null)
                        {
                            volumePoints.Visible = false;
                        }
                    }

                    if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayVolumePointsForEventTicket &&
                        ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        var volumePoints = (HtmlTableCell) e.Item.FindControl("TdVolumePoint");
                        if (volumePoints != null)
                        {
                            volumePoints.Visible = false;
                        }
                    }

                    if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                    {
                        var volumePoints = (HtmlTableCell)e.Item.FindControl("TdVolumePoint");
                        if (volumePoints != null)
                        {
                            volumePoints.Visible = false;
                        }
                    }

                    if (ShoppingCart.CustomerOrderDetail != null)
                    {
                        var chkDelete = (CheckBox)e.Item.FindControl("uxDelete");
                        foreach (DistributorShoppingCartItem item in ShoppingCart.ShoppingCartItems)
                        {
                            if (item.SKU.ToUpper().Trim() == lbSKU.Text.Trim().ToUpper())
                            {
                                if (item.MinQuantity > 0)
                                {
                                    chkDelete.Visible = false;
                                }
                            }
                        }
                    }
                }
                
            }
            
        }

        private void getLineItemWithAllCharges()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasYourPrice &&
                HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges &&
                Totals != null && Totals.ItemTotalsList != null)
            {
                foreach (ListViewDataItem s in uxProducts.Items)
                {
                    ListViewItem item = s;
                    if (item.ItemType == ListViewItemType.DataItem)
                    {
                        var hf = item.FindControl("uxProductoID") as HiddenField;

                        var yourVolumePoints = (Label) item.FindControl("YourVolumePoints");
                        if (yourVolumePoints != null)
                        {
                            string sku = hf.Value;
                            var lineItem =
                                Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                            if (lineItem != null)
                            {
                                var volumePoints = lineItem.VolumePoints;
                                yourVolumePoints.Text = ProductsBase.GetVolumePointsFormat(volumePoints);
                            }
                        }

                        var yourPrice = (Label) item.FindControl("YourPrice");
                        if (hf != null && yourPrice != null)
                        {
                            var lineItemTotal = (decimal) 0;
                            string sku = hf.Value;
                            var lineItem =
                                Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                            if (lineItem != null)
                            {
                                lineItemTotal = OrderProvider.getPriceWithAllCharges(Totals, sku, lineItem.Quantity);
                                yourPrice.Text = getAmountString(lineItemTotal);
                            }
                        }
                    }
                }
            }
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPriceForLiterature &&
                    !HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
            {
                foreach (var s in uxProducts.Items)
                {
                    ListViewItem item = s;
                    if (item.ItemType == ListViewItemType.DataItem)
                    {
                        var hf = item.FindControl("uxProductoID") as HiddenField;
                        var volumepoints = item.FindControl("YourVolumePoints") as Label;
                        //for KR all the literature items have 0 VP
                        if (hf != null && volumepoints != null
                            && volumepoints.Text != null &&
                            volumepoints.Text == "0.00")
                        {
                            //get retail price
                            //show in earnbase
                            var retailPriceCol = (HtmlTableCell) item.FindControl("TdRetailPrice");
                            var earnBase = (HtmlTableCell)item.FindControl("TdEarnBase");
                            if (retailPriceCol != null && earnBase != null)
                            {
                                earnBase.InnerHtml = retailPriceCol.InnerHtml;
                            }
                        }
                    }
                }
            }
        }
        private void getLineItemWithAllChargesforpromo()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasYourPrice &&
                HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges &&
                Totals != null && Totals.ItemTotalsList != null)
            {
                foreach (ListViewDataItem s in uxPromo.Items)
                {
                    ListViewItem item = s;
                    if (item.ItemType == ListViewItemType.DataItem)
                    {
                        var hf = item.FindControl("uxPromoID") as HiddenField;

                        var yourVolumePoints = (Label)item.FindControl("YourVolumePoints");
                        if (yourVolumePoints != null)
                        {
                            string sku = hf.Value;
                            var lineItem =
                                Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                            if (lineItem != null)
                            {
                                var volumePoints = lineItem.VolumePoints;
                                yourVolumePoints.Text = ProductsBase.GetVolumePointsFormat(volumePoints);
                            }
                        }

                        var yourPrice = (Label)item.FindControl("YourPrice");
                        if (hf != null && yourPrice != null)
                        {
                            var lineItemTotal = (decimal)0;
                            string sku = hf.Value;
                            var lineItem =
                                Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                            if (lineItem != null)
                            {
                                lineItemTotal = OrderProvider.getPriceWithAllCharges(Totals, sku, lineItem.Quantity);
                                yourPrice.Text = getAmountString(lineItemTotal);
                            }
                        }
                    }
                }
            }
        }

        private void getLineItemWithAllChargesChildSKUs()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.HasYourPrice &&
                HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges &&
                Totals != null && Totals.ItemTotalsList != null)
            {
                foreach (ListViewDataItem s in uxChildSKU.Items)
                {
                    ListViewItem item = s;
                    if (item.ItemType == ListViewItemType.DataItem)
                    {
                        var hf = item.FindControl("uxProductoID") as HiddenField;

                        //var yourVolumePoints = (Label)item.FindControl("YourVolumePoints");
                        //if (yourVolumePoints != null)
                        //{
                        //    string sku = hf.Value;
                        //    var lineItem =
                        //        Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                        //    if (lineItem != null)
                        //    {
                        //        var volumePoints = lineItem.VolumePoints;
                        //        yourVolumePoints.Text = ProductsBase.GetVolumePointsFormat(volumePoints);
                        //    }
                        //}

                        var yourPrice = (Label)item.FindControl("YourPrice");
                        if (hf != null && yourPrice != null)
                        {
                            var lineItemTotal = (decimal)0;
                            string sku = hf.Value;
                            var lineItem =
                                Totals.ItemTotalsList.Find(x => (x as ItemTotal_V01).SKU == sku) as ItemTotal_V01;
                            if (lineItem != null)
                            {
                                lineItemTotal = OrderProvider.getPriceWithAllCharges(Totals, sku, lineItem.Quantity);
                                yourPrice.Text = getAmountString(lineItemTotal);
                            }
                        }
                    }
                }
            }
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPriceForLiterature &&
                    !HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
            {
                foreach (var s in uxChildSKU.Items)
                {
                    ListViewItem item = s;
                    if (item.ItemType == ListViewItemType.DataItem)
                    {
                        //var hf = item.FindControl("uxProductoID") as HiddenField;
                        //var volumepoints = item.FindControl("YourVolumePoints") as Label;
                        ////for KR all the literature items have 0 VP
                        //if (hf != null && volumepoints != null
                        //    && volumepoints.Text != null &&
                        //    volumepoints.Text == "0.00")
                        //{
                            //get retail price
                            //show in earnbase
                            var retailPriceCol = (HtmlTableCell)item.FindControl("TdRetailPrice");
                            var earnBase = (HtmlTableCell)item.FindControl("TdEarnBase");
                            if (retailPriceCol != null && earnBase != null)
                            {
                                earnBase.InnerHtml = retailPriceCol.InnerHtml;
                        }
                        //}
                    }
                }
            }
        }

        /// <summary>
        ///     The shopping cart on data bound.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void ShoppingCartOnDataBound(object sender, EventArgs e)
        {
            ListView listviewID = (ListView) sender;
            if (DisplayReadOnlyGrid)
            {
                if (listviewID.FindControl("THRemove") != null)
                {
                    listviewID.FindControl("THRemove").Visible = false;
                }
            }

            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase ||
                ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                if (listviewID.FindControl("THEarnBase") != null)
                {
                    listviewID.FindControl("THEarnBase").Visible = false;
                }
            }
            else if (HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBase &&
                     !HLConfigManager.Configurations.CheckoutConfiguration.HasEarnBaseBySku)
            {
                if (listviewID.FindControl("THEarnBase") != null)
                {
                    listviewID.FindControl("THEarnBase").Visible = false;
                }
            }

            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasYourPrice)
            {
                if (listviewID.FindControl("THYourPrice") != null)
                {
                    listviewID.FindControl("THYourPrice").Visible = false;
                }
            }

            if (!HLConfigManager.Configurations.CheckoutConfiguration.HasVolumePoints &&
                ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                if (listviewID.FindControl("THVolPoints") != null)
                {
                    listviewID.FindControl("THVolPoints").Visible = false;
                }
            }
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayVolumePointsForEventTicket &&
                ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                if (listviewID.FindControl("THVolPoints") != null)
                {
                    listviewID.FindControl("THVolPoints").Visible = false;
                }
            }

            if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
            {
                if (listviewID.FindControl("THVolPoints") != null)
                {
                    listviewID.FindControl("THVolPoints").Visible = false;
                }
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HideRetailPriceOnCOP1 ||
                !HLConfigManager.Configurations.CheckoutConfiguration.HasRetailPrice)
            {
                if (listviewID.FindControl("THRetailPrice") != null)
                {
                    listviewID.FindControl("THRetailPrice").Visible = DisplayReadOnlyGrid &&
                                                                      HLConfigManager.Configurations
                                                                                     .CheckoutConfiguration
                                                                                     .HasRetailPrice;
                }
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.HideYourPriceOnCOP1)
            {
                if (listviewID.FindControl("THYourPrice") != null)
                {
                    listviewID.FindControl("THYourPrice").Visible = DisplayReadOnlyGrid;
                }
            }
           
            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO &&
                HLConfigManager.Configurations.DOConfiguration.DisplayBifurcationKeys && 
                ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                if (listviewID.FindControl("THVolPoints") != null && listviewID.FindControl("lbVolumePoints") != null)
                {
                    (listviewID.FindControl("lbVolumePoints") as Label).Text = GetLocalResourceObject("lbVolumePointsResource1MB.Text").ToString();
                }

                if (listviewID.FindControl("THEarnBase") != null)
                {
                    listviewID.FindControl("THEarnBase").Visible = false;
                }

                if (listviewID.FindControl("THRemove") != null)
                {
                    (listviewID.FindControl("THRemove") as HtmlTableCell).Attributes.Add("class", "wdth50");
                }
            }


        }

        #endregion ControlEvents

        #region HelperMethods

        /// <summary>
        ///     The do calculation.
        /// </summary>
        /// <param name="products">
        ///     The products.
        /// </param>
        private void doCalculation(List<ShoppingCartItem_V01> products)
        {
            try
            {
                if (_shoppingCart != null && _shoppingCart.ShoppingCartItems != null)
                {
                    //List<ShoppingCartItem_V01> temp;

                    //// store proc will add quantity to existing quantity
                    //// so deduct quantity here in case quantity is different.
                    //foreach (ShoppingCartItem_V01 p in products)
                    //{
                    //    if ((temp = _shoppingCart.CartItems.Where(s => s.SKU == p.SKU).ToList()) != null && temp.Count > 0)
                    //    {
                    //        p.Quantity = p.Quantity - (temp.Count > 0 ? temp[0].Quantity : 0);
                    //    }
                    //}
                    OrderTotals_V02 totals = _shoppingCart.Totals as OrderTotals_V02;                
                    if (totals != null && (_shoppingCart.CartItems.Count == 0 &&
                                           (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU && totals.Donation == 0.00m)))
                    {
                        doCartEmpty();
                    }
                }
                else
                {
                    doCartEmpty();
                }

                if (_prodsDeleted.Count > 0)
                {
                    ProductsBase.DeleteItemsFromCart(_prodsDeleted);
                    if (_shoppingCart != null && _shoppingCart.RuleResults != null)
                    {
                    foreach (ShoppingCartRuleResult result in _shoppingCart.RuleResults)
                    {
                        if ((result.Result == RulesResult.Failure || result.Result == RulesResult.Recalc) &&
                                result.Messages != null && result.Messages.Count > 0 )
                        {
                            _errors.Add(result.Messages[0]);
                        }
                    }

                    if (ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback) && ShoppingCart.RuleResults != null && ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback).Messages != null &&
                        ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback).Messages.Any())
                    {
                        // Checking for specific message to display
                        var feedbackResult =
                            ShoppingCart.RuleResults.FirstOrDefault(
                                r =>
                                    r.Result == RulesResult.Feedback &&
                                    r.Messages.Any(m => m != null && m.StartsWith("HTML|")));
                        if (feedbackResult != null)
                        {
                                if (feedbackResult.Messages.Any())
                                {
                            var fragmentName = feedbackResult.Messages[0].Split('|');
                            if (fragmentName.Count() == 2 && fragmentName[1].Length > 0)
                            {
                                (this.Master as OrderingMaster).DisplayHtml(fragmentName[1]);
                            }
                        }
                    }
                        }

                    //There should be at least one product in the cart
                        if (_shoppingCart!=null && null != _shoppingCart.CartItems && _shoppingCart.CartItems.Count > 0)
                    {
                        string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
                        string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
                        var catItems = _shoppingCart.ShoppingCartItems.Select(s => s.CatalogItem).ToList();
                        if (HLConfigManager.Configurations.DOConfiguration.TodayMagazineProdTypeRestricted && catItems != null &&
                            !catItems.Any(
                                c =>
                                c.ProductType == ServiceProvider.CatalogSvc.ProductType.Product &&
                                catItems.Any(i => i.SKU.Trim() == primarySku || i.SKU.Trim() == secondarySku)))
                        {
                            if (catItems != null && catItems.Any(c => c.SKU.Trim() == primarySku))
                                _shoppingCart.DeleteTodayMagazine(primarySku);
                            else
                                _shoppingCart.DeleteTodayMagazine(secondarySku);
                        }
                    }

                    if (_prodsDeleted.Any(a => a == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku))
                    {
                        HttpContext.Current.Session[
                            ShoppingCartProvider.getTodayMagazineDeletedSessionKey(DistributorID, Locale,
                                                                                   _shoppingCart.ShoppingCartID)] = true;
                    }

                    OrderTotals_V02 totals = _shoppingCart.Totals as OrderTotals_V02;
                        if (totals != null && (_shoppingCart.CartItems != null && (_shoppingCart.CartItems.Count == 0 && (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU && totals.Donation == 0.00m))))
                    {
                        doCartEmpty();
                    }

                    _shoppingCart.RuleResults.Clear();
                }
                }
                if (products.Count > 0)
                {
                    // _shoppingCart = ShoppingCartHelper.InsertShoppingCartItems(_shoppingCart, products);
                    ProductsBase.AddItemsToCart(products, AddingItemOption.ModifyQuantity);
                    if (_shoppingCart != null)
                    {
                    foreach (ShoppingCartRuleResult result in _shoppingCart.RuleResults)
                    {
                        if (result.Result == RulesResult.Failure || result.Result == RulesResult.Recalc)
                        {
                                if (null != _errors && result.Messages!=null && result.Messages.Count>0)
                                {
                                    if (!_errors.Contains(result.Messages[0]))
                            {
                                if (result.RuleName == "Back Order")
                                {
                                    _backorderMessages.Add(result.Messages[0]);
                                }
                                else if (result.RuleName == "PurchaseRestriction Rules")
                                {
                                    _errors.AddRange(result.Messages.ToList());
                                }
                                else
                                {
                                    _errors.Add(result.Messages[0]);
                                }
                            }
                        }
                    }
                        }

                    if (ShoppingCart.RuleResults != null && ShoppingCart.RuleResults.Any(rs => rs.Result == RulesResult.Feedback) && ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback).Messages != null &&
                        ShoppingCart.RuleResults.First(r => r.Result == RulesResult.Feedback).Messages.Any())
                    {
                        // Checking for specific message to display
                        var feedbackResult =
                            ShoppingCart.RuleResults.FirstOrDefault(
                                r =>
                                    r.Result == RulesResult.Feedback &&
                                    r.Messages.Any(m => m != null && m.StartsWith("HTML|")));
                        if (feedbackResult != null)
                        {
                            var fragmentName = feedbackResult.Messages[0].Split('|');
                            if (fragmentName.Count() == 2 && fragmentName[1].Length > 0)
                            {
                                (this.Master as OrderingMaster).DisplayHtml(fragmentName[1]);
                            }
                        }
                    }

                    if (_shoppingCart.RuleResults != null && _shoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                        {
                            exceedsPurchasingLimit = true;
                            _errors.Clear();
                            if (IsChina)
                            {
                                if (ShoppingCart.RuleResults.Any(rs => rs.Messages != null && rs.Messages.Count() > 0))
                                {
                                    var ruleResultMsgs = ShoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                                .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                                    if (ruleResultMsgs != null && ruleResultMsgs.Any())
                                    {
                                        _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                                    }
                                    ShoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                                }
                            }

                            else
                            {
                                PurchasingLimitProvider.SetPostErrorRemainingLimitsSummaryMessage(_shoppingCart);
                                //bug:2258888 :Splunk error: Recalculate error- ArgumentOutOfRangeException happen because the messages is not null but the count is 0 
                                var ruleResultMessages =
                                    _shoppingCart.RuleResults.Where(
                                        r => r.Messages != null && r.Messages.Count > 0);
                                if (ruleResultMessages != null)
                                {
                                    _errors.AddRange(_shoppingCart.RuleResults.Select(oi => oi.Messages[0]));
                                }
                            }
                        }
                    _shoppingCart.RuleResults.Clear();
                }
                }
                else
                {
                    if (FOPEnabled)
                    {
                        exceedsPurchasingLimit =
                            PurchaseRestrictionManager(DistributorID)
                                .PurchasingLimitsAreExceeded(DistributorID, ShoppingCart);
                        if (_shoppingCart != null && (exceedsPurchasingLimit && _shoppingCart.RuleResults != null && _shoppingCart.RuleResults.Any(oi => oi.Messages.Count > 0)))
                        {
                            _errors.Clear();
                            _errors.AddRange(_shoppingCart.RuleResults.Select(oi => oi.Messages[0]));
                            _shoppingCart.RuleResults.Clear();
                        }
                    }
                }

                setupShoppingCartDataSource(Level);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("Error in DoCalculation:" +
                                   string.Format(
                                       "DistributorId:{0},Locale {1}, Stack Trace:{2}, Message: {3}, rule result:{4},OrderNumber: {5},shopping cart Version: {6} ",
                                       DistributorID, Locale, ex.StackTrace, ex.Message,
                                       _shoppingCart != null ? GetRuleResult(_shoppingCart) : "Empty Shhopping Cart",
                                       _shoppingCart != null ? _shoppingCart.OrderNumber : "unable get Order Number Empty Shhopping Cart",
                                       _shoppingCart != null ? _shoppingCart.Version : "Unable to Get Version Empty Shhopping Cart"));
            }
        }

        private string GetRuleResult(MyHLShoppingCart shoppingCart)
        {
            if (shoppingCart.RuleResults == null) return "Empty Rule Result";
            if (shoppingCart.RuleResults.Count <= 0) return "Empty Rule Result";
            StringBuilder sb = new StringBuilder();
            foreach (var rule in shoppingCart.RuleResults)
            {
                sb.Append(rule.RuleName + " Rule Result" +
                          rule.Result + " ");
            }
            return sb.ToString();
        }

        /// <summary>
        ///     The check max quantity.
        /// </summary>
        /// <param name="itemList">
        ///     The item list.
        /// </param>
        /// <param name="sku">
        ///     The sku.
        /// </param>
        /// <param name="maxQuantity">
        ///     The max quantity.
        /// </param>
        /// <param name="quantity">
        ///     The quantity.
        /// </param>
        /// <returns>
        ///     The check max quantity.
        /// </returns>
        private bool checkMaxQuantity(List<DistributorShoppingCartItem> itemList,
                                      string sku,
                                      int maxQuantity,
                                      int quantity)
        {
            return quantity <= maxQuantity;
        }

        /// <summary>
        ///     The same quantity.
        /// </summary>
        /// <param name="itemList">
        ///     The item list.
        /// </param>
        /// <param name="sku">
        ///     The sku.
        /// </param>
        /// <param name="quantity">
        ///     The quantity.
        /// </param>
        /// <returns>
        ///     The same quantity.
        /// </returns>
        private bool sameQuantity(List<DistributorShoppingCartItem> itemList, string sku, int quantity)
        {
            if (itemList != null && itemList.Exists(i => i.SKU == sku))
            {
                return itemList.Where(l => l.SKU == sku).First().Quantity == quantity;
            }

            return false;
        }

        /// <summary>
        ///     The get inv error.
        /// </summary>
        /// <param name="oldQty">
        ///     The old qty.
        /// </param>
        /// <param name="newQty">
        ///     The new qty.
        /// </param>
        /// <param name="sku">
        ///     The sku.
        /// </param>
        /// <returns>
        ///     The get inv error.
        /// </returns>
        public static string GetInvError(int oldQty, int newQty, string sku)
        {
            if (oldQty != 0 && newQty == 0)
            {
                return string.Format(MyHL_ErrorMessage.OutOfInventory, sku);
            }
            else if (oldQty - newQty > 0)
            {
                return string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "LessInventory"), sku, newQty);
            }

            return string.Empty;
        }

        public void ShoppingCartItemsDataBind()
        {
            setupShoppingCartDataSource(Level);
        }

        /// <summary>
        ///     The add items.
        /// </summary>
        /// <param name="dlg">
        ///     The dlg.
        /// </param>
        private void AddItems(AddItemsDelegate dlg)
        {
            _prodsDeleted.Clear();
            List<ListView> lstViews = new List<ListView>();
            lstViews.Add(uxProducts);
            lstViews.Add(uxPromo);

            if (_IsResponsiveControl)
            {
                #region Responsive
                lstViews.Add(uxProductsResponsive);
                lstViews.Add(uxPromoResponsive);
                foreach (var listView in lstViews)
                {
                    foreach (ListViewDataItem s in listView.Items)
                    {
                        ListViewItem item = s;
                        if (item.ItemType == ListViewItemType.DataItem)
                        {
                            bool bDelete = false;
                            var uxDelete = listView.ID.Equals("uxProductsResponsive") ? item.FindControl("uxDelete") as CheckBox : item.FindControl("uxPromoDelete") as CheckBox;
                            if (uxDelete != null && uxDelete.Checked)
                            {
                                bDelete = true;
                            }

                            var uxQuantity = listView.ID.Equals("uxProductsResponsive") ? item.FindControl("uxQuantity") as TextBox : item.FindControl("uxPromoQuantity") as TextBox;
                            int quantity = 0;
                            if (uxQuantity != null && uxQuantity.Text != string.Empty)
                            {
                                int.TryParse(uxQuantity.Text, out quantity);
                                if (bDelete)
                                {
                                    quantity = 0;
                                }

                                string productID = string.Empty;
                                var hf = listView.ID.Equals("uxProductsResponsive") ? item.FindControl("uxProductoID") as HiddenField : item.FindControl("uxPromoID") as HiddenField;
                                if (hf != null)
                                {
                                    productID = hf.Value;
                                }

                                if (ShoppingCart.CustomerOrderDetail != null)
                                {
                                    var lbSKU = listView.ID.Equals("uxProductsResponsive") ? item.FindControl("idSKU") as Label : item.FindControl("idPromoSKU") as Label;
                                    foreach (DistributorShoppingCartItem cartItem in ShoppingCart.ShoppingCartItems)
                                    {
                                        if (cartItem.SKU.Trim().ToUpper() == lbSKU.Text.Trim().ToUpper())
                                        {
                                            if (quantity < cartItem.MinQuantity)
                                            {
                                                quantity = cartItem.Quantity;
                                                uxQuantity.Text = cartItem.Quantity.ToString();
                                                customerOrderMinQuantityError = true;
                                                _errors.Add(
                                                    string.Format(
                                                        GetLocalResourceObject("CustomerOrderMinQuantitySKUDisplay").ToString(),
                                                        lbSKU.Text, cartItem.MinQuantity));
                                            }
                                        }
                                    }
                                }

                                if (quantity > 0 && !string.IsNullOrEmpty(productID))
                                {
                                    // check max quantity
                                    // it does not excess max, ok
                                    int maxQuantity = ProductsBase.GetMaxQuantity(productID);

                                    if (checkMaxQuantity(_shoppingCart.ShoppingCartItems, productID, maxQuantity, quantity))
                                    {
                                        // if quantity the same, don't add
                                        if (!sameQuantity(_shoppingCart.ShoppingCartItems, productID, quantity))
                                        {
                                            SKU_V01 skuV01;
                                            if (AllSKUS.TryGetValue(productID, out skuV01))
                                            {
                                                int backorderCoverage = ProductsBase.CheckBackorderCoverage(quantity, skuV01, 
                                                                                                            _friendlyMessages);
                                                // not backordered, check inventory
                                                if (backorderCoverage == 0)
                                                {
                                                    int availQty = ProductsBase.CheckInventory(quantity, skuV01, _errors);
                                                    if (availQty != quantity && availQty != 0)
                                                    {
                                                        uxQuantity.Text = availQty.ToString();
                                                        recalculateForQuantityChange();
                                                    }
                                                    dlg(productID, quantity, false);
                                                }
                                                else
                                                {
                                                    dlg(productID, quantity, true);
                                                }

                                                if (APFDueProvider.IsAPFSku(productID))
                                                {
                                                    _HasErrors = true;
                                                    _errors.Add(
                                                        HttpContext.GetGlobalResourceObject(
                                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                            "CannotAddAPFSku") as string);
                                                    uxQuantity.Text =
                                                        _shoppingCart.ShoppingCartItems.Where(p => p.SKU == productID)
                                                                     .First()
                                                                     .Quantity.
                                                                      ToString();
                                                }
                                            }
                                            else if (APFDueProvider.IsAPFSku(productID))
                                            {
                                                dlg(productID, quantity, true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _HasErrors = true;
                                        _errors.Add(string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantity"), productID, maxQuantity));


                                        SKU_V01 sku;
                                        if (AllSKUS.TryGetValue(productID, out sku))
                                        {
                                            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayETOMaxQuantityMessage && sku.CatalogItem.IsEventTicket)
                                            {
                                                string error = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantityETO"), maxQuantity);
                                                if (!_errors.Contains(error))
                                                {
                                                    _errors.Add(error);
                                                }
                                            }
                                        }

                                        // should always be in the shopping cart
                                        uxQuantity.Text =
                                            _shoppingCart.ShoppingCartItems.Where(p => p.SKU == productID).First().Quantity.
                                                          ToString();
                                    }
                                }
                                else
                                {
                                    // if 0 quantity, delete the item
                                    _prodsDeleted.Add(productID);
                                    if (HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku.Equals(productID)
                                        || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(sk => sk.Equals(productID)))
                                    {
                                        // DeclineCasa = true;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region Regular site
                foreach (var listView in lstViews)
                {
                    foreach (ListViewDataItem s in listView.Items)
                    {
                        ListViewItem item = s;
                        if (item.ItemType == ListViewItemType.DataItem)
                        {
                            bool bDelete = false;
                            var uxDelete = listView.ID.Equals("uxProducts") ? item.FindControl("uxDelete") as CheckBox : item.FindControl("uxPromoDelete") as CheckBox;
                            if (uxDelete != null && uxDelete.Checked)
                            {
                                bDelete = true;
                            }

                            var uxQuantity = listView.ID.Equals("uxProducts") ? item.FindControl("uxQuantity") as TextBox : item.FindControl("uxPromoQuantity") as TextBox;
                            int quantity = 0;
                            if (uxQuantity != null && uxQuantity.Text != string.Empty)
                            {
                                int.TryParse(uxQuantity.Text, out quantity);
                                if (bDelete)
                                {
                                    quantity = 0;
                                }

                                string productID = string.Empty;
                                var hf = listView.ID.Equals("uxProducts") ? item.FindControl("uxProductoID") as HiddenField : item.FindControl("uxPromoID") as HiddenField;
                                if (hf != null)
                                {
                                    productID = hf.Value;
                                }

                                if (ShoppingCart.CustomerOrderDetail != null)
                                {
                                    var lbSKU = listView.ID.Equals("uxProducts") ? item.FindControl("idSKU") as Label : item.FindControl("idPromoSKU") as Label;
                                    foreach (DistributorShoppingCartItem cartItem in ShoppingCart.ShoppingCartItems)
                                    {
                                        if (cartItem.SKU.Trim().ToUpper() == lbSKU.Text.Trim().ToUpper())
                                        {
                                            if (quantity < cartItem.MinQuantity)
                                            {
                                                quantity = cartItem.Quantity;
                                                uxQuantity.Text = cartItem.Quantity.ToString();
                                                customerOrderMinQuantityError = true;
                                                _errors.Add(
                                                    string.Format(
                                                        GetLocalResourceObject("CustomerOrderMinQuantitySKUDisplay").ToString(),
                                                        lbSKU.Text, cartItem.MinQuantity));
                                            }
                                        }
                                    }
                                }

                                if (quantity > 0 && !string.IsNullOrEmpty(productID))
                                {
                                    // check max quantity
                                    // it does not excess max, ok
                                    int maxQuantity = ProductsBase.GetMaxQuantity(productID);

                                    if (checkMaxQuantity(_shoppingCart.ShoppingCartItems, productID, maxQuantity, quantity))
                                    {
                                        // if quantity the same, don't add
                                        if (!sameQuantity(_shoppingCart.ShoppingCartItems, productID, quantity))
                                        {
                                            SKU_V01 skuV01;
                                            if (AllSKUS.TryGetValue(productID, out skuV01))
                                            {
                                                int backorderCoverage = ProductsBase.CheckBackorderCoverage(quantity, skuV01,
                                                                                                            _friendlyMessages);
                                                // not backordered, check inventory
                                                if (backorderCoverage == 0)
                                                {
                                                    int availQty = ProductsBase.CheckInventory(quantity, skuV01, _errors);
                                                    if (availQty != quantity && availQty != 0)
                                                    {
                                                        uxQuantity.Text = availQty.ToString();
                                                        recalculateForQuantityChange();
                                                    }
                                                    dlg(productID, quantity, false);
                                                }
                                                else
                                                {
                                                    dlg(productID, quantity, true);
                                                }

                                                if (APFDueProvider.IsAPFSku(productID))
                                                {
                                                    _HasErrors = true;
                                                    _errors.Add(
                                                        HttpContext.GetGlobalResourceObject(
                                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                            "CanOnlyPrepayOneAPF") as string);
                                                    uxQuantity.Text =
                                                        _shoppingCart.ShoppingCartItems.Where(p => p.SKU == productID)
                                                                     .First()
                                                                     .Quantity.
                                                                      ToString();
                                                }
                                            }
                                            else if (APFDueProvider.IsAPFSku(productID))
                                            {
                                                dlg(productID, quantity, true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _HasErrors = true;
                                        _errors.Add(string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantity"), productID, maxQuantity));

                                        SKU_V01 sku;
                                        if (AllSKUS.TryGetValue(productID, out sku))
                                        {
                                            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayETOMaxQuantityMessage && sku.CatalogItem.IsEventTicket)
                                            {
                                                string error = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantityETO"), maxQuantity);
                                                if (!_errors.Contains(error))
                                                {
                                                    _errors.Add(error);
                                                }
                                            }
                                        }

                                        // should always be in the shopping cart
                                        uxQuantity.Text =
                                            _shoppingCart.ShoppingCartItems.Where(p => p.SKU == productID).First().Quantity.
                                                          ToString();
                                    }
                                }
                                else
                                {
                                    // if 0 quantity, delete the item
                                    _prodsDeleted.Add(productID);
                                    if (HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku.Equals(productID)
                                        || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(sk => sk.Equals(productID)))
                                    {
                                        // DeclineCasa = true;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        ///     The recalculate for quantity change.
        /// </summary>
        private void recalculateForQuantityChange()
        {
            List<string> dummy = null;
            if ((ProductsBase.CanAddProduct(DistributorID, ref dummy)))
            {
                var products = new List<ShoppingCartItem_V01>();
                AddItems((string sku, int quantity, bool bPartialBackordered) =>
                    {
                        products.Add(new ShoppingCartItem_V01
                            {
                                SKU = sku,
                                Quantity = quantity,
                                Updated = DateTime.Now,
                                PartialBackordered = bPartialBackordered,
                            });
                    });
                doCalculation(products);
            }

            blErrors.DataSource = _errors;
            blErrors.DataBind();
        }

        /// <summary>
        ///     The recalculate.
        /// </summary>
        private void recalculate()
        {
            try
            {
                _errors.Clear();
                _friendlyMessages.Clear();
                _backorderMessages.Clear();
                _HasErrors = false;

                List<string> dummy = null;

                if ((ProductsBase.CanAddProduct(DistributorID, ref dummy)))
                {
                    var products = new List<ShoppingCartItem_V01>();
                    AddItems((string sku, int quantity, bool bPartialBackordered) =>
                        {
                            products.Add(new ShoppingCartItem_V01
                                {
                                    SKU = sku,
                                    Quantity = quantity,
                                    Updated = DateTime.Now,
                                    PartialBackordered = bPartialBackordered,
                                });
                        });
                    if (_errors.Count > 0)
                    {
                        // this is to display friendly msg, but don't prevent them from checking out
                        if (customerOrderMinQuantityError)
                        {
                            _errors.Insert(0, GetLocalResourceObject("CustomerOrderMinQuantityError").ToString());
                        }
                        blErrors.DataSource = _errors;
                        blErrors.DataBind();
                        return;
                    }

                    doCalculation(products);
                    NotifyTodaysMagazineRecalculate(this, null);

                    if (_friendlyMessages.Count > 0)
                    {
                        ProductsBase.ShowBackorderMessage(_friendlyMessages);
                    }
                    if (_backorderMessages.Count > 0)
                    {
                        blErrors.DataSource = _backorderMessages;
                        blErrors.DataBind();
                        return;
                    }
                }

                //MyHerbalife3.Ordering.Web.Ordering.Controls.ChinaAPF a = (Page as ProductsBase).FindControl()

                // clear errors, if there is none
                blErrors.DataSource = _errors;
                blErrors.DataBind();                         
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("recalculate error : " + ex);
            }
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
        ///     The get avail.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <returns>
        /// </returns>
        protected ProductAvailabilityType getAvail(DistributorShoppingCartItem item)
        {
            if (APFDueProvider.IsAPFSku(item.SKU))
            {
                return ProductAvailabilityType.Available;
            }

            string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
            string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
            bool isChildSKU = this.ChildSKUs.Contains(item);

            if (!string.IsNullOrEmpty(item.SKU) && (item.SKU.Trim() == primarySku || item.SKU.Trim() == secondarySku || isChildSKU))
            {
                CatalogItem_V01 catItem = CatalogProvider.GetCatalogItem(item.SKU, this.ProductsBase.CountryCode);
                if (catItem != null)
                {
                    WarehouseInventory warehouseInventory;
                    if (catItem.InventoryList.TryGetValue(ProductsBase.CurrentWarehouse, out warehouseInventory))
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && !warehouseInventory01.IsBlocked)
                        {
                            var inventoryQty = ShoppingCartProvider.CheckInventory(catItem, item.Quantity,
                                                                                    this.ProductsBase.CurrentWarehouse);
                            return inventoryQty > 0
                                        ? ProductAvailabilityType.Available
                                        : ProductAvailabilityType.Unavailable;
                        }
                    }
                }
                return ProductAvailabilityType.Unavailable;
            }

            if (item.SKU.Trim() == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku 
                || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(item.SKU.Trim())))
            {
                return ProductAvailabilityType.Available;
            }

            if (_AllSKUS != null)
            {
                SKU_V01 sku;
                if (_AllSKUS.TryGetValue(item.SKU, out sku))
                {
                    //if (item.PartialBackordered == true)
                    //    return ProductAvailabilityType.Available;
                    return CatalogProvider.GetProductAvailability(sku,
                                                                  ShoppingCart.DeliveryInfo != null
                                                                      ? ShoppingCart.DeliveryInfo.WarehouseCode
                                                                      : HLConfigManager.Configurations
                                                                                       .ShoppingCartConfiguration
                                                                                       .DefaultWarehouse,
                                                                  ShoppingCart.DeliveryInfo != null
                                                                       ? (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), ShoppingCart.DeliveryInfo.Option.ToString())
                                                                       : ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown,
                                                                  ShoppingCart.DeliveryInfo != null
                                                                      ? ShoppingCart.DeliveryInfo.FreightCode
                                                                      : null);
                }
            }
            return ProductAvailabilityType.Unavailable;
        }

        /// <summary>
        ///     The get link.
        /// </summary>
        /// <param name="item">
        ///     The item.
        /// </param>
        /// <returns>
        ///     The get link.
        /// </returns>
        protected string GetLink(DistributorShoppingCartItem item)
        {
            string link = string.Empty;

            if (!APFDueProvider.IsAPFSku(item.SKU) && null != item.ProdInfo && null != item.ParentCat)
            {
                link = string.Format("~/Ordering/ProductDetail.aspx?ProdInfoID={0}&CategoryID={1}", item.ProdInfo.ID,
                                     item.ParentCat.ID);
            }

            return link;
        }

        protected void ProductDetailClicked(object sender, EventArgs e)
        {
            int CategoryID = 0, ProductID = 0;
            var lb = sender as LinkButton;
            if (lb != null)
            {
                string commandArgument = lb.CommandArgument.Trim();
                if (!String.IsNullOrEmpty(commandArgument))
                {
                    var commandParts = commandArgument.Split(' ');
                    ProductID = int.Parse(commandParts[0]);
                    CategoryID = int.Parse(commandParts[1]);
                    ProductDetailBeingLaunched(this, new ProductDetailEventArgs(CategoryID, ProductID));
                }
              
            }

            //CntrlProductDetail.LoadProduct(CategoryID, ProductID);
        }

        /// <summary>
        ///     The do cart empty.
        /// </summary>
        private void doCartEmpty()
        {
            ProductsBase.ClearCart();

            if (null != _shoppingCart && _shoppingCart.ShoppingCartItems != null &&
                _shoppingCart.ShoppingCartItems.Count > 0)
            {
                cartEmpty.Visible = false;
            }
            else
            {
                cartEmpty.Visible = true;
            }

            //uxCancelOrder.Visible = false;
            //checkOutButton.Visible = false;
            pProdAvail.Visible = false;
        }

        /// <summary>
        ///     The create catalog list.
        /// </summary>
        /// <param name="shoppingCartItemList">
        ///     The shopping cart item list.
        /// </param>
        /// <returns>
        /// </returns>
        private CatalogItemList createCatalogList(List<DistributorShoppingCartItem> shoppingCartItemList)
        {
            return CatalogProvider.GetCatalogItems((from s in shoppingCartItemList select s.SKU).ToList(), CountryCode);
        }

        /// <summary>
        ///     The setup shopping cart data source.
        /// </summary>
        /// <param name="level">
        ///     The level.
        /// </param>
        private void setupShoppingCartDataSource(string level)
        {
            try
            {
                if (_shoppingCart == null)
                {
                    return;
                }

                if (_shoppingCart.Totals == null)
                {
                    _shoppingCart.Totals = new OrderTotals_V01();
                    (_shoppingCart.Totals as OrderTotals_V01).ChargeList = new ChargeList();
                }
                var skuList = new List<SKU_V01>();
                skuList.AddRange(from s in _shoppingCart.CartItems
                                 from k in AllSKUS
                                 where s.SKU == s.SKU
                                 select k.Value);
                HLRulesManager.Manager.ProcessCatalogItemsForInventory(Locale, _shoppingCart, skuList);
                //CatalogProvider.GetProductAvailability(skuList, this.Locale, this.DistributorID, ProductsBase.CurrentWarehouse);

                //Check whether the Shopping cart Item is containing TM SKU
                if (HLConfigManager.Configurations.DOConfiguration.PromoSkuGrid)
                {
                    if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
                    {
                        uxProducts.DataSource =
                            _shoppingCart.ShoppingCartItems.Where(
                                i => i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                                     i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku && !i.IsPromo);
                        Totals = _shoppingCart.Totals as OrderTotals_V01;
                        uxProducts.DataBind();
                        getLineItemWithAllCharges();
                    }
                    else
                    {
                        if (_IsResponsiveControl)
                        {
                            Totals = _shoppingCart.Totals as OrderTotals_V01;
                            uxProductsResponsive.DataSource = _shoppingCart.ShoppingCartItems.Where(i => !i.IsPromo);
                            uxProductsResponsive.DataBind();
                            uxPromoResponsive.DataSource =
                            _shoppingCart.ShoppingCartItems.Where(
                                i => i.IsPromo);
                            uxPromoResponsive.DataBind();
                        }
                        else
                        {
                            #region removing selectable promosku for china while SR placing order for PC Oct 2016 to Decm 2016

                            if (IsChina && SessionInfo.IsReplacedPcOrder)
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
                            #endregion
                            if (IsChina && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                            {
                                string[] words = null;
                                string eligibleSKu = string.Empty;
                                var rsp =
                                    MyHerbalife3.Ordering.Providers.China.OrderProvider.GetEventEligibility(
                                        ShoppingCart.DistributorID);
                                if (rsp.IsEligible)
                                {
                                    words = rsp.Remark.Split('|');
                                    eligibleSKu = words[words.Length - 2];
                                }
                                SKU_V01 etoSku;
                                AllSKUS.TryGetValue(eligibleSKu, out etoSku);
                                if (etoSku != null)
                                {
                                    var skuETO = new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased
                                    {
                                            SKU = etoSku.CatalogItem.StockingSKU.Trim(),
                                            Category = "ETO",
                                        };
                                    var etoskuList = new List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased>();
                                    etoskuList.Add(skuETO);
                                    var totalETOCount =
                                        Providers.China.OrderProvider.GetSkuOrderedAndPurchased(
                                            ShoppingCart.CountryCode, null, null,
                                            null, etoskuList);
                                    var skulimit = Settings.GetRequiredAppSetting("ETOskulimit",string.Empty).Split('|');

                                    if (skulimit.Length>1 && eligibleSKu.Trim().Equals(skulimit[0]) && totalETOCount != null &&
                                        totalETOCount.Any(
                                            x =>
                                            x.QuantityPurchased >= Convert.ToInt16(skulimit[1]) &&
                                            x.SKU == etoSku.CatalogItem.StockingSKU.Trim()))
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
                            }
                            uxProducts.DataSource = _shoppingCart.ShoppingCartItems.Where(i => !i.IsPromo);
                            Totals = _shoppingCart.Totals as OrderTotals_V01;
                            uxProducts.DataBind();
                            uxPromo.DataSource =
                            _shoppingCart.ShoppingCartItems.Where(
                                i => i.IsPromo);

                            uxPromo.DataBind();
                        }
                        Totals = _shoppingCart.Totals as OrderTotals_V01;
                        getLineItemWithAllCharges();
                        getLineItemWithAllChargesforpromo();
                        divPromo.Visible = _shoppingCart.ShoppingCartItems.Any(i => i.IsPromo);

                    }
                }
                else
                {
                    //responsive control, not for china
                    if (_IsResponsiveControl)
                    {
                        Totals = _shoppingCart.Totals as OrderTotals_V01;
                        uxProductsResponsive.DataSource = _shoppingCart.ShoppingCartItems.Where(i => !i.IsPromo);
                        uxProductsResponsive.DataBind();
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
                    {
                        uxProducts.DataSource =
                            _shoppingCart.ShoppingCartItems.Where(
                                i => i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                                     i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku);
                        Totals = _shoppingCart.Totals as OrderTotals_V01;
                        uxProducts.DataBind();
                        getLineItemWithAllCharges();
                    }
                    else
                    {
                        uxProducts.DataSource = _shoppingCart.ShoppingCartItems;
                        Totals = _shoppingCart.Totals as OrderTotals_V01;
                        uxProducts.DataBind();
                        getLineItemWithAllCharges();
                    }

                }

                // loads the list view that displays the child skus (normal or responsive)            
                CalculateChildSKUs();
                var listViewControl = _IsResponsiveControl ? uxChildSKUResponsive : uxChildSKU;
                listViewControl.DataSource = this.ChildSKUs;
                listViewControl.DataBind();
                //getLineItemWithAllChargesChildSKUs();

                if (ShoppingCart.CustomerOrderDetail != null && CustomerOrder == null)
                {
                    CustomerOrder = ObjectMappingHelper.Instance.GetToOrder(CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID));
                }

                var omnitureTotals = _shoppingCart.Totals as OrderTotals_V01;
                OmnitureHelper.RegisterOmnitureCartScript(Parent.Page, omnitureTotals, _shoppingCart.ShoppingCartItems, OmnitureState);

                //if (APFDueProvider.IsAPFSkuPresent(_shoppingCart.ShoppingCartItems))
                {
                    delDisableControls doWork = delegate(ListViewItem item, bool isSP)
                    {
                        var uxQty = item.ClientID.Contains("uxProducts") ? item.FindControl("uxQuantity") as TextBox : item.FindControl("uxPromoQuantity") as TextBox;
                        var uxDelete = item.ClientID.Contains("uxProducts") ? item.FindControl("uxDelete") as CheckBox : item.FindControl("uxPromoDelete") as CheckBox;
                        var uxProductoId = item.ClientID.Contains("uxProducts") ? item.FindControl("uxProductoID") as HiddenField : item.FindControl("uxPromoID") as HiddenField;
                        var uxIsPromo = item.FindControl("uxIsPromo") as HiddenField;
                        if (uxDelete != null && uxProductoId != null)
                        {
                            bool isApFsku = APFDueProvider.IsAPFSku(uxProductoId.Value);
                            if (isApFsku)
                            {
                                uxDelete.Visible = shouldDeleteVisible(level, isApFsku, uxProductoId.Value);
                                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                                    if (uxQty != null) uxQty.ReadOnly = true;
                            }

                            if (ShoppingCart.CustomerOrderDetail != null && !isApFsku)
                            {
                                uxDelete.Visible = CanDeleteRow(uxProductoId.Value);
                            }

                            if (uxIsPromo != null && uxIsPromo.Value == "True")
                            {
                                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                                {
                                    if (uxQty != null) uxQty.ReadOnly = true;

                                    var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(DistributorID, "CN");
                                    if (!ChinaPromotionProvider.IsSKUDeletable(distributorOrderingProfile.CNCustomorProfileID.ToString(), uxProductoId.Value))
                                    {
                                        uxDelete.Enabled = false;
                                    }
                                }
                            }

                            if (!uxDelete.Visible)
                            {
                                if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
                                {
                                    if (uxQty != null) uxQty.ReadOnly = !uxDelete.Visible;
                                }
                                else
                                {
                                    if (!isApFsku)
                                    {
                                        if (uxQty != null) uxQty.ReadOnly = !uxDelete.Visible;
                                    }
                                }
                            }

                            if (APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale) &&
                                APFDueProvider.containsOnlyAPFSku(_shoppingCart.ShoppingCartItems))
                            {
                                uxCancelOrder.Enabled = !APFDueProvider.CantDeleteAllAPFs(DistributorID, level);
                                uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                                mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                            }
                        }
                    };
                    Array.ForEach(uxProducts.Items.ToArray(), a => doWork(a, true));

                    Array.ForEach(uxPromo.Items.ToArray(), a => doWork(a, true));
                }

                if (_shoppingCart.ShoppingCartItems.Count > 0)
                {
                    uxCancelOrder.Enabled = !(APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) && HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed);
                    uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                    mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                    uxRecalculate.Enabled = true;
                    uxRecalculate.Disabled = !uxRecalculate.Enabled;
                    checkOutButton.Enabled = true;
                    checkOutButton.Disabled = !checkOutButton.Enabled;
                    checkOutButton.OnClientClick = "$(this).removeClass('disabled');";
                    lblHeaderSummary.Visible = true;
                    ProductAvailability1.Visible = true;
                    ProductAvailability1.ShowLabel = true;
                    cartEmpty.Visible = false;


                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && _shoppingCart.ShoppingCartItems.Count == 1)
                    {
                        if (_shoppingCart.ShoppingCartItems.Any(x => x.SKU == APFDueProvider.GetAPFSku()))
                            _saveCardCommand.Visible = false;
                    }
                }
                else
                {
                    if (_shoppingCart.SKUsRemoved != null && _shoppingCart.SKUsRemoved.Count > 0)
                    {
                        _errors.AddRange(_shoppingCart.SKUsRemoved);
                    }
                    uxCancelOrder.Enabled = false;
                    uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                    uxRecalculate.Enabled = false;
                    uxRecalculate.Disabled = !uxRecalculate.Enabled;
                    checkOutButton.Enabled = false;
                    checkOutButton.Disabled = !checkOutButton.Enabled;
                    lblHeaderSummary.Visible = false;
                    ProductAvailability1.Visible = false;
                    var localResourceObject = GetLocalResourceObject("ContinueShopping");
                    if (localResourceObject != null)
                        ContinueShopping.Text = localResourceObject.ToString();
                    divEventTicketMessage.Visible = false;
                    mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                    //validates donations in China since they're not linked to any SKU
                    //when there are donations, checkout and cancel buttons should be enabled
                    OrderTotals_V02 totals = _shoppingCart.Totals as OrderTotals_V02;
                    if (totals != null && (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU &&
                                           totals.Donation > 0.00m))
                    {
                        uxCancelOrder.Enabled = true;
                        uxCancelOrder.Disabled = !uxCancelOrder.Enabled;
                        mdlConfirmDelete.Enabled = uxCancelOrder.Enabled;
                        checkOutButton.Enabled = true;
                        checkOutButton.Disabled = !checkOutButton.Enabled;
                        checkOutButton.OnClientClick = "$(this).removeClass('disabled');";
                    }
                }

                //blErrors.DataSource = _errors;
                //blErrors.DataBind();
                if (_HasErrors)
                {
                    checkOutButton.Enabled = false;
                    checkOutButton.Disabled = !checkOutButton.Enabled;
                }
                if (IsChina && (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC || (SessionInfo != null && SessionInfo.ReplacedPcDistributorOrderingProfile != null && SessionInfo.ReplacedPcDistributorOrderingProfile.IsPC)))
                {
                    if (_shoppingCart.RuleResults != null && _shoppingCart.RuleResults.Any(rs => rs.RuleName == "PurchasingLimits Rules" && rs.Result == RulesResult.Failure))
                    {
                        var ruleResultMsgs = _shoppingCart.RuleResults.Where(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules")
                                    .Select(r => r.Messages.Where(str => string.IsNullOrWhiteSpace(str) == false));
                       
                        _errors.AddRange(ruleResultMsgs.First().Distinct().ToList());
                        _shoppingCart.RuleResults.RemoveAll(x => x.RuleName == "PurchasingLimits Rules");
                    }
                    
                }
                if (_errors.Count > 0)
                {
                    for (int count = 0; count < _errors.Count; count++)
                    {
                        if (!blErrors.Items.Contains(new ListItem(_errors[count])))
                        blErrors.Items.Add(_errors[count]);
                    }
                }
                else
                {
                    blErrors.Items.Clear();
                }
                if (_backorderMessages.Count > 0)
                {
                    for (int count = 0; count < _backorderMessages.Count; count++)
                    {
                        blErrors.Items.Add(_backorderMessages[count]);
                    }
                }
                foreach (DistributorShoppingCartItem item in _shoppingCart.ShoppingCartItems)
                {
                    if (!string.IsNullOrEmpty(item.ErrorMessage) && blErrors.Items.FindByText(item.ErrorMessage) == null)
                        blErrors.Items.Add(item.ErrorMessage);
                }

                SetSplitOrderMessage();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Error in setupShoppingCartDataSource: Message :{0}, StackTrace:{1}, DistributorId:{2}, Localce{3},ShoppingCart:{4}",
                        ex.Message, ex.StackTrace, DistributorID, Locale,
                        (ShoppingCart == null) ? "null" : ShoppingCart.CartName + " " + ShoppingCart.ShoppingCartID));
            }
        }

        private bool CanEditQuantity()
        {
            return APFDueProvider.CanRemoveAPF(DistributorID, CultureInfo.CurrentCulture.Name, null);
        }

        /// <summary>
        ///     The should delete visible.
        /// </summary>
        /// <param name="level">
        ///     The level.
        /// </param>
        /// <param name="isAPF">
        ///     The is apf.
        /// </param>
        /// <param name="sku">
        ///     The sku.
        /// </param>
        /// <returns>
        ///     The should delete visible.
        /// </returns>
        private bool shouldDeleteVisible(string level, bool isAPF, string sku)
        {
            bool canRemove = true;
            if (isAPF)
            {
                canRemove = false;
                if (APFDueProvider.CanRemoveAPF(DistributorID, CultureInfo.CurrentCulture.Name, level))
                {
                    canRemove = !APFDueProvider.CantDeleteAllAPFs(DistributorID, level);
                }
                if (DistributorOrderingProfile != null && HLConfigManager.Configurations.DOConfiguration.IsChina && (DistributorOrderingProfile.CNAPFStatus == 0 || DistributorOrderingProfile.CNAPFStatus == 1))
                {
                    canRemove = true;
                }
            }

            return canRemove;
        }

        public List<DistributorShoppingCartItem> GetListToRemove()
        {
            var unavailableItems = new List<DistributorShoppingCartItem>();
            var backOrderedItems = new List<DistributorShoppingCartItem>();
            if (_shoppingCart != null && _shoppingCart.ShoppingCartItems != null &&
                _shoppingCart.ShoppingCartItems.Count > 0 && !exceedsPurchasingLimit)
            {
                var AllSKUS = ProductsBase.AllSKUS;

                //Browse through shopping cart and check availability
                foreach (DistributorShoppingCartItem item in _shoppingCart.ShoppingCartItems)
                {
                    var prodAvail = getAvail(item);
                    if (prodAvail == ProductAvailabilityType.Unavailable)
                    {
                        unavailableItems.Add(item);
                    }

                    if (prodAvail == ProductAvailabilityType.AllowBackOrder)
                    {
                        backOrderedItems.Add(item);
                    }

                    if (prodAvail == ProductAvailabilityType.Available && !APFDueProvider.IsAPFSku(item.SKU))
                    {
                        SKU_V01 sku;
                        if (AllSKUS.TryGetValue(item.SKU, out sku))
                        {
                            int backorderCoverage = ProductsBase.CheckBackorderCoverage(item.Quantity, sku,
                                                                                        new List<string> {string.Empty});
                            if (backorderCoverage == 0)
                            {
                                // if it is not partial backorder, check inventory.
                                int availQuantity = ProductsBase.CheckInventory(item.Quantity, sku,
                                                                                new List<string> {string.Empty});
                                if (availQuantity != item.Quantity)
                                {
                                    unavailableItems.Add(item);
                                }
                            }
                        }
                    }
                }

                if (_shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                {
                    if (!HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder ||
                        _shoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        unavailableItems.AddRange(backOrderedItems);
                    }
                }
                else if (_shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                {
                    if (!HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickup)
                    {
                        unavailableItems.AddRange(backOrderedItems);
                    }
                    else
                    {
                        foreach (DistributorShoppingCartItem d in backOrderedItems)
                        {
                            if (_AllSKUS != null)
                            {
                                SKU_V01 sku;
                                if (_AllSKUS.TryGetValue(d.SKU, out sku))
                                {
                                    //Check when country is allowed to back order all P,L and A types
                                    if (!HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorderForPickupAllTypes)
                                    {
                                        if (sku.CatalogItem.ProductType != ServiceProvider.CatalogSvc.ProductType.Product)
                                        {
                                            unavailableItems.Add(d);
                                        }
                                    }
                                }
                                else
                                {
                                    unavailableItems.Add(d);
                                }
                            }
                        }
                    }
                }
            }

            return unavailableItems;
        }

        /// <summary>
        /// Check if usplit order to display message
        /// </summary>
        private void SetSplitOrderMessage()
        {
            ShoppingCartProvider.CheckSplitOrder(_shoppingCart);
            divSplitOrder.Visible = _shoppingCart.IsSplit;
            SplitOrderReader.ContentPath = ShoppingCart.CartItems.Count > 1 ? "splitMultipleOrder.html" : "splitOrder.html";
            SplitOrderReader.LoadContent();
        }

        /// <summary>
        /// Check if currency converter display message
        /// </summary>
        private void SetCurrencyConvectorMessage()
        {
            if (_shoppingCart != null && ShoppingCart.DeliveryInfo != null)
            {
                divCurrencyConvector.Visible = HLConfigManager.Configurations.DOConfiguration.IsEventInProgress 
                                                && (_shoppingCart.DeliveryInfo.WarehouseCode ==HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse 
                                                || _shoppingCart.DeliveryInfo.WarehouseCode == "M0");
            }
        }

        /// <summary>
        /// Check if currency converter display message
        /// </summary>
        private void SetApparelMessage()
        {
            divApparelMessage.Visible = (HLConfigManager.Configurations.DOConfiguration.IsEventInProgress
                                        && (_shoppingCart.DeliveryInfo.WarehouseCode == HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SpecialEventWareHouse
                                               || _shoppingCart.DeliveryInfo.WarehouseCode == "M0")
                                               && ShoppingCart.OrderCategory != OrderCategoryType.ETO);
        }
        private bool CanDeleteRow(string sku)
        {
            if (ShoppingCart.CustomerOrderDetail != null)
            {
                return CustomerOrder.OrderItems.All(i => i.SKU.ToUpper() != sku.ToUpper());
            }
            return true;
        }

        #endregion HelperMethods

        protected void ProcedToChcekOut_Click(object sender, EventArgs e)
        {
            AddressRestrictionConfirmPopupExtender.Hide();
            var redirectUrl = (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && _shoppingCart.OrderCategory == OrderCategoryType.HSO) ? "~/Ordering/Checkout.aspx?HAP=True" : "~/Ordering/Checkout.aspx";
            try
            {
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            finally
            {
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        protected void CancelToProced_Click(object sender, EventArgs e)
        {
            AddressRestrictionConfirmPopupExtender.Hide();
            return;
        }

        protected void btnOK_Click(object sender, EventArgs e)
        {
            AutoSelectShippingaddresPopupExtender.Hide();
            var redirectUrl = (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && _shoppingCart.OrderCategory == OrderCategoryType.HSO) ? "~/Ordering/Checkout.aspx?HAP=True" : "~/Ordering/Checkout.aspx";
            try
            {
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            finally
            {
                Response.Redirect(redirectUrl, false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            AutoSelectShippingaddresPopupExtender.Hide();
            return;
        }
    }
}
