<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_vi-VN.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_vi_VN" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:UpdatePanel ID="pnlCards" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlVNCards">
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
                                    <asp:RadioButtonList runat="server" ID="ddlCards" RepeatDirection="Horizontal" RepeatLayout="Flow" OnSelectedIndexChanged="ddlCards_SelectedIndexChanged" AutoPostBack="true">
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="height: 20px"></td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
