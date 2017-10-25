#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using ExpertPdf.HtmlToPdf;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Communication;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.ViewModel.Models;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.Invoices.Shipping;
using MyHerbalife3.Ordering.Invoices.Price;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class InvoiceProvider : IInvoiceProvider,
        IAsyncLoader<InvoiceModel, GetInvoiceById>, IAsyncLoader<IEnumerable<SavedCartViewModel>, GetSavedCartsByFilter>,
        IAsyncLoader<List<InvoiceFilterDataModel>, string>,
        IAsyncLoader<List<InvoiceFilterDataModel>, GetInvoiceStatusByLocale>,
         IAsyncLoader<List<InvoiceFilterDataModel>, GetInvoicePaymentTypeByLocale>,
        IAsyncLoader<List<decimal>, GetMemberDiscountsByLocale>,
        IAsyncLoader<InvoiceAddressModel, GetMemberAddressById>,
        IAsyncLoader<Dictionary<string, string>, GetMemberInvoiceErrorListByLocale>,
        IAsyncLoader<List<InvoiceFilterDataModel>, GetInvoiceTypesByLocale>
    {
        public const int InvoiceCacheMinutes = 60;
        private const string AllInvoiceCachePrefix = "Member_InvAll_{0}_{1}";
        private const string InvoiceModelCachePrefix = "Member_InvModel_{0}_{1}_{2}";
        private const string InvoiceModelStatesCachePrefix = "Member_Inv_States_{0}";
        private const string InvoiceModelStatusCachePrefix = "Member_Inv_Status_{0}";
        private const string InvoiceModelTypesCachePrefix = "Member_Inv_Types_{0}";
        private const string InvoiceModelPaymentTypesCachePrefix = "Member_Inv_PaymentTypes_{0}";
        private const string CRMContactsCachePrefix = "CRMContacts_{0}";
        private static string _orderEndPoint;
        private static string _communicationEndPoint;
        private readonly ISimpleCache _cache = CacheFactory.Create();
        private readonly IInvoiceCatalogProvider _invoiceCatalogProvider;
        private readonly InvoiceConverter _invoiceConverter;
        private readonly IAsyncLoader<InvoiceModel, GetInvoiceById> _invoiceLoader;
        private readonly IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter> _invoicesLoader;
        private readonly IShippingProvider _iShippingProvider;

        public InvoiceProvider(IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter> invoicesLoader,
            IAsyncLoader<InvoiceModel, GetInvoiceById> invoiceLoader,
            InvoiceConverter invoiceConverter)
        {
            var country = CultureInfo.CurrentUICulture.Name.Substring(3, 2);
            _invoiceCatalogProvider = new InvoiceCatalogProvider();
            _iShippingProvider = ShippingProvider.GetShippingProvider(country);
            _invoicesLoader = invoicesLoader;
            _invoiceLoader = invoiceLoader;
            _invoiceConverter = invoiceConverter;
        }

        public Task<Dictionary<string, string>> Load(GetMemberInvoiceErrorListByLocale query)
        {
            return Task<Dictionary<string, string>>.Factory.StartNew(() =>
            {
                var fileName = query.Locale == "en-US" ? "MemberInvoiceError" : "MemberInvoiceError." + query.Locale;
                var list =
                    GlobalResourceHelper.GetGlobalEnumeratorElements(fileName,
                        new CultureInfo(query.Locale));
                return list.ToDictionary(value => value.Key, value => value.Value);
            });
        }
      
        public Task<IEnumerable<SavedCartViewModel>> Load(GetSavedCartsByFilter query)
        {
            if (null == query)
            {
                return null;
            }
            if (string.IsNullOrEmpty(query.MemberId) || string.IsNullOrEmpty(query.Locale))
            {
                return null;
            }

            var result =
                Task<IEnumerable<SavedCartViewModel>>.Factory.StartNew(
                    () => LoadSavedCarts(query.MemberId, query.Locale, query.Filter));
            return result;
        }

        public Task<InvoiceAddressModel> Load(GetMemberAddressById query)
        {
            var task =
                Task<InvoiceAddressModel>.Factory.StartNew(
                    () => LoadMemberAddress(query.MemberId, query.Locale, query.Country));
            return task;
        }


        public Task<InvoiceModel> Load(GetInvoiceById query)
        {
            if (null == query)
            {
                return null;
            }

            var result =
                Task<InvoiceModel>.Factory.StartNew(
                    () => FilterInvoicesById(query.Id, query.MemberId, query.Locale));
            return result;
        }

        public Task<List<decimal>> Load(GetMemberDiscountsByLocale query)
        {
            var task = Task<List<decimal>>.Factory.StartNew(() => GetMemberDiscounts(query.Locale));
            return task;
        }

        public Task<List<InvoiceFilterDataModel>> Load(GetInvoiceStatusByLocale query)
        {
            var cacheKey = string.Format(InvoiceModelStatusCachePrefix, query.Locale);
            var result =
                Task<List<InvoiceFilterDataModel>>.Factory.StartNew(
                    () => _cache.Retrieve(_ => GetInvoiceStatus(query.Locale), cacheKey,
                        TimeSpan.FromMinutes(60)));
            return result;
        }
        public Task<List<InvoiceFilterDataModel>> Load(GetInvoicePaymentTypeByLocale query)
        {
            var cacheKey = string.Format(InvoiceModelPaymentTypesCachePrefix, query.Locale);
            var result =
                Task<List<InvoiceFilterDataModel>>.Factory.StartNew(
                    () => _cache.Retrieve(_ => GetPaymentType(query.Locale), cacheKey,
                        TimeSpan.FromMinutes(60)));
            return result;
        }

        public Task<List<InvoiceFilterDataModel>> Load(GetInvoiceTypesByLocale query)
        {
            var cacheKey = string.Format(InvoiceModelTypesCachePrefix, query.Locale);
            var result =
                Task<List<InvoiceFilterDataModel>>.Factory.StartNew(
                    () => _cache.Retrieve(_ => GetInvoiceTypes(query.Locale), cacheKey,
                        TimeSpan.FromMinutes(60)));
            return result;
        }

        public Task<List<InvoiceFilterDataModel>> Load(string locale)
        {
            var cacheKey = string.Format(InvoiceModelStatesCachePrefix, locale);
            var result =
                Task<List<InvoiceFilterDataModel>>.Factory.StartNew(
                    () => _cache.Retrieve(_ => GetStates(locale), cacheKey,
                        TimeSpan.FromMinutes(60)));
            return result;
        }

        public Task<List<DisplayInvoiceAddressModel>> GetShippingAddresses(string memberId, string locale)
        {
            var shippingAddresses = _iShippingProvider.GetShippingAddresses(memberId, locale);
            if (shippingAddresses != null && shippingAddresses.Count > 0)
            {
                var list = Task<List<DisplayInvoiceAddressModel>>.Factory.StartNew(
                    () => convertListToMemberAddress(shippingAddresses));
                return list;
            }
            return null;
        }

        public Task<InvoiceAddressModel> FilterGetShippingAddresses(string memberId, string locale, int id)
        {
            var shippingAddresses = _iShippingProvider.GetShippingAddresses(memberId, locale);
            if (shippingAddresses != null && shippingAddresses.Count > 0)
            {
                var address = Task<InvoiceAddressModel>.Factory.StartNew(
                    () => convertToMemberAddress(shippingAddresses.First(x => x.Id == id)));
                return address;
            }
            return null;
        }


        public UpdateInvoiceModelResult Edit(InvoiceModel invoiceModel, string locale, string countryCode)
        {
            if (null == invoiceModel)
            {
                LoggerHelper.Error("invoiceModel is null - Edit InvoiceProvider");
                return null;
            }
            invoiceModel.ApplicationCountryCode = countryCode;
            var invoiceModelResult = SaveInvoice(invoiceModel, locale, countryCode);
            //if (HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ||
            //    (!invoiceModelResult.IsSuccess)) return invoiceModelResult;
          //  NotificationProvider.InsertCustomerReceipt(invoiceModel, locale);
           // SendInvoiceEmail(invoiceModel);
            return invoiceModelResult;
        }

   
        public bool Delete(int id, string memberId, string locale)
        {
            try
            {
                if (id > 0)
                {
                    return DeleteInvoice(id, memberId, locale);
                }
                LoggerHelper.Error("invoiceModel is null - Delete InvoiceProvider");
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "invoiceModel.InvoiceNumber is not long valid numbers - Delete InvoiceProvider, Ex: {0}",
                        ex.Message));
                return false;
            }
        }


        public InvoiceModel CalculateTotalDue(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId, bool isReset)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateTotalDue(invoiceModel, memberId, locale, countryCode, isReset);
        }
        public bool CheckEmailAndPhone(InvoiceModel invoiceModel,string countryCode)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                var InvoiceModelResult = new UpdateInvoiceModelResult();
                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();                  
                var DistributorProfile = DistributorOrderingProfileProvider.GetProfile(membershipUser.Value.Id, countryCode); 
                List<string> Email = new List<string>();
                List<string> Phone = new List<string>();
                foreach (var email in DistributorProfile.EmailAddresses)
                {
                    if(!string.IsNullOrEmpty( email.Address))
                       Email.Add(email.Address.ToString().ToUpper());
                }
                foreach (var phone in DistributorProfile.PhoneNumbers)
                {
                    if (!string.IsNullOrEmpty(phone.Number))
                        Phone.Add(phone.Number.ToString().Replace("(", string.Empty)
                            .Replace(")", string.Empty)
                            .Replace(" ", string.Empty)
                            .Replace("-", string.Empty));
                }
                if (invoiceModel.Email != null)
                {
                    if (Email.Contains(invoiceModel.Email.ToUpper()))                  
                        return false;                  
                }
                if (invoiceModel.Phone != null)
                {
                    if (Phone.Contains(invoiceModel.Phone))                   
                        return false;                       
                }
            }
            else
            {
                return true;
            }
            return true;
        }

        public InvoiceModel CalculateBasicPrice(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId, bool isReset)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateBasicPrice(invoiceModel, memberId, locale, countryCode, isReset);
        }

        public InvoiceModel CalculateMemberTotal(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateMemberTotal(invoiceModel, memberId, locale, countryCode);
        }


        public InvoiceModel CalculateDistributorPriceOnCustomerSection(InvoiceModel invoiceModel, string locale,
            string countryCode,
            string memberId)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateDistributorPriceOnCustomerSection(invoiceModel, memberId, locale,
                countryCode);
        }

        public InvoiceModel CalculateModifiedPriceOnCustomerSection(InvoiceModel invoiceModel, string locale,
            string countryCode,
            string memberId)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateModifiedPriceOnCustomerSection(invoiceModel, memberId, locale,
                countryCode);
        }

        public UpdateInvoiceModelResult Price(InvoiceModel invoice, string memberId, string locale, string countryCode)
        {
            var updateInvoiceModelResult = new UpdateInvoiceModelResult();
            ServiceProvider.ShippingSvc.Address_V01 outAddress = null;
            ServiceProvider.ShippingSvc.Address_V01 outMemberAddress = null;
            
            if (invoice.ReceiptChannel == "ClubSaleReceipt" ||  invoice.ReceiptChannel == "Club Visit/Sale")
            {
                var qty = Convert.ToInt32(invoice.ClubInvoice.ClubRecieptQuantity);
                var VP = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptTotalVolumePoints, CultureInfo.InvariantCulture);
                var Price = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture);
                invoice.InvoiceLines.Add(new InvoiceLineModel()
                {
                    ProductName = invoice.ClubInvoice.ClubRecieptProductName,
                    Quantity = Convert.ToInt32(invoice.ClubInvoice.ClubRecieptQuantity),
                    VolumePoint = VP / qty,
                    RetailPrice= Price/qty,
                    TotalVolumePoint = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptTotalVolumePoints, CultureInfo.InvariantCulture),
                    YourPrice = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture),
                    TotalRetailPrice = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture),
                    CalcDiscountedAmount= Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture),
                    DisplayTotalRetailPrice = invoice.ClubInvoice.ClubRecieptDisplayTotalDue.ToString(),
                    Sku ="9999"
                });
                invoice.InvoicePrice.TotalDue = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture);
                invoice.InvoicePrice.TotalYourPrice = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture);
                invoice.InvoicePrice.CalcDiscountAmount = 0;
                invoice.InvoicePrice.SubTotal=Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture);
                invoice.InvoicePrice.TaxAmount = 0;
                invoice.InvoicePrice.ShippingAmount = 0;
                invoice.InvoicePrice.TotalVolumePoints = Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptTotalVolumePoints, CultureInfo.InvariantCulture);

                invoice.InvoicePrice.DisplayCalculatedTax = "0.00";
                invoice.InvoicePrice.DisplayDiscountedAmount = "0.00";
                invoice.InvoicePrice.DisplayMemberFreight = "0.00";
                invoice.InvoicePrice.DisplayMemberTax = "0.00";
                invoice.InvoicePrice.DisplayMemberTotal = invoice.ClubInvoice.ClubRecieptDisplayTotalDue.ToString();
                invoice.InvoicePrice.DisplayShipping = "0.00";
                invoice.InvoicePrice.DisplaySubtotal = invoice.ClubInvoice.ClubRecieptDisplayTotalDue.ToString();
                invoice.InvoicePrice.DisplayTax = "0.00";
                invoice.InvoicePrice.DisplayTotalDue = invoice.ClubInvoice.ClubRecieptDisplayTotalDue.ToString();
                invoice.InvoicePrice.DisplayTotalYourPrice = invoice.ClubInvoice.ClubRecieptDisplayTotalDue.ToString();


            }
            if (HLConfigManager.Configurations.DOConfiguration.InvoiceHasHMSCal)
            {
                if (invoice.InvoiceShipToAddress && !ValidateAddress(invoice.Address, countryCode, out outAddress))
                {
                    updateInvoiceModelResult.InvoiceModel = invoice;
                    updateInvoiceModelResult.IsSuccess = false;
                    updateInvoiceModelResult.ErrorCodeKey = "invalidAddressPricing";

                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.ShippingAmount = 0M;
                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.TaxAmount = 0M;
                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.ShippingPercentage = 0;
                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.TaxPercentage = 0;
                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.DisplayShipping =
                        updateInvoiceModelResult.InvoiceModel.InvoicePrice.DisplayCurrencySymbol +
                        updateInvoiceModelResult.InvoiceModel.InvoicePrice.ShippingAmount.ToString("N2");
                    updateInvoiceModelResult.InvoiceModel.InvoicePrice.DisplayCalculatedTax =
                        updateInvoiceModelResult.InvoiceModel.InvoicePrice.DisplayCurrencySymbol +
                        updateInvoiceModelResult.InvoiceModel.InvoicePrice.TaxAmount.ToString("N2");
                    return updateInvoiceModelResult;
                }

                if (!invoice.InvoiceShipToAddress &&
                    !ValidateAddress(invoice.MemberAddress, countryCode, out outMemberAddress))
                {
                    updateInvoiceModelResult.InvoiceModel = invoice;
                    updateInvoiceModelResult.IsSuccess = false;
                    updateInvoiceModelResult.ErrorCodeKey = "invalidAddressPricing";
                    return updateInvoiceModelResult;
                }
                if (ValidateAddress(invoice.Address, countryCode, out outAddress))
                {
                    invoice.Address = outAddress != null
                        ? ConvertToInvoiceAddress(outAddress, countryCode)
                        : invoice.Address;
                }
                if (invoice.InvoiceShipToAddress)
                {
                    invoice.Address = ConvertToInvoiceAddress(outAddress, countryCode);
                }
                else
                {
                    invoice.MemberAddress = ConvertToInvoiceAddress(outMemberAddress, countryCode);
                }
            }

            if (invoice.ReceiptChannel == "ClubSaleReceipt" || invoice.ReceiptChannel == "Club Visit/Sale")
            {
                invoice.PricingType = "Customer";
                updateInvoiceModelResult.ErrorCodeKey = string.Empty;
                updateInvoiceModelResult.InvoiceModel = invoice;
                updateInvoiceModelResult.IsSuccess = true;
                return updateInvoiceModelResult;
            }

            var model = CalculateBasicPrice(invoice, locale, countryCode, memberId, false);
            
            if (HLConfigManager.Configurations.DOConfiguration.InvoiceHasHMSCal)
            {
                model = CalculateDistributorPrice(model, locale, countryCode, memberId);
            }

            //if (model.Type == "Customer" && model.OrderSource == "GDO")
            //{
            //    model.PricingType = "Modified";
            //    model = CalculateModifiedPriceOnCustomerSection(invoice, locale, countryCode, memberId);
            //}
            //else
            if (model.Type == "Distributor")
            {
                model.PricingType = "DistrubutorWithDiscount";
                model = CalculateDistributorPriceOnCustomerSection(invoice, locale, countryCode, memberId);
            }
            else
            {
                model.PricingType = "Customer";
                model = CalculateCustomerPrice(invoice, locale, countryCode, memberId);
            }

            updateInvoiceModelResult.ErrorCodeKey = string.Empty;
            updateInvoiceModelResult.InvoiceModel = model;
            updateInvoiceModelResult.IsSuccess = true;
            return updateInvoiceModelResult;
        }

        public InvoiceModel CalculateCustomerPrice(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateCustomerPrice(invoiceModel, memberId, locale, countryCode);
        }

        public InvoiceModel CalculateDistributorPrice(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId)
        {
            if (null == invoiceModel || null == invoiceModel.Address || null == invoiceModel.InvoiceLines)
            {
                LoggerHelper.Error(
                    "invoiceModel is null Price InvoiceProvider");
                return null;
            }

            var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
            return invoicePriceProvider.CalculateDistributorPrice(invoiceModel, memberId, locale, countryCode);
        }


        public bool ExpireCache(string memberId, string locale)
        {
            try
            {
                var cacheKey = string.Format(AllInvoiceCachePrefix, memberId, locale);
                _cache.Expire(typeof(Task<List<InvoiceModel>>), cacheKey);
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("OrderService: Fail to clear GetInvoices cache for DistributorID: " + memberId +
                                   " \n");
            }
            return false;
        }

        public InvoiceModel GetInvoiceFromOrderId(string orderId, string locale, string countryCode, string memberId,
            string source = null)
        {
            var shoppingCart = ShoppingCartProvider.GetShoppingCart(memberId, locale, orderId);
            if (shoppingCart != null)
            {
                var invoiceModel = ConvertShoppingCartToInvoiceModel(shoppingCart, locale, countryCode, memberId);
                var invoicePriceProvider = GetInvoicePriceProvider(countryCode);
                invoiceModel = invoicePriceProvider.CalculateDistributorPrice(invoiceModel, memberId, locale, countryCode);
                if (HttpContext.Current.Request.UrlReferrer != null)
                {
                    var uri = HttpContext.Current.Request.UrlReferrer.ToString();
                    if (uri.Contains("CreateByOrderId"))
                    {
                        invoiceModel.InvoicePrice.TaxPercentage = 0;
                        invoiceModel.InvoicePrice.TaxAmount = 0;
                        invoiceModel.InvoicePrice.ShippingPercentage = 0;
                        invoiceModel.InvoicePrice.ShippingAmount = 0;
                    }
                }
              
                return invoiceModel;
            }
            var order = OrdersProvider.GetOrderDetail(orderId);
            if (null != order && null != order.PurchaserInfo && order.PurchaserInfo.PurchaserId != memberId)
            {
                return null;
            }
            return ConvertOrderToInvoiceModel(order, locale, countryCode, memberId, source);
        }

        public InvoiceModel GetInvoiceFromCartId(int cartId, string memberId, string locale, string countryCode)
        {
            var shoppingCart = ShoppingCartProvider.GetShoppingCart(memberId, locale, cartId);
            return ConvertShoppingCartToInvoiceModel(shoppingCart, locale, countryCode, memberId);
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            var pdfConverter = new PdfConverter
            {
                LicenseKey = HL.Common.Configuration.Settings.GetRequiredAppSetting("Invoice_Pdf"),
                InternetSecurityZone = InternetSecurityZone.LocalMachine
            };
            var pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(htmlContent);
            return pdfBytes;
        }

        public Task<InvoiceModel> Copy(GetInvoiceById request)
        {
            var task = Load(request);
            if (null != task && null != task.Result)
            {
                var model = task.Result.Clone() as InvoiceModel;
                model.InvoicePrice = model.InvoicePrice.Clone() as InvoicePriceModel;
                model.Address = model.Address.Clone() as InvoiceAddressModel;
                model.InvoiceLines = model.InvoiceLines.Select(x => (InvoiceLineModel)x.Clone()).ToList();

                return Task<InvoiceModel>.Factory.StartNew(
                    () => ClearIds(model));
            }
            return task;
        }
        public bool SendInvoiceSMS(InvoiceModel invoice)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                LoadMemberInfo(invoice);
                string locale = CultureInfo.CurrentCulture.ToString();
                var result = NotificationProvider.SendInvoiceSMS(invoice, locale);
            }
            return true;
          
        }

        public bool SendInvoiceEmail(InvoiceModel invoice)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                LoadMemberInfo(invoice);
                string locale = CultureInfo.CurrentCulture.ToString();
                var result =  NotificationProvider.InsertCustomerReceipt(invoice, locale);
                invoice.NotificationURL = result.AcknowledgeUrl;
            }
            
            try
            {
                return CommunicationSvcProvider.SendInvoiceEmail(invoice);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "InvoiceProvider - SendInvoiceEmail - : An error occured while send invoice Email invoiceid:{0}, member id: {1} invoice error: {2}",
                        invoice.DisplayMemberInvoiceNumber, invoice.MemberId, ex.Message));
                return false;
                throw ex;
            }
        }

        public Task<List<InvoiceModel>> Filter(GetInvoicesByFilter query)
        {
            if (null == query)
            {
                return null;
            }
            var invoices = GetInvoicesFromCache(query.MemberId, query.Locale,
                query.InvoiceFilterModel != null ? query.InvoiceFilterModel.From : null,
                query.InvoiceFilterModel != null ? query.InvoiceFilterModel.To : null);
            if (null == invoices)
            {
                return null;
            }

            var result =
                Task<List<InvoiceModel>>.Factory.StartNew(
                    () => FilterInvoices(invoices.Result, query.InvoiceFilterModel, query.MemberId, query.Locale));
            return result;
        }
        public Task<List<InvoiceCRMConatactModel>> GetCRMAddress(GetCRMAddress value)
        {
            if (null == value)
            {
                return null;
            }
            var cacheKey = string.Format(CRMContactsCachePrefix, value.MemberId);
            var result =
               Task<List<InvoiceCRMConatactModel>>.Factory.StartNew(
                   () => GetCRMAddress(value.MemberId,value.CountryCode));

            return result;
           
        }
        public Task<SaveUpdateResponseModel> SaveUpdate(InvoiceCRMConatactModel contact, string memberId)
        {
            if (null == contact)
            {
                return null;
            }
            ExpireInvoiceCRMConatactModelCache(memberId);
            var result =
                Task<SaveUpdateResponseModel>.Factory.StartNew(
                    () => CrmSaveUpdate(contact, memberId));

            return result;
        }
        private static SaveUpdateResponseModel CrmSaveUpdate(InvoiceCRMConatactModel contact, string memberId)
        {
            if (null == contact)
            {
                return null;
            }
            var result = DistributorContactProvider.SaveUpdate(contact,memberId);
            return result;
        }

        public bool UpdateStatus(int id, string status, string memberId, string locale)
        {
            try
            {
                if (id > 0)
                {
                    return UpdateInvoiceStatus(id, status, memberId, locale);
                }
                LoggerHelper.Error("invoiceModel id is null - UpdateStatus InvoiceProvider");
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "invoiceModel.Id is not long valid numbers - UpdateStatus InvoiceProvider, Ex: {0}",
                        ex.Message));
                return false;
            }
        }

        private bool UpdateInvoiceStatus(int id, string status, string memberId, string locale)
        {
            MemberInvoiceStatus memberInvoiceStatus;
            if (!Enum.TryParse(status, true, out memberInvoiceStatus))
            {
                LoggerHelper.Error("ConvertToMemberInvoice - InvoiceProvider: InvoiceStatus parse error");
                return false;
            }
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new SaveMemberInvoiceStatusRequest_V01
                {
                    Id = id,
                    Status = memberInvoiceStatus
                };
                var response = proxy.SaveMemberInvoiceStatus(new SaveMemberInvoiceStatusRequest1(request)).SaveMemberInvoiceStatusResult as SaveMemberInvoiceStatusResponse_V01;
                if (response != null && response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success && response.IsUpdated)
                {
                    ExpireCache(memberId, locale);
                    return response.IsUpdated;
                }
                return false;
            }
            catch
            {
                LoggerHelper.Error(
                    string.Format(
                        "Unable to Delete Invoice - UpdateInvoiceStatus method InvoiceProvider memberID: {0}, InvoiceID {1}: ",
                        memberId, id));
            }
            return false;
        }

        private List<DisplayInvoiceAddressModel> convertListToMemberAddress(List<DeliveryOption> address)
        {
            var listAddress = from x in address
                select new DisplayInvoiceAddressModel
                {
                    Address1 = x.Address.Line1,
                    Address2 = x.Address.Line2,
                    City = x.Address.City,
                    Country = x.Address.Country,
                    County = x.Address.CountyDistrict,
                    PostalCode = x.Address.PostalCode,
                    State = x.Address.StateProvinceTerritory,
                    Alias = string.IsNullOrEmpty(x.Alias)
                        ? x.FirstName + " " + x.LastName + " " + x.Address.Line1 + " " + x.Address.Line2
                        : x.Alias,
                    Id = x.Id
                };
            return listAddress.ToList();
        }

        private InvoiceAddressModel convertToMemberAddress(DeliveryOption address)
        {
            var listAddress = new InvoiceAddressModel
            {
                Address1 = address.Address.Line1,
                Address2 = address.Address.Line2,
                City = address.Address.City,
                Country = address.Address.Country,
                County = address.Address.CountyDistrict,
                PostalCode = address.Address.PostalCode,
                State = address.Address.StateProvinceTerritory
            };

            return listAddress;
        }


        private static InvoiceModel ClearIds(InvoiceModel invoiceModel)
        {
            invoiceModel.Id = 0;
            invoiceModel.MemberInvoiceNumber = 0;
            invoiceModel.OrderSource = string.Empty;
            invoiceModel.PricingType = string.Empty;
            if (null != invoiceModel.InvoiceLines && invoiceModel.InvoiceLines.Any())
            {
                invoiceModel.InvoiceLines.ForEach(i => i.InvoiceId = 0);
            }
            return invoiceModel;
        }


        private static InvoiceAddressModel ConvertToInvoiceAddress(ServiceProvider.ShippingSvc.Address address, string countryCode)
        {
            return new InvoiceAddressModel
            {
                Address1 = address.Line1,
                Address2 = address.Line2,
                City = address.City,
                Country = string.IsNullOrEmpty(address.Country) ? countryCode : address.Country,
                County = address.CountyDistrict,
                PostalCode = address.PostalCode,
                State = address.StateProvinceTerritory
            };
        }

        private InvoiceModel ConvertShoppingCartToInvoiceModel(MyHLShoppingCart shoppingCart, string locale,
            string countryCode, string memberId)
        {
            if (shoppingCart != null)
            {
            var invoiceModel = new InvoiceModel
            {
                MemberId = shoppingCart.DistributorID,
                InvoiceDate = DateTime.Now,
                ShippingMethod = shoppingCart.FreightCode,
                OrderSource = "GDO",
                        Type = "Customer"
            };
            if (invoiceModel.Type == "Distributor")
            {
                invoiceModel.IsGdoMemberPricing = true;
            }
            PopulateName(invoiceModel, memberId);
            PopulateInvoiceAddress(invoiceModel, shoppingCart.DeliveryInfo);
            PopulateInvoiceLines(invoiceModel, shoppingCart.CartItems, locale, countryCode);
            PopulateInvoicePrice(invoiceModel, 0, countryCode, memberId, locale);
            invoiceModel.MemberAddress = LoadMemberAddress(memberId, locale, countryCode);
            return invoiceModel;
        }
            else
            {
                return new InvoiceModel();
            }
        }

        private void PopulateInvoiceLines(InvoiceModel invoiceModel,
            IEnumerable<ShoppingCartItem_V01> shoppingCartItemList, string locale, string countryCode)
        {
            invoiceModel.InvoiceLines = new List<InvoiceLineModel>();
            foreach (
                var invoiceLine in
                    shoppingCartItemList.Select(
                        shoppingCartItem => _invoiceCatalogProvider.GetInvoiceLineFromSku(shoppingCartItem.SKU,
                            locale, countryCode, shoppingCartItem.Quantity,
                            !string.IsNullOrEmpty(invoiceModel.Type) && invoiceModel.Type.ToUpper() == "CUSTOMER"))
                        .Where(invoiceLine => null != invoiceLine))
            {
                invoiceModel.InvoiceLines.Add(invoiceLine);
            }
        }

        private static void PopulateInvoiceAddress(InvoiceModel invoiceModel, ShippingInfo shippingInfo)
        {
            if (null != shippingInfo && null != shippingInfo.Address && null != shippingInfo.Address.Address)
            {
                invoiceModel.Address = new InvoiceAddressModel
                {
                    Address1 = shippingInfo.Address.Address.Line1,
                    Address2 = shippingInfo.Address.Address.Line2,
                    City = shippingInfo.Address.Address.City,
                    Country = shippingInfo.Address.Address.Country,
                    County = shippingInfo.Address.Address.CountyDistrict,
                    PostalCode = shippingInfo.Address.Address.PostalCode,
                    State = shippingInfo.Address.Address.StateProvinceTerritory
                };

                invoiceModel.FirstName = !string.IsNullOrEmpty(shippingInfo.Address.Recipient)
                    ? GetFirstName(shippingInfo.Address.Recipient)
                    : string.Empty;
                invoiceModel.LastName = !string.IsNullOrEmpty(shippingInfo.Address.Recipient)
                    ? GetLastName(shippingInfo.Address.Recipient)
                    : string.Empty;
                invoiceModel.Phone = shippingInfo.Address.Phone
                    ;
                invoiceModel.InvoiceShipToAddress = true;
            }
        }

        private InvoiceModel ConvertOrderToInvoiceModel(Order_V01 order, string locale, string countryCode,
            string memberId, string source = null)
        {
            var invoiceModel = new InvoiceModel
            {
                OrderSource = string.IsNullOrEmpty(source) ? GetOrderSource(order) : source,
                Type = "Distributor",
                OrderId = order.OrderID,
                InvoiceDate = DateTime.Now
            };
            if (invoiceModel.Type == "Distributor")
            {
                invoiceModel.IsGdoMemberPricing = true;
            }
            invoiceModel.InvoiceShipToAddress = true;
            PopulateInvoiceAddress(invoiceModel, order.Shipment);
            PopulateInvoiceLines(invoiceModel, order, countryCode, locale);
            invoiceModel.MemberAddress = LoadMemberAddress(memberId, locale, countryCode);
            if (null != invoiceModel.InvoiceLines && invoiceModel.InvoiceLines.Any())
            {
                PopulateInvoicePrice(invoiceModel, order.DiscountPercentage, countryCode, memberId, locale);
            }
            return invoiceModel;
        }

        private static string GetOrderSource(Order_V01 order)
        {
            return string.Empty;
        }

        private void PopulateInvoicePrice(InvoiceModel invoiceModel, decimal discount, string countryCode,
            string memberId, string locale)
        {
            invoiceModel.InvoicePrice = new InvoicePriceModel { MemberStaticDiscount = discount };
            var priceFactory = GetInvoicePriceProvider(countryCode);
            priceFactory.CalculateBasicPrice(invoiceModel, memberId, locale, countryCode, true);
            if (invoiceModel.IsGdoMemberPricing)
            {
                invoiceModel.InvoicePrice.DiscountPercentage = 25;
            }
        }

        private void PopulateInvoiceLines(InvoiceModel invoiceModel, Order_V01 order, string countryCode, string locale)
        {
            invoiceModel.InvoiceLines = new List<InvoiceLineModel>();
            foreach (var invoiceLine in (from item in order.OrderItems
                let itemV02 = item as OrderItem_V02
                select
                    _invoiceCatalogProvider.GetInvoiceLineFromSku(item.SKU, locale, countryCode, item.Quantity,
                        !string.IsNullOrEmpty(invoiceModel.Type) && invoiceModel.Type.ToUpper() == "CUSTOMER"))
                .Where(invoiceLine => null != invoiceLine))
            {
                invoiceModel.InvoiceLines.Add(invoiceLine);
            }
        }

        private void PopulateName(InvoiceModel invoiceModel, string memberId)
        {
            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (null != membershipUser && null != membershipUser.Value)
            {
                invoiceModel.FirstName = membershipUser.Value.FirstName;
                invoiceModel.LastName = membershipUser.Value.LastName;
            }
        }

        private static void PopulateInvoiceAddress(InvoiceModel invoiceModel,
            ServiceProvider.OrderSvc.ShippingInfo shipment)
        {
            var shippingInfo = shipment as ShippingInfo_V01;
            if (null == shippingInfo || null == shippingInfo.Address) return;
            invoiceModel.Address = new InvoiceAddressModel
            {
                Address1 = shippingInfo.Address.Line1,
                Address2 = shippingInfo.Address.Line2,
                City = shippingInfo.Address.City,
                State = shippingInfo.Address.StateProvinceTerritory,
                County = shippingInfo.Address.CountyDistrict,
                PostalCode = shippingInfo.Address.PostalCode,
                Country = shippingInfo.Address.Country
            };

            invoiceModel.FirstName = !string.IsNullOrEmpty(shippingInfo.Recipient)
                ? GetFirstName(shippingInfo.Recipient)
                : string.Empty;
            invoiceModel.LastName = !string.IsNullOrEmpty(shippingInfo.Recipient)
                ? GetLastName(shippingInfo.Recipient)
                : string.Empty;
            invoiceModel.Phone = shippingInfo.Phone;
            invoiceModel.ShippingMethod = shippingInfo.ShippingMethodID;
            invoiceModel.InvoiceShipToAddress = true;
        }

        private static string GetFirstName(string recipient)
        {
            try
            {
                var spliByPeriod = recipient.Split(' ');
                if (null != spliByPeriod && spliByPeriod.Length > 1)
                {
                    return spliByPeriod[0];
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetFirstName: Error: {0}{1}\n", ex.Message, recipient));

            }
            return string.Empty;

        }

        private static string GetLastName(string recipient)
        {
            try
            {
                var spliByPeriod = recipient.Split(' ');
                var lastName = string.Empty;
                if (null != spliByPeriod && spliByPeriod.Length > 2)
                {
                    for (var i = 1; i < spliByPeriod.Length; i++)
                    {
                        lastName += spliByPeriod[i] + " ";
                    }
                }
                return !string.IsNullOrEmpty(lastName) ? lastName.Trim() : string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetLastName: Error: {0}{1}\n", ex.Message, recipient));
            }
            return string.Empty;
        }


        private InvoicePriceProvider GetInvoicePriceProvider(string countryCode)
        {
            InvoicePriceProvider invoicePriceProvider = null;
            switch (countryCode)
            {
                case "US":
                {
                    var invoicePriceFactory = new USInvoicePriceFactory();
                    invoicePriceProvider =
                        invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                }
                    break;
                case "GB":
                {
                    var invoicePriceFactory = new UKInvoicePriceFactory();
                    invoicePriceProvider =
                        invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                }
                    break;
                case "KR":
                {
                    var invoicePriceFactory = new KRInvoicePriceFactory();
                    invoicePriceProvider =
                        invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                }
                    break;
                case "JM":
                    {
                        var invoicePriceFactory = new JMInvoicePriceFactory();
                        invoicePriceProvider =
                            invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                    }
                    break;                    
                case "TT":
                    {
                        var invoicePriceFactory = new TTInvoicePriceFactory();
                        invoicePriceProvider =
                            invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                    }
                    break;
                case "CA":
                    {
                        var invoicePriceFactory = new CAInvoicePriceFactory();
                        invoicePriceProvider =
                            invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                    }
                    break;
                default:
                {
                    var invoicePriceFactory = new USInvoicePriceFactory();
                    invoicePriceProvider =
                        invoicePriceFactory.GetInvoicePriceProvider(GetInvoiceShippingProvider(countryCode));
                }
                    break;
            }
            return invoicePriceProvider;
        }

        private IInvoiceShippingDetails GetInvoiceShippingProvider(string countryCode)
        {
            IInvoiceShippingDetails invoiceShipping = null;
            switch (countryCode)
            {
                case "US":
                {
                    var invoiceShippingFactory = new USInvoiceShippingDetailsFactory();
                    var shippingProvider = new ShippingProvider_US();
                    invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                }
                    break;
                case "GB":
                {
                    var invoiceShippingFactory = new UKInvoiceShippingDetailsFactory();
                    var shippingProvider = new ShippingProvider_UK();
                    invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                }
                    break;

                case "KR":
                {
                    var invoiceShippingFactory = new KRInvoiceShippingDetailsFactory();
                    var shippingProvider = new ShippingProvider_KR();
                    invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                }
                    break;
                case "CA":
                    {
                        var invoiceShippingFactory = new CAInvoiceShippingDetailsFactory();
                        var shippingProvider = new ShippingProvider_CA();
                        invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                    }
                    break;
                case "JM":
                    {
                        var invoiceShippingFactory = new JMInvoiceShippingDetailsFactory();
                        var shippingProvider = new ShippingProvider_JM();
                        invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                    }
                    break;
                case "TT":
                    {
                        var invoiceShippingFactory = new TTInvoiceShippingDetailsFactory();
                        var shippingProvider = new ShippingProviderBase();
                        invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                    }
                    break;
                default:
                {
                    var invoiceShippingFactory = new USInvoiceShippingDetailsFactory();
                    var shippingProvider = new ShippingProvider_US();
                    invoiceShipping = invoiceShippingFactory.GetInvoiceShippingDetails(shippingProvider);
                }
                    break;
            }
            return invoiceShipping;
        }

        private static IEnumerable<SavedCartViewModel> LoadSavedCarts(string memberId, string locale, string filter)
        {
            var carts = ShoppingCartProvider.GetCarts(memberId, locale);
            if (null == carts)
            {
                return null;
            }

            if (!carts.Any())
            {
                return null;
            }

            var savedCarts = ConvertToSavedCartModel(carts);
            savedCarts = string.IsNullOrEmpty(filter)
                ? savedCarts
                : savedCarts.Where(s => !string.IsNullOrEmpty(s.Name) && s.Name.ToUpper().Contains(filter.ToUpper()));
            return savedCarts;
        }

        private static IEnumerable<SavedCartViewModel> ConvertToSavedCartModel(IEnumerable<MyHLShoppingCart> carts)
        {
            var savedCarts = carts.Select(cart => new SavedCartViewModel
            {
                Id = cart.ShoppingCartID,
                Name = cart.CartName,
                ShipToName = cart.DeliveryInfo != null ? cart.DeliveryInfo.Name : string.Empty,
                SavedDate = cart.LastUpdated,
                VolumePoints = cart.VolumeInCart,
                OrderMonth = cart.OrderDate.Year.ToString()
            });
            return savedCarts;
        }

        private List<InvoiceFilterDataModel> GetStates(string locale)
        {
            var invoiceShipping = GetInvoiceShippingProvider(locale);

            var stateList = (from state in invoiceShipping.GetStates(locale)
                select new InvoiceFilterDataModel
                {
                    Value = state.Key,
                    DisplayName = string.Format("{0}-{1}", state.Key, state.Value)
                }).OrderBy(y => y.DisplayName).ToList();
            return stateList;
        }

        private List<InvoiceFilterDataModel> GetInvoiceStatus(string locale)
        {
            var fileName = locale == "en-US" ? "InvoiceStatusTypes" : "InvoiceStatusTypes." + locale;
            var entries =
                GlobalResourceHelper.GetGlobalEnumeratorElements(fileName, new CultureInfo(locale));
            var stateList = (from entry in entries
                select new InvoiceFilterDataModel
                {
                    Value = entry.Key,
                    DisplayName = entry.Value
                }).ToList();
            return stateList;
        }
        private List<InvoiceFilterDataModel> GetPaymentType(string locale)
        {
            var fileName = "PaymentTypes." + locale;
           
            var entries =
                GlobalResourceHelper.GetGlobalEnumeratorElements(fileName, new CultureInfo(locale));

            var stateList = (from entry in entries
                             select new InvoiceFilterDataModel
                             {
                                 Value = entry.Key,
                                 DisplayName = entry.Value
                             }).OrderBy(s => s.Value).ToList();
           
            return stateList;
        }

        private List<InvoiceFilterDataModel> GetInvoiceTypes(string locale)
        {
            var fileName = locale == "en-US" ? "InvoiceTypes" : "InvoiceTypes." + locale;
            var entries =
                GlobalResourceHelper.GetGlobalEnumeratorElements(fileName, new CultureInfo(locale));
            var typeList = (from entry in entries
                select new InvoiceFilterDataModel
                {
                    Value = entry.Key,
                    DisplayName = entry.Value
                }).ToList();
            return typeList;
        }

        private static List<decimal> GetMemberDiscounts(string locale)
        {
            var discounts = new List<decimal> { 25, 35, 42, 50 };
            return discounts;
        }

        public string GetDisplayInvoiceStatus(string locale, string status)
        {
            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(status))
            {
                return string.Empty;
            }

            try
            {
                var invoiceStatus = Load(new GetInvoiceStatusByLocale { Locale = locale });
                if (null != invoiceStatus && null != invoiceStatus.Result)
                {
                    var result = invoiceStatus.Result.Where(s => s.Value == status);
                    if (null != result && result.Any())
                    {
                        return result.FirstOrDefault().DisplayName;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetDisplayInvoiceStatus: Error: {0}{1}\n", ex.Message, locale));
                return string.Empty;
            }
            return string.Empty;
        }
        public string GetDisplayPaymentType(string locale, string status)
        {
            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(status))
            {
                return string.Empty;
            }

            try
            {
                var paymentType = Load(new GetInvoicePaymentTypeByLocale { Locale = locale });
                if (null != paymentType && null != paymentType.Result)
                {
                    var result = paymentType.Result.Where(s => s.Value == status);
                    if (null != result && result.Any())
                    {
                        return result.FirstOrDefault().DisplayName;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetDisplayPaymentType: Error: {0}{1}\n", ex.Message, locale));
                return string.Empty;
            }
            return string.Empty;
        }
        public string ConvertReceiptChannel(string locale, string ReceiptChannel)
        {
            try
            {
                if (ReceiptChannel == "ClubSaleReceipt")
                {
                    return "Club Visit/Sale";
                }
                else if (ReceiptChannel == "ProductSaleReceipt")
                {
                    return "Retail Sale";
                }
                return ReceiptChannel;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("ConvertReceiptChannel: Error: {0}{1}\n", ex.Message, locale));
                return string.Empty;
            }
        }
        public string GetDisplayReceiptChannel(string locale, string ReceiptChannel)
        {
            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(ReceiptChannel))
            {
                return string.Empty;
            }

            try
            {
                if (ReceiptChannel == "ClubSaleReceipt")
                {
                    if (locale.ToUpper() == "FR-CA")
                    {
                        return "Visite/vente de club";
                    }
                    return "Club Visit/Sale";
                }
                else if (ReceiptChannel == "ProductSaleReceipt")
                {
                    if (locale.ToUpper() == "FR-CA")
                    {
                        return "Vente au détail";
                    }
                    return "Retail Sale";
                }
                else if (locale.ToUpper() == "FR-CA")
                {
                    if (ReceiptChannel == "Club Visit/Sale")
                    {
                        return "Visite/vente de club";
                    }
                    else if (ReceiptChannel == "Retail Sale")
                    {
                        return "Vente au détail";
                    }
                }
                return ReceiptChannel;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetDisplayReceiptChannel: Error: {0}{1}\n", ex.Message, locale));
                return string.Empty;
            }
        }

        public string GetDisplayInvoiceType(string locale, string type)
        {
            if (string.IsNullOrEmpty(locale) || string.IsNullOrEmpty(type))
            {
                return string.Empty;
            }

            try
            {
                var invoiceTypes = Load(new GetInvoiceTypesByLocale { Locale = locale });
                if (null != invoiceTypes && null != invoiceTypes.Result)
                {
                    var result =
                        invoiceTypes.Result.Where(
                            s => String.Equals(s.Value, type, StringComparison.CurrentCultureIgnoreCase));
                    if (null != result && result.Any())
                    {
                        return result.FirstOrDefault().DisplayName;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetDisplayInvoiceType: Error: {0}{1}\n", ex.Message, locale));
                return string.Empty;
            }
            return string.Empty;
        }

        #region internal/private methods

        public bool ValidateAddress(InvoiceAddressModel invoiceAddress, string countryCode,
            out ServiceProvider.ShippingSvc.Address_V01 avsOutAddress)
        {
            try
            {
                var address = _invoiceConverter.ConvertToAddress(invoiceAddress, countryCode);
                var shippingProvider = GetInvoiceShippingProvider(countryCode);
                return shippingProvider.ValidateAddress(ObjectMappingHelper.Instance.GetToShipping(address), out avsOutAddress);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("InvoiceProvider: ValidateAddress Error: {0}\n", ex.Message));
                avsOutAddress = null;
                return false;
            }
        }

        private static bool IsAddressValid(InvoiceAddressModel address)
        {
            return
                !(address == null || string.IsNullOrEmpty(address.Address1) || string.IsNullOrEmpty(address.City) ||
                  string.IsNullOrEmpty(address.PostalCode));
        }
        private UpdateInvoiceModelResult SaveInvoice(InvoiceModel invoiceModel, string locale, string countryCode)
        {
            var saveInvoiceModelResult = new UpdateInvoiceModelResult();
            ServiceProvider.ShippingSvc.Address_V01 outAddress = null;


            if (IsAddressValid(invoiceModel.Address))
            {
                if (!ValidateAddress(invoiceModel.Address, countryCode, out outAddress))
                {
                    saveInvoiceModelResult.InvoiceModel = invoiceModel;
                    saveInvoiceModelResult.IsSuccess = false;
                    saveInvoiceModelResult.ErrorCodeKey = "invalidAddressSave";
                   // return saveInvoiceModelResult;
                }
            }
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                invoiceModel.Source = "MyHl";
            }
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                invoiceModel.Address = outAddress != null
                    ? ConvertToInvoiceAddress(outAddress, countryCode)
                    : invoiceModel.Address;

                invoiceModel.Phone = !string.IsNullOrEmpty(invoiceModel.Phone)
                    ? invoiceModel.Phone.Replace("(", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty)
                    : invoiceModel.Phone;

                var request = new SaveMemberInvoiceRequest_V01
                {
                    MemberInvoice = _invoiceConverter.ConvertToMemberInvoice(invoiceModel)
                };

                var response = proxy.SaveMemberInvoice(new SaveMemberInvoiceRequest1(request)).SaveMemberInvoiceResult as SaveMemberInvoiceResponse_V01;
                if (response == null || response.Status != ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                {
                    LoggerHelper.Error("OrderService: SaveInvoice Error: ");
                }
                ExpireCache(invoiceModel.MemberId, locale);
                invoiceModel.MemberInvoiceNumber = response.MemberInvoiceNumber;

                invoiceModel.Id = response.MemberInvoiceId;
                if (null != invoiceModel.InvoiceLines && invoiceModel.InvoiceLines.Any())
                {
                    invoiceModel.InvoiceLines.ForEach(i => i.InvoiceId = response.MemberInvoiceId);
                }
                ExpireInvoiceModelCache(invoiceModel.Id, invoiceModel.MemberId, locale);
                LoadMemberInfo(invoiceModel);
                invoiceModel.DisplayInvoiceStatus = GetDisplayInvoiceStatus(locale, invoiceModel.Status);
                invoiceModel.DisplayInvoiceType = GetDisplayInvoiceType(locale, invoiceModel.Type);
                if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
                {
                    invoiceModel.DisplayReceiptChannel = GetDisplayReceiptChannel(locale, invoiceModel.ReceiptChannel.ToString());
                    invoiceModel.DisplayPaymentType = GetDisplayPaymentType(locale, invoiceModel.PaymentType.ToString());
                }
                _cache.Add(invoiceModel,
                    string.Format(InvoiceModelCachePrefix, invoiceModel.Id, invoiceModel.MemberId, locale),
                    TimeSpan.FromMinutes(20));
                saveInvoiceModelResult.IsSuccess = true;
            }
            catch (Exception ex)
            {
                saveInvoiceModelResult.IsSuccess = false;
                LoggerHelper.Error(string.Format("OrderService: SaveInvoice Error: {0}\n", ex.Message));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            saveInvoiceModelResult.InvoiceModel = invoiceModel;
            return saveInvoiceModelResult;
        }

        private bool IsVelidDate(InvoiceModel invoiceModel)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice)
            {
                var date = DateTime.Now.Date.ToString("MM/dd/yyyy");
                if (invoiceModel.InvoiceDate.Date.ToString("MM/dd/yyyy") == date)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool DeleteInvoice(int invoiceId, string memberId, string locale)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new DeleteMemberInvoiceRequest_V01
                {
                    MemberInvoiceId = invoiceId
                };

                //First get the invoice & check whether it belongs to this member
                var invoice = GetInvoiceModelFromService(invoiceId, memberId, locale);
                if (null != invoice && invoice.MemberId.ToUpper() != memberId.ToUpper())
                {
                    return false;
                }

                var response = proxy.DeleteMemberInvoice(new DeleteMemberInvoiceRequest1(request)).DeleteMemberInvoiceResult as DeleteMemberInvoiceResponse_V01;
                if (response != null && response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success && response.IsDeleted)
                {
                    ExpireCache(memberId, locale);
                    return response.IsDeleted;
                }
                return false;
            }
            catch
            {
                LoggerHelper.Error(
                    string.Format(
                        "Unable to Delete Invoice - Delete method InvoiceProvider memberID: {0}, InvoiceID {1}: ",
                        memberId, invoiceId));
            }
            return false;
        }

        private List<InvoiceCRMConatactModel> GetCRMAddress(string memberId,string CountryCode)
        {
            var CRMContacts = new List<InvoiceCRMConatactModel>();
            List < string > memberIds= new List<string>()
            {
                memberId
            };
            CRMContacts = DistributorContactProvider.GetCustomersFromService(memberIds, CountryCode);
            
            return CRMContacts;
        }
       
        private List<InvoiceModel> FilterInvoices(List<InvoiceModel> list, InvoiceFilterModel filter, string memberId,
            string locale)
        {
            if (null == filter)
            {
                return list;
            }

            list = FilterByDate(memberId, locale, filter.From, filter.To, list);

            if (null == list) return new List<InvoiceModel>();

            if (filter.SelectedFilterId <= 0 || string.IsNullOrEmpty(filter.SelectedFilterValue)) return list;

            switch (filter.SelectedFilterId)
            {
                case 1:
                {
                    list =
                        list.Where(
                            x =>
                                !(string.IsNullOrEmpty(x.FirstName)) &&
                                (x.FirstName.ToUpper().Contains(filter.SelectedFilterValue.ToUpper()))).ToList();
                }
                    break;
                case 2:
                {
                    list =
                        list.Where(
                            x =>
                                !(string.IsNullOrEmpty(x.LastName)) &&
                                (x.LastName.ToUpper().Contains(filter.SelectedFilterValue.ToUpper()))).ToList();
                }
                    break;
                case 3:
                {
                    var filteredIntValue = 0;
                    var enteredValue = filter.SelectedFilterValue;
                    if (enteredValue.Length > 6)
                    {
                        enteredValue = enteredValue.Substring(enteredValue.Length - 6);
                    }
                    Int32.TryParse(enteredValue, out filteredIntValue);
                    list =
                        list.Where(
                            x =>
                                x.MemberInvoiceNumber != null &&
                                x.MemberInvoiceNumber == filteredIntValue).ToList();
                }
                    break;
                case 4:
                {
                    decimal vp = 0;
                    decimal.TryParse(filter.SelectedFilterValue, out vp);
                    list =
                        list.Where(x => x.TotalVolumePoints != null &&
                                        x.TotalVolumePoints == vp).ToList();
                }
                    break;
                case 5:
                {
                    decimal total = 0;
                    decimal.TryParse(filter.SelectedFilterValue, out total);
                    list =
                        list.Where(x => x.Total != null &&
                                        x.Total == total).ToList();
                }
                    break;
            }
            return list;
        }

        public List<InvoiceModel> FilterByDate(string memberId, string locale, DateTime? from, DateTime? to,
            List<InvoiceModel> list)
        {
            if (null == from || null == to)
            {
                return list;
            }

            var invoices =
                _invoicesLoader.Load(new GetInvoicesByFilter
                {
                    MemberId = memberId,
                    Locale = locale,
                    InvoiceFilterModel = new InvoiceFilterModel { From = from, To = to }
                });
            return invoices.Result;
        }

        private InvoiceModel FilterInvoicesById(int id, string memberId, string locale)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
            var cacheKey = string.Format(InvoiceModelCachePrefix, id, memberId, locale);
            return _cache.Retrieve(_ => GetInvoiceModelFromService(id, memberId, locale), cacheKey,
                TimeSpan.FromMinutes(20));
        }

        private void ExpireInvoiceModelCache(int id, string memberId, string locale)
        {
            var cacheKey = string.Format(InvoiceModelCachePrefix, id, memberId, locale);
            _cache.Expire(typeof(InvoiceModel), cacheKey);
        }
        private void ExpireInvoiceCRMConatactModelCache( string memberId)
        {
            var cacheKey = string.Format(CRMContactsCachePrefix, memberId);
            _cache.Expire(typeof(InvoiceCRMConatactModel), cacheKey);
        }


        private InvoiceModel GetInvoiceModelFromService(int id, string memberId, string locale)
        {
            if (id > 0)
            {
                var invoiceModel =
                    _invoiceLoader.Load(new GetInvoiceById { Id = id, Locale = locale, MemberId = memberId }).Result ??
                    new InvoiceModel
                    {
                        Address = new InvoiceAddressModel(),
                        InvoicePrice = new InvoicePriceModel()
                    };

                invoiceModel.DisplayInvoiceStatus = GetDisplayInvoiceStatus(locale, invoiceModel.Status);
                invoiceModel.ReceiptChannel = ConvertReceiptChannel(locale, invoiceModel.ReceiptChannel);
                invoiceModel.DisplayInvoiceType = GetDisplayInvoiceType(locale, invoiceModel.Type);
                invoiceModel.MemberAddress = LoadMemberAddress(memberId, locale, locale.Substring(3));
                LoadMemberInfo(invoiceModel);
                return invoiceModel;
            }
            return null;
        }

        public static void LoadMemberInfo(InvoiceModel invoiceModel)
        {
            var membershipUser = (Membership.GetUser(invoiceModel.MemberId)) as MembershipUser<DistributorProfileModel>;
            if (membershipUser != null && membershipUser.Value != null)
            {
                var member = membershipUser.Value;
                invoiceModel.MemberEmailAddress = member.PrimaryEmail;
                invoiceModel.MemberFirstName = member.FirstName;
                invoiceModel.MemberLastName = member.LastName;
                if(HLConfigManager.Configurations.DOConfiguration.MemberHasPhoneNumber)
                {
                    LoadMemeberPhoneNumber(invoiceModel);
                }

            }
        }
        public static void LoadMemeberPhoneNumber(InvoiceModel invoiceModel)
        {
            var dsProfile = DistributorOrderingProfileProvider.GetProfile(invoiceModel.MemberId,
                                                                             invoiceModel.ApplicationCountryCode);
            if(dsProfile!=null && dsProfile.PhoneNumbers !=null)
            {
                foreach(var phone in dsProfile.PhoneNumbers)
                {
                    if(phone.IsPrimary==true)
                    {
                        invoiceModel.MemberPhoneNumber = phone.Number.ToString();
                        break;
                    }

                }
            }
        }

        private InvoiceAddressModel LoadMemberAddress(string memberId, string locale, string country)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);

            var address = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, memberId,
                country);
            if (address.Country != country)
            {
                var shippingAddresses = _iShippingProvider.GetShippingAddresses(memberId, locale);
                if (shippingAddresses != null && shippingAddresses.Count > 0)
                {
                    var primaryAddress = shippingAddresses.Find(a => a.IsPrimary);
                    if (null != primaryAddress && null != primaryAddress.Address)
                    {
                        return ConvertToInvoiceAddress(primaryAddress.Address, country);
                    }
                    return ConvertToInvoiceAddress(shippingAddresses.FirstOrDefault().Address, country);
                }
                return null;
            }
            return null == address ? null : ConvertToInvoiceAddress(ObjectMappingHelper.Instance.GetToShipping(address), country);
        }

        private Task<List<InvoiceModel>> GetInvoicesFromCache(string memberId, string locale, DateTime? from,
            DateTime? to)
        {
            if (string.IsNullOrWhiteSpace(memberId))
            {
                return null;
            }
            var distributorCacheName = string.Format(AllInvoiceCachePrefix, memberId, locale);

            var result = (null != from && null != to)
                ? _invoicesLoader.Load(new GetInvoicesByFilter
                {
                    MemberId = memberId,
                    Locale = locale,
                    InvoiceFilterModel = new InvoiceFilterModel { From = from, To = to }
                })
                : _cache.Retrieve(
                    _ =>
                        _invoicesLoader.Load(new GetInvoicesByFilter
                        {
                            MemberId = memberId,
                            Locale = locale,
                            InvoiceFilterModel = new InvoiceFilterModel { From = from, To = to }
                        }),
                    distributorCacheName,
                    TimeSpan.FromMinutes(InvoiceCacheMinutes));

            return result;
        }

        #endregion
    }
}