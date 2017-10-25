<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductDetailPopup.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductDetailPopup" %>
<%@ Register TagPrefix="gv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>

<asp:Panel ID="pnlProductDetailControl" runat="server" Style="display: none; background-color: White" CssClass="absolute">
    <progress:UpdatePanelProgressIndicator ID="progressPaymentInfoPopup" runat="server" TargetControlID="upProductDetailControl"/>
    <asp:UpdatePanel ID="upProductDetailControl" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="TB_window_ProductDetail" class="productDetailPopUp" style="display: block;">
                <div id="TB_ajaxContent">
                    <div class="<% if(HLConfigManager.Configurations.DOConfiguration.IsChina) { %>gdo-popup searchresultpopup<% }else{ %>hrblModalSkinOnly<% } %>">
                        <asp:Panel ID="PanelProductDetail" runat="server" Width="100%">
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<gv:ProductDetailModalPopupExtender ID="popup_ProductDetailControl" runat="server"
    TargetControlID="lnkHidden" PopupControlID="pnlProductDetailControl" RepositionMode="None"
    CancelControlID="lnkHidden" BackgroundCssClass="modalBackground" DynamicServicePath=""
    Enabled="True" Y="30">
</gv:ProductDetailModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>

    <script type="text/javascript">

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);

        function EndRequestHandler(sender, args) {

            <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) { %>
                jQuery('.myhl2 [id$="upProductDetailControl"] .hrblModalSkinOnly').css({
                    'max-height': $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)),
                    'overflow-y': 'scroll',
                    'width': $(window).outerWidth() + 20
                });

                jQuery(window).resize(function () {
                    jQuery('.myhl2 [id$="upProductDetailControl"] .hrblModalSkinOnly').css({
                        'max-height': $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)),
                        'overflow-y': 'scroll',
                        'width': $(window).outerWidth() + 20
                    });
                });
            <% } %>

            <% if (!HLConfigManager.Configurations.DOConfiguration.IsChina) { %>
                // Recalculate on Scroll
                jQuery(window).resize(function () { repositionProductDetailModal($("#<%=pnlProductDetailControl.ClientID%>")) });

                setTimeout(function () { repositionProductDetailModal($("#<%=pnlProductDetailControl.ClientID%>")) }, 50);
            <% } %>

            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(EndRequestHandler);
        }

    </script>
