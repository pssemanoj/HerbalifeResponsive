<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="INAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.INAddressControl" %>
<div id="gdo-popup-container">
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbFirstName" Text="First Name*:" meta:resourcekey="lbFirstNameResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtFirstName" MaxLength="18" meta:resourcekey="txtFirstNameResource1"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbLastName" Text="Last Name*:" meta:resourcekey="lbLastNameResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtLastName" MaxLength="18" meta:resourcekey="txtLastNameResource1"></asp:TextBox>
        </li>
    </ul>
            <asp:Label runat="server" ID="lbStreet1" Text="Street 1*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet1" MaxLength="40" meta:resourcekey="txtStreet1Resource1"></asp:TextBox>

            <asp:Label runat="server" ID="lbStreet2" Text="Street 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource1"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbState"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" meta:resourcekey="dnlStateResource1"></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCity"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" meta:resourcekey="dnlCityResource1"></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostal"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="6" meta:resourcekey="txtPostCodeResource1"></asp:TextBox>
        </li>
        <li class="help-text"><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">6 numbers no spaces</asp:Localize>
            </span>
        </li>
    </ul>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="Label1" Text="Area Code*:" meta:resourcekey="lbAreaCode"></asp:Label>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="5" meta:resourcekey="txtAreaCodeResource1"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="10" meta:resourcekey="txtNumberResource1"></asp:TextBox>
        </li>
        <li class="help-text"><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">8 numbers no spaces</asp:Localize>
            </span>
        </li>

        <li><asp:Label runat="server" ID="Label2" Text="Ext:" meta:resourcekey="lbExtension"></asp:Label>
            <asp:TextBox runat="server" ID="txtExtension" MaxLength="5" meta:resourcekey="txtExtensionResource1"></asp:TextBox>
        </li>
    </ul>

    <div id="labelIndiaShippingInfo">
        <asp:Label runat="server" meta:resourcekey="txtDisclaimerResource1" Text="Important Shipping Information:<br />Due to documentation requirements we are unable to accept online orders for the following states:<br />· Arunachal Pradesh, Bihar, Himachal Pradesh, Jammu & Kashmir, Jharkhand<br />· Meghalaya, Mizoram, Nagaland, Sikkim, Tripura" />
    </div>

</div>
