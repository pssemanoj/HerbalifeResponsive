using HL.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.Kount
{
    public partial class Logo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //m: Supplied Merchant ID 
            //s: Session ID 
            Response.Redirect(string.Format("{0}/logo.htm?m={1}&s={2}", Settings.GetRequiredAppSetting("DataCollectorURL"), Request["m"], Request["s"]));
        }
    }
}