using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class SyndicatedWidget : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (HLConfigManager.Configurations.DOConfiguration.DisplaySyndicatedWidget && ProductsBase.IsEventTicketMode != true)
            {
                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled &&
                    SessionInfo.DsType == ServiceProvider.DistributorSvc.Scheme.Member && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
                {
                    Syndicated.Visible = false;
                }
                else
                {
                    Syndicated.Visible = true;
                }
            }
        }
    }
}