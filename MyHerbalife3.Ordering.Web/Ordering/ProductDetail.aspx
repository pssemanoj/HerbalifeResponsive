<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master"
    AutoEventWireup="true" CodeBehind="ProductDetail.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.ProductDetail" meta:resourcekey="PageResource1"  EnableEventValidation="false" %><asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">

    <div runat="server" id="DivProductDetail">
    </div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            toggleCatalog();
        });
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ProductRecomendationsContent" runat="server">
    <script type="text/javascript">
        if (Salesforce) {
            _etmc.push(Salesforce);
        }
    </script>
</asp:Content>