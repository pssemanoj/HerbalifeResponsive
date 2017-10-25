<%@ Page Language="C#" MasterPageFile="~/MasterPages/Ordering.master"  AutoEventWireup="true" 
CodeBehind="PendingOrders.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.PendingOrders" meta:resourcekey="PageResource1"%>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">

</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">

 <div class="field" id="ContentDisplay">
      <cc1:ContentReader ID="_ContentReader" runat="server" ContentPath="PendingOrderInstr.Html" SectionName="ordering" ValidateContent="true" UseLocal="true" />
 </div>

 <div id="divGrid" class="limeContentGrad">
    <telerik:RadGrid ID="OrdersGrid" Width="100%" AllowPaging="True" runat="server"
            AutoGenerateColumns="False" GridLines="None" ShowHeader="true" 
            meta:resourcekey="OrdersGrid"  OnNeedDataSource="OrdersGrid_NeedDataSource"  OnItemDataBound="OrdersGrid_ItemDataBound" EnableLinqExpressions="false">
           <PagerStyle Mode="NextPrevAndNumeric" Position="TopAndBottom" AlwaysVisible="true"
           PagerTextFormat="{4} Page {0} of {1}  " ShowPagerText="true" CssClass="gdo-pagination-active gdo-NumericPager" />
               <MasterTableView Width="100%" AllowPaging="true">
                <NoRecordsTemplate>
                    <asp:Literal ID="ltNoRecords" runat="server" Text="No Records" meta:resourcekey="NoRecords"></asp:Literal>
                </NoRecordsTemplate>
                <ItemStyle VerticalAlign="Top" />
                <Columns>
                    <telerik:GridTemplateColumn HeaderText="Order Number" meta:resourcekey="OrderNumberResource" SortExpression="OrderId">
					<ItemTemplate>
							<asp:Label ID="lblOrderId" runat="server" />
					</ItemTemplate>
				</telerik:GridTemplateColumn>
                <telerik:GridTemplateColumn HeaderText="Payment Status" meta:resourcekey="PaymentStatusResource" SortExpression="PaymentStatus">
					<ItemTemplate>
							<asp:Label ID="lblPaymentStatus" runat="server" />
					</ItemTemplate>
				</telerik:GridTemplateColumn>
                 <telerik:GridTemplateColumn HeaderText="Submitted Date" meta:resourcekey="SubmittedDateResource" SortExpression="SubmittedDate">
					<ItemTemplate>
							<asp:Label ID="lblSubmittedDate" runat="server" />
					</ItemTemplate>
				</telerik:GridTemplateColumn>
                <telerik:GridTemplateColumn HeaderText="Amount Due" meta:resourcekey="AmountDueResource" SortExpression="AmountDue">
					<ItemTemplate>
							<asp:Label ID="lblAmountDue" runat="server" />
					</ItemTemplate>
				</telerik:GridTemplateColumn>
                <telerik:GridTemplateColumn HeaderText="Volume Points" meta:resourcekey="VolumePointsResource" SortExpression="VolumePoints">
					<ItemTemplate>
							<asp:Label ID="lblVolumePoints" runat="server" />
					</ItemTemplate>
				</telerik:GridTemplateColumn>
                 </Columns>
               </MasterTableView>
    </telerik:RadGrid>        
 </div>

</asp:Content>
