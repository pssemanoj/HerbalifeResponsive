<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FullHeader.ascx.cs" Inherits="HL.MyHerbalife.Web.Controls.Navigation.FullHeader" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Resources" %>

<% if (IsMicroServiceEnabled)
    { %>
    <% =MyHerbalife.Navigation.ApiAdapter.Helpers.NavigationHelper.GetContent("Header",
                                                        currentExperience.NavigationVersion,
                                                         _GlobalContext.CultureConfiguration.Locale,
                                                        currentExperience.BrowseScheme
                                                        )%>
<% }
    else
    { %>
<!-- HEADER -->
<header>
    <!-- Local selector -->
    <section id="locales">

        <div class="wrap">
            <a href="#hl" id="closeLocalSel">
                <i class="icon-x"></i>
            </a>
        </div>
        <%
            if (LocaleSelectionData == null)
            {
        %>
        <!-- no data, can't render selector -->
        <%
            }
            else if (LocaleSelectionData.Children == null || !LocaleSelectionData.Children.Any())
            {
        %>
        <!-- no child nodes in data, can't render selector -->
        <%
            }
            else
            {%>
            <div class="locales-wrap">
            <%
                int i = 0;
                foreach (var item in LocaleSelectionData.Children)
                { %>
                <div class="locales-drop" id="locales-drop-<%= i %>">
                    <h4 class="black"><%= item.Text %></h4>
                    <select class="localeSelector">
                        <option value="" selected="selected"></option>
                        <% if (item.Children != null && item.Children.Any())
                            {
                                foreach (var child in item.Children)
                                {
                                    string selectedTag = child.Url.Equals(System.Threading.Thread.CurrentThread.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase) ? "selected=\"selected\"" : string.Empty;
                                    %>
                                    <option value="<%= child.Url %>" <%= selectedTag %>><%= child.Text %></option>
                                    <%
                                            }
                                        }
                        %>
                    </select>
                </div>
            <%
                    i++;
                }%>
            </div>
        <%
            }
        %>

    </section>


    <!-- place holders for header partial views -->
    <section id="DSInfo">


        <ul id="DSStats">

            <li>
                <span id="mobile-nav-btn" wire-model="mobileNavViewModel" data-bind="click: open">
                    <i class="icon-list-fl-1"></i>
                </span>

                <a href="/Home/Default" class="logo">
                    <img src="/SharedUI/Images/logo-myherbalife-sm-green.png">
                </a>

            </li>

            <!-- /LOCALE SELECTOR -->
            <li class="locale-picker">
                <a href="#" id="localeSel" class="capitalize">
                    <span>
                        <%: (new RegionInfo(CultureInfo.CurrentUICulture.Name).EnglishName) %> - <%: (new CultureInfo(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName).NativeName) %>
                    </span>
                    <i class="icon-chevron-down"></i>
                </a>
            </li>

            <li class="user-info-details">

                <asp:PlaceHolder runat="server" ID="notificationsEnvelope" >
                <% if (IsDsAlertsEnabled)
                    { %>

                    <a class="alert-notifications" id="alertsLocaleSelector" wire-model="InboxViewModel" href="/Account/Communication/Notifications.aspx" rel="tooltip">
                        <i class="icon-email">
                            <span data-bind="text: AlertCount"></span>
                        </i>
                    </a>       
         
                <% } %>
                </asp:PlaceHolder>  

                <div class="profileData" wire-model="ProfileViewModel">
                    <div>
                        <span data-bind="visible: loading"><i class="green icon-loading"></i></span>
                        <span class="bold profile-text" data-bind="text: Name, visible: loaded"></span>
                    </div>
                </div>
                <div class="profileData" wire-model="ProfileViewModel">
                    <div>
                        <span data-bind="visible: loading"><i class="green icon-loading"></i></span>
                        <span class="profile-text icon-trophy" data-bind="text: TeamLevelName, visible: loaded"></span>

                    </div>
                </div> 
                <div class="volumeData" wire-model="VolumeViewModel">
                    <span data-bind="visible: loading" class="green icon-loading"></span>

                    <div class="single" data-bind="visible: loadSingle" style="display: none">
                        <a href="/Account/Volume.aspx" class="volume-text icon-bars" data-bind="text: CurrentMonthVolume.PlainHeaderText"></a>
                    </div>
                    

                    <div class="dual" data-bind="visible: loadDual" style="display: none">
                        <span class="icon-bars">
                            <%= VolumeHeaderDualMonth %>
                        </span>
                        <a href="/Account/Volume.aspx" data-bind="text: CurrentMonthVolume.DualMonthHeaderVolume">
                        </a>
                        <a href="/Account/Volume.aspx" data-bind="text: LastMonthVolume.DualMonthHeaderVolume">
                        </a>                         
                    </div>

                    <span data-bind="style: { display: error }" class="red hide"><i class="icon-x-circle"></i>  <%= VolumeNotAvailable %></span>
                </div>

                <% if (IsOrderingEnabled){ %>
                    <a wire-model="CartViewModel" href="#" class="cart-link" data-bind="events: { click: checkout }">
                        <i class="icon-cart-ln-6"></i>
                    </a>
                <% } %> 
            </li>

        </ul>

    </section>


    <!--TOP NAV -->
    <nav id="main-nav" wire-model="topNavViewModel" data-bind="attr: { class: hideClass }">
        <div class="container">

            <% if (MainNavigationData == null)
                { %>
            <!-- no data, can't render menu -->
            <% }
                else if (MainNavigationData.Children == null || !MainNavigationData.Children.Any())
                {
            %>
            <!-- no child nodes in data, can't render menu -->
            <% }
                else
                {
            %>
            <ul class="hrblMenu" id="topNav" role="menubar" tabindex="-1" wire-model="topNavViewModel">
                <%
                    int i = 0;
                    int j;
                    foreach (var item in MainNavigationData.Children.Where(c => c.IsHidden == false))
                    { %>
                    <li role="menuitem" tabindex="-1" aria-haspopup="true" data-bind="events: { mouseover: openLevel, mouseout: openLevel }">
                        <span data-bind="click: openLevel" id="firstlevel-<%= i %>">
                            <span>
                                <%= item.Text %>
                            </span>
                        </span>
                        <% if (item.Children != null && item.Children.Any())
                            {
                        %>
                        <ul role="menu" tabindex="-1" style="display: none;">
                            <% 
                                j = 0;
                                foreach (var child in item.Children.Where(c => c.IsHidden == false))
                                {
                                %>
                                <li role="menuitem" tabindex="-1" id="firstlevel-<%= i %>-secondlevel-<%= j %>">
                                    <a href="<%= child.Url %>" target="<%= child.Target %>" title="<%= child.Description %>">
                                        <i class="<%= child.CssClass %>"></i><%= child.Text %>
                                    </a>
                                </li>
                                <%
                                        j++;
                                    }
                                %>
                        </ul>
                    <% } %>
                    </li>
                <% 
                        i++;
                    } %>
            </ul>
            <%
                }
            %>

            <div id="rightTopNav">
                <% if (IsSearchBoxEnabled){ %>  

                    <input type="search" class="styledFields search" name="sp_q" placeholder="<%= this.SearchText %>"/>

                <% } %>
                <ul class="hrblMenu">

                    <% if (IsOrderingEnabled){ %>
                    <li id="cartInfoDropDown" data-bind="events: { mouseover: openLevel, mouseout: openLevel }">
                        <span data-bind="click: openLevel">
                            <span>
                                <i class="icon-cart-ln-6"></i>
                            </span>
                        </span>                        
                        <ul style="display: none">
                            <li>
                                <div id="dvCartInfo" wire-model="CartViewModel">
                                    <div class="hrblPreLoader-miniCart"  data-bind="visible: loading"></div>
                                    <div data-bind="visible: loaded">
                                        <h5>
                                        <span class="darkgreen bold" data-bind="text: quantity"></span>
                                        <%: ShoppingCart_Items %>
                                        </h5>
                                        <hr>
                                        <h6><%: ShoppingCart_Subtotal %>:
                                            <span data-bind="text: subTotal"></span>
                                        </h6>
                                        <h6><%: ShoppingCart_VolumePoints %>:
                                            <span data-bind="text: volumePoints"></span>
                                        </h6>
                                
                                        <a class="btnForward" data-bind=" events: { click: checkout }"><%: ShoppingCart_Checkout %></a>
                                        <hr>
                                        <ul class="vert-links">
                                            <li><a data-bind=" events: { click: viewSavedCarts }" href="#hrbl"><i class="icon-eye"></i><%: ShoppingCart_ViewCarts %></a></li>
                                        </ul>
                                    </div>
                                    <div data-bind="visible: error">
                                        <h4 class="red center"><i class="icon-x-circle"></i><br />Error Processing your request.</h4>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </li>    <% } %>            
                    <li id="myProfileDropDown" data-bind="events: { mouseover: openLevel, mouseout: openLevel }">
                        <span data-bind="click: openLevel">
                            <span>
                                <i class="icon-user"></i>
                            </span>
                        </span>                        
                        <ul wire-model="ProfileViewModel" style="display: none">
                            <li>
                                <nav>
                                    <h4 data-bind="text: Name"></h4>
                                    <ul>
                                        <!-- Subscriptions -->
                                        <li><i class="icon-trophy orange"></i><span data-bind="text: TeamLevelName"></span></li>                                    
                                        <li data-bind="visible: IsBizworksSubscriber"><i class="icon-herbalife green"></i><span data-bind="text: DisplayBizworksStatus"></span></li>
                                        <hr />
                                        <!-- Settings -->
                                        <li><a href="/Account/Profile/Default.aspx"><i class="icon-card"></i><span><%: MiniProfile_MyAccount %></span></a></li>
                                        <li><a href="<%=MiniProfile_SettingsLink %>"><i class="icon-cog"></i><span><%: MiniProfile_Settings %></span></a></li>
                                        <% if (IsOnlineProfileEnabled){ %>  
                                        <li><a href="/Account/Profile/LoginPreferences.aspx"><i class="icon-lock-ln-1"></i><span><%: MiniProfile_LoginPreferences %></span></a></li>
                                        <% } %> 
                                        <hr/>
                                        <a href="/Authentication/Logout" class="btn full-width">
                                            <i class="icon-power-ln-3"></i>
                                            <%: MiniProfile_LogOut %>   
                                        </a>
                                    </ul>
                                </nav>
                            </li>
                        </ul>
                    </li>
                    <% if (IsSearchBoxEnabled){ %>  
                        <li id="search-holder" data-bind="events: { mouseover: openLevel, mouseout: openLevel }">
                            <span data-bind="click: openLevel">
                                <span>
                                    <%= this.SearchText %>
                                </span>
                            </span>
                            <ul style="display: none">
                                <li>
                                    <input type="search" class="styledFields search" name="sp_q" placeholder="<%= this.SearchText %>" />
                                </li>
                            </ul>
                        </li>
                    <% } %>                                    
                </ul>
            </div>
        </div>
    </nav>
    <!-- /TOP NAV -->
    
    <!-- Emergency Notifications fh-->
    <% if (IsDsAlertsEnabled){ %>  
        <div data-template="notificationTemplate" wire-model="AlertsViewModel" data-bind="source: BannerAlerts"></div>
        <script id="notificationTemplate" type="text/x-kendo-template">
            <div class="systemNotification greenNotification" data-bind="click: CollapseNotification, style: { background-color: NotificationColor }">
                <div class="notificationWrapper">
                    <i data-bind="visible: CanCollapse, attr: {class: CollapseClass}" aria-hidden="true"></i> 
                    <span data-bind="html: Body"></span>
                    <div data-bind="invisible: IsCollapsed">
                        <span class="left" data-bind="visible: HasLink">
	                        <a class="left" target="_blank" data-bind="attr: { href: LinkUrl }, text: LinkLabel, visible: HasLink"></a>
	                    </span> 
                    </div>
                </div>
            </div>
        </script>          
    <% } %>
    <!-- /Emergency Notifications -->

    <!-- end header place holders -->
</header>
<!-- /HEADER -->
<%} %>