<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageBox2.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.MessageBox2" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="uc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:Panel ID="pnlMessageBox" runat="server" Style="display: none; background-color: White; margin: 10px; border: 5px solid silver; max-width: 100%; height: auto; width: 640px; min-height: 480px;" meta:resourcekey="pnlMessageBoxResource1">
    <asp:UpdatePanel ID="upMessageBox" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div>
                <div style="text-align: center; margin-top:5px;">
                    <asp:CheckBox runat="server" ID="cbDontShowAgain2" AutoPostBack="True" OnCheckedChanged="cbDontShowAgain_CheckedChanged" Text="Don't show this message again" meta:resourcekey="cbDontShowAgainResource" />

                </div>
            <asp:Label Font-Bold="True" runat="server" ID="txtTitle" meta:resourcekey="txtTitleResource1"></asp:Label>
            <cc1:ContentReader runat="server" ContentPath="PromoSpecialDay.html" UseLocal="True" HtmlContent="" meta:resourcekey="ContentReaderResource1" ValidateContent="False" />
            <div style="text-align: center">
                <uc1:OvalButton ID="BtnYes" runat="server" meta:resourcekey="Yes" Coloring="Silver"
                    Text="OK" OnClick="OnYes" ArrowDirection="" IconPosition="" CssClass="btnForward" />
                <asp:CheckBox runat="server" ID="cbDontShowAgain" AutoPostBack="True" OnCheckedChanged="cbDontShowAgain_CheckedChanged" Text="Don't show this message again" meta:resourcekey="cbDontShowAgainResource" />

            </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<ajaxToolkit:ModalPopupExtender ID="popup_MessageBox" runat="server" TargetControlID="lnkHidden"
    PopupControlID="pnlMessageBox" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
    DynamicServicePath="" Enabled="True">    
</ajaxToolkit:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>