using System.Collections.Generic;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public interface PaymentInfoBase
    {
        PaymentGatewayControl PaymentGatewayInterface { get; set; }
        void LoadCards();
        bool IsUsingPaymentGateway { get; }
        PaymentMethodType PaymentGatewayPaymentMethod { get; set; }
        List<Payment> GetPayments(Address_V01 shippingAddress);
        List<PaymentInformation> GetCurrentPaymentInformation(string distributorId, string locale);
        void SetCurrentPaymentInformation(List<PaymentInformation> payment, string distributorId, string locale);
        List<string> Errors { get; }
        bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments);
        bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments, bool showErrors);
        bool IsPaymentError();
        bool IsAcknowledged { get; }
        void Refresh();
    }
}