<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="HAPOrders.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.HAPOrders" %>
<%@ Register Src="~/Ordering/Controls/HAP/HAPModal.ascx" TagPrefix="uc1" TagName="HAPModal" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript">
        function DisableCreateHapOrder() {
            $('#<%= CreateHapOrder.ClientID %>').css('display', 'none');
            $('#<%= CreateHapOrderDisabled.ClientID %>').css('display', 'inline-block');
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ProductsContent" runat="server">
    <div id="divHAPDisabled" style="max-width:940px" runat="server" visible="false">
        <cc1:ContentReader ID="hapDisableMessage" runat="server" ContentPath="HapDisableMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
    <div>
        <asp:Label ID="NoteForHapOrder" runat="server" Text="Text"  ForeColor="Red"  meta:resourcekey="DistributorNotification"></asp:Label>
    </div>
     <div>
        <asp:Label ID="lblNotification" runat="server" Text="Text"  ForeColor="Red" Visible="false"  meta:resourcekey="NotificationMessage"></asp:Label>
    </div>
    <div style="max-width:940px;" runat="server" id="divMainContent">
        <div id="divNotificationRenew" runat="server" class="notification-warning">
            <i class="icon-alert-ln-2"></i>
            <asp:Literal ID="litNotificationRenew" runat="server"></asp:Literal>
            <asp:LinkButton ID="LinkRenewHAPOrder" runat="server" OnClick="LinkRenewHAPOrder_Click" meta:resourcekey="renewButton"></asp:LinkButton>
        </div>
        <p>
            <asp:Label ID="lblHapDescription" runat="server" Text="Label" meta:resourcekey="hapDescription"></asp:Label> 
        </p>

        <div id="divHAPbuttons" runat="server">
            <cc1:DynamicButton ID="CreateHapOrder" runat="server" ButtonType="Forward" 
                OnClick="CreateHapOrder_Click" OnClientClick="DisableCreateHapOrder();" meta:resourcekey="CreateHapOrderResource1" name="createHAP" />
            <cc1:DynamicButton ID="CreateHapOrderDisabled" runat="server" ButtonType="Forward" 
                Disabled="true" Hidden="true" meta:resourcekey="CreateHapOrderResource1" />
        </div>

        <asp:Panel ID="pnlHAPorders" runat="server">
            <asp:ListView ID="lstHAPOrders" runat="server" EnableModelValidation="True" OnItemCommand="lstHAPOrders_ItemCommand" OnDataBound="lstHAPOrders_DataBound">
                <LayoutTemplate>
                    <table id="hap-table">
                        <thead>
                            <tr class="head">
                                <th class="l"><asp:Literal ID="lbTblHead_HAP" runat="server" Text="HAP Order ID" meta:resourcekey="lbHAPResource1"></asp:Literal></th>
                                <th class="l hide-mobile"><asp:Literal ID="lbTblHead_Type" runat="server" Text="Type" meta:resourcekey="lbTypeResource1"></asp:Literal></th>
                                <th class="hide-mobile"><asp:Literal ID="lbTblHead_Country" runat="server" Text="Country" meta:resourcekey="lbCountryResource1"></asp:Literal></th>
                                <th class="l hide-mobile"><asp:Literal ID="lbTblHead_StartDate" runat="server" Text="StartDate" meta:resourcekey="lbStartDateResource1"></asp:Literal></th>
                                <th class="l hide-mobile"><asp:Literal ID="blTblHead_ExpDate" runat="server" Text="Exp. Date" meta:resourcekey="lbExpDateResource1"></asp:Literal></th>
                                <th class="l"><asp:Literal ID="lbTblHead_Status" runat="server" Text="Status" meta:resourcekey="lbStatusResource1"></asp:Literal></th>
                                <th class="r"><asp:Literal ID="lbTblHead_Volume" runat="server" Text="Volume" meta:resourcekey="lbVolumeResource1"></asp:Literal></th>
                                <th class="r"><asp:Literal ID="lbTblHead_Total" runat="server" Text="Total Due" meta:resourcekey="lbTotalResource1"></asp:Literal></th>
                                <th><asp:Literal ID="lbTblHead_Cancel" runat="server" Text="Cancel" meta:resourcekey="lbCancelResource1"></asp:Literal></th>
                                <th><asp:Literal ID="lbTblHead_Edit" runat="server" Text="Edit" meta:resourcekey="lbEditResource1"></asp:Literal></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr runat="server" id="itemPlaceholder"></tr>
                        </tbody>
                    </table>
                </LayoutTemplate>
                <ItemTemplate>
                    <tr>
                        <td class="l"><asp:LinkButton ID="LinkHAPOrderID" runat="server" CommandName="ViewOrder" CommandArgument='<%# Eval("OrderID") %>'><%# Eval("OrderID") %></asp:LinkButton></td>
                        <td class="l hide-mobile"><%# Eval("ProgramType") %></td>
                        <td class="hide-mobile"><%# Eval("CountryCode") %></td>
                        <td class="l hide-mobile"><%# Eval("StartDate", "{0:MM/dd/yyyy}") %></td>
                        <td class="l hide-mobile"><%# Eval("ExpirationDate", "{0:MM/dd/yyyy}") %></td>
                        <td class="l"><%# Eval("Status") %></td>
                        <td class="r"><%# Eval("Volume") %></td>
                        <td class="r"><%# Eval("AmontDue") %></td>
                        <td><asp:LinkButton ID="LinkCancelHAPOrder" runat="server" CommandName="CancelOrder" CommandArgument='<%# Eval("OrderID") %>'><%= GetLocalResourceString("lbCancelResource1.Text") %></asp:LinkButton></td>
                        <td><asp:LinkButton ID="LinkEditHAPOrder" runat="server" CommandName="EditOrder" CommandArgument='<%# Eval("OrderID") %>'><i class="icon-edit-ln-2"></i></asp:LinkButton></td>
                    </tr>
                </ItemTemplate>
            </asp:ListView>
        </asp:Panel>
        <asp:Literal ID="litHapEditBullet" runat="server" meta:resourcekey="EditHapExtraBullet" Visible="false"></asp:Literal>
        <cc1:ContentReader ID="hapInfo" runat="server" ContentPath="HapLandingPageInfo.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
    <uc1:HAPModal runat="server" id="HAPModal" Visible="false" EnableViewState="true" />
</asp:Content>
