using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public class Invoicebase : ProductsBase
    {
        protected override void OnInit(EventArgs e)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AllowToCreateInvoice)
            {
                Response.Redirect("~/Ordering/Catalog.aspx");
                Response.End();
            }
        }
    }
}