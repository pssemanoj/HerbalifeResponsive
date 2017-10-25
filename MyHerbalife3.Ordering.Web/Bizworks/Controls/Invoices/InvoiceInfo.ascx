<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceInfo.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.InvoiceInfo" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/InvoiceCatalog.ascx" TagPrefix="Catalog" TagName="invoiceCatalog" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/BrowseContacts.ascx" TagPrefix="Contacts" TagName="invoiceContacts" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register TagPrefix="myhl" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
	<script type="text/javascript">
		function HideInvCatalogPopup() {
			var tooltip = $find("<%=invCatalogPopup.ClientID%>");
			tooltip.hide();
			return false;
		}

		function HideInvContactPopup() {
			var tooltip = $find("<%=invContactPopup.ClientID%>");
			tooltip.hide();
			return false;
		}

		//shan - mar 12, 2012 - open products popup
		function showProductsPopup() {
			var popup = $find("<%=invCatalogPopup.ClientID%>");
			//the method is in Invoicecatalog.ascx control
			//loop through the grid and clear the value in quantity fields
			clearQuantity();
			//show the popup
			popup.show();
			return false;
		}

		//shan - mar 15, 2012 - open contacts popup
		function showContactsPopup() {
			var popup = $find("<%=invContactPopup.ClientID%>");
			//the method is in BrowseContacts.ascx control
			//loop through the grid and clear the checkbox values
			clearContactSelection();
			//show the popup
			popup.show();
			return false;
		}
	</script>
</telerik:RadScriptBlock>
<telerik:RadAjaxLoadingPanel ID="InvoicePopupLoadingPanel" runat="server" ZIndex="100000" BackgroundPosition="Center">
</telerik:RadAjaxLoadingPanel>
<asp:UpdatePanel ID="updInvoiceInfo" runat="server" UpdateMode="Conditional">
	<ContentTemplate>
		<table class="tblInvoiceInfo">
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltInvoiceDateCaption" runat="server" meta:resourcekey="ltInvoiceDateCaption" Text="Invoice Date:"></asp:Literal>
                    </span>
				</td>
				<td>
					<telerik:RadDatePicker ID="rdpInvoiceDate" runat="server" Calendar-FastNavigationStep="12" Calendar-ShowRowHeaders="false" DateInput-ReadOnly="true" EnableEmbeddedSkins="False" EnableTyping="True">
					</telerik:RadDatePicker>
					<asp:Literal ID="ltInvoiceDate" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltInvoiceCaption" runat="server" meta:resourcekey="ltInvoiceCaption" Text="Invoice #:"></asp:Literal>
                    </span>
				</td>
				<td id="invoice_number">
					<asp:TextBox ID="txtInvoiceNumber" runat="server"></asp:TextBox>
					<asp:Literal ID="ltInvoiceNumber" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr id="trNameEdit" runat="server">
				<td>
                    <span>
					    <asp:Literal ID="ltFirstNameCaption" runat="server" Text="First Name:" meta:resourcekey="ltFirstNameCaption"></asp:Literal>
                    </span>
				</td>
				<td>
					<asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
                    <span>
					    <asp:Literal ID="ltLastNameCaption" runat="server" meta:resourcekey="ltLastNameCaption" Text="Last Name:"></asp:Literal>
                    </span>
				</td>
				<td>
					<asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					<asp:Panel runat="server" ID="InvContactsPanel" >
						<myhl:ArrowHyperLink ID="lnkBrowseContacts" runat="server" meta:resourcekey="lnkBrowseContacts" Text="Browse Contacts" onclick="javascript:return showContactsPopup();"></myhl:ArrowHyperLink>
						<telerik:RadToolTip ID="invContactPopup" runat="server" Modal="True" ShowEvent="OnClick" HideEvent="ManualClose" Position="TopCenter" RelativeTo="BrowserWindow" EnableEmbeddedSkins="False" ManualClose="True" meta:resourcekey="invContactPopup">
							<telerik:RadAjaxPanel runat="server" ID="InvContactsAjaxPanel" UpdateMode="Conditional" HorizontalAlign="NotSet" meta:resourcekey="InvContactsAjaxPanel" LoadingPanelID="InvoicePopupLoadingPanel">
								<Contacts:invoiceContacts ID="invContact" runat="server" />
							</telerik:RadAjaxPanel>
						</telerik:RadToolTip>
					</asp:Panel>
				</td>
			</tr>
			<tr id="trName" runat="server">
				<td>
                    <span>
					    <asp:Literal ID="ltNameCaption" runat="server" meta:resourcekey="ltNameCaption" Text="Name:"></asp:Literal>
                    </span>
				</td>
				<td>
					 <asp:Literal ID="ltName" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltAddressCaption" runat="server" meta:resourcekey="ltAddressCaption" Text="Street Address:"></asp:Literal>
                    </span>
				</td>
				<td colspan="4">
					<asp:TextBox ID="txtAddress" runat="server" Width="316px"></asp:TextBox>
					<asp:Literal ID="ltAddress" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltCityCaption" runat="server" meta:resourcekey="ltCityCaption" Text="City / State / Zip:"></asp:Literal>
                    </span>
				</td>
				<td>
					<asp:TextBox ID="txtCity" runat="server"></asp:TextBox>
                    <asp:Literal ID="ltCityEdit" runat="server"></asp:Literal>
					<asp:Literal ID="ltCityState" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					<asp:DropDownList ID="ddlState" runat="server">
					</asp:DropDownList>
				</td>
				<td>
					<asp:TextBox ID="txtZip" runat="server" Width="104px"></asp:TextBox>
					<asp:Literal ID="ltZipDash" runat="server" meta:resourcekey="ltZipDash" Text=" - "></asp:Literal>
					<asp:TextBox ID="txtZip2" runat="server" Width="104px"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltPhoneCaption" runat="server" meta:resourcekey="ltPhoneCaption" Text="Phone Number:"></asp:Literal>
                    </span>
				</td>
				<td>
					<asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
					<asp:Literal ID="ltPhone" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltEmailCaption" runat="server" meta:resourcekey="ltEmailCaption" Text="Email Address:"></asp:Literal>
                    </span>
				</td>
				<td colspan="4">
					<asp:TextBox ID="txtEmail" runat="server" Width="314px"></asp:TextBox>
					<asp:Literal ID="ltEmail" runat="server"></asp:Literal>
				</td>
				<td>
					&nbsp;
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td>
                    <span>
					    <asp:Literal ID="ltInvoiceTypeCaption" runat="server" meta:resourcekey="ltInvoiceTypeCaption" Text="Invoice Type:"></asp:Literal>
                    </span>
				</td>
				<td colspan="6">
					<div id="invoice_type_div">
						<asp:RadioButton ID="rbCustomer" runat="server" GroupName="InvoiceType" Text="Customer" meta:resourcekey="rbCustomer" Checked="true" />
						<asp:RadioButton ID="rbDistributor" runat="server" GroupName="InvoiceType" Text="Distributor" meta:resourcekey="rbDistributor" />
						<asp:Literal ID="ltInvoiceType" runat="server"></asp:Literal>
					</div>
				</td>
			</tr>
		</table>
	</ContentTemplate>
	<Triggers>
		<asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
	</Triggers>
</asp:UpdatePanel>
<asp:UpdatePanel ID="updRptProductItems" runat="server" UpdateMode="Conditional">
	<ContentTemplate>
		<table class="tblInvoiceInfoRepeater">
			<tr class="darker_green_bg">
				<td>
					<asp:Literal ID="ltSkuCaption" runat="server" meta:resourcekey="ltSkuCaption" Text="SKU#"></asp:Literal>
				</td>
				<td>
					<asp:Literal ID="ltProductionDescriptionCaption" runat="server" meta:resourcekey="ltProductionDescriptionCaption" Text="Product Description"></asp:Literal>
				</td>
				<td>
					<asp:Literal ID="ltQtyCaption" runat="server" meta:resourcekey="ltQtyCaption" Text="Qty."></asp:Literal>
				</td>
				<td>
					<asp:Literal ID="ltRetailPriceCaption" runat="server" meta:resourcekey="ltRetailPriceCaption" Text="Unit Retail Price"></asp:Literal>
				</td>
				<td>
					<asp:Literal ID="ltTotalVolPointsCaption" runat="server" meta:resourcekey="ltTotalVolPointsCaption" Text="Total Vol. Points"></asp:Literal>
				</td>
				<td>
					<asp:Literal ID="ltYourTotalPriceCaption" runat="server" meta:resourcekey="ltYourTotalPriceCaption" Text="Your Total Price"></asp:Literal>
				</td>
			</tr>
			<asp:Repeater ID="rptProductItems" runat="server">
				<ItemTemplate>
					<tr class="invoice_products">
						<td id="SKU_val">
							<span>
                                <asp:Literal ID="ltSkuTemplate" runat="server" />
                            </span>
							<asp:HiddenField ID="hdnItemID" runat="server" />
						</td>
						<td>
                            <span>
							    <asp:Literal ID="ltProductionDescriptionTemplate" runat="server" />
                            </span>
							<asp:LinkButton ID="lnkDeleteTemplate" runat="server" Text="remove" meta:resourcekey="lnkDeleteTemplate" />
						</td>
						<td>
							<asp:TextBox ID="txtQty" runat="server" AutoPostBack="true" MaxLength="3" /><asp:Literal ID="ltQty" runat="server" />
						</td>
						<td>
							<span class="right">
                                $ <asp:Literal ID="ltRetailPriceTemplate" runat="server" />
							</span>
						</td>
						<td>
							<span class="right">
								<asp:Literal ID="ltTotalVolPointsTemplate" runat="server" />
							</span>
						</td>
						<td>
							<span class="right">
                                $ 
								<asp:TextBox ID="txtTotalPriceTemplate" runat="server" AutoPostBack="true" />
								<asp:Literal ID="ltTotalPriceTemplate" runat="server" />
								<img id="imgErrPrice" runat="server" src="/Content/Global/bizworks/img/icons/icon-error.gif" alt="" visible="false" />
							</span>
						</td>
					</tr>
				</ItemTemplate>
			</asp:Repeater>
			<tr id="trNewItem" runat="server">
				<td>
					<asp:TextBox ID="txtNewSku" runat="server" AutoPostBack="true"></asp:TextBox>
				</td>
				<td>
					<asp:Panel runat="server" ID="InvCatalogPanel" >
						<myhl:ArrowHyperLink ID="lnkBrowseProduct" runat="server" meta:resourcekey="lnkBrowseProduct" Text="Browse Products" onclick="javascript:return showProductsPopup();"></myhl:ArrowHyperLink>
						<telerik:RadToolTip ID="invCatalogPopup" runat="server" Modal="True" ShowEvent="OnClick" HideEvent="ManualClose" Position="TopCenter" RelativeTo="BrowserWindow" EnableEmbeddedSkins="False" ManualClose="True" meta:resourcekey="invCatalogPopup">
							<Catalog:invoiceCatalog ID="invCatalog" runat="server" />
						</telerik:RadToolTip>
					</asp:Panel>
				</td>
				<td>
					<asp:TextBox ID="txtNewQty" runat="server" MaxLength="3" />
				</td>
				<td>
				</td>
				<td>
				</td>
				<td>
				</td>
			</tr>
			<tr>
				<td colspan="6">
				</td>
			</tr>
			<tr>
				<td colspan="3">
				</td>
				<td class="big_bold_letters">
					<asp:Literal ID="ltSubtotal" runat="server" Text="Subtotals:" meta:resourcekey="ltSubtotal" />
				</td>
				<td class="soft_green_bg big_bold_letters">
					<span>
						<asp:Literal ID="ltSubtotalTotalVolumePoints" runat="server" />
					</span>
				</td>
				<td class="darker_green_bg big_bold_letters">
					<span>$ <asp:Literal ID="ltSubtotalTotalPrice" runat="server" />
					</span>
				</td>
			</tr>
			<tr>
				<td colspan="6">
					<hr />
				</td>
			</tr>
			<tr>
				<td colspan="6">
					<asp:Label ID="lblErrMsg" runat="server" Style="color: Red;" />
				</td>
			</tr>
			<tr>
				<td colspan="5" align="right">
					<table width="1%">
						<tr>
							<td align="right">
								<img id="imgErrLoyalty" runat="server" src="/Content/Global/bizworks/img/icons/icon-error.gif" alt="" visible="false" />
								<asp:Literal ID="ltLoyaltyCaption" runat="server" Text="Customer Loyalty Program:" meta:resourcekey="ltLoyaltyCaption" />
								<asp:Literal ID="ltLoyaltyCurrency" runat="server" Text="$" />
								<asp:TextBox ID="txtLoyaltyAmount" runat="server" />
								<asp:Literal ID="ltLoyaltyOr" runat="server" Text="or" meta:resourcekey="ltLoyaltyOr" />
								<asp:TextBox ID="txtLoyaltyPercentage" runat="server" />
								<asp:Literal ID="ltLoyaltyPercentage" runat="server" Text="%" meta:resourcekey="ltLoyaltyPercentage" />
							</td>
							<td>
		                        <content:DynamicButton runat="server" ID="btnApplyLoyalty" meta:resourcekey="btnApplyLoyalty" Text="Apply" ButtonType="Neutral" />
							</td>
						</tr>
					</table>
				</td>
				<td class="soft_green_bg big_bold_letters">
					<span>-$ <asp:Literal ID="ltDiscount" runat="server" />
					</span>
				</td>
			</tr>
			<tr>
				<td colspan="5" align="right" class="big_bold_letters">
					<asp:Literal ID="ltNetSubtotalCaption" runat="server" Text="Your Subtotal:" meta:resourcekey="ltNetSubtotalCaption" />
				</td>
				<td class="darker_green_bg big_bold_letters">
					<span>$ <asp:Literal ID="ltNetSubtotal" runat="server" />
					</span>
				</td>
			</tr>
		</table>
	</ContentTemplate>
	<Triggers>
		<asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
	</Triggers>
</asp:UpdatePanel>
<div style="float: right;">
	<content:DynamicButton runat="server" ID="btnContinue"  meta:resourcekey="btnContinue" Text="Continue" ButtonType="Forward" />
</div>
<div style="float: right; margin-right: 5px;">
	<content:DynamicButton runat="server" ID="btnClear"  meta:resourcekey="btnClear" Text="Clear" ButtonType="Back" OnClick="btnClear_Click" />
</div>
<div style="width: 1px; height: 1px; clear: both;">
</div>
<asp:Literal ID="ltNoteCaption" runat="server" Text="This invoice will not adjust any credit card charges that are processed for orders made on your website. For those orders, please be sure the invoice you create reflects exact charges as paid." meta:resourcekey="ltNoteCaption" />