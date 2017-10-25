<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HAPCheckoutOptions.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HAP.HAPCheckoutOptions" %>

<asp:UpdatePanel ID="upHAPCheckoutOptions" runat="server">
    <ContentTemplate>
        <div class="gdo-order-details-wrapper">
            <asp:Label ID="lbOrderTotalsDisclaimer" runat="server" Text="" meta:resourcekey="lbOrderTotalsDisclaimerResource1" CssClass="gdo-right-column-text-blue" style="float:left; margin-bottom:10px;"></asp:Label>
            <div class="gdo-order-details-container col-md-12"></div>
            <table class="gdo-order-details-tbl">
                <tr>
                    <td colspan="2" class="gdo-totals-header">
                        <asp:Label ID="lblDisplayHAPDetails" Text="Recurring Order Details" runat="server" meta:resourcekey="lblDisplayHAPDetailsResource1" />
                    </td>
                </tr>
                <tr>
                    <td class="gdo-totals-header-spacer"></td>
                </tr>
            </table>
            <%--HAP Order Type--%>
            <div class="gdo-order-details-container col-md-4">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayOrderType" Text="Recurring Order Type" runat="server" meta:resourcekey="lblDisplayOrderTypeResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblOrderType" Text="Personal" runat="server" meta:resourcekey="lblOrderTypeResource1" />
                        </td>
                    </tr>
                </table>
            </div>
            <%--HAP Deadline Date--%>
            <div class="gdo-order-details-container col-md-4">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayScheduleDate" Text="HAP Deadline Date" runat="server" meta:resourcekey="lblDisplayScheduleDateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblScheduleDate" Text="4th" runat="server" meta:resourcekey="lblScheduleDateResource1" />
                        </td>
                    </tr>
                </table>
            </div>
            <%--HAP Expiration Date--%>
            <div class="gdo-order-details-container-last col-md-4">
                <table class="gdo-order-details-tbl">
                    <tr>
                        <td class="gdo-details-label">
                            <asp:Label ID="lblDisplayExpDate" Text="Expiration Date" runat="server" meta:resourcekey="lblDisplayExpDateResource1" />:
                        </td>
                        <td class="gdo-details-value">
                            <asp:Label ID="lblExpDate" Text="08/04/2016" runat="server" meta:resourcekey="lblExpDateResource1" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
