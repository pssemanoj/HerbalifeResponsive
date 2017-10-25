<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductList.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductList" %>
<%@ Register TagPrefix="gv" Namespace="MyHerbalife3.Ordering.Web.Ordering.Controls" Assembly="MyHerbalife3.Ordering.Web" %>
<%@ Register Src="~/Controls/Content/UpdatePanelProgressIndicator.ascx" TagName="UpdatePanelProgressIndicator" TagPrefix="progress" %>

<script type="text/javascript">
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        try {
            var showall1 = document.getElementById('ctl00_ctl00_ContentArea_ProductsContent_SubCat_DataPager1_ctl00_ShowAll');
            if (showall1 != null && showall1.getAttribute("ShowAllDisabled") != null) {
                showall1.removeAttribute('href');
                showall1.style.color = "gray";
                showall1.style.textDecoration = 'none';
            }
            showall1 = document.getElementById('ctl00_ctl00_ContentArea_ProductsContent_SubCat_DataPager2_ctl00_ShowAll');
            if (showall1 != null && showall1.getAttribute("ShowAllDisabled") != null) {
                showall1.removeAttribute('href');
                showall1.style.color = "gray";
                showall1.style.textDecoration = 'none';
            }
        }
        catch (e) {
        }
    }
    </script>  
<div class="col-md-12">
    <progress:UpdatePanelProgressIndicator ID="progressProductList" runat="server" TargetControlID="upProductList" />
    <asp:UpdatePanel ID="upProductList" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-catalog">
                <div class="gdo-pagination row">
                    <table class="gdo-main" cellspacing="0" cellpadding="0">
	                    <tbody>
		                    <tr>
			                    <td valign="top">
				                    <p class="gdo-product-cat-titles">
					                    <asp:Literal runat="server" ID="uxSubTablaHeader" />
				                    </p>
			                    </td>
			                    <td>
				                    &nbsp;
			                    </td>
			                    <td class="gdo-product-td right">
				                    <p class="gdo-pagination gdo-no-border">
					                    <gv:CategoryDataPager PagedControlID="Products" ID="DataPager1" runat="server" PageSize="10"
						                    QueryStringField="ispage">
						                    <Fields>
							                    <asp:TemplatePagerField OnPagerCommand="TemplatePagerField_OnPagerCommand">
								                    <PagerTemplate>
									                    <asp:LinkButton runat="server" ID="ShowAll" CommandName="ShowAll" meta:resourcekey="ShowAll">Show All</asp:LinkButton>
									                    <asp:Label runat="server" ID="lblPage" Text="Page" meta:resourcekey="Page"></asp:Label>
									                    <asp:Label runat="server" ID="lblNumPage" Text="1" />
									                    <asp:Label runat="server" ID="lbOF" Text="of" meta:resourcekey="Of" />
									                    <asp:Label runat="server" ID="TotalPagesLabel" />
									                    <asp:Label runat="server" ID="TotalItemsLabel" />
								                    </PagerTemplate>
							                    </asp:TemplatePagerField>
							                    <asp:NextPreviousPagerField ButtonType="Link" FirstPageText="|<" ShowFirstPageButton="True"
								                    ShowNextPageButton="False" ShowPreviousPageButton="False" meta:resourcekey="NextPreviousPager" />
							                    <gv:CategoryNumericPagerField meta:resourcekey="CategoryNumericPager" />
							                    <asp:NextPreviousPagerField ButtonType="Link" LastPageText=">|" ShowLastPageButton="True"
								                    ShowNextPageButton="False" ShowPreviousPageButton="False" meta:resourcekey="NextPerviousPager" />
						                    </Fields>
					                    </gv:CategoryDataPager>
				                    </p>
			                    </td>
		                    </tr>
	                    </tbody>
                    </table>
                </div>
                <div class="gdo-products_wrap" style="width: 100%">
                    <p class="gdoAlert">
                        <asp:Literal runat="server" ID="specialMessageText"  meta:resourcekey="SpecialMessageText"/>
                    </p>
                </div>
                <div class="gdo-products_wrap" style="width: 100%">
                    <div class="rowspace">&nbsp;</div>
                    <asp:ListView ID="Products" runat="server" GroupItemCount="3" OnPagePropertiesChanging="OnPagePropertiesChanging_Click"
                        >
                        <LayoutTemplate>
                            <div id="groupPlaceholderContainer" runat="server">
			                    <div id="groupPlaceholder" runat="server" ></div>
		                    </div>
                        </LayoutTemplate>
                        <GroupTemplate>
                            <div id="itemPlaceholderContainer" runat="server">
			                    <div id="itemPlaceholder" runat="server" class="row"></div>
		                    </div>
                        </GroupTemplate>
                        <EmptyDataTemplate>
                            <div class="div1" onmouseover="className='div1-over';" onmouseout="className='div1';">
                                  <asp:Localize ID="Localize1" runat="server"  meta:resourcekey="NoDataReturned">No data was returned.</asp:Localize>
                            </div>
                        </EmptyDataTemplate>
                        <ItemTemplate>
                            <div class="gdo-product-td col-lg-4 col-md-4 col-xs-6" runat="server" id="tdProduct">
			                    <div <%# GetSpecialMessage() %> onmouseout="className='gdo-ProdFrame';" onmouseover="className='gdo-ProdFrame-over';"class="gdo-ProdFrame">
                                    <div class="product-image">
				                        <a href='ProductDetail.aspx?ProdInfoID=<%# Eval("ProductID") %>&amp;CategoryID=<%# Eval("CategoryID") %>'>
                                            <img id="ProdImg" class="gdo-catalog_img" alt="Product Image" src='<%# Eval("ThumbnailImageName") %>'/>
				                        </a>
                                    </div>
				                    <asp:HiddenField ID="CatID" runat="server" Value='<%# Eval("CategoryID") %>'></asp:HiddenField>
				                    <asp:HiddenField ID="ProductID" runat="server" Value='<%# Eval("ProductID") %>'></asp:HiddenField>
                                    <div class="product-details">
				                        <a href='ProductDetail.aspx?ProdInfoID=<%# Eval("ProductID") %>&amp;CategoryID=<%# Eval("CategoryID") %>'>
                                            <div class="product-description">
                                                <asp:Literal ID="ProdName" runat="server" Text='<%# Eval("Description") %>'></asp:Literal>
                                            </div>
                                            <div class="product-price">
					                            <asp:Literal ID="lPrice" runat="server" Text='<%# Eval("Price") %>'></asp:Literal>
                                            </div>
				                        </a>
                                    </div>
				                    <asp:Label ID="lbOutofstock" runat="server" Text="Out of Stock" Visible="False" meta:resourcekey="OutOfStock"></asp:Label>
			                    </div>
		                    </div>
                        </ItemTemplate>
                    </asp:ListView>
                </div>
                <div style="clear: both;">
                </div>
                <div class="gdo-pagination">
                    <table class="gdo-main" cellspacing="0" cellpadding="0">
                        <tbody>
                            <tr>
                                <td valign="top">
                                    <p class="gdo-product-cat-titles">
                                        <asp:Literal runat="server" ID="uxSubTablaFooter" />
                                    </p>
                                </td>
                                <td>
                                    &nbsp;
                                </td>
                                <td class="gdo-product-td right">
                                    <p class="gdo-pagination gdo-no-border">
                                        <gv:CategoryDataPager PagedControlID="Products" ID="DataPager2" runat="server" PageSize="10"
                                            QueryStringField="ispage">
                                            <Fields>
                                                <asp:TemplatePagerField OnPagerCommand="TemplatePagerField_OnPagerCommand">
                                                    <PagerTemplate>
                                                        <asp:LinkButton runat="server" ID="ShowAll" CommandName="ShowAll" meta:resourcekey="ShowAll">Show All</asp:LinkButton>
                                                        <asp:Label runat="server" ID="lblPage2" Text="Page" meta:resourcekey="Page"></asp:Label>
                                                        <asp:Label runat="server" ID="lblNumPage" Text="1" />
                                                        <asp:Label runat="server" ID="lbOF" Text="of" meta:resourcekey="Of" />
                                                        <asp:Label runat="server" ID="TotalPagesLabel" />
                                                        <asp:Label runat="server" ID="TotalItemsLabel" />
                                                    </PagerTemplate>
                                                </asp:TemplatePagerField>
                                                <asp:NextPreviousPagerField ButtonType="Link" FirstPageText="|<" ShowFirstPageButton="True"
                                                    ShowNextPageButton="False" ShowPreviousPageButton="False" meta:resourcekey="NextPreviousPager" />
                                                <gv:CategoryNumericPagerField meta:resourcekey="CategoryNumericPager" />
                                                <asp:NextPreviousPagerField ButtonType="Link" LastPageText=">|" ShowLastPageButton="True"
                                                    ShowNextPageButton="False" ShowPreviousPageButton="False" meta:resourcekey="NextPreviousPager" />
                                            </Fields>
                                        </gv:CategoryDataPager>
                                    </p>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>