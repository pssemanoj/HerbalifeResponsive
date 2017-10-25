<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HUAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.HUAddressControl" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<div id="gdo-popup-container">
    <asp:Label runat="server" ID="lblRecipent" Text="Name*:" meta:resourcekey="Recipent"/>
    <asp:TextBox runat="server" ID="txtCareOfName" meta:resourcekey="txtCareOfName" MaxLength="40" />

    <asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="City"/>
    <asp:DropDownList runat="server" ID="ddlCity" AutoPostBack="True" OnSelectedIndexChanged="ddlCity_SelectedIndexChanged"/>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li><asp:Label runat="server" ID="lblSuburb" Text="Suburb:" meta:resourcekey="Suburb"/>
            <asp:DropDownList runat="server" ID="ddlSuburb" AutoPostBack="True" OnSelectedIndexChanged="ddlSuburb_SelectedIndexChanged"/>
            <asp:TextBox runat="server" ID="txtSuburb" Visible="false" />
        </li>

        <li><asp:Label runat="server" ID="lblDistrict" Text="District:" meta:resourcekey="District"/>
            <asp:DropDownList  runat="server" ID="ddlDistrict"  AutoPostBack="True" OnSelectedIndexChanged="ddlDistrict_SelectedIndexChanged"/>
            <asp:TextBox runat="server" ID="txtDistrict" Visible="false" />
        </li>

        <li><asp:Label runat="server" ID="lblStreet" Text="Street*:" meta:resourcekey="Street"/>
            <asp:DropDownList   runat="server" ID="ddlStreet" AutoPostBack="True" OnSelectedIndexChanged="ddlStreet_SelectedIndexChanged"/>
            <asp:TextBox runat="server" ID="txtStreet" Visible="false" />
        </li>

        <li><asp:Label runat="server" ID="lblStreetType" Text="Street type*:" meta:resourcekey="StreetType" />
            <asp:DropDownList  runat="server" ID="ddlStreetType" AutoPostBack="True" OnSelectedIndexChanged="ddlStreetType_SelectedIndexChanged" />
            <asp:TextBox runat="server" ID="txtStreetType" Visible="false" />
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lblHouseNumber" Text="House Number*:" meta:resourcekey="HouseNumber"/>
            <asp:TextBox runat="server" ID="txtHouseNumber" MaxLength="10"  meta:resourcekey="txtHouseNumber"/>
        </li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" Text="Format: 3 chars firts always number" meta:resourcekey="HouseNumberFormat"/>
            </span>
        </li>

        <li><asp:Label runat="server" ID="lblPostalCode" Text="Postal Code*:" meta:resourcekey="PostalCode"/>
            <asp:DropDownList runat="server" ID="ddlPostalCode" Visible="false" />
            <asp:TextBox runat="server" ID="txtPostalCode" MaxLength="4" meta:resourcekey="txtPostalCode" />
        </li>

        <li><asp:Label runat="server" ID="lblPhoneNumber" Text="Phone Number*:" meta:resourcekey="PhoneNumber"/>
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="9" meta:resourcekey="txtPhoneNumberResource"/>
        <li>
        <li><span class="gdo-form-format">
                <asp:Localize ID="Localize2" runat="server" Text="Format: Min 8 Max 9 digits" meta:resourcekey="PhoneNumberFormat"/>
            </span>
        </li>
    </ul>
</div>
