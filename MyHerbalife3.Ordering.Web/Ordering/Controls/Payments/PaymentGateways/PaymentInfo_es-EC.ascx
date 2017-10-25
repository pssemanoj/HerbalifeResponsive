<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_es-EC.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_es_EC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:UpdatePanel ID="pnlCards" runat="server">
<ContentTemplate>
    <asp:Panel runat="server" ID="pnlECCards">
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
                                <asp:Label runat="server" ID="lblCardsList" meta:resourceKey="lblCardsList">Credit Cards:</asp:Label>
                            </td>
                            <td>
                                <asp:DropDownList runat="server" ID="ddlCards">
                                    <asp:ListItem Text="Visa - Tarjeta de Credito" Value="VI" meta:resourcekey="VICard" />
                                    <asp:ListItem Text="MasterCard - Tarjeta de Credito" Value="MC" meta:resourcekey="MCCard"/>
                                    <asp:ListItem Text="Dinners - Tarjeta de Credito" Value="DN" meta:resourcekey="DDCard" />
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