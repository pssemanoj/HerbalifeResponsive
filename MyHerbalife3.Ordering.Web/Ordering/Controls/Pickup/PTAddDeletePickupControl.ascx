<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PTAddDeletePickupControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.PTAddDeletePickupControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:HiddenField ID="hfDiableSavedCheckbox" runat="server" />
<h3>
    <asp:Label ID="lblHeader" runat="server" Text="Pickup Location" Font-Bold="True"
        meta:resourcekey="lblHeader"></asp:Label>
</h3>
<div id="divAddPickUp" runat="server">
    <div>
        <asp:Label runat="server" ID="lblRegion" Text="Region" meta:resourcekey="lblRegion"
            Width="70px"></asp:Label>
        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlRegion" Style="margin: 0px 0px 0px 0px;
            width: 150px" OnSelectedIndexChanged="dnlRegion_SelectedIndexChanged" meta:resourcekey="dnlRegion">
        </asp:DropDownList>
    </div>
    <div id="divLocations" class="ShippingLocations" runat="server">
        <asp:Label ID="lblLocations" runat="server" Text="Please Select a Courier *:" meta:resourcekey="lblLocations"></asp:Label>
        <asp:DataList ID="dlPickupInfo" runat="server" RepeatColumns="2" ItemStyle-VerticalAlign="Top"
            ShowFooter="False" ShowHeader="False" EditItemStyle-BorderColor="#CCCCCC" EditItemStyle-BorderStyle="Solid"
            EditItemStyle-BorderWidth="1" ItemStyle-BorderColor="Silver" ItemStyle-BorderStyle="Solid"
            ItemStyle-BorderWidth="1px" meta:resourcekey="dlPickupInfo">
            <EditItemStyle BorderColor="#CCCCCC" BorderWidth="1px" BorderStyle="Solid"></EditItemStyle>
            <ItemStyle VerticalAlign="Top" BorderColor="Silver" BorderWidth="1px" BorderStyle="Solid">
            </ItemStyle>
            <ItemTemplate>
                <table>
                    <tr>
                        <td>
                            <asp:RadioButton onclick="javascript:PickupSelected(this);" CssClass="NoBorder" ID="rbSelected"
                                runat="server" meta:resourcekey="rbSelected" />
                        </td>
                        <td>
                            <div class="courierImages">
                                <asp:Image ID="Image1" runat="server" ImageUrl="/Content/Global/Products/img/order_icon_HL.png"
                                    meta:resourcekey="Image1Resource1" />
                            </div>
                        </td>
                        <td>
                            <asp:HiddenField ID="lbID" runat="server" Value='<%# Eval("ID") %>' />
                            <asp:Label ID="lbAditianlInfo" runat="server" Text='<%# Eval("Information") %>' />
                            <br />
                            <asp:HiddenField ID="lbFreightCode" runat="server" Value='<%# Eval("FreightCode") %>' />
                            <asp:HiddenField ID="lbWarehouse" runat="server" Value='<%# Eval("Warehouse") %>' />
                            <asp:Label runat="server" ID="lbBranchName" Text='<%# Eval("BranchName") %>' ForeColor="Blue"
                                Font-Bold="True" meta:resourcekey="lbBranchNameResource1"></asp:Label>
                            <br />
                            <asp:BulletedList ID="lbAddress" runat="server" BulletStyle="Disc" DataSource='<%# GetAddress(Eval("Address") as MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01) %>'
                                meta:resourcekey="lbAddressResource1" />
                        </td>
                    </tr>
                </table>
            </ItemTemplate>
        </asp:DataList>
    </div>
    <div>
        <asp:HyperLink ID="lnkSpeedexSite" runat="server" />
    </div>
    <div>
        <asp:Label ID="lblNickname" runat="server" Text="Nickname:" meta:resourcekey="lblNicknameResource1"></asp:Label>
        <asp:TextBox ID="txtNickname" runat="server" Width="386px" meta:resourcekey="txtNicknameResource1"></asp:TextBox>
    </div>
    <div>
        <asp:CheckBox ID="cbSaveThis" runat="server" Text="Save this pickup location preferences"
            OnCheckedChanged="cbSaveThis_CheckedChanged" AutoPostBack="True" meta:resourcekey="cbSaveThisResource1" />
    </div>
    <div>
        <asp:CheckBox ID="cbMakePrimary" runat="server" Text="Make this my primary pickup location preferences"
            meta:resourcekey="cbMakePrimaryResource1" />
    </div>
</div>
<div id="divDeletePickUp" runat="server">
    <table>
        <tr>
            <td colspan="3">
                <h3>
                    <asp:Label ID="lblDeleteHeader" runat="server" meta:resourcekey="lblDeleteHeaderResource1"></asp:Label>
                </h3>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:" meta:resourcekey="lblDeleteNicknameResource1"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteNicknameText" runat="server" meta:resourcekey="lblDeleteNicknameTextResource1"></asp:Label>
            </td>
        </tr>
        <tr id="trLocation" runat="server">
            <td valign="top">
                <asp:Label ID="lblDeleteAddress" runat="server" Text="PickupLocation Name:" meta:resourcekey="lblDeleteAddressResource1"></asp:Label>
            </td>
            <td id="colDeletePickUp" runat="server" valign="top">
                <asp:Label ID="lblName" runat="server" meta:resourcekey="lblName"></asp:Label>
                &nbsp;
            </td>
        </tr>
        <tr id="trPrimary" runat="server">
            <td>
                <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:" meta:resourcekey="lblDeleteIsPrimary"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDeleteIsPrimaryText" runat="server" meta:resourcekey="lblDeleteIsPrimaryText"></asp:Label>
            </td>
        </tr>
    </table>
</div>
<asp:Label ID="lblErrors" runat="server" ForeColor="Red" meta:resourcekey="lblErrors"></asp:Label>
<cc1:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked"
    Text="Cancel" ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelResource1"></cc1:OvalButton>
<cc1:OvalButton ID="btnContinue" runat="server" Coloring="Silver" OnClick="ContinueChanges_Clicked"
    Text="Continue" ArrowDirection="" IconPosition="" meta:resourcekey="btnContinueResource1"></cc1:OvalButton>