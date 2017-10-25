using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Mvc;

namespace MyHerbalife3.Ordering.Widgets
{
    public class TopSellerProductsController : ApiController
    {
        private static ITopSellerSource _topSellerSource;

        public static void Inject(ITopSellerSource topSellerSource)
        {
            _topSellerSource = topSellerSource;
        }


        [WebApiCultureSwitching]
        [Authorize]
        public List<TopSellerProductModel> Get()
        {
            return Get(User.Identity.Name);
        }

        internal List<TopSellerProductModel> Get(string id)
        {
            try
            {
                var locale = CultureInfo.CurrentCulture.Name;
                var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
                var model =
                    _topSellerSource.GetTopSellerProducts(id, countrycode, locale);
                return model;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading topSellerProducts for id" + id);
            }

            return null;
        }
    }
}