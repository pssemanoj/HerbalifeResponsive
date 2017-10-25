<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Menu.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Menu" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Shared.ViewModel" %>
<% if (IsChina)
    {%>
<script>
    function showModalPopUp() {
            var url = 'OnlineInvoice.aspx';

            if (typeof firstShow == 'undefined' || firstShow == null)
                GetNeverShowAgain();

            if (firstShow === false) {
                LoadModalDiv();
                $('#<%=btnOK.ClientID%>').attr('href', url);
            }
            else {
                window.location.href = url;
            }
    }
    function LoadModalDiv() {

        var bcgDiv = document.getElementById("divBackground");
        var fromtDiv = document.getElementById("divConfirm");

        bcgDiv.style.display = "block";
        fromtDiv.style.display = "block";
        if (HL.Util.Browser.isMobile.any()) {
            if ($("#divConfirm").is(":visible")) {
                $('[id$="divleft"]').css('overflow', 'inherit');
            };
        }


    }
    function GetNeverShowAgain() {

        $.ajax({
            type: "POST",
            url: "OnlineInvoice.aspx/GetNeverShowAgainSession",
            data: "{}",
            async: false,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: OnSuccessGet,
            failure: function (response) {
                alert(response.d);
            }
        });
    }
    var firstShow;
    function OnSuccessGet(response) {
        firstShow = response.d;
    }
    $(document).ready(function () {

        $('#<%=btnOK.ClientID%>').css('display', 'none');
        $('#<%=btnOKDisabled.ClientID%>').css('display', 'inline-block');
        $('#<%=btnOKDisabled.ClientID%>').attr('class', 'forward disabled');
        $('#<%=btnOKDisabled.ClientID%>').attr('onclick', 'return false;');
        $('#<%=btnCancel.ClientID%>').attr('onclick', 'return false;');
        $('#<%=btnCancelDisabled.ClientID%>').css('display', 'none');
        $("#<%=chkConfirmRead.ClientID%>").change(function () {

            if ($(this).is(":checked"))
            {
                $('#<%=btnOK.ClientID%>').css('display', 'inline-block');
                $('#<%=btnOKDisabled.ClientID%>').css('display', 'none');
            }
            else{

                $('#<%=btnOK.ClientID%>').css('display', 'none');
                $('#<%=btnOKDisabled.ClientID%>').css('display', 'inline-block');
            };

        });

        $('#<%=btnOK.ClientID%>').click(function () {

            $('#<%=btnOK.ClientID%>').css('display', 'none');
            $('#<%=btnOKDisabled.ClientID%>').css('display', 'inline-block');
            
            $('#<%=btnCancelDisabled.ClientID%>').attr('class', 'backward disabled');
            $('#<%=btnCancel.ClientID%>').css('display', 'none');
            $('#<%=btnCancelDisabled.ClientID%>').css({ 'display': 'inline-block', 'float': 'left' });
            if (HL.Util.Browser.isMobile.any()) {
                if ($("#divConfirm").is(":visible")) {
                    $('[id$="divleft"]').css('overflow', 'scroll');
                };
            }


                if ($("#<%=chkDontShowAgain.ClientID%>").is(":checked")) {
                    setNeverShowAgain(false);
                } else {
                    setNeverShowAgain(true);
                };

        });

        $("#<%= btnCancel.ClientID%>").click(function () {
            
            var bcgDiv = document.getElementById("divBackground");
            var fromtDiv = document.getElementById("divConfirm");

            if (HL.Util.Browser.isMobile.any()) {
                if ($("#divConfirm").is(":visible")) {
                    $('[id$="divleft"]').css('overflow', 'scroll');
                };
            }


            bcgDiv.style.display = "none";
            fromtDiv.style.display = "none";


            $('#<%=chkConfirmRead.ClientID%>').attr('checked', false);
            $('#<%=chkDontShowAgain.ClientID%>').attr('checked', false);
            $('#<%=btnOK.ClientID%>').css('display', 'none');
            $('#<%=btnOKDisabled.ClientID%>').css('display', 'inline-block');

        });

        function setNeverShowAgain(bolRemoveSession) {

            $.ajax({
                type: "POST",
                url: "OnlineInvoice.aspx/SetNeverShowAgain",
                data: "{'bolRemoveSession':" + bolRemoveSession + "}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function () { firstShow = null },
                failure: function (response) {
                    alert(response.d);
                }
            });
        }


    });

</script>
<% } %>
<% if (IsChina)
    {%>

<div id = "divBackground" class="InvoiceBackground" ></div>
<div id="divConfirm" class="InvoiceConfirm gdo-popup" style="max-height:750px;">
    <h1><asp:Label ID="lblInvoiceOrderTitle" runat="server" Font-Size="XX-Large" Font-Bold="true" meta:resourcekey="OnlineInvoiceHerbalifeLabel"></asp:Label></h1>
    <ol>
        <li>网络订单可通过此开票界面进行发票开具</li>
        <li>系统可开具三个月内的订单</li>
        <li>发票开具后，公司会以挂号信的方式进行寄送。 寄送时效为七个工作日左右</li>
        <li>发票内容请仔细填写，确认无误后进行提交。发票一旦提交无法修改。</li>
        <li>开票中，如有任何问题请致电400客服热线。</li>
        <li>按国家税务要求，各寄送区域发票内容及抬头要求参考如下：</li>
    </ol>
    <table border="1" style="overflow-y: scroll">
        <thead>
            <tr>
                <th>产品寄送区域</th>
                <th style="width: 100px">开票内容</th>
                <th>发票抬头</th>
                <th>单张订单拆分发票最大张数</th>
            </tr>
        </thead>
        <tr>
            <td>江苏/广东/江西/湖北/安徽/广西/海南/ 北京</td>
            <td>康宝莱产品及配送费<br />
                康宝莱产品
            </td>
            <td>1. 订货人姓名
                <br />
                2. 公司抬头
            </td>
            <td>3张</td>
        </tr>
        <tr>
            <td>山东/内蒙古/天津/河南/陕西/河北/黑龙江/山西/新疆/辽宁/吉林/宁夏/甘肃</td>
            <td>康宝莱产品及配送费<br />
                康宝莱产品
            </td>
            <td>1. 订货人姓名<br />
                2.公司抬头</td>
            <td>3张</td>
        </tr>
        <tr>
            <td>四川/云南/重庆/贵州/福建/湖南/浙江/上海</td>
            <td>康宝莱产品及配送费</td>
            <td>1. 个人<br />
                2.公司抬头</td>
            <td>3张</td>
        </tr>
    </table>
    <div>
        <asp:CheckBox ID="chkConfirmRead" meta:resourcekey="ConfirmRead" runat="server" />
    </div>
    <div>
        <asp:CheckBox ID="chkDontShowAgain" meta:resourcekey="DoNotShow" runat="server" />
    </div>
    <div>
        <cc2:DynamicButton ID="btnCancel" runat="server" ButtonType="Back" meta:resourcekey="ButtonCancel" />
        <cc2:DynamicButton ID="btnOK" runat="server" ButtonType="Forward" meta:resourcekey="ButtonOk" />
        <cc2:DynamicButton ID="btnOKDisabled" runat="server" ButtonType="Forward disabled" meta:resourcekey="ButtonOk" />
        <cc2:DynamicButton ID="btnCancelDisabled" runat="server" ButtonType="Back" meta:resourcekey="ButtonCancel" />
    </div>
</div>
 <% } %>
<progress:UpdatePanelProgressIndicator ID="progressUpdatePanelMenu" runat="server"
    TargetControlID="UpdatePanelMenu" />
<asp:UpdatePanel ID="UpdatePanelMenu" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Label runat="server" ID="errDSFraud" CssClass="red" Visible="False" />

        <% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile() && ProductsBase.GlobalContext.CurrentExperience.ExperienceType == MyHerbalife3.Shared.ViewModel.ValueObjects.ExperienceType.Green)
           { %>
        <a class="menu-responsive-toggle" href="javascript:;">
            <i class="icon icon-list-fl-1"></i>
            <%= GetLocalResourceObject("quicklinks") as string %>
            <i class="icon icon-arrow-down-2 right"></i>
        </a>
        <% } %>

        <ul id="ProductMenu" class="gdo-nav-div leftMenuNew">
            <%if (HLConfigManager.Configurations.DOConfiguration.IsDemoVideo)
                { %>
            <li class="gdo-left-nav-level0" id="liMenuOnlineDemoVideo" runat="server">
                <asp:HyperLink runat="server" ID="MenuOnlineDemoVideo" Text="Cómo comprar"
                    NavigateUrl="~/Ordering/DemoVideo.aspx"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <%} %>
            <li class="gdo-left-nav-level0" id="liMenuOnlinePriceList" runat="server">
                <asp:HyperLink runat="server" ID="MenuOnlinePriceList" Text='<%# getMenuText("MenuOnlinePriceList") %>'
                    NavigateUrl='<%# getMenuLink("~/Ordering/PriceList.aspx") %>'></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <li class="gdo-left-nav-level0" id="liMenuOrderBySKU" runat="server">
                <asp:HyperLink runat="server" ID="MenuOrderBySKU" Text='<%# getMenuText("MenuOrderBySKU") %>'
                    NavigateUrl='<%# getMenuLink("~/Ordering/ProductSKU.aspx") %>'></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <li class="gdo-left-nav-level0" id="liMenuSearchProduct" runat="server">
                <asp:HyperLink runat="server" ID="MenuSearchProduct" Text="Search Product" NavigateUrl="~/Ordering/SearchProducts.aspx"
                    meta:resourcekey="MenuSearchProductResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>

            <li id="liMenuCatalog" runat="server" class="gdo-left-nav-level0 grandpa catalog" ck="showcat">
                <asp:HyperLink runat="server" ID="MenuProductCatalog" Text='<%# getMenuText("MenuProductCatalog") %>'
                    NavigateUrl='<%# getMenuLink("~/Ordering/Catalog.aspx") %>'></asp:HyperLink>
                <i class="arrow icon icon-add-ln-1 hide"></i>
            </li>
            <div id="toggleMe" runat="server" class="toggle-child grandson">
                <asp:Repeater runat="server" ID="CatetoryMenu" OnItemDataBound="topLeveCategoryDataBound">
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="gdo-left-nav-level2 toggle-parent">
                            <asp:HyperLink runat="server" ID="topLevelCatetory" meta:resourcekey="topLevelCatetoryResource1"></asp:HyperLink>
                            <i class="arrow icon icon-add-ln-1" id="iconArrow" runat="server"></i>
                        </li>
                        <asp:PlaceHolder runat="server" ID="uxSubCategory"></asp:PlaceHolder>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <li class="gdo-left-nav-level0" id="liMenuEventTickets" runat="server">
                <asp:HyperLink runat="server" ID="MenuEventTickets" Text="Order Event Tickets" NavigateUrl="~/Ordering/Catalog.aspx?ETO=TRUE"
                    meta:resourcekey="MenuEventTicketsResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            
            
            <li class="gdo-left-nav-level0 grandpa apparel" id="liMenuApparelAndAccessories" runat="server" ck="showapp">
                <asp:HyperLink runat="server" ID="MenuApparelAndAccessories" Text="Apparel And Accessories"
                    meta:resourcekey="MenuApparelAccessoriesResource1"></asp:HyperLink>
                <i class="arrow icon icon-add-ln-1 hide"></i>
            </li>
            <div id="toggleMeApparel" runat="server" class="toggle-child grandson">
                <asp:Repeater runat="server" ID="ApparelMenu" OnItemDataBound="topLeveApparelDataBound">
                    <HeaderTemplate>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:PlaceHolder runat="server" ID="apparelCategoryPlaveHolder"></asp:PlaceHolder>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <li class="gdo-left-nav-level0" id="liMenuOrderProducts" runat="server">
                <asp:HyperLink runat="server" ID="MenuOrderProducts" Text="Order Products" NavigateUrl="~/Ordering/Catalog.aspx?ETO=FALSE"
                    meta:resourcekey="MenuOrderProductsResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <li class="gdo-left-nav-level0" id="liMenuPendingOrders" runat="server">
                <asp:HyperLink runat="server" ID="MenuPendingOrders" Text="Pending Orders" NavigateUrl="~/Ordering/PendingOrders.aspx"
                    meta:resourcekey="MenuPendingOrdersResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <li class="gdo-left-nav-level0" id="liMenuShoppingCart" runat="server">
                <asp:LinkButton runat="server" ID="MenuShoppingCart" Text="Shopping Cart" OnClick="ShoppingCartLink_Click"
                    meta:resourcekey="MenuShoppingCartResource1"></asp:LinkButton>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <li class="gdo-left-nav-level0" id="liMenuPreference" runat="server">
                <asp:HyperLink runat="server" ID="MenuPreference" Text="Saved Address & Payment"
                    NavigateUrl="~/Ordering/OrderPreferences.aspx" meta:resourcekey="MenuPreferenceResource1"></asp:HyperLink>

                <li class="gdo-left-nav-divider"></li>
            </li>

            <asp:Panel ID="PanelOrderPreferenceSubMenu" runat="server" CssClass="gdo-left-nav-level2">
                <li class="gdo-left-nav-level2" id="liMenuSavedShippingAddress" runat="server">
                    <asp:HyperLink runat="server" ID="MenuSavedShippingAddress" Text="Saved Shipping Address"
                        NavigateUrl="~/Ordering/SavedShippingAddress.aspx" meta:resourcekey="MenuSavedShippingAddressResource1"></asp:HyperLink>
                </li>
                <li class="gdo-left-nav-level2" id="liMenuSavedPickupLocation" runat="server">
                    <asp:HyperLink runat="server" ID="MenuSavedPickupLocation" Text="Saved Pickup Location"
                        NavigateUrl="~/Ordering/SavedPickupLocation.aspx" meta:resourcekey="MenuSavedPickupLocationResource1"></asp:HyperLink>
                </li>
                <li class="gdo-left-nav-level2" id="liMenuSavedPUFromCourierLocation" runat="server">
                    <asp:HyperLink runat="server" ID="MenuSavedPUFromCourierLocation" Text="Saved Pickup From Courier Location"
                        NavigateUrl="~/Ordering/SavedPickupCourierLocation.aspx" meta:resourcekey="MenuSavedPUFromCourierLocationResource1"></asp:HyperLink>
                </li>
                <li class="gdo-left-nav-level2" id="liMenuSavedPaymentInformation" runat="server">
                    <asp:HyperLink runat="server" ID="MenuSavedPaymentInformation" Text="Saved Credit Cards"
                        NavigateUrl="~/Ordering/SavedPaymentInformation.aspx" meta:resourcekey="MenuSavePaymentInformationResource1"></asp:HyperLink>
                </li>
            </asp:Panel>
            <li class="gdo-left-nav-level0" id="liMenuOrderListView" runat="server">
                <asp:HyperLink runat="server" ID="MenuOrderListView" Text="My Orders" NavigateUrl="~/Ordering/OrderListView.aspx"
                    meta:resourcekey="MenuOrderListViewResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <% if(HLConfigManager.Configurations.DOConfiguration.IsChina) { %>
            <li class="gdo-left-nav-level0" id="liMenuMyFavourite" runat="server">
                <asp:HyperLink runat="server" ID="MyFavorite" Text="My Favorites" NavigateUrl="~/Ordering/PriceListFavourite.aspx"
                    meta:resourcekey="MenuMyFavouriteResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <% } %>
            <li class="gdo-left-nav-level0" id="liMenuOrderHistoryUrl" runat="server">
                <asp:HyperLink runat="server" ID="MenuOrderHistoryUrl" Text="Order History" NavigateUrl="~/Account/MyOrders.aspx"
                    meta:resourcekey="MenuOrderHistoryUrl"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>

            <li class="gdo-left-nav-level0" id="liMenuSavedCarts" runat="server">
                <asp:HyperLink runat="server" ID="MenuSavedCarts" Text="Saved Drafts"
                    NavigateUrl="~/Ordering/SavedCarts.aspx" meta:resourcekey="MenuSavedCartsResource1"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>

            <li class="gdo-left-nav-level0" id="liMenuHFFUrl" runat="server">
                <asp:HyperLink runat="server" ID="MenuHFFUrl" Text="About Casa Herbalife" NavigateUrl="http://www.herbalifefamilyfoundation.org/?nd=donate_summary"
                    meta:resourcekey="MenuHFFUrlResource1" Target="_blank"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>

            <li class="gdo-left-nav-level0" id="liMenuFreightSimulation" runat="server">
                <asp:HyperLink runat="server" ID="MenuFreightSimulation" Text="Freight Simulation"
                    NavigateUrl="~/Ordering/FreightSimulation.aspx" meta:resourcekey="MenuFreightSimulationResource"></asp:HyperLink>
                <li class="gdo-left-nav-divider"></li>
            </li>

            <li class="gdo-left-nav-level0" id="liMenuFAQ" runat="server">
                <asp:HyperLink runat="server" ID="MenuFAQ" Text="FAQ" Target="_blank" meta:resourcekey="MenuFAQResource1"></asp:HyperLink>
                <cc1:ContentReader ID="crAdditionalItems" runat="server" ContentPath="AdditionalItems.html"
                    SectionName="Products" ValidateContent="true" UseLocal="true" />
                <li class="gdo-left-nav-divider"></li>
            </li>
            <% bool OnlineInvoiceEnable  = HL.Common.Configuration.Settings.GetRequiredAppSetting("OnlineInvoiceEnable",false); %>
            <% if (IsChina && OnlineInvoiceEnable )
               { %>
            <li class="gdo-left-nav-level0" id="li1" runat="server">
                <asp:Label runat="server" ID="HyperLink1" Text="Online Order" onclick="showModalPopUp();"
                     meta:resourcekey="MenuOnlineInvoiceResource1"></asp:Label>
                <li class="gdo-left-nav-divider"></li>
            </li>
            <% } %>
        </ul>

    </ContentTemplate>
</asp:UpdatePanel>
