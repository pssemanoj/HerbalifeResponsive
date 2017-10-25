<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MiniCart.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.MiniCart" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc3" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<div id="gdo-right-column-minicart">
    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
        function GetRtHelp() {
            var tooltip = $find("<%=ModalpnlHelp.ClientID%>").show();
            event.returnValue = false;
            return false;
        }

        function HideRtHelp() {
            var tooltip = $find("<%=ModalpnlHelp.ClientID%>").hide();
            event.returnValue = false;
            return false;

        }
        function EndRequestHandler(sender, args) {
            try {
                // clear the width property
                if (document.getElementById('<%= upMiniCart.ClientID %>').style.filter) {
                    document.getElementById('<%= upMiniCart.ClientID %>').style.filter = "";
                }
            }
            catch (e) {
            }
        }

        function GetOrderMonthHelpText() {
            var tooltip = $find("<%=HoverMenuExtenderOrderMonthVolume.ClientID%>").show();
            event.returnValue = false;
            return false;
        }

    </script>
    <div class="gdo-right-column-tbl">
        <div class="minicart-icon">
            <img width="16" height="16" alt="mini cart" src="/Content/Global/img/gdo/icons/mini_cart.png" /></div>
        <div class="gdo-right-column-header">
            <h3>
                <asp:Label runat="server" ID="TextMiniCart" Text="Mini Cart" meta:resourcekey="TextMiniCartResource1"></asp:Label></h3>
        </div>
        <div style="display: none;">
            <a href="javascript:GetRtHelp();">
                <img alt="image" src="/Content/Global/img/gdo/icons/question-mark-blue.png" class="gdo-question-mark-blue" /></a></div>
        <div class="gdo-clear gdo-horiz-div">
        </div>
        <div runat="server" id="SavedCartTitle" visible="false" class="headersavedcart">
            <asp:Label ID="lblSavedCartName" runat="server" meta:resourcekey="lblSavedCartName"></asp:Label>
        </div>
        <div>
            <h4>
                <asp:Label runat="server" ID="RecentlyAdded" Text="Recently Added" meta:resourcekey="RecentlyAddedResource1"></asp:Label></h4>
        </div>
    </div>
    <progress:UpdatePanelProgressIndicator ID="progressProductDetail" runat="server"
        TargetControlID="upMiniCart" />
    <asp:UpdatePanel ID="upMiniCart" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-mini-cart-div">
                <div>
                    <p class="left">
                        <asp:Label runat="server" ID="noItemToPurchase" CssClass="red" Text="There are no items to purchase in the order.<br>"
                            Visible="False" meta:resourcekey="noItemToPurchaseResource1" />
                        <asp:Label runat="server" ID="noItem" Text="no items in your cart<br>" Visible="False"
                            meta:resourcekey="noItemResource1" />
                        <asp:Label runat="server" ID="errDRFraud" CssClass="red errorMinicart" Visible="False" />
                    </p>
                </div>
                <asp:Panel ID="CartPanel" runat="server" ScrollBars="Auto" class="gdo-mini-cart-div">
                    <asp:ListView runat="server" ID="CartItemListView" OnItemDataBound="CartItemListView_ItemDataBound">
                        <LayoutTemplate>
                            <ul>
                                <li runat="server" id="itemPlaceholder"></li>
                            </ul>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <li>
                                <table border="0" cellspacing="0" cellpadding="0">
                                    <tr class="<%# Container.DisplayIndex % 2 == 0 ? "gdo-right-column-text gdo-row-odd" : "gdo-right-column-text gdo-row-even" %>">
                                        <td class="cart-item-qty">
                                            <%#Eval("Quantity")%>&nbsp;&nbsp;
                                        </td>
                                        <td class="cart-item-desc">
                                            <cc1:DynamicButton CssClass="left" ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                                CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                                Text='<%# string.Format("{0}", Eval("Description")) %>' ButtonType="Link" />
                                            <asp:HiddenField ID="hdnSKU" runat="server" Value='<%#Eval("SKU")%>' />
                                        </td>
                                    </tr>
                                </table>
                            </li>
                        </ItemTemplate>
                    </asp:ListView>
                </asp:Panel>
                <div class="gdo-mini-cart-container-bottom">
                    <div>
                        <p>
                            <asp:Label runat="server" ID="TotalQty" meta:resourcekey="TotalQtyResource1"></asp:Label>
                            <asp:Label runat="server" ID="lbNumItems" Text="item(s) in cart" meta:resourcekey="lbNumItemsResource1"></asp:Label>
                        </p>
                        <p>
                            <cc1:DynamicButton ID="ViewEntireCart"  runat="server" OnClick="ViewEntireCart_Clicked" name="ViewEntireCart"
                                Text="View Entire Shopping Cart" ButtonType="Link" meta:resourcekey="ViewEntireCartResource1" />
                        </p>
                        <p>
                            <cc1:DynamicButton ID="ClearCart" runat="server" Text="Clear Cart" ButtonType="Link"
                                meta:resourcekey="ClearCartResource1" name="ClearCart"/>
                        </p>
                    </div>
                    <div class="gdo-show">
                        <div class="gdo-spacer1">
                        </div>
                        <div class="gdo-right-column-header">
                            <h3>
                                <asp:Label runat="server" ID="TxtOrderTotals" Text="Order Totals" meta:resourcekey="TxtOrderTotalsResource1"></asp:Label></h3>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div class="gdo-input-container">
                            <table width="165" border="0" cellspacing="0" cellpadding="0" class="gdo-order-totals-tbl">
                                <tr runat="server" id="trOrderMonth">
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="TxtOrderMonth" Text="Order Month:" meta:resourcekey="TxtOrderMonthResource1"></asp:Label>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Panel runat="server" ID="pnlOrderMonth" meta:resourcekey="pnlOrderMonthResource1">
                                        </asp:Panel>
                                        <asp:Panel runat="server" ID="pnlOrderMonthLabel">
                                        </asp:Panel>
                                    </td>
                                </tr>
                                <tr runat="server" id="trOrderMonthVP">
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="lblDisplayOrderMonthVolume" Text="Order Month Volume:"
                                            meta:resourcekey="lblDisplayOrderMonthVolumeResource1">
                                        </asp:Label>
                                        <a href="javascript:GetOrderMonthHelpText();">
                                            <img id="imgOrderMonth" runat="server" src="/Content/global/Events/cruise/img/icon_info.gif"
                                                height="12" width="12" />
                                        </a>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label runat="server" ID="lblOrderMonthVolume"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trSubtotal">
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="TxtSubtotal" Text="Subtotal:" meta:resourcekey="TxtSubtotalResource1"></asp:Label>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label runat="server" ID="uxSubtotal" meta:resourcekey="uxSubtotalResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="TxtVolumePoint" Text="Volume Point:" meta:resourcekey="TxtVolumePointResource1"></asp:Label>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label ID="uxVolume" runat="server" Text="0.00" meta:resourcekey="uxVolumeResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr id="trDiscountRate" runat="server">
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="TxtCurrentDiscountRate" Text="Current Discount Rate:"
                                            meta:resourcekey="TxtCurrentDiscountRateResource1"></asp:Label>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label ID="uxDiscountRate" runat="server" Text="0.0%" meta:resourcekey="uxDiscountRateResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr id="trYourLevel" runat="server">
                                    <td class="gdo-minicart-label">
                                        <asp:Label ID="TxtYourLevel" runat="server" Text="Your Level:" meta:resourcekey="TxtYourLevelResource1" />
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label ID="uxYourLevel" runat="server" meta:resourcekey="uxYourLevelResource1" />
                                    </td>
                                </tr>
                                <tr id="trVolumePointRange" runat="server" visible="false">
                                    <td class="gdo-minicart-label">
                                        <asp:Label runat="server" ID="TxtVolumePointRange" Text="Volume Point Range:" meta:resourcekey="TxtVolumePointRangeResource1"></asp:Label>
                                    </td>
                                    <td align="right" valign="bottom" class="gdo-minicart-value">
                                        <asp:Label ID="uxVolumePointRange" runat="server" Text="0" meta:resourcekey="uxVolumePointRangeResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr id="trPurchaseLimits" runat="server" visible="false">
                                    <td colspan="2">
                                        <asp:Panel ID="pnlPurchaseLimits" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                                    </td>
                                </tr>
                                <tr id="trHAPControl" runat="server" visible="false">
                                    <td colspan="2">
                                        <asp:Panel ID="pnlHAP" runat="server" meta:resourcekey="pnlPurchaseLimitsResource1" />
                                    </td>
                                </tr>
                                
                                
                                

                                <tr>
                                    <td colspan="2">
                                        <div class="gdo-edit-links-conatainer">
                                            <cc1:DynamicButton ID="CheckoutButton" runat="server" ButtonType="Forward" name="CheckoutButton"
                                                    IconType="ArrowRight" Text="Checkout" OnClick="checkoutClicked" meta:resourcekey="CheckoutButtonResource1" />                                            
                                            <div runat="server" id="divSavedCartCommands" class="gdo-button-margin-rt bttn-recalculate">
                                                <asp:PlaceHolder ID="plSavedCartCommand" runat="server" />
                                            </div>                                            
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
            <ajaxToolkit:ModalPopupExtender ID="mdlConfirmClearCart" runat="server" TargetControlID="ClearCart"
                PopupControlID="pnlConfirmClearCart" CancelControlID="btnNo" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderOrderMonthVolume" runat="server"
                TargetControlID="imgOrderMonth" PopupControlID="pnlOrderMonthVolumeText" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:Panel ID="pnlHelp" Style="display: none;" runat="server">
        <asp:UpdatePanel ID="upnlHelp" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlHelpInner">
                    <table class="popup-layout-table">
                        <tr>
                            <td class="popup-content">
                                <div id="divPnlHelp" class="popwrapper">
                                    <table class="tablecontent">
                                        <tr>
                                            <td>
                                                <!-- content -->
                                                <asp:Panel ID="pnlError" runat="server" CssClass="error-message" Visible="false">
                                                    <!-- error message -->
                                                </asp:Panel>
                                                some content test
                                                <br />
                                                some content test
                                                <br />
                                                some content test
                                                <br />
                                                some content test
                                                <br />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div id="divPnlHelpCommands" class="popup-buttons">
                                    <!-- command buttons -->
                                    <div id="divCmdClose" class="unique-ID-button">
                                        <cc3:OvalButton runat="server" ID="btnRtHelpClose" Text="Close" Coloring="Silver"
                                            ArrowDirection="" IconPosition="" />
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <cc2:ModalPopupExtender ID="ModalpnlHelp" runat="server" TargetControlID="lnkHidden"
        PopupControlID="pnlHelp" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
        DynamicServicePath="" Enabled="True">
    </cc2:ModalPopupExtender>
    <asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;"></asp:LinkButton>
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
<asp:Panel ID="pnlConfirmClearCart" runat="server" Style="display: none">
    <div class="gdo-popup confirmClearCart">
        <div class="gdo-float-left gdo-popup-title">
            <h2>
                <asp:Label ID="lblConfirmClearCart" runat="server" Text="Confirmation" meta:resourcekey="lblConfirmClearCart"></asp:Label></h2>
        </div>
        <div class="gdo-form-label-left miniCartPopUp">
            <asp:Label ID="lblConfirmClearCartText" runat="server" Text="Are you sure you wish to clear the cart ?"
                meta:resourcekey="lblConfirmClearCartText"></asp:Label>
        </div>
        <div class="gdo-form-label-left confirmButtons">
            <cc1:DynamicButton ID="btnYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="btnYesCancel_Click"
                meta:resourcekey="Yes" />
            <cc1:DynamicButton ID="btnNo" runat="server" ButtonType="Back" Text="No" meta:resourcekey="No" />
        </div>
    </div>
</asp:Panel>
<asp:UpdatePanel ID="updtpnlSpain" runat="server" UpdateMode="Always">
<ContentTemplate>
       <ajaxToolkit:ModalPopupExtender ID="SpainPlasticBagAlert" runat="server" TargetControlID="SpainPlasticBagAlertTarget"
            PopupControlID="pnlSpainPlasticBagAlert" CancelControlID="SpainPlasticBagAlertTarget" BackgroundCssClass="modalBackground" 
            DropShadow="false" />
             <div>
             <asp:Button ID="SpainPlasticBagAlertTarget" runat="server" CausesValidation="False" Style="display: none" />
             <asp:Panel ID="pnlSpainPlasticBagAlert" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblSpainPlasticBagAlert" Font-Size="Medium"  runat="server" Text="¿Quieres incluir bolsas a tu pedido?"  ></asp:Label>
                </div>
                <br />
                <div class="gdo-form-label-left confirmButtons">
                     <cc1:DynamicButton ID="btnOK" runat="server" Text="Yes" ButtonType="Forword" OnClick="btnOK_Click"   meta:resourcekey="btnOK"/>
                    <cc1:DynamicButton ID="btnCancel" runat="server" Text="No" ButtonType="Back" OnClick="btnCancel_Click"    meta:resourcekey="btnCancel"/>
                </div>
                
            </div>
        </asp:Panel>
          </div>
    </ContentTemplate>
    </asp:UpdatePanel>