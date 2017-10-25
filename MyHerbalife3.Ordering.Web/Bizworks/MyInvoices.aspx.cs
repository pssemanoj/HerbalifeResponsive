using System;
using MyHerbalife3.Ordering.Web.MasterPages;

namespace MyHerbalife3.Ordering.Web.Ordering
{
	public partial class MyInvoices : Invoicebase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            Response.Redirect("~/Ordering/Invoice");
            Response.End();
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-nav-mid-invoices");
		}
	}
}