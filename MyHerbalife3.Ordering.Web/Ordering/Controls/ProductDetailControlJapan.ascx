<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductDetailControlJapan.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductDetailControlJapan" %>
<%@ Register Src="~/Ordering/Controls/ProductList.ascx" TagName="ProductList" TagPrefix="hrblProductList" %>
<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="ProductAvailability"
    TagPrefix="hrblProductAvailability" %>
<%@ Register Src="~/Ordering/Controls/ProductInfoFooter.ascx" TagName="ProductInfoFooter"
    TagPrefix="hrblProductInfoFooter" %>
<%@ Register Src="~/Ordering/Controls/ProductLink.ascx" TagName="ProductLink" TagPrefix="hrblProductLink" %>
<%@ Register Src="~/Ordering/Controls/ProductImage.ascx" TagName="ProductImage" TagPrefix="hrblProdImg" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:UpdatePanel ID="upProductDetail" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div ID="divProductDetail" runat="server" class="product-detail-jp" >
            <div>
                <div class="align-right">
                    <cc1:OvalButton ID="CancelButton" runat="server" Coloring="Silver" Text="Close" OnClick="OnCancel"
                        ArrowDirection="" IconPosition="" meta:resourcekey="CancelButtonResource1" />
                </div>
                <div>
                    <h4 id="uxProductName" runat="server" class="blue">
                        <input type="hidden" runat="server" id="hdCategoryID" value="0" />
                        <input id="ProdID" runat="server" type="hidden" value="0" />
                    </h4>
                </div>
                <div>
                    <div class="product-overview">
                        <div class="" style="width: 100%">
                            <p>
                                <asp:Label ID="lbOverview" runat="server" CssClass="blue" Text="Overview" meta:resourcekey="lbOverviewResource1"></asp:Label>
                                <p id="uxOverview" runat="server" />
                                <asp:Label ID="lbKeyBenefits" runat="server" CssClass="blue" Text="Key Benefits"
                                    meta:resourcekey="lbKeyBenefitsResource1" />
                                <p id="pBenefits" runat="server" />
                            </p>
                        </div>
                    </div>
                    <div class="product-image">
                        <hrblProdImg:ProductImage ID="ProdImage" runat="server" Title="Product" />
                    </div>
                </div>
                <div>
                    <hrblProductLink:ProductLink ID="ProductLinks" runat="server" />
                </div>
                <div class="myclear">
                    <hrblProductInfoFooter:ProductInfoFooter ID="ProductInfoFooter1" runat="server" />
                    <asp:Label runat="server" ID="VolumePointText" Text="Volume For Selected Items:"
                        meta:resourcekey="VolumePointTextResource1"></asp:Label>
                    <asp:Label runat="server" ID="lbVolumePoint" meta:resourcekey="lbVolumePointResource1"></asp:Label>
                    <div class="productDetailsButtons">
                        <cc2:DynamicButton ID="recalcVPSubmit" runat="server" Text="Recalculate Volume" OnClick="OnRecalculate"
                            ButtonType="Neutral" IconType="Plus" meta:resourcekey="recalcVPSubmitResource1" />
                        <cc2:DynamicButton ID="DynamicButton1" runat="server" ButtonType="Forward" Text="Add to Cart" meta:resourcekey="CheckoutButtonResource1"
                            Disabled="true" Hidden="true" />
                        <cc2:DynamicButton ID="DynamicButton2" runat="server" ButtonType="Forward" OnClick="AddToCart" meta:resourcekey="CheckoutButtonResource1"
                            Text="Add to Cart" OnClientClick="$('#ctl00_ctl00_ContentArea_ctl06_CntrlProductDetail_ctl01_CheckoutButton').css('display', 'none');$('#ctl00_ctl00_ContentArea_ctl06_CntrlProductDetail_ctl01_CheckoutButton1').css('display', '-block');" />
                    </div>
                </div>
                <div>
                    <div class="table-container tabs-SKU">
                        <asp:BulletedList ID="uxErrores" runat="server" BulletStyle="Circle" Font-Bold="True"
                            ForeColor="Red" meta:resourcekey="uxErroresResource1">
                        </asp:BulletedList>
                        <asp:Repeater ID="Products" runat="server" OnItemCreated="Products_OnItemCreated">
                            <HeaderTemplate>
                                <table cellpadding="1" cellspacing="2" style="width: 100%; ">
                                    <tr>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbSKU" runat="server" Text="SKU" meta:resourcekey="lbSKUResource1"></asp:Label>
                                        </th>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbFlavorType" runat="server" Text="Product" meta:resourcekey="lbFlavorTypeResource1"></asp:Label>
                                        </th>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbDoc" runat="server" Text="Details" meta:resourcekey="lbDocResource1" />
                                        </th>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbVolumePoint" runat="server" Text="Volume Point" meta:resourcekey="lbVolumePointResource1" />
                                        </th>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbRetailPrice" runat="server" Text="Retail Price" meta:resourcekey="lbRetailPriceResource1" />
                                        </th>
                                        <th style="text-align: center;">
                                            <asp:Label ID="lbDiscountedPrice" runat="server" Text="Discounted Price" meta:resourcekey="lbDiscountedPriceResource1" />
                                        </th>
                                        <th class="NoBorder" style="text-align: center;">
                                            <asp:Label ID="lbQuantity" runat="server" Text="Quantity" meta:resourcekey="lbQuantityResource1" />
                                        </th>
                                    </tr>
                            </HeaderTemplate>
                            <ItemTemplate>
                                    <tr>
                                        <td class="td_pricelist_item" style="width: 60px">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <hrblProductAvailability:ProductAvailability ID="ProductAvailibility" runat="server"
                                                            Available='<%# Eval("ProductAvailability") %>' ShowLabel="false" />
                                                    </td>
                                                    <td>
                                                        <%#Eval("CatalogItem.SKU")%>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:HiddenField ID="ProductoID" runat="server" Value='<%# Eval("SKU") %>' />
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td style="text-align: center; width: 220px">
                                            <asp:Label ID="FlavorType" runat="server" Text='<%# Eval("Description") %>' meta:resourcekey="FlavorTypeResource2"></asp:Label>
                                        </td>
                                        <td class="td_pricelist_item" style="text-align: center; width: 60px;">
                                            <div id="pdfLabel">
                                                <a id="pdfLink" runat="server" href='<%# Eval("PdfName") %>' target="_blank">
                                                    <img id="Img1" runat="server" alt="PDF Icon" src="/Content/Global/img/icon_pdf.gif"
                                                        style="margin: 0px 5px -3px 0px; width: 16px; height: 16px" visible='<%# string.IsNullOrEmpty(Eval("PdfName") as string)==false %>'></img>
                                                </a>
                                            </div>
                                        </td>
                                        <td class="td_pricelist_item" style="text-align: center" width="66px">
                                            <asp:Label ID="VolumePoints" runat="server" Text='<%# Eval("CatalogItem.VolumePoints", "{0:N2}") %>'
                                                meta:resourcekey="VolumePointsResource1"></asp:Label>
                                        </td>
                                        <td class="td_pricelist_item" style="width: 76px; text-align: center">
                                            <asp:Label ID="Retail" runat="server" Text='<%# Eval("CatalogItem.ListPrice", getAmountString((decimal)Eval("CatalogItem.ListPrice")))  %>'
                                                meta:resourcekey="RetailResource1"></asp:Label>
                                        </td>
                                        <td class="td_pricelist_item" style="text-align: center">
                                            <asp:Label ID="DiscountedPrice" runat="server" Text='Purchasable'
                                                meta:resourcekey="DiscountedPriceResource2"></asp:Label>
                                        </td>
                                        <td width="74px" align="center">
                                            <asp:TextBox ID="Quantity" runat="server" BackColor='<%# (bool)getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)Eval("ProductAvailability")) ? System.Drawing.ColorTranslator.FromHtml("white") : System.Drawing.ColorTranslator.FromHtml("#CCCCCC") %>'
                                                Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)Eval("ProductAvailability")) %>'
                                                MaxLength="3" size="5" CssClass="onlyNumbers" style="width: 45px;"></asp:TextBox>
                                            <asp:HiddenField ID="ProductID" runat="server" Value='<%# Eval("SKU") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            <FooterTemplate>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
                <div>
                    <div>
                        <div class="myclear">
                            <div class="productDetailsButtons">
                                <cc2:DynamicButton ID="DynamicButton3" runat="server" Text="Recalculate Volume" OnClick="OnRecalculate"
                                                    ButtonType="Neutral" IconType="Plus" meta:resourcekey="recalcVPSubmitResource1" />
                                <cc2:DynamicButton ID="CheckoutButton1" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                                    Text="Add to Cart" Disabled="true" Hidden="true" />
                                <cc2:DynamicButton ID="CheckoutButton" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                                    OnClick="AddToCart" Text="Add to Cart" OnClientClick="$('#ctl00_ctl00_ContentArea_ctl06_CntrlProductDetail_ctl01_CheckoutButton').css('display', 'none');$('#ctl00_ctl00_ContentArea_ctl06_CntrlProductDetail_ctl01_CheckoutButton1').css('display', 'inline-block');" />
                            </div>
                        </div>

                        <div class="" style="width: 100%">
                            <p>
                                <asp:Label ID="lbUsage" runat="server" class="blue" Text="Usage" meta:resourcekey="lbUsageResource1" />
                            </p>
                            <p id="pUsage" runat="server">
                            </p>
                        </div>
                    </div>
                </div>
                <div>
                    <div>
                        <div class="" style="width: 100%">
                            <p>
                                <asp:Label ID="lbFastFacts" runat="server" class="blue" meta:resourcekey="lbFastFactsResource1"
                                    Text="Fast Facts" />
                            </p>
                            <p id="pQuickFacts" runat="server">
                            </p>
                        </div>
                    </div>
                </div>
                <div>
                    <div>
                        <div id="divAvail" style="margin: 5px;">
                            <hrblProductAvailability:ProductAvailability ID="ProductAvailability" runat="server" />
                        </div>
                    </div>
                </div>
                <div>
                    <div>
                        <div>
                            <hrblProductList:ProductList ID="ProductsPanel" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
