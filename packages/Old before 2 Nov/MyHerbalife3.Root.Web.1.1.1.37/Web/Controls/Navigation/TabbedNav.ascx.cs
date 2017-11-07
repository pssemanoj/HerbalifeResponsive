using MyHerbalife3.Core.ContentProxyProvider;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.SiteMapProvider;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Telerik.Web.UI;
using System.Collections.Generic;
using MyHerbalife3.Shared.ViewModel.ValueObjects;

namespace HL.MyHerbalife.Web.Controls.Navigation
{
    public partial class TabbedNav : UserControlBase
    {
        internal ILegacyNavigationBuilder _LegacyNavigationBuilder;
        internal IAsyncLoader<NavigationItemModel, GetNavigationByName> _NavigationLoader;
        public NavigationItemModel MainNavigationData { get; protected set; }

        protected string[] PcPsrHiddenSections
        {
            get
            {
                return new string[] { "HerbalifeNews", "HerbalifeCalendar", "ConferencePromotion", "Storiesandcities", "MYPCPSR" };
            }
        }

        protected bool IsPcPsr 
        { 
            get
            {
                var pcPsr = new List<string> { "PC", "PSR" };
                return pcPsr.Any(x => x.Equals(GlobalContext.CurrentDistributor.CustomerCategoryType, StringComparison.OrdinalIgnoreCase));
            }
        }

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            _NavigationLoader = new ContentProxyProvider();
            _LegacyNavigationBuilder = new LegacyNavigationBuilder();

            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            // Set the page title from the sitemap.
            if (_SiteMapDS.Provider != null && _SiteMapDS.Provider.CurrentNode != null)
            {
                // Filter out any HTML breaks.
                string title = _SiteMapDS.Provider.CurrentNode.Title;
                title = Regex.Replace(title, @"\s*<br\s?/>\s*", " ", RegexOptions.IgnoreCase);
                Page.Title = title;
            }
            base.OnPreRender(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.User.Identity.IsAuthenticated)
            {
                SiteMapHelper.SetMainMenuSitemap(_SiteMapDS);

                if (GlobalContext.CultureConfiguration.DefaultExperienceType == ExperienceType.Green)
                {
                    SetMainNavigation();
                }
                else
                {
                    SetMainNavigationFromLegacy();
                }
            }
        }

        public void SetMainNavigationFromLegacy()
        {
            var data = _LegacyNavigationBuilder.BuildMainMenuNavigationModel(CultureInfo.CurrentCulture.Name);
            MainNavigationData = data;
        }

        public async Task SetMainNavigation()
        {
            var query = new GetNavigationByName
                {
                    Name = Constants.Navigation.MainNavigationName,
                    Locale = CultureInfo.CurrentUICulture.Name,
                    NavigationOrigin = NavigationOrigin.Cms
                };

            NavigationItemModel data = null;
            data = await _NavigationLoader.Load(query);

            MainNavigationData = data;
        }

        #endregion Methods

        #region properties

        /// <summary>
        ///     Gets or sets the current section.
        /// </summary>
        /// <value>The section this tab is to display as.</value>
        public SectionType Section { get; set; }

        /// <summary>
        ///     Gets the provider the menu is using.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Web.SiteMapProvider" /> used by the menu.
        /// </value>
        public SiteMapProvider Provider
        {
            get { return _SiteMapDS.Provider; }
        }

        #endregion
    }
}