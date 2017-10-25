<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvoiceTotal.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.InvoiceTotal" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div>
	<table style="width: 100%">
		<tr>
			<td>
				<asp:Label ID="lblErrMsg" runat="server" Style="color: Red; font-weight: normal" />
			</td>
			<td align="right" id="colnTaxErrMsg" runat="server" style="font-weight: normal">
				<table>
					<tr>
						<td valign="top">
							<img width="19" height="19" style="padding: 3px 2px 0 0" src="/content/Global/bizworks/img/icons/icon_caution.gif">
						</td>
						<td align="left">
							<asp:Literal ID="litTaxErrMsg1" runat="server" Text="We were unable to calculate tax based on the address you entered." meta:resourcekey="litTaxErrMsg1" />
							<br />
							<asp:Literal ID="litTaxErrMsg2" runat="server" Text="You may enter tax below, or click Edit above to change the address." meta:resourcekey="litTaxErrMsg2" />
						</td>
					</tr>
				</table>
			</td>
		</tr>
	</table>
</div>
<asp:Literal ID="ltInvoiceStatus" runat="server" Text="Invoice Status:" meta:resourcekey="ltInvoiceStatusResource1" />&nbsp;
<asp:DropDownList ID="ddlInvoiceStatus" runat="server">
</asp:DropDownList>
<br />
<table>
	<tr>
		<td valign="top">
			<asp:Literal ID="ltNote" runat="server" Text="Notes:" meta:resourcekey="ltNote"></asp:Literal><br />
			<asp:TextBox ID="txtNote" runat="server" TextMode="MultiLine"></asp:TextBox><br />
			<br />
			<asp:Literal ID="ltPaymentAddress" runat="server" Text="Payment Address:" meta:resourcekey="ltPaymentAddress"></asp:Literal><br />
			<asp:TextBox ID="txtPaymentAddress" runat="server" TextMode="MultiLine"></asp:TextBox>
		</td>
		<td valign="top" align="right">
			<table>
				<tr>
					<td align="right">
						<img id="imgErrTax" runat="server" src="/Content/Global/bizworks/img/icons/icon-error.gif" alt="" visible="false" />
						<asp:Literal ID="ltTax" runat="server" Text="Tax:" meta:resourcekey="ltTax"></asp:Literal>
					</td>
					<td>
						<asp:Literal ID="ltTaxCurrency" runat="server" Text="$" meta:resourcekey="ltTaxCurrency"></asp:Literal>
						<asp:TextBox ID="txtTaxAmount" runat="server"></asp:TextBox>
						<asp:Literal ID="ltTaxOr" runat="server" Text="or" meta:resourcekey="ltTaxOr"></asp:Literal>
						<asp:TextBox ID="txtTaxPercentage" runat="server"></asp:TextBox>
						<asp:Literal ID="ltTaxPercentage" runat="server" Text="%" meta:resourcekey="ltTaxPercentage"></asp:Literal>
					</td>
					<td>
		                <content:DynamicButton runat="server" ID="btnApplyTax" meta:resourcekey="btnApplyTax" Text="apply" ButtonType="Neutral" />
					</td>
					<td>
						<span id="tax_amount">$<asp:Literal ID="ltTaxAmount" runat="server"></asp:Literal>
						</span>
					</td>
				</tr>
				<tr>
					<td align="right">
						<img id="imgErrShip" runat="server" src="/Content/Global/bizworks/img/icons/icon-error.gif" alt="" visible="false" />
						<asp:Literal ID="ltShip" runat="server" Text="Shipping & Handling:" meta:resourcekey="ltShip"></asp:Literal>
					</td>
					<td>
						<asp:Literal ID="ltShipCurrency" runat="server" Text="$" meta:resourcekey="ltShipCurrency"></asp:Literal>
						<asp:TextBox ID="txtShipAmount" runat="server"></asp:TextBox>
						<asp:Literal ID="ltShipOr" runat="server" Text="or" meta:resourcekey="ltShipOr"></asp:Literal>
						<asp:TextBox ID="txtShipPercentage" runat="server"></asp:TextBox>
						<asp:Literal ID="ltShipPercentage" runat="server" Text="%" meta:resourcekey="ltShipPercentage"></asp:Literal>
					</td>
					<td>
		                <content:DynamicButton runat="server" ID="btnApplyShip" meta:resourcekey="btnApplyShip" Text="apply" ButtonType="Neutral" />
					</td>
					<td>
						<span id="ship_amount">$<asp:Literal ID="ltShipAmount" runat="server"></asp:Literal>
						</span>
					</td>
				</tr>
			</table>
			<br />
			<div id="createinv_step2_btns">
		        <content:DynamicButton runat="server" ID="btnClear" meta:resourcekey="btnClear" Text="Clear" ButtonType="Back" />
		        <content:DynamicButton runat="server" ID="btnCalculate"  meta:resourcekey="btnCalculate" Text="Continue" ButtonType="Neutral" />
			</div>
			<br />
			<br />
			<span id="Total_due">
				<asp:Literal ID="ltTotalDue" runat="server" Text="Total Due:" meta:resourcekey="ltTotalDue"></asp:Literal>&nbsp;$<asp:Literal ID="ltTotalDueAmount" runat="server"></asp:Literal>
			</span>
		</td>
	</tr>
</table>
