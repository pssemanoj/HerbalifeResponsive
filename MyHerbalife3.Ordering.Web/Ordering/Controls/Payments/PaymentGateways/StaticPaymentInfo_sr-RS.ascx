<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StaticPaymentInfo_sr-RS.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.StaticPaymentInfo_RS" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<asp:Panel ID="pnlAlternatePayment" runat="server" meta:resourcekey="pnlAlternatePaymentResource1">
    <table style="width: 100%">
        <tr>
            <td style="width: 130px" class="gdo-header-reference">
                <strong>
                    <asp:Label ID="lblReferenceText" runat="server" Text="Reference #:" meta:resourcekey="lbReferenceResource1"></asp:Label>
                </strong>
            </td>
            <td class="gdo-header-reference">
                <asp:Label ID="lblReference" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="mxReference" runat="server" style="display: none">
            <td style="width: 130px" class="gdo-header-reference">
                <strong>
                    <asp:Label ID="lblReferenceMXText" runat="server" Text="Reference #:" meta:resourcekey="lbReferenceResource1"></asp:Label>
                </strong>
            </td>
            <td class="gdo-header-reference">
                <asp:Label ID="lblReferenceMX" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 20%">
                <strong>
                    <asp:Label ID="lblAlternatePaymentMethodText" runat="server" Text="Payment Method:"
                        meta:resourcekey="PaymentMethod"></asp:Label>
                </strong>
            </td>
            <td style="width: 80%">
                <asp:Label ID="lblAlternatePaymentMethod" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblAlternateAmountText" runat="server" Text="Amount:" meta:resourcekey="lblAmountTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAlternateAmount" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 100%" colspan="2">
                <cc1:ContentReader ID="wireMessage" runat="server" ContentPath="" SectionName="Ordering"
                    ValidateContent="true" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:Panel ID="pnlCardPayments" runat="server">
    <p>
        <strong>
            <asp:Literal ID="lbSuccesfulTransaction" runat="server" meta:resourcekey="lbSuccesfulTransactionResource1"></asp:Literal>
        </strong>
    </p>
    <table style="width: 100%">
        <tr id="trCardHolderName" runat="server">
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblCardHolderText" runat="server" Text="Nombre del Comprador: " meta:resourcekey="lblCardHolderTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardHolderName" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trCardType" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblCardTypeText" runat="server" Text="Card Type: " meta:resourcekey="lblCardTypeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardType" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <strong>
                    <asp:Label ID="lblCardNumberText" runat="server" Text="Tarjeta de Crédito: " meta:resourcekey="lblCardNumberTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardNumber" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trCardExpDate" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblCardExpirationText" runat="server" Text="Exp Date: " meta:resourcekey="lblCardExpirationTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardExpiration" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trAmountText" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblAmountText" runat="server" Text="Monto y Moneda: " meta:resourcekey="lblAmountTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAmount" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trTrasanctionDate" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionDateText" runat="server" Text="Fecha de la Transaccion: " meta:resourcekey="lblTransactionDateTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionDate" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trAuthorizationCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblAuthorizationCodeText" runat="server" Text="Código de Autorización: " meta:resourcekey="lblAuthorizationCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAuthorizationCode" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trPaymentCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblPaymentCodeText" runat="server" Text="Código de Pago: " meta:resourcekey="lblPaymentCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPaymentCode" runat="server"></asp:Label>
            </td>
        </tr>
                <tr id="trTransactionCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionCodeText" runat="server" Text="Código de Transacción: " meta:resourcekey="lblTransactionCodeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionCode" runat="server"></asp:Label>
            </td>
        </tr>
        <%--Begin - NestPay data--%>
        <tr id="trNestPayOrderId" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblOrderIdText" runat="server" Text="Numero de Orden: " meta:resourcekey="lblOrderIdTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblOrderId" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trNestPayAuthCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblAuthorizationCode2Text" runat="server" Text="Código de Autorización: " meta:resourcekey="lblAuthorizationCode2TextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAuthorizationCode2" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trNestPayPaymentCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblPaymentCode2Text" runat="server" Text="Código de Pago: " meta:resourcekey="lblPaymentCode2TextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPaymentCode2" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trNestPayTransStatus" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionStatusText" runat="server" Text="Estatus de Transacción: " meta:resourcekey="lblTransactionStatusTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionStatus" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trNestPayTransCode" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionCode2Text" runat="server" Text="Código de Transacción: " meta:resourcekey="lblTransactionCode2TextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionCode2" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trNestPayTransDate" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionDate2Text" runat="server" Text="Fecha de la Transaccion: " meta:resourcekey="lblTransactionDate2TextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionDate2" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="tr3DTransStatus" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lbl3DTransactionStatusText" runat="server" Text="Fecha de la Transaccion: " meta:resourcekey="lbl3DTransactionStatusTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lbl3DTransactionStatus" runat="server"></asp:Label>
            </td>
        </tr>
        <%--End - NestPay data--%>
        <tr id="trTransactionType" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblTransactionTypeText" runat="server" Text="Tipo de transacción: " meta:resourcekey="lblTransactionTypeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblTransactionType" Text="Venta" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trPaymentOptions" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblPayOptionsText" runat="server" Text="Numero de Cuotas: " meta:resourcekey="lblPayOptionsResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPayOptions" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <strong>
                    <asp:Label ID="lblInstallmentTypeText" runat="server" Text="Tipos de Cuotas: " meta:resourcekey="lblInstallmentTypeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblInstallmentType" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <strong>
                    <asp:Label ID="lblUrlText" runat="server" Text="URL del Comercio: " meta:resourcekey="lblUrlTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblUrl" runat="server">cl.myherbalife.com</asp:Label>
            </td>
        </tr>
        <tr id="trAddress" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblBillingAddressText" runat="server" Text="Billing Address: " meta:resourcekey="lblBillingAddressTextResource1"></asp:Label>
                </strong>
            </td>
            <td valign="top">
                <asp:Label ID="lblBillingAddress" runat="server"></asp:Label>
            </td>
        </tr>
    </table>
</asp:Panel>
