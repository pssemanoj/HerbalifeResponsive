<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="SavedPickupCourierLocation.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.SavedPickupCourierLocation" 
    enableEventValidation="false" meta:resourcekey="PageResource1"%>
<%@ Register Src="Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl"
    TagPrefix="hrblShippingInfoControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="dpgv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div class="gdo-overview-page-title">
        <asp:Localize runat="server" meta:resourcekey="SavedPickupFromCourierLocation">Saved Pickup From Courier Location</asp:Localize>
    </div>
    <div class="gdo-body-text-12">
        <asp:Localize runat="server" meta:resourcekey="ManagePickupFromCourierLocation">Create and manage your pickup from courier location for faster check out in the future.</asp:Localize>
    </div>
    <div class="gdo-spacer1">
    </div>
    <cc1:DynamicButton ID="btnAddPUFromCourierLocation" runat="server" ButtonType="Forward" Text="Add New Pickup From Courier Location"
        OnClick="btnAddPUFromCourierLocation_Click" meta:resourcekey="btnAddPUFromCourierLocation"></cc1:DynamicButton>
    <div class="gdo-clear gdo-spacer1">
    </div>
    <div class="table-container">
        <table class="gdo-table-width-100 gdo-border-blue" cellpadding="0" id="tblSavedPickupLocation" runat="server">
            <tr>
                <td align="right">
                    <div class="gdo-table-header" style="text-align: right; padding: 5px">
                        <asp:DataPager ID="pager" runat="server" PageSize="10" PagedControlID="gvSavedPUFromCourierLocation">
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
                    <dpgv:DataPagerGridView ID="gvSavedPUFromCourierLocation" runat="server" AllowPaging="True"
                        AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="Id"
                        OnDataBound="gvSavedPUFromCourierLocation_DataBound" OnRowDataBound="gvSavedPUFromCourierLocation_RowDataBound" 
                        OnRowCommand="gvSavedPUFromCourierLocation_RowCommand" OnPageIndexChanging="gvSavedPUFromCourierLocation_PageIndexChanging"
                        OnSorting="gvSavedPUFromCourierLocation_Sorting" PagerStyle-HorizontalAlign="Right"
                        CssClass="gdo-table-width-100 " BorderStyle="Solid" EnableModelValidation="True"
                        meta:resourcekey="gvSavedPUFromCourierLocation">
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
                            <asp:TemplateField HeaderText="Nickname" SortExpression="Alias" meta:resourcekey="TemplateNicknameResource">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Nickname" runat="server" Text='<%# Bind("Alias") %>' meta:resourcekey="NicknameResource"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Name" SortExpression="Description" meta:resourcekey="TemplateNameResource">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="Name" runat="server" Text='<%# Bind("Description") %>' meta:resourcekey="NameResource"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Location" meta:resourcekey="TemplateLocationResource">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderStyle HorizontalAlign="Right" Width="27%" />
                                <ItemStyle VerticalAlign="Top" HorizontalAlign="Right" />
                                <HeaderTemplate>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Primary" runat="server" Text='Primary |' Visible="False" meta:resourcekey="PrimaryResource"></asp:Label>
                                    <asp:LinkButton ID="Delete" runat="server" CommandName="DeleteCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        meta:resourcekey="DeleteResource">Delete </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </dpgv:DataPagerGridView>
                </td>
            </tr>
        </table>
    </div>
    <br />
    <div align="center">
        <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />
                                    Saturday 6 a.m. to 2 p.m. PST" meta:resourcekey="lblCustomerSupportResource"></asp:Label>
    </div>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl"
                runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnAddPUFromCourierLocation" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>