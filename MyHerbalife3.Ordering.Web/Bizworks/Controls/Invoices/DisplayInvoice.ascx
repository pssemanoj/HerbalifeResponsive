<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DisplayInvoice.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.DisplayInvoice" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<telerik:RadScriptBlock ID="RadScriptBlockFollowups" runat="server">
    <script type="text/javascript">
        $(document).ready(
            function () {
                getHtml();
            });

        function getHtml() {
            var $pdfHidden = $('.pdfHidden'),
                $divDisplayInvoice = $('#divDisplayInvoice')
                $pdfBold = $('.pdfBold'),
                $pdfBackground = $('.pdfBackground');
            document.getElementById('divManageInvoice').style.visibility = 'hidden';
            $pdfHidden.css('visibility', 'hidden');
            $divDisplayInvoice.css({'background': 'url("/content/global/Bizworks/img/hlf_leaf_large.png") no-repeat scroll 250px -60px #E8F5E8',
                'font-family' : 'Helveltica, Arial',
                'font-size' : '12px'});
            $pdfBold.css('font-weight', 'bold');
            $pdfBackground.css('background-color', '#DDD');
            var src = document.getElementById("divDisplayInvoice").outerHTML;
            document.getElementById("<%=HhtmlContent.ClientID%>").value = src.replace('no-repeat no-repeat', 'no-repeat');
            document.getElementById('divManageInvoice').style.visibility = 'visible';
            document.getElementById('divThankYou').style.visibility = 'hidden';
            document.getElementById('divInvText').style.visibility = 'hidden';
            $pdfHidden.css('visibility', 'visible');
            $divDisplayInvoice.css({'background' : '',
                'font-family': '',
                'font-size': ''
            });
            $pdfBold.css('font-weight', '');
            $pdfBackground.css('background-color', '');
        }

        //shan - mar 09, 2012 - show confirm delete popup
        function showConfirmDeletePopup() {
            var popup = $find("<%=radConfirmDeletePopup.ClientID%>");
            popup.show();
            return false;
        }

        //hide confirm delete popup
        function hideConfirmDeletePopup() {
            var popup = $find("<%=radConfirmDeletePopup.ClientID%>");
            popup.hide();
            return false;
        }
    </script>
</telerik:RadScriptBlock>
<!-- begin - shan - for page heading -->
<div>
    <div class="title-div">
        <h1><asp:Literal ID="ltPageHeading" runat="server" Text="View Invoice" meta:resourcekey="ltPageHeading" /></h1>
    </div>
    <p>
        <asp:Literal ID="ltPageDesc1" runat="server" Text="Please review your invoice below."
            meta:resourcekey="ltPageDesc1" />
        <asp:Literal ID="ltPageDesc2" runat="server" Text="Use the links at the top right to manage this invoice and more."
            meta:resourcekey="ltPageDesc2" />
    </p>
</div>
<!-- end -->

<cc1:DynamicButton ID="lnkBackToInvoiceList" runat="server" meta:resourcekey="lnkBackToInvoiceList" Text="Return to My Invoices" IconType="ArrowLeft" />
         
<asp:HiddenField ID="HhtmlContent" runat="server" Value="" />
<div id="divDisplayInvoice">
    <div class="header ovrFlwH">
        <img alt="logo" src="/Content/Global/img/dark_logo.png" class="left" />
        <h1>
            invoice
        </h1>
    </div>
    <div id ="divThankYou">
        <table>
            <tr>
                <td style="float: left; font-weight: bold; width: 20%">
                    <asp:Literal runat="server" ID="ltThankYou" Text="Thank You For Your Order!" ></asp:Literal>
                </td>
                <td style="float: right; font-weight: bold; width: 20%">
                    <asp:Literal runat="server" ID="ltInvoice" Text="INVOICE" ></asp:Literal>
                </td>
            </tr>
        </table>
    </div>
    <table>
        <tr>
        <td valign="top">
           <table id="invoice-info-topleft" cellpadding="3">
                <tr>
                    <td style="float: left; font-weight: bold;">
                        <asp:Literal runat="server" ID="ltName" Text="Name:" meta:resourcekey="ltName"></asp:Literal>
                    </td>
                    <td>
                        <asp:Literal runat="server" ID="ltNameVal"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td style="float: left; font-weight: bold;" valign="top">
                        <asp:Literal runat="server" ID="ltShipto" Text="Ship To:" meta:resourcekey="ltShipto"></asp:Literal>
                    </td>
                    <td valign="top">
                        <asp:Literal runat="server" ID="ltShiptoVal"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td style="float: left; font-weight: bold;">
                        <asp:Literal runat="server" ID="ltPhone" Text="Phone #:" meta:resourcekey="ltPhone"></asp:Literal>
                    </td>
                    <td>
                        <asp:Literal runat="server" ID="ltPhoneVal"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td style="float: left; font-weight: bold;">
                        <asp:Literal runat="server" ID="ltEmail" Text="Email Address:" meta:resourcekey="ltEmail"></asp:Literal>
                    </td>
                    <td>
                        <asp:Literal runat="server" ID="ltEmailVal"></asp:Literal>
                    </td>
                </tr>
                <tr class="pdfHidden">
                    <td style="float: left; font-weight: bold;">
                        <asp:Literal runat="server" ID="ltInvoiceType" Text="Invoice Type:" meta:resourcekey="ltInvoiceType"></asp:Literal>
                    </td>
                    <td id="invoice_type_val">
                        <asp:Literal runat="server" ID="ltInvoiceTypeVal"></asp:Literal>
                    </td>
                </tr>
            </table>
    </td>
    <td valign="top">

        <p class="rightAlgn">
            <span class="Bold">
                <asp:Literal runat="server" ID="ltInvoiceDate" Text="Invoice Date:" meta:resourcekey="ltInvoiceDate"></asp:Literal>
            </span>
            <span>
                <asp:Literal runat="server" ID="ltInvoiceDateVal"></asp:Literal>
            </span>
            <br />

            <span class="Bold">
                <asp:Literal runat="server" ID="ltInvoiceNumber" Text="Invoice #:" meta:resourcekey="ltInvoiceNumber"></asp:Literal>
            </span>
            <span>
                <asp:Literal runat="server" ID="ltInvoiceNumberVal"></asp:Literal>
            </span>
        </p>

        <div id="divManageInvoice">
    <%--        <h3>
                <asp:Literal ID="ltManageInvoice" runat="server" Text="Manage Your Invoice" meta:resourcekey="ltManageInvoice"></asp:Literal>
            </h3>--%>
            <div>
                <cc1:DynamicButton ID="lnkMakeChanges" runat="server" meta:resourcekey="lnkMakeChanges" Text="Make Changese" IconType="ArrowLeft" OnClick="OnEditInvoice"/>
                <cc1:DynamicButton ID="lnkDeleteInvoice" runat="server" meta:resourcekey="lnkDeleteInvoice" Text="Delete Invoice" IconType="ArrowLeft" OnClientClick="javascript:return showConfirmDeletePopup();"/>
                <cc1:DynamicButton ID="lnkCopyInvoice" runat="server" meta:resourcekey="lnkCopyInvoice" Text="Copy to New Invoice" IconType="ArrowLeft" OnClick="OnCopyInvoice"/>
                <cc1:DynamicButton ID="lnkCreateOrder" runat="server" meta:resourcekey="lnkCreateOrder" Text="Create Order" IconType="ArrowLeft" OnClick="OnCreateOrder" Visible="false"/>
            </div>
            <div>           
                <cc1:DynamicButton ID="lnkPrintInvoice" runat="server" meta:resourcekey="lnkPrintInvoice" Text="Print Invoice" IconType="ArrowLeft" />
                <cc1:DynamicButton ID="lnkEmailInvoice" runat="server" meta:resourcekey="lnkEmailInvoice" Text="Email Invoice" IconType="ArrowLeft" />
                <cc1:DynamicButton ID="lnkSavePDF" runat="server" meta:resourcekey="lnkSavePDF" Text="Save as a PDF" IconType="ArrowLeft" />

            </div>           
            <%--<asp:LinkButton ID="lnkBackToInvoiceList" runat="server" meta:resourcekey="lnkBackToInvoiceList" Text="Return to My Invoices" ></asp:LinkButton><br />--%>     
        </div>
    </td>
    </tr>
    </table>

    <table id="invoice-skus" cellspacing="0">
        <tr class="hlightedRow">
            <td class="pdfBold pdfBackground">
                <asp:Label ID="GridSKU" runat="server" Text="SKU" meta:resourcekey="GridSKU" />
            </td>
            <td class="pdfBold pdfBackground">
                <asp:Label ID="GridDescription" runat="server" Text="Description" meta:resourcekey="GridDescription"/>
            </td>
            <td class="pdfBold pdfBackground">
                <asp:Label ID="GridQty" runat="server" Text="Qty" meta:resourcekey="GridQty"/>
            </td>
            <td class="pdfBold pdfBackground">
                <asp:Label ID="GridUnitRetailPrice" runat="server" Text="Unit Retail Price" meta:resourcekey="GridUnitRetailPrice"/>
            </td>
            <td>
                <asp:Label ID="GridTotalVolPoints" CssClass="pdfHidden" runat="server" Text="Total Vol Pts." meta:resourcekey="GridTotalVolPoints"/>
            </td>
            <td class="pdfBold pdfBackground">
                <asp:Label ID="GridTotalPrice" runat="server" Text="Your Total Price" meta:resourcekey="GridTotalPrice"/>
            </td>
        </tr>
    <asp:Repeater ID="skuItems" runat="server">
        <ItemTemplate>
            <tr>
                <td class="pdfBackground">
                    <asp:Label ID="lblGridSku" runat="server" Text='<%# Eval("SKU") %>' />
                </td>
                <td class="pdfBackground">
                    <asp:Label ID="lblGridDescription" runat="server" Text='<%# Eval("Description") %>' />
                </td>
                <td class="pdfBackground">
                    <asp:Label ID="lblGridQty" runat="server" Text='<%# Eval("Quantity") %>' />
                </td>
                <td align="right" class="pdfBackground">
                    $<asp:Label ID="lblGridUnitRetailPrice" runat="server" Text='<%# Eval("UnitTotalPrice", "{0:N2}")%>' />
                </td>
                <td align="right">
                    <asp:Label ID="lblGridTotalVolPoints" CssClass="pdfHidden" runat="server" Text='<%# Eval("TotalVolumePoints", "{0:N2}")%>'></asp:Label>
                </td>
                <td align="right" class="pdfBackground">
                    $<asp:Label ID="lblGridTotalPrice" runat="server" Text='<%# Eval("TotalPrice", "{0:N2}")%>' />
                </td>
            </tr>
        </ItemTemplate>
    </asp:Repeater>
        <tr class="hlightedRow">
            <td colspan="3"></td>
            <td align="right" class="pdfBold pdfBackground">
                <asp:Label ID="lblItemSubTotalCaption" runat="server" Text="Subtotals:" meta:resourcekey="lblItemSubTotalCaption" />
            </td>
            <td align="right">
                <asp:Label ID="lblItemTotalVolume" CssClass="pdfHidden" runat="server" />
            </td>
            <td align="right" class="pdfBackground">
                $<asp:Label ID="lblItemSubTotal" runat="server" />
            </td>
        </tr>
    </table>

    <table>
        <tr>
            <td valign="top">
                <div id="invoice-status">
                    <span class="pdfHidden"><asp:Literal runat="server" ID="ltInvoiceStatus" Text="Invoice Status:" meta:resourcekey="ltInvoiceStatus"></asp:Literal></span><br />
                    <span class="pdfHidden"><asp:Literal runat="server" ID="ltInvoiceStatusVal"></asp:Literal></span><br />
                    <span class="pdfBold"><asp:Literal runat="server" ID="ltNotes" Text="Notes:" meta:resourcekey="ltNotes"></asp:Literal></span><br />
                    <span><asp:Literal runat="server" ID="ltNotesVal"></asp:Literal></span><br />
                    <span class="pdfBold"><asp:Literal runat="server" ID="ltSendPaymentTo" Text="Please Send Payment To:" meta:resourcekey="ltSendPaymentTo"></asp:Literal></span><br />
                    <span><asp:Literal runat="server" ID="ltSendPaymentToVal"></asp:Literal></span><br />
                </div>
            </td>
            <td valign="top">
            
                <div class="totals">
                    <table cellspacing="0" cellpadding="0">
                        <tr class="pdfBackground">
                            <td align="right">
                                <asp:Literal runat="server" ID="lblPrice" Text="Your Price:" meta:resourcekey="lblPrice"></asp:Literal>
                            </td>
                            <td align="right">
                                $<asp:Literal runat="server" ID="lblPriceVal" />
                            </td>
                        </tr>
                        <tr class="pdfBackground" runat="server" id="trCustomerLoyalty" visible="false">
                            <td align="right">
                                <asp:Literal runat="server" ID="lblLoyaltyProgram" Text="Customer Loyalty Program:" meta:resourcekey="lblLoyaltyProgram"></asp:Literal>
                            </td>
                            <td align="right">
                                -$<asp:Literal runat="server" ID="lblLoyaltyProgramVal" />
                            </td>
                        </tr>
                        <tr class="pdfBackground">
                            <td align="right">
                                <asp:Literal runat="server" ID="lblSubtotal" Text="Subtotal:" meta:resourcekey="lblSubtotal"></asp:Literal>
                            </td>
                            <td align="right">
                                $<asp:Literal runat="server" ID="lblSubtotalVal" />
                            </td>
                        </tr>
                        <tr class="view_invoice_spaces"><span></span></tr>
                        <tr class="pdfBackground">
                            <td align="right">
                                <asp:Literal runat="server" ID="lblTax" Text="Tax:" meta:resourcekey="lblTax"></asp:Literal>
                            </td>
                            <td align="right">
                                $<asp:Literal runat="server" ID="lblTaxVal" />
                            </td>
                        </tr>                        
                        <tr class="pdfBackground">
                            <td align="right">
                                <asp:Literal runat="server" ID="lblShipping" Text="Shipping & Handling:" meta:resourcekey="lblShipping"></asp:Literal>
                            </td>
                            <td align="right">
                                $<asp:Literal runat="server" ID="lblShippingVal" />
                            </td>
                        </tr>
                        <tr class="view_invoice_spaces"><span></span></tr>
                        <tr id="total-due-row" class="hlightedRow">
                            <td align="right" class="pdfBold">
                                <asp:Literal runat="server" ID="ltlTotalDue" Text="Your Total Due:" meta:resourcekey="ltlTotalDue"></asp:Literal>
                            </td>
                            <td align="right">
                                $<asp:Literal runat="server" ID="ltlTotalDueVal" />
                            </td>
                        </tr>
                    </table>
                </div>

            </td>
        </tr>
    </table>
    <div id="divInvText" style="font-family: Arial; font-size: 12px;">
            <p>
            I understand that this order may be considered as an invitation to call upon me from time to time, with the understanding that I will be
            under no obligation to buy.
            </p>
            <div style="width: 100%; overflow: hidden">
                <p style="float: left; font-weight: bold; width: 20%">
                Important Notice 
                </p>
                <p  style="float: left; width: 80%;">
                You, the buyer, may cancel this transaction at any time prior to midnight of the third business day after the
                date of this transaction.<br />
                See the "Notice of Cancellation" on the reverse of this form for an explanation of this right. After the 3-day
                cancellation period provided above, you are still protected by the HERBALIFE REFUND POLICY as set
                forth.
                </p>
            </div>

            <div style="page-break-before: always">
            <br />
            <span style=" font-weight: bold">Herbalife Refund Policy</span><br />
            <p>
            Herbalife offers an exchange or a full refund. Simply request a refund from your Distributor within thirty (30) days from your receipt of
            the product, and return the unused portion with the product containers to the Distributor named on the reverse side.
               <br /><br />
            FEDERAL AND STATE LAW: Regulations require that we print the following Notice of Cancellation. The Herbalife Refund Policy offers
            and provides you greater protection than the law requires.
            </p>
            
            <br />
            
            <div>
                <p>
                    <span style=" font-weight: bold">Notice of Cancellation</span>
                </p>
            
                <p>
                Date of Transaction: _______ / _______ /_______
                </p>

                <p>
                You may CANCEL this transaction, without any penalty or obligation, within THREE BUSINESS DAYS from the above date.
                </p>

                <p>
                If you cancel, any property traded in, any payments made by you under the contract or sale, and any negotiable instrument executed by
                you will be returned within TEN BUSINESS DAYS following the receipt of the seller of your cancellation notice, and any security interest
                arising out of the transaction will be canceled.
                </p>

                <p>            
                If you cancel, you must make available to the seller at your residence, in substantially as good condition as when received, any goods
                delivered to you under this contract or sale; or you may, if you wish, comply with the instructions of the seller regarding the return
                shipment of the goods at the seller#s expense and risk.
                </p>

                <p>            
                If you do make the goods available to the seller and the seller does not pick them up within 20 days of the date of your Notice of
                Cancellation, you may retain or dispose of the goods without any further obligation. If you fail to make the goods available to the seller,
                or if you agree to return the goods to the seller and fail to do so, then you remain liable for performance of all obligations under the
                contract.
                </p>

                <p>
                To cancel this transaction, mail or deliver a signed copy of this Cancellation Notice or any other written notice, or send a telegram to:
                </p>

                <p style="margin-left: 20%;">
                Herbalife Distributor: ____________________________________<br /><br />
                Address: ______________________________________________<br /><br />
                City: ________________________________ State:______ Zip Code: ________<br /><br />
                NOT LATER THAN MIDNIGHT OF: _______ / _______ /_______<br />
                (Date: 3 days after date of order)<br /><br />
                I HEREBY CANCEL THIS TRANSACTION:<br /><br /><br />

                    <span style="border-top: 1px solid;display: block;float: left;margin-right: 30px;padding-top: 3px;width: 130px;">
                            Day/Month/Year
                    </span> 

                    <span style="border-top: 1px solid;display: block;float: left;padding-top: 3px;width: 250px;">
                           Buyer's Signature
                    </span>
                
                </p>
            </div>
            </div>
        </div>
</div>
<iframe id="pdfInvoker" style="display:none;"></iframe>
<!-- begin - shan - mar 09, 2012 - confirm panel while clicking on delete button -->
<telerik:RadToolTip ID="radConfirmDeletePopup" runat="server"
        Modal="True" HideEvent="ManualClose" Position="Center"
        RelativeTo="BrowserWindow" 
        ManualClose="true">
    <asp:Panel ID="panelConfirmDelete" runat="server" CssClass="popupwrapper">
        <table class="popup-layout-table" style="width:auto !important">
            <tr>
                <td>
                    <div class="popup-content">
                        <div class="popwrapper">
                            <asp:Literal ID="litConfirmDeleteMsg" runat="server"
                                Text="Are you sure you want to delete this invoice?"
                                />
                        </div>
                    </div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="popup-buttons">
                        <div class="buttongap">
                            <cc1:OvalButton ID="btnConfirmYes" runat="server" Coloring="Silver"
                                Text="Yes" meta:resourcekey="btnConfirmYes" OnClick="OnDeleteInvoiceClick" />
                        </div>
                        <div class="buttongap">
                            <cc1:OvalButton ID="btnConfirmNo" runat="server" Coloring="Silver"
                                Text="No" meta:resourcekey="btnConfirmNo" OnClientClick="return hideConfirmDeletePopup();" />
                        </div>
                    </div>                    
                </td>
            </tr>
        </table>
    </asp:Panel>
</telerik:RadToolTip>
<!-- end -->
<script type="text/javascript">
    $(document).ready(function () {
        $('.pdf-link').click(function () {
            var contentDisposition = $(this).hasClass('inline') ? 'inline' : 'attachment',
                printUrl = "<%= PrintUrl %>&disposition=" + contentDisposition;
            if (contentDisposition === "inline") {
                window.open(printUrl);
            }
            else {
                $('#pdfInvoker').attr('src', printUrl);
            }
            return false;
        });
    });
</script>