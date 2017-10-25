<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_es-AR.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_es_AR" %>

<asp:UpdatePanel ID="upInstallments" runat="server" UpdateMode="Conditional" >
    <ContentTemplate>
        <asp:Panel runat="server">
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
                                    <asp:DropDownList runat="server" ID="ddlCards" AutoPostBack="True" DataTextField="Name" DataValueField="CardId" OnSelectedIndexChanged="ddlCards_SelectedIndexChanged" ></asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                 <td>
                                    <asp:Label runat="server" ID="lblInstallmentsList" meta:resourceKey="lblInstallmentsList"></asp:Label>
                                </td>
                                <td>
                                    <asp:DropDownList runat="server" ID="ddlInstallments" AutoPostBack="True" OnSelectedIndexChanged="ddlInstallments_SelectedIndexChanged" ></asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>                                            
