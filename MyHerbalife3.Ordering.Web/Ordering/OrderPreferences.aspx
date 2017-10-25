<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="OrderPreferences.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OrderPreferences"
    EnableEventValidation="false" meta:resourcekey="PageResource1" %>
<%@ Register Src="Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl" TagPrefix="hrblShippingInfoControl" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
 <asp:Content runat="server" ID="Content3" ContentPlaceHolderID="HeaderContent">
     <hrblAdvertisement:Advertisement ID="Advertisement1" runat="server" />
    </asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">    
    <div>
        <table border="0" cellpadding="0" cellspacing="0" width="100%">
            <tr>
                <%--MAIN SECTION--%>
                <td valign="top">
                    <div class="gdo-order-pref-container">
                        <div class="gdo-overview-page-title">
                            <asp:Label ID="lbOrderPref" runat="server"></asp:Label></div>
                        <div class="gdo-body-text-12">
                            <asp:Localize runat="server" meta:resourcekey="CreateManageShippingAddress">
                            Create and manage your shipping address, pickup location, and payment information faster check out in the future.</asp:Localize></div>
                        <div>
                            <div class="gdo-spacer1">
                            </div>
                            <div>
                                <div id="dvSavedShippingAddress" runat="server" class="gdo-float-left">
                                    <div class="gdo-order-pref-header-box">
                                        <span style="font-size: 12px; font-weight: bold">
                                            <asp:Localize runat="server" meta:resourcekey="SavedShippingAddress">Saved Shipping Address</asp:Localize></span>
                                    </div>
                                    <div class="gdo-order-pref-display-box">
                                        <table>
                                            <tr>
                                                <td valign="top">
                                                    <span class="gdo-body-text gdo-bold gdo-spacer2">
                                                        <asp:Localize runat="server" meta:resourcekey="Primary">Primary:</asp:Localize></span>
                                                </td>
                                                <td>
                                                    <div id="dvPrimaryShippingAddress" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <!--<asp:Label ID="lblFirstNameShippingAddress" runat="server" meta:resourcekey="lblFirstNameShippingAddressResource1"></asp:Label>
                                                                        <br />-->
                                                                        <asp:Panel ID="pnlPrimaryShippngAddress" runat="server" meta:resourcekey="pnlPrimaryShippngAddressResource1">
                                                                        </asp:Panel>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvNonPrimaryShippingAddress" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtAddShippingAddress" Text="Add Shipping Address" runat="server"
                                                                            OnClick="lbtAddShippingAddress_Click" meta:resourcekey="lbtAddShippingAddressResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvViewAllShippingAddress" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-bold"></span>
                                                                </td>
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtShowAllShippingAddress" Text="View All Shipping Addresses"
                                                                            runat="server" meta:resourcekey="lbtShowAllShippingAddressResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </div>
                                <div id="dvSavedPickupLocation" style="float: left" runat="server">
                                    <div class="gdo-order-pref-header-box">
                                        <span style="font-size: 12px; font-weight: bold">
                                            <asp:Localize runat="server" meta:resourcekey="SavedPickupLocation">Saved Pickup Location</asp:Localize></span>
                                    </div>
                                    <div class="gdo-order-pref-display-box">
                                        <table>
                                            <tr>
                                                <td valign="top">
                                                    <span class="gdo-body-text gdo-bold">
                                                        <asp:Localize ID="lblPickupDisplay" runat="server" meta:resourcekey="Primary">Primary:</asp:Localize></span>
                                                </td>
                                                <td>
                                                    <div id="dvPrimaryPickupLocation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:Panel ID="pnlPrimaryPickupLocation" runat="server" meta:resourcekey="pnlPrimaryPickupLocationResource1">
                                                                        </asp:Panel>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvNonPrimaryPickupLocation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtAddPickupLocation" Text="Add Pickup Location" OnClick="lbtAddPickupLocation_Click"
                                                                            runat="server" meta:resourcekey="lbtAddPickupLocationResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvViewAllPickupLocation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtViewAllPickupLocation" Text="View All Pickup Locations" runat="server"
                                                                            meta:resourcekey="lbtViewAllPickupLocationResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </div>
                                <div id="dvSavedPickpFromCourier" runat="server">
                                    <div class="gdo-order-pref-header-box">
                                        <span style="font-size: 12px; font-weight: bold">
                                            <asp:Localize runat="server" meta:resourcekey="SavedPickupFromCourier">Saved Pickup From Courier Location</asp:Localize></span>
                                    </div>
                                    <div class="gdo-order-pref-display-box">
                                        <table>
                                            <tr>
                                                <td valign="top">
                                                    <span class="gdo-body-text gdo-bold">
                                                        <asp:Localize ID="lblPUFromCourierLocation" runat="server" meta:resourcekey="Primary">Primary:</asp:Localize></span>
                                                </td>
                                                <td>
                                                    <div id="dvPrimaryPUFromCourierLocation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:Panel ID="pnlPrimaryPUFromcourierLocation" runat="server" meta:resourcekey="pnlPrimaryPUFromcourierLocation">
                                                                        </asp:Panel>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvNonPrimaryPUFromCourierLocation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtAddPUFromCourier" Text="Add Pickup From Courier Location" OnClick="lbtAddPUFromCourier_Click"
                                                                            runat="server" meta:resourcekey="lbtAddPUFromCourier"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvViewAllPUFromCourier" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtViewAllPUFromCourier" Text="View All Pickup From Courier Locations" runat="server"
                                                                            meta:resourcekey="lbtViewAllPUFromCourier"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </div>
                                <asp:Panel ID="dvSavedPaymentInformation" Style="float: left" runat="server">
                                    <div class="gdo-order-pref-header-box">
                                        <span style="font-size: 12px; font-weight: bold">
                                            <asp:Localize runat="server" meta:resourcekey="SavedPaymentInfo">Saved Credit Cards</asp:Localize></span>
                                    </div>
                                    <div class="gdo-order-pref-display-box">
                                        <table>
                                            <tr>
                                                <td valign="top">
                                                    <span class="gdo-body-text gdo-bold">
                                                        <asp:Localize ID="Localize1" runat="server" meta:resourcekey="Primary">Primary:</asp:Localize></span>
                                                </td>
                                                <td>
                                                    <div id="dvPrimaryPaymentInformation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:Label ID="lblPrimaryPaymentInformation" runat="server" meta:resourcekey="lblPrimaryPaymentInformationResource1"></asp:Label>
                                                                        <br />
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvNonPrimaryPaymentInformation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtAddPaymentInformation" Text="Add Payment Information" OnClick="lbtAddPaymentInformation_Click"
                                                                            runat="server" meta:resourcekey="lbtAddPaymentInformationResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                    <div id="dvViewAllPaymentInformation" runat="server">
                                                        <table>
                                                            <tr valign="top" align="left">
                                                                <td>
                                                                    <span class="gdo-body-text gdo-float-left">
                                                                        <asp:LinkButton ID="lbtViewAllPaymentInformation" Text="View All Credit Cards"
                                                                            runat="server" meta:resourcekey="lbtViewAllPaymentInformationResource1"></asp:LinkButton>
                                                                    </span>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </td>
                                            </tr>
                                        </table>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="gdo-spacer1 gdo-clear">
                        </div>
                    </div>
                    <div align="center">
                        <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />Saturday 6 a.m. to 2 p.m. PST"
                            meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
    </asp:ScriptManagerProxy>
    <progress:UpdatePanelProgressIndicator ID="progressShippingInfo" runat="server" TargetControlID="upShippingInfo" />
    <asp:UpdatePanel ID="upShippingInfo" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <progress:UpdatePanelProgressIndicator ID="progressPaymentInfo" runat="server" TargetControlID="upPaymentInfo" />
    <asp:UpdatePanel ID="upPaymentInfo" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:PlaceHolder ID="phPaymentInfoControl" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
