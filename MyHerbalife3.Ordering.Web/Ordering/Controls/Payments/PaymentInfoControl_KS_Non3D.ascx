<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoControl_KS_Non3D.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoControl_KS_Non3D" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc2" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<script src="/Ordering/Controls/Payments/Scripts/PaymentInfo.js" type="text/javascript"></script>
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>
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
            <div id="TB_window_Address" style="margin-top: 0px; display: block;" class="addnewccbilling">
                <div id="TB_ajaxContent" class="addnewccbillingcontent myclear">
                    <div class="addnewccbillingajaxcontent hrblModalSkinOnly">
                        <div id="dvCreditCard" class="myclear" runat="server">
                            <div class="gdo-popup-title myclear">
                                <ul class="hl-form-left">
                                    <li class="required-fields">
                                        <asp:Label ID="lblLabel1" runat="server" Text="*필수입력" ></asp:Label>
                                    </li>
                                    <li>
                                        <asp:Label ID="lblMessage" runat="server" Text="" ForeColor="Red"></asp:Label>
                                    </li>
                                </ul>
                            </div>

                            <div>
                                <div id="dvCardDetails" runat="server" class="hl-form-left">
                                    <div>
                                        <h3>
                                            <asp:Label ID="lblHeaderAddNewCreditCard" runat="server" Text="신용카드 추가하기" ></asp:Label>
                                        </h3>
                                    </div>
                                    <ul>
                                        <li>
                                            <asp:Label ID="lblCardHolderName" Text="신용카드 소지자  이름*:" runat="server" ></asp:Label>
                                        <asp:TextBox ID="txtCardHolderName" MaxLength="50" runat="server"
                                            meta:resourcekey="txtCardHoderNameResource1" TabIndex="1"></asp:TextBox>
                                        </li>

                                        <li>
                                            <asp:Label ID="lblCardNumber" Text="카드 번호*:" runat="server" ></asp:Label>
                                            <asp:TextBox ID="txtCardNumber" MaxLength="16" runat="server" meta:resourcekey="txtCardNumberResource1" TabIndex="2"></asp:TextBox>
                                        </li>

                                        <li class="hl-form-two-cols myclear">
                                            <asp:Label ID="lblCardType" Text="카드 종류*:" runat="server" ></asp:Label>
                                            <ul>
                                                <li>
                                                    <asp:DropDownList ID="ddlCardType" runat="server" meta:resourcekey="ddlCardTypeResource1" TabIndex="3">
                                                        <asp:ListItem Text="선택" ></asp:ListItem>
                                                    </asp:DropDownList>
                                                </li>
                                            </ul>
                                        </li>

                                        <li class="hl-form-two-cols myclear">
                                            <asp:Panel ID="pnlExpDate" runat="server" Width="100%" Style="white-space: nowrap" meta:resourcekey="pnlExpDateResource1">
                                                <asp:Label ID="lblExpDate" Text="유효기간*:" runat="server" ></asp:Label>
                                                <ul class="myclear">
                                                    <li>
                                                        <asp:DropDownList ID="ddlExpMonth" runat="server" TabIndex="4">
                                                            <asp:ListItem Value="0" Text="선택" meta:resourcekey="ListItemResource1"></asp:ListItem>
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
                                                            <asp:ListItem Value="0" Text="선택" meta:resourcekey="ListItemResource1"></asp:ListItem>
                                                            <asp:ListItem Text="2016" Value="16"></asp:ListItem>
                                                            <asp:ListItem Text="2017" Value="17"></asp:ListItem>
                                                            <asp:ListItem Text="2018" Value="18"></asp:ListItem>
                                                            <asp:ListItem Text="2019" Value="19"></asp:ListItem>
                                                            <asp:ListItem Text="2020" Value="20"></asp:ListItem>
                                                            <asp:ListItem Text="2021" Value="21"></asp:ListItem>
                                                            <asp:ListItem Text="2022" Value="22"></asp:ListItem>
                                                            <asp:ListItem Text="2023" Value="23"></asp:ListItem>
                                                            <asp:ListItem Text="2024" Value="24"></asp:ListItem>
                                                            <asp:ListItem Text="2025" Value="25"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </li>
                                                </ul>
                                                <asp:CompareValidator ID="reqVldExpYear" ControlToValidate="ddlExpYear" ValueToCompare="0" ValidationGroup="vldRQCreditCardGroup" Text="*" runat="server" Operator="NotEqual" meta:resourcekey="reqVldExpYearResource1" CssClass="required"></asp:CompareValidator>
                                            </asp:Panel>
                                        </li>

                                        <li class="hl-form-two-cols myclear"> 
                                            <asp:Label ID="lblTIN" Text="주민번호앞자리*:" runat="server" />  
                                            <ul class="myclear">
                                                <li>
                                                    <asp:TextBox ID="txtTIN" MaxLength="6" runat="server" TabIndex="6"></asp:TextBox>
                                                </li>
                                                <li class="last">     
                                                    <asp:RegularExpressionValidator ID="txtTINValidation" ControlToValidate="txtTIN" runat="server" ErrorMessage="6자리" ValidationExpression="^[0-9]{6}$" />                                                                      
                                                </li>
                                            </ul>                                
                                        </li>

                                        <li>
                                            <asp:Label ID="lblNickName" Text="닉네임:" runat="server" ></asp:Label>
                                            <asp:TextBox ID="txtNickName" MaxLength="30" runat="server"
                                            meta:resourcekey="txtNickNameResource1" TabIndex="7"></asp:TextBox>
                                        </li>

                                        <li>
                                            <div>
                                                <asp:CheckBox ID="chkSaveCreditCard" Text="신용카드 저장하기" runat="server" TabIndex="8" />
                                                <asp:CheckBox ID="chkMakePrimaryCreditCard" Text="기본 신용카드로 지정합니다" Checked="True" runat="server" TabIndex="9"/>
                                </div>
                                        </li>
                           
                                    </ul>
                                    <asp:Label ID="lblBillingAddressMessage" runat="server" ForeColor="Red" meta:resourcekey="lblBillingAddressMessageResource1"></asp:Label>
                                </div>

                                <div id="dvBillingAddress" runat="server" class="dvBillingAddress hl-form-right">

                                    <div class="dvBillingAddressHead">
                                        <h3>
                                            <asp:Label ID="lblCreditCardBillingAddress" runat="server" Text="Credit Card Billing Address" meta:resourcekey="lblCreditCardBillingAddressResource1"></asp:Label>
                                        </h3>
                                        <asp:RadioButtonList runat="server" ID="rblBillingAddress" AutoPostBack="True" OnSelectedIndexChanged="SetBillingAddressOption" RepeatDirection="Vertical" TabIndex="9">
                                            <asp:ListItem Value="0" Text="Same as Shipping Address" meta:resourcekey="chkSameAsShippingAddressResource1"></asp:ListItem>
                                            <asp:ListItem Value="1" Selected="True" Text="New Billing Address" meta:resourcekey="chkNewBillingAddressResource1"></asp:ListItem>
                                        </asp:RadioButtonList>
                                    </div>
                            
                                     <div id="dvBillingAddressText" runat="server">
                                        <ul class="street-address">
                                        <li>
                                            <asp:Label ID="lblStreetAddress" Text="Street Address*:" runat="server" meta:resourcekey="lblStreetAddressResource1"></asp:Label>
                                            <asp:TextBox ID="txtStreetAddress" MaxLength="50" runat="server"
                                                        meta:resourcekey="txtStreetAddressResource1" TabIndex="10"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="reqVldStreetAddress" ControlToValidate="txtStreetAddress" ValidationGroup="vldRQBillingAddressGroup" runat="server" Text="*" meta:resourcekey="reqVldStreetAddressResource1" CssClass="required"></asp:RequiredFieldValidator>
                                        </li>

                                        <li>
                                            <asp:Label ID="lblStreetAddress2" Text="Street Address 2:" runat="server" meta:resourcekey="lblStreetAddressResource2"></asp:Label>
                                            <asp:TextBox ID="txtStreetAddress2" MaxLength="50" runat="server"
                                                        meta:resourcekey="txtStreetAddressResource1" TabIndex="11"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtStreetAddress" ValidationGroup="vldRQBillingAddressGroup" runat="server" Text="*" meta:resourcekey="reqVldStreetAddressResource1" CssClass="required"></asp:RequiredFieldValidator>
                                        </li>

                                        <li>
                                            <asp:Label ID="lblCity" Text="City*:" runat="server" meta:resourcekey="lblCityResource1"></asp:Label>
                                            <asp:TextBox ID="txtCity" MaxLength="50" runat="server"
                                                        meta:resourcekey="txtCityResource1" TabIndex="12"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="reqVldCity" ControlToValidate="txtCity" runat="server" ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldCityResource1" CssClass="required"></asp:RequiredFieldValidator>
                                        </li>

                                        <li class="hl-form-two-cols myclear">
                                            <ul>
                                                <li>
                                                    <asp:Label ID="lblState" Text="State*:" runat="server" meta:resourcekey="lblStateResource1"></asp:Label>
                                                                    <asp:TextBox ID="txtState" runat="server" TabIndex="13"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="reqVldState" ControlToValidate="txtState" runat="server"
                                                        ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldStateResource1"></asp:RequiredFieldValidator>
                                                </li>

                                                <li class="last">
                                                    <asp:Label ID="lblZip" Text="Zip Code*:" runat="server" meta:resourcekey="lblZipResource1"></asp:Label>
                                                        <asp:TextBox ID="txtZip" MaxLength="10" runat="server"
                                                                        meta:resourcekey="txtZipResource1" TabIndex="14"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="reqVldZip" ControlToValidate="txtZip" runat="server" ValidationGroup="vldRQBillingAddressGroup" Text="*" meta:resourcekey="reqVldZipResource1"></asp:RequiredFieldValidator>
                                                </li>
                                            </ul>
                                        </li>
                                                
                                </ul>
                            </div>

                            <div id="dvBillingAddressLabel" runat="server">
                                        <ul>

                                            <li>
                                                <asp:Label ID="Label4" Text="Street Address*:" runat="server" meta:resourcekey="lblStreetAddressResource1"></asp:Label>
                                                <asp:TextBox ID="sametxtStreetAddress" MaxLength="40" runat="server" meta:resourcekey="txtStreetAddressResource1" TabIndex="10" CssClass="lettersNumbersWithSpace disabled" Enabled="false"></asp:TextBox>
                                            </li>

                                            <li>
                                                <asp:Label ID="Label5" Text="Street Address 2:" runat="server" meta:resourcekey="lblStreetAddressResource2"></asp:Label>
                                                <asp:TextBox ID="sametxtStreetAddress2" MaxLength="20" runat="server" meta:resourcekey="txtStreetAddress2Resource1" TabIndex="11" CssClass="disabled" Enabled="false"></asp:TextBox>
                                            </li>

                                            <li>
                                                <asp:Label ID="Label3" Text="City*:" runat="server" meta:resourcekey="lblCityResource1"></asp:Label>
                                                <asp:TextBox ID="sametxtCity" MaxLength="50" runat="server"
                                                    meta:resourcekey="txtCityResource1" TabIndex="12" Enabled="false"></asp:TextBox>
                                            </li>

                                            <li class="hl-form-two-cols myclear">

                                                <ul>
                                                    <li>
                                                        <asp:Label ID="Label7" Text="State*:" runat="server" meta:resourcekey="lblStateResource1"></asp:Label>
                                                        <asp:TextBox ID="sametxtState" runat="server" TabIndex="13" Enabled="false"></asp:TextBox>
                                                    </li>

                                                    <li class="last">
                                                        <asp:Label ID="Label8" Text="Zip Code*:" runat="server" meta:resourcekey="lblZipResource1"></asp:Label>
                                                        <asp:TextBox ID="sametxtZip" MaxLength="10" runat="server"
                                                        meta:resourcekey="txtZipResource1" TabIndex="14" Enabled="false"></asp:TextBox>
                                                    </li>
                                                </ul>
                                            
                                            </li>
                                            
                                        </ul>
                                    </div>
                                            
                                    </div>

                            </div>

                        </div>

                        <div class="gdo-error-message-div">
                            <asp:Label ID="lblCreditCardMesssage" runat="server" ForeColor="Red" meta:resourcekey="lblCreditCardMesssageResource1"></asp:Label>
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
                                        <asp:Localize runat="server" meta:resourcekey="NickName" Text="Nick Name"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblNickNameForDelete" runat="server" meta:resourcekey="lblNickNameForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize runat="server" meta:resourcekey="CardHolderName" Text="Card Holder Name"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardHolderNameForDelete" runat="server" meta:resourcekey="lblCardHolderNameForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize runat="server" meta:resourcekey="CardType" Text="Card Type"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardTypeForDelete" runat="server" meta:resourcekey="lblCardTypeForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize runat="server" meta:resourcekey="CardNumber" Text="Card Number"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblCardNumberForDelete" runat="server" meta:resourcekey="lblCardNumberForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize runat="server" meta:resourcekey="ExpDate" Text="Exp Date"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblExpDateForDelete" runat="server" meta:resourcekey="lblExpDateForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td valign="top" class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Localize runat="server" meta:resourcekey="Primary" Text="Primary"></asp:Localize>
                                    </td>
                                    <td class="gdo-form-label-left gdo-popup-form-label-padding2">
                                        <asp:Label ID="lblPrimaryForDelete" runat="server" meta:resourcekey="lblPrimaryForDeleteResource1"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div class="addnewccbillingbtns">

                            <div>
                                <ul>
                                    <li class="align-right action-buttons">

                            <cc1:DynamicButton ID="btnCancel" runat="server" ButtonType="Back" Text="Cancel" OnClick="btnCancel_Click"
                                meta:resourcekey="btnCancelResource1" TabIndex="11"></cc1:DynamicButton>

                            <cc1:DynamicButton ID="btnContinue" runat="server" ButtonType="Forward" Text="Continue" OnClick="btnContinue_Click"
                                meta:resourcekey="btnContinueResource1" TabIndex="12"></cc1:DynamicButton>

                            <cc1:DynamicButton ID="hdButton" runat="server" ButtonType="Forward" Style="display: none;" Text="Continue" OnClick="btnContinue_Click"
                                meta:resourcekey="btnContinueResource1" TabIndex="12"></cc1:DynamicButton>

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
    Enabled="True" BehaviorID="ppPaymentInfoPopupBehavior" RepositionMode="None">
</cc2:ModalPopupExtender>
<asp:LinkButton ID="lnkHidden" runat="server" Style="display: none;" meta:resourcekey="lnkHiddenResource1"></asp:LinkButton>