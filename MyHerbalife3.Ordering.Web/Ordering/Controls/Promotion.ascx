<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Promotion.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Promotion" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<div id="gdo-right-column-hff">
    <asp:UpdatePanel runat="server" ID="promotionPanel" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="gdo-right-column-tbl gdo-hff-module" runat="server" id="divPromo">
                <div class="gdo-right-column-header">
                    <h3>
                        <asp:Literal ID="ltLine1" runat="server" Text="Your Free Gift" meta:resourcekey="ltLine1Resource1"></asp:Literal></h3>
                </div>
                <div class="gdo-clear gdo-horiz-div">
                </div>
                <div class="gdo-right-column-text">
                    <asp:Label ID="lblFreeGift" runat="server" ForeColor="red" Text="Your order qualifies for a free gift" meta:resourcekey="lblFreeGiftResource1"></asp:Label>
                </div>
                <div class="gdo-spacer2">
                </div>
                <div class="gdo-right-column-text gdo-tdmag-radio" id="dvFreeGiftSKU" runat="server">
                    <asp:Panel runat="server" ID="pnlFreeGift">
                    <asp:RadioButtonList ID="rblFreeGiftlist" runat="server" EnableViewState="true" Visible="false"></asp:RadioButtonList>  
                    <asp:CheckBoxList ID="cblFreeGiftlist" runat="server" EnableViewState="true"></asp:CheckBoxList>
                                 
                         <%-- <asp:RadioButton ID="rbFirstGift" runat="server" Text="Good Stuff1"
                        GroupName="rbGroup" meta:resourcekey="rbFirstGiftResource1" />
                    <asp:HiddenField runat="server" ID="FirstGiftSKU" />
                    <br />
                    <asp:RadioButton ID="rbSecondGift" runat="server" Text="Good Stuff2"
                        GroupName="rbGroup" meta:resourcekey="rbSecondGiftResource1" />
                    <asp:HiddenField runat="server" ID="SecondGiftSKU" />
                     <br />
                     <asp:RadioButton ID="rbThirdGift" runat="server" Text="Good Stuff3"
                        GroupName="rbGroup" meta:resourcekey="rbThirdGiftResource1" />
                    <asp:HiddenField runat="server" ID="ThirdGiftSKU" />
                     <br />
                     <asp:RadioButton ID="rbForthGift" runat="server" Text="Good Stuff4"
                        GroupName="rbGroup" meta:resourcekey="rbForthGiftResource1" />
                    <asp:HiddenField runat="server" ID="ForthGiftSKU" />
                     <br />
                     <asp:RadioButton ID="rbFifthGift" runat="server" Text="Good Stuff5"
                        GroupName="rbGroup" meta:resourcekey="rbFifthGiftResource1" />
                    <asp:HiddenField runat="server" ID="FifthGiftSKU" />--%>
                    </asp:Panel>
                  
                </div>
                <div class="tmagbuttonaddcart">
                    <cc1:DynamicButton ID="btnAddToCart" runat="server" ButtonType="Forward" Text="Add to Cart"
                        OnClick="btnAddToCart_Click" IconClass="" IconPosition="Left" meta:resourcekey="btnAddToCartResource1" NavigateUrlToNewWindow="False" Rel="" />
                </div>
                <div class="gdo-spacer2">
                </div>
                <div class="gdo-right-column-text-error">
                    <asp:Label ID="lblError" runat="server" meta:resourcekey="lblErrorResource1"></asp:Label>
                </div>
                <div class="gdo-spacer2">
                </div>
                <br />
                <div class="gdo-right-column-text">
                    <asp:Label ID="lblPromoMessage" Visible="False"  runat="server" ForeColor="red" Text="Please be noticed that the free gift you selected will be shipped to you separately after sept 25. Thank you for your understanding." meta:resourcekey="lblPromoMessageResource1"></asp:Label>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

</div>
