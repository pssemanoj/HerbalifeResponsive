<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutTotalsDetailed_KR.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutTotalsDetailed_KR" %>
<script type="text/javascript">
    function GetOrderMonthHelpText() {
        var tooltip = $find("<%=HoverMenuExtenderOrderMonthVolume.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
</script>
 <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server" TargetControlID="imgOrderMonth"
                PopupControlID="pnlOrderMonthVolumeText"/>
<div class="gdo-order-details-wrapper">
    <table>
        <tr>
            <td colspan="3" class="gdo-order-details-container">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-totals-header">
                            <asp:Label ID="lblDisplayOrderSummary" Text="Order Summary" runat="server" meta:resourcekey="lblDisplayOrderSummaryResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-totals-header-spacer">
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="gdo-order-details-container" valign="top">
                <!-- Order Details -->
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label" style="width:180px">
                            <asp:Label ID="lblDisplayOrderMonth" Text="Order Month" runat="server" meta:resourcekey="lblDisplayOrderMonthResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonth" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                                meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />:
                            <a href="javascript:GetOrderMonthHelpText();">
                                <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif" height="12" width="12" />
                            </a>
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonthVolume" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDiscountRate" Text="Discount Rate" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblDiscountRate" runat="server" />
                        </td>
                    </tr>
                    <tr runat="server" id="trLimits" visible="false">
                        <td class="gdo-details-label" colspan="2">
                            <asp:Panel ID="pnlPurchaseLimits" runat="server"/>
                        </td>
                    </tr>
                </table>
            </td>
            <!-- Order Total-->
            <td class="gdo-order-details-container" valign="top">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayVolumePoints" Text="Volume Points" runat="server" meta:resourcekey="lblDisplayVolumePointsResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblVolumePoints" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayRetailPrice" Text="Retail Price" runat="server" meta:resourcekey="lblDisplayRetailPriceResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblRetailPrice" runat="server" />
                        </td>
                    </tr>
                </table>
            </td>
            <!-- Tooltip Help Icon-->
            <td class="gdo-order-details-container-last"  valign="top">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDistributorTotal" Text="Distributor Subtotal" runat="server"
                                meta:resourcekey="lblDisplayDistributorTotalResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblGrandTotal" runat="server" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
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