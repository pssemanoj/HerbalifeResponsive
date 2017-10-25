<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HFFModal.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HFFModal" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<telerik:RadCodeBlock ID="RCScript" runat="server">
    <script type="text/javascript">
        var canSubmit = "<%= CanSubmitNow %>";
        var pnlHFF = "<%= pnlHFF.ClientID %>";
        var tbQuantity = "<%= tbQuantity.ClientID %>";
        var divHFFDonation_Step1 = "<%= divHFFDonation_Step1.ClientID %>";
        var divHFFDonation_Step2 = "<%= divHFFDonation_Step2.ClientID %>";
        var divOrderComplete = "<%= divOrderComplete.ClientID %>";
        var divEmailNotification = "<%= divEmailNotification.ClientID %>";
        var divPayment = "<%= divPayment.ClientID %>";
        var divSubmitCommand = "<%= divSubmitCommand.ClientID %>";
        var divEndCommand = "<%= divEndCommand.ClientID %>";
    </script>
    <script type="text/javascript" src="/Ordering/Scripts/HFFModal.js"></script>
        <script type="text/javascript" language="javascript">
            function ConfPrint(ctrl) {
                if (ctrl != null) {
                    window.print();
                    ctrl.removeAttribute("href");
                }
            }
    </script>
</telerik:RadCodeBlock>

<asp:Panel ID="pnlHFF" runat="server">
    <asp:UpdatePanel ID="updHFF" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="divHFFDonation_Step1" class="gdo-popup" runat="server">
                <h2><asp:Label runat="server" ID="lbTitle" Text="Success" meta:resourcekey="lbTitle"></asp:Label></h2>
                <asp:Label runat="server" ID="lbOrderDone" Text="Your order has been processed" meta:resourcekey="lbOrderDone"></asp:Label>
	            <asp:Label runat="server" ID="lbToDonate" Text="Would you like to make a donation to the Herbalife Family Foundation?" meta:resourcekey="lbToDonate"></asp:Label>
                <asp:Image ID="imgHFF" runat="server" ImageAlign="Middle" ImageUrl="/Content/Global/Assets/EMEA/Shared_images/Generic_images/uc10_img3.gif" />
                <asp:HyperLink ID="lnkAbout" runat="server" Visible="false" Text="about Herbalife Family Fundation" meta:resourcekey="lnkAbout"></asp:HyperLink>
                <asp:Label runat="server" ID="lbQty" Text="Quantity" meta:resourcekey="lbQty"></asp:Label>
	            <a id="substractQuantity" onclick="SubsOne()">-</a>
	            <input type="text" value="0" id="txtHFFQuantity" class="hide"/>
                <asp:TextBox ID="tbQuantity" runat="server" meta:resourcekey="tbQuantity" MaxLength="5" Width="42px" Text="1"></asp:TextBox>
	            <a id="addQuantity" onclick="AddOne()">+</a>
                <asp:Label runat="server" ID="lbUnit" Text="1 unit of HFF = €1" meta:resourcekey="lbUnit"></asp:Label>
                <cc1:DynamicButton runat="server" ID="btnNo" ButtonType="Neutral"
                                    OnClick="OnCancel" meta:resourcekey="btnNo" Text="No Thanks" />
                <cc1:DynamicButton runat="server" ID="btnMake" ButtonType="Forward"
                                    OnClick="btnMake_Click" meta:resourcekey="btnMake" Text="Make donation" />
            </div>
            <div id="divHFFDonation_Step2" class="gdo-popup hide" runat="server">
                <div id="divHeader">
                    <asp:Image ID="imgHFFHeader" runat="server" ImageAlign="Middle" ImageUrl="/Content/Global/Assets/EMEA/Shared_images/Generic_images/uc10_img3.gif" />
                    <asp:Label ID="lbHeader" runat="server" Text="The Herbalife Family Fundation appeciates your donation" meta:resourcekey="lbHeader"></asp:Label>
                    <div style="background-color: #b0b0b0;height: 1px;"></div>
                </div>
                <div id="divOrderComplete" runat="server" class="hide">
                    <asp:Label ID="lbComplete" runat="server" Text="Order Complete" meta:resourcekey="lblComplete"></asp:Label>
                    <asp:Label ID="lbOrderNum" runat="server" Text="Donation Order #" meta:resourcekey="lbOrderNum"></asp:Label>
                    <asp:Label ID="lbOrderNumValue" runat="server"></asp:Label>
                </div>    
                <div id="divEmailNotification" runat="server" class="hide">
                    <asp:Label ID="lbEmailTitle" runat="server" Text="EmailNotification" meta:resourcekey="lbEmailTitle"></asp:Label>
                    <div style="background-color: #b0b0b0;height: 1px;"></div>
                    <asp:Label ID="lbEmail" runat="server" Text="Email" meta:resourcekey="lbEmail"></asp:Label>
                    <asp:Label ID="lbEmailValue" runat="server"></asp:Label>
                    <p id="pEmailComment" runat="server">
                        Herbalife will send you confirmation, status or updates to this email address.
                        To make this your primary address go to My Account -> My Profile to update your email address.
                    </p>
                </div>
                <div id="divTotals">
                    <asp:Label ID="lbTotalsTitle" runat="server" Text="Order Totals" meta:resourcekey="lbTotalsTitle"></asp:Label>
                    <div style="background-color: #b0b0b0;height: 1px;"></div>
                    <asp:Label ID="lbTax" runat="server" Text="Tax" meta:resourcekey="lbTax"></asp:Label>
                    <asp:Label ID="lbTaxValue" runat="server"></asp:Label>
                    <asp:Label ID="lbTotal" runat="server" Text="Grand Total" meta:resourcekey="lbTotal"></asp:Label>
                    <asp:Label ID="lbTotalValue" runat="server"></asp:Label>
                </div>
                <div id="divPayment" runat="server">
                    <asp:PlaceHolder ID="plPaymentOptions" runat="server" />
                </div>
                <div id="divPaymentSummary" runat="server" class="hide">
                    <asp:PlaceHolder ID="plPaymentSummary" runat="server" />
                </div>
                <div style="background-color: #b0b0b0;height: 1px;"></div>
                <div id="divSubmitCommand" runat="server">
                    <asp:Label ID="lbSubmit" runat="server" Text="Your order will be placed when you click 'Submit Order'" meta:resourcekey="lbSubmit"></asp:Label>
                    <cc1:DynamicButton runat="server" ID="btnCancel" ButtonType="Neutral"
                                    OnClick="OnCancel" meta:resourcekey="btnCancel" Text="Cancel" />
                    <cc1:DynamicButton runat="server" ID="btnBack" ButtonType="Neutral"
                                    OnClick="btnBack_Click" meta:resourcekey="btnBack" Text="Back" />
                    <cc1:DynamicButton runat="server" ID="btnSubmit" ButtonType="Forward"
                                    OnClick="btnSubmit_Click" meta:resourcekey="btnSubmit" Text="Submit Order" />
                </div>
                <div id="divEndCommand" runat="server" class="hide">
                    <cc1:DynamicButton runat="server" ID="btnPrint" ButtonType="Neutral"
                                    OnClientClick="ConfPrint(this);" meta:resourcekey="btnPrint" Text="Print This Page" />
                    <cc1:DynamicButton runat="server" ID="btnDone" ButtonType="Forward"
                                    OnClick="OnCancel" meta:resourcekey="btnDone" Text="Done" />
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnMake" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Panel>