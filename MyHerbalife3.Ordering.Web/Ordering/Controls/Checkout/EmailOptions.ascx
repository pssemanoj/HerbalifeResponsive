<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EmailOptions.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.EmailOptions" %>
<div class="ntbox">
    <div class="ntbox-hdr">
        <asp:Label ID="lblEmailBoxHeader" runat="server" Text="Email Notification" meta:resourcekey="lblEmailBoxHeader"></asp:Label></div>
    <div id="dvEmailEdit" runat="server">
        <div class="ntbox-msg">
            <asp:Label ID="lblEmailBoxText1" runat="server" Text="Herbalife will send your order confirmation, status, or updates to this email address:"
                meta:resourcekey="lblEmailBoxText1"></asp:Label></div>
        <div class="ntbox-email">
            <asp:Label ID="lblEmailDisplay" runat="server" Text="Email*:" meta:resourcekey="lblEmailDisplay"></asp:Label>
            <asp:TextBox ID="txtEmail" runat="server" OnTextChanged="tbEmailAddress_Changed"
                AutoPostBack="true" meta:resourcekey="txtEmailResource1" MaxLength="320" />
            <asp:Label ForeColor="Red" runat="server" ID="lbEmailError" meta:resourcekey="lbEmailErrorResource1"></asp:Label>
        </div>
        <div class="ntbox-msg2">
            <asp:Label ID="lblEmailBoxText2" runat="server" Text="To make this your primary email address go to My Account > My Profile to update your email address"
                meta:resourcekey="lblEmailBoxText2"></asp:Label>
        </div>
    </div>
    <div id="dvEmailReadOnly" runat="server">
        <div class="ntbox-email">
            <asp:Label ID="lblEmailLabelDisplay" runat="server" Text="Email:" meta:resourcekey="lblEmailLabelDisplay"></asp:Label>
            <asp:Label ID="lblShortEmail" runat="server"></asp:Label>
            <asp:TextBox ID="txtLongEmailAddress" Visible="false" runat="server" BorderWidth="0"
                ReadOnly="true" BorderStyle="none" TextMode="multiLine" Wrap="true" Width="270px"></asp:TextBox>
        </div>
    </div>
</div>
