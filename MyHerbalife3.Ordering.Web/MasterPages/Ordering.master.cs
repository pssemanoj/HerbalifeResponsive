using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using HL.Common.Utilities;
using HL.MyHerbalife.Web.Controls.Navigation;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.Ordering;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.MasterPages
{
    using HL.Common.Logging;
    using System.Threading;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

    public partial class OrderingMaster : System.Web.UI.MasterPage
    {
        internal IGlobalContext Global;
        public MyHLEventBus<MyHLEventTypes> EventBus = new MyHLEventBus<MyHLEventTypes>();
        public const string SessionMessageKey = "SessionMessageKey";
        public const string SessionRedirectKey = "SessionRedirectKey";

        protected override void OnInit(EventArgs e)
        {
            EventBus.RegisterObject(this);
        }

        [Description("Gets the reference to the left nav menu control on the page.")]
        [Browsable(true)]
        [Category("Appearence")]
        public HtmlGenericControl OrderingLeftNavMenu
        {
            get
            {
                if (null != divleft)
                {
                    return divleft;
                }
                return null;
            }
        }

        [Description("Gets or sets the Visible property of the LeftnavMenu panel.")]
        [Browsable(true)]
        [Category("Appearence")]
        public bool IsleftOrderingMenuVisible
        {
            get { return OrderingLeftNavMenu.Visible; }
            set
            {
                if (OrderingLeftNavMenu != null)
                {
                    OrderingLeftNavMenu.Visible = value;
                }
            }
        }

        [Description("Gets the reference to the left nav menu control on the page.")]
        [Browsable(true)]
        [Category("Appearence")]
        public LeftNavMenu LeftNavMenu2
        {
            get { return _LeftNavMenu2; }
        }


        // protected HtmlTableCell LeftNavMenuCell;
        [Description("Gets or sets the Visible property of the LeftnavMenu panel.")]
        [Browsable(true)]
        [Category("Appearence")]
        public bool IsleftMenuVisible
        {
            get { return _LeftNavMenuPanel2.Visible; }
            set
            {
                LeftNavMenu2.Visible = value;
                _LeftNavMenuPanel2.Visible = value;
            }
        }

        static Regex MobileCheck = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|ipad|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino/Mobile|Android", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex MobileVersionCheck = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        [Description("Gets the validation if ordering is opened in a mobile devicd.")]
        [Browsable(true)]
        [Category("Appearence")]
        public bool IsMobile()
        {
            if (HttpContext.Current.Request != null && HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null)
            {
                var u = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                if (u.Length < 4)
                    return false;

                if (MobileCheck.IsMatch(u) || MobileVersionCheck.IsMatch(u.Substring(0, 4)))
                    return true;
            }
            return false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                var meta = new HtmlMeta { Name = "apple-itunes-app", Content = "app-id= 967364034" };
                Page.Header.Controls.Add(meta);
            }
            if (pnlAnnouncements != null)
            {
                if (HLConfigManager.Configurations.DOConfiguration.ShowBulletinBoard)
                {
                    pnlAnnouncements.Visible = true;
                    if (Session["showBulletinBoard"] == null)
                    {
                        Session["showBulletinBoard"] = true;
                    }
                    var annoInfo =  Providers.China.CatalogProvider.GetAnnouncementInfo();
                    var str = new StringBuilder();
                    if (annoInfo != null)
                    {
                        var countryCode = "CN";
                        
                        var pageBase = this.Page as ProductsBase;
                        if (pageBase != null) countryCode = pageBase.CountryCode;

                        var localDateTime = DateUtils.GetCurrentLocalTime(countryCode);

                        str.Append("<ul class=\"a\">");
                        str.Append("<ul>");
                        annoInfo.ForEach(a =>
                        {
                            var info = (AnnouncementInfo_V01)a;
                            if (info == null) return;
                            if (info.IsUseNew == 0 && info.NewBeginDate <= localDateTime && info.NewEndDate >= localDateTime)
                            {
                                str.Append(string.Format(
                                    " <li> {0} <img id=\'newImg\' src=\'/Ordering/Images/China/new_head.gif\' /> </li> ",
                                    info.AnnouncementDesc));
                            }
                            else
                            {
                                str.Append(string.Format("<li>{0}</li>", info.AnnouncementDesc));
                            }
                        });
                        str.Append("</ul>");
                       
                    }
                    litAnnouncementsInfo.Text = str.ToString();
                }
                else
                {
                    Session["showBulletinBoard"] = false;
                    pnlAnnouncements.Visible = false;
                }
            }
            Master.IsleftNavMenuVisible = false;
            Master.SetPageHeaderBySiteMap = false;

            //This needs to be figured out. ViewState is getting lost between different pages.
            if (!string.IsNullOrEmpty(Message))
            {
                ShowMessage("Error", Message);
                Message = string.Empty;
            }
            //Substitute method - use Session
            var message = Session[SessionMessageKey] as string;
            if (!string.IsNullOrEmpty(message))
            {
                //ShowMessage(string.Empty, message);
                //StatusDisplay.DisplayType = StatusDisplayType.Popup;
                Session[SessionMessageKey] = string.Empty;
                StatusDisplay.AddMessage(StatusMessageType.Error, message);
            }

            if (HLConfigManager.Configurations.DOConfiguration.ShowEOFTimer)
            {
                var now = DateUtils.GetCurrentLocalTime(Thread.CurrentThread.CurrentCulture.ToString().Substring(3));
                //var servDate = DateTime.Now;
                var eomDate = ShoppingCartProvider.GetEOMDate(now, CultureInfo.CurrentCulture.Name.Substring(3, 2));
                var beginDate = eomDate.AddDays(-HLConfigManager.Configurations.DOConfiguration.EOMCounterDisplayDays);
                if (now >= beginDate)
                {
                    hTargetDate.Value = eomDate.ToString("yyyy MM dd HH mm ss", CultureInfo.InvariantCulture);
                    //hcurrentDate.Value = servDate.ToString("yyyy MM dd HH mm ss", CultureInfo.InvariantCulture);
                    hcurrentDate.Value = now.ToString("yyyy MM dd HH mm ss", CultureInfo.InvariantCulture);
                    counter.Visible = true;
                }
            }

            MembershipUser<DistributorProfileModel> member = null;
            try
            {
                var memberBase = Membership.GetUser();
                member = (MembershipUser<DistributorProfileModel>)memberBase;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                            string.Format("Order.Master.cs Null member cast failed : {0} ", ex.Message));
            }

            if (member != null && member.Value != null)
            {
                var user = member.Value;
                SessionInfo sessionInfo = SessionInfo.GetSessionInfo(user.Id, CultureInfo.CurrentCulture.Name);
                var page = Page as ProductsBase;

                // Check if the page is ordering base to load the control
                if (!String.IsNullOrEmpty(sessionInfo.CustomerOrderNumber) && page != null)
                {
                    if (trCustomerOrder != null)
                    {
                        trCustomerOrder.Visible = true;
                    }
                    else
                    {
                        divCustomerOrder.Visible = true;
                    }
                    SetPageHeader(GetLocalResourceObject("CustomerOrderingHeader").ToString());
                    var customerOrderinfo =
                        LoadControl("~/Ordering/Controls/CustomerOrderInfo.ascx") as CustomerOrderInfo;
                    plCustomerOrder.Controls.Add((Control)customerOrderinfo);
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && IsMobile())
            {                
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), "var responsiveMode = true;", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), "var responsiveMode = false;", true);
            }

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && HttpContext.Current != null && member != null && member.Value != null )
            {
                string rawURL = HttpContext.Current.Request.RawUrl;
                var currentSessionInfo = SessionInfo.GetSessionInfo(member.Value.Id, CultureInfo.CurrentCulture.Name);
                if (currentSessionInfo.IsHAPMode || rawURL.Contains("HAPOrders.aspx"))
                {
                    counter.Visible = false;
                }
            }

        }
        public void setTotalItemsMobile(int TotalItems)
        {
            cartItems.InnerText = TotalItems.ToString();
        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (null != Session[SessionRedirectKey])
            {
                var args = Session[SessionRedirectKey] as PageVisitRefusedEventArgs;
                if (null != args)
                {
                    if (args.Reason == PageVisitRefusedReason.BlockedBySponsor ||
                        args.Reason == PageVisitRefusedReason.CantBuy ||
                        args.Reason == PageVisitRefusedReason.HardCashNoSuitablePaymentMethods ||
                        args.Reason == PageVisitRefusedReason.CartIsEmpty ||
                        args.Reason == PageVisitRefusedReason.InvalidDeliveryInfo ||
                        args.Reason == PageVisitRefusedReason.PurchasingLimitsExceeded ||
                        args.Reason == PageVisitRefusedReason.UnableToPrice)
                    {
                        if (!string.IsNullOrEmpty(args.Message))
                        {
                            StatusDisplay.AddMessage(StatusMessageType.Error, args.Message);
                        }
                        Session[SessionRedirectKey] = null;
                    }
                }
            }

            if (CantBuyCantVisitShoppingCart)
            {
                StatusDisplay.AddMessage(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
            }

            if (CantBuyDueToTrainingInComplete)
            {
                string trainingIncomplete = string.Format("{0}<br><br>{1}",
                                                   PlatformResources.GetGlobalResourceString("ErrorMessage", "CantBuy"),
                                                   PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                             "CantBuyBecauseofTraining"));
                StatusDisplay.AddMessage(trainingIncomplete);
        }
        }

        [SubscribesTo(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public void OnCartItemRemovedDueToSKULimitationRules(object sender, EventArgs e)
        {
            var args = e as CartModifiedForSKULimitationsEventArgs;
            if (args != null)
            {
                StatusDisplay.AddMessage(args.Message);
            }
        }

        [SubscribesTo(MyHLEventTypes.QuoteError)]
        public void OnQuoteError(object sender, EventArgs e)
        {
            var msg = PlatformResources.GetGlobalResourceString("ErrorMessage", "PricingError");

            if(string.IsNullOrEmpty(msg))
            {
                msg = PlatformResources.GetGlobalResourceString("ErrorMessage", "UnableToPrice");
            }

            StatusDisplay.AddMessage(msg);
        }

        //[SubscribesTo( MyHLEventTypes.PageVisitRefused)]
        //public void PreviousPageRedirected(object sender, EventArgs e)
        //{
        //    PageVisitRefusedEventArgs args = e as PageVisitRefusedEventArgs;
        //    if (null != args)
        //    {
        //        StatusDisplay.AddMessage(StatusMessageType.Error, "From the Master Page itself: <br>" + args.Message);
        //    }
        //}

        #region public methods

        public bool CantBuyCantVisitShoppingCart { get; set; }

        public bool CantBuyDueToTrainingInComplete { get; set; }

        public string Message
        {
            set
            {
                ViewState["ErrorMessage"] = value;
                //this.hidMessage.Text = value; 
            }
            get { return ViewState["ErrorMessage"] as string; } //this.hidMessage.Text; }
        }

        public StatusDisplay Status
        {
            get { return this.StatusDisplay; }
        }

        public void SetDivSpacerVisibility(bool visible)
        {
            if (null != divSpacer)
            {
            divSpacer.Visible = visible;
        }
        }

        public void SetPageHeader(string headerText)
        {
            _PageHeader.Text = headerText;
        }

        public void SetHeaderRowVisibility(bool visible)
        {
            if (null != trHeaderRow)
            {
                trHeaderRow.Visible = visible;
            }
            else
            {
                divHeaderRow.Visible = visible;
            }
        }

        public void SetRightPanelStyle(string key, string value)
        {
            RightPanel.Style.Add(key, value);
        }

        public void LoadControls(PanelControlsConfiguration panelConfig)
        {
            if (panelConfig != null)
            {
                PageConfig pageConfig = getConfigForCurrentPage(panelConfig);
                if (pageConfig != null)
                {
                    if (pageConfig.Params != null)
                    {
                        foreach (UserControlParam param in pageConfig.Params)
                        {
                            setParam(Page, param.Name, param.Value);
                        }
                    }
                    if (null != pageConfig.ConfiguredProperties)
                    {
                        foreach (ConfiguredProperty prop in pageConfig.ConfiguredProperties)
                        {
                            prop.Init();
                            setParam(Page, prop.TargetName, prop.TargetValue);
                        }
                    }
                    if (pageConfig.LeftPanelConfig.Controls.Length > 0)
                    {
                        loadControlsForPanel(LeftPanel, pageConfig.LeftPanelConfig.Controls);
                    }
                    else
                    {
                        if (null != trHeaderRow)
                        {
                            tdleft.Visible = false;
                        }
                        else
                        {
                            divleft.Visible = false;
                        }

                        try
                        {
                            if (!String.IsNullOrEmpty(tdleft.Width))
                            {
                                int widthOfLeftPanel = int.Parse(tdleft.Width.Substring(0, tdleft.Width.Length - 2));
                                int widthofCenterPanel =
                                    int.Parse(divCenter.Style["width"].Substring(0, divCenter.Style["width"].Length - 2));
                                widthofCenterPanel += widthOfLeftPanel;
                                divCenter.Style.Remove("width");
                                divCenter.Style.Add("width", widthofCenterPanel + "px");
                            }
                        }
                        catch
                        {
                        }
                    }
                    loadControlsForPanel(RightPanel, pageConfig.RightPanelConfig.Controls);
                }
            }
        }

        public void ShowMessage(string title, string messsage)
        {
            uchrblMessageBoxControl.ShowMessage(messsage, title);
        }

        public void DisplayHtml(string fragmentName)
        {
            uchrblMessageBoxControl.DisplayHtml(fragmentName);
        }

        public void ShowTrainingMessage(string title, string messsage)
        {
            if (null != pnlMessageBox)
            {
                pnlMessageBox.Visible = true;
            }
            txtMessage.InnerHtml = messsage;
            txtTitle.Text = title;
        }

        public void ShowAnnouncementMessage()
        {
            uchrblMessageBoxControl2.ShowMessage();
        }

        public void SetBulletinBoardVisibility(bool visible)
        {
            if (null != pnlAnnouncements)
            {
                pnlAnnouncements.Visible = visible;
            }
        }
        #endregion

        #region private methods

        private string getPageName(string urlPart)
        {
            int idxQuestionMark = urlPart.IndexOf('?');
            if (idxQuestionMark != -1)
            {
                return urlPart.Substring(0, idxQuestionMark);
            }
            return urlPart;
        }

        private PageConfig getConfigForCurrentPage(PanelControlsConfiguration panelConfig)
        {
            string[] parts = Request.RawUrl.Split('/');
            foreach (PageConfig pConfig in panelConfig.Pages)
            {
                if (pConfig.PageName.ToUpper() == getPageName(parts[parts.Length - 1].ToUpper()))
                {
                    return pConfig;
                }
            }
            return null;
        }

        private object convertValue(string value, PropertyInfo destinationInfo)
        {
            if (destinationInfo.PropertyType.Name == "Boolean")
            {
                return value.ToUpper() == "FALSE" ? false : true;
            }
            else if (destinationInfo.PropertyType.Name == "Int32")
            {
                int output;
                if (int.TryParse(value, out output))
                {
                    return output;
                }
            }
            else if (destinationInfo.PropertyType.Name == "string")
            {
                return value;
            }
            return null;
        }

        private void setParam(Object obj, string name, string value)
        {
            PropertyInfo pInfo = obj.GetType().GetProperty(name);
            if (pInfo != null)
            {
                object convertedValue = null;
                if ((convertedValue = convertValue(value, pInfo)) != null)
                {
                    pInfo.SetValue(obj, convertedValue, null);
                }
            }
        }

        private void setParam(Object obj, string name, object value)
        {
            PropertyInfo pInfo = obj.GetType().GetProperty(name);
            if (pInfo != null)
            {
                pInfo.SetValue(obj, value, null);
            }
        }

        private void loadControlsForPanel(Control parentControl, PanelUserControl[] controls)
        {
            foreach (PanelUserControl cntrl in controls)
            {
                Control uc = LoadControl(cntrl.Name);
                if (uc != null)
                {
                    if (null != cntrl.ConfiguredProperties)
                    {
                        foreach (ConfiguredProperty prop in cntrl.ConfiguredProperties)
                        {
                            prop.Init();
                            setParam(uc, prop.TargetName, prop.TargetValue);
                        }
                    }
                    if (uc.Visible)
                    {
                        parentControl.Controls.Add(uc);
                        //HtmlGenericControl ulCntrl = new HtmlGenericControl("br");
                        //parentControl.Controls.Add(ulCntrl);

                        if (cntrl.Params != null)
                        {
                            foreach (UserControlParam param in cntrl.Params)
                            {
                                setParam(uc, param.Name, param.Value);
                            }
                        }
                    }
                }
            }
        }

        #endregion


        protected void OnYes(object sender, EventArgs e)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var currentSessionInfo = SessionInfo.GetSessionInfo(member.Value.Id, CultureInfo.CurrentCulture.Name);
            currentSessionInfo.TrainingBreached = true;
            if (null != pnlMessageBox)
            {
            pnlMessageBox.Visible = false;
            }
            
            DistributorOrderingProfile dsProfile = DistributorOrderingProfileProvider.GetProfile(member.Value.Id,
                                                                                                 member.Value
                                                                                                       .ProcessingCountryCode);
            dsProfile.CantBuyOverride = true;
        }

        // protected HtmlTableCell LeftNavMenuCell;
        [Description("Sets the Visible property of the divleft container.")]
        [Browsable(true)]
        [Category("Appearence")]
        public bool divLeftVisibility
        {
            set
            {
                if (null != tdleft)
                {
                    tdleft.Visible = value;
                }
                else
                {
                    divleft.Visible = value;
                }
            }
        }
        public void gdoNavMidCSS(string classes)
        {
            if (null != gdoNavMid)
            {
                gdoNavMid.Attributes["class"] = classes;
            }
        }
        public void gdoNavRightCSS(string classes)
        {
            if (null != gdoNavRight)
            {
                gdoNavRight.Attributes["class"] = classes;
            }
        }
    }
}
