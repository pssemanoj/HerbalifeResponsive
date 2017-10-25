<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_es-UY.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_es_UY" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:UpdatePanel ID="pnlCards" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlPECards">
               <table>
                <tr>
                    <td style="padding-right: 10px; padding-bottom: 0px; vertical-align: top; padding-top: 4px; width: 500px">
                        <asp:Label ForeColor="Red" runat="server" ID="lblError"></asp:Label>
                        <table>
                            <tr>
                                <td>
                                    <asp:Label runat="server" ID="lblCardsList" meta:resourceKey="lblCardsList"></asp:Label>
                                </td>
                                <td>
                                    <asp:RadioButtonList runat="server" ID="ddlCards" RepeatDirection="Horizontal" RepeatLayout="Flow">
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr>
                                 <td>
                                    <asp:Label runat="server" ID="lblInstallmentsList" meta:resourceKey="lblInstallmentsList"></asp:Label>
                                </td>
                                <td>
                                    <asp:DropDownList runat="server" ID="ddlInstallments">
                                          <asp:ListItem Text="1" Value="1" />
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
            <br />
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>


