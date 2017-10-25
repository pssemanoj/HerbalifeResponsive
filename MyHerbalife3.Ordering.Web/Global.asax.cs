using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.App_Start;
using MyHerbalife3.Ordering.Widgets;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.Extensions;

namespace MyHerbalife3.Ordering.Web
{
    public class Global : MyHerbalife3HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            //// Not using MVC Areas  right now.
            // AreaRegistration.RegisterAllAreas();
            
            // standard MVC explicit initialization
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // Base behavior from shared MyHerbalife3HttpApplication
            OnApplicationStart(sender, e);
            ControllerBuilder.Current.DefaultNamespaces.Add("MyHerbalife3.Ordering.Controllers");

            // Ensure Ordering specific API are using real implementation (not stand-ins)
            TopSellerProductsController.Inject(new TopSellerSource());
            RecentOrdersController.Inject(new RecentOrdersSource());
            CartWidgetController.Inject(new CartWidgetSource());
            VolumeController.Inject(new VolumeSource());
            ContactsController.Inject(new ContactsSourceStandIn());
            MyOrdersController.Inject(new MyOrdersSource());
        }


        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected new void Application_BeginRequest(object sender, EventArgs e)
        {
            base.Application_BeginRequest(sender,e);

            //if (Settings.GetRequiredAppSetting("IsChina", false))
            //{
            //    var locale = "zh-CN";

            //    if ((!CultureInfo.CurrentCulture.Name.Equals(locale)
            //        || !CultureInfo.CurrentUICulture.Name.Equals(locale))
            //    )
            //    {
            //        Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            //        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            //    }
            //    URLPrefix = new LocaleProvider().GetPrefix(CultureInfo.CurrentCulture.Name);
            //}
            //else
            SetCulture();
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var reqUrl = System.Web.HttpContext.Current.Request.Url;
            var reqSegment = reqUrl.Segments;
            var requestPage = reqSegment[reqSegment.Length - 1].ToLower();


            if ((requestPage == "orderpaymentresult.aspx") || (requestPage == "orderpaymentstatus.aspx"))
            {
                
                var configEntries = Settings.GetRequiredAppSetting("99BillUrlReferrer", string.Empty);

                if (!string.IsNullOrEmpty(configEntries))
                {
                    var referrerUrl = System.Web.HttpContext.Current.Request.UrlReferrer;
                    if (referrerUrl != null &&
                        GetConfigEntries(configEntries, referrerUrl.AbsoluteUri.ToLower()))
                    {
                        System.Web.HttpContext.Current.SkipAuthorization = true;
                    }
                }
                else
                {
                    System.Web.HttpContext.Current.SkipAuthorization = true;
                }
            }
        }

        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            OnApplicationError(sender, e);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            OnApplicationEndRequest(sender, e);
        }

        protected void Application_PostAuthorizeRequest()
        {
            PostAuthorizeRequest("/Ordering/api/");
        }

        public bool GetConfigEntries(string configEntries, string entryName)
        {
            if (!string.IsNullOrEmpty(configEntries))
            {
                var list = configEntries.Split(';');
                if (list != null && list.Any())
                {
                    if (list.Contains(entryName))
                    {
                        return true;
                    }
                }
                
            }
            return false;
        }
    }
}