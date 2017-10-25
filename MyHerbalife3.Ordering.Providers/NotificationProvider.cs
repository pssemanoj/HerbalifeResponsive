using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.NotificationSVC;
using MyHerbalife3.Ordering.ViewModel.Model;
using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.NotificationAlertSVC;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers
{
    public class NotificationProvider
    {
        public static bool SendInvoiceSMS(InvoiceModel invoiceModel, string locale)
        {
           LoggerHelper.Info($"NotificationSvcProvider:SendReceiptSms. customerId={invoiceModel.CustomerId}, receiptId={invoiceModel.DisplayMemberInvoiceNumber}, distributorId={invoiceModel.MemberId}, customerPhone: {invoiceModel.SMSNumber}.");
            try
            {
                var result = new NotifyCustomerReceiptResponse_V01();
                var proxy = ServiceClientProvider.GetNotificationServiceProxy();
                var request = new NotifyOrderingReceiptRequest_V01
                {
                    CustomerPhone = "1"+ invoiceModel.SMSNumber,
                    CustomerFirstName = invoiceModel.FirstName,
                    CustomerLastName = invoiceModel.LastName,
                    MemberType = "DS",
                    CustomerId = string.IsNullOrEmpty(invoiceModel.CustomerId) ? 0: Convert.ToInt32(invoiceModel.CustomerId),
                    ReceiptId = Convert.ToString(invoiceModel.MemberInvoiceNumber),
                    LocaleCode = locale,
                    DistributorId = invoiceModel.MemberId,
                    CustomerEmail=invoiceModel.Email,
                   
                };
                var response = proxy.NotifyOrdering(new NotifyOrderingRequest(request)) as NotifyOrderingResponse;
                if (response.NotifyOrderingResult.Status == ServiceProvider.NotificationSVC.ServiceResponseStatusType.Success)
                {
                    return true;
                }
                else
                {
                  
                    LoggerHelper.Error($"NotificationService Service : NotifyOrdering Error: {result.Message}, DistributorID:{invoiceModel.MemberId}");
                    return false;
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(string.Format("NotificationService  Service : NotifyOrdering Error: {0}\n", ex.Message));
            }
            return false;
        }
        public static NotifyLegacyCustomerReceiptResponse_V01 InsertCustomerReceipt(InvoiceModel invoiceModel,string locale)
        {
            

            var result = new NotifyLegacyCustomerReceiptResponse_V01();
            try
            {
                
                var proxy = ServiceClientProvider.GetNotificationServiceProxy();
                var request = new NotifyLegacyCustomerReceiptRequest_V01
                {
                    SenderEmail= invoiceModel.Email,
                    SenderName = invoiceModel.MemberFirstName +' '+invoiceModel.LastName,
                    Receipt = new LegacyCustomerReceiptInfo
                    {
                        ContactEmailAddress = invoiceModel.Email,
                        ContactFirstName= invoiceModel.FirstName,
                        ContactId= invoiceModel.CustomerId,
                        ContactLastName = invoiceModel.LastName,
                        ContactMobilePhoneNumber = invoiceModel.Phone,
                        DistributorFirstName= invoiceModel.MemberFirstName,
                        DistributorId= invoiceModel.MemberId,
                        DistributorLastName= invoiceModel.MemberLastName,
                        InvoiceNumber= invoiceModel.Id.ToString(),
                        LocaleCode= locale
                    }
                    
                };
                result = proxy.NotifyCustomerReceipts(new NotifyCustomerReceiptsRequest(request))
                    .NotifyCustomerReceiptsResult as NotifyLegacyCustomerReceiptResponse_V01;
                if(result != null && result.Status != ServiceProvider.NotificationSVC.ServiceResponseStatusType.Success)
                {
                    LoggerHelper.Error(
                        $"Notification Service : NotifyCustomerReceipts Error: {result.Message}, DistributorID:{invoiceModel.MemberId}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Notification Service : NotifyCustomerReceipts Error: {0}\n", ex.Message));
            }
            return result;
        }
    }
}
