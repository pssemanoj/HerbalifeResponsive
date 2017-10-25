<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PurchasingLimits_FR.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.PurchasingLimits_FR" %>

<table style="width:100%" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="gdo-right-column-text" colspan="2">
            <asp:Label ID="lbltext" runat="server" Font-Bold="True" Font-Size="Small" 
                Visible="False" ForeColor="Blue" meta:resourcekey="lbltextResource1"></asp:Label>
        </td>        
    </tr>
    <tr id="trOrderType" runat="server">
    <td colspan="2">
        <table><tr>
        <td class="gdo-minicart-label">
            <asp:Label runat="server" ID="lblOrderType" Text="OrderType:" 
                meta:resourcekey="lblOrderTypeResource1"></asp:Label>
        </td>
        <tr></tr>
        <td valign="bottom" class="gdo-minicart-value">
            <asp:DropDownList ID="ddl_DSSubType" runat="server" AutoPostBack="True" 
                onselectedindexchanged="ddl_DSSubType_SelectedIndexChanged" 
                meta:resourcekey="ddl_DSSubTypeResource1"></asp:DropDownList>
                <asp:Label ID="lblOrderTypeVal" runat="server" Visible="False" 
                meta:resourcekey="lblOrderTypeValResource1" />
        </td></tr></table></td>
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

