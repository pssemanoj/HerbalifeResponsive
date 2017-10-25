#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

#endregion

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    public class MockInvoiceProvider : IInvoiceProvider, IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter>,
        IAsyncLoader<InvoiceModel, GetInvoiceById>
    {
        public Task<InvoiceModel> Load(GetInvoiceById query)
        {
            return Task.FromResult(new InvoiceModel());
        }

        public Task<List<InvoiceModel>> Load(GetInvoicesByFilter query)
        {
            return Task.FromResult(new List<InvoiceModel>());
        }

        public UpdateInvoiceModelResult Create(InvoiceModel invoiceModel)
        {
            return new UpdateInvoiceModelResult();
        }

        public UpdateInvoiceModelResult Edit(InvoiceModel invoiceModel, string locale, string countryCode)
        {
            return new UpdateInvoiceModelResult();
        }

        public bool Delete(int id, string memberId, string locale)
        {
            return true;
        }

        public bool UpdateStatus(int id, string status, string memberId, string locale)
        {
            throw new NotImplementedException();
        }

        public InvoiceModel CalculateCustomerPrice(InvoiceModel invoiceModel, string locale, string countryCode, string memberId)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel CalculateDistributorPrice(InvoiceModel invoiceModel, string locale, string countryCode, string memberId)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel CalculateBasicPrice(InvoiceModel invoiceModel, string locale, string countryCode, string memberId, bool isReset)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel CalculateTotalDue(InvoiceModel invoice, string memberId, string locale, string countryCode, bool isReset)
        {
            throw new NotImplementedException();
        }

        public InvoiceModel CalculateMemberTotal(InvoiceModel invoiceModel, string locale, string countryCode, string memberId)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel CalculateDistributorPriceOnCustomerSection(InvoiceModel invoice, string memberId, string locale,
            string countryCode)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel CalculateModifiedPriceOnCustomerSection(InvoiceModel invoice, string memberId, string locale,
            string countryCode)
        {
            throw new System.NotImplementedException();
        }

        public UpdateInvoiceModelResult Price(InvoiceModel invoice, string memberId, string locale, string countryCode)
        {
            throw new System.NotImplementedException();
        }


        public bool ExpireCache(string memberId, string locale)
        {
            return true;
        }

        public InvoiceModel GetInvoiceFromOrderId(string orderId, string locale, string countryCode, string memberId, string source)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel GetInvoiceFromCartId(int cartId, string memberId, string locale, string countryCode)
        {
            throw new System.NotImplementedException();
        }

        public byte[] GeneratePdf(string htmlContent)
        {
            throw new System.NotImplementedException();
        }

        public Task<InvoiceModel> Copy(GetInvoiceById request)
        {
            throw new System.NotImplementedException();
        }

        public InvoiceModel GetInvoiceFromOrderId(string orderId)
        {
            throw new System.NotImplementedException();
        }
        public bool SendInvoiceEmail(InvoiceModel invoice)
        {
            return true;
        }
         public bool SendInvoiceSMS(InvoiceModel invoice)
        {
            return true;
        }
        public bool ValidateAddress(InvoiceAddressModel address, string countryCode, out Address_V01 avsOutAddress)
        {
            throw new System.NotImplementedException();
        }

        public bool ValidateAddress(Address_V01 address, string countryCode, out Address_V01 avsOutAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<DisplayInvoiceAddressModel>> GetShippingAddresses(string memberId, string locale)
        {
            throw new System.NotImplementedException();
        }

        public Task<InvoiceAddressModel> FilterGetShippingAddresses(string memberId, string locale, int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<InvoiceModel>> Filter(GetInvoicesByFilter query)
        {
            throw new NotImplementedException();
        }

        public bool CheckEmailAndPhone(InvoiceModel invoiceModel, string countryCode)
        {
            throw new NotImplementedException();
        }

        public Task<List<InvoiceCRMConatactModel>> GetCRMAddress(GetCRMAddress query)
        {
            throw new NotImplementedException();
        }
        public Task<SaveUpdateResponseModel> SaveUpdate(InvoiceCRMConatactModel contact, string memberId)
        {
            throw new NotImplementedException();
        }
    }
}