<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceCatalog.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.InvoiceCatalog" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<telerik:RadScriptBlock ID="RadScriptBlockFollowups" runat="server">
	<script type="text/javascript">
		function OnInvoiceCatalogCancelClick() {
			return HideInvCatalogPopup();
		}
		//shan - mar 12, 2012 - to clear the qty fields on opening the popup
		function clearQuantity() {
			//get the item and clear the value
			$('.divCatalogRepeaterTable').find('input[type=text]').each(function () {
				this.value = '';
			});
			//clear hidden field having the previously entered qty values
			$('.hiddenEnteredQtys').find('input[type=hidden]')[0].value = '';
		}

		function setQtyNumeric(evt) {
			var charCode = (evt.which) ? evt.which : event.keyCode
			if (charCode > 31 && (charCode < 48 || charCode > 57))
				return false;

			return true;
		}
	</script>
</telerik:RadScriptBlock>
<asp:UpdatePanel ID="pnlInvCatalog" runat="server" UpdateMode="Conditional">
	<ContentTemplate>
		<div class="divInvCatalogClass">
			<div class="gdo-spacer3">
			</div>
			<table width="100%" cellspacing="1" cellpadding="0" border="0" class="gdo-pricelist-tbl" style="background-color: white;">
				<tbody>
					<tr>
						<td>
							<table class="gdo-pricelist-tbl-top" border="0" cellspacing="0" cellpadding="0">
								<tr>
									<td colspan="2">
										<div class="gdo-pricelist-label">
											<asp:Literal runat="server" ID="litStep1" Text="Step 1: " meta:resourcekey="litStep1" />
											<span class="gdo-body-text">
												<asp:Literal runat="server" ID="litStep1Instr" Text="Select a category to view products." meta:resourcekey="litStep1Instr" />
											</span><span>
												<asp:LinkButton ID="lnkClose" Text="Close" runat="server" OnClick="lnkClose_OnCLick" OnClientClick="OnInvoiceCatalogCancelClick();" meta:resourcekey="lnkClose" />
											</span>
										</div>
										<div class="gdo-pricelist-label">
											<asp:Literal runat="server" ID="litStep2" Text="Step 2: " meta:resourcekey="litStep2" />
											<span class="gdo-body-text">
												<asp:Literal runat="server" ID="litStep2Instr" Text="For each product you wish to order, enter the desired quantity." meta:resourcekey="litStep2Instr" />
											</span>
										</div>
										<div class="gdo-pricelist-label">
											<asp:Literal runat="server" ID="litStep3" Text="Step 3: " meta:resourcekey="litStep3" />
											<span class="gdo-body-text">
												<asp:Literal runat="server" ID="litStep3Instr" Text="Once you have finished selecting the products you want, click the Add to Invoice button." meta:resourcekey="litStep3Instr" />
											</span>
										</div>
									</td>
								</tr>
								<tr>
									<td colspan="2">
										<span class="gdo-pricelist-label">
											<asp:Literal runat="server" ID="litSelectCategory" Text="Select Category:" meta:resourcekey="litSelectCategory" />
										</span>
										<asp:DropDownList AutoPostBack="True" runat="server" ID="CategoryDropdown" DataTextField="DisplayName" DataValueField="ID" OnSelectedIndexChanged="OnCategoryDropdown_SelectedIndexChanged" OnDataBound="OnCategoryDropdown_DataBound" />
									</td>
								</tr>
								<tr>
									<td colspan="2" valign="bottom" align="right">
										<div style="float: right; width: auto;" id="divPriceListButton2">
		                                    <content:DynamicButton runat="server" ID="btnClearInvoice" Text="Clear" meta:resourcekey="btnClearInvoice" OnClick="OnClearClick" ButtonType="Back" />
		                                    <content:DynamicButton runat="server" ID="btnAddToInvoice" Text="Add to Invoice" meta:resourcekey="btnAddToInvoice" OnClick="AddToInvoiceClicked" ButtonType="Forward" OnClientClick="OnInvoiceCatalogCancelClick();return true;" />
										</div>
									</td>
								</tr>
								<tr>
									<td colspan="2">
										<div class="clear">
										</div>
										<asp:BulletedList runat="server" ID="blstErrores" Font-Bold="True" ForeColor="Red" />
										<asp:Label ID="lblSuccess" runat="server" CssClass="headerbar_text" meta:resourcekey="lblProdAddSuccessResource" Visible="false" />
									</td>
								</tr>
							</table>
						</td>
					</tr>
					<tr>
						<td>
							<div class="hiddenEnteredQtys">
								<asp:HiddenField ID="hdnSelectedPrds" runat="server" />
							</div>
							<div style="height: 400px; overflow: scroll" class="divCatalogRepeaterClass">
								<table cellspacing="0" cellpadding="0" class="divCatalogRepeaterTable">
									<tbody>
										<tr>
											<th class="col-Avail">
											</th>
											<th class="col-SKU">
												<asp:Literal runat="server" Text="SKU" ID="litSku" meta:resourcekey="litSku" />
											</th>
											<th class="col-Product">
												<asp:Literal runat="server" Text="Product Name" ID="litProductName" meta:resourcekey="litProductName" />
											</th>
											<th class="col-QTY">
												<asp:Literal runat="server" Text="QTY" ID="litQuantity" meta:resourcekey="litQuantity" />
											</th>
											<th class="col-VolumePoint">
												<asp:Literal runat="server" Text="Volume Point" ID="litVolumePoint" meta:resourcekey="litVolumePoint" />
											</th>
											<th class="col-RetailPrice">
												<asp:Literal runat="server" Text="Retail Price" ID="litRetailPrice" meta:resourcekey="litRetailPrice" />
											</th>
										</tr>
										<asp:Repeater runat="server" ID="Subcategories" OnItemDataBound="OnSubcategories_ItemDataBound">
											<ItemTemplate>
												<tr runat="server" id="trBreadcrumb">
													<td id="Td1" class="gdo-pricelist-breadcrumb" colspan='<%# getProductTableColumns() %>' runat="server">
														<asp:HiddenField ID="hdnRootCategoryID" runat="server" />
														<asp:Label ID="lbBreadCrumb" runat="server" Text='<%# getBreadCrumbText(((System.Web.UI.WebControls.RepeaterItem)Container).DataItem as MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.UICategoryProduct ) %>'></asp:Label></font>
													</td>
												</tr>
												<asp:Repeater runat="server" ID="Products" OnItemCreated="OnProducts_OnItemCreated" EnableViewState="true">
													<ItemTemplate>
														<tr runat="server" id="LineItem" class="gdo-row-odd gdo-pricelist-tbl-data">
															<td id="Td2" valign="top" colspan='<%# getProductTableColumns() %>' runat="server">
																<table id="Table1" runat="server" cellspacing="0" cellpadding="0" border="0" class="pricelist-data">
																	<tr id="Tr1" runat="server">
																		<td id="Td4" runat="server" class="col-SKU">
																			<%# Eval("SKU") %>
																			<asp:HiddenField ID="ProductID" runat="server" Value='<%# Eval("SKU") %>' />
																			<asp:HiddenField ID="ParentProductID" runat="server" Value='<%# DataBinder.Eval(((System.Web.UI.WebControls.RepeaterItem)Container.Parent.Parent).DataItem,"Product.ID") %>' />
																			<asp:HiddenField ID="ParentCategoryID" runat="server" Value='<%# DataBinder.Eval(((System.Web.UI.WebControls.RepeaterItem)Container.Parent.Parent).DataItem,"Category.ID") %>' />
																			<asp:HiddenField runat="server" ID="VolumePointsValue" Value='<%# Eval("CatalogItem.VolumePoints", "{0:N2}") %>' />
																		</td>
																		<td id="Td5" runat="server" class="col-Product">
																			<asp:Label ID="lblProductDetail" runat="server" Text='<%# string.Format("{0} {1}", DataBinder.Eval(((System.Web.UI.WebControls.RepeaterItem)Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>'></asp:Label>
																		</td>
																		<td id="Td6" runat="server" align="center" class="col-QTY">
																			<asp:TextBox ID="QuantityBox" runat="server" EnableViewState="true" MaxLength="3" size="5" onkeypress="return setQtyNumeric(event)"></asp:TextBox>
																		</td>
																		<td id="Td8" runat="server" style="text-align: center" class="col-VolumePoint">
																			<asp:Label ID="VolumePoints" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.VolumePoints", "{0:N2}") : "" %>'></asp:Label>
																		</td>
																		<td id="Td7" runat="server" style="text-align: center" class="col-RetailPrice">
																			<asp:Label ID="Label1" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? GetSymbol() : "" %>'></asp:Label>
																			<asp:Label ID="Retail" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.ListPrice", getPriceFormat((decimal)Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
																		</td>
																	</tr>
																</table>
															</td>
														</tr>
													</ItemTemplate>
													<AlternatingItemTemplate>
														<tr runat="server" id="LineItem" class="gdo-row-even gdo-pricelist-tbl-data">
															<td id="Td9" valign="top" colspan='<%# getProductTableColumns() %>' runat="server">
																<table cellspacing="0" cellpadding="0" class="pricelist-data" border="0">
																	<tbody>
																		<tr>
																			<td class="col-SKU">
																				<%#Eval("SKU")%>
																				<asp:HiddenField runat="server" ID="ProductID" Value='<%# Eval("SKU") %>' />
																				<asp:HiddenField runat="server" ID="ParentProductID" Value='<%# DataBinder.Eval(((System.Web.UI.WebControls.RepeaterItem)Container.Parent.Parent).DataItem,"Product.ID") %>' />
																				<asp:HiddenField runat="server" ID="ParentCategoryID" Value='<%# DataBinder.Eval(((System.Web.UI.WebControls.RepeaterItem)Container.Parent.Parent).DataItem,"Category.ID") %>' />
																				<asp:HiddenField runat="server" ID="VolumePointsValue" Value='<%# Eval("CatalogItem.VolumePoints", "{0:N2}") %>' />
																			</td>
																			<td class="col-Product">
																				<asp:Label runat="server" ID="LinkProductDetail" Text='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem)Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>'></asp:Label>
																			</td>
																			<td align="center" class="col-QTY">
																				<asp:TextBox runat="server" ID="QuantityBox" size="5" MaxLength="3" onkeypress="return setQtyNumeric(event)" Enabled='<%# (bool)getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)Eval("ProductAvailability")) %>' BackColor='<%# (bool)getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)Eval("ProductAvailability")) ? System.Drawing.ColorTranslator.FromHtml("white") : System.Drawing.ColorTranslator.FromHtml("#CCCCCC") %>'></asp:TextBox>
																			</td>
																			<td style="text-align: center" class="col-VolumePoint">
																				<asp:Label runat="server" ID="VolumePoints" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.VolumePoints", "{0:N2}") : "" %>'></asp:Label>
																			</td>
																			<td style="text-align: center" class="col-RetailPrice">
																				<asp:Label ID="Label2" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? GetSymbol() : "" %>'></asp:Label>
																				<asp:Label runat="server" ID="Retail" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.ListPrice", getPriceFormat((decimal)Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
																			</td>
																		</tr>
																	</tbody>
																</table>
															</td>
														</tr>
													</AlternatingItemTemplate>
												</asp:Repeater>
											</ItemTemplate>
										</asp:Repeater>
									</tbody>
								</table>
							</div>
						</td>
					</tr>
				</tbody>
			</table>
		</div>
	</ContentTemplate>
</asp:UpdatePanel>
