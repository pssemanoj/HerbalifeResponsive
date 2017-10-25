using System;
using System.Globalization;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Mvc;

namespace MyHerbalife3.Ordering.Widgets
{
    public class CartWidgetController : ApiController
    {
        private static ICartWidgetSource _cartWidgetSource;

        public static void Inject(ICartWidgetSource cartWidgetSource)
        {
            _cartWidgetSource = cartWidgetSource;
        }


        [WebApiCultureSwitching]
        [Authorize]
        public CartWidgetModel Get()
        {
            return Get(User.Identity.Name);
        }

        [HttpPost]
        [Authorize]
        public CartWidgetModel Post(CartWidgetModel cartWidgetModel)
        {
            try
            {
                var id = User.Identity.Name;
                var locale = CultureInfo.CurrentCulture.Name;
                var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
                return _cartWidgetSource.AddToCart(cartWidgetModel, id, countrycode, locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed HttpPost AddToCart for DS id" + User.Identity.Name);
            }
            return null;
        }

        internal CartWidgetModel Get(string id)
        {
            try
            {
                var locale = CultureInfo.CurrentCulture.Name;
                var countrycode = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
                var model =
                    _cartWidgetSource.GetCartWidget(id, countrycode, locale);
                return model;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading CartwidgetModel for id" + id);
            }

            return null;
        }
    }
}