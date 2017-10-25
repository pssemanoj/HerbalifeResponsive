<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PAAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PAAddressControl" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
        <div id="gdo-popup-container">
            <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>
            <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
            <asp:Label runat="server" ID="lbStreet2" Text="Street 2*:" meta:resourcekey="lbStreet2Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>
            <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
                <li>
                    <asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlProvince"
                        OnSelectedIndexChanged="dnlProvince_SelectedIndexChanged" meta:resourcekey="ddlStateResource">
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged"
                        meta:resourcekey="ddlCityResource">
                    </asp:DropDownList>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCounty" Text="County*:" meta:resourcekey="lbCountyResource"></asp:Label>
                    <asp:DropDownList AutoPostBack="False" runat="server" ID="dnlCounty" meta:resourcekey="ddlCountyResource" />
                </li>
            </ul>
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
                    <asp:TextBox runat="server" ID="txtNumber" MaxLength="8" meta:resourcekey="txtNumberResource"></asp:TextBox>
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 7-8 digits</asp:Localize>
                    </span>
                </li>
            </ul>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
