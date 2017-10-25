<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="ProductSKU.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.ProductSKU"
    meta:resourcekey="PageResource1" EnableEventValidation="false" %>
<%@ Register TagPrefix="CnChkout24h" TagName="CnChkout24h" Src="~/Ordering/Controls/CnChkout24h.ascx" %>
<%@ Register Src="~/Ordering/Controls/MessageBoxPC.ascx" TagName="PCMsgBox" TagPrefix="PCMsgBox" %>
<%@ Register Src="~/Ordering/Controls/Promotion_MY.ascx" TagPrefix="PromoMY" TagName="Promotion_MY" %>
<%@ Register Src="~/Ordering/Controls/ExpireDatePopUp.ascx" TagPrefix="ExpirePopUp" TagName="ExpireDatePopUp" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register Src="~/Ordering/Controls/APFDueReminderPopUp.ascx" TagPrefix="APFReminderPopUp" TagName="APFDuePopUp" %>
<%@ Register Src="~/Ordering/Controls/AddressRestrictionPopUp.ascx" TagPrefix="AdrsPopUp" TagName="AddressResPopUP" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
    <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />

    <script language="javascript" type="text/javascript">
        function DisableAddtoCart() {
            $('#<%= CheckoutButton1.ClientID %>').css('display', 'none');
            $('#<%= CheckoutButton2.ClientID %>').css('display', 'none');

            $('#<%= CheckoutButton1Disabled.ClientID %>').css('display', 'inline-block');
            $('#<%= CheckoutButton2Disabled.ClientID %>').css('display', 'inline-block');

            // === Callback when ends ajax request  ===
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(end_request);

            function end_request() {
                <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) { %>
                    $("#TotalItems").text($('[id$="TotalQty"]').text());
                <% } %>

                Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(end_request);
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
<asp:UpdatePanel ID="upProductSku" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
    <div class="product-sku-wrapper">
        <div id="divChinaPCMessageBox" style="margin: 5px;" runat="server">
            <PCMsgBox:PCMsgBox runat="server" ID="PcMsgBox1" DisplaySubmitButton="True"></PCMsgBox:PCMsgBox>
        </div>
         <div class="gdo-spacer3">
         </div>
        <div class="alertNoCC" id="alertNoCC" runat="server" visible="false">
            <asp:Label runat="server" ID="lblCraditcard" Text=""></asp:Label>
            <asp:Image ID="imgWarning" runat="server" Visible="false" />
            <asp:HyperLink runat="server" ID="lnkSavedCards" NavigateUrl="OrderPreferences.aspx" Text=""></asp:HyperLink>
        </div>
        <div class="tabs-button-wrap">
            <div class="inline tab-blu-long">
                <asp:Label class="header" runat="server" ID="TopTab" Text="Commander par Référence"></asp:Label>
            </div>
            <div class="inline tab-blu-bottomline">
                &nbsp;
            </div>
            <div class="clear">
            </div>
        </div>

        <table width="100%" cellspacing="0" cellpadding="0" border="0" class="gdo-order-by-sku-tbl">
            <%--<tr>
           <td>
               <div class="alertNoCC">
                   <asp:Label runat="server" ID="lblCraditcard" Text=""></asp:Label>
                   <asp:HyperLink runat="server" ID="lnkSavedCards" NavigateUrl="OrderPreferences.aspx" Text=""></asp:HyperLink>
               </div>
           </td> 
        </tr>--%>
            <tr>
                <td valign="top">
                    <table class="gdo-pricelist-tbl-top" border="0" cellspacing="0" cellpadding="0">
                        <tbody>
                            <tr>
                                <td class="instruction-labels">
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
                                    <div class="gdo-spacer1">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div class="gdo-order-by-sku-header">
                                        <asp:Label runat="server" Text="Enter SKUs" meta:resourcekey="EnterSKUs"></asp:Label></div>
                                    <div class="gdo-pricelist-horiz-rule">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td valign="bottom">
                                    <div class="gdo-error-message-div">
                                        <asp:BulletedList runat="server" ID="uxErrores" class="gdo-error-message-txt" BulletStyle="Circle"
                                            meta:resourcekey="uxErroresResource1">
                                        </asp:BulletedList>
                                    </div>
                                    <div class="gdo-error-message-div backorderErrorMsg">
                                        
                                        <asp:Label ID="labelBackOrderMessage" runat="server" Text="" class="gdo-error-message-txt"  />
                                    </div>
                                    </div>
                                    <div class="align-right">
                                        <cc1:DynamicButton ID="CheckoutButton1" runat="server" OnClientClick="DisableAddtoCart();"
                                            OnClick="AddToCart" ButtonType="Forward" Text="Add to Cart" name="addCart_1"  />
                                        <cc1:DynamicButton ID="CheckoutButton1Disabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                                            Disabled="true" Hidden="true" />
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <cc2:ContentReader ID="OrderSKUMessage" runat="server" ContentPath="orderSkuMessage.html"
                                    SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div class="clear15">
                                    </div>
                                    <table runat="server" id="tblSKU" style="width: 100%" cellspacing="0" cellpadding="0"
                                        border="0" class="gdo-sku-quantity-tbl">
                                        <tr>
                                            <td>
                                                <table cellspacing="0" cellpadding="0" border="0" class="table-sku">
                                                    <tbody>
                                                        <tr>
                                                            <th width="30">
                                                                &nbsp;
                                                            </th>
                                                            <th>
                                                                <asp:Label runat="server" ID="lbSKU1" Text="SKU" meta:resourcekey="lbSKU1Resource1"></asp:Label>
                                                            </th>
                                                            <th>
                                                                <asp:Label runat="server" ID="lbQty" Text="Quantity" meta:resourcekey="lbQtyResource1"></asp:Label>
                                                            </th>
                                                            <th width="30" class="hide-xs">
                                                                &nbsp;
                                                            </th>
                                                            <th class="hide-xs">
                                                                <asp:Label runat="server" ID="lbSKU2" Text="SKU" meta:resourcekey="lbSKU2Resource1"></asp:Label>
                                                            </th>
                                                            <th class="hide-xs">
                                                                <asp:Label runat="server" ID="lbQuantity2" Text="Quantity" meta:resourcekey="lbQuantity2Resource1"></asp:Label>
                                                            </th>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError1"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError1Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox1" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox1Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox size="9" runat="server" ID="QuantityBox1" MaxLength="3"
                                                                    meta:resourcekey="QuantityBox1Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError11"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError11Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox11" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox11Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3" 
                                                                    size="9" runat="server" ID="QuantityBox11" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox11Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError2"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError2Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox2" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox2Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                     size="9" runat="server" ID="QuantityBox2" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox2Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td valign="middle" align="center" class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError12"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError12Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox12" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox12Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox12" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox12Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError3"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError3Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox3" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox3Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox3" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox3Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td valign="middle" align="center" class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError13"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError13Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox13" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox13Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox13" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox13Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError4"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError4Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox4" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox4Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox4" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox4Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td valign="middle" align="center" class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError14"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError14Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox14" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox14Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox14" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox14Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError5"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError5Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox5" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox5Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox5" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox5Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td valign="middle" align="center" class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError15"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError15Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox15" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox15Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox15" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox15Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError6"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError6Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox6" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox6Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox6" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox6Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError16"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError16Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox16" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox16Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox16" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox16Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError7"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError7Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox7" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox7Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox7" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox7Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError17"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError17Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox17" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox17Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox17" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox17Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError8"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError8Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox8" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox8Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox8" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox8Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError18"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError18Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox18" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox18Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox18" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox18Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError9"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError9Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox9" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox9Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox9" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox9Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError19"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError19Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox19" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox19Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox19" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox19Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr class="gdo-order-by-sku-tbl-data">
                                                            <td>
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError10"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError10Resource1" /></div>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox10" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox10Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox10" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox10Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                            <td class="hide-xs">
                                                                &nbsp;<div class="gdo-error-message-div gdo-show">
                                                                    <asp:Image class="gdo-error-message-icon" Visible="False" runat="server" ID="imgError20"
                                                                        ImageUrl="/Content/Global/Controls/LogonBox/img/icon_error.gif" meta:resourcekey="imgError20Resource1" /></div>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox runat="server" size="4" ID="SKUBox20" class="gdo-order-sku-input" style="text-transform: uppercase" meta:resourcekey="SKUBox20Resource1"></asp:TextBox>
                                                            </td>
                                                            <td align="center" class="hide-xs">
                                                                <asp:TextBox MaxLength="3"
                                                                    size="9" runat="server" ID="QuantityBox20" class="gdo-order-sku-input"
                                                                    meta:resourcekey="QuantityBox20Resource1" CssClass="onlyNumbers"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                    <div class="clear10">
                                    </div>
                                    <div class="clear15">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td valign="bottom">
                                    <div class="align-right">
                                        <cc1:DynamicButton ID="CheckoutButton2" runat="server" OnClientClick="DisableAddtoCart();" ButtonType="Forward"
                                            OnClick="AddToCart" Text="Add to Cart" name="addCart_2" />
                                        <cc1:DynamicButton ID="CheckoutButton2Disabled" runat="server" ButtonType="Forward" Text="Add to Cart"
                                            Disabled="true" Hidden="true" />
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <br />
                    <div align="center">
                        <asp:Label runat="server" ID="lbCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />Saturday 6 a.m. to 2 p.m. PST"
                            meta:resourcekey="lbCustomerSupportResource1"></asp:Label>
                        <br />
                        <br />
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div>
        <cc2:ContentReader ID="productSkuLogos" runat="server" ContentPath="paymentLogos.html"
            SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
    <script type="text/javascript" language="javascript">
        function checkSKUChar(e, ctrl)
        {
            var keynum;
            var keychar;

            if (!e) var e = window.event
            if (e.keyCode) keynum = e.keyCode;
            else if (e.which) keynum = e.which;

            keychar = String.fromCharCode(keynum);

            if (!/^ *[0-9a-zA-Z]+ *$/.test(keychar))
            {
                e.cancelBubble = true;
                if (e.keyCode) // IE
                {
                    e.keyCode = 2;
                }
                else if (e.which)
                {
                    if (e.preventDefault) e.preventDefault();
                    if (e.stopPropagation) e.stopPropagation();

                }
            }
        }
    </script>
    </ContentTemplate>
</asp:UpdatePanel>
     <asp:UpdatePanel ID="UpdatePanelDupeOrder" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="dupeOrderPopupExtender" runat="server" TargetControlID="DupeOrderFakeTarget"
                PopupControlID="pnldupeOrderMonth" CancelControlID="DupeOrderFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="DupeOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupeOrderMonth" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Recent Order" meta:resourcekey="lblDupeOrder"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server" Text="You recently placed an order, verifiying the orders status. Otherwise, please click Cancel Order in MyOrder Page."></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>

    </asp:UpdatePanel>
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
                PopupControlID="pnldupeSlowMovingSkuTarget" CancelControlID="DynamicButton1" BackgroundCssClass="modalBackground"
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
                        <cc1:DynamicButton ID="DynamicButton1" runat="server" ButtonType="Forward" Text="OK" OnClick="HideSlowMovingMsg" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel> 

    <PromoMY:Promotion_MY runat="server" id="Promotion_MY" />
    <ExpirePopUp:ExpireDatePopUp runat="server" id="ExpireDatePopUp1" />
    <CnChkout24h:CnChkout24h runat="server" ID="CnChkout24h" />
    <APFReminderPopUp:APFDuePopUp runat="server" id="APFDuermndrPopUp" />
     <AdrsPopUp:AddressResPopUP runat="server" ID="AddressResPopUP1" />
</asp:Content>

