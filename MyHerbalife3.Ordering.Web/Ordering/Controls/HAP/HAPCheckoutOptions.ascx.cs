using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.HAP
{
    public partial class HAPCheckoutOptions : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                {
                    lblOrderType.Text = getHAPOrderType();
                    lblScheduleDate.Text = getHAPScheduleDate();
                    lblExpDate.Text = getExpDate();
                    string rawURL = HttpContext.Current.Request.RawUrl;
                    lbOrderTotalsDisclaimer.Visible = rawURL.Contains("Checkout.aspx") || rawURL.Contains("Confirm.aspx");
                }
            }
        }

        private string getHAPOrderType()
        {
            string hapOrderType = string.Empty;
            switch (ShoppingCart.HAPType)
            {
                case "01": hapOrderType = GetLocalResourceObject("PersonalResource1.Text").ToString(); break;
                case "02": hapOrderType = GetLocalResourceObject("ResaleResource1.Text").ToString(); break;
            }

            return hapOrderType;
        }

        private string getHAPScheduleDate()
        {
            string hapScheduleDate = string.Empty;

            hapScheduleDate = ShoppingCart.HAPScheduleDay > 0 ? ShoppingCart.HAPScheduleDay.ToString() + "th" : string.Empty;

            return hapScheduleDate;
        }

        private string getExpDate()
        {
            ShoppingCart.HAPExpiryDate = DateTime.Today.AddYears(1);

            DateTime? hapExpirationDate = (Page as ProductsBase).getHapExpirationDate();
            if (hapExpirationDate != null && hapExpirationDate > new DateTime())
            {
                ShoppingCart.HAPExpiryDate = ((DateTime)hapExpirationDate);
            }

            return ShoppingCart.HAPExpiryDate.ToString("MM/dd/yyyy");
        }
    }
}