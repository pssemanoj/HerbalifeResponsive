<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TodaysMagazine.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.TodaysMagazine" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<div id="gdo-right-column-todaysmagazine">
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
    <asp:UpdatePanel runat="server" ID="todayMagazinePanel" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="divTodaysMagazine" runat="server">
                <div class="gdo-right-column-tbl gdo-tdmag-module">
                    <div class="gdo-right-column-header">
                        <h3>
                            <asp:Literal ID="ltLine1" runat="server" meta:resourcekey="ltLine1Resource1" Text="Herbalife Today Magazine"></asp:Literal></h3>
                    </div>
                    <div class="gdo-clear gdo-horiz-div">
                    </div>
                    <div class="gdo-right-column-text">
                        <asp:Label ID="lblTodaysMagazine" runat="server" Text="Your order qualifies for herbalife Today Magazine"
                            meta:resourcekey="lblTodaysMagazineResource1"></asp:Label>
                        <cc2:ContentReader ID="tmContentReader" runat="server" ContentPath="viewTodaysMagazine.html"
                            SectionName="ordering" ValidateContent="true" UseLocal="true"/>
                    </div>
                    <div class="gdo-spacer2">
                    </div>
                    <div class="gdo-right-column-text gdo-tdmag-radio">
                        <asp:RadioButton ID="rbPrimaryLanguage" runat="server" OnCheckedChanged="rbPrimaryLanguage_OnCheckedChanged"
                            meta:resourcekey="rbPrimaryLanguageResource1" GroupName="rbGroup" />
                        <br />
                        <asp:RadioButton ID="rbSecondaryLanguage" runat="server" OnCheckedChanged="rbSecondaryLanguage_OnCheckedChanged"
                            meta:resourcekey="rbSecondaryLanguageResource1" GroupName="rbGroup" />
                    </div>
                    <div class="gdo-tdmag-input">
                        <asp:TextBox ID="tbQuantity" runat="server" meta:resourcekey="tbQuantityResource1"
                            AutoPostBack="True" MaxLength="5" Width="50px" OnTextChanged="tbQuantityTextChanged"
                            Text="1"></asp:TextBox>
                    </div>
                    <div class="tmagbuttonaddcart">
                        <cc1:DynamicButton ID="btnAddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                            OnClick="btnAddToCart_Click" meta:resourcekey="btnAddToCartResource1" />
                    </div>
                    <div class="gdo-spacer2">
                    </div>
                    <div class="gdo-right-column-text-error">
                        <asp:Label ID="lblError" runat="server" meta:resourcekey="lblErrorResource1"></asp:Label></div>
                    <div class="gdo-spacer2">
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>