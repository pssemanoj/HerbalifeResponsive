<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RSAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.RSAddressControl" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lblRecipent" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
        Text="Name*:" meta:resourcekey="lblRecipent"/>
    <asp:TextBox runat="server" ID="txtCareOfName" CssClass="gdo-popup-form-field-padding gdo-address-input"
        MaxLength="40" meta:resourcekey="txtCareOfNameResource1"/>
    <asp:Label runat="server" ID="lblStreet1" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
        Text="Street*:" meta:resourcekey="lblStreet1"/>
    <asp:TextBox runat="server" ID="txtStreet1" CssClass="gdo-popup-form-field-padding gdo-address-input"
        MaxLength="58" meta:resourcekey="txtStreet1Resource1"/>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li class="sameRow">
            <asp:Label runat="server" ID="lblCity" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
            Text="City*:" meta:resourcekey="lblCity"/>
            <telerik:RadComboBox runat="server" ID="dnlCity" AllowCustomText="false" MarkFirstMatch="true" AutoPostBack="true"
            EnableTextSelection="false" ShowToggleImage="false" MaxHeight="220" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged"
            Filter="Contains" />
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblPostalCode" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
            Text="Postal Code*:" meta:resourcekey="lblPostalCode"/>
            <asp:TextBox runat="server" ID="txtPostalCode" Enabled="False" 
            meta:resourcekey="txtPostalCodeResource1"/>
        </li>
        <li class="sameRow">
            <asp:Label runat="server" ID="lblPhoneNumber" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Phone Number*:" meta:resourcekey="lblPhoneNumber"/>
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="10" CssClass="gdo-popup-form-field-padding"
                meta:resourcekey="txtPhoneNumberResource1"></asp:TextBox>
        </li>
        <li class="sameRow">
            <span class="gdo-form-format">
                <asp:Label runat="server" ID="lblPhoneFormat" meta:resourcekey="lblPhoneFormat"
                Text="Area code 3 digits,number min. 6 digits, max. 7 digits. eg. 333666666 or 3337777777" />
            </span>
        </li>
    </ul>
</div>
