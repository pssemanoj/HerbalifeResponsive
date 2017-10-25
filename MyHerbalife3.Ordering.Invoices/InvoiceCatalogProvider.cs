#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class InvoiceCatalogProvider : IInvoiceCatalogProvider
    {
        private const int InvoiceCategoryCacheMinutes = 60;
        private readonly ISimpleCache _cache = CacheFactory.Create();

        public List<InvoiceRootCategoryModel> GetRootCategories(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            var cacheKey = string.Format("Inv_{0}_{1}", "rootCategories", locale);

            var result = _cache.Retrieve(_ => GetRootCategoriesFromSource(locale), cacheKey,
                TimeSpan.FromMinutes(InvoiceCategoryCacheMinutes));
            return result;
        }

        public List<InvoiceCategoryModel> GetCategories(int rootCategoryId, string locale, bool isCustomer)
        {
            if (string.IsNullOrEmpty(locale) || rootCategoryId == 0)
            {
                return null;
            }

            var cacheKey = string.Format("Inv_{0}_{1}_{2}_{3}", "Categories", locale, rootCategoryId, isCustomer);

            var result = _cache.Retrieve(_ => GetCategoriesFromSource(rootCategoryId, locale, isCustomer), cacheKey,
                TimeSpan.FromMinutes(InvoiceCategoryCacheMinutes));
            return result;
        }

        public IEnumerable<InvoiceCategoryModel> SearchCategories(GetInvoiceCategoryByFilter invoiceCategoryFilter)
        {
            if (null == invoiceCategoryFilter)
            {
                return new List<InvoiceCategoryModel>();
            }

            var categories = GetCategories(invoiceCategoryFilter.RootCategoryId, invoiceCategoryFilter.Locale, !string.IsNullOrEmpty(invoiceCategoryFilter.Type) && invoiceCategoryFilter.Type.ToUpper() == "CUSTOMER");

            return SearchCategories(invoiceCategoryFilter, categories);
        }

        public InvoiceLineModel GetInvoiceLineFromSku(string sku, string locale, string countryCode, int quantity, bool isCustomer)
        {
            SKU_V01 skuItem;
            CatalogProvider.GetAllSKU(locale).TryGetValue(sku, out skuItem);
            if (null == skuItem||skuItem.CatalogItem.IsEventTicket==true) return null;
            var retailPrice = isCustomer && (locale == "en-GB" || locale =="ko-KR")
                ? GetCustomerRetailPrice(skuItem.SKU, locale)
                : skuItem.CatalogItem.ListPrice;

            var invoiceLineModel = new InvoiceLineModel
            {
                Sku = skuItem.SKU,
                DisplayCurrencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                RetailPrice = retailPrice,
                ProductCategory = skuItem.CatalogItem.TaxCategory,
                ProductName = string.Format("{0}{1}", skuItem.Product.DisplayName, skuItem.Description),
                ProductType = skuItem.Product.TypeOfProduct.ToString(),
                StockingSku = skuItem.CatalogItem.StockingSKU,
                EarnBase = skuItem.CatalogItem.EarnBase,
                VolumePoint = skuItem.CatalogItem.VolumePoints,
                Quantity = quantity,
                TotalEarnBase = quantity*skuItem.CatalogItem.EarnBase,
                TotalVolumePoint = quantity*skuItem.CatalogItem.VolumePoints,
                TotalRetailPrice = quantity*retailPrice,
                DisplayRetailPrice = retailPrice.FormatPrice(),
                DisplayTotalRetailPrice = (quantity*retailPrice).FormatPrice(),
                DisplayTotalVp = (quantity*skuItem.CatalogItem.VolumePoints).FormatPrice()
            };
            return invoiceLineModel;
        }

        public IEnumerable<InvoiceLineModel> GetInvoiceModelListforAutocomplete(string locale, string countryCode, bool isCustomer)
        {
            var cacheKey = string.Format("Inv_{0}_{1}_{2}", "CacheInvoiceModelList", locale, isCustomer);

            var Result = _cache.Retrieve(_ => InvoiceModelListforAutocomplete(locale, countryCode, isCustomer), cacheKey,
                TimeSpan.FromMinutes(InvoiceCategoryCacheMinutes));
            return Result;
        }

        private static IEnumerable<InvoiceCategoryModel> SearchCategories(
            GetInvoiceCategoryByFilter invoiceCategoryFilter,
            IEnumerable<InvoiceCategoryModel> categories)
        {
            var invoiceCategories = new List<InvoiceCategoryModel>();

            if (null == categories)
            {
                return invoiceCategories;
            }

            var result= categories.Select(c => new InvoiceCategoryModel
            {
                Id = c.Id,
                Name = c.Name,
                RootCategoryId = c.RootCategoryId,
                Products = c.Products.Where(p =>
                    p.ProductName.ToUpper().Contains(invoiceCategoryFilter.Filter.ToUpper()) ||
                    p.Sku == invoiceCategoryFilter.Filter).ToList()
            });

            if (null != result && result.Any())
            {
                return result.Where(r => r.Products.Any());
            }
            return invoiceCategories;
        }

        private IEnumerable<InvoiceLineModel> InvoiceModelListforAutocomplete(string locale, string countryCode, bool isCustomer)
        {
            var productinfocatalog = CatalogProvider.GetProductInfoCatalog(locale);
            var result = new List<InvoiceLineModel>();
            foreach (var category in productinfocatalog.RootCategories.Where(cat => null != cat))
            {
                result.AddRange(GetInvoiceLinesFromCategory(category, locale, countryCode, isCustomer));
            }
            var distinctList =
                result.Where(s => s != null && s.Sku != null).GroupBy(p => p.Sku).Select(g => g.First()).ToList();
            return distinctList;
        }

        private IEnumerable<InvoiceLineModel> GetInvoiceLinesFromCategory(Category_V02 category, string locale,
            string countryCode, bool isCustomer)
        {
            var result = new List<InvoiceLineModel>();
            //set & check the cache.....
            if (null != category.Products)
            {
                foreach (var productInfoV02 in category.Products)
                {
                    if (null != productInfoV02.SKUs && productInfoV02.SKUs.Any())
                    {
                        result.AddRange(
                            productInfoV02.SKUs
                                .Where(x => x != null && x.SKU != null)
                                .Select(p => GetInvoiceLineFromSku(p.SKU, locale, countryCode, 1, isCustomer)
                                ));
                    }
                }
            }
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                foreach (var subcategory in category.SubCategories)
                {
                    result.AddRange(GetInvoiceLinesFromCategory(subcategory, locale, countryCode, isCustomer));
                }
            }
            return result;
        }

        private static List<InvoiceRootCategoryModel> GetRootCategoriesFromSource(string locale)
        {
            var productInfo = CatalogProvider.GetProductInfoCatalog(locale);
            var invoiceRootCategoryModels = new List<InvoiceRootCategoryModel>();
            if (null == productInfo) return invoiceRootCategoryModels;
            var rootCategories = from r in productInfo.RootCategories
                where ShouldTake(r, false)
                select r;
            invoiceRootCategoryModels.AddRange(rootCategories.Select(rootCategory => new InvoiceRootCategoryModel
            {
                Id = rootCategory.ID,
                Name = rootCategory.DisplayName
            }));
            return invoiceRootCategoryModels;
        }

        private  List<InvoiceCategoryModel> GetCategoriesFromSource(int rootCategoryId, string locale, bool isCustomer)
        {
            var productInfo = CatalogProvider.GetProductInfoCatalog(locale);
            var categories = productInfo.RootCategories.Where(c => c.ID == rootCategoryId);
            var category = categories.Any() ? categories.First() : null;
            var prodList = new List<CategoryProductModel>();
            prodList = GetAllProducts(category, category, prodList);
            return ConvertToInvoiceCategoryModel(prodList, rootCategoryId, locale, isCustomer);
        }

        private  List<InvoiceCategoryModel> ConvertToInvoiceCategoryModel(
            IEnumerable<CategoryProductModel> productList, int rootCategoryId, string locale, bool isCustomer)
        {
            var categoryModels = new List<InvoiceCategoryModel>();
            var allSKUS = CatalogProvider.GetAllSKU(locale);
            foreach (var categoryProductModel in productList)
            {
                var categoryModel = new InvoiceCategoryModel
                {
                    RootCategoryId = rootCategoryId,
                    Id = categoryProductModel.Category.ID,
                    Name = Regex.Replace(CatalogHelper.getBreadCrumbText(categoryProductModel.Category,
                        categoryProductModel.RootCategory, categoryProductModel.Product), "<.*?>", string.Empty)
                };

                var products = from p in categoryProductModel.Product.SKUs
                    from a in allSKUS.Keys
                    where a == p.SKU && p.SKU !="9909" 
                               select allSKUS[a];

                var invoiceLineModels = products.Select(product => new InvoiceLineModel
                {
                    Sku = product.SKU,
                    StockingSku = product.CatalogItem.StockingSKU,
                    ProductType = product.CatalogItem.ProductType.ToString(),
                    ProductCategory = product.CatalogItem.TaxCategory,
                    RetailPrice = isCustomer && (locale == "en-GB" || locale == "ko-KR") ? GetCustomerRetailPrice(product.SKU, locale): product.CatalogItem.ListPrice ,
                    DisplayRetailPrice = isCustomer && (locale == "en-GB" || locale == "ko-KR") ? GetCustomerRetailPrice(product.SKU, locale).FormatPrice() : product.CatalogItem.ListPrice.FormatPrice(),
                    ProductName =
                        WebUtility.HtmlDecode(string.Format("{0} {1}",
                        Regex.Replace(categoryProductModel.Product.DisplayName, "<.*?>", string.Empty),
                        Regex.Replace(product.Description, "<.*?>", string.Empty)))
                }).ToList();

                categoryModel.Products = invoiceLineModels;
                categoryModels.Add(categoryModel);
            }
            return categoryModels;
        }

        private static bool ShouldTake(Category_V02 category, bool bEventTicket)
        {
            if (category.Products == null)
                return category.SubCategories != null &&
                       category.SubCategories.Any(sub => ShouldTake(sub, bEventTicket));
            if (category.Products.Any(prod => ShouldTake(prod, bEventTicket)))
            {
                return true;
            }

            return category.SubCategories != null && category.SubCategories.Any(sub => ShouldTake(sub, bEventTicket));
        }

        private static bool ShouldTake(ProductInfo_V02 prod, bool bEventTicket)
        {
            var take = bEventTicket
                ? (prod.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                : (prod.TypeOfProduct != ServiceProvider.CatalogSvc.ProductType.EventTicket);
            if (take)
            {
                take = prod.SKUs.Any(s => s.IsDisplayable);
            }

            return take;
        }

        private static List<CategoryProductModel> GetAllProducts(Category_V02 rooCategory, Category_V02 category,
            List<CategoryProductModel> allProducts)
        {
            if (category.SubCategories != null)
            {
                allProducts = category.SubCategories.Aggregate(allProducts,
                    (current, sub) => GetAllProducts(rooCategory, sub, current));
            }

            if (category.Products != null)
            {
                allProducts.AddRange(from p in category.Products
                    where
                        p.SKUs != null && p.SKUs.Count > 0 &&
                        (from s in p.SKUs where s.IsDisplayable select s).Any()
                    select new CategoryProductModel
                    {
                        Category = category,
                        Product = p,
                        RootCategory = rooCategory,
                    }
                    );
            }

            return allProducts;
        }

        private  decimal GetCustomerRetailPrice(string sku, string locale)
        {
            var taxRates = GetTaxRatesFromTableStorage(locale);
            if (null != taxRates && taxRates.Any())
            {
                var taxRate = taxRates.Where(t => t.SKU == sku);
                if (null != taxRate && taxRate.Any())
                {
                    if (locale == "ko-KR")
                    {
                        return taxRate.FirstOrDefault().Price +
                               ((taxRate.FirstOrDefault().VAT/100)*taxRate.FirstOrDefault().Price);

                    }
                    return taxRate.FirstOrDefault().GermanyPrice;
                }
            }
            return 0;
        }

        public List<TaxRate> GetTaxRatesFromTableStorage(string locale)
        {
            var cacheKey = string.Format("Inv_Customer_TaxRates_{0}", locale);
            var result = _cache.Retrieve(_ => GetTaxRatesFromCache(locale), cacheKey,
                        TimeSpan.FromMinutes(60));
            return result;
        }

        private List<TaxRate> GetTaxRatesFromCache(string locale)
        {
            var request = new GetTaxRateRequestV01 { Locale = locale };
            try
            {
                var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy();
                var response = proxy.GetTaxRate(new GetTaxRateRequest1(request)).GetTaxRateResult;
                if (null != response && response.Status == ServiceProvider.CustomerOrderSvc.ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as GetTaxRateResponse_V01;
                    if (null != responseV01 && responseV01.TaxRates.Any())
                    {
                        return responseV01.TaxRates;
                    }
                }
                LoggerHelper.Error("Invoice Customer pricing GetTaxRatesFromTableStorage return null");
                return new List<TaxRate>();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling GetTaxDataForDwsFromVertex service method {0}",
                        ex.Message));
            }
            return new List<TaxRate>();
        }

    }

    public class CategoryProductModel
    {
        /// <summary>
        ///     Gets or sets Category.
        /// </summary>
        public Category_V02 Category { get; set; }

        /// <summary>
        ///     Gets or sets Product.
        /// </summary>
        public ProductInfo_V02 Product { get; set; }

        /// <summary>
        ///     Gets or sets Root Category.
        /// </summary>
        public Category_V02 RootCategory { get; set; }
    }
}