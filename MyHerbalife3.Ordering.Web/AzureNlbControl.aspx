<%@ Page Language="C#"%>

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
        lblServerInfo.Text = string.Format("{0} | {1:MM/dd/yyyy hh:mm:ss tt} | {2}", System.Threading.Thread.CurrentThread.CurrentCulture.Name,  DateTime.Now, Environment.MachineName);
        string nlbGlobalFilePath = @"C:\Program Files\Herbalife\Configuration\NLBControl.Online";
        string localFilePath = @"App_Data\NLBControl.Online";
        string nlbLocalFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, localFilePath );

        if ( File.Exists(nlbLocalFilePath) && File.Exists(nlbGlobalFilePath) )
        {
            return "--ONLINE--";
        }
        Response.Redirect("sendmeanerror.aspx");
        return string.Empty;
    }
</script>

<asp:label id="lblStatus" runat="server"></asp:label>
<asp:label id="lblServerInfo" runat="server"></asp:label>

