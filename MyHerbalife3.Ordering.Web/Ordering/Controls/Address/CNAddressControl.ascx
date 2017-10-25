<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CNAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.CNAddressControl" %>

    
<div class="gdo-form-label-left gdo-popup-form-label-padding2 col-md-12">
    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource" AssociatedControlID="txtCareOfName"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
        <ajaxToolkit:FilteredTextBoxExtender ID="fttxtCareOfName" runat="server" TargetControlID="txtCareOfName"
            FilterMode="InvalidChars" InvalidChars="0123456789" />
</div>
<div class="gdo-form-label-left gdo-popup-form-label-padding2 col-md-4">
    <asp:Label runat="server" ID="lbProvince" Text="State*:" meta:resourcekey="lbProvinceResource" AssociatedControlID="dnlProvince"></asp:Label>
    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlProvince" 
        meta:resourcekey="ddlProvinceResource"
        onselectedindexchanged="dnlProvince_SelectedIndexChanged" >
    </asp:DropDownList>
</div>

<div class="gdo-form-label-left gdo-popup-form-label-padding2 col-md-4">
    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource" AssociatedControlID="dnlCity"></asp:Label>
    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
            meta:resourcekey="ddlCityResource"
            onselectedindexchanged="dnlCity_SelectedIndexChanged">
        </asp:DropDownList>
</div>
<div class="gdo-form-label-left gdo-popup-form-label-padding2 col-md-4">
    <asp:Label runat="server" ID="lbDistrict" Text="Comuna/Localidad*:" meta:resourcekey="lbDistrictResource" AssociatedControlID="dnlDistrict"></asp:Label>
    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlDistrict"
            onselectedindexchanged="dnlDistrict_SelectedIndexChanged"  meta:resourcekey="ddlDistrictResource">
        </asp:DropDownList>
</div>

<div class="col-md-12">
    <asp:Label runat="server" ID="lblUnsupportedAddress" Text="" ForeColor="Red" ></asp:Label>
</div>
    
<div class="gdo-popup-form-field-padding street-address col-md-12">
    <asp:Label runat="server" ID="lbStreet1" Text="Street 1*:" meta:resourcekey="lbStreet1Resource" AssociatedControlID="txtStreet"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" class="gdo-address-input" ></asp:TextBox>
    <%--<asp:RegularExpressionValidator runat="server" ID="ChinaCharacterOnlyValidator" ControlToValidate="txtStreet" Display="Dynamic" meta:resourcekey="ChineseCharacterValidator" ValidationExpression="^[0-9\u4e00-\u9fa5 ]*$"></asp:RegularExpressionValidator>--%>
    <asp:TextBox runat="server" ID="txtStreetAddress4" MaxLength="40" class="gdo-address-input" Visible="false" ></asp:TextBox>
    <asp:TextBox runat="server" ID="txtStreetAddress3" class="gdo-address-input" Visible="false" />
    <asp:TextBox runat="server" ID="txtStreetAddress2" class="gdo-address-input" Visible="false" />
</div>
<div class="gdo-popup-form-field-padding col-md-2">
    <asp:Label runat="server" ID="lbPostCode" Text="Postal Code*:" meta:resourcekey="lbPostCodeResource" AssociatedControlID="txtPostCode"></asp:Label>
    <asp:TextBox runat="server" ID="txtPostCode" MaxLength="6" ></asp:TextBox>
    <span class="gdo-form-format">
        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PostalCodeFormat" Text="Format: 6 digits"></asp:Localize>
    </span>
</div>