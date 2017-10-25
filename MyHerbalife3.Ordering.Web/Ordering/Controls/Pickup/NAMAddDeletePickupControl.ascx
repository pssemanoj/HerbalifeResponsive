<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NAMAddDeletePickupControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.NAMAddDeletePickupControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div>
    
</div>
<h3>
    <asp:Label ID="lblHeader" runat="server" Text="Pickup Location" Font-Bold="True" meta:resourcekey="lblHeaderResource"></asp:Label>
</h3>

<div>
    <asp:Label ID="lblErrors" runat="server" ForeColor="Red" meta:resourcekey="lblErrors"></asp:Label>
</div>

<div id="divAddPickUp" runat="server">
    <div id="dvFragment" runat="server">
        <cc1:ContentReader ID="locationInfo" runat="server" ContentPath="SelectPickupLocationInfo.html"
        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
    
    <div id="divCityLookup" runat="server" Visible="False">
        <asp:Label runat="server" ID="lblState" Text="Enter state:" meta:resourcekey="lblStateResource"
            Width="70px"></asp:Label>
        <asp:DropDownList AutoPostBack="False" runat="server" ID="dnlState" Style="margin: 0px 0px 0px 0px;
            width: 150px" meta:resourcekey="dnlStateResource">
        </asp:DropDownList>
        <br/>
        <asp:Label runat="server" ID="lblCity" Text="Enter city:" meta:resourcekey="lblCityResource"
            Width="70px"></asp:Label>
        <asp:TextBox ID="txtCity" runat="server" AutoPostBack="True"
            meta:resourcekey="txtCityResource"></asp:TextBox>
        <br/>
    </div>    
    <div id="divZipLookup" runat="server" class="fedex-zipcode">
        <asp:Label runat="server" ID="lblZipCode" Text="Enter ZIP code:" meta:resourcekey="lblZipCodeResource" AssociatedControlID="txtPostalCode"></asp:Label>
        <asp:TextBox ID="txtPostalCode" runat="server" AutoPostBack="True"
            meta:resourcekey="txtPostalCodeResource"></asp:TextBox>    
    </div>
    
    <div class="left">
        <cc2:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked" 
            Text="Cancel" ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelResource" CssClass="backward"></cc2:OvalButton>
    </div>
    <div class="right">
        <cc2:OvalButton ID="btnContinue" runat="server" Coloring="Silver" OnClick="ContinueChanges_Clicked" CommandArgument="Add"
            Text="Continue" ArrowDirection="" IconPosition="" meta:resourcekey="btnContinueResource" CssClass="forward"></cc2:OvalButton>
    </div>
    <div class="clear"></div>
    <div id="divLocations" class="" runat="server">
        <div style="background-color: #b0b0b0;height: 1px;"></div>
        <asp:Label ID="lblLocations" runat="server" Text="There are X locations within the area"></asp:Label>
        <div id="locationResult" runat="server">
            <div>
                <asp:RadioButtonList runat="server" ID="rblDistanceRadio" Style="width: 80%" RepeatDirection="Horizontal"
                     AutoPostBack="True" OnSelectedIndexChanged="rblDistanceRadio_OnSelectedIndexChanged">
                    <asp:ListItem Selected="True" Text="1" Value="5" meta:resourcekey="ListItemResource1"></asp:ListItem>
                    <asp:ListItem Text="10" Value="2" meta:resourcekey="ListItemResource2"></asp:ListItem>
                    <asp:ListItem Text="15" Value="3" meta:resourcekey="ListItemResource3"></asp:ListItem>
                    <asp:ListItem Text="20" Value="4" meta:resourcekey="ListItemResource4"></asp:ListItem>
                </asp:RadioButtonList>
                
                <asp:LinkButton ID="lnkViewLocations" runat="server"
                    meta:resourcekey="lblLocationResource" Text="Locations" OnClick="lnkViewLocations_Click"></asp:LinkButton>      
                |
                <asp:LinkButton ID="lnkViewMap" runat="server" OnClick="lnkViewMap_OnClick"
                    meta:resourcekey="lblLocationsMapResource" Text="View map"></asp:LinkButton>                                
            </div>
        
            <div>
                <div class="clear"></div>
                <div style="display: inline-block" id="dvLocations" runat="server">
                    <asp:GridView runat="server" ID="grdPickupInfo" AllowPaging="False" AllowSorting="False" 
                        AutoGenerateColumns="False" OnRowCommand="GrdLocation_RowCommand"
                        DataKeyNames="ID,DisplayName,BranchName" PagerStyle-HorizontalAlign="Right"
                        CssClass="gdo-table-width-120 "  EnableModelValidation="True">
                        <PagerSettings Visible="False"></PagerSettings>
                        <PagerStyle HorizontalAlign="Right"></PagerStyle>
                        <RowStyle CssClass="gdo-row-even gdo-body-text" />
                        <AlternatingRowStyle CssClass="gdo-row-odd gdo-body-text" />
                        <HeaderStyle BorderStyle="None" CssClass="gdo-table-header" />
                        <Columns>
                            <asp:TemplateField HeaderText="Location Office" meta:resourcekey="locationOfficeResource">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <span>
                                        <strong><asp:Label ID="lblNameDescription" runat="server" Text='<%# Bind("DisplayName") %>'></asp:Label></strong>
                                    </span>
                                    <br/>
                                    <p id="address" runat="server">
                                        <asp:Label ID="lblName" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                                        <br/>
                                        <asp:Label ID="lblStreet" runat="server" Text='<%# Bind("Address.Line1") %>'></asp:Label>
                                        <br/>
                                        <asp:Label ID="lblCity1" runat="server" Text='<%# Bind("Address.City") %>'></asp:Label>,
                                        <asp:Label ID="lblState" runat="server" Text='<%# Bind("Address.StateProvinceTerritory") %>'></asp:Label>
                                        &nbsp;
                                        <asp:Label ID="lblZipCode1" runat="server" Text='<%# Bind("Address.PostalCode") %>'></asp:Label>
                                        <br/>
                                        <asp:Label ID="lblPhone" runat="server" Text='<%# Bind("Phone") %>'></asp:Label>
                                    </p>
                                    <span>
                                        <asp:Label ID="lblDistance" runat="server" Text='<%# Eval("Distance","{0:N2}") %>' Visible='<%# Bind("HasDistance") %>'></asp:Label>
                                        <asp:Label ID="lblUnit" runat="server" Text='<%# Bind("Unit") %>' Visible='<%# Bind("HasDistance") %>'></asp:Label>
                                        <asp:Label ID="lblSpace" runat="server" Text=" | " Visible='<%# Bind("HasDistance") %>'></asp:Label>
                                        <asp:LinkButton ID="lnkMap" runat="server" CommandName="DisplayCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                            meta:resourcekey="lblMapResource" Text="View map"></asp:LinkButton>                                
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Availability" meta:resourcekey="availabilityResource">
                                <ItemStyle VerticalAlign="Top" />
                                <ItemTemplate>
                                    <asp:Label ID="lblAvailability" runat="server" Text='<%# Bind("AdditionalInformation") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderStyle HorizontalAlign="Center"/>
                                <ItemStyle VerticalAlign="Top" HorizontalAlign="Center"/>
                                <HeaderTemplate>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <cc2:OvalButton ID="btnSelect" runat="server" Coloring="Silver" CommandName="SelectCommand" CommandArgument="<%# Container.DataItemIndex %>"
                                        Text="Select" ArrowDirection="" IconPosition="" meta:resourcekey="btnSelectResource" CssClass="forward"/>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div id="displayMap" runat="server" style="display: none">
                    
                    <asp:PlaceHolder runat="server" ID="phMap" />
                </div>
                <div class="clear"></div>
            </div>
        </div>
    </div>
    
    <div id="divSaveOptions" class="" runat="server">
        <div>
            <asp:Label ID="lblNickname" runat="server" Text="Nickname:" meta:resourcekey="lblNicknameResource"></asp:Label>
            <asp:TextBox ID="txtNickname" runat="server" Width="386px" meta:resourcekey="txtNicknameResource"></asp:TextBox>
        </div>
        <div>
            <asp:CheckBox ID="cbSaveThis" runat="server" Text="Save this pickup location preferences"
                OnCheckedChanged="cbSaveThis_CheckedChanged" AutoPostBack="True" meta:resourcekey="cbSaveThisResource" />
        </div>
        <div>
            <asp:CheckBox ID="cbMakePrimary" runat="server" Text="Make this my primary pickup location preferences"
                meta:resourcekey="cbMakePrimaryResource" />
        </div>        
    </div>
</div>

<div id="divDeletePickUp" runat="server">
    <div>
        <h3>
            <asp:Label ID="lblDeleteHeader" runat="server" meta:resourcekey="lblDeleteHeaderResource"></asp:Label>
        </h3>        
    </div>
    <div>
        <asp:Label ID="lblDeleteNickname" runat="server" Text="Nickname:" meta:resourcekey="lblDeleteNicknameResource"></asp:Label>
        <asp:Label ID="lblDeleteNicknameText" runat="server"></asp:Label>
    </div>
    <div id="divLocation" runat="server">
        <asp:Label ID="lblDeleteAddress" runat="server" Text="PickupLocation Name:" meta:resourcekey="lblDeleteAddressResource"></asp:Label>
        <asp:Label ID="lblName" runat="server"></asp:Label>
    </div>
    <div id="divPrimary" runat="server">
        <asp:Label ID="lblDeleteIsPrimary" runat="server" Text="Primary:" meta:resourcekey="lblDeleteIsPrimary"></asp:Label>
        <asp:Label ID="lblDeleteIsPrimaryText" runat="server" meta:resourcekey="lblDeleteIsPrimaryText"></asp:Label>
    </div>

    <div>    
        <cc2:OvalButton ID="btnCancelDelete" runat="server" Coloring="Silver" OnClick="CancelChanges_Clicked" 
            Text="Cancel" ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelResource" CssClass="backward left"></cc2:OvalButton>
        <cc2:OvalButton ID="btnContinueDelete" runat="server" Coloring="Silver" OnClick="ContinueChanges_Clicked" CommandArgument="Delete"
            Text="Continue" ArrowDirection="" IconPosition="" meta:resourcekey="btnDeleteResource" CssClass="forward"></cc2:OvalButton>
        <div class="clear"></div>
    </div>
</div>

