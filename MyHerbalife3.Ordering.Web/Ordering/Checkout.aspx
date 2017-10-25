<%@ Page Title="Check Out" Language="C#" MasterPageFile="~/MasterPages/Ordering.Master" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Checkout" meta:resourcekey="PageResource1" EnableEventValidation="false" ValidateRequest="false" %>

<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="ProductAvailability" TagPrefix="uc2" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            var cookieLocal = $.cookie("RENDERING_LOCALE");
            if (cookieLocal == "zh_CN") {
                $("a[name='checkoutButton']").live('click', function () {
                    $("div[id$='updateProgressDiv']").addClass('opacity full-window');
                });
            }
        });

        function showProgress() {
            $("div[id$='updateProgressDiv']").addClass('opacity full-window').css('z-index', '100002');
        }
    </script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">

    <script type="text/javascript">
        var processingButton = document.getElementById('<%= processingButton.ClientID %>');
        var continueShoppingButton = document.getElementById('<%= ContinueShopping.ClientID %>');
        var continueShoppingButtonDisabled = document.getElementById('<%= ContinueShoppingDisabled.ClientID %>');
        var cancelButton = document.getElementById('<%= uxCancelOrder.ClientID %>');

        function S4() {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1)
        }

        function SetRefIdCookie() {
            var guid = (S4() + S4() + S4() + S4() + S4() + S4() + S4() + S4());
            var orderRefId = document.getElementById('<%= OrderRefId.ClientID %>');
            var cartId = document.getElementById('<%= ShoppingCartID.ClientID %>');
            orderRefId.value = cartId.value + '-' + guid;
            $.cookie('DUPCHECKWITHREFID_STATUS', orderRefId.value);
        }


        function HideSubmit(btnCtrl) {
            SetRefIdCookie();
            btnCtrl.style.display = 'none';
            document.getElementById('<%= processingButton.ClientID %>').style.display = '';
            document.getElementById('<%= ContinueShopping.ClientID %>').style.display = 'none';
            document.getElementById('<%= ContinueShoppingDisabled.ClientID %>').style.display = '';
            HideCancel(cancelButton);

            return CheckPaymentMethod(btnCtrl);
        }

        function HideCancel(btnCtrl) {
            if (btnCtrl) {
                btnCtrl.style.display = 'none';
                document.getElementById('<%= uxCancelOrderDisabled.ClientID %>').style.display = '';
            }
            return true;
        }

        function showCancel() {
            document.getElementById('<%= uxCancelOrder.ClientID %>').style.display = '';
            document.getElementById('<%= uxCancelOrderDisabled.ClientID %>').style.display = 'none';
            return true;
        }

        function HideDMSubmit(btnDMCtrl) {
            btnDMCtrl.style.display = 'none';
            document.getElementById('<%= btnDMProcessing.ClientID %>').style.display = '';
            document.getElementById('<%= btnDMNO.ClientID %>').disabled = "disabled";
            return true;
        }

        function LoadUrl(param) {
            document.getElementById('iframe1').src = param;
        }

        var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
        pageRequestManager.add_endRequest(function () {
            var errors = document.getElementById('<%=this.blErrors.ClientID%>');
            if (null != errors) {
                $('html, body').animate({ scrollTop: 0 }, 0);
            }

            $("div[id$='updateProgressDiv']").removeClass('opacity full-window');

        });

            function CheckPaymentMethod(btnCtrl) {
                var process = true;
                try {

                    if (typeof PaymentGatewayHasCardDataControlId !== 'undefined' && null != PaymentGatewayHasCardDataControlId) {
                        if (document.getElementById(PaymentGatewayHasCardDataControlId).value == 'true') {
                            if (null != ValidateNewCard) {
                                process = ValidateNewCard(null, this);
                            }
                        }
                    }
                }
                catch (e) {
                    process = false;
                }

                if (!process) {
                    process = false;
                    btnCtrl.style.display = '';
                    document.getElementById('<%= processingButton.ClientID %>').style.display = 'none';
                    document.getElementById('<%= ContinueShopping.ClientID %>').style.display = '';
                    document.getElementById('<%= ContinueShoppingDisabled.ClientID %>').style.display = 'none';
                    showCancel();
                }

                return process;
            }
    </script>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <table class="gdo-main-table chkoutFullWidth">
        <tr>
            <td>
                <div>
                    <asp:UpdatePanel ID="upErrors" runat="server" UpdateMode="Always">
                        <ContentTemplate>
                            <asp:HiddenField runat="server" ID="hfCancelOrder" />
                            <asp:HiddenField runat="server" ID="DupeCheckDone" />
                            <asp:HiddenField runat="server" ID="CurrentOrderMonth" />
                            <asp:HiddenField ID="DupeOkClicked" runat="server" />
                            <asp:HiddenField ID="OrderRefId" runat="server" />
                            <asp:HiddenField ID="ShoppingCartID" runat="server" />
                            <asp:BulletedList ID="blErrors" runat="server" CssClass="gdo-error-message-txt" meta:resourcekey="blErrorsResource1">
                            </asp:BulletedList>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="headersavedcart" runat="server" id="SavedCartTitle" visible="false">
                    <asp:Label ID="lblSavedCartName" runat="server"></asp:Label>
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div id="divHapEditMessage" runat="server" visible="false" class="gdo-right-column-text-blue">
                    <asp:Label ID="lbHAPEditMessage" runat="server" Text="To save your changes for your Hap Standing Order you must click the submit button." meta:resourcekey="lbHAPEditMessageResource1" ></asp:Label>
                </div>
                <div class="headerbar">
                    <div class="headerbar_leftcap">
                    </div>
                    <div class="headerbar_slidingdoor">
                        <div class="headerbar_icon">
                            <img src="/Content/Global/img/gdo/icons/step3.gif">
                        </div>
                        <div class="headerbar_text">
                            <asp:Label ID="lblStep3Heading" runat="server" Text="Step 3 of 4 - Review Order Summary"
                                meta:resourcekey="lblStep3Resource1"></asp:Label>
                        </div>
                        <div class="headerbar_edit">
                            <a href="ShoppingCart.aspx">
                                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="Edit">Edit</asp:Localize>
                            </a>
                        </div>
                    </div>
                    <asp:PlaceHolder ID="plCheckOutOptions" runat="server" />
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div class="headerbar">
                    <div>
                        <cc1:ContentReader ID="CheckoutMessage1" runat="server" ContentPath="reviewcartstep3.html"
                            SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                    </div>
                    <asp:PlaceHolder ID="plCheckOutTotalsDetails" runat="server" />
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <asp:PlaceHolder ID="plCheckOutHAPOptions" runat="server" />
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <asp:PlaceHolder ID="plCheckOutOrderDetails" runat="server" />
                <table class="gdo-order-tbl-legend" border="0" cellspacing="0">
                    <tr>
                        <td>
                            <table class="gdo-order-tbl-bottom" border="0" cellspacing="0" cellpadding="0">
                                <tr>
                                    <td>
                                        <div>
                                            <p style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 3px 0px; padding-top: 0px"
                                                runat="server" id="pProdAvail" />
                                            &nbsp;<uc2:ProductAvailability ID="ProductAvailability1" runat="server" />
                                        </div>
                                    </td>
                                    <td valign="top" class="gdo-order-tbl-data-right">
                                        <!-- EMPTY -->
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                <cc1:ContentReader ID="HFFMessage" runat="server" Visible="false" ContentPath="hffmessage.html" SectionName="ordering" ValidateContent="true" />
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div class="headerbar">
                    <div class="headerbar_leftcap">
                    </div>
                    <div class="headerbar_slidingdoor">
                        <div class="headerbar_icon">
                            <img src="/Content/Global/img/gdo/icons/step4.gif">
                        </div>
                        <div class="headerbar_text">
                            <asp:Label ID="lblStep3" runat="server" Text="Step 4 of 4 - Invoice & Payment Options"
                                meta:resourcekey="lblSTep4Resource1"></asp:Label>
                        </div>
                        <div class="headerbar_edit">
                        </div>
                    </div>
                </div>
                <div>
                    <asp:PlaceHolder ID="plDeclinedInfo" runat="server" />
                </div>
                <table cellpadding="15" class="invoice-option">
                    <tr>
                        <td valign="top">
                            <asp:PlaceHolder ID="plInvoiceOptions" runat="server" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell payment-section">
                <div class="fragment">
                    <cc1:ContentReader ID="CheckoutMessage3" runat="server" ContentPath="reviewcartstep4.html"
                        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                </div>
                <asp:PlaceHolder ID="plPaymentOptions" runat="server" />
                <asp:PlaceHolder ID="plPaymentOptionsSummary" runat="server" />
                <asp:PlaceHolder ID="plFraudControl" runat="server" />
            </td>
        </tr>
        <tr>
            <td>
                <div>
                    <cc1:ContentReader ID="checkOutLogos" runat="server" ContentPath="checkOutLogos.html"
                        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div>
                    <cc1:ContentReader ID="PolicyMessage" runat="server" Visible="false" ContentPath="policytext.html"
                        SectionName="Ordering" ValidateContent="true" />
                </div>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell checkout-action-buttons">
                <asp:UpdatePanel ID="upButtons" runat="server" UpdateMode="Always">
                    <ContentTemplate>
                        <table class="buttonstbl" border="0" cellpadding="5">
                            <tr>
                                <td class="buttonstbl-msg" colspan="2">
                                    <asp:Label runat="server" ID="SpecialMessage" class="checkout-message" Text="" meta:resourcekey="lblSpecialMessage" />
                                    <asp:Label ID="lblSubmitText" runat="server" Text="Your order will be placed when you click Submit Order"
                                        meta:resourcekey="lblSubmitTextResource1"></asp:Label>
                                    <asp:Label ID="lblHAPSubmitText" runat="server" Text="Your HAP Standing Order will be activated when you click the Submit button." style="font-size:14px !important;"
                                        meta:resourcekey="lblHAPSubmitText"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <!-- CANCEL ORDER BUTTON -->
                                    <div>
                                        <div class="col-md-3 buttonstbl-cancel">
                                            <cc2:DynamicButton ID="uxCancelOrder" name="uxCancelOrder" runat="server" ButtonType="Back" Text="Cancel Order"
                                                meta:resourcekey="uxCancelOrderResource1" OnClientClick="return HideCancel(this);" />
                                            <cc2:DynamicButton ID="uxCancelOrderDisabled" runat="server" ButtonType="Back" Text="Cancel Order"
                                                Disabled="true" Style="display: none" meta:resourcekey="uxCancelOrderResource1"
                                                OnClientClick="return false;" />
                                            <cc2:DynamicButton ID="uxReturnToCustomerOrder" runat="server" ButtonType="Back"
                                                Visible="false" Text="To My Customer Orders Search" meta:resourcekey="CustomerOrdersSearchPageReturn"
                                                OnClick="ReturnToCustomerOrdersSearch" />
                                            <cc2:DynamicButton ID="uxReturnToCustomerOrderDisabled" runat="server" ButtonType="Back"
                                                Text="To My Customer Orders Search" Disabled="true" Visible="false" OnClientClick="return false;"
                                                meta:resourcekey="CustomerOrdersSearchPageReturn" />
                                            <input type="hidden" id="hdnCustomerOrder" runat="server" />
                                        </div>
                                        <div class="col-md-9 buttons-fwd">
                                            <!-- BACK & SUBMIT ORDER -->
                                            <div class="gdo-button-margin-rt bttn-addcartfinal">
                                                <cc2:DynamicButton ID="checkOutButton" runat="server" ButtonType="Forward" IconType="ArrowRight" name="checkoutButton"
                                                    Text="Submit" OnClick="OnSubmit" meta:resourcekey="checkOutButtonResource1" OnClientClick="return HideSubmit(this);" />
                                                <cc2:DynamicButton ID="processingButton" runat="server" ButtonType="Back" IconType="ArrowRight"
                                                    Text="Processing" meta:resourcekey="OrderProcessing" Disabled="true" Style="display: none"
                                                    OnClientClick="return false;" />
                                            </div>
                                            <div class="gdo-button-margin-rt bttn-back">
                                                <cc2:DynamicButton ID="ContinueShopping" name="ContinueShopping" runat="server" ButtonType="Back" Text="Back"
                                                    OnClick="OnContinueShopping" meta:resourcekey="ContinueShoppingResource1" />
                                                <cc2:DynamicButton ID="ContinueShoppingDisabled" runat="server" ButtonType="Back"
                                                    Text="Back" Disabled="true" Style="display: none" meta:resourcekey="ContinueShoppingResource1" />
                                            </div>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <ajaxToolkit:ModalPopupExtender ID="mdlConfirmDelete" runat="server" TargetControlID="uxCancelOrder"
                            PopupControlID="pnlConfirmCancel" CancelControlID="btnNo" BackgroundCssClass="modalBackground"
                            DropShadow="false" />
                        <asp:Button ID="MpeFakeTarget1" runat="server" CausesValidation="False" Style="display: none" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td>
                <input type="hidden" id="hdnProcButtonID" runat="server" />
            </td>
        </tr>
    </table>
    <asp:Panel ID="pnlConfirmCancel" runat="server" Style="display: none">
        <div class="gdo-popup confirmCancel">
            <div class="gdo-float-left gdo-popup-title">
                <h2>
                    <asp:Label ID="lblConfirmation" runat="server" Text="Confirmation" meta:resourcekey="lblConfirmation"></asp:Label>
                </h2>
            </div>
            <div class="gdo-form-label-left">
                <asp:Label ID="lblConfirmText" runat="server" Text="Are you sure you wish to cancel ?"
                    meta:resourcekey="lblConfirmText"></asp:Label>
            </div>
            <div class="gdo-form-label-left confirmButtons">
                <cc2:DynamicButton ID="btnYes" runat="server" ButtonType="Forward" Text="Yes" meta:resourcekey="Yes"
                    OnClick="btnYes_Click" />
                <cc2:DynamicButton ID="btnNo" runat="server" ButtonType="Back" Text="No" meta:resourcekey="No"
                    OnClientClick="showCancel();" />
            </div>
        </div>
    </asp:Panel>
    <asp:UpdatePanel ID="UpdatePanelDupeOrder" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="dupeOrderPopupExtender" runat="server" TargetControlID="DupeOrderFakeTarget"
                PopupControlID="pnldupeOrderMonth" CancelControlID="DupeOrderFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="DupeOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupeOrderMonth" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Identical Order" meta:resourcekey="lblDupeOrder"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server" Text="You recently placed an identical order on {0}. If you wish to continue with this order, please submit it now. Otherwise, please click Cancel Order."></asp:Label>
                    </div>
                    <div class="gdo-form-label-center">
                        <br></br>
                        <asp:Label ID="lblPleaseSelect" runat="server" ForeColor="Red" Visible="false" Text="Please select your choice" meta:resourcekey="lblPleaseSelect"></asp:Label>
                        <asp:DropDownList runat="server" ID="ddlConfirmSubmit">
                            <asp:ListItem Value="select" Text="Select" meta:resourcekey="Select"></asp:ListItem>
                            <asp:ListItem Value="SubmitAnyway" Text="I'd like to submit order" meta:resourcekey="SubmitAnyway"></asp:ListItem>
                            <asp:ListItem Value="DonotSubmit" Text="Do not submit order" meta:resourcekey="DonotSubmit"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanelDualMonth" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="dualOrderMonthPopupExtender" runat="server" TargetControlID="MpeFakeTarget"
                PopupControlID="pnldualOrderMonth" CancelControlID="MpeFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="MpeFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldualOrderMonth" runat="server" Style="display: none">
                <div class="gdo-popup">
                    <table width="510" cellspacing="5" cellpadding="0" border="0">
                        <tbody>
                            <tr>
                                <td>
                                    <div class="gdo-float-left gdo-popup-title">
                                        <h2>
                                            <asp:Label ID="lblDMStaticTitle" runat="server" Text="Dual Order Month Confirmation"
                                                meta:resourcekey="DualOrderMonthConfirmation"></asp:Label>
                                        </h2>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td class="gdo-form-label-left">
                                    <div>
                                        <asp:Label ID="lblDMValue" runat="server" meta:resourcekey="lblDMValue"></asp:Label>
                                    </div>
                                    <div class="align-right">
                                        <cc2:DynamicButton ID="btnDMYes" runat="server" ButtonType="Forward" Text="ConfirmDualOrderMonthYes"
                                            OnClick="btnDMYes_Click" meta:resourcekey="Yes" OnClientClick="return HideDMSubmit(this);" />
                                        <cc2:DynamicButton ID="btnDMProcessing" runat="server" ButtonType="Back" Text="Processing"
                                            meta:resourcekey="OrderProcessing" Disabled="true" Style="display: none" OnClientClick="return false;" />
                                        <cc2:DynamicButton ID="btnDMNO" runat="server" ButtonType="Back" Text="No" OnClick="btnDMNO_Click"
                                            meta:resourcekey="No" />
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanelOrderStatus" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="OrderStatusPopupExtender" runat="server" TargetControlID="OrderStatusFakeTarget"
                PopupControlID="pnlOrderStatusMonth" CancelControlID="OrderStatusFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="OrderStatusFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnlOrderStatusMonth" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblOrderStatusConfirmMessage" runat="server" meta:resourcekey="lblOrderStatus"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblOrderStatusMessage" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="OrderStatusDynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnOrderStatusOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:Button ID="btnModalPopup" runat="server" Text="" Visible="True" Style="display: none;" />

    <ajaxToolkit:ModalPopupExtender ID="btnModalPopup_ModalPopupExtender" BehaviorID="ModalPopupBehaviorID" runat="server" PopupControlID="ModalPopup"
        BackgroundCssClass="modalBackground" Enabled="True" TargetControlID="btnModalPopup">
    </ajaxToolkit:ModalPopupExtender>

    <asp:Panel ID="ModalPopup" runat="server" Style="display: none" CssClass="gdo-popup">
        <iframe id="iframe1" name="iframePopup" width="700" scrolling="yes" height="500"></iframe>
    </asp:Panel>

    <div id="panelLoad" runat="server" style="display: none">
        <div class="gdo-popup">
            <img src="/SharedUI/Images/icons/LoadingGreenCircle.gif" alt="Order Processing..." />
        </div>
    </div>
    <asp:Button ID="btnHidePopup" runat="server" Text="" Visible="True" Style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="popupHide" BehaviorID="popupHide" runat="server" PopupControlID="panelLoad"
        BackgroundCssClass="modalBackground" TargetControlID="btnHidePopup">
    </ajaxToolkit:ModalPopupExtender>
    <div id="dialog" title="Basic dialog">
    </div>
    <asp:UpdatePanel ID="UpdatePanelOrderMismatch" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="TermConditionPopupExtender" runat="server" TargetControlID="TermConditionFakeTarget"
                PopupControlID="pnlTermCondition" CancelControlID="TermConditionFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="TermConditionFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="PnlTermCondition" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblTermConditionHeading" runat="server" Text="You Cannot Change Order Items" meta:resourcekey="lblTermConditionHeading"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="btnTermConditionYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="ReturnToOrderHistory" meta:resourcekey="TermConditionOk" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:PlaceHolder ID="OTPPanel" runat="server"></asp:PlaceHolder>
</asp:Content>
