<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="FreightSimulation.aspx.cs" 
    MasterPageFile="~/MasterPages/Ordering.master" Inherits="MyHerbalife3.Ordering.Web.Ordering.FreightSimulation" 
    EnableEventValidation="false" meta:resourcekey="PageResource1" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content ID="content2" runat="server" ContentPlaceHolderID="HeaderContent">
     <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:PlaceHolder ID="pnlFreightEstimation" runat="server" />
</asp:Content>