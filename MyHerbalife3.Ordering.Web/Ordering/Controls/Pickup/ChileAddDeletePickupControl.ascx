<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChileAddDeletePickupControl.ascx.cs" 
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.ChileAddDeletePickupControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>


<asp:HiddenField ID="hfDiableSavedCheckbox" runat="server" />
<asp:HiddenField ID="hfCourierType" runat="server" />

<table>
    <tr>
        <td>
            <asp:Label ID="lblHeader" runat="server" Text="Pickup Location"
                Font-Bold="True" meta:resourcekey="lblHeaderResource1"></asp:Label>
        </td>
    </tr>
    
</table>
<div id="divAddPickUp" runat="server">
    <table>
        <tr>
            <td>
                <div>
                    <span>
                        <asp:Label runat="server" ID="lblState" Text="Comuna&nbsp;&nbsp;" Width="70px"></asp:Label>
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" Style="margin: 0px 0px 0px 0px; width: 150px"
                            OnSelectedIndexChanged="dnlState_SelectedIndexChanged"
                            meta:resourcekey="dnlState">
                        </asp:DropDownList>
                    </span>
                </div>
                <div>
                    <span>
                        <asp:Label runat="server" ID="lblCitybox" Text="Citybox" 
                            Width="70px"></asp:Label>
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCitybox" Style="margin: 0px 0px 0px 0px; width: 150px"
                            OnSelectedIndexChanged="dnlCitybox_SelectedIndexChanged">
                        </asp:DropDownList>
                    </span>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div id="divLocations" class="ShippingLocations" runat="server">
     <p id="pAddress" runat="server">
                </p>
</div>

            </td>

        </tr>
       <%--<tr>
            <td>
                <asp:CheckBox ID="cbSaveThis" runat="server" Text="Save this pickup location preferences" OnCheckedChanged="cbSaveThis_CheckedChanged"
                    AutoPostBack="True" Visible="False"
                    meta:resourcekey="cbSaveThisResource1" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:CheckBox ID="cbMakePrimary" runat="server" Visible="False"
                    Text="Make this my primary pickup location preferences" meta:resourcekey="cbMakePrimaryResource1" />
            </td>
        </tr>--%>

    </table>
</div>

<div id="divDeletePickUp" runat="server">
    <table>
        <tr>
            <td colspan="3">
                <h3>
                    <asp:Label ID="lblDeleteHeader" runat="server"
                        meta:resourcekey="lblDeleteHeaderResource1"></asp:Label></h3>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:"
                    meta:resourcekey="lblDeleteNicknameResource1"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteNicknameText" runat="server"
                    meta:resourcekey="lblDeleteNicknameTextResource1"></asp:Label>
            </td>
        </tr>
        <tr id="trLocation" runat="server">
            <td valign="top">
                <asp:Label ID="lblDeleteAddress" runat="server" Text="PickupLocation Name:"
                    meta:resourcekey="lblDeleteAddressResource1"></asp:Label>
            </td>
            <td id="colDeletePickUp" runat="server" valign="top">
                <asp:Label ID="lblName" runat="server" meta:resourcekey="lblNameResource1"></asp:Label>
                &nbsp;
            </td>
        </tr>
        <tr id="trPrimary" runat="server">
            <td>
                <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:"
                    meta:resourcekey="lblDeleteIsPrimaryResource1"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteIsPrimaryText" runat="server"
                    meta:resourcekey="lblDeleteIsPrimaryTextResource1"></asp:Label>
            </td>
        </tr>
    </table>
</div>
<asp:Label ID="lblErrors"
    runat="server" ForeColor="Red" meta:resourcekey="lblErrorsResource1"></asp:Label>
<cc1:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked"
    Text="Cancel" ArrowDirection="" IconPosition=""
    meta:resourcekey="btnCancelResource1"></cc1:OvalButton>
<cc1:OvalButton ID="btnContinue" runat="server"
    Coloring="Silver" OnClick="ContinueChanges_Clicked"
    Text="Continue" ArrowDirection="" IconPosition=""
    meta:resourcekey="btnContinueResource1"></cc1:OvalButton>