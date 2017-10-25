<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MKAddressControl.ascx.cs" 
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.MKAddressControl" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<div id="gdo-popup-container">

    <asp:Label runat="server" ID="lblRecipent" Text="Care of Name*:" meta:resourcekey="lblRecipent"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource1"></asp:TextBox>

    <asp:Label runat="server" ID="lblStreet1" Text="Address*:" meta:resourcekey="lblStreet1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet1" MaxLength="58" meta:resourcekey="txtStreet1Resource1"></asp:TextBox>

    <asp:Label runat="server" ID="lblStreet2" Text="Address 2*:" meta:resourcekey="lblStreet2"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="58" meta:resourcekey="txtStreet2Resource1"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">  
        <li class="sameRow"><asp:Label runat="server" ID="lblPostalCode" Text="Zip Code*:" meta:resourcekey="lblPostalCode"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostalCode" Enabled="False" MaxLength="4"
                meta:resourcekey="txtPostCodeResource1"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblCity" Text="City*:" meta:resourcekey="lblCity"></asp:Label>
            <telerik:RadComboBox runat="server" MaxLength="30" ID="dnlCity" AllowCustomText="false" MarkFirstMatch="true" AutoPostBack="true"
                EnableTextSelection="true" ShowToggleImage="false" MaxHeight="220" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged"
                Filter="Contains" ></telerik:RadComboBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblPhoneNumber" Text="Phone Number*:" meta:resourcekey="lblPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="10" meta:resourcekey="txtPhoneNumberResource1"></asp:TextBox>
        </li>

        <li class="sameRow"><span class="gdo-form-format">
                <asp:Label runat="server" ID="lblPhoneFormat" Text="Area code min. 2 digits max. 3 digits,number min. 6 digits, max. 7 digits. eg. 33666666 or 3337777777" meta:resourcekey="lblPhoneFormat"></asp:Label>
            </span>
        </li>       
    </ul>
</div>