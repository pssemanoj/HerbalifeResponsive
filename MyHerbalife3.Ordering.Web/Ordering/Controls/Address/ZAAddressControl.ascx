<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ZAAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ZAAddressControl" %>

<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbFirstName" Text="Name*:" meta:resourcekey="lbFirstNameResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource1"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet" Text="Street 1*:" meta:resourcekey="lbStreetResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreetResource1" ></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet2" Text="Street 2:" meta:resourcekey="lbStreet2Resource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource1" ></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbProvince" Text="Province*:" meta:resourcekey="lbProvinceResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlProvince" meta:resourcekey="ddlProvinceResource1" 
                onselectedindexchanged="dnlProvince_SelectedIndexChanged" >
            </asp:DropDownList>
        </li>

        <li>
            <asp:Label runat="server" ID="lbSuburb" Text="Suburb*:" meta:resourcekey="lbSuburbResource1"></asp:Label>
            <telerik:RadComboBox runat="server" ID="dnlSuburb" AllowCustomText="false" MarkFirstMatch="true" 
                AutoPostBack="true" EnableTextSelection="true" ShowDropDownOnTextboxClick="true" Filter="StartsWith" 
                ChangeTextOnKeyBoardNavigation="true" Height="220px"/>
        </li>

        <li>
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlPostalCode" meta:resourcekey="ddlPostalCodeResource1">
            </asp:DropDownList>
        </li>
    </ul>

    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtNumber" MaxLength="11" Width="147px" meta:resourcekey="txtNumberResource1"></asp:TextBox>
    <span class="gdo-form-format">
        <asp:Localize ID="lbPhoneNumberFormat" runat="server" Text="Format: 9-11 numbers no spaces" meta:resourcekey="lbPhoneNumberFormatResource1"></asp:Localize>
    </span>
    <br />
    <p class="gdo-form-format" style="text-align:left;">
        <asp:Localize ID="lbNotes" runat="server" Text="" meta:resourcekey="lbAddressControlNotes"></asp:Localize>
    </p>
</div>