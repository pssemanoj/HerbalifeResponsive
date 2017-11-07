using HL.Common.Configuration;
using MyHerbalife3.Shared.Analytics.Providers;
using MyHerbalife3.Shared.Infrastructure.Helpers;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Web.UI;

namespace HL.MyHerbalife.Web.MasterPages
{
    public partial class Blank : MasterPage
    {
        private readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);

        protected int randomNumber = new Random().Next(0, 1000);

        public string vertical = Settings.GetRequiredAppSetting("Spa.Vertical", "Root");

        public bool oldKendo = Settings.GetRequiredAppSetting("OldKendoEnable", false);

        public bool oldCacheBusting = Settings.GetRequiredAppSetting("oldCacheBusting", false);

        public string locale
        {
            get { return _GlobalContext.CultureConfiguration.Locale; }
        }

        public StatusDisplay Status
        {
            get { return this.StatusDisplay; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if ((!Settings.GetRequiredAppSetting("IsAntiXsrfOn", false))) return;
            var xsrfvalidation = new XsrfValidationHelper(ViewState) { pageCrossBack = Page };
            xsrfvalidation.ValidateCookie();
        }  

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Header.DataBind(); 
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            //Add Omniture Script
            AnalyticsProvider.RegisterAspxAnalyticsScript(Page, _GlobalContext);

            //Adding Right to Left functionality
            if (_GlobalContext.CultureConfiguration.IsRightToLeft)
            {
                body.Attributes.Add("dir", "rtl");
            }

            //getting vertical name for bundles
            vertical = (vertical != "Root" ? "/" + vertical : "");
        }

        public string GetFiles(string path, string prefix, string type)
        {
            var reader = new List<string>();
            path = Server.MapPath(path);
            var result = "";

            if (File.Exists(path))
            {
                reader.AddRange(File.ReadLines(path)
                    .Select(line => line.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                var arr = reader.Select(fileName => prefix + fileName).ToArray();
                foreach (var item in arr)
                {
                    if (type == "css") result += "<link rel=\"stylesheet\" type =\"text/css\" href =\"" + item + "\" />";
                    else result += "<script type =\"text/javascript\" src=\"" + item + "\"></script>";
                }
            }

            return (new HtmlString(result)).ToString();
        }

        public long GetBundleVersion(string path)
        {
            return File.GetLastWriteTime(HostingEnvironment.MapPath(path)).Ticks;
        }

    }
}