#region

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.Interfaces;
using System;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Invoice
{
    public class InvoiceEditController : ApiController
    {
        internal IAsyncLoader<InvoiceAddressModel, GetMemberAddressById> _addressLoader;
        internal IAsyncLoader<List<decimal>, GetMemberDiscountsByLocale> _discountsLoader;
        internal IAsyncLoader<Dictionary<string, string>, GetMemberInvoiceErrorListByLocale> _errorLoader;
        internal IInvoiceCatalogProvider _invoiceCatalogProvider;
        internal IInvoiceProvider _invoiceProvider;
        internal IAsyncLoader<IEnumerable<SavedCartViewModel>, GetSavedCartsByFilter> _savedCartsLoader;

        public InvoiceEditController()
        {
            _invoiceCatalogProvider = new InvoiceCatalogProvider();
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);
            _invoiceProvider = invoiceProvider;
            _savedCartsLoader = invoiceProvider;
            _discountsLoader = invoiceProvider;
            _addressLoader = invoiceProvider;
            _errorLoader = invoiceProvider;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel Get(int id)
        {
            return null;
        }

        [HttpPost]
        public UpdateInvoiceModelResult Price(InvoiceModel invoiceModel)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return _invoiceProvider.Price(invoiceModel, memberId, locale, countryCode);
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel CalculateBasicPrice(InvoiceModel invoiceModel)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return _invoiceProvider.CalculateBasicPrice(invoiceModel, locale, countryCode, memberId, true);
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel CalculateTotalDue(InvoiceModel invoiceModel)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return _invoiceProvider.CalculateTotalDue(invoiceModel, locale, countryCode, memberId, true);
        }
        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public bool CheckEmailAndPhone(InvoiceModel invoiceModel)
        {
           
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return _invoiceProvider.CheckEmailAndPhone(invoiceModel, countryCode);
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel CalculateMemberTotal(InvoiceModel invoiceModel)
        {
          
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            return _invoiceProvider.CalculateMemberTotal(invoiceModel, locale, countryCode, memberId);
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel GetInvoiceFromOrderId(CreateInvoiceFromOrderIdRequest createInvoiceFromOrderIdRequest)
        {
            var prevUrl = System.Web.HttpContext.Current.Request.UrlReferrer;
            if (null == createInvoiceFromOrderIdRequest || string.IsNullOrEmpty(createInvoiceFromOrderIdRequest.OrderId))
            {
                return null;
            }
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;

            var result =_invoiceProvider.GetInvoiceFromOrderId(createInvoiceFromOrderIdRequest.OrderId, locale, countryCode,
                memberId, createInvoiceFromOrderIdRequest.Source);
            result.Status = string.IsNullOrEmpty(result.Status) ? "Unpaid" : result.Status;
            result.HasShippingAddress = true;
            return result;
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public InvoiceModel GetInvoiceFromCartId([FromBody] int cartId)
        {
            if (cartId == 0 || cartId < 0)
            {
                return null;
            }
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            var result= _invoiceProvider.GetInvoiceFromCartId(cartId, memberId, locale, countryCode);
            result.HasShippingAddress=true;
            return result;
        }

        [HttpPost]
        [Authorize]
        [WebApiCultureSwitching]
        public UpdateInvoiceModelResult Post(InvoiceModel invoiceModel)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            if (null != invoiceModel)
            {
                invoiceModel.MemberId = memberId.ToUpper();
            }
            var locale = CultureInfo.CurrentUICulture.Name;
            var countryCode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;

            var result = IsAddressValid(invoiceModel.Address)
                ? Price(invoiceModel)
                : CreateUpdateInvoiceResult(invoiceModel);
            if (null != result && result.IsSuccess && null != result.InvoiceModel)
            {
                return _invoiceProvider.Edit(invoiceModel, locale, countryCode);
            }
            else if (invoiceModel.InvoiceShipToAddress && !result.IsSuccess && result.InvoiceModel != null)
            {
                return _invoiceProvider.Edit(invoiceModel, locale, countryCode);
            }
            return result;
        }

        private static UpdateInvoiceModelResult CreateUpdateInvoiceResult(InvoiceModel invoiceModel)
        {
            return new UpdateInvoiceModelResult
            {
                InvoiceModel = invoiceModel,
                IsSuccess = true
            };
        }

        private static bool IsAddressValid(InvoiceAddressModel address)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                return true;
            }
            else
            {
                return !(address == null || string.IsNullOrEmpty(address.Address1) || string.IsNullOrEmpty(address.City) || string.IsNullOrEmpty(address.PostalCode));
            }
          
          
            
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<IEnumerable<SavedCartViewModel>> LoadSavedCarts()
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var savedCarts = await _savedCartsLoader.Load(new GetSavedCartsByFilter
            {
                Filter = string.Empty,
                MemberId = memberId,
                Locale = locale
            });
            return savedCarts;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<IEnumerable<SavedCartViewModel>> FilterSavedCarts(string id)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var savedCarts = await _savedCartsLoader.Load(new GetSavedCartsByFilter
            {
                Filter = id,
                MemberId = memberId,
                Locale = locale
            });
            return savedCarts;
        }

        [HttpGet]
        public async Task<InvoiceModel> Copy(int id)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name.ToUpper().Trim();
            var invoice =
                await _invoiceProvider.Copy(new GetInvoiceById {Id = id, MemberId = memberId, Locale = locale});
            return invoice;
        }
        [HttpGet]
        public async Task<List<decimal>> GetMemberDiscounts()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var discounts =
                await _discountsLoader.Load(new GetMemberDiscountsByLocale {Locale = locale});
            return discounts;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<InvoiceAddressModel> LoadMemberAddress()
        {
            var memberId = User.Identity.Name.ToUpper().Trim();
            var locale = CultureInfo.CurrentUICulture.Name;
            var counrty = locale.Substring(3);
            var address = await _addressLoader.Load(new GetMemberAddressById
            {
                MemberId = memberId,
                Locale = locale,
                Country = counrty
            });
            return address;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<List<DisplayInvoiceAddressModel>> GetShippingAddress()
        {
            var memberId = User.Identity.Name.ToUpper().Trim();;
            var locale = CultureInfo.CurrentUICulture.Name;

            var addList = await _invoiceProvider.GetShippingAddresses(memberId, locale);
            return addList;
        }

        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<InvoiceAddressModel> FilterShippingAddress(int id)
        {
            var memberId = User.Identity.Name.ToUpper().Trim();;
            var locale = CultureInfo.CurrentUICulture.Name;
            var address = await _invoiceProvider.FilterGetShippingAddresses(memberId, locale, id);
            return address;
        }


        [HttpGet]
        [Authorize]
        [WebApiCultureSwitching]
        public async Task<Dictionary<string, string>> LoadErrorMessage()
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var errorMsgs = await _errorLoader.Load(new GetMemberInvoiceErrorListByLocale
            {
                Locale = locale
            });
            return errorMsgs;
        }
    }
}