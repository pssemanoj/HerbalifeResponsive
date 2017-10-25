<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HFF_CN.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HFF_CN" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %><%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>

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
        function checkDec(e) {
            var ex = /^[0-9.]+$/
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
        }

        function checName(e) {
            //var exs = /^[A-Za-z ]+$/;
            var ex = /^[A-Za-z \u4E00-\u9FFF\u3400-\u4DFF\uF900-\uFAFF]+$/;
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
        }

        function allLetter(e) {
            alert(e.value);
            var x = e.value;
            return /^[A-z ]+$/.test(x);
        }
        //$(document).ready(function () {
        //    var cookieLocal = $.cookie("RENDERING_LOCALE");
        //    if (cookieLocal == "zh_CN") {
        //        $("div[id$='divHFF'] .gdo-hff-input").css('background', "url('/Ordering/Images/China/HFF-CN-logo.png') no-repeat");
        //    }
        //});
        function Check(val) {
            if (val.indexOf("btn5Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = true;
            }
            if (val.indexOf("btn10Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnOtherAmount") > -1)
                document.getElementById('<%= txtOtherAmount.ClientID %>').disabled = false;

            if (val.indexOf("btnBehalf5Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount2.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnBehalf10Rmb") > -1) {
                document.getElementById('<%= txtOtherAmount2.ClientID %>').value = '';
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = true;
            }
            if (val.indexOf("btnBehalfOther") > -1)
                document.getElementById('<%= txtOtherAmount2.ClientID %>').disabled = false;

        }
        function checNumber(e) {
            var ex = /^[0-9]+$/;
            if (ex.test(e.value) == false) {
                e.value = e.value.substring(0, e.value.length - 1);
            }
        }

    </script>
    <asp:UpdatePanel runat="server" ID="hffPanel" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="divLabelErrors" runat="server">
                <table>
                    <tr>
                        <td>
                            <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"
                                meta:resourcekey="blErrorsResource1" CssClass="error-list">
                            </asp:BulletedList>
                        </td>
                    </tr>
                </table>
            </div>

            <div class="gdo-right-column-tbl gdo-hff-module" runat="server" id="divHFF">
                <div class="gdo-right-column-header">
                    <h3>
                        <asp:Literal ID="ltLine1" runat="server" meta:resourcekey="ltLine1Resource1" Text="HFF Donation"></asp:Literal>
                    </h3>
                </div>
                <div class="gdo-clear gdo-horiz-div"></div>


                <div class="gdo-right-column-text" id="divMsg" runat="server">
                    <asp:Label ID="lblHFFMsg" runat="server" ForeColor="Blue" meta:resourcekey="lblHFFMsgResource1"></asp:Label>
                    <div class="gdo-spacer2">
                    </div>
                </div>
                <div class="gdo-right-column-text">
                    <asp:Label ID="lblHFF" runat="server" Text="Do you wish to add to this order an optional donation to the Herbalife's Family Foundation project?"
                        AutoPostBack="True" meta:resourcekey="lblHFFResource1" />
                </div>
                <div class="donation-buttons align-right">
                    <asp:Button Text="本人捐赠" BorderStyle="None" ID="btnSelf" class=" backward" runat="server"
                        OnClick="btnSelf_Click" meta:resourcekey="btnSelfResource1" />
                    <asp:Button Text="代顾客捐赠" BorderStyle="None" ID="btnBehalfof" class=" backward" runat="server"
                        OnClick="btnBehalfof_Click" meta:resourcekey="btnBehalfofResource1" />
                </div>
                <div>
                    <asp:HyperLink ID="hlHFF" runat="server" Target="_blank" meta:resourcekey="hlHFFResource1"
                        Text="About Casa Herbalife"></asp:HyperLink>
                    <% if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
                       { %>
                    <cc1:DynamicButton ID="ClearDonation" runat="server" Text="Clear Donation" ButtonType="Link"
                        OnClick="btnClearSelfDonation_Click" meta:resourcekey="ClearDonationResource1" IconPosition="Left" NavigateUrlToNewWindow="False" />&nbsp;&nbsp;
                    <% } %>
                    <asp:Label ID="lblHFFCurrencyType" runat="server" meta:resourcekey="lblHFFCurrencyTypeResource1" CssClass="right"></asp:Label>
                    <asp:MultiView ID="MainView" runat="server" ActiveViewIndex="0">
                        <asp:View ID="selfDonationView" runat="server">
                            <table class="gdo-order-details-tbl" style="width: 100%" border="0">
                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btn5Rmb" runat="server" Text="5RMB" GroupName="SelectOne" onclick="Check(this.id)" meta:resourcekey="btn5RmbResource1" />
                                    </td>
                                    <%-- </tr>
                                <tr>--%>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btn10Rmb" runat="server" Text="10RMB" GroupName="SelectOne" onclick="Check(this.id)" meta:resourcekey="btn10RmbResource1" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btnOtherAmount" runat="server" Text="OtherAmount" onclick="Check(this.id)" GroupName="SelectOne" meta:resourcekey="btnOtherAmount" />
                                    </td>
                                    <td class="gdo-details-label">
                                        <asp:TextBox ID="txtOtherAmount" runat="server" MaxLength="6" onkeyup="checkDec(this);" CssClass="donate-other-amnt" meta:resourcekey="txtOtherAmountResource1"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <div class="donation-buttons align-right">
                                        <% if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                                           { %>
                                        <cc1:DynamicButton ID="ClearDonation2" runat="server" Text="Clear Donation" ButtonType="Link"
                                            OnClick="btnClearSelfDonation_Click" Visible="False" meta:resourcekey="ClearDonationResource1" IconPosition="Left" NavigateUrlToNewWindow="False" CssClass="backward" />
                                        <% } %>
                                        <cc1:DynamicButton ID="btnAddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                                            meta:resourcekey="btnAddToCartResource1" OnClick="btnAddToCart_Click" name="hffAddToCart" IconPosition="Left" NavigateUrlToNewWindow="False" />
                                        <%-- </div>--%>
                                    </td>
                                </tr>
                            </table>
                        </asp:View>
                        <asp:View ID="behalfOfDonationView" runat="server">
                            <table class="gdo-order-details-tbl" style="width: 100%" border="0">

                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btnBehalf5Rmb" runat="server" Text="5RMB" GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btn5RmbResource1" />
                                    </td>
                                    <%-- </tr>
                                <tr>--%>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btnBehalf10Rmb" runat="server" Text="10RMB" GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btn10RmbResource1" />
                                    </td>
                                </tr>
                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:RadioButton ID="btnBehalfOther" runat="server" Text="OtherAmount" GroupName="SelectTwo" onclick="Check(this.id)" meta:resourcekey="btnOtherAmount" />
                                    </td>
                                    <td class="gdo-details-label">
                                        <asp:TextBox ID="txtOtherAmount2" runat="server" MaxLength="6" CssClass="donate-other-amnt" onkeyup="checkDec(this);" meta:resourcekey="btnOtherAmount"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:Label ID="lblDonatorName" runat="server" Text="顾客姓名" meta:resourcekey="lblDonatorName"></asp:Label></td>
                                    <td class="gdo-details-label">
                                        <asp:TextBox ID="txtDonatorName" runat="server" onkeyup="checName(this);" MaxLength="25" CssClass="donator-data"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="gdo-details-label">
                                        <asp:Label ID="lblContactNumber" runat="server" Text="手机号码" meta:resourcekey="lblContactNumber"></asp:Label>
                                    </td>
                                    <td class="gdo-details-label">
                                        <asp:TextBox runat="server" ID="txtContactNumber" onkeyup="checNumber(this);" CssClass="inputPickupPhone donator-data" MaxLength="11"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <div class="donation-buttons align-right">
                                            <% if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                                               { %>
                                            <cc1:DynamicButton ID="ClearBehalfDonation2" runat="server" Text="Clear Donation" ButtonType="Link"
                                                OnClick="btnClearBehalfDonation_Click" Visible="False" meta:resourcekey="ClearDonationResource1" IconPosition="Left" NavigateUrlToNewWindow="False" CssClass="backward" />
                                            <% } %>
                                        
                                            <cc1:DynamicButton ID="btnAddToCart2" runat="server" ButtonType="Forward" Text="Add to Cart"
                                            meta:resourcekey="btnAddToCartResource1" OnClick="btnAddToCart2_Click" name="hffAddToCart" IconPosition="Left" NavigateUrlToNewWindow="False" />
                                        </div>
                                    </td>

                                </tr>
                            </table>
                        </asp:View>
                    </asp:MultiView>
                    <div class="gdo-spacer2">
                    </div>

                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
