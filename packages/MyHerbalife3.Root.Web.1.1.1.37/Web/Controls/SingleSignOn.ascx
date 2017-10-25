<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SingleSignOn.ascx.cs" Inherits="MyHerbalife3.Web.Controls.SingleSignOn" %>
<%@ Import Namespace="HL.Common.Configuration" %>
<%@ Import Namespace="MyHerbalife3.Shared.Common.Infrastructure" %>
<%@ Import Namespace="MyHerbalife3.Shared.Providers" %>

<script runat="server">
    protected bool IsLoggingOut = CookieHandler.IsLoggingOutSso();
    protected string AccessToken = CookieHandler.GetSsoAccessTokenCookie();
    protected string AuthToken = CookieHandler.GetSsoAuthToken();
    protected string AuthTokenId = CookieHandler.GetSsoAuthTokenId();
    protected bool MustEstablish()
    {
        return Session[Constants.Authentication.SsoEstablishSession] != null && Convert.ToBoolean(Session[Constants.Authentication.SsoEstablishSession]) ;
    }
</script>

<script type="text/javascript">
    function setAuth(authToken) {
        <%if (string.IsNullOrEmpty(AccessToken) && !Request.IsAuthenticated)
          {
              Response.Write(new HtmlString("window.location = '/Authentication/SingleSignOn?authToken' = authToken;"));
          }%>
    }
</script>

<%if (MustEstablish())
  {
      Session[Constants.Authentication.SsoEstablishSession] = null;
      Session.Remove(Constants.Authentication.SsoEstablishSessio);
      
      if (!string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(AuthToken) && !string.IsNullOrEmpty(AuthTokenId))
      {
          %>
            <script type="text/javascript" src="<%=string.Format(@"{0}api/establish?authTokenId={1}&accessToken={2}&authtoken={3}&callback=", Settings.GetRequiredAppSetting("SsoProxyLogoutUri", string.Empty), AuthTokenId, AccessToken, AuthToken)%>"></script>
          <%  
      }
      
      CookieHandler.ClearSsoTempData();
  }
  
  if (IsLoggingOut)
  {
        CookieHandler.RemoveSsoCookie();
        %>
        <script type="text/javascript" src="<%=Settings.GetRequiredAppSetting("SsoProxyLogoutUri", string.Empty)%>"></script>
        <script type="text/javascript" src="<%=Settings.GetRequiredAppSetting("SsoProfileLogoutUrl", string.Empty)%>"></script>
        <%  

  }
  else
  {
        %>
        <script type="text/javascript" src="<%=Settings.GetRequiredAppSetting("SsoExchangeUrl", string.Empty)%>"></script>
        <%
  } %>