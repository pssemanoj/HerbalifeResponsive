<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AlertContainer.ascx.cs" Inherits="HL.MyHerbalife.Web.Controls.AlertContainer" %>

<div id="bannerAlertContainer" class="hrblAlert" style="display:none;" 
     data-bind="invisible: getShouldShowBanner(<%=IsEnabled.ToString().ToLower()%>), attr: { class: getBannerType },
    style: { padding: ContainerPadding } " >
    <div id="alertTitle" data-bind="html: CurrentAlert.Title"></div>
    <div id="alertContent" data-bind="html: CurrentAlert.Body, invisible: Collapsed"></div>
    <div id="alertLink" data-bind="invisible: Collapsed">
        <span class="left">
	        <a id="lnkMore" class="left" target="_blank" data-bind="attr: { href: CurrentAlert.LinkUrl }, text: CurrentAlert.LinkLabel"></a>
	    </span> 
    </div>
    <div id="alertNav" class="nav">	           
	    <span class="pagination" invisible="getShouldHidePagination">
	        <a data-bind="click: previousAlert" style="background: url(/SharedUI/Images/buttons/btn-small-arrow-left.png) no-repeat center center;"></a>
	        <span id="alertCounter" data-bind="text: getCurrentCount('<%=GetLocalResourceObject("AlertContainer_Of.text").ToString()%>    ')"></span>
	        <a data-bind="click: nextAlert" style="background: url(/SharedUI/Images/buttons/btn-small-arrow-right.png) no-repeat center center;"></a>
	    </span>
        <a id="collapseBtn" class="up" data-bind="click: collapseAlert, attr: { class: UpDownClass }">
            <span data-bind="invisible: Collapsed"><asp:Literal ID="lt_link" meta:resourcekey="AlertContainer_Collapse" runat="server"></asp:Literal></span>
            <span data-bind="visible: Collapsed"><asp:Literal ID="lt_link_expand" meta:resourcekey="AlertContainer_Expand" runat="server"></asp:Literal></span>
        </a>
    </div>
</div>