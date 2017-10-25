<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckOutTotalsMini_IT.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckOutTotalsMini_IT" %>
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
</script>
<progress:UpdatePanelProgressIndicator ID="progressOrderTotalsMiniIT" runat="server"
    TargetControlID="upOrderTotalsMiniIT" />
<asp:UpdatePanel ID="upOrderTotalsMiniIT" runat="server">
    <ContentTemplate>
         <div id="divLabelErrors" runat="server" class="gdo-edit-header" style="border-bottom: 0px">
            <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red" meta:resourcekey="blErrorsResource1">
            </asp:BulletedList>
        </div>
        <asp:Label Visible="False" runat="server" ID="lblOrderMonthChanged" meta:resourcekey="lblOrderMonthChangedResource1"></asp:Label>
        <div class="gdo-order-details-wrapper">
            <%--Order Details--%>
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
                            <asp:Panel runat="server" ID="pnlOrderMonthLabel">
                                        </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderMonthVolume" Text="Order Month Volume" runat="server"
                                meta:resourcekey="lblDisplayOrderMonthVolumeResource1" />:
                                <a href="javascript:GetOrderMonthHelpText();">
                                 <img id="imgOrderMonth" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif" height="12" width="12"/>
                            </a>
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderMonthVolume" runat="server" meta:resourcekey="lblOrderMonthVolumeResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDiscountRate" Text="Discount Rate" runat="server" meta:resourcekey="lblDisplayDiscountRateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblDiscountRate" runat="server" meta:resourcekey="lblDiscountRateResource1" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gdo-details-label" colspan="2">
                            <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                        </td>
                    </tr>
                </table>
            </div>
            <%--Order Total--%>
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
                    <tr runat="server" id="trEarnBase">
                        <td class="gdo-details-label" runat="server">
                            <asp:Label ID="lblDisplayEarnBase" Text="Earn Base" runat="server" meta:resourcekey="lblDisplayEarnBaseResource1" />:
                            <a href="javascript:GetEarnBaseHelpText();">
                                <img id="imgEarnBase" Visible="False" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                                    height="12" width="12" />
                            </a>
                        </td>
                        <td class="gdo-details-value" runat="server">
                            <%# GetSymbol() %><asp:Label ID="lblEarnBase" runat="server" />
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
            <%--Tooltip Help Icon--%>
            <div class="gdo-order-details-container-last">
                <div class="gdo-totals-header-spacer-last">
                </div>
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayDistributorSubtotal" Text="Distributor Subtotal" runat="server"
                                meta:resourcekey="lblDisplayDistributorSubtotalResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblSubtotal" runat="server" meta:resourcekey="lblSubtotalResource1" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server" TargetControlID="imgOrderMonth"
                PopupControlID="pnlOrderMonthVolumeText"/>
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