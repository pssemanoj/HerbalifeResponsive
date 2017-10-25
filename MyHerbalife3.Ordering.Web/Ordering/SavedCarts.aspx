<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="SavedCarts.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SavedCarts"
    EnableEventValidation="false" meta:resourcekey="PageResource1" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register Src="~/Ordering/Controls/MessageBoxPC.ascx" TagName="PCMsgBox" TagPrefix="PCMsgBox" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register Src="~/Ordering/Controls/AddressRestrictionPopUp.ascx" TagPrefix="AdrsPopUp" TagName="AddressResPopUP" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>

<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">
   <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
    <script type="text/javascript">
        var CartsGridClientID = "<%= CartsGrid.ClientID %>";
        var RadAjaxLoadingPanelClientID = "<%= RadAjaxLoadingPanel.ClientID %>";
        var mdlDeleteCartID = "<%= mdlDeleteCart.ClientID %>";
        var pnlDeleteCartID = "<%= pnlDeleteCart.ClientID %>";
        var pnlClearCartID = "<%= pnlClearCart.ClientID %>";
        var mdlClearCartID = "<%= mdlClearCart.ClientID %>";
        var pnlNonResidentMessageID = "<%= pnlNonResidentMessage.ClientID %>";
        var mdlNonResidentMessageID = "<%= mdlNonResidentMessage.ClientID %>";
        var hidCartID = "<%= hidCartID.ClientID %>";
        var hidActionID = "<%= hidAction.ClientID %>";
        var txtSearchID = "<%= txtSearch.ClientID %>";
        var btnSearchID = "<%= btnSearch.ClientID %>";
        var locale = "<%= this.Locale %>";
        var distributorID = "<%= this.DistributorID %>";
        var lnkShowMoreProducts = '<%= GetLocalResourceObject("MoreLinkResource").ToString() %>';
        var lnkShowLessProducts = '<%= GetLocalResourceObject("LessLinkResource").ToString() %>';
        var SearchText = '<%= GetLocalResourceObject("Search.Text").ToString() %>';
        var CopyOrderMode = false;
        var CopyOrderIndex = 0;
        var CopyOrderMaxLength = 8;
        var orderBy = '';

        <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) { %>
            jQuery('[id$="updClearCart"] .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
            jQuery(window).resize(function () {
                $('[id$="updClearCart"] .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
            });
        <% } %>
    </script>
    <script type="text/javascript"> var IsChina="<%=IsChina%>"; </script>
    <script type="text/javascript" src="/Ordering/Scripts/SavedCartsAndRecentOrders.js"></script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div id="divChinaPCMessageBox" style="margin: 5px;" runat="server">
        <PCMsgBox:PCMsgBox runat="server" id="PcMsgBox1" displaysubmitbutton="True"></PCMsgBox:PCMsgBox>
    </div>
    <div id="topSavedCarts">
        <p id="pMessaging" runat="server">
        </p>
        <div class="search_cart savedcarts">
            <div class="row search-box">
                <div class="col-xs-10">
                    <asp:TextBox ID="txtSearch" runat="server" meta:resourcekey="Search"></asp:TextBox>
                </div>
                <div class="col-xs-2 align-right">
                    <asp:Button runat="server" ID="btnSearch" CssClass="actionCtrl"></asp:Button>
                </div>
            </div>
            <div class="row order-by">
                <div class="col-xs-3 label">
                    <asp:Label runat="server" ID="lblOrderBy" meta:resourcekey="OrderBy" Text="Sort By"></asp:Label>
                </div>
                <div class="col-xs-9">
                    <telerik:RadComboBox runat="server" ID="ddlOrderBy" CssClass="order-by-combobox" OnClientSelectedIndexChanged="OrderBySelectedChanged">
                    </telerik:RadComboBox>
                </div>
            </div>
            <div class="row search-button input">
                <div class="col-xs-12">
                    <cc1:DynamicButton ID="NewOrder" runat="server" Text="New Order" meta:resourcekey="NewOrder" OnClientClick="RemoveHideClass()" CssClass="actionCtrl forward" />
                </div>
            </div>
        </div>
        
        <div class="gdo-form-label-left">
            <a href="javascript:;" id="tabSavedCarts" class="focusedTab">
                <%= GetLocalResourceObject("SavedCartsTab").ToString()%>
            </a>
            <span id="DiscSku" style="display: none;" class="red"><%= GetLocalResourceObject("ErrorCopyOrderDiscontinueSkuExist").ToString()%></span>
            <span id="DiscSkuP" style="display: none;" class="red"><%= GetLocalResourceObject("ErrorSaveCartPromoExpired").ToString()%></span>
            <span id="DiscSkus" style="display: none;" class="red"><%= GetLocalResourceObject("SaveCartAllSkuInvalid").ToString()%></span>

            <a href="javascript:;" id="tabRecentOrders" class="unFocusedTab">
                <%= GetLocalResourceObject("RecentOrdersTab").ToString()%>
            </a>
        </div>
    </div>
    <div id="divGrid" class="savedCarts">
        <telerik:RadGrid ID="CartsGrid" runat="server" AllowPaging="true" AllowSorting="True"
            OnItemCreated="ItemCreated" AllowFilteringByColumn="false" GridLines="None" Skin="Hay"
            EnableViewState="false">
            <MasterTableView PageSize="4">
                <PagerStyle Mode="NextPrevAndNumeric" AlwaysVisible="true" ShowPagerText="true" CssClass="gdo-pagination-active gdo-NumericPager" />
                <NoRecordsTemplate>
                    <asp:Literal ID="NoRecordsMessage" runat="server" meta:resourcekey="NoRecordsMessage"
                        Text="No orders were found matching your search criterias. Please redefine your search."></asp:Literal>
                </NoRecordsTemplate>
                <ItemStyle VerticalAlign="Top" />
                <Columns>
                    <telerik:GridTemplateColumn UniqueName="CartSummary" HeaderText="Cart Summary" meta:resourcekey="OrderSummaryResource">
                        <ItemTemplate>
                            <a href="#" class="lnkCartName"></a><span class="lblDate"></span>
                            <p class="hiddenClass hide pAddress">
                                <span class="lblRecipient"></span>
                                <br />
                                <asp:Label runat="server" ID="lblAddressText" meta:resourcekey="lblAddressText" Text="Address"></asp:Label>
                                <br />
                                <span class="lblAddress"></span>
                            </p>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="Products" HeaderText="Products" meta:resourcekey="ProductsResource">
                        <ItemTemplate>
                            <div class="productsCont">
                                <table>
                                    <tr>
                                        <th class="sv_qty">
                                            <asp:Label runat="server" ID="lblQtyColumn" meta:resourcekey="QtyResource" Text="QTY"></asp:Label>
                                        </th>
                                        <th class="sv_sku">
                                            <asp:Label runat="server" ID="lblSKUCoulmn" meta:resourcekey="SkuResource" Text="SKU"></asp:Label>
                                        </th>
                                        <th class="sv_product">
                                            <asp:Label runat="server" ID="lblProductColumn" meta:resourcekey="ProductResource"
                                                Text="Product"></asp:Label>
                                        </th>
                                    </tr>
                                </table>
                                <a href="#" class="lnkTotalProducts">.
                                    <%= GetLocalResourceObject("MoreLinkResource").ToString()%></a>
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                    <telerik:GridTemplateColumn UniqueName="Actions" HeaderText="Action" meta:resourcekey="ActionResource">
                        <ItemTemplate>
                            <div class="checkOutBtn">
                                <cc1:DynamicButton OnClientClick="return false;" ID="btnCheckOut" IconType="Plus"
                                    ButtonType="Forward" CssClass="actionCtrl" runat="server" Text="Checkout" meta:resourcekey="CheckOutResource" />
                            </div>
                            <div class="deleteCartBtn">
                                <cc1:DynamicButton OnClientClick="return false;" ID="btnDelete" ButtonType="Link"
                                    IconType="X" CssClass="actionCtrl" runat="server" Text="Delete" meta:resourcekey="DeleteCommandResource" />
                            </div>
                            <div class="copyOrderBtn hide">
                                <cc1:DynamicButton OnClientClick="return false;" ID="btnCopyOrder" ButtonType="Forward"
                                    IconType="Plus" CssClass="actionCtrl" runat="server" Text="Copy Order" meta:resourcekey="CopyNewResource" />
                            </div>
                        </ItemTemplate>
                    </telerik:GridTemplateColumn>
                </Columns>
            </MasterTableView>
            <ClientSettings>
                <ClientEvents OnCommand="CartsGrid_Command" OnRowDataBound="CartsGrid_RowDataBound" />
            </ClientSettings>
        </telerik:RadGrid>
        <a id="ShowMoreRows" class="hide btnForward" onclick="AddMoreRows()">
            <asp:Label CssClass="actionCtrl" runat="server" ID="lblMoreRows" meta:resourcekey="lblMoreRows"
                Text="Show more rows"></asp:Label>
        </a>
    </div>
    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel" runat="server" Skin="Sitefinity" />
    <ajaxToolkit:ModalPopupExtender ID="mdlClearCart" runat="server" TargetControlID="NewOrder"
        PopupControlID="pnlClearCart" CancelControlID="FakeTarget" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <asp:Button ID="FakeTarget" CssClass="hide" runat="server" CausesValidation="False" />
    <asp:Button ID="FakeButtonTarget1" runat="server" CausesValidation="False" CssClass="hide" />
    <asp:Panel ID="pnlClearCart" CssClass="hide copyCart" runat="server">
        <asp:UpdatePanel ID="updClearCart" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="gdo-popup">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblClearCartTitle" runat="server" Text="Confirmation" meta:resourcekey="lblClearCartTitle"></asp:Label></h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblClearCartMessage1" runat="server" Text="You currently have items in your cart. If you would like to save your
						current cart for later use, enter a name below." meta:resourcekey="lblClearCartMessage1"></asp:Label>
                    </div>
                    <div runat="server" id="divExistentCart" visible="false" class="gdo-form-label-left">
                        <img alt="errorIcon" src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon" />
                        <asp:Label runat="server" ID="lblExistentCart" CssClass="lblErrors hide" Text="The name you entered already exists.
						Please enter a different name." meta:resourcekey="lblExistentCart"></asp:Label>
                        <asp:Label runat="server" ID="lblEmptyCartName" CssClass="lblErrors hide" Text="Please enter a valid cart name."
                            meta:resourcekey="lblEmptyCartName"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label runat="server" ID="lblSaveCartName" Text="New Cart Name" meta:resourcekey="lblSaveCartName"> </asp:Label>
                        <asp:TextBox runat="server" ID="txtSaveCartName" MaxLength="40"></asp:TextBox>
                        <ajaxToolkit:FilteredTextBoxExtender ID="fttxtSaveCartName" runat="server" TargetControlID="txtSaveCartName"
                            FilterMode="InvalidChars" InvalidChars="<>%!?{}[]´+¨*\¿?¡$=|~./;," />
                        <div class="gdo-button-margin-rt bttn-recalculate">
                            <cc1:DynamicButton ID="ClearCartSave" runat="server" ButtonType="Forward" Text="Save & Continue"
                                OnClick="OnSaveCart" meta:resourcekey="ClearCartSave" />
                        </div>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label runat="server" ID="lblClearCartMessage2" Text="Use suggested name or enter a new unique name."
                            meta:resourcekey="lblClearCartMessage2"> </asp:Label>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblClearCartMessage3" runat="server" Text="If you do not save your cart,
						the items in your cart will be discared." meta:resourcekey="lblClearCartMessage3"></asp:Label>
                    </div>
                    <div class="gdo-button-margin-rt bttn-recalculate">
                        <cc1:DynamicButton ID="ClearCartCancel" runat="server" ButtonType="Back" Text="Cancel"
                            meta:resourcekey="ClearCartCancel" CssClass="clearCartCancel" OnClientClick="$find(mdlClearCartID).hide();return false;" />
                        <div id="ContinueDo">
                            <cc1:DynamicButton ID="ClearCartDo" runat="server" ButtonType="Forward" Text="Continue Without Saving"
                                OnClick="OnNewOrderIgnoringCartNotNull" meta:resourcekey="ClearCartDo" />
                        </div>
                        <div id="ContinueCo" class="hide" onclick="ConfirmCopyOrder()">
                            <cc1:DynamicButton ID="ClearCartCo" runat="server" ButtonType="Forward" Text="Continue Without Saving"
                                meta:resourcekey="ClearCartDo" />
                        </div>
                        <div id="ContinueCh" class="hide" onclick="CheckNonResidentMessage(null)">
                            <cc1:DynamicButton ID="ClearCartCh" runat="server" ButtonType="Forward" Text="Continue Without Saving"
                                meta:resourcekey="ClearCartDo" />
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mdlContinue" runat="server" TargetControlID="FakeTarget"
        PopupControlID="pnlSavedCart" BackgroundCssClass="modalBackground" DropShadow="false"
        CancelControlID="FakeButtonTarget1" />
    <asp:Panel ID="pnlSavedCart" runat="server" Style="display: none; width: 300px">
        <asp:UpdatePanel ID="updSavedCart" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="gdo-popup saveCartsPopUp" style="width: 300px">
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblSavedCartMessage1" runat="server" Visible="true" Text="Your current cart has been saved."></asp:Label>
                        <asp:Label runat="server" ID="lblSavedCartMessage2" Visible="false" Text="You will now be redirected to the Online Price List."
                            meta:resourcekey="lblSavedCartMessage2"> </asp:Label>
                    </div>
                    <div class="gdo-button-margin-rt bttn-recalculate">
                        <cc1:DynamicButton ID="Continue" runat="server" ButtonType="Forward" Text="OK" OnClick="OnContinue"
                            meta:resourcekey="OK" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mdlDeleteCart" runat="server" TargetControlID="FakeTarget"
        PopupControlID="pnlDeleteCart" CancelControlID="FakeButtonTarget1" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <asp:Panel ID="pnlDeleteCart" CssClass="hide deleteCart" runat="server">
        <asp:UpdatePanel ID="updDeleteCart" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="gdo-popup">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblDeleteCartTitle" runat="server" Text="Delete Cart" meta:resourcekey="lblDeleteCartTitle"></asp:Label></h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDeleteCartMessage" runat="server" Text="Are you sure you wish
							to delete the selected cart." meta:resourcekey="lblDeleteCartMessage"></asp:Label>
                    </div>
                    <div class="gdo-button-margin-rt bttn-recalculate">
                        <cc1:DynamicButton ID="DeleteCartCancel" runat="server" ButtonType="Back" Text="Cancel"
                            meta:resourcekey="DeleteCartCancel" OnClientClick="CancelDelete()" />
                        <cc1:DynamicButton ID="DeleteCartDo" runat="server" ButtonType="Forward" Text="Delete"
                            OnClientClick="OnDeleteCart()" meta:resourcekey="DeleteCartDo" />
                        <asp:HiddenField ID="hidAction" runat="server" />
                        <asp:HiddenField ID="hidCartID" runat="server" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
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
                        <asp:Label ID="lblDupeOrderMessage" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <ajaxToolkit:ModalPopupExtender ID="mdlNonResidentMessage" runat="server" TargetControlID="FakeTarget"
        PopupControlID="pnlNonResidentMessage" CancelControlID="FakeTarget" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <asp:Panel ID="pnlNonResidentMessage" runat="server" CssClass="hide" Style="background-color: White; margin:10px; border: 5px solid silver; width: 450px; padding: 10px 13px;">
        <asp:UpdatePanel ID="updNonResidentMessage" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div>
                    <br /><br />
                    <cc2:ContentReader ID="contentReader" runat="server" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                    <p id="txtMessage" style="max-width: 400px"></p>
                    <div style="float: right;">
                        <cc1:OvalButton ID="btnYes" runat="server" meta:resourcekey="Yes" Coloring="Silver"
                            Text="OK" OnClientClick="ConfirmCheckOut()" ArrowDirection="" IconPosition="" IconType="" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
     <AdrsPopUp:AddressResPopUP runat="server" ID="AddressResPopUP1" />
</asp:Content>