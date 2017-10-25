<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HRAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.HRAddressControl" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<div id="gdo-popup-container">
        <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbNameResource1" />
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource1" />
        <ajaxToolkit:FilteredTextBoxExtender ID="ftCareOfName" runat="server" TargetControlID="txtCareOfName" FilterMode="InvalidChars" InvalidChars="0123456789" Enabled="True"/>

        <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreetResource1" />
        <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreetResource1" />

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li runat="server" id="divHRCity" class="sameRow">
            <asp:Label runat="server" ID="lbCity" Text="City:" meta:resourcekey="lbCityResource1" />
            <telerik:RadComboBox runat="server" ID="dnlCity" AutoPostBack="True"  OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" MarkFirstMatch="true" AllowCustomText="True" MaxHeight="220" />
        </li>

        <li runat="server" id="divHRPostal" class="sameRow">
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code:" meta:resourcekey="lbPostalResource1" />
            <asp:TextBox runat="server" Enabled="False" ID="txtPostCode" MaxLength="5" meta:resourcekey="txtPostCodeResource1" />
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource1" />
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="12" meta:resourcekey="txtNumberResource1" />
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" Text="Format: Min 11 Max 12 digits" meta:resourcekey="Localize2Resource1" />
            </span>
        </li>

    </ul>
</div>
