<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductInfoFooter.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductInfoFooter" %>


&nbsp;
<table class="specialtable" cellspacing="0" border="0" width="100%" runat="server" id="tabIcons">
    <tr>
        <td align="right">
            <div runat="server" id="divIcons" class="DivProdIcons">
            </div>
        </td>
    </tr>
</table>
<div class="disclaimer" runat="server" id="divDisclaimer">
    <asp:Repeater runat="server" ID="Disclaimer" OnItemDataBound="DisclaimerDataBound">
        <ItemTemplate>
            <p runat="server" id="pDisclaimer">
            </p>
        </ItemTemplate>
    </asp:Repeater>
</div>
<!-- /.disclaimer -->

