﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="PriceListFavourite.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.PriceListFavourite"
    meta:resourcekey="PageResource1" EnableEventValidation="false" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.Ordering" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="MyAvail" TagPrefix="hrblAvail" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register Src="~/Ordering/Controls/MessageBoxPC.ascx" TagName="PCMsgBox" TagPrefix="PCMsgBox" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register TagPrefix="CnChkout24h" TagName="CnChkout24h" Src="~/Ordering/Controls/CnChkout24h.ascx" %>
<%@ Register Src="~/Ordering/Controls/Promotion_MY.ascx" TagPrefix="PromoMY" TagName="Promotion_MY" %>



<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript" src='<%= Page.ResolveUrl("~/Ordering/Scripts/ProductFavourite.js") %>' ></script>
    <script type="text/javascript">
        $(document).ready(function () {
            
            $("input[id*='txt_']").each(function () {
                $(this).on('blur', function () { show(this); });
            });

            // === Recalculate VP & Total Ammount on click event
            if (responsiveMode != true) {
                $("input[id*='txt_']").live('keyup', function () {
                    show(this);
                    VPCalculation.get_skus();

                    if ($('[id$="lbVolumePoint"]').length > 0) {
                    $('[id$="lbVolumePoint"]').text(VPCalculation.get_total_vp());
                    }
                    if ($('[id$="lbTotalAmount"]').length > 0) {
                    $('[id$="lbTotalAmount"]').text(VPCalculation.get_total_ammount());
                    }
                    
                });
            }

        });
        
        function PrdDetailPopUp(id1, id2) {
            document.getElementById('<%=hiddnProductIDCatagoriID.ClientID %>').value = id1+"|"+id2;
            document.getElementById('<%=DummyLinkProductDetail.ClientID %>').click();
            return false;
        }

        function DisableAddtoCart() {
            $('#<%= AddToCart.ClientID %>').css('display', 'none');
            $('#<%= CheckoutButton.ClientID %>').css('display', 'none');

            $('#<%= AddToCartDisabled.ClientID %>').css('display', 'inline-block');
            $('#<%= CheckoutButtonDisabled.ClientID %>').css('display', 'inline-block');

            // For Add to cart buttons from product details popup.
            $('.productDetailsButtons a:nth-child(3)').css('display', 'none');
            $('.productDetailsButtons a:nth-child(2)').css('display', 'inline-block');

            $('.add-item').css('display', 'none');
            $('.add-item-disabled').css('display', 'inline-block');

            // === Fix for re-render the Product List table 76315 ===
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(update_table);

        }

        // === Fix for re-render the Product List table 76315 ===
        function update_table() {
            if ($.browser.msie && $.browser.version == '9.0') {
                tableHtml = $('div[id$="upPriceList"] .gdo-pricelist-tbl').html();
                var expr = new RegExp('>[ \t\r\n\v\f]*<', 'g');
                tableHtml = tableHtml.replace(expr, '><');
                $('div[id$="upPriceList"] .gdo-pricelist-tbl').html(tableHtml);
            }

            <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) { %>
                $('a.cart-items div').text($('[id$="TotalQty"]').text());
            <% } %>
            
            
            <% if ( HLConfigManager.Configurations.DOConfiguration.IsChina ) {%>
                freeze_header_cols();
            <% } %>

            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(update_table);
        }

        function onRecalculate() {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(endRequest);
        }
        function endRequest() {
            <% if ( HLConfigManager.Configurations.DOConfiguration.IsChina ) {%>
                freeze_header_cols();
            <% } %>

            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(endRequest);
        }
      
        function show(sku1) {
            var txtIDwithVal = document.getElementById('<%= pricelistGridInfo.ClientID %>').value;
            txtIDwithVal = txtIDwithVal + "," + sku1.id.toString() + "|" + sku1.value;
            document.getElementById('<%= pricelistGridInfo.ClientID %>').value = txtIDwithVal;
        }

    </script>

</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <script type="text/javascript" src='<%= Page.ResolveUrl("~/Ordering/Scripts/ProductFavourite.js") %>' ></script>
    <progress:UpdatePanelProgressIndicator ID="progressPriceList" runat="server" TargetControlID="upPriceList" />
    <asp:UpdatePanel ID="upPriceList" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div>
                
                <div id="divChinaPCMessageBox" style="margin: 5px;" runat="server">
                    <PCMsgBox:PCMsgBox runat="server" ID="PcMsgBox1" DisplaySubmitButton="True" ></PCMsgBox:PCMsgBox>
                </div>
                <div class="gdo-spacer3">
                </div>
                <div  class="alertNoCC">
                    <asp:Label runat="server" ID="lblCraditcard" meta:resourcekey="lblCraditcardResource1"></asp:Label>
                    <asp:Image ID="imgWarning" runat="server" Visible="False" meta:resourcekey="imgWarningResource1" />
                    <asp:HyperLink runat="server" ID="lnkSavedCards" NavigateUrl="OrderPreferences.aspx" meta:resourcekey="lnkSavedCardsResource1"></asp:HyperLink>
                </div>
                <table width="100%" cellspacing="1" cellpadding="0" border="0" class="gdo-pricelist-tbl">
                    <tbody>
                        <tr>
                            <td>
                                <table class="gdo-pricelist-tbl-top" border="0" cellspacing="0" cellpadding="0">
                                    <tr>
                                        <td>
                                            <div id="divChinaAvail" style="margin: 5px;">
                                                <hrblavail:MyAvail id="ChinaProductAvailability" runat="server" />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <span class="gdo-pricelist-label">
                                                <asp:Label runat="server" Visible ="False" ID="lbSelectCategory" Text="Select Category:" meta:resourcekey="TxtSelectCategory"></asp:Label></span>
                                            <asp:DropDownList Visible="False" AutoPostBack="True" runat="server" ID="CategoryDropdown" DataTextField="DisplayName"
                                                DataValueField="ID" OnSelectedIndexChanged="OnCategoryDropdown_SelectedIndexChanged"
                                                OnDataBound="OnCategoryDropdown_DataBound" meta:resourcekey="CategoryDropdownResource1">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr class="freeze-vp">
                                        <td colspan="2">
                                            <div class="gdo-body-text">
                                                <span class="gdo-selected-item-volume">
                                                    <asp:Label runat="server" ID="VolumePointText" Text="Volume For Selected Items:"
                                                        meta:resourcekey="VolumePointTextResource1"></asp:Label>
                                                    <asp:Label runat="server" ID="lbVolumePoint" meta:resourcekey="lbVolumePointResource1"></asp:Label>
                                                    <br/>
                                                    <asp:Label runat="server" ID="TotalAmountText" Text="Amount For Selected Items:"
                                                        meta:resourcekey="TotalAmountTextResource1"></asp:Label>
                                                    <asp:Label runat="server" ID="lbTotalAmount" meta:resourcekey="TotalAmountResource1"></asp:Label>
                                                </span>
                                            </div>
                                        </td>
                                    </tr>
                                   
                                     <tr>
                                          <td colspan="2" valign="bottom" align="right">
                                               <asp:HyperLink ID="CnpTable" runat="server" Text="PDF Link"  Target="_blank" meta:resourcekey="pdflink"></asp:HyperLink>
                                              </td>
                                      </tr>
                                    
                                    <tr class="freeze-actions">
                                        <td colspan="2" valign="bottom" align="right">
                                            <div id="divPriceListButton2">
                                                <cc1:DynamicButton ID="recalcVPSubmit" runat="server" Text="Recalculate Volume" OnClick="OnRecalculate"
                                                    ButtonType="Neutral" IconType="Plus" meta:resourcekey="recalcVPSubmitResource1" name="recalcVPSubmit_1" OnClientClick="onRecalculate();" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel=""/>
                                                <cc1:DynamicButton ID="CheckoutButton" runat="server" ButtonType="Forward" Text="Add to Cart"
                                                    OnClick="OnAddToCart" OnClientClick="DisableAddtoCart();" meta:resourcekey="AddToCartResource1" name="addCart_1" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel=""/>
                                                <cc1:DynamicButton ID="CheckoutButtonDisabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                                                    Disabled="True" Hidden="True" meta:resourcekey="AddToCartResource1" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <div class="clear">
                                            </div>
                                            <asp:BulletedList runat="server" ID="blstErrores" Font-Bold="True" ForeColor="Red"
                                                meta:resourcekey="blstErroresResource1">
                                            </asp:BulletedList>
                                            <asp:Label ID="lblSuccess" runat="server" CssClass="headerbar_text successfuly-added" meta:resourcekey="lblProdAddSuccessResource"
                                                Visible="False"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <cc2:ContentReader ID="PriceListMessage" runat="server" ContentPath="priceListMessage.html"
                        SectionName="Ordering" ValidateContent="True" UseLocal="True" HtmlContent="" meta:resourcekey="PriceListMessageResource1" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="0" cellpadding="0" class="gdo-border pricelist-table">
                                    <tbody>
                                         <div id="dvSubcategories" runat="server">
                                        <tr>
                                            <th class="col-Avail">
                                            </th>
                                            <th class="col-SKU">
                                                <asp:Label runat="server" Text="SKU" ID="titleSKU" meta:resourcekey="titleSKUResource1"></asp:Label>
                                            </th>
                                            <th class="col-Product">
                                                <asp:Label runat="server" Text="Product" ID="titleProduct" meta:resourcekey="titleProductResource1"></asp:Label>
                                            </th>
                                            <th class="col-QTY">
                                                <asp:Label runat="server" Text="QTY" ID="titleQTY" meta:resourcekey="titleQTYResource1"></asp:Label>
                                            </th>
                                            <th runat="server" id="thTitleRetailPrice" class="col-RetailPrice">
                                                <asp:Label runat="server" Text="Retail Price" ID="titleRetailPrice" meta:resourcekey="titleRetailPriceResource1"></asp:Label>
                                            </th>
                                            <th runat="server" id="thTitleDiscountedPrice" class="col-RetailPrice">
                                                <asp:Label runat="server" Text="Discounted Price" ID="titleDiscountedPrice" meta:resourcekey="titleDiscountedPriceResource1"></asp:Label>
                                            </th>
                                            <th runat="server" id="thTitleVolumePoint" class="col-VolumePoint">
                                                <asp:Label runat="server" Text="Volume Point" ID="titleVolumePoint" meta:resourcekey="titleVolumePointResource1"></asp:Label>
                                            </th>
                                            <th runat="server" id="thTitleEarnBase" class="col-EarnBase">
                                                <asp:Label runat="server" Text="Earn Base" ID="titleEarnBase" meta:resourcekey="titleEarnBaseResource1"></asp:Label>
                                            </th>
                                        </tr>
                                        </div>
                                        <asp:Repeater runat="server" ID="Subcategories" OnItemDataBound="OnSubcategories_ItemDataBound">
                                            <ItemTemplate>
                                                <tr runat="server" id="trBreadcrumb">
                                                    <td class="gdo-pricelist-breadcrumb" colspan='<%# getProductTableColumns() %>' runat="server">
                                                        <asp:Label ID="lbBreadCrumb" runat="server" Text='<%# getBreadCrumbText((Container).DataItem as UICategoryProduct) %>'></asp:Label></font>
                                                    </td>
                                                </tr>
                                                <asp:Repeater runat="server" ID="Products" OnItemCreated="OnProducts_OnItemCreated" OnItemDataBound="OnProducts_ItemDataBound" >
                                                    <ItemTemplate>
                                                        <tr runat="server" id="LineItem" class="gdo-row-odd gdo-pricelist-tbl-data pricelist-data">
                                                                        <td runat="server" class="col-Avail">
                                                                            <hrblAvail:MyAvail ID="MyAvail1" runat="server" ShowLabel="false" Available='<%# Eval("ProductAvailability") %>' />
                                                                        </td>
                                                                        <td runat="server" class="col-SKU">
                                                                            <span>
                                                                                <%# Eval("SKU") %>
                                                                            </span>
                                                                            <asp:HiddenField ID="ProductID" runat="server" Value='<%# Eval("SKU") %>' />
                                                                            <asp:HiddenField ID="ParentProductID" runat="server" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID") %>' />
                                                                            <asp:HiddenField ID="ParentCategoryID" runat="server" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID") %>' />
                                                                        </td>
                                                                        <td runat="server" class="col-Product">
                                                                            <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                                                                CommandArgument='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID"), DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID")) %>'
                                                                                Text='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>'></asp:LinkButton>
                                                                        </td>
                                                                        <td runat="server" align="center" class="col-QTY">
                                                                            <asp:TextBox ID="QuantityBox" runat="server" BackColor='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) ? ColorTranslator.FromHtml("white") : ColorTranslator.FromHtml("#CCCCCC") %>'
                                                                                Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'
                                                                                MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="5" CssClass="onlyNumbers"></asp:TextBox>
                                                                        </td>
                                                                        <td runat="server" style="text-align: center" class="col-RetailPrice" id="tdRetailPrice">
                                                                            <asp:Label ID="Retail" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice", getAmountString((decimal) Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
                                                                        </td>
                                                                        <td runat="server" id="tdDiscountedPrice" class="col-RetailPrice" style="text-align: center">
                                                                            <asp:Label ID="DiscountedPrice" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? "Purchasable" : "" %>'></asp:Label>
                                                                        </td>
                                                                        <td runat="server" id="tdVolumePoint" style="text-align: center" class="col-VolumePoint">
                                                                            <asp:Label ID="VolumePoints" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.VolumePoints", GetVolumePointsFormat((decimal) Eval("CatalogItem.VolumePoints"))) : "" %>'></asp:Label>
                                                                        </td>
                                                                        <td runat="server" id="tdEarnBase" style="text-align: center" class="col-EarnBase">
                                                                            <asp:Label ID="EarnBase" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? (MyHerbalife3.Ordering.Configuration.ConfigurationManagement.HLConfigManager.Configurations.ShoppingCartConfiguration.ShowEarnBaseCurrencySymbol ? Eval("CatalogItem.EarnBase", getAmountString((decimal) Eval("CatalogItem.EarnBase"))) : Eval("CatalogItem.EarnBase", GetPriceFormat((decimal) Eval("CatalogItem.EarnBase")))) : "" %>'></asp:Label>
                                                                        </td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <AlternatingItemTemplate>
                                                        <tr runat="server" id="LineItem" class="gdo-row-even gdo-pricelist-tbl-data pricelist-data">
                                                                            <td class="col-Avail" runat="server">
                                                                                &nbsp;<hrblAvail:MyAvail ID="MyAvail1" runat="server" ShowLabel="false" Available='<%# Eval("ProductAvailability") %>' />
                                                                            </td>
                                                                            <td class="col-SKU" runat="server">
                                                                                <span>
                                                                                    <%#Eval("SKU") %>
                                                                                </span>
                                                                                <asp:HiddenField runat="server" ID="ProductID" Value='<%# Eval("SKU") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentProductID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentCategoryID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID") %>' />
                                                                            </td>
                                                                            <td class="col-Product" runat="server">
                                                                                <asp:LinkButton runat="server" ID="LinkProductDetail" CommandArgument='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID"), DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID")) %>'
                                                                                    Text='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>'
                                                                                    OnClick="ProductDetailClicked"></asp:LinkButton>
                                                                            </td>
                                                                            <td align="center" class="col-QTY" runat="server">
                                                                                <asp:TextBox runat="server" ID="QuantityBox" size="5" MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'
                                                                                    BackColor='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) ? ColorTranslator.FromHtml("white") : ColorTranslator.FromHtml("#CCCCCC") %>' CssClass="onlyNumbers"></asp:TextBox>
                                                                            </td>
                                                                            <td style="text-align: center" class="col-RetailPrice" id="tdRetailPrice" runat="server">
                                                                                <asp:Label runat="server" ID="Retail" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice", getAmountString((decimal) Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
                                                                            </td>
                                                                            <td runat="server" id="tdDiscountedPrice" class="col-RetailPrice" style="text-align: center">
                                                                                <asp:Label ID="DiscountedPrice" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? "Purchasable" : "" %>'></asp:Label>
                                                                            </td>
                                                                            <td runat="server" id="tdVolumePoint" style="text-align: center" class="col-VolumePoint">
                                                                                <asp:Label runat="server" ID="VolumePoints" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.VolumePoints", GetVolumePointsFormat((decimal) Eval("CatalogItem.VolumePoints"))) : "" %>'></asp:Label>
                                                                            </td>
                                                                            <td id="tdEarnBase2" runat="server" style="text-align: center" class="col-EarnBase">
                                                                                <asp:Label ID="EarnBase" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? (MyHerbalife3.Ordering.Configuration.ConfigurationManagement.HLConfigManager.Configurations.ShoppingCartConfiguration.ShowEarnBaseCurrencySymbol ? Eval("CatalogItem.EarnBase", getAmountString((decimal) Eval("CatalogItem.EarnBase"))) : Eval("CatalogItem.EarnBase", GetPriceFormat((decimal) Eval("CatalogItem.EarnBase")))) : "" %>'></asp:Label>
                                                                            </td>
                                                        </tr>
                                                    </AlternatingItemTemplate>
                                                </asp:Repeater>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                                 <asp:PlaceHolder ID="priceListHTMLGrid" runat="server"/>
                                <asp:HiddenField runat="server" ID="pricelistGridInfo"/>
                                 <asp:HiddenField runat="server" ID="pricelistGrid"/>
                                <asp:Button runat="server" ID="DummyLinkProductDetail" OnClick="DummyProductDetailClicked" style="visibility:hidden" meta:resourcekey="DummyLinkProductDetailResource1"></asp:Button>
                                <asp:HiddenField runat="server" ID="hiddnProductIDCatagoriID"/>
                                <asp:Repeater runat="server" ID="SubcategoriesResponsive" OnItemDataBound="OnSubcategories_ItemDataBound">
                                    <ItemTemplate>
                                        <div runat="server" id="trBreadcrumb">
                                                    <div id="Td1" class="gdo-pricelist-breadcrumb" runat="server">
                                                        <asp:Label ID="lbBreadCrumb" runat="server" Text='<%# getBreadCrumbText((Container).DataItem as UICategoryProduct) %>' meta:resourcekey="lbBreadCrumbResource1"></asp:Label></font>
                                                    </div>
                                                </div>
                                        <asp:Repeater runat="server" ID="Products" OnItemCreated="OnProducts_OnItemCreated">
                                            <ItemTemplate>
                                                <div class="row product-info gdo-row-odd myclear">
                                                    <div class="col-xs-9 left-side">
                                                        <div class="col-xs-12 col-Product">
                                                            <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                                                                CommandArgument='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID"), DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID")) %>'
                                                                                Text='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>' meta:resourcekey="LinkProductDetailResource3"></asp:LinkButton>
                                                        </div>
                                                        <div class="col-xs-6 bold">
                                                            <div><asp:Label runat="server" Text="SKU" ID="titleSKU" meta:resourcekey="titleSKUResource1"></asp:Label></div>
                                                            <div><asp:Label runat="server" Text="Retail Price" ID="titleRetailPrice" meta:resourcekey="titleRetailPriceResource1"></asp:Label></div>
                                                            <div><asp:Label runat="server" Text="Volume Point" ID="titleVolumePoint" meta:resourcekey="titleVolumePointResource1"></asp:Label></div>
                                                        </div>
                                                        <div class="col-xs-4 align-right">
                                                            <div><%# Eval("SKU") %></div>
                                                            <asp:HiddenField runat="server" ID="ProductID" Value='<%# Eval("SKU") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentProductID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentCategoryID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID") %>' />
                                                            <div><asp:Label ID="Retail" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice", getAmountString((decimal) Eval("CatalogItem.ListPrice"))) : "" %>' meta:resourcekey="RetailResource2"></asp:Label></div>
                                                            <div><asp:Label ID="VolumePoints" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.VolumePoints", GetVolumePointsFormat((decimal) Eval("CatalogItem.VolumePoints"))) : "" %>' meta:resourcekey="VolumePointsResource2"></asp:Label></div>
                                                        </div>

                                                    </div>
                                                    <div class="col-xs-3 right-side">
                                                        <div class="align-right col-Avail" id="ctl00_ctl00_ContentArea_ProductsContent_Subcategories_ctl00_Products_ctl00_MyAvail1_divGreen">
                                                            <hrblAvail:MyAvail ID="MyAvail1" runat="server" ShowLabel="false" Available='<%# Eval("ProductAvailability") %>' />
                                                            
                                                        </div>
                                                        <div class="center col-QTY">
                                                            <asp:Label class="qty" runat="server" Text="QTY" ID="titleQTY" meta:resourcekey="titleQTYResource1"></asp:Label>
                                                            
                                                            <asp:TextBox ID="QuantityBox" runat="server" BackColor='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) ? ColorTranslator.FromHtml("white") : ColorTranslator.FromHtml("#CCCCCC") %>'
                                                                                Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'
                                                                                MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="5" CssClass="onlyNumbers" meta:resourcekey="QuantityBoxResource2"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                </ItemTemplate>
                                            <AlternatingItemTemplate>
                                                <div class="row product-info gdo-row-even myclear">
                                                    <div class="col-xs-9 left-side">
                                                        <div class="col-xs-12 col-Product">
                                                            <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                                                                CommandArgument='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID"), DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID")) %>'
                                                                                Text='<%# string.Format("{0} {1}", DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.DisplayName"), Eval("Description")) %>' meta:resourcekey="LinkProductDetailResource2"></asp:LinkButton>
                                                        </div>
                                                        <div class="col-xs-6 bold">
                                                            <div><asp:Label runat="server" Text="SKU" ID="titleSKU" meta:resourcekey="titleSKUResource1"></asp:Label></div>
                                                            <div><asp:Label runat="server" Text="Retail Price" ID="titleRetailPrice" meta:resourcekey="titleRetailPriceResource1"></asp:Label></div>
                                                            <div><asp:Label runat="server" Text="Volume Point" ID="titleVolumePoint" meta:resourcekey="titleVolumePointResource1"></asp:Label></div>
                                                        </div>
                                                        <div class="col-xs-4 align-right">
                                                            <div><%# Eval("SKU") %></div>
                                                            <asp:HiddenField runat="server" ID="ProductID" Value='<%# Eval("SKU") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentProductID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Product.ID") %>' />
                                                                                <asp:HiddenField runat="server" ID="ParentCategoryID" Value='<%# DataBinder.Eval(((RepeaterItem) Container.Parent.Parent).DataItem, "Category.ID") %>' />
                                                            <div><asp:Label ID="Retail" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice", getAmountString((decimal) Eval("CatalogItem.ListPrice"))) : "" %>' meta:resourcekey="RetailResource1"></asp:Label></div>
                                                            <div><asp:Label ID="VolumePoints" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.VolumePoints", GetVolumePointsFormat((decimal) Eval("CatalogItem.VolumePoints"))) : "" %>' meta:resourcekey="VolumePointsResource1"></asp:Label></div>
                                                        </div>

                                                    </div>
                                                    <div class="col-xs-3 right-side">
                                                        <div class="align-right col-Avail" id="ctl00_ctl00_ContentArea_ProductsContent_Subcategories_ctl00_Products_ctl00_MyAvail1_divGreen">
                                                            <hrblAvail:MyAvail ID="MyAvail1" runat="server" ShowLabel="false" Available='<%# Eval("ProductAvailability") %>' />
                                                            
                                                        </div>
                                                        <div class="center col-QTY">
                                                            
                                                            <asp:Label class="qty" runat="server" Text="QTY" ID="titleQTY" meta:resourcekey="titleQTYResource1"></asp:Label>
                                                            <asp:TextBox ID="QuantityBox" runat="server" BackColor='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) ? ColorTranslator.FromHtml("white") : ColorTranslator.FromHtml("#CCCCCC") %>'
                                                                                Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'
                                                                                MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="5" CssClass="onlyNumbers" meta:resourcekey="QuantityBoxResource1"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </AlternatingItemTemplate>
                                            </asp:Repeater>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div id="divPriceListButton3">
                                    <cc1:DynamicButton ID="recalcVPSubmit2" runat="server" Text="Recalculate Volume"
                                        ButtonType="Neutral" IconType="Plus" OnClick="OnRecalculate" meta:resourcekey="recalcVPSubmitResource1" name="recalcVPSubmit_2" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                                    <cc1:DynamicButton ID="AddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                                        OnClick="OnAddToCart" OnClientClick="DisableAddtoCart();" meta:resourcekey="AddToCartResource1" name="addCart_2" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                                    <cc1:DynamicButton ID="AddToCartDisabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                                        Disabled="True" Hidden="True" meta:resourcekey="AddToCartResource1" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div id="divAvail" style="margin: 5px;">
                                    <hrblAvail:MyAvail ID="ProductAvailability" runat="server" />
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div>
                <cc2:ContentReader ID="PlLogos" runat="server" ContentPath="paymentLogos.html"
                    SectionName="Ordering" ValidateContent="True" UseLocal="True" HtmlContent="" meta:resourcekey="PlLogosResource1" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="UpdatePanelDupeOrder" runat="server">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="dupeOrderPopupExtender" runat="server" TargetControlID="DupeOrderFakeTarget"
                PopupControlID="pnldupeOrderMonth" CancelControlID="DupeOrderFakeTarget" BackgroundCssClass="modalBackground" DynamicServicePath="" Enabled="True" />
            <asp:Button ID="DupeOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" meta:resourcekey="DupeOrderFakeTargetResource1" />
            <asp:Panel ID="pnldupeOrderMonth" runat="server" Style="display: none" meta:resourcekey="pnldupeOrderMonthResource1">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Recent Order" meta:resourcekey="lblDupeOrder"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server" meta:resourcekey="lblDupeOrderMessageResource1"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
 
       <asp:UpdatePanel ID="UpdatePanelPromosku" runat="server">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="promoSkuPopupExtender" runat="server" TargetControlID="PromoSkuFakeTarget"
                PopupControlID="pnldupePromoSku" CancelControlID="DynamicButtonPromoYes" BackgroundCssClass="modalBackground" DynamicServicePath="" Enabled="True" />
            <asp:Button ID="PromoSkuFakeTarget" runat="server" CausesValidation="False" Style="display: none" meta:resourcekey="PromoSkuFakeTargetResource1" />
            <asp:Panel ID="pnldupePromoSku" runat="server" Style="display: none" meta:resourcekey="pnldupePromoSkuResource1">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblheader" runat="server" Text="Information" meta:resourcekey="Promoheader"  ></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblPromoMessage" Text="You have received a free gift and it has been automatically added to your card" meta:resourcekey="PromoMessage" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="DynamicButtonPromoYes" runat="server" ButtonType="Forward" Text="OK" OnClick="HidePromoMsg" meta:resourcekey="OK" IconClass="" IconPosition="Left" NavigateUrlToNewWindow="False" Rel="" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel> 
    <PromoMY:Promotion_MY runat="server" id="Promotion_MY" />
    <CnChkout24h:CnChkout24h runat="server" ID="CnChkout24h" />

</asp:Content>
