using System;

namespace MyHerbalife3.Ordering.SharedProviders.Invoices
{
    public class InvoiceSearchFilter
    {
        public bool SearchByDateOrAmount { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? StartAmount { get; set; }
        public decimal? EndAmount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }
        public decimal? TotalVolumePoints { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public bool ReturnEmptyResulys { get; set; }
    }
}