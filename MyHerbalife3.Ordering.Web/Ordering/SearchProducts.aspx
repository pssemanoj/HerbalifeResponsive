<%@ Page Title="Search Product Page" Language="C#" MasterPageFile="~/MasterPages/Ordering.master"
    AutoEventWireup="true" CodeBehind="SearchProducts.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SearchProducts"
    EnableEventValidation="false" meta:resourcekey="PageResource1" ValidateRequest="false" %>
<%@ Register Src="Controls/ProductDetailPopup.ascx" TagName="ProductDetailPopup"
    TagPrefix="uc2" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register Src="~/Ordering/Controls/MessageBoxPC.ascx" TagName="PCMsgBox" TagPrefix="PCMsgBox" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>

 <asp:Content runat="server" ID="Content3" ContentPlaceHolderID="HeaderContent">
     <hrblAdvertisement:Advertisement ID="Advertisement1" runat="server" />
    </asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
    <script type="text/javascript">
        // Resx values. 
        var noSKUFoundText = '';
        var autoPlaceHolderText = '';
        </script>
    <div class="gdo-order-pref-container">
        <table>
            <tr>
                <td>
                    <asp:Panel ID="pnlSearch" runat="server" DefaultButton="btnGo">
                        <div id="divChinaPCMessageBox" style="margin: 5px;" runat="server">
                            <PCMsgBox:PCMsgBox runat="server" ID="PcMsgBox1" DisplaySubmitButton="True"></PCMsgBox:PCMsgBox>
                        </div>
                        <div id="OrderBySkuMain">
                        <asp:Label runat="server" ID="lblSearchInstruction" Text=""  meta:resourcekey="lblSearchInstruction"></asp:Label>
                        <br />
                        <asp:TextBox ID="txtSearchTerm" runat="server" meta:resourcekey="txtSearchTermResource1"
                            MaxLength="50" Columns="40" /><asp:RequiredFieldValidator ID="rfvSearchTerm" runat="server"
                                Text="*" ValidationGroup="SubmitSearch" ControlToValidate="txtSearchTerm" Display="Static" EnableClientScript="true" />&nbsp;
                            </div>
                        
                        <div class="gdo-error-message-div gdo-error-message-txt errorList">
                        <asp:BulletedList runat="server" ID="uxErrores" CssClass="gdo-error-message-txt" BulletStyle="Circle"></asp:BulletedList>
                    </div>
                    </asp:Panel>
                </td>
                <td>
                    <span>&nbsp;</span>
                    <div>
                        <cc1:OvalButton ID="btnGo" runat="server" Coloring="Silver" Text="Go" OnClick="btnGo_Click"
                            ArrowDirection="" ValidationGroup="SubmitSearch" IconPosition="" IconType="" meta:resourcekey="btnGoResource1"></cc1:OvalButton>
                    </div>
                </td>
            </tr>
        </table>
        <table class="gdo-table-width-100 gdo-border-blue">
            <tr>
                <td>
                    <telerik:RadGrid ID="rgSearchProducts" Width="100%" AllowPaging="True" PageSize="10"
                        runat="server" AllowSorting="false" OnNeedDataSource="rgSearchProducts_NeedDataSource"
                        AutoGenerateColumns="False" GridLines="None" OnItemCommand="rgSearchProducts_ItemCommand"
                        OnItemDataBound="rgSearchProducts_ItemDataBound" EnableEmbeddedSkins="true" Skin="Outlook" EnableLinqExpressions="false" meta:resourcekey="rgSearchProducts">
                        <PagerStyle Mode="NextPrevAndNumeric" Position="Top" AlwaysVisible="true" ShowPagerText="true"
                            PagerTextFormat="{4}  Showing {2} - {3} of {5} Results" />
                        <MasterTableView Width="100%" DataKeyNames="ID" AllowFilteringByColumn="true" AllowPaging="true">
                            <Columns>
                                <telerik:GridTemplateColumn UniqueName="OverdueStatus" AllowFiltering="false">
                                    <ItemTemplate>
                                        <asp:Panel ID="pnlProductImage" runat="server" meta:resourcekey="pnlProductImageResource1">
                                            <asp:LinkButton ID="imageBtnProductImage" CommandName="ClickImageBtnProductImage"
                                                CommandArgument='<%# Eval("Product.ID") %>' runat="server">
                                                <asp:Image ID="imageProduct" ImageUrl='<%# Eval("ImagePath") %>' ToolTip=" " runat="server"
                                                    Width="30px" Height="30px" meta:resourcekey="imageProductResource1" />
                                            </asp:LinkButton>
                                        </asp:Panel>
                                        <asp:Panel ID="pnlProductDesc" runat="server" meta:resourcekey="pnlProductDescResource1">
                                            <asp:LinkButton ID="searchResultLine1" runat="server" CommandName="ClickSearchResultLine1"
                                                CommandArgument='<%# Eval("Product.ID") %>' Text='<%# Eval("Product.DisplayName") %>'
                                                meta:resourcekey="searchResultLine1Resource1"></asp:LinkButton>
                                            <br />
                                            <asp:Label ID="searchResultLine2" Text='<%# Eval("Product.Overview") %>' runat="server"
                                                meta:resourcekey="searchResultLine2Resource1"></asp:Label>
                                            <asp:LinkButton ID="searchResultLine3" runat="server" PostBackUrl='<%# getProductDetailUrl(Eval("Product.ID").ToString()) %>'
                                                Text='<%# getProductDetailUrl(Eval("Product.ID").ToString()) %>' meta:resourcekey="searchResultLine3Resource1"></asp:LinkButton>
                                        </asp:Panel>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                            </Columns>
                        </MasterTableView>
                    </telerik:RadGrid>
                </td>
            </tr>
        </table>
        <br />
        <div align="center">
            <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br /> Saturday 6 a.m. to 2 p.m. PST"
                meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
        </div>
    </div>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc2:ProductDetailPopup ID="ucProductDetail" runat="server" />
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
                        <asp:Label ID="lblDupeOrderMessage" runat="server"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
