<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="PriceListGenerator.aspx.cs" meta:resourcekey="PageResource1" Inherits="MyHerbalife3.Ordering.Web.Ordering.PriceListGenerator" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Import Namespace="MyHerbalife3.Shared.ViewModel" %>
<asp:Content ID="PriceListGeneratorHeader" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript">
        var ProductListGridID = "<%= ProductListGrid.ClientID %>";
        var LoadingPanelClientID = "<%= RadAjaxLoadingPanel.ClientID %>";
        var currentCategory = '';
        var currentSalesTax = '';
        var currentShippingAndHandling = '';
        var currentDistributorCost = '';
        var currentCustomerDiscount = '';
        var isMultipleRate = '';
        var invalidInputMessage = "<%= GetLocalResourceObject("InvalidInputText") as string %>";
        var invalidAddressMessage = "<%= GetLocalResourceObject("InvalidAddresstText") as string %>";
        var defaultCityText = "<%= GetLocalResourceObject("DefaultCityText") as string %>";
        var defaultStreetAddress = "<%= GetLocalResourceObject("DefaultStreetAddressText") as string %>";
        var defaultZipCodeText = "<%= GetLocalResourceObject("DefaultZipCodeText") as string %>";
        var defaultStateOption = "<%= GetLocalResourceObject("DefaultStateOption") as string %>";
        var btnExportId = "<%= btnExport.ClientID %>";
        var btnExportDisabledId = "<%= btnExportDisabled.ClientID %>";
        var btnExportXLSId = "<%= btnExportExcel.ClientID %>";
        var btnExportXLSDisableId = "<%= btnExportExcelDisabled.ClientID %>";
        var priceListUpdatedText = "<%= GetLocalResourceObject("PriceListUpdatedText") as string %>";
        var priceListErrorText = "<%= GetLocalResourceObject("PriceListErrorText") as string %>";
        var singleTaxRateText = "<%= GetLocalResourceObject("SingleTaxMessage.Text") as string %>";
        var multipleTaxRateText = "<%= GetLocalResourceObject("MultipleTaxMessage.Text") as string %>";
        
        var localePLGVid = "<%= System.Threading.Thread.CurrentThread.CurrentCulture.Name%>";
        $(document).ready(function () {
           
            if (localePLGVid == 'es-US' || localePLGVid == 'en-US' || localePLGVid == 'fr-CA' || localePLGVid == 'en-CA') {
                var PLGVidVar;
                var PLGModalTitle;
                var PLGBtnText;

                if (localePLGVid == 'es-US') {
                    PLGVidVar = 'v48t5e80';
                    PLGModalTitle = 'Resumen Generador de Lista de Precio';
                    PLGBtnText = 'Ver Video';
                } else if (localePLGVid == 'fr-CA') {
                    PLGVidVar = 'l03076wi';
                    PLGModalTitle = 'Prix ​​Générateur de liste Vue générale';
                    PLGBtnText = 'Regarder vidéo';
                }else {
                    PLGVidVar = 'l03076wi';
                    PLGModalTitle = 'Price List Generator Overview';
                    PLGBtnText = 'Watch Video';
                }
                $('#PLGVidBtn').show().attr('data-asset_id', PLGVidVar).text(PLGBtnText);
            } 
            
        });
    </script>
    <script type="text/javascript" src="/Ordering/Scripts/PriceListGenerator.js"></script>
</asp:Content>
<asp:Content ID="PriceListGeneratorContent" ContentPlaceHolderID="ProductsContent" runat="server">
    <span id="TopMessage" runat="server" meta:resourcekey="TopMessage">This tool calculates the total price to charge customers, including tax, shipping and handling, and discounts.
    </span>
    <br />
    <% if (GlobalContext.CurrentExperience.ExperienceType != MyHerbalife3.Shared.ViewModel.ValueObjects.ExperienceType.Green){ %>
        <a href="#" id="PLGVidBtn" data-player_id="q68p3258" data-width="640" data-height="320" class="backward modalEmbed"></a>
    <% } %>
    

    <p id="errorMessageList" class="gdo-error-message-txt"></p>
    <!-- PLG STEPS -->
    <div id="PriceListGeneratorInputs">
        <div class="fourColumns">
            <div>
                <div>
                    <p>
                        <span>1</span>
                        <span id="SalesTaxTitle" runat="server" meta:resourcekey="SalesTaxTitle">Sales Tax</span>
                    </p>
                    <div id="Flag" class="hide">
                        <asp:Label ID="multipleFlag" runat="server" Text="MULTIPLE" style="color:red; font-weight:bold;" meta:resourcekey="MultipleFlag"/>    
                        <br/>
                    </div>
                    <telerik:RadNumericTextBox runat="server" MaxLength="6" ID="txtSalesTax" MaxValue="100" MinValue="0"
                        CssClass="txtSalesTax" Value="00.00" Skin="Hay" NumberFormat="">
                    </telerik:RadNumericTextBox>
                    %
                    <input type="hidden" id="txtSalesTaxCalculated" class="txtSalesTaxCalculated" runat="server" value="0" />
                    <div id="TaxControls" class="hide">
                        <span id="arrowTip"></span>
                        <asp:Label runat="server" ID="lblErrorMsgTaxlookup1" CssClass="lblErrorMsgTaxlookup1" meta:resourcekey="lblErrorMsgTaxlookup1"></asp:Label>
                        <asp:Label runat="server" ID="lblErrorMsgTaxlookup2" CssClass="lblErrorMsgTaxlookup2 hide" meta:resourcekey="lblErrorMsgTaxlookup2"></asp:Label>
                        <asp:TextBox runat="server" ID="txtStreetAddress" CssClass="txtStreetAddress"
                            MaxLength="40"></asp:TextBox>
                        <asp:TextBox runat="server" ID="txtTaxCity" CssClass="txtTaxCity"
                            MaxLength="40"></asp:TextBox>
                        <asp:DropDownList runat="server" ID="ddlTaxState" CssClass="txtTaxState" skin="Telerik" />
                        <asp:TextBox runat="server" ID="txtTaxZipCode" CssClass="txtTaxZipCode" Width="60"></asp:TextBox>

                        <cc1:DynamicButton ID="btnFixdTax" ButtonType="Forward" IconPosition="Right" IconType="ArrowRight" Text=">" runat="server" />
                    </div>
                    <div>
                        <cc1:DynamicButton ID="btnShowFindTax" ButtonType="Link" IconPosition="Right" IconType="ArrowLeft"
                            runat="server" Text="Look Up tax" meta:resourcekey="FindTaxButton" />
                    </div>
                </div>
            </div>
            <div>
                <div>
                    <p>
                        <span>2</span>
                        <span id="ShippingAndHandlingTitle" runat="server" meta:resourcekey="ShippingAndHandlingTitle">Shipping & Handling</span>
                    </p>
                    <telerik:RadNumericTextBox runat="server" MaxLength="6" ID="txtShippingAndHandling" MaxValue="100" MinValue="0"
                        CssClass="txtShippingAndHandling" Value="00.00" Skin="Hay">
                    </telerik:RadNumericTextBox>
                    %
                <div>
                    <a class="modalBtn" title="estimateModal" runat="server" id="SHLookup">
                        <asp:Label ID="lblSH" runat="server" Text="Lookup S&H" meta:resourcekey="EstimateButton"/>
                    </a>
                </div>
                </div>
            </div>
            <div>
                <div>
                    <p>
                        <span>3</span>
                        <span id="DistributorCostTitle" runat="server" meta:resourcekey="DistributorCostTitle">Discount Percentage</span>
                    </p>
                    <telerik:RadComboBox runat="server" ID="ddlDistributorCost" Width="52"
                        CssClass="txtDistributorCost" Skin="Telerik" />
                    %            
                </div>
            </div>
            <div>
                <div>
                    <p>
                        <span>4</span>
                        <span id="CustomerDiscountTitle" runat="server" meta:resourcekey="CustomerDiscountTitle">Customer Discount</span>
                    </p>
                    <telerik:RadNumericTextBox runat="server" MaxLength="6" ID="txtCustomerDiscount" MaxValue="100" MinValue="0"
                        CssClass="txtCustomerDiscount" Value="00.00" Skin="Hay">
                    </telerik:RadNumericTextBox>
                    % 
                </div>
            </div>
        </div>
        <asp:CheckBox runat="server" ID="cbCustomerPrice" CssClass="cbCustomerPrice" Text="Show ONLY Customer Price Column"
            meta:resourcekey="OnlyCustomerPrice" /><br />
        <cc1:DynamicButton ID="btnRecalculate" ButtonType="Neutral" CssClass="btnRecalculate"
            runat="server" Text="Generate Price List" meta:resourcekey="CalculateButton"/>
        <h3 id="notificationMessage" class="hide">
            <span id="msgIcon"></span>
        </h3>
    </div>
    <!-- /PLG STEPS -->
    <!-- PLG GRID -->
    <telerik:RadGrid ID="ProductListGrid" runat="server" AllowPaging="true" AllowSorting="True" OnPdfExporting="ExportingPdf"
        AllowFilteringByColumn="false" GridLines="None" Skin="" CssClass="hrblTable hide" OnItemCreated="ItemCreated"
        EnableViewState="false">
        <ExportSettings IgnorePaging="true" OpenInNewWindow="true">
            <Pdf PageBottomMargin="20mm" PageTopMargin="5mm" PageLeftMargin="5mm" PageRightMargin="5mm" />
        </ExportSettings>
        <MasterTableView Width="100%" TableLayout="Auto">
            <PagerStyle Mode="Advanced" Visible="False" AlwaysVisible="False" />
            <NoRecordsTemplate>
                <asp:Literal ID="NoRecordsMessage" runat="server" meta:resourcekey="NoRecordsMessage"
                    Text="Catalog not available."></asp:Literal>
            </NoRecordsTemplate>
            <ItemStyle VerticalAlign="Top" />
            <Columns>
                <telerik:GridBoundColumn DataField="Sku" UniqueName="Sku" HeaderText="SKU"
                    meta:resourcekey="SkuColumnName" AllowSorting="False">
                    <HeaderStyle Width="42px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="Description" UniqueName="ProductName" HeaderText="Product Name"
                    meta:resourcekey="ProductNameColumnName" AllowSorting="False">
                    <HeaderStyle Width="236px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="VolumePoints" UniqueName="VolumePoints" HeaderText="Volume Points"
                    meta:resourcekey="VolumePointsColumnName" AllowSorting="False">
                    <HeaderStyle Width="44px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="EarnBase" UniqueName="EarnBase" HeaderText="Earn Base"
                    meta:resourcekey="EarnBaseColumnName" AllowSorting="False">
                    <HeaderStyle Width="44px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="RetailPrice" UniqueName="RetailPrice" HeaderText="Retail Price"
                    meta:resourcekey="RetailPriceColumnName" AllowSorting="False">
                    <HeaderStyle Width="49px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="CustomerRetailPrice" UniqueName="CustomerRetailPrice" HeaderText="Retail Price"
                    meta:resourcekey="RetailPriceColumnName" AllowSorting="False">
                    <HeaderStyle Width="49px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="DistributorDiscountCost" UniqueName="DistributorCost" HeaderText="Discounted Retail"
                    meta:resourcekey="DistributorCostColumnName" AllowSorting="False">
                    <HeaderStyle Width="60px" CssClass="distributorCostColumn" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="DistributorLoadedCost" UniqueName="DistributorLoadedCost" HeaderText="Distributor Price"
                    meta:resourcekey="DistributorLoadedCostColumnName" AllowSorting="False">
                    <HeaderStyle Width="54px" CssClass="distributorloadedCostColumn" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="CustomerPrice" UniqueName="CustomerPrice" HeaderText="Customer Price"
                    meta:resourcekey="CustomerPriceColumnName" AllowSorting="False">
                    <HeaderStyle Width="58px" CssClass="customerPriceColumn" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
                <telerik:GridBoundColumn DataField="TaxRate" UniqueName="TaxRate" HeaderText="Tax % by Line"
                    meta:resourcekey="TaxRateColumnName" AllowSorting="False">
                    <HeaderStyle Width="58px" HorizontalAlign="Left" />
                </telerik:GridBoundColumn>
            </Columns>
        </MasterTableView>
        <ClientSettings>
            <ClientEvents OnCommand="GridCommand" OnRowDataBound="GridRowDataBound" />
            <Scrolling AllowScroll="True" UseStaticHeaders="True" ScrollHeight="350" SaveScrollPosition="True" />
        </ClientSettings>
    </telerik:RadGrid>
    <!-- /PLG GRID -->

    <div id="PriceListGeneratorButtons" class="hide">
        <div>
        </div>
        <div>
            <cc1:DynamicButton ID="btnExport" ButtonType="Forward" IconPosition="Left" IconType="Plus"
                runat="server" Text="Download PDF" OnClick="ExportToPdf" meta:resourcekey="ExportButton" />
            <cc1:DynamicButton ID="btnExportDisabled" ButtonType="Back" IconPosition="Left" IconType="Plus"
                runat="server" Text="Download PDF" OnClientClick="return false;" meta:resourcekey="ExportButton" Hidden="True" />
        </div>
        <div>
            <cc1:DynamicButton ID="btnExportExcel" ButtonType="Forward" IconPosition="Left" IconType="Plus"
                runat="server" Text="Download Excel" meta:resourcekey="ExportButtonExcel" />
            <cc1:DynamicButton ID="btnExportExcelDisabled" ButtonType="Back" IconPosition="Left" IconType="Plus"
                runat="server" Text="Download Excel" OnClientClick="return false;" meta:resourcekey="ExportButtonExcel" Hidden="True" />
        </div>
    </div>
    <a href="javascript:ShowHelpText('<%=HMenuDistributorDiscount.ClientID%>');" id="imgDistributorDiscountHelpIcon">
        <img alt="" id="imgDistributorDiscountHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.png"
            height="12" width="12" />
    </a>
    <asp:Panel ID="pnlimgDistributorDiscountText" runat="server" Style="display: none; width: 300px">
        <div class="helpTip">
            <asp:Label ID="lblDiscountedRetailHelp" runat="server" meta:resourcekey="DiscountCostHelpText" />
        </div>
    </asp:Panel>
    <ajaxToolkit:HoverMenuExtender ID="HMenuDistributorDiscount" runat="server" TargetControlID="imgDistributorDiscountHelp"
        PopupControlID="pnlimgDistributorDiscountText" PopupPosition="Left" />
    <a href="javascript:ShowHelpText('<%=HMenuDistributorLoadedCost.ClientID%>');" id="imgDistributorLoadedCostIcon">
        <img alt="" id="imgDistributorLoadedCostHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.png"
            height="12" width="12" />
    </a>
    <asp:Panel ID="pnlimgDistributorLoadedCostText" runat="server" Style="display: none; width: 300px">
        <div class="helpTip">
            <asp:Label ID="lblDistributorLoadedCostHelp" runat="server" meta:resourcekey="DistributorLoadedCostText" />
        </div>
    </asp:Panel>
    <ajaxToolkit:HoverMenuExtender ID="HMenuDistributorLoadedCost" runat="server" TargetControlID="imgDistributorLoadedCostHelp"
        PopupControlID="pnlimgDistributorLoadedCostText" PopupPosition="Left" />
    <a href="javascript:ShowHelpText('<%=HMenuCustomerPrice.ClientID%>');" id="imgCustomerPriceIcon">
        <img alt="" id="imgCustomerPriceHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.png"
            height="12" width="12" />
    </a>
    <asp:Panel ID="pnlimgCustomerPriceText" runat="server" Style="display: none; width: 300px">
        <div class="helpTip">
            <asp:Label ID="lblCutomerPriceHelp" runat="server" meta:resourcekey="CustomerPriceHelpText" />
        </div>
    </asp:Panel>
    <ajaxToolkit:HoverMenuExtender ID="HMenuCustomerPrice" runat="server" TargetControlID="imgCustomerPriceHelp"
        PopupControlID="pnlimgCustomerPriceText" PopupPosition="Left" />
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel" runat="server" Skin="Sitefinity" />
    
    <div class="hrblModal pnlEstimateSH hide" id="estimateModal">
        <cc2:ContentReader ID="crShippingH" runat="server" ContentPath="lblShippingHandlingTable.html"
            SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>

    <div class="hrblModal pnlMessage hide" id="messageModal">
        <div>
            <div>
                <p id="pMessage"></p>
            </div>            
            <a id="close-message" class="backward">
                <asp:Label ID="lblOk" runat="server" Text="Ok" meta:resourcekey="OkButton"/>
            </a>
        </div>
    </div>
</asp:Content>
