<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UpdatePanelProgressIndicator.ascx.cs"
    Inherits="HL.MyHerbalife.Web.Controls.Content.UpdatePanelProgressIndicator" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit, Version=3.0.11119.25533, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e" %>
<ajaxToolkit:UpdatePanelAnimationExtender ID="aeProgress" runat="server">
    <Animations>
        <OnUpdating>
            <Condition ConditionScript="{alwaysUpdate} || checkParentControlExists(currentPostBackElement, '{targetControlClientID}');">
                <Parallel>
                    <ScriptAction Script="aeProgress_onUpdating('{progressDivClientID}', '{targetControlClientID}');" />
                    <FadeOut Duration="0.5" MinimumOpacity="30" Fps="24" />                
                </Parallel>
            </Condition>
        </OnUpdating>
        <OnUpdated>
            <Condition ConditionScript="{alwaysUpdate} || checkParentControlExists(currentPostBackElement, '{targetControlClientID}');">
                <Parallel>
                    <FadeIn Duration="0.5" Fps="24" />
                    <ScriptAction Script="aeProgress_onUpdated('{progressDivClientID}');" /> 
                </Parallel>
            </Condition>
        </OnUpdated>
    </Animations>
</ajaxToolkit:UpdatePanelAnimationExtender>
<%-- Panel to be overlaid on the target control. --%>
<asp:Panel ID="updateProgressDiv" runat="server" Style="display: none; z-index: 100;
    background-image: url('/SharedUI/Images/icons/LoadingGreenCircle.gif'); background-repeat: no-repeat;
    background-position: center;">
</asp:Panel>
