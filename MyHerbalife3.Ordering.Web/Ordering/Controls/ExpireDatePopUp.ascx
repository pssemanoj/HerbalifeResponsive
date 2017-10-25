<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExpireDatePopUp.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ExpireDatePopUp" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:UpdatePanel ID="UpdatePanelAddPromosku" runat="server" UpdateMode="Always">
    <ContentTemplate>
        <ajaxToolkit:ModalPopupExtender ID="addPromoSkuPopupExtender" runat="server" TargetControlID="AddPromoSkuFakeTarget"
            PopupControlID="pnldupeAddPromoSku" CancelControlID="AddPromoSkuFakeTarget" BackgroundCssClass="modalBackground"
            DropShadow="false" />
        <asp:Button ID="AddPromoSkuFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Panel ID="pnldupeAddPromoSku" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                   <asp:BulletedList runat="server" ID="listMessage" Font-Bold="True" >
                                            </asp:BulletedList>

                </div>
                <div class="gdo-form-label-left confirmButtons">
                    <cc1:DynamicButton ID="DynamicButtonPromoYes" runat="server" ButtonType="Forward" Text="OK" OnClick="HidePopUp" meta:resourcekey="OK" />
                    <cc1:DynamicButton ID="DynamicButtonPromoNo" runat="server" ButtonType="Back" Text="Cancel" OnClick="DeleteSKUSFromCart" meta:resourcekey="Cancel" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
