#region

using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IInvoiceShippingDetails
    {
        string GetWarehouseCode(Address address, string locale);
        string GetShippingMethodId(Address address, string locale);
        IEnumerable<KeyValuePair<string, string>> GetStates(string locale);
        bool ValidateAddress(Address_V01 address, out Address_V01 avsOutputAddress);
    }
}