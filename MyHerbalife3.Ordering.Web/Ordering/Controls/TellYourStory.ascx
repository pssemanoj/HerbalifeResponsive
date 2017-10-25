<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TellYourStory.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.TellYourStory" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<div class="confirm-tellyourstory" runat="server">
                <cc2:ContentReader ID="TellYourStoryBox" runat="server" ContentPlaceHolderID="HeaderContent" Visible="false" ContentPath="tellstory.html"
                                SectionName="TellYourStory" ValidateContent="true" UseLocal="true"/>
                        </div>