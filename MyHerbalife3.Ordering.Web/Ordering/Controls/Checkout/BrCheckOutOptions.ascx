<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BrCheckOutOptions.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout.BrCheckOutOptions" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Register Src="/Ordering/Controls/ShippingInfoControl.ascx" TagName="ShippingInfoControl"
    TagPrefix="hrblShippingInfoControl" %>
<%@ Register Src="/Ordering/Controls/Checkout/ConfirmAddress.ascx" TagName="ConfirmAddress"
    TagPrefix="hrblConfirmAddress" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Register TagPrefix="ContentReader" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
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
                                        meta:resourcekey="blErrorsResource1">
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
                                    meta:resourcekey="DeliveryTypeResource1">
                                    <asp:ListItem Text="Shipping" Value="Shipping" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                    <asp:ListItem Text="Pickup" Value="Pickup" meta:resourcekey="ListItemResource2"></asp:ListItem>
                                    <asp:ListItem Text="Delivery to Courier office" Value="ShipToCourier" meta:resourcekey="ListItemResource5"></asp:ListItem>
                                    <asp:ListItem Text="Pick up from Courier office" Value="PickupFromCourier" meta:resourcekey="ListItemResource6"></asp:ListItem>
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
                                    meta:resourcekey="DropdownNickNameResource1">
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
                                        <ContentReader:ContentReader ID="ContentReader2" runat="server" ContentPath="shippingmethod.html"
                                            SectionName="ordering" ValidateContent="true" UseLocal="true" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableShippingMethod">
                                    <td colspan="2">
                                        <cc2:NostalgicDropDownList runat="server" ID="ddlShippingMethod" DataTextField="Description"
                                            DataValueField="FreightCode" AutoPostBack="True" OnSelectedIndexChanged="ddlBRShippingMethod_OnSelectedIndexChanged"
                                            OnDataBound="ddlShippingMethod_OnDataBound" meta:resourcekey="ddlShippingMethodResource1">
                                        </cc2:NostalgicDropDownList>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticShippingMethod">
                                    <td colspan="2">
                                        <asp:Label runat="server" ID="lbShippingMethod" meta:resourcekey="lbShippingMethodResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditableShippingTime">
                                    <td>
                                        <asp:Label runat="server" ID="txtTime" Text="*Time:" meta:resourcekey="txtDeliveryTime"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:DropDownList runat="server" AutoPostBack="true" ID="ddlShippingTime" OnSelectedIndexChanged="OnDeliveryTimeChanged"
                                            meta:resourcekey="ddlShippingTimeResource1">
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
                                            Width="100%" nTextChanged="OnShippingInstruction_Changed" meta:resourcekey="txtShippingInstruction">
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
                        <div runat="server" id="divDeliveryMethodShipToCourier">
                            <table class="delivery-method">
                                <tr runat="server" id="trCourierName">
                                    <td>
                                        <asp:Label runat="server" ID="lbDisplayCourierName" Text="Courier Name *" meta:resourcekey="lbDisplayCourierName"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txtCourierName" CssClass="inputPickupName" OnTextChanged="OnCourierName_Changed"
                                            meta:resourcekey="txtCourierName" MaxLength="48"></asp:TextBox>
                                        <ajaxToolkit:FilteredTextBoxExtender ID="fttxtCourierName" runat="server" TargetControlID="txtCourierName"
                                            FilterType="Custom,LowercaseLetters" ValidChars="ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890()_- " />
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticCourierName">
                                    <td>
                                        <asp:Label runat="server" ID="lbDisplayCourierNameStatic" Text="Courier Name" meta:resourcekey="lbDisplayCourierNameStatic"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="lbCourierName" meta:resourcekey="lbCourierName"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div runat="server" id="divDeliveryMethodPickup">
                            <table>
                                <tr runat="server" id="trPickupDetails">
                                    <td colspan="2">
                                        <cc1:ContentReader ID="ContentReader1" runat="server" ContentPath="pickupdetails.html"
                                            SectionName="ordering" ValidateContent="true" UseLocal="true" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trPickupFromCourierDetails">
                                    <td colspan="2">
                                        <cc1:ContentReader ID="ContentReader3" runat="server" ContentPath="pickupfromcourierdetails.html"
                                            SectionName="ordering" ValidateContent="true" />
                                    </td>
                                </tr>
                                <tr runat="server" id="trEditablePickupName">
                                    <td>
                                        <asp:Label runat="server" ID="PickupName" Text="Pickup Name *" meta:resourcekey="PickupNameResource1"></asp:Label>
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
                                        <asp:Label runat="server" ID="PickupPhone" Text="Pickup Phone *" meta:resourcekey="PickupPhoneResource1"></asp:Label>
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
                                <!-- RG# Field -->
                                <tr runat="server" id="trEditableRGNumber">
                                    <td>
                                        <asp:Label runat="server" ID="lblRGNumber" Text="RG# *" meta:resourcekey="lblRGNumberResource"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txtRGNumber" MaxLength="15" CssClass="inputPickupPhone"
                                            OnTextChanged="OnRGNumber_Changed" meta:resourcekey="txtPickupPhoneResource1"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr runat="server" id="trStaticRGNumber">
                                    <td>
                                        <asp:Label runat="server" ID="lblRGNumberStatic" Text="RG#" meta:resourcekey="lblRGNumberResource"></asp:Label>
                                    </td>
                                    <td>
                                        <asp:Label runat="server" ID="lblRGNumberValue"></asp:Label>
                                    </td>
                                </tr>
                                <!-- End RG# Field -->
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
                                                meta:resourcekey="pickupdateTextBoxResource1">
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
                            <asp:Label runat="server" ID="lbDeliveryTime" Text="Deadline for delivery after payment::" meta:resourcekey="lbDeliveryTimeResource"></asp:Label>
                            <asp:HyperLink ID="hlDeliveryTime" runat="server" Target="_blank" 
                                NavigateUrl="" meta:resourcekey="hlDeliveryTimeResource" ></asp:HyperLink>
                            <div class="gdo-shipto-container" id="divDeliveryTimeInstructions">
                                <cc1:ContentReader ID="ContentReader5" runat="server" ContentPath="deliverytimeinstructions.html"
                                    SectionName="ordering" ValidateContent="true" UseLocal="true" />
                            </div>
                        </div>                        
                    </div>

                    <!-- *** Delivery Terms *** -->
                    <div id="DeliveryTerms" runat="server" class="checkout-shipmethod" visible="false">
                        <div class="gdo-right-column-header">
                            <h3>
                                <asp:Label runat="server" ID="Label2" Text="Terms of Delivery" meta:resourcekey="TextDeliveryTerms"></asp:Label></h3>
                        </div>
                        <div class="gdo-clear gdo-horiz-div">
                        </div>
                        <div>
                            <table class="delivery-method">
                                <tr>
                                    <td colspan="2">
                                        <cc1:ContentReader ID="ContentReader4" runat="server" ContentPath="deliveryterms.html"
                                            SectionName="ordering" ValidateContent="true" UseLocal="true" />
                                    </td>
                                </tr>
                            </table>
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
                                        MaxLength="2" OnKeyPress="CheckNumeric(event,this)"></asp:TextBox>
                                    <asp:Label ID="Label1" runat="server" Text="-"></asp:Label>
                                    <asp:TextBox runat="server" ID="txtMobileNumber" OnTextChanged="txtMobileNumber_Changed"
                                        MaxLength="9" OnKeyPress="CheckNumeric(event,this)"></asp:TextBox>
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
                                <asp:Label ForeColor="Red" runat="server" ID="lbEmailError" meta:resourcekey="lbEmailErrorResource1"></asp:Label>
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
                        <div id="dvDSAddressEdit" runat="server" visible="false">
                            <div class="ntbox-email">
                                <asp:Label ID="lblDSAddressDisplay" runat="server" Text="Distrbutor Address*" CssClass="Bold"
                                    meta:resourcekey="lblDSAddressDisplay"></asp:Label>
                                <asp:DropDownList AutoPostBack="True" runat="server" ID="DropdownDSAddressNickName"
                                    DataTextField="DisplayName" DataValueField="ID" Width="170px" OnDataBound="OnNickName_Databind"
                                    OnSelectedIndexChanged="OnDSAddressNickNameChanged">
                                </asp:DropDownList>
                                <div runat="server" id="divAddDSAddressLink">
                                    <asp:LinkButton runat="server" ID="lnkAddDSAddress" Text="Add Address" OnClick="AddAddressClicked"
                                        meta:resourcekey="lnAddAddressResource1"></asp:LinkButton>
                                </div>
                                <div class="gdo-shipto-container" id="divDSAddress" runat="server">
                                    <div class="gdo-right-column-labels">
                                        <p runat="server" visible="true" id="pDSAddress">
                                        </p>
                                    </div>
                                </div>
                                <div runat="server" id="divDSAddressLink">
                                    <asp:LinkButton runat="server" ID="DSAddressLinkAdd" Text="Add" OnClick="AddDSAddressClicked"
                                        meta:resourcekey="LinkAddResource1"></asp:LinkButton>&nbsp; <span class="gdo-right-column-text">
                                            |</span>&nbsp;
                                    <asp:LinkButton runat="server" ID="DSAddressLinkEdit" Text="Edit" OnClick="EditDSAddressClicked"
                                        meta:resourcekey="LinkEditResource1"></asp:LinkButton>&nbsp; <span class="gdo-right-column-text">
                                            |</span>&nbsp;
                                    <asp:LinkButton runat="server" ID="DSAddressLinkDelete" OnClick="DeleteDSAddressClicked"
                                        Text="Delete" meta:resourcekey="LinkDeleteResource1"></asp:LinkButton>
                                </div>
                            </div>
                        </div>
                        <div id="dvDSAddressReadOnly" runat="server" visible="false">
                            <asp:Label ID="lblDSAddressDisplaysReadOnly" runat="server" Text="Distrbutor Address*"
                                meta:resourcekey="lblDSAddressDisplay"></asp:Label>
                            <div class="gdo-shipto-container" id="divDSAddressReadOnly" runat="server">
                                <div class="gdo-right-column-labels">
                                    <p runat="server" visible="true" id="pDSAddressReadOnly">
                                    </p>
                                </div>
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
        <asp:Panel runat="server" ID="pnlShippingMethod" meta:resourcekey="pnlShippingMethodResource1">
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