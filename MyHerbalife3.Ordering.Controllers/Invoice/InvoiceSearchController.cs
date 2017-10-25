#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Invoice
{
    public class InvoiceSearchController : ApiController
    {
        internal IAsyncLoader<InvoiceModel, GetInvoiceById> _invoiceLoader;
        internal IInvoiceProvider _invoiceProvider;
        internal IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter> _invoicesLoader;
        internal ILocalizationManager _localization;


        public InvoiceSearchController(ILocalizationManager localizationManager)
        {
            _localization = localizationManager;
        }

        public InvoiceSearchController()
            : this(new LocalizationManager())
        {
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);
            _invoicesLoader = invoiceLoader;
            _invoiceLoader = invoiceProvider;
            _invoiceProvider = invoiceProvider;
        }

        [HttpPost]
        [Authorize]
        public void Delete(int id)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            _invoiceProvider.Delete(id, memberId, locale);
        }

        [HttpGet]
        public async Task<List<InvoiceModel>> LoadAll()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            var invoices =
                await
                    _invoicesLoader.Load(new GetInvoicesByFilter
                    {
                        InvoiceFilterModel = null,
                        MemberId = memberId,
                        Locale = locale
                    });
            return invoices;
        }
        [HttpGet]
        [Authorize]
        public async Task<List<InvoiceCRMConatactModel>> GetCRMAddress()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            var invoices =
                await
                    _invoiceProvider.GetCRMAddress(new GetCRMAddress
                    {
                        MemberId = memberId,
                        CountryCode = locale.Substring(3)                    
                    });
            return invoices;
        }
    
    [HttpPost]
    [Authorize]
    public async Task<SaveUpdateResponseModel> SaveUpdate(InvoiceCRMConatactModel invoiceCRMConatactModel)
    {
        var locale = CultureInfo.CurrentUICulture.Name;
        var memberId = User.Identity.Name.ToUpper().Trim();
        var invoices =
            await
                _invoiceProvider.SaveUpdate(invoiceCRMConatactModel, memberId);
        return invoices;
    }

    [HttpPost]
        public async Task<List<InvoiceModel>> Filter(InvoiceFilterModel invoiceFilterModel)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            var invoices =
                await
                    _invoiceProvider.Filter(new GetInvoicesByFilter
                    {
                        InvoiceFilterModel = invoiceFilterModel,
                        MemberId = memberId,
                        Locale = locale
                    });
            return invoices;
        }
       
        [HttpGet]
        [Authorize]
        public async Task<InvoiceModel> Load(int id)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var invoice =
                await _invoiceLoader.Load(new GetInvoiceById {Id = id, MemberId = memberId, Locale = locale});
            if (invoice.MemberFirstName == null || invoice.MemberEmailAddress == null)
            {
                InvoiceProvider.LoadMemberInfo(invoice);
            }
            if (invoice.ReceiptChannel== "ClubSaleReceipt" || invoice.ReceiptChannel == "Club Visit/Sale")
            {
                if (invoice.ClubInvoice == null)
                {
                    ConvertToClubInvoiceLines(invoice.InvoiceLines, invoice);
                }
                else
                {
                    invoice.InvoiceLines = new List<InvoiceLineModel>();
                    invoice.InvoicePrice = new InvoicePriceModel();
                }
            }
            if(!HLConfigManager.Configurations.DOConfiguration.AddressOnInvoice)
            {
                invoice.MemberAddress = new InvoiceAddressModel();
            }
            return invoice;
        }

        private ClubInvoiceModel ConvertToClubInvoiceLines(List<InvoiceLineModel> invoiceLines, InvoiceModel invoice)
        {
            ClubInvoiceModel cm = new ClubInvoiceModel();
            cm.ClubRecieptDisplayTotalDue = invoice.InvoicePrice.TotalDue.FormatPrice().ToString();
            cm.ClubRecieptQuantity = invoiceLines.FirstOrDefault().Quantity.ToString();
            cm.ClubRecieptTotalVolumePoints = invoiceLines.FirstOrDefault().TotalVolumePoint.ToString();
            cm.ClubRecieptProductName = invoiceLines.FirstOrDefault().ProductName;          
            return cm;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public List<InvoiceFilterDataModel> LoadFilterCategory()
        {
            var filterCriterias = (from object invoiceFilterCriteria in Enum.GetValues(typeof (InvoiceFilterCriteria))
                select new InvoiceFilterDataModel
                {
                    Id = (int) invoiceFilterCriteria,
                    Name = invoiceFilterCriteria.ToString(),
                    DisplayName =
                        _localization.GetString("~/Views/Invoice/Index.cshtml",
                            invoiceFilterCriteria.ToString(),
                            CultureInfo.CurrentUICulture)
                }).ToList();
            return filterCriterias;
        }
    }
}