namespace MyHerbalife3.Ordering.Providers.Shipping
{
    using System;
    using HL.Common.ValueObjects;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
    using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.OrderCategoryType;

    [Serializable]
    public class DeliveryOption : ShippingAddress_V02
    {
        #region Constructors and Destructors

        public DeliveryOption()
        {
        }

        public DeliveryOption(ShippingAddress_V02 address)
        {
            this.Option = DeliveryOptionType.Shipping;
            CopyAddress(address);
            this.Id = address.ID;
        }

        public DeliveryOption(ShippingAddress_V01 address)
        {
            this.Option = DeliveryOptionType.Shipping;
            CopyAddress(address);
            this.Id = address.ID;
        }

        public DeliveryOption(ShippingOption_V01 option)
        {
            this.Option = DeliveryOptionType.Shipping;
            this.Name = option.CourierName;
            this.Description = option.Description;
            this.FreightCode = option.FreightCode;
            this.WarehouseCode = option.ShippingSource == null ? "" : option.ShippingSource.Warehouse;
            this.ShippingIntervalDays = option.ShippingSource == null ? 0 : option.ShippingSource.ShippingInterval.Days;
            this.Id = option.ID;
            this.IsDefault = option.IsDefault;
            this.displayIndex = option.DisplayOrder;
        }

        public DeliveryOption(PickupOption_V01 option)
        {
            this.Option = DeliveryOptionType.Pickup;
            this.Name = option.BranchName;
            this.Description = option.CourierName;
            this.FreightCode = option.FreightCode;
            this.WarehouseCode = option.ShippingSource.Warehouse;
            CopyAddress(option.PickupAddress);
            this.Id = option.ID;
        }

        public DeliveryOption(DeliveryPickupOption_V02 option)
        {
            this.Option = option.DeliveryOptionType;
            this.OrderCategory = option.OrderCategoryType;
            this.Name = option.BranchName;
            this.Description = option.Description;
            this.FreightCode = option.FreightCode;
            this.WarehouseCode = option.ShippingSource.Warehouse;
            this.Information = option.AdditionalInformation;
            this.State = option.State;
            this.PostalCode = string.IsNullOrEmpty(option.PostalCode) ? string.Empty : option.PostalCode.Trim();
            CopyAddress(option.PickupAddress);
            this.Id = option.ID;
            this.IsETO = option.IsETO;
            if (option.ShippingSource != null)
            {
                ShippingIntervalDays = option.ShippingSource.ShippingInterval.Days;
            }
        }

        public DeliveryOption(DeliveryPickupOption_V03 option, bool isCourier = false)
        {
            this.Option = option.DeliveryOptionType;
            this.OrderCategory = option.OrderCategoryType;
            this.Name = option.CourierAddress;
            this.Description = option.Description;
            this.FreightCode = option.FreightCode;
            this.WarehouseCode = option.WarehouseCode;
            this.Information = option.AdditionalInformation;
            this.State = option.State;
            this.PostalCode = string.IsNullOrEmpty(option.PostalCode) ? string.Empty : option.PostalCode.Trim();
            if (option.ShippingSource != null)
            {
                AddressType = option.ShippingSource.Status;
                ShippingIntervalDays = option.ShippingSource.ShippingInterval.Days;
            }
            
            if (isCourier)
            {
                this.Description = option.CourierName;
                this.Name = option.Description;
                this.CourierType = option.BranchName;
                CopyAddress(option.PickupAddress);
            }
            this.Id = option.ID;
            this.Address = option.PickupAddress != null ? option.PickupAddress.Address : null;
            this.Phone = option.PickupAddress.Phone;
        }

        public DeliveryOption(DeliveryPickupOption_V04 option, bool isCourier = false)
        {
            this.Id = option.ID;
            this.Option = option.DeliveryOptionType;
            this.OrderCategory = option.OrderCategoryType;
            this.Name = option.CourierName;
            this.Description = option.Description;
            this.FreightCode = option.FreightCode;
            this.WarehouseCode = option.WarehouseCode;
            this.Information = option.AdditionalInformation;
            this.State = option.State;
            this.PostalCode = string.IsNullOrEmpty(option.PostalCode) ? string.Empty : option.PostalCode.Trim();
            this.displayIndex = option.DisplayOrder;
            if (isCourier)
            {
                this.CourierType = option.BranchName;
                this.CourierStoreId = option.CourierStoreId;
                this.GeographicPoint = option.GeographicalPoints;
                //this.Distance = string.Format("{0} {1}", option.Distance.ToString("N2"), option.DistanceUnit);
                this.Distance = option.Distance;
                this.DistanceUnit = option.DistanceUnit;
                CopyAddress(option.PickupAddress);
            }
            else
            {
                this.Address = option.PickupAddress != null ? option.PickupAddress.Address : null;
                this.Phone = option.PickupAddress != null ? option.PickupAddress.Phone : string.Empty;
            }
            if (option.ShippingSource != null)
            {
                AddressType = option.ShippingSource.Status;
                ShippingIntervalDays = option.ShippingSource.ShippingInterval.Days;
            }
        }

        public DeliveryOption(string warehouseCode, string freightCode, DeliveryOptionType type)
        {
            this.Option = type;
            this.FreightCode = freightCode;
            this.WarehouseCode = warehouseCode;
        }

        public DeliveryOption(string warehouseCode, string freightCode, DeliveryOptionType type, string description)
        {
            this.Option = type;
            this.FreightCode = freightCode;
            this.WarehouseCode = warehouseCode;
            this.Description = description;
        }

        #endregion

        #region Public Properties

        public string AddressType { get; set;  } // added for China


        public string Description { get; set; }

        public bool Display { get; set; }

        public string FreightCode { get; set; }

        public int Id { get; set; }

        public string Information { get; set; }

        public bool IsActive { get; set; }

        public bool IsDefault { get; set; }

        public string Name { get; set; }

        public bool IsETO { get; set; }
        //Id from respective data source / provider
        public DeliveryOptionType Option { get; set; }

        //Shipping or Pickup
        /// TODO : replace name here 
        public OrderCategoryType OrderCategory { get; set; }

        //NickName or DS Name?
        //public ShippingAddress_V01 Address { get; set; } //Ship to or Pickup address
        public int ShippingIntervalDays { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        //Deliver days

        //Display control - ie show or not show when not IsActive
        public string WarehouseCode { get; set; }

        public int displayIndex { get; set; }

        public string CourierType { get; set; }

        public string GeographicPoint { get; set; }

        public string CourierStoreId { get; set; }

        public decimal Distance { get; set; }

        public string DistanceUnit { get; set; }

        public decimal? FirstWeight { get; set; }

        public decimal? FirstPrice { get; set; }

        public decimal? BasePrice { get; set; }

        public decimal? RenewalPrice { get; set; }

        public decimal? EstimatedFee { get; set; }

        #endregion

        // added 8-26-2010

        #region Methods

        private void CopyAddress(ShippingAddress_V01 address)
        {
            this.Address = address.Address;
            this.Alias = address.Alias;
            this.AltAreaCode = address.AltAreaCode;
            this.AltPhone = address.AltPhone;
            this.AreaCode = address.AreaCode;
            this.Phone = address.Phone;
            this.Attention = address.Attention;
            this.Created = address.Created;
            this.DisplayName = address.DisplayName;
            this.IsPrimary = address.IsPrimary;
            this.Recipient = address.Recipient;
            this.ID = address.ID;
        }

        private void CopyAddress(ShippingAddress_V02 address)
        {
            this.Address = address.Address;
            this.Alias = address.Alias;
            this.AltAreaCode = address.AltAreaCode;
            this.AltPhone = address.AltPhone;
            this.AreaCode = address.AreaCode;
            this.Phone = address.Phone;
            this.Attention = address.Attention;
            this.Created = address.Created;
            this.DisplayName = address.DisplayName;
            this.IsPrimary = address.IsPrimary;
            this.Recipient = address.Recipient;
            this.ID = address.ID;
            this.FirstName = address.FirstName;
            this.LastName = address.LastName;
            this.MiddleName = address.MiddleName;
            this.AddressId = address.AddressId;
            this.CustomerId = address.CustomerId;
            this.HasAddressRestriction = address.HasAddressRestriction;
        }

        #endregion
    }
}