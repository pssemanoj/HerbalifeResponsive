<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PrintThisPageContent.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PrintThisPageContent" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div align="center">
    <div class="gdo-pf-product-title">
        <asp:Label runat="server" ID="lbProductName" meta:resourcekey="lbProductNameResource1"></asp:Label>
    </div>
    <div align="center">
        <img src="" width="150" id="imgProduct" runat="server" height="150" alt="prodImage" />
        <div>
            <cc1:DynamicButton ID="btnPrintThisPage" runat="server" ButtonType="Neutral" Text="Print This Page" meta:resourcekey="PrintThisPage" CssClass="btnPrintThisPage"/>
        </div>
    </div>

    <div class="gdo-spacer1">
    </div>
</div>
<div class="css-panes-subheader">
    <asp:Label ID="lbProductOverview" runat="server" Text="Overview" meta:resourcekey="lbProductOverview"></asp:Label>
</div>
<div class="gdo-spacer2">
</div>
<div class="css-panes-subtext">
    <asp:Label ID="lbOverview" runat="server" Text="Overview" meta:resourcekey="lbOverviewResource1"></asp:Label>
</div>
<div class="gdo-spacer2">
</div>
<span style="vertical-align: middle">
    <table class="specialtable" cellspacing="0" border="0" width="100%" runat="server"
        id="tabIcons">
        <tr>
            <td align="right">
                <div runat="server" id="divIcons" class="DivProdIcons">
                </div>
            </td>
        </tr>
    </table>
</span>
<div style="">
    <div class="gdo-spacer1">
    </div>
    <div class="css-panes-subheader">
        <asp:Label ID="lbDetails" runat="server" Text="Details" meta:resourcekey="lbDetails" />
    </div>
    <div class="gdo-spacer2">
    </div>
    <div class="css-panes-subtext">
        <p runat="server" id="pDetails">
        </p>
    </div>
    <div class="gdo-spacer1">
    </div>
    <div class="css-panes-subheader">
        <asp:Label ID="lbKeyBenefits" runat="server" Text="Key Benefits" meta:resourcekey="lbKeyBenefits" />
    </div>
    <div class="gdo-spacer2">
    </div>
    <div class="css-panes-subtext">
        <p runat="server" id="pBenefits">
        </p>
    </div>
    <div class="gdo-spacer1">
    </div>
    <div class="css-panes-subheader">
        <asp:Label ID="lbUsage" runat="server" Text="Usage" meta:resourcekey="lbUsage" />
    </div>
    <div class="gdo-spacer2">
    </div>
    <div class="css-panes-subtext">
        <p runat="server" id="pUsage">
        </p>
    </div>
    <div class="gdo-spacer1">
    </div>
    <div class="css-panes-subheader">
        <asp:Label ID="lbFastFacts" runat="server" Text="Fast Facts" meta:resourcekey="lbFastFacts" />
    </div>
    <div class="gdo-spacer2">
    </div>
    <div class="css-panes-subtext">
        <p runat="server" id="pQuickFacts">
        </p>
    </div>
    <br />
    <div style="">
        <asp:Repeater runat="server" ID="ProductSKU" OnItemCreated="Products_OnItemCreated">
            <HeaderTemplate>
                <table cellspacing="1" cellpadding="2" border="1">
                    <tr>
                        <th class="gdo-product-details-header-print" runat="server">
                            <asp:Label ID="lbSKU" runat="server" meta:resourceKey="lbSKUResource1" Text="SKU"></asp:Label>
                        </th>
                        <th class="gdo-product-details-header-print" runat="server">
                            <asp:Label ID="lbFlavorType" runat="server" meta:resourceKey="lbFlavorTypeResource1"
                                Text="Flavor/Type"></asp:Label>
                        </th>
                        <th class="gdo-product-details-header-print" runat="server" id="thDoc">
                            <asp:Label ID="lbDoc" runat="server" meta:resourceKey="lbDocResource1" Text="Details"></asp:Label>
                        </th>
                        <th class="gdo-product-details-header-print" id="thVolumePoint" runat="server">
                            <asp:Label ID="lbVolumePoint" runat="server" meta:resourceKey="lbVolumePointResource1"
                                Text="Volume Point"></asp:Label>
                        </th>
                        <th id="thEarnBase" runat="server" class="gdo-product-details-header-print">
                            <asp:Label ID="lbEarnBase" runat="server" meta:resourceKey="lbEarnBaseResource1"
                                Text="Earn Base"></asp:Label>
                        </th>
                        <th class="gdo-product-details-header-print" id="idRetailPrice" runat="server">
                            <asp:Label ID="lbRetailPrice" runat="server" meta:resourceKey="lbRetailPriceResource1"
                                Text="Retail Price"></asp:Label>
                        </th>
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="gdo-td-sku-print" align="center">
                        <table>
                            <tr>
                                <td>
                                    <asp:Image class="gdo-availability-image-print" ImageAlign="AbsMiddle" ID="prodAvailImage" runat="server" ImageUrl='<%# getImage((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)Eval("ProductAvailability")) %>' />
                                </td>
                                <td class="gdo-td-sku-number-print">
                                    <%# Eval("SKU") %>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td class="gdo-td-pricelist-item-print td_pricelist_item">
                        <asp:Label ID="FlavorType" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                    </td>
                    <td class="gdo-td-details-item-print" id="tdDoc" runat="server">
                        <div>
                            <a id="pdfLink" runat="server" href='<%# Eval("PdfName") %>' target="_blank">
                                <img runat="server" alt="PDF Icon" src="/Content/Global/img/icon_pdf.gif"
                                    visible='<%# string.IsNullOrEmpty(Eval("PdfName") as string)==false %>' />
                                &nbsp;&nbsp;</a>
                        </div>
                    </td>
                    <td class="gdo-td-vp-print" id="tdVolumePoint" runat="server">
                        <asp:Label ID="VolumePoints" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.VolumePoints", GetVolumeString((decimal)Eval("CatalogItem.VolumePoints"))) : "" %>'></asp:Label>
                    </td>
                    <td id="tdEarnBase" runat="server" class="gdo-td-earnbase-print">
                        <asp:Label ID="EarnBase" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.EarnBase", getAmountString((decimal)Eval("CatalogItem.EarnBase"))) : "" %>'></asp:Label>
                    </td>
                    <td class="gdo-td-rp-print" runat="server" id="tdRetailPrice">
                        <asp:Label ID="Retail" runat="server" Text='<%# (bool)Eval("IsPurchasable") == true ? Eval("CatalogItem.ListPrice", getAmountString((decimal)Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>
    <br />
    <div align="left">
        <div style="margin-left: 0px" class="disclaimer" runat="server" id="divDisclaimer">
            <asp:Repeater runat="server" ID="Disclaimer" OnItemDataBound="DisclaimerDataBound">
                <ItemTemplate>
                    <p runat="server" id="pDisclaimer">
                    </p>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <div class="center">
        <cc1:DynamicButton ID="btnPrintThisPageBottom" runat="server" ButtonType="Neutral" Text="Print This Page" meta:resourcekey="PrintThisPage" CssClass="btnPrintThisPage"/>
    </div>
</div>
