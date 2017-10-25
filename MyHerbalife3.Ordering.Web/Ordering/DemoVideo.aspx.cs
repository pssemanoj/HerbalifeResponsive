using MyHerbalife3.Ordering.Web.MasterPages;
using System;
using System.Globalization;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class DemoVideo : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Title = GetLocalResourceObject("_PageHeader.Title") as string;
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
             MenuFAQ.NavigateUrl = string.Format("/content/{0}/pdf/{1}", CultureInfo.CurrentCulture.Name, "products/GDO_FAQs.pdf"); ;
        }
    }
}