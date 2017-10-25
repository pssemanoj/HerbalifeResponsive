using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class InvoiceCatalog : MyHerbalife3.Shared.UI.UserControlBase
    {
        /// The _allow decimal.
        /// </summary>
        private readonly bool _allowDecimal = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal;

        /// <summary>
        /// The _errors.
        /// </summary>
        private List<string> _errors = new List<string>();

        /// <summary>
        /// The _errors.
        /// </summary>
        private List<string> _friendlyMessages = new List<string>();
        
        private int _productTableColumns = 6;

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
            
        }

        protected override void OnInit(EventArgs e)
        {
            if (!IsPostBack)
            {
                CategoryDropdown.DataSource = from r in ProductInfoCatalog.RootCategories
                                              where shouldTake(r, false)
                                              select r;
                CategoryDropdown.DataBind();
            }
        }
        
        /// <summary>
        /// The get category id.
        /// </summary>
        /// <param name="uiCategoryProduct">
        /// The ui category product.
        /// </param>
        /// <returns>
        /// The get category id.
        /// </returns>
        protected int GetCategoryID(UICategoryProduct uiCategoryProduct)
        {
            return uiCategoryProduct.Category.ID;
        }

        /// <summary>
        /// The get product id.
        /// </summary>
        /// <param name="uiCategoryProduct">
        /// The ui category product.
        /// </param>
        /// <returns>
        /// The get product id.
        /// </returns>
        protected int GetProductID(UICategoryProduct uiCategoryProduct)
        {
            return uiCategoryProduct.Product.ID;
        }

        /// <summary>
        /// The should take.
        /// </summary>
        /// <param name="prod">
        /// The prod.
        /// </param>
        /// <param name="bEventTicket">
        /// The b event ticket.
        /// </param>
        /// <returns>
        /// The should take.
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

            return take;
        }

        /// <summary>
        /// The should take.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="bEventTicket">
        /// The b event ticket.
        /// </param>
        /// <returns>
        /// The should take.
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

        /// <summary>
        /// The get category text.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <returns>
        /// The get category text.
        /// </returns>
        private string getCategoryText(Category_V02 category)
        {
            string text = string.Empty;
            Category_V02 parent = category.ParentCategory;
            while (parent != null)
            {
                text = parent.DisplayName + "&nbsp;&gt;&nbsp;";
                parent = parent.ParentCategory;
            }

            return text;
        }

        /// <summary>
        /// The get bread crumb text.
        /// </summary>
        /// <param name="categoryProudct">
        /// The category proudct.
        /// </param>
        /// <returns>
        /// The get bread crumb text.
        /// </returns>
        protected string getBreadCrumbText(UICategoryProduct categoryProudct)
        {
            //return getCategoryText(categoryProudct.Category) + categoryProudct.Category.DisplayName + "&nbsp;&gt;&nbsp;" +
            //       categoryProudct.Product.DisplayName;
            return CatalogHelper.getBreadCrumbText(categoryProudct.Category, categoryProudct.RootCategory, categoryProudct.Product);
        }
               
        /// <summary>
        /// The find root category from id.
        /// </summary>
        /// <param name="categoryID">
        /// The category id.
        /// </param>
        /// <returns>
        /// </returns>
        private Category_V02 findRootCategoryFromID(int categoryID)
        {
            IEnumerable<Category_V02> varCat = ProductInfoCatalog.RootCategories.Where(c => c.ID == categoryID);
            return varCat.Count() > 0 ? varCat.First() : null;
        }

        /// <summary>
        /// The get enabled.
        /// </summary>
        /// <param name="availType">
        /// The avail type.
        /// </param>
        /// <returns>
        /// The get enabled.
        /// </returns>
        protected bool getEnabled(ProductAvailabilityType availType)
        {
            return true;
        }

        /// <summary>
        /// The on category dropdown_ data bound.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnCategoryDropdown_DataBound(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            ddl.SelectedIndex = -1;
            ListItem selected = ddl.Items.Count > 0 ? ddl.Items[0] : null;
            if (selected != null)
            {
                selected.Selected = true;
                populateCategories(findRootCategoryFromID(int.Parse(selected.Value)));
            }
        }

        /// <summary>
        /// The on category dropdown_ selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnCategoryDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            //begin - shan - mar 15, 2012 - to retain the entered quantity for the previous skus
            List<string> existingPrds = hdnSelectedPrds.Value.Trim().Split('|').ToList();
            string rootCategory = string.Empty;
            string selectedPrds = string.Empty;
            //loop thru items to get the entered qtys along with the sku fields
            foreach (RepeaterItem categoryItem in Subcategories.Items)
            {
                //get root category..if got already set the same value..
                rootCategory = string.IsNullOrEmpty(rootCategory) ?
                    (categoryItem.FindControl("hdnRootCategoryID") as HiddenField).Value : rootCategory;
                //get product list
                Repeater prdList = categoryItem.FindControl("Products") as Repeater;
                //get the entered qtys
                if (null != prdList && 0 < prdList.Items.Count)
                {
                    var enteredPrds = from prdItem in prdList.Items.Cast<RepeaterItem>()
                                      where (!string.IsNullOrEmpty((prdItem.FindControl("QuantityBox") as TextBox).Text.Trim()))
                                      select (
                                      rootCategory + ":" +
                                      (prdItem.FindControl("ProductID") as HiddenField).Value + ":" +
                                      (prdItem.FindControl("VolumePointsValue") as HiddenField).Value + ":" +
                                      (prdItem.FindControl("QuantityBox") as TextBox).Text);
                    //add the newly selected products to a temp list
                    enteredPrds.ToList().ForEach(prd =>
                    {
                        selectedPrds += !string.IsNullOrEmpty(selectedPrds) ? "|" : string.Empty;
                        selectedPrds += !string.IsNullOrEmpty(prd) ? prd : string.Empty;
                    });
                }
            }
            //clear the existing prds related to current category
            existingPrds.RemoveAll(prd => rootCategory == prd.Split(':')[0]);
            //clear the hidden field and add again
            hdnSelectedPrds.Value = string.Empty;
            //update the hidden field with exisitng prds
            existingPrds.ForEach(prd =>
            {
                hdnSelectedPrds.Value += !string.IsNullOrEmpty(hdnSelectedPrds.Value) ? "|" : string.Empty;
                hdnSelectedPrds.Value += !string.IsNullOrEmpty(prd) ? prd : string.Empty; ;
            });
            //add the newly selected list
            hdnSelectedPrds.Value += !string.IsNullOrEmpty(hdnSelectedPrds.Value) ? "|" : string.Empty;
            hdnSelectedPrds.Value += !string.IsNullOrEmpty(selectedPrds) ? selectedPrds : string.Empty;
            //end

            //fill the products corresponding to newly selected category
            if (ProductInfoCatalog.RootCategories != null)
            {
                populateCategories(CategoryDropdown.SelectedItem != null
                                       ? findRootCategoryFromID(int.Parse(CategoryDropdown.SelectedItem.Value))
                                       : ProductInfoCatalog.RootCategories[0]);
                // 24061
                _errors.Clear();
                // 27237
                lblSuccess.Visible = false;
                blstErrores.DataSource = _errors;
                blstErrores.DataBind();
            }

            //begin - shan - mar 15, 2012 - for the populated categories,
            //loop thru items to check for the already entered qty and fill in
            foreach (RepeaterItem categoryItem in Subcategories.Items)
            {
                //get product list
                Repeater prdList = categoryItem.FindControl("Products") as Repeater;
                //loop the prds to check if already selected and fill qty
                if (null != prdList && 0 < prdList.Items.Count)
                {
                    foreach (RepeaterItem prdItem in prdList.Items)
                    {
                        string currentSku = (prdItem.FindControl("ProductID") as HiddenField).Value;
                        //check if the item exists for the current sku and fill in the qty value
                        //the items w.r.t the newly selected category
                        //should be available from the selected prds list itself..if already entered..
                        //check only if count > 1, as the
                        var enteredPrd =
                            existingPrds.Find(prd => currentSku == (!string.IsNullOrEmpty(prd) ? prd.Split(':')[1] : string.Empty));
                        //if available..fill the qty field..
                        (prdItem.FindControl("QuantityBox") as TextBox).Text =
                            !string.IsNullOrEmpty(enteredPrd) ? enteredPrd.Split(':')[3] : string.Empty;
                    }
                }
            }
            //end
        }

        /// <summary>
        /// The get all products.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        /// <param name="allProducts">
        /// The all products.
        /// </param>
        /// <returns>
        /// </returns>
        private List<UICategoryProduct> getAllProducts(Category_V02 rooCategory, Category_V02 category, List<UICategoryProduct> allProducts)
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
                allProducts.AddRange(from p in category.Products
                                     where p.SKUs != null && p.SKUs.Count > 0 && (from s in p.SKUs where s.IsDisplayable select s).Count() > 0
                                     select new UICategoryProduct
                                     {
                                         Category = category,
                                         Product = p,
                                         RootCategory = rooCategory,
                                     }
                    );
            }

            return allProducts;
        }

        /// <summary>
        /// The on subcategories_ item data bound.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnSubcategories_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                if (e.Item.DataItem != null)
                {
                    //begin - shan - mar 16, 2012 - set root category id
                    UICategoryProduct prd = e.Item.DataItem as UICategoryProduct;
                    (e.Item.FindControl("hdnRootCategoryID") as HiddenField).Value =
                        null != prd && null != prd.RootCategory ? prd.RootCategory.ID.ToString() : string.Empty;
                    //end
                    var rp = e.Item.FindControl("Products") as Repeater;
                    rp.DataSource = from p in (e.Item.DataItem as UICategoryProduct).Product.SKUs
                                        from a in AllSKUS.Keys
                                        where a == p.SKU
                                        select AllSKUS[a];
                    
                    rp.DataBind();
                }
            }
        }

        private ProductInfoCatalog_V01 _productInfoCatalog;
        public ProductInfoCatalog_V01 ProductInfoCatalog
        {
            get
            {
                if (_productInfoCatalog == null)
                {
                    _productInfoCatalog = CatalogProvider.GetProductInfoCatalog("en-US");
                }

                if (_productInfoCatalog == null)
                {
                    throw new ApplicationException(
                        string.Format("Unable to retrieve Product Info Catalog. Locale: {0}", "en-US"));
                }

                return _productInfoCatalog;
            }

            set { _productInfoCatalog = value; }
        }

        private Dictionary<string, SKU_V01> _allSKUS;
        /// <summary>
        /// Gets or sets AllSKUS.
        /// </summary>
        public Dictionary<string, SKU_V01> AllSKUS
        {
            get
            {
                if (_allSKUS == null)
                {
                    _allSKUS = CatalogProvider.GetAllSKU("en-US");
                }

                return _allSKUS;
            }

            set { _allSKUS = value; }
        }

        /// <summary>
        /// The get bread crumb text.
        /// </summary>
        /// <returns>
        /// The get bread crumb text.
        /// </returns>
        private string getBreadCrumbText()
        {
            return null;
        }

        /// <summary>
        /// The get price format.
        /// </summary>
        /// <returns>
        /// The get price format.
        /// </returns>
        protected string getPriceFormat(decimal number)
        {
            return _allowDecimal ? "{0:N2}" : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
        }

        /// <summary>
        /// The get price format 2.
        /// </summary>
        /// <returns>
        /// The get price format 2.
        /// </returns>
        protected string getPriceFormat2(decimal number)
        {
            return _allowDecimal ? "N2" : (number == (decimal)0.0 ? "0" : "#,###");
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        /// <summary>
        /// The populate categories.
        /// </summary>
        /// <param name="category">
        /// The category.
        /// </param>
        private void populateCategories(Category_V02 category)
        {
            var prodList = new List<UICategoryProduct>();
            prodList = getAllProducts(category, category, prodList);
            if (prodList != null)
            {
                Subcategories.DataSource = prodList;
                Subcategories.DataBind();
                refreshBreadcrumbs();
            }
            else
            {
                LoggerHelper.Error(string.Format("No category found for {0}", CategoryDropdown.SelectedValue));
            }
        }

        /// <summary>
        /// The on products_ on item created.
        /// </summary>
        /// <param name="Sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnProducts_OnItemCreated(object Sender, RepeaterItemEventArgs e)
        {
        }
                
        /// <summary>
        /// The refresh breadcrumbs.
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
                        var repeaterProducts = item.FindControl("Products") as Repeater;
                        if (repeaterProducts != null)
                        {
                            trCntrl.Visible = repeaterProducts.Items.Count != 0;
                        }
                    }
                }
            }
        }

        protected int getProductTableColumns()
        {
            return _productTableColumns;
        }

        /// <summary>
        /// The add items delegate.
        /// </summary>
        /// <param name="sku">
        /// The sku.
        /// </param>
        /// <param name="quantity">
        /// The quantity.
        /// </param>
        private delegate void AddItemsDelegate(string sku, int quantity, bool partialBackordered);

        public string GetSymbol()
        {
            return "$";
            //if (HLConfigManager.Configurations.CheckoutConfiguration != null)
            //{
            //    return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            //}
            //return System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
        }

        protected  void lnkClose_OnCLick(object sender, EventArgs e)
        {
            CategoryDropdown.SelectedIndex = 0;
            OnCategoryDropdown_SelectedIndexChanged(this, e);
        }
        
        public event EventHandler onAddToInvoiceClick;
        protected void AddToInvoiceClicked(object sender, EventArgs e)
        {
            //set this to add the items which are available in the hidden field
            //based on this the items can be added for the other not visible categories
            string visibleCategory = string.Empty;
            var invoiceSkus = new List<InvoiceSKU>();
            foreach (object s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    //get root category..if got already set the same value..
                    visibleCategory = string.IsNullOrEmpty(visibleCategory) ?
                        (item.FindControl("hdnRootCategoryID") as HiddenField).Value : visibleCategory;
                    //get product list
                    var uxProducts = item.FindControl("Products") as Repeater;
                    foreach (object p in uxProducts.Items)
                    {
                        var productItem = p as RepeaterItem;
                        if (productItem.ItemType == ListItemType.AlternatingItem ||
                            productItem.ItemType == ListItemType.Item)
                        {
                            var uxQuantity = productItem.FindControl("QuantityBox") as TextBox;
                            var VolumePointsValue = (productItem.FindControl("VolumePointsValue") as HiddenField).Value;
                            var quantity = 0;
                            decimal volumePoints = 0;
                            if (uxQuantity.Text != string.Empty)
                            {
                                string productID = (productItem.FindControl("ProductID") as HiddenField).Value;
                                decimal.TryParse(VolumePointsValue, out volumePoints);
                                if (!int.TryParse(uxQuantity.Text, out quantity) || quantity <= 0)
                                {
                                    uxQuantity.Text = string.Empty;
                                }
                                else
                                {
                                    var invoiceSku = new InvoiceSKU();
                                    invoiceSku.Quantity = quantity;
                                    invoiceSku.SKU = productID;
                                    invoiceSku.UnitVolumePoints = volumePoints;
                                    invoiceSkus.Add(invoiceSku);
                                }
                            }
                        }
                    }
                }
            }

            //begin - shan - mar 15, 2012
            //add the previously selected skus to the list
            //only if not available in the above list
            List<string> selectedPrds = hdnSelectedPrds.Value.Trim().Split('|').ToList();
            selectedPrds.ForEach(prd =>
            {
                if (!string.IsNullOrEmpty(prd.Trim()))
                {
                    string[] prdArr = prd.Split(':');
                    if (null != prdArr && 3 < prdArr.Length)
                    {
                        //get category id
                        string categoryID = prdArr[0];
                        //add only if the category is not one the which was visible
                        if (categoryID != visibleCategory.ToString())
                        {
                            string sku = prdArr[1];
                            //add sku if not in the list
                            if (null == invoiceSkus.Find(invSku => sku == invSku.SKU))
                            {
                                var invoiceSku = new InvoiceSKU();
                                invoiceSku.SKU = sku;
                                decimal volumePoints = 0;
                                invoiceSku.UnitVolumePoints = decimal.TryParse(prdArr[2], out volumePoints) ? volumePoints : 0;
                                decimal qty = 0;
                                invoiceSku.Quantity = decimal.TryParse(prdArr[3], out qty) ? qty : 0;
                                //insert it to previous index, so that it can add on order
                                invoiceSkus.Insert(invoiceSkus.Count > 0 ? invoiceSkus.Count - 1 : 0, invoiceSku);
                            }
                        }
                    }
                }
            });
            //end

            if(invoiceSkus.Count>0)
            {
                if (onAddToInvoiceClick != null)
                {
                    onAddToInvoiceClick(this, new CommandEventArgs("InvoiceSKUs", invoiceSkus));
                }
            }
            CategoryDropdown.SelectedIndex = 0;
            OnCategoryDropdown_SelectedIndexChanged(this, e);
        }

        protected void OnClearClick(object sender, EventArgs e)
        {
            foreach (object s in Subcategories.Items)
            {
                var item = s as RepeaterItem;
                if (item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.Item)
                {
                    var uxProducts = item.FindControl("Products") as Repeater;
                    foreach (object p in uxProducts.Items)
                    {
                        var productItem = p as RepeaterItem;
                        if (productItem.ItemType == ListItemType.AlternatingItem ||
                            productItem.ItemType == ListItemType.Item)
                        {
                            var uxQuantity = productItem.FindControl("QuantityBox") as TextBox;
                            if (uxQuantity.Text != string.Empty)
                            {
                                uxQuantity.Text = string.Empty;
                            }
                        }
                    }
                }
            }
            //begin - shan - mar 16, 2012
            //clear the hidden field having the list of previously entered qtys
            hdnSelectedPrds.Value = string.Empty;
            //end
        }
    }

    /// <summary>
    /// The ui category product.
    /// </summary>
    public class UICategoryProduct
    {
        /// <summary>
        /// Gets or sets Category.
        /// </summary>
        public Category_V02 Category { get; set; }

        /// <summary>
        /// Gets or sets Product.
        /// </summary>
        public ProductInfo_V02 Product { get; set; }

        /// <summary>
        /// Gets or sets Root Category.
        /// </summary>
        public Category_V02 RootCategory { get; set; }
    }
}