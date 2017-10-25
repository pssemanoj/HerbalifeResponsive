<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyInvoices.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.MyInvoices" 
    MasterPageFile="~/MasterPages/Ordering.Master" ValidateRequest="false"  %>

<%@ Register Src="~/Bizworks/Controls/Invoices/InvoiceListView.ascx" TagName="InvoiceListView" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
	<uc1:InvoiceListView ID="InvoiceListView1" runat="server" />
</asp:Content>
