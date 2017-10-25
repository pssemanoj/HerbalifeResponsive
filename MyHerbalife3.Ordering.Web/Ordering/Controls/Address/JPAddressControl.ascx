<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="JPAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.JPAddressControl" %> 

<style>
    .gdo-popup  span,.gdo-popup label,.gdo-popup .gdo-form-label-right{
        font-weight:bold !important;
    }
    .gdo-popup input[type="text"]{
        font-size:14px;
    }
    
</style> 
<div id="gdo-popup-container">  
            <b>
                <asp:Label ID="lblNotification" runat="server" meta:resourcekey="Ibnotification" ForeColor="#ff9900"></asp:Label>
            </b>
    <br />
    <asp:Label runat="server" ID="lbName" Text="Name*:" meta:resourcekey="lbRecipentResource"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 ">
        <li><asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostalCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="8" OnTextChanged="txtPostCode_TextChanged" AutoPostBack="True" placeholder="例:123-4567" MaximumLength="8"
                 onblur="formatPostalCode(this)" meta:resourcekey="txtPostalCodeResource" CssClass="txtPostCode" onkeypress="return isNumberKey(event)"></asp:TextBox>
        </li>
        <li><br /><span class="gdo-form-format">
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="PostalCodeFormat">Format: 7 digits</asp:Localize>
            </span>
        </li>        
        <li>
                <asp:Label runat="server" ID="lbPrefecture" Text="Prefecture:" meta:resourcekey="lbCountyResource"></asp:Label>
                <asp:TextBox AutoPostBack="False" runat="server" ID="txtPrefecture" ReadOnly="true" meta:resourcekey="dnlCountyResource"></asp:TextBox>
        </li>       
       
    </ul>
    
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li><asp:Label runat="server" ID="lbCity" Text="City*:"  meta:resourcekey="lbCityResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="20"  meta:resourcekey="txtCityResource"></asp:TextBox>
        </li>
        <li></li>
         <li><asp:Label runat="server" ID="lbTown" Text="Town*:" meta:resourcekey="lbTownResource" ></asp:Label>
            <asp:TextBox runat="server" ID="txtTown" MaxLength="20"></asp:TextBox>
        </li>
    </ul>

    <asp:Label runat="server" ID="lbStreet" Text="Street1*:" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" meta:resourcekey="lbStreet1Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet" MaxLength="60" CssClass="gdo-popup-form-field-padding gdo-address-input" meta:resourcekey="txtStreet1Resource"></asp:TextBox>
    <asp:Label runat="server" ID="lbStreet2" Text="Street2:" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" meta:resourcekey="lbStreet2Resource"></asp:Label>
    <asp:TextBox runat="server" ID="txtStreet2" MaxLength="60" CssClass="gdo-popup-form-field-padding gdo-address-input" meta:resourcekey="txtStreet2Resource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbAreaCode" Text="AreaCode*:"  CssClass="gdo-form-label-left gdo-popup-form-label-padding2"  meta:resourcekey="lbAreaCodeResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="5" style="width: 150px" Required="true" ValReqMsg="NoAreaCode"   ValErrorMsg="InvalidAreaCode"  MaximumLength="5" RegExp="^(\d{2,5})$" IndexForTab="2"  CssClass="gdo-popup-form-field-padding gdo-address-input"></asp:TextBox>
        </li>
          <li><span class="gdo-form-format">
            <asp:Localize ID="Localize2" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 10-15 digits</asp:Localize>
            </span>
        </li>
        <li><asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" CssClass="gdo-form-label-left gdo-popup-form-label-padding2" meta:resourcekey="lbPhoneNumberResource"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="8" style="width: 150px"   CssClass="gdo-popup-form-field-padding gdo-address-input" meta:resourcekey="txtPhoneNumberResource"></asp:TextBox>
        </li>

      
   </ul>
</div>