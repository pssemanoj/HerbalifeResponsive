<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ARAddDeletePickupControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.ARAddDeletePickupControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:HiddenField ID="hfDiableSavedCheckbox" runat="server" />
<div>
    <div>
        <div>
            <asp:Label ID="lblHeader" runat="server" Text="Pickup Location" Font-Bold="True" meta:resourcekey="lblHeaderResource1"></asp:Label><br />
            <asp:Label ID="lblSubHeader" runat="server" Text="You can see the locations on this map" meta:resourcekey="lblSubHeaderResource1"></asp:Label>
        </div>
        <div runat="server" id="trStateRadio" style="margin: 10px 0 10px 30px;">
            <span>
                <asp:RadioButton ID="rbState" runat="server" GroupName="StateAndPostalCode"
                    OnCheckedChanged="rbState_CheckedChanged" AutoPostBack="True"
                    Text="Search by State, County and Province"
                    meta:resourcekey="rbStateResource1" />
            </span>
            <br />
            <span>
                <asp:RadioButton ID="rbPostalCode" runat="server" GroupName="StateAndPostalCode"
                    OnCheckedChanged="rbPostalCode_CheckedChanged" AutoPostBack="True"
                    Text="Search by Postal Code"
                    meta:resourcekey="rbPostalCodeResource1" />
            </span>
            <br />
            <span>
                <asp:RadioButton ID="rbHerbalifeCenters" runat="server" GroupName="StateAndPostalCode"
                    OnCheckedChanged="rbHerbalifeCenters_CheckedChanged"
                    AutoPostBack="True" Text="Herbalife Centers"
                    meta:resourcekey="rbHerbalifeCentersResource1" />
            </span>
            <br />
        </div>
    </div>
    <div id="divAddPickUp" runat="server">
        <div>
            <div id="divSearchByState" runat="server">
                <div>
                    <span>
                        <asp:Label runat="server" ID="lblState" Text="Estado&nbsp;&nbsp;" meta:resourcekey="lbStateResource1" Width="70px"></asp:Label>
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" Style="margin: 0px 0px 0px 0px; width: 150px"
                            OnSelectedIndexChanged="dnlState_SelectedIndexChanged" meta:resourcekey="dnlState">
                        </asp:DropDownList>
                    </span>
                </div>
                <div>
                    <span>
                        <asp:Label runat="server" ID="lblMunicipal" Text="Municipio" meta:resourcekey="lnMunicipalResource1" Width="70px"></asp:Label>
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlMunicipal" Style="margin: 0px 0px 0px 0px; width: 150px"
                            OnSelectedIndexChanged="dnlMunicipal_SelectedIndexChanged" meta:resourcekey="dnlMunicipal">
                        </asp:DropDownList>
                    </span>
                </div>
                <div>
                    <span>
                        <asp:Label runat="server" ID="lblColonia" Text="Colonia&nbsp;&nbsp;" meta:resourcekey="lbColoniaResource1" Width="70px"></asp:Label>
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlTown" Style="margin: 0px 0px 0px 0px; width: 150px"
                            meta:resourcekey="dnlTown" OnSelectedIndexChanged="dnlTown_SelectedIndexChanged">
                        </asp:DropDownList>
                    </span>
                </div>
            </div>
            <div class="mxpickup-slect" id="divSearchByZip" runat="server">
                <div>
                    <asp:Label ID="lblPostalCode" runat="server" Text="Postal Code*:" meta:resourcekey="lblPostalCodeResource1"></asp:Label>
                    <asp:TextBox ID="txtPostalCode" runat="server" OnTextChanged="PostalCode_TextChanged" AutoPostBack="True" meta:resourcekey="txtPostalCodeResource1"></asp:TextBox>
                </div>
            </div>
            <div>
                <div id="divLocations" style="width: auto; overflow: Auto" class="ShippingLocations" runat="server">
                    <span>
                        <asp:Label ID="lblLocations" runat="server" Text="Please select a location*:" meta:resourcekey="lblLocationsResource1"></asp:Label>
                        <asp:DataList ID="dlPickupInfo" runat="server"
                            RepeatColumns="1" RepeatDirection="Horizontal" ShowFooter="False"
                            ShowHeader="False" meta:resourcekey="dlPickupInfoResource1">
                            <ItemTemplate>
                                <div style="margin-top: 10px; display: inline-flex;">
                                    <span style="display: inline-block; vertical-align: middle; line-height: 80px;">
                                        <asp:RadioButton onclick="javascript:PickupSelected(this);" CssClass="NoBorder" ID="rbSelected" runat="server" meta:resourcekey="rbSelectedResource1" />
                                    </span>
                                    <span style="display: inline-block; vertical-align: middle; line-height: 93.5px;">
                                        <asp:Image ID="Image1"
                                            Visible='<%# (bool)Eval("IsPickup") == true %>'
                                            runat="server" ImageUrl='<%# GetImage((string) Eval("Description")) %>'
                                            meta:resourcekey="Image1Resource1" />
                                    </span>
                                    <span style="padding-left: 10px;">
                                        <asp:HiddenField ID="lbID" runat="server" Value='<%# Eval("ID") %>' />
                                        <asp:HiddenField ID="lbWarehouse" runat="server" Value='<%# Eval("Warehouse") %>' />
                                        <asp:Label Visible='<%# (bool)Eval("IsPickup") == true %>' runat="server" ID="lbBranchName"
                                            Text='<%# Eval("BranchName") %>' Font-Bold="True" meta:resourcekey="lbBranchNameResource1"></asp:Label>

                                        <asp:BulletedList ID="lbAddress" runat="server" BulletStyle="Disc" CssClass="ar-store-address"
                                            DataSource='<%# GetAddress(Eval("Address") as MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01) %>'
                                            meta:resourcekey="lbAddressResource1" />
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:DataList>
                    </span>
                </div>
            </div>
            <div>
                <asp:Label ID="lblNicknameDesc" runat="server" Text="Set a nickname to easily identify this location." meta:resourcekey="lblNicknameDescResource1"></asp:Label><br />
                <asp:Label ID="lblNickname" runat="server" Text="Nickname:" meta:resourcekey="lblNicknameResource1"></asp:Label>
                <div style="overflow: hidden;">
                    <asp:TextBox ID="txtNickname" runat="server" Width="100%" meta:resourcekey="txtNicknameResource1"></asp:TextBox>
                </div>
            </div>
            <div>
                <asp:CheckBox ID="cbSaveThis" runat="server" Text="Save this pickup location preferences" OnCheckedChanged="cbSaveThis_CheckedChanged" AutoPostBack="True" meta:resourcekey="cbSaveThisResource1" />
            </div>
            <div>
                <asp:CheckBox ID="cbMakePrimary" runat="server" Text="Make this my primary pickup location preferences" meta:resourcekey="cbMakePrimaryResource1" />
            </div>
        </div>
    </div>
    <div id="divDeletePickUp" runat="server">
        <div>
            <div>

                <h3>
                    <asp:Label ID="lblDeleteHeader" runat="server"
                        meta:resourcekey="lblDeleteHeaderResource1"></asp:Label></h3>

            </div>
            <div>
                <div>
                    <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:"
                        meta:resourcekey="lblDeleteNicknameResource1"></asp:Label>
                </div>
                <div>
                    <asp:Label ID="lblDeleteNicknameText" runat="server"
                        meta:resourcekey="lblDeleteNicknameTextResource1"></asp:Label>
                </div>
            </div>
            <div id="trLocation" runat="server">
                <div>
                    <asp:Label ID="lblDeleteAddress" runat="server" Text="PickupLocation Name:"
                        meta:resourcekey="lblDeleteAddressResource1"></asp:Label>
                </div>
                <div id="colDeletePickUp" runat="server" valign="top">
                    <asp:Label ID="lblName" runat="server" meta:resourcekey="lblNameResource1"></asp:Label>
                    &nbsp;
                </div>
            </div>
            <div id="trPrimary" runat="server">
                <div>
                    <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:"
                        meta:resourcekey="lblDeleteIsPrimaryResource1"></asp:Label>
                </div>
                <div>
                    <asp:Label ID="lblDeleteIsPrimaryText" runat="server"
                        meta:resourcekey="lblDeleteIsPrimaryTextResource1"></asp:Label>
                </div>
            </div>
        </div>
    </div>
    <asp:Label ID="lblErrors" runat="server" ForeColor="Red" meta:resourcekey="lblErrorsResource1"></asp:Label>
    <cc1:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked"
        Text="Cancel" ArrowDirection="" IconPosition=""
        meta:resourcekey="btnCancelResource1" CssClass="backward"></cc1:OvalButton>
    <cc1:OvalButton ID="btnContinue" runat="server"
        Coloring="Silver" OnClick="ContinueChanges_Clicked"
        Text="Continue" ArrowDirection="" IconPosition=""
        meta:resourcekey="btnContinueResource1" CssClass="forward"></cc1:OvalButton>
    <div class="clear"></div>
</div>
