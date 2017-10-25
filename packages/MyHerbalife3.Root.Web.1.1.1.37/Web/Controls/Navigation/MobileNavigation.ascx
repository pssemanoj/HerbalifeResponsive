<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Resources" %>
<%@ Control Language="C#" ClassName="WebUserControl" Inherits="HL.MyHerbalife.Web.Controls.Navigation.MobileNavigation" %>
<% if (IsMicroServiceEnabled && _GlobalContext.CurrentExperience.ExperienceType!=MyHerbalife3.Shared.ViewModel.ValueObjects.ExperienceType.Black)
    { %>

        <% =MyHerbalife.Navigation.ApiAdapter.Helpers.NavigationHelper.GetContent("MobileNavigation",
                                                        currentExperience.NavigationVersion,
                                                         _GlobalContext.CultureConfiguration.Locale,
                                                        currentExperience.BrowseScheme
                                                        )%>

<% } else {%>

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

    <nav id="mobile-main-nav" wire-model="mobileNavViewModel">
        <% if (IsSearchBoxEnabled){ %> 

                <input type="search" placeholder="<%= this.SearchText %>" name="sp_q" class="styledFields search">

        <% } %>
        <aside wire-model="ProfileViewModel">
            <i class="icon-user"></i>
            <h6 data-bind="text: Name"></h6>
            <hr />
            <h6>
                <span data-bind="text: TeamLevelName"></span>
            </h6>
            <h6>
                <span data-bind="text: DisplayBizworksStatus"></span>
            </h6>
            <a class="settings-link" href="<%=MiniProfile_SettingsLink %>">
                <span><%: MiniProfile_Settings %></span>
            </a>
            <a href="/Account/Profile/Default.aspx">
                <span><%: MiniProfile_MyAccount %></span>
            </a>
            <% if (IsOnlineProfileEnabled){ %>  
                <a href="/Account/Profile/LoginPreferences.aspx">
                    <span><%: MiniProfile_LoginPreferences %></span>
                </a>
            <% } %>

            <a href="/Authentication/Logout" class="action-button">
                <%: MiniProfile_LogOut %>
            </a>
        </aside>



        <ul class="mobile-nav-links">
            <% foreach (var item in MainNavigationData.Children.Where(c => c.IsHidden == false))
                { %>
                <li>
                    <span>
                            <%= item.Text %>
                    </span>
                    <% if (item.Children != null && item.Children.Any())
                        {
                    %>
                    <ul>
                        <% foreach (var child in item.Children.Where(c => c.IsHidden == false))
                            {
                            %>
                            <li>
                                <a href="<%= child.Url %>" target="<%= child.Target %>" title="<%= child.Description %>">
                                    <%= child.Text %>
                                </a>
                            </li>
                            <% 
                            } 
                            %>
                    </ul>
                <% } %>
                </li>
            <% } %>
        </ul>
    </nav>
<%
    }
%>
<%} %>