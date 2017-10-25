<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="COAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.COAddressControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<div id="gdo-popup-container">

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 three-fields">
        <li class="full last">
            <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource" CssClass="inline"></asp:Label>
            <a href="javascript:;" class="toggler-abbr">
                <asp:Label runat="server" ID="lbNomenclatureDIAN" Text="NomenclatureDIAN" meta:resourcekey="lbNomenclatureDIANResource" CssClass="inline"></asp:Label>            
            </a>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" class="gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>
        </li>

        <li class="group last togglable-abbr" style="display: none;">
            <cc1:ContentReader ID="crAddressAbbreaviation" runat="server" ContentPath="address-abbreviation.html" SectionName="ordering" ValidateContent="true" UseLocal="true" />
        </li>

        <li class="group last">
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" meta:resourcekey="ddlStateResource"
                        onselectedindexchanged="dnlState_SelectedIndexChanged" >
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" meta:resourcekey="ddlCityResource" onselectedindexchanged="dnlCity_SelectedIndexChanged">
                    </asp:DropDownList>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbCounty" Text="County:" meta:resourcekey="lbCountyResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" meta:resourcekey="ddlCountyResource">
                    </asp:DropDownList>
                </li>
            </ul>
        </li>
        
        <li class="group last">
            <ul>
                <li class="last">
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtNumber" MaxLength="10" meta:resourcekey="txtNumberResource"></asp:TextBox>
                </li>
                <li class="help-text">
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 6-10 digits</asp:Localize>
                    </span>
                </li>
            </ul>
        </li>
    </ul>
</div>