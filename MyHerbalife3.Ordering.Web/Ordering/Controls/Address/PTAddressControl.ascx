<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PTAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PTAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbFirstName" Text="Name*:" meta:resourcekey="lbFirstNameResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource1"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet" Text="Street 1*:" meta:resourcekey="lbStreetResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreetResource1"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet2" Text="Street 2:" meta:resourcekey="lbStreet2Resource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource1"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">    
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalResource1"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="8" OnTextChanged="txtPostCode_TextChanged"
                AutoPostBack="True"  meta:resourcekey="txtPostCodeResource1"></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" Text="Format: 3200-335" meta:resourcekey="Localize1Resource1"></asp:Localize>
            </span>
        </li>
        <li>
            <p runat="server" style="color:Red" id="PostalCodeHelp" visible="false" meta:resourcekey="PostalCodeHelp"></p>
        </li>
        
        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" meta:resourcekey="dnlCityResource1"></asp:DropDownList>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">    
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource1"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="9" Width="147px" meta:resourcekey="txtNumberResource1"></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" Text="Format: 9 numbers no spaces" meta:resourcekey="Localize2Resource1"></asp:Localize>
            </span>
        </li>
    </ul>
</div>