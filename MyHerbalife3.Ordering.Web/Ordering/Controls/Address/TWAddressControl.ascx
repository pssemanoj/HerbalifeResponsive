<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TWAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.TWAddressControl" %>
<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lbRecipient" Text="Recipient*:" meta:resourcekey="lbRecipientResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtRecipient" MaxLength="49" meta:resourcekey="txtRecipientResource1"></asp:TextBox>

    <asp:Label runat="server" ID="lbStreet" Text="Street*:" meta:resourcekey="lbStreetResource1"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="50" meta:resourcekey="txtStreetResource1"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="PhoneNumber*:" meta:resourcekey="lbPhoneNumberResource1"></asp:Label>
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="10" meta:resourcekey="txtPhoneNumberResource1"></asp:TextBox>
        </li>

        <li><asp:Label runat="server" ID="lbState" Text="State*:" meta:resourcekey="lbStateResource1"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="ddlState" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" meta:resourcekey="ddlStateResource1"></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="lbCityResource1"></asp:Label>
                <asp:DropDownList AutoPostBack="True" runat="server" ID="ddlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" meta:resourcekey="ddlCityResource1"></asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbZipcode" Text="Zipcode*:" meta:resourcekey="lbZipcodeResource1"></asp:Label>
                <asp:TextBox runat="server" ID="txtZipcode" MaxLength="3" meta:resourcekey="txtZipcodeResource1" ReadOnly="True"></asp:TextBox>
        </li>
    </ul>
</div>
