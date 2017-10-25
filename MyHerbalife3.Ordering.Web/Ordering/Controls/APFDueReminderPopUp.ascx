<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="APFDueReminderPopUp.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.APFDueReminderPopUp" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:UpdatePanel ID="UpdatePanelAPFDueReminder" runat="server" UpdateMode="Always">
    <ContentTemplate>
        <ajaxToolkit:ModalPopupExtender ID="APFDueReminderPopupExtender" runat="server" TargetControlID="APFDueReminderFakeTarget"
            PopupControlID="pnldupeAPFDueReminder" CancelControlID="APFDueReminderFakeTarget" BackgroundCssClass="modalBackground" 
            DropShadow="false" />
        <asp:Button ID="APFDueReminderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Panel ID="pnldupeAPFDueReminder" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblRemindltr" runat="server" Text="Your Annual Membership Services Fee is due on {ApfDueDate}. In order to prevent any interruptions, please pay the Annual Membership Services Fee before or on the due date." meta:resourcekey="lblRemindltr"></asp:Label>

                </div>
                <div class="gdo-form-label-left confirmButtons">
                    <cc1:DynamicButton ID="btnRemind" runat="server" ButtonType="Forward" Text="Remind Me Later" OnClick="HidePopUp" meta:resourcekey="RemindMeLate" />
                    <cc1:DynamicButton ID="btnPay" runat="server" ButtonType="Forward" Text="Pay Now" OnClick="Navigatetocheckout" meta:resourcekey="PayNow" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
