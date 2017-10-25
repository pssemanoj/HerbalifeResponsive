<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master"
    AutoEventWireup="true" CodeBehind="OnlineInvoice.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OnlineInvoice" meta:resourcekey="PageResource1"  EnableEventValidation="false" %>
<%@ Register Src="~/Ordering/Controls/PagingControl.ascx" TagName="PagingControl" TagPrefix="uc1" %>
<%@ Register TagPrefix="CnInvoiceRule" TagName="CnInvoiceRule" Src="~/Ordering/Controls/RuleRegulation.ascx" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <link rel="stylesheet" href="//code.jquery.com/ui/1.11.4/themes/smoothness/jquery-ui.css">
    <script type="text/javascript" src="/Ordering/Scripts/order-list-view.js"></script>
    <script src="//code.jquery.com/ui/1.11.4/jquery-ui.js"></script>
    <script type="text/javascript">
        setTimeout(function(){ 
            $("[id$='printThisPage']")
                .css('margin-top', '15px')
                .parent()
                .css('min-height', '800px')
                .css('height', 'auto');
        }, 5000);

        $(document).ready(function () {
            toggleCatalog();
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
                if (month !== "") {
                    kendo.cultures.current.calendars.standard.months.namesAbbr[key] = month;
                }
            });
        });

        function ShowOrHideDetail(orderId, type) {

            if (type === 'SI')
            {
                $('#divDetailSingle').css('display', 'inline-block');
                $('#divDetailSplit').css('display', 'none');
            }
            else if (type === 'SP')
            {
                $('#divDetailSingle').css('display', 'none');
                $('#divDetailSplit').css('display', 'inline-block');
            }
            else
            {
                $('#divDetailSingle').css('display', 'none');
                $('#divDetailSplit').css('display', 'none');
            }

            if (type === 'SI')
            {
                $('#lblInvoiceAmount').Text();

            }



        }
    </script>
    <script type="text/javascript">   
        function showModalPopUp(type, orderid, startDate, endDate) {
            var url = 'OnlineInvoiceDetail.aspx?type=' + type + '&orderid=' + orderid + '&startDate=' + startDate + '&endDate=' + endDate;
             window.location.href = url;
            }

    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server"> </asp:ScriptManagerProxy>

<div runat="server" id="DivProductDetail">
    <div id = "divBackground" class="InvoiceBackground" >
        <cc1:DynamicButton ID="DynamicButton1" runat="server" ButtonType="Forward"  meta:resourcekey="ButtonOk" />
    </div>

    <div id="divGrid" class="orderHistory">
        <div class="col-sm-2">
            <div class="search_cart">
                <div class="row">
                    <div class="col-sm-2">
                        <asp:Label runat="server" ID="lblStartYearMonth" AssociatedControlID="txtStartDate" meta:resourcekey="LabelStartDate"></asp:Label>
                        <asp:TextBox ID="txtStartDate" runat="server" />
                    </div>
                    <div class="col-sm-2">
                        <asp:Label runat="server" ID="lblEndYearMonth" AssociatedControlID="txtEndDate" meta:resourcekey="LabelEndDate"></asp:Label>
                        <asp:TextBox ID="txtEndDate" runat="server" />
                    </div>
                    <div class="col-sm-1">
                        <asp:Label runat="server" ID="lblInvoiceStatus" AssociatedControlID="InvoiceStatusddl" meta:resourcekey="OnlineInvoiceOrderStatusDropdown"></asp:Label>
                        <asp:DropDownList runat="server" ID="InvoiceStatusddl" Width="150px" >
                            <asp:ListItem meta:resourcekey="InvoiceStatusAll" Selected="True" Value="0"></asp:ListItem>
                            <asp:ListItem meta:resourcekey="InvoiceStatusUnbilled" Value="01"></asp:ListItem>
                            <asp:ListItem meta:resourcekey="InvoiceStatusApplication" Value="02"></asp:ListItem>
                            <asp:ListItem meta:resourcekey="InvoiceStatusInvoiced" Value="03"></asp:ListItem>
                            <asp:ListItem meta:resourcekey="InvoiceStatusFailed" Value="04"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-sm-2 col-xs-11 txtSearch">
                        <div class="search-wrapper">
                            <label>&nbsp;</label>
                            <div style="display: inline-block; float: left;">
                                <asp:Button runat="server" ID="btnSearch" OnClick="SearchOrderInfo"></asp:Button>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div>
                        <asp:RadioButtonList runat="server" ID="InvoiceStatusRadioButton" CssClass="radioWithProperWrap" RepeatDirection= "Horizontal">
                            <asp:ListItem meta:resourcekey="OnlineInvoiceFilterCurrent" Value="0" Selected="True" ></asp:ListItem>
                            <asp:ListItem meta:resourcekey="OnlineInvoiceFilterArchived" Value="1"></asp:ListItem>
                        </asp:RadioButtonList>
                    </div>

                </div>
                <div class="clear"></div>
                <div></div>
            </div>
        </div>
<asp:UpdateProgress ID="UpdateProgress1" runat="server">
    <ProgressTemplate>
        <h3>页面加载中 ...</h3>
    </ProgressTemplate>
</asp:UpdateProgress>
        <br />
<asp:UpdatePanel ID="upLoadOrders" runat="server" UpdateMode="Conditional">
<ContentTemplate>
        <asp:PlaceHolder ID="OrderViewPlaceHolder" runat="server" />
        <asp:Label runat="server" ID="lblNoRecords" Visible="False" meta:resourcekey="GridNoRecordsMessage"></asp:Label>
        <uc1:PagingControl ID="PagingControl1" runat="server" PageSize="5"  />
</ContentTemplate>
<Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click"  />
</Triggers>
</asp:UpdatePanel>

        <CnInvoiceRule:CnInvoiceRule runat="server" ID="CnInvoiceRule" />
   </div>
</div>


</asp:Content>
