<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TRAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.TRAddressControl" %>
<div id="gdo-popup-container">

    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 more-margin">
        <li>
            <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                meta:resourcekey="ddlCityResource" 
                onselectedindexchanged="dnlCity_SelectedIndexChanged">
            </asp:DropDownList>
        </li>

        <li>
            <asp:Label runat="server" ID="lbSuburb" Text="Suburb*:" meta:resourcekey="lbSuburbResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlSuburb" 
                meta:resourcekey="ddlSuburbResource"
                onselectedindexchanged="dnlSuburb_SelectedIndexChanged">
            </asp:DropDownList>
        </li>

        <li>
            <asp:Label runat="server" ID="lbDistrict" Text="District*:" meta:resourcekey="lbDistrictResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlDistrict" 
                meta:resourcekey="ddlDistrictResource"
                OnSelectedIndexChanged="dnlDistrict_SelectedIndexChanged">
            </asp:DropDownList>
        </li>
        <li class="last">
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalResource"></asp:Label>
            <asp:DropDownList AutoPostBack="false" runat="server" ID="dnlPostalCode" 
                meta:resourcekey="ddlPostCodeResource">
            </asp:DropDownList>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="5" AutoPostBack="True"
                meta:resourcekey="txtPostCodeResource" Visible="false"></asp:TextBox>
        </li>
    </ul>

    <asp:Label runat="server" ID="lbStreet" Text="Street 1*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40"  meta:resourcekey="txtStreet1Resource"></asp:TextBox>

    <asp:Label runat="server" ID="Label1" Text="Street 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="12" meta:resourcekey="txtNumberResource" ></asp:TextBox>
        </li>
        <li class="help-text two-blocks">
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 8-12 digits</asp:Localize>
            </span>
        </li>
    </ul>
</div>