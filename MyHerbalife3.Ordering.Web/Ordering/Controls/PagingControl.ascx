<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PagingControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PagingControl" %>
<div class="PagingControl">
    <asp:LinkButton runat="server" Text="First" CssClass="paginglink" CommandName="MoveFirst" CommandArgument='<%= CurrentPage %>' ID="btnFirst" meta:resourcekey="FirstPagingControlResource"></asp:LinkButton>&nbsp;
    <asp:LinkButton runat="server" Text="Prev" CssClass="paginglink" CommandName="MovePrev" CommandArgument='<%= CurrentPage %>' ID="btnPrev" meta:resourcekey="PrevPagingControlResource"></asp:LinkButton>&nbsp;
    <asp:PlaceHolder runat="server" ID= "PagesPlaceHolder"></asp:PlaceHolder>
    <asp:LinkButton runat="server" Text="Next" CssClass="paginglink" CommandName="MoveNext" CommandArgument='<%= CurrentPage %>' ID="btnNext" meta:resourcekey="NextPagingControlResource"></asp:LinkButton>&nbsp;
    <asp:LinkButton runat="server" Text="Last" CssClass="paginglink" CommandName="MoveLast" CommandArgument='<%= CurrentPage %>' ID="btnLast" meta:resourcekey="LastPagingControlResource"></asp:LinkButton>&nbsp;
    <asp:Literal runat="server" ID="ltlTotalOrders"></asp:Literal>&nbsp;
 </div>
