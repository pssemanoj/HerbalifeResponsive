<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CRAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.CRAddressControl" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
        <div id="gdo-popup-container">
            <asp:Label runat="server" ID="lbName" Text="Nombre *:" meta:resourcekey="lbRecipentResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

            <asp:Label runat="server" ID="lbStreet" Text="Calle *:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
            <asp:Label runat="server" ID="lbStreet2" Text="Calle 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

            <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
                <li>
                    <asp:Label runat="server" ID="lbState" Text="Provincia *:" meta:resourcekey="lbStateResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlProvince"
                        OnSelectedIndexChanged="dnlProvince_SelectedIndexChanged" meta:resourcekey="ddlStateResource">
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="Cantón *:" meta:resourcekey="lbCityResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged"
                        meta:resourcekey="ddlCityResource">
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCounty" Text="Distrito *:" meta:resourcekey="lbCountyResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCounty" meta:resourcekey="ddlCountyResource"
                        OnSelectedIndexChanged="dnlCounty_SelectedIndexChanged">
                    </asp:DropDownList>
                </li>
            </ul>
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbPostal" Text="Código Postal:" meta:resourcekey="lbPostalResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtPostCode" MaxLength="5" AutoPostBack="True" Style='width: 150px' ReadOnly="True"
                        meta:resourcekey="txtPostCodeResource"></asp:TextBox>
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Formato: 5 números</asp:Localize>
                    </span>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Número de Teléfono *:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtNumber" MaxLength="8" meta:resourcekey="txtNumberResource"></asp:TextBox>
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Formato: 6-8 números</asp:Localize>
                    </span>
                </li>
            </ul>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
