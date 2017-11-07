<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="MyHerbalife3.Shared.UI.Helpers" %>

<!DOCTYPE html>
<html lang="en" xmlns="http://www.w3.org/1999/xhtml" class="myhl2">
<head>
    <meta charset="utf-8" />
    <title><asp:Localize ID="Localize8" runat="server" meta:resourcekey="lbTitle" Text="404 - Hmmm... Something is missing here"></asp:Localize></title>
    <link rel="stylesheet" type="text/css" media="all" href="/SharedUI/CSS/myhl2-global.css" />
    <link rel="stylesheet" type="text/css" media="all" href="/SharedUI/CSS/myhl2-hl-ui/myhl2-hl-ui-1.0.4.css" />
    <style type="text/css">
        h1 {
            padding: 10px 0;
            border-top: 10px solid #7ac142;
            border-bottom: 2px solid #fcb034;
            color: #7ac142;
        }

        body {
            font-family: Helvetica, Arial, sans-serif;
            font-size: 14px;
            line-height: 20px;
        }

        .buttons {
            margin: 20px 0;
        }

        .icon-home {
            font-size: 20px;
            vertical-align: middle;
        }

        .forward {
            line-height: 20px;
        }
    </style>
</head>
<body>

    <script language="CS" runat="server"> 
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Response.Status = "404 not found";
        Response.StatusCode = 404;
    } 
    </script>
    <div class="container" id="page404">
        <header>
            <img src="/SharedUI/Images/logos/HeaderLogo.gif" />
        </header>
        <h1>
            <asp:Localize ID="Localize5" runat="server" meta:resourcekey="lbTitle" Text="404 - Hmmm... Something is missing here"></asp:Localize>
        </h1>

        <h2>
            <asp:Localize ID="Localize6" runat="server" meta:resourcekey="lbTitle2" Text="The server cannot make your request"></asp:Localize>
        </h2>

        <h3>
            <asp:Localize ID="Localize7" runat="server" meta:resourcekey="lbTitle3" Text="What should you do?"></asp:Localize>
        </h3>
        <ul>
            <li>
                <asp:Localize ID="Localize1" runat="server" meta:resourcekey="lbOption1" Text="If you typed in the URL, please try typing it again"></asp:Localize>
            </li>
            <li>
                <asp:Localize ID="Localize2" runat="server" meta:resourcekey="lbOption2" Text="Our technical team has been notified and is looking into the issue"></asp:Localize>
            </li>
        </ul>

        <div class="buttons">
            <!-- button type="button" class="neutral" onclick="goBack()">&laquo; Back To Previous Page</button -->
            <a href="/" class="forward">
                <asp:Localize ID="Localize3" runat="server" meta:resourcekey="btHome" Text="Go To Home Page"></asp:Localize>
                <i class="icon-home"></i>
            </a>
        </div>

        <footer>
            <div style="padding: 10px;">
                <asp:Localize ID="Localize4" runat="server" meta:resourcekey="Footer"
                    Text="Copyright &copy; Herbalife International of America, Inc. No reproduction in whole or in part without written permission. All Rights Reserved. All trademarks and product images exhibited on this site, unless otherwise indicated, are the property of Herbalife International, Inc."></asp:Localize>
            </div>
            <div class="noindex serverSig">
                <span>
                    <%= HttpContext.Current.ApplicationInstance.ServerStamp() %>
                </span>
            </div>
        </footer>
    </div>

</body>
</html>
