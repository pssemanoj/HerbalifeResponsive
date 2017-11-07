<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LogonStatusView.ascx.cs"
    Inherits="HL.MyHerbalife.Web.Controls.Logon.LogonStatusView" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure" %>
<%@ Register TagPrefix="myhl" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div id="DistributorInfoTable">

    <div>
        <asp:Label ID="_FullName" runat="server" Text="First {0} Last{0}" CssClass="FullName"
            meta:resourcekey="FullNamePattern"></asp:Label>
        <myhl:SecuritizedContent runat="server" ID="SecuritizedContent1" DenyRoles="Customer"
            UnauthorizedRenderMode="DoNotRender">
            <p id="headerVolumeSummary">
                <span data-bind="visible: loading"><i class="green icon-loading"></i></span>
                <span class="Volume" data-bind="html: CurrentMonthVolume.HeaderText, visible: loaded"></span>
                <span data-bind="style: { display: error }" class="red hide"><i class="icon-x-circle"></i>
                     <asp:Literal ID="lblvolumeNotAvailable" runat="server" Text="Volume Unavailable"></asp:Literal>
                </span>
            </p>
            
        </myhl:SecuritizedContent>        
        <myhl:SecuritizedContent runat="server" ID="Securitized1" DenyRoles="Customer" UnauthorizedRenderMode="DoNotRender">
            <div id="divBizworks" runat="server">
                <asp:Panel ID="BizWorksLogo" runat="server" CssClass="BizWorksLogo">
                    <img src="/Content/Global/img/hl_icon_10px.gif" />&nbsp;<asp:Label ID="_BizWorksSubscriber"
                        runat="server" Text="BIZWORKS SUBSCRIBER" meta:resourcekey="BizWorksSubscriber"></asp:Label>
                </asp:Panel>
            </div>
        </myhl:SecuritizedContent>
    </div>

    <div>
        <asp:HyperLink runat="server" 
            ID="_LogonStatus" 
            CssClass="LogoutLink" 
            Target="_parent"
            Text="Logout"
            NavigateUrl="~/Authentication/Logout"
            
            EnableViewState="False" />

         <asp:Panel runat="server" ID="pnlAlertIcon" Visible="<%#GlobalContext.CultureConfiguration.IsDSAlertsEnabled %>">
            <div id="alertIcon" data-bind="attr: { class: EnvelopeClass }">
                <asp:HyperLink runat="server" ID="alertNumber" ClientIDMode="Static" NavigateUrl="~/Account/Communication/Notifications.aspx" >
                    <span id="_AlertInit">                                        
                        <span  id="_Alerts" class="TabTeam dsIcon" data-bind="text: AlertCount"></span>
                    </span>                                    
                    <span id="_AlertsUpdate" class="TabTeam dsIcon"></span>
                </asp:HyperLink>
            </div>
        </asp:Panel>
        <asp:Label ID="_Salutation" runat="server" Text="[salutation key]" CssClass="TabTeam salutation"></asp:Label>
    </div>    
    <span style="display: none" id="_salutid">
        <%=salutation%>
    </span>

    <script type="text/javascript">
        //Investigate what this line does!!
        if (typeof (s) != "undefined") {
            s.prop26 = "<%=salutation.ToLower()%>";
        }
    </script>

</div>