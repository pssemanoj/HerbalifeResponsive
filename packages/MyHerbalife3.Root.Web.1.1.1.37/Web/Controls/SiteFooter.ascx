<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SiteFooter.ascx.cs"
    Inherits="HL.MyHerbalife.Web.Controls.SiteFooter" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>


<div id="FooterRibbon">
    <asp:Label ID="_CountryLanguage" runat="server" Text="Language | Country"></asp:Label>
</div>
<asp:LoginView ID="_BottomNavLoginView" runat="server">
    <LoggedInTemplate>
        <div id="FooterArea">
            <asp:Repeater ID="_FooterLinks" runat="server" DataSourceID="_FooterNavDataSource">
                <ItemTemplate>
                    <asp:HyperLink runat="server" ID="footnav" 
                        NavigateUrl='<%# ((SiteMapNode)Container.DataItem).Url %>'
                        Text='<%# ((SiteMapNode)Container.DataItem).Title %>'
                        Target='<%# ((SiteMapNode)Container.DataItem)["target"]%>' />
                </ItemTemplate>
                <SeparatorTemplate>
                    |</SeparatorTemplate>
            </asp:Repeater>
        </div>
    </LoggedInTemplate>
</asp:LoginView>

<div id="FooterText">
    <cc1:ContentReader ID="_NewFooterText" runat="server" ContentPath="footer.html" SectionName="home"  />
</div>
<cc1:ContentReader ID="_ContentReader" runat="server" ContentPath="footer-logos.html" SectionName="home" />
<asp:SiteMapDataSource ID="_FooterNavDataSource" runat="server" ShowStartingNode="False" />
