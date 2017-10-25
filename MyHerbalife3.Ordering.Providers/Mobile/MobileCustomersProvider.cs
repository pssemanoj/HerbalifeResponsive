#region

using System.Collections.Generic;
using System.Globalization;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileCustomersProvider : IMobileCustomersProvider
    {
        public PreferredCustomerResponseViewModel GetPreferredCustomers(PreferredCustomerRequestViewModel request)
        {
            var response = new PreferredCustomerResponseViewModel();

            #region validation

            if ((request == null) || string.IsNullOrWhiteSpace(request.MemberId))
            {
                response.Customers = null;
                return response;
            }

            #endregion

            var result = China.OrderProvider.GetPreferredCustomers(request.MemberId, request.From, request.To);
            if (result == null) return response;

            response.Customers = new List<PreferredCustomerViewModel>();

            foreach (var model in result)
            {
                var retItem = new PreferredCustomerViewModel
                {
                    MemberId = !string.IsNullOrEmpty(model.CustomerId) ? model.CustomerId.Trim(): model.CustomerId,
                    CustomerId = model.CustomerProfileId.ToString(CultureInfo.InvariantCulture),
                    Name = model.NameCn,
                };

                response.Customers.Add(retItem);
            }

            return response;
        }
    }
}