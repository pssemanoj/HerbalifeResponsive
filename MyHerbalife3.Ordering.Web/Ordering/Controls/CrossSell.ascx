<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CrossSell.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.CrossSell" %>
<%@ Register Src="ProductDetailPopup.ascx" TagName="ProductDetailPopup" TagPrefix="hrblProductDetailPopup" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div id="gdo-right-column-crosssell">
    <script type="text/javascript">

        function GetPnlCrossSellHelp() {
            var tooltip = $find("<%=ModalPnlCrossSell.ClientID%>").show();
            event.returnValue = false;
            return false;
        }

        function HidePnlCrossSellHelp() {
            var tooltip = $find("<%=ModalPnlCrossSell.ClientID%>").hide();
            event.returnValue = false;
            return false;
        }
    </script>
    <%--<progress:UpdatePanelProgressIndicator ID="progressPnlCrossSell" runat="server" TargetControlID="upPnlCrossSell" />--%>
    <hrblProductDetailPopup:ProductDetailPopup runat="server" ID="CntrlProductDetail">
                </hrblProductDetailPopup:ProductDetailPopup>
    <asp:UpdatePanel ID="upPnlCrossSell" runat="server" UpdateMode="Conditional" class="upPnlCrossSell">
        <ContentTemplate>
            <asp:Panel ID="TryAlsoPanel" runat="server" ScrollBars="Auto" CssClass="gdo-right-column-tbl-grey visible-sm visible-md visible-lg"
                meta:resourcekey="TryAlsoPanelResource1">
                
                <asp:ListView runat="server" ID="CrossSellProduct" EnableModelValidation="True">
                    <LayoutTemplate>
                        <div>
                            <div class="gdo-right-column-header">
                                <h3>
                                    <asp:Label runat="server" ID="TextTryAlso" Text="Try Also"></asp:Label>
                                </h3>
                            </div>
                            <%-- <div>
                        <a href="javascript:GetPnlCrossSellHelp();">
                            <img alt="question" src="/Content/Global/img/gdo/icons/question-mark-blue.png" class="gdo-question-mark-blue" /></a>
                    </div>--%>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div runat="server" id="itemPlaceholder">
                        </div>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <div id="divProd" runat="server">
                            <div>
                                <p class="left" style="text-align: center">
                                    <asp:LinkButton runat="server" ID="LinkProductDetail2" CommandArgument='<%# string.Format("{0} {1}", Eval("ID"), CategoryID) %>'
                                        Text='<%# Eval("DisplayName") %>' OnClick="ProductDetailClicked" meta:resourceKey="LinkProductDetail2Resource1"></asp:LinkButton>
                                </p>
                            </div>
                            <p style="text-align: center">
                                <img alt="Image" src='<%# Eval("DefaultSKU.ImagePath") %>' width="80" height="80" />
                                <br />
                                <asp:Label runat="server" ID="lbProd" Text='<%# Eval("Overview") %>' meta:resourceKey="lbProdResource1" />
                                <br />
                                <asp:LinkButton runat="server" ID="LinkProductDetail3" CommandArgument='<%# string.Format("{0} {1}", Eval("ID"), CategoryID) %>'
                                    Text="Read More" OnClick="ProductDetailClicked" meta:resourceKey="LinkProductDetail3Resource1"></asp:LinkButton>
                                <img id="imgReadMore" alt="" src='/Content/Global/img/arrowRight.gif' />
                            </p>
                        </div>
                    </ItemTemplate>
                    <LayoutTemplate>
                        <div>
                            <div class="gdo-right-column-header">
                                <h3>
                                    <asp:Label ID="TextTryAlso" runat="server" meta:resourceKey="TextTryAlsoResource1"
                                        Text="Try Also"></asp:Label>
                                </h3>
                            </div>
                            <%--<div>
                        <a href="javascript:GetPnlCrossSellHelp();">
                            <img alt="question" src="/Content/Global/img/gdo/icons/question-mark-blue.png" class="gdo-question-mark-blue" /></a>
                    </div>--%>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div id="itemPlaceholder" runat="server">
                        </div>
                    </LayoutTemplate>
                </asp:ListView>
            </asp:Panel>
            <asp:Panel ID="PnlCrossSell" Style="display: none;" runat="server">
                <asp:Panel runat="server" ID="PnlCrossSellInner">
                    <table class="popup-layout-table">
                        <tr>
                            <td class="popup-content">
                                <div id="divPnlCrossSell" class="popwrapper">
                                    <table class="tablecontent">
                                        <tr>
                                            <td>
                                                <!-- content -->
                                                <asp:Panel ID="PnlCrossSellError" runat="server" CssClass="error-message" Visible="false">
                                                    <!-- error message -->
                                                </asp:Panel>
                                                Sample tooltip. For the real tooltip, the HTML filename and the ID referenced by
                                                the tooltip should be the same as what Webpro supplies us with.<br />
                                                So the contract with WebPro is made up of 1) HTML Filename 2) An ID inside the HTML
                                                file that .NET will reference. 3) Resx based localizations<br />
                                                Also the height of the tooltip is depenedent on its content and needs to be set
                                                by .NET once the real content is published by WebPro.</p>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div id="divPnlCrossSellCommands" class="popup-buttons">
                                    <!-- command buttons -->
                                    <div id="divCmdPnlCrossSellClose" class="unique-ID-button">
                                        <cc1:OvalButton runat="server" ID="btnPnlCrossSellClose" Text="Close" Coloring="Silver"
                                            ArrowDirection="" IconPosition="" />
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <cc2:ModalPopupExtender ID="ModalPnlCrossSell" runat="server" TargetControlID="lnkHidden"
        PopupControlID="PnlCrossSell" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
        DynamicServicePath="" Enabled="True">
    </cc2:ModalPopupExtender>
    <asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>
</div>
