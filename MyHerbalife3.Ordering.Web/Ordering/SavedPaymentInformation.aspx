<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="SavedPaymentInformation.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SavedPaymentInformation" EnableEventValidation="false"
    meta:resourcekey="PageResource1" %>

<%@ Import Namespace="MyHerbalife3.Ordering.ServiceProvider.OrderSvc" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="dpgv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
    <script type="text/javascript">
    <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()){ %>
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
            function EndRequestHandler(sender, args) {
            $('#TB_ajaxContent .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
       }
    <% } %>
    </script>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy99" runat="server">
    </asp:ScriptManagerProxy>
    <div class="gdo-order-pref-container">
        <div class="gdo-overview-page-title">
            <asp:Localize ID="Localize1" runat="server" meta:resourcekey="SavedPaymentInformationTitle">Saved Credit Cards</asp:Localize>
        </div>
        <div class="gdo-body-text-12">
            <asp:Localize ID="Localize2" runat="server" meta:resourcekey="CreateAndManagePaymentInfo">Create and manage your Payment Information for faster check out in the future.</asp:Localize>
        </div>
        <div class="gdo-spacer1">
        </div>
        <div>
            <cc1:DynamicButton ID="btnAddPaymentInfo" runat="server" ButtonType="Forward" Text="Add New Credit Card"
                OnClick="btnAddPaymentInfo_Click" meta:resourcekey="btnAddPaymentInfoResource1"></cc1:DynamicButton>
        </div>
        <div class="gdo-clear gdo-spacer1">
        </div>

        <table class="gdo-table-width-100 gdo-border-blue responsive-table" cellpadding="0" id="tblPaymentInfo" runat="server">            
            <tr>
                <td>
                    <dpgv:DataPagerGridView ID="gvSavedPaymentInformation" runat="server" AllowPaging="True"
                        AllowSorting="True" AutoGenerateColumns="False" OnDataBound="gvSavedPaymentInformation_DataBound"
                        OnRowDataBound="gvSavedPaymentInformation_RowDataBound" OnRowCommand="gvSavedPaymentInformation_RowCommand"
                        OnRowEditing="gvSavedPaymentInformation_RowEditing" DataKeyNames="ID" OnSelectedIndexChanged="gvSavedPaymentInformation_SelectedIndexChanged"
                        OnRowCreated="gvSavedPaymentInformation_RowCreated" OnPageIndexChanging="gvSavedPaymentInformation_PageIndexChanging"
                        OnSorting="gvSavedPaymentInformation_Sorting" PagerStyle-HorizontalAlign="Right"
                        CssClass="gdo-table-width-100 " BorderStyle="None" EnableModelValidation="True"
                        meta:resourcekey="gvSavedPaymentInformationResource1">
                        <PagerSettings Visible="False"></PagerSettings>
                        <PagerStyle HorizontalAlign="Right"></PagerStyle>
                        <RowStyle CssClass="gdo-row-even gdo-body-text" />
                        <AlternatingRowStyle CssClass="gdo-row-odd gdo-body-text" />
                        <HeaderStyle BorderStyle="None" CssClass="gdo-table-header" />
                        <Columns>
                            <asp:TemplateField Visible="false" meta:resourcekey="TemplateFieldResource1">
                                <ItemTemplate>
                                    <asp:HiddenField ID="Id" Value='<%# Bind("ID") %>' runat='server' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Nickname" SortExpression="Alias" meta:resourcekey="TemplateFieldResource2">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Nickname" runat="server" Text='<%# GetAlias(Container.DataItem as PaymentInformation) %>' meta:resourcekey="NicknameResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Card Holder Name" SortExpression="CardHolder" meta:resourcekey="TemplateFieldResource3">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="CardHolder" runat="server" Text='<%# string.Format("{0} {1} {2}", Eval("CardHolder.First"), Eval("CardHolder.Middle"), Eval("CardHolder.Last")) %>'
                                        meta:resourcekey="CardHolderResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Card Type" SortExpression="CardType" meta:resourcekey="TemplateFieldResource4">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="CardType" runat="server" Text='<%# getCardName(Eval("CardType") as string) %>' meta:resourcekey="CardTypeResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Card Number" meta:resourcekey="TemplateFieldResource5">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="CardNumber" runat="server" Text='<%# Bind("CardNumber") %>' meta:resourcekey="CardNumberResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Exp Date" meta:resourcekey="TemplateFieldResource6">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Expiration" runat="server" Text='<%# (DateTime.Parse(Eval("Expiration").ToString()) == getMyKeyExpirationDate()) ? "" : Eval("Expiration", "{0:MM-yyyy}") %>'
                                        meta:resourcekey="ExpirationResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField meta:resourcekey="TemplateFieldResource7">
                                <HeaderStyle HorizontalAlign="Right" Width="27%" />
                                <ItemStyle VerticalAlign="Top" HorizontalAlign="Right" />
                                <HeaderTemplate>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Primary" runat="server" Text='Primary |' Visible="False" meta:resourcekey="PrimaryResource1"></asp:Label>
                                    <asp:LinkButton ID="Edit" runat="server" CommandName="EditCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="EditResource1">Edit</asp:LinkButton>&nbsp;|&nbsp;
                                    <asp:LinkButton ID="Delete" runat="server" CommandName="DeleteCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="DeleteResource1">Delete</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <%--                        <PagerStyle HorizontalAlign="Right" BorderStyle="None" CssClass="" /> --%>
                    </dpgv:DataPagerGridView>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <div class="gdo-table-header" style="text-align: right; padding: 10PX 0">
                        <asp:DataPager ID="pager" runat="server" PageSize="10" PagedControlID="gvSavedPaymentInformation">
                            <Fields>
                                <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" FirstPageText="<<"
                                    PreviousPageText="PREVIOUS" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="true"
                                    ShowPreviousPageButton="true" ShowLastPageButton="false" ShowNextPageButton="false" meta:resourcekey="Pager" />
                                <asp:NumericPagerField ButtonCount="7" NumericButtonCssClass="gdo-pagination-active"
                                    CurrentPageLabelCssClass="gdo-pagination-active" NextPreviousButtonCssClass="gdo-pagination-active" />
                                <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" LastPageText=">>"
                                    NextPageText="NEXT" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="false"
                                    ShowPreviousPageButton="false" ShowLastPageButton="true" ShowNextPageButton="true" meta:resourcekey="Pager" />
                            </Fields>
                        </asp:DataPager>
                    </div>
                </td>
            </tr>
        </table>
        <%--        <br />
        <p style="text-align: center">
            For order support, please call 1-866-666-4744, 9am to 8pm PST Monday - Friday.</p>
        <br />
        <p style="text-align: center">
            Saturday 6am -2pm PST</p>
        --%>
        <div align="center">
            <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />
                                    Saturday 6 a.m. to 2 p.m. PST"
                meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
        </div>
    </div>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:PlaceHolder ID="phPaymentInfoControl" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddPaymentInfo" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
