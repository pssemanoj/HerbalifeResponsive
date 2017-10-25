<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfo_es-CR.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways.PaymentInfo_es_CR" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register src="CCTokenizer.ascx" tagname="CCTokenizer" tagprefix="uc1" %>

<asp:UpdatePanel ID="pnlCards" runat="server">
<ContentTemplate>
    <asp:Panel runat="server" ID="pnlCRCards">
        <div>
            <asp:Label ForeColor="Red" runat="server" ID="lblError"></asp:Label>
        </div>

        <div id="ccType">
            <asp:Label runat="server" ID="lblCardsList" meta:resourceKey="lblCardsList">Credit Cards:</asp:Label>
            <asp:DropDownList runat="server" ID="ddlCards" TabIndex="1">
                <asp:ListItem Text="Visa" Value="VI" />
                <asp:ListItem Text="MasterCard" Value="MC" />
                <asp:ListItem Text="American Express" Value="AX" />
            </asp:DropDownList>
        </div>

        <div id="ccDataPnl">
            <div>
                <asp:Label runat="server" Text="Card Number:" meta:resourcekey="lblCardNumberResource" CssClass="crPaymentLbl"></asp:Label>
                <asp:TextBox ID="txtCardNumber" MaxLength="16" runat="server" meta:resourcekey="txtCardNumberResource1" TabIndex="2" ></asp:TextBox>
            </div>

            <asp:Panel ID="pnlExpDate" runat="server" meta:resourcekey="pnlExpDateResource1" CssClass="expDatePnl">
                <asp:Label ID="lblExpDate" Text="Exp Date*:" runat="server" meta:resourcekey="lblExpDateResource1"></asp:Label>
                <asp:DropDownList ID="ddlExpMonth" runat="server" TabIndex="3">
                    <asp:ListItem Text="01" Value="1"></asp:ListItem>                         
                    <asp:ListItem Text="02" Value="2"></asp:ListItem>                         
                    <asp:ListItem Text="03" Value="3"></asp:ListItem>                         
                    <asp:ListItem Text="04" Value="4"></asp:ListItem>                         
                    <asp:ListItem Text="05" Value="5"></asp:ListItem>                         
                    <asp:ListItem Text="06" Value="6"></asp:ListItem>                         
                    <asp:ListItem Text="07" Value="7"></asp:ListItem>                         
                    <asp:ListItem Text="08" Value="8"></asp:ListItem>                         
                    <asp:ListItem Text="09" Value="9"></asp:ListItem>                         
                    <asp:ListItem Text="10" Value="10"></asp:ListItem>                         
                    <asp:ListItem Text="11" Value="11"></asp:ListItem>                         
                    <asp:ListItem Text="12" Value="12"></asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddlExpYear" runat="server" TabIndex="4">
                </asp:DropDownList>
            </asp:Panel>

            <div>
                <asp:Label runat="server" Text="CVV:" meta:resourcekey="lblCVVResource" CssClass="crPaymentLbl"></asp:Label>
                <asp:HyperLink ID="lnkCVVHelp" runat="server" ImageUrl="/Content/Global/img/gdo/icons/question-mark-blue.png" Target="_blank" CssClass="gdo-question-mark-blue"></asp:HyperLink>
                <asp:TextBox ID="txtCVV" MaxLength="4" runat="server" meta:resourcekey="txtCardCVVResource1" TabIndex="5"></asp:TextBox> 
            </div>

        </div>
        <uc1:CCTokenizer runat="server" ID="CCTokenizer" CardNumberControlId="txtCardNumber" CardTypeControlId="ddlCards" CVVControlId="txtCVV" ExpirationMonthControlId="ddlExpMonth" 
            ExpirationYearControlId="ddlExpYear" MessageLabelControlId="lblError" SubmitButtonControlId="checkOutButton" ValidateAddCard="ValidateAddCard" ValidateAddCardBadCard="ValidateAddCardBadCard" 
            ValidateAddCardExpired="ValidateAddCardExpired" ValidateAddCardMissingCard="ValidateAddCardMissingCard" ValidateAddCardMissingExpDate="ValidateAddCardMissingExpDate" 
            ValidateSelectCardType="ValidateSelectCardType" ValidateTokenizationFailed="ValidateTokenizationFailed" ValidateCVVRequired="ValidateCVVRequired"
            DontTokenizeJustMaskAndUseSessionStorage="true"/>
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>
