<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MexicoOrderQuickView.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.MexicoOrderQuickView" %>
<%@ Register Src="ShippingInfoControl.ascx" TagName="ShippingInfoControl" TagPrefix="hrblShippingInfoControl" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<script type="text/javascript">

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        try {
            // clear the width property
            if (document.getElementById('<%= upOrderQuickView.ClientID %>').style.filter) {
                document.getElementById('<%= upOrderQuickView.ClientID %>').style.filter = "";
            }
        }
        catch (e) {
        }
    }
</script>
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>
<div id="gdo-right-column-mexicoorderquickview" class="gdo-right-column-tbl shipping-pickup-module">
    <div id="QuickViewFrame">
        <div>
            <div class="gdo-right-column-header">
                <h3>
                    <asp:Label runat="server" ID="TextOrderQuickView" Text="Order Quick View" meta:resourcekey="TextOrderQuickViewResource1">
                    </asp:Label></h3>
            </div>
            <div style="display: none;">
                <a href="#TBH_inline?height=90&amp;width=370&amp;inlineId=tooltipCountry" title=""
                    class="thickbox" tabindex="1">
                    <img alt="question" src="/Content/Global/img/gdo/icons/question-mark-blue.png" class="gdo-question-mark-blue" /></a></div>
        </div>
        <div class="gdo-clear gdo-horiz-div">
        </div>
        <div>
            <h4>
                <asp:Label runat="server" ID="ShippingPickupOptions" Text="Shipping / Pickup Options"
                    meta:resourcekey="ShippingPickupOptionsResource1"></asp:Label></h4>
        </div>
        <div>
            <cc1:ContentReader ID="DeliveryMethodContentReader" runat="server" ContentPath="DeliveryMethod.html"
                SectionName="ordering" ValidateContent="true" />
        </div>
        <asp:Panel runat="server" ID="pnlReadonlyDeliveryOptionSelection" meta:resourcekey="pnlReadonlyDeliveryOptionSelectionResource1">
            <div class="gdo-input-container">
                <div class="gdo-right-column-labels">
                    <asp:Label runat="server" ID="TypeReadonly" Text="Type:" meta:resourcekey="TypeReadonlyResource1"></asp:Label>
                </div>
                <asp:Label runat="server" ID="DeliveryTypeReadonly" meta:resourcekey="DeliveryTypeReadonlyResource1"></asp:Label>
            </div>
            <div class="gdo-input-container">
                <div class="gdo-right-column-labels">
                    <asp:Label runat="server" ID="NickNameReadonly" Text="Nick Name:" meta:resourcekey="NickNameReadonlyResource1"></asp:Label>
                </div>
                <asp:Label runat="server" ID="DropdownNickNameReadonly" meta:resourcekey="DropdownNickNameReadonlyResource1"></asp:Label>
            </div>
            <div class="gdo-shipto-container">
                <div class="gdo-right-column-labels">
                    <asp:Label runat="server" ID="lbShiptoOrPickupReadonly" Text="Ship To:" meta:resourcekey="lbShiptoOrPickupReadonlyResource1"></asp:Label></div>
                <div class="gdo-right-column-text" runat="server" id="divAddreaddReadOnly">
                </div>
            </div>
        </asp:Panel>
        <progress:UpdatePanelProgressIndicator ID="progressOrderQuickView" runat="server"
            TargetControlID="upOrderQuickView" />
        <asp:UpdatePanel ID="upOrderQuickView" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel runat="server" ID="pnlDeliveryOptionSelection" meta:resourcekey="pnlDeliveryOptionSelectionResource1">
                    <div>
                        <p class="left">
                            <asp:Label runat="server" ID="errDRFraud" CssClass="red" Visible="False" />
                        </p>
                    </div>
                    <div class="gdo-input-container" runat="server" id="divDeliveryOptionSelection">
                        <div class="gdo-right-column-labels">
                            <asp:Label runat="server" ID="Type" Text="Type:" meta:resourcekey="TypeResource1"></asp:Label></div>
                        <div class="gdo-select-box">
                            <asp:DropDownList AutoPostBack="True" runat="server" ID="DeliveryType" OnSelectedIndexChanged="OnDeliveryTypeChanged"
                                meta:resourcekey="DeliveryTypeResource1">
                                <asp:ListItem Text="Shipping" Value="Shipping" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                <asp:ListItem Text="Pickup" Value="Pickup" meta:resourcekey="ListItemResource2"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div runat="server" id="divAddAddressLink">
                        <asp:LinkButton runat="server" ID="lnAddAddress" Text="Add Address" OnClick="AddAddressClicked"
                            meta:resourcekey="lnAddAddressResource1"></asp:LinkButton>
                    </div>
                    <div runat="server" id="divNicknameInfoAndLink">
                        <div class="gdo-input-container">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="NickName" Text="Nick Name:" meta:resourcekey="NickNameResource1"></asp:Label></div>
                            <div class="gdo-select-box">
                                <asp:DropDownList AutoPostBack="True" runat="server" ID="DropdownNickName" DataTextField="DisplayName"
                                    DataValueField="ID" OnDataBound="OnNickName_Databind" OnSelectedIndexChanged="OnNickNameChanged"
                                    meta:resourcekey="DropdownNickNameResource1">
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="gdo-shipto-container">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="lbShiptoOrPickup" Text="Ship To:" meta:resourcekey="lbShiptoOrPickupResource1"></asp:Label></div>
                            <p runat="server" visible="true" id="pAddress">
                            </p>
                        </div>
                        <div runat="server" id="divLinks" class="gdo-edit-links-conatainer">
                            <asp:LinkButton runat="server" ID="LinkAdd" Text="Add" OnClick="AddClicked" meta:resourcekey="LinkAddResource1"></asp:LinkButton>&nbsp;
                            <span class="gdo-right-column-text">|</span>&nbsp;
                            <asp:LinkButton runat="server" ID="LinkEdit" Text="Edit" OnClick="EditClicked" meta:resourcekey="LinkEditResource1"></asp:LinkButton>&nbsp;
                            <span class="gdo-right-column-text">|</span>&nbsp;
                            <asp:LinkButton runat="server" ID="LinkDelete" Text="Delete" OnClick="DeleteClicked"
                                meta:resourcekey="LinkDeleteResource1"></asp:LinkButton>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="pnlInventoryView" meta:resourcekey="pnlInventoryViewResource1">
                    <table width="100%" border="0" cellspacing="0" cellpadding="0">
                        <tr>
                            <td>
                                <div class="gdo-clear gdo-horiz-div">
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div class="gdo-input-container">
                                    <asp:Label CssClass="gdo-right-column-labels" runat="server" ID="InventoryView" Text="Inventory View"
                                        meta:resourcekey="InventoryViewResource2"></asp:Label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div runat="server" class="gdo-radio" id="divInventoryView">
                                    <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                        <tr>
                                            <td>
                                                <asp:RadioButton AutoPostBack="true" OnCheckedChanged="RBListInventoryView_OnSelectedIndexChanged"
                                                    ID="rbShowAll" runat="server" GroupName="InventoryView" />
                                            </td>
                                            <td>
                                                <asp:Label runat="server" CssClass="gdo-right-column-text" Text="Show All Inventory"
                                                    meta:resourcekey="ListItemResource3"></asp:Label>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:RadioButton AutoPostBack="true" OnCheckedChanged="RBListInventoryView_OnSelectedIndexChanged"
                                                    ID="rbShowOnlyAvail" runat="server" GroupName="InventoryView" />
                                            </td>
                                            <td>
                                                <asp:Label runat="server" CssClass="gdo-right-column-text" Text="Show Only Available Inventory"
                                                    meta:resourcekey="ListItemResource4"></asp:Label>
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ucShippingInfoControl" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <progress:UpdatePanelProgressIndicator ID="progressShippingInfoControl" runat="server"
        TargetControlID="ppShippingInfoControl" />
    <asp:UpdatePanel ID="ppShippingInfoControl" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl" runat="server" />
            <asp:Panel runat="server" ID="pnlShippingMethod" meta:resourcekey="pnlShippingMethodResource1">
            </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="LinkEdit" />
            <asp:AsyncPostBackTrigger ControlID="LinkDelete" />
            <asp:AsyncPostBackTrigger ControlID="LinkAdd" />
            <asp:AsyncPostBackTrigger ControlID="lnAddAddress" />
            <asp:AsyncPostBackTrigger ControlID="DropdownNickName" />
            <asp:AsyncPostBackTrigger ControlID="DeliveryType" />
        </Triggers>
    </asp:UpdatePanel>
    <!-- TOOLTIP COUNTRY -->
    <div style="display: none" id="tooltipCountry">
        <div class="tooltip">
            <p>
                Sample tooltip. For the real tooltip, the HTML filename and the ID referenced by
                the tooltip should be the same as what Webpro supplies us with.<br />
                So the contract with WebPro is made up of 1) HTML Filename 2) An ID inside the HTML
                file that .NET will reference. 3) Resx based localizations<br />
                Also the height of the tooltip is depenedent on its content and needs to be set
                by .NET once the real content is published by WebPro.</p>
        </div>
    </div>
    <!-- END TOOLTIP COUNTRY -->
</div>