using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using HL.Common.Configuration;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Faq
{
    public partial class Faq_CN : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HolidayFileId.UseLocal = false;
                HolidayFileId.SectionName = "ordering";
                HolidayFileId.ContentPath = "faq.html";
                HolidayFileId.LoadContent();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Faq_CN : file path not found {0}", ex.Message));
            }

        }
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (File.Exists(HolidayFileId.FilePath))
            {
                if (!CheckFile(HolidayFileId.HtmlContent))
                {
                    HolidayFileId.HtmlContent = "<em class='FAQnoHoliday'>" + GetLocalResourceObject("HolidayTable.Text") + "</em>";
                }
            }
            else
            {
                HolidayFileId.HtmlContent = "<em class='FAQnoHoliday'>" + GetLocalResourceObject("HolidayTable.Text") + "</em>";
            }
        }

        private bool CheckFile(string htmlContent)
        {
            var regex = new Regex("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.IgnoreCase);
            var urls = regex.Matches(htmlContent).OfType<Match>().Select(m => m.Groups["href"].Value).SingleOrDefault();

            if (!string.IsNullOrEmpty(urls))
            {
                HttpWebResponse response = null;
                var request = (HttpWebRequest)WebRequest.Create(urls);
                request.Method = "GET";
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    return true;
                }
                catch (WebException ex)
                {
                    /* A WebException will be thrown if the status of the response is not `200 OK` */
                    return false;
                }
                finally
                {
                    // Don't forget to close your response.
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
            else
            {
                return false;
            }
        }
    }
}

