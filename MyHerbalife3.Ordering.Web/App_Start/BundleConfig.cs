#region

using System.Web.Optimization;
using MyHerbalife3.Shared.UI.Helpers;

#endregion

namespace MyHerbalife3.Ordering
{
    public class BundleConfig
    {
        private const string VerticalRoutingPrefix = "ordering";

        public static void RegisterBundles(BundleCollection bundles)
        {
            // registers the core scripts that all verticals use
            bundles.WithPrefix(VerticalRoutingPrefix).RegisterCoreScripts();

            // register the scripts this vertical uses
            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new ScriptBundle(BundleHelper.VerticalBundleName.ToBundlePath())
                    .Include(
                        "~/SharedUI/Scripts/thickbox.js",
                        "~/Ordering/Scripts/ProductDetailControl.js",
                        "~/Ordering/Scripts/ProductBySKU.js",
                        "~/Ordering/Scripts/jquery.ajaxPost.js",
                        "~/SharedUI/Scripts/jquery.cookie.js",
                        "~/Ordering/Scripts/ordering.js",
                        "~/Ordering/Scripts/FileSaver.js",
                        "~/Ordering/Scripts/moment.js",
                        "~/Ordering/Scripts/invoiceSearch.js",
                        "~/Ordering/Scripts/invoiceEdit.js",
                        "~/Ordering/Scripts/invoiceDisplay.js",
                        "~/Ordering/Scripts/InventoryDetails.js"
                    ));

            // register the scripts this vertical uses
            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new ScriptBundle(BundleHelper.VerticalLegacyBundleName.ToBundlePath())
                    .Include("~/SharedUI/Scripts/thickbox.js",
                        "~/Ordering/Scripts/ProductDetailControl.js",
                        "~/Ordering/Scripts/ProductBySKU.js",
                        "~/Ordering/Scripts/jquery.ajaxPost.js",
                        "~/SharedUI/Scripts/jquery.cookie.js",
                        "~/Ordering/Scripts/myhl2-ordering.js",
                        "~/Ordering/Scripts/FileSaver.js",
                        "~/Ordering/Scripts/moment.js",
                        "~/Ordering/Scripts/invoiceSearch.js",
                        "~/Ordering/Scripts/invoiceEdit.js",
                        "~/Ordering/Scripts/invoiceDisplay.js",
                        "~/Ordering/Scripts/InventoryDetails.js"));

            /* CSS bundling */


            bundles.WithPrefix(VerticalRoutingPrefix).RegisterCoreStyles();            

            /* Any extra styles for this vertical can be added here*/
            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new StyleBundle(BundleHelper.CommonStylesBundleName1.ToBundlePath())
                    .Include(
                        "~/Ordering/CSS/main-" + VerticalRoutingPrefix + ".css"
                    ));
            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new StyleBundle(BundleHelper.CommonStylesBundleNameIE1.ToBundlePath())
                    .Include(
                        "~/Ordering/CSS/main-" + VerticalRoutingPrefix + "_1.css"
                    ));

            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new StyleBundle(BundleHelper.CommonStylesBundleNameIE2.ToBundlePath())
                    .Include(
                        "~/Ordering/CSS/main-" + VerticalRoutingPrefix + "_2.css"
                    ));

            bundles.WithPrefix(VerticalRoutingPrefix).Add(
                new StyleBundle(BundleHelper.CommonStylesBundleNameIE3.ToBundlePath())
                    .Include(
                        "~/Ordering/CSS/main-" + VerticalRoutingPrefix + "_3.css"
                    ));
        }
    }
}