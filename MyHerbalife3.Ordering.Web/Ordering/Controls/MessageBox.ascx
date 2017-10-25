<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageBox.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.MessageBox" %>
<%@ Register TagPrefix="uc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<asp:Panel ID="pnlMessageBox" runat="server" Style="display: none; background-color: White; margin:10px; border: 5px solid silver"
    meta:resourcekey="pnlMessageBoxResource1">
    <asp:UpdatePanel ID="upMessageBox" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div>
                <asp:Label Font-Bold="true" runat="server" ID="txtTitle" meta:resourcekey="txtTitleResource1"></asp:Label>
                <br />
                <br />
                <cc1:ContentReader ID="contentReader" runat="server" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                <p runat="server" id="txtMessage" style="max-width: 400px">
                </p>
                <div style="float: right">
                    <uc1:OvalButton ID="BtnYes" runat="server" meta:resourcekey="Yes" Coloring="Silver"
                        Text="OK" OnClick="OnYes" ArrowDirection="" IconPosition="" IconType="" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<AjaxToolkit:ModalPopupExtender ID="popup_MessageBox" runat="server" TargetControlID="lnkHidden"
    PopupControlID="pnlMessageBox" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
    DynamicServicePath="" Enabled="True">
</AjaxToolkit:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>
