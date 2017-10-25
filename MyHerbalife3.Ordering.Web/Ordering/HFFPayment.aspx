<%@ Page Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="HFFPayment.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.HFFPayment" enableEventValidation="false" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit, Version=3.0.11119.25533, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">

    <asp:Button ID="btnDonate" runat="server" Text="Donate"/>

<%--    <asp:UpdatePanel ID="upDonate" runat="server">
        <ContentTemplate>--%>
            <asp:Panel ID="pnlDonate" runat="server">
                <table class="gdo-main-table">
                    <tr>
                        <td valign="top" class="gdo-main-tablecell">
                            <asp:PlaceHolder ID="plHFFModal" runat="server" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <ajaxToolkit:ModalPopupExtender ID="mdlDonate" runat="server" TargetControlID="btnDonate"
                PopupControlID="pnlDonate" CancelControlID="" BackgroundCssClass="modalBackground"
                DropShadow="false" />
<%--        </ContentTemplate>
    </asp:UpdatePanel>--%>

</asp:Content>
