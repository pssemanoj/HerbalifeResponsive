#region

using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IInvoiceCatalogProvider
    {
        IEnumerable<InvoiceLineModel> GetInvoiceModelListforAutocomplete(string locale, string countryCode,
            bool isCustomer);

        List<InvoiceRootCategoryModel> GetRootCategories(string locale);
        List<InvoiceCategoryModel> GetCategories(int rootCategoryId, string locale, bool isCustomer);
        IEnumerable<InvoiceCategoryModel> SearchCategories(GetInvoiceCategoryByFilter invoiceCategoryFilter);

        InvoiceLineModel GetInvoiceLineFromSku(string sku, string locale, string countryCode, int quantity,
            bool isCustomer);
    }
}