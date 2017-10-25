<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoGrid_KS.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoGrid_KS" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="rad" %>
<%@ Register Src="~/Ordering/Controls/Payments/PaymentInfoControl_KS.ascx" tagName="PaymentInfoControl_KS" tagPrefix="cc3" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server"></asp:ScriptManagerProxy>
<asp:Panel ID="pnlPaymentOptionsMessage" runat="server" >
    <cc1:ContentReader ID="ContentReaderPaymentOptionsMessage" runat="server" ContentPath="PaymentOptionsMessage.html" SectionName="Ordering" ValidateContent="true"/>
</asp:Panel>                            

<asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlSelectPager" runat="server" CssClass="pmtbox" meta:resourcekey="pnlSelectPagerResource1">
            <asp:RadioButtonList runat="server" ID="rblPaymentOptions" Style="width: 80%" RepeatDirection="Horizontal" OnSelectedIndexChanged="SelectCurrentPaymentView" AutoPostBack="True" CssClass="radioselectnbg" meta:resourcekey="rblPaymentOptionsResource1">
                <asp:ListItem Selected="True" Text="Credit Card" Value="1" meta:resourcekey="ListItemResource1"></asp:ListItem>
                <asp:ListItem Text="Payment Gateway" Value="2" meta:resourcekey="ListItemResource2"></asp:ListItem>
                <asp:ListItem Text="Wire Transfer" Value="3" meta:resourcekey="ListItemResource3"></asp:ListItem>
                <asp:ListItem Text="Direct Deposit" Value="4" meta:resourcekey="ListItemResource4"></asp:ListItem>
            </asp:RadioButtonList>
        </asp:Panel>
        <asp:Panel ID="pnlMessages" runat="server">
        <table>
        <tr><td>
            <asp:Label ID="lblErrorMessages" runat="server" ForeColor="Red"></asp:Label>
        </td></tr>
        </table>
        </asp:Panel>
        <asp:Panel ID="pnlPaymentOptions" runat="server">
        <rad:RadMultiPage runat="server" ID="mpPaymentOptions" SelectedIndex="0" meta:resourcekey="mpPaymentOptionsResource1">
            <rad:RadPageView ID="pvCredit" runat="server" CssClass="MultiPage" meta:resourcekey="pvCreditResource1" Selected="True">
                <asp:Panel runat="server" ID="pnlCreditTable" meta:resourcekey="pnlCreditTableResource1">
                    <table>
                        <tr>
                            <td>
                                <table class="gdo-checkout-step4-btn-tbl">
                                    <tr>
                                        <td align="right">
                                            <asp:Label ID="lblCardsLimitMessage" runat="server" meta:resourcekey="lblCardsLimitMessageResource1" Text="You can use up to {0} Cards to place this order"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                            <td>
                                <asp:Label ID="lblError" runat="server" ForeColor="Red" meta:resourcekey="lblErrorResource1"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:DataList ID="dataListCardInfo" runat="server" RepeatDirection="Horizontal" RepeatColumns="2" OnItemDataBound="dataListCardInfo_ItemDataBound" ShowFooter="False" ShowHeader="False" Width="100%" ItemStyle-VerticalAlign="Top">
                                    <ItemTemplate>
                                        <cc3:PaymentInfoControl_KS ID="creditPaymentInfo" runat="server" />
                                    </ItemTemplate>
                                </asp:DataList>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Panel class="totalPay" HorizontalAlign="Right" runat="server" ID="pnlTotalDue" Style="display: none" meta:resourcekey="pnlTotalDueResource1">
                                    <asp:Label ID="lbTotalAmount" runat="server" Text="Balance:" meta:resourcekey="Label1Resource2"></asp:Label>
                                    <asp:Label ID="lblCurrencySymbol" runat="server" Visible="false"></asp:Label>
                                    <asp:TextBox ID="totalAmountBalance" runat="server" BorderWidth="0px" BorderStyle="None" ReadOnly="True" BackColor="Transparent" Style="width: 54px" meta:resourcekey="totalAmountBalanceResource1" />
                                </asp:Panel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label Style="margin-left: 10px"  CssClass="red" runat="server" ID="lbMessage" Visible="False" meta:resourcekey="lbMessage"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </rad:RadPageView>
            <rad:RadPageView ID="pvPaymentGateway" runat="server" CssClass="MultiPage">
                <asp:Panel runat="server" ID="pnlPaymentGatewayTable" meta:resourcekey="pnlPaymentGatewayTableResource1">
                    <table>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:HiddenField ID="hidPaymentMethod" runat="server" />
                                            <asp:Label ID="lblPaymentGatewayAmountDue" runat="server" Text="Total Amount to be authorized: " />
                                            <asp:Label ID="totalPaymentGatewayDue" runat="server" meta:resourcekey="totalPaymentGatewayDueResource1" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <cc1:ContentReader ID="paymentGatewayMessage" runat="server" ContentPath="lblPaymentGatewayMessage.html" SectionName="Ordering" ValidateContent="true" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </rad:RadPageView>
            <rad:RadPageView ID="pvWire" runat="server" CssClass="MultiPage" meta:resourcekey="pvWireResource1">
                <asp:Panel runat="server" ID="pnlWireTable">
                    <table>
                        <tr>
                            <td>
                                <asp:DropDownList ID="ddlWire" runat="server">
                                </asp:DropDownList>
                            </td>
                        </tr>                        
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblWireAmountDue" runat="server" Text="Total Wire Amount Due:" meta:resourcekey="lblWireAmountDueResource1" /><asp:Label ID="totalWireDue" runat="server" meta:resourcekey="totalWireDueResource1" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <cc1:ContentReader ID="wireMessage" runat="server" ContentPath="lblWireMessage.html" SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </rad:RadPageView>
            <rad:RadPageView ID="pvDirectDeposit" runat="server" CssClass="MultiPage" meta:resourcekey="pvDirectDepositResource1">
                <asp:Panel runat="server" ID="pnlDirectDepositTable" meta:resourcekey="pnlDirectDepositTableResource1">
                    <table>
                        <tr>
                            <td>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddlDirectDeposit" runat="server" Style="display: none">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
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
                                <cc1:ContentReader ID="directDepositMessage" runat="server" ContentPath="lblDirectDepositMessage.html" SectionName="Ordering" ValidateContent="true"/>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </rad:RadPageView>
        </rad:RadMultiPage>
        </asp:Panel>
        <asp:Panel ID="pnlTotals" runat="server">
        <table>
            <tr align="right">
                <td>
                    <asp:Panel HorizontalAlign="Right" class="totalPay" runat="server" ID="pnlGrandTotal"
                        meta:resourcekey="pnlGrandTotalResource1">
                        <asp:Label runat="server" ID="lblGrandTotal" Text="Grand Total:" meta:resourcekey="lblGrandTotalResource1"></asp:Label>
                        <asp:Label ID="txtGrandTotal" runat="server" readonly="readonly" />
                    </asp:Panel>
                </td>
            </tr>
        </table>
        </asp:Panel>
        <div id="dvReturnedValues" style="display:none">
            <asp:HiddenField ID="XID" runat="server"/> 
            <asp:HiddenField ID="ECI" runat="server"/>
            <asp:HiddenField ID="CAVV" runat="server"/>
            <asp:HiddenField ID="SessionKey" runat="server"/>
            <asp:HiddenField ID="EncryptedData" runat="server"/>
            <asp:HiddenField ID="CardNumber" runat="server"/>
            <asp:HiddenField ID="Verified" runat="server" />
        </div>
        <asp:Button ID="btnAuthorized" runat="server" OnClick="OnAuthorizeAttempt" Text="WasAuthorized" style="display:none"/>   
        <asp:Button ID="btnRefresh" runat="server" style="display:none"/>   
        <script type="text/javascript">
            function setReturnedValues(xid, eci, cavv, skey, enc, card) {
                document.getElementById("<%=XID.ClientID %>").value = xid;
                document.getElementById("<%=ECI.ClientID %>").value = eci;
                document.getElementById("<%=CAVV.ClientID %>").value = cavv;
                document.getElementById("<%=SessionKey.ClientID %>").value = skey;
                document.getElementById("<%=EncryptedData.ClientID %>").value = enc;
                document.getElementById("<%=CardNumber.ClientID %>").value = card;
                document.getElementById("<%=btnAuthorized.ClientID %>").click();
            }
    </script>    
        <asp:Literal ID="ActionCode" runat="server"></asp:Literal> 
    </ContentTemplate>
</asp:UpdatePanel>






