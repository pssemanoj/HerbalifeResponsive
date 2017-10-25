<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutTotalsDetailed_IN.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutTotalsDetailed_IN" %>
<script type="text/javascript">
    function GetOrderMonthHelpText() {
        var tooltip = $find("<%=HoverMenuExtenderOrderMonthVolume.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
    function GetEarnBaseHelpText() {
        var tooltip = $find("<%=HoverMenuExtenderEarnBase.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
</script>
<ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server"
    TargetControlID="imgOrderMonth" PopupControlID="pnlOrderMonthVolumeText" />
<ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderEarnBase" runat="server"
    TargetControlID="imgEarnBase" PopupControlID="pnlEarnBaseText" />
<div class="gdo-order-details-wrapper">
    <!-- Order Details -->
    <div class="gdo-order-details-container col-md-4">
        <table class="gdo-order-details-tbl">
            <tr>
                <td colspan="2" class="gdo-totals-header">
                    <h3>
                        <asp:Label ID="lblDisplayOrderDetails" Text="Order Details" runat="server" meta:resourcekey="lblDisplayOrderDetailsResource1" />
                    </h3>
                </td>
            </tr>
            <tr>
                <td class="gdo-totals-header-spacer"></td>
            </tr>
            <tr>
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayOrderMonth" Text="Order Month" runat="server" meta:resourcekey="lblDisplayOrderMonthResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblOrderMonth" runat="server" meta:resourcekey="lblOrderMonthResource1" />
                </td>
            </tr>
            <tr runat="server" id="trOrderMonthVolume">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                        meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />: <a href="javascript:GetOrderMonthHelpText();">
                            <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                                height="12" width="12" />
                        </a>
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblOrderMonthVolume" runat="server" meta:resourcekey="lblOrderMonthVolumeResource1" name="lblOrderMonthVolume" />
                </td>
            </tr>
            <tr runat="server" ID="trDiscountRate">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayDiscountRate" Text="Discount Rate" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblDiscountRate" runat="server" meta:resourcekey="lblDiscountRateResource1" name="lblDiscountRate" />
                </td>
            </tr>
            <tr runat="server" id="trLimits" visible="false">
                <td class="gdo-details-label" colspan="2">
                    <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                </td>
            </tr>
            <tr runat="server" id="tr1" visible="false">
                <td class="gdo-details-label">
                    <asp:Panel ID="Panel1" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                </td>
            </tr>
        </table>
    </div>
    <!-- Order Total-->
    <div class="gdo-order-details-container col-md-4">
        <table class="gdo-order-details-tbl">
            <tr>
                <td colspan="2" class="gdo-totals-header">
                    <h3>
                        <asp:Label ID="lblDisplayOrderTotal" Text="Order Total" runat="server" meta:resourcekey="lblDisplayOrderTotalResource1" />
                    </h3>
                </td>
            </tr>
            <tr>
                <td class="gdo-totals-header-spacer"></td>
            </tr>
            <tr id="trVolumePoints" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayVolumePoints" Text="Volume Points" runat="server" meta:resourcekey="lblDisplayVolumePointsResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblVolumePoints" runat="server" meta:resourcekey="lblVolumePointsResource1" name="lblVolumePoints" />
                </td>
            </tr>
            <tr runat="server" id="trEarnBase">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayEarnBase" Text="Earn Base" runat="server" meta:resourcekey="lblDisplayEarnBaseResource1" />:
                    <a href="javascript:GetEarnBaseHelpText();">
                        <img id="imgEarnBase" visible="False" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                            height="12" width="12" />
                    </a>
                </td>
                <td class="gdo-details-value">
                    <%# GetSymbol() %><asp:Label ID="lblEarnBase" runat="server" meta:resourcekey="lblEarnBaseResource1" name="lblEarnBase" />
                </td>
            </tr>
            <tr runat="server" id="trRetailPrice">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayRetailPrice" Text="Retail Price" runat="server" meta:resourcekey="lblDisplayRetailPriceResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblRetailPrice" runat="server" meta:resourcekey="lblRetailPriceResource1" name="lblRetailPrice" />
                </td>
            </tr>
            <tr runat="server" id="trDiscountTotal">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayDiscountTotal" Text="Retail Price" runat="server" meta:resourcekey="lblDisplayDiscountTotal" />:
                </td>
                <td class="gdo-details-value">
                    <%# GetSymbol() %><asp:Label ID="lblDiscountTotal" runat="server" name="lblDiscountTotal" />
                </td>
            </tr>
        </table>
    </div>
    <!-- Tooltip Help Icon-->
    <div class="gdo-order-details-container-last col-md-4">
        <div class="gdo-totals-header-spacer-last">
        </div>
        <table class="gdo-order-details-tbl">
            <tr id="trDistributorSubtotal" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayDistributorSubtotal" Text="Distributor Subtotal" runat="server"
                        meta:resourcekey="lblDisplayDistributorSubtotalResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblDistributorSubtotal" runat="server" meta:resourcekey="lblDistributorSubtotalResource1" name="lblDistributorSubtotal" />
                </td>
            </tr>
            <tr id="trPackingHandling" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayPackageHandling" Text="Package & Handling" runat="server"
                        meta:resourcekey="lblDisplayPackageHandlingResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblPackageHandling" runat="server" meta:resourcekey="lblPackageHandlingResource1" name="lblPackageHandling" />
                </td>
            </tr>
            <tr id="trOtherCharges" runat="server" visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayOtherCharges" Text="Other Charges" runat="server" meta:resourcekey="lblDisplayOtherChargesResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblOtherCharges" runat="server" meta:resourcekey="lblOtherChargesResource1" />
                </td>
            </tr>
            <tr id="trLogisticCharge" runat="server" visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayLogisticCharges" Text="Logistic Charge" runat="server" meta:resourcekey="lblDisplayLocalTax" />
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblLogisticCharges" runat="server" />
                </td>
            </tr>
            <tr id="trShippingCharge" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayShippingCharges" Text="Shipping Charges" runat="server"
                        meta:resourcekey="lblDisplayShippingChargesResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblShippingCharges" runat="server" meta:resourcekey="lblShippingChargesResource1" name="lblShippingCharges" />
                </td>
            </tr>
            <tr id="trTaxVat" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayTaxVAT" Text="Tax/VAT" runat="server" meta:resourcekey="lblDisplayTaxVATResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblTaxVAT" runat="server" meta:resourcekey="lblTaxVATResource1" name="lblTaxVAT"/>
                </td>
            </tr>
            <tr id="serviceTAX" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="Label3" Text="Service Tax" runat="server" meta:resourcekey="lblDisplayServiceTaxResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblServiceTax" runat="server" meta:resourcekey="lblServiceTaxResource1" name="lblServiceTax"/>
                </td>
            </tr>
            <tr id="bharatCessTAX" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="Label4" Text="Swachh Bharat Cess" runat="server" meta:resourcekey="lblDisplayBharatCessTaxResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblBharatCessTax" runat="server" meta:resourcekey="lblBharatCessTaxResource1" name="lblBharatCessTax"/>
                </td>
            </tr>
             <tr id="cstTax" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="Label7" Text="CST Tax " runat="server" meta:resourcekey="lblDisplayCSTCessTaxResource1"  />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblCSTTax" runat="server"  name="lblCSTTax" meta:resourcekey="lblCSTCessTaxResource1"/>
                </td>
            </tr>
             <tr id="additionalTax" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="Label6" Text="Additional Tax " runat="server" meta:resourcekey="lblDisplayAdditionalCessTaxResource1"   />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblAdditionalTax" runat="server"  name="lblAdditionalTax"  meta:resourcekey="lblAdditionalCessTaxResource1"/>
                </td>
            </tr>
            <tr id="kkcTax" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="Label5" Text="Krishi Kalyan Cess @ 0.5%" runat="server" meta:resourcekey="lblDisplayKKCTaxResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblKKCTax" runat="server" meta:resourcekey="lblKKCTaxResource1" name="lblKKCTax"/>
                </td>
            </tr>
            <tr id="trSubtotal" runat="server">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplaySubtotal"
                        Text="Subtotal" runat="server"
                        meta:resourcekey="lblDisplaySubtotal" /></td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblSubtotal" runat="server"
                        meta:resourcekey="lblSubtotal" /></td>
            </tr>
            <tr id="trLocalTax" runat="server" visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayLocalTax" Text="Local Tax" runat="server" meta:resourcekey="lblDisplayLocalTax" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblLocalTax" runat="server" meta:resourcekey="lblLocalTax" />
                </td>
            </tr>
            <tr id="trAdditionalDiscount" runat="server" visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayAdditionalDiscount" Text="Additional Discount" runat="server" meta:resourcekey="lblAdditionalDiscount" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblAdditionalDiscount" runat="server" meta:resourcekey="lblAdditionalDiscount" />
                </td>
            </tr>
            <tr runat="server" id="trDiscountAmount" Visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayDiscountAmount" Text="Discount Amount" runat="server" meta:resourcekey="lblDisplayDiscountAmount" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblDiscountAmount" runat="server" />
                </td>
            </tr>
            <tr runat="server" id="trDonationAmount" Visible="false">
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayDonationAmount" Text="Donation Amount" runat="server" meta:resourcekey="lblDisplayDonationAmount" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblDonationAmount" runat="server" />
                </td>
            </tr>
            <tr>
                <td class="gdo-details-label">
                    <asp:Label ID="lblDisplayGrandTotal" Text="Grand Total" runat="server" meta:resourcekey="lblDisplayGrandTotalResource1" />:
                </td>
                <td class="gdo-details-value">
                    <asp:Label ID="lblGrandTotal" runat="server" meta:resourcekey="lblGrandTotalResource1" name="lblGrandTotal" />
                </td>
            </tr>
        </table>
    </div>
</div>
<asp:Panel ID="pnlOrderMonthVolumeText" runat="server" Style="display: none">
    <div class="gdo-popup">
        <table cellspacing="0" cellpadding="0" border="0">
            <tbody>
                <tr>
                    <td>
                        <div class="gdo-float-left gdo-popup-title">
                            <h3>
                                <asp:Label runat="server" ID="lblMonthVolume" Text="Order Month Volume" meta:resourcekey="lblDisplayOrderMonthVolumeResource1"></asp:Label>
                            </h3>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td class="gdo-form-label-left">
                        <asp:Label ID="lblOrderMonthVolumeText" runat="server" meta:resourcekey="OrderMonthVolumeInfo"
                            Text="As a non-Supervisor, order month volume shows the total of your <BR>Personally Purchased Volume plus your Downline Volume.">

                        </asp:Label>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Panel>
<asp:Panel ID="pnlEarnBaseText" runat="server" Style="display: none">
    <div class="gdo-popup">
        <table cellspacing="0" cellpadding="0" border="0">
            <tbody>
                <tr>
                    <td>
                        <div class="gdo-float-left gdo-popup-title">
                            <h3>
                                <asp:Label runat="server" ID="Label1" Text="Earn Base" meta:resourcekey="lblDisplayEarnBaseResource1"></asp:Label>
                            </h3>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td class="gdo-form-label-left">
                        <asp:Label ID="Label2" runat="server" meta:resourcekey="EarnBaseInfo"
                            Text="Earn Base help text.<BR>Earn Base help text.">
                        </asp:Label>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Panel>
