#region

using System;
using System.Collections.Generic;
using System.Globalization;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceModel : ICloneable
    {
        public int Id { get; set; }
        public string MemberId { get; set; }
        public int MemberInvoiceNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CustomerId { get; set; }
        public InvoiceAddressModel Address { get; set; }
        public List<InvoiceLineModel> InvoiceLines { get; set; }
        public InvoicePriceModel InvoicePrice { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string OrderId { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public decimal TotalVolumePoints { get; set; }
        public decimal Total { get; set; }
        public string DisplayTotal { get; set; }
        public bool InvoiceShipToAddress { get; set; }
        public bool HasShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        //property used for UK
        public string Vat { get; set; }
        public string ApplicationCountryCode { get; set; }
        public string NotificationURL { get; set; }

        public bool IsDisplayVat
        {
            get
            {
                var locale = CultureInfo.CurrentUICulture.Name;
                return ApplicationCountryCode == "GB" || locale.Substring(3) == "GB" || ApplicationCountryCode == "KR" ||
                       locale.Substring(3) == "KR";
            }
        }

        public bool HideTax
        {
            get
            {
                var locale = CultureInfo.CurrentUICulture.Name;
                return (ApplicationCountryCode == "KR" ||
                        locale.Substring(3) == "KR") && (OrderSource == "DWS");

            }
        }

        public bool IsDisplayFreeShipping
        {
            get
            {                
                return (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasFreeShippingLabel && null != InvoicePrice && InvoicePrice.CalcShippingAmount == 0);                
            }
        }

        public string DisplayMemberInvoiceNumber
        {
            get
            {
                var decimalength = MemberInvoiceNumber.ToString("D").Length + 6 - MemberInvoiceNumber.ToString().Length;
                var locale = CultureInfo.CurrentUICulture.Name;
                if (ApplicationCountryCode == "GB" || locale.Substring(3) == "CA" || locale.Substring(3) == "TT"|| locale.Substring(3) == "JM"|| locale.Substring(3) == "GB")
                {
                    return MemberId + MemberInvoiceNumber.ToString("D" + decimalength);
                }
                return MemberInvoiceNumber.ToString("D" + decimalength);
            }
        }

        public InvoiceAddressModel MemberAddress { get; set; }
        public string MemberEmailAddress { get; set; }

        public string MemberFirstName { get; set; }
        public string MemberLastName { get; set; }
        public string MemberPhoneNumber { get; set; }

        public string OrderSource { get; set; }

        public string PricingType { get; set; }

        public string Source { get; set; }

        public bool IsFaceToFace { get; set; }

        public bool IsGdoMemberPricing { get; set; }

        public string DisplayInvoiceStatus { get; set; }

        public string DisplayInvoiceType { get; set; }

        public bool IsMemberTaxEdited { get; set; }

        public bool IsMemberHandlingFeeEdited { get; set; }

        public bool MakeMemberTaxZero { get; set; }

        public bool MakeMemberHandlingFeeZero { get; set; }

        public bool IsCustomerTaxEdited { get; set; }

        public bool IsCustomerShippingHandlingEdited { get; set; }
        public string ReceiptChannel { get; set; }
        public string DisplayReceiptChannel { get; set; }
        public string DisplayPaymentType { get; set; }
        public string OtherEmail { get; set; }
        public string SMSNumber { get; set; }

        public bool ResetCustomerTaxValue { get; set; }
        public ClubInvoiceModel ClubInvoice { get; set; }
        public string PaymentType { get; set; }
        public bool isProfitable { get; set; }
        public bool isPurchased { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}