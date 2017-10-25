
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NorthAmericaAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.NorthAmericaAddressControl" %>


<div id="gdo-popup-container">
    <ul id="dvName" runat="server" class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbFirstName" Text="First Name*:" meta:resourcekey="lbFirstNameResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtFirstName" MaxLength="18"  
                meta:resourcekey="txtFirstNameResource1"></asp:TextBox></li>
        <li><asp:Label runat="server" ID="lbMiddleName" Text="M.I.:" meta:resourcekey="lbMiddleNameResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtMiddleName" MaxLength="2" 
                meta:resourcekey="txtMiddleNameResource1"></asp:TextBox></li>
        <li><asp:Label runat="server" ID="lbLastName" Text="Last Name*:" meta:resourcekey="lbLastNameResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtLastName" MaxLength="18" 
                 meta:resourcekey="txtLastNameResource1"></asp:TextBox></li>
    </ul>

    <div id="dvCareOfName" runat="server" class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbRecipient" Text="Care of Name*:" meta:resourcekey="lbRecipientResource"></asp:Label>
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="30" 
                 meta:resourcekey="txtCareOfNameResource1"></asp:TextBox>
    </div>

    <div runat="server" class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbStreetAddress" Text="Street Address*:" meta:resourcekey="lbStreetAddressResource"></asp:Label>
         <asp:TextBox runat="server" ID="txtStreet" MaxLength="40"  
                meta:resourcekey="txtStreetResource1"></asp:TextBox>
    </div>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCity"></asp:Label>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="30"
                meta:resourcekey="txtCityResource1"></asp:TextBox></li>

        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbState"></asp:Label>
            <asp:DropDownList runat="server" ID="dnlState" Style="margin: 0px 0px 0px 0px;" 
                meta:resourcekey="dnlStateResource1">
            </asp:DropDownList>
            <asp:TextBox runat="server" ID="txtState" MaxLength="30"></asp:TextBox></li>

        <li><asp:Label runat="server" ID="lbPostal" Text="Zip Code*:" meta:resourcekey="lbPostal"></asp:Label>
        <asp:TextBox runat="server" ID="txtPostCode" MaxLength="10" 
                meta:resourcekey="txtPostCodeResource1"></asp:TextBox></li>
        <li class="help-text"><span class="gdo-form-format" id="usFormat" runat="server"><asp:Localize ID="Localize1" runat="server" meta:resourcekey="USPostalCodeFormat">5 numbers no spaces</asp:Localize></span>
            <span class="gdo-form-format" id="caFormat" runat="server"><asp:Localize ID="Localize2" runat="server" meta:resourcekey="CAPostalCodeFormat">Format: A1A 1A1</asp:Localize></span>
            <span class="gdo-form-format" id="prFormat" runat="server"><asp:Localize ID="Localize3" runat="server" meta:resourcekey="PRPostalCodeFormat">4 numbers no spaces</asp:Localize></span></li>
    </ul>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="Label1" Text="Area Code*:" meta:resourcekey="lbAreaCode"></asp:Label>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="3" 
                meta:resourcekey="txtAreaCodeResource1"></asp:TextBox>
        </li>
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="7" 
                meta:resourcekey="txtNumberResource1" OnKeyPress="CheckNumeric(event,this)"></asp:TextBox>
        </li>
        <li><asp:Label runat="server" ID="lblExt" Text="Ext:" meta:resourcekey="lbExtension"></asp:Label>
            <asp:TextBox runat="server" ID="txtExtension" MaxLength="5" ></asp:TextBox>
        </li>
    </ul>
</div>

<!--<table border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="gdo-form-label-left gdo-popup-form-label-padding">
            <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" 
                ForeColor="Red" meta:resourcekey="blErrorsResource1">
            </asp:BulletedList>
        </td>
    </tr> 
    </table>-->

