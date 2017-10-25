using System;
using System.Web.UI;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Linq;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class APFDueReminderPopUp : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string level = APFDueProvider.GetDSLevel();

                DateTime dt = APFDueProvider.GetAPFDueDate(DistributorID, Locale.Substring(3));
                if (dt < DateTime.Now)
                {
                    btnRemind.Visible = false;
                    lblRemindltr.Text = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "APFFeeDuePastPopup"), dt.ToShortDateString());
                }
                else if (dt > DateTime.Now)
                {
                    lblRemindltr.Text = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "APFFeeDueFuturePopup"), dt.ToShortDateString());
                }
                else if (dt.Date == DateTime.Now.Date)
                {
                    lblRemindltr.Text = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "APFFeeDueTodayPopup"), dt.ToShortDateString());
                }
            }
        }
        protected void HidePopUp(object sender, EventArgs e)
        {
            APFDueReminderPopupExtender.Hide();
            Session["showedAPFPopup"] = true;
        }
        protected void Navigatetocheckout(object sender, EventArgs e)
        {
            Session["showedAPFPopup"] = true;
            var apfSku = new List<ShoppingCartItem_V01>();
            string level = APFDueProvider.GetDSLevel();
            if (level != null && level == "DS")
            {
                apfSku.Add(new ShoppingCartItem_V01(0, HLConfigManager.Configurations.APFConfiguration.DistributorSku, 1, DateTime.Now));
            }
            else
            {
                apfSku.Add(new ShoppingCartItem_V01(0, HLConfigManager.Configurations.APFConfiguration.SupervisorSku, 1, DateTime.Now));
            }
            if (ShoppingCart.CartItems.Count > 0)
            {
                var itemsToRemove = from item in ShoppingCart.CartItems
                                    select item;
                if (itemsToRemove.Any())
                {
                    var removeSkus = itemsToRemove.Select(s => s.SKU).ToList();
                    ShoppingCart.DeleteItemsFromCart(removeSkus, true);
                }
            }

            if (SessionInfo != null)
                SessionInfo.IsAPFOrderFromPopUp = true;
            if (ShoppingCart.CartItems.Find(s => s.SKU.Contains(apfSku[0].SKU)) == null)
            {
                ShoppingCart.AddItemsToCart(apfSku, true);
            }
            Response.Redirect("~/ordering/ShoppingCart.aspx");
        }
        public void ShowPopUp()
        {
            APFDueReminderPopupExtender.Show();
        }
    }
}