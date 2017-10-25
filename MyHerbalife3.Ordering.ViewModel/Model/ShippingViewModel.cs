#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ShippingViewModel
    {
        public Guid Id { get; set; }
        public string Recipient { get; set; }

        public string RecipientIdentification { get; set; }
        public string RecipientIdentificationType { get; set; }

        public string Phone { get; set; }

        public string ShippingMethodId { get; set; }

        public string Carrier { get; set; }

        public string WarehouseCode { get; set; }

        public string FreightVariant { get; set; }

        public AddressViewModel Address { get; set; }

        /// <summary>
        ///     Id (int) of the NickName dropdownlist - DeliveryOptionType.Pickup, DeliveryOptionType.PickupFromCourier
        /// </summary>
        public int DeliveryOptionId { get; set; }

        //Pickup, PickupFromCourier, Shipping
        public string DeliveryType { get; set; }

        public string Email { get; set; }

        public string Greeting { get; set; }

        public string StoreName { get; set; }
    }
}