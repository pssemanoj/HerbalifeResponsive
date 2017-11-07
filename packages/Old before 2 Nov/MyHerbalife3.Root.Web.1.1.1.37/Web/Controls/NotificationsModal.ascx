<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NotificationsModal.ascx.cs" Inherits="MyHerbalife3.Web.Controls.NotificationsModal" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.Interfaces" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.ValueObjects" %>

<%
    ILocalizationManager _LocalizationManager;
    _LocalizationManager = new LocalizationManager();
%>

<div id="modalNotif" wire-mode="own, 1, 0, 2" wire-model="dismissableUiModel" class="hrblModalSkinOnly" data-role="window" data-modal="true" data-actions="" data-draggable="false" data-resizable="false" data-bind="visible: isVisible, centerWindow: true, getWidget: actualWidget" style="display:none">
    <img data-bind="attr: { src: imgSrc }, visible: imgSrc" src="" />
    <h3 data-bind="text: title, visible: title"></h3>
    <div data-bind="html: body">
    </div>
    <div class="bottomActBtns textLeft">
        <a data-bind="click: onCallToAction, text: callToAction.text, attr: { href: callToAction.value }, visible: callToAction" class="btnForward right" href="#hrbl"></a>     
        <a data-bind="click: onDismissForLater, text: showMeLater.text, visible: showMeLater" class="btn right" href="#hrbl"></a>
        <a data-bind="click: onDismissIndefinite, text: doNotShowAgain.text, visible: doNotShowAgain" class="btn not-show-again-button" href="#hrbl"></a>        
    </div>
</div>