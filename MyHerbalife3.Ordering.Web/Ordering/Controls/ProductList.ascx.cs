// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductList.ascx.cs" company="Herbalife">
//   Copyright (c) Herbalife 2010.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Resources;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using HL.Common.Configuration;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    /// <summary>
    ///     The ui product info.
    /// </summary>
    public class UIProductInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UIProductInfo" /> class.
        /// </summary>
        /// <param name="catID">
        ///     The cat id.
        /// </param>
        /// <param name="imageName">
        ///     The image name.
        /// </param>
        /// <param name="description">
        ///     The description.
        /// </param>
        /// <param name="productID">
        ///     The product id.
        /// </param>
        /// <param name="price">
        ///     The price.
        /// </param>
        /// <param name="sortOrder">
        ///     The sort order.
        /// </param>
        public UIProductInfo(int catID, string imageName, string description, int productID, string price, int sortOrder)
        {
            CategoryID = catID;
            ThumbnailImageName = imageName ?? string.Empty;
            Description = description;
            ProductID = productID;
            Price = price;
            SortOrder = sortOrder;
        }

        /// <summary>
        ///     Gets or sets CategoryID.
        /// </summary>
        public int CategoryID { get; set; }

        /// <summary>
        ///     Gets or sets ThumbnailImageName.
        /// </summary>
        public string ThumbnailImageName { get; set; }

        /// <summary>
        ///     Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets ProductID.
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        ///     Gets or sets Price.
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        ///     Gets or sets SortOrder.
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    ///     The category data pager.
    /// </summary>
    public class CategoryDataPager : DataPager
    {
        /// <summary>
        ///     The create pager fields.
        /// </summary>
        protected override void CreatePagerFields()
        {
            if (PageSize != int.MaxValue)
            {
                PageSize = (NamingContainer as ProductList).CategoryPageSize;
            }

            base.CreatePagerFields();
        }

        /// <summary>
        ///     The find control in.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        /// <param name="controlName">
        ///     The control name.
        /// </param>
        /// <returns>
        /// </returns>
        private Control FindControlIn(Control control, string controlName)
        {
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.ID == controlName)
                    return ctrl;

                Control f = FindControlIn(ctrl, controlName);
                if (f != null)
                    return f;
            }

            return null;
        }

        protected override void OnInit(EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ispage"]))
                {
                    string pagenumber = HttpContext.Current.Request.QueryString["ispage"];
                    int pageNumber;
                    bool isnumber = int.TryParse(pagenumber, out pageNumber);
                    if (isnumber)
                    {
                        if (pageNumber <= 0)
                        {
                            HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsolutePath);
                        }
                    }
                }
            }
            base.OnInit(e);
        }

        /// <summary>
        ///     The set page properties.
        /// </summary>
        /// <param name="startRowIndex">
        ///     The start row index.
        /// </param>
        /// <param name="maximumRows">
        ///     The maximum rows.
        /// </param>
        /// <param name="databind">
        ///     The databind.
        /// </param>
        public override void SetPageProperties(int startRowIndex, int maximumRows, bool databind)
        {
            if (maximumRows != Int32.MaxValue)
            {
                var subCategories = NamingContainer as ProductList;
                int pageSize = subCategories.CategoryPageSize;
                string pageNumber;
                if ((pageNumber = subCategories.Request.QueryString["ispage"]) != null)
                {
                    //ensure that page can never be lower than zero
                    int page = int.Parse(pageNumber) - 1;
                    page = (page < 0) ? 0 : page;
                    base.SetPageProperties(PageSize*page, pageSize, databind);
                }
                else
                {
                    base.SetPageProperties(startRowIndex, maximumRows, databind);
                }
            }
            else
            {
                CategoryNumericPagerField.disableShowAll(Controls[0]);
                base.SetPageProperties(startRowIndex, maximumRows, databind);
            }
        }
    }

    /// <summary>
    ///     The category numeric pager field.
    /// </summary>
    public class CategoryNumericPagerField : NumericPagerField
    {
        /// <summary>
        ///     The page size.
        /// </summary>
        public int PageSize;

        /// <summary>
        ///     The create data pagers.
        /// </summary>
        /// <param name="container">
        ///     The container.
        /// </param>
        /// <param name="startRowIndex">
        ///     The start row index.
        /// </param>
        /// <param name="maximumRows">
        ///     The maximum rows.
        /// </param>
        /// <param name="totalRowCount">
        ///     The total row count.
        /// </param>
        /// <param name="fieldIndex">
        ///     The field index.
        /// </param>
        public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows,
                                              int totalRowCount,
                                              int fieldIndex)
        {
            PageSize = (container.TemplateControl as ProductList).CategoryPageSize;
            base.CreateDataPagers(container, startRowIndex, PageSize, totalRowCount, fieldIndex);
            if (maximumRows == Int32.MaxValue)
            {
                // show all
                if (DataPager.Controls.Count > 2)
                {
                    // find show all button and disable it
                    disableShowAll(DataPager.Controls[0]);

                    // recreate "1" as LinkButton
                    createNumber1(DataPager.Controls[2], (container.TemplateControl as ProductList).Request);
                }
            }

            if (DataPager.Controls.Count > 2)
            {
                // hide Back button
                hideBackButton(DataPager.Controls[1]);
            }

            string pageNumber;
            if ((pageNumber = (container.TemplateControl as ProductList).Request.QueryString["ispage"]) != null)
            {
                int page = int.Parse(pageNumber);

                showPageNumber(DataPager.Controls[0], page);
            }
        }

        /// <summary>
        ///     The show page number.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        /// <param name="page">
        ///     The page.
        /// </param>
        private static void showPageNumber(Control control, int page)
        {
            ControlCollection col = control.Controls;
            foreach (Control ctrl in col)
            {
                if (ctrl is Label && ctrl.ID.Contains("lblNumPage"))
                {
                    (ctrl as Label).Text = page.ToString();
                    break;
                }
            }
        }


        /// <summary>
        ///     The disable show all.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        public static void disableShowAll(Control control)
        {
            ControlCollection col = control.Controls;
            foreach (Control ctrl in col)
            {
                if (ctrl is LinkButton && ctrl.ID.Contains("ShowAll"))
                {
                    (ctrl as LinkButton).Enabled = false;
                    (ctrl as LinkButton).Attributes["ShowAllDisabled"] = "true";
                    break;
                }
            }
        }

        /// <summary>
        ///     The create number 1.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        /// <param name="request">
        ///     The request.
        /// </param>
        private static void createNumber1(Control control, HttpRequest request)
        {
            ControlCollection col = control.Controls;
            foreach (Control ctrl in col)
            {
                if (ctrl is Label)
                {
                    var lb = ctrl as Label;
                    if (lb.Text == "1")
                    {
                        var lbt = new HyperLink();
                        string queryString = request.QueryString["ispage"];
                        if (queryString != null)
                        {
                            NameValueCollection coll = request.QueryString;
                            string[] arr = coll.AllKeys;
                            var myStringBuilder = new StringBuilder();
                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i] != "ispage")
                                {
                                    myStringBuilder.AppendFormat("{0}={1}&", arr[i], coll[arr[i]]);
                                }
                            }

                            if (ctrl.Page is ProductDetail)
                            {
                                lbt.NavigateUrl = string.Format("{0}?{1}ispage=1", "~/Ordering/ProductDetail.aspx",
                                                                myStringBuilder);
                            }
                            else
                            {
                                lbt.NavigateUrl = string.Format("{0}?{1}ispage=1", "~/Ordering/Catalog.aspx",
                                                                myStringBuilder);
                            }
                        }
                        else
                        {
                            if (ctrl.Page is ProductDetail)
                            {
                                lbt.NavigateUrl = string.Format("{0}?{1}&ispage=1", "~/Ordering/ProductDetail.aspx",
                                                                request.QueryString);
                            }
                            else
                            {
                                lbt.NavigateUrl = string.Format("{0}?{1}&ispage=1", "~/Ordering/Catalog.aspx",
                                                                request.QueryString);
                            }
                        }

                        lbt.Text = "1";

                        // lbt.CommandArgument = "1";
                        // lbt.CommandName = "0";
                        col.AddAt(0, lbt);
                        col.Remove(lb);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     The hide back button.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        private static void hideBackButton(Control control)
        {
            ControlCollection col = control.Controls;
            foreach (Control ctrl in col)
            {
                if (ctrl is LinkButton)
                {
                    var lb = ctrl as LinkButton;
                    if (lb.Text.Contains("Back") && lb.Enabled == false)
                    {
                        lb.Visible = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     The handle event.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        public override void HandleEvent(CommandEventArgs e)
        {
            int page;
            if (Int32.TryParse(e.CommandName, out page))
            {
                DataPager dataPager = base.DataPager;
                int pageSize = PageSize;
                int nextStartRowIndex = pageSize*page;

                if (nextStartRowIndex > dataPager.TotalRowCount)
                {
                    nextStartRowIndex = dataPager.TotalRowCount - pageSize;
                }

                if (nextStartRowIndex < 0)
                {
                    nextStartRowIndex = 0;
                }

                dataPager.SetPageProperties(nextStartRowIndex, pageSize, true);
            }
        }
    }

    /// <summary>
    ///     The product list.
    /// </summary>
    public partial class ProductList : UserControlBase
    {
        /// <summary>
        ///     Gets or sets Pager CategoryID.
        /// </summary>
        public int PagerCategoryID { get; set; }

        /// <summary>
        ///     Gets or sets Pager CategoryID.
        /// </summary>
        public int PagerRootCategoryID { get; set; }

        /// <summary>
        ///     The category page size.
        /// </summary>
        public int CategoryPageSize;

        /// <summary>
        ///     The on init.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected override void OnInit(EventArgs e)
        {
            CategoryPageSize = HLConfigManager.Configurations.DOConfiguration.CategoryPageSize;
            SetpageSize();
        }

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
        }

        /// <summary>
        ///     The template pager field_ on pager command.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void TemplatePagerField_OnPagerCommand(object sender, DataPagerCommandEventArgs e)
        {
            // Check which button raised the event
            switch (e.CommandName)
            {
                case "ShowAll":
                    if (e.Item.Pager.PageSize == Int32.MaxValue)
                    {
                        e.Item.Pager.SetPageProperties(0, CategoryPageSize, true);
                    }
                    else
                    {
                        e.Item.Pager.SetPageProperties(0, Int32.MaxValue, true);
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        ///     The get default sku image path.
        /// </summary>
        /// <param name="prodInfo">
        ///     The prod info.
        /// </param>
        /// <returns>
        ///     The get default sku image path.
        /// </returns>
        private string getDefaultSKUImagePath(ProductInfo_V02 prodInfo)
        {
            if (prodInfo.DefaultSKU != null)
            {
                return prodInfo.DefaultSKU.ImagePath;
            }

            return string.Empty;
        }

        /// <summary>
        ///     The get default sku price.
        /// </summary>
        /// <param name="prodInfo">
        ///     The prod info.
        /// </param>
        /// <returns>
        ///     The get default sku price.
        /// </returns>
        private string getDefaultSKUPrice(ProductInfo_V02 prodInfo)
        {
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
            {
                if (prodInfo.DefaultSKU != null)
                {
                    SKU_V01 HLSKU;
                    if (AllSKUS.TryGetValue(prodInfo.DefaultSKU.SKU, out HLSKU))
                    {
                        return getAmountString(HLSKU.CatalogItem != null ? HLSKU.CatalogItem.ListPrice : 0.0M);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///     The collect all products.
        /// </summary>
        /// <param name="currentCategory">
        ///     The current category.
        /// </param>
        /// <param name="itemList">
        ///     The item list.
        /// </param>
        private void collectAllProducts(bool bShowAllInventory, Category_V02 currentCategory,
                                        List<UIProductInfo> itemList)
        {
            if (currentCategory == null)
                return;
            if (currentCategory.SubCategories != null)
            {
                foreach (Category_V02 sub in currentCategory.SubCategories)
                {
                    collectAllProducts(bShowAllInventory, sub, itemList);
                }
            }

            if (currentCategory.Products != null)
            {
                int sortIndx = 0;
                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                {
                    itemList.AddRange(
                        (from p in currentCategory.Products
                         where
                             p.TypeOfProduct == ProductType.Product &&
                             checkProductAvailability(bShowAllInventory, p) &&
                             (from s in p.SKUs where s.IsDisplayable select s).Count() > 0
                         select
                             new UIProductInfo(currentCategory.ID, getDefaultSKUImagePath(p), p.DisplayName, p.ID,
                                               getDefaultSKUPrice(p), sortIndx++)).ToList()
                        );
                }
                else
                {
                    if (IsChina && ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                    {
                        string[] words = null;
                        string eligibleSKu = string.Empty;
                        var rsp = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetEventEligibility(ShoppingCart.DistributorID);
                        if (rsp != null && rsp.IsEligible)
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
                            var skuList = new List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased>();
                            skuList.Add(skuETO);
                            var totalETOCount =
                                Providers.China.OrderProvider.GetSkuOrderedAndPurchased(ShoppingCart.CountryCode, null, null,
                                                                                        null, skuList);
                            var skulimit = Settings.GetRequiredAppSetting("ETOskulimit",string.Empty).Split('|');

                            if (skulimit.Length>1 && eligibleSKu.Trim().Equals(skulimit[0]) && totalETOCount.Any(x => x.QuantityPurchased >= Convert.ToInt16(skulimit[1]) && x.SKU == etoSku.CatalogItem.StockingSKU.Trim()))
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
                        itemList.AddRange(
                        (from p in currentCategory.Products
                         where
                             checkProductAvailability(bShowAllInventory, p) &&
                             (from s in p.SKUs where s.IsDisplayable && s.SKU.Contains(eligibleSKu.Trim()) select s).Count() > 0 
                         select
                             new UIProductInfo(currentCategory.ID, getDefaultSKUImagePath(p), p.DisplayName, p.ID,
                                               getDefaultSKUPrice(p), sortIndx++)).ToList()
                        );
                    }
                    else
                    {                        
                        itemList.AddRange(
                        (from p in currentCategory.Products
                         where
                             checkProductAvailability(bShowAllInventory, p) &&
                             (from s in p.SKUs where s.IsDisplayable select s).Count() > 0
                         select
                             new UIProductInfo(currentCategory.ID, getDefaultSKUImagePath(p), p.DisplayName, p.ID,
                                               getDefaultSKUPrice(p), sortIndx++)).ToList()
                        );
                }
            }
        }
        }

        /// <summary>
        ///     The create hyper link.
        /// </summary>
        /// <param name="currentCategory">
        ///     The current category.
        /// </param>
        /// <param name="parentCat">
        ///     The parent cat.
        /// </param>
        /// <param name="rootCat">
        ///     The root cat.
        /// </param>
        /// <returns>
        ///     The create hyper link.
        /// </returns>
        private string createHyperLink(Category_V02 currentCategory,
            Category_V02 rootCat,
            List<Category_V02> listCategory)
        {
            string breadCrumb = string.Empty;

            Category_V02 hyperLinkCategory = currentCategory;
            Category_V02 hyperLinkCategoryParent = rootCat;

            for (int idx = 0; idx < listCategory.Count; idx++)
            {
                if (idx != listCategory.Count - 1) // not last 
                {
                    breadCrumb = breadCrumb + listCategory[idx].DisplayName + " &gt; ";
                    hyperLinkCategory = listCategory[idx];
                }
            }
            if (hyperLinkCategory != null)
            {
                for (int idx = 0; idx < listCategory.Count; idx++)
                {
                    if (listCategory[idx].ID == hyperLinkCategory.ID)
                    {
                        hyperLinkCategoryParent = idx == 0 ? hyperLinkCategory : listCategory[idx - 1];
                        break;
                    }
                }
            }

            return
                string.Format("<a href='catalog.aspx?cid={0}&root={1}&parent={2}'>{3}</a>{4}",
                hyperLinkCategory.ID,
                              rootCat.ID,
                              hyperLinkCategoryParent == null ? hyperLinkCategory.ID : hyperLinkCategoryParent.ID,
                breadCrumb, currentCategory.DisplayName);
        }


        /// <summary>
        ///     The get bread crumb.
        /// </summary>
        /// <param name="currentCategory">
        ///     The current category.
        /// </param>
        /// <param name="rootCategory">
        ///     The root category.
        /// </param>
        /// <returns>
        ///     The get bread crumb.
        /// </returns>
        private string getBreadCrumb(Category_V02 currentCategory, Category_V02 rootCategory)
        {
            if (currentCategory == null || rootCategory == null)
            {
                return string.Empty;
            }
            try
            {
                Category_V02 category = rootCategory;
                var listCategory = new List<Category_V02>();
                while (category != null)
                {
                    category = CatalogHelper.getCategory(currentCategory, category, ref listCategory);
                }

                return createHyperLink(currentCategory, rootCategory, listCategory);
            }
            catch
            {
                LoggerHelper.Error(string.Format("Error getBreadCrumb catID : {0}", currentCategory.ID));
            }
            return string.Empty;
        }


        /// <summary>
        ///     The setpage size.
        /// </summary>
        public void SetpageSize()
        {
            DataPager1.PageSize = DataPager2.PageSize = CategoryPageSize;
        }

        /// <summary>
        ///     The check product availability.
        /// </summary>
        /// <param name="showAllInventory">
        ///     The show all inventory.
        /// </param>
        /// <param name="itemList">
        ///     The item list.
        /// </param>
        private void checkProductAvailability(bool showAllInventory, List<UIProductInfo> itemList)
        {
            // ListView Products
            if (!showAllInventory)
            {
                ProductInfoCatalog_V01 ProductInfoCatalog = ProductsBase.ProductInfoCatalog;
                foreach (ListViewDataItem listItem in Products.Items)
                {
                    var hdProductID = listItem.FindControl("ProductID") as HiddenField;
                    var hdCatID = listItem.FindControl("CatID") as HiddenField;
                    if (hdProductID != null && hdCatID != null)
                    {
                        string prodID = hdProductID.Value;
                        string catID = hdCatID.Value;

                        ProductInfo_V02 product = CatalogHelper.getProduct(ProductInfoCatalog, int.Parse(catID),
                                                                           int.Parse(prodID));

                        IEnumerable<SKU_V01> hasStock = from l in product.SKUs
                                                        from s in AllSKUS.Keys
                                                        where
                                                            l.SKU == s &&
                                                            AllSKUS[s].ProductAvailability !=
                                                            ProductAvailabilityType.Unavailable
                                                        select l;
                        //var lbOutofstock = listItem.FindControl("lbOutofstock") as Label;
                        //if (lbOutofstock != null)
                        //{
                        //    lbOutofstock.Visible = hasStock.Count() == 0;
                        //}
                        var td = listItem.FindControl("tdProduct") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible = (hasStock != null && hasStock.Count() != 0) || showAllInventory;
                        }
                    }
                }
            }
            //else
            //{
            //    foreach (ListViewDataItem listItem in Products.Items)
            //    {
            //        var lbOutofstock = listItem.FindControl("lbOutofstock") as Label;
            //        if (lbOutofstock != null)
            //        {
            //            lbOutofstock.Visible = false;
            //        }
            //    }
            //}
        }

        private bool checkProductAvailability(bool showAllInventory, ProductInfo_V02 product)
        {
            if (product.SKUs == null || product.SKUs.Count == 0)
            {
                return false;
            }
            // ListView Products
            if (!showAllInventory)
            {
                IEnumerable<SKU_V01> hasStock = from l in product.SKUs
                                                from s in AllSKUS.Keys
                                                where
                                                    l.SKU == s &&
                                                    AllSKUS[s].ProductAvailability !=
                                                    ProductAvailabilityType.Unavailable
                                                select l;
                return hasStock.Count() != 0;
            }
            else
            {
                IEnumerable<SKU_V01> hasStock = from l in product.SKUs
                                                from s in AllSKUS.Keys
                                                where l.SKU == s
                                                select l;
                return hasStock.Count() != 0;
            }
        }

        /// <summary>
        ///     The populate products.
        /// </summary>
        /// <param name="currentCategory">
        ///     The current category.
        /// </param>
        /// <param name="rootCategory">
        ///     The root category.
        /// </param>
        public void PopulateProducts(Category_V02 currentCategory, Category_V02 rootCategory)
        {
            if (currentCategory == null)
            {
                return;
            }

            uxSubTablaHeader.Text = getBreadCrumb(currentCategory, rootCategory);
            uxSubTablaFooter.Text = uxSubTablaHeader.Text;

            var itemList = new List<UIProductInfo>();
            collectAllProducts(SessionInfo.ShowAllInventory, currentCategory, itemList);
            if (itemList != null)
            {
                Products.DataSource = itemList.OrderBy(item => item.SortOrder).ToList();
                Products.DataBind();
                upProductList.Update();
                //checkProductAvailability(ProductsBase.SessionInfo.ShowAllInventory, itemList);
            }
        }

        /// <summary>
        ///     The find control in.
        /// </summary>
        /// <param name="control">
        ///     The control.
        /// </param>
        /// <param name="controlName">
        ///     The control name.
        /// </param>
        /// <returns>
        /// </returns>
        private Control FindControlIn(Control control, string controlName)
        {
            foreach (Control ctrl in control.Controls)
            {
                if (ctrl.ID == controlName)
                    return ctrl;

                Control f = FindControlIn(ctrl, controlName);
                if (f != null)
                    return f;
            }

            return null;
        }

        /// <summary>
        ///     The on page properties changing_ click.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected void OnPagePropertiesChanging_Click(object sender, PagePropertiesChangingEventArgs e)
        {
            var productsBase = Page as ProductsBase;
            if (string.IsNullOrEmpty(productsBase.Locale))
                return;

            DataPager2.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            DataPager1.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);

            if (Request.QueryString["cid"] != null && Request.QueryString["root"] != null)
            {
                PagerCategoryID = int.Parse(Request.QueryString["cid"] ?? "0");
                PagerRootCategoryID = int.Parse(Request.QueryString["root"] ?? "0");
            }
            int parentcategoryID = int.Parse(Request.QueryString["parent"] ?? "0");

            Category_V02 rootCategory = null;

            // when it is from product detail screen
            if (PagerCategoryID == 0)
            {
                int categoryId = 0;
                if(int.TryParse(Request.QueryString["CategoryID"], out categoryId))
                {
                    PagerCategoryID = categoryId;
                }
                else
                {
                    PagerCategoryID = 0;
                }

                rootCategory = CatalogHelper.GetRootCategory(ProductInfoCatalog, PagerCategoryID);
               // Category_V02 currentCategory;
            }
            else if (PagerRootCategoryID != 0)
            {
                rootCategory = ProductInfoCatalog.RootCategories.Find(r => r.ID == PagerRootCategoryID);
            }
            if (PagerCategoryID != 0 && rootCategory != null)
            {
                Category_V02 currentCategory;
                if (ProductInfoCatalog.AllCategories.TryGetValue(PagerCategoryID, out currentCategory))
                {
                    PopulateProducts(currentCategory, rootCategory);
                }
            }
        }

        protected string GetSpecialMessage()
        {
            string message = string.Empty;
            if (null != base.ShoppingCart)
            {
                if (base.ShoppingCart.OrderCategory == OrderCategoryType.ETO)
                {
                    string text = MyHL_GlobalResources.EventTicketSpecialMessage;
                    if (!string.IsNullOrEmpty(text))
                    {
                        message = string.Format(" title='{0}' ", text);
                    }
                }
            }

            return message;
        }
    }
}
