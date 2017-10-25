using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Web;
using System.Web.Services;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class Menu : UserControlBase
    {
        private SessionInfo _sessionInfo;

        public Menu()
        {
            DisplayOrderPreferenceSubmenu = false;
        }

        public bool DisplayProducts { get; set; }
        public bool DisplayOrderPreferenceSubmenu { get; set; }
        
        [WebMethod]
        public static bool GetNeverShowAgainSession()
        {
            try
            {
                return HttpContext.Current.Session["OnlineOrderShowOnce"] != null && ((bool)HttpContext.Current.Session["OnlineOrderShowOnce"]);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        [WebMethod(EnableSession = true)]
        public static bool SetNeverShowAgain(bool bolRemoveSession)
        {
            try
            {
                if (bolRemoveSession)
                {
                    HttpContext.Current.Session["OnlineOrderShowOnce"] = false;
                }
                else
                {
                    HttpContext.Current.Session["OnlineOrderShowOnce"] = true;
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _sessionInfo = SessionInfo;
            if (DisplayProducts == false)
            {
                CatetoryMenu.Visible = ApparelMenu.Visible = false;                    
            }                
            else
            {
                if (ProductInfoCatalog != null)
                {
                    SetCategoryMenu();
                }
            }
            if (!IsPostBack)
            {
                liMenuSearchProduct.Visible = HLConfigManager.Configurations.DOConfiguration.EnableSearch;
                liMenuFAQ.Visible = HLConfigManager.Configurations.DOConfiguration.HasFAQ;

                string eventTicketURL = HLConfigManager.Configurations.DOConfiguration.EventTicketUrl;
                string eventTicketURLTarget = HLConfigManager.Configurations.DOConfiguration.EventTicketUrlTarget;
                if (HLConfigManager.Configurations.DOConfiguration.AllowEventPurchasing)
                {
                    liMenuEventTickets.Visible = !_sessionInfo.IsEventTicketMode;
                    liMenuOrderProducts.Visible = _sessionInfo.IsEventTicketMode;
                }
                else
                {
                    if (!string.IsNullOrEmpty(eventTicketURL))
                    {
                        liMenuEventTickets.Visible = true;
                        liMenuOrderProducts.Visible = false;
                        MenuEventTickets.NavigateUrl = eventTicketURL;
                        MenuEventTickets.Target = eventTicketURLTarget;
                    }
                    else
                    {
                        liMenuEventTickets.Visible = liMenuOrderProducts.Visible = false;
                    }
                }

                MenuProductCatalog.Text = getMenuText("MenuProductCatalog");
                MenuOnlinePriceList.Text = getMenuText("MenuOnlinePriceList");
                MenuOrderBySKU.Text = getMenuText("MenuOrderBySKU");

                MenuProductCatalog.NavigateUrl = getMenuLink("~/Ordering/Catalog.aspx");
                MenuOnlinePriceList.NavigateUrl = getMenuLink("~/Ordering/PriceList.aspx");
                MenuOrderBySKU.NavigateUrl = getMenuLink("~/Ordering/ProductSKU.aspx");

                if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
                {
                    liMenuCatalog.Visible = false;
                    toggleMe.Visible = false;

                    if (CountryCode == "CN")
                    {
                        MenuEventTickets.NavigateUrl = "~/Ordering/Pricelist.aspx?ETO=TRUE";
                    }

                }

                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFUrl) &&
                    HLConfigManager.Configurations.DOConfiguration.ShowHFFLinkOnMenu && (!_sessionInfo.IsEventTicketMode || HLConfigManager.Configurations.DOConfiguration.ShowHFFLinkOnETO))
                {
                    liMenuHFFUrl.Visible = true;
                    MenuHFFUrl.NavigateUrl = HLConfigManager.Configurations.DOConfiguration.HFFUrl;
                }
                else
                {
                    liMenuHFFUrl.Visible = false;
                }

                PanelOrderPreferenceSubMenu.Visible = DisplayOrderPreferenceSubmenu;
                MenuSavedPickupLocation.Visible = HLConfigManager.Configurations.DOConfiguration.HasPickupPreference;
                MenuSavedPaymentInformation.Visible =
                    HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards;
                liMenuSavedPickupLocation.Visible = HLConfigManager.Configurations.DOConfiguration.HasPickupPreference;
                liMenuSavedPUFromCourierLocation.Visible =
                    HLConfigManager.Configurations.DOConfiguration.HasPickupFromCourierPreference;
                liMenuSavedPaymentInformation.Visible =
                    HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards;
                liMenuSavedShippingAddress.Visible = HLConfigManager.Configurations.DOConfiguration.AllowShipping;

                //liMenuHFFUrl.Visible = !_sessionInfo.IsEventTicketMode;
                liMenuSearchProduct.Visible = HLConfigManager.Configurations.DOConfiguration.EnableSearch &&
                                              !_sessionInfo.IsEventTicketMode;
                if (!HLConfigManager.Configurations.DOConfiguration.LoadFAQMenuFromHTML)
                {
                    if (CountryCode == "CN")
                    {
                        MenuFAQ.Visible = true;
                        MenuFAQ.Target = "_self";
                        MenuFAQ.NavigateUrl = "~/Ordering/Faq.aspx";
                        
                    }
                    else
                    {
                        MenuFAQ.Visible = true;
                        MenuFAQ.Target = "_blank";
                        MenuFAQ.NavigateUrl = string.Format("/content/{0}/pdf/{1}", CultureInfo.CurrentCulture.Name,
                                                            (_sessionInfo.IsHAPMode ? "ordering/HAP_FAQ.pdf" : "products/GDO_FAQs.pdf"));
                        crAdditionalItems.Visible = false;
                    }
                }
                else
                {
                    MenuFAQ.Visible = false;
                    crAdditionalItems.Visible = true;
                }

                liMenuOrderHistoryUrl.Visible = HLConfigManager.Configurations.DOConfiguration.HasOrderHistory;
                liMenuSavedCarts.Visible = HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts;
                liMenuPreference.Visible = HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCardsWithAddress;
                liMenuPendingOrders.Visible = HLConfigManager.Configurations.DOConfiguration.AllowPendingOrders;
                liMenuFreightSimulation.Visible = HLConfigManager.Configurations.DOConfiguration.AllowFreightSimulation;
                liMenuOrderListView.Visible = HLConfigManager.Configurations.DOConfiguration.HasMyOrder;

                if (SessionInfo.IsHAPMode)
                {
                    liMenuPreference.Visible = false;
                    liMenuSavedCarts.Visible = false;
                    liMenuEventTickets.Visible = false;
                    liMenuHFFUrl.Visible = false;
                }

                //Apparel Category
                string apparelCat = HLConfigManager.Configurations.DOConfiguration.ApparelCategoryName;                
                if (string.IsNullOrEmpty(apparelCat))
                {
                    ApparelMenu.DataSource = null;
                    ApparelMenu.Visible = false;
                    liMenuApparelAndAccessories.Visible = false;
                }
                else
                {
                    MenuApparelAndAccessories.Text = apparelCat;
                    SetCategoryMenu();
                }
                
            }
            
        }

        protected string getMenuText(string key)
        {
            return !_sessionInfo.IsEventTicketMode
                       ? GetLocalResourceObject(key) as string
                       : GetLocalResourceObject(string.Format("{0}{1}", key, "Event")) as string;
        }

        protected string getMenuLink(string url)
        {
            if (!SessionInfo.IsHAPMode)
            {
            return string.Format("{0}?ETO={1}", url, !_sessionInfo.IsEventTicketMode ? "FALSE" : "TRUE");
        }
            else
            {
                return string.Format("{0}?HAP={1}", url, !_sessionInfo.IsHAPMode ? "FALSE" : "TRUE");
            }
        }

        protected void topLeveCategoryDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var rootItem = e.Item.DataItem as Category_V02;
                if (rootItem != null)
                {
                    int level = 1;
                    var hl = (HyperLink) e.Item.FindControl("topLevelCatetory");
                    var i = (HtmlGenericControl)e.Item.FindControl("iconArrow");
                    if (hl != null)
                    {
                        hl.Text = rootItem.DisplayName;
                        if (IsChina && SessionInfo.IsEventTicketMode)
                        {
                            hl.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?ETO=TRUE");

                        }
                        else 
                        {
                            hl.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}",
                                                      rootItem.ID, rootItem.ID, 0);
                        }
                       
                        var pl = (PlaceHolder) e.Item.FindControl("uxSubCategory");
                        if (pl != null)
                        {
                            populateSubCategories(pl, rootItem, level, rootItem, i);
                        }
                    }
                }
            }
        }

        protected void topLeveApparelDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var rootItem = e.Item.DataItem as Category_V02;
                var apparelCategoryHolder = (PlaceHolder)e.Item.FindControl("apparelCategoryPlaveHolder");

                MenuApparelAndAccessories.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}", rootItem.ID, rootItem.ID, 0);

                if (rootItem.SubCategories != null)
                {
                    foreach (Category_V02 apparelCategory in rootItem.SubCategories)
                    {
                        int level = 1;

                        if (apparelCategoryHolder != null)
                        {
                            var hl = new HyperLink();
                            var li = new HtmlGenericControl("li");
                            var i = new HtmlGenericControl("i");

                            hl.Text = apparelCategory.DisplayName;
                            hl.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}", apparelCategory.ID, rootItem.ID, 0);
                            li.Attributes["class"] = "gdo-left-nav-level2 toggle-parent";
                            i.Attributes["class"] = "arrow icon icon-add-ln-1";

                            li.Controls.Add(hl);
                            li.Controls.Add(i);
                            apparelCategoryHolder.Controls.Add(li);

                            var ulCntrl = new HtmlGenericControl("ul");
                            ulCntrl.Attributes["class"] = "toggle-child";

                            if (apparelCategory.SubCategories != null)
                            {
                                foreach (Category_V02 item in apparelCategory.SubCategories)
                                {
                                    if (ShouldTake(item, SessionInfo.IsEventTicketMode, _sessionInfo.ShowAllInventory, _sessionInfo.IsHAPMode))
                                    {
                                        var s_li = new HtmlGenericControl("li");
                                        var item_link = new HyperLink(); //Sub Category Hyperlink

                                        s_li.Attributes["class"] = "gdo-left-nav-level" + (level + 1).ToString();

                                        item_link.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}", item.ID, rootItem.ID, apparelCategory.ID);
                                        item_link.Text = item.DisplayName;

                                        s_li.Controls.Add(item_link);

                                        ulCntrl.Controls.Add(s_li);
                                    }
                                }
                            }
                            else
                            {
                                if (i != null) 
                                {
                                    i.Visible = false;
                                }
                            }
                            apparelCategoryHolder.Controls.Add(ulCntrl);

                        }
                    }

                }
            }
        }

        public static bool ShouldTake(ProductInfo_V02 prod, bool bEventTicket, bool showAllInventory, bool isHAP)
        {
            bool bValid = bEventTicket
                              ? (prod.TypeOfProduct == ProductType.EventTicket)
                              : (prod.TypeOfProduct != ProductType.EventTicket);

            if (IsChina && bEventTicket && bValid)
            {
                var distributorId = System.Web.HttpContext.Current.User.Identity.Name;
                //if (!string.IsNullOrWhiteSpace(distributorId))
                //    bValid = MyHerbalife3.Ordering.Providers.China.OrderProvider.IsEligibleForEvents(distributorId);
            }

            bValid = isHAP 
                ? (prod.TypeOfProduct == ProductType.Product) 
                : bValid;

            if (bValid)
            {
                //bValid = prod.SKUs != null && (showAllInventory ? true : !prod.SKUs.Any(s => s.ProductAvailability == ProductAvailabilityType.Unavailable || s.CatalogItem == null ));
                bValid = prod.SKUs != null && prod.SKUs.Any(s => s.IsDisplayable) &&
                         (showAllInventory
                              ? true
                              : prod.SKUs.Any(
                                  s =>
                                  s.ProductAvailability != ProductAvailabilityType.Unavailable && s.CatalogItem != null));
            }
            return bValid;
        }

        public static bool ShouldTake(Category_V02 category, bool bEventTicket, bool showAllInventory, bool isHAP, string action = "", List<string> categoryList = null)
        {
            // === Exclude Categories
            if (categoryList != null)
            {
                if (action == "exclude" && categoryList.Contains(category.DisplayName))
                {
                    return false;
                }
            }

            if (category.Products != null)
            {
                foreach (ProductInfo_V02 prod in category.Products)
                {
                    if (ShouldTake(prod, bEventTicket, showAllInventory, isHAP))
                        return true;
                }
            }
            else
            {
                // this is 
                if (category.SubCategories == null)
                {
                    return false;
                }
            }
            if (category.SubCategories != null)
            {
                // === Include Only Categories
                if (categoryList != null)
                {
                    if (action == "display" && !categoryList.Contains(category.DisplayName))
                    {
                        return false;
                    }
                }
                foreach (Category_V02 sub in category.SubCategories)
                {
                    if (ShouldTake(sub, bEventTicket, showAllInventory, isHAP))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void populateSubCategories(Control pl, Category_V02 category, int level, Category_V02 rootCategory, Control i = null)
        {
            if (ProductInfoCatalog != null)
            {
                bool hasSubMenuItems = false;

                var sessionInfo = SessionInfo;

                int parentCatID = category.ID;
                // don't display the last level 
                var subCategoryList = category.SubCategories;

                //var liCtr = new HtmlGenericControl("li");
                //liCtr.Attributes["class"] = "gdo-left-nav-divider";
                
                level++;
                if (subCategoryList != null && subCategoryList.Count > 0)
                {
                    var ulCntrl = new HtmlGenericControl("ul");

                    foreach (Category_V02 item in subCategoryList)
                    {
                        //int subLevel = level;
                        if (ShouldTake(item, sessionInfo.IsEventTicketMode, _sessionInfo.ShowAllInventory, sessionInfo.IsHAPMode ))
                        {
                            hasSubMenuItems = true;

                            var liCntrl = new HtmlGenericControl("li");
                            //liCntrl.Attributes["class"] = subLevel == 2 ? "gdo-left-nav-level2" : "gdo-left-nav-level3";
                            liCntrl.Attributes["class"] = "gdo-left-nav-level" + (level + 1).ToString();
                            var hl = new HyperLink();
                            hl.NavigateUrl = string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}",
                                                           item.ID, rootCategory.ID, parentCatID);
                            hl.Text = item.DisplayName + "<br>";
                            liCntrl.Controls.Add(hl);
                            ulCntrl.Controls.Add(liCntrl);
                            ulCntrl.Attributes["class"] = "toggle-child hide";
                            populateSubCategories(liCntrl, item, level + 1, rootCategory);
                        }
                    }
                    pl.Controls.Add(ulCntrl);
                }
                else
                {
                    if (i != null) 
                    {
                        i.Visible = false;
                    }
                    
                }

                /*if (hasSubMenuItems)
                {
                    pl.Controls.Add(liCtr);
                }

                if (level == 2 && hasSubMenuItems == false)
                {
                    pl.Controls.Add(liCtr);
                }*/
            }
        }

        protected void ShoppingCartLink_Click(object sender, EventArgs e)
        {
            if (((Page) as ProductsBase).CantBuy)
            {
                //this.errDSFraud.Visible = true;
                //this.errDSFraud.Text = MyHL_ErrorMessage.CantOrder;
                (Master).CantBuyCantVisitShoppingCart = true;
            }
            else
            {
                Response.Redirect("~/Ordering/ShoppingCart.aspx");
                errDSFraud.Visible = false;
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            try
            {
                if (ProductInfoCatalog != null)
                {
                    SetCategoryMenu();
                    UpdatePanelMenu.Update();
                }
            }
            catch
            {
            }
        }

        [SubscribesTo(MyHLEventTypes.ShowAllInventory)]
        public void OnShowAllInventory(object sender, EventArgs e)
        {
            try
            {
                if (ProductInfoCatalog != null)
                {
                    SetCategoryMenu();
                    UpdatePanelMenu.Update();
                }
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
                if (ProductInfoCatalog != null)
                {
                    SetCategoryMenu();
                    UpdatePanelMenu.Update();
                }
            }
            catch
            {
            }
        }

        private void SetCategoryMenu()
        {
            List<string> apparelCategories = HLConfigManager.Configurations.DOConfiguration.ApparelCategoryName.Split(',').ToList();
                        
            CatetoryMenu.DataSource = from r in ProductInfoCatalog.RootCategories
                                        where
                                            ShouldTake(r, _sessionInfo.IsEventTicketMode,
                                                        _sessionInfo.ShowAllInventory, _sessionInfo.IsHAPMode, "exclude", apparelCategories) &&
                                            CatalogProvider.IsDisplayable(r, Locale)
                                        select r;
            CatetoryMenu.DataBind();

            if (apparelCategories != null)
            {
                string apparelCat = apparelCategories.FirstOrDefault();
                if (!string.IsNullOrEmpty(apparelCat))
                {
                    ApparelMenu.DataSource = from r in ProductInfoCatalog.RootCategories
                                             where
                                                 ShouldTake(r, _sessionInfo.IsEventTicketMode,
                                                             _sessionInfo.ShowAllInventory, _sessionInfo.IsHAPMode, "display", apparelCategories) &&
                                                 CatalogProvider.IsDisplayable(r, Locale)
                                             select r;
                    ApparelMenu.DataBind();
                }
                else
                {
                    ApparelMenu.DataSource = null;
                    ApparelMenu.Visible = false;
                    liMenuApparelAndAccessories.Visible = false;
                }
            }
        }

        private static bool IsChina
        {
            get
            {
                return HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
        }
    }
}