using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobilePickUpProvider
    {
        List<PickupViewModel> GetDeliveryOptions(string locale, AddressViewModel address);

        List<PickupViewModel> GetSavedDeliveryOptions(string memberId, string locale);

        PickupViewModel SavePickUpPreference(string memberId, PickupViewModel model, string locale);

        bool DeletePickUpLocation(string memberId, PickupViewModel model, string locale);

    }
}
