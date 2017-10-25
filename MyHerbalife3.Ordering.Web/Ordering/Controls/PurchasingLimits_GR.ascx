<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PurchasingLimits_GR.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PurchasingLimits_GR" %>
<table style="width:100%" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="gdo-right-column-text" colspan="2">
            <asp:Label ID="lbltext" runat="server" Font-Bold="True" Font-Size="Small" 
                Visible="False" ForeColor="Blue" meta:resourcekey="lbltextResource1"></asp:Label>
        </td>        
    </tr>
    <tr id="trOrderType" runat="server">
        <td colspan="2">
            <table>
                <tr>
                    <td class="gdo-minicart-label">
                        <asp:Label runat="server" ID="lblOrderType" Text="OrderType:" meta:resourcekey="lblOrderTypeResource1"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td valign="bottom" class="gdo-minicart-value">
                        <asp:DropDownList ID="ddl_DSSubType" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddl_DSSubType_SelectedIndexChanged"
                            meta:resourcekey="ddl_DSSubTypeResource1">
                            <asp:ListItem Text="Personal Consumption" Value="PC" meta:resourcekey="ListItemShipPersonalConsumption"></asp:ListItem>
                            <asp:ListItem Text="Ship to Customer" Value="RE" meta:resourcekey="ListItemShipToCustomer"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:Label ID="lblOrderTypeVal" runat="server" Visible="False" meta:resourcekey="lblOrderTypeValResource1" />
                    </td>
                </tr>
                <tr>
                    <td class="gdo-minicart-label">
                        <asp:Label runat="server" ID="lblMessageZeroPercent" Text="Please enter the data and the address of your customer."
                            meta:resourcekey="lbDiscountMessage"></asp:Label>
                    </td>
                </tr>
            </table>
        </td>
    </tr>
    <tr id="trRemainingVal" runat="server">
        <td class="gdo-minicart-label">
            <asp:Label runat="server" ID="lblRemainingValDisplay" 
                meta:resourcekey="lblRemainingValDisplayResource1"></asp:Label>
        </td>
        <td align="right" valign="bottom" class="gdo-minicart-value">
            <asp:Label ID="lblRemainingVal" runat="server" 
                meta:resourcekey="lblRemainingValResource1"></asp:Label>
        </td>
    </tr>

</table>