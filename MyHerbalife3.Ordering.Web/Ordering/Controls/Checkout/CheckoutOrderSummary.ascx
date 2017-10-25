<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckoutOrderSummary.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.CheckoutOrderSummary" %>
<%@ Register Src="~/Ordering/Controls/ProductAvailability.ascx" TagName="ProductAvailability"
    TagPrefix="uc1" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register Src="~/Ordering/Controls/Promotion_MY.ascx" TagPrefix="PromoMY" TagName="Promotion_MY" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:HiddenField runat="server" ID="hfCancelOrder" />
<script type="text/javascript">
    function ConfDelete(text) {
        var resp = window.confirm(text);
        var elements = document.getElementsByTagName("input");

        for (i = 0; i < elements.length; i++) {
            var p = elements[i];
            if (p.name.indexOf("hfCancelOrder") != -1) {
                p.value = resp;
                break;
            }
        }
    }

    
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(update_table);
    

    // === Fix for re-render the Product List table 76315 ===
    function update_table(sender, args) {
        if ($('[id$="hasValidationErrors"]').val() == "true") {
            scroll_to_element('[id$="divLabelErrors"]', 750);
        }
        Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(update_table);
    }
</script>
<progress:UpdatePanelProgressIndicator ID="progressOrderSummary" runat="server" TargetControlID="upOrderSummary" />
<asp:UpdatePanel ID="upOrderSummary" runat="server">
    <ContentTemplate>
        <cc1:ContentReader ID="StatementReader" runat="server" ContentPath="Statement.html" SectionName="ordering" ValidateContent="true" UseLocal="true" />
        <%--<hrblProductDetailPopup:ProductDetailPopup runat="server" ID="CntrlProductDetail" />--%>
        <div class="gdo-edit-summary">
            <div class="gdo-edit-header" id="divLabelOrderSummary" runat="server">
                <asp:Label ID="lblHeaderSummary" runat="server" Text="Edit Order Summary" meta:resourcekey="lblHeaderSummaryResource1"></asp:Label>
            </div>
            <asp:LinkButton runat="server" ID="DiscSkuLabelListLinkButton" CssClass="red" Text="Discontinued Sku Exist" Visible="False"></asp:LinkButton>
            <div class="gdo-edit-header" id="divEventTicketMessage" runat="server" visible="false"
                style="border-bottom: 0px">
                <asp:Label runat="server" ID="Label2" meta:resourcekey="lblEventTicketsMessageResource"></asp:Label>
            </div>
            <div class="gdo-edit-header" id="divSplitOrder" runat="server" visible="false" style="border-bottom: 0px">
                <cc1:ContentReader ID="SplitOrderReader" runat="server" ContentPath="splitOrder.html"
                    SectionName="ordering" ValidateContent="true" UseLocal="true" />
            </div>
            <div id="divSummaryMessage" runat="server" visible="false">
                <asp:Label runat="server" ID="lblSummaryMessage" meta:resourcekey="lblSummaryMessageResource"></asp:Label>
            </div>
            <div class="gdo-edit-header" id="divApparelMessage" runat="server" visible="true" style="border-bottom: 0px">
                <cc1:ContentReader ID="ApparelMessage" runat="server" ContentPath="apparelWarning.html"
                    SectionName="ordering" ValidateContent="true" UseLocal="true" />
            </div>
            <div class="gdo-edit-header" id="divCurrencyConvector" runat="server" visible="true" style="border-bottom: 0px">
                <cc1:ContentReader ID="CurrencyConvectorReader" runat="server" ContentPath="currencyConvector.html"
                    SectionName="ordering" ValidateContent="true" UseLocal="true" />
            </div>
            <div id="divLabelErrors" runat="server" class="gdo-edit-header" style="border-bottom: 0px">
                <table>
                    <tr>
                        <td>
                            <asp:Label Visible="False" runat="server" ID="cartEmpty" meta:resourcekey="lbCartEmpty"
                                Text="Cart is empty."></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"
                                meta:resourcekey="blErrorsResource1">
                            </asp:BulletedList>
                        </td>
                    </tr>
                </table>
            </div>

            <% if (this.DisplayChildSKUs) { %>
            <div class="headerConfirm" id="div2" runat="server">
                <asp:Label ID="Label5" runat="server" Text="Parent SKU/Items" meta:resourcekey="lblHeaderParentSKU"></asp:Label>
            </div>
            <% } %>

            <asp:ListView runat="server" ID="uxProducts" OnDataBound="ShoppingCartOnDataBound"
                OnItemDataBound="ShoppingCartOnItemDataBound"
                EnableModelValidation="True">
                <LayoutTemplate>
                    <table class="gdo-order-tbl parentSKUTable" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <tbody>
                            <tr>
                                <th class="wdth50">
                                    <asp:Label runat="server" ID="lblSku" Text="SKU" meta:resourcekey="lblSkuResource1"></asp:Label>
                                </th>
                                <th class="wdth330">
                                    <asp:Label runat="server" ID="lbProdName" Text="Product Name" Style="text-align: left;
                                        padding-left: 10px" meta:resourcekey="lbProdNameResource1"></asp:Label>
                                </th>
                                <th class="wdth50">
                                    <asp:Label runat="server" ID="lbQty" Text="Qty" meta:resourcekey="lbQtyResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THRetailPrice" class="wdth50">
                                    <asp:Label runat="server" ID="lbRetail" Text="Retail<br />Price" meta:resourcekey="lbRetailResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THVolPoints" class="wdth60">
                                    <asp:Label runat="server" ID="lbVolumePoints" Text="Volume</br>Points" meta:resourcekey="lbVolumePointsResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THEarnBase" class="wdth60">
                                    <asp:Label runat="server" ID="lbEarnBase" Text="Earn</br>Base" meta:resourcekey="lbEarnBaseResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THYourPrice" class="wdth60">
                                    <asp:Label runat="server" ID="lbYourPrice" Text="Your<br />Price" meta:resourcekey="lbYourPriceResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THRemove">
                                    <asp:Label runat="server" ID="lbRemove" Text="Delete" meta:resourcekey="lbRemoveResource1"></asp:Label>
                                    <% if (!this.IsHelpIconHidden)
                                       {
                                    %>
                                    <div>
                                        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderDelete" runat="server" TargetControlID="imgDelete"
                                            PopupControlID="pnlDeleteText" />
                                        <a href="javascript:return false;" onclick="return false;">
                                            <img id="imgDelete" runat="server" src="/Content/Global/img/gdo/icons/question-mark-blue.png"
                                                alt="info" height="12" width="12" />
                                        </a>
                                        <asp:Panel ID="pnlDeleteText" runat="server" Style="display: none">
                                            <div class="gdo-popup">
                                                <asp:Label ID="lblDeleteHelpText" runat="server" meta:resourcekey="DeleteHelp">
                                                </asp:Label>
                                            </div>
                                        </asp:Panel>
                                    </div>
                                    <%
                                       }
                                    %>
                                </th>
                            </tr>
                            <%--/HEADER ROW--%>
                            <%--PRODUCT ROW--%>
                            <tr runat="server" id="itemPlaceholder">
                            </tr>
                            <%--/PRODUCT ROW--%>
                        </tbody>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                     <tr runat="server" class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "gdo-row-even gdo-order-tbl-data" : "gdo-row-odd gdo-order-tbl-data" %>'>
                          <td id="TdSKU" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left">
                                        <uc1:ProductAvailability ID="ProductAvailability2" runat="server" ShowLabel="false"
                                            Available='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) %>' />
                                    </td>
                                    <td class="gdo-order-tbl-data-left">
                                        <asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdDescription" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left">
                                        <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>
                                        <asp:Label runat="server" ID="lbProductName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdQuantity" runat="server">
                            <p>
                                <asp:HiddenField runat="server" ID="uxProductoID" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />
                                <asp:TextBox MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="2" Style="margin-left: 0px; margin-right: 2px; text-align: right;"
                                    runat="server" ID="uxQuantity" Enabled='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) != MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType.Unavailable %>'
                                    Text='<%# Eval("Quantity") %>' CssClass="onlyNumbers"></asp:TextBox>
                                <asp:Label ID="lblQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>'
                                    Visible="False" />
                            </p>
                        </td>
                        <td id="TdRetailPrice" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice")))%></p>
                        </td>
                        <td id="TdVolumePoint" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourVolumePoints" Text='<%#Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints")))%>' />
                                </span>
                            </p>
                        </td>
                        <td id="TdEarnBase" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("EarnBase", getAmountString((decimal)Eval("EarnBase")))%>
                            </p>
                        </td>
                        <td id="TdYourPrice" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourPrice" Text='<%#Eval("DiscountPrice", getAmountString((decimal)Eval("DiscountPrice")))%>' />
                                </span>
                            </p>
                        </td>
                        <td id="TdDeleteCheckbox" class="gdo-order-tbl-data-center" runat="server">
                            <p>
                                &nbsp;
                                <asp:CheckBox runat="server" class="checkerbox" ID="uxDelete" />
                            </p>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
            
            <asp:ListView runat="server" ID="uxProductsResponsive" OnDataBound="ShoppingCartOnDataBound"
                OnItemDataBound="ShoppingCartOnItemDataBound"
                EnableModelValidation="True"
                Visible="False">
                <LayoutTemplate>
                    <div class="gdo-order-tbl" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <div runat="server" id="itemPlaceholder">
                        </div>    
                    </div>
                </LayoutTemplate>
                <ItemTemplate>
                    <div class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "row product-info gdo-row-even myclear" : "row product-info gdo-row-odd myclear" %>'>
                        <div class="col-xs-9 left-side">
                            <div class="col-xs-12 col-Product">
                                <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>
                                <asp:Label runat="server" ID="lbProductName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                            </div>
                            <div class="col-xs-6 bold">
                                <div><span><%= (GetLocalResourceObject("lblSkuResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                                <div><span><%= (GetLocalResourceObject("lbRetailResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                                <div><span><%= (GetLocalResourceObject("lbVolumePointsResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                            </div>
                            <div class="col-xs-4 align-right">
                                <div><asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label></div>
                                <div><%#Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice")))%></div>
                                <div><asp:Label ID="VolumePoints" runat="server" Text='<%#Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints")))%>'></asp:Label></div>
                            </div>
                        </div>
                        <div class="col-xs-3 right-side">
                            <div class="align-right col-Avail" id="ctl00_ctl00_ContentArea_ProductsContent_Subcategories_ctl00_Products_ctl00_MyAvail1_divGreen">
                                <uc1:ProductAvailability ID="ProductAvailability2" runat="server" ShowLabel="false"
                                    Available='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) %>' />
                            </div>
                            <div class="center col-QTY">
                                <asp:Label runat="server" ID="Label1" Text="Qty" meta:resourcekey="lbQtyResource1" class="qty"></asp:Label>
                                <asp:HiddenField runat="server" ID="uxProductoID" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />
                                <asp:TextBox MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="2" Style="margin-left: 0px; margin-right: 2px; text-align: right;"
                                    runat="server" ID="uxQuantity" Enabled='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) != MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType.Unavailable %>'
                                    Text='<%# Eval("Quantity") %>' CssClass="onlyNumbers"></asp:TextBox>
                                <asp:Label ID="lblQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>'
                                    Visible="False" />
                            </div>
                            <div id="TdDeleteCheckbox" runat="server" class='align-left'>
                                <asp:CheckBox runat="server" class="checkerbox" ID="uxDelete" meta:resourcekey="lbRemoveResource1"/>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:ListView>

            <% if (this.DisplayChildSKUs) { %>
            <div class="headerConfirm" id="div1" runat="server">
                <p></p>
                <asp:Label ID="Label4" runat="server" Text="Child SKU/Items" meta:resourcekey="lblHeaderChildSKU"></asp:Label>
            </div>
            <% } %>

            <asp:ListView runat="server" ID="uxChildSKU" OnDataBound="ShoppingCartOnDataBound" OnItemDataBound="ShoppingCartOnItemDataBound">
                <LayoutTemplate>
                    <table class="gdo-order-tbl childSKUTable" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <tbody>
                            <tr>
                                <th class="wdth50">
                                    <asp:Label runat="server" ID="lblSku" Text="SKU" meta:resourcekey="lblSkuResource1"></asp:Label>
                                </th>
                                <th class="wdth330">
                                    <asp:Label runat="server" ID="lbProdName" Text="Product Name" Style="text-align: left;
                                        padding-left: 10px" meta:resourcekey="lbProdNameResource1"></asp:Label>
                                </th>
                                <th class="wdth50">
                                    <asp:Label runat="server" ID="lbQty" Text="Qty" meta:resourcekey="lbQtyResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THRetailPrice" class="wdth50">
                                    <asp:Label runat="server" ID="lbRetail" Text="Retail<br />Price" meta:resourcekey="lbRetailResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THVolPoints" class="wdth60">
                                    <asp:Label runat="server" ID="lbVolumePoints" Text="Volume</br>Points" meta:resourcekey="lbVolumePointsResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THEarnBase" class="wdth60">
                                    <asp:Label runat="server" ID="lbEarnBase" Text="Earn</br>Base" meta:resourcekey="lbEarnBaseResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THYourPrice" class="wdth60">
                                    <asp:Label runat="server" ID="lbYourPrice" Text="Your<br />Price" meta:resourcekey="lbYourPriceResource1"></asp:Label>
                                </th>
                            </tr>
                            <%--/HEADER ROW--%>
                            <%--PRODUCT ROW--%>
                            <tr runat="server" id="itemPlaceholder"></tr>
                            <%--/PRODUCT ROW--%>
                        </tbody>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr id="Tr2" runat="server" class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "gdo-row-even gdo-order-tbl-data" : "gdo-row-odd gdo-order-tbl-data" %>'>
                        <td id="TdSKU" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left">
                                        <uc1:ProductAvailability ID="ProductAvailability2" runat="server" ShowLabel="false"
                                            Available='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) %>' />
                                    </td>
                                    <td class="gdo-order-tbl-data-left">
                                        <asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdDescription" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left">
                                        <%--<asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>--%>
                                        <asp:Label runat="server" ID="lbProductName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdQuantity" runat="server">
                            <p>
                                <asp:HiddenField runat="server" ID="uxProductoID" Value='<%# Eval("SKU") %>' />
                                <%--<asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />--%>
                                <%--<asp:TextBox MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="2" Style="margin-left: 0px; margin-right: 2px; text-align: right;"
                                    runat="server" ID="uxQuantity" Enabled='<%# (ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) != ProductAvailabilityType.Unavailable %>'
                                    Text='<%# Eval("Quantity") %>' CssClass="onlyNumbers"></asp:TextBox>--%>
                                <asp:Label ID="lblQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>' />
                            </p>
                        </td>
                        <td id="TdRetailPrice" runat="server" class="gdo-order-tbl-data-right">
                            <%--<p style="margin-right: 3px; text-align: right">
                                <%# GetLocalResourceObject("NotAvaibleText").ToString() %></p>--%>
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice")))%></p>
                        </td>
                        <td id="TdVolumePoint" runat="server" class="gdo-order-tbl-data-right">
                            <%--<p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourVolumePoints" Text='<%# GetLocalResourceObject("NotAvaibleText").ToString() %>' />
                                </span>
                            </p>--%>
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourVolumePoints" Text='<%#Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints")))%>' />
                                </span>
                            </p>
                        </td>
                        <td id="TdEarnBase" runat="server" class="gdo-order-tbl-data-right">                            
                            <%--<p style="margin-right: 3px; text-align: right">
                                <%# GetLocalResourceObject("NotAvaibleText").ToString() %>
                            </p>--%>
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("EarnBase", getAmountString((decimal)Eval("EarnBase")))%>
                            </p>
                        </td>
                        <td id="TdYourPrice" runat="server" class="gdo-order-tbl-data-right">
                            <%--<p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourPrice" Text='<%# GetLocalResourceObject("NotAvaibleText").ToString() %>' />
                                </span>
                            </p>--%>
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourPrice" Text='<%#Eval("DiscountPrice", getAmountString((decimal)Eval("DiscountPrice")))%>' />
                                </span>
                            </p>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>

            <asp:ListView runat="server" ID="uxChildSKUResponsive" OnDataBound="ShoppingCartOnDataBound"
                OnItemDataBound="ShoppingCartOnItemDataBound"
                EnableModelValidation="True"
                Visible="False">
                <LayoutTemplate>
                    <div class="gdo-order-tbl" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <div runat="server" id="itemPlaceholder">
                        </div>    
                    </div>
                </LayoutTemplate>
                <ItemTemplate>
                    <div class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "row product-info gdo-row-even" : "row product-info gdo-row-odd" %>'>
                        <div class="col-xs-9">
                            <div class="col-xs-12 col-Product">
                                <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>
                                <asp:Label runat="server" ID="lbProductName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                            </div>
                            <div class="col-xs-6 bold">
                                <div><asp:Label runat="server" ID="lblSku" Text="SKU" meta:resourcekey="lblSkuResource1"></asp:Label></div>
                                <div><asp:Label runat="server" ID="lbRetail" Text="Retail<br />Price" meta:resourcekey="lbRetailResource1"></asp:Label></div>
                                <div><asp:Label runat="server" ID="lbVolumePoints" Text="Volume</br>Points" meta:resourcekey="lbVolumePointsResource1"></asp:Label></div>
                            </div>
                            <div class="col-xs-4 align-right">
                                <div><asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label></div>
                                <div><asp:Label runat="server" Text='<%# Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice"))) %>' ID="idRetail"></asp:Label></div>
                                <div><asp:Label runat="server" Text='<%# Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints"))) %>' ID="idVolumePoints"></asp:Label></div>
                                <%--<div><%# GetLocalResourceObject("NotAvaibleText").ToString() %></div>
                                <div><asp:Label ID="VolumePoints" runat="server" Text='<%# GetLocalResourceObject("NotAvaibleText").ToString() %>'></asp:Label></div>--%>
                            </div>
                        </div>
                        <div class="col-xs-3">
                            <div class="align-right col-Avail" id="ctl00_ctl00_ContentArea_ProductsContent_Subcategories_ctl00_Products_ctl00_MyAvail1_divGreen">
                                <uc1:ProductAvailability ID="ProductAvailability2" runat="server" ShowLabel="false"
                                    Available='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) %>' />
                            </div>
                            <div class="center col-QTY">
                                <asp:Label runat="server" ID="Label1" Text="Qty" meta:resourcekey="lbQtyResource1" class="qty"></asp:Label>
                                <asp:HiddenField runat="server" ID="uxProductoID" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />
                                <asp:Label ID="lblQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>' />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:ListView>

            <%--<br/>
            <br/>--%>
            <div class="gdo-edit-header" id="divPromo" runat="server" Visible="False">
             <asp:Label runat="server" ID="lblPromoHeaderSummary" Text="Premiums Details" meta:resourcekey="lblPromoHeaderSummaryResource1"></asp:Label>
                </div>
            <%--<br/>--%>
             <asp:ListView runat="server" ID="uxPromo" OnDataBound="ShoppingCartOnDataBound"
                OnItemDataBound="ShoppingCartOnItemDataBound"
                EnableModelValidation="True">
                <LayoutTemplate>
                    <table class="gdo-order-tbl" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <tbody>
                            <tr>
                                <th>
                                    <asp:Label runat="server" ID="lblSku" Text="SKU" meta:resourcekey="lblSkuResource1"></asp:Label>
                                </th>
                                <th>
                                    <asp:Label runat="server" ID="lbProdName" Text="Product Name" Style="text-align: left;
                                        padding-left: 20px" meta:resourcekey="lbProdNameResource1"></asp:Label>
                                </th>
                                <th>
                                    <asp:Label runat="server" ID="lbQty" Text="Qty" meta:resourcekey="lbQtyResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THRetailPrice">
                                    <asp:Label runat="server" ID="lbRetail" Text="Retail<br />Price" meta:resourcekey="lbRetailResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THVolPoints">
                                    <asp:Label runat="server" ID="lbVolumePoints" Text="Volume</br>Points" meta:resourcekey="lbVolumePointsResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THEarnBase">
                                    <asp:Label runat="server" ID="lbEarnBase" Text="Earn</br>Base" meta:resourcekey="lbEarnBaseResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THYourPrice">
                                    <asp:Label runat="server" ID="lbYourPrice" Text="Your<br />Price" meta:resourcekey="lbYourPriceResource1"></asp:Label>
                                </th>
                                <th runat="server" id="THRemove">
                                    <asp:Label runat="server" ID="lbRemove" Text="Delete" meta:resourcekey="lbRemoveResource1"></asp:Label>
                                    <% if (!this.IsHelpIconHidden)
                                       {
                                    %>
                                    <div>
                                        <ajaxToolkit:HoverMenuExtender ID="HoverMenuExtenderDelete" runat="server" TargetControlID="imgDelete"
                                            PopupControlID="pnlDeleteText" />
                                        <a href="javascript:return false;" onclick="return false;">
                                            <img id="imgDelete" runat="server" src="/Content/Global/img/gdo/icons/question-mark-blue.png"
                                                alt="info" height="12" width="12" />
                                        </a>
                                        <asp:Panel ID="pnlDeleteText" runat="server" Style="display: none">
                                            <div class="gdo-popup">
                                                <asp:Label ID="lblDeleteHelpText" runat="server" meta:resourcekey="DeleteHelp">
                                                </asp:Label>
                                            </div>
                                        </asp:Panel>
                                    </div>
                                    <%
                                       }
                                    %>
                                </th>
                            </tr>
                            <%--/HEADER ROW--%>
                            <%--PRODUCT ROW--%>
                            <tr runat="server" id="itemPlaceholder">
                            </tr>
                            <%--/PRODUCT ROW--%>
                        </tbody>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                     <tr id="Tr1" runat="server" class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "gdo-row-even gdo-order-tbl-data" : "gdo-row-odd gdo-order-tbl-data" %>'>
                          <td id="TdSKU" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left"> &nbsp;&nbsp;&nbsp;&nbsp; </td>
                                    <td class="gdo-order-tbl-data-left">
                                        <asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idPromoSKU"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdDescription" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-order-tbl-data-left">
                                        <asp:LinkButton ID="LinkPromoDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>
                                        <asp:Label runat="server" ID="lbPromoName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td id="TdQuantity" runat="server">
                            <p>
                                <asp:HiddenField runat="server" ID="uxPromoID" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />
                                <asp:TextBox MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="2" Style="margin-left: 0px; margin-right: 2px; text-align: right;"
                                    runat="server" ID="uxPromoQuantity" Enabled='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) != MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType.Unavailable %>'
                                    Text='<%# Eval("Quantity") %>' CssClass="onlyNumbers"></asp:TextBox>
                                <asp:Label ID="lblPromoQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>'
                                    Visible="False" />
                            </p>
                        </td>
                        <td id="TdRetailPrice" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice")))%></p>
                        </td>
                        <td id="TdVolumePoint" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourVolumePoints" Text='<%#Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints")))%>' />
                                </span>
                            </p>
                        </td>
                        <td id="TdEarnBase" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <%#Eval("EarnBase", getAmountString((decimal)Eval("EarnBase")))%>
                            </p>
                        </td>
                        <td id="TdYourPrice" runat="server" class="gdo-order-tbl-data-right">
                            <p style="margin-right: 3px; text-align: right">
                                <span>
                                    <asp:Label runat="server" ID="YourPrice" Text='<%#Eval("DiscountPrice", getAmountString((decimal)Eval("DiscountPrice")))%>' />
                                </span>
                            </p>
                        </td>
                        <td id="TdDeleteCheckbox" class="gdo-order-tbl-data-center" runat="server">
                            <p>
                                &nbsp;
                                <asp:CheckBox runat="server" class="checkerbox" ID="uxPromoDelete" Enabled="true" />
                            </p>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
            
            <asp:ListView runat="server" ID="uxPromoResponsive" OnDataBound="ShoppingCartOnDataBound"
                OnItemDataBound="ShoppingCartOnItemDataBound"
                EnableModelValidation="True"
                Visible="False">
                <LayoutTemplate>
                    <div class="gdo-order-tbl" cellspacing="0" cellpadding="0" width="100%" border="0">
                        <%--HEADER ROW--%>
                        <div runat="server" id="itemPlaceholder">
                        </div>    
                    </div>
                </LayoutTemplate>
                <ItemTemplate>
                    <div class='<%# Convert.ToBoolean(Container.DataItemIndex % 2) ? "row product-info gdo-row-even myclear" : "row product-info gdo-row-odd myclear" %>'>
                        <div class="col-xs-9">
                            <div class="col-xs-12 col-Product">
                                <asp:LinkButton ID="LinkProductDetail" runat="server" OnClick="ProductDetailClicked"
                                            CommandArgument='<%# string.Format("{0} {1}", Eval("ProdInfo.ID"), Eval("ParentCat.ID") ) %>'
                                            Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:LinkButton>
                                <asp:Label runat="server" ID="lbProductName" Text='<%# string.Format("{0}", Eval("Description")) %>'></asp:Label>
                            </div>
                            <div class="col-xs-6 bold">
                                <div><span><%= (GetLocalResourceObject("lblSkuResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                                <div><span><%= (GetLocalResourceObject("lbRetailResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                                <div><span><%= (GetLocalResourceObject("lbVolumePointsResource1.Text") as string).Replace("<br />", " ").Replace("</br>", " ").Replace("<br>", " ") %></span></div>
                            </div>
                            <div class="col-xs-4 align-right">
                                <div><asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label></div>
                                <div><%#Eval("RetailPrice", getAmountString((decimal)Eval("RetailPrice")))%></div>
                                <div><asp:Label ID="VolumePoints" runat="server" Text='<%#Eval("VolumePoints", GetVolumeString((decimal)Eval("VolumePoints")))%>'></asp:Label></div>
                            </div>
                        </div>
                        <div class="col-xs-3">
                            <div class="align-right col-Avail" id="ctl00_ctl00_ContentArea_ProductsContent_Subcategories_ctl00_Products_ctl00_MyAvail1_divGreen">
                                <uc1:ProductAvailability ID="ProductAvailability2" runat="server" ShowLabel="false"
                                    Available='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) %>' />
                            </div>
                            <div class="center col-QTY">
                                <asp:Label runat="server" ID="Label1" Text="Qty" meta:resourcekey="lbQtyResource1" class="qty"></asp:Label>
                                <asp:HiddenField runat="server" ID="uxProductoID" Value='<%# Eval("SKU") %>' />
                                <asp:HiddenField runat="server" ID="uxIsPromo" Value='<%# Eval("IsPromo") %>' />
                                <asp:TextBox MaxLength='<%# getMaxLength((string)Eval("SKU")) %>' size="2" Style="margin-left: 0px; margin-right: 2px; text-align: right;"
                                    runat="server" ID="uxQuantity" Enabled='<%# (MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType)(this.getAvail(Container.DataItem as MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem)) != MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductAvailabilityType.Unavailable %>'
                                    Text='<%# Eval("Quantity") %>' CssClass="onlyNumbers"></asp:TextBox>
                                <asp:Label ID="lblQuantity" runat="server" MaxLength="3" Text='<%# Eval("Quantity") %>'
                                    Visible="False" />
                            </div>
                            <div id="TdDeleteCheckbox" runat="server" class='align-left'>
                                <asp:CheckBox runat="server" class="checkerbox" ID="uxPromoDelete" meta:resourcekey="lbRemoveResource1"/>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:ListView>
            


            <%--LEGEND--%>
            <table id="tblLegend" runat="server" class="gdo-order-tbl-legend" border="0" cellspacing="0">
                <tr runat="server">
                    <td runat="server">
                        <table class="gdo-order-tbl-bottom" border="0" cellspacing="0" cellpadding="0">
                            <tr>
                                <td>
                                    <div class="left">
                                        <p style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 3px 0px;
                                            padding-top: 0px" runat="server" id="pProdAvail">
                                            <uc1:ProductAvailability ID="ProductAvailability1" runat="server" />
                                        </p>
                                    </div>
                                    <div class="gdo-button-margin-rt orange-buttons">
                                        <div class="add-more-products">
                                            <cc1:DynamicButton ID="ContinueShopping" runat="server" ButtonType="Neutral" Text="Add More Products"
                                                OnClick="OnContinueShopping" meta:resourcekey="AddMoreProducts" name="ContinueShopping" />
                                        </div>
                                        <div class="recalculate">
                                            <cc1:DynamicButton ID="uxRecalculate" runat="server" ButtonType="Neutral" Text="Recalulate"
                                                OnClick="OnRecalculate" meta:resourcekey="Recalculate" name="recalculateButton"/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div id="cancelSaveCartBtn" class="left">
                                        <div runat="server" id="OrderButtons" class="gdo-button-margin-lt bttn-cancel">
                                            <cc1:DynamicButton ID="uxCancelOrder" runat="server" Text="Cancel Order" ButtonType="Back"
                                                meta:resourcekey="CancelOrder" name="uxCancelOrder" />
                                            <cc1:DynamicButton ID="uxReturnToCustomerOrder" runat="server" Visible="false" Text="To My Customer Orders Search"
                                                ButtonType="Forward" meta:resourcekey="CustomerOrdersSearchPageReturn" OnClick="ReturnToCustomerOrdersSearch" name="uxReturnToCustomerOrder" />
                                        </div>
                                        <div runat="server" id="divSaveCarts" visible="false" class="gdo-button-margin-rt bttn-recalculate">
                                            <asp:PlaceHolder ID="plSavedCartCommand" runat="server" />
                                        </div>
                                    </div>
                                    <div class="gdo-button-margin-rt bttn-addcart">
                                        <a href="" id="" onclick="DisableOnClick(this);">
                                            <cc1:DynamicButton ID="checkOutButton" runat="server" ButtonType="Forward" Text="Checkout"
                                                OnClick="OnCheckout" meta:resourcekey="Checkout" name="checkOutButton"/>
                                        </a>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
        <ajaxToolkit:ModalPopupExtender ID="mdlConfirm" runat="server" TargetControlID="MpeFakeTarget"
            PopupControlID="pnlConfirmUnavailable" CancelControlID="btnCloseModal" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <ajaxToolkit:ModalPopupExtender ID="mdlConfirmDelete" Enabled="false" runat="server" TargetControlID="uxCancelOrder"
            PopupControlID="pnlConfirmCancel" CancelControlID="btnNo" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <ajaxToolkit:ModalPopupExtender ID="mdlObservation" runat="server" TargetControlID="FakeTarget"
            PopupControlID="pnlContentObservation" CancelControlID="btnObservationOk" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <ajaxToolkit:ModalPopupExtender ID="displaySkuPopupModal" runat="server" TargetControlID="DiscSkuLabelListLinkButton"
            PopupControlID="pnlshowDiscSku" CancelControlID="ClosePopupButton" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <asp:Button ID="MpeFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Button ID="FakeTarget" runat="server" CausesValidation="False" Style="display: none" />
    </ContentTemplate>
</asp:UpdatePanel>
<p style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 0px;
    padding-top: 0px">
    &nbsp;</p>
<asp:Panel ID="pnlConfirmUnavailable" runat="server" Style="display: none">
    <progress:UpdatePanelProgressIndicator ID="progressConfirmUnavailable" runat="server"
        TargetControlID="updConfirmUnavailable" />
    <asp:UpdatePanel ID="updConfirmUnavailable" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-popup">
                <table width="510" cellspacing="5" cellpadding="0" border="0">
                    <tbody>
                        <tr>
                            <td>
                                <div class="gdo-float-left gdo-popup-title">
                                    <h2>
                                        <asp:Label ID="lblConfirmationUnavailable" runat="server" Text="Confirmation" meta:resourcekey="lblConfirmationUnavailable"></asp:Label></h2>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td class="gdo-form-label-left">
                                <p>
                                    <asp:Label ID="lblConfirmUnavailableText1" runat="server" Text="Changing your shipping address selection indicates these item(s) in your cart are now unavailable or backordered."
                                        meta:resourcekey="lblConfirmUnavailableText1"></asp:Label></p>
                                <asp:ListView runat="server" ID="uxProdToRemove" EnableModelValidation="True">
                                    <LayoutTemplate>
                                        <table cellspacing="1" cellpadding="0" width="100%" border="1" style="border-spacing: 1px">
                                            <%--HEADER ROW--%>
                                            <tbody>
                                                <tr>
                                                    <td colspan="2">
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="itemPlaceholder">
                                                </tr>
                                            </tbody>
                                        </table>
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td class="red">
                                                <asp:Label runat="server" Text='SKU: ' ID="Label3"></asp:Label>&nbsp
                                                <asp:Label runat="server" Text='<%# Eval("SKU") %>' ID="idSKU"></asp:Label>&nbsp;<%# string.Format("{0}-{1}", Eval("Description"), Eval("Flavor"))%></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table id="changeshippinginstructions">
                                    <tr>
                                        <td class="instructionsleft gdo-form-label-lef">
                                            <asp:Label ID="lblConfirmGoBack" runat="server" Text="If you click Go Back, your previous shipping address will be displayed."
                                                meta:resourcekey="lblConfirmGoBack"></asp:Label>
                                        </td>
                                        <td class="instructionsright gdo-form-label-lef">
                                            <asp:Label ID="lblConfirmRemoveItems" runat="server" Text="If you click Remove, the unavailable item(s) will be displayed in red and the available for backorder item will be displayed in yellow."
                                                meta:resourcekey="lblConfirmRemoveItems"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table id="Table1" class="confirmcancelbuttons">
                                    <tr>
                                        <td>
                                            <div align="right" style="width: 100%" class="ConfirmBackOrderButton">
                                                <cc1:DynamicButton ID="btnCloseModal" runat="server" ButtonType="Back" Text="Go Back"
                                                    meta:resourcekey="GoBack" />
                                                <cc1:DynamicButton ID="btnConfirmDelete" runat="server" ButtonType="Forward" Text="Remove"
                                                    OnClick="OnSaveClicked" meta:resourcekey="Remove" />
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<asp:Panel ID="pnlConfirmCancel" runat="server" Style="display: none">
    <div class="gdo-popup confirmCancel">
        <div class="gdo-float-left gdo-popup-title">
            <h2>
                <asp:Label ID="lblConfirmCancel" runat="server" Text="Confirmation" meta:resourcekey="lblConfirmCancel"></asp:Label></h2>
        </div>
        <div class="gdo-form-label-left">
            <asp:Label ID="lblConfirmCancelText" runat="server" Text="Are you sure you wish to cancel ?"
                meta:resourcekey="lblConfirmCancelText"></asp:Label>
        </div>
        <div class="gdo-form-label-left confirmButtons">
            <cc1:DynamicButton ID="btnYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="btnYesCancel_Click"
                meta:resourcekey="Yes" />
            <cc1:DynamicButton ID="btnNo" runat="server" ButtonType="Back" Text="No" meta:resourcekey="No" />
        </div>
    </div>
</asp:Panel>
<asp:panel ID="pnlContentObservation" runat="server">
<asp:UpdatePanel ID="pnlObservation" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
        <div class="gdo-popup confirmCancel">
            <div class="gdo-float-left gdo-popup-title">
                <h2>
                    <asp:Label ID="lblObservationTitle" runat="server" Text="Please note..." meta:resourcekey="lblObservationTitle"/>
                </h2>
            </div>
            <div class="gdo-form-label-left">
                <asp:Label ID="lblObservationText" runat="server" Text=""/>
            </div>
            <div class="gdo-form-label-left confirmButtons">
                <cc1:DynamicButton ID="btnObservationOk" runat="server" ButtonType="Forward" Text="Ok" meta:resourcekey="btnObservationOk" />
            </div>
        </div>        
    </ContentTemplate>
</asp:UpdatePanel>
</asp:panel>
<asp:Panel ID="pnlshowDiscSku" runat="server"  Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="DiscSkuLabelList" runat="server" ></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="Label6" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="ClosePopupButton" runat="server" CausesValidation="False" ButtonType="Forward" Text="Ok" OnClick="OnDisplayDiscontinueSkuClose" meta:resourcekey="btnObservationOk" />
                    </div>
                </div>
            </asp:Panel>
<asp:UpdatePanel ID="UpdateAddressRestrictionPopUp" runat="server" UpdateMode="Always">
    <ContentTemplate>
       <ajaxToolkit:ModalPopupExtender ID="AddressRestrictionConfirmPopupExtender" runat="server" TargetControlID="AddressRestrictionConfirmFakeTarget"
            PopupControlID="pnldupeAddressRestrictionConfirm" CancelControlID="AddressRestrictionConfirmFakeTarget" BackgroundCssClass="modalBackground" 
            DropShadow="false" />
             <asp:Button ID="AddressRestrictionConfirmFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Panel ID="pnldupeAddressRestrictionConfirm" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblRemindltr" runat="server" Text="Please Confirm that Order is Being Shipped to personal Address" meta:resourcekey="lblRemindltr" ></asp:Label>

                </div>
                <div class="gdo-form-label-left confirmButtons">
                    <cc1:DynamicButton ID="ProcedToChcekOut" runat="server"   ButtonType="Forword"  Text="Yes" OnClick="ProcedToChcekOut_Click" meta:resourcekey="btnProcedToChcekOut"/>
                    <cc1:DynamicButton ID="CancelToProced" runat="server"  ButtonType="Forword" Text="NO" OnClick="CancelToProced_Click" meta:resourcekey="btnCancelToProced"/>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
<ContentTemplate>
       <ajaxToolkit:ModalPopupExtender ID="AutoSelectShippingaddresPopupExtender" runat="server" TargetControlID="AutoSelectShippingaddresFakeTarget"
            PopupControlID="pnldupeAutoSelectShippingaddres" CancelControlID="AutoSelectShippingaddresFakeTarget" BackgroundCssClass="modalBackground" 
            DropShadow="false" />
             <div>
             <asp:Button ID="AutoSelectShippingaddresFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
             <asp:Panel ID="pnldupeAutoSelectShippingaddres" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblMessage"  Font-Size="Medium" runat="server" Text="Tu pedido será enviado a la siguiente dirección:"  ></asp:Label>
                </div>
                <p runat="server" visible="true" id="AutoSelectShippingaddres"></p>
                <br />
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblMessage2" Font-Size="Medium"  runat="server" Text="Si no es correcta, haz click en Editar bajo el área donde se muestra tu dirección de envío en la parte superior de la pantalla."  ></asp:Label>
                </div>
                <br />
                <div class="gdo-form-label-left confirmButtons">
                     <cc1:DynamicButton ID="btnCancel" runat="server" Text="Cancel" ButtonType="Back" OnClick="btnCancel_Click"    meta:resourcekey="btnCancel"/>
                     <cc1:DynamicButton ID="btnOK" runat="server" Text="OK" ButtonType="Forword" OnClick="btnOK_Click"   meta:resourcekey="btnOK"/>
                </div>
                
            </div>
        </asp:Panel>
          </div>
    </ContentTemplate>
    </asp:UpdatePanel>
<PromoMY:Promotion_MY runat="server" id="Promotion_MY_Control" />
