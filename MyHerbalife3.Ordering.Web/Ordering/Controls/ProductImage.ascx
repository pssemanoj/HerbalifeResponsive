<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductImage.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductImage" %>
<%@ Register TagPrefix="gv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>


<div id="zoomIntoProduct" class="hrblModal hide">
    <h4></h4>
    <img runat="server" id="ProdImageLarge" alt="image" src="" />
</div>

<div runat="server" id="imageSection">
<asp:HiddenField runat="server" id="HdFromPopup" />
    <img alt="prodImage" id="ProdImage" runat="server" src="" class="gdo-panes-product-img"
        width="150" height="150" rel="#photo1" />
    <div>
        <asp:HyperLink meta:resourcekey="lbClick" runat="server" title="zoomIntoProduct" CssClass="modalBtn" ID="lbClickToEnlarge" Text="Click to enlarge image."></asp:HyperLink>
    </div>
    <div runat="server" id="divPrint">
            <asp:HyperLink runat="server" title="printThisPage" CssClass="print modalBtn icon icon-printer printer-ln-1" ID="printThisPageLink"><asp:Label runat="server" Text="Print This Page" meta:resourcekey="lnkPrintThisPage"></asp:Label></asp:HyperLink>
            <div align="center" runat="server" id="divPrintThisPage">
            </div>
    </div>
    <asp:Panel ID="divSizeChartLnk" runat="server" ClientIDMode="Static">
        <asp:HyperLink runat="server" title="showSizeChart" ID="showSizeChartLink" ClientIDMode="Static" CssClass="showSizeChartLink">
            <asp:Label ID="lblShowSizeChartLink" runat="server" Text="Size Chart" meta:resourcekey="lblShowSizeChartLink"></asp:Label>
            <i class="arrow icon icon-arrow-down-3"></i>
        </asp:HyperLink>
    </asp:Panel>
    <gv:PrintPageModalPopupExtender ID="popup_PrintThisPage" runat="server" TargetControlID="lnkHidden"
        PopupControlID="divPrintThisPage" RepositionMode="RepositionOnWindowResize" CancelControlID="lnkHidden"
        BackgroundCssClass="modalBackground" Enabled="True" Y="30">
    </gv:PrintPageModalPopupExtender>
    <asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>
</div>
<div runat="server" id="imageSectionFromPopup">
    <div runat="server" id="divImageEnlarge" align="center">
        <img alt="prodImage" id="ProdImgPopup" runat="server" src="" class="gdo-panes-product-img"
            width="150" height="150" />
        <div>
            <asp:HyperLink meta:resourcekey="lbClick" runat="server" ID="LinkEnlargeImage" Text="Click to enlarge image."></asp:HyperLink>
        </div>
    </div>
</div>

<div id="printThisPage" class="hrblModal hide">
    <h4></h4>
    <div id="printThisPageContainer" runat="server"></div>
</div>

<script type="text/javascript">
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        $(".hrblModal:not(.k-window-content.k-content)").remove();
        Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(EndRequestHandler);
    }
</script>