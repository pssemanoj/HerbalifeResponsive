using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class Catalog : ProductsBase
    {
        #region AdobeTarget_Salesforce
        public string AT_categoryName = string.Empty;
        #endregion

        [Publishes(MyHLEventTypes.CrossSellFound)]
        public event EventHandler CrossSellFound;

        [Publishes(MyHLEventTypes.NoCrossSellFound)]
        public event EventHandler NoCrossSellFound;

        [SubscribesTo(MyHLEventTypes.PageVisitRefused)]
        public void PreviousPageRedirected(object sender, EventArgs e)
        {
            var args = e as PageVisitRefusedEventArgs;
            if (null != args)
            {
                if (args.Reason == PageVisitRefusedReason.InvalidDeliveryInfo)
                {
                    (Master as OrderingMaster).ShowMessage(
                        GetLocalResourceObject("MissingShippingInformation") as string, args.Message);
                    //"Missing Shipping Information"
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
            {
                //Response.Redirect("~/ordering/PriceList.aspx?ETO=False", false);
            }
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
            if (!IsPostBack)
            {
                if (Session["showedAPFPopup"] == null) Session["showedAPFPopup"] = false;
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
                        divChinaPCMessageBox.Visible = SessionInfo.IsReplacedPcOrder;
                    }
                }

                if (Request.QueryString["SKU"] != null)
                {
                    if (Request.QueryString["CMP"]!=null)
                       navigateToProductDetailPage(Request.QueryString["SKU"], Request.QueryString["CMP"]);
                   else 
                       navigateToProductDetailPage(Request.QueryString["SKU"],null);
                }

                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);

                // Make sure that a shopping cart exists.
                if (ShoppingCart == null)
                {
                    throw new ApplicationException("ShoppingCart is null. Shopping cart value is required.");
                }

                if (ShoppingCart != null && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("EventTickets.Title") as string);
                    Page.Title = GetLocalResourceObject("EventTickets.Title") as string;
                }
                dataBind();
                //CatchRedirectedEvent();

                DisplayAPFMessage();
                SetNqsMessage();
                // TODO: Please remove this code
                // This is for HFF Modal control testing 
                //if (HL.MyHerbalife.Providers.ConfigurationManagement.HLConfigManager.Configurations.DOConfiguration.AllowHFFModal && this.ShoppingCart.OrderCategory == OrderCategoryType.RSO)
                //{
                //    var _hFFModal = LoadControl("~/Ordering/Controls/HFFModal.ascx") as HFFModal;
                //    this.plHFFModal.Controls.Add(_hFFModal);
                //}
                if (SessionInfo != null && !SessionInfo.IsAPFOrderFromPopUp)
                    SessionInfo.IsAPFOrderFromPopUp = false;
                var allowedCountries = HL.Common.Configuration.Settings.GetRequiredAppSetting("AllowAPFPopupForStandAloneContries", "CH");
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
            else
            {
                findAndSetFirstRootCategory(IsEventTicketMode);
            }
            if (null != ShoppingCart && null != ShoppingCart.DeliveryInfo)
            {
                hfWarehouseCode.Value = string.Format("Warehouse Code : {0}", ShoppingCart.DeliveryInfo.WarehouseCode);
            }
            DisplayELearningMessage();
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
        }

        private void navigateToProductDetailPage(string sku, string cmp)
        {
            Dictionary<string, SKU_V01> allSKUs = CatalogProvider.GetAllSKU(Locale, base.CurrentWarehouse);
            if (allSKUs != null)
            {
                SKU_V01 sku01;
                if (allSKUs.TryGetValue(sku, out sku01))
                {
                    if (sku01.Product != null)
                    {
                        Category_V02 category = findCategoryByProductID(ProductInfoCatalog.RootCategories,
                                                                        sku01.Product.ID);
                        if (category != null)
                        {
                           if (cmp != null)
                                Response.Redirect(
                            "~/ordering/ProductDetail.aspx?ProdInfoID=" + sku01.Product.ID + "&CategoryID=" +
                            category.ID+"&CMP="+cmp, false);
                          else
                            Response.Redirect(
                                "~/ordering/ProductDetail.aspx?ProdInfoID=" + sku01.Product.ID + "&CategoryID=" +
                                category.ID, false);
                        }
                    }
                }
            }
        }

        private Category_V02 findCategoryByProductID(List<Category_V02> categoryList, int prodID)
        {
            Category_V02 cat = null;
            foreach (Category_V02 category in categoryList)
            {
                if (category.SubCategories != null)
                {
                    if (category.Products != null && category.Products.Exists(p => p.ID == prodID))
                    {
                        return category;
                    }
                    if ((cat = findCategoryByProductID(category.SubCategories, prodID)) != null)
                        return cat;
                }
                else
                {
                    if (category.Products != null && category.Products.Exists(p => p.ID == prodID))
                    {
                        return category;
                    }
                }
            }
            return cat;
        }

        private void getIDS(out int categoryID, out int rootCategoryID, out int parentCategoryID)
        {
            categoryID = rootCategoryID = parentCategoryID = 0;
            string cid = Request.QueryString["cid"];
            string rootID = Request.QueryString["root"];
            string parentID = Request.QueryString["parent"];
            categoryID = string.IsNullOrEmpty(cid) ? 0 : int.Parse(cid);
            rootCategoryID = string.IsNullOrEmpty(rootID) ? 0 : int.Parse(rootID);
            parentCategoryID = string.IsNullOrEmpty(parentID) ? 0 : int.Parse(parentID);
        }

        private Category_V02 findAndSetFirstRootCategory(bool isEventTicketMode)
        {
            Category_V02 current = null;
            int categoryID, rootCategoryID, parentCategoryID = 0;
            getIDS(out categoryID, out rootCategoryID, out parentCategoryID);
            if (categoryID == 0 && rootCategoryID == 0)
            {
                current = findRootCategory(isEventTicketMode, ProductInfoCatalog.RootCategories);
                if (current != null)
                {
                    SubCat.PagerCategoryID = current.ID;
                    SubCat.PagerRootCategoryID = current.ID;
                }
            }
            return current;
        }

        private void resetPage()
        {
            Category_V02 current = findRootCategory(false, ProductInfoCatalog.RootCategories);
            if (current != null)
            {
                ShowCategory(current.ID, current.ID, 0);
                findCrossSell(current.ID);
            }
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            upCategoryInfo.Update();
        }

        private void dataBind()
        {
            int categoryID, rootCategoryID, parentCategoryID = 0;
            getIDS(out categoryID, out rootCategoryID, out parentCategoryID);

            if (categoryID != 0 && rootCategoryID != 0)
            {
                ShowCategory(categoryID, rootCategoryID, parentCategoryID);
                findCrossSell(categoryID);
            }
            else
            {
                if (ProductInfoCatalog != null && !IsPostBack)
                {
                    if (ProductInfoCatalog.RootCategories != null && ProductInfoCatalog.RootCategories.Count > 0)
                    {
                        Category_V02 current = findAndSetFirstRootCategory(IsEventTicketMode);
                        if (current != null)
                        {
                            

                            if ((IsChina && SessionInfo.IsEventTicketMode && SessionInfo.IsReplacedPcOrder) || (IsChina && SessionInfo.IsEventTicketMode ))
                            {
                                var rsp = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetEventEligibility(this.DistributorID);
                                if (rsp != null && !rsp.IsEligible)
                                {
                                    switch (rsp.Remark)
                                    {
                                        case "Invalid":
                                            (Master as OrderingMaster).ShowMessage(base.GetLocalResourceString("EventTicket"),
                                                                               base.GetLocalResourceString(
                                                                                   "NoValidEvent"));
                                            break;
                                        case "StartDateGT":
                                            (Master as OrderingMaster).ShowMessage(base.GetLocalResourceString("EventTicket"),
                                                                               base.GetLocalResourceString(
                                                                                   "YouAreEligibleForEventPeriod"));
                                            break;
                                        case "InvalidEventPeriod":
                                        case "InvalidEventPeriodNoSKU":
                                            (Master as OrderingMaster).ShowMessage(base.GetLocalResourceString("EventTicket"),
                                                                               base.GetLocalResourceString(
                                                                                   "InvalidEventPeriod"));
                                            break;
                                        case "SKUOutOfStock":
                                            (Master as OrderingMaster).ShowMessage(base.GetLocalResourceString("EventTicket"),
                                                                              base.GetLocalResourceString(
                                                                                  "SKUOutOfStock"));
                                            break;
                                    }
                                    // at this moment we knew thet there us no product id or anything else therefore we hide the frame
                                    NoCrossSellFound(this, null);
                                }
                                else
                                {
                                    ShowCategory(current.ID, current.ID, 0);
                                    findCrossSell(current.ID);
                                }
                            }
                            else
                            {
                                ShowCategory(current.ID, current.ID, 0);
                                findCrossSell(current.ID);
                            }
                        }
                        else
                        {
                            if (IsEventTicketMode)
                            {
                                (Master as OrderingMaster).ShowMessage(base.GetLocalResourceString("EventTicket"),
                                                                       base.GetLocalResourceString(
                                                                           "EventTicketOderingNotAvailable"));
                                SessionInfo.HasEventTicket = false;
                                SessionInfo.IsEventTicketMode = false;
                                // at this moment we knew thet there us no product id or anything else therefore we hide the frame
                                if(IsChina)
                                NoCrossSellFound(this, null);
                            }
                        }
                    }
                }
            }
        }

        private Category_V02 findRootCategory(bool eventTicketMode, List<Category_V02> rootCategories)
        {
            Category_V02 current = null;

            foreach (Category_V02 category in rootCategories)
            {
                // Validate category display by rule
                if (CatalogProvider.IsDisplayable(category, Locale))
                {
                    if (Menu.ShouldTake(category, eventTicketMode, SessionInfo.ShowAllInventory, SessionInfo.IsHAPMode))
                    {
                        current = category;
                        break;
                    }
                }
            }

            return current;
        }

        private void findCrossSell(int id)
        {
            if (ProductInfoCatalog != null)
            {
                var crossSellList = new List<CrossSellInfo>();
                ProductDetail.CollectAllCrossSellProducts(CatalogHelper.FindCategory(ProductInfoCatalog, id),
                                                          crossSellList);

                if (crossSellList.Count > 0)
                {
                    CrossSellInfo candidate = null;
                    foreach (CrossSellInfo c in crossSellList)
                    {
                        if (ProductDetail.ShouldSelectThisCrossSell(c.Product, ShoppingCart,
                                                                    Session[ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY]
                                                                    as CrossSellInfo,
                                                                    Session[
                                                                        ProductDetail
                                                                            .LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY]
                                                                    as CrossSellInfo))
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
                                                                                                       ProductDetail
                                                                                                           .LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
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
            }
            else
            {
                LoggerHelper.Error(string.Format("ProductInfoCatalog is null in finding cross-sell : {0}", this.Locale));
            }
            NoCrossSellFound(this, null);
        }

        public void ShowCategory(int id, int rootID, int parentID)
        {
            if (ProductInfoCatalog != null)
            {
                Category_V02 category = CatalogHelper.FindCategory(ProductInfoCatalog, id, parentID, rootID);
                Category_V02 rootCategory = CatalogHelper.FindCategory(ProductInfoCatalog, rootID);
                if (category != null 
                    && rootCategory != null
                    && CatalogProvider.IsDisplayable(rootCategory, Thread.CurrentThread.CurrentCulture.Name))
                {
                    CategoryName.Text = category.DisplayName;
                    Overview.Text = category.Description;
                    DivImage.Style["background"] = "url(" + category.ImagePath +
                                                   ");background-repeat: no-repeat; background-position: center right";
                    SubCat.PopulateProducts(category, rootCategory);

                    if (HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations)
                    {
                        // Send data to Adobe Target and Salesforce, only for main categories
                        if (category.ID == rootCategory.ID)
                        {
                            AT_categoryName = string.Format("{0}_{1}", Locale, category.DisplayName);
                        }
                    }
                }
                else
                {
                    if (!(id == 0 && rootID == 0 && parentID == 0))
                    {
                        LoggerHelper.Warn(string.Format("No category found in ShowCategory for {0}-{2}-{3} {1}", id, this.Locale,rootID,parentID));
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ShowAllInventory)]
        public void OnShowAllInventory(object sender, EventArgs e)
        {
            try
            {
                int categoryID, rootCategoryID, parentCategoryID = 0;
                getIDS(out categoryID, out rootCategoryID, out parentCategoryID);

                ShowCategory(categoryID, rootCategoryID, parentCategoryID);
            }
            catch
            {
            }
        }

        [SubscribesTo(MyHLEventTypes.ShowAvailableInventory)]
        public void OnShowAvailableInventory(object sender, EventArgs e)
        {
            try
            {
                int categoryID, rootCategoryID, parentCategoryID = 0;
                getIDS(out categoryID, out rootCategoryID, out parentCategoryID);

                ShowCategory(categoryID, rootCategoryID, parentCategoryID);
            }
            catch
            {
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            try
            {
                int categoryID, rootCategoryID, parentCategoryID = 0;
                getIDS(out categoryID, out rootCategoryID, out parentCategoryID);

                ShowCategory(categoryID, rootCategoryID, parentCategoryID);
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
                int categoryID, rootCategoryID, parentCategoryID = 0;
                getIDS(out categoryID, out rootCategoryID, out parentCategoryID);
                findCrossSell(categoryID);
            }
            catch
            {
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

        //private void CatchRedirectedEvent()
        //{
        //    if (null != Session[OrderingMaster.SessionRedirectKey])
        //    {
        //        PageVisitRefusedEventArgs args = Session[Ordering.MasterPage.OrderingMaster.SessionRedirectKey] as PageVisitRefusedEventArgs;
        //        if (null != args)
        //        {
        //            if (args.Reason == PageVisitRefusedReason.InvalidDeliveryInfo)
        //            {
        //                HL.MyHerbalife.Providers.Ordering.Interfaces.IShippingProvider Provider = GetShippingProvider();
        //                List<DeliveryOption> options = Provider.GetShippingAddresses(DistributorID, Locale);
        //                if (null != options && options.Count > 0)
        //                {
        //                    //DeliveryOption option = options.Find(delegate(DeliveryOption p) { return p.IsPrimary; });
        //                    //if (option != null)
        //                    //{
        //                    //    ShoppingCart.
        //                    //}
        //                    (this.Master as OrderingMaster).Status.AddMessage( Web.Controls.Content.StatusMessageType.Error, base.GetLocalResourceString("CompleteDeliveryInfo"));
        //                }
        //                else
        //                {
        //                    base.NoSavedAddress();
        //                }
        //                Session[OrderingMaster.SessionRedirectKey] = null;
        //            }
        //        }
        //    }
        //}
    }
}