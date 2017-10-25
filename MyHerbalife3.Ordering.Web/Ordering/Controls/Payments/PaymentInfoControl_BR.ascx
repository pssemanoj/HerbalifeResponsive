<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoControl_BR.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoControl_BR" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<script src="/Ordering/Controls/Payments/Scripts/PaymentInfo.js" type="text/javascript"></script>
<script src="/SharedUI/Scripts/MyHerbalife.js" type="text/javascript"></script>

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server"></asp:ScriptManagerProxy>
<asp:HiddenField ID="Validations" runat="server" />
<asp:HiddenField runat="server" ID="hdCommand" />
<asp:HiddenField runat="server" ID="hdParentUri" />
<asp:HiddenField runat="server" ID="hdID" />
<asp:HiddenField runat="server" ID="hdLocale" />
<asp:HiddenField runat="server" ID="hdNickNameList" />
<asp:HiddenField runat="server" ID="hdTokenTimerEnabled" />
<asp:Panel ID="Pandel1" Style="display: none;" runat="server" CssClass="absolute">
    <progress:UpdatePanelProgressIndicator ID="progressPaymentInfoPopup" runat="server" TargetControlID="pnlPaymentInfoPopup"/>
    <asp:UpdatePanel ID="pnlPaymentInfoPopup" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="TB_window_Address" style="margin-top: 0px; display: block;" class="addnewccbilling br">
                <div id="TB_ajaxContent" class="addnewccbillingcontent br myclear">
                    <div class="addnewccbillingajaxcontent hrblModalSkinOnly">
                        <div id="dvCreditCard" class="dvCreditCard" runat="server">
                            
                            <div class="hl-form-left">
                                <div>
                                    <h3>
                                        <asp:Label ID="lblHeaderAddNewCreditCard" runat="server" Text="Add New Credit Card" meta:resourcekey="lblHeaderAddNewCreditCardResource1"></asp:Label>
                                    </h3>
                                </div>
                                <ul class="gdo-popup-form-field-padding">
                                    <li>
                                        <asp:Label ID="lblCardHolderName" Text="Card Holder Name*:" runat="server" meta:resourcekey="lblCardHolderNameResource1"></asp:Label>
                                        <asp:TextBox ID="txtCardHolderName" MaxLength="50" runat="server"
                                            meta:resourcekey="txtCardHoderNameResource1" TabIndex="1" CssClass="lettersWithSpace"></asp:TextBox>
                                    </li>
                                    <li>
                                        <asp:Label ID="lblCardNumber" Text="Card Number*:" runat="server" meta:resourcekey="lblCardNumberResource1"></asp:Label>
                                        <asp:TextBox ID="txtCardNumber" MaxLength="16" runat="server" meta:resourcekey="txtCardNumberResource1" TabIndex="2"></asp:TextBox>
                                    </li>

                                    <li class="hl-form-two-cols myclear myclear">
                                        <ul>
                                            <li>
                                                <asp:Label ID="lblCardType" Text="Card type*:" runat="server" meta:resourcekey="lblCardTypeResource1"></asp:Label>
                                                <asp:DropDownList ID="ddlCardType" runat="server" meta:resourcekey="ddlCardTypeResource1" TabIndex="3">
                                                    <asp:ListItem Text="Select" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                                </asp:DropDownList>
                                            </li>
                                            <li class="last">
                                                <div class="align-right" style="min-height: 50px;">
                                                    <img src="/Ordering/Images/Selecione.jpg" id="ccBranding" style="margin-top: 10px;" runat="server"/>
                                                </div>
                                            </li>
                                        </ul>
                                    </li>
                                    <li class="hl-form-two-cols myclear">

                                        <asp:Panel ID="pnlExpDate" runat="server" meta:resourcekey="pnlExpDateResource1">
                                            <asp:Label ID="lblExpDate" Text="Exp Date*:" runat="server" meta:resourcekey="lblExpDateResource1"></asp:Label>

                                            <ul>
                                                <li>
                                                    <asp:DropDownList ID="ddlExpMonth" runat="server" TabIndex="4">
                                                        <asp:ListItem Value="0" Text="Select" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                                        <asp:ListItem Text="01" Value="01"></asp:ListItem>
                                                        <asp:ListItem Text="02" Value="02"></asp:ListItem>
                                                        <asp:ListItem Text="03" Value="03"></asp:ListItem>
                                                        <asp:ListItem Text="04" Value="04"></asp:ListItem>
                                                        <asp:ListItem Text="05" Value="05"></asp:ListItem>
                                                        <asp:ListItem Text="06" Value="06"></asp:ListItem>
                                                        <asp:ListItem Text="07" Value="07"></asp:ListItem>
                                                        <asp:ListItem Text="08" Value="08"></asp:ListItem>
                                                        <asp:ListItem Text="09" Value="09"></asp:ListItem>
                                                        <asp:ListItem Text="10" Value="10"></asp:ListItem>
                                                        <asp:ListItem Text="11" Value="11"></asp:ListItem>
                                                        <asp:ListItem Text="12" Value="12"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </li>
                                                <li class="last">
                                                    <asp:DropDownList ID="ddlExpYear" runat="server" TabIndex="5">
                                                        <asp:ListItem Value="0" Text="Select" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                                        <asp:ListItem Text="2011" Value="11"></asp:ListItem>
                                                        <asp:ListItem Text="2012" Value="12"></asp:ListItem>
                                                        <asp:ListItem Text="2013" Value="13"></asp:ListItem>
                                                        <asp:ListItem Text="2014" Value="14"></asp:ListItem>
                                                        <asp:ListItem Text="2015" Value="15"></asp:ListItem>
                                                        <asp:ListItem Text="2016" Value="16"></asp:ListItem>
                                                        <asp:ListItem Text="2017" Value="17"></asp:ListItem>
                                                        <asp:ListItem Text="2018" Value="18"></asp:ListItem>
                                                        <asp:ListItem Text="2019" Value="19"></asp:ListItem>
                                                        <asp:ListItem Text="2020" Value="20"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </li>
                                            </ul>
                                            
                                            <asp:CompareValidator ID="reqVldExpYear" ControlToValidate="ddlExpYear" ValueToCompare="0" ValidationGroup="vldRQCreditCardGroup" Text="*" runat="server" Operator="NotEqual" meta:resourcekey="reqVldExpYearResource1" Display="Dynamic"></asp:CompareValidator>
                                        </asp:Panel>
                                    </li>

                                    <li>
                                        <asp:Label ID="lblNickName" Text="NickName:" runat="server" meta:resourcekey="lblNickNameResource1"></asp:Label>
                                        <asp:TextBox ID="txtNickName" MaxLength="30" runat="server" meta:resourcekey="txtNickNameResource1" TabIndex="6"></asp:TextBox>
                                    </li>

                                    <li>
                                        <asp:CheckBox ID="chkSaveCreditCard" Text="Save this Credit Card" runat="server" meta:resourcekey="chkSaveCreditCardResource1" TabIndex="7"/>
                                    </li>
                                    <li>
                                        <asp:CheckBox ID="chkMakePrimaryCreditCard" Text="Make this my primary Credit Card" Checked="True" runat="server" meta:resourcekey="chkMakePrimaryCreditCardResource1" TabIndex="8" />
                                    </li>
                                </ul>

                                <asp:Label ID="lblBillingAddressMessage" runat="server" ForeColor="Red" meta:resourcekey="lblBillingAddressMessageResource1"></asp:Label>

                            </div>

                            <div class="hl-form-right">
                                <div id="dvBillingAddress" runat="server">
                                    <div id="dvBillingAddressHead">
                                        <h3>
                                            <asp:Label ID="lblCreditCardBillingAddress" runat="server" Text="Credit Card Billing Address" meta:resourcekey="lblCreditCardBillingAddressResource1"></asp:Label>
                                        </h3>
                                        <asp:RadioButtonList runat="server" ID="rblBillingAddress" AutoPostBack="True" OnSelectedIndexChanged="SetBillingAddressOption" RepeatDirection="Vertical" TabIndex="9">
                                            <asp:ListItem Value="0" Text="Same as Shipping Address" meta:resourcekey="chkSameAsShippingAddressResource1"></asp:ListItem>
                                            <asp:ListItem Value="1" Selected="True" Text="New Billing Address" meta:resourcekey="chkNewBillingAddressResource1"></asp:ListItem>
                                        </asp:RadioButtonList>
                                    </div>

                                    <div id="dvBillingAddressText" runat="server">
                                        <ul class="gdo-popup-form-label-padding2">
                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="lblZip" Text="Zip Code*:" runat="server" meta:resourcekey="lblZipResource1"></asp:Label>
                                                        <asp:TextBox ID="txtZip" MaxLength="8" runat="server" AutoPostBack="true" OnTextChanged="txtZip_TextChanged"
                                                            meta:resourcekey="txtZipResource1" TabIndex="10"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="reqVldZip" ControlToValidate="txtZip" runat="server" ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldZipResource1" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    </li>
                                                    <li class="last">
                                                        <span class="gdo-form-format" style="margin-top: 30px; text-align: left;">
                                                            <asp:Localize ID="ZipFormat" runat="server" Text="Formato: 8 digitos" meta:resourcekey="ZipFormat"></asp:Localize>
                                                        </span>
                                                    </li>
                                                </ul>
                                            </li>

                                            <li>
                                                <asp:Label ID="lblStreetAddress" Text="Street Address*:" runat="server" meta:resourcekey="lblStreetAddressResource1"></asp:Label>
                                                <asp:TextBox ID="txtStreetAddress" MaxLength="40" runat="server" meta:resourcekey="txtStreetAddressResource1" TabIndex="11" CssClass="lettersNumbersWithSpace"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="reqVldStreetAddress" ControlToValidate="txtStreetAddress" ValidationGroup="vldRQBillingAddressGroup" runat="server" Text="*" meta:resourcekey="reqVldStreetAddressResource1" Display="Dynamic" CssClass="required"></asp:RequiredFieldValidator>
                                            </li>

                                            <li>
                                                <asp:Label ID="lblStreetAddress2" Text="Street Address 2:" runat="server" meta:resourcekey="lblStreetAddressResource2"></asp:Label>
                                                <asp:TextBox ID="txtStreetAddress2" MaxLength="20" runat="server" meta:resourcekey="txtStreetAddress2Resource1" TabIndex="12"></asp:TextBox>
                                            </li>

                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="lblNumber" Text="Number*:" runat="server" meta:resourcekey="lblNumberResource1"></asp:Label>
                                                        <asp:TextBox ID="txtNumber" MaxLength="8" runat="server" meta:resourcekey="txtNumberResource1" TabIndex="13" CssClass="lettersNumbersWithSpace"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="reqVldNumber" ControlToValidate="txtNumber" ValidationGroup="vldRQBillingAddressGroup" runat="server" Text="*" meta:resourcekey="reqVldNumberResource1" Display="Dynamic" CssClass="required"></asp:RequiredFieldValidator>
                                                    </li>
                                                    <li class="last">
                                                        <asp:Label ID="lblNeighborhood" Text="Neighborhood*:" runat="server" meta:resourcekey="lblNeighborhoodResource2"></asp:Label>
                                                        <asp:TextBox ID="txtNeighborhood" MaxLength="40" runat="server" TabIndex="14"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="reqVldNeighborhood" ControlToValidate="txtNeighborhood" runat="server" ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldNeighborhoodResource1" Display="Dynamic" CssClass="required"></asp:RequiredFieldValidator>
                                                    </li>
                                                </ul>
                                            </li>
                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="lblCity" Text="City*:" runat="server" meta:resourcekey="lblCityResource1"></asp:Label>
                                                        <asp:TextBox ID="txtCity" MaxLength="50" runat="server" meta:resourcekey="txtCityResource1" TabIndex="15"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="reqVldCity" ControlToValidate="txtCity" runat="server" ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldCityResource1" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    </li>
                                                    <li class="state last">
                                                        <asp:Label ID="lblState" Text="State*:" runat="server" meta:resourcekey="lblStateResource1"></asp:Label>
                                                        <asp:TextBox ID="txtState" runat="server" MaxLength="2" TabIndex="16"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="reqVldState" ControlToValidate="txtState" runat="server"
                                                            ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldStateResource1" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>
                                    </div>

                                    <div id="dvBillingAddressLabel" runat="server">
                                        <ul class="gdo-popup-form-label-padding2">
                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="Label3" Text="Zip Code*:" runat="server" meta:resourcekey="lblZipResource1"></asp:Label>
                                                        <asp:TextBox ID="sameBillingAddress_Zip" MaxLength="8" runat="server" meta:resourcekey="txtZipResource1" TabIndex="10" CssClass="disabled" Enabled="false"></asp:TextBox>
                                                    </li>
                                                    <li class="last">
                                                        <span class="gdo-form-format" style="margin-top: 30px; text-align: left;">
                                                            <asp:Localize ID="Localize7" runat="server" Text="Format: 8 digits" meta:resourcekey="ZipFormat"></asp:Localize>
                                                        </span>
                                                    </li>
                                                </ul>
                                            </li>

                                            <li>
                                                <asp:Label ID="Label4" Text="Street Address*:" runat="server" meta:resourcekey="lblStreetAddressResource1"></asp:Label>
                                                <asp:TextBox ID="sameBillingAddress_Street" MaxLength="40" runat="server" meta:resourcekey="txtStreetAddressResource1" TabIndex="11" CssClass="lettersNumbersWithSpace disabled" Enabled="false"></asp:TextBox>
                                            </li>

                                            <li>
                                                <asp:Label ID="Label5" Text="Street Address 2:" runat="server" meta:resourcekey="lblStreetAddressResource2"></asp:Label>
                                                <asp:TextBox ID="sameBillingAddress_Street2" MaxLength="20" runat="server" meta:resourcekey="txtStreetAddress2Resource1" TabIndex="12" CssClass="disabled" Enabled="false"></asp:TextBox>
                                            </li>

                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="Label7" Text="Number*:" runat="server" meta:resourcekey="lblNumberResource1"></asp:Label>
                                                        <asp:TextBox ID="sameBillingAddress_Number" MaxLength="8" runat="server" meta:resourcekey="txtNumberResource1" TabIndex="13" CssClass="lettersNumbersWithSpace disabled" Enabled="false"></asp:TextBox>
                                                    </li>
                                                    <li class="last">
                                                        <asp:Label ID="Label8" Text="Neighborhood*:" runat="server" meta:resourcekey="lblNeighborhoodResource2"></asp:Label>
                                                        <asp:TextBox ID="sameBillingAddress_Neighborhood" MaxLength="40" runat="server" TabIndex="14" CssClass="disabled" Enabled="false"></asp:TextBox>
                                                    </li>
                                                </ul>
                                            </li>
                                            <li class="hl-form-two-cols myclear">
                                                <ul>
                                                    <li>
                                                        <asp:Label ID="Label9" Text="City*:" runat="server" meta:resourcekey="lblCityResource1"></asp:Label>
                                                        <asp:TextBox ID="sameBillingAddress_City" MaxLength="50" runat="server" meta:resourcekey="txtCityResource1" TabIndex="15" CssClass="disabled" Enabled="false"></asp:TextBox>
                                                    </li>
                                                    <li class="state last">
                                                        <asp:Label ID="Label10" Text="State*:" runat="server" meta:resourcekey="lblStateResource1"></asp:Label>
                                                        <asp:TextBox ID="sameBillingAddress_State" runat="server" MaxLength="2" TabIndex="16" CssClass="disabled" Enabled="false"></asp:TextBox>
                                                    </li>
                                                </ul>
                                            </li>
                                        </ul>
                                    </div>
                                    
                                </div>
                            </div>
                            <div class="clear"></div>
                            <p><asp:Label ID="lblCreditCardMesssage" runat="server" ForeColor="Red" meta:resourcekey="lblCreditCardMesssageResource1"></asp:Label></p>

                        </div>

                        
                        <div id="dvDeleteCreditCard" style="font-size: x-small;" runat="server">
                            <table>
                                <tr>
                                    <td class="gdo-float-left gdo-popup-title">
                                        <h3>
                                            <asp:Label ID="LABEL2" runat="server" Text="Delete Credit Card" meta:resourcekey="LABEL2Resource1"></asp:Label></h3>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize1" runat="server" meta:resourcekey="NickName" Text="Nick Name"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblNickNameForDelete" runat="server" meta:resourcekey="lblNickNameForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize2" runat="server" meta:resourcekey="CardHolderName" Text="Card Holder Name"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardHolderNameForDelete" runat="server" meta:resourcekey="lblCardHolderNameForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize3" runat="server" meta:resourcekey="CardType" Text="Card Type"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardTypeForDelete" runat="server" meta:resourcekey="lblCardTypeForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize4" runat="server" meta:resourcekey="CardNumber" Text="Card Number"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardNumberForDelete" runat="server" meta:resourcekey="lblCardNumberForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize5" runat="server" meta:resourcekey="ExpDate" Text="Exp Date"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblExpDateForDelete" runat="server" meta:resourcekey="lblExpDateForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize ID="Localize6" runat="server" meta:resourcekey="Primary" Text="Primary"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblPrimaryForDelete" runat="server" meta:resourcekey="lblPrimaryForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </div>


                        <div class="addnewccbillingbtns">
                            <div class="hl-form-left">
                                <ul>
                                    <li>
                                        <asp:Label ID="lblMessage" runat="server" Text="" ForeColor="Red"></asp:Label>
                                    </li>
                                    <li class="mandatory-fields-label">
                                        <asp:Label ID="lblLabel1" runat="server" Text="*Required fields" meta:resourcekey="lblLabel1Resource1"></asp:Label>
                                    </li>
                                </ul>
                            </div>
                            <div>
                                <ul>
                                    <li class="align-right action-buttons">
                                        <cc1:DynamicButton ID="btnCancel" runat="server" Coloring="Silver" Text="Cancel" OnClick="btnCancel_Click"
                                            ArrowDirection="" IconPosition="" IconType="" meta:resourcekey="btnCancelResource1" TabIndex="17" CssClass="backward"></cc1:DynamicButton>
                                        <cc1:DynamicButton ID="btnContinue" runat="server" Coloring="Silver" Text="Continue" OnClick="btnContinue_Click"
                                            ArrowDirection="" IconPosition="" IconType="" meta:resourcekey="btnContinueResource1" TabIndex="18" CssClass="forward"></cc1:DynamicButton>

                                        <cc1:DynamicButton ID="hdButton" runat="server" ButtonType="Forward" Style="display: none;" Text="Continue" OnClick="btnContinue_Click"></cc1:DynamicButton>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Panel>
<cc2:ModalPopupExtender ID="ppPaymentInfoControl" runat="server" TargetControlID="lnkHidden"
    PopupControlID="Pandel1" CancelControlID="lnkHidden" BackgroundCssClass="modalBackground"
    DynamicServicePath="" Enabled="True" BehaviorID="ppPaymentInfoPopupBehavior" RepositionMode="None">
</cc2:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>
