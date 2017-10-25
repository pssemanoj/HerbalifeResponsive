#region

using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using MyHerbalife3.Ordering.Controllers.ApiVersioning;
using System.Web.Http.Dispatcher;

#endregion

namespace MyHerbalife3.Ordering.Web.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute("invoiceCategories_LoadCategories",
                "ordering/api/InvoiceBase/LoadCategories/{id}/{type}",
                new {controller = "InvoiceBase", action = "LoadCategories", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("invoiceCategories_GetInvoiceLineModel",
                "ordering/api/InvoiceBase/GetInvoiceLineModel/{id}/{type}",
                new {controller = "InvoiceBase", action = "GetInvoiceLineModel", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("invoiceCategories_LoadProductsForAutocomplete",
                "ordering/api/InvoiceBase/LoadProductsForAutocomplete/{id}/{type}",
                new {controller = "InvoiceBase", action = "LoadProductsForAutocomplete", id = UrlParameter.Optional});
            

            config.Routes.MapHttpRoute("apiMethodCall", "ordering/api/{controller}/{action}/{id}",
                new {controller = "InvoiceSearch", action = "Get", id = UrlParameter.Optional},
                new {controller = @"(InvoiceSearch|InvoiceBase|InvoiceCreate|InvoiceEdit)"}
                );

            config.Routes.MapHttpRoute("ordering mobile web api DualOrderMonth", "ordering/V5/orders/month",
                new {controller = "MobileDualOrderMonth"});

            config.Routes.MapHttpRoute("ordering mobile web api getOrders", "ordering/V5/{memberId}/orders",
                new {controller = "MobileOrder"});

            config.Routes.MapHttpRoute("ordering mobile web api DeleteAndGetOrder by orderId",
                "ordering/V5/{memberId}/orders/{id}",
                new {controller = "MobileOrder", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("ordering mobile web api cancellOrder by orderNumber",
                "ordering/V5/{memberId}/order/{id}/cancellation",
                new {controller = "MobileOrder", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("ordering mobile web api quote", "ordering/V5/{memberId}/quote",
                new { controller = "MobileQuote" });

            config.Routes.MapHttpRoute("ordering mobile web api GetFavouriteSKU", "ordering/V5/{memberId}/GetFavouriteSKUs",
                new { controller = "MobileFavouriteSKU" });

            config.Routes.MapHttpRoute("ordering mobile web api UpdateFavouriteSKUs", "ordering/V5/{memberId}/UpdateFavouriteSKU",
                new { controller = "MobileFavouriteSKU" });

            config.Routes.MapHttpRoute("ordering mobile web api address", "ordering/V5/{memberId}/addresses",
                new {controller = "MobileAddress"});

            config.Routes.MapHttpRoute("ordering mobile web api DeleteAndGetAddress by CloudId",
                "ordering/V5/{memberId}/addresses/{id}",
                new {controller = "MobileAddress", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("ordering mobile web api shippingMethods by CloudId",
                "ordering/V5/{memberId}/addresses/{id}/shippingMethods",
                new {controller = "MobileShippingMethods", id = UrlParameter.Optional});

            config.Routes.MapHttpRoute("ordering mobile pickup", "ordering/V5/{memberId}/PickupStores",
                new {controller = "MobilePickup"});

            config.Routes.MapHttpRoute("ordering mobile web api announcement", "ordering/V5/announcement",
                new {controller = "MobileAnnouncement"});

            config.Routes.MapHttpRoute("ordering mobile web api PayByPhone",
                "ordering/V5/{memberId}/PayByPhone",
                new {controller = "MobilePayByPhone"});

            config.Routes.MapHttpRoute("ordering mobile web api ExpireCatalogCache",
                "ordering/V5/ExpireCatalogCache/{cacheName}/{inputCacheKey}",
                new { controller = "MobileExpireCatalogCache" });

            config.Routes.MapHttpRoute("ordering mobile web api GetPrefferedCustomers",
                "ordering/V5/{memberId}/Customers",
                new {controller = "MobileCustomers"});

            config.Routes.MapHttpRoute("ordering mobile web api GetOrderList",
                "ordering/V5/{memberId}/order/summary",
                new {controller = "MobileOrderSummary", action = "Get"});

            config.Routes.MapHttpRoute("ordering mobile web api pgh insert",
                "ordering/V5/{memberId}/order/pgh",
                new {controller = "MobilePgh"});

            config.Routes.MapHttpRoute("ordering mobile web api wechat prepay",
                "ordering/V5/{memberId}/order/wechat",
                new { controller = "MobileWechat" });

            config.Routes.MapHttpRoute("Vertical Specific Web API", "{vertical}/api/{controller}/{id}",
                new {id = RouteParameter.Optional}, new {vertical = "(custom|account|ordering|bizworks)"}
                );

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new {id = RouteParameter.Optional}
                );

            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            config.Routes.MapHttpRoute("Sandbox WebApi", "ordering/V5/Sandbox",
                new {controller = "MobileSandbox"});

            config.Routes.MapHttpRoute("ordering mobile web api express info by orderNumber",
                "ordering/V5/{memberId}/ordertracking/{orderid}",
                new { controller = "MobileOrderTracking", id = UrlParameter.Optional });

            //Mobile Quick Pay API Routes
            config.Routes.MapHttpRoute("ordering mobile web api QuickPay MobilePin",
                "ordering/V5/{memberId}/QuickPay/MobilePin",
                new { controller = "MobileQuickPay" });
                
            config.Routes.MapHttpRoute("ordering mobile web api QuickPay GetBindedCards",
               "ordering/V5/{memberId}/QuickPay/GetBindedCards",
               new { controller = "MobileQuickPay" });

            config.Services.Replace(typeof(IHttpControllerSelector), new CustomHeaderControllerSelector(config));

        }
    }
}