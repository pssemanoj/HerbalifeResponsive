<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PYAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PYAddressControl" %>
<div id="gdo-popup-container">
    <div class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbName" 
            Text="Name*:" meta:resourcekey="lbRecipentResource"/>
        <asp:TextBox runat="server" ID="txtCareOfName" 
            MaxLength="40" meta:resourcekey="txtCareOfNameResource"/>
    
        <asp:Label runat="server" ID="lbStreet" 
            Text="Street*:" meta:resourcekey="lbStreet1Resource"/>
        <asp:TextBox runat="server" ID="txtStreet" 
            MaxLength="40" meta:resourcekey="txtStreet1Resource"/>
    
        <asp:Label runat="server" ID="lbStreet2" 
            Text="Street2:" meta:resourcekey="lbStreet2Resource"/>
        <asp:TextBox runat="server" ID="txtStreet2" 
            MaxLength="40" meta:resourcekey="txtStreet2Resource"/>
    </div>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li class="sameRow">
        <asp:Label runat="server" ID="lbState" 
            Text="State*:" meta:resourcekey="lbStateResource"/>
        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" meta:resourcekey="ddlStateResource"
            onselectedindexchanged="dnlState_SelectedIndexChanged" />
        </li>
        <li class="sameRow">
        <asp:Label runat="server" ID="lbCity" 
            Text="City*:" meta:resourcekey="lbCityResource"/>
        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" meta:resourcekey="ddlCityResource"  onselectedindexchanged="dnlCity_SelectedIndexChanged"/>
        </li>
        <li class="sameRow">
            <asp:Label runat="server" ID="lbPostal" 
            Text="Postal Code:" meta:resourcekey="lbPostalResource"/>
            <asp:DropDownList runat="server" ID="dnlPostCode" AutoPostBack="False" meta:resourcekey="txtPostCodeResource" />
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" Text="Format: 4 digits" runat="server" meta:resourcekey="PostalCodeFormatPY"/>
            </span>
        </li>
        <li class="sameRow">
            <asp:Label runat="server" ID="lbPhoneNumber" 
            Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"/>
            <asp:TextBox runat="server" ID="txtNumber" 
            MaxLength="10" meta:resourcekey="txtNumberResource"/>
                <span class="gdo-form-format">
                    <asp:Localize ID="Localize2" Text="Format: 6-10 digits" runat="server" meta:resourcekey="PhoneNumberFormatPY"/>
            </span>
        </li>
    </ul>

</div>