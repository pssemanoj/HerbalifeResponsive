using MyHerbalife3.Account.AlertsProvider.Providers;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Globalization;
using System.Web;
using System.Web.Security;

namespace HL.MyHerbalife.Web.Controls.Logon
{
    public partial class LogonStatusView : UserControlBase
    {
        #region Protected fields

        private readonly ILocalizationManager _localizationManager = new LocalizationManager(new ResourcePathResolver(), new ResourceProviderFactoryResolver());
        protected string IsDsAlertsEnabled;

        #endregion

        protected string salutation = String.Empty;

        private DistributorProfileModel DistributorProfileModel
        {
            get
            {
                var membershipUser = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
                return membershipUser != null ? membershipUser.Value : null;
            }
        }

        // writing salutation into html span for js consumption
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlAlertIcon.Visible = pnlAlertIcon.Enabled = GlobalContext.CultureConfiguration.IsDSAlertsEnabled && !Roles.IsUserInRole(RoleDefinitions.ROLE_DELETED_ID);
            if (GlobalContext.IsLoggedIn && null != DistributorProfileModel)
            {
                _FullName.Text = DistributorProfileModel.DistributorName();
                _Salutation.Text = _localizationManager.GetGlobalString("HrblUI", DistributorProfileModel.TeamLevelNameResourceKey);
            }
            _LogonStatus.Text = _localizationManager.GetString("~/Controls/Logon/LogonStatusView.ascx", "_LogonStatus.LogoutText",
                                                               CultureInfo.CurrentUICulture);
            BizWorksLogo.Visible = !Roles.IsUserInRole(RoleDefinitions.ROLE_BIZWORKS_NOT_SUBSCRIBED);

            lblvolumeNotAvailable.Text = _localizationManager.GetGlobalString("HrblUI", "volumeNotAvailable");
        }

        protected string GetNumberDsAlerts()
        {
            var context = HttpContext.Current.ApplicationInstance as IGlobalContext;
            return DsAlertProvider.GetNumberOfNewAlerts(
                "",
                GlobalContext.CurrentDistributor.Id,
                true,
                CultureInfo.CurrentUICulture.Name,
                DistributorProfileModel.ProcessingCountryCode, 
                context.CultureConfiguration);
        }
    }
}