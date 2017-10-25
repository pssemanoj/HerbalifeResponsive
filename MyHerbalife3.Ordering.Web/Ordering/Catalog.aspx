<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="Catalog.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Catalog" meta:resourcekey="PageResource1"
    EnableEventValidation="false" %>
<%@ Register Src="~/Ordering/Controls/ProductList.ascx" TagName="ProductList" TagPrefix="hrblProductList" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/MessageBoxPC.ascx" TagName="PCMsgBox" TagPrefix="PCMsgBox" %>
<%@ Register Src="~/Ordering/Controls/ExpireDatePopUp.ascx" TagPrefix="ExpirePopUp" TagName="ExpireDatePopUp" %>
<%@ Register Src="~/Ordering/Controls/APFDueReminderPopUp.ascx" TagPrefix="APFReminderPopUp" TagName="APFDuePopUp" %>
<%@ Register Src="~/Ordering/Controls/AddressRestrictionPopUp.ascx" TagPrefix="AdrsPopUp" TagName="AddressResPopUP" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
     <script type="text/javascript">
         window.onload = function () {
             toggleCatalog();
         }
    </script>
    <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ProductRecomendationsContent" runat="server">
   
    <% if (MyHerbalife3.Ordering.Configuration.ConfigurationManagement.HLConfigManager.Configurations.DOConfiguration.AddScriptsForRecommendations) { %>
    <script type="text/javascript">
        AdobeTarget = {
            "entity" : {
                "categoryId" : "<%= AT_categoryName %>"
            }
        };
        
        _etmc.push(["trackPageView", { "category" : "<%= AT_categoryName %>" }]);
    </script>
    <% } %>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:HiddenField ID="hfWarehouseCode" runat="server"></asp:HiddenField>
    <div class="row no-margins">
        <%--<tr>
            <td>
                <h2><asp:Label runat="server" ForeColor="Red" ID="lbNoEventTicketCatalog" 
                     Text="No Event Ticket Catalog!"></asp:Label></h2>
            </td>
        </tr>--%>
        <div id="divChinaPCMessageBox" runat="server">
            <PCMsgBox:PCMsgBox runat="server" ID="PcMsgBox1" DisplaySubmitButton="True"></PCMsgBox:PCMsgBox>
        </div>
        <div class="alertNoCC" id="alertNoCC" runat="server" visible="false">
            <asp:Label runat="server" ID="lblCraditcard" Text=""></asp:Label>
            <asp:Image ID="imgWarning" runat="server" Visible="false" />
            <asp:HyperLink runat="server" ID="lnkSavedCards" NavigateUrl="OrderPreferences.aspx" Text=""></asp:HyperLink>
        </div>
        <asp:UpdatePanel ID="upCategoryInfo" runat="server" UpdateMode="Conditional" class="visible-lg visible-md">
                    <ContentTemplate>
                        <div class="inline tabs-main-wrap" runat="server" id="uxCategoryFrame">
                    <div class="tabs-button-wrap col-md-12">
                                <div class="inline tab-blu">
                            <asp:Label class="header" runat="server" ID="CatLabel" meta:resourcekey="CatLabelResource1" Text="Category"></asp:Label>
                                </div>
                        <div class="inline tab-blu-bottomline">&nbsp;</div>
                        <div class="clear"></div>
                            </div>
                    <div class="tabs-bodywrap-blu col-md-12" runat="server" id="DivImage">
                                <div class="tabs-content-wrap">
                                    <h4 class="blue">
                                        <asp:Label runat="server" ID="CategoryName" meta:resourcekey="CategoryNameResource1"></asp:Label>
                                    </h4>
                                    <div class="setwidth">
                                        <p>
                                            <asp:Literal runat="server" ID="Overview" meta:resourcekey="OverviewResource1">
                                            </asp:Literal>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
        <div class="clear"></div>
                <div>
                     <cc1:ContentReader ID="PriceListMessage" runat="server" ContentPath="catalogMessage.html"
                        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                </div>
        <p class="spacer visible-md visible-lg">&nbsp;</p>
                <div class="noindex" id="catalogContent">
                    <hrblProductList:ProductList Id="SubCat" runat="server" title="Categories"></hrblProductList:ProductList>
            <p>&nbsp;</p>
            <div class="col-md-12 center">
                        <asp:Label runat="server" ID="lblCustomerSupport" Text="For order support, please call 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday.<br />
                                    Saturday 6 a.m. to 2 p.m. PST" meta:resourcekey="lblCustomerSupportResource1"></asp:Label>
                    </div>
                </div>
                <div>
                    <cc1:ContentReader ID="catalogLogos" runat="server" ContentPath="paymentLogos.html"
                        SectionName="Ordering" ValidateContent="true" UseLocal="true" />
                </div>
    </div>

    <!-- Please remove the next link, this is for HFF testing -->
	<%--<a onclick="javascript: $HFFModal.init();" href="#">HFF Modal</a>--%>

	<!-- HFF modal content -->
	<div id="modal-overlay" class="simplemodal-overlay" style="opacity: 0.5; height: 642px; width: 1263px; position: fixed; left: 0px; top: 0px; z-index: 1001; display: none;"></div>

	<div id="modal-container" class="simplemodal-container" style="position: fixed; z-index: 1002; height: 58px; width: 1000px; top: 15%; display: none;">
		<a href="#" title="Close" class="modal-close simplemodal-close">x</a>
		<div tabindex="-1" class="simplemodal-wrap" style="height: 100%; outline: 0px; width: 100%; overflow: visible;">
			<div style="" id="simplemodal-data" class="simplemodal-data">
				<div class="modal-top"></div>
				<div id="divMainContainer" class="modal-content">
					<h1 class="modal-title"></h1>
					<div class="modal-loading" style="display: none"></div>
					<div class="error-message" style="display: none"></div>
					<div class="modal-innerContent" style="display: none; padding: 10px; height: 600px;">
                            <div runat="server" id="divHFFModal" >
                                <asp:PlaceHolder ID="plHFFModal" runat="server"/>
                            </div>
					</div>
				</div>
				<div class="contact-bottom"></div>
			</div>
		</div>
	</div>
	<!-- End HFF modal content -->

     <asp:UpdatePanel ID="UpdatePanelDupeOrder" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="dupeOrderPopupExtender" runat="server" TargetControlID="DupeOrderFakeTarget"
                PopupControlID="pnldupeOrderMonth" CancelControlID="DupeOrderFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" />
            <asp:Button ID="DupeOrderFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="pnldupeOrderMonth" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblConfirmMessage" runat="server" Text="Recent Order" meta:resourcekey="lblDupeOrder"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblDupeOrderMessage" runat="server" Text="You recently placed an order, verifiying the orders status. Otherwise, please click Cancel Order in MyOrder Page."></asp:Label>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="DynamicButtonYes" runat="server" ButtonType="Forward" Text="OK" OnClick="OnDupeOrderOK" meta:resourcekey="OK" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <ExpirePopUp:ExpireDatePopUp runat="server" id="ExpireDatePopUp1" />
    <APFReminderPopUp:APFDuePopUp runat="server" id="APFDuermndrPopUp" /> 
    <AdrsPopUp:AddressResPopUP runat="server" ID="AddressResPopUP1" />
      
</asp:Content>
