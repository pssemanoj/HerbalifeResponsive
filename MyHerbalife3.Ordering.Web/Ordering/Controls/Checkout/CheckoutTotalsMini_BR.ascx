<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutTotalsMini_BR.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutTotalsMini_BR" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>

<script type="text/javascript">
    function GetOrderMonthHelpText() {
        var tooltip = $find("<%=HoverMenuExtenderOrderMonthVolume.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
</script>
<progress:UpdatePanelProgressIndicator ID="progressOrderTotalsMiniShopping" runat="server"
    TargetControlID="upOrderTotalsMiniShopping" />
<asp:UpdatePanel ID="upOrderTotalsMiniShopping" runat="server">
    <ContentTemplate>
        <asp:Label Visible="False" runat="server" ID="lblOrderMonthChanged" meta:resourcekey="lblOrderMonthChangedResource1"></asp:Label>
        <div class="gdo-order-details-wrapper">
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
                            <asp:Label ID="lblDisplayOrderMonth" Text="Order Month*" runat="server" meta:resourcekey="lblDisplayOrderMonthResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Panel ID="pnlOrderMonth" runat="server" meta:resourcekey="pnlOrderMonthResource1" />
                            <asp:Panel runat="server" ID="pnlOrderMonthLabel" meta:resourcekey="pnlOrderMonthLabelResource1">
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                                meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />: <a href="javascript:GetOrderMonthHelpText();">
                                    <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                                        height="12" width="12" /></a>
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonthVolume" runat="server" meta:resourcekey="lblOrderMonthVolumeResource1" />
                        </td>
                    </tr>
                    <tr runat="server" id="divDiscountRate">
                        <td class="gdo-details-label" runat="server">
                            <asp:Label ID="lblDisplayDiscountRate" Text="Discount Rate" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                        </td>
                        <td class="gdo-details-value" runat="server">
                            <asp:Label ID="lblDiscountRate" runat="server" meta:resourcekey="lblDiscountRateResource1" />
                        </td>
                    </tr>
                     <tr>
                        <td colspan="2" class="gdo-details-label">
                            <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1"
                                Visible="false" />
                        </td>
                    </tr>
                </table>
            </div>
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
   
                </table>
            </div>
            <div class="gdo-order-details-container-last" runat="server" id="divDistributorSubtotal">
                <div class="gdo-totals-header-spacer-last">
                </div>
                <table class="gdo-order-details-tbl">
                </table>
            </div>
        </div>
        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server"
            TargetControlID="imgOrderMonth" PopupControlID="pnlOrderMonthVolumeText" DynamicServicePath=""
            Enabled="True" />
    </ContentTemplate>
</asp:UpdatePanel>
<asp:Panel ID="pnlOrderMonthVolumeText" runat="server" Style="display: none" meta:resourcekey="pnlOrderMonthVolumeTextResource1">
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
                            Text="As a non-Supervisor, order month volume shows the total of your <BR>Personally Purchased Volume plus your Downline Volume."></asp:Label>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</asp:Panel>