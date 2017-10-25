using MyHerbalife3.Core.ContentProxyProvider;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.UI.Helper;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using MyHerbalife3.Shared.ViewModel.ValueObjects;
using System;
using System.ComponentModel;
using System.Web.Security;
using System.Web.UI;

namespace HL.MyHerbalife.Web.Controls.Navigation
{
    public partial class LeftNavMenu : UserControl
    {
        internal IGlobalContext Global;
        internal ILegacyNavigationBuilder LegacyNavigationBuilder;
        internal IAsyncLoader<NavigationItemModel, GetNavigationByName> NavigationLoader;
        public NavigationItemModel MainNavigationData { get; protected set; }

        public NavigationItemModel CurrentNode
        {
            get { return NavigationInfo.CurrentNode; }
        }

        [Description("The section name, lower case, for the skin to be based on")]
        public string SectionName { get; set; }

        protected PageNavigationInfo NavigationInfo { get; private set; }

        public bool IsMyHerbalife3Enabled { get; private set; }

        protected void Page_Init()
        {
            NavigationLoader = new ContentProxyProvider();
            LegacyNavigationBuilder = new LegacyNavigationBuilder();
            Global = (IGlobalContext)Context.ApplicationInstance;
            IsMyHerbalife3Enabled = Global.CultureConfiguration.DefaultExperienceType != ExperienceType.Brown;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SectionName))
            {
                SectionName = "home";
            }

            GetNavigationInfo();
        }

        private void GetNavigationInfo()
        {
            if (IsMyHerbalife3Enabled)
            {
                NavigationInfo = (new PageNavigationInfo())
                    .SetMainNavigation(NavigationLoader)
                    .EstablishImmediateOwnerNode(Roles.GetRolesForUser(), Request.Url);
            }
            else
            {
                NavigationInfo = (new PageNavigationInfo())
                    .SetMainNavigationFromLegacy(LegacyNavigationBuilder)
                    .EstablishLegacyImmediateOwnerNode(2, Request.Url);
            }
        }
    }
}
