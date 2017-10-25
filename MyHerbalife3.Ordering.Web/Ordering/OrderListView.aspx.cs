using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Caching;
using System.Web.Security;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using Telerik.Web.UI;
using System.Web;
using System.Xml.Serialization;
using MyHerbalife3.Ordering.Providers.Payments;
using ProviderChina = MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OrderListView : ProductsBase
    {
        public static string OrderListCaheKey = "OrderListView_";
        public static string PreOrderListCaheKey = "PreOrderListView_";
        public const string YearMonthDayFormat = "yyyy-MM-dd";

        private bool _isPreOrdering = false;
        private int _currentPageIndex = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtStartDate.Text = DateTime.Now.AddDays(-30).ToString(YearMonthDayFormat);
                txtEndDate.Text = DateTime.Now.ToString(YearMonthDayFormat);

                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);

                btnOrdering.Text = GetLocalResourceString("Order");
                btnPreOrdering.Text = GetLocalResourceString("Pre-Order");
                txtSearch.Attributes["placeholder"] = GetLocalResourceObject("Search.Text") as string;

                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("TextSelect"), ""));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_va"), "va"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_vd"), "vd"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_oa"), "oa"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_od"), "od"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_pa"), "pa"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_pd"), "pd"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_ma"), "ma"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_md"), "md"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_osa"), "osa"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_osd"), "osd"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_aa"), "aa"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_ad"), "ad"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_sa"), "sa"));
                ddlOrderBy.Items.Add(new ListItem(GetLocalResourceString("SortExp_sz"), "sz"));

                ddlOrderStatus.DataSource = OrderStatusList;
                ddlOrderStatus.DataBind();

                CreateOrderHistoryInfo(0, 4, ddlOrderBy.SelectedValue, string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text, false, 0, 8, string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text, string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text, ddlOrderStatus.SelectedValue);
            }

            _isPreOrdering = btnPreOrdering.CssClass == "selectedActionButton";
            _currentPageIndex = PagingControl1.FirstItemIndex;

            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
            {
                (Master as OrderingMaster).divLeftVisibility = true;
            }
            else
            {
                (Master as OrderingMaster).divLeftVisibility = false;
            }

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-xs-12 gdo-nav-mid-olv");

            //bug 228594 removed because no longer in use
            //if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrdernumber"]) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrderinfo"]) && HttpContext.Current.Request.Form["hdnBtnType"].Equals("checkOut"))
            //{
            //    OnContinue(HttpContext.Current.Request.Form["hdnFeedBack"], System.Net.WebUtility.HtmlDecode(HttpContext.Current.Request.Form["hdnOrderinfo"]));
            //}

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrdernumber"]) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrderinfo"]) && HttpContext.Current.Request.Form["hdnBtnType"].Equals("copyOrder"))
            {
                CopyOrder(System.Net.WebUtility.HtmlDecode(HttpContext.Current.Request.Form["hdnOrderinfo"]));
            }

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrdernumber"]) && !string.IsNullOrEmpty(HttpContext.Current.Request.Form["hdnOrderinfo"]) && HttpContext.Current.Request.Form["hdnBtnType"].Equals("deleteCart"))
            {
                //DeleteCart(System.Net.WebUtility.HtmlDecode(HttpContext.Current.Request.Form["hdnOrderinfo"]));

                CreateOrderHistoryInfo(0, 4, ddlOrderBy.SelectedValue, string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text, false, 0, 8, string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text, string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text, ddlOrderStatus.SelectedValue);
                conformationPopupExtender.Show();
                ViewState["Orderinfo"] = System.Net.WebUtility.HtmlDecode(HttpContext.Current.Request.Form["hdnOrderinfo"]);
            }
        }

        /// <summary>
        ///     Gets or sets static locale.
        /// </summary>
        public string StaticLocale
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.ToString();
            }
            set { }
        }
        protected void Onokclick(object sender, EventArgs e)
        {

            if (ViewState["Orderinfo"] != null)
            {
                DeclineShoppingCart(ViewState["Orderinfo"].ToString());

            }

        }

        protected void Onnoclock(object sender, EventArgs e)
        {
            conformationPopupExtender.Hide();
        }
        protected void OnDisplayDiscontinueSkuClose(object sender, EventArgs e)
        {
            displaySkuPopupModal.Hide();
        }

        public void DeclineShoppingCart(string Ordeinfo)
        {

            try
            {

                int cartId = 0;
                var jsonvalue = Newtonsoft.Json.JsonConvert.DeserializeObject<MyHLShoppingCartView>(Ordeinfo);
                if (int.TryParse(jsonvalue.ID, out cartId))
                {
                    if (!string.IsNullOrEmpty(jsonvalue.ID))
                    {

                        int cartID = int.Parse(jsonvalue.ID);
                        CatalogProvider.CloseShoppingCart(cartID, DistributorID, String.Empty, DateTime.Now);
                        ShoppingCart.ClearCart();
                        var requestString = "User declined for OrderNumber: " + jsonvalue.OrderNumber;
                        PaymentGatewayInvoker.LogMessageWithInfo(
                            PaymentGatewayLogEntryType.Response,
                            jsonvalue.OrderNumber,
                            string.Empty,
                            jsonvalue.OrderStatus.GetType().Name.Replace("Response", string.Empty),
                            PaymentGatewayRecordStatusType.Declined, requestString + jsonvalue.OrderStatus

                   );
                        //string cacheKey = string.Format("{0}_{1}", Ordeinfo, cartId);
                        IDictionaryEnumerator en = Cache.GetEnumerator();

                        while (en.MoveNext())
                        {
                            if (
                                en.Key.ToString()
                                  .StartsWith(string.Format(OrderListCaheKey + jsonvalue.DistributorID)))
                            {
                                HttpRuntime.Cache.Remove(en.Key.ToString());
                            }
                        }
                        HttpContext.Current.Cache.Remove(OrderListCaheKey);



                        Response.Redirect("Pricelist.aspx?ETO=FALSE");
                    }


                }

            }
            catch (ThreadAbortException)
            { }


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
                var cmbPageSize = (RadComboBox)e.Item.FindControl("PageSizeComboBox");
                if (cmbPageSize != null)
                {
                    cmbPageSize.Visible = false;
                }

                var lbl = (Label)e.Item.FindControl("ChangePageSizeLabel");
                if (lbl != null)
                {
                    lbl.Visible = false;
                }
            }
        }

        protected void GetData(bool isPreOrdering)
        {
            if (isPreOrdering)
            {
                CreatePreOrderHistoryInfo(PagingControl1.FirstItemIndex, 4, ddlOrderBy.SelectedValue, string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text, string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text, string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text, ddlOrderStatus.SelectedValue);
            }
            else
            {
                CreateOrderHistoryInfo(PagingControl1.FirstItemIndex, 4, ddlOrderBy.SelectedValue, string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text, false, 0, 8, string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text, string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text, ddlOrderStatus.SelectedValue);
            }

            PagingControl1.BuildPages();
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
        /// <param name="startYearMonthDay"></param>
        /// <param name="endYearMonthDay"></param>
        /// <param name="orderStatus"></param>
        /// <returns>Saved carts according the grid parameters.</returns>
        public MyHlShoppingCartViewResult GetData(
            int startIndex,
            int maximumRows,
            string sortExpressions,
            string filterExpressions,
            bool copyOrderMode,
            int copyOrderIndex,
            int copyOrderMaxLength,
            string startYearMonthDay, string endYearMonthDay, string orderStatus)
        {
            try
            {
                var distributorID = string.Empty;
                DistributorOrderingProfile distributorOrderingProfile = null;

                #region determine startDate and endDate
                DateTime? startDate = null;
                if (IsValidDate(startYearMonthDay))
                {
                    startDate = DateTime.ParseExact(startYearMonthDay, YearMonthDayFormat, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    var proposedStartDate = DateTime.Now.AddDays(-30);
                    startDate = new DateTime(proposedStartDate.Year, proposedStartDate.Month, proposedStartDate.Day); //ensure the time is the start of the day, which is 00:00:00
                    //Update UI incase the date have been modified from the above logic.
                    txtStartDate.Text = startDate.Value.ToString(YearMonthDayFormat);
                }

                DateTime? endDate = null;
                if (IsValidDate(endYearMonthDay))
                {
                    endDate = DateTime.ParseExact(endYearMonthDay, YearMonthDayFormat, System.Globalization.CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);
                }
                else
                {
                    endDate = DateTime.Now;
                    //Update UI incase the date have been modified from the above logic.
                    txtEndDate.Text = endDate.Value.ToString(YearMonthDayFormat);
                }
                #endregion

                #region chinaOrdSts
                MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType chinaOrdSts = MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All;
                Enum.TryParse<MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType>(orderStatus, out chinaOrdSts);
                #endregion

                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (membershipUser != null && membershipUser.Value != null)
                {
                    distributorID = membershipUser.Value.Id;
                    distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorID, "CN");
                }
                else
                {
                    return new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                }

                if (distributorOrderingProfile == null)
                {
                    return new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                }

                var objBL = new MyHLShoppingCartView();
                var cartViewList = new List<MyHLShoppingCartView>();

                string cacheKey = string.Format(OrderListCaheKey + distributorID + "CN" + chinaOrdSts + filterExpressions + sortExpressions + startYearMonthDay + endYearMonthDay);

                if (Session["FeedbackSaved"] != null && (bool)Session["FeedbackSaved"])
                {
                    HttpContext.Current.Cache.Remove(cacheKey);
                    HttpContext.Current.Session.Remove("FeedbackSaved");
                }

                cartViewList = objBL.GetOrdersWithDetail(distributorID, distributorOrderingProfile.CNCustomorProfileID, Thread.CurrentThread.CurrentCulture.ToString(), startDate.Value, endDate.Value, chinaOrdSts, filterExpressions, sortExpressions, true);

                // Getting the range.
                if (cartViewList != null)
                {
                    int totalRows = cartViewList.Count;
                    var maxRows = startIndex + maximumRows > cartViewList.Count
                                      ? cartViewList.Count - startIndex
                                      : maximumRows;
                    cartViewList = cartViewList.GetRange(maxRows >= 0 ? startIndex : 0, maxRows >= 0 ? maxRows : 0);

                    #region determine HasEventItem?

                    var skuList = CatalogProvider.GetAllSKU(Thread.CurrentThread.CurrentCulture.ToString(), base.CurrentWarehouse);

                    foreach (var cartView in cartViewList)
                    {
                        var cartItems = cartView.CartItems;
                        if (!Helper.GeneralHelper.Instance.HasData(cartItems)) continue;

                        foreach (var cartItem in cartItems)
                        {
                            var m = skuList.FirstOrDefault(x => x.Value.SKU == cartItem.SKU);

                            var mv = m.Value;
                            if (mv == null) continue;

                            var p = mv.Product;
                            cartView.HasEventItem = (p != null) && (p.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket);
                        }
                    }

                    #endregion

                    return new MyHlShoppingCartViewResult(totalRows, cartViewList);
                }

                var retEmpty = new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                return retEmpty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());

                var retErr = new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                retErr.ErrorMessage = ex.ToString();
                return retErr;
            }
        }

        /// <summary>
        ///     On continue to the saved cart confirmation.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event agrs.</param>
        protected void OnContinue(string Ordernumber, string Orderinfo)
        {
            //var jsonvalue = Newtonsoft.Json.JsonConvert.DeserializeObject <dynamic>(result.Value);
            var jsonvalue = Newtonsoft.Json.JsonConvert.DeserializeObject<MyHLShoppingCartView>(Orderinfo);
            if (jsonvalue != null)
            {
                int shoppingCartID = 0;
                if (int.TryParse(jsonvalue.ID, out shoppingCartID))
                {
                    var orderNum = string.Empty;
                    ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorOrderingProfile.Id, Thread.CurrentThread.CurrentCulture.ToString(), shoppingCartID);


                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        if (ShoppingCart.DeliveryInfo.Address != null)
                        {
                            ShoppingCart.DeliveryInfo.Address.AltPhone = jsonvalue.PendingOrderAltPhone;
                            //GetEntryValue(dicInfo, "PendingOrderAltPhone");
                            ShoppingCart.DeliveryInfo.Address.Phone = jsonvalue.PendingOrderPhone;
                            //GetEntryValue(dicInfo, "PendingOrderPhone");
                            ShoppingCart.DeliveryInfo.Address.Recipient = jsonvalue.PendingOrderRecipient;
                            //GetEntryValue(dicInfo, "PendingOrderRecipient");
                        }
                        ShoppingCart.DeliveryInfo.Instruction = jsonvalue.PendingOrderInstruction;
                        //GetEntryValue(dicInfo, "PendingOrderInstruction");
                        ShoppingCart.DeliveryInfo.RGNumber = jsonvalue.PendingOrderRGNumber;
                        //GetEntryValue(dicInfo, "PendingOrderRGNumber");

                    }
                    ShoppingCart.EmailAddress = jsonvalue.PendingOrderEmail;//GetEntryValue(dicInfo, "PendingOrderEmail");
                    OrderProvider.CreateOrder(orderNum, ShoppingCart, null);
                    var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
                    //currentSession.PendingOrderId = orderNum;
                    SessionInfo.SetSessionInfo(DistributorOrderingProfile.Id, StaticLocale, currentSession);
                    Response.Redirect(string.Format("~/Ordering/Checkout.aspx"));
                }
            }
        }

        private Dictionary<string, string> GetEntries(string value)
        {
            Dictionary<string, string> dictEntries = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(value))
            {
                dictEntries = (from p in value.Split(new[] { ';' })
                               where p.Contains('=')
                               select p.Split(new[] { '=' })).ToDictionary(k => k[0], v => v[1]);
            }
            return dictEntries;
        }

        private string GetEntryValue(Dictionary<string, string> dictEntries, string entryName)
        {
            if (dictEntries.ContainsKey(entryName))
            {
                return dictEntries[entryName];
            }
            return string.Empty;
        }

        /// <summary>
        ///     Copy an order shopping cart.
        /// </summary>
        /// <param name="shoppingCartID">Shopping cart ID to copy.</param>
        /// <param name="ignoreCartNotEmpty">Ignore if current shopping cart is not empty.</param>
        /// <param name="orderinfo"></param>
        /// <returns>My HL Copy Order Result.</returns>
        public MyHLCopyOrderResult CopyOrder(string orderinfo)
        {
            try
            {
                var jsonvalue = Newtonsoft.Json.JsonConvert.DeserializeObject<MyHLShoppingCartView>(orderinfo);
                var shoppingCart = CopyOrderProvider.CopyShoppingCart(int.Parse(jsonvalue.ID), DistributorID,
                                                                          Thread.CurrentThread.CurrentCulture.ToString(), ProductInfoCatalog.AllSKUs);
                if (shoppingCart != null)
                {
                    if (shoppingCart.DeliveryInfo == null)
                    {
                        pMessaging.InnerText = GetLocalResourceString("RequiredCreateAddress") as string;
                        CreateOrderHistoryInfo(PagingControl1.FirstItemIndex, 4, ddlOrderBy.SelectedValue, string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text, false, 0, 8, string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text, string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text, ddlOrderStatus.SelectedValue);
                }
                else
                {
      
                        var discontinuedSku = ShoppingCartProvider.GetDiscontinuededSku(int.Parse(jsonvalue.ID), DistributorID, 
                            Thread.CurrentThread.CurrentCulture.ToString(), 0, ProductInfoCatalog.AllSKUs);
                        var skutobeQueried = discontinuedSku.Select(x => x.SKU).ToList();
                        var listofDiscProd = CatalogProvider.GetCatalogItems(skutobeQueried, CountryCode);

                        if (shoppingCart.CartItems!=null && shoppingCart.CartItems.Any())
                        // means after the cart being copy and filtered other valid item still exist
                        {
                            Response.Redirect("ShoppingCart.aspx?CartID=" + shoppingCart.ShoppingCartID + "", false);
                            HttpContext.Current.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count == 0 )
                            // means after copy all cart item removed
                            {
                                if (discontinuedSku != null && listofDiscProd != null)
                {
                                    //DiscSkuLabelListLinkButton.Text =
                                    //    string.Format(GetLocalResourceString("ErrorCopyOrderDiscontinueSkuExist"));
                                    //DiscSkuLabelListLinkButton.Visible = false;
                                    ErrorMessageDiscSkuLabel.Text =
                                        string.Format(GetLocalResourceString("ErrorCopyOrderDiscontinueSkuExist"));
                                    ErrorMessageDiscSkuLabel.Visible = true;
                }
                else
                {
                                    LoggerHelper.Error(
                                        string.Format(
                                            "failed to get product name listofDiscProd is null, Distributor{0}, locale{1},shoppingcart isnull:{2}",
                                            DistributorID, Locale, (shoppingCart == null) ? "null" : shoppingCart.CartName));
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return null;
        }

        

        public static void CartRetrieved(string cartId)
        {
            var id = 0;
            if (int.TryParse(cartId, out id))
            {
                var distributorID = string.Empty;
                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (membershipUser != null && membershipUser.Value != null)
                {
                    distributorID = membershipUser.Value.Id;
                }
                if (!string.IsNullOrEmpty(distributorID))
                {
                    var shoppingCart = ShoppingCartProvider.GetShoppingCart(distributorID,
                                                                            Thread.CurrentThread.CurrentCulture.ToString
                                                                                (), id);
                    if (shoppingCart.IsSavedCart || shoppingCart.IsFromCopy)
                    {
                        HLRulesManager.Manager.ProcessSavedCartManagementRules(shoppingCart,
                                                                               ShoppingCartRuleReason.CartRetrieved);
                    }
                }
            }
        }

        /// <summary>
        ///     Delete Cart method.
        /// </summary>
        /// <param name="cartID">Cart id string.</param>
        /// <returns>If cart has been deleted.</returns>
        public void DeleteCart(string Orderinfo)
        {
            try
            {
                int cartId = 0;
                var jsonvalue = Newtonsoft.Json.JsonConvert.DeserializeObject<MyHLShoppingCartView>(Orderinfo);
                if (int.TryParse(jsonvalue.ID, out cartId))
                {
                    var distributorID = string.Empty;
                    var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    if (membershipUser != null && membershipUser.Value != null)
                    {
                        distributorID = membershipUser.Value.Id;
                    }
                    if (!string.IsNullOrEmpty(distributorID))
                    {
                        var cart = ShoppingCartProvider.GetShoppingCart(distributorID,
                                                                        Thread.CurrentThread.CurrentCulture.ToString(),
                                                                        cartId);
                        if (cart != null)
                        {
                            if (ShoppingCartProvider.DeleteShoppingCart(cart, null))
                            {
                                ShoppingCartProvider.DeleteCartFromCache(distributorID,
                                                                         Thread.CurrentThread.CurrentCulture.ToString(),
                                                                         cartId);
                                if (!string.IsNullOrEmpty(jsonvalue.OrderNumber))
                                {
                                    OrderProvider.UpdatePaymentGatewayRecord(jsonvalue.OrderNumber,
                                                                             "Pending Order deleted by DS",
                                                                             PaymentGatewayLogEntryType.Request,
                                                                             PaymentGatewayRecordStatusType
                                                                                 .Declined);
                                }
                                Response.Redirect("Pricelist.aspx?ETO=FALSE");
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        protected void OnDelete(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/Pricelist.aspx?ETO=FALSE");
        }

        #region OrderStatusList

        #region OOS Order-Status-Code-List
        // taken from http://www.myherbalife.cn/Ordering/OrderList.aspx as at 2014/Oct/23
        //<option value="-1">全部</option>
        //<option value="待付款">待付款</option>
        //<option value="支付成功">支付成功</option>
        //<option value="支付失败">支付失败</option>
        //<option value="In Progress">处理中</option>
        //<option value="To Be Assign">待配送</option>
        //<option value="NTS Printed">配货中</option>
        //<option value="Complete">已出库</option>
        //<option value="Cancel Order">取消订单</option>
        #endregion
        Dictionary<ProviderChina.OrderStatusFilterType, string> _OrderStatusList = null;

        Dictionary<ProviderChina.OrderStatusFilterType, string> OrderStatusList
        {
            get
            {
                if (_OrderStatusList == null)
                {
                    Dictionary<ProviderChina.OrderStatusFilterType, string> list = new Dictionary<ProviderChina.OrderStatusFilterType, string>
                    {
                        { ProviderChina.OrderStatusFilterType.All, GetLocalResourceString("OrderStatus_All") },
                        { ProviderChina.OrderStatusFilterType.Payment_Pending, GetLocalResourceString("OrderStatus_PaymentPending") },
                        { ProviderChina.OrderStatusFilterType.Payment_Success, GetLocalResourceString("OrderStatus_PaymentSuccess") },
                        { ProviderChina.OrderStatusFilterType.Payment_Failed, GetLocalResourceString("OrderStatus_PaymentFailed") },
                        { ProviderChina.OrderStatusFilterType.In_Progress, GetLocalResourceString("OrderStatus_In_Progress") },
                        { ProviderChina.OrderStatusFilterType.To_Be_Assign, GetLocalResourceString("OrderStatus_To_Be_Assign") },
                        { ProviderChina.OrderStatusFilterType.NTS_Printed, GetLocalResourceString("OrderStatus_NTS_Printed") },
                        { ProviderChina.OrderStatusFilterType.Complete, GetLocalResourceString("OrderStatus_Complete") },
                        { ProviderChina.OrderStatusFilterType.Cancel_Order, GetLocalResourceString("OrderStatus_Cancel_Order") },
                    };

                    _OrderStatusList = list;
                }

                return _OrderStatusList;
            }
        }

        Dictionary<ProviderChina.PreOrderStatusFilterType, string> _PreOrderStatusList = null;

        Dictionary<ProviderChina.PreOrderStatusFilterType, string> PreOrderStatusList
        {
            get
            {
                if (_PreOrderStatusList == null)
                {
                    Dictionary<ProviderChina.PreOrderStatusFilterType, string> list = new Dictionary<ProviderChina.PreOrderStatusFilterType, string>
                    {
                        { ProviderChina.PreOrderStatusFilterType.All, GetLocalResourceString("OrderStatus_All") },
                        { ProviderChina.PreOrderStatusFilterType.ReadyToSubmit_PreOrder, GetLocalResourceString("PreOrderStatus_ReadyToSubmit") },
                        { ProviderChina.PreOrderStatusFilterType.Cancel_PreOrder, GetLocalResourceString("PreOrderStatus_Cancel") },
                        { ProviderChina.PreOrderStatusFilterType.Hold_PreOrder, GetLocalResourceString("PreOrderStatus_Hold") },
                    };

                    _PreOrderStatusList = list;
                }

                return _PreOrderStatusList;
            }
        }

        #endregion

        /// <summary>
        /// Returns null if value is the 1st option.
        /// </summary>
        /// <param name="orderStatus"></param>
        /// <returns></returns>
        static string FormatOrderStatus(string orderStatus)
        {
            return (string.IsNullOrWhiteSpace(orderStatus)) ? null : orderStatus;
        }

        static bool IsValidDate(string yearMonthDay)
        {
            if (string.IsNullOrWhiteSpace(yearMonthDay) || (yearMonthDay.Length != 10)) return false;

            DateTime tmpDateTime;

            if (DateTime.TryParseExact(yearMonthDay, YearMonthDayFormat, null, System.Globalization.DateTimeStyles.None, out tmpDateTime))
            {
                if (tmpDateTime.Year < 1970) //ECMAScript min date
                    return false;
                else
                    return true;
            }

            return false;
        }

        private void CreateOrderHistoryInfo(int startIndex,
              int maximumRows,
              string sortExpressions,
              string filterExpressions,
              bool copyOrderMode,
              int copyOrderIndex,
              int copyOrderMaxLength,
              string startYearMonthDay, string endYearMonthDay, string orderStatus)
        {
            try
            {
                var cartResult = GetData(startIndex, maximumRows, sortExpressions, filterExpressions, copyOrderMode, copyOrderIndex, copyOrderMaxLength, startYearMonthDay, endYearMonthDay, orderStatus);

                var htmlTable = new StringBuilder();
                htmlTable.Append("<table id=\"TABLE-ID\" class=\"rgMasterTable order-list-view\" border=\"0\" >");
                htmlTable.Append("<thead>" +
                                 "<tr>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderSummaryResource.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("StoreInfo.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderMonth.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderStatus.HeaderText") + "</th>");
                if (!DistributorOrderingProfile.IsPC)
                {
                    htmlTable.Append("<th class=\"rgHeader\" scope=\"col\">" +
                                     GetLocalResourceObject("VolumePoints.HeaderText") + "</th>");
                }
                htmlTable.Append("<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("TotalAmount.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("ProductsResource.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("ActionResource.HeaderText") + "</th>" +
                             "</tr>" +
                             "</thead>");
                htmlTable.Append("<tbody>");
                int i = 0;
                if (cartResult.TotalRows > 0)
                {
                    PagingControl1.Visible = true;
                    PagingControl1.TotalRecordsCount = cartResult.TotalRows;
                    lblNoRecords.Visible = false;
                }
                else
                {
                    lblNoRecords.Visible = true;
                    PagingControl1.Visible = false;
                }

                if (cartResult.ResultList != null)
                {
                    bool isPendingOrder = false;
                    cartResult.ResultList.ForEach(c =>
                    {
                        if (c.IsPaymentPending)

                            isPendingOrder = true;

                    });
                    foreach (var shoppingcartview in cartResult.ResultList)
                    {
                        i++;
                        //Double encoding to prevent form submission issue when the input element contains html syntax.
                        string jsonformatedorderinfo = System.Net.WebUtility.HtmlEncode(System.Net.WebUtility.HtmlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(shoppingcartview)));
                        htmlTable.Append("<tr class='" + ((i % 2 != 0) ? "rgRow" : "rgAltRow") + "'>");
                        htmlTable.Append("<td class=\"olv-cart-sumary\" scope=\"col\">");
                        //htmlTable.Append("<a class=\"lnkCartName\" href=\"javascript:;\"><i class=\"icon-arrow-triangle-right icon-arrow-triangle-down green\"></i>" + shoppingcartview.OrderNumber + "</a>");
                        htmlTable.Append("<i class=\"icon-arrow-triangle-right icon-arrow-triangle-down green\"></i>" + shoppingcartview.OrderNumber + "</a>");
                        htmlTable.Append("<span class=\"lblDate\">" + shoppingcartview.Date + "</span>");
                        htmlTable.Append("<p class=\"hiddenClass pAddress\" style=\"display: none;\">");
                        htmlTable.Append("<span class=\"lblRecipient\">" + shoppingcartview.Recipient + "</span>");
                        htmlTable.Append("<br/>");
                        htmlTable.Append("<span id=\"lblAddressText\">" + GetLocalResourceObject("lblAddressText.Text") + "</span>");
                        htmlTable.Append("<br/>");
                        htmlTable.Append("<span class=\"lblAddress\">" + shoppingcartview.Address + "</span>");
                        htmlTable.Append("</p>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("<td class=\"olv-cart-names\" scope=\"col\">");
                        htmlTable.Append("<span id=\"lblStoreInfo\">" + shoppingcartview.StoreInfo + "</span>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("<td class=\"olv-month\" scope=\"col\">");
                        htmlTable.Append("<span id=\"lblOrderMonth\">" + shoppingcartview.OrderMonth + "</span></td>");
                        htmlTable.Append("<td class=\"olv-status\" scope=\"col\"><span id=\"lblOrderStatus\">" + shoppingcartview.OrderStatus + "</span></td>");
                        if (!DistributorOrderingProfile.IsPC)
                        {
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + (shoppingcartview.CartItems.Count > 0 ? shoppingcartview.VolumePoints : string.Empty) + "</span></td>");
                        }
                        decimal amountTotalValue;
                        decimal? tmpTotalAmount = null;
                        if (decimal.TryParse((string)shoppingcartview.TotalAmount, out amountTotalValue)) tmpTotalAmount = amountTotalValue;

                        htmlTable.Append("<td class=\"olv-payment\" scope=\"col\"><span id=\"lblTotalColumn\">" + (shoppingcartview.CartItems.Count > 0 || amountTotalValue > 0 ? shoppingcartview.TotalAmount : shoppingcartview.DonationAmount.ToString()) + "</span></td>");
                        htmlTable.Append("<td class=\"olv-products\" scope=\"col\">" +
                        "<div class=\"productsCont\">" +
                            "<table>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<th class=\"sv_qty\">" +
                                                "<span id=\"lblQtyColumn\">" + GetLocalResourceObject("QtyResource.Text") + "</span>" +  // globalresource
                                        "</th>" +
                                        "<th class=\"sv_sku\">" +
                                                "<span id=\"lblSKUCoulmn\">" + GetLocalResourceObject("SkuResource.Text") + "</span>" + // globalresource
                                        "</th>" +
                                        "<th class=\"sv_product\">" +
                                                "<span id=\"lblProductColumn\">" + GetLocalResourceObject("ProductResource.Text") + "</span>" + // globalresource
                                        "</th>" +
                                    "</tr>");
                        var itemNumber = 1;

                        if (shoppingcartview.CartItems != null)
                        {
                            //Donation Order
                            if (shoppingcartview.CartItems.Count == 0)
                            {
                                htmlTable.Append("<tr>");
                                htmlTable.Append("<td><span>1</span></td>");
                                htmlTable.Append("<td><span></span></td>");
                                htmlTable.Append("<td><span>天使听见爱</span></td>");
                                htmlTable.Append("</tr>");
                            }
                            else
                            {
                                foreach (var cartitem in shoppingcartview.CartItems)
                                {
                                    if (itemNumber++ > 3)
                                    {
                                        htmlTable.Append("<tr class='moreRowsItem" + shoppingcartview.OrderNumber + " hide'>");
                                    }
                                    else
                                    {
                                        htmlTable.Append("<tr>");
                                    }

                                    htmlTable.Append("<td><span>" + cartitem.Quantity + "</span></td>");
                                    htmlTable.Append("<td><span>" + cartitem.SKU + "</span></td>");
                                    htmlTable.Append("<td><span>" + cartitem.Description + "</span></td>");
                                    htmlTable.Append("</tr>");
                                }
                            }
                        }

                        htmlTable.Append("</tbody></table>");

                        if (shoppingcartview.CartItems != null && shoppingcartview.CartItems.Count > 3)
                        {
                            htmlTable.Append("<a class=\"lnkTotalProducts\" href=\"javascript:;\" style=\"display: block;\" show='moreRowsItem" + shoppingcartview.OrderNumber + "'>" + string.Format(GetLocalResourceObject("MoreLinkResource").ToString(), shoppingcartview.CartItems.Count.ToString()) + "</a>");
                        }

                        htmlTable.Append("</div>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("<td class=\"olv-action\" scope=\"col\">");

                        if (shoppingcartview.IsCopyEnabled && !isPendingOrder == true)
                        {
                            htmlTable.Append("<div class=\"copyOrderBtn\">" +
                                                 "<form action=\"OrderListView.aspx\" method=\"post\">" +
                                             "<input type=\"hidden\" name='hdnOrdernumber' value=\'" +
                                             shoppingcartview.OrderNumber +
                                             "' />" +
                                             "<input type=\"hidden\" name='hdnBtnType' value=\"copyOrder\"/>" +
                                             "<input type=\"hidden\" name='hdnOrderinfo' value=\'" + jsonformatedorderinfo +
                                             "' />" +
                                             "<input type=\"submit\" id='btncopyOrderBtn_" + i + "' value='" + GetLocalResourceObject("CopyNewResource.Text") + "' class=\"forward\"/>" +
                                            "<input type=\"hidden\" name='isPostback' value=\"true\"/>" +
                                             "</form>" +
                                             "</div>");
                        }

                        if (shoppingcartview.OrderStatus.Equals("未知") && isPendingOrder == true)
                        {
                            htmlTable.Append("<div class=\"deleteCartBtn\">" +
                                            "<form action=\"OrderListView.aspx\" method=\"post\">" +
                                        "<input type=\"hidden\" name='hdnOrdernumber' value=\'" + shoppingcartview.OrderNumber +
                                        "' />" +
                                        "<input type=\"hidden\" name='hdnBtnType' value=\"deleteCart\"/>" +
                                        "<input type=\"hidden\" name='hdnOrderinfo' value=\'" + jsonformatedorderinfo + "' />" +
                                        "<input type=\"submit\" id='btndeleteCartBtn_" + i + "' value='" + GetLocalResourceObject("DeleteCommandResource.Text") + "' class=\"forward\"/>" +
                                        "<input type=\"hidden\" name='isPostback' value=\"true\"/>" +
                                        "</form>" +
                                        "</div>");
                        }

                        if (shoppingcartview.OrderStatus.Equals("完成") && !shoppingcartview.HasFeedBack && shoppingcartview.CartItems.Count() > 0 && !isPendingOrder == true)
                        {
                            htmlTable.Append("<div class=\"FeedBackBtn\">" +
                                             "<form action=\"OrderFeedBack.aspx?OrderHeaderId=" + shoppingcartview.OrderHeaderId +
                                             "method=\"post\">" +
                                             "<input type=\"hidden\" name='OrderHeaderId' value=\'" +
                                             shoppingcartview.OrderHeaderId +
                                             "' />" +
                                             "<input type=\"submit\" id='btnFeedBack_" + i +
                                             "' value='" + GetLocalResourceObject("FeedBackCommandResource.Text") + "' class=\"forward\" />" +
                                             "<input type=\"hidden\" name='isPostback' value=\"true\"/>" +
                                             "</form>" +
                                             "</div>");
                        }

                        if (!shoppingcartview.OrderStatus.Equals("支付失败") && !shoppingcartview.OrderStatus.Equals("未知") && !isPendingOrder == true)
                        {
                            htmlTable.Append("<div class=\"ExpressTrackBtn\">" +
                                             "<form action=\"ExpressTrack.aspx?OrderId=" + shoppingcartview.OrderNumber + "&OrderDate=" + shoppingcartview.DateTimeForOrder.ToString(YearMonthDayFormat) +
                                             "\" method=\"post\">" +
                                             "<input type=\"hidden\" name='OrderId' value=\'" + shoppingcartview.OrderNumber + "' />" +
                                             "<input type=\"submit\" id='btnExpressTrackBtn_" + i +
                                             "' value='" + GetLocalResourceObject("ExpressTrackCommandResource.Text") + "' class=\"forward\" />" +
                                             "<input type=\"hidden\" name='isPostback' value=\"true\"/>" +
                                             "</form>" +
                                             "</div>");
                        }
                    }
                    htmlTable.Append("</td>");
                    htmlTable.Append("</tr>");

                }

                htmlTable.Append("</tbody>");
                htmlTable.Append("</table>");
                if (OrderHistoryGrid.HasControls())
                {
                    OrderHistoryGrid.Controls.RemoveAt(0);
                }
                var result = htmlTable.ToString();
                OrderHistoryGrid.Controls.Add(new Literal { Text = result });

            }
            catch (Exception ex)
            {

                LoggerHelper.Error(ex.ToString());
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs e = (CommandEventArgs)args;

            if (txtSearch.Text == GetLocalResourceObject("Search.Text") as string)
            {
                txtSearch.Text = "";
            }

            //Go to Next Page
            if (string.Equals(e.CommandName, "MoveNext", StringComparison.OrdinalIgnoreCase))
            {
                GetData(_isPreOrdering);
            }
            //Go to Previous Page
            else if (string.Equals(e.CommandName, "MovePrev", StringComparison.OrdinalIgnoreCase))
            {
                GetData(_isPreOrdering);
            }
            //Go to First Page
            else if (string.Equals(e.CommandName, "MoveFirst", StringComparison.OrdinalIgnoreCase))
            {
                GetData(_isPreOrdering);
            }
            //Go to Last Page
            else if (string.Equals(e.CommandName, "MoveLast", StringComparison.OrdinalIgnoreCase))
            {
                GetData(_isPreOrdering);
            }
            //Go to Page Number ...
            else if (string.Equals(e.CommandName, "GoToPage", StringComparison.OrdinalIgnoreCase))
            {
                GetData(_isPreOrdering);
            }

            return base.OnBubbleEvent(source, e);
        }

        protected void OrderStatusSelectedChanged(object sender, EventArgs e)
        {
            PagingControl1.CurrentPage = 0;
            _currentPageIndex = 0;
            GetData(_isPreOrdering);
        }

        protected void OrderBySelectedChanged(object sender, EventArgs e)
        {
            GetData(_isPreOrdering);
        }

        protected void SearchOrderInfo(object Source, EventArgs e)
        {
            PagingControl1.CurrentPage = 0;
            _currentPageIndex = 0;
            GetData(_isPreOrdering);
        }

        private void CreatePreOrderHistoryInfo(int startIndex,
              int maximumRows,
              string sortExpressions,
              string filterExpressions,
              string startYearMonthDay, string endYearMonthDay, string orderStatus)
        {
            try
            {
                var cartResult = GetPreOrderData(startIndex, maximumRows, sortExpressions, filterExpressions, startYearMonthDay, endYearMonthDay, orderStatus);
                var htmlTable = new StringBuilder();
                htmlTable.Append("<table id=\"TABLE-ID\" class=\"rgMasterTable order-list-view\" border=\"0\" >");
                htmlTable.Append("<thead>" +
                                 "<tr>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderSummaryResource.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("StoreInfo.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderMonth.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" +
                                 GetLocalResourceObject("OrderStatus.HeaderText") + "</th>");
                if (!DistributorOrderingProfile.IsPC)
                {
                    htmlTable.Append("<th class=\"rgHeader\" scope=\"col\">" +
                                     GetLocalResourceObject("VolumePoints.HeaderText") + "</th>");
                }
                htmlTable.Append("<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("TotalAmount.HeaderText") + "</th>" +
                                 "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("ProductsResource.HeaderText") + "</th>" +
                             "</tr>" +
                             "</thead>");
                htmlTable.Append("<tbody>");
                int i = 0;
                if (cartResult.TotalRows > 0)
                {
                    PagingControl1.Visible = true;
                    PagingControl1.TotalRecordsCount = cartResult.TotalRows;
                    lblNoRecords.Visible = false;
                }
                else
                {
                    lblNoRecords.Visible = true;
                    PagingControl1.Visible = false;
                }

                if (cartResult.ResultList != null)
                {
                    foreach (var shoppingcartview in cartResult.ResultList)
                    {
                        i++;
                        string jsonformatedorderinfo =  System.Net.WebUtility.HtmlEncode(System.Net.WebUtility.HtmlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(shoppingcartview)));
                        htmlTable.Append("<tr class='" + ((i % 2 != 0) ? "rgRow" : "rgAltRow") + "'>");
                        htmlTable.Append("<td class=\"olv-cart-sumary\" scope=\"col\">");
                        //htmlTable.Append("<a class=\"lnkCartName\" href=\"javascript:;\"><i class=\"icon-arrow-triangle-right icon-arrow-triangle-down green\"></i>" + shoppingcartview.OrderNumber + "</a>");
                        htmlTable.Append("<i class=\"icon-arrow-triangle-right icon-arrow-triangle-down green\"></i>" + shoppingcartview.OrderNumber + "</a>");
                        htmlTable.Append("<span class=\"lblDate\">" + shoppingcartview.Date + "</span>");
                        htmlTable.Append("<p class=\"hiddenClass pAddress\" style=\"display: none;\">");
                        htmlTable.Append("<span class=\"lblRecipient\">" + shoppingcartview.Recipient + "</span>");
                        htmlTable.Append("<br/>");
                        htmlTable.Append("<span id=\"lblAddressText\">" + GetLocalResourceObject("lblAddressText.Text") + "</span>");
                        htmlTable.Append("<br/>");
                        htmlTable.Append("<span class=\"lblAddress\">" + shoppingcartview.Address + "</span>");
                        htmlTable.Append("</p>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("<td class=\"olv-cart-names\" scope=\"col\">");
                        htmlTable.Append("<span id=\"lblStoreInfo\">" + shoppingcartview.StoreInfo + "</span>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("<td class=\"olv-month\" scope=\"col\">");
                        htmlTable.Append("<span id=\"lblOrderMonth\">" + shoppingcartview.OrderMonth + "</span></td>");
                        htmlTable.Append("<td class=\"olv-status\" scope=\"col\"><span id=\"lblOrderStatus\">" + shoppingcartview.OrderStatus + "</span></td>");
                        if (!DistributorOrderingProfile.IsPC)
                        {
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + shoppingcartview.VolumePoints + "</span></td>");
                        }
                        htmlTable.Append("<td class=\"olv-payment\" scope=\"col\"><span id=\"lblTotalColumn\">" + (shoppingcartview.CartItems.Count > 0 ? shoppingcartview.TotalAmount : shoppingcartview.DonationAmount.ToString()) + "</span></td>");
                        htmlTable.Append("<td class=\"olv-products\" scope=\"col\">" +
                        "<div class=\"productsCont\">" +
                            "<table>" +
                                "<tbody>" +
                                    "<tr>" +
                                        "<th class=\"sv_qty\">" +
                                                "<span id=\"lblQtyColumn\">" + GetLocalResourceObject("QtyResource.Text") + "</span>" +  // globalresource
                                        "</th>" +
                                        "<th class=\"sv_sku\">" +
                                                "<span id=\"lblSKUCoulmn\">" + GetLocalResourceObject("SkuResource.Text") + "</span>" + // globalresource
                                        "</th>" +
                                        "<th class=\"sv_product\">" +
                                                "<span id=\"lblProductColumn\">" + GetLocalResourceObject("ProductResource.Text") + "</span>" + // globalresource
                                        "</th>" +
                                    "</tr>");
                        var itemNumber = 1;

                        if (shoppingcartview.CartItems != null)
                        {
                            foreach (var cartitem in shoppingcartview.CartItems)
                            {

                                if (itemNumber++ > 3)
                                {
                                    htmlTable.Append("<tr class='moreRowsItem" + shoppingcartview.OrderNumber + " hide'>");
                                }
                                else
                                {
                                    htmlTable.Append("<tr>");
                                }

                                htmlTable.Append("<td><span>" + cartitem.Quantity + "</span></td>");
                                htmlTable.Append("<td><span>" + cartitem.SKU + "</span></td>");
                                htmlTable.Append("<td><span>" + cartitem.Description + "</span></td>");
                                htmlTable.Append("</tr>");
                            }
                        }

                        htmlTable.Append("</tbody></table>");

                        if (shoppingcartview.CartItems != null && shoppingcartview.CartItems.Count > 3)
                        {
                            htmlTable.Append("<a class=\"lnkTotalProducts\" href=\"javascript:;\" style=\"display: block;\" show='moreRowsItem" + shoppingcartview.OrderNumber + "'>" + string.Format(GetLocalResourceObject("MoreLinkResource").ToString(), shoppingcartview.CartItems.Count.ToString()) + "</a>");
                        }

                        htmlTable.Append("</div>");
                        htmlTable.Append("</td>");
                        htmlTable.Append("</tr>");
                    }
                }

                htmlTable.Append("</tbody>");
                htmlTable.Append("</table>");
                if (OrderHistoryGrid.HasControls())
                {
                    OrderHistoryGrid.Controls.RemoveAt(0);
                }
                var result = htmlTable.ToString();
                OrderHistoryGrid.Controls.Add(new Literal { Text = result });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        ///     Get the saved carts according the grid parameters.
        /// </summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="maximumRows">Maximum of rows.</param>
        /// <param name="sortExpressions">Sort Expression.</param>
        /// <param name="filterExpressions">Filter Expression.</param>
        /// <param name="startYearMonthDay"></param>
        /// <param name="endYearMonthDay"></param>
        /// <param name="orderStatus"></param>
        /// <returns>Saved carts according the grid parameters.</returns>
        public MyHlShoppingCartViewResult GetPreOrderData(
            int startIndex,
            int maximumRows,
            string sortExpressions,
            string filterExpressions,
            string startYearMonthDay, string endYearMonthDay, string orderStatus)
        {
            try
            {
                var distributorID = string.Empty;
                DistributorOrderingProfile distributorOrderingProfile = null;

                #region determine startDate and endDate
                DateTime? startDate = null;
                if (IsValidDate(startYearMonthDay))
                {
                    startDate = DateTime.ParseExact(startYearMonthDay, YearMonthDayFormat, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    var proposedStartDate = DateTime.Now.AddDays(-30);
                    startDate = new DateTime(proposedStartDate.Year, proposedStartDate.Month, proposedStartDate.Day); //ensure the time is the start of the day, which is 00:00:00
                    //Update UI incase the date have been modified from the above logic.
                    txtStartDate.Text = startDate.Value.ToString(YearMonthDayFormat);
                }

                DateTime? endDate = null;
                if (IsValidDate(endYearMonthDay))
                {
                    endDate = DateTime.ParseExact(endYearMonthDay, YearMonthDayFormat, System.Globalization.CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);
                }
                else
                {
                    endDate = DateTime.Now;
                    //Update UI incase the date have been modified from the above logic.
                    txtEndDate.Text = endDate.Value.ToString(YearMonthDayFormat);
                }
                #endregion

                #region chinaOrdSts
                MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType chinaOrdSts = MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType.All;
                Enum.TryParse<MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType>(orderStatus, out chinaOrdSts);
                #endregion

                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                if (membershipUser != null && membershipUser.Value != null)
                {
                    distributorID = membershipUser.Value.Id;
                    distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorID, "CN");
                }
                else
                {
                    return new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                }

                if (distributorOrderingProfile == null)
                {
                    return new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                }

                var objBL = new MyHLShoppingCartView();
                var cartViewList = new List<MyHLShoppingCartView>();
                string cacheKey =
                   string.Format(PreOrderListCaheKey + distributorID + "CN" + chinaOrdSts + filterExpressions + sortExpressions + startYearMonthDay + endYearMonthDay);

                if (Settings.GetRequiredAppSetting("GetOrderinfoCache") == "true")
                {

                    var result = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCartView>;
                    if (result == null)
                    {
                        cartViewList = objBL.GetPreOrders(distributorID, distributorOrderingProfile.CNCustomorProfileID, Thread.CurrentThread.CurrentCulture.ToString(), startDate, endDate, chinaOrdSts, filterExpressions, sortExpressions);
                        HttpRuntime.Cache.Insert(cacheKey,
                                                 cartViewList,
                                                 null,
                                                 DateTime.Now.AddMinutes(Convert.ToDouble(Settings.GetRequiredAppSetting("GetOrderinfoCacheTimeout"))),
                                                 Cache.NoSlidingExpiration,
                                                 CacheItemPriority.Low,
                                                 null);
                    }
                    else
                    {
                        cartViewList = result;
                    }
                }
                else
                {
                    cartViewList = objBL.GetPreOrders(distributorID, distributorOrderingProfile.CNCustomorProfileID, Thread.CurrentThread.CurrentCulture.ToString(), startDate, endDate, chinaOrdSts, filterExpressions, sortExpressions);
                }

                // Getting the range.
                if (cartViewList != null)
                {
                    int totalRows = cartViewList.Count;
                    var maxRows = startIndex + maximumRows > cartViewList.Count
                                      ? cartViewList.Count - startIndex
                                      : maximumRows;
                    cartViewList = cartViewList.GetRange(maxRows >= 0 ? startIndex : 0, maxRows >= 0 ? maxRows : 0);

                    #region determine HasEventItem?

                    var skuList = CatalogProvider.GetAllSKU(Thread.CurrentThread.CurrentCulture.ToString(), base.CurrentWarehouse);

                    foreach (var cartView in cartViewList)
                    {
                        var cartItems = cartView.CartItems;
                        if (!Helper.GeneralHelper.Instance.HasData(cartItems)) continue;

                        foreach (var cartItem in cartItems)
                        {
                            var m = skuList.FirstOrDefault(x => x.Value.SKU == cartItem.SKU);

                            var mv = m.Value;
                            if (mv == null) continue;

                            var p = mv.Product;
                            cartView.HasEventItem = (p != null) && (p.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket);
                        }
                    }

                    #endregion

                    return new MyHlShoppingCartViewResult(totalRows, cartViewList);
                }

                var retEmpty = new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                return retEmpty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());

                var retErr = new MyHlShoppingCartViewResult(0, new List<MyHLShoppingCartView>());
                retErr.ErrorMessage = ex.ToString();
                return retErr;
            }
        }

        protected void btnOrdering_Click(object sender, EventArgs e)
        {
            ddlOrderStatus.DataSource = OrderStatusList;
            ddlOrderStatus.DataBind();

            ddlOrderBy.SelectedIndex = -1;
            ddlOrderStatus.SelectedIndex = 0;
            txtSearch.Text = "";
            txtStartDate.Text = "";
            txtEndDate.Text = "";

            btnPreOrdering.CssClass = "actionButton";
            btnOrdering.CssClass = "selectedActionButton";
            PagingControl1.CurrentPage = 0;
            _currentPageIndex = 0;

            _isPreOrdering = false;
            GetData(_isPreOrdering);
        }

        protected void btnPreOrdering_Click(object sender, EventArgs e)
        {
            ddlOrderStatus.DataSource = PreOrderStatusList;
            ddlOrderStatus.DataBind();

            ddlOrderBy.SelectedIndex = -1;
            ddlOrderStatus.SelectedIndex = 0;
            txtSearch.Text = "";
            txtStartDate.Text = "";
            txtEndDate.Text = "";

            btnPreOrdering.CssClass = "selectedActionButton";
            btnOrdering.CssClass = "actionButton";
            PagingControl1.CurrentPage = 0;
            _currentPageIndex = 0;

            _isPreOrdering = true;
            GetData(_isPreOrdering);
        }
    }
}