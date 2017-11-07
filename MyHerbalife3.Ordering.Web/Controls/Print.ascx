<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Print.ascx.cs" Inherits="HL.MyHerbalife.Web.Controls.Print" %>
<div id="Print" class="right">
    <asp:HyperLink CssClass="icon-printer" ID="PrintThisPageHL" runat="server" NavigateUrl="javascript:window.print()"></asp:HyperLink>
    <asp:HyperLink ID="aPrintThisPage" runat="server" meta:resourcekey="PrintThisPage" NavigateUrl="javascript:window.print()"></asp:HyperLink>
</div>
