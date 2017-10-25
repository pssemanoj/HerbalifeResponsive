<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_es-CO.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_es_CO" %>
<asp:UpdatePanel ID="pnlCards" runat="server">
<ContentTemplate>
    <asp:Panel runat="server" ID="pnlPECards">
        <table>
            <tr>
                <td colspan="2" style="padding-right: 10px; padding-bottom: 0px; vertical-align: top; padding-top: 4px; width: 500px">
                    <asp:Label ForeColor="Red" runat="server" ID="lblError"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <table>
                        <tr>
                            <td>
                                <asp:Label runat="server" ID="lblCardsList" meta:resourceKey="lblCardsList">Tarjeta de Crédito:</asp:Label>
                            </td>
                            <td>
                                <asp:DropDownList runat="server" ID="ddlCards" RepeatDirection="Horizontal" RepeatLayout="Flow">                                     
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr><td colspan="2" style="height:20px"></td></tr>
                    </table>
                </td>
            </tr>
        </table>
        <br/> 
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>
