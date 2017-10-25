<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Promotion_MY.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Promotion_MY" %>
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
                    <strong><asp:Label ID="lblheader" runat="server" Text="Purchase with purchase: " meta:resourcekey="PromoBoldMessage"></asp:Label></strong>
                    <asp:Label ID="lblPromoMessage" Text="Buy K301 Multivitamin & Minerals (2 bottles). You get to buy a H24 branded Exercise Mat (orange) at RM30." meta:resourcekey="PromoMessage" runat="server"></asp:Label>
                </div>
                <div class="gdo-form-label-left confirmButtons">
                    <cc1:DynamicButton ID="DynamicButtonPromoYes" runat="server" ButtonType="Forward" Text="OK" OnClick="ApplyPromo" meta:resourcekey="OK" />
                    <cc1:DynamicButton ID="DynamicButtonPromoNo" runat="server" ButtonType="Back" Text="Cancel" OnClick="HidePromoMsg" meta:resourcekey="Cancel" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
