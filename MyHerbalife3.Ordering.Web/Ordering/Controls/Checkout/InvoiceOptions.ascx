<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceOptions.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.InvoiceOptions" %>
<%@ Register TagPrefix="uiContent" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<div class="ntbox">
    <div class="ntbox-hdr" id="divInvoiceOptionSection" runat="server">
        <table border="0" cellspacing="0" cellpadding="0" style="margin: 10px">
            <tr>
                <td colspan="2" style="width: 130px">
                    <strong>
                        <asp:Label runat="server" ID="lbInvoiceOptionText" meta:resourcekey="lbInvoiceOptionTextResource1"
                            Text="Invoice Options :"></asp:Label></strong>
                </td>
            </tr>
        </table>
    </div>
    <div class="ntbox-hdr" id="div1" runat="server" style="border-bottom: 0px;">
        <table border="0" cellspacing="0" cellpadding="0" style="margin: 10px">
            <tr runat="server" id="trContentReader">
                <td colspan="2" runat="server" id="Td2" class="NoBorder">
                    <uiContent:ContentReader ID="ContentReaderInvoice" runat="server" ContentPath="invoiceoptions.html"
                        SectionName="ordering" ValidateContent="true" UseLocal="true" />
                </td>
            </tr>
            <tr runat="server" id="trEditInvoice">
                <td colspan="2" runat="server" id="divInvoiceOptions" class="NoBorder">
                </td>
            </tr>
            <tr runat="server" id="trReadOnly">
                <td colspan="2" runat="server" id="Td1" class="NoBorder">
                    <asp:Label ID="lblInvoiceOptions" runat="server" meta:resourcekey="lblInvoiceOptionsResource1" />
                </td>
            </tr>
        </table>
    </div>
    <div class="ntbox-hdr" id="divMessage" runat="server" style="border-bottom: 0px;">
    <uiContent:ContentReader ID="InvoiceMessage" runat="server" ContentPath="invoiceMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
</div>
