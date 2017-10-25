<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PFAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PFAddressControl" %>

<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbName" Text="A l'attention de*:" meta:resourcekey="lbRecipentResource"/>		
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"/>

    <asp:Label runat="server" ID="lbStreet" Text="Rue*:" meta:resourcekey="lbStreet1Resource"/>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource"/>

    <asp:Label runat="server" ID="lbStreet2" Text="Rue 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">  
        <li><asp:Label runat="server" ID="lbCity" Text="Ville*:" meta:resourcekey="lbCityResource"/>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="20" meta:resourcekey="txtCityResource" OnTextChanged="txtCity_TextChanged" AutoPostBack="true" />
        </li>

        <li><asp:Label runat="server" ID="lbPostal" Text="Code Postal*:" meta:resourcekey="lbPostalResource"/>
            <asp:TextBox runat="server" ID="txtPostCode"  MaxLength="5" meta:resourcekey="txtPostCodeResource" OnTextChanged="txtPostCode_TextChanged" AutoPostBack="True"/>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" Text="Format: 5 chiffres" meta:resourcekey="PostalCodeFormat"/>
            </span>
        </li>
        <li>
            <asp:Label runat="server" ForeColor="Red" ID="lblNoMatch" Visible="false" Text="Zip Code entered not valid" meta:resourcekey="lblNoMatch"></asp:Label>
        </li>
    </ul>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">             
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Numéro de téléphone*:" meta:resourcekey="lbPhoneNumberResource"/><br />
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="12" meta:resourcekey="txtNumberResource"/>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" Text="Format: 10-12 chiffres" meta:resourcekey="PhoneNumberFormat"/>
            </span>
        </li>
    </ul>

</div>