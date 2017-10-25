<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddEditShippingControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Shipping.AddEditShippingControl" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc3" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<asp:HiddenField ID="hfDiableSavedCheckbox" runat="server" />
<asp:Panel runat="server" ID="AddressControls" DefaultButton="btnContinue">
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>

<div id="divAddEditShippingAddress" runat="server">
    <table border="0" cellspacing="1" cellpadding="0">
        <tr>
            <td colspan="3">
                <div class="gdo-float-left gdo-popup-title">
                    <h2>
                        <asp:Label ID="lblHeader" runat="server" meta:resourcekey="lblHeaderResource1"></asp:Label></h2>
                    <h2>
                         <asp:Label ID="lblAddressRestrictionMsg" runat="server" Text="Order Should only be shipped to your Personal Address" meta:resourcekey="lblAddressrestrictionmsg" Visible="false" ></asp:Label></h2>
                        <cc3:ContentReader ID="ShippingMessageText" runat="server" Visible="true" ContentPath="ShippingMessage.html" UseLocal="true" ValidateContent="true"/>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="gdo-form-label-right">
                    <asp:Localize ID="Localize2" runat="server" meta:resourcekey="RequiredFieldFormat">*Required Field</asp:Localize></div>
            </td>
        </tr>
        <tr>
            <td id="colNewShippingAddress" class="gdo-popup-form-field-padding" runat="server" colspan="3">
            </td>
        </tr>
        <tr>
            <td class="gdo-popup-form-field-padding">
                <div class="col-md-4">
                    <asp:Label ID="lblNickname" runat="server" Text="Nickname:" meta:resourcekey="lblNicknameResource1" AssociatedControlID="txtNickname"></asp:Label>
                    <asp:TextBox ID="txtNickname" runat="server" MaxLength="40" CssClass="addressNicknameControl"
                        meta:resourcekey="txtNicknameResource1" TabIndex="16"></asp:TextBox>
                    <cc2:FilteredTextBoxExtender ID="fttxtNickname" runat="server" TargetControlID="txtNickname" 
                        FilterType="Custom" FilterMode="InvalidChars" InvalidChars="<>" />
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="3" class="gdo-popup-form-label-padding2">
                <div class="col-md-4">
                    <div class="chk-button">
                        <asp:CheckBox runat="server" ID="cbSaveThis" Text="Save this shipping address" OnCheckedChanged="cbSaveThis_CheckedChanged"
                        class="gdo-no-border" AutoPostBack="True" meta:resourcekey="cbSaveThisResource1"
                        TabIndex="17"></asp:CheckBox>
                    </div>
                    <div class="chk-button">
                        <asp:CheckBox runat="server" ID="cbMakePrimary" Text="Make this my primary shipping address"
                            class="gdo-no-border" meta:resourcekey="cbMakePrimaryResource1" TabIndex="18">
                        </asp:CheckBox>
                    </div>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="3" class="gdo-popup-form-label-padding2">
                <asp:Label Visible="false" class="gdo-no-border" runat="server" ID="lblHongkongAddtinalInfo"
                    Text="Please note that if orders are delivered to the locations below there will be an additional freight charge collected by<br />the shipping agents. Discovery Bay($270), Tung Chung($150), Ma Wan($150), Airport($150)"
                    meta:resourcekey="lblHongkongAddtinalInfoResource"></asp:Label>
                <asp:Label Visible="false" class="gdo-no-border gdo-error-message-div" runat="server" ID="lblNote" ForeColor="Red" Text="" meta:resourcekey="lblNoteResource"></asp:Label>
            </td>
        </tr>
    </table>
</div>
<div id="divDeleteShippingAddress" runat="server">
    <div class="gdo-popup-sm">
        <table border="0" cellspacing="2" cellpadding="0">
            <tr>
                <td colspan="2">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblDeleteHeader" runat="server" meta:resourcekey="lblDeleteHeaderResource1"></asp:Label></h2>
                    </div>
                </td>
            </tr>
            <tr>
                <td valign="top" class="gdo-form-label-right gdo-popup-form-label-padding">
                    <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:" meta:resourcekey="lblDeleteNicknameResource1"></asp:Label>
                </td>
                <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding">
                    <asp:Label ID="lblDeleteNicknameText" runat="server" meta:resourcekey="lblDeleteNicknameTextResource1"></asp:Label>
                </td>
            </tr>
            <tr>
                <td valign="top" class="gdo-form-label-right gdo-popup-form-label-padding">
                    <asp:Label ID="lblDeleteAddress" runat="server" Text="Address:" meta:resourcekey="lblDeleteAddressResource1"></asp:Label>
                </td>
                <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding" id="colDeleteShippingAddress"
                    runat="server">
                    <asp:Label ID="lblName" runat="server" meta:resourcekey="lblNameResource1"></asp:Label>
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td valign="top" class="gdo-form-label-right gdo-popup-form-label-padding">
                    <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:" meta:resourcekey="lblDeleteIsPrimaryResource1"></asp:Label>
                </td>
                <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding">
                    <asp:Label ID="lblDeleteIsPrimaryText" runat="server" meta:resourcekey="lblDeleteIsPrimaryTextResource1"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
</div>
<table border="0" cellspacing="1" cellpadding="0" class="newaddressbuttons">
    <tr id="trError" runat="server">
        <td>
            <div class="gdo-error-message-div">
                <img alt="errorIcon" src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon"
                    style="display: none" />
                <span class="gdo-error-message-txt">
                    <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"
                        meta:resourcekey="blErrorsResource1">
                    </asp:BulletedList>
                </span>
            </div>
        </td>
    </tr>
    <tr class="align-left">
        <td>
            <div class="gdo-popup-footer-btns align-left<%= (HLConfigManager.Configurations.DOConfiguration.IsChina) ? " is-china" : ""%>">
                <cc1:DynamicButton ID="btnCancel" runat="server" ButtonType="Back" Text="Cancel"
                    OnClick="CancelChanges_Clicked" meta:resourcekey="btnCancelResource1" TabIndex="19"></cc1:DynamicButton>
                <cc1:DynamicButton ID="btnContinue" runat="server" ButtonType="Forward" Text="Continue" OnClientClick="Submission();"
                    OnClick="ContinueChanges_Clicked" DisableOnClick="true" TabIndex="20" />
            </div>
        </td>
    </tr>
</table>
</asp:Panel>