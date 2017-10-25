<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="Faq.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Faq" EnableEventValidation="false" meta:resourcekey="PageResource1" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content ID="content2" runat="server" ContentPlaceHolderID="HeaderContent">
        <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:PlaceHolder ID="pnlFaq" runat="server" />
</asp:Content>
