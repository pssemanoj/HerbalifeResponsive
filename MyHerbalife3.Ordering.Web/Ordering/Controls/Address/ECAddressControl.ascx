<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ECAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ECAddressControl" %>

<div id="gdo-popup-container">
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 four-fields">
        <li class="full last">
            <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40"  meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbStreet1" Text="Street 1*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input"  meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbStreet2" Text="Street 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" class="gdo-address-input"  meta:resourcekey="txtStreet2Resource"></asp:TextBox>
        </li>

        <li class="group last">
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" 
                        meta:resourcekey="ddlStateResource"
                        onselectedindexchanged="dnlState_SelectedIndexChanged" >
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                        meta:resourcekey="ddlCityResource"
                        onselectedindexchanged="dnlCity_SelectedIndexChanged">
                    </asp:DropDownList>
                </li>

                <li>
                    <asp:Label runat="server" ID="lbCounty" Text="Comuna/Localidad*:" meta:resourcekey="lbCountyResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" 
                        meta:resourcekey="ddlCountyResource" 
                        onselectedindexchanged="dnlCounty_SelectedIndexChanged">
                    </asp:DropDownList>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbPostalCode" Text="Postal Code*:" meta:resourcekey="lbPostalCodeResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlPostalCode" meta:resourcekey="dnltPostalCodeResource">
                    </asp:DropDownList>
                </li>
            </ul>
        </li>
        
        <li class="group last">
            <ul>
                <li class="last">
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtNumber" MaxLength="12" meta:resourcekey="txtNumberResource"></asp:TextBox>
                </li>
                <li class="help-text gdo-form-format">
                    <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 1-12 digits</asp:Localize>
                </li>
            </ul>
        </li>
    </ul>
</div>