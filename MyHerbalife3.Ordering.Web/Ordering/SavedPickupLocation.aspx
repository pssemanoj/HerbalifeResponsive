<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="SavedPickupLocation.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SavedPickupLocation" enableEventValidation="false" 
    meta:resourcekey="PageResource1" %>
<%@ Register Src="Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl"
    TagPrefix="hrblShippingInfoControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="dpgv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div class="gdo-overview-page-title">
        <asp:Localize runat="server" meta:resourcekey="SavedPickupLocation">Saved Pickup Location</asp:Localize>
    </div>
    <div class="gdo-body-text-12">
        <asp:Localize runat="server" meta:resourcekey="CreateManagePickupLocation">Create and manage your pickup location for faster check out in the future.</asp:Localize>
    </div>
    <div class="gdo-spacer1">
    </div>
    <cc1:DynamicButton ID="btnAddPickupLocation" runat="server" ButtonType="Forward" Text="Add New Pickup Location"
        OnClick="btnAddPickupLocation_Click" meta:resourcekey="btnAddPickupLocationResource1"></cc1:DynamicButton>
    <div class="gdo-clear gdo-spacer1">
    </div>
    <div class="table-container">
        <table class="gdo-table-width-100 gdo-border-blue" cellpadding="0" id="tblSavedPickupLocation" runat="server">
            <tr>
                <td align="right">
                    <div class="gdo-table-header" style="text-align: right; padding: 0px 0px 5px 0px">
                        <asp:DataPager ID="pager" runat="server" PageSize="10" PagedControlID="gvSavedPickupLocation">
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
            <tr>
                <td>
                    <dpgv:DataPagerGridView ID="gvSavedPickupLocation" runat="server" AllowPaging="True"
                        AllowSorting="True" AutoGenerateColumns="False" OnDataBound="gvSavedPickupLocation_DataBound"
                        OnRowDataBound="gvSavedPickupLocation_RowDataBound" OnRowCommand="gvSavedPickupLocation_RowCommand"
                        OnRowEditing="gvSavedPickupLocation_RowEditing" DataKeyNames="Id" OnSelectedIndexChanged="gvSavedPickupLocation_SelectedIndexChanged"
                        OnRowCreated="gvSavedPickupLocation_RowCreated" OnPageIndexChanging="gvSavedPickupLocation_PageIndexChanging"
                        OnSorting="gvSavedPickupLocation_Sorting" PagerStyle-HorizontalAlign="Right"
                        CssClass="gdo-table-width-100 " BorderStyle="None" EnableModelValidation="True"
                        meta:resourcekey="gvSavedPickupLocationResource1">
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
                            <asp:TemplateField HeaderText="Nickname" SortExpression="Alias" meta:resourcekey="TemplateFieldResource2">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Nickname" runat="server" Text='<%# Bind("Alias") %>' meta:resourcekey="NicknameResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Name" SortExpression="Description" meta:resourcekey="TemplateFieldResource3">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Name" runat="server" Text='<%# Bind("Description") %>' meta:resourcekey="NameResource1"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Location" meta:resourcekey="TemplateFieldResource4">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField meta:resourcekey="TemplateFieldResource5">
                                <HeaderStyle HorizontalAlign="Right" Width="27%" />
                                <ItemStyle VerticalAlign="Top" HorizontalAlign="Right" />
                                <HeaderTemplate>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Primary" runat="server" Text='Primary |' Visible="False" meta:resourcekey="PrimaryResource1"></asp:Label>
                                    <asp:LinkButton ID="Delete" runat="server" CommandName="DeleteCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="DeleteResource1">Delete </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <%--                        <PagerStyle HorizontalAlign="Right" BorderStyle="None" CssClass="" /> --%>
                    </dpgv:DataPagerGridView>
                </td>
            </tr>
        </table>
    </div>
    <br />
    <%--        <p style="text-align: center">
            For order support, please call 1-866-666-4744, 9am to 8pm PST Monday - Friday.</p>
        <br />
        <p style="text-align: center">
            Saturday 6am -2pm PST</p>
    --%>
    <div align="center">
        <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />
                                    Saturday 6 a.m. to 2 p.m. PST" meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
    </div>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl"
                runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddPickupLocation" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
