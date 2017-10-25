<%@ Page Language="C#"%>

<%@ Import Namespace="HL.Common.Utilities" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%@ Import Namespace="System.IO" %>

<script language="C#" runat="server">
    
    void Page_Load(object sender, EventArgs e)
    {
        lblStatus.Text = GetStatus();
    }

        protected string GetStatus()
    {
        Session.Abandon();
        lblServerInfo.Text = WebUtilities.GetFooterDebugText();

                string nlbGlobalFilePath = @"C:\Program Files\Herbalife\Configuration\NLBControl.Online";
        string localFilePath = @"App_Data\NLBControl.Online";
        string nlbLocalFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, localFilePath );
        var applicationPath = HostingEnvironment.ApplicationPhysicalPath.Replace(@"\\","/").Replace(@"\","/");
        var splitArray = applicationPath.Split('/');
        var appname = splitArray[splitArray.Length-1];
        appname = string.IsNullOrEmpty(appname) ? splitArray[splitArray.Length - 2] : appname;


        if ( File.Exists(nlbLocalFilePath) && File.Exists(nlbGlobalFilePath) )
        {

            return "--ONLINE--" + "|" + appname+"|";
        }

            return "--OFFLINE--" +"|" + appname +"|";
        
    }

</script>

<asp:label id="lblStatus" runat="server"></asp:label>
<asp:label id="lblServerInfo" runat="server"></asp:label>
