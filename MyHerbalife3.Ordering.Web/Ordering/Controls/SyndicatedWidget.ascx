<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SyndicatedWidget.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.SyndicatedWidget" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%-- Syndicated Widget --%>
<div class="syndicated-widget" runat="server">
    <cc2:ContentReader ID="Syndicated" runat="server" ContentPlaceHolderID="HeaderContent" Visible="false" ContentPath="syndicated.html" 
        SectionName="SyndicatedWidget" ValidateContent="false" UseLocal="true"/>
</div>