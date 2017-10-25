<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoGrid.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoGrid" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="rad" %>
<%@ Register Src="~/Ordering/Controls/Payments/PaymentInfoControl.ascx" TagName="PaymentInfoControl" TagPrefix="hrblPaymentInfoControl" %>
<%@ Register TagPrefix="cc3" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>
<asp:Panel ID="pnlPaymentOptionsMessage" runat="server">
    <script type="text/javascript">
        var PaymentGatewayHasCardDataControlId = '<%= PaymentGatewayHasCardData.ClientID %>';
</script>
    <cc1:ContentReader ID="ContentReaderPaymentOptionsMessage" runat="server" ContentPath="PaymentOptionsMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
</asp:Panel>

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="PaymentGatewayHasCardData" />
        <asp:Panel ID="pnlSelectPager" runat="server" CssClass="pmtbox" meta:resourcekey="pnlSelectPagerResource1">
            <asp:RadioButtonList runat="server" ID="rblPaymentOptions" Style="width: 80%" RepeatDirection="Horizontal"
                OnSelectedIndexChanged="SelectCurrentPaymentView" AutoPostBack="True" CssClass="radioselectnbg"
                meta:resourcekey="rblPaymentOptionsResource1">
                <asp:ListItem Selected="True" Text="Credit Card" Value="1" meta:resourcekey="ListItemResource1"></asp:ListItem>
                <asp:ListItem Text="Payment Gateway" Value="2" meta:resourcekey="ListItemResource2"></asp:ListItem>
                <asp:ListItem Text="Wire Transfer" Value="3" meta:resourcekey="ListItemResource3"></asp:ListItem>
                <asp:ListItem Text="Direct Deposit" Value="4" meta:resourcekey="ListItemResource4"></asp:ListItem>
            </asp:RadioButtonList>
        </asp:Panel>
        <asp:Panel ID="pnlMessages" runat="server">
            <asp:Label ID="lblErrorMessages" runat="server" CssClass="gdo-error-message-txt"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="pnlPaymentOptions" runat="server">
            <rad:RadMultiPage runat="server" ID="mpPaymentOptions" SelectedIndex="0" meta:resourcekey="mpPaymentOptionsResource1">
                <rad:RadPageView ID="pvCredit" runat="server" CssClass="MultiPage" meta:resourcekey="pvCreditResource1"
                    Selected="True">
                    <asp:Panel runat="server" ID="pnlCreditTable" meta:resourcekey="pnlCreditTableResource1">
                        <table>
                            <tr>
                                <td>
                                    <cc1:ContentReader ID="merchantDetailsTC" runat="server" ContentPath="merchantDetails.html" SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table class="gdo-checkout-step4-btn-tbl">
                                        <tr>
                                            <td>
                                                <div class="gdo-button-margin-lt bttn-addnewcc">
                                                    <p>
                                                        <asp:Label ID="lblCardsLimitMessage" runat="server" meta:resourcekey="lblCardsLimitMessageResource1"
                                                            Text="You can use up to {0} Cards to place this order"></asp:Label>
                                                    </p>
                                                    <cc2:DynamicButton ID="btnAddNewCreditCard" runat="server" ButtonType="Neutral" meta:resourcekey="btnAddNewCreditCardResource1"
                                                        OnClick="btnAddNewCreditCard_Click" Text="Add New Credit Card" />
                                                </div>
                                            </td>
                                            <%--<td align="right">
                                                <asp:Label ID="lblCardsLimitMessage" runat="server" meta:resourcekey="lblCardsLimitMessageResource1"
                                                    Text="You can use up to {0} Cards to place this order"></asp:Label>
                                            </td>--%>
                                            <td>
                                                <cc1:ContentReader CssClass="PCILeveBox" runat="server" Visible="true" ContentPath="pcilevelbox.html" UseLocal="true"/>
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
                                        CssClass="gdo-order-tbl spacepixel responsive-table" EnableModelValidation="True">
                                        <AlternatingRowStyle CssClass="gdo-row-odd gdo-order-tbl-data"></AlternatingRowStyle>
                                        <Columns>
                                            <asp:TemplateField meta:resourceKey="TemplateFieldResource1">
                                                <ItemTemplate>
                                                    <asp:Image ID="imgDeclined" runat="server" Visible="false" />
                                                    <asp:TextBox ID="cardID" runat="server" Text='<%# Bind("ID") %>' Style="display: none" />
                                                    <asp:TextBox ID="PayOptionsOffset" runat="server" Style="display: none" />
                                                </ItemTemplate>
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Nickname" meta:resourceKey="hdrNickName">
                                                <ItemTemplate>
                                                    <cc3:NostalgicDropDownList ID="ddlCards" runat="server" AutoPostBack="True" CssClass="ccselect"
                                                        meta:resourcekey="ddlCardsResource1" OnSelectedIndexChanged="OnCardSelected">
                                                    </cc3:NostalgicDropDownList>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Card Holder" meta:resourceKey="hdrCardHolder">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCardHolder" runat="server" meta:resourceKey="lblCardHolderResource1"
                                                        Text='<%# string.Format("{0} {1} {2}", Eval("CardHolder.First"), Eval("CardHolder.Middle"), Eval("CardHolder.Last")) %>'></asp:Label></ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Card Type" meta:resourceKey="hdrCardType">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCardType" runat="server" meta:resourceKey="lblCardTypeResource1"
                                                        Text='<%# getCardName(Eval("CardType") as string) %>'></asp:Label>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-left" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Card Number" meta:resourceKey="hdrCardNumber">
                                                <ItemTemplate>
                                                    <asp:Label ID="lblCardNumber" runat="server" meta:resourceKey="lblCardNumberResource1"
                                                        Text='<%# getCardNumber(Eval("CardNumber") as string, Eval("CardType") as string) %>'></asp:Label>
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
                                                    <asp:Label ID="lblExp" runat="server" meta:resourceKey="lblExpResource1" Text='<%# (DateTime.Parse(Eval("Expiration").ToString())==GetMyKeyExpirationDate()) ? "" :Eval("Expiration","{0:MM-yyyy}") %>'></asp:Label>
                                                    <asp:Label ID="Label1" runat="server" CssClass="red" meta:resourceKey="Label1Resource1"
                                                        Text='<%# isExpires(DateTime.Parse(Eval("Expiration").ToString())) == false ? "" : (this.GetLocalResourceObject("strExpires") as string) %>'></asp:Label>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Issue Number or Date" meta:resourceKey="hdrIssueNumber"
                                                Visible="False">
                                                <ItemTemplate>
                                                    <asp:TextBox ID="txtIssueNumber" runat="server" MaxLength="6" meta:resourcekey="txtIssueNumberResource1"
                                                        size="4" Style="display: none"></asp:TextBox></ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField meta:resourcekey="TemplateFieldResource2">
                                                <HeaderTemplate>
                                                    <asp:Label ID="lblCVVHeader" runat="server" meta:resourcekey="lblCVVHeaderResource1"
                                                        Style="float: left" Text="CVV*"></asp:Label>
                                                    <div style="float: right; padding-right: 3px">
                                                        <asp:HyperLink ID="lnkCVVHelp" runat="server" NavigateUrl='<%#GetCVVHelpLink()%>'
                                                            ImageUrl="/Content/Global/img/gdo/icons/question-mark-blue.png" Target="_blank"
                                                            CssClass="gdo-question-mark-blue"></asp:HyperLink>
                                                    </div>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:TextBox ID="txtCVV" runat="server" AutoCompleteType="None" CssClass="gdo-product-qty"
                                                        Enabled='<%# isExpires(DateTime.Parse(Eval("Expiration").ToString())) == false %>'
                                                        size="3"></asp:TextBox>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Installments"
                                                Visible="false">
                                                <HeaderTemplate>
                                                    <asp:Label ID="lblInstallmentsHeader" runat="server" meta:resourcekey="lblInstallmentsHeaderResource"
                                                        Style="float: left" Text="Installments"></asp:Label>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:DropDownList ID="drpInstallments" Visible="false" runat="server">
                                                    </asp:DropDownList>
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField HeaderText="Amount" meta:resourceKey="Amount">
                                                <ItemTemplate>
                                                    <asp:TextBox ID="txtAmount" runat="server" CssClass="gdo-product-qty" Enabled='<%# isExpires(DateTime.Parse(Eval("Expiration").ToString())) == false %>'
                                                        size="6"></asp:TextBox></ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                            <asp:TemplateField meta:resourcekey="TemplateFieldResource3">
                                                <ItemTemplate>
                                                    <cc2:DynamicButton ID="lnkEdit" runat="server" CausesValidation="False" meta:resourceKey="lnkEdit"
                                                        Visible='<%#IsEditable() == true%>' OnClick="btnEdit_Click" Text="Edit" ButtonType="Link" />
                                                </ItemTemplate>
                                                <ControlStyle CssClass="gdo-order-tbl-data-center" />
                                                <HeaderStyle HorizontalAlign="Center" />
                                            </asp:TemplateField>
                                        </Columns>
                                        <HeaderStyle CssClass="gray_background" Height="30px" />
                                        <RowStyle CssClass="gdo-row-even gdo-order-tbl-data" />
                                    </asp:GridView>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Panel class="totalPay" runat="server" ID="pnlTotalDue" Style="display: none"
                                        meta:resourcekey="pnlTotalDueResource1">
                                        <asp:Label ID="lbTotalAmount" runat="server" Text="Balance:" meta:resourcekey="Label1Resource2"></asp:Label>
                                        <asp:Label ID="lblCurrencySymbol" runat="server" Visible="false"></asp:Label>
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
                </rad:RadPageView>
                <rad:RadPageView ID="pvPaymentGateway" runat="server" CssClass="MultiPage" meta:resourcekey="pvPaymentGatewayResource1">
                    <asp:Panel runat="server" ID="pnlPaymentGatewayTable" meta:resourcekey="pnlPaymentGatewayTableResource1">
                        <table>
                            <tr>
                                <td>
                                    <cc1:ContentReader ID="merchantDetailsPG" runat="server" ContentPath="merchantDetails.html" SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblPaymentGatewayErrorMessage" runat="server" ForeColor="Red"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <cc1:ContentReader CssClass="PCILeveBox" runat="server" Visible="true" ContentPath="pcilevelbox.html" UseLocal="true"/>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Panel ID="pnlPaymentGatewayControlHolder" runat="server">
                                    </asp:Panel>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:HiddenField ID="hidPaymentMethod" runat="server" />
                                                <asp:Label ID="lblPaymentGatewayAmountDue" runat="server" Text="Total Amount to be authorized: " meta:resourcekey="lblPaymentGatewayAmountDueResource1" />
                                                <asp:Label ID="totalPaymentGatewayDue" runat="server" meta:resourcekey="totalPaymentGatewayDueResource1" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <cc1:ContentReader ID="paymentGatewayMessage" runat="server" ContentPath="lblPaymentGatewayMessage.html" SectionName="Ordering" ValidateContent="true" uselocal="true"/>
                                    <asp:Label ID="lblPaymentGatewayMessage" runat="server" meta:resourcekey="lblpaymentGatewayMessageText" CssClass="bold" ></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </rad:RadPageView>
                <rad:RadPageView ID="pvWire" runat="server" CssClass="MultiPage" meta:resourcekey="pvWireResource1">
                    <asp:Panel runat="server" ID="pnlWireTable" meta:resourcekey="pnlWireTableResource1">
                        <table>
                            <tr>
                                <td>
                                    <asp:DropDownList ID="ddlWire" AutoPostBack="true"  runat="server" Style="display: none"></asp:DropDownList>
                                    <br />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lblWireAmountDue" runat="server" Text="Total Wire Amount Due:" meta:resourcekey="lblWireAmountDueResource1" />
                                                <asp:Label ID="totalWireDue" runat="server" meta:resourcekey="totalWireDueResource1" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                             <td>
                                    <cc1:ContentReader ID="wireMessage" runat="server" ContentPath="lblWireMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                                </td>
                            <tr>
                                <td>
                                    <cc1:ContentReader ID="wireMessage1" runat="server" ContentPath="lblWireMessage3.html" Visible="false" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                   <cc1:ContentReader ID="wireMessage2" runat="server" ContentPath="lblWireMessage2.html"  Visible="false" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </rad:RadPageView>
                <rad:RadPageView ID="pvDirectDeposit" runat="server" CssClass="MultiPage" meta:resourcekey="pvDirectDepositResource1">
                    <asp:Panel runat="server" ID="pnlDirectDepositTable" meta:resourcekey="pnlDirectDepositTableResource1">
                        <div>
                            <asp:DropDownList ID="ddlDirectDeposit" runat="server" Style="display: none">
                            </asp:DropDownList>
                        </div>
                        <div>
                            <asp:Label ID="lblDirectDepositAmountDue" runat="server" Text="Total Direct Deposit Amount: "
                                meta:resourcekey="lblDirectDepositAmountDueResource1" />
                            <asp:Label ID="totalDirectDepositDue" runat="server" meta:resourcekey="totalDirectDepositDueResource1" />
                        </div>
                        <div>
                            <cc1:ContentReader ID="directDepositMessage" runat="server" ContentPath="lblDirectDepositMessage.html"
                                SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                        </div>
                    </asp:Panel>
                </rad:RadPageView>
            </rad:RadMultiPage>
            <asp:Panel ID="pnlHAPAcknowledgement" runat="server" Visible="false">
                <cc1:ContentReader ID="hapMessage" runat="server" ContentPath="lblHapActivationAcknowledgement.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
            </asp:Panel>
            <asp:Panel ID="pnlAcknowledCheckContent" runat="server" CssClass="errorConfirm" Visible="false">
                <asp:CheckBox runat="server" ID="chkAcknowledgeTransaction" AutoPostBack="true" OnCheckedChanged="AcknowledgeChanged" meta:resourceKey="PaymentAcknowledgement" Text="I acknowledge this transaction" />
                <asp:Label runat="server" ID="lblAcknoledgeCheckContent2"  meta:Resourcekey="PaymentAcknowledgement2" />
            </asp:Panel>
            <asp:Panel ID="pnlAcknowledgeCheckTerms" runat="server" CssClass="errorConfirm" Visible="false">
                <asp:CheckBox runat="server" ID="chkAcknowledgeTerms" AutoPostBack="true" OnCheckedChanged="AcknowledgeChanged_2" meta:resourceKey="ConditionsAcknowledgement" Text="I agree with the conditions and privacy policy" />
            </asp:Panel>
        </asp:Panel>
        <asp:Panel ID="pnlTotals" runat="server">
            <table>
                <tr align="right">
                    <td>
                        <asp:Panel class="totalPay" runat="server" ID="pnlGrandTotal" meta:resourcekey="pnlGrandTotalResource1">
                            <asp:Label runat="server" ID="lblGrandTotal" style="font-weight: bold" Text="Grand Total:" meta:resourcekey="lblGrandTotalResource1"></asp:Label>
                            <asp:Label ID="txtGrandTotal" runat="server" style="font-weight: bold" readonly="readonly" meta:resourcekey="txtGrandTotalResource1" />
                        </asp:Panel>
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlBottomMessages" runat="server">
            <cc1:ContentReader ID="creditCardMessage" runat="server" ContentPath="CreditCardMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
            <asp:Panel ID="pnlHapBottomMessage" runat="server" Visible="false" style="clear:both;text-align:right;">
                <asp:Label ID="lblHapBottomMessage" runat="server" Text="" meta:resourcekey="lblHapBottomMessageResource1"></asp:Label>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="ucPaymentInfoControl" />
        <asp:AsyncPostBackTrigger ControlID="chkAcknowledgeTransaction" />
    </Triggers>
</asp:UpdatePanel>
<asp:Panel ID="PayOptionsQH" CssClass="gdo-right-column-tbl gdo-hff-module" runat="server" Style="border-style: solid; border-width: thin; width: 190px; height: 270; background-color: White" Visible="False" >
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
                        <td>
                        </td>
                        <td>
                        </td>
                        <td>
                        </td>
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
                                    <asp:Label runat="server" ID="lbInstallments" Text="Installments" meta:resourcekey="lbInstallments" /></label>
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
                        <td colspan="3" style="height: 12px">
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <span class="NoBorder">
                                <input id="Bonus1" type="radio" name="chkPayOptionQH" value="7" />
                                <label for="Bonus1">
                                    <asp:Label runat="server" ID="lbBonus1" Text="Bonus 1" meta:resourcekey="lbBonus1"></asp:Label>
                                </label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth" Text="Select Month:" meta:resourcekey="lbSelectMonth" />
                        </td>
                        <td style="width: 20px">
                        </td>
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
                                    <asp:Label runat="server" ID="lbBonus2" Text="Bonus 2" meta:resourcekey="lbBonus2" />
                                </label>
                            </span>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth1" Text="Select Month 1:" meta:resourcekey="lbSelectMonth1" />
                        </td>
                        <td style="width: 20px">
                        </td>
                        <td style="border-style: none" align="right">
                            <asp:DropDownList ID="Bonus2Month1" runat="server" meta:resourcekey="Bonus2Month1Resource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server" ID="lbSelectMonth2" Text="Select Month 2:" meta:resourcekey="lbSelectMonth2" />
                        </td>
                        <td style="width: 20px">
                        </td>
                        <td align="right">
                            <asp:DropDownList ID="Bonus2Month2" runat="server" meta:resourcekey="Bonus2Month2Resource1">
                            </asp:DropDownList>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td colspan="2" style="height: 12px">
            </td>
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
    Style="border-style: solid; border-width: thin; width: 130px; height: 140px;
    background-color: White" Visible="False" meta:resourcekey="PayOptionsALResource1">
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
            <td colspan="2" style="height: 12px">
            </td>
        </tr>
        <tr>
            <td>
                <asp:Button ID="btnCancelAL" runat="server" Text="Cancel" OnClientClick="return false;"
                    meta:resourcekey="btnCancelALResource1" />
            </td>
            <td align="right">
                <asp:Button ID="btnOkAL" runat="server" Text="Ok" meta:resourcekey="btnOkALResource1" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:HiddenField ID="Validations" runat="server" />
<asp:Button runat="server" ID="btnQH" Style="display: none" meta:resourcekey="btnQHResource1" />
<asp:Button runat="server" ID="btnAL" Style="display: none" meta:resourcekey="btnALResource1" />
<asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <hrblPaymentInfoControl:PaymentInfoControl ID="ucPaymentInfoControl" runat="server" />
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnAddNewCreditCard" />
        <asp:AsyncPostBackTrigger ControlID="gridViewCardInfo" />
        <asp:AsyncPostBackTrigger ControlID="ddlWire" />
    </Triggers>
</asp:UpdatePanel>