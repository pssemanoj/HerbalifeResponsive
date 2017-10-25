<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PrintThisPage.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PrintThisPage" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:Panel ID="pnlPrintThisPage" runat="server" Style="display: none; background-color: White"
    meta:resourcekey="pnlPrintThisPageResource1">
    <asp:UpdatePanel ID="upPrintThisPage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="TB_window_PrintProduct" style="margin-left: 0px; margin-top: 0px; display: block;
                height: 600px;">
                <div id="TB_ajaxContent">
                    <div class="gdo-popup">
                        <div style="width: 600px; height: 460px;">
                            <div class="popupClose" style="margin-bottom: 5px; text-align: right;">
                                <asp:LinkButton ID="CancelButton" runat="server" Text="X Close" OnClick="OnCancel"
                                    meta:resourcekey="CancelButton" />
                            </div>
                            <asp:Panel ID="Panel1" runat="server">
                            </asp:Panel>
                        </div>
                    </div>
                    <div style="margin-left: 400px;">
                        <cc1:DynamicButton ID="btnPrintThisPage" runat="server" ButtonType="Neutral" Text="Print This Page" OnClick="OnPrintPage"
                            meta:resourcekey="PrintThisPage" />
                    </div>
                </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
