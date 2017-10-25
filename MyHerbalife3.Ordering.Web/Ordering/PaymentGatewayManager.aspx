<%@ Page Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" ValidateRequest="false"
    CodeBehind="PaymentGatewayManager.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.PaymentGatewayManager"
    meta:resourcekey="PageResource1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
    <div>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Timer ID="Timer1" runat="server" Interval="5000" OnTick="Timer_Tick" />
                <div style="height:400px">
                <table border="0" align="center" cellpadding="0" cellspacing="0" style="width: 800px;">
                    <tr>
                        <td>
                            <br /><br />
                            <div align="center" class="gdo-loading-message-img">
                                <img src="/Content/Global/img/gdo/gdo_loading_message_logo.png" alt="Herbalife" /></div>
                            <br />
                            <div align="center" class="gdo-loading-message">
                                <asp:Label ID="lbSubmitOrderStatus" runat="server" Text="Your order is being processed. Please wait..."
                                    meta:resourcekey="lbSubmitOrderStatusResource1"></asp:Label></div>
                        </td>
                    </tr>
                </table>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
