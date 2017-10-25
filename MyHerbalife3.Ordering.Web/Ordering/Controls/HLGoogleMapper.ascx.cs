using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Xml;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.Map;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{

    public partial class HLGoogleMapper : UserControlBase
    {
        private HLAbstractMapper obj = null;

        public void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasBaiduMap)
            {
                obj = new HLBaiduMap();
            }
            else if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasGoogleMap)
            {
                obj = new HLGoogleMap();
            }

        }

        public void DispalyAddressOnMap(List<Address_V01> listaddress)
        {
            string mapScript = "";

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasBaiduMap)
            {
                var pageBase = this.Page as ProductsBase;

                if (pageBase != null)
                    mapScript = pageBase.GetShippingProvider().GetMapScript(Providers.Shipping.ShippingMapType.Baidu);
            }

            string script = mapScript + ";" + obj.ShowMap(listaddress);
            script += "jQuery(\"#l-map\").css(\"height\", \"300px\");";
            ScriptManager.RegisterStartupScript(this, typeof (Page), "DisplayMap", script, true);
        }

        public void DispalyAddressOnMap(List<DeliveryOption> locations)
        {
            string mapScript = "";

            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasBaiduMap)
            {
                var pageBase = this.Page as ProductsBase;

                if (pageBase != null)
                    mapScript = pageBase.GetShippingProvider().GetMapScript(Providers.Shipping.ShippingMapType.Baidu);
            }

            string script = mapScript + ";" + obj.ShowMap(locations);
            ScriptManager.RegisterStartupScript(this, typeof(Page), "DisplayMap", script, true);
        }
    }
}