<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DeprecationMessage.ascx.cs" Inherits="MyHerbalife3.Web.Controls.DeprecationMessage" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.Interfaces" %>
<%@ Import Namespace="MyHerbalife3.Shared.Infrastructure.ValueObjects" %>

<%
    ILocalizationManager _LocalizationManager;
    _LocalizationManager = new LocalizationManager();
%>

<section wire-model="SessionMsgViewModel" wire-mode='own, "ieDeprecationMsg", true' data-bind="visible: isVisible" class="section-outdated_message" style="display: none">
    <div class="browserVersion newBrowser" aria-hidden="false" role="banner">
        <a class="close-icon" data-bind="click: closeMessage"><i class="icon-delete-fl-5"></i></a>
        <h1><%= _LocalizationManager.GetGlobalString("HrblUI", "UnsupportedIEVersion") %></h1>
        <p><%= _LocalizationManager.GetGlobalString("HrblUI", "GetLatestUpgrading") %></p>
        <ul>
            <li>
                <a href="https://www.google.com/chrome/" target="_blank">
                    <img src="/SharedUI/Images/fallback/google_chrome_logo-min.png" title="Download Google Chrome" />
                    <span><%= _LocalizationManager.GetGlobalString("HrblUI", "InstallChrome") %></span>
                </a>
            </li>
            <li>

                <a href="http://windows.microsoft.com/en-US/internet-explorer/downloads/ie" target="_blank">
                    <img src="/SharedUI/Images/fallback/internet_explorer_logo-min.png" title="Download Internet Explorer" />
                    <span><%= _LocalizationManager.GetGlobalString("HrblUI", "UpgradeIE") %></span>
                </a>
            </li>
        </ul>
    </div>
</section>
