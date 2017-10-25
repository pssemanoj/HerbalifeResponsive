<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PurchasingLimits_IN.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PurchasingLimits_IN" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<progress:UpdatePanelProgressIndicator ID="progressPurhasingLimitUpdatePanel" runat="server"
    TargetControlID="purhasingLimitUpdatePanel" />
<asp:UpdatePanel ID="purhasingLimitUpdatePanel" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
        <table style="width:100%" border="0" cellspacing="0" cellpadding="0" id="RemainingVolume">
            <tr id="trRemainingVal" runat="server">
                <td class="gdo-minicart-label">
                    <asp:Label runat="server" ID="lblRemainingValDisplay" Text="Remaining Volume" ></asp:Label>
                </td>
                <td align="right" valign="bottom" class="gdo-minicart-value">
                    <asp:Label ID="lblRemainingVal" runat="server"></asp:Label>
                </td>
            </tr>
        </table>
    </ContentTemplate>
</asp:UpdatePanel>
