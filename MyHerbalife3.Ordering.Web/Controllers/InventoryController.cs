#region

using System.Web.Mvc;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.LegacyProviders.ValueObjects;
using MyHerbalife3.Shared.Analytics.Mvc;

#endregion

namespace MyHerbalife3.Ordering.Web
{
    [AmbientNavigation]
    [CultureSwitching]
    [AmbientAnalyticsFact]
    [Authorize]
    public class InventoryController : AsyncController
    {
        
        public ActionResult Index()
        {
            return View("InventoryIndex");
        }

        public ActionResult HtmlFragment(string path)
        {
            var result = new ContentResult {ContentType = "text/html"};

            var reader = new ContentReader {Enabled = true, Visible = true, ContentPath = path, UseLocal = true};
            reader.LoadContent();
            result.Content = reader.HtmlContent;

            return result;
        }
        
    }
}