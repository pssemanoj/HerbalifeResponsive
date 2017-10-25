using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface ICartWidgetSource
    {
        CartWidgetModel GetCartWidget(string id, string countryCode, string locale);
        CartWidgetModel AddToCart(CartWidgetModel cartWidgetModel, string id, string countryCode, string locale);
    }
}
