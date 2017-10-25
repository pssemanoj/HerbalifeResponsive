<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomerOrderInfo.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.CustomerOrderInfo" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server">
</asp:ScriptManagerProxy>

<asp:UpdatePanel ID="upCustOrderInfo" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
            <asp:Label ID="lblErrors" runat="server" Visible="false"></asp:Label>
        	<div id="CustomerInformation">
            	<h2><asp:Label ID="lblHeader" runat="server" Text="Customer Information" 
                        meta:resourcekey="lblHeaderResource1"></asp:Label></h2>
                <hr class="shadow" />
                <div class="customerInfoLeft">
                	<div class="dataDiv">
                        <asp:Label ID="lblCustomerOrderDateDisplay" runat="server" 
                            Text="Customer Order Date" 
                            meta:resourcekey="lblCustomerOrderDateDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerOrderDate" runat="server" 
                            meta:resourcekey="lblCustomerOrderDateResource1"></asp:Label>                        
                    </div>
                    <div class="dataDiv">
                        <asp:Label ID="lblCustomerOrderNumberDisplay" runat="server" 
                            Text="Customer Request Number" 
                            meta:resourcekey="lblCustomerOrderNumberDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerOrderNumber" runat="server" 
                            meta:resourcekey="lblCustomerOrderNumberResource1"></asp:Label>
                    </div>
                    <div class="dataDiv">
                        <asp:Label ID="lblCustomerNameDisplay" runat="server" Text="Customer Name" 
                            meta:resourcekey="lblCustomerNameDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerName" runat="server" 
                            meta:resourcekey="lblCustomerNameResource1"></asp:Label>
                    </div>
                    <div class="dataDiv">
                        <asp:Label ID="lblOrderStatusDisplay" runat="server" 
                            Text="Customer Order Status" meta:resourcekey="lblOrderStatusDisplayResource1"></asp:Label>
                        <asp:Label ID="lblOrderStatus" runat="server" 
                            meta:resourcekey="lblOrderStatusResource1"></asp:Label>
                    </div>                    
                    <div class="dataDiv">
                        <asp:Label ID="lblNotesToSelf" runat="server" Text="Note to Self" 
                            meta:resourcekey="lblNotesToSelfResource1"></asp:Label>
                        <asp:DropDownList ID="ddlNotesToSelf" runat="server" AutoPostBack="True" 
                            onselectedindexchanged="ddlNotesToSelf_SelectedIndexChanged" 
                            meta:resourcekey="ddlNotesToSelfResource1"></asp:DropDownList>                        
                    </div>
                    <div class="dataDiv">
                        <asp:Label ID="lblCustomerPrefferedShippingMethodDisplay" runat="server" 
                            Text="Customer Preffered Shipping Method" meta:resourcekey="lblCustomerPrefferedShippingMethodDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerPrefferedShippingMethod" runat="server" 
                            meta:resourcekey="lblOrderStatusResource1"></asp:Label>
                    </div>
                    <div class="dataDiv">
                        <asp:Label ID="lblCustomerPaymentPreferenceDisplay" runat="server" 
                            Text="Customer Payment Preference" meta:resourcekey="lblCustomerPaymentPreferenceDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerPaymentPreference" runat="server" 
                            meta:resourcekey="lblOrderStatusResource1"></asp:Label>
                    </div>
                </div>                
                <div class="customerInfoRight">
                    <div class="dataDiv" id="dvPaymentStatus" runat="server" visible="False">
                        <asp:Label ID="lblCustomerPaymentStatusDisplay" runat="server" 
                            Text="Payment Status" 
                            meta:resourcekey="lblCustomerPaymentStatusDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerPaymentStatus" runat="server" 
                            meta:resourcekey="lblCustomerPaymentStatusResource1"></asp:Label>
                    </div>
                	<div class="dataDiv">
                        <asp:Label ID="lblCustomerCommentsDisplay" runat="server" 
                            Text="Customer Comments" meta:resourcekey="lblCustomerCommentsDisplayResource1"></asp:Label>
                        <asp:Label ID="lblCustomerComments" runat="server" 
                            meta:resourcekey="lblCustomerCommentsResource1"></asp:Label>
                    </div>
                    <div class="dataDiv blue">
                        <content:ContentReader ID="customerOrderInstructions" runat="server" ContentPath="submitDO.html" SectionName="DSAdmin" ValidateContent="true"/>
                    </div>
                </div>
                <div class="clearfix"></div>
            </div>
            </ContentTemplate>
            </asp:UpdatePanel>

