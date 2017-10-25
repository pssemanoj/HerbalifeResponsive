﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PTCheckOutOptions.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.PTCheckOutOptions" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register Src="~/Ordering/Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl"
    TagPrefix="hrblShippingInfoControl" %>
<%@ Register Src="~/Ordering/Controls/Checkout/ConfirmAddress.ascx" TagName="ConfirmAddress"
    TagPrefix="hrblConfirmAddress" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>
<asp:UpdatePanel ID="upCheckOutOptions" runat="server">
    <ContentTemplate>
        <table>
            <tr>
                <td id="checkout-stepone">
                    <div id="divLabelErrors" runat="server" class="gdo-edit-header" style="border-bottom: 0px">
                        <table>
                            <tr>
                                <td>
                                    <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"
                                       >
                                    </asp:BulletedList>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <cc2:MaskedEditExtender ID="txtPhoneNumber_MaskedEditExtender" runat="server" CultureAMPMPlaceholder=""
                        CultureCurrencySymbolPlaceholder="" CultureDateFormat="" CultureDatePlaceholder=""
                        CultureDecimalPlaceholder="" CultureThousandsPlaceholder="" CultureTimePlaceholder=""
                        Enabled="True" Mask="999-9999999" MaskType="Number" TargetControlID="txtPickupPhone"
                        ClearMaskOnLostFocus="false" ClipboardEnabled="false">
                    </cc2:MaskedEditExtender>
                    <!--  *** Shipping/Pick Up Options  ***  -->
                    <div id="DeliveryOptionsView" class="checkout-ship-pick-options" runat="server">
                        <div class="gdo-right-column-header">
                            <h3>
                                <asp:Label runat="server" ID="ShippingPickupOptions" Text="Shipping / Pickup Options"
                                    meta:resourcekey="ShippingPickupOptionsResource1"></asp:Label></h3>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div class="gdo-input-container" id="divDeliveryOptionSelection" runat="server">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="TypeReadonly" Text="Type:" meta:resourcekey="Type"></asp:Label></div>
                            <div class="gdo-select-box">
                                <asp:DropDownList AutoPostBack="True" runat="server" ID="DeliveryType" OnSelectedIndexChanged="OnDeliveryTypeChanged"
                                    >
                                    <asp:ListItem Text="Shipping" Value="Shipping" meta:resourcekey="DeliveryOptionType_Shipping"></asp:ListItem>
                                    <asp:ListItem Text="Pickup" Value="Pickup" meta:resourcekey="DeliveryOptionType_Pickup"></asp:ListItem>
                                    <asp:ListItem Text="Delivery to Courier office" Value="ShipToCourier" meta:resourcekey="DeliveryOptionType_ShipToCourier"></asp:ListItem>
                                    <asp:ListItem Text="Pick up from Courier office" Value="PickupFromCourier" meta:resourcekey="DeliveryOptionType_PickupFromCourier"></asp:ListItem>
                                </asp:DropDownList>
                                <asp:Label ID="lblSelectedDeliveryType" runat="server" Visible="false"></asp:Label>
                            </div>
                        </div>
                        <div runat="server" id="divAddAddressLink">
                            <asp:LinkButton runat="server" ID="lnAddAddress" Text="Add Address" OnClick="AddAddressClicked"
                                meta:resourcekey="lnAddAddressResource1"></asp:LinkButton>
                        </div>
                        <div class="gdo-input-container" id="divNicknameSelection" runat="server">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="NickNameReadonly" Text="Nick Name:" meta:resourcekey="NickNameReadonlyResource1"></asp:Label></div>
                            <div class="gdo-select-box">
                                <asp:Label runat="server" ID="lblNickName" Visible="false"></asp:Label>
                                <asp:DropDownList AutoPostBack="True" runat="server" ID="DropdownNickName" DataTextField="DisplayName"
                                    DataValueField="ID" OnDataBound="OnNickName_Databind" OnSelectedIndexChanged="OnNickNameChanged"
                                    >
                                </asp:DropDownList>
                                <asp:Label ID="lblSelectedDeliveryAddress" runat="server" Visible="false"></asp:Label>
                            </div>
                        </div>
                        <div class="gdo-shipto-container" id="divshipToOrPickup" runat="server">
                            <div class="gdo-right-column-labels">
                                <asp:Label runat="server" ID="lbShiptoOrPickup" Text="Ship To:" meta:resourcekey="lbShiptoOrPickupResource1"></asp:Label></div>
                            <p runat="server" visible="true" id="pAddress">
                            </p>
                        </div>
                        <div class="gdo-shipto-container" id="divLocationInformation" runat="server">
                            <div class="gdo-right-column-labels">
                                <p runat="server" visible="true" id="pInformation">
                                </p>
                            </div>
                        </div>
                        <div runat="server" id="divLinks">
                            <asp:LinkButton runat="server" ID="LinkAdd" Text="Add" OnClick="AddClicked" meta:resourcekey="LinkAddResource1"></asp:LinkButton>&nbsp;
                            <span class="gdo-right-column-text">|</span>&nbsp;
                            <asp:LinkButton runat="server" ID="LinkEdit" Text="Edit" OnClick="EditClicked" meta:resourcekey="LinkEditResource1"></asp:LinkButton>&nbsp;
                            <span class="gdo-right-column-text">|</span>&nbsp;
                            <asp:LinkButton runat="server" ID="LinkDelete" OnClick="DeleteClicked" Text="Delete"
                                meta:resourcekey="LinkDeleteResource1"></asp:LinkButton>
                        </div>
                        <div class="gdo-ConfirmAddress-container" id="divConfirmAddress" runat="server">
                            <hrblConfirmAddress:ConfirmAddress runat="server" ID="cntrlConfirmAddress" />
                        </div>
                    </div>
                    <!-- *** PICK UP BY ***  -->
                    <div id="DeliveryOptionsInstructionsView" class="checkout-shipmethod" runat="server">
                        <div class="gdo-right-column-header">
                            <h3>
                                <asp:Label runat="server" ID="TextShippingMethod" Text="Edit Shipping Method" meta:resourcekey="TextShippingMethodResource1"></asp:Label></h3>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div runat="server" id="divDeliveryMethodShipping">
                            <table class="delivery-method">
                                <tr>
                                    <td colspan="2">
                                        <cc1:ContentReader ID="ContentReader2" runat="server" ContentPath="shippingmethod.html"
                                            SectionName="ordering" ValidateContent="True" UseLocal="true" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableShippingMethod">
                                    <td colspan="2">
                                        <cc2:NostalgicDropDownList runat="server" ID="ddlShippingMethod" DataTextField="Description"
                                            DataValueField="FreightCode" AutoPostBack="True" OnSelectedIndexChanged="ddlShippingMethod_OnSelectedIndexChanged"
                                            OnDataBound="ddlShippingMethod_OnDataBound">
                                        </cc2:NostalgicDropDownList>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticShippingMethod">
                                    <td colspan="2">
                                        <asp:Label runat="server" ID="lbShippingMethod" ></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableShippingTime">
                                    <td>
                                        <asp:Label runat="server" ID="txtTime" Text="*Time:" meta:resourcekey="txtDeliveryTime"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:DropDownList runat="server" AutoPostBack="true" ID="ddlShippingTime" OnSelectedIndexChanged="OnDeliveryTimeChanged"
                                            >
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticShippingTime">
                                    <td>
                                        <asp:Label runat="server" ID="txtTime2" Text="*Time:" meta:resourcekey="txtDeliveryTime"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="txtDeliveryTime" meta:resourcekey="txtDeliveryTimeResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableFreeFormShippingInstruction">
                                    <td colspan="2">
                                        <telerik:RadTextBox ID="txtShippingInstruction" CssClass="shipping-instruction" BorderStyle="Inset"
                                            BorderWidth="1" runat="server" MaxLength="90" TextMode="MultiLine"
                                            Width="100%" nTextChanged="OnShippingInstruction_Changed">
                                        </telerik:RadTextBox>
                                        <br />
                                        <asp:Label runat="server" ID="textShippingInstruction" Text="For a limited time shipping by **** is free."
                                            meta:resourcekey="txtFreeFormShippingInstruction"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticFreeFormShippingInstruction">
                                    <td colspan="2">
                                        <asp:Label runat="server" ID="txtShippingInstruction2" Text=""></asp:Label><br />
                                        <asp:Label runat="server" ID="textShippingInstruction2" Text="For a limited time shipping by **** is free."
                                            meta:resourcekey="txtFreeFormShippingInstruction"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div runat="server" id="divDeliveryMethodPickup">
                            <table>
                                <tr>
                                    <td colspan="2">
                                        <cc1:ContentReader ID="ContentReader1" runat="server" ContentPath="pickupdetails.html"
                                            SectionName="ordering" meta:resourcekey="ContentReader2Resource1" UseLocal="true" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditablePickupName">
                                    <td>
                                        <asp:Label runat="server" ID="PickupName" Text="Pickup Name" meta:resourcekey="PickupNameResource1"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txtPickupName" CssClass="inputPickupName" OnTextChanged="OnPickupName_Changed"
                                            meta:resourcekey="txtPickupNameResource1"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticPickupName">
                                    <td>
                                        <asp:Label runat="server" ID="PickUpnameStatic" Text="Pickup Name" meta:resourcekey="PickupNameResource1"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="lbPickupname" Text="Pickup Name" meta:resourcekey="lbPickupnameResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditablePickupPhone">
                                    <td>
                                        <asp:Label runat="server" ID="PickupPhone" Text="Pickup Phone" meta:resourcekey="PickupPhoneResource1"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txtPickupPhone" CssClass="inputPickupPhone" OnTextChanged="OnPickupPhone_Changed"
                                            meta:resourcekey="txtPickupPhoneResource1"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticPickupPhone">
                                    <td>
                                        <asp:Label runat="server" ID="PickUpPhoneStatic" Text="Pickup Phone" meta:resourcekey="PickupPhoneResource1"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="lblPickUpPhone" meta:resourcekey="lbPickupnameResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditablePickUpTimeDropDown">
                                    <td>
                                        <asp:Label runat="server" ID="txtTime3" Text="*Time:" meta:resourcekey="txtPickupTime"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:DropDownList runat="server" AutoPostBack="true" ID="ddlPickupTime" OnSelectedIndexChanged="OnPickupTimeChanged"
                                            meta:resourcekey="ddlPickupTimeResource1">
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticPickUpTimeDropDown">
                                    <td>
                                        <asp:Label runat="server" ID="txtTime4" Text="*Time:" meta:resourcekey="txtPickupTime"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="txtPickupTime" meta:resourcekey="txtPickupTimeResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableDatePicker">
                                    <td>
                                        <asp:Label runat="server" ID="PickupDate" Text="Pickup Date" meta:resourcekey="PickupDateResource1"></asp:Label>
                                    </td>
                                    <td>
                                        <div id="calendar-div">
                                            <telerik:RadDatePicker runat="server" ID="pickupdateTextBox" CssClass="rsAdvDatePicker"
                                                MinDate="1900-01-01" ZIndex="10000" EnableTyping="False"
                                                >
                                                <Calendar ID="Calendar1" runat="server" FastNavigationStep="12" ShowRowHeaders="false">
                                                </Calendar>
                                                <DatePopupButton Visible="true" />
                                                <DateInput ID="ScheduleDateTime" runat="server" EmptyMessageStyle-CssClass="radInvalidCss_Default"
                                                    EmptyMessage=" " LabelCssClass="">
                                                    <EmptyMessageStyle CssClass="radInvalidCss_Default"></EmptyMessageStyle>
                                                </DateInput>
                                            </telerik:RadDatePicker>
                                        </div>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticDatePicker">
                                    <td>
                                        <asp:Label runat="server" ID="PickupDate2" Text="Pick Up Date:" meta:resourcekey="PickupDateResource1"></asp:Label></b>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="lbPickupdate" Text="Pickup Date" meta:resourcekey="PickupDateResource1"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div class="gdo-shipto-container" id="divDeliveryTime" runat="server">
                            <asp:Label runat="server" ID="lbDeliveryTime" Text="Deadline for delivery after payment::"
                                meta:resourcekey="lbDeliveryTimeResource"></asp:Label>
                            <asp:HyperLink ID="hlDeliveryTime" runat="server" Target="_blank" NavigateUrl=""
                                meta:resourcekey="hlDeliveryTimeResource"></asp:HyperLink>
                        </div>
                    </div>
                    <!-- *** EMAIL *** -->
                    <div class="checkout-email" runat="server">
                        <div class="gdo-right-column-header">
                            <h3>
                                <asp:Label ID="lblEmailBoxHeader" runat="server" Text="Email Notification" meta:resourcekey="lblEmailBoxHeader"></asp:Label>
                            </h3>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div id="dvEmailEdit" runat="server">
                            <div runat="server" id="dvMobileNumber" class="ntbox-email">
                                <asp:Label runat="server" ID="lblMobileNumber" Text="SMS Mobile Number*:" meta:resourcekey="lblMobileNumber"></asp:Label>
                                <br />
                                <span>
                                    <asp:TextBox Width="45px" runat="server" ID="txtMobileAreaCode" OnTextChanged="txtMobileAreaCode_Changed"
                                        MaxLength="3" OnKeyPress="CheckNumeric(event,this)"></asp:TextBox>
                                    <asp:Label runat="server" Text="-"></asp:Label>
                                    <asp:TextBox runat="server" ID="txtMobileNumber" OnTextChanged="txtMobileNumber_Changed"
                                        MaxLength="8" OnKeyPress="CheckNumeric(event,this)"></asp:TextBox>
                                </span>
                                <br />
                                <asp:Label class="ntbox-msg" runat="server" ID="lblMessageNotify" Text="We will notify you when your order has been shipped."
                                    meta:resourcekey="lblMessageNotify"></asp:Label>
                                <div class="gdo-clear gdo-horiz-div">
                                </div>
                            </div>
                            <div class="ntbox-email">
                                <asp:Label ID="lblEmailDisplay" runat="server" Text="Email*:" meta:resourcekey="lblEmailDisplay"></asp:Label>
                                <asp:TextBox ID="txtEmail" runat="server" OnTextChanged="tbEmailAddress_Changed"
                                    meta:resourcekey="txtEmailResource1" MaxLength="320" />
                                <asp:Label ForeColor="Red" runat="server" ID="lbEmailError"></asp:Label>
                            </div>
                            <div class="ntbox-msg">
                                <asp:Label ID="lblEmailBoxText1" runat="server" Text="Herbalife will send your order confirmation, status, or updates to this email address:"
                                    meta:resourcekey="lblEmailBoxText1"></asp:Label></div>
                            <div class="ntbox-msg2">
                                <asp:Label ID="lblEmailBoxText2" runat="server" Text="To make this your primary email address go to My Account > My Profile to update your email address"
                                    meta:resourcekey="lblEmailBoxText2"></asp:Label>
                            </div>
                        </div>
                        <div id="dvEmailReadOnly" runat="server" visible="false">
                            <div class="ntbox-email">
                                <div runat="server" id="dvMobileNumberReadOnly">
                                    <asp:Label runat="server" ID="lblMobileNumberReadOnly" Text="SMS Mobile Number*:"
                                        meta:resourcekey="lblMobileNumber"></asp:Label>
                                    <asp:Label runat="server" ID="lblMobileNumberEntered" Text=""></asp:Label>
                                    <asp:Label class="ntbox-msg" runat="server" ID="lblMessageNotifyReadOnly" Text="We will notify you when your order has been shipped."
                                        meta:resourcekey="lblMessageNotify"></asp:Label>
                                    <div class="gdo-clear gdo-horiz-div">
                                    </div>
                                </div>
                                <asp:Label ID="lblEmailLabelDisplay" runat="server" Text="Email:" meta:resourcekey="lblEmailLabelDisplay"></asp:Label>
                                <asp:Label ID="lblShortEmail" runat="server"></asp:Label>
                                <asp:TextBox ID="txtLongEmailAddress" Visible="false" runat="server" BorderWidth="0"
                                    ReadOnly="true" BorderStyle="none" TextMode="multiLine" Wrap="true" Width="270px"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
    </ContentTemplate>
</asp:UpdatePanel>
<asp:UpdatePanel ID="ppShippingInfoControl" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <hrblShippingInfoControl:ShippingInfoControl ID="ucShippingInfoControl" runat="server" />
        <asp:Panel runat="server" ID="pnlShippingMethod">
        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="LinkEdit" />
        <asp:AsyncPostBackTrigger ControlID="LinkDelete" />
        <asp:AsyncPostBackTrigger ControlID="LinkAdd" />
        <asp:AsyncPostBackTrigger ControlID="lnAddAddress" />
        <asp:AsyncPostBackTrigger ControlID="DropdownNickName" />
        <asp:AsyncPostBackTrigger ControlID="DeliveryType" />
    </Triggers>
</asp:UpdatePanel>
<script type="text/javascript">    
    //Bug 244383: This closure was added to fix the masked edit behavior, added the delete / backspace functionality
    (function () {
        try {
            var p = AjaxControlToolkit.MaskedEditBehavior.prototype,
                en = p._ExecuteNav;
            p._ExecuteNav = function (e) {
                var type = e.type;
                if (type == 'keydown') { e.type = 'keypress' };
                en.apply(this, arguments);
                e.type = type;
            }
        } catch (e) {
            return;
        }
    })();
</script>