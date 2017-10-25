using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_HR : ShippingProviderBase
    {
        public const int CitylookupCacheMinutes = 60;
        public const string CitylookupCachePrefix = "CITYLOOKUP_HR";

        public override List<string> GetCitiesForState(string country, string state)
        {
            if (HttpRuntime.Cache[CitylookupCachePrefix] != null)
            {
                return HttpRuntime.Cache[CitylookupCachePrefix] as List<string>;
            }
            try
            {
                List<string> dictCityZip = null;
                var proxy = ServiceClientProvider.GetShippingServiceProxy();
                var request = new CitiesForStateRequest_V02 { Country = country };
                var response = proxy.GetCitiesForState(new GetCitiesForStateRequest(request)).GetCitiesForStateResult as CitiesForStateResponse_V02;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    if (response.DictCityZip != null && response.DictCityZip.Count() > 0)
                    {
                        // order by city name
                        dictCityZip =
                            response.DictCityZip.OrderBy(l => l.Value).Select(l => l.Key + "," + l.Value).ToList();
                        HttpRuntime.Cache.Insert(
                            CitylookupCachePrefix,
                            dictCityZip,
                            null,
                            DateTime.Now.AddMinutes(CitylookupCacheMinutes),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Normal,
                            null);
                    }
                }

                return dictCityZip;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("GetCitiesForState error: Country {0}, error: {1}", country, ex));
            }
            return null;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            string  instructions = "";
            if (shoppingCart.DeliveryInfo.PickupDate == null)
            {
                instructions = string.Format("{0}", shoppingCart.DeliveryInfo.Address.Phone);
            }
          
            else
            {
                instructions = string.Format("{0} {1}", shoppingCart.DeliveryInfo.Address.Phone, shoppingCart.DeliveryInfo.PickupDate.Value.ToString("dd/MM/yy"));
            }
            

            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                instructions = string.Format("{0} {1}", shoppingCart.DeliveryInfo.Address.Recipient, instructions);
            }

            return instructions;
        }
    }
}
