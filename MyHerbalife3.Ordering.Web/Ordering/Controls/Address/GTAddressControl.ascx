<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GTAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.GTAddressControl" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
        <div id="gdo-popup-container">
        <div>
            <asp:Label runat="server" ID="lbName" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Care of Name*:" 
                meta:resourcekey="lbRecipentResource"/>
		    <asp:TextBox runat="server" ID="txtCareOfName" CssClass="gdo-popup-form-field-padding gdo-address-input" MaxLength="40" 
                meta:resourcekey="txtCareOfNameResource"/>			
            <ajaxToolkit:FilteredTextBoxExtender ID="ftCareOfName" runat="server" TargetControlID="txtCareOfName" FilterType="LowercaseLetters, UppercaseLetters, Custom" ValidChars=" ÑñÁÉÍÓÚáéíóú'"/>

            <asp:Label runat="server" ID="lbStreet" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Street*:" meta:resourcekey="lbStreet1Resource"/>
            <asp:TextBox runat="server" ID="txtStreet" CssClass="gdo-popup-form-field-padding gdo-address-input" MaxLength="40" meta:resourcekey="txtStreet1Resource"/>     

            <asp:Label runat="server" ID="lbStreet2" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Street2:" meta:resourcekey="lbStreet2Resource"/>
            <asp:TextBox runat="server" ID="txtStreet2" CssClass="gdo-popup-form-field-padding gdo-address-input" MaxLength="40" meta:resourcekey="txtStreet2Resource"/>      	
	    </div>
            
        <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
            <li>
                <asp:Label runat="server" ID="lbState" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="State*:" meta:resourcekey="lbStateResource"/>
                <asp:DropDownList runat="server" ID="dnlState" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlStateResource" AutoPostBack="true" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" />
            </li>
            <li>
                <asp:Label runat="server" ID="lbCity" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="City*:" meta:resourcekey="lbCityResource"/>
                <asp:DropDownList runat="server" ID="dnlCity" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlCity" AutoPostBack="true" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" />
            </li>
            <li>
                <asp:Label runat="server" ID="lbZone" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Zone:" meta:resourcekey="lbZoneResource"/>
                <asp:DropDownList runat="server" ID="dnlZone" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlZoneResource" />
            </li>
        </ul>
        <ul class="gdo-form-label-left gdo-popup-form-label-padding2">                           
             <li class="sameRow">         
                <asp:Label runat="server" ID="lbPhoneNumber" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"/>
                <asp:TextBox runat="server" ID="txtPhoneNumber" CssClass="gdo-popup-form-field-padding" MaxLength="8" meta:resourcekey="txtNumberResource"/>
                <span class="gdo-form-format">
                    <asp:Localize ID="Localize2" runat="server" Text="Format: 8 digits" meta:resourcekey="PhoneNumberFormat" />
                </span>
            </li>
        </ul>
    </div>
    </ContentTemplate>
</asp:UpdatePanel>