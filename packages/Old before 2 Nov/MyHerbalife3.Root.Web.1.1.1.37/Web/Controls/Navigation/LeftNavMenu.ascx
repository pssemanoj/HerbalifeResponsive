<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LeftNavMenu.ascx.cs"
    Inherits="HL.MyHerbalife.Web.Controls.Navigation.LeftNavMenu" %>
<%
    if (NavigationInfo.LeftNavOwner == null)
    {
%>
<!-- no data, can't render menu -->
<%
    }
    else if (NavigationInfo.LeftNavOwner.Children == null || !NavigationInfo.LeftNavOwner.Children.Any())
    {
%>
<!-- no child nodes in data, can't render menu -->
<%
    }
    else
    {
%>
<ul id="left_menu" class="leftMenuNew" role="menu" tabindex="-1">
    <!--BROWN LEFT NAV CONTROL HOMEPAGE AND ASPX PAGES -->
    <%
            foreach (var c1 in NavigationInfo.LeftNavOwner.Children)
            {
    %>
    <li role="menuitem" tabindex="-1"><a href="<%= c1.Url %>" target="<%= c1.Target %>" title="<%= c1.Description %>"><%= c1.Text %></a>
        <%
                    if (c1.Children != null && c1.Children.Any() && !IsMyHerbalife3Enabled)
                    {
        %>
        <ul role="menu" tabindex="-1">
            <% foreach (var c2 in c1.Children)
                           { %>
            <li role="menuitem" tabindex="-1"><a href="<%= c2.Url %>" target="<%= c2.Target %>" title="<%= c2.Description %>"><%= c2.Text %></a>


                <%
                                if (c2.Children != null && c2.Children.Any())
                                {
                %>
                <ul role="menu" tabindex="-1">
                    <% foreach (var c3 in c2.Children)
                                       { %>
                    <li role="menuitem" tabindex="-1"><a href="<%= c3.Url %>" target="<%= c3.Target %>" title="<%= c3.Description %>"><%= c3.Text %></a></li>
                    <% } %>
                </ul>
                <% } %>            
            </li>
            <% } %>
        </ul>
        <% } %>
    </li>
    <% } %>
</ul>
<%
    }
%>
