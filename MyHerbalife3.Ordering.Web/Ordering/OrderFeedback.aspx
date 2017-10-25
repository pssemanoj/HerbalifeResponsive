<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="OrderFeedback.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OrderFeedback" EnableEventValidation="false" meta:resourcekey="PageResource1"  %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">
    <style type="text/css">
    .hide
    {
        display:none;
    }
</style>
    <style type="text/css">
        .bottomBorder td
        {
            border-color: White;
            border-style: solid;
            border-bottom-width: 20px;
        }

</style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server" >
    <asp:GridView ID="FeedBackGridView" runat="server" AutoGenerateColumns="false" OnRowDataBound="FeedBackGridView_OnRowDataBound" 
        ShowHeader="False"  RowStyle-VerticalAlign="Top" BorderStyle="None" GridLines="None" AlternatingRowStyle-CssClass="bottomBorder" RowStyle-CssClass="bottomBorder">
        <Columns>
            <asp:TemplateField>
                <ItemTemplate>
                    <%#(Container.DataItemIndex + 1)%> )
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Id" >
                <ItemStyle CssClass="hide"></ItemStyle>
            </asp:BoundField>
            <asp:BoundField DataField="ItemName"/>
            <asp:BoundField DataField="ItemType">
                <ItemStyle CssClass="hide"></ItemStyle>
            </asp:BoundField>
            <asp:TemplateField>
                <ItemTemplate></ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <cc1:DynamicButton OnClientClick="return false;" ID="btnSaveFeedBack" ButtonType="Forward" OnClick="btnSaveFeedBack_OnClick"
        IconType="Plus" CssClass="actionCtrl" runat="server" Text="Save Feed Back" meta:resourcekey="SaveFeebBackResource" />
    <cc1:DynamicButton OnClientClick="return false;" ID="btnCancelFeedBack" ButtonType="Forward" OnClick="btnCancelFeedBack_OnClick"
        IconType="Plus" CssClass="actionCtrl" runat="server" Text="Cancel Feed Back" meta:resourcekey="CancelFeebBackResource" />
</asp:Content>