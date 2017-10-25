#region

using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileAddressProvider
    {
        List<AddressViewModel> Get(GetAddressRequestViewModel request);
        List<AddressViewModel> Get(string memberId, string locale);
        int Save(ref AddressViewModel address, string memberId, string locale);
        AddressViewModel Delete(string memberId, Guid id, string locale);

        List<CustomShippingMethodViewModel> GetShippingMethods(string memberId, Guid id, string locale);
    }
}