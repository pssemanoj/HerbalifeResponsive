<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="THThirdPartyPickupControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup.THThirdPartyPickupControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc2" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Web.MasterPages" %>

<script type="text/javascript">
<% if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
   { %>
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
    function EndRequestHandler(sender, args) {
        $('#TB_ajaxContent .gdo-popup').css('max-height', $(window).outerHeight() - (Math.round($(window).outerHeight() * 0.084)));
    }
<% } %>
</script>
<div></div>
<h3>
    <asp:Label ID="lblHeader" runat="server" Text="Find 7Eleven Pick-up Locations" meta:resourcekey="lblHeaderResource"></asp:Label>
</h3>
<div>
    <asp:Label ID="lblErrors" runat="server" ForeColor="Red" meta:resourcekey="lblErrors"></asp:Label>
</div>
<div id="divAddPickupLocation" runat="server">
    <div>
        <cc2:ContentReader ID="locationInfo" runat="server" ContentPath="SelectPickupLocationInfo.html" SectionName="Ordering" ValidateContent="true" UseLocal="true" />
    </div>
    <div>
        <asp:Label ID="lblSelectLocations" runat="server" Text="Select Locations on Map" meta:resourcekey="lblSelectLocations" CssClass="select-location"></asp:Label>
        <div class="right">
            <cc1:OvalButton ID="btnCancel" runat="server" Coloring="Silver" OnClick="btnCancel_Click" Text="Cancel"
                ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelResource" CssClass="backward"></cc1:OvalButton>
        </div>
    </div>
    <div hidden>
            <asp:Button ID="btnContinueHidden" runat="server" Text="Button" OnClick="ContinueChanges_Click" CommandArgument="Add" />
    </div>
    <div class="clear"></div>
    <div id="divMap">
        <iframe id="ifTh" src="<%= HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupFromCourierMapURL %>">
        </iframe>
        <asp:HiddenField ID="hdnMapResponse" runat="server" />
    </div>
</div>
<div id="divDeletePickupLocation" runat="server">
    <div>
        <h3>
            <asp:Label ID="lblDeleteHeader" runat="server" meta:resourcekey="lblDeleteHeaderResource"></asp:Label>
        </h3>
    </div>
    <div>
        <asp:Label ID="lblMessageConfirmation" runat="server" Text="Nickname:" meta:resourcekey="lblMessageConfirmationResource"></asp:Label>
    </div>
    <div>
        <cc1:OvalButton ID="btnContinueDelete" runat="server" Coloring="Silver" OnClick="ContinueChanges_Click" CommandArgument="Delete"
            Text="OK" ArrowDirection="" IconPosition="" meta:resourcekey="btnDeleteResource" CssClass="forward"></cc1:OvalButton>
        <cc1:OvalButton ID="btnCancelDelete" runat="server" Coloring="Silver" OnClick="btnCancel_Click"
            Text="Cancel" ArrowDirection="" IconPosition="" meta:resourcekey="btnCancelDeleteResource" CssClass="backward left"></cc1:OvalButton>
        <div class="clear"></div>
    </div>
</div>
