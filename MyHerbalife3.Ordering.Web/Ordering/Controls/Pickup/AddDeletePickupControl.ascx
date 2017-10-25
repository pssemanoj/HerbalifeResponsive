<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddDeletePickupControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.AddDeletePickupControl" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div id="divAddPickUp" runat="server">
    <table width="710" border="0" cellspacing="5" cellpadding="0">
        <tr>
            <td colspan="3">
                <h3>
                    <asp:Label ID="header" Text="Pickup Location" runat="server" meta:resourcekey="headerResource1"></asp:Label></h3>
            </td>
        </tr>
        <tr>
            <td class="gdo-form-label-right gdo-popup-form-label-padding">
                <asp:Label ID="lblLocation" runat="server" Text="Locations*:" meta:resourcekey="lblLocationResource1"></asp:Label>
            </td>
            <td colspan="2">
                <asp:DropDownList ID="ddlPickupLocations" runat="server" Width="300px" OnSelectedIndexChanged="ddlPickupLocations_IndexChanged"
                    meta:resourcekey="ddlPickupLocationsResource1">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="gdo-form-label-right gdo-popup-form-label-padding">
                <asp:Label ID="lblAddress" runat="server" Text="Address:"></asp:Label>
            </td>
            <td colspan="2">
                <p id="pAddress" runat="server">
                </p>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <div class="gdo-spacer1">
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <div class="gdo-error-message-div">
                    <img src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon"
                        alt="Error" /><span class="gdo-error-message-txt">
                            <asp:Label ID="lblErrors" runat="server" meta:resourcekey="lblErrorsResource1"></asp:Label></span></div>
            </td>
        </tr>
    </table>
</div>
<div id="divDeletePickUp" runat="server">
    <table>
        <tr>
            <td colspan="3">
                <h3>
                    <asp:Label ID="lblDeleteHeader" runat="server" meta:resourcekey="lblDeleteHeaderResource1"
                        Text="Delete Pickup Location"></asp:Label></h3>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:" meta:resourcekey="lblDeleteNicknameResource1"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteNicknameText" runat="server" meta:resourcekey="lblDeleteNicknameTextResource1"></asp:Label>
            </td>
        </tr>
        <tr>
            <td valign="top">
                <asp:Label ID="lblDeleteAddress" runat="server" Text="Address:" meta:resourcekey="lblDeleteAddressResource1"></asp:Label>
            </td>
            <td id="colDeletePickUp" runat="server" valign="top">
                <asp:Label ID="lblName" runat="server" meta:resourcekey="lblNameResource1"></asp:Label>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:" meta:resourcekey="lblDeleteIsPrimaryResource1"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteIsPrimaryText" runat="server" meta:resourcekey="lblDeleteIsPrimaryTextResource1"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <div class="gdo-error-message-div">
                    <img src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon"
                        alt="Error" /><span class="gdo-error-message-txt">
                            <asp:Label ID="lblDeleteError" runat="server" meta:resourcekey="lblErrorsResource1"></asp:Label></span></div>
            </td>
        </tr>
    </table>
</div>
<cc2:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked"
    Text="Cancel" ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelResource1" CssClass="backward"></cc2:OvalButton>
<cc2:OvalButton ID="btnContinue" runat="server" Coloring="Silver" OnClick="ContinueChanges_Clicked"
    Text="Continue" ArrowDirection="" IconPosition="" meta:resourcekey="btnContinueResource1" CssClass="forward"></cc2:OvalButton>
