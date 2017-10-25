<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaymentInfoControl_KS.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentInfoControl_KS" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>   

<asp:Panel ID="pnlPaymentInfo" runat="server" Width="80%">
    <table>
        <tr>
            <td>
                <div>
                    <table class="gdo-popup-pad3">
                        <tr>
                            <td>
                                 <asp:Label ID="lblMessage" runat="server" ForeColor="Red"/>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <tr class="lblstaticcards">
            <td>
                <asp:Label ID="lblCardHeader" Text="First Card:" runat="server"/>
            </td>
            <td></td>
        </tr>
        <tr>
            <td>
                <div class="separator10"></div>
            </td>
            <td>                               
            </td>
        </tr>
        <tr>
          <td>
                <div id="dvCreditCardData" style="font-size: x-small;" runat="server">
                    <table>
                        <tr>
                            <td>
                                <asp:LinkButton runat="server" ID="hlClearCard" OnClick="ClearCard" Text="Clear Card"></asp:LinkButton>
                            </td>
                        </tr>
                       <tr>
                            <td>
                                 <asp:Label ID="lblStaticPaymentMethod" Text="CardType:" runat="server" meta:resourcekey="lblCardTypeResource1"/>
                            </td>
                             <td>
                                <asp:Label ID="lblStaticPaymentMethodText" runat="server"/>
                            </td>
                       </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblStaticAmount" Text="Amount-*:" runat="server" meta:resourcekey="lblAmountResource1"/>
                            </td>
                            <td>
                                <asp:Label ID="lblStaticAmountText" runat="server"/>
                            </td>
                        </tr>
                    </table>
                </div>
                <div id="dvCreditCardInfo" runat="server">
                    <table class="gdo-popup-pad3">
                         <tr>
                            <td align="right">
                                <asp:Label ID="lblCardType" Text="Card type*:" runat="server" meta:resourcekey="lblCardTypeResource1"/>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddlCardType" runat="server" TabIndex="2" AutoPostBack="True" onselectedindexchanged="ddlCardType_SelectedIndexChanged"  >                                    
                                    <asp:ListItem Value="0" Text="Select" meta:resourcekey="SelectCard" />                                   
                                    <asp:ListItem Text="Hyundai" Value="4x" meta:resourcekey="HyundaiCard" />
                                    <asp:ListItem Text="Shinhan" Value="6x" meta:resourcekey="ShinhanCard" />
                                    <asp:ListItem Text="KB Card" Value="1i" meta:resourcekey="KBCard" />
                                    <asp:ListItem Text="BC Card" Value="2i" meta:resourcekey="BCCard" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="paymentMethod" runat="server">
                            <td align="right" runat="server">
                                <asp:Label ID="lblPaymentMethod" Text="Payment Method:" runat="server" meta:resourcekey="lblPaymentMethodResource1"/>
                            </td>
                            <td runat="server">
                                <asp:TextBox ID="txtPaymentMethod" runat="server" ReadOnly="true" Enabled="false"></asp:TextBox>
                                <asp:TextBox ID="txtPaymentMethodType" runat="server" style="display: none"></asp:TextBox>
                            </td>
                        </tr>
                        <tr valign="middle">
                            <td align="right">
                                <asp:Label ID="Label1" Text="Cardholder Name:" runat="server" meta:resourcekey="Label1Resource1"/>
                            </td>
                            <td>
                                <asp:TextBox ID="txtCardholderName" MaxLength="30" runat="server"/>
                            </td>
                        </tr>
                        <tr valign="middle">
                            <td align="right">
                                <asp:Label ID="lblAmount" Text="Amount-*:" runat="server" meta:resourcekey="lblAmountResource1"/>
                            </td>
                            <td>
                                <asp:TextBox ID="txtAmount" MaxLength="16" runat="server" TabIndex="3" />
                            </td>
                        </tr>
                        <tr valign="middle">
                            <td align="right">
                                <asp:Label ID="lblInstallments" Text="Installment*:" runat="server" meta:resourcekey="lblInstallmentsResource1"/>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddlInstallments" runat="server" AutoPostBack="True" onselectedindexchanged="ddlInstallments_SelectedIndexChanged"></asp:DropDownList>
                            </td>
                        </tr>
                        <tr id="ispBCTopPoints" runat="server">
                            <td align="right" runat="server">
                                <asp:Label ID="lblBCPoint" Text="BC Top Point*:" runat="server" ></asp:Label>
                            </td>
                            <td runat="server">
                                <asp:DropDownList ID="ddlBCPoint" runat="server">
                                    <asp:ListItem Text="Use" Value="60" meta:resourcekey="BCPointUse" />
                                    <asp:ListItem Text="Dont Use" Value="0" Selected="True" meta:resourcekey="BCPointDontUse" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <cc1:DynamicButton ID="btnContinue" OnClientClick="return ValidateAndSubmit(this)" OnClick="btnContinue_Click" runat="server" Coloring="Gold" Text="Continue" meta:resourcekey="btnContinueResource1" CssClass="forward" />
                                
                                <cc1:DynamicButton ID="hdButton" runat="server" Style="display: none;" Text="Continue" OnClick="btnContinue_Click"></cc1:DynamicButton>

                                <asp:TextBox ID="txtId" runat="server" Style="display: none" />
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
    </table>
    <asp:Literal runat="server" ID="theFrame"></asp:Literal>
</asp:Panel>