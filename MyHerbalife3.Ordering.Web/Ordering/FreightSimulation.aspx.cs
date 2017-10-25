using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Web.MasterPages;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class FreightSimulation : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var freightCtrl = LoadControl("~/Ordering/Controls/FreightSimulator.ascx");
            pnlFreightEstimation.Controls.Add(freightCtrl);

            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            }

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-10 gdo-nav-mid-fs");
        }
    }
}