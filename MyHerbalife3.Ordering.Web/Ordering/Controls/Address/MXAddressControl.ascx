<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MXAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.MXAddressControl" %>
<div id="gdo-popup-container">
    <div class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbNombre" Text="Nombre *" meta:resourcekey="lbNombre"></asp:Label>
        <asp:TextBox runat="server" id="txtNombre" MaxLength="50" 
            meta:resourcekey="txtNombreResource1"></asp:TextBox>

        <asp:Label runat="server" ID="lbDirecton" MaxLength="40" Text="Dirección de calle *" meta:resourcekey="lbDirectonResource1"></asp:Label>
        <asp:TextBox runat="server" ID="tbDir" MaxLength="40" meta:resourcekey="tbPostalCodeResource1"></asp:TextBox>
   
    </div>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbState" Text="Estado&nbsp;&nbsp;*" meta:resourcekey="lbStateResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" OnDataBound="dnlState_DataBound"
                meta:resourcekey="dnlState">
            </asp:DropDownList>
        </li>

        <li>
            <asp:Label runat="server" ID="lnMunicipal" Text="Municipio *" meta:resourcekey="lnMunicipalResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlMunicipal" OnSelectedIndexChanged="dnlMunicipal_SelectedIndexChanged" OnDataBound="dnlMunicipal_DataBound"
                meta:resourcekey="dnlMunicipal">
            </asp:DropDownList>
        </li>
 
        <li>
            <asp:Label runat="server" ID="lbColonia" Text="Colonia&nbsp;&nbsp;*" meta:resourcekey="lbColoniaResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlTown" OnSelectedIndexChanged="dnlTown_SelectedIndexChanged" meta:resourcekey="dnlTown">
            </asp:DropDownList>
        </li>
    </ul>
    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbNumber" Text="Número de teléfono *" meta:resourcekey="lbNumber"></asp:Label>
            <asp:TextBox runat="server" id="txtNumber" meta:resourcekey="txtNumberResource1" MaxLength="10"></asp:TextBox>
        </li>
        <li>
            <asp:Label runat="server" ID="lbPostal" Text="Código postal *" meta:resourcekey="lbPostalResource1"></asp:Label>
            <asp:TextBox runat="server" ID="tbPostalCode" MaxLength="5" Enabled="false" meta:resourcekey="tbPostalCodeResource1"></asp:TextBox>
        </li>
    </ul>
        
    <asp:Label runat="server" id="lbError" ForeColor="Red" 
                meta:resourcekey="lbErrorResource1"></asp:Label>
</div>
