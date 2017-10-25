<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DOAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.DOAddressControl" %>

<div id="gdo-popup-container">
    <div class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbName" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Name*:" meta:resourcekey="lbRecipentResource"/><br />
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"/><br />			
        <ajaxToolkit:FilteredTextBoxExtender ID="ftCareOfName" runat="server" TargetControlID="txtCareOfName" FilterType="LowercaseLetters, UppercaseLetters, Custom" ValidChars=" ÑñÁÉÍÓÚáéíóú'"/>

        <asp:Label runat="server" ID="lbStreet"  Text="Street*:" meta:resourcekey="lbStreet1Resource"/><br />
        <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource"/><br />        

        <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"/><br />
        <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"/>
    </div>
                
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbProvince" Text="Province*:" meta:resourcekey="lbProvinceResource"/>
            <asp:DropDownList runat="server" ID="dnlProvince" AutoPostBack="True" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlProvinceResource" OnSelectedIndexChanged="dnlProvince_SelectedIndexChanged" />
        </li>
        
        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"/>
            <asp:DropDownList runat="server" ID="dnlState" AutoPostBack="True" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlStateResource" />
        </li>        
    </ul>    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPostal" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Postal Code:" meta:resourcekey="lbPostalResource"/>
            <asp:TextBox runat="server" ID="txtPostCode" CssClass="gdo-popup-form-field-padding" MaxLength="5" meta:resourcekey="txtPostCodeResource"/>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" Text="Format: 5 digits" meta:resourcekey="PostalCodeFormat"/>
            </span>
        </li>
        <li>            
            <asp:Label runat="server" ID="lbPhoneNumber" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"/>
            <asp:TextBox runat="server" ID="txtNumber" CssClass="gdo-popup-form-field-padding" MaxLength="10" meta:resourcekey="txtNumberResource"/>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" Text="Format: Min 8 Max 10 digits" meta:resourcekey="PhoneNumberFormat"/>
            </span>
        </li>
    </ul>
</div>