using HL.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.Kount
{
    public partial class DataCollector : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                // KOUNT
                string merchantId = Settings.GetRequiredAppSetting("KountMerchantId");
                this.ShoppingCart.FraudControlSessonId = SessionID.Value = (Guid.NewGuid()).ToString().Replace("-", "");
                var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                string logoURL = string.Format("{3}{2}/Ordering/Controls/Payments/Kount/Logo.aspx?m={0}&s={1}", merchantId, SessionID.Value, Request.Url.DnsSafeHost, termUrlPrefix);
                string logoGifURL = string.Format("{3}{2}/Ordering/Controls/Payments/Kount/logo_gif.aspx?m={0}&s={1}", merchantId, SessionID.Value, Request.Url.DnsSafeHost, termUrlPrefix);
                this.logoFrame.Attributes["src"] = logoURL;
                this.logoImage.Attributes["src"] = logoGifURL;
            }
        }
    }
}