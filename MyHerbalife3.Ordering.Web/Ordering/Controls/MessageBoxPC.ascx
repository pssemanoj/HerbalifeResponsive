<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageBoxPC.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.MessageBoxPC" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Panel ID="pnlMessageBox" runat="server" meta:resourcekey="pnlMessageBoxResource1" CssClass="china-pc-message center">
    <asp:UpdatePanel ID="UpdatePanelMessage" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <div class="col-sm-4">
                <asp:Label Font-Bold="true" runat="server" ID="lbMessage" Text=""></asp:Label>
            </div>
            <div class="col-sm-4">
                <cc1:DynamicButton ID="btnSubmit" ButtonType="Forward" OnClick="btnSubmit_OnClick" IconType="Plus"
                    runat="server" Text="Ok" meta:resourcekey="btnSubmitResource" />
            </div>


            <ajaxToolkit:ModalPopupExtender ID="mdlConfirm" runat="server" TargetControlID="btnSubmit"
                PopupControlID="pnlConfirm" CancelControlID="btnNo" BackgroundCssClass="modalBackground"
                DropShadow="false" />

            <asp:Panel ID="pnlConfirm" runat="server" Style="display: none">
                <div class="gdo-popup confirmClearCart">
                    <div class="gdo-form-label-left miniCartPopUp">
                        <asp:Label ID="lblConfirmClearCartText" runat="server" meta:resourcekey="lblConfirmText"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc1:DynamicButton ID="btnYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="btnYes_OnClick"
                            meta:resourcekey="Yes" />
                        <cc1:DynamicButton ID="btnNo" runat="server" ButtonType="Back" Text="No" meta:resourcekey="No" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Panel>