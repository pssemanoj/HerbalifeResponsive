<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BOAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.BOAddressControl" %>
<%@ Register TagPrefix="asp" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls.Address" Assembly="MyHerbalife3.Ordering.Web" %>
<asp:UpdatePanel runat="server" ID="upAddressDialog" RenderMode="Inline">
    <ContentTemplate>
        <div id="gdo-popup-container">    
        <asp:Label runat="server" ID="lbName" Text="Nombre:" meta:resourcekey="lbRecipentResource"></asp:Label>
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

        <asp:Label runat="server" ID="lbStreet" Text="Calle 1*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
        <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        <asp:Label runat="server" ID="lbStreet2" Text="Calle 2:" meta:resourcekey="lbStreet2Resource"></asp:Label>
        <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40" CssClass="gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>
        <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
            <li><asp:Label runat="server" ID="lbState" Text="Departamento *:" meta:resourcekey="lbStateResource"></asp:Label>
                <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState"
                    onselectedindexchanged="dnlState_SelectedIndexChanged" meta:resourcekey="ddlStateResource">
                </asp:DropDownList>
            </li>
            <li>
                <asp:Label runat="server" ID="lbCounty" Text="Ciudad *:" meta:resourcekey="lbCountyResource"></asp:Label>
                <asp:DropDownListAttributes AutoPostBack="True" runat="server" ID="dnlCounty" meta:resourcekey="ddlCountyResource" 
                    OnSelectedIndexChanged="dnlCounty_SelectedIndexChanged"></asp:DropDownListAttributes>
            </li>
            <li><asp:Label runat="server" ID="lbCity" Text="Provincia *:" meta:resourcekey="lbCityResource"></asp:Label>
                <asp:TextBox runat="server" ID="txtCity" ReadOnly="true"></asp:TextBox>
            </li>            
        </ul>
        <ul>            
            <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number *:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
                <asp:TextBox runat="server" ID="txtNumber" MaxLength="10"  meta:resourcekey="txtNumberResource"></asp:TextBox>
                <span class="gdo-form-format">
                    <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Formato: 6-10 números sin espacio</asp:Localize>
                </span>
            </li>
        </ul>        
    </div>
    </ContentTemplate>
</asp:UpdatePanel>

