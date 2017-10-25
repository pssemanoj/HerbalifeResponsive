<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PEAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PEAddressControl" %>

<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
    <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbState" Text="Province*:" meta:resourcekey="lbStateResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlProvince"
                onselectedindexchanged="dnlProvince_SelectedIndexChanged" meta:resourcekey="ddlStateResource">
            </asp:DropDownList>
        </li>
        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged"
                 meta:resourcekey="ddlCityResource">
            </asp:DropDownList>
        </li>
        <li>
            <asp:Label runat="server" ID="lbCounty" Text="County/Suburb*:" meta:resourcekey="lbCountyResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" meta:resourcekey="ddlCountyResource" OnSelectedIndexChanged="dnlCounty_SelectedIndexChanged"></asp:DropDownList>
        </li>
    </ul>
    <ul>
        <li>
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code:" meta:resourcekey="lbPostalResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="6" AutoPostBack="True" style='width: 150px' ReadOnly="True" meta:resourcekey="txtPostCodeResource"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 6 digits</asp:Localize>
            </span>
        </li>
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="10"  meta:resourcekey="txtNumberResource"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 6-10 digits</asp:Localize>
            </span>
        </li>
    </ul>

</div>
