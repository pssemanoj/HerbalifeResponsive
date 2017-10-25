<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateInvoice.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.CreateInvoice" 
    MasterPageFile="~/MasterPages/Ordering.Master" ValidateRequest="false" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/InvoiceInfo.ascx" TagName="InvoiceInfo" TagPrefix="uc1" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/InvoiceTotal.ascx" TagName="InvoiceTotal" TagPrefix="uc2" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Content ID="ContentHeader" runat="server" ContentPlaceHolderID="HeaderContent">
	<!-- begin - shan - mar 06, 2012 - js functions to show/hide confirm popup -->
	<script type="text/javascript">
		//to show confirm cancel popup
		function showConfirmCancelPopup() {
			var popup = $find("<%=radConfirmCancelPopup.ClientID%>");
			popup.show();
			$get("pagetop").focus();
			window.location.hash = "pagetop";
			return false;
		}

		//to hide confirm cancel popup
		function hideConfirmCancelPopup() {
			var popup = $find("<%=radConfirmCancelPopup.ClientID%>");
			popup.hide();
			return false;
		}
	</script>
	<!-- end -->
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
	<a name="pagetop" id="pagetop">&nbsp;</a>
	<div>
		<div class="title-div">
			<h1>
				<asp:Literal ID="ltPageHeading" runat="server" Text="Create Invoice" meta:resourcekey="ltPageHeading" /></h1>
		</div>
		<p>
			<asp:Literal ID="ltPageDesc" runat="server" Text="Complete the fields below, then click the Continue button to view your invoice" meta:resourcekey="ltPageDesc" />
		</p>
	</div>
	<table width="100%" class="tblCreateInvoice">
		<tr>
			<td>
				<span class="steps_labels">
					<asp:Literal ID="litInvoiceInfo" runat="server" Text="Invoice Information" meta:resourcekey="litInvoiceInfo" />
				</span>
			</td>
			<td align="right">
				<span class="steps_labels">
					<asp:Label ID="lblPanelIndex1" runat="server" Text="Step 1 of 2" meta:resourcekey="lblPanelIndex1" />
				</span>
				<asp:LinkButton ID="lnkStep1Edit" runat="server" Text="Edit" meta:resourcekey="lnkStep1Edit" OnClick="lnkStep1Edit_click" Visible="false" />
			</td>
		</tr>
	</table>
	<asp:Panel ID="pnlStep1" runat="server">
		<uc1:InvoiceInfo ID="InvoiceInfo1" runat="server" />
	</asp:Panel>
	<table width="100%" class="tblCreateInvoiceShipping" cellspacing="0" cellpadding="0">
		<tr>
			<td>
				<span class="steps_labels">
					<asp:Literal ID="litTaxInfo" runat="server" Text="Tax, Shipping and Total" meta:resourcekey="litTaxInfo" />
				</span>
			</td>
			<td align="right">
				<span class="steps_labels">
					<asp:Literal ID="litStep2of2" runat="server" Text="Step 2 of 2" meta:resourcekey="litStep2of2" />
				</span>
			</td>
		</tr>
	</table>
	<asp:Panel ID="pnlStep2" runat="server">
		<uc2:InvoiceTotal ID="InvoiceTotal1" runat="server" />
	</asp:Panel>
	
    <div id="divtblCreateInvoiceButton">
		<content:DynamicButton runat="server" ID="btnCancel" meta:resourcekey="btnCancel" Text="Cancel" ButtonType="Back" OnClientClick="return showConfirmCancelPopup();" />
		<content:DynamicButton runat="server" ID="btnSave" meta:resourcekey="btnSave" Text="Save & View Invoice" ButtonType="Forward" />
		<p>
			<asp:Literal ID="litSaveInstr" runat="server" Text="Automatically Recalculates Total" meta:resourcekey="litSaveInstr" />
        </p>
	</div>

	<telerik:RadToolTip ID="radConfirmCancelPopup" runat="server" Modal="True" HideEvent="ManualClose" Position="Center" RelativeTo="BrowserWindow" EnableEmbeddedSkins="False" ManualClose="true">
		<asp:Panel ID="panelConfirmCancel" runat="server" CssClass="popupwrapper">
			<table class="popup-layout-table" style="width: auto !important">
				<tr>
					<td>
						<div class="popup-content">
							<div class="popwrapper">
								<asp:Literal ID="litConfirmCancelMsg" runat="server" Text="Clicking cancel will delete all the invoice information. Are you sure you want to cancel?" meta:resourcekey="litConfirmCancelMsg" />
							</div>
						</div>
					</td>
				</tr>
				<tr>
					<td>
						<div class="popup-buttons">
							<div class="buttongap">
		                        <content:DynamicButton runat="server" ID="btnConfirmYes" Text="Yes" meta:resourcekey="btnConfirmYes" OnClick="btnConfirmYes_Click" ButtonType="Forward" />
							</div>
							<div class="buttongap">
		                        <content:DynamicButton runat="server" ID="btnConfirmNo" Text="No" meta:resourcekey="btnConfirmNo" ButtonType="Back" OnClientClick="return hideConfirmCancelPopup();" />
							</div>
						</div>
					</td>
				</tr>
			</table>
		</asp:Panel>
	</telerik:RadToolTip>
</asp:Content>
