<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StaticPaymentInfo_MY.ascx.cs" 
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.StaticPaymentInfo_MY" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<asp:Panel ID="pnlAlternatePayment" runat="server" meta:resourcekey="lblAlternatePaymentResource1">
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
    <table style="width: 100%">
        <tr runat="server" id="trCardHolderName">
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblCardHolderText" runat="server" Text="Cardholder Name:" meta:resourcekey="lblCardHolderTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardHolderName" runat="server"></asp:Label>
            </td>
        </tr>
        <tr runat="server" id="trCardType">
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblCardTypeText" runat="server" Text="Card Type: " meta:resourcekey="lblCardTypeTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardType" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblCardNumberText" runat="server" Text="Card No: " meta:resourcekey="lblCardNumberTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardNumber" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblCardExpirationText" runat="server" Text="Exp Date: " meta:resourcekey="lblCardExpirationTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblCardExpiration" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trAmountText" runat="server">
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblAmountText" runat="server" Text="Amount:" meta:resourcekey="lblAmountTextResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblAmount" runat="server"></asp:Label>
            </td>
        </tr>
        <tr id="trPaymentOptions" runat="server">
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblPayOptionsText" runat="server" Text="Payment Options:" meta:resourcekey="lblPayOptionsResource1"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblPayOptions" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px" valign="top">
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
<asp:Panel ID="pnlFPXData" runat="server">
    <table style="width: 100%">
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblMerchantNameText" runat="server" Text="Merchant Name" meta:resourcekey="lblMerchantNameTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblMerchantName" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblFPXTransactionIdText" runat="server" Text="FPX Transaction ID" meta:resourcekey="lblFPXTransactionIdTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblFPXTransactionId" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblProductDescriptionText" runat="server" Text="Product Description" meta:resourcekey="lblProductDescriptionTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblProductDescription" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblTransactionStatusText" runat="server" Text="Transaction Status" meta:resourcekey="lblTransactionStatusTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <strong>
                    <asp:Label ID="lblTransactionStatus" runat="server"></asp:Label>
                </strong>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblDateAndTimeText" runat="server" Text="Date & Time" meta:resourcekey="lblDateAndTimeTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblDateAndTime" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblBuyerBankText" runat="server" Text="Buyer Bank" meta:resourcekey="lblBuyerBankResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblBuyerBank" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td style="width: 130px">
                <strong>
                    <asp:Label ID="lblBankReferenceNumberText" runat="server" Text="Bank Reference No." meta:resourcekey="lblBankReferenceNumberTextResource"></asp:Label>
                </strong>
            </td>
            <td>
                <asp:Label ID="lblBankReferenceNumber" runat="server"></asp:Label>
            </td>
        </tr>
    </table>
</asp:Panel>

