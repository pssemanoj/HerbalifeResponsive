using System;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingInfo
    {
        public event EventHandler FreightCodeChanged;

        #region Construction/Finalization

        public ShippingInfo()
        {
        }

        public ShippingInfo(PickupLocationPreference_V01 pickupPref, DeliveryOption deliveryOption)
        {
            this.Id = pickupPref.ID;
            Option = DeliveryOptionType.Pickup;
            Description = deliveryOption.Name;
            Name = pickupPref.PickupLocationNickname;
            if (deliveryOption.Address != null)
            {
                Address = CopyAddress(deliveryOption);
            }
            FreightCode = deliveryOption.FreightCode;
            WarehouseCode = deliveryOption.WarehouseCode;
            ShippingIntervalDays = deliveryOption.ShippingIntervalDays;
        }

        public ShippingInfo(DeliveryOption deliveryOption)
        {
            Id = deliveryOption.Id;
            Option = deliveryOption.Option;
            FreightCode = deliveryOption.FreightCode;
            WarehouseCode = deliveryOption.WarehouseCode;
            Description = deliveryOption.Description;
            Name = deliveryOption.Name;
            AdditionalInformation = deliveryOption.Information;
            if (deliveryOption.Address != null)
            {
                Address = CopyAddress(deliveryOption);
            }
            ShippingIntervalDays = deliveryOption.ShippingIntervalDays;
        }

        public ShippingInfo(DeliveryOptionType option, DeliveryOption deliveryOption)
        {
            if (deliveryOption != null)
            {
                Id = deliveryOption.Id;
                Option = option;
                FreightCode = deliveryOption.FreightCode;
                WarehouseCode = deliveryOption.WarehouseCode;
                Description = deliveryOption.Description;
                Name = deliveryOption.Name;
                AdditionalInformation = deliveryOption.Information;
                if (deliveryOption.Address != null)
                {
                    Address = CopyAddress(deliveryOption);
                }
                ShippingIntervalDays = deliveryOption.ShippingIntervalDays;
            }
            else
            {
                Id = 0;
                Option = option;
                FreightCode = string.Empty;
                WarehouseCode = string.Empty;
                Description = string.Empty;
                Name = string.Empty;
            }
        }

        public ShippingAddress_V01 CopyAddress(DeliveryOption srcAddress)
        {
            return new ShippingAddress_V01()
            {
                Address = new Address_V01()
                {
                    Line1 = srcAddress.Address.Line1,
                    Line2 = srcAddress.Address.Line2,
                    Line3 = srcAddress.Address.Line3,
                    Line4 = srcAddress.Address.Line4,
                    City = srcAddress.Address.City,
                    CountyDistrict = srcAddress.Address.CountyDistrict,
                    StateProvinceTerritory = srcAddress.Address.StateProvinceTerritory,
                    Country = srcAddress.Address.Country,
                    PostalCode = srcAddress.Address.PostalCode
                },
                Alias = srcAddress.Alias,
                AltAreaCode = srcAddress.AltAreaCode,
                AltPhone = srcAddress.AltPhone,
                AreaCode = srcAddress.AreaCode,
                Phone = srcAddress.Phone,
                Attention = srcAddress.Attention,
                Created = srcAddress.Created,
                DisplayName = srcAddress.DisplayName,
                IsPrimary = srcAddress.IsPrimary,
                Recipient = srcAddress.Recipient,
                ID = srcAddress.ID,
            };
        }

        public ShippingAddress_V01 CopyAddress(ShippingAddress_V01 srcAddress)
        {
            return new ShippingAddress_V01()
            {
                Address = new Address_V01()
                {
                    Line1 = srcAddress.Address.Line1,
                    Line2 = srcAddress.Address.Line2,
                    Line3 = srcAddress.Address.Line3,
                    Line4 = srcAddress.Address.Line4,
                    City = srcAddress.Address.City,
                    CountyDistrict = srcAddress.Address.CountyDistrict,
                    StateProvinceTerritory = srcAddress.Address.StateProvinceTerritory,
                    Country = srcAddress.Address.Country,
                    PostalCode = srcAddress.Address.PostalCode
                },
                Alias = srcAddress.Alias,
                AltAreaCode = srcAddress.AltAreaCode,
                AltPhone = srcAddress.AltPhone,
                AreaCode = srcAddress.AreaCode,
                Phone = srcAddress.Phone,
                Attention = srcAddress.Attention,
                Created = srcAddress.Created,
                DisplayName = srcAddress.DisplayName,
                IsPrimary = srcAddress.IsPrimary,
                Recipient = srcAddress.Recipient,
                ID = srcAddress.ID,
            };
        }

        public static ShippingAddress_V02 PromoteAddress(ShippingAddress_V01 srcAddress)
        {
            return new ShippingAddress_V02()
            {
                Address = new Address_V01()
                {
                    Line1 = srcAddress.Address.Line1,
                    Line2 = srcAddress.Address.Line2,
                    Line3 = srcAddress.Address.Line3,
                    Line4 = srcAddress.Address.Line4,
                    City = srcAddress.Address.City,
                    CountyDistrict = srcAddress.Address.CountyDistrict,
                    StateProvinceTerritory = srcAddress.Address.StateProvinceTerritory,
                    Country = srcAddress.Address.Country,
                    PostalCode = srcAddress.Address.PostalCode
                },
                Alias = srcAddress.Alias,
                AltAreaCode = srcAddress.AltAreaCode,
                AltPhone = srcAddress.AltPhone,
                AreaCode = srcAddress.AreaCode,
                Phone = srcAddress.Phone,
                Attention = srcAddress.Attention,
                Created = srcAddress.Created,
                DisplayName = srcAddress.DisplayName,
                IsPrimary = srcAddress.IsPrimary,
                Recipient = srcAddress.Recipient,
                ID = srcAddress.ID,
            };
        }

        public ShippingInfo(DeliveryOption deliveryOption, ShippingAddress_V01 shippingAddress)
        {
            if (deliveryOption != null)
            {
                Id = deliveryOption.Id;

                Option = deliveryOption.Option;
                FreightCode = deliveryOption.FreightCode;
                WarehouseCode = deliveryOption.WarehouseCode;
                Description = deliveryOption.Description;
                Name = deliveryOption.Name;
                ShippingIntervalDays = deliveryOption.ShippingIntervalDays;
            }

            if (shippingAddress != null)
            {
                Address = CopyAddress(shippingAddress);
            }

            if (deliveryOption != null)
            {
                this.AdditionalInformation = deliveryOption.Information;
            }
        }

        #endregion Construction/Finalization

        #region Fields

        private string _FreightCode = string.Empty;
        private string _FreightVariant = string.Empty;

        #endregion Fields

        #region Public properties

        public int Id { get; set; }

        public DeliveryOptionType Option { get; set; } // shipping or pickup

        public string WarehouseCode { get; set; }

        public string FreightVariant { get { return GetFrieghtVariant(); } set { _FreightVariant = value; } } // japan

        public string Description { get; set; } // for pickup: this is something like "Tokyo pickup location

        public string Name { get; set; } //NickName or DS Name?

        public ShippingAddress_V01 Address { get; set; } // shipping or pickup address + Phone + Recipient

        public string Instruction { get; set; } // shipping or pickup instructions

        public DateTime? PickupDate { get; set; }

        public string AdditionalInformation { get; set; } //Location information

        public string AddressType { get; set;  } // added for china DO

        public int ShippingIntervalDays { get; set; } // added for china DO

        public string FreightCode
        {
            get { return _FreightCode; }
            set
            {
                if (_FreightCode != value)
                {
                    _FreightCode = value;
                    //OnFreightCodeChanged();
                }
            }
        }

        // TODO, Verify if this value should be stored inside the Address field, in which case the ShippingAddress_V01 object must be modified
        public string RGNumber { get; set; }

        public string HKID { get; set; }

        public string DeliveryNickName { get; set; }

        #endregion Public properties

        #region Private methods

        private void OnFreightCodeChanged()
        {
            if (null != FreightCodeChanged)
            {
                FreightCodeChanged(this, new EventArgs());
            }
        }

        private string GetFrieghtVariant()
        {
            string variant = this._FreightVariant;
            //if (string.IsNullOrEmpty(variant))
            {
                //if (this.Option == DeliveryOptionType.Shipping && null != this.Address && null != this.Address.Address)
                {
                    variant = Shipping.ShippingProvider.GetShippingProvider(null).GetFreightVariant(this);
                }
            }

            return variant;
        }

        #endregion Private methods
    }
}