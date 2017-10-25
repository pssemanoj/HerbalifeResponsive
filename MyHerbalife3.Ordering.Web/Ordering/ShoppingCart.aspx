<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="ShoppingCart.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.ShoppingCart"
    meta:resourcekey="PageResource1" EnableEventValidation="false" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register Src="~/Ordering/Controls/ExpireDatePopUp.ascx" TagPrefix="ExpirePopUp" TagName="ExpireDatePopUp" %>
<%@ Register Src="~/Ordering/Controls/APFDueReminderPopUp.ascx" TagPrefix="APFReminderPopUp" TagName="APFDuePopUp" %>
<%@ Register Src="~/Ordering/Controls/AddressRestrictionPopUp.ascx" TagPrefix="AdrsPopUp" TagName="AddressResPopUP" %>
<%@ Register Src="~/Ordering/Controls/Advertisement.ascx" TagName="Advertisement" TagPrefix="hrblAdvertisement" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeaderContent" runat="server">
      <script type="text/javascript">
          $(document).ready(function () {
              if ($(".myhl3").length != 0) { cleaningTable(); }
          });
    </script>

       <hrblAdvertisement:Advertisement ID="Advertisement" runat="server" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <div class="gdo-main-table">
        <div class="headersavedcart" runat="server" id="SavedCartTitle" Visible="false" >
            <asp:Label ID="lblSavedCartName" runat="server"></asp:Label>
        </div>
        <div class="gdo-main-tablecell row">
            <div>
                <cc1:ContentReader ID="ShippingAlertMsg" runat="server" ContentPath="shippingAlert.html"
                    SectionName="Ordering" ValidateContent="true" UseLocal="true"/>                
            </div>
            <div id="divHapEditMessage" runat="server" visible="false" class="gdo-right-column-text-blue">
                <asp:Label ID="lbHAPEditMessage" runat="server" Text="To save your changes for your Hap Standing Order you must click the submit button." meta:resourcekey="lbHAPEditMessageResource1" ></asp:Label>
            </div>
            <div class="headerbar">
                <div class="headerbar_leftcap">
                </div>
                <div class="headerbar_slidingdoor">
                    <div class="headerbar_icon">
                        <img alt="step1" src="/Content/Global/img/gdo/icons/step1.gif" /></div>
                    <div class="headerbar_text">
                        <asp:Label ID="lblStep3" runat="server" Text="Step 1 of 4 - Review Address and Email"
                            meta:resourcekey="lblStep3Resource1"></asp:Label></div>
                    <div class="headerbar_edit">
                    </div>
                </div>
            </div>           
            <div class="gdo-edit-header" id="divcustomerOrderStaticMessage" runat="server" visible="false"
                style="border-bottom: 0px">
                <asp:Label ID="lblCustomerOrderStaticMessage" runat="server"></asp:Label>
            </div>
            <div>
                <cc1:ContentReader ID="ReviewCartMessage2" runat="server" ContentPath="reviewcartstep1.html"
                    SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                <asp:PlaceHolder ID="plCartOptions" runat="server" />
            </div>
            <br />
        </div>
        <div class="clear"></div>
        <div class="gdo-main-tablecell row">
            <div class="headerbar">
                <div class="headerbar_leftcap">
                </div>
                <div class="headerbar_slidingdoor">
                    <div class="headerbar_icon">
                        <img src="/Content/Global/img/gdo/icons/step2.gif" alt="step2" /></div>
                    <div class="headerbar_text">
                        <asp:Label ID="Label1" runat="server" Text="Step 2 of 4 - Review Cart" meta:resourcekey="Label1Resource1"></asp:Label></div>
                    <div class="headerbar_edit">
                    </div>
                </div>
            </div>
            <div>
                <cc1:ContentReader ID="ReviewCartMessage1" runat="server" ContentPath="reviewcartstep2.html"
                    SectionName="Ordering" ValidateContent="true" UseLocal="true"/>
                <asp:PlaceHolder ID="plCheckOutTotalsMini" runat="server" />
            </div>
            <div><asp:PlaceHolder ID="plChinaAPF" runat="server" /></div>
        </div>
        <div class="clear"></div>
        <div class="gdo-main-tablecell row">
            <asp:PlaceHolder ID="plCheckOutHAPOptions" runat="server" />
        </div>
        <div class="clear"></div>
        <div class="gdo-main-tablecell row">
            <asp:PlaceHolder ID="plCheckOutOrderDetails" runat="server" />
        </div>       
        <div class="clear"></div>
    </div>
    <ajaxToolkit:ModalPopupExtender ID="mdlClearCart" runat="server" TargetControlID="FakeTarget"
        PopupControlID="pnlClearCart" BackgroundCssClass="modalBackground"
        DropShadow="false" />
    <asp:Label ID="FakeTarget" CssClass="hide" runat="server"/>
    <asp:Panel ID="pnlClearCart" runat="server">
        <asp:UpdatePanel ID="updClearCart" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="gdo-popup">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblClearCartTitle" runat="server" Text="Confirmation" meta:resourcekey="lblClearCartTitle"></asp:Label></h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblClearCartMessage1" runat="server" Text="You currently have items in your cart. If you would like to save your
						current cart for later use, enter a name below." meta:resourcekey="lblClearCartMessage1"></asp:Label>
                    </div>
                    <div runat="server" id="divExistentCart" visible="false" class="gdo-form-label-left">
                        <img alt="errorIcon" src="/Content/Global/img/gdo/icons/gdo-error-icon.gif" class="gdo-error-message-icon" />
                        <asp:Label runat="server" ID="lblExistentCart" CssClass="lblErrors hide" Text="The name you entered already exists.
						Please enter a different name." meta:resourcekey="lblExistentCart"></asp:Label>
                        <asp:Label runat="server" ID="lblEmptyCartName" CssClass="lblErrors hide" Text="Please enter a valid cart name."
                            meta:resourcekey="lblEmptyCartName"></asp:Label>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label runat="server" ID="lblSaveCartName" Text="New Cart Name" meta:resourcekey="lblSaveCartName"> </asp:Label>
                        <asp:TextBox runat="server" ID="txtSaveCartName" MaxLength="40"></asp:TextBox>
                        <ajaxToolkit:FilteredTextBoxExtender ID="fttxtSaveCartName" runat="server" TargetControlID="txtSaveCartName"
                            FilterMode="InvalidChars" InvalidChars="<>%!?{}[]´+¨*\¿?¡$=|~./;," />
                        <div class="gdo-button-margin-rt bttn-recalculate">
                            <cc2:DynamicButton ID="ClearCartSave" runat="server" ButtonType="Forward" Text="Save & Continue"
                                OnClick="OnSaveCart" meta:resourcekey="ClearCartSave" />
                        </div>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label runat="server" ID="lblClearCartMessage2" Text="Use suggested name or enter a new unique name."
                            meta:resourcekey="lblClearCartMessage2"> </asp:Label>
                    </div>
                    <div class="gdo-form-label-left">
                        <asp:Label ID="lblClearCartMessage3" runat="server" Text="If you do not save your cart,
						the items in your cart will be discared." meta:resourcekey="lblClearCartMessage3"></asp:Label>
                    </div>
                    <div class="gdo-button-margin-rt bttn-recalculate">
                        <cc2:DynamicButton ID="ClearCartCancel" runat="server" ButtonType="Back" Text="Cancel"
                            meta:resourcekey="ClearCartCancel" CssClass="clearCartCancel" OnClick="OnCancelCopyInvoice" />
                        <div id="ContinueDo">
                            <cc2:DynamicButton ID="ClearCartDo" runat="server" ButtonType="Forward" Text="Continue Without Saving"
                                OnClick="OnCopyWOSave" meta:resourcekey="ClearCartDo" />
                        </div>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
    <asp:UpdatePanel ID="UpdatePanelTermCondition" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <ajaxToolkit:ModalPopupExtender ID="TermConditionPopupExtender" runat="server" TargetControlID="TermConditionFakeTarget"
                PopupControlID="pnlTermCondition" CancelControlID="TermConditionFakeTarget" BackgroundCssClass="modalBackground"
                DropShadow="false" Enabled="False" />
            <asp:Button ID="TermConditionFakeTarget" runat="server" CausesValidation="False" Style="display: none" />
            <asp:Panel ID="PnlTermCondition" runat="server" Style="display: none">
                <div class="gdo-popup confirmCancel">
                    <div class="gdo-float-left gdo-popup-title">
                        <h2>
                            <asp:Label ID="lblTermConditionHeading" runat="server" Text="" meta:resourcekey="lblTermConditionHeading"></asp:Label>
                        </h2>
                    </div>
                    <div class="gdo-form-label-left">
                        <cc1:ContentReader ID="TermConditionMessage" runat="server" ContentPath="lblTermConditionMessage.html" SectionName="Ordering" ValidateContent="true" uselocal="true"/>
                    </div>
                    <div class="gdo-form-label-left confirmButtons">
                        <cc2:DynamicButton ID="btnTermConditionYes" runat="server" ButtonType="Forward" Text="Yes" OnClick="OnTermConditionOk" meta:resourcekey="TermConditionOk" />
                        <cc2:DynamicButton ID="btnTermConditionNo" runat="server" ButtonType="Forward" Text="No" OnClick="OnTermConditionNo" meta:resourcekey="TermConditionNo" />
                    </div>
                </div>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
    <ExpirePopUp:ExpireDatePopUp runat="server" id="ExpireDatePopUp1" />
    <APFReminderPopUp:APFDuePopUp runat="server" id="APFDuermndrPopUp" />
    <AdrsPopUp:AddressResPopUP runat="server" ID="AddressResPopUP1" />
</asp:Content>
