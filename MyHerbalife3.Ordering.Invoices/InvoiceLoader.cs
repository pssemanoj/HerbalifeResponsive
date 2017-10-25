#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class InvoiceLoader : IAsyncLoader<List<InvoiceModel>, GetInvoicesByFilter>,
        IAsyncLoader<InvoiceModel, GetInvoiceById>
    {
        private readonly InvoiceConverter _invoiceConverter;

        public InvoiceLoader(InvoiceConverter invoiceConverter)
        {
            _invoiceConverter = invoiceConverter;
        }

        public Task<InvoiceModel> Load(GetInvoiceById query)
        {
            var result =
                Task<InvoiceModel>.Factory.StartNew(
                    () => GetInvoiceModel(query.Id, query.Locale, query.MemberId));
            return result;
        }

        public Task<List<InvoiceModel>> Load(GetInvoicesByFilter query)
        {
            if (null == query)
            {
                return null;
            }
            var invoices = LoadFromService(query.MemberId, query.Locale,
                query.InvoiceFilterModel != null ? query.InvoiceFilterModel.From : null,
                query.InvoiceFilterModel != null ? query.InvoiceFilterModel.To : null);
            return invoices;
        }


        private async Task<List<InvoiceModel>> LoadFromService(string memberId, string locale, DateTime? from,
            DateTime? to)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new GetMemberInvoicesRequest_V01
                {
                    MemberId = memberId,
                    ApplicationCountryCode = locale.Substring(3),
                    From = from,
                    To = to
                };
                var response = await proxy.GetMemberInvoicesAsync(new GetMemberInvoicesRequest1(request));

                if (response != null)
                {
                    var responseV01 = response.GetMemberInvoicesResult as GetMemberInvoicesResponse_V01;
                    var list = _invoiceConverter.ConvertToInvoiceModels(responseV01.MemberInvoices, locale);
                    return list.OrderBy(x => x.MemberInvoiceNumber).Reverse().ToList();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("OrderService: getMemberInvoicesResponse loadFromService error: {0}",
                    ex.Message));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
            }
            return null;
        }


        private InvoiceModel GetInvoiceModel(int invoiceId, string locale, string memberId)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            var response =
                proxy.GetMemberInvoice(new GetMemberInvoiceRequest1(new GetMemberInvoiceRequest_V01 {MemberInvoiceId = invoiceId})).GetMemberInvoiceResult;
            if (null == response || response.Status != ServiceResponseStatusType.Success) return null;
            var responseV01 = response as GetMemberInvoiceResponse_V01;
            if (null != responseV01 && null != responseV01.MemberInvoice)
            {
                var invoiceModel = _invoiceConverter.ConvertToInvoiceModel(responseV01.MemberInvoice, locale);
                if (invoiceModel.MemberId.ToUpper() != memberId.ToUpper())
                {
                    LoggerHelper.Error(string.Format("InvoiceLoader: GetInvoiceModel error memberId mismatch: {0},{1}", memberId, invoiceModel.MemberId));
                    return null;
                }
                if(invoiceModel.ReceiptChannel== "ClubSaleReceipt" || invoiceModel.ReceiptChannel == "Club Visit/Sale")
                {
                    
                    invoiceModel.ClubInvoice = _invoiceConverter.ConvertToClubInvoiceLines(responseV01.MemberInvoice.Items,invoiceModel);
                    invoiceModel.InvoiceLines = new List<InvoiceLineModel>();
                    invoiceModel.InvoicePrice = new InvoicePriceModel();
                    invoiceModel.InvoicePrice.DisplayCalculatedTax = "0.00";
                    invoiceModel.InvoicePrice.DisplayDiscountedAmount = "0.00";
                    invoiceModel.InvoicePrice.DisplayMemberFreight = "0.00";
                    invoiceModel.InvoicePrice.DisplayMemberTax = "0.00";
                    invoiceModel.InvoicePrice.DisplayMemberTotal = (Convert.ToDecimal(invoiceModel.ClubInvoice.ClubRecieptDisplayTotalDue).FormatPrice()).ToString();
                    invoiceModel.InvoicePrice.DisplayShipping = "0.00";
                    invoiceModel.InvoicePrice.DisplaySubtotal = (Convert.ToDecimal(invoiceModel.ClubInvoice.ClubRecieptDisplayTotalDue).FormatPrice()).ToString();
                    invoiceModel.InvoicePrice.DisplayTax = "0.00";
                    invoiceModel.InvoicePrice.DisplayTotalDue = (Convert.ToDecimal(invoiceModel.ClubInvoice.ClubRecieptDisplayTotalDue).FormatPrice()).ToString();
                    invoiceModel.InvoicePrice.DisplayTotalYourPrice = (Convert.ToDecimal(invoiceModel.ClubInvoice.ClubRecieptDisplayTotalDue).FormatPrice()).ToString();
                }
               else if (null != invoiceModel && null != responseV01.MemberInvoice.Items &&
                    responseV01.MemberInvoice.Items.Any())
                {
                    invoiceModel.InvoiceLines = _invoiceConverter.ConvertToInvoiceLines(
                        responseV01.MemberInvoice.Items, invoiceId);
                }
                return invoiceModel;
            }
            return null;
        }
    }
}