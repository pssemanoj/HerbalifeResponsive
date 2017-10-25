<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GoBackButton.ascx.cs" Inherits="MyHerbalife3.Web.Controls.GoBackButton" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.Interfaces" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.ValueObjects" %>

<%
    ILocalizationManager _LocalizationManager;
    _LocalizationManager = new LocalizationManager();
%>

<div class="back-button">
    <a href="#" wire-model="HistoryNavigationViewModel" data-bind="click: goToPreviousPage, visible: isVisible" style="display: none"><i class="icon-arrow-circle-ln-27"></i> <%= _LocalizationManager.GetGlobalString("HrblUI", "GoBack") %></a>
</div>