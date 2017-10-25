<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="SavedShippingAddress.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SavedShippingAddress"
    EnableEventValidation="false" meta:resourcekey="PageResource1" %>

<%@ Register Src="Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl"
    TagPrefix="hrblShippingInfoControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="dpgv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div class="gdo-overview-page-title">
        <asp:Localize runat="server" meta:resourcekey="SavedShippingAddress">Saved Shipping Address</asp:Localize>
    </div>
    <div class="gdo-body-text-12">
        <asp:Localize runat="server" meta:resourcekey="CreaetManageShippingLocation">Create and manage your shipping location for faster check out in the future.</asp:Localize>
    </div>
    <div class="gdo-spacer1">
    </div>
    <cc1:DynamicButton ID="btnAddShippingAddress" runat="server" ButtonType="Forward" Text="Add New Shipping Address"
        OnClick="btnAddShippingAddress_Click" meta:resourcekey="btnAddShippingAddressResource1"></cc1:DynamicButton>
    <div class="gdo-clear gdo-spacer1">
    </div>
    <asp:UpdatePanel ID="UpdatePanelAddress" runat="server" UpdateMode="Conditional">
        <ContentTemplate>

            <%if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
              { %>
            <div id="divAddress" runat="server">
                <dpgv:DataPagerGridView runat="server" ID="gvmobSavedShippingAddress" AllowPaging="True"
                    AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="Id"
                    CssClass="gdo-table-width-100" EnableModelValidation="True"
                    OnDataBound="gvSavedShippingAddress_DataBound" OnRowDataBound="gvSavedShippingAddress_RowDataBound"
                    OnRowCommand="gvSavedShippingAddress_RowCommand" OnPageIndexChanging="gvSavedShippingAddress_PageIndexChanging"
                    PagerStyle-HorizontalAlign="Right">
                    <RowStyle CssClass="gdo-row-even gdo-body-text" />
                    <AlternatingRowStyle CssClass="gdo-row-odd gdo-body-text" />
                    <Columns>
                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                <asp:HiddenField ID="Id" Value='<%# Bind("Id") %>' runat='server' />
                                <asp:Label ID="Nickname" runat="server" Text='<%# Bind("DisplayName") %>' meta:resourcekey="NicknameResource1"></asp:Label>
                                <asp:Label ID="Name" runat="server" Text='<%# Bind("Recipient") %>' meta:resourcekey="NameResource1"></asp:Label>
                                <p id="Address" runat="server">
                                </p>
                                <div class="align-right">
                                    <asp:Label ID="Primary" runat="server" Text='
                                        
                                         |' Visible="False" meta:resourcekey="PrimaryResource1"></asp:Label>&nbsp;
                                    <asp:LinkButton ID="Edit" runat="server" CommandName="EditCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="EditResource1">Edit</asp:LinkButton>&nbsp;&nbsp;&nbsp,
                                    <asp:LinkButton ID="Delete" runat="server" CommandName="DeleteCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="DeleteResource1">Delete</asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </dpgv:DataPagerGridView>

                <div class="table-footer gdo-table-header">
                    <asp:DataPager ID="pagermob" runat="server" PageSize="10" PagedControlID="gvmobSavedShippingAddress">
                        <Fields>
                            <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" FirstPageText="<<"
                                PreviousPageText="PREVIOUS" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="true"
                                ShowPreviousPageButton="true" ShowLastPageButton="false" ShowNextPageButton="false"
                                meta:resourcekey="FirstPerviousPager" />
                            <asp:NumericPagerField ButtonCount="7" NumericButtonCssClass="gdo-pagination-active"
                                CurrentPageLabelCssClass="gdo-pagination-active" NextPreviousButtonCssClass="gdo-pagination-active" />
                            <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" LastPageText=">>"
                                NextPageText="NEXT" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="false"
                                ShowPreviousPageButton="false" ShowLastPageButton="true" ShowNextPageButton="true"
                                meta:resourcekey="LastNextPager" />
                        </Fields>
                    </asp:DataPager>
                </div>
            </div>
            <%}
              else
              { %>
            <table class="gdo-table-width-100 gdo-border-blue" cellpadding="0" id="tblAddress" runat="server">
                <tr>
                    <td>
                        <dpgv:DataPagerGridView ID="gvSavedShippingAddress" runat="server" AllowPaging="True"
                            AllowSorting="True" AutoGenerateColumns="False" OnDataBound="gvSavedShippingAddress_DataBound"
                            OnRowDataBound="gvSavedShippingAddress_RowDataBound" OnRowCommand="gvSavedShippingAddress_RowCommand"
                            DataKeyNames="Id" OnSelectedIndexChanged="gvSavedShippingAddress_SelectedIndexChanged"
                            OnRowCreated="gvSavedShippingAddress_RowCreated" OnPageIndexChanging="gvSavedShippingAddress_PageIndexChanging"
                            OnSorting="gvSavedShippingAddress_Sorting" PagerStyle-HorizontalAlign="Right"
                            CssClass="gdo-table-width-100 " BorderStyle="None" EnableModelValidation="True"
                            meta:resourcekey="gvSavedShippingAddressResource1">
                            <PagerSettings Visible="False"></PagerSettings>
                            <PagerStyle HorizontalAlign="Right"></PagerStyle>
                            <RowStyle CssClass="gdo-row-even gdo-body-text" />
                            <AlternatingRowStyle CssClass="gdo-row-odd gdo-body-text" />
                            <HeaderStyle BorderStyle="None" CssClass="gdo-table-header" />
                            <Columns>
                                <asp:TemplateField Visible="false" meta:resourcekey="TemplateFieldResource1">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="Id" Value='<%# Bind("Id") %>' runat='server' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Nickname" SortExpression="DisplayName" meta:resourcekey="TemplateFieldResource2">
                                    <ItemStyle VerticalAlign="Top" />
                                    <ItemTemplate>
                                        <asp:Label ID="Nickname" runat="server" Text='<%# Bind("DisplayName") %>' meta:resourcekey="NicknameResource1"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Name" SortExpression="Recipient" meta:resourcekey="TemplateFieldResource3">
                                    <ItemStyle VerticalAlign="Top" />
                                    <ItemTemplate>
                                        <asp:Label ID="Name" runat="server" Text='<%# Bind("Recipient") %>' meta:resourcekey="NameResource1"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Address" meta:resourcekey="TemplateFieldResource4">
                                    <ItemStyle VerticalAlign="Top" />
                                    <ItemTemplate>
                                        <p id="Address" runat="server">
                                        </p>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField   HeaderText="AddressType" SortExpression="AddressType" meta:resourcekey="TemplateFieldResource5" >
                                    <ItemStyle VerticalAlign="Top" />
                                    <ItemTemplate>
                                        <asp:Label ID="lblAddrssType" runat="server"></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField meta:resourcekey="TemplateFieldResource5">
                                    <HeaderStyle HorizontalAlign="Right" Width="27%" />
                                    <ItemStyle VerticalAlign="Top" HorizontalAlign="Right" />
                                    <HeaderTemplate>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Label ID="Primary" runat="server" Text='Primary |' Visible="False" meta:resourcekey="PrimaryResource1"></asp:Label>&nbsp;
                                        <asp:LinkButton ID="Edit" runat="server" CommandName="EditCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                            meta:resourcekey="EditResource1">Edit</asp:LinkButton>&nbsp;&nbsp;&nbsp;
                                        <asp:LinkButton ID="Delete"
                                            runat="server" CommandName="DeleteCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                            meta:resourcekey="DeleteResource1">Delete</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </dpgv:DataPagerGridView>
                    </td>
                </tr>
                <tr>
                    <td align="right">
                        <div class="table-footer gdo-table-header">
                            <asp:DataPager ID="pager" runat="server" PageSize="10" PagedControlID="gvSavedShippingAddress">
                                <Fields>
                                    <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" FirstPageText="<<"
                                        PreviousPageText="PREVIOUS" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="true"
                                        ShowPreviousPageButton="true" ShowLastPageButton="false" ShowNextPageButton="false"
                                        meta:resourcekey="FirstPerviousPager" />
                                    <asp:NumericPagerField ButtonCount="7" NumericButtonCssClass="gdo-pagination-active"
                                        CurrentPageLabelCssClass="gdo-pagination-active" NextPreviousButtonCssClass="gdo-pagination-active" />
                                    <asp:NextPreviousPagerField ButtonCssClass="gdo-pagination-active" LastPageText=">>"
                                        NextPageText="NEXT" RenderDisabledButtonsAsLabels="true" ShowFirstPageButton="false"
                                        ShowPreviousPageButton="false" ShowLastPageButton="true" ShowNextPageButton="true"
                                        meta:resourcekey="LastNextPager" />
                                </Fields>
                            </asp:DataPager>
                        </div>
                    </td>
                </tr>
            </table>
            <% }%>
        </ContentTemplate>
    </asp:UpdatePanel>
    <br />
    <div align="center">
        <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />Saturday 6 a.m. to 2 p.m. PST"
            meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
    </div>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddShippingAddress" />
        </Triggers>
    </asp:UpdatePanel>

</asp:Content>
