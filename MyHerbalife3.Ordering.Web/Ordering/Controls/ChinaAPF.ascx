<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChinaAPF.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ChinaAPF" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<script type="text/javascript">
    function Numeric(e, ctrl) {
        var keynum;
        var keychar;
        if (!e)
            var e = window.event
        if (e.keyCode) keynum = e.keyCode;
        else if (e.which) keynum = e.which;

        keychar = String.fromCharCode(keynum);

        // take only numbers
        if (!/^ *[0-9]+ *$/.test(keychar)) {
            e.cancelBubble = true;
            if (e.keyCode) // IE
            {
                e.keyCode = 2;
            }
            else if (e.which) {
                if (e.preventDefault) e.preventDefault();
                if (e.stopPropagation) e.stopPropagation();

            }
        }
    }
</script>
<asp:UpdatePanel ID="upAPF" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlAPFIsDueWithinThreeMonth" runat="server" meta:resourcekey="pnlAPFResource2"
            CssClass="gdo-right-column-tbl gdo-apf-module">
            <div class="gdo-spacer2">
            </div>
            <table style="width: 100%">
                <tr>
                    <td>
                        <table style="width: 100%" id="tblMessage" runat="server">
                            <tr style="height: 20px">
                                <td colspan="2">
                                    <h3>
                                        <asp:Label ID="lblHeading" runat="server" meta:resourcekey="APF" /></h3>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="height: 15px; width: 50%">
                                    <asp:Label ID="lblAPFMessage" runat="server" Text="APF Message:" meta:resourcekey="lblAPFMessageResource1" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div class="gdo-spacer2">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td>
                        <table style="width: 100%" id="tblEditable" runat="server">
                            <tr>
                                <td style="height: 15px">
                                    <asp:Label ID="lblAPFType" runat="server" Text="Supervisor APF" />
                                </td>
                                <td style="height: 15px">
                                    <div class="gdo-apf-input">
                                        <asp:Label ID="lblAPFAmount" runat="server" meta:resourcekey="lblAPFAmountResource1" />
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <div class="gdo-spacer2">
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td style="height: 15px">
                                    <asp:Label ID="lblQuantity" runat="server" Text="Quantity: " meta:resourcekey="lblQuantityResource1" />
                                </td>
                                <td style="height: 15px">
                                    <div class="gdo-apf-input">
                                        <asp:TextBox ID="txtQuantity" runat="server" MaxLength="1" Width="45px" ReadOnly="true" />
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2" style="height: 15px">
                                    <asp:Label ID="lblError" runat="server" ForeColor="Red" Visible="false" />
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <div class="apfbuttonaddcart">
                                        <cc1:DynamicButton ID="btnAddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                                            OnClick="btnAddToCart_Click" meta:resourcekey="btnAddToCartResource1" />
                                    </div>
                                    <div class="gdo-spacer2">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="pnlAPFPaid" runat="server" CssClass="gdo-right-column-tbl gdo-apf-module"
            Height="100px">
            <div class="gdo-spacer2">
            </div>
            <table style="width: 100%">
                <tr style="height: 20px">
                    <td colspan="2">
                        <h3>
                            <asp:Label ID="lblHeading2" runat="server" meta:resourcekey="APF" /></h3>
                    </td>
                </tr>
                <tr>
                    <td>
                        <div class="gdo-spacer2">
                        </div>
                    </td>
                </tr>
                <tr>
                    <td style="height: 15px">
                        <asp:Label ID="lblAPFPaidMessage" runat="server" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
        
        <div class="gdo-spacer2">
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
