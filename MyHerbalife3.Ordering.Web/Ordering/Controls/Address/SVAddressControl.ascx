<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SVAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.SVAddressControl" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
        <div id="gdo-popup-container">
            <asp:Label runat="server" ID="lbName" Text="Care of Name*:" meta:resourcekey="lbRecipentResource" />
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource" />
            <ajaxToolkit:FilteredTextBoxExtender ID="ftCareOfName" runat="server" TargetControlID="txtCareOfName" FilterType="LowercaseLetters, UppercaseLetters, Custom" ValidChars=" ÑñÁÉÍÓÚáéíóú'" />

            <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource" />
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" meta:resourcekey="txtStreet1Resource" />

            <asp:Label runat="server" ID="lbStreet2" Text="Street2:" meta:resourcekey="lbStreet2Resource" />
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" meta:resourcekey="txtStreet2Resource" />

            <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
                <li class="sameRow">
                    <asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource" />
                    <asp:DropDownList runat="server" ID="dnlState" AutoPostBack="True" CssClass="gdo-popup-form-field-padding" meta:resourcekey="ddlStateResource" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" />
                </li>

                <li class="sameRow">
                    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource" />
                    <asp:DropDownList runat="server" ID="dnlCity" meta:resourcekey="ddlCityResource" />
                </li>

                <li class="sameRow">
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource" />
                    <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="8" meta:resourcekey="txtNumberResource" />
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize2" runat="server" Text="Format: 8 digits" meta:resourcekey="PhoneNumberFormat" />
                    </span>
                </li>
            </ul>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
