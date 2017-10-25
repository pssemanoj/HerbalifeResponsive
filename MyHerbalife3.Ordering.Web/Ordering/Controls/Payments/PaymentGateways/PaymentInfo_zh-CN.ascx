<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_zh_CN.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_zh_CN" %>
<asp:UpdatePanel ID="pnlCards" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlBanks">
            <asp:Label ForeColor="Red" runat="server" ID="lblError"></asp:Label>
            <asp:RadioButtonList runat="server" ID="ddlBanks" RepeatDirection="Horizontal" RepeatLayout="Table" RepeatColumns="7" CellPadding="5">
       
            </asp:RadioButtonList>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
