#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoicePriceModel : ICloneable
    {
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CalcDiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingPercentage { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalDue { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalEarnBase { get; set; }
        public decimal CalcShippingAmount { get; set; }
        public decimal CalcTaxAmount { get; set; }
        public decimal TotalDiscountedAmount { get; set; }
        public decimal MemberTax { get; set; }
        public decimal MemberFreight { get; set; }
        public decimal MemberTotal { get; set; }
        public decimal TotalYourPrice { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercentage { get; set; }
        public decimal MemberStaticDiscount { get; set; }
        public decimal MemberDiscount { get; set; }
        public decimal TotalVolumePoints { get; set; }
        public string DisplayDiscountedAmount { get; set; }

        public string DisplayCurrencySymbol { get; set; }

        public string DisplaySubtotal { get; set; }

        public string DisplayShipping { get; set; }

        public string DisplayTax { get; set; }

        public string DisplayTotalDue { get; set; }

        public string DisplayCalculatedTax { get; set; }

        public string DisplayProfit { get; set; }

        public string DisplayMemberTax { get; set; }

        public string DisplayMemberFreight { get; set; }

        public string DisplayMemberTotal { get; set; }

        public string DisplayTotalYourPrice { get; set; }

        public string DisplayProfitPercentage
        {
            get
            {
                return String.Format("({0}{1})",
                    ProfitPercentage,
                    "%");
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}