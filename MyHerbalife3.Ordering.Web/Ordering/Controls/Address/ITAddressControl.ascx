<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ITAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ITAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbFirstName" Text="Name*:" meta:resourcekey="lbRecipent"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource1" ></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreetResource"></asp:Label>        
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreetResource1"></asp:TextBox>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostal"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="5" ontextchanged="txtPostCode_TextChanged" AutoPostBack="True" meta:resourcekey="txtPostCodeResource1"></asp:TextBox>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 00010</asp:Localize>
            </span>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCity"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" meta:resourcekey="dnlCityResource1" >  </asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbState" Text="Province*:" meta:resourcekey="lbState"></asp:Label>
            <asp:TextBox runat="server" ID="txtState" MaxLength="6" ReadOnly="True"  meta:resourcekey="txtStateResource1"></asp:TextBox>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="11" meta:resourcekey="txtNumberResource1"></asp:TextBox>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">11 numbers no spaces</asp:Localize>
            </span>
        </li>
    </ul>
</div>

        
