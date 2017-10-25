<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsBasicSearch.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.ContactsBasicSearch" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Panel runat="server" ID="BasicSearchPanel" meta:resourcekey="BasicSearchPanel">
    <div id="basic-search-content">
        <table class="basic-search-table">
            <tr>
                <td colspan="3" id="advanced-search-link">
                    <asp:LinkButton runat="server" ID="AdvancedSearchLinkButton" Text="Advanced Search Options"
                        CommandName="ShowAdvancedSearch" meta:resourceKey="AdvancedSearchButton"></asp:LinkButton>
                </td>
            </tr>
            <tr>
                <td id="options-td">
                    <asp:Label ID="SearchContactsPrompt" runat="server" meta:resourceKey="SearchContactsPrompt">Search Contacts:</asp:Label>
                    <asp:DropDownList runat="server" ID="SearchContactsDropDown" meta:resourceKey="SearchContactsDropDown">
                        <asp:ListItem Value="NameContains" Text="Name Contains" meta:resourceKey="SearchContactsDropDown_NameContains" />
                        <asp:ListItem Value="FirstNameStartsWith" Text="First Name Starts With" meta:resourceKey="SearchContactsDropDown_FirstNameStartsWith" />
                        <asp:ListItem Value="LastNameStartsWith" Text="Last Name Starts With" meta:resourceKey="SearchContactsDropDown_LastNameStartsWith" />
                    </asp:DropDownList>
                </td>
                <td id="searchfield-td">
                    <asp:TextBox ID="SearchField" runat="server" meta:resourceKey="SearchField" /><br />
                </td>
                <td id="search-button-td">
                    <div style="float: right;">
                        <cc1:OvalButton ID="BasicSearch" runat="server" Coloring="Silver" ArrowDirection="rt"
                            IconType="arr" IconPosition="rt" OnClick="BasicSearch_click" meta:resourceKey="BasicSearchButton">Search</cc1:OvalButton>
                    </div>
                    <div style="width: 10px; float: right;">
                        &nbsp;</div>
                    <div style="float: right;">
                        <cc1:OvalButton ID="ClearSearch" runat="server" Coloring="Silver" ArrowDirection="rt"
                            IconType="arr" IconPosition="rt" OnClick="ClearSearch_click" meta:resourceKey="ClearSearchButton">Clear</cc1:OvalButton>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Panel>
