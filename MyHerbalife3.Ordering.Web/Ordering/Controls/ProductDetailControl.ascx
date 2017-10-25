<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductDetailControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductDetailControl" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Register Src="~/Ordering/Controls/ProductList.ascx" TagName="ProductList" TagPrefix="hrblProductList" %>
<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="ProductAvailability"
    TagPrefix="hrblProductAvailability" %>
<%@ Register Src="~/Ordering/Controls/ProductInfoFooter.ascx" TagName="ProductInfoFooter"
    TagPrefix="hrblProductInfoFooter" %>
<%@ Register Src="~/Ordering/Controls/ProductLink.ascx" TagName="ProductLink" TagPrefix="hrblProductLink" %>
<%@ Register Src="~/Ordering/Controls/ProductImage.ascx" TagName="ProductImage" TagPrefix="hrblProdImg" %>
<%@ Register TagPrefix="CnChkout24h" TagName="CnChkout24h" Src="~/Ordering/Controls/CnChkout24h.ascx" %>
<%@ Register Src="~/Ordering/Controls/Promotion_MY.ascx" TagPrefix="PromoMY" TagName="Promotion_MY" %>
<%@ Register Src="~/Ordering/Controls/ExpireDatePopUp.ascx" TagPrefix="ExpirePopUp" TagName="ExpireDatePopUp" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>

<%--<script type="text/javascript" src='<%= Page.ResolveUrl("~/Ordering/Scripts/ProductFavourite.js") %>' ></script>--%>
<script type="text/javascript">
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);

    function EndRequestHandler(sender, args) {
        try {
            // clear the width property
            if (document.getElementById('<%= upProductDetail.ClientID %>').style.filter) {
                document.getElementById('<%= upProductDetail.ClientID %>').style.filter = "";
            }
        } catch(e) {
        }
    }
    
    function DisableAddtoCart() {
        $('#<%= DynamicButton2.ClientID %>').css('display', 'none');
        $('#<%= CheckoutButton.ClientID %>').css('display', 'none');
        
        $('#<%= DynamicButton1.ClientID%>').css('display', 'inline-block');
        $('#<%= CheckoutButton1.ClientID%>').css('display', 'inline-block');
    }
    var showAvailabilityText = '<%= (this.GetLocalResourceObject("ShowAvailability.Text") as string) %>';
    var hideAvailabilityText = '<%= (this.GetLocalResourceObject("hideAvailability.Text") as string) %>';
    var linkInstructionsText = '<%= (this.GetLocalResourceObject("linkInstructions.Text") as string) %>';
    var anchorText           = '<%= (this.GetLocalResourceObject("anchorText.Text") as string) %>';


</script>
<% if (HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations) { %>
    <script type="text/javascript">
        AdobeTarget = {
            "entity" : {
                "id" : "<%= AT_id %>",
                "item" : "<%= AT_item %>",
                "categoryId" : "<%= AT_categories %>",
                "value" : <%= AT_value %>,
                "inventory": <%= AT_inventory %>,
            }
        };
    
        Salesforce = ["trackPageView", { "item" : "<%= AT_id %>" }];
    </script>
<% } %>
<asp:UpdatePanel ID="upProductDetail" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="popupClose" style="margin-bottom: 5px;" id="popupClose" runat="server">
            <% if ( HLConfigManager.Configurations.DOConfiguration.IsChina ) {%>
                <asp:LinkButton ID="LinkButton1" runat="server" Text="" OnClick="OnCancel" CssClass="icon icon-delete-fl-5 hide"/>
            <% } else{%>
                <asp:LinkButton ID="CancelButton" runat="server" Text="" OnClick="OnCancel" CssClass=" icon icon-delete-ln-4" />
            <% } %>
        </div>
        <div id="divProductDetail" runat="server">

            <div>
                <div class="tabs-main-wrap three-tabs-box" id="3tabsbox">
                    <div class="tabs-button-wrap" runat="server" id="divAddscript">
                        <div runat="server" id="tabone">
                            <div id="vis1" class="focusTab" onclick=" switchingTab('1'); adjustDividerLineTab2_(); ">
                                <span id="vis1-header">
                                    <asp:Label ID="lbProductOverview" runat="server" Text="Product Overview" meta:resourcekey="lbProductOverviewResource1"></asp:Label>
                                </span>
                            </div>
                        </div>
                        <div runat="server" id="tabtwo">
                            <div class="unFocusTab inline" id="vis2" onclick=" switchingTab('2'); adjustDividerLineTab2_(); ">
                                <span id="vis2-header">
                                    <asp:Label ID="lbProdDetails" runat="server" Text="Product Details" meta:resourcekey="lbProdDetailsResource1"></asp:Label></span>
                            </div>
                        </div>
                        <div runat="server" id="tabthree">
                            <div class="unFocusTab inline" id="vis3" onclick=" switchingTab('3'); adjustDividerLineTab3_(); ">
                                <span id="vis3-header">
                                    <asp:Label ID="lbFastFacts2" runat="server" Text="Fast Facts" meta:resourcekey="lbFastFacts2Resource1"></asp:Label></span>
                            </div>
                            <div class="clear">
                            </div>
                        </div>
                    </div>
                    <div class="tabs-bodywrap-blu2">
                        <div class="tabs-content-wrap">
                            <div class="col-md-12 myclear">
                                <h4 class="blue" id="uxProductName" runat="server"></h4>
                            </div>
                            <div class="col-md-12 myclear">
                                <div class="inline static_wrap col-md-8">
                                    <div class="inline" id="dynamic_wrap1">
                                        <div runat="server" id="DivOverviewTab1">
                                            <div class="inline" id="tab1-col1">
                                                <div>
                                                    <asp:Label ID="lbOverview" runat="server" Text="Overview" meta:resourcekey="lbOverviewResource1"></asp:Label>
                                                </div>
                                                <div runat="server" id="uxOverview">
                                                </div>
                                            </div>
                                            <div class="inline" id="tab1-divider">
                                            </div>
                                            <div class="inline" id="tab1-col2" style="padding-left: 10px; width: 179px">
                                                <div>
                                                    <asp:Label ID="lbKeyBenefits" runat="server" Text="Key Benefits" meta:resourcekey="lbKeyBenefitsResource1" />
                                                </div>
                                                <div runat="server" id="pBenefits">
                                                </div>
                                            </div>
                                        </div>
                                        <div runat="server" id="DivOverviewTab2">
                                            <div class="inline" id="tab12-col1">
                                                <div>
                                                    <asp:Label ID="lbOverview2" runat="server" Text="Overview" meta:resourcekey="lbOverview2Resource1"></asp:Label>
                                                </div>
                                                <div runat="server" id="uxOverview2">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="inline" id="dynamic_wrap2">
                                        <div runat="server" id="DivUsageTab1" class="myclear">
                                            <div class="inline" id="tab2-col1">
                                                <div>
                                                    <asp:Label ID="lbDetails" runat="server" Text="Details" meta:resourcekey="lbDetailsResource1" />
                                                </div>
                                                <div runat="server" id="pDetails">
                                                </div>
                                            </div>
                                            <div class="inline" id="tab2-divider">
                                            </div>
                                            <div class="inline" id="tab2-col2">
                                                <div>
                                                    <asp:Label ID="lbUsage" runat="server" Text="Usage" meta:resourcekey="lbUsageResource1" />
                                                </div>
                                                <div runat="server" id="pUsage">
                                                </div>
                                            </div>
                                        </div>
                                        <div runat="server" id="DivUsageTab2">
                                            <div class="inline" id="tab22-col1">
                                                <div>
                                                    <asp:Label ID="Label1" runat="server" Text="Details" meta:resourcekey="Label1Resource1" />
                                                </div>
                                                <div runat="server" id="pDetails2">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="inline" id="dynamic_wrap3">
                                        <div runat="server" id="DivFastFactsTab1">
                                            <div class="inline" id="tab3-col1">
                                                <div>
                                                    <asp:Label ID="lbFastFacts" runat="server" Text="Fast Facts" meta:resourcekey="lbFastFactsResource1" />
                                                </div>
                                                <div runat="server" id="pQuickFacts">
                                                </div>
                                            </div>
                                            <div class="inline" id="tab3-divider">
                                            </div>
                                            <div class="inline" id="tab3-col2">
                                            </div>
                                        </div>
                                        <div runat="server" id="DivFastFactsTab2">
                                            <div class="inline" id="tab32-col1">
                                                <div>
                                                    <asp:Label ID="lbFastFacts3" runat="server" Text="Fast Facts" meta:resourcekey="lbFastFacts3Resource1" />
                                                </div>
                                                <div runat="server" id="pQuickFacts2">
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="inline col-md-4 center product-image">
                                    <hrblProdImg:ProductImage ID="ProdImage" runat="server"></hrblProdImg:ProductImage>
                                </div>
                            </div>

                            <%--Size Chart starts--%>
                            <div runat="server" id="sizeChartTable" class="hide sizeChartTable">
                                <div class="measurement-message">
                                    <asp:Literal ID="measurementMessage" runat="server" Text="" meta:resourcekey="measurementMessage" />
                                </div>
                                <asp:Table runat="server" ID="tblSizeChart" CssClass="cf size-chart">
                                    
                                </asp:Table>
                                <div id='responsive-table-shadow'></div>
                            </div>
                            <%--Size Chart ends--%>

                            <div class="clear"></div>
                            <hrblProductLink:ProductLink ID="ProductLinks" runat="server" />
                            <input type="hidden" runat="server" id="hdCategoryID" value="0" />
                            <input type="hidden" runat="server" id="ProdID" value="0" />
                            <hrblProductInfoFooter:ProductInfoFooter ID="ProductInfoFooter1" runat="server" />

                            <div class="gdo-body-text">
                                <asp:Label runat="server" ID="VolumePointText" Text="Volume For Selected Items:"
                                    meta:resourcekey="VolumePointTextResource1"></asp:Label>
                                <asp:Label runat="server" ID="lbVolumePoint" meta:resourcekey="lbVolumePointResource1" CssClass="totalVP"></asp:Label>
                                <br/>
                                <asp:Label runat="server" ID="TotalAmountText" Text="Amount For Selected Items:"
                                    meta:resourcekey="TotalAmountTextResource1" Visible="false"></asp:Label>
                                <asp:Label runat="server" ID="lbTotalAmount" meta:resourcekey="TotalAmountResource1" Visible="false" Text="0.00" CssClass="totalAmount"></asp:Label>
                            </div>
                            <div class="productDetailsButtons add-cart-top">
                                <cc1:DynamicButton ID="recalcVPSubmit" runat="server" Text="Recalculate Volume" OnClick="OnRecalculate"
                                    ButtonType="Neutral" IconType="Plus" meta:resourcekey="recalcVPSubmitResource1" name="recalcVPSubmit_1" />
                                <cc1:DynamicButton ID="DynamicButton1" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                                    Text="Add to Cart" Disabled="true" Hidden="true" />
                                <cc1:DynamicButton ID="DynamicButton2" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                                    OnClick="AddToCart" Text="Add to Cart" OnClientClick="DisableAddtoCart();" name="addCart_1" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="tabs-SKU">
                <asp:BulletedList runat="server" ID="uxErrores" ForeColor="Red" Font-Bold="True"
                    BulletStyle="Circle" meta:resourcekey="uxErroresResource1">
                </asp:BulletedList>
                <div class="table-container">
                    <asp:Repeater runat="server" ID="Products" OnItemCreated="Products_OnItemCreated" >
                        <HeaderTemplate>
                            <table cellspacing="1" cellpadding="2">
                                <tr>
                                    <th style="text-align: center;">
                                        <asp:Label runat="server" ID="lbSKU" Text="SKU" meta:resourcekey="lbSKUResource1"></asp:Label>
                                    </th>
                                    <th style="text-align: center;">
                                        <asp:Label runat="server" ID="lbFlavorType" Text="Flavor/Type" meta:resourcekey="lbFlavorTypeResource1"></asp:Label>
                                    </th>
                                    <th class="DocumentDetails_element" style="text-align: center;" runat="server" id="thDoc">
                                        <asp:Label runat="server" ID="lbDoc" Text="Details" meta:resourcekey="lbDocResource1" />
                                    </th>
                                    <th style="text-align: center;" runat="server" id="thVolumePoint">
                                        <asp:Label runat="server" ID="lbVolumePoint" Text="Volume Point" meta:resourcekey="lbVolumePointResource1" CssClass="label"/>
                                    </th>
                                    <th style="text-align: center;" runat="server" id="thEarnBase">
                                        <asp:Label runat="server" ID="lbEarnBase" Text="Earn Base" meta:resourcekey="lbEarnBaseResource1" />
                                    </th>
                                    <th style="text-align: center;" runat="server" id="thRetailPrice">
                                        <asp:Label runat="server" ID="lbRetailPrice" Text="Retail Price" meta:resourcekey="lbRetailPriceResource1" />
                                    </th>
                                    <th runat="server" id="thTitleDiscountedPrice" style="text-align: center;">
                                        <asp:Label ID="lbDiscountedPrice" runat="server" Text="Discounted Price" meta:resourcekey="lbDistributorPrice" />
                                    </th>
                                    <th style="text-align: center;">
                                        <asp:Label runat="server" ID="lbQuantity" Text="Quantity" meta:resourcekey="lbQuantityResource1" />
                                    </th>
                                    <th style="text-align: center;<%# IsDisplay() %>" >
                                        <asp:Label runat="server" ID="lbFavourite" Text="Favourite" meta:resourcekey="lbFavouriteResource1" />
                                    </th>
                                </tr>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr runat="server" id="trLineItem">
                                <td class="td_pricelist_item" style="width: 60px">
                                    <table>
                                        <tr>
                                            <td>
                                                <hrblProductAvailability:ProductAvailability ID="ProductAvailibility" runat="server"
                                                    ShowLabel="false" Available='<%# Eval("ProductAvailability") %>' />
                                            </td>
                                            <td>
                                                <%#Eval("SKU") %>
                                            </td>
                                        </tr>
                                    </table>
                                    <asp:HiddenField runat="server" ID="ProductoID" Value='<%# Eval("SKU") %>' />
                                </td>
                                <td style="text-align: center; width: 220px">
                                    <asp:Label runat="server" ID="FlavorType" Text='<%# Eval("Description") %>'></asp:Label>
                                </td>
                                <td runat="server" id="tdPdf" class="td_pricelist_item DocumentDetails_element" style="text-align: center; width: 60px;">
                                    <div id="pdfLabel">
                                        <a runat="server" id="pdfLink" target="_blank" href='<%# Eval("PdfName") %>'>
                                            <img id="Img1" runat="server" visible='<%# string.IsNullOrEmpty(Eval("PdfName") as string) == false %>'
                                                style="margin: 0px 5px -3px 0px; width: 16px; height: 16px" alt="PDF Icon" src="/Content/Global/img/icon_pdf.gif" />
                                            &nbsp;&nbsp;</a>
                                    </div>
                                </td>
                                <td runat="server" id="tdVolumePoint" class="td_pricelist_item" style="width: 76px; text-align: center">
                                    <asp:Label runat="server" ID="VolumePoints" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.VolumePoints", GetVolumeString((decimal)Eval("CatalogItem.VolumePoints"))) : "" %>'></asp:Label>
                                </td>
                                <td runat="server" id="tdEarnBase" class="td_pricelist_item" style="width: 66; text-align: center">
                                    <asp:Label runat="server" ID="EarnBase" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.EarnBase", getAmountString((decimal) Eval("CatalogItem.EarnBase"))) : "" %>'></asp:Label>
                                </td>
                                <td runat="server" id="tdRetailPrice" class="td_pricelist_item" style="text-align: center" list-price='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice") : "" %>'>
                                    <asp:Label runat="server" ID="Retail" Text='<%# (bool) Eval("IsPurchasable") ? Eval("CatalogItem.ListPrice", getAmountString((decimal) Eval("CatalogItem.ListPrice"))) : "" %>'></asp:Label>
                                </td>
                                <td runat="server" id="tdDiscountedPrice" class="td_pricelist_item" style="text-align: center">
                                    <asp:Label ID="DiscountedPrice" runat="server" Text='<%# (bool) Eval("IsPurchasable") ? "Purchasable" : "" %>'></asp:Label>
                                </td>
                                <td width="74px" align="center">
                                    <asp:TextBox runat="server" ID="Quantity" BackColor='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) ? ColorTranslator.FromHtml("white") : ColorTranslator.FromHtml("#CCCCCC") %>'
                                    size="5" MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' Enabled='<%# getEnabled((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>' CssClass="onlyNumbers" Visible='<%# getVisible((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'></asp:TextBox>
                                <a id="LinkBackOrderDetails" runat="server" meta:resourcekey="ShowAvailability" Visible='<%# !getVisible((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType) Eval("ProductAvailability")) %>'
                                    onclick='getSKUBackOrder(this, event)' sku='<%# Eval("SKU") %>' uid="ProductID" ctg='Category.ID'><asp:Literal runat="server" Text="Show Availability" meta:resourcekey="ShowAvailability"></asp:Literal></a>
                                    <asp:HiddenField ID="ProductID" runat="server" Value='<%# Eval("SKU") %>' />
                                </td>
                                <td width="74px" align="center" style='<%# IsDisplay() %>'  >
                                    <img src="<%# (string)Eval("Version")=="Favor"?imgFavorON:imgFavorOFF  %>" id=<%# "prdDet" + Eval("ID") %> onclick='<%# "AddRemoveFavouriteClick(\"" + DistributorID +  "\",\"prdDet" + Eval("ID") +  "\",\"" + Eval("SKU") + "\"," + ((MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem_V01)DataBinder.Eval(Container.DataItem,"CatalogItem")).StockingSKU + ",99,0)" %>' />
                                </td>
                            </tr>
                        <% if (HLConfigManager.Configurations.DOConfiguration.DisplayBackOrderEnhancements){ %>
                        <tr>
                            <td colspan="7">
                                <div id='<%# "BackOrderContainer" + Eval("SKU") %>'>
                                    <table id='<%# "BackOrderTable" + Eval("SKU") %>' uid="ProductID" ctg='Category.ID' class="gdo-pricelist-tbl-data pricelist-data hide backOrderTable">
                                        <tr class="tr-head">
                                            <th colspan="2"><asp:Label runat="server" Text="Location" ID="titleBO" meta:resourcekey="titleBOLocationResource1"></asp:Label></th>
                                            <th><asp:Label ID="Label1" runat="server" Text="Type"  meta:resourcekey="titleBOTypeResource1"></asp:Label></th>
                                            <th><asp:Label ID="Label2" runat="server" Text="Status"  meta:resourcekey="titleBOStatusResource1"></asp:Label></th>
                                            <th><asp:Label ID="Label3" runat="server" Text="Availability Date" meta:resourcekey="titleBODateResource1"></asp:Label></th>
                                            <th colspan="2"><asp:Label ID="Label4" runat="server" Text="Comments"  meta:resourcekey="titleBOCommentsResource1"></asp:Label></th>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                             <% } %>
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div class="productDetailsButtons">
                    <cc1:DynamicButton ID="DynamicButton3" runat="server" Text="Recalculate Volume" OnClick="OnRecalculate"
                        ButtonType="Neutral" IconType="Plus" meta:resourcekey="recalcVPSubmitResource1" name="recalcVPSubmit_2" CssClass="recalculate-vp" />
                    <cc1:DynamicButton ID="CheckoutButton1" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                        Text="Add to Cart" Disabled="true" Hidden="true" name="CheckoutButton1" />
                    <cc1:DynamicButton ID="CheckoutButton" runat="server" ButtonType="Forward" meta:resourcekey="CheckoutButtonResource1"
                        OnClick="AddToCart" Text="Add to Cart" OnClientClick="DisableAddtoCart();" name="addCart_2" />
                </div>
            </div>
            <div id="divAvail" style="margin: 5px;">
                <hrblProductAvailability:ProductAvailability ShowLabel="true" ID="ProductAvailability" runat="server" />
            </div>
            <div class="noindex">
                <hrblProductList:ProductList ID="ProductsPanel" runat="server"></hrblProductList:ProductList>
            </div>

        </div>
        
        <asp:UpdatePanel ID="UpdatePanelPromosku" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="promoSkuPopupExtender" runat="server" TargetControlID="PromoSkuFakeTarget"
                PopupControlID="pnldupePromoSku" CancelControlID="PromoSkuFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="PromoSkuFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupePromoSku" runat="server" Style="display: none">
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
                        <cc1:DynamicButton ID="DynamicButtonPromoYes" runat="server" ButtonType="Forward" Text="OK" OnClick="HidePromoMsg" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel> 
          <asp:UpdatePanel ID="UpdatePanelSlowMoving" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="SlowmovingSkuPopupExtender" runat="server" TargetControlID="SlowMovingSkuFakeTarget"
                PopupControlID="pnldupeSlowMovingSkuTarget" CancelControlID="btnOK" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="SlowMovingSkuFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupeSlowMovingSkuTarget" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                          
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblSlowMovingDescription" Text="" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="btnOK" runat="server" ButtonType="Forward" Text="OK" OnClick="HideSlowMovingMsg" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel> 
        <PromoMY:Promotion_MY runat="server" id="Promotion_MY_Control" />
    <CnChkout24h:CnChkout24h runat="server" ID="CnChkout24h" />

    </ContentTemplate>

</asp:UpdatePanel>
<ExpirePopUp:ExpireDatePopUp runat="server" id="ExpireDatePopUp1" />