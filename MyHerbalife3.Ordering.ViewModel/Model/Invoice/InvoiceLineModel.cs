using System;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceLineModel : ICloneable
    {
        public int InvoiceId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal TotalRetailPrice { get; set; }
        public decimal EarnBase { get; set; }
        public string StockingSku { get; set; }
        public string ProductType { get; set; }
        public string ProductCategory { get; set; }
        public decimal VolumePoint { get; set; }
        public decimal CalcDiscountedAmount { get; set; }
        public decimal TotalEarnBase { get; set; }
        public decimal TotalVolumePoint { get; set; }
        public decimal FreightCharge { get; set; }
        public decimal YourPrice { get; set; }

        public string DisplayRetailPrice { get; set; }

        public string DisplayTotalRetailPrice { get; set; }

        public string DisplayCurrencySymbol { get; set; }

        public string DisplayTotalVp { get; set; }

        public string DisplayYourPrice { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}