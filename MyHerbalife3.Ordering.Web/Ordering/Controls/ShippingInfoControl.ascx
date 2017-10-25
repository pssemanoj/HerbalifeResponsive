<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ShippingInfoControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ShippingInfoControl" ClassName="ShippingInfoControlClass" %>
<%@ Register TagPrefix="gv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>

<script type="text/javascript">
<% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
   { %>
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        $('#TB_ajaxContent .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
    }
<% } %>
</script>
<asp:Panel ID="pnlShippingInfoControl" runat="server" Style="display: none; background-color: White; border: 10px solid #ccc;">
    <asp:UpdatePanel ID="upShippingInfoControl" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="TB_window" style="margin-left: 0px; margin-top: 0px; display: block;">
                <div id="TB_ajaxContent">
                    <div class="gdo-popup">
                        <table>
                            <%-- <tr runat="server" id="trTypeSelection">
                                <td>
                                    <div class="gdo-right-column-labels">
                                        <asp:Label runat="server" ID="Type" Text="Type:"></asp:Label></div>
                                </td>
                                <td>
                                    <div class="gdo-select-box">
                                        <asp:DropDownList AutoPostBack="True" runat="server" ID="DeliveryType" OnSelectedIndexChanged="OnDeliveryTypeChanged">
                                            <asp:ListItem Text="Shipping" Value="Shipping"></asp:ListItem>
                                            <asp:ListItem Text="Pickup" Value="Pickup"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </td>
                            </tr>--%>
                            <tr>
                                <td colspan="2">
                                    <asp:Panel ID="PanelShipping" runat="server">
                                    </asp:Panel>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Panel ID="PanelPickup" runat="server">
                                    </asp:Panel>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<gv:ShippingModalPopupExtender ID="popup_ShippingInfoControl" runat="server" RepositionMode="RepositionOnWindowResizeAndScroll" TargetControlID="lnkHidden"
    PopupControlID="pnlShippingInfoControl" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground">
</gv:ShippingModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>