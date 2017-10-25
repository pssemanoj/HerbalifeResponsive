<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OneTimePin.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.OneTimePin" %>

<asp:UpdatePanel ID="OneTimePinUpdatePanel" runat="server" UpdateMode="Always">
    <ContentTemplate>
        <ajaxToolkit:ModalPopupExtender ID="OneTimePinPopupExtender" runat="server" TargetControlID="OneTimePinFakeTarget"
            PopupControlID="pnlOneTimePin" CancelControlID="btnBack" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <asp:Button ID="OneTimePinFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Panel ID="pnlOneTimePin" runat="server" Style="display: none; background-color: white; width: 60%; padding: 30px;">
            <div style="margin: auto; width: 200px;">
                <div style="padding: 10px;">
                    <asp:Label ID="lblErrorMessage" runat="server" Text="Invalid OTP Pin" CssClass="gdo-error-message-txt" Visible="false" meta:resourcekey="lblErrorMessage"></asp:Label>
                </div>
                <div style="padding: 10px;">
                    <asp:Label ID="lblOTPCaption" runat="server" Text="Enter Received OTP Pin" meta:resourcekey="lblOTPCaption"></asp:Label>
                </div>
                <div style="padding: 10px;">
                    <asp:TextBox ID="txtOTP" runat="server" MaxLength="6" size="6" autocomplete="off"></asp:TextBox>
                </div>
                <div style="padding: 10px;">
                    <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="actionButton" meta:resourcekey="btnBack" />
                    <asp:Button ID="btnConfirm" runat="server" Text="Confirm" OnClick="btnConfirm_Click" OnClientClick="showProgress();" CssClass="selectedActionButton" meta:resourcekey="btnConfirm" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
