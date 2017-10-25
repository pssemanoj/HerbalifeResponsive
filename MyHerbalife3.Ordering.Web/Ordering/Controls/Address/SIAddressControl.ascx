<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SIAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.SIAddressControl" %>
<div id="gdo-popup-container">   
    <asp:Label runat="server" ID="lbName" Text="Care of Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li><asp:Label runat="server" ID="lbStreet" Text="Address*:" meta:resourcekey="lbStreet1Resource"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="58" class="gdo-address-input"
                meta:resourcekey="txtStreet1Resource"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbState" Text="City*:" meta:resourcekey="lbCityResource"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                meta:resourcekey="ddlCityResource" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" />
        </li>
    
        <li><asp:Label runat="server" ID="Label1" Text="Zip Code*:" meta:resourcekey="lbPostalResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="4" ReadOnly="True"
                meta:resourcekey="txtPostCodeResource"></asp:TextBox>
        </li>
 
        <li><asp:Label runat="server" ID="lblAreaCode" Text="Area Code*:" meta:resourcekey="lbAreaCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="3"  meta:resourcekey="txtAreaCodeResource"></asp:TextBox>
        </li>
    </ul>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumberResource"></asp:Label>   
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="8" meta:resourcekey="txtNumberResource"></asp:TextBox>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: Area code + phone 8 digits max</asp:Localize>
            </span>
        </li>
    </ul>
</div>