using HL.Common.Configuration;
using HL.MyHerbalife.Web.Controls.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MyHerbalife3.Shared.Infrastructure.Helpers;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.UI.Extensions;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Shared.ViewModel;
using Telerik.Web.UI;
using System.IO;
using System.Web.Hosting;
using MyHerbalife3.Shared.ViewModel.ValueObjects;
using MyHerbalife3.Shared.Analytics.Providers;
using MyHerbalife3.Core.ExperienceProvider.Interfaces;

namespace HL.MyHerbalife.Web
{
    public partial class Site : MasterPage
    {
        public IExperience currentExperience;

        public const string EmptySiteMapProviderName = "EmptySiteMap";        

        protected int randomNumber = new Random().Next(0, 1000000);

        protected readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);

        private List<SiteMapNode> nodes;

        public string vertical = Settings.GetRequiredAppSetting("Spa.Vertical", "Root");

        public bool summerPromoBannerEnabled = Settings.GetRequiredAppSetting("SummerPromo.BannerEnabled", false);

        public bool oldKendo = Settings.GetRequiredAppSetting("OldKendoEnable", false);

        public bool oldCacheBusting = Settings.GetRequiredAppSetting("oldCacheBusting", false);

        public string locale
        {
            get { return _GlobalContext.CultureConfiguration.Locale; }
        }

        [Description("Gets the reference to the menu control on the page.")]
        [Browsable(true)]
        [Category("Appearence")]
        public TabbedNav MainMenu
        {
            get { return _MainMenu; }
        }

        [Description("Gets the reference to the left nav menu control on the page.")]
        [Browsable(true)]
        [Category("Appearence")]
        public LeftNavMenu LeftNavMenu
        {
            get { return _LeftNavMenu; }
        }

        // protected HtmlTableCell LeftNavMenuCell;
        [Description("Gets or sets the Visible property of the LeftnavMenu panel.")]
        [Browsable(true)]
        [Category("Appearence")]
        public bool IsleftNavMenuVisible
        {
            get { return _LeftNavMenuPanel.Visible; }
            set
            {
                LeftNavMenu.Visible = value;
                _LeftNavMenuPanel.Visible = value;
            }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsSearchBoxEnabled
        {
            //TODO: Base on locale?
            get { return _GlobalContext.CultureConfiguration.SearchEnabled; }
            set { ; }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsSubHeaderDisabled
        {
            get; set;
        }

        [Browsable(true)]
        [Category("Appearence")]
        public bool IsMyHerbalife3Enabled
        {
            get { return _GlobalContext.CultureConfiguration.DefaultExperienceType == ExperienceType.Green; }
            set { ; }
        }

        [Browsable(true)]
        [Category("Appearence")]
        public string LogoUrl
        {
            get { return _GlobalContext.CultureConfiguration.HeaderLogoUrl; }
            set { ; }
        }

        public HtmlGenericControl GetBody
        {
            get { return body; }
        }

        public HtmlTitle _Title
        {
            get { return Title; }
            set { Title = value; }
        }

        /// <summary>
        ///     When set to true, the page header automatically is set to the text of current site map's top node. IF page header needs to be
        ///     different than the site map node, set this property to false.
        /// </summary>
        public bool SetPageHeaderBySiteMap { get; set; }

        public RadScriptManager RadScriptManager
        {
            get { return _RadScriptManager; }
        }        

        protected void Page_Init(object sender, EventArgs e)
        {
            if ((!Settings.GetRequiredAppSetting("IsAntiXsrfOn", true))) return;
            var xsrfvalidation = new XsrfValidationHelper(ViewState) { pageCrossBack = Page };
            xsrfvalidation.ValidateCookie();
        }       

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            SetPageHeaderBySiteMap = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Roles.IsUserInRole(RoleDefinitions.ROLE_WELCOME_ADVISORY_PENDING))
            {
                Response.Redirect("~/Advisory.aspx");
            }

            //Page.Header.DataBind();
            if (pn_deletedId != null && restrictedmsg != null)
            {
                if (Roles.IsUserInRole(RoleDefinitions.ROLE_DELETED_ID))
                {
                    pn_deletedId.Visible = false;
                    restrictedmsg.Visible = true;
                }
                else
                {
                    pn_deletedId.Visible = true;
                    restrictedmsg.Visible = false;
                }
            }

            // If the section name has not been set till now
            if (_MainMenu != null && null == _MainMenu.Section)
            {
                _MainMenu.Section = SectionType.Home;
            }

            //Add Omniture Script
            AnalyticsProvider.RegisterAspxAnalyticsScript(Page, _GlobalContext);

            // Add Local Timezone fetch Script
            LocalizationHelper.RegisterClientTimeZoneScript(Page);

            // Html numbered/not numbered fragments feature.
            if (!Settings.GetRequiredAppSetting("HL.Blocks.Localization.AddCommentsToValue", false))
            {
                var css = new HtmlGenericControl
                    {
                        TagName = "style",
                        InnerHtml = ".translation-number { display: none } "
                    };
                css.Attributes.Add("type", "text/css");
                Page.Header.Controls.Add(css);
            }

            //Adding Right to Left functionality
            if (_GlobalContext.CultureConfiguration.IsRightToLeft)
            {
                body.Attributes.Add("dir", "rtl");
            }

            //getting vertical name for bundles
            vertical = (vertical != "Root" ? "/" + vertical : "");
            currentExperience = _GlobalContext.CurrentExperience;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            var titleSb = new StringBuilder(80);

            GetCurrentSiteMapNodes().ForEach(n => titleSb.AppendFormat("{0}: ", n.ToString()));
            titleSb.Append(FormsAuthentication.CookieDomain);
            Title.Text = titleSb.ToString();

            //setting page header by site map
            if (SetPageHeaderBySiteMap && PageHeaderArea != null)
            {
                var pageHeader =
                    PageHeaderArea.EnumerateChildControls(c => c.ID == "_PageHeader").FirstOrDefault() as ITextControl;
                string pageHeaderText = GetCurrentTopNodeText();

                //when pageHeaderText is null or empty, means most probably the page is not in sitemap
                //so we ignore it.
                if (pageHeader != null && !string.IsNullOrEmpty(pageHeaderText))
                {
                    pageHeader.Text = pageHeaderText;
                }
            }

            //base.OnPreRender(e);
        }

        /// <summary>
        ///     Returns current site map nodes in bottom-up order. Top node is last in the list.
        /// </summary>
        public List<SiteMapNode> GetCurrentSiteMapNodes()
        {
            if (nodes != null)
            {
                return nodes;
            }
            nodes = new List<SiteMapNode>();

            if (_MainMenu != null 
                && _MainMenu.Provider != null 
                && _MainMenu.Provider.CurrentNode != null 
                && !_MainMenu.Provider.Name.Equals(EmptySiteMapProviderName))
            {
                var siteMapNode = _MainMenu.Provider.CurrentNode;
                // if current node is the leaf node, igonre parent's title
                var siteMapNodeShouldIgnore = siteMapNode == null || siteMapNode.HasChildNodes
                                                  ? null
                                                  : siteMapNode.ParentNode;
                if (siteMapNodeShouldIgnore != null &&
                    siteMapNodeShouldIgnore.ParentNode == _MainMenu.Provider.RootNode)
                    siteMapNodeShouldIgnore = null;

                //StringBuilder titleSB = new StringBuilder(80);
                while (siteMapNode != null && siteMapNode != _MainMenu.Provider.RootNode)
                {
                    if (siteMapNodeShouldIgnore != siteMapNode)
                    {
                        string nodeName = siteMapNode.ToString();
                        if (!string.IsNullOrEmpty(nodeName))
                        {
                            nodes.Add(siteMapNode);
                        }
                    }
                    siteMapNode = siteMapNode.ParentNode;
                }
            }

            return nodes;
        }

        /// <summary>
        ///     Returns text of the top node of the current site map hierarchy. It is used by default for Page Header.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentTopNodeText()
        {
            var topNode = GetCurrentSiteMapNodes().LastOrDefault();

            if (topNode != null)
            {
                return topNode.ToString();
            }

            return null;
        }

        public string GetFiles(string path, string prefix, string type)
        {
            var reader = new List<string>();
            path = Server.MapPath(path);
            var result = "";

            if (File.Exists(path))
            {
                reader.AddRange(File.ReadLines(path)
                    .Select(line => line.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                var arr = reader.Select(fileName => prefix + fileName).ToArray();
                foreach (var item in arr)
                {
                    if (type == "css") result += "<link rel=\"stylesheet\" type =\"text/css\" href =\"" + item + "\" />";
                    else result += "<script type =\"text/javascript\" src=\"" + item + "\"></script>";
                }
            }

            return (new HtmlString(result)).ToString();
        }

        public long GetBundleVersion(string path)
        {
            return File.GetLastWriteTime(HostingEnvironment.MapPath(path)).Ticks;
        }

    }
}