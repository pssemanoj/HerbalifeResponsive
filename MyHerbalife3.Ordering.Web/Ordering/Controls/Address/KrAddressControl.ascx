<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="KrAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.KrAddressControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div id="gdo-popup-container">
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 KO">
        <li>
            <asp:Label runat="server" ID="lbRecipent" Text="Name*:" meta:resourcekey="lbRecipent"></asp:Label>
            <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40"></asp:TextBox>
        </li>
        <li>
            <asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number*:" meta:resourcekey="lbPhoneNumber"></asp:Label>
            <div class="KO">
                <asp:TextBox runat="server" ID="txtAreaCode" MaxLength="3" Width="30px"></asp:TextBox></>
                <asp:TextBox runat="server" ID="txtNumber" MaxLength="4" Width="50px"></asp:TextBox></>
                <asp:TextBox runat="server" ID="txtExtension" MaxLength="4" Width="50px"></asp:TextBox>
            </div>
        </li>
    </ul>
    
   <%-- <ul class="gdo-form-label-left gdo-popup-form-label-padding2 KO" style="border: 1px solid red">
        <li>
            <asp:Label runat="server" ID="lbAddressSearch" Text="Address Search*" meta:resourcekey="lbAddressSearch"></asp:Label>
            <asp:TextBox runat="server" ID="txtSearchTerm"></asp:TextBox>
            
        </li>
        <li>
            <cc1:DynamicButton id="OvalButtonGo" runat="server" onclick="GoButtonClicked" buttontype="Forward" text="GO" meta:resourcekey="OvalButtonGoResource1" />
        </li>
    </ul>
        <span>
            <asp:Label runat="server" ID="lbErrorMessage" meta:resourcekey="lbErrorMessageResource1"></asp:Label>
        </span>

    <asp:ListBox AutoPostBack="True" OnSelectedIndexChanged="lbAddresses_OnSelectedIndexChanged" ID="lbAddresses" runat="server" DataTextField="Value" DataValueField="Key"></asp:ListBox>--%>

    <input type="button" onclick="execDaumPostcode()" class="forward" style="background: #0080ff none repeat scroll 0 0; color: white;" value="우편번호 찾기">
    <a  onclick="execDaumPostcode()"></a>
    
    <div id="wrapDaum">
        <img src="//i1.daumcdn.net/localimg/localimages/07/postcode/320/close.png" id="btnFoldWrap" style="cursor:pointer;position:absolute;right:0px;top:-1px;z-index:1;width:15px;" onclick="foldDaumPostcode()" alt="접기 버튼">
    </div>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2" >
        <li>
            <asp:Label runat="server" ID="lbAddress1" Text="Address 1*:" meta:resourcekey="lbAddress1"></asp:Label>
            <asp:TextBox Enabled="False" runat="server" ID="txtAddress1"></asp:TextBox>
        </li>

        <li>
            <asp:Label Enabled="False" runat="server" ID="lbAddress2" Text="Address 2*:" meta:resourcekey="lbAddress2"></asp:Label>
            <asp:TextBox Enabled="False" runat="server" ID="txtAddress2"></asp:TextBox>
        </li>

        <li>
            <asp:Label runat="server" ID="lbAddress3" Text="Address 3*:" meta:resourcekey="lbAddress3"></asp:Label>
            <asp:TextBox Enabled="False" runat="server" ID="txtAddress3" MaxLength="40"></asp:TextBox>
        </li>

        <li>
            <asp:Label runat="server" ID="lbPostal" Text="Postal Code*:" meta:resourcekey="lbPostal"></asp:Label>
            <asp:TextBox Enabled="False" runat="server" ID="txtPostalCode1"></asp:TextBox>
        </li>
    </ul>
</div>