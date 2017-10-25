using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System;
using System.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class TellYourStory : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.DisplayTellYourStory)
            {
                TellYourStoryBox.Visible = true;
            }
        }
    }
}