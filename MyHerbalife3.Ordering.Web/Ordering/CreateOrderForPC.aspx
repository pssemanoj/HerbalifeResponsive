<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateOrderForPC.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.CreateOrderForPC" MasterPageFile="~/MasterPages/Ordering.master" %>

<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="HeaderContent">
  <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div class="row create-order-for-pc center">
        <div class="col-sm-4">
            <div class="gdo-input-container" id="divCustomerOptionSelection" runat="server">
                <div class="gdo-select-box">
                    <asp:Label runat="server" ID="lbCustomer" Text="贵宾客户资格证号:" meta:resourcekey="lbCustomerResource" AssociatedControlID="ddlMemberID" CssClass="bold"></asp:Label>
                    <asp:DropDownList runat="server" ID="ddlMemberID" >
                    </asp:DropDownList>
                </div>
            </div>
        </div>
        <div class="col-sm-4 action">
            <cc1:DynamicButton ID="btnSubmit" ButtonType="Forward" OnClick="btnSubmit_OnClick" IconType="Plus"
             runat="server" Text="Ok" meta:resourcekey="btnSubmitResource" />
        </div>
    </div> 
</asp:Content>
 