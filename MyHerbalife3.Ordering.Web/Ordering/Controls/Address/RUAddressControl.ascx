<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RUAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.RUAddressControl" %>
<div id="gdo-popup-container">
        <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code:" meta:resourcekey="lbPostalCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="6" AutoPostBack="True" meta:resourcekey="txtPostalCodeResource" ontextchanged="txtPostCode_TextChanged"></asp:TextBox>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 6 digits</asp:Localize>
            </span>
        </li>
        
        <li><asp:Label runat="server" ID="lbState" Text="Province*:" meta:resourcekey="lbStateResource"></asp:Label>
            <asp:DropDownList AutoPostBack="False" runat="server" ID="dnlState" meta:resourcekey="txtStateResource"></asp:DropDownList>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbArea" Text="Area:" meta:resourcekey="lbCountyResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtArea" MaxLength="20" meta:resourcekey="txtCountyResource"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="25" meta:resourcekey="txtCityResource"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="25" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbStreet2" Text="Building/Apartment:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="25" meta:resourcekey="txtStreet2Resource"></asp:TextBox>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="15" meta:resourcekey="txtPhoneNumberResource"></asp:TextBox>
        <li><span class="gdo-form-format">
            <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 15 digits</asp:Localize>
            </span>
        </li>
    </ul>

        <asp:HyperLink runat="server" ID="lnkZipCodeValidationSite" Text="Postal code validation site" NavigateUrl="http://www.ruspostindex.ru/"
          meta:resourcekey="lnkZipCodeValidationSiteResource" Target="_blank"></asp:HyperLink><br/>
        <span class="gdo-form-format">
            <asp:Localize ID="Localize3" runat="server" meta:resourcekey="StaticMessage">We don’t take any responsibility for the entry of an incorrect address which can cause delays in receiving of the products.</asp:Localize>
        </span>

</div>