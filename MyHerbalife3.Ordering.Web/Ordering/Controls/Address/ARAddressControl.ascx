<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ARAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ARAddressControl" %>
<div id="gdo-popup-container">

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 four-fields">
        <li class="full last">
            <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40"  meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        </li>

        <li class="group last">
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" 
                        meta:resourcekey="ddlStateResource" onselectedindexchanged="dnlState_SelectedIndexChanged">
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                        meta:resourcekey="dnlCityResource" onselectedindexchanged="dnlCity_SelectedIndexChanged"></asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCounty" Text="County:" meta:resourcekey="lbCountyResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" MaxLength="30" 
                        meta:resourcekey="dnlCountyResource" onselectedindexchanged="dnlCounty_SelectedIndexChanged"> </asp:DropDownList>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalResource"></asp:Label>
                    <asp:DropDownList runat="server" ID="dnlPostCode" meta:resourcekey="dnlPostCodeResource"></asp:DropDownList>
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 4 digits</asp:Localize>
                    </span>
                </li>
            </ul>
        </li>
        <li class="group last">
            <ul>
                <li class="the-moiety">
                    <asp:Label runat="server" ID="lblAreaCode" Text="Area Code*:" meta:resourcekey="lbAreaCodeResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="4" style='width: 80px !important;' meta:resourcekey="txtAreaCodeResource"></asp:TextBox>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource" ></asp:Label>
                    <asp:TextBox runat="server" ID="txtNumber" MaxLength="8" meta:resourcekey="txtNumberResource" ></asp:TextBox>           
                </li>
                <li class="last help-text two-blocks gdo-form-format">
                    <span>
                        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: Area code + phone 10 digits max</asp:Localize>
                    </span>
                </li>
            </ul>
        </li>
    </ul>    
</div>