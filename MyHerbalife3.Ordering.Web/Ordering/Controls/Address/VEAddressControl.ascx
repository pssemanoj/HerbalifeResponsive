<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="VEAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.VEAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

        <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"></asp:TextBox>
    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" 
                meta:resourcekey="ddlStateResource"
                onselectedindexchanged="dnlState_SelectedIndexChanged" >
            </asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity"  
                meta:resourcekey="ddlCityResource" onselectedindexchanged="dnlCity_SelectedIndexChanged">
            </asp:DropDownList>
        </li>
         
        <li><asp:Label runat="server" ID="lbCounty" Text="Comuna/Localidad*:" meta:resourcekey="lbCountyResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" 
                        meta:resourcekey="ddlCountyResource" 
                        onselectedindexchanged="dnlCounty_SelectedIndexChanged">
            </asp:DropDownList>
        </li>
   
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalResource"></asp:Label>
            <asp:DropDownList runat="server" ID="dnlPostCode" AutoPostBack="True"
                meta:resourcekey="txtPostCodeResource"></asp:DropDownList>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 4 digits</asp:Localize>
            </span>
        </li>

        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="11" meta:resourcekey="txtNumberResource"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 6-10 digits</asp:Localize>
            </span>
        </li>
    </ul>
</div>