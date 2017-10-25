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
    public class FullHeader : UserControl
    {
        protected readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);

        internal IAsyncLoader<NavigationItemModel, GetNavigationByName> _NavigationLoader;


        public IExperience currentExperience
        {
            get { return _GlobalContext.CurrentExperience; }
        }

        public bool IsMicroServiceEnabled
        {
            get { return Settings.GetRequiredAppSetting("MyHLNavigationEnabled", false); }
        } 

        private NavigationItemModel mainNavigationData;

        public NavigationItemModel MainNavigationData
        {
            get
            {
                if (mainNavigationData == null)
                {
                    SetMainNavigation();
                }

                return mainNavigationData;
            }
        }
        private NavigationItemModel localeSelectionData;
        public NavigationItemModel LocaleSelectionData
        {
            get
            {
                if (localeSelectionData == null)
                {
                    SetLocaleSelectorData();
                }

                return localeSelectionData;
            }
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
            var taskLocale = SetLocaleSelectorData();
            var taskNavigation = SetMainNavigation();

            Task.WaitAll(new Task[] { taskLocale, taskNavigation });
        }

        public async Task SetLocaleSelectorData()
        {
            var query = new GetNavigationByName { Name = Constants.Navigation.LocaleSelectorName, Locale = CultureInfo.CurrentUICulture.Name };

            localeSelectionData = await GetNavigationModel(query).ConfigureAwait(false);
        }

        public async Task SetMainNavigation()
        {
            var query = new GetNavigationByName
            {
                Name = Constants.Navigation.MainNavigationName,
                Locale = CultureInfo.CurrentUICulture.Name,
                NavigationOrigin = NavigationOrigin.Default
            };

            mainNavigationData = await GetNavigationModel(query).ConfigureAwait(false);
        }

        internal async Task<NavigationItemModel> GetNavigationModel(GetNavigationByName query)
        {
            NavigationItemModel data = null;
            data = await _NavigationLoader.Load(query).ConfigureAwait(false);

            return data;
        }

        protected string ShoppingCart_Items
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "ShoppingCart_Items"); }
        }

        protected string SearchText
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "SearchText"); }
        }

        protected string ShoppingCart_Subtotal
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "ShoppingCart_Subtotal"); }
        }

        protected string ShoppingCart_VolumePoints
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "ShoppingCart_VolumePoints"); }
        }

        protected string ShoppingCart_Checkout
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "ShoppingCart_Checkout"); }
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
        protected string ShoppingCart_ViewCarts
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "ShoppingCart_ViewCarts"); }
        }

        protected string VolumeHeaderDualMonth
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "VolumeHeaderDualMonth"); }
        }

        protected string VolumeNotAvailable
        {
            get { return _LocalizationManager.GetGlobalString("HrblUI", "volumeNotAvailable"); }
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