<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchBox.ascx.cs" Inherits="HL.MyHerbalife.Web.Controls.SearchBox" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<div id="SearchBox">

                <asp:TextBox runat="server" ID="_search_txt" Width="130px" Text="[Search]" meta:resourcekey="SearchTerms"
                    onclick="this.value='';" />

                <cc2:DynamicButton ID="_GoButton" ButtonType="Neutral" runat="server" Text="Go" ValidationGroup="SearchTeamLevel"
                        meta:resourceKey="GoButton" />

</div>
