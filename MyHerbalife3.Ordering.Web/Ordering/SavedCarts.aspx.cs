using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Web.Script.Services;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Telerik.Web.UI;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using System.Web;
    using MyHerbalife3.Shared.ViewModel.Models;

    /// <summary>
    ///     Used to get the saved carts and orders to copy for the grids.
    /// </summary>
    [ScriptService]
    public partial class SavedCarts : ProductsBase
    {
        /// <summary>
        ///     Gets or sets a value indicating whether current shopping cart is not empty.
        /// </summary>
        public static bool IsShoppingCartNotEmpty { get; set; }

        /// <summary>
        ///     Gets or sets static copy order index.
        /// </summary>
        public static int StaticCopyOrderIndex { get; set; }

        /// <summary>
        ///     Gets or sets static copy order max length.
        /// </summary>
        public static int StaticCopyOrderMaxLength { get; set; }

        /// <summary>
        ///     Gets or sets static distributor ID.
        /// </summary>
        public static string StaticDistributorID
        {
            
            get
            {
                var user = Membership.GetUser();
                if (user != null)
                {
                    return user.UserName;
                }
                return string.Empty;
            }
            set { }
        }


        public static SKU_V01ItemCollection SkuCollection
        {
            get
            {
                var shoppingcartinfo = SessionInfo.GetSessionInfo(StaticDistributorID, StaticLocale);
                if (shoppingcartinfo != null && shoppingcartinfo.ShoppingCart !=null && shoppingcartinfo.ShoppingCart.DeliveryInfo != null)
                {
                    var productInfo = CatalogProvider.GetProductInfoCatalog(StaticLocale, 
                                                                            shoppingcartinfo.ShoppingCart.DeliveryInfo.WarehouseCode);
                    if (productInfo != null)
                    {
                        return productInfo.AllSKUs;
                    }
                    return null;
                }
                return null;
            }
            set { }
        }
      
        /// <summary>
        ///     Gets or sets static locale.
        /// </summary>
        public static string StaticLocale
        {
            get
            {
                // return CultureInfo.CurrentUICulture.Name;
                return Thread.CurrentThread.CurrentCulture.ToString();
            }
            set { }
        }

        /// <summary>
        ///     Copy an order shopping cart.
        /// </summary>
        /// <param name="shoppingCartID">Shopping cart ID to copy.</param>
        /// <param name="ignoreCartNotEmpty">Ignore if current shopping cart is not empty.</param>
        /// <returns>My HL Copy Order Result.</returns>
        [WebMethod]
        [ScriptMethod]
        public static MyHLCopyOrderResult CopyOrder(string shoppingCartID, bool ignoreCartNotEmpty)
        {
            try
            {
                if (IsShoppingCartNotEmpty && !ignoreCartNotEmpty)
                {
                    return new MyHLCopyOrderResult(0, false, IsShoppingCartNotEmpty);
                }

                var shoppingCart = CopyOrderProvider.CopyShoppingCart(int.Parse(shoppingCartID), StaticDistributorID,
                                                                      StaticLocale, SkuCollection);
                return new MyHLCopyOrderResult(shoppingCart != null ? shoppingCart.ShoppingCartID : 0,
                                               shoppingCart != null, IsShoppingCartNotEmpty);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw ex;
            }
          
        }

       
      
        /// <summary>
        ///     Delete Cart method.
        /// </summary>
        /// <param name="cartID">Cart id string.</param>
        /// <returns>If cart has been deleted.</returns>
        [WebMethod]
        [ScriptMethod]
        public static bool DeleteCart(string cartID)
        {
            try
            {
                int cartId = 0;
                if (int.TryParse(cartID, out cartId))
                {
                    var cart = ShoppingCartProvider.GetShoppingCart(StaticDistributorID, StaticLocale, cartId);
                    if (cart != null)
                    {
                        if (ShoppingCartProvider.DeleteShoppingCart(cart, null))
                        {
                            ShoppingCartProvider.DeleteCartFromCache(StaticDistributorID, StaticLocale, cartId);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Get the saved carts according the grid parameters.
        /// </summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="maximumRows">Maximum of rows.</param>
        /// <param name="sortExpressions">Sort Expression.</param>
        /// <param name="filterExpressions">Filter Expression.</param>
        /// <param name="copyOrderMode">If copy order mode is activated.</param>
        /// <param name="copyOrderIndex">Copy order index.</param>
        /// <param name="copyOrderMaxLength">Copy order max length.</param>
        /// <returns>Saved carts according the grid parameters.</returns>
        [WebMethod]
        [ScriptMethod]
        public static MyHlResultViewResult GetData(
            int startIndex,
            int maximumRows,
            string sortExpressions,
            string filterExpressions,
            bool copyOrderMode,
            int copyOrderIndex,
            int copyOrderMaxLength)
        {
            try
            {
                bool indexChanged = StaticCopyOrderIndex != copyOrderIndex;

                StaticCopyOrderIndex = copyOrderIndex;
                StaticCopyOrderMaxLength = copyOrderMaxLength;

                // Getting saved carts or shopping carts history from the shopping cart provider.
                var myHlShoppingCarts = copyOrderMode
                                            ? ShoppingCartProvider.GetInternetShoppingCarts(StaticDistributorID,
                                                                                            StaticLocale,
                                                                                            StaticCopyOrderIndex,
                                                                                            StaticCopyOrderMaxLength,
                                                                                            indexChanged)
                                            : ShoppingCartProvider.GetCarts(StaticDistributorID, StaticLocale);

                var cartViewList = MyHLShoppingCartView.WrappToShoppingCartViewList(myHlShoppingCarts, StaticLocale,
                                                                                    filterExpressions, sortExpressions,
                                                                                    !copyOrderMode);

                // Getting the range.
                int totalRows = cartViewList.Count;
                var maxRows = startIndex + maximumRows > cartViewList.Count
                                  ? cartViewList.Count - startIndex
                                  : maximumRows;
                cartViewList = cartViewList.GetRange(maxRows >= 0 ? startIndex : 0, maxRows >= 0 ? maxRows : 0);

                return new MyHlShoppingCartViewResult(totalRows, cartViewList);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        ///     Set the default month when dual.
        /// </summary>
        public static void SetDefaultMonth()
        {
            var orderMonth = new OrderMonth(StaticLocale.Substring(3));
            if (orderMonth.IsDualOrderMonth && HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled)
            {
                orderMonth.CurrentChosenOrderMonth =
                    OrderMonth.DualOrderMonthSelection.Previous;
            }
        }

        /// <summary>
        ///     Check if actual shopping cart is null.
        /// </summary>
        /// <returns>True if shopping cart is not null.</returns>
        [WebMethod]
        [ScriptMethod]
        public static bool ShoppingCartIsNotEmpty()
        {
            // Set the previous month for dual.
            if (!IsShoppingCartNotEmpty)
            {
                SetDefaultMonth();
            }
            return IsShoppingCartNotEmpty;
        }

        [WebMethod]
        [ScriptMethod]
        public static void CartRetrieved(string cartId)
        {
            var id = 0;
            if (int.TryParse(cartId, out id))
            {
                var shoppingCart = ShoppingCartProvider.GetShoppingCart(StaticDistributorID, StaticLocale, id);
                 if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                 {
                    var SKUsToRemove = new List<string>(new[]
                                {
                                    HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                                    HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                                    HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                                    HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                                });
                    SKUsToRemove.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

                     foreach (var cartitem in shoppingCart.CartItems)
                     {
                         SKU_V01 sku;
                         if (SkuCollection != null && SkuCollection.TryGetValue(cartitem.SKU, out sku))
                         {
                             if (!sku.IsPurchasable)
                             {
                                 SKUsToRemove.Add(sku.SKU);
                             }
                         }
                     }

             
                     SKUsToRemove.RemoveAll(x=>x.ToString().Equals(string.Empty));
                     shoppingCart.DeleteItemsFromCart(SKUsToRemove.ToList());
                     if (shoppingCart.IsSavedCart)
                     {
                         HLRulesManager.Manager.ProcessSavedCartManagementRules(shoppingCart, ShoppingCartRuleReason.CartRetrieved);
                     }

                 }
                if (shoppingCart.IsSavedCart || shoppingCart.IsFromCopy)
                {
                    HLRulesManager.Manager.ProcessSavedCartManagementRules(shoppingCart, ShoppingCartRuleReason.CartRetrieved);
                }
            }
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod]
        public static bool DisplayNonResidentMessage(string shoppingCartID)
        {
            bool isNotified;
            bool.TryParse(HttpContext.Current.Session["isNonResidentNotified"] != null ? HttpContext.Current.Session["isNonResidentNotified"].ToString() : "false", out isNotified);

            if (HLConfigManager.Configurations.DOConfiguration.DisplayNonResidentsMessage && !isNotified)
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (member != null && member.Value != null && !member.Value.ProcessingCountryCode.Equals(StaticLocale.Substring(3)))
                {
                    var id = 0;
                    if (int.TryParse(shoppingCartID, out id))
                    {
                        var shoppingCart = ShoppingCartProvider.GetShoppingCart(StaticDistributorID, StaticLocale, id);
                        if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup &&
                            !SessionInfo.GetSessionInfo(StaticDistributorID, StaticLocale).IsEventTicketMode && !APFDueProvider.IsAPFSkuPresent(shoppingCart.CartItems))
                        {
                            HttpContext.Current.Session["isNonResidentNotified"] = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     Item created method, used to manage some pagination settings.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event agrs.</param>
        protected void ItemCreated(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType == GridItemType.Pager)
            {
                var cmbPageSize = (RadComboBox) e.Item.FindControl("PageSizeComboBox");
                if (cmbPageSize != null)
                {
                    cmbPageSize.Visible = false;
                }

                var lbl = (Label) e.Item.FindControl("ChangePageSizeLabel");
                if (lbl != null)
                {
                    lbl.Visible = false;
                }
            }
        }

        [WebMethod]
        [ScriptMethod]
        public static int IsDiscSku(string cartId)
        {
            //if ischina check for discontinued sku else return 0 which will go straight to cop1
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (!string.IsNullOrEmpty(cartId))
                {
                    var specialSkulist = ShoppingCartProvider.GetSpecialSkulist();
                    int cartIds;
                    int.TryParse(cartId, out cartIds);
                    var shoppingcart = UnfilteredCartRetrieved(cartIds);
                    var discontinuedSku = ShoppingCartProvider.GetDiscontinuededSku(shoppingcart);
                    var discontinue = discontinuedSku.Where(c => specialSkulist.All(d => d != c.SKU)).Where(e => e.IsPromo == false).Select(f => f.SKU).ToList();
                    var listofPromoSKu = discontinuedSku.Where(c => specialSkulist.All(e => e != c.SKU)).Where(f => f.IsPromo == true).Select(g => g.SKU).ToList();

                    foreach (var item in discontinuedSku)
                    {
                        shoppingcart.removeItem(item.SKU);
                    }
                    if (discontinue.Count > 0 && listofPromoSKu.Count > 0 && shoppingcart.CartItems.Count == 0)
                        return 3;
                    if (discontinue.Count > 0 && shoppingcart.CartItems.Count == 0)
                        return 1;
                    if (listofPromoSKu.Count > 0 && shoppingcart.CartItems.Count == 0)
                        return 2;
                }
            }
            return 0;
        }

        public static MyHLShoppingCart UnfilteredCartRetrieved(int cartId)
        {

            var shoppingCart = ShoppingCartProvider.GetShoppingCart(StaticDistributorID, StaticLocale, cartId);
            if (shoppingCart != null && shoppingCart.CartItems != null)
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var SKUsToRemove = new List<string>(new[]
                    {
                        HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                        HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                        HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                    });


                    foreach (var cartitem in shoppingCart.CartItems)
                    {
                        SKU_V01 sku;
                        if (SkuCollection != null && SkuCollection.TryGetValue(cartitem.SKU, out sku))
                        {
                            if (!sku.IsPurchasable)
                            {
                                SKUsToRemove.Add(sku.SKU);
                            }
                        }
                    }
                    SKUsToRemove.RemoveAll(x => x.ToString().Equals(string.Empty));
                    shoppingCart.DeleteItemsFromCart(SKUsToRemove.ToList());
                }
                return shoppingCart;
            }
            return new MyHLShoppingCart();
        }


        /// <summary>
        ///     On continue to the saved cart confirmation.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event agrs.</param>
        protected void OnContinue(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hidCartID.Value) && !string.IsNullOrEmpty(hidAction.Value))
            {
                SetDefaultMonth();
                switch (hidAction.Value)
                {
                    case "CheckOut":
                        int shCart = 0;
                        if (int.TryParse(hidCartID.Value, out shCart))
                        {
                            ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale, shCart);
                            if (this.ShoppingCart.IsSavedCart)
                            {
                                HLRulesManager.Manager.ProcessSavedCartManagementRules(this.ShoppingCart, ShoppingCartRuleReason.CartRetrieved);
                            }

                            if (DisplayNonResidentMessage(hidCartID.Value))
                            {
                                ShowNonResidentPopup(hidCartID.Value);
                                return;
                            }
                        }
                        ClearShoppingCartModuleCache();
                        Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?CartID={0}", hidCartID.Value));
                        break;

                    case "New":
                        OnNewOrderIgnoringCartNotNull(this, null);
                        break;

                    case "Copy":
                        var shoppingCart = CopyOrderProvider.CopyShoppingCart(int.Parse(hidCartID.Value), DistributorID,
                                                                              Locale, ProductInfoCatalog.AllSKUs);
                        if (shoppingCart.ShoppingCartID > 0)
                        {
                            ShoppingCart = shoppingCart;
                            if (this.ShoppingCart.IsFromCopy)
                            {
                                HLRulesManager.Manager.ProcessSavedCartManagementRules(this.ShoppingCart, ShoppingCartRuleReason.CartRetrieved);
                            }
                        }

                        if (DisplayNonResidentMessage(shoppingCart.ShoppingCartID.ToString()))
                        {
                            ShowNonResidentPopup(shoppingCart.ShoppingCartID.ToString());                            
                            return;
                        }
                        ClearShoppingCartModuleCache();
                        Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?CartID={0}",
                                                        shoppingCart.ShoppingCartID));
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(hidAction.Value))
            {
                switch (hidAction.Value)
                {
                    case "New":
                        OnNewOrderIgnoringCartNotNull(this, null);
                        break;
                }
            }
        }

        private void ClearShoppingCartModuleCache()
        {
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
        }


        /// <summary>
        ///     On new order clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event agrs.</param>
        protected void OnNewOrderIgnoringCartNotNull(object sender, EventArgs e)
        {
            if (ShoppingCart.CustomerOrderDetail == null)
            {
                ShoppingCart.ClearCart();
                Response.Redirect("~/Ordering/PriceList.aspx");
            }
            else
            {
                (Page as ProductsBase).NewCart();
                Response.Redirect(string.Format("~/Ordering/PriceList.aspx?CartID={0}",
                                                (Page as ProductsBase).ShoppingCart.ShoppingCartID));
            }
        }

        /// <summary>
        ///     On new order clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event agrs.</param>
        protected void OnNewOrder(object sender, EventArgs e)
        {
            if (ShoppingCart.IsSavedCart)
            {
                (this).NewCart();
                Response.Redirect(string.Format("~/Ordering/PriceList.aspx?CartID={0}", ShoppingCart.ShoppingCartID));
            }
            else
            {
                if (IsShoppingCartNotEmpty)
                {
                    lblSavedCartMessage1.Text =
                        string.Format(
                            GetLocalResourceObject("lblSavedCartMessage1.Text") as string,
                            txtSaveCartName.Text);
                    updSavedCart.Update();
                    mdlContinue.Show();
                    mdlClearCart.Hide();
                }
                else
                {
                    OnContinue(sender, e);
                }
            }
        }

        /// <summary>
        ///     Verifies if the cart can be saved
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected void OnSaveCart(object sender, EventArgs e)
        {
            string cartName = txtSaveCartName.Text;
            if (!string.IsNullOrEmpty(cartName))
            {
                var shoppingCart = (Page as ProductsBase).ShoppingCart;
                if (!ShoppingCartProvider.CartExists(DistributorID, Locale, cartName))
                {
                    SaveCart(cartName, false);
                }
                else
                {
                    // Validate if the active cart is the same cart to save
                    if (ShoppingCart.IsSavedCart && ShoppingCart.CartName.ToUpper().Equals(cartName.ToUpper()))
                    {
                        SaveCart(cartName, true);
                    }
                    else
                    {
                        ShowInvalidCartName();
                    }
                }
            }
            else
            {
                ShowInvalidCartName();
            }
        }

        /// <summary>
        ///     Page load method.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">EventArgs arguments.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                StaticCopyOrderIndex = 0;
                StaticCopyOrderMaxLength = 8;

                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                pMessaging.InnerText = GetLocalResourceObject("SaveCartMessaging") as string;
                if (ShoppingCart.CartItems.Count != 0)
                {
                    txtSaveCartName.Text = SaveCartCommand.SuggestCartName(ShoppingCart.DeliveryInfo, string.Empty,
                                                                           DistributorID, Locale);
                }

                // Avoiding a jquery error.
                CartsGrid.CreateTableView();

                // Loading sort dropdown options
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_ca"), "ca"));
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_cz"), "cz"));
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_sa"), "sa"));
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_sz"), "sz"));
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_da"), "da"));
                ddlOrderBy.Items.Add(new RadComboBoxItem(GetLocalResourceString("SortExp_dz"), "dz"));

                // Refresh order shipping addresses
                var provider = new ShippingProviderBase();
                provider.ReloadOrderShippingAddressFromService(StaticDistributorID, StaticLocale);

                // Loading recent orders and saved carts.
                ShoppingCartProvider.ResetInternetShoppingCartsCache(StaticDistributorID, StaticLocale);
                ShoppingCartProvider.GetInternetShoppingCarts(StaticDistributorID, StaticLocale, StaticCopyOrderIndex,
                                                              StaticCopyOrderMaxLength, false);
                ShoppingCartProvider.GetCarts(StaticDistributorID, StaticLocale, true, false);

                // Hidding grid until loads.
                CartsGrid.Style.Add(HtmlTextWriterStyle.Display, "none");

                // Initial sort selected value.
                ddlOrderBy.SelectedValue = "da";

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
                    var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && (OrderTotals.AmountDue - OrderTotals.Donation == 0))
                    {
                        ShoppingCart.DeliveryInfo = null;
                    }
                }

                if(HLConfigManager.Configurations.DOConfiguration.DisplayNonResidentsMessage)
                {
                    contentReader.ContentPath = @"NonResidentsDisclaimer.html";
                    contentReader.LoadContent();
                }
            }

            CartsGrid.MasterTableView.PagerStyle.PagerTextFormat = GetLocalResourceObject("PagerTextFormat") as string;
            if (ShoppingCart.CartItems.Count == 0 || ShoppingCart.IsSavedCart)
            {
                mdlClearCart.TargetControlID = "FakeTarget";
                NewOrder.Click += OnNewOrder;
            }

            IsShoppingCartNotEmpty = !mdlClearCart.TargetControlID.Equals("FakeTarget");

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-nav-mid-sc");
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
        ///     Saves the cart and redirects.
        /// </summary>
        /// <param name="cartName">Cart name.</param>
        /// <param name="update">Flag to update the same saved cart</param>
        private void SaveCart(string cartName, bool update)
        {
            if (!update)
            {
                var optionType = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), OptionType.ToString());
                ShoppingCart.CopyCartWithShippingInfo(true, cartName, ShippingAddresssID, DeliveryOptionID, optionType);
                if (ShoppingCart.CustomerOrderDetail == null)
                {
                    ShoppingCart.ClearCart();
                }
            }

            OnNewOrder(this, new EventArgs());
        }

        /// <summary>
        ///     Show invalid cart name div.
        /// </summary>
        private void ShowInvalidCartName()
        {
            // Set the action.
            var script = "if (localAction) { SetConfirmationPopUp(localAction, false); }";
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script, true);

            // Show the div.
            divExistentCart.Visible = true;
            txtSaveCartName.BorderColor = Color.Red;
            if (string.IsNullOrEmpty(txtSaveCartName.Text.Trim()))
            {
                lblEmptyCartName.CssClass = lblEmptyCartName.CssClass.Replace("hide", "").Trim();
                lblExistentCart.CssClass += " hide";
            }
            else
            {
                lblExistentCart.CssClass = lblExistentCart.CssClass.Replace("hide", "").Trim();
                lblEmptyCartName.CssClass += " hide";
            }
        }

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

        private void ShowNonResidentPopup(string shoppingCartID)
        {
            // Hide Continue popup
            mdlContinue.Hide();

            //Display Non Residents Popup
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), string.Format(@"SetCurrentShoppingCartID({0}); ShowOnProgressStatus(false);", shoppingCartID), true);

            pnlNonResidentMessage.CssClass = pnlNonResidentMessage.CssClass.Replace("hide", "").Trim();
            updNonResidentMessage.Update();
            mdlNonResidentMessage.Show();
        }
    }
}