#region

using System;
using System.Runtime.Serialization;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PaymentViewModel
    {
        public Guid Id { get; set; }
        public string LineId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string ReferenceId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string TransactionType { get; set; }
        public AddressViewModel Address { get; set; }
        public String ApsDistributorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}