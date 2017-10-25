using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ServiceProvider.ShippingSvc
{
    public partial class ShippingAddress
    {
        public ShippingAddress() { }

        public ShippingAddress(int id)
        {
            this.ID = id;
        }
    }

    public partial class ShippingOption_V01 
    {
        public ShippingOption_V01() : base() { }

        public ShippingOption_V01(string freightCode, string description, DateTime start, DateTime end)
            : base()
        {
            this.FreightCode = freightCode;
            this.Description = description;
            this.Start = start;
            this.End = end;
        }
    }

    public partial class ShippingAddress_V01 : ShippingAddress 
    {
        public ShippingAddress_V01() { }

        public ShippingAddress_V01(int id, string recipient, Address_V01 address, string phone, string altphone, bool isPrimary, string alias, DateTime created)
            : base(id)
        {
            ID = id;
            Recipient = recipient;
            Address = address;
            Phone = phone;
            AreaCode = AltAreaCode = AltPhone = string.Empty;
            IsPrimary = isPrimary;
            Alias = alias;
            Created = created;
            AltPhone = altphone;
        }
    }

    public partial class ShippingAddress_V02 : ShippingAddress_V01
    {
        public ShippingAddress_V02() { }

        public ShippingAddress_V02(int id, string recipient, string firstName, string lastName, string middleName, 
            Address_V01 address, string phone, string altphone, bool isPrimary, string alias, DateTime created)
            : base(id, recipient, address, phone, altphone, isPrimary, alias, created)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.MiddleName = middleName;
        }

        public ShippingAddress_V02(ShippingAddress_V02 srcAddress)
        {
            this.ID = srcAddress.ID;
            this.FirstName = srcAddress.FirstName;
            this.LastName = srcAddress.LastName;
            this.MiddleName = srcAddress.MiddleName;
            this.Recipient = srcAddress.Recipient;
            this.Attention = srcAddress.Attention;
            this.Address = new Address_V01();
            this.Address.City = srcAddress.Address.City;
            this.Address.Country = srcAddress.Address.Country;
            this.Address.CountyDistrict = srcAddress.Address.CountyDistrict;
            this.Address.Line1 = srcAddress.Address.Line1;
            this.Address.Line2 = srcAddress.Address.Line2;
            this.Address.Line3 = srcAddress.Address.Line3;
            this.Address.Line4 = srcAddress.Address.Line4;
            this.Address.PostalCode = srcAddress.Address.PostalCode;
            this.Address.CountyDistrict = srcAddress.Address.CountyDistrict;
            this.Address.StateProvinceTerritory = srcAddress.Address.StateProvinceTerritory;
            this.AreaCode = AltAreaCode = AltPhone = string.Empty;
            this.Phone = srcAddress.Phone;
            this.IsPrimary = srcAddress.IsPrimary;
            this.Alias = srcAddress.Alias;
            this.Created = srcAddress.Created;
        }

        public ShippingAddress_V02(ShippingAddress_V01 srcAddress)
        {
            this.ID = srcAddress.ID;
            this.Recipient = srcAddress.Recipient;
            this.Attention = srcAddress.Attention;
            this.Address = new Address_V01();
            this.Address.City = srcAddress.Address.City;
            this.Address.Country = srcAddress.Address.Country;
            this.Address.CountyDistrict = srcAddress.Address.CountyDistrict;
            this.Address.Line1 = srcAddress.Address.Line1;
            this.Address.Line2 = srcAddress.Address.Line2;
            this.Address.Line3 = srcAddress.Address.Line3;
            this.Address.Line4 = srcAddress.Address.Line4;
            this.Address.PostalCode = srcAddress.Address.PostalCode;
            this.Address.CountyDistrict = srcAddress.Address.CountyDistrict;
            this.Address.StateProvinceTerritory = srcAddress.Address.StateProvinceTerritory;
            this.AreaCode = AltAreaCode = AltPhone = string.Empty;
            this.Phone = srcAddress.Phone;
            this.IsPrimary = srcAddress.IsPrimary;
            this.Alias = srcAddress.Alias;
            this.Created = srcAddress.Created;
        }
    }
}