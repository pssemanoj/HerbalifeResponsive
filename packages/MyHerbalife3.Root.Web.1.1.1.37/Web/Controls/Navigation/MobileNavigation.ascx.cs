using HL.Common.Configuration;
using MyHerbalife3.Core.ContentProxyProvider;
using MyHerbalife3.Core.ExperienceProvider.Interfaces;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace HL.MyHerbalife.Web.Controls.Navigation
{
    public class MobileNavigation : UserControl
    {
        protected readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);

        public NavigationItemModel MainNavigationData { get; protected set; }

        internal IAsyncLoader<NavigationItemModel, GetNavigationByName> _NavigationLoader;


        public IExperience currentExperience
        {
            get { return _GlobalContext.CurrentExperience; }
        }

        public bool IsMicroServiceEnabled
        {
            get { return Settings.GetRequiredAppSetting("MyHLNavigationEnabled", false); }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsSearchBoxEnabled
        {
            get { return _GlobalContext.CultureConfiguration.SearchEnabled; }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsOrderingEnabled
        {
            get { return _GlobalContext.CultureConfiguration.IsOrderingEnabled; }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsDsAlertsEnabled
        {
            get
            {
                return _GlobalContext.CultureConfiguration.IsDSAlertsEnabled && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, RoleDefinitions.ROLE_DELETED_ID);
            }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsOnlineProfileEnabled
        {
            get { return _GlobalContext.CultureConfiguration.IsOnlineProfileEnabled; }
        }

        private ILocalizationManager _LocalizationManager;

        protected void Page_Init()
        {
            _NavigationLoader = new ContentProxyProvider();
            _LocalizationManager = new LocalizationManager();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (currentExperience.ExperienceType != MyHerbalife3.Shared.ViewModel.ValueObjects.ExperienceType.Black)
            {
                SetMainNavigation();
            }
        }

        public async Task SetMainNavigation()
        {
            var query = new GetNavigationByName
            {
                Name = Constants.Navigation.MainNavigationName,
                Locale = CultureInfo.CurrentUICulture.Name,
                NavigationOrigin = NavigationOrigin.Default
            };

            MainNavigationData = await GetNavigationModel(query).ConfigureAwait(false);
        }

        internal async Task<NavigationItemModel> GetNavigationModel(GetNavigationByName query)
        {
            NavigationItemModel data = null;
            data = await _NavigationLoader.Load(query).ConfigureAwait(false);

            return data;
        }

        protected string SearchText
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "SearchText"); }
        }

        protected string MiniProfile_MyAccount
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "MiniProfile_MyAccount"); }
        }

        protected string MiniProfile_Settings
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "MiniProfile_Settings"); }
        }

        protected string MiniProfile_LoginPreferences
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "MiniProfile_LoginPreferences"); }
        }

        protected string MiniProfile_LogOut
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "MiniProfile_LogOut"); }
        }

        protected string MiniProfile_SettingsLink
        {
            get
            {
                return IsDsAlertsEnabled
                        ? "/Account/Communication/NotificationsSubscriptions.aspx"
                        : "/Account/Communication/Paperless.aspx";
            }
        }
    }
}