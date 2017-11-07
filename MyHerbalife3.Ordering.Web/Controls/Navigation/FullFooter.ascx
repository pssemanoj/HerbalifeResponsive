<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FullFooter.ascx.cs" Inherits="MyHerbalife3.Web.Controls.Navigation.FullFooter" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Resources" %>
<%@ Import  Namespace="MyHerbalife3.Shared.UI.Helpers" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Core.ContentProxyProvider.Helpers" Assembly="MyHerbalife3.Core.ContentProxyProvider" %>
<% if (IsMicroServiceEnabled)
    { %>
    <% =MyHerbalife.Navigation.ApiAdapter.Helpers.NavigationHelper.GetContent("Footer",
                                                        currentExperience.NavigationVersion,
                                                         _GlobalContext.CultureConfiguration.Locale,
                                                        currentExperience.BrowseScheme
                                                        )%>
<% }
    else
    { %>

        <!-- FOOTER -->
        <footer>
            <nav class="footer-links">
                <!-- begin footerLinks  -->
                <cc1:FragmentReader id="_FooterLinksFragment" path="footerLinks.xml" runat="server" />
                <!-- end footerLinks -->
            </nav>
            <cc1:FragmentReader id="_FooterFragment" path="footer.xml" runat="server" />
        </footer>
            <!-- /FOOTER -->

<%} %>

<div class="noindex serverSig">
    <span>
        <%: HttpContext.Current.ApplicationInstance.ServerStamp() %>
    </span>
</div>