<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectDeliveryOptionType.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.SelectDeliveryOptionType" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit, Version=3.0.11119.25533, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e" %>
<asp:Panel ID="pnlSelectDeliveryOptionType" runat="server" Style="display: none;
    background-color: White">
    <asp:UpdatePanel ID="upSelectDeliveryOptionType" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="TB_window" style="margin-left: 0px; margin-top: 0px; display: block;">
                <div id="TB_ajaxContent">
                    <div class="gdo-popup">
                        <div class="gdo-input-container">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="Type" Text="Type:"></asp:Label></div>
                            <div class="gdo-select-box">
                                <asp:DropDownList AutoPostBack="True" runat="server" ID="DeliveryType" OnSelectedIndexChanged="OnDeliveryTypeChanged" name="DeliveryType">
                                    <asp:ListItem Text="Select Type" Value="Type"></asp:ListItem>
                                    <asp:ListItem Text="Shipping" Value="Shipping"></asp:ListItem>
                                    <asp:ListItem Text="Pickup" Value="Pickup"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<ajaxToolkit:ModalPopupExtender ID="popup_SelectDeliveryOptionType" runat="server"
    TargetControlID="lnkHidden" PopupControlID="pnlSelectDeliveryOptionType" CancelControlID="lnkHidden"
    BackgroundCssClass="modalBackground">
</ajaxToolkit:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;"></asp:LinkButton>
