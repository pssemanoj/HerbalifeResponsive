<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutTotalsDetailed_BR.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutTotalsDetailed_BR" %>
<script type="text/javascript">
    function GetOrderMonthHelpText() {
        var tooltip = $find("<%=HoverMenuExtenderOrderMonthVolume.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
</script>
<asp:UpdatePanel ID="pnlTotalsUpdate" runat="server">
    <ContentTemplate>
        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server"
            TargetControlID="imgOrderMonth" PopupControlID="pnlOrderMonthVolumeText" />
        <div class="gdo-order-details-wrapper">
            <!-- Order Details -->
            <div class="gdo-order-details-container">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td colspan="2" class="gdo-totals-header">
                            <asp:Label ID="lblDisplayOrderDetails" Text="Order Details" runat="server" meta:resourcekey="lblDisplayOrderDetailsResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-totals-header-spacer">
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonth" Text="Order Month" runat="server" meta:resourcekey="lblDisplayOrderMonthResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonth" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                                meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />: <a href="javascript:GetOrderMonthHelpText();">
                                    <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                                        height="12" width="12" />
                                </a>
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonthVolume" runat="server" meta:resourcekey="lblOrderMonthVolumeResource1" />
                        </td>
                    </tr>
                    <tr runat="server" id="divDiscountRate">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDiscountRate" Text="Volume Point Range" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblDiscountRate" runat="server" meta:resourcekey="lblDiscountRateResource1" />
                        </td>
                    </tr>
                    <tr runat="server" id="trLimits" visible="false">
                        <td class="gdo-details-label">
                            <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                        </td>
                    </tr>
                </table>
            </div>
            <!-- Order Total-->
            <div class="gdo-order-details-container">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td colspan="2" class="gdo-totals-header">
                            <asp:Label ID="lblDisplayOrderTotal" Text="Order Total" runat="server" meta:resourcekey="lblDisplayOrderTotalResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-totals-header-spacer">
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayVolumePoints" Text="Volume Points" runat="server" meta:resourcekey="lblDisplayVolumePointsResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblVolumePoints" runat="server" meta:resourcekey="lblVolumePointsResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayRetailPrice" Text="Retail Price" runat="server" meta:resourcekey="lblDisplayRetailPriceResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblRetailPrice" runat="server" meta:resourcekey="lblRetailPriceResource1" />
                        </td>
                    </tr>
                </table>
            </div>
            <!-- Tooltip Help Icon-->
            <div class="gdo-order-details-container-last">
                <div class="gdo-totals-header-spacer-last">
                </div>
                <table class="gdo-order-details-tbl">
                    <asp:Panel ID="pnlForProductandPromoteRetail" runat="server">
                        <tr>
                            <td class="gdo-details-label">
                                <asp:Label ID="lblDisplayDiscountedProductRetail" Text="" runat="server" meta:resourcekey="lblDisplayDiscountedProductRetailRes" />:
                            </td>
                            <td class="gdo-details-value">
                                <asp:Label ID="lblDiscountedProductRetail" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label">
                                <asp:Label ID="lblDisplayDiscountedPromoteRetail" Text="" runat="server" meta:resourcekey="lblDisplayDiscountedPromoteRetailRes" />:
                            </td>
                            <td class="gdo-details-value">
                                <asp:Label ID="lblDiscountedPromoteRetail" runat="server" />
                            </td>
                        </tr>
                    </asp:Panel>
                    <asp:Panel ID="pnlForLiteratureRetail" runat="server">
                        <tr>
                            <td class="gdo-details-label">
                                <asp:Label ID="lblDisplayDiscountedLiteratureRetail" Text="" runat="server" meta:resourcekey="lblDisplayDiscountedLiteratureRetailRes" />:
                            </td>
                            <td class="gdo-details-value">
                                <asp:Label ID="lblDiscountedLiteratureRetail" runat="server" />
                            </td>
                        </tr>
                    </asp:Panel>
                    <tr id="trPackingHandling" runat="server">
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayPackageHandling" Text="Package & Handling" runat="server"
                                meta:resourcekey="lblDisplayPackageHandlingResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblPackageHandling" runat="server" meta:resourcekey="lblPackageHandlingResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayFreightCharges" Text="Freight Charges" runat="server" meta:resourcekey="lblDisplayFreightChargesRes" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblFreightCharges" runat="server" />
                        </td>
                    </tr>
                    <%-- <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplaySubtotal" Text="Subtotal" runat="server" meta:resourcekey="lblDisplaySubtotal" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lbSubtotal" runat="server" />
                        </td>
                    </tr>--%>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayICMS" Text="ICMS" runat="server" meta:resourcekey="lblDisplayICMSRes" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblICMS" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayIPI" Text="IPI" runat="server" meta:resourcekey="lblDisplayIPIRes" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblIPI" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayGrandTotal" Text="Grand Total" runat="server" meta:resourcekey="lblDisplayGrandTotalResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblGrandTotal" runat="server" meta:resourcekey="lblGrandTotalResource1" />
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
    </ContentTemplate>
</asp:UpdatePanel>