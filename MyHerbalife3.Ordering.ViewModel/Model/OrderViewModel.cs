#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using HL.Common.Configuration;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderViewModel
    {
        public List<OrderItemViewModel> OrderItems { get; set; }
        public string OrderMonth { get; set; }
        public string CategoryType { get; set; }
        public ShippingViewModel Shipping { get; set; }
        public HandlingInfoViewModel HandlingInfo { get; set; }
        public string CountryOfProcessing { get; set; }
        public decimal DiscountPercentage { get; set; }
        public List<PaymentViewModel> Payments { get; set; }
        public int pcLearningPointOffSet { get; set; }
        public int pcLearningPointEligibleOffSet { get; set; }
        public OrderTotalsViewModel Quote { get; set; }
        
        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? CreatedDate { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? LastUpdatedDate { get; set; }

        public string MemberId { get; set; }
        public string CustomerId { get; set; }
        public Guid Id { get; set; }
        public string Locale { get; set; }
        public string Status { get; set; }

        public string StatusForDisplay { get; set; }

        public string OrderNumber { get; set; }

        public string OrderNumberDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(OrderNumber) &&
                    !string.IsNullOrEmpty(Settings.GetRequiredAppSetting("OrderNumberPrefix")) &&
                    CountryOfProcessing == "CN")
                {
                    return Settings.GetRequiredAppSetting("OrderNumberPrefix") + '-' + OrderNumber;
                }
                return OrderNumber;
            }
        }

        public List<DonationViewModel> Donations { get; set; }

        [JsonIgnore]
        public string OrderMemberId { get; set; }

        public bool CopyEnabled { get; set; }
    }
}