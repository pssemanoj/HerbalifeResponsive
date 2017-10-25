<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddressRestrictionPopUp.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.AddressRestrictionPopUp" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:UpdatePanel ID="UpdateAddressRestrictionPopUp" runat="server" UpdateMode="Always">
    <ContentTemplate>
       <ajaxToolkit:ModalPopupExtender ID="AddressRestrictionPopupExtender" runat="server" TargetControlID="AddressRestrictionFakeTarget"
            PopupControlID="pnldupeAddressRestriction" CancelControlID="AddressRestrictionFakeTarget" BackgroundCssClass="modalBackground" 
            DropShadow="false" />
             <asp:Button ID="AddressRestrictionFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
        <asp:Panel ID="pnldupeAddressRestriction" runat="server" Style="display: none">
            <div class="gdo-popup confirmCancel">
                <div class="gdo-form-label-left">
                    <asp:Label ID="lblRemindltr" runat="server" Text="Message"  meta:resourcekey="MessageResource"></asp:Label>

                </div>
                <div class="gdo-form-label-left confirmButtons">
                   <cc1:DynamicButton ID="btnUpdate" runat="server" Text="Update" ButtonType="Forword" OnClick="btnUpdate_Click"   meta:resourcekey="UpdateResource"/>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>

</asp:UpdatePanel>
