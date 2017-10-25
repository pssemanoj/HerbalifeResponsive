<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HNAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.HNAddressControl" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
    <div id="gdo-popup-container">
        <div class="gdo-form-label-left gdo-popup-form-label-padding2">
            <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"/>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"/>
            <ajaxToolkit:FilteredTextBoxExtender ID="ftCareOfName" runat="server" TargetControlID="txtCareOfName"
                FilterType="LowercaseLetters, UppercaseLetters, Custom" ValidChars=" ÑñÁÉÍÓÚáéíóú'"/>

            <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"/>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource"/>
            <ajaxToolkit:FilteredTextBoxExtender ID="ftStreet1" runat="server" TargetControlID="txtStreet"
                FilterType="LowercaseLetters, UppercaseLetters, Custom" ValidChars=" ÑñÁÉÍÓÚáéíóú'"/>

            <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource"/>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource"/>
        </div>
        <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
            <li class="sameRow">
                <asp:Label runat="server" ID="lbState" Text="State:" meta:resourcekey="lbStateResource"/>
                <asp:DropDownList runat="server" ID="dnlState" AutoPostBack="True" onselectedindexchanged="dnlState_SelectedIndexChanged" meta:resourcekey="ddlStateResource"/>
            </li>
            <li class="sameRow">
                <asp:Label runat="server" ID="lbCity2" Text="City:" meta:resourcekey="lbCityResource"/>
                <asp:DropDownList runat="server" ID="dnlCity" AutoPostBack="true" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" meta:resourcekey="ddlCityResource" />
            </li>
            <li class="sameRow">
                <asp:Label runat="server" ID="lbVillage" Text="Village:" meta:resourcekey="lbVillageResource"/>
                <asp:DropDownList runat="server" ID="dnlVillage" AutoPostBack="True" meta:resourcekey="ddlVillageResource" />
            </li>
        </ul>
        <ul class="gdo-form-label-left gdo-popup-form-label-padding2">      
            <li>
                <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"/>
                <asp:TextBox runat="server" ID="txtNumber" MaxLength="12" meta:resourcekey="txtNumberResource"/>
                <span class="gdo-form-format">
                    <asp:Localize ID="Localize2" runat="server" Text="Format: Min 11 Max 12 digits" meta:resourcekey="PhoneNumberFormat"/>
                </span>
            </li>
        </ul>
    </div>
    </ContentTemplate>
</asp:UpdatePanel>