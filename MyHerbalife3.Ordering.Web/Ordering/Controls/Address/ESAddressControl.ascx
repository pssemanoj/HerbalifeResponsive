<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ESAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ESAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbFirstName" Text="Name*:" meta:resourcekey="lbRecipent"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="35" meta:resourcekey="txtCareOfNameResource1"></asp:TextBox>
            
    <asp:Label runat="server" ID="lbStreet1" Text="Street 1*:" meta:resourcekey="lbStreet1Resource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet1" MaxLength="35" meta:resourcekey="txtStreet1Resource1"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li class="sameRow"><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostal"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="5" ontextchanged="txtPostCode_TextChanged" AutoPostBack="True" meta:resourcekey="txtPostCodeResource1"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 31671</asp:Localize>
            </span>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCity"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" meta:resourcekey="dnlCityResource1"></asp:DropDownList>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbState"></asp:Label>
            <asp:TextBox runat="server" ID="txtState" MaxLength="20" AutoPostBack="True" meta:resourcekey="txtStateResource1"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="9" meta:resourcekey="txtNumberResource1"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 9 numbers no spaces</asp:Localize>
            </span>
        </li>
    </ul>
</div>