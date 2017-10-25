<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SaveCartCommand.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.SaveCartCommand" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register TagPrefix="myHLControls" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<script type="text/javascript" language="javascript">
    function GetSavedCartsHelpText() {
        var tooltip = $find("<%=HMenuExtSavedCarts.ClientID%>")
        if (tooltip) {
            if (tooltip.show) {
                tooltip.show();
            }
        }
        if (event) {
            event.returnValue = false;
        }
    }
    var mdlSaveCartID = "<%= mdlSaveCart.ClientID %>";
    var mdlContinueID = "<%= mdlContinue.ClientID %>";
    function CancelSavedCart(button, saved) {
        if (saved) {
            $find(mdlContinueID).hide();
        }
        else {
            $find(mdlSaveCartID).hide();
        }

        aeProgress_onUpdated(globalProgressDivID);
        button.href = '#';
        if (event) {
            event.returnValue = false;
        }
        return false;
    }
    // Hide continue modal popup.
    function HideContinueModal() {
        $find(mdlContinueID).hide();
        aeProgress_onUpdated(globalProgressDivID);
        if (event) {
            event.returnValue = false;
        }
        return false;
    }
</script>
<div>
    <asp:Label ID="FakeButtonTarget" runat="server" CssClass="hide" />
    <asp:Label ID="FakeButtonTarget1" runat="server"  CssClass="hide" />
    <myHLControls:DynamicButton ID="OvalSaveCartButton" runat="server" ButtonType="Back"
        OnClick="OnSaveCartClick" meta:resourcekey="SaveCart" Visible="false" name="OvalSaveCartButton" />
    <div id="saveCartLnk">
        <asp:HyperLink ID="lnkToSavedCarts" runat="server" meta:resourcekey="lnkToSavedCarts"
            Text="Go to Saved Carts" NavigateUrl="~/Ordering/SavedCarts.aspx" name="lnkToSavedCarts"></asp:HyperLink>
        <myHLControls:DynamicButton ID="OvalSaveCartLink" runat="server" Text="Save Cart" OnClick="OnSaveCartClick"
            meta:resourcekey="SaveCart" Visible="false" ButtonType="Link" name="OvalSaveCartLink"></myHLControls:DynamicButton>
        <a href="javascript:GetSavedCartsHelpText();" class="hide-xs">
            <img id="imgSavedCartsHelp" runat="server" src="/Content/global/Events/cruise/img/icon_info.gif"
                height="12" width="12" />
        </a>
    </div>
</div>
<ajaxToolkit:HoverMenuExtender ID="HMenuExtSavedCarts" runat="server" TargetControlID="imgSavedCartsHelp"
    PopupControlID="pnlSavedCartHelpText" />
<ajaxToolkit:ModalPopupExtender ID="mdlSaveCart" runat="server" TargetControlID="FakeButtonTarget1"
    PopupControlID="pnlSaveCart" BackgroundCssClass="modalBackground" DropShadow="false"
    CancelControlID="FakeButtonTarget" />
<ajaxToolkit:ModalPopupExtender ID="mdlContinue" runat="server" TargetControlID="FakeButtonTarget"
    PopupControlID="pnlSavedCart" BackgroundCssClass="modalBackground" DropShadow="false"
    CancelControlID="FakeButtonTarget1" />
<asp:Panel ID="pnlSaveCart" runat="server" Style="display: none">
    <asp:UpdatePanel ID="updSaveCart" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-popup saveCartsPopUp">
                <div class="gdo-float-left gdo-popup-title">
                    <h2>
                        <asp:Label ID="lblSaveCartTitle" runat="server" Text="Save Cart" meta:resourcekey="lblSaveCartTitle"></asp:Label></h2>
                </div>
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblSaveCartMessage1" runat="server" Text="The Saved Carts feature provides a convenient way to create, manage and save multiple carts for faster checout in the future."
                        meta:resourcekey="lblSaveCartMessage1"></asp:Label>
                </div>
                <div runat="server" id="divExistentCart" visible="false" class="gdo-form-label-left"
                    style="white-space: nowrap">
                    <img alt="errorIcon" src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon" />
                    <asp:Label runat="server" ID="lblExistentCart" Text="The name you entered already exists.
                    Please enter a different name." Style="color: Red" meta:resourcekey="lblExistentCart"></asp:Label>
                    <asp:Label runat="server" ID="lblEmptyCartName" Text="Please enter a valid cart name."
                        meta:resourcekey="lblEmptyCartName"></asp:Label>
                </div>
                <div class="gdo-form-label-left">
                    <asp:Label runat="server" ID="lblSaveCartName" CssClass="lblName" Text="Save New Cart"
                        meta:resourcekey="lblSaveCartName"> </asp:Label>
                    <asp:TextBox runat="server" ID="txtSaveCartName" CssClass="txtName" MaxLength="40"></asp:TextBox>
                    <asp:Label runat="server" ID="lblSaveCartMessage2" CssClass="lblMessage2" Text="Use suggested name or enter a new unique name."
                        meta:resourcekey="lblSaveCartMessage2"> </asp:Label>
                    <ajaxToolkit:FilteredTextBoxExtender ID="fttxtSaveCartName" runat="server" TargetControlID="txtSaveCartName"
                        FilterMode="InvalidChars" InvalidChars="<>%!?{}[]´+¨*\¿?¡$=|~./;," />
                </div>
                <div class="gdo-button-margin-rt bttn-recalculate saveCartsButtonsDiv">
                    <myHLControls:DynamicButton ID="SaveCartCancel" runat="server" ButtonType="Back"
                        Text="Cancel" OnClientClick="CancelSavedCart(this, false);" meta:resourcekey="SaveCartCancel" />
                    <myHLControls:DynamicButton ID="SaveCartDo" runat="server" ButtonType="Forward" Text="Save"
                        OnClick="OnSaveCart" meta:resourcekey="SaveCartDo" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<asp:Panel ID="pnlSavedCartHelpText" runat="server" Style="display: none; width: 300px">
    <div class="gdo-popup saveCartsPopUp">
        <asp:Label ID="lblSavedCartsHelp" runat="server" meta:resourcekey="lblSavedCartsHelp">
        </asp:Label>
    </div>
</asp:Panel>
<asp:Panel ID="pnlSavedCart" runat="server" Style="display: none; width: 300px">
    <asp:UpdatePanel ID="updSavedCart" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-popup saveCartsPopUp" style="max-width: 300px">
                <div class="gdo-float-left gdo-popup-title">
                    <h2>
                        <asp:Label ID="lblSavedCartTitle" runat="server" Text="Save Cart & Start New Order"
                            meta:resourcekey="lblSaveCartTitle"></asp:Label></h2>
                </div>
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblSavedCartMessage1" runat="server" Text="Your current cart has been saved."></asp:Label>
                    <asp:Label runat="server" ID="lblSavedCartMessage2" Text="You will now be redirected to the Online Price List."
                        meta:resourcekey="lblSavedCartMessage2"> </asp:Label>
                </div>
                <div class="gdo-button-margin-rt bttn-recalculate">
                    <myHLControls:DynamicButton ID="btnCancel" runat="server" ButtonType="Back" Text="Cancel"
                        meta:resourcekey="SaveCartCancel" Visible="false" OnClientClick="HideContinueModal();" />
                    <myHLControls:DynamicButton ID="Continue" runat="server" ButtonType="Forward" Text="OK"
                        OnClick="OnContinue" meta:resourcekey="OK" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>