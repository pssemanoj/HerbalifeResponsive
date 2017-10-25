<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="IDAddressControl.ascx.cs" 
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.IDAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbRecipent" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet1" Text="Street1*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet1"  MaxLength="40" meta:resourcekey="txtStreet1Resource"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" meta:resourcekey="dnlStateResource"></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" meta:resourcekey="dnlCityResource" ></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbPostalCode" Text="Postal code*:" meta:resourcekey="lbPostalCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostalCode" MaxLength="5" meta:resourcekey="txtPostalCodeResource"></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 5 digits</asp:Localize>
            </span>
        </li>
    </ul>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">           
        <li><asp:Label runat="server" ID="lbAreaCode" Text="Area Code*:" meta:resourcekey="lbAreaCode"></asp:Label>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize3" runat="server" meta:resourcekey="AreaCodeFormat">Format: 2-4 digits</asp:Localize>
            </span>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="4" Width="70px" meta:resourcekey="txtAreaCodeResource1"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="10" meta:resourcekey="txtNumberResource1"></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 5-10 digits</asp:Localize>
            </span>
        </li>
    </ul>
</div>
