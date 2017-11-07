<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TabbedNav.ascx.cs" Inherits="HL.MyHerbalife.Web.Controls.Navigation.TabbedNav" %>
<asp:SiteMapDataSource ID="_SiteMapDS" runat="server" ShowStartingNode="false" />
<%

    if (MainNavigationData == null)
    {
%>
    <!-- no data, can't render menu -->
<%
    }
    else if (MainNavigationData.Children == null || !MainNavigationData.Children.Any())
    {
%>
    <!-- no child nodes in data, can't render menu -->
<%
    }
    else
    {
%>
    <ul id="top_Nav" role="menubar" tabindex="-1">
        <!--BROWN TOP NAV CONTROL HOMEPAGE AND ASPX PAGES -->
        <%
            foreach (var c1 in MainNavigationData.Children)
            {
                if (!IsPcPsr || (IsPcPsr && !PcPsrHiddenSections.Any(x => x.Equals(c1.SectionName, StringComparison.OrdinalIgnoreCase))))
                {
        %>
            <li role="menuitem" tabindex="-1" aria-haspopup="true"><a href="<%= c1.Url %>" target="<%= c1.Target %>" title="<%= c1.Description %>"><i class="<%= c1.CssClass %>"></i><%= c1.Text %></a>
                <%
                    if (c1.Children != null && c1.Children.Any())
                    {
                %>
                    <ul role="menu" tabindex="-1">
                        <% foreach (var c2 in c1.Children)
                           {
                               var hasChildren = c2.Children != null && c2.Children.Any();
                             %>
                            <li role="menuitem" tabindex="-1"><a href="<%= c2.Url %>" target="<%= c2.Target %>" title="<%= c2.Description %>"> <% if(hasChildren) { %> <span class="k-icon k-i-arrow-e"></span><% }%>  <i class="<%= c2.CssClass %>"></i><%= c2.Text %> </a>
            
            
                            <%
                                if (hasChildren)
                                {
                            %>
                                <ul role="menu" tabindex="-1">
                                    <% foreach (var c3 in c2.Children)
                                       { %>
                                        <li role="menuitem" tabindex="-1"><a href="<%= c3.Url %>" target="<%= c3.Target %>" title="<%= c3.Description %>"><i class="<%= c3.CssClass %>"></i><%= c3.Text %></a></li>
                                    <% } %>
                                </ul>
                            <% } %>

                            </li>        
                         <% } %>
                    </ul>
                <% } %>
            </li>
        <% } } %>
    </ul>
<%
    }
%>