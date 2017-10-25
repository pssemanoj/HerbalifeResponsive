<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpressTrack.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.ExpressTrack" MasterPageFile="~/MasterPages/Ordering.master" %>

<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">
    <style>
        .edimore {
            cursor: pointer;
        }

        .gdo-nav-left, .gdo-nav-right {
            display: none;
        }

        .gdo-nav-mid {
            width: 100%;
        }
    </style>
    <script type="text/javascript">
        var RadAjaxLoadingPanelClientID = "<%= RadAjaxLoadingPanel.ClientID %>";

        var Txt_MoreDetails = "<%=GetLocalResourceString("MoreDetails")%>";
        var Txt_LessDetails = "<%=GetLocalResourceString("LessDetails")%>";
        var Txt_NoExpressTrackingDetails = "<%=GetLocalResourceString("NoExpressTrackingDetails")%>";
        var Txt_ExpressTrackingError = "<%=GetLocalResourceString("ExpressTrackingError")%>";
        var Txt_ContactUsForExpressIssue = "<%=GetLocalResourceString("ContactUsForExpressIssue")%>";
</script>
    <script type="text/javascript">
        function SetTarget() {
            document.forms[0].target = "_blank";
        }
    </script>
<script type="text/javascript" src="/Ordering/Scripts/ExpressTrack.js"></script>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
     <asp:BulletedList runat="server" ID="blstErrores" Font-Bold="True" ForeColor="Red"
                                                meta:resourcekey="blstErroresResource1">
                                            </asp:BulletedList>
    <table class="gdo-main-table" style="width: 100%">
        <tr>
            <td valign="top" class="gdo-main-tablecell">
                <div class="headerbar">
                    <div class="headerbar_leftcap">
                    </div>
                    <div class="headerbar_slidingdoor">
                        <div class="headerbar_text">
                            <asp:Label ID="lbExpressHeading" runat="server" Text="Order" meta:resourcekey="lbExpressHeading"></asp:Label>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    </table>
    <table>
        <colgroup>
            <col style="width: 10%" />
            <col style="width: 25%" />
            <col style="width: 15%" />
            <col />
        </colgroup>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblOrdernumberText" runat="server" Text="订单号： "></asp:Label>
                </div>
            </td>
            <td>
                <div>
                    <asp:Label runat="server" ID="lblOrdernumber"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblOrderMonthText" runat="server" Text="订单月： "></asp:Label>
                </div>
            </td>
            <td>
                <div>
                    <asp:Label runat="server" ID="lblOrderMonth"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblOrderStatusText" runat="server" Text="订单状态： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblOrderStatus"></asp:Label>
                </div>
            </td>

        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblCustomerNameText" runat="server" Text="客户名称： "></asp:Label>
                </div>
            </td>
            <td>
                <div>
                    <asp:Label runat="server" ID="lblCustomerName"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblCustomerNumberText" runat="server" Text="客户编号： "></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label runat="server" ID="lblCustomerNumber"></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label ID="lblSalesChannelsText" runat="server" Text="销售渠道： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblSalesChannels"></asp:Label>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblProcessingStoreText" runat="server" Text="处理商店： "></asp:Label>
                </div>
            </td>
            <td>
                <asp:Label runat="server" ID="lblProcessingStore"></asp:Label></td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label runat="server" ID="lblShipStoreText" Text="发货商店："></asp:Label>
                </div>
            </td>
            <td>
                <div>
                    <asp:Label runat="server" ID="lblShipStore"></asp:Label>
                </div>
            </td>

        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblNTSdateText" runat="server" Text="NTS日期： "></asp:Label>
                </div>
            </td>
            <td>
                <asp:Label runat="server" ID="lblNTSdate"></asp:Label></td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblPaymentTimeText" runat="server" Text="付款时间： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblPaymentTime"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lbldeliverydateText" runat="server" Text="发货日期：	 "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lbldeliverydate"></asp:Label>
                </div>
            </td>

        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblReceivingModeText" runat="server" Text="收货方式： "></asp:Label>
                </div>
            </td>
            <td>
                <asp:Label ID="lblReceivingMode" runat="server"></asp:Label></td>
            <td class="valign">
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblShippingAddressText" runat="server" Text="收货地址： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblShippingAddress"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblCarriersText" runat="server" Text="承运公司： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblCarriers"></asp:Label>
                </div>
            </td>

        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblReceiverText" runat="server" Text="收货人： "></asp:Label>
                </div>
            </td>
            <td>
                <asp:Label runat="server" ID="lblReceiver"></asp:Label></td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblReceiverPhoneText" runat="server" Text="收货人电话： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblReceiverPhone"></asp:Label>
                </div>
            </td>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblPackageUnitText" runat="server" Text="发货箱数： "></asp:Label>
                </div>
            </td>
            <td>
                <div >
                    <asp:Label runat="server" ID="lblPackageUnit"></asp:Label>
                </div>
            </td>

        </tr>
        <tr>
            <td>
                <div class="gdo-right-column-labels expressHeaderDetailLabel">
                    <asp:Label ID="lblWaybillNumberText" runat="server" Text="运单号：	 "></asp:Label>
                </div>
            </td>
            <td>
                <asp:Label runat="server" ID="lblWaybillNumber"></asp:Label></td>
            <td></td>
        </tr>
    </table>
    <br />
    <br />
    <div class="express-track-items">
        <asp:Label ID="pricelist" runat="server" Text="产品信息" CssClass="gdo-page-subheader green"></asp:Label>

        <asp:GridView runat="server" ID="ProductList" AutoGenerateColumns="false">

            <Columns>
                <asp:BoundField DataField="Sku" HeaderText="产品SKU" HeaderStyle-CssClass="rgHeader" />
                <asp:BoundField DataField="Description" HeaderText="产品名称" HeaderStyle-CssClass="rgHeader" />
                <asp:BoundField DataField="Quantity" HeaderText="数量" HeaderStyle-CssClass="rgHeader align-center" ItemStyle-CssClass="align-center"/>
                <asp:BoundField DataField="RetailPrice" HeaderText="产品金额" HeaderStyle-CssClass="rgHeader align-right" ItemStyle-CssClass="align-right" DataFormatString="{0:F2}"/>
            </Columns>

        </asp:GridView>
    </div>
    <br />
    <br />
    <asp:Label ID="Donation" runat="server" Text="捐款信息"  Visible="false" CssClass="gdo-page-subheader green"></asp:Label>
    <div class="express-track-items">
        <asp:GridView runat="server" ID="DonationGrd" AutoGenerateColumns="false" CellPadding="0" CellSpacing="0">
            <Columns>
                <asp:TemplateField HeaderText="捐款类型"  >
                     <ItemTemplate>
                         <div class="idonate">
                            <asp:Label ID="Label1" runat="server" Text="本人捐赠"></asp:Label>
                         </div>
                         <div class="behalf-donation">
                            <asp:Label ID="Label2" runat="server" Text="代顾客捐赠"></asp:Label>
                         </div>
                     </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="捐款金额" >
                    <ItemTemplate>
                        <div class="idonate">
                            <asp:Label ID="Label1" runat="server" Text=<%# Eval("SelfDonationAmount")%> CssClass="big_bold_letters"></asp:Label>
                        </div>
                        <div class="behalf-donation">
                            <asp:Label ID="Label2" runat="server" Text=<%# Eval("OnBehalfDonationAmount")%> CssClass="big_bold_letters"></asp:Label>
                        </div>               
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    <div class="express-track-items">
        <asp:Label runat="server" ID="Payment" Text="付款信息" CssClass="gdo-page-subheader green"></asp:Label>
        <br />

        <asp:GridView runat="server" ID="PaymentDetail" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="PaymentType" HeaderText="付款方式" HeaderStyle-CssClass="rgHeader" />
                <asp:BoundField DataField="PaymentStatus" HeaderText="付款状态" HeaderStyle-CssClass="rgHeader" />
                <asp:BoundField DataField="Amount" HeaderText="应付金额" DataFormatString="{0:0.00}" HeaderStyle-CssClass="rgHeader align-right" ItemStyle-CssClass="align-right"/>
                <asp:BoundField DataField="PaymentDate" dataformatstring="{0:MM/dd/yyyy HH:mm:ss}"  HeaderText="付款日期" HeaderStyle-CssClass="rgHeader align-right" ItemStyle-CssClass="align-right" />
            </Columns>

        </asp:GridView>
        
        <div  class="gdo-button-margin-rt bttn-back">
            <cc1:DynamicButton OnClientClick="return false;" ID="btnExpressTrack" ButtonType="Forward" OnClick="btnExpressTrack_OnClick" IconType="Plus"
                runat="server" Text="Ok" meta:resourcekey="Ok" />
        </div>
    </div>
    <table>
        <tr>
            <td>

    <div id="EDIdetail" style="display:none">
	                <div class="logo">
                        <asp:Label runat="server" ID="lblExpressTrackingHeader" CssClass="logoTitle" Text="Express Tracking Header" meta:resourcekey="lblExpressTrackingHeader" />
	                </div>
	                <div class="context">
		                <div class="detail" id="EDIContent">
                            <%=GetLocalResourceString("Downloading")%>
                        </div>
	                </div>
                </div>
            </tr>
        
     </table>
    <table>
        <tr>
            <td>
                  <div class="gdo-button-margin-rt bttn-back">
         <cc1:DynamicButton  runat="server" ButtonType="Neutral" meta:resourcekey="QRcodeDownload"  ID="btnQRcodeDownload" style="display:block;"  OnClick="btnQRcodeDownload_Click" OnClientClick="SetTarget();"/>    
    </div>
            </td>
        </tr>
    </table>
    <telerik:radajaxloadingpanel id="RadAjaxLoadingPanel" runat="server" skin="Sitefinity" />
</asp:Content>