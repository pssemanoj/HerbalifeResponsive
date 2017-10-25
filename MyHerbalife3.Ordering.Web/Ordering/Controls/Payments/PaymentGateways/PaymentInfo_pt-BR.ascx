<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_pt-BR.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_pt_BR" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:UpdatePanel ID="pnlCards" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlBRCards">
            <asp:Label ForeColor="Red" runat="server" ID="lblError"></asp:Label>
            <asp:RadioButtonList runat="server" ID="ddlGateways" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="brGateways">
            </asp:RadioButtonList>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
