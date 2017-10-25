<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceListView.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.InvoiceListView" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<script type="text/javascript" >
    $(function () {
        $('.rgHeader').last().append($('#createOrderHelpIcon'));
    });
    function GetCreateOrderHelpText() {
        var tooltip = $find("<%=HMenuExtCreateOrder.ClientID%>")
        if (tooltip) {
            if (tooltip.show) {
                tooltip.show();
            }
        }
        if (event) {
            event.returnValue = false;
        }
    }
</script>
<%--Export to Excel--%>
<div>
	<asp:Panel runat="server" ID="invoiceListMainPanel" meta:resourcekey="invoiceListMainPanel">
		<div class="title-div">
			<h1>
				<asp:Literal ID="ltMyInvoices" runat="server" Text="My Invoices" meta:resourcekey="ltMyInvoices"></asp:Literal>
			</h1>
		</div>
		<asp:Panel runat="server" ID="searchInvoicePanel"  CssClass="hrblPanel">
			<div>
				<h3>
					<asp:Literal ID="ltSearchInvoicePanel" runat="server" Text="Filter and Display My Invoices" meta:resourcekey="ltSearchInvoicePanel"></asp:Literal>
				</h3>
			</div>
			<div id="searchfilterinv">
				<div id="searchbydate" class="searchinvblocks">
					<span class="search_titles">
						<asp:Literal runat="server" ID="ltSearchByInvoiceDate" Text="Search By Invoice Date" meta:resourcekey="ltSearchByInvoiceDate" />
					</span>
					<div>
						<span class="filter_invoice_labels">
							<asp:Literal runat="server" ID="ltFrom" Text="From:" meta:resourcekey="ltFrom" />
						</span>
						<telerik:RadDatePicker ID="FromDate" runat="server" Skin="Hay" meta:resourcekey="FromDate">
							<Calendar runat="server" Skin="Hay" FastNavigationStep="12" ShowRowHeaders="False" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x">
							</Calendar>
							<DateInput runat="server" DateFormat="d" DisplayDateFormat="d" Skin="Hay" Width="">
							</DateInput>
							<DatePopupButton CssClass="" HoverImageUrl="" ImageUrl="" />
						</telerik:RadDatePicker>
					</div>
					<div>
						<span class="filter_invoice_labels">
							<asp:Literal runat="server" ID="ltTo" Text="To:" meta:resourcekey="ltTo" />
						</span>
						<telerik:RadDatePicker ID="ToDate" runat="server" Skin="Hay" meta:resourcekey="ToDate" Culture="en-US">
							<Calendar runat="server" Skin="Hay" FastNavigationStep="12" ShowRowHeaders="False" UseColumnHeadersAsSelectors="False" UseRowHeadersAsSelectors="False" ViewSelectorText="x">
							</Calendar>
							<DateInput runat="server" DateFormat="d" DisplayDateFormat="d" Skin="Hay" Width="">
							</DateInput>
							<DatePopupButton CssClass="" HoverImageUrl="" ImageUrl="" />
						</telerik:RadDatePicker>
					</div>
				</div>
				<div id="searchbyamount" class="searchinvblocks">
					<span class="search_titles">
						<asp:Literal runat="server" ID="ltSearchAmount" Text="Search By Amount Due" meta:resourcekey="ltSearchAmount" />
					</span>
					<div>
						<span class="filter_invoice_labels">
							<asp:Literal runat="server" ID="ltStartAmount" Text="$:" meta:resourcekey="ltStartAmount" />
						</span>
						<asp:TextBox runat="server" ID="txtStartAmount"></asp:TextBox>
					</div>
					<div>
						<span class="filter_invoice_labels">
							<asp:Literal runat="server" ID="ltEndAmount" Text="$:" meta:resourcekey="ltEndAmount" />
						</span>
						<asp:TextBox runat="server" ID="txtEndAmount"></asp:TextBox>
					</div>
				</div>
				<div id="searchby" class="searchinvblocks">
					<span class="search_titles">
						<asp:Literal runat="server" ID="ltSearchBy" Text="Search By" meta:resourcekey="ltSearchBy" />
					</span>
					<div>
						<asp:DropDownList ID="ddlSearchBy" runat="server" >
							<asp:ListItem Text="First Name" Value="FirstName" meta:resourcekey="ltFirstName"></asp:ListItem>
							<asp:ListItem Text="Last Name" Value="LastName" meta:resourcekey="ltLastName"></asp:ListItem>
							<asp:ListItem Text="Street Address" Value="StreetAddress" meta:resourcekey="ltStreetAddress"></asp:ListItem>
							<asp:ListItem Text="City" Value="City" meta:resourcekey="ltCity"></asp:ListItem>
							<asp:ListItem Text="State" Value="State" meta:resourcekey="ltState"></asp:ListItem>
							<asp:ListItem Text="Zip Code" Value="ZipCode" meta:resourcekey="ltZipCode"></asp:ListItem>
							<asp:ListItem Text="SKU" Value="SKU" meta:resourcekey="ltSKU"></asp:ListItem>
							<asp:ListItem Text="Description" Value="Description" meta:resourcekey="ltDescription"></asp:ListItem>
							<asp:ListItem Text="Total Volume Points" Value="TotalVolumePoints" meta:resourcekey="ltTotalVolumePoints"></asp:ListItem>
							<asp:ListItem Text="Invoice Total" Value="InvoiceTotal" meta:resourcekey="ltInvoiceTotal"></asp:ListItem>
						</asp:DropDownList>
					</div>
					<div>
						<asp:TextBox runat="server" ID="txtSearchBy"></asp:TextBox>
						<ajaxToolkit:TextBoxWatermarkExtender runat="server" ID="SearchByWatermark" TargetControlID="txtSearchBy" meta:resourcekey="SearchByWatermark" />
					</div>
				</div>
				<div id="searchgobuttons">
					<div id="goSearchByDate_div">
						<content:DynamicButton runat="server" ID="goSearchByDate" Text="Go" meta:resourcekey="goSearchByDate" ButtonType="Forward"  />
					</div>
					<div id="goSearchBy_div">
		                <content:DynamicButton runat="server" ID="goSearchBy" Text="Go" meta:resourcekey="goSearchBy" ButtonType="Forward"  />
					</div>
				</div>
			</div>
		</asp:Panel>
		<content:DynamicButton runat="server" ID="createInvoice" Text="Create New Invoice" meta:resourcekey="createInvoice" ButtonType="Forward"  />
		<div id="divExport">
			<asp:Image ID="imgExport" runat="server" ImageUrl="/Content/Global/img/xls20x20.gif" meta:resourcekey="imgExport" />
			<asp:LinkButton ID="lnkExportToExcel" Text="Export to Excel" OnClick="OnExportToExcel" runat="server" meta:resourcekey="lnkExportToExcel" />
		</div>
		<asp:Panel ID="rgdInvoicesPanel" runat="server" meta:resourcekey="rgdInvoicesPanel" CssClass="show_invoices_div">
			<telerik:RadGrid EnableLinqExpressions="False" ID="rgdInvoices" Width="100%" AllowPaging="True" runat="server" AutoGenerateColumns="False" GridLines="None" OnItemDataBound="rgdInvoices_ItemDataBound" meta:resourcekey="rgdInvoices" PageSize="25"
                OnItemCommand="InvoicesItemCommand" CssClass="hrblTable">
                <PagerStyle Mode="NextPrevAndNumeric"></PagerStyle>
				<MasterTableView DataKeyNames="ID" Width="100%" AllowSorting="true" PagerStyle-Position="TopAndBottom" AllowNaturalSort="false">
					<SortExpressions>
						<telerik:GridSortExpression FieldName="DistributorInvoiceNumber" SortOrder="Descending" />
					</SortExpressions>
					<Columns>
						<telerik:GridTemplateColumn HeaderText="Name" meta:resourcekey="rgdInvoices_Name" SortExpression="LastName" UniqueName="Name">
							<ItemTemplate>
								<asp:Label ID="lbName" runat="server" Text='<%# Eval("LastName") + ", " + Eval("FirstName") %>'></asp:Label>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Invoice Type" meta:resourceKey="rgdInvoices_Type" SortExpression="Type" UniqueName="Type">
							<ItemTemplate>
								<asp:Label ID="lbType" runat="server" Text='<%# LocalizeType(Eval("Type").ToString()) %>'></asp:Label>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Invoice #" meta:resourceKey="rgdInvoices_InvoiceNumber" SortExpression="DistributorInvoiceNumber" UniqueName="InvoiceNumber">
							<ItemTemplate>
								<asp:Label ID="lblID" runat="server" Visible="false" Text='<%# LocalizeType(Eval("ID").ToString()) %>'></asp:Label>
								<asp:HyperLink ID="invoiceNumberLinkButton" runat="server" CommandArgument='<%# Eval("DistributorInvoiceNumber") %>' Text='<%# Eval("DistributorInvoiceNumber","{0:000000}") %>' NavigateUrl='<%# "~/Bizworks/MyInvoiceDetails.aspx?invoiceId=" + Eval("DistributorInvoiceNumber") %>'></asp:HyperLink>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="Invoice #" meta:resourceKey="rgdInvoices_InvoiceNumber" SortExpression="DistributorInvoiceNumber2" UniqueName="InvoiceNumber2" Visible="False">
							<ItemTemplate>
								<asp:Label ID="invoiceNumberLabel" runat="server" Text='<%# Eval("DistributorInvoiceNumber","{0:000000}") %>'></asp:Label>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Invoice Date" meta:resourceKey="rgdInvoices_CreatedDate" SortExpression="InvoiceDate" UniqueName="InvoiceDate">
							<ItemTemplate>
								<asp:Label ID="lblCreatedDateVal" runat="server" Text='<%# Eval("InvoiceDate","{0:d}") %>'></asp:Label>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Invoice Status" meta:resourceKey="rgdInvoices_Status" SortExpression="Status" UniqueName="Status">
							<ItemTemplate>
								<asp:Label ID="lbStatus" runat="server" Text='<%# LocalizeStatus(Eval("Status").ToString()) %>'></asp:Label>
							</ItemTemplate>
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Volume Points" meta:resourceKey="rgdInvoices_VolumePoints" SortExpression="TotalVolumePoints" UniqueName="VolumePoints">
							<ItemTemplate>
								<asp:Label ID="lbVolumenPoints" runat="server" Text='<%# Eval("TotalVolumePoints","{0:0.00}") %>'></asp:Label>
							</ItemTemplate>
							<HeaderStyle HorizontalAlign="Right" />
							<ItemStyle HorizontalAlign="Right" />
						</telerik:GridTemplateColumn>
						<telerik:GridTemplateColumn HeaderText="Invoice Total" meta:resourceKey="rgdInvoices_Total" SortExpression="TotalDue" UniqueName="Total">
							<ItemTemplate>
								<asp:Label ID="lbTotal" runat="server" Text='<%# Eval("TotalDue","{0:0.00}") %>'></asp:Label>
							</ItemTemplate>							
							<HeaderStyle HorizontalAlign="Right" />
							<ItemStyle HorizontalAlign="Right" />
						</telerik:GridTemplateColumn>
						<telerik:GridBoundColumn HeaderText="Products" UniqueName="Skus" Visible="false">
						</telerik:GridBoundColumn>
						<telerik:GridTemplateColumn HeaderText="Create Order" meta:resourceKey="rgdInvoices_CreateOrder" UniqueName="CreateOrderColumn" Visible="false">
							<ItemTemplate>
		                        <content:DynamicButton runat="server" ID="createOrder" ButtonType="Link" CommandName="CreateOrder" IconType="Cart" Text="Create Order" meta:resourceKey="rgdInvoices_CreateOrder"/>
							</ItemTemplate>							
							<HeaderStyle HorizontalAlign="Right" />
							<ItemStyle HorizontalAlign="Right" />
						</telerik:GridTemplateColumn>
					</Columns>
				</MasterTableView>
			</telerik:RadGrid>
            <a href="javascript:GetCreateOrderHelpText();" id="createOrderHelpIcon">
                <img alt="" id="imgCreateOrderHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif"
                    height="12" width="12" />
            </a>
            <asp:Panel ID="pnlCreateOrderHelpText" runat="server" Style="display: none; width: 300px">
                <div class="gdo-popup saveCartsPopUp">
                    <asp:Label ID="lblCreateOrderHelp" runat="server" meta:resourcekey="lblCreateOrderHelp"/>
                </div>
            </asp:Panel>
            <ajaxToolkit:HoverMenuExtender ID="HMenuExtCreateOrder" runat="server" TargetControlID="imgCreateOrderHelp"
                PopupControlID="pnlCreateOrderHelpText" />
		</asp:Panel>
	</asp:Panel>
</div>
