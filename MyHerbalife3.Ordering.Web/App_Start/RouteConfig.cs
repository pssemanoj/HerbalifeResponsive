using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace MyHerbalife3.Ordering.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.EnableFriendlyUrls();
            routes.Ignore("{*staticfile}", new { staticfile = @".*\.(css|js|gif|jpg|jpeg|png)" });

            routes.MapRoute(
               name: "OrderingInventoryDetailsRoute",
               url: "{vertical}/Inventory/{action}/{id}",
               defaults: new { controller = "Inventory", action = "Index", id = UrlParameter.Optional },
               constraints: new { vertical = "(ordering)" }
               );

            routes.MapRoute(
               name: "OrderingInvoiceRoute",
               url: "{vertical}/Invoice/{action}/{id}",
               defaults: new { controller = "Invoice", action = "Index", id = UrlParameter.Optional },
               constraints: new { vertical = "(ordering)" }
               );

            routes.MapRoute(
                name: "orderingAuthRoute",
                url: "ordering/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { controller = @"(orderingAuth)" }
                );

            // Only out controllers caught by this route. This prevents rogue urls from activating non-existing controllers.
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { controller = @"(authentication|orderingAuth|cms|home|error)" }
                );
        }
    }
}