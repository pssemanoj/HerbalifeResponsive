<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StaticPaymentInfo_VN.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.StaticPaymentInfo_VN" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<asp:Panel ID="pnlAlternatePayment" runat="server" meta:resourcekey="pnlAlternatePaymentResource1">
    <table style="width: 100%">
        <tr>
            <td class="gdo-header-reference">
                <strong>
                    <asp:Label ID="lblReferenceText" runat="server" Text="Reference #:" meta:resourcekey="lbReferenceResource1"></asp:Label>
                </strong>
            </td>
            <td class="gdo-header-reference">
                <asp:Label ID="lblReference" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="mxReference" runat="server" style="display: none">
            <td class="gdo-header-reference">
                <strong>
                    <asp:Label ID="lblReferenceMXText" runat="server" Text="Reference #:" meta:resourcekey="lbReference2Resource1"></asp:Label>
                </strong>
                <div id="divHelpIcon" runat="server" style="display: none">
                    <ajaxToolkit:HoverMenuExtender ID="HoverHelpIcon" runat="server" TargetControlID="imgHelp"
                        PopupControlID="pnlHelpText" />
                    <a href="javascript:return false;" onclick="return false;">
                        <img id="imgHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                            alt="info" height="12" width="12" />
                    </a>
                    <asp:Panel ID="pnlHelpText" runat="server" Style="display: none">
                        <div class="gdo-popup">
                            <asp:Label ID="lblHelp" runat="server" meta:resourcekey="lblHelp">
                            </asp:Label>
                        </div>
                    </asp:Panel>
                </div>
            </td>
            <td class="gdo-header-reference">
                <asp:Label ID="lblReferenceMX" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trAlternatePaymentMethod" runat="server">
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
        <tr id="trPGHPaymentMethod" runat="server" visible="false">
            <td style="width: 20%">
                <strong>
                    <asp:Label ID="lblPGHPaymentMethodText" runat="server" Text="Payment Method:"
                        meta:resourcekey="PaymentMethod"></asp:Label>
                </strong>
            </td>
            <td style="width: 80%">
                <strong>
                    <asp:Label ID="lblPGHPaymentMethod" runat="server" Text="Credit Card" meta:resourcekey="PGHPaymentMethodResource"></asp:Label>
                </strong>
                <asp:Label ID="lblPGHPaymentMethodData" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
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
    <table style="width: 100%">
        <tr runat="server" id="trCardHolderName">
            <td>
                <strong>
                    <asp:Label ID="lblCardHolderText" runat="server" Text="Cardholder Name:" meta:resourcekey="lblCardHolderTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardHolderName" runat="server"></asp:Label>
            </td>
        </tr>
        <tr runat="server" id="trCardType">
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
                    <asp:Label ID="lblCardNumberText" runat="server" Text="Card No: " meta:resourcekey="lblCardNumberTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardNumber" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
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
                    <asp:Label ID="lblAmountText" runat="server" Text="Amount:" meta:resourcekey="lblAmountTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAmount" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trPaymentOptions" runat="server">
            <td>
                <strong>
                    <asp:Label ID="lblPayOptionsText" runat="server" Text="Payment Options:" meta:resourcekey="lblPayOptionsResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPayOptions" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td valign="top">
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
