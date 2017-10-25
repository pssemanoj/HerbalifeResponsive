<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentsSummary_VN.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.PaymentsSummary_VN" %>
<%@ Register Src="~/Ordering/Controls/Payments/PaymentGateways/StaticPaymentInfo_VN.ascx" TagName="pInfo" TagPrefix="uc" %>

<asp:Panel ID="pnlPaymentsSummary" runat="server" 
    meta:resourcekey="pnlPaymentsSummaryResource1">
    <asp:DataList ID="dlPaymentInfo" runat="server" RepeatDirection="Horizontal" 
        RepeatColumns="3" onitemdatabound="dlPaymentInfo_ItemDataBound" 
        ShowFooter="False" ShowHeader="False" 
        meta:resourcekey="dlPaymentInfoResource1" >
        <ItemTemplate>
            <uc:pInfo ID="paymentInfo" runat="server" ></uc:pInfo>
        </ItemTemplate>
    </asp:DataList>
</asp:Panel>