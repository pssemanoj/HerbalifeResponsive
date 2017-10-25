<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoGrid_CN.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoGrid_CN" %>
<%@ Register Src="~/Ordering/Controls/Payments/PaymentInfoControl.ascx" TagName="PaymentInfoControl"
    TagPrefix="hrblPaymentInfoControl" %>
<%@ Register TagPrefix="cc3" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>
<asp:Panel ID="pnlPaymentOptionsMessage" runat="server" meta:resourcekey="pnlPaymentOptionsMessageResource1">
    <cc1:ContentReader ID="ContentReaderPaymentOptionsMessage" runat="server" ContentPath="PaymentOptionsMessage.html"
        SectionName="Ordering" ValidateContent="True" meta:resourcekey="ContentReaderPaymentOptionsMessageResource1" UseLocal="False" />
</asp:Panel>
<style type="text/css">
    .selectedActionButton {
        background: #4eb106 !important;
        border: none !important;
        cursor: pointer;
        font-size: 13px;
        color: #ffffff !important;
        padding: 6px 13px;
    }

    .actionButton {
        background: #e6e6e6 !important;
        border: none !important;
        cursor: pointer;
        font-size: 13px;
        color: #212121 !important;
        padding: 6px 13px;
    }
</style>
<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlSelectPager" runat="server" CssClass="pmtbox" meta:resourcekey="pnlSelectPagerResource1">
            <asp:RadioButtonList runat="server" ID="rblPaymentOptions" RepeatDirection="Horizontal" CssClass="radioselectnbg" meta:resourcekey="rblPaymentOptionsResource1">
                <asp:ListItem Text="Quick Pay" Selected="True" Value="6" meta:resourcekey="ListItemResource7" href="pnlQuickPayTable" class="tab_link tab_link_selected"></asp:ListItem>
                <asp:ListItem Text="eBanking" Value="2" meta:resourcekey="ListItemResource2" href="pnlPaymentGatewayTable" class="tab_link"></asp:ListItem>
                <asp:ListItem Text="Wire Transfer" Value="3" meta:resourcekey="ListItemResource3" href="pnlWireTable" class="tab_link"></asp:ListItem>
                <asp:ListItem Text="Direct Deposit" Value="4" meta:resourcekey="ListItemResource4" href="pnlDirectDepositTable" class="tab_link"></asp:ListItem>
                <asp:ListItem Text="99 Bill" Value="5" meta:resourcekey="ListItemResource5" href="pnl99BillTable" class="tab_link"></asp:ListItem>
                <asp:ListItem Text="CNP" Value="1" meta:resourcekey="ListItemResource1" href="pnlCreditTable" class="tab_link"></asp:ListItem>
            </asp:RadioButtonList>
        </asp:Panel>
        <asp:Panel ID="pnlMessages" runat="server" meta:resourcekey="pnlMessagesResource1">
            <asp:Label ID="lblErrorMessages" runat="server" CssClass="gdo-error-message-txt" meta:resourcekey="lblErrorMessagesResource1"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="pnlPaymentOptions" runat="server" meta:resourcekey="pnlPaymentOptionsResource1">
            
            <asp:Panel runat="server" ID="pnlQuickPayTable" meta:resourcekey="pnlQuickPayTableResource1" CssClass="tab">
                <div class="quick-pay-container">
                    <div class="tabs">
                        <asp:Button ID="btnCreditCard_QuickPay" runat="server" OnClick="btnCreditCard_QuickPay_Click" OnClientClick="showProgress();return true;" CssClass="selectedActionButton" Text="Credit Card" meta:resourcekey="btnCreditCard_QuickPay" />
                        <asp:Button ID="btnDebitCard_QuickPay" runat="server" OnClick="btnDebitCard_QuickPay_Click" OnClientClick="showProgress();return true;" CssClass="actionButton" Text="Debit Card" meta:resourcekey="btnDebitCard_QuickPay" />
                    </div>
                    <div class="content">
                        <table class="quickPayTable">
                            <tr>
                                <td class="quickPayCol1">
                                    <asp:Literal runat="server" ID="lblBankList_QuickPay" Text="Bank" meta:resourcekey="lblBankList_QuickPay"></asp:Literal>
                                </td>
                                <td class="quickPayCol2">
                                    <asp:DropDownList ID="BankList_QuickPay" runat="server" AutoPostBack="True" CssClass="ccselect" onChange="showProgress();"
                                        DataTextField="DisplayName" DataValueField="ID" OnSelectedIndexChanged="OnBankList_QuickPaySelected">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>

                        <hr />

                        <asp:Panel runat="server" ID="NewCardBinding" Visible="false">
                            <table class="quickPayTable">
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblCardNumber_QuickPay" runat="server" Text="Credit Card Number" meta:resourcekey="lblCardNumber_QuickPay"></asp:Literal>
                                    </td>
                                    <td class="quickPayCol2">
                                        <asp:TextBox ID="CardNumber_QuickPay" runat="server" MaxLength="16"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblCardExpiredDate_QuickPay" runat="server" Text="Expired Date" meta:resourcekey="lblCardExpiredDate_QuickPay"></asp:Literal>
                                    </td>
                                    <td class="quickPayCol2">
                                        <asp:TextBox ID="CardExpiredDate_Month_QuickPay" runat="server" size="2" MaxLength="2"></asp:TextBox>月/
                                                <asp:TextBox ID="CardExpiredDate_Year_QuickPay" runat="server" size="2" MaxLength="2"></asp:TextBox>年
                                    </td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblCardCVV_QuickPay" runat="server" Text="CVV*" meta:resourcekey="lblCardCVV_QuickPay"></asp:Literal>
                                        <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl='/content/zh-CN/pdf/ordering/cvvhelp.pdf'
                                            ImageUrl="/Content/Global/img/gdo/icons/question-mark-blue.png" Target="_blank"
                                            CssClass="gdo-question-mark-blue" meta:resourcekey="lnkCVVHelpResource1"></asp:HyperLink>
                                    </td>
                                    <td class="quickPayCol2">
                                        <asp:TextBox ID="CardCVV_QuickPay" runat="server" size="3" MaxLength="3"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblCardHolderName_QuickPay" runat="server" Text="Card Holder Name" meta:resourcekey="lblCardHolderName_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="CardHolderName_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblIdentityNumber_QuickPay" runat="server" Text="ID Number" meta:resourcekey="lblIdentityNumber_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="IdentityNumber_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblPhoneNumber_QuickPay" runat="server" Text="Phone Number" meta:resourcekey="lblPhoneNumber_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="PhoneNumber_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1" colspan="2">
                                        <label>
                                            <asp:CheckBox ID="BindCard_QuickPay" runat="server" />
                                            <asp:Literal ID="lblBindCard_QuickPay" runat="server" Text="Bind Card?" meta:resourcekey="lblBindCard_QuickPay"></asp:Literal>
                                        </label>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <asp:Panel runat="server" ID="ExistingCard" Visible="false">
                            <table class="quickPayTable">
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="lblCardNumberLabel_QuickPay" runat="server" Text="Storable Card Number" meta:resourcekey="lblCardNumberLabel_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="CardNumberLabel_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="Literal9" runat="server" Text="Card Holder Name" meta:resourcekey="lblCardHolderName_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="CardHolderNameLabel_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="Literal10" runat="server" Text="ID Number" meta:resourcekey="lblIdentityNumber_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="IdentityNumberLabel_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Literal ID="Literal8" runat="server" Text="Phone Number" meta:resourcekey="lblPhoneNumber_QuickPay"></asp:Literal></td>
                                    <td class="quickPayCol2">
                                        <asp:Label ID="PhoneNumberLabel_QuickPay" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td class="quickPayCol1">
                                        <asp:Button ID="btnUseAnotherCard_QuickPay" runat="server" OnClick="btnUseAnotherCard_QuickPay_Click" CssClass="actionButton" Text="Use Another Card" meta:resourcekey="btnUseAnotherCard_QuickPay" OnClientClick="showProgress();return true;" />
                                    </td>
                                    <td class="quickPayCol2"></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        <div class="row">
                            <div class="col-xs-12 col-sm-12 col-md-12 col-lg-12 form-group">
                                <asp:Panel class="totalPay" runat="server" ID="Panel2" Style="display: none"
                                    meta:resourcekey="pnlTotalDueResource1">
                                    <asp:Label ID="Label4" runat="server" Text="Balance:" meta:resourcekey="Label1Resource2"></asp:Label>
                                    <asp:Label ID="Label5" runat="server" Visible="False" meta:resourcekey="lblCurrencySymbolResource1"></asp:Label>
                                    <asp:TextBox ID="TextBox8" runat="server" ReadOnly="True" meta:resourcekey="totalAmountBalanceResource1" />
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>
                <cc1:ContentReader ID="ContentReader1" runat="server" ContentPath="lblQuickPayMessage.html"
                    SectionName="Ordering" ValidateContent="True" UseLocal="True" />
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlPaymentGatewayTable" meta:resourcekey="pnlPaymentGatewayTableResource1" CssClass="tab">
                <table>
                    <tr>
                        <td>
                            <asp:Label ID="lblPaymentGatewayErrorMessage" runat="server" ForeColor="Red" meta:resourcekey="lblPaymentGatewayErrorMessageResource1"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Panel ID="pnlPaymentGatewayControlHolder" runat="server" meta:resourcekey="pnlPaymentGatewayControlHolderResource1">
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <asp:HiddenField ID="hidPaymentMethod" runat="server" />
                                        <asp:Label ID="lblPaymentGatewayAmountDue" runat="server" Text="Total Amount to be authorized: "
                                            meta:resourcekey="lblPaymentGatewayAmountDueResource1" />
                                        <asp:Label ID="totalPaymentGatewayDue" runat="server" meta:resourcekey="totalPaymentGatewayDueResource1" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:ContentReader ID="paymentGatewayMessage" runat="server" ContentPath="lblPaymentGatewayMessage.html"
                                SectionName="Ordering" ValidateContent="True" UseLocal="True" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlWireTable" ValidateContent="true" meta:resourcekey="pnlWireTableResource1" CssClass="tab">
                <table>
                    <tr>
                        <td>
                            <asp:DropDownList ID="ddlWire" runat="server" Style="display: none" meta:resourcekey="ddlWireResource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblWireAmountDue" runat="server" />
                                        <asp:Label ID="totalWireDue" runat="server" meta:resourcekey="totalWireDueResource1" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:ContentReader ID="wireMessage" runat="server" ContentPath="lblWireMessage.html"
                                SectionName="Ordering" ValidateContent="True" UseLocal="True" meta:resourcekey="wireMessageResource1" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnlDirectDepositTable" meta:resourcekey="pnlDirectDepositTableResource1" CssClass="tab">
                <table>
                    <tr>
                        <td>
                            <asp:DropDownList ID="ddlDirectDeposit" runat="server" Style="display: none" meta:resourcekey="ddlDirectDepositResource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <asp:Label ID="lblDirectDepositAmountDue" runat="server" Text="Total Direct Deposit Amount: "
                                            meta:resourcekey="lblDirectDepositAmountDueResource1" />
                                        <asp:Label ID="totalDirectDepositDue" runat="server" meta:resourcekey="totalDirectDepositDueResource1" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:ContentReader ID="directDepositMessage" runat="server" ContentPath="lblDirectDepositMessage.html"
                                SectionName="Ordering" ValidateContent="True" UseLocal="True" meta:resourcekey="directDepositMessageResource1" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Panel runat="server" ID="pnl99BillTable" CssClass="tab">
                <table>
                    <tr>
                        <td>
                            <cc1:ContentReader ID="reader99BillMessage" runat="server" ContentPath="lbl99BillMessage.html"
                                SectionName="Ordering" ValidateContent="True" UseLocal="True" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            
            <asp:Panel runat="server" ID="pnlCreditTable" meta:resourcekey="pnlCreditTableResource1" CssClass="tab">
                <table>
                    <tr>
                        <td>
                            <table class="gdo-checkout-step4-btn-tbl" style="display: none">
                                <tr>
                                    <td>
                                        <div style="white-space: nowrap" class="gdo-button-margin-lt bttn-addnewcc">
                                            <cc2:DynamicButton ID="btnAddNewCreditCard" runat="server" ButtonType="Neutral" meta:resourcekey="btnAddNewCreditCardResource1"
                                                OnClick="btnAddNewCreditCard_Click" Text="Add New Credit Card" IconPosition="Left" IconType="None" NavigateUrlToNewWindow="False" />
                                        </div>
                                    </td>
                                    <td align="right">
                                        <asp:Label ID="lblCardsLimitMessage" runat="server" meta:resourcekey="lblCardsLimitMessageResource1"
                                            Text="You can use up to {0} Cards to place this order"></asp:Label>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblError" runat="server" ForeColor="Red" meta:resourcekey="lblErrorResource1"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:GridView ID="gridViewCardInfo" runat="server" AutoGenerateColumns="False" OnRowDataBound="gridViewCardInfo_RowDataBound"
                                OnDataBound="gridViewCardInfo_DataBound" meta:resourcekey="gridViewCardInfoResource1"
                                CssClass="gdo-order-tbl spacepixel hide-xs" Visible="False">
                                <AlternatingRowStyle CssClass="gdo-row-odd gdo-order-tbl-data"></AlternatingRowStyle>
                                <Columns>
                                    <asp:TemplateField meta:resourceKey="TemplateFieldResource1">
                                        <ItemTemplate>
                                            <asp:Image ID="imgDeclined" runat="server" Visible="False" meta:resourcekey="imgDeclinedResource1" />
                                            <asp:TextBox ID="cardID" runat="server" Text='<%# Bind("ID") %>' Style="display: none" meta:resourcekey="cardIDResource1" />
                                            <asp:TextBox ID="PayOptionsOffset" runat="server" Style="display: none" meta:resourcekey="PayOptionsOffsetResource1" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Nickname" meta:resourceKey="hdrNickName">
                                        <ItemTemplate>
                                            <cc3:NostalgicDropDownList ID="ddlCards" runat="server" AutoPostBack="True" CssClass="ccselect" DataTextField="DisplayName"
                                                DataValueField="ID"
                                                meta:resourcekey="ddlCardsResource1">
                                            </cc3:NostalgicDropDownList>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField Visible="False" HeaderText="Bank Name" meta:resourceKey="hdrNickName">
                                        <ItemTemplate>
                                            <cc3:NostalgicDropDownList ID="ddlBankds" runat="server" AutoPostBack="True" CssClass="ccselect" meta:resourcekey="ddlBankdsResource1">
                                            </cc3:NostalgicDropDownList>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField Visible="False" HeaderText="Card Holder" meta:resourceKey="hdrCardHolder">
                                        <ItemTemplate>
                                            <asp:Label ID="lblCardHolder" runat="server" meta:resourceKey="lblCardHolderResource1"
                                                Text='<%# string.Format("{0} {1} {2}", Eval("CardHolder.First"), Eval("CardHolder.Middle"), Eval("CardHolder.Last")) %>'></asp:Label>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField Visible="False" HeaderText="Card Type" meta:resourceKey="hdrCardType">
                                        <ItemTemplate>
                                            <asp:Label ID="lblCardType" runat="server" meta:resourceKey="lblCardTypeResource1"></asp:Label>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Card Number" meta:resourceKey="hdrCardNumber">
                                        <ItemTemplate>
                                            <asp:Label Visible="False" ID="lblCardNumber" runat="server" meta:resourceKey="lblCardNumberResource1"
                                                Text='<%# getCardNumber(Eval("CardNumber") as string, Eval("CardType") as string) %>'></asp:Label>
                                            <asp:TextBox ID="txtCardNumber" runat="server" CssClass="gdo-product-qty" meta:resourcekey="txtCardNumberResource1"></asp:TextBox>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Pay Options" meta:resourceKey="hdrPayOptions" Visible="False">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkPaymentOptions" runat="server" meta:resourcekey="lnkPaymentOptionsResource1"
                                                Visible="False"></asp:LinkButton>
                                            <asp:TextBox ID="txtOption" runat="server" meta:resourcekey="txtOptionResource1"
                                                Style="display: none"></asp:TextBox>
                                            <asp:TextBox ID="txtChoice1" runat="server" meta:resourcekey="txtChoice1Resource1"
                                                Style="display: none"></asp:TextBox>
                                            <asp:TextBox ID="txtChoice2" runat="server" meta:resourcekey="txtChoice2Resource1"
                                                Style="display: none"></asp:TextBox>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Expiration" meta:resourceKey="hdrExpiration">
                                        <ItemTemplate>
                                            <asp:Label Visible="False" ID="lblExp" runat="server" meta:resourceKey="lblExpResource1" Text='<%# (DateTime.Parse(Eval("Expiration").ToString())==GetMyKeyExpirationDate()) ? "" :Eval("Expiration","{0:MM-yyyy}") %>'></asp:Label>
                                            <asp:Label ID="Label1" runat="server" CssClass="red" meta:resourceKey="Label1Resource1"
                                                Text='<%# isExpires(DateTime.Parse(Eval("Expiration").ToString())) == false ? "" : (this.GetLocalResourceObject("strExpires") as string) %>'></asp:Label>
                                            <asp:TextBox ID="txtExpMonth" runat="server" CssClass="gdo-product-qty" Width="15px" meta:resourcekey="txtExpMonthResource1" MaxLength="2"></asp:TextBox>月/
                                                    <asp:TextBox ID="txtExpYear" runat="server" CssClass="gdo-product-qty" Width="25px" meta:resourcekey="txtExpYearResource1" MaxLength="2"></asp:TextBox>年
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                        <HeaderStyle HorizontalAlign="Center" />
                                        <ItemStyle CssClass="expiration-fields" />
                                    </asp:TemplateField>
                                    <asp:TemplateField meta:resourcekey="TemplateFieldResource2">
                                        <HeaderTemplate>
                                            <asp:Label ID="lblCVVHeader" runat="server" meta:resourcekey="lblCVVHeaderResource1"
                                                Style="float: left" Text="CVV*"></asp:Label>
                                            <div style="float: right; padding-right: 3px">
                                                <asp:HyperLink ID="lnkCVVHelp" runat="server" NavigateUrl='<%# GetCVVHelpLink() %>'
                                                    ImageUrl="/Content/Global/img/gdo/icons/question-mark-blue.png" Target="_blank"
                                                    CssClass="gdo-question-mark-blue" meta:resourcekey="lnkCVVHelpResource1"></asp:HyperLink>
                                            </div>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtCVV" runat="server" CssClass="gdo-product-qty"
                                                meta:resourcekey="txtCVVResource1" size="3"></asp:TextBox>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                        <HeaderStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Amount" meta:resourceKey="Amount">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtAmount" runat="server" CssClass="gdo-product-qty"
                                                size="6" meta:resourcekey="txtAmountResource1"></asp:TextBox>
                                        </ItemTemplate>
                                        <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                        <HeaderStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                </Columns>
                                <HeaderStyle CssClass="gray_background" Height="30px" />
                                <RowStyle CssClass="gdo-row-even gdo-order-tbl-data" />
                            </asp:GridView>

                            <% //if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile()) { %>
                            <div class="row cc-grid">
                                <div>
                                    <asp:Image ID="imgDeclinedMob" runat="server" Visible="False" meta:resourcekey="imgDeclinedResource1" />
                                    <asp:TextBox ID="txtCardIdMob" runat="server" Style="display: none" meta:resourcekey="cardIDResource1" />
                                    <asp:TextBox ID="PayOptionsOffsetMob" runat="server" Style="display: none" meta:resourcekey="PayOptionsOffsetResource1" />
                                </div>
                                <div class="col-sm-3">
                                    <div class="payment-header">
                                        <asp:Literal runat="server" ID="ltBankNameMob" Text="银行名称"></asp:Literal>
                                    </div>
                                    <div class="payment-input">
                                        <cc3:NostalgicDropDownList ID="ddlBankdsMob" runat="server" AutoPostBack="True" CssClass="ccselect form-control" onChange="showProgress();"
                                            DataTextField="DisplayName" DataValueField="ID" meta:resourcekey="ddlCardsResource1" OnSelectedIndexChanged="OnCardBankdsMobSelected">
                                        </cc3:NostalgicDropDownList>
                                    </div>
                                </div>
                                <div class="col-sm-3">
                                    <div class="payment-header">
                                        <asp:Literal ID="ltCarNumberMob" runat="server" Text="卡号"></asp:Literal>
                                    </div>
                                    <div class="payment-input">
                                        <asp:TextBox ID="txtCardNumberMob" runat="server" CssClass="form-control" meta:resourcekey="txtCardNumberResource1"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-sm-2">
                                    <div class="payment-header">
                                        <asp:Literal ID="ltExpiration" runat="server" Text="有效期限"></asp:Literal>
                                    </div>
                                    <div class="payment-input row exp-date">
                                        <div class="col-xs-6">
                                            <asp:TextBox ID="txtExpMonthMob" runat="server" CssClass="form-control" meta:resourcekey="txtExpMonthResource1" MaxLength="2"></asp:TextBox>月/
                                        </div>
                                        <div class="col-xs-6">
                                            <asp:TextBox ID="txtExpYearMob" runat="server" CssClass="form-control" meta:resourcekey="txtExpYearResource1" MaxLength="2"></asp:TextBox>年
                                        </div>
                                    </div>
                                </div>
                                <div class="col-sm-2">
                                    <div class="payment-header">
                                        <asp:Literal ID="ltCVVMob" runat="server" Text="卡片背面后三码*" meta:resourcekey="lblCVVHeaderResource1"></asp:Literal>
                                        <asp:HyperLink ID="lnkCVVHelpMov" runat="server" NavigateUrl='/content/zh-CN/pdf/ordering/cvvhelp.pdf'
                                            ImageUrl="/Content/Global/img/gdo/icons/question-mark-blue.png" Target="_blank"
                                            CssClass="gdo-question-mark-blue" meta:resourcekey="lnkCVVHelpResource1"></asp:HyperLink>
                                    </div>
                                    <div class="payment-input">
                                        <asp:TextBox ID="txtCVVMob" runat="server" CssClass="form-control"
                                            meta:resourcekey="txtCVVResource1" size="3" MaxLength="3"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="col-sm-2">
                                    <div class="payment-header">
                                        <asp:Literal ID="ltAmountMob" runat="server" Text="付款金額"></asp:Literal>
                                    </div>
                                    <div class="payment-input last">
                                        <asp:TextBox ID="txtAmountMob" runat="server" CssClass="form-control"
                                            size="6" meta:resourcekey="txtAmountResource1"></asp:TextBox>
                                    </div>
                                </div>
                            </div>
                            <% //} %>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:ContentReader ID="ContentReaderCNP" runat="server" ContentPath="lblCNPMessage.html"
                                SectionName="Ordering" ValidateContent="True" UseLocal="True" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Panel class="totalPay" runat="server" ID="pnlTotalDue" Style="display: none"
                                meta:resourcekey="pnlTotalDueResource1">
                                <asp:Label ID="lbTotalAmount" runat="server" Text="Balance:" meta:resourcekey="Label1Resource2"></asp:Label>
                                <asp:Label ID="lblCurrencySymbol" runat="server" Visible="False" meta:resourcekey="lblCurrencySymbolResource1"></asp:Label>
                                <asp:TextBox ID="totalAmountBalance" runat="server" ReadOnly="True" meta:resourcekey="totalAmountBalanceResource1" />
                            </asp:Panel>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label Style="margin-left: 10px" CssClass="red" runat="server" ID="lbNoCard"
                                Visible="False" meta:resourcekey="lbNoCard"></asp:Label><asp:Label Style="margin-left: 10px"
                                    CssClass="red" runat="server" ID="lbCardExpires" meta:resourcekey="lbCardExpiresResource1"></asp:Label>
                            <asp:Label Style="margin-left: 10px" CssClass="red" runat="server" ID="lbAmoutOrCVVError"
                                meta:resourcekey="lbAmoutOrCVVErrorResource1"></asp:Label>
                            <asp:Label Style="margin-left: 10px" CssClass="red" runat="server" ID="lbMessage"
                                Visible="False" meta:resourcekey="lbMessage"></asp:Label>
                            <asp:Label Style="margin-left: 10px" CssClass="red" runat="server" ID="lbAddCardError"
                                Visible="False" meta:resourcekey="lbAddCardErrorResource1"></asp:Label>
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <asp:Label ID="cnMessage" runat="server" ForeColor="Red" Text="如遇支付问题，请致电快钱服务热线：400-635-5799"></asp:Label>
            <asp:Panel ID="pnlAcknowledCheckContent" runat="server" CssClass="errorConfirm" Visible="False" meta:resourcekey="pnlAcknowledCheckContentResource1">
                <asp:CheckBox runat="server" ID="chkAcknowledgeTransaction" AutoPostBack="True" OnCheckedChanged="AcknowledgeChanged"
                    meta:resourceKey="PaymentAcknowledgement" Text="I acknowledge this transaction" />
            </asp:Panel>
        </asp:Panel>
        <asp:Panel ID="pnlTotals" runat="server" meta:resourcekey="pnlTotalsResource1">
            <table>
                <tr align="right">
                    <td>
                        <asp:Panel class="totalPay" runat="server" ID="pnlGrandTotal" meta:resourcekey="pnlGrandTotalResource1">
                            <asp:Label runat="server" ID="lblGrandTotal" Text="Grand Total:" meta:resourcekey="lblGrandTotalResource1"></asp:Label>
                            <asp:Label ID="txtGrandTotal" runat="server" readonly="readonly" meta:resourcekey="txtGrandTotalResource1" />
                        </asp:Panel>
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlBottomMessages" runat="server" meta:resourcekey="pnlBottomMessagesResource1">
            <cc1:ContentReader ID="creditCardMessage" runat="server" ContentPath="CreditCardMessage.html"
                SectionName="Ordering" ValidateContent="True" UseLocal="True" meta:resourcekey="creditCardMessageResource1" />
        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ucPaymentInfoControl" />
        <asp:AsyncPostBackTrigger ControlID="chkAcknowledgeTransaction" />
    </Triggers>
</asp:UpdatePanel>
<asp:Panel CssClass="gdo-right-column-tbl gdo-hff-module" ID="PayOptionsQH" runat="server" Style="border-style: solid; border-width: thin; width: 190px; height: 270px; background-color: White" Visible="False" meta:resourcekey="PayOptionsQHResource1">
    <table style="width: 100%;">
        <tr>
            <td colspan="2" align="center">
                <strong>
                    <asp:Label runat="server" ID="lbPayOptions" Text="Pay Options" meta:resourcekey="lbPayOptions"></asp:Label>
                </strong>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <table style="width: 100%">
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <span class="NoBorder">
                                <input id="LumpSumQH" type="radio" checked="checked" name="chkPayOptionQH" value="1" />
                                <label for="LumpSumQH">
                                    <asp:Label runat="server" ID="lbLumpSum" Text="Lump Sum" meta:resourcekey="lbLumpSum" /></label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <span class="NoBorder">
                                <input id="RevolvingQH" type="radio" name="chkPayOptionQH" value="2" /><label for="RevolvingQH">
                                    <asp:Label runat="server" ID="lbRevolving" Text="Revolving" meta:resourcekey="lbRevolving" /></label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <span class="NoBorder">
                                <input id="InstallmentsQH" type="radio" name="chkPayOptionQH" value="3" />
                                <label for="InstallmentsQH">
                                    <asp:Label runat="server" ID="lbInstallments" Text="Installments" /></label>
                            </span>
                        </td>
                        <td align="right">
                            <select id="NumberInstallmentsQH">
                                <option value="0"></option>
                                <option value="3">03</option>
                                <option value="6">06</option>
                                <option value="10">10</option>
                                <option value="12">12</option>
                                <option value="15">15</option>
                                <option value="18">18</option>
                                <option value="20">20</option>
                                <option value="24">24</option>
                                <option value="30">30</option>
                                <option value="36">36</option>
                            </select>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3" style="height: 12px"></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <span class="NoBorder">
                                <input id="Bonus1" type="radio" name="chkPayOptionQH" value="7" />
                                <label for="Bonus1">
                                    <asp:Label runat="server" ID="lbBonus1" Text="Bonus 1"></asp:Label>
                                </label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth" Text="Select Month:" meta:resourcekey="lbSelectMonth" />
                        </td>
                        <td style="width: 20px"></td>
                        <td align="right">
                            <asp:DropDownList ID="Bonus1Month" runat="server" meta:resourcekey="Bonus1MonthResource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <span class="NoBorder">
                                <input id="Bonus2" type="radio" name="chkPayOptionQH" value="8" />
                                <label for="Bonus2">
                                    <asp:Label runat="server" ID="lbBonus2" Text="Bonus 2" />
                                </label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth1" Text="Select Month 1:" meta:resourcekey="lbSelectMonth1" />
                        </td>
                        <td style="width: 20px"></td>
                        <td style="border-style: none" align="right">
                            <asp:DropDownList ID="Bonus2Month1" runat="server" meta:resourcekey="Bonus2Month1Resource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth2" Text="Select Month 2:" meta:resourcekey="lbSelectMonth2" />
                        </td>
                        <td style="width: 20px"></td>
                        <td align="right">
                            <asp:DropDownList ID="Bonus2Month2" runat="server" meta:resourcekey="Bonus2Month2Resource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="2" style="height: 12px"></td>
        </tr>
        <tr>
            <td>
                <asp:Button ID="btnCancelQH" runat="server" Text="Cancel" OnClientClick="return false;"
                    meta:resourcekey="btnCancelQHResource1" />
            </td>
            <td align="right">
                <asp:Button ID="btnOkQH" runat="server" Text="Ok" meta:resourcekey="btnOkQHResource1" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:Panel ID="PayOptionsAL" CssClass="gdo-right-column-tbl gdo-hff-module" runat="server"
    Style="border-style: solid; border-width: thin; width: 130px; height: 140px; background-color: White"
    Visible="False" meta:resourcekey="PayOptionsALResource1">
    <table style="width: 100%">
        <tr>
            <td colspan="2" align="center">
                <strong>
                    <asp:Label runat="server" ID="lbPayOptions2" Text="Pay Options" meta:resourcekey="lbPayOptions" />
                </strong>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <table style="width: 100%">
                    <tr>
                        <td colspan="2">
                            <span class="NoBorder">
                                <input id="LumpSumAL" type="radio" checked="checked" name="chkPayOptionAL" value="1" />
                                <label for="LumpSumQH">
                                    <asp:Label runat="server" ID="lbLumpSum2" Text="Lump Sum" meta:resourcekey="lbLumpSum" />
                                </label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <span class="NoBorder">
                                <input id="RevolvingAL" type="radio" name="chkPayOptionAL" value="2" class="NoBorder" />
                                <label for="RevolvingQH">
                                    <asp:Label runat="server" ID="lbRevolving2" Text="Revolving" meta:resourcekey="lbRevolving" /></label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <span class="NoBorder">
                                <input id="InstallmentsAL" type="radio" name="chkPayOptionAL" value="3" class="NoBorder" />
                                <label for="InstallmentsQH">
                                    <asp:Label runat="server" ID="lbInstallments2" Text="Installments" meta:resourcekey="lbInstallments" /></label>
                            </span>
                        </td>
                        <td align="right">
                            <select id="NumberInstallmentsAL">
                                <option value="0"></option>
                                <option value="3">03</option>
                                <option value="6">06</option>
                                <option value="10">10</option>
                                <option value="12">12</option>
                                <option value="15">15</option>
                                <option value="18">18</option>
                                <option value="20">20</option>
                            </select>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="2" style="height: 12px"></td>
        </tr>
        <tr>
            <td>
                <asp:Button ID="btnCancelAL" runat="server" Text="Cancel" OnClientClick="return false;" />
            </td>
            <td align="right">
                <asp:Button ID="btnOkAL" runat="server" Text="Ok" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:HiddenField ID="Validations" runat="server" />
<asp:Button runat="server" ID="btnQH" Style="display: none" />
<asp:Button runat="server" ID="btnAL" Style="display: none" />
<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <hrblPaymentInfoControl:PaymentInfoControl ID="ucPaymentInfoControl" runat="server" />
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnAddNewCreditCard" />
        <asp:AsyncPostBackTrigger ControlID="gridViewCardInfo" />
    </Triggers>
</asp:UpdatePanel>