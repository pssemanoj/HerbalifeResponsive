using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Mvc;

namespace MyHerbalife3.Ordering.Widgets
{
    public class RecentOrdersController : ApiController
    {
        private static IRecentOrdersSource _recentOrdersSource;

        public static void Inject(IRecentOrdersSource recentOrdersSource)
        {
            _recentOrdersSource = recentOrdersSource;
        }

        [WebApiCultureSwitching]
        [Authorize]
        public Task<IEnumerable<RecentOrderModel>> Get()
        {
            return Get(User.Identity.Name);
        }

        internal async Task<IEnumerable<RecentOrderModel>> Get(string id)
        {
            try
            {
                var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
                //TODO: make this an async loader
                var model =
                    _recentOrdersSource.GetRecentOrders(id, countrycode);
                return model;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading MyRecentOrders for id" + id);
            }

            return null;
        }
    }
}