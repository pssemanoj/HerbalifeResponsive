#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceAddressModel : ICloneable
    {
        public int Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string County { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class DisplayInvoiceAddressModel : InvoiceAddressModel
    {
        public int Id { get; set; }
        public string Alias { get; set; }
    }
}