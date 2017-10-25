<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UAAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.UAAddressControl" %>
        
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lblRecipent" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
        Text="Name*:" meta:resourcekey="lblRecipent"/>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="50" 
        meta:resourcekey="txtCareOfName" CssClass="gdo-popup-form-field-padding gdo-address-input"/>
    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li class="sameRow">
            <asp:Label runat="server" ID="lblState" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Region 1*:" meta:resourcekey="lblState"/>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" 
                OnSelectedIndexChanged="dnlState_SelectedIndexChanged" 
                meta:resourcekey="dnlStateResource1"/>
        </li>

        <li class="sameRow">
             <asp:Label runat="server" ID="lblCity" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="City*:" meta:resourcekey="lblCity"/>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" 
                meta:resourcekey="dnlCityResource1"/>
        </li>
   
        <li class="sameRow">
            <asp:Label runat="server" ID="lblRegion" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Region 2:" meta:resourcekey="lblRegion"/>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlRegion" 
                OnSelectedIndexChanged="dnlRegion_SelectedIndexChanged" 
                meta:resourcekey="dnlRegionResource1"/>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblPostalCode" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Postal Code*:" meta:resourcekey="lblPostalCode"/>
            <asp:DropDownList AutoPostBack="false" runat="server" ID="dnlPostalCode" 
                meta:resourcekey="dnlPostalCodeResource1"/>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblStreet" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Street*:" meta:resourcekey="lblStreet"/>
            <asp:TextBox runat="server" ID="txtStreet"  MaxLength="60" CssClass="gdo-popup-form-field-padding gdo-address-input"
                meta:resourcekey="txtStreetResource1"></asp:TextBox>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblBuilding1" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Building 1*:" meta:resourcekey="lblBuilding1"/>
            <asp:TextBox runat="server" ID="txtBuilding1" MaxLength="10" CssClass="gdo-popup-form-field-padding"
                meta:resourcekey="txtBuilding1Resource1"/>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblBuilding2" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Building 2:" meta:resourcekey="lblBuilding2"/>
            <asp:TextBox runat="server" ID="txtBuilding2" MaxLength="10" CssClass="gdo-popup-form-field-padding"
                meta:resourcekey="txtBuilding2Resource1"/>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lblFlat" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Flat:" meta:resourcekey="lblFlat"/>
            <asp:TextBox runat="server" ID="txtFlat"  MaxLength="5" CssClass="gdo-popup-form-field-padding"
                meta:resourcekey="txtFlatResource1"/>
        </li>

        <li class="sameRow">
            <asp:Label runat="server" ID="lbPhoneNumber" CssClass="gdo-form-label-left gdo-popup-form-label-padding2"
                Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"/>
            <asp:TextBox runat="server" ID="txtNumber" CssClass="gdo-popup-form-field-padding"
                MaxLength="10" meta:resourcekey="txtNumberResource"/>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" Text="Format: 10 digits" meta:resourcekey="PhoneNumberFormat"/>
            </span>
        </li>
    </ul>
   
</div>