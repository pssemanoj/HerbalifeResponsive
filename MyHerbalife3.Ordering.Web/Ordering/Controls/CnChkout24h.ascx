<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CnChkout24h.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.CnChkout24h" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<ajaxToolkit:ModalPopupExtender ID="Chkout24hPopupExtender" runat="server" TargetControlID="Chkout24hFakeTarget"
    PopupControlID="pnlChkout24h" CancelControlID="OK" BackgroundCssClass="modalBackground"
    DropShadow="false" />
<asp:Button ID="Chkout24hFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
<asp:Panel ID="pnlChkout24h" runat="server" Style="display: none;" BackColor="#cccccc">
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
            <cc1:DynamicButton ID="OK" runat="server" ButtonType="Forward" Text="OK" meta:resourcekey="OK" />
        </div>
    </div>
</asp:Panel>