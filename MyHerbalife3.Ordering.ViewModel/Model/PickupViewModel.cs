#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PickupViewModel
    {
        public int DeliveryOptionId { get; set; }
        public string State { get; set; }
        public string DeliveryType { get; set; }
        public string OrderCategory { get; set; }
        public string Information { get; set; }
        public string Description { get; set; }
        public string FreightCode { get; set; }
        public string Warehouse { get; set; }
        public string Name { get; set; }
        public string AddressType { get; set; }
        public int ShippingIntervaldays { get; set; }
        public AddressViewModel Address { get; set; }
        public string Alias { get; set; }
        public string AltAreaCode { get; set; }
        public string AltPhone { get; set; }
        public string AreaCode { get; set; }
        public string Attention { get; set; }
        public DateTime Created { get; set; }
        public string DisplayName { get; set; }
        public bool IsPrimary { get; set; }
        public string Recipient { get; set; }
        public string Phone { get; set; }
        public string CourierType { get; set; }
        public int Id { get; set; }//id from the pulocation
        public int IDSaved { get; set; }//id from the saved PU location in the DB 
    }
}