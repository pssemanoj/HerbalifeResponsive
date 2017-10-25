<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BYAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.BYAddressControl" %>
<div id="gdo-popup-container">

    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="6" AutoPostBack="True"
                meta:resourcekey="txtPostalCodeResource" ></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 6 digits</asp:Localize>
            </span>
        </li>
        <li><asp:HyperLink runat="server" ID="lnkZipCodeValidationSite" Text="Postal code validation site" NavigateUrl="http://zip.belpost.by/"
            CssClass="gdo-popup-form-label-padding2"  meta:resourcekey="lnkZipCodeValidationSiteResource" Target="_blank"></asp:HyperLink> 
        </li>
    </ul>
    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbState" Text="Region:" meta:resourcekey="lbStateResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtState" MaxLength="30" meta:resourcekey="txtStateResource"></asp:TextBox> 
        </li>           

        <li><asp:Label runat="server" ID="lbArea" Text="Area*:" meta:resourcekey="lbCountyResource"></asp:Label>
            <asp:DropDownList AutoPostBack="False" runat="server" ID="dnlCounty" meta:resourcekey="dnlCountyResource"></asp:DropDownList>     
        </li>       

        <li><asp:Label runat="server" ID="lbCity" Text="City*:"  meta:resourcekey="lbCityResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="20"  meta:resourcekey="txtCityResource"></asp:TextBox>
        </li>
    </ul>

    <asp:Label runat="server" ID="lbStreet" Text="Street*:" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="60" CssClass="gdo-popup-form-field-padding gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="15" style="width: 150px"  CssClass="gdo-popup-form-field-padding gdo-address-input" meta:resourcekey="txtPhoneNumberResource"></asp:TextBox>
        </li>

        <li><span class="gdo-form-format">
            <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 10-15 digits</asp:Localize>
            </span>
        </li>
   </ul>
</div>