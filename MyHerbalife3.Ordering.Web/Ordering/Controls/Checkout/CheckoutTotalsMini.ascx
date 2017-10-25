<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutTotalsMini.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutTotalsMini" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>

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

    function isNumber(evt) {
        evt = (evt) ? evt : window.event;
        var charCode = (evt.which) ? evt.which : evt.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }
        return true;
    }
</script>
<progress:UpdatePanelProgressIndicator ID="progressOrderTotalsMiniShopping" runat="server"
    TargetControlID="upOrderTotalsMiniShopping" />
<asp:UpdatePanel ID="upOrderTotalsMiniShopping" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hasValidationErrors" runat="server" />
        <asp:Label Visible="False" runat="server" ID="lblOrderMonthChanged" meta:resourcekey="lblOrderMonthChangedResource1"></asp:Label>
        <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red" meta:resourcekey="blErrorsResource1"></asp:BulletedList>
        <div class="gdo-order-details-wrapper">
            <%--Order Details--%>
            <div class="gdo-order-details-container col-md-4">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td colspan="2" class="gdo-totals-header">
                            <asp:Label ID="lblDisplayOrderDetails" Text="Order Details" runat="server" meta:resourcekey="lblDisplayOrderDetailsResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-totals-header-spacer"></td>
                    </tr>
                    <tr runat="server" id="trOrderMonth">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonth" Text="Order Month*" runat="server" meta:resourcekey="lblDisplayOrderMonthResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Panel ID="pnlOrderMonth" runat="server" meta:resourcekey="pnlOrderMonthResource1" name="pnlOrderMonth" />
                            <asp:Panel runat="server" ID="pnlOrderMonthLabel" name="pnlOrderMonthLabel">
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr runat="server" id="trOrderMonthVP">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                                meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />:
                            <a href="javascript:GetOrderMonthHelpText();">
                                <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif" height="12" width="12" />
                            </a>
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonthVolume" runat="server" meta:resourcekey="lblOrderMonthVolumeResource1" name="lblOrderMonthVolume" />
                        </td>
                    </tr>
                    <tr runat="server" id="trDiscountRate">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDiscountRate" Text="Discount Rate" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblDiscountRate" runat="server" meta:resourcekey="lblDiscountRateResource1" name="lblDiscountRate" />
                        </td>
                    </tr>
                    <tr runat="server" ID="trYourLevel">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayYourLevel" Text="Your Level" runat="server" meta:resourceKey="lblDisplayYourLevelResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblYourLevel" runat="server" meta:resourceKey="lblYourLevelResource1" name="lblYourLevel" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1"
                                Visible="false" />
                        </td>
                    </tr>
                </table>
            </div>
            <%--Order Total--%>
            <div class="gdo-order-details-container col-md-4">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td colspan="2" class="gdo-totals-header">
                            <asp:Label ID="lblDisplayOrderTotal" Text="Order Total" runat="server" meta:resourcekey="lblDisplayOrderTotalResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-totals-header-spacer"></td>
                    </tr>
                    <tr runat="server" id="trVolumePoints">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayVolumePoints" Text="Volume Points" runat="server" meta:resourcekey="lblDisplayVolumePointsResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblVolumePoints" runat="server" meta:resourcekey="lblVolumePointsResource1" name="lblVolumePoints" />
                        </td>
                    </tr>
                    <tr runat="server" id="trEarnBase">
                        <td class="gdo-details-label" runat="server">
                            <asp:Label ID="lblDisplayEarnBase" Text="Earn Base" runat="server" meta:resourcekey="lblDisplayEarnBaseResource1" />:
                            <a href="javascript:GetEarnBaseHelpText();">
                                <img id="imgEarnBase" visible="False" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                                    height="12" width="12" />
                            </a>
                        </td>
                        <td class="gdo-details-value" runat="server">
                            <%# GetSymbol() %><asp:Label ID="lblEarnBase" runat="server" name="lblEarnBase" />
                        </td>
                    </tr>
                    <tr runat="server" id="trRetailPrice">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayRetailPrice" Text="Retail Price" runat="server" meta:resourcekey="lblDisplayRetailPriceResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <%# GetSymbol() %><asp:Label ID="lblRetailPrice" runat="server" meta:resourcekey="lblRetailPriceResource1" name="lblRetailPrice" />
                        </td>
                    </tr>
                    <tr runat="server" id="trWeight">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayWeight" Text="Total Weight (Kg):" runat="server" meta:resourcekey="lblDisplayWeightResource" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblWeight" runat="server" meta:resourcekey="lblRetailPriceResource1" name="lblWeight" />
                        </td>
                    </tr>
                </table>
            </div>
            <%--Tooltip Help Icon--%>
            <div class="gdo-order-details-container-last col-md-4" runat="server" id="divDistributorSubtotal">
                <div class="gdo-totals-header-spacer-last">
                </div>
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDistributorSubtotal" Text="Distributor Subtotal" runat="server"
                                meta:resourcekey="lblDisplayDistributorSubtotalResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <%# GetSymbol() %><asp:Label ID="lblSubtotal" runat="server" meta:resourcekey="lblSubtotalResource1" name="lblSubtotal" />
                        </td>
                    </tr>
                    <tr id="trPackingHandling" runat="server">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayPackageHandling" Text="Package & Handling" runat="server"
                                meta:resourcekey="lblDisplayPackageHandlingResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblPackageHandling" runat="server" name="lblPackageHandling" />
                        </td>
                    </tr>
                    <tr id="trOtherCharges" runat="server" visible="false">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOtherCharges" Text="Other Charges" runat="server" meta:resourcekey="lblDisplayOtherChargesResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOtherCharges" runat="server" />
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
                            <asp:Label ID="lblDisplayShippingCharges" Text="Shipping Charges" runat="server" Font-Bold="true"
                                meta:resourcekey="lblDisplayShippingChargesResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblShippingCharges" runat="server" name="lblShippingCharges" Font-Bold="true"/>
                        </td>
                    </tr>
                    <tr id="trTaxVat" runat="server">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayTaxVAT" Text="Tax/VAT" runat="server" meta:resourcekey="lblDisplayTaxVATResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblTaxVAT" runat="server" name="lblTaxVAT" />
                        </td>
                    </tr>
                    <tr id="trLocalTax" runat="server" visible="false">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayLocalTax" Text="Local Tax" runat="server" meta:resourcekey="lblDisplayLocalTax" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblLocalTax" runat="server" />
                        </td>
                    </tr>
                    <tr runat="server" id="trDiscountTotal">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDiscountTotal" Text="Additional Discount" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" name="lblDisplayDiscountTotal" />:
                        </td>
                        <td class="gdo-details-value">
                            <%# GetSymbol() %><asp:Label ID="lblDiscountTotal" runat="server" />
                        </td>
                    </tr>
                    <tr runat="server" id="trDonationTotal" visible="false">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDonationTotal" Text="Donation" runat="server" meta:resourcekey="lblDisplayDonationTotal" />:
                        </td>
                        <td class="gdo-details-value">
                            <%# GetSymbol() %><asp:Label ID="lblDonationTotal" runat="server" />
                        </td>
                    </tr>
                    <tr id="trGrandTotal" runat="server">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayGrandTotal" runat="server" meta:resourcekey="lblDisplayGrandTotalResource1" Text="Grand Total" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblGrandTotal" runat="server" name="lblGrandTotal" />
                        </td>
                    </tr>
                    <tr id="trPCLearningPoint" runat="server" visible="false">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayPCLearningPoint" runat="server" meta:resourcekey="lblPCLearningPointTotalResource1" Text="PC Learning Point" />
                        </td>
                        <td class="gdo-details-value">
                            <asp:TextBox ID="txtPCLearningPoint" runat="server" name="txtPCLearnedPoint"  size="2" maxlength="5"  onkeypress="return isNumber(event)"  oncopy="return false" onpaste="return false" oncut="return false" />
                        </td>
                    </tr>
                    <tr id="trPCLearningPointLimit" runat="server" visible="false">
                        <td class="gdo-details-label">
                             <asp:Label ID="lblEligiblePCLearningPoint" runat="server" Text="" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server" TargetControlID="imgOrderMonth"
            PopupControlID="pnlOrderMonthVolumeText" />
        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderEarnBase" runat="server"
            TargetControlID="imgEarnBase" PopupControlID="pnlEarnBaseText" />
    </ContentTemplate>
</asp:UpdatePanel>
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
