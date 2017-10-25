#region

using System.Collections.Generic;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IInvoiceProvider
    {
       
        UpdateInvoiceModelResult Edit(InvoiceModel invoiceModel, string locale, string countryCode);
        bool Delete(int id, string memberId, string locale);
        bool UpdateStatus(int id, string status, string memberId, string locale);


        InvoiceModel CalculateCustomerPrice(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId);

        InvoiceModel CalculateDistributorPrice(InvoiceModel invoiceModel, string locale, string countryCode,
            string memberId);

        InvoiceModel CalculateBasicPrice(InvoiceModel invoiceModel, string locale, string countryCode, string memberId, bool isReset);

        InvoiceModel CalculateTotalDue(InvoiceModel invoiceModel, string locale, string countryCode, string memberId, bool isReset);
        bool CheckEmailAndPhone(InvoiceModel invoiceModel, string countryCode);

        InvoiceModel CalculateMemberTotal(InvoiceModel invoiceModel, string locale, string countryCode, string memberId);

        InvoiceModel CalculateDistributorPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode);

        InvoiceModel CalculateModifiedPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode);

        UpdateInvoiceModelResult Price(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode);
            
        bool ExpireCache(string memberId, string locale);
        InvoiceModel GetInvoiceFromOrderId(string orderId, string locale, string countryCode, string memberId, string source =null);
        InvoiceModel GetInvoiceFromCartId(int cartId, string memberId, string locale, string countryCode);
        byte[] GeneratePdf(string htmlContent);
        Task<InvoiceModel> Copy(GetInvoiceById request);
        bool SendInvoiceEmail(InvoiceModel invoice);
        bool ValidateAddress(InvoiceAddressModel address, string countryCode, out Address_V01 avsOutAddress);
        Task<List<DisplayInvoiceAddressModel>> GetShippingAddresses(string memberId, string locale);
        Task<InvoiceAddressModel> FilterGetShippingAddresses(string memberId, string locale, int id);

        Task<List<InvoiceModel>> Filter(GetInvoicesByFilter query);
        Task<List<InvoiceCRMConatactModel>> GetCRMAddress(GetCRMAddress query);
        bool SendInvoiceSMS(InvoiceModel invoice);

        Task<SaveUpdateResponseModel> SaveUpdate(InvoiceCRMConatactModel contact, string memberId);


    }
}