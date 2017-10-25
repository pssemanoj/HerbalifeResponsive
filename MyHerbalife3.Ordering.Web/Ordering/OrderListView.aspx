<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="OrderListView.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OrderListView" EnableEventValidation="false" meta:resourcekey="PageResource1" %>

<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register Src="~/Ordering/Controls/PagingControl.ascx" TagName="PagingControl" TagPrefix="uc1" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">
     <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
    <link rel="stylesheet" href="//code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css">
    <script src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>
    <script type="text/javascript">
        $(function () {
            $('#<%=txtStartDate.ClientID %>').kendoDatePicker({
                format: "yyyy-MM-dd",
                start: "year",
                depth: "month",
                footer: false
            });
            $('#<%=txtEndDate.ClientID %>').kendoDatePicker({
                format: "yyyy-MM-dd",
                start: "year",
                depth: "month",
                footer: false
            });
        });
        var months = [
            '<%= GetLocalResourceObject("January") as string %>',
            '<%= GetLocalResourceObject("February") as string %>',
            '<%= GetLocalResourceObject("March") as string %>',
            '<%= GetLocalResourceObject("April") as string %>',
            '<%= GetLocalResourceObject("May") as string %>',
            '<%= GetLocalResourceObject("June") as string %>',
            '<%= GetLocalResourceObject("July") as string %>',
            '<%= GetLocalResourceObject("August") as string %>',
            '<%= GetLocalResourceObject("September") as string %>',
            '<%= GetLocalResourceObject("October") as string %>',
            '<%= GetLocalResourceObject("November") as string %>',
            '<%= GetLocalResourceObject("December") as string %>'
        ];

        $.each(months, function (key, month) {
            if (month != "") {
                kendo.cultures.current.calendars.standard.months.namesAbbr[key] = month;
            }
        });
    </script>
    <style type="text/css">
        .ui-tabs-active {
            background: #4eb106 !important;
        }

        #OrderingMenu {
            border-top-right-radius: 0 !important;
            border-top-left-radius: 0 !important;
        }

        #PreOrderingMenu {
            border-top-right-radius: 0 !important;
            border-top-left-radius: 0 !important;
        }

        #divOrderHistoryPanel {
            padding: 0px !important;
        }

        #divPreOrderHistoryPanel {
            padding: 0px !important;
        }

        .borderless {
            border: none !important;
        }

        .nobg {
            background: none !important;
        }

        .loadingPanel {
            z-index: 100;
            background-image: url('/SharedUI/Images/icons/LoadingGreenCircle.gif');
            background-repeat: no-repeat;
            background-position: center center;
            height: 100px;
        }

        .selectedActionButton {
            background: #4eb106 !important;
            border: none !important;
            cursor: pointer;
            font-size: 13px;
            color: #ffffff !important;
            padding: 6px 13px;
        }

        .actionButton {
            background: #e6e6e6 !important;
            border: none !important;
            cursor: pointer;
            font-size: 13px;
            color: #212121 !important;
            padding: 6px 13px;
        }

    </style>
    <script type="text/javascript" src="/Ordering/Scripts/order-list-view.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:Button ID="btnOrdering" runat="server" OnClick="btnOrdering_Click" CssClass="selectedActionButton" />
    <asp:Button ID="btnPreOrdering" runat="server" OnClick="btnPreOrdering_Click"  CssClass="actionButton" />
    <div id="divOrderHistoryPanel">
        <div id="topRecentCarts">
            <p id="pMessaging" class="red" runat="server">
            </p>
            <div class="search_cart">
                <div class="row">
                    <div class="col-sm-2">
                        <asp:Label runat="server" ID="lblStartYearMonth" meta:resourcekey="lblStartYearMonth" AssociatedControlID="txtStartDate"></asp:Label>
                        <asp:TextBox ID="txtStartDate" runat="server" />
                    </div>
                    <div class="col-sm-2">
                        <asp:Label runat="server" ID="lblEndYearMonth" meta:resourcekey="lblEndYearMonth" AssociatedControlID="txtEndDate"></asp:Label>
                        <asp:TextBox ID="txtEndDate" runat="server" />
                    </div>
                    <div class="col-sm-2 hidden-xs">
                        <asp:Label runat="server" ID="lblOrderStatus" meta:resourcekey="lblOrderStatus" AssociatedControlID="ddlOrderStatus"></asp:Label>
                        <asp:DropDownList runat="server" ID="ddlOrderStatus" OnSelectedIndexChanged="OrderStatusSelectedChanged" DataTextField="Value" DataValueField="Key" CssClass="ddlOrderStatus"></asp:DropDownList>
                    </div>
                    <div class="col-sm-2 col-xs-11 txtSearch">
                        <div class="search-wrapper">
                            <label>&nbsp;</label>
                            <div style="float: left; display: inline-block; width: 80%;">
                                <asp:TextBox ID="txtSearch" runat="server"></asp:TextBox>
                            </div>
                            <div style="display: inline-block; float: left;">
                                <asp:Button runat="server" ID="btnSearch" OnClick="SearchOrderInfo"></asp:Button>
                            </div>
                        </div>
                    </div>
                    <div class="col-sm-2 hidden-xs">
                        <div>
                            <asp:Label runat="server" ID="lblOrderBy" meta:resourcekey="OrderBy" Text="Sort By" AssociatedControlID="ddlOrderBy"></asp:Label>
                            <asp:DropDownList AutoPostBack="True" runat="server" ID="ddlOrderBy" OnSelectedIndexChanged="OrderBySelectedChanged"></asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="clear"></div>
            </div>
            <asp:LinkButton runat="server" ID="DiscSkuLabelListLinkButton" CssClass="red" Visible="False" Text="Discontinued Sku Exist" meta:resourcekey="ErrorCopyOrderDiscontinueSkuExist"></asp:LinkButton>
            <asp:Label runat="server" ID="ErrorMessageDiscSkuLabel" CssClass="red" Visible="False" Text="Discontinued Sku Exist" meta:resourcekey="ErrorCopyOrderDiscontinueSkuExist"></asp:Label>
            <div class="gdo-form-label-left"></div>
            <div class="clear"></div>
        </div>
        <form action="OrderListView.aspx" method="post"></form>
        <div id="divGrid" class="orderHistory">
            <asp:PlaceHolder ID="OrderHistoryGrid" runat="server" />
            <asp:Label runat="server" ID="lblNoRecords" Text="No Records" Visible="False" meta:resourcekey="NoRecordsMessage"></asp:Label>
            <uc1:PagingControl ID="PagingControl1" runat="server" PageSize="4" />
        </div>
    </div>
    <div id="divLoading" class="hide loadingPanel">
    </div>
    <asp:UpdatePanel ID="UpdatePanelDupeOrder" runat="server" UpdateMode="Conditional" >
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="conformationPopupExtender" runat="server" TargetControlID="DupeOrderFakeTarget"
                PopupControlID="pnldupeOrderMonth" CancelControlID="DupeOrderFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false"  />
            <asp:Button ID="DupeOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupeOrderMonth" runat="server"  Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Are you sure to Decline ?" meta:resourcekey="lblConfirm"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <%--<asp:Button ID="OK" Text="Yes" runat="server" OnClick="Ok_click" meta:resourcekey="BtnOK"/>--%>
                        <cc1:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="Onokclick" meta:resourcekey="BtnOK" />
                      <%--  <asp:Button ID="No" Text="No" runat="server" OnClick="No_click" meta:resourcekey="BtnNo"/>--%>
                        <cc1:DynamicButton ID="DynamicButtonNo" runat="server" ButtonType="Forward" Text="No" OnClick="Onnoclock" meta:resourcekey="BtnNo" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <ajaxToolkit:ModalPopupExtender ID="displaySkuPopupModal" runat="server" TargetControlID="DiscSkuLabelListLinkButton"
                PopupControlID="pnlshowDiscSku" BackgroundCssClass="modalBackground" CancelControlID="ClosePopupButton"
                DropShadow="false"  />
            <asp:Panel ID="pnlshowDiscSku" runat="server"  Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="DiscSkuLabelList" runat="server" ></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="Label2" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="ClosePopupButton" runat="server" CausesValidation="False" ButtonType="Forward" Text="Ok" OnClick="OnDisplayDiscontinueSkuClose" meta:resourcekey="BtnOK" />
                    </div>
                </div>
            </asp:Panel>
</asp:Content>

