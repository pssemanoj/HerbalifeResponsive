using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.Web.MasterPages;
using Telerik.Web.UI;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class SearchProducts : ProductsBase
    {
        #region constant / enum

        public static string PLATFORM = "MyHL";

        #endregion

        #region fields

        private ModalPopupExtender mpProductDetail;

        #endregion

        #region Property

        public int CategoryID { get; set; }
        public int ProductID { get; set; }

        #endregion

        /// <summary>
        /// Products list.
        /// </summary>
        private static IEnumerable<string> Products { get; set; }

        private static IEnumerable<string> ProductsClone { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            EnableViewState = true;
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                txtSearchTerm.CssClass = "productInput gdo-order-sku-input";
                txtSearchTerm.Width = Unit.Percentage(98);
            }
            if (!IsPostBack)
            {
                if (AllSKUS.Any())
                {
                    if (IsChina)
                    {
                        Products = AllSKUS.Where(sku => sku.Value.ProductAvailability == ProductAvailabilityType.Available && sku.Value.IsDisplayable && sku.Value.IsPurchasable)
                                      .Select(
                                          sku =>
                                          {
                                              var str = string.Concat(sku.Key, " ",
                                                sku.Value.Product != null ? sku.Value.Product.Overview != null ? Regex.Replace(sku.Value.Product.Overview, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.DisplayName != null ? Regex.Replace(sku.Value.Product.DisplayName, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Usage != null ? Regex.Replace(sku.Value.Product.Usage, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.SpecialNotes != null ? Regex.Replace(sku.Value.Product.SpecialNotes, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.FastFacts != null ? Regex.Replace(sku.Value.Product.FastFacts, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Benefits != null ? Regex.Replace(sku.Value.Product.Benefits, "<.*?>", String.Empty) + " " : string.Empty : string.Empty);
                                              return str;

                                          }
                                        );

                    ProductsClone = AllSKUS.Where(sku => sku.Value.ProductAvailability == ProductAvailabilityType.Available && sku.Value.IsDisplayable && sku.Value.IsPurchasable)
                                      .Select(
                                          sku =>
                                          {
                                              var str = string.Concat(sku.Key, " ",
                                                sku.Value.Product != null ? sku.Value.Product.Overview != null ? sku.Value.Product.Overview + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.DisplayName != null ? sku.Value.Product.DisplayName + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Usage != null ? sku.Value.Product.Usage + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.SpecialNotes != null ? sku.Value.Product.SpecialNotes + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.FastFacts != null ? sku.Value.Product.FastFacts + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Benefits != null ? sku.Value.Product.Benefits + " " : string.Empty : string.Empty);
                                              return str;

                                          }
                                        );
                    }
                    else
                    {
                        Products = AllSKUS.Where(sku => sku.Value.ProductAvailability == ProductAvailabilityType.Available)
                                      .Select(
                                          sku =>
                                          {
                                              var str = string.Concat(sku.Key, " ",
                                                sku.Value.Product != null ? sku.Value.Product.Overview != null ? Regex.Replace(sku.Value.Product.Overview, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.DisplayName != null ? Regex.Replace(sku.Value.Product.DisplayName, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Usage != null ? Regex.Replace(sku.Value.Product.Usage, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.SpecialNotes != null ? Regex.Replace(sku.Value.Product.SpecialNotes, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.FastFacts != null ? Regex.Replace(sku.Value.Product.FastFacts, "<.*?>", String.Empty) + " " : string.Empty : string.Empty,
                                                sku.Value.Product != null ? sku.Value.Product.Benefits != null ? Regex.Replace(sku.Value.Product.Benefits, "<.*?>", String.Empty) + " " : string.Empty : string.Empty);
                                              return str;

                }
                                        );

                        ProductsClone = AllSKUS.Where(sku => sku.Value.ProductAvailability == ProductAvailabilityType.Available)
                                          .Select(
                                              sku =>
                                              {
                                                  var str = string.Concat(sku.Key, " ",
                                                    sku.Value.Product != null ? sku.Value.Product.Overview != null ? sku.Value.Product.Overview + " " : string.Empty : string.Empty,
                                                    sku.Value.Product != null ? sku.Value.Product.DisplayName != null ? sku.Value.Product.DisplayName + " " : string.Empty : string.Empty,
                                                    sku.Value.Product != null ? sku.Value.Product.Usage != null ? sku.Value.Product.Usage + " " : string.Empty : string.Empty,
                                                    sku.Value.Product != null ? sku.Value.Product.SpecialNotes != null ? sku.Value.Product.SpecialNotes + " " : string.Empty : string.Empty,
                                                    sku.Value.Product != null ? sku.Value.Product.FastFacts != null ? sku.Value.Product.FastFacts + " " : string.Empty : string.Empty,
                                                    sku.Value.Product != null ? sku.Value.Product.Benefits != null ? sku.Value.Product.Benefits + " " : string.Empty : string.Empty);
                                                  return str;

                                              }
                                            );
                    }
                }
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);

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
            }

            mpProductDetail = (ModalPopupExtender) ucProductDetail.FindControl("ppProductDetail");
        }

        protected void gvSearchProducts_RowDataBound(object sender, GridViewRowEventArgs e)
        {
        }

        protected string getDefaultSKUImagePath(ProductInfo_V02 product)
        {
            if (product.DefaultSKU != null)
            {
                return product.DefaultSKU.ImagePath;
            }
            return string.Empty;
        }

        protected void getIDFromQueryString()
        {
            string strProductID = Request.QueryString["ProdInfoID"] ?? string.Empty;
            int productID = 0, categoryID = 0;

            if (!string.IsNullOrEmpty(strProductID))
            {
                productID = int.Parse(strProductID);
                ProductID = productID;
            }

            string strCategoryID = Request.QueryString["CategoryID"] ?? string.Empty;
            if (!string.IsNullOrEmpty(strCategoryID))
            {
                categoryID = int.Parse(strCategoryID);
                CategoryID = categoryID;
            }
        }

        /// <summary>
        ///     load search result
        /// </summary>
        private void loadSearchResult()
        {
            rgSearchProducts.Rebind();
        }

        /// <summary>
        ///     Find a category ID for a given product ID.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        protected int getCategoryID(int productId)
        {
            var varCategory = (from c in ProductInfoCatalog.AllCategories.Values
                               where (c.Products != null && c.Products.Exists(p => p.ID.Equals(productId)))
                               select c).FirstOrDefault();

            return (varCategory == null ? 0 : varCategory.ID);
        }

        /// <summary>
        ///     Go Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGo_Click(object sender, EventArgs e)
        {
            loadSearchResult();
        }

        protected void ProductDetailClicked(object sender, EventArgs e)
        {
            int categoryID = 0, productID = 0;
            var lb = sender as LinkButton;
            if (lb != null)
            {
                string commandArgument = lb.CommandArgument;
                var commandParts = commandArgument.Split(' ');
                productID = int.Parse(commandParts[0]);
                categoryID = int.Parse(commandParts[1]);
            }
        }

        protected string getProductDetailUrl(string Id)
        {
            string urlWithoutPort = GetRequestURLWithOutPort();

            string prductDetailUrl =
                urlWithoutPort.Replace(urlWithoutPort.Substring(urlWithoutPort.LastIndexOf(@"/") + 1), "ProductDetail.aspx") +
                "?ProdInfoID=" + Id;

            return prductDetailUrl;
        }

        protected void rgSearchProducts_ItemDataBound(object sender, GridItemEventArgs e)
        {
        }

        protected void rgSearchProducts_ItemCommand(object source, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ClickImageBtnProductImage":
                case "ClickSearchResultLine1":
                    int productId;
                    if (!int.TryParse(e.CommandArgument.ToString(), out productId))
                    {
                        break;
                    }
                    int categoryId = getCategoryID(productId);
                    mpProductDetail = (ModalPopupExtender) ucProductDetail.FindControl("popup_ProductDetailControl");
                    ucProductDetail.LoadProduct(categoryId, productId);
                    mpProductDetail.Show();
                    break;
                case "ClickSearchResultLine3":
                    break;
                default:
                    break;
            }
        }

        protected void rgSearchProducts_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            if (!IsPostBack) return;
            if (string.IsNullOrEmpty(txtSearchTerm.Text)) return;
            var searchString = txtSearchTerm.Text;
            string locale = Locale.Replace("-", "_");

            var skuList = new List<SKU_V01>();
            try
            {
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    var indx = txtSearchTerm.Text.IndexOf(" ");
                    if (indx > 0)
                    {
                        var skuid = txtSearchTerm.Text.Substring(0, indx);
                        var prd = ProductsClone.FirstOrDefault(c => c.Contains(skuid));
                        if (!string.IsNullOrEmpty(prd))
                        {
                            searchString = prd;
                        }
                    }
                }

                var searchResults = CatalogProvider.GetSKUsByList(locale, PLATFORM, searchString);
                if (searchResults == null)
                {
                    throw new ApplicationException(
                        string.Format(
                            "No result received from CatalogProvider.GetSKUsByList. Locale: {0}, Platform: {1}, Search Term: {2}",
                            locale, PLATFORM, txtSearchTerm.Text));
                }
                foreach (var item in searchResults)
                {
                    skuList.Add(ProductInfoCatalog.AllSKUs[item]);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("rgSearchProducts_NeedDataSource", ex);
            }

            if (skuList == null || skuList.Count() == 0)
            {
                string noResultsMessage = GetLocalResourceString("NoResults",
                                                                 "Sorry. No matches were found containing {0}. To conduct a new search, use the search field at the top of the page. For better results, try a different keyword or search item.");

                if (noResultsMessage.Contains("{0}"))
                {
                    noResultsMessage = string.Format(noResultsMessage, txtSearchTerm.Text.Replace("'", "\\'"));
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "alert1", "alert('" + noResultsMessage + "');",
                                                    true);
            }

            rgSearchProducts.DataSource = skuList;
        }

        /// <summary>
        /// Gets the SKU list.
        /// </summary>
        /// <returns>List of products information.</returns>
        [WebMethod]
        [ScriptMethod]
        public static ProductsResultView GetAutoCompleteOptions()
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var result = new ProductsResultView()
                    {
                        ProductsList = Products,
                        SkuList = Products != null
                                      ? Products.Select(
                                          product => product.Substring(0, product.IndexOf(' '))).ToList()
                                      : Products
                    };
                return result;
            }
            return null;

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
    }
}