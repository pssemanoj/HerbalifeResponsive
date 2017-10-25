<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PopUpMessage.ascx.cs" 
Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PopUpMessage" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="AjaxToolkit" %>
<%@ Register TagPrefix="uc1" Namespace="MyHerbalife3.Shared.UI" Assembly="MyHerbalife3.Shared.UI" %>
<style type="text/css">
.modalBackground 
{ 
position:fixed; 
height:100%; 
width:100%; 
top:0px; 
left:0px; 
background-color:#000000; 
filter:alpha(opacity=50); 
-moz-opacity:.50; 
opacity:.50; 
z-index:50; 
} 


.msg_box_container 
{ 
position:fixed; 
background-color:#FFFFFF; 
border:1px solid #999999; 
z-index:50; 
left:20%; 
right:20%; 
top:20%;
    width: 400px;
} 

.popupHeader {
    background-color: #dcdcdc;
    font-weight: bold;
    font-size: 12pt;
    padding: 5px;
    border: solid 1px #dcdcdc;
}
</style>
<asp:Panel ID="pnlMessageBox" runat="server" Style="display: none; background-color: White; margin:10px; border: 5px solid silver"
    meta:resourcekey="pnlMessageBoxResource1">
    <asp:UpdatePanel ID="upMessageBox" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div>
                <asp:Label Font-Bold="true" runat="server" ID="txtTitle" meta:resourcekey="txtTitleResource1"></asp:Label>
                <br />
                <br />
                <p runat="server" id="txtMessage" style="max-width: 400px">
                </p>
                <div style="float: right">
                    <uc1:OvalButton ID="BtnYes" runat="server" meta:resourcekey="Yes" Coloring="Silver"
                        Text="OK" OnClick="OnYes" ArrowDirection="" IconPosition="" IconType="" />
                    
                    <uc1:OvalButton ID="BtnNo" runat="server" meta:resourcekey="No" Coloring="Silver"
                        Text="OK" OnClick="OnNo" ArrowDirection="" IconPosition="" IconType="" />

                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<AjaxToolkit:ModalPopupExtender ID="popup_MessageBox" runat="server" TargetControlID="lnkHidden"
    PopupControlID="pnlMessageBox" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
    DynamicServicePath="" Enabled="True" DropShadow="False">
</AjaxToolkit:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>
