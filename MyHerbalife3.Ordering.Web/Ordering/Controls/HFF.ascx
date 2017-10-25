<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HFF.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HFF" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
                
<div id="gdo-right-column-hff">
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
         function Check(val) {
             if (val.indexOf("RadioButton1") > -1) {
                document.getElementById('<%= txtAmount.ClientID %>').value = '';
                document.getElementById('<%= txtAmount.ClientID %>').disabled = true;
            }
             if (val.indexOf("RadioButton2") > -1) {
                document.getElementById('<%= txtAmount.ClientID %>').value = '';
                document.getElementById('<%= txtAmount.ClientID %>').disabled = true;
            }
             if (val.indexOf("RadioButton3") > -1)
                document.getElementById('<%= txtAmount.ClientID %>').disabled = false;
        }
    </script>
    <asp:UpdatePanel runat="server" ID="hffPanel" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-right-column-tbl gdo-hff-module" runat="server" id="divHFF">
                <div class="gdo-right-column-header">
                    <h3>
                        <asp:Literal ID="ltLine1" runat="server" meta:resourcekey="ltLine1Resource1" Text="HFF Donation"></asp:Literal>
                    </h3>
                </div>
                <div class="gdo-clear gdo-horiz-div">
                </div>
                <div class="gdo-right-column-text" id="divMsg" runat="server">
                    <asp:Label ID="lblHFFMsg" runat="server" ForeColor="Blue"></asp:Label>
                    <div class="gdo-spacer2">
                    </div>
                </div>
                <div class="gdo-right-column-text">
                    <asp:Label ID="lblHFF" runat="server" Text="Do you wish to add to this order an optional donation to the Herbalife's Family Foundation project?"
                        AutoPostBack="True" meta:resourcekey="lblHFFResource1" />
                </div>
                <div class="gdo-spacer2">
                </div>          
                    <div id="HffBase" runat="server" >
                    <asp:HyperLink ID="hlHFF" runat="server" Target="_blank" meta:resourcekey="hlHFFResource1"
                        Text="About Casa Herbalife" NavigateUrl=""></asp:HyperLink><br />
                    <% if (!HLConfigManager.Configurations.DOConfiguration.IsChina) { %>
                        <cc1:DynamicButton ID="ClearDonation" runat="server" Text="Clear Donation" ButtonType="Link"
                           OnClick="btnClearDonation_Click" meta:resourcekey="ClearDonationResource1" />&nbsp;&nbsp;
                    <% } %>
                        <div id="NewHFF" runat="server">
                            <table class="gdo-order-details-tbl">
                                <tr>
                                    <td >
                                        <asp:RadioButton ID="RadioButton1" runat="server" Text="$1" TextAlign="Right"  GroupName="SelectOne" meta:resourcekey="RadioButton1Resource" onclick="Check(this.id)" />
                                    </td>
                                    <td></td>
                                </tr>
                                <tr>
                                    <td >
                                        <asp:RadioButton ID="RadioButton2" runat="server" Text="$5" TextAlign="Right" GroupName="SelectOne" meta:resourcekey="RadioButton2Resource" onclick="Check(this.id)" />
                                    </td>
                                    <td></td>
                                </tr>
                                <tr>
                                    <td >
                                        <asp:RadioButton ID="RadioButton3" Checked="true"  runat="server" Text="OtherAmount" onclick="Check(this.id)" meta:resourcekey="RadioButton3Resource" GroupName="SelectOne" />
                                    </td>
                                    <td >
                                        <asp:TextBox ID="txtAmount" runat="server" AutoPostBack="True" Width="42px" OnTextChanged="tbQuantity_TextChanged" MaxLength="3" CssClass="onlyNumbers"></asp:TextBox>
                                    </td>
                                </tr>
                            </table>
                        </div>
                        <div id="OldHFF" runat="server">
                            <asp:Label ID="lblHFFCurrencyType" runat="server"></asp:Label>
                            <asp:TextBox ID="tbQuantity" runat="server" meta:resourcekey="tbQuantityResource1"
                                AutoPostBack="True" OnTextChanged="tbQuantity_TextChanged" Width="42px" CssClass="onlyNumbers"></asp:TextBox>
                        </div>                  
                    <div class="gdo-spacer2">
                    </div>
                    <div class="hffbuttonaddcart">
                    <% if (HLConfigManager.Configurations.DOConfiguration.IsChina) { %>
                        <cc1:DynamicButton ID="ClearDonation2" runat="server" Text="Clear Donation" ButtonType="Link"
                               OnClick="btnClearDonation_Click" meta:resourcekey="ClearDonationResource1" />&nbsp;&nbsp;
                    <% } %>
                        <cc1:DynamicButton ID="btnAddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                            meta:resourcekey="btnAddToCartResource1" OnClick="btnAddToCart_Click" name="hffAddToCart" />
                    </div>
                    </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>