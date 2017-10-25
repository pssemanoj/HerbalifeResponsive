<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BRAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.BRAddressControl" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" %>

<div id="gdo-popup-container">
    <div class="gdo-form-label-left gdo-popup-form-label-padding2">
        <asp:Label runat="server" ID="lbFirstName" Text="Name *" meta:resourcekey="lbFirstName"></asp:Label>
        <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" ></asp:TextBox>
    </div>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code *" meta:resourcekey="lbPostal"></asp:Label>
            <asp:TextBox runat="server" ID="txtPostCode" MaxLength="5" OnBlur="CheckChanges(this, '.txtPostCode2');"
                CssClass="txtPostCode1" Width="67px" OnTextChanged="txtPostCode_TextChanged"></asp:TextBox>
            <ajaxToolkit:FilteredTextBoxExtender ID="ftTxtPostCode" runat="server" TargetControlID="txtPostCode"
                FilterType="Numbers" />
            <asp:TextBox runat="server" ID="txtPostCode2" MaxLength="3" OnBlur="CheckChanges(this, '.txtPostCode1');"
                    CssClass="txtPostCode2"  Width="67px" OnTextChanged="txtPostCode_TextChanged"></asp:TextBox>
                <ajaxToolkit:FilteredTextBoxExtender ID="ftTxtPostCode2" runat="server" TargetControlID="txtPostCode2"
                    FilterType="Numbers" />
        </li>
        <li>
            <span class="gdo-form-format">               
                <asp:Localize ID="Localize1" runat="server" Text="Format: 8 digits" meta:resourcekey="Localize1"></asp:Localize>
            </span>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbCity" Text="City *" meta:resourcekey="lbCity"></asp:Label>
            <asp:TextBox runat="server" ID="txtCity" MaxLength="40"  ReadOnly="true"></asp:TextBox>
            <asp:Label runat="server" ForeColor="Red" ID="lblNoMatch" Visible="false" Text="No match were found"
                meta:resourcekey="lblNoMatch"></asp:Label>
        </li>
        <li>
            <asp:Label runat="server" ID="lbStreet" Text="Street 1 *" meta:resourcekey="lbStreet"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet" MaxLength="40" ></asp:TextBox>
        </li>
        <li>
            <asp:Label runat="server" ID="lbNumber" Text="Number *" meta:resourcekey="lbNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="8"></asp:TextBox>
            <ajaxToolkit:FilteredTextBoxExtender ID="fttxtNumber" runat="server" TargetControlID="txtNumber"
                FilterType="Numbers" />
        </li>
    </ul>
        <div class="gdo-form-label-left gdo-popup-form-label-padding2">
            <asp:Label runat="server" ID="lbStreet2" Text="Street 2" meta:resourcekey="lbStreet2"></asp:Label>
            <asp:TextBox runat="server" ID="txtStreet2" MaxLength="40"></asp:TextBox>
        </div>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbNeighborhood" Text="Neighborhood *" meta:resourcekey="lbNeighborhood"></asp:Label>
            <asp:TextBox runat="server" ID="txtNeighborhood" MaxLength="40"></asp:TextBox>
        </li>
        <li>
            <asp:Label runat="server" ID="lbState" Text="State *" meta:resourcekey="lbState"></asp:Label>
            <asp:TextBox runat="server" ID="txtState" MaxLength="2"
                ReadOnly="true"></asp:TextBox>
        </li>
        <li>
            <asp:Label runat="server" ID="lbAreaCode" Text="Area Code *" meta:resourcekey="lbAreaCode"></asp:Label>
            <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="2"></asp:TextBox>
            <ajaxToolkit:FilteredTextBoxExtender ID="fttxtAreaCode" runat="server" TargetControlID="txtAreaCode"
                FilterType="Numbers" />
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2">
        <li>
            <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number *" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtPhoneNumber" MaxLength="9" ></asp:TextBox>
            <ajaxToolkit:FilteredTextBoxExtender ID="fttxtPhoneNumber" runat="server" TargetControlID="txtPhoneNumber"
                FilterType="Numbers" />
        </li>
        <li>
            <span class="gdo-form-format" >
                <asp:Localize ID="Localize2" runat="server" Text="Format: 11 Digits" meta:resourcekey="Localize2"></asp:Localize>
            </span>
        </li>
    </ul>
</div>


            

            

            

            
