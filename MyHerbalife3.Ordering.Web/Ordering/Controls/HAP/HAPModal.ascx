<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HAPModal.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HAP.HAPModal" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<style>
    /* Modals (mixins)
    ================================================== */
	/* Modals (Style, Positioning, Size)
    ================================================== */
    .reveal-modal-bg {
      position: fixed;
      height: 100%;
      width: 100%;
      background: #000;
      background: rgba(0, 0, 0, 0.8);
      z-index: 100;
      display: none;
      top: 0;
      left: 0; }

    .reveal-modal {
      visibility: hidden;
      position: fixed;
      width: 100%;
      height: 100%;
      padding: 40px 10px 15px;
      background: #fff;
      z-index: 101;
      -moz-box-shadow: 0 0 10px rgba(0, 0, 0, 0.4);
      -webkit-box-shadow: 0 0 10px rgba(0, 0, 0, 0.4);
      box-shadow: 0 0 10px rgba(0, 0, 0, 0.4); }
      @media (min-width: 320px) and (max-width: 660px) {
        .reveal-modal {
          top: 0 !important;
          left: 0; } }
      @media (min-width: 660px) {
        .reveal-modal {
          top: 100px;
          left: 50%;
          height: auto;
          width: 620px;
          margin-left: -310px;
          padding: 40px 40px 30px; } }
      @media (min-width: 660px) {
        .reveal-modal.large {
          width: 620px;
          margin-left: -310px; } }
      @media (min-width: 1000px) {
        .reveal-modal.large {
          width: 780px;
          margin-left: -390px; } }
      @media (min-width: 1320px) {
        .reveal-modal.large {
          width: 1100px;
          margin-left: -550px; } }
      @media (min-width: 660px) {
        .reveal-modal.notify, .reveal-modal.small {
          top: 100px;
          left: 50%;
          height: auto;
          width: 380px;
          margin-left: -190px; } }
      .reveal-modal.notify {
        text-align: center; }
        .reveal-modal.notify a {
          margin-bottom: 0 !important; }
      .reveal-modal .close-icon {
        font-size: 22px;
        line-height: .5;
        position: absolute;
        top: 12px;
        right: 12px;
        color: #ccc;
        text-shadow: 0 -1px 1px rbga(0, 0, 0, 0.6);
        font-weight: bold;
        cursor: pointer; }
      .button-set a { float: right; }
</style>
    <!-- Renew HAP Order Modal -->
    <div id="modalRenew" class="reveal-modal medium" style="opacity: 1; visibility: visible; top: 100px;" runat="server">
        <asp:Literal ID="litRenewDescription" runat="server" meta:resourcekey="RenewModalDescription" Text=""></asp:Literal>
        <div class="button-set">
            <cc1:DynamicButton ID="RenewHapOrder" runat="server" ButtonType="Forward" OnClick="RenewHapOrder_Click" meta:resourcekey="RenewHapOrderResource1" name="renewHAP" />
            <cc1:DynamicButton ID="CancelRenew" runat="server" ButtonType="Back" OnClick="Cancel_Click" meta:resourcekey="CancelRenewResource1" name="cancelRenew" Text="Cancel" />
        </div>
        <a class="close-reveal-modal close-icon"><i class="icon-delete-fl-5"></i></a>
    </div>    

    <!-- Cancel HAP Order Modal -->
    <div id="modalCancel" class="reveal-modal medium" style="opacity: 1; visibility: visible; top: 100px;" runat="server">
        <asp:Literal ID="litCancelDescription" runat="server" meta:resourcekey="CancelModalDescription" Text=""></asp:Literal>
        <div class="button-set">
            <cc1:DynamicButton ID="CancelHapOrder" runat="server" ButtonType="Forward" OnClick="CancelHapOrder_Click" meta:resourcekey="CancelHapOrderResource1" name="cancelHAP" />
            <cc1:DynamicButton ID="Cancel2" runat="server" ButtonType="Back" OnClick="Cancel_Click" meta:resourcekey="CancelResource1" name="cancelButton" Text="Cancel" />
        </div>
        <a class="close-reveal-modal close-icon"><i class="icon-delete-fl-5"></i></a>
    </div>

    <!-- Edit HAP Order Modal -->
    <div id="modalEdit" class="reveal-modal medium" style="opacity: 1; visibility: visible; top: 100px;" runat="server">
        <asp:Literal ID="litEditDescription" runat="server" meta:resourcekey="EditModalDescription" Text=""></asp:Literal>
        <div class="button-set">
            <cc1:DynamicButton ID="EditHapOrder" runat="server" ButtonType="Forward" OnClick="EditHapOrder_Click" meta:resourcekey="EditHapOrderResource1" name="editHAP" />
            <cc1:DynamicButton ID="Cancel3" runat="server" ButtonType="Back" OnClick="Cancel_Click" meta:resourcekey="CancelResource1" name="cancelEdit" Text="Cancel" />
        </div>
        <a class="close-reveal-modal close-icon"><i class="icon-delete-fl-5"></i></a>
    </div>

        <!-- JS
    ================================================== -->
    <!-- Grab Google CDN's jQuery, with a protocol relative URL; fall back to local if necessary -->
    <script src="http://code.jquery.com/jquery-1.10.2.js"></script>
    <!-- Modal JS -->
    <script src="../../../../SharedUI_V2/Prod/Trunk/js/jquery.reveal.js"></script>
    <script src="D:\SRC\DTS\App\Web\SharedUI_V2\Prod\Trunk\js\jquery.reveal.js"></script>

    <!-- Turns background grid blocks on and off -->
    <script>
    $( "#toggleBlocks" ).click(function() {
         $(".content-block").toggleClass("lightblue");
             
            var isBlockOn = $("#toggleBlocks").html();
               
            if (isBlockOn == "on") {
                $("#toggleBlocks").html("off")
            }
            else {
                $("#toggleBlocks").html("on");
            }
    });
    </script>
<div class="reveal-modal-bg" style="display: block; cursor: pointer;"></div>