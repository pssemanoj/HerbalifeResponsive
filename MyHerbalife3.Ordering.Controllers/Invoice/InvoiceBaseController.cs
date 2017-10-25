#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Model.Invoice;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Invoice
{
    public class InvoiceBaseController : ApiController
    {
        internal IInvoiceCatalogProvider _invoiceCatalogProvider;
        internal IAsyncLoader<InvoiceModel, GetInvoiceById> _invoiceLoader;
        internal IInvoiceProvider _invoiceProvider;

        internal ILocalizationManager _localization;
        internal IAsyncLoader<List<InvoiceFilterDataModel>, string> _statesLoader;
        internal IAsyncLoader<List<InvoiceFilterDataModel>, GetInvoiceStatusByLocale> _statusLoader;
        internal IAsyncLoader<List<InvoiceFilterDataModel>, GetInvoicePaymentTypeByLocale> _PaymentTypeLoader;


        public InvoiceBaseController(ILocalizationManager localizationManager)
        {
            _localization = localizationManager;
        }

        public InvoiceBaseController()
            : this(new LocalizationManager())
        {
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);
            _invoiceLoader = invoiceProvider;

            _invoiceProvider = invoiceProvider;
            _statesLoader = invoiceProvider;
            _statusLoader = invoiceProvider;
            _PaymentTypeLoader = invoiceProvider;
            _invoiceCatalogProvider = new InvoiceCatalogProvider();
        }


        [HttpGet]
        public async Task<List<InvoiceFilterDataModel>> LoadStates()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            return await _statesLoader.Load(locale);
        }


        [HttpGet]
        public async Task<List<InvoiceFilterDataModel>> LoadStatus()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            return await _statusLoader.Load(new GetInvoiceStatusByLocale {Locale = locale});
        }
        [HttpGet]
        public async Task<List<InvoiceFilterDataModel>> LoadPaymentType()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            return await _PaymentTypeLoader.Load(new GetInvoicePaymentTypeByLocale { Locale = locale });
        }

        [HttpGet]
        public IEnumerable<InvoiceLineModel> LoadProductsForAutocomplete(string startswith, string type)
        {
            var isCustomer = !string.IsNullOrEmpty(type) && type.ToUpper() == "CUSTOMER";
            var locale = CultureInfo.CurrentUICulture.Name;
            var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return string.IsNullOrEmpty(startswith)
                ? _invoiceCatalogProvider.GetInvoiceModelListforAutocomplete(locale, countrycode, isCustomer)
                : _invoiceCatalogProvider.GetInvoiceModelListforAutocomplete(locale, countrycode, isCustomer)
                    .Where(
                        s =>
                            s != null && s.Sku != null &&
                            (s.Sku.StartsWith(startswith) ||
                             (!string.IsNullOrEmpty(s.ProductName) &&
                              s.ProductName.ToUpper().Contains(startswith.ToUpper()))));
        }

        [HttpGet]
        public InvoiceLineModel GetInvoiceLineModel(string id, string type)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            var item =
                _invoiceCatalogProvider.GetInvoiceModelListforAutocomplete(locale, countrycode,
                    !string.IsNullOrEmpty(type) && type.ToUpper() == "CUSTOMER")
                    .FirstOrDefault(x => x != null && x.Sku == id);
            return item;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public List<InvoiceRootCategoryModel> LoadRootCategories()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            return _invoiceCatalogProvider.GetRootCategories(locale);
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public List<InvoiceCategoryModel> LoadCategories(int id, string type)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            return _invoiceCatalogProvider.GetCategories(id, locale,
                !string.IsNullOrEmpty(type) && type.ToUpper() == "CUSTOMER");
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public IEnumerable<InvoiceCategoryModel> SearchCategories(GetInvoiceCategoryByFilter invoiceCategoryFilter)
        {
            invoiceCategoryFilter.Locale = CultureInfo.CurrentUICulture.Name;
            return _invoiceCatalogProvider.SearchCategories(invoiceCategoryFilter);
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public bool SendInvoiceEmail(InvoiceModel invoice)
        {
            return _invoiceProvider.SendInvoiceEmail(invoice);
        }
        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public bool SendInvoiceSMS(InvoiceModel invoice)
        {
            return _invoiceProvider.SendInvoiceSMS(invoice);
        }
        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public bool UpdateStatus(UpdateStatusModel model)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            return _invoiceProvider.UpdateStatus(model.Id, model.Status, memberId, locale);
        }
    }
}