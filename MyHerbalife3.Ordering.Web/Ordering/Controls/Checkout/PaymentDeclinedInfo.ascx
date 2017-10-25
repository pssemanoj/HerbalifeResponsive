<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentDeclinedInfo.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.PaymentDeclinedInfo" %>
<asp:Panel ID="pnlDelinedPaymentsDetails" runat="server">

<table  style="width: 100%">
    <tr>
            <td>
                <strong>
                    <asp:Label ID="lblTransactionDateTimeText" runat="server" Text="Transaction Date/Time: " meta:resourcekey="lblTransactionDateTimeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionDatetime" runat="server"></asp:Label>
            </td>
        </tr>
    <tr>
            <td>
                <strong>
                    <asp:Label ID="lblPaymentCodeText" runat="server" Text="Payment Code: " meta:resourcekey="lblPaymentCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPaymentCode" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <strong>
                    <asp:Label ID="lblTransactionCodeText" runat="server" Text="transaction Code: " meta:resourcekey="lblTransactionCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionCode" runat="server"></asp:Label>
            </td>
        </tr>
            <tr>
            <td>
                <strong>
                    <asp:Label ID="lblAutorizationCodeText" runat="server" Text="Autorization Code: " meta:resourcekey="lblAutorizationCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAutorizationCode" runat="server"></asp:Label>
            </td>
        </tr>

</table>
    </asp:Panel>