<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="OrderBySKU.aspx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.OrderBySKU" %>

<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="PCMsgBox" TagName="PCMsgBox" Src="~/Ordering/Controls/MessageBoxPC.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <link href="CSS/TempOrderBySKU.css" rel="stylesheet" />
    <script type="text/javascript">
        // Legacy javascript code.
        function DisableAddtoCart() {
            $('#<%= CheckoutButton1.ClientID %>').css('display', 'none');
            $('#<%= CheckoutButton2.ClientID %>').css('display', 'none');

            $('#<%= CheckoutButton1Disabled.ClientID %>').css('display', 'inline-block');
            $('#<%= CheckoutButton2Disabled.ClientID %>').css('display', 'inline-block');
        }

        // Resx values.
        var noSKUFoundText = '<%= GetLocalResourceObject("NoSKUFound").ToString() %>';
        var autoPlaceHolderText = '<%= GetLocalResourceObject("AutoPlaceHolder").ToString() %>';
    </script>
    <script src="/Ordering/Scripts/ProductBySKU.js" type="text/javascript"></script>
    <script src="/Ordering/Scripts/jquery.ajaxPost.js" type="text/javascript"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:UpdatePanel ID="upProductSku" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="OrderBySkuMain">
                <div id="divChinaPCMessageBox" style="margin: 5px;" runat="server">
                    <PCMsgBox:PCMsgBox runat="server" ID="PcMsgBox1" DisplaySubmitButton="True" ></PCMsgBox:PCMsgBox>
                </div>
                <div class="gdo-spacer3">
                </div>
                <div class="tab">
                    <asp:Label CssClass="header" runat="server" ID="TopTab" Text="Commander par Référence"></asp:Label>
                </div>
                <div id="OrderBySkuContainer">
                    <div class="gdo-pricelist-label">
                        <asp:Label runat="server" ID="TxtStep1" Text="Step 1: " meta:resourcekey="TxtStep1Resource1"></asp:Label>
                        <span class="gdo-body-text">
                            <asp:Label runat="server" ID="TxtInstr1" Text="Enter the item by its SKU number."
                                meta:resourcekey="TxtInstr1Resource1"></asp:Label>
                        </span>
                    </div>
                    <div class="gdo-pricelist-label">
                        <asp:Label runat="server" ID="TxtStep2" Text="Step 2: " meta:resourcekey="TxtStep2Resource1"></asp:Label>
                        <span class="gdo-body-text">
                            <asp:Label runat="server" ID="TxtInstr2" Text="For each product you wish to order, enter the desired quantity."
                                meta:resourcekey="TxtInstr2Resource1"></asp:Label>
                        </span>
                    </div>
                    <div class="gdo-pricelist-label">
                        <asp:Label runat="server" ID="TxtStep3" Text="Step 3: " meta:resourcekey="TxtStep3Resource1"></asp:Label>
                        <span class="gdo-body-text">
                            <asp:Label runat="server" ID="TxtInstr3" Text="Once you have finished selecting the products you want, click the Add to Cart button."
                                meta:resourcekey="TxtInstr3Resource1"></asp:Label>
                        </span>
                    </div>
                    <div class="spacer"></div>
                    <div class="gdo-order-by-sku-header">
                        <asp:Label ID="Label1" runat="server" Text="Enter SKUs" meta:resourcekey="EnterSKUs"></asp:Label>
                    </div>
                    <div class="gdo-pricelist-horiz-rule"></div>
                    <div class="gdo-error-message-div gdo-error-message-txt errorList">
                        <asp:BulletedList runat="server" ID="uxErrores" CssClass="gdo-error-message-txt" BulletStyle="Circle"
                            meta:resourcekey="uxErroresResource1">
                        </asp:BulletedList>
                    </div>
                    <div class="addButtonContainer">
                        <cc1:DynamicButton ID="CheckoutButton1" runat="server" OnClientClick="DisableAddtoCart();"
                            OnClick="AddToCart" ButtonType="Forward" Text="Add to Cart" />
                        <cc1:DynamicButton ID="CheckoutButton1Disabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                            Disabled="true" Hidden="true" />
                    </div>
                    <div>
                        <cc2:ContentReader ID="OrderSKUMessage" runat="server" ContentPath="orderSkuMessage.html"
                        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                    </div>
                    <div id="skuControlsContainer">
                        <table runat="server" id="tblSKU" class="tblSKU" cellspacing="0" cellpadding="0">
                            <tr>
                                <td>
                                    <table cellspacing="0" cellpadding="0" border="0">
                                        <tbody>
                                            <tr>
                                                <th width="10%">&nbsp;
                                                </th>
                                                <th width="80%">
                                                    <asp:Label runat="server" ID="lbSKU1" Text="Product" meta:resourcekey="lbSKU1Resource1"></asp:Label>
                                                </th>
                                                <th width="10%">
                                                    <asp:Label runat="server" ID="lbQty" Text="Quantity" meta:resourcekey="lbQtyResource1"></asp:Label>
                                                </th>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError1"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError1Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox1" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox1Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox1" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox1Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError2"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError11Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox2" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox11Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox2" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox11Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError3"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError2Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox3" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox2Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox3" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox2Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td valign="middle" align="center">&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError4"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError12Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox4" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox12Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox4" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox12Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError5"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError3Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox5" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox3Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox5" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox3Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td valign="middle" align="center">&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError6"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError13Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox6" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox13Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox6" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox13Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError7"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError4Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox7" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox4Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox7" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox4Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td valign="middle" align="center">&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError8"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError14Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox8" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox14Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox8" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox14Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError9"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError5Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox9" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox5Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox9" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox5Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td valign="middle" align="center">&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError10"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError15Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox10" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox15Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox10" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox15Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr class="gdo-order-by-sku-tbl-data hide">
                                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError11"
                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError6Resource1" />
                                                </div>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox11" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox6Resource1"></asp:TextBox>
                                                </td>
                                                <td align="center">
                                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox11" class="qtyInput gdo-order-sku-input"
                                                        meta:resourcekey="QuantityBox6Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError12"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError7Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox12" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox7Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox12" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox7Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError13"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError7Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox13" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox7Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox13" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox7Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                                <td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError14"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError17Resource1" />
                                </div>
                                </td>
										        <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox14" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox17Resource1"></asp:TextBox>
                                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox14" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox17Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError15"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError8Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox15" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox8Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox15" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox8Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                                <td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError16"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError18Resource1" />
                                </div>
                                </td>
										        <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox16" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox18Resource1"></asp:TextBox>
                                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox16" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox18Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError17"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError9Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox17" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox9Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox17" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox9Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                                <td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError18"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError19Resource1" />
                                </div>
                                </td>
										        <td align="center">
                                                    <asp:TextBox runat="server" size="4" ID="SKUBox18" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox19Resource1"></asp:TextBox>
                                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox18" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox19Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError19"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError10Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox19" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox10Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox19" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox10Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                            <tr class="gdo-order-by-sku-tbl-data hide">
                                <td>&nbsp;<div class="gdo-error-message-div gdo-show">
                                    <asp:Image class="gdo-error-message-icon hide" runat="server" ID="imgError20"
                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError20Resource1" />
                                </div>
                                </td>
                                <td align="center">
                                    <asp:TextBox runat="server" size="4" ID="SKUBox20" class="productInput gdo-order-sku-input" Style="text-transform: uppercase" meta:resourcekey="SKUBox20Resource1"></asp:TextBox>
                                </td>
                                <td align="center">
                                    <asp:TextBox MaxLength="4" size="9" runat="server" ID="QuantityBox20" class="qtyInput gdo-order-sku-input"
                                        meta:resourcekey="QuantityBox20Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="addButtonContainer">
                        <cc1:DynamicButton ID="CheckoutButton2" runat="server" OnClientClick="DisableAddtoCart();" ButtonType="Forward"
                            OnClick="AddToCart" Text="Add to Cart" />
                        <cc1:DynamicButton ID="CheckoutButton2Disabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                            Disabled="true" Hidden="true" />
                    </div>
                    <div id="footer">
                        <asp:Label runat="server" ID="lbCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />Saturday 6 a.m. to 2 p.m. PST"
                            meta:resourcekey="lbCustomerSupportResource1"></asp:Label>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
