using System;
using System.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class PaymentGatewayResponseGenerator : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request.Params["Agency"] == "WebPay")
                {
                    if (Request.Params["Result"] == "App")
                    {
                        Response.Write("ACEPTADO");
                    }
                    else
                    {
                        Response.Write("RECHAZADO");
                    }
                }
                Response.End();
            }
        }
    }
}