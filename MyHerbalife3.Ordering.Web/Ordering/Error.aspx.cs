using System;
using System.Collections.Generic;
using System.Threading;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Shared.UI;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class Error : PageBase
    {
        #region Methods
       
        protected void Page_Load(object sender, EventArgs e)
        {
            var orderingMaster = Master as OrderingMaster;
            if (orderingMaster != null)
            {
                orderingMaster.SetPageHeader(GetLocalResourceString("PageResource1.Title"));
                if(HLConfigManager.Configurations.DOConfiguration.IsChina)
                  HLConfigManager.Configurations.DOConfiguration.ShowBulletinBoard = false;
            }

            var errors = new List<string>();

            _ContentReader.UseLocal = true;
            var allowDo = Settings.GetRequiredAppSetting("allowDO", true);
            //Putting All DO in maintenance
            if (!allowDo)
            {
                _ContentReader.ContentPath = "errorMsg_emergencyMaintenance.html";
            }
            else if (!HLConfigManager.Configurations.DOConfiguration.AllowDO)
            {
                switch (HLConfigManager.Configurations.DOConfiguration.OrderingUnavailableReason)
                {
                    case OrderingUnavailableReason.CountryNotSupported:
                        _ContentReader.ContentPath = "errorMsg_countryNotSupported.html";
                        break;

                    case OrderingUnavailableReason.EmergencyMaintenance:
                        _ContentReader.ContentPath = "errorMsg_emergencyMaintenance.html";
                        break;

                    case OrderingUnavailableReason.ScheduledDowntime:
                        _ContentReader.ContentPath = "errorMsg_scheduledDownTime.html";
                        break;
                }
            }

            if (HLConfigManager.DefaultConfiguration.DOConfiguration.InMaintenance
                || HLConfigManager.Configurations.DOConfiguration.InMaintenance)
            {
                _ContentReader.ContentPath = "errorMsg_emergencyMaintenance.html";
            }

            if (!string.IsNullOrEmpty(_ContentReader.ContentPath) && _ContentReader.ExistsLocalContentPath())
            {
                _ContentReader.Visible = true;
            }
            else
            {
                errors.Add(
                    string.Format(
                        "{0}: {1}",
                        Thread.CurrentThread.CurrentCulture.Name,
                        PlatformResources.GetGlobalResourceString("ErrorMessage", "DONotAllowed") ?? "Not Permitted"));
            }

            if (Request.QueryString["AllowAll"] != null)
            {
                errors.Add(
                    PlatformResources.GetGlobalResourceString("ErrorMessage", "DONotAvailable") ?? "Not Available");
            }

            uxErrores.DataSource = errors;
            uxErrores.DataBind();
        }

        #endregion
    }
}