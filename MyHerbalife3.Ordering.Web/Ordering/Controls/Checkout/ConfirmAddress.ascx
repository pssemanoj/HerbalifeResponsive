<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfirmAddress.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.ConfirmAddress" %>
<asp:Panel ID="panelConfirmAddress" runat="server" CssClass="mandatoryConfirm">
    <table>
        <tr>
            <td class="headerConfirm">
                <asp:Label CssClass="headerMsg" runat="server" ID="lblDisplayConfirmationRequired"
                    Text="" meta:resourcekey="lblDisplayConfirmationRequiredRes"></asp:Label>
            </td>
        </tr>
        <tr>
            <td runat="server" id="ConfirmAddressContent">
                <asp:CheckBox ID="cbConfirmAddress" runat="server"
                    Text="" meta:resourcekey="cbConfirmAddressRes" name="cbConfirmAddress" />
            </td>
        </tr>
    </table>
</asp:Panel>