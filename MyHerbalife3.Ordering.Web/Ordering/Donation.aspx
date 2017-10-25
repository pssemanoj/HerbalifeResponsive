<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/Ordering.master" CodeBehind="Donation.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Donation" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="HeaderContent">
      <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
    <style type="text/css">
        .donationdiv {
            float: right;
        }
        .auto-style1 {
            font-weight: bold;
            text-align: left;
            width: 428px;
        }
    </style>
    <script type="text/javascript">
        function checkDec(e) {
            var ex = /^[0-9.]+$/
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
        }

        function checName(e) {
            //var exs = /^[A-Za-z ]+$/;
            var ex = /^[A-Za-z \u4E00-\u9FFF\u3400-\u4DFF\uF900-\uFAFF]+$/;
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
        }
        function checNumber(e) {
            var ex = /^[0-9]+$/;
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
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
            document.getElementById('<%= uxCancelOrder.ClientID %>').style.display = 'none';
            document.getElementById('<%= uxCancelOrderDisabled.ClientID %>').style.display = '';
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
        function Check(val) {
            if (val.indexOf("btn5Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = true;
            }
            if (val.indexOf("btn10Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnOtherAmount") > -1)
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = false;

            if (val.indexOf("btnBehalf5Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount2.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnBehalf10Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount2.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnBehalfOther") > -1)
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = false;

        }

        $(document).ready(function () {
            var cookieLocal = $.cookie("RENDERING_LOCALE");
            if (cookieLocal == "zh_CN") {
                $("a[name='checkoutButton']").live('click', function () {
                    $("div[id$='updateProgressDiv']").addClass('opacity full-window').show();
                });
            }

            var pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
            pageRequestManager.add_endRequest(function () {
                var errors = document.getElementById('<%=this.blErrors.ClientID%>');
                if (null != errors) {
                    $('html, body').animate({ scrollTop: 0 }, 0);
                }

                $("div[id$='updateProgressDiv']").removeClass('opacity full-window');

             });
        });

        function showProgress() {
            $("div[id$='updateProgressDiv']").addClass('opacity full-window').css('z-index', '100002');
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
      <asp:HiddenField ID="OrderRefId" runat="server" />
       <asp:HiddenField ID="ShoppingCartID" runat="server" />
    <asp:HiddenField ID="SelfOther" runat="server" />
    <asp:HiddenField ID="behalfOfOther" runat="server" />
    <div id="divLabelErrors" runat="server" class="gdo-edit-header" style="border-bottom: 0px">
        <table>
            <tr>
                <td>
                    <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"
                        meta:resourcekey="blErrorsResource1">
                    </asp:BulletedList>
                </td>
            </tr>
        </table>
    </div>
    <span class="error" style="color: Red; display: none"></span>
    <%--    <div class="donations cc-grid">--%>
    <table class="donation-page" border="0" style="width: 100%">
        <tr>
            <td class="donate" valign="top">
                <div class="wrapper">
                    <table class="gdo-order-details-tbl">
                        <tr>
                            <td colspan="2">
                                <asp:Image runat="server" ID="imgDonationLogo" Width="30px" Height="30px" ImageUrl="/Ordering/Images/China/donation logo.png" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <h2><asp:Label ID="lblSelfDonation" runat="server" Text="本人捐赠"></asp:Label></h2>
                            </td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btn5Rmb" runat="server" Text="5RMB" GroupName="SelectOne" onclick="Check(this.id)" meta:resourcekey="btn5Rmb" /></td>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btn10Rmb" runat="server" Text="10RMB"  GroupName="SelectOne" onclick="Check(this.id)" meta:resourcekey="btn10Rmb" /></td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btnOtherAmount" runat="server" Text="OtherAmount" GroupName="SelectOne" onclick="Check(this.id)" meta:resourcekey="btnOtherAmount" /></td>
                            <td class="gdo-details-label ammount">
                                <asp:TextBox ID="txtOtherAmount" runat="server" Visible="true" MaxLength="6" onkeyup="checkDec(this);" Width="48px"></asp:TextBox></td>
                        </tr>
                    </table>
                </div>
            </td>
            <td class="donate" valign="top">
                <div class="wrapper">
                    <table class="gdo-order-details-tbl">
                        <tr>
                            <td colspan="2">
                                <h2><asp:Label ID="lblBehalfOfDonation" runat="server" Text="代顾客捐赠"></asp:Label></h2>
                            </td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btnBehalf5Rmb" runat="server" Text="5RMB" GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btn5Rmb" /></td>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btnBehalf10Rmb" runat="server" Text="10RMB"  GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btn10Rmb" /></td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label ammount">
                                <asp:RadioButton ID="btnBehalfOther" runat="server" Text="OtherAmount"  GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btnOtherAmount" /></td>
                            <td class="gdo-details-label ammount">
                                <asp:TextBox ID="txtOtherAmount2" runat="server" Visible="true" Width="48px" MaxLength="6" onkeyup="checkDec(this);"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label">
                                <asp:Label ID="lblDonatorName" runat="server" Text="顾客姓名" meta:resourcekey="lblDonatorName"></asp:Label></td>
                            <td class="gdo-details-label">
                                <asp:TextBox ID="txtDonatorName" runat="server" onkeyup="checName(this);" MaxLength="25"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="gdo-details-label">
                                <asp:Label ID="lblContactNumber" runat="server" Text="手机号码" meta:resourcekey="lblContactNumber"></asp:Label>
                            </td>
                            <td class="gdo-details-label">
                                <asp:TextBox runat="server" ID="txtContactNumber" MaxLength="11" onkeyup="checNumber(this);"></asp:TextBox>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <div class="col-sm-3">
                    <asp:LinkButton class="backward" runat="server" ID="btnDonationNow" OnClick="btnDonationNow_Click" >
                        <i class="icon icon-heart-fl-1 red"></i>&nbsp;&nbsp;<asp:Literal runat="server" ID="donateNowText" meta:resourcekey="btnDonationNow"></asp:Literal>
                    </asp:LinkButton>
                </div>
            </td>
        </tr>
    </table>

    <div class="col-sm-12 row">
    </div>
    <div class="col-sm-12">
        <asp:Label ID="lbldnmsg" runat="server"></asp:Label>
    </div>
    <%-- </div>--%>
    <br />
    <br />
    <%--Donation Order Summary--%>
    <div id="divdonationSummary" runat="server">
        <asp:PlaceHolder ID="plCheckOutTotalsMini" runat="server" />
    </div>
    <br />
    <br />
    <div id="divPaymentGrid" runat="server" class="donation-page">
        <asp:PlaceHolder ID="plPaymentOptions" runat="server" />
    </div>

    <div class="col-md-9 buttons-fwd donation-page" runat="server" id="divSubmitDonation">
        <!-- BACK & SUBMIT ORDER -->
        <div class="gdo-button-margin-rt bttn-addcartfinal">
            <cc2:DynamicButton ID="checkOutButton" runat="server" ButtonType="Forward" IconType="ArrowRight" name="checkoutButton"
                Text="Submit" OnClick="OnSubmit" meta:resourcekey="checkOutButton" OnClientClick="return HideSubmit(this);" />
            <cc2:DynamicButton ID="processingButton" runat="server" ButtonType="Back" IconType="ArrowRight"
                Text="Processing" meta:resourcekey="OrderProcessing" Disabled="true" Style="display: none"
                OnClientClick="return false;" />
        </div>
        <div class="col-md-3 buttonstbl-cancel" runat="server" id="divCancelDonation">
            <cc2:DynamicButton ID="uxCancelOrder" name="uxCancelOrder" runat="server" ButtonType="Back" Text="Cancel Order"
                meta:resourcekey="uxCancelOrder" OnClick="OnCancelDonation" />
            <cc2:DynamicButton ID="uxCancelOrderDisabled" runat="server" ButtonType="Back" Text="Cancel Order"
                Disabled="true" Style="display: none" meta:resourcekey="uxCancelOrder"
                OnClientClick="return false;" />
            <input type="hidden" id="hdnCustomerOrder" runat="server" />
        </div>
    </div>
    <%--Showing popup if Cart not empty--%>
    <asp:Button ID="btnDonationNowDup" runat="server" CausesValidation="False" Style="display: none" />
    <ajaxToolkit:ModalPopupExtender ID="mdlConfirm" runat="server" TargetControlID="btnDonationNowDup"
        PopupControlID="pnlConfirm" CancelControlID="btnDonationNowDup" BackgroundCssClass="modalBackground"
        DropShadow="false" />

    <asp:Panel ID="pnlConfirm" runat="server" Style="display: none">
        <div class="gdo-popup confirmClearCart">
            <div class="gdo-form-label-left miniCartPopUp">
                <asp:Label ID="lblConfirmClearCartText" runat="server" meta:resourcekey="lblConfirmText"></asp:Label>
            </div>
            <div class="gdo-form-label-left confirmButtons">
                <cc1:DynamicButton ID="btnYes" runat="server" ButtonType="Forward" Text="OK" OnClick="btnYes_OnClick"
                    meta:resourcekey="Yes" />
                <cc1:DynamicButton ID="btnNo" runat="server" ButtonType="Back" Text="No" OnClick="OnNoClick" meta:resourcekey="No" />
            </div>
        </div>
    </asp:Panel>

    <%--PC order redirect to Pricelist page--%>
     <ajaxToolkit:ModalPopupExtender ID="mdlPCConfirm" runat="server" TargetControlID="btnDonationNowDup"
        PopupControlID="pnlPCredirect" CancelControlID="btnDonationNowDup" BackgroundCssClass="modalBackground"
        DropShadow="false" />

    <asp:Panel ID="pnlPCredirect" runat="server" Style="display: none">
        <div class="gdo-popup confirmClearCart">
            <div class="gdo-form-label-left miniCartPopUp">
                <asp:Label ID="lblPCConfirmText" runat="server" meta:resourcekey="lblPCConfirmText"></asp:Label>
            </div>
            <div class="gdo-form-label-left confirmButtons">
                <cc1:DynamicButton ID="btnPCok" runat="server" ButtonType="Forward" Text="OK" OnClick="btnYes_OnClick"
                    meta:resourcekey="OK" />
            </div>
        </div>
    </asp:Panel>

    <%--Cancle Donation--%>
    <asp:Button ID="uxCancelOrderdup" runat="server" CausesValidation="False" Style="display: none" />
     <ajaxToolkit:ModalPopupExtender ID="mdlConfirmDelete" runat="server" TargetControlID="uxCancelOrderdup"
                            PopupControlID="pnlConfirmCancel" CancelControlID="uxCancelOrderdup" BackgroundCssClass="modalBackground"
                            DropShadow="false" />

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
                <cc2:DynamicButton ID="btnDonateYes" runat="server" ButtonType="Forward" Text="Yes" meta:resourcekey="Yes"
                    OnClick="btnDonateYes_Click" />
                <cc2:DynamicButton ID="btnDonateNo" runat="server" ButtonType="Back" Text="No" meta:resourcekey="No"
                    OnClientClick="showCancel();" />
            </div>
        </div>
    </asp:Panel>
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
                        <cc2:DynamicButton ID="btnTermConditionYes" runat="server" ButtonType="Forward" Text="Yes" meta:resourcekey="TermConditionOk" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
     <asp:Button ID="btnModalPopup" runat="server" Text="" Visible="True" Style="display: none;" />
      <ajaxToolkit:ModalPopupExtender ID="btnModalPopup_ModalPopupExtender" BehaviorID="ModalPopupBehaviorID" runat="server" PopupControlID="ModalPopup"
        BackgroundCssClass="modalBackground" Enabled="True" TargetControlID="btnModalPopup" >
    </ajaxToolkit:ModalPopupExtender>
            <asp:Panel ID="ModalPopup" runat="server" Style="display: none" CssClass="gdo-popup">
                <iframe id="iframe1"  name="iframePopup" width="700" scrolling="yes" height="500"></iframe>
            </asp:Panel>
    <asp:PlaceHolder ID="OTPPanel" runat="server"></asp:PlaceHolder>

    <%--To process unknown orders to decline--%>
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
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Recent Order" meta:resourcekey="lblDupeOrder"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server" Text="You recently placed an order, verifiying the orders status. Otherwise, please click Cancel Order in MyOrder Page."></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

