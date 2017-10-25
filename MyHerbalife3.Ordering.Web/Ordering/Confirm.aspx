<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/Ordering.master"
    CodeBehind="Confirm.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Confirm" meta:resourcekey="PageResource1" %>

<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="ProductAvailability"
    TagPrefix="uc2" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Content ID="ContentHeader" runat="server" ContentPlaceHolderID="HeaderContent">
    <script type="text/javascript">
        $(document).ready(function () {
            cookieEater();
        });
        //$(document).ready(function () {
        //    $.cookie('DUPCHECKWITHREFID_STATUS', null);

        //});

    </script>


    <style type="text/css">
        .headerbar_edit {
            font-size: 11px;
            float: right;
            padding: 6px 20px 0px 3px;
        }
    </style>
    <link rel="stylesheet" href="/Content/Global/css/gdo/print.css" type="text/css" media="print" />
    <script type="text/javascript">
        var onlyPrint = false;
        function ConfPrint(ctrl) {
            if ($('[id$="divLabelErrors"] + table').hasClass('chkoutFullWidth')) {
                $('[id$="divLabelErrors"] + table').removeClass('chkoutFullWidth');                
                $('[id$="divLabelOrderSummary"] + table.gdo-order-tbl').css('width', '645px');

                $('.gdo-main-table').css('width', '65%');
                $('.translation-number').css('display', 'none');

                if ($('[id$="gdoNavMid"]').hasClass('gdo-nav-mid-confirm-tellstory')) {
                    onlyPrint = true;
                    var setWidth = 745;
                }
                else {
                    onlyPrint = true;
                    var setWidth = 930;
                }
            } 

            if (ctrl != null) {
                window.print();
                ctrl.removeAttribute("href");
            }

            if (onlyPrint) {
                $('[id$="divLabelErrors"] + table').addClass('chkoutFullWidth');
                $('.gdo-main-table').css('width', '100%');
                $('[id$="divLabelOrderSummary"] + table.gdo-order-tbl').css('width', setWidth+'px');
                $('.gdo-order-details-container-last').css('margin-left', '0' );
            }
        }
       
    </script>
    <script type="text/javascript">
        function SetTarget() {
            document.forms[0].target = "_blank";
        }
        function GetNeverShowAgain() {

           
        }
    </script>
    <script type="text/javascript">
        function HidePopUp() {
           
                $find('<%= ForeignPPVPopupExtender.ClientID %>').hide();   
               $.ajax({
                type: "POST",
                url: "Confirm.aspx/RemoveLegacyReceiptSession",
                data: "{}",
                async: false,
                contentType: "application/json; charset=utf-8",
                dataType: "json"
               

            });   
            
            return true;
                      
        }
    </script>
    
    
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ProductRecomendationsContent" runat="server">
    <% if (MyHerbalife3.Ordering.Configuration.ConfigurationManagement.HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations) { %>
    <script type="text/javascript">
        AdobeTarget = {
            "entity" : {
                "orderTotal" : <%= totalOrder %>,
                "orderId" : "<%= orderNumber %>",
                "productPurchasedId" : "<%= skuList %>"
            }
        };
        
        _etmc.push(["trackConversion", { "cart": [<%= cartItems %>], "order_number" : "<%= orderNumber %>" }]);
    </script>
    <% } %>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div id="divLabelErrors" runat="server" class="gdo-edit-header" style="border-bottom: 0px">
        <table width="100%">
            <tr id="trPendingOrder" runat="server" visible="false">
                <td>
                    <asp:Label ID="lblPendingOrder" runat="server" 
                        meta:resourcekey="lblPendingOrderResource" CssClass="gdo-error-message-txt" />
                      <cc2:ContentReader ID="PendingNotification" ContentPath="PendingMessage.html" runat="server" UseLocal="true" ForeColor="Red" Visible="false" />
                        
                </td>
                <td>
                    
                </td>
            </tr>
            <tr id="trSalesConfirm" runat="server">
                <td>
                    <cc2:ContentReader ID="MPE" runat="server" ContentPath="NewSalesConfirmPage.html" SectionName="Ordering" UseLocal="true" />
                </td>
            </tr>
            <tr id="trConfirm" runat="server" Visible="True">
                <td>
                    <asp:Label Width="100%" ID="lblSuccess" runat="server" CssClass="headerbar_text-confirmation"
                        meta:resourcekey="lblOrderPlacedSuccessResource"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblOrderStatus" runat="server" Text="Order" meta:resourcekey="lblOrderStatusResource1"
                        CssClass="OrderNumberDisplay" />
                    <asp:Label runat="server" ID="lblOrderNumber" meta:resourcekey="lblOrderNumberResource1"
                        CssClass="OrderNumberDisplay" />
                    <br />
                    <%if (CountryCode == "PY")
                        {%>
                    <asp:Label ID="InfoMessage" runat="server" Text="Order" meta:resourcekey="lblMessageResource1"
                        CssClass="OrderNumberDisplay" />
                    <%} %>
                    <br />
                    <div>
                    <asp:HyperLink id="hyperLinkInvoice"  Font-Size="Medium"  NavigateUrl="~/Ordering/invoice" Text="CreateReceipt"  Visible="false" runat="server" meta:resourcekey="CreateReceipt"/> 
                        </div>
                    <div id="dvCreateInvoice" runat="server" Visible="False">
                            <div class="ntbox-email">
                                <asp:LinkButton runat="server" ID="lnkCreateInvoice" Text="Click here to create an invoice for your customer with the order" OnClick="lnkCreateInvoiceClicked" meta:resourcekey="lnkCreateInvoice"></asp:LinkButton>
                            </div>
                        </div>
                    <div id="dvHapOrderPage" runat="server" visible="false">
                        <div class="ntbox-email">
                            <asp:LinkButton runat="server" ID="lnkHapOrderPage" Text="Go to My Hap Order page" OnClick="lnkHapOrderPage_Click" meta:resourcekey="lnkOrderPage"></asp:LinkButton>
                        </div>
                    </div>
                     <div id="Div2" runat="server" visible="true">
                        <div class="ntbox-email">
                           <cc1:DynamicButton runat="server"  ButtonType="Neutral" meta:resourcekey="DownLoadQRCode1"  ID="btnQrCodeDownload"    OnClick="btnQrCodeDownload_Click" Visible="true" OnClientClick="SetTarget();" ></cc1:DynamicButton>
                              </div>
                    </div>
                  
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="lblTransactiontime" runat="server" Text="" 
                        CssClass="OrderNumberDisplay" />
                 </td>
            </tr>
            <tr id="trWire" runat="server" visible="false">
                <td style="display: none">
                    <asp:Label ID="lblWireRefNumDisplay" runat="server" Text="Wire Reference Number"
                        meta:resourcekey="lblWireRefNumDisplayResource1" CssClass="OrderNumberDisplay" />
                    <asp:Label runat="server" ID="lblWireRefNum" meta:resourcekey="lblOrderNumberResource1"
                        CssClass="OrderNumberDisplay" />
                </td>
            </tr>
            <tr id="trFControl" runat="server" visible="false">
                <td>
                    <asp:Label ID="lblFControlMessage" runat="server" 
                        meta:resourcekey="lblFControlMessageResource1" CssClass="gdo-error-message-txt" />                    
                </td>
            </tr>
            <tr id="trMPCFraud" runat="server" visible="false">
                <td>
                    <asp:Label ID="lblMPCFraudMessage" runat="server" 
                        meta:resourcekey="lblMPCFraudMessage" CssClass="gdo-error-message-txt" />                    
                </td>
            </tr>
            <tr id="trPendingOrderLink" runat="server" visible="false">
                <td>
                        <asp:HyperLink runat="server" NavigateUrl="PendingOrders.aspx" Text="ViewPendingOrders" meta:resourcekey="ViewPendingOrderResource"></asp:HyperLink>
                </td>
                <td>
                     
                </td>
            </tr>
        </table>
    </div>
    <br />
    <div>

    </div>
    <table class="gdo-main-table chkoutFullWidth">
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <asp:PlaceHolder ID="plCheckOutOptions" runat="server" /> 
                 
            </td>                              
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <asp:PlaceHolder ID="plCheckOutTotalsDetails" runat="server" />
               
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
                                            <p style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 3px 0px; padding-top: 0px" runat="server" id="pProdAvail">
                                                &nbsp;<uc2:ProductAvailability ID="ProductAvailability1" runat="server" ></uc2:ProductAvailability>
                                            </p>
                                        </div>
                                    </td>
                                    <td valign="top" class="gdo-order-tbl-data-right">
                                        <%--EMPTY--%>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="gdo-order-tbl-legend">
                <cc2:ContentReader ID="HFFMessage" runat="server" Visible="false" ContentPath="hffmessage.html" SectionName="ordering" ValidateContent="true" />
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <table cellpadding="15">
                    <tr>
                        <td valign="top">
                            <asp:PlaceHolder ID="plInvoiceOptions" runat="server" />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <asp:PlaceHolder ID="plPaymentOptions" runat="server" />
            </td>
        </tr>
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div>
                    <cc2:ContentReader ID="PolicyMessage" runat="server" Visible="false" ContentPath="policytext.html"
                        SectionName="Ordering" ValidateContent="true" />
                </div>
            </td>
        </tr>
        <tr class="checkout-action-buttons">
            <td style="width: 70%" class="buttons-fwd">
                <%--BACK & SUBMIT ORDER--%>
                <div class="gdo-button-margin-rt bttn-addcartfinal confirm">
                    <a href="" id="A1">
                            <cc1:DynamicButton ID="printThisButton" runat="server" ButtonType="Neutral" Text="Submit"
                                meta:resourcekey="PrintThisButtonResource1" OnClientClick=" ConfPrint(this); " />
                    </a>
                </div>
                <div class="gdo-button-margin-rt bttn-back">
                    <a href="" id="">
                            <cc1:DynamicButton ID="ContinueShopping" runat="server" ButtonType="Forward" Text="Back"
                                OnClick="OnContinueShopping" meta:resourcekey="ContinueShoppingResource1" />
                    </a>
                </div>
              
            </td>
        </tr>
    </table>
    <asp:HiddenField ID="hdnOrderType" runat="server" />
    <%--Start HFF Modal--%>
    <asp:Button ID="btnDonate" runat="server" CausesValidation="False" Style="display: none" />
    <asp:Panel ID="pnlDonate" runat="server">
        <table class="gdo-main-table">
            <tr>
                <td valign="top" class="gdo-main-tablecell">
                    <asp:PlaceHolder ID="plHFFModal" runat="server" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mdlDonate" runat="server" TargetControlID="btnDonate"
        PopupControlID="pnlDonate" CancelControlID="" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <%--End HFF Modal--%>

    <ajaxToolkit:ModalPopupExtender ID="PBPOrderPopupExtender" runat="server" TargetControlID="PBPOrderFakeTarget"
        PopupControlID="pnlPBPOrderAlert" CancelControlID="PBPOrderFakeTarget" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <asp:Button ID="PBPOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
    <asp:Panel ID="pnlPBPOrderAlert" runat="server" Style="display: none">
        <div class="gdo-popup confirmCancel">
            <div class="gdo-float-left gdo-popup-title">
                <h2>
                    <asp:Label ID="lblPBPMessageTitle" runat="server" Text="PBP Message Title"></asp:Label>
                </h2>
            </div>
            <div class="gdo-form-label-left">
                <asp:Label ID="lblPBPOrderMessage" runat="server"></asp:Label>
            </div>
            <div class="gdo-form-label-left confirmButtons">
                <cc1:DynamicButton ID="PBPDynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnPBPOrderOK" meta:resourcekey="OK" />
            </div>
        </div>
    </asp:Panel>
<ajaxToolkit:ModalPopupExtender ID="ForeignPPVPopupExtender" runat="server" TargetControlID="ForeignPPVPopupFakeTarget"
                                PopupControlID="pnlForeignPPVPopupTarget" CancelControlID="ForeignPPVPopupFakeTarget" BackgroundCssClass="modalBackground"
                                DropShadow="false" />
<asp:Button ID="ForeignPPVPopupFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
<asp:Panel ID="pnlForeignPPVPopupTarget" runat="server" Style="display: none">
    <div class="gdo-popup confirmCancel">
        <div class="gdo-float-left gdo-popup-title">
            <h2>
                <asp:Label ID="lblForeignPPV" runat="server" Text="Message" meta:resourcekey="ForeignPPV" ></asp:Label>   
            </h2>
            <h2>
                <asp:Label ID="lblReceiptLater" runat="server" Text="Message" meta:resourcekey="lblReceiptLater" ></asp:Label>   
            </h2>
        </div>
        <div class="gdo-form-label-left confirmButtons">
            <button type="button" onclick="return HidePopUp(this);" value="Ok" class="forward"><%=GetLocalResourceObject("Later.Text").ToString()%></button>
            <cc1:DynamicButton ID="btnCreateReceipt" runat="server" ButtonType="Forward" Text="Create Receipt" OnClick="OnCreateRecipt" meta:resourcekey="CreateReceipt" />
           <%-- <asp:Button ID="btnOK"  Text="OK" OnClientClick="" meta:resourcekey="OK" />--%>
        </div>
    </div>
</asp:Panel>
</asp:Content>