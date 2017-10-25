using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class CatalogHelper
    {
        /// <summary>
        /// The find category.
        /// </summary>
        /// <param name="ProductInfoCatalog"></param>
        /// <param name="categoryID">
        /// The category id.
        /// </param>
        /// <returns>
        /// </returns>
        public static Category_V02 FindCategory(ProductInfoCatalog_V01 ProductInfoCatalog, int categoryID)
        {
            Category_V02 category;
            if (ProductInfoCatalog.AllCategories.TryGetValue(categoryID, out category))
            {
                return category;
            }

            return null;
        }

        /// <summary>
        /// The find category.
        /// </summary>
        /// <param name="productInfoCatalog"></param>
        /// <param name="categoryID"></param>
        /// <param name="ParentCategoryID"> The category id. </param>
        /// <returns>
        /// </returns>
        /// <param name="rootCategoryID"></param>
        public static Category_V02 FindCategory(ProductInfoCatalog_V01 productInfoCatalog, int categoryID, int ParentCategoryID, int rootCategoryID)
        {
            var varRootCategories = productInfoCatalog.RootCategories.Where(r => r.ID == rootCategoryID);
            if (varRootCategories.Count() > 0)
            {
                Category_V02 rootCategory = varRootCategories.First();
                return CatalogHelper.findCategory(categoryID, ParentCategoryID, rootCategory);

            }

            return null;
        }

        public static Category_V02 findCategory(int categoryID, int ParentCategoryID, Category_V02 category)
        {
            if (category.ID == categoryID)
            {
                return category;
            }
            foreach (Category_V02 c in category.SubCategories)
            {
                if (c.ID == categoryID)
                {
                    return c;
                }
                if (c.SubCategories != null)
                {
                    Category_V02 cat = findCategory(categoryID, ParentCategoryID, c);
                    if (cat != null)
                    {
                        return cat;
                    }
                }
            }
            return null;
        }
        public static Category_V02 getCategory(Category_V02 currentCategory, Category_V02 parentCategory, ref List<Category_V02> listCategory)
        {
            if (parentCategory.SubCategories != null)
            {
                if (!listCategory.Any(l => l.ID == parentCategory.ID))
                    listCategory.Add(parentCategory);
                if (currentCategory.ID == parentCategory.ID)
                {
                    return null;
                }
                var varCategories = parentCategory.SubCategories.Where(s => s.ID == currentCategory.ID);
                if (varCategories.Count() > 0)
                {
                    Category_V02 thisCategory = varCategories.First();
                    if (!listCategory.Any(l => l.ID == thisCategory.ID))
                        listCategory.Add(thisCategory);
                    if (!listCategory.Any(l => l.ID == currentCategory.ID))
                        listCategory.Add(currentCategory);
                    return thisCategory;
                }
                else
                {
                    foreach (Category_V02 c in parentCategory.SubCategories)
                    {
                        List<Category_V02> tmpListCategory = new List<Category_V02>(listCategory);
                        Category_V02 category = getCategory(currentCategory, c, ref tmpListCategory);
                        if (category != null && category == currentCategory)
                        {
                            listCategory = tmpListCategory;
                            return category;
                        }
                    }
                }
            }
            return null;
        }
        public static Category_V02 GetRootCategory(ProductInfoCatalog_V01 productInfoCatalog, int categoryID)
        {
            foreach (Category_V02 cat in productInfoCatalog.RootCategories)
            {
                Category_V02 categoryToFind = findCategoryByID(categoryID, cat);
                if (categoryToFind != null)
                    return cat;
            }

            return null;
        }

        public static Category_V02 findCategoryByID(int categoryID, Category_V02 thisCategory)
        {
            if (thisCategory.ID == categoryID)
                return thisCategory;
            if (thisCategory.SubCategories != null)
            {
                foreach (Category_V02 c in thisCategory.SubCategories)
                {
                    var catFound = findCategoryByID(categoryID, c);
                    if (catFound != null)
                    {
                        return catFound;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// The get product.
        /// </summary>
        /// <param name="categoryID">
        /// The category id.
        /// </param>
        /// <param name="productID">
        /// The product id.
        /// </param>
        /// <returns>
        /// </returns>
        public static ProductInfo_V02 getProduct(ProductInfoCatalog_V01 ProductInfoCatalog, int categoryID, int productID)
        {
            Category_V02 category;
            if (ProductInfoCatalog.AllCategories.TryGetValue(categoryID, out category))
            {
                IEnumerable<ProductInfo_V02> varProd = category.Products.Where(p => p.ID == productID);
                return varProd.Count() > 0 ? varProd.First() : null;
            }

            return null;
        }

        /// <summary>
        /// The get bread crumb.
        /// </summary>
        /// <param name="currentCategory">
        /// The current category.
        /// </param>
        /// <param name="rootCategory">
        /// The root category.
        /// </param>
        /// <returns>
        /// The get bread crumb.
        /// </returns>
        public static string getBreadCrumbText(Category_V02 currentCategory, Category_V02 rootCategory, ProductInfo_V02 product)
        {
            if (currentCategory == null || rootCategory == null)
            {
                return string.Empty;
            }
            try
            {
                Category_V02 category = rootCategory;
                List<Category_V02> listCategory = new List<Category_V02>();
                while (category != null)
                {
                    category = CatalogHelper.getCategory(currentCategory, category, ref listCategory);
                }
                if (rootCategory.SubCategories == null)
                {
                    listCategory.Add(rootCategory);
                }
                return CatalogHelper.getBreadCrumbText(currentCategory, rootCategory, listCategory) + product.DisplayName;
            }
            catch
            {
                LoggerHelper.Error(string.Format("Error getBreadCrumb catID : {0}", currentCategory.ID));
            }
            return string.Empty;

        }

        /// <summary>
        /// get BreadCrumb Text
        /// </summary>
        /// <param name="currentCategory"></param>
        /// <param name="rootCat"></param>
        /// <param name="listCategory"></param>
        /// <returns></returns>
        static string getBreadCrumbText(Category_V02 currentCategory,
            Category_V02 rootCat,
            List<Category_V02> listCategory)
        {
            string breadCrumb = string.Empty;

            for (int idx = 0; idx < listCategory.Count; idx++)
            {
               breadCrumb = breadCrumb + listCategory[idx].DisplayName + " &gt; ";
            }

            return breadCrumb;
        }
    }
}