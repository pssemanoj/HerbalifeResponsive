<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RuleRegulation.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.RuleRegulation" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<ajaxToolkit:ModalPopupExtender ID="InvoiceRulePopupExtender" runat="server" TargetControlID="InvoiceRuleFakeTarget"
    PopupControlID="pnlInvoiceRule" CancelControlID="OK" BackgroundCssClass="modalBackground"
    DropShadow="false" />
<asp:Button ID="InvoiceRuleFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
<asp:Panel ID="pnlInvoiceRule" runat="server" Style="display: none">
    <div class="gdo-popup confirmCancel">
        <div class="gdo-float-left gdo-popup-title">
            <h2>
                <asp:Label ID="lblChkout24hTitle" runat="server" Text="Chkout24h Title" meta:resourcekey="lblChkout24hTitle"></asp:Label>
            </h2>
        </div>
        <div class="gdo-form-label-left">
            <asp:Label ID="lblChkout24hMessage" runat="server"></asp:Label>
        </div>
        <div class="gdo-form-label-left confirmButtons">
            <cc1:DynamicButton ID="Cancel" runat="server" ButtonType="Cancel" Text="Cancel" meta:resourcekey="OK"  />
            <cc1:DynamicButton ID="OK" runat="server" ButtonType="Forward" Text="Confirm" meta:resourcekey="OK" />
            <asp:LinkButton ID="btnConfirm" runat="server" Text="Confirm" OnClick="btnConfirm_Click" />
        </div>
    </div>
</asp:Panel>
