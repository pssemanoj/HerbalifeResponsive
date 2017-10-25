using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Web.MasterPages;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Web.Ordering
{
	public partial class Faq : ProductsBase
    {
        //private string GetFAQFilePath()
        //{
        //    StringBuilder sb = new StringBuilder(128);
        //    sb.Append("~/content/");
        //    sb.Append(CultureInfo.CurrentCulture.Name);
        //    sb.Append("/pdf/products/GDO_FAQs.pdf");            
           
        //    return MapPath(sb.ToString());
        //}

		protected void Page_Load(object sender, EventArgs e)
		{
            try
            {
                Control faqCtrl = LoadControl(string.Format("~/Ordering/Controls/Faq/Faq_{0}.ascx", CountryCode));
                if (faqCtrl != null)
                    pnlFaq.Controls.Add(faqCtrl);
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(string.Format("Error: Country: {0} has no Faq user control presents. Exception: {1}", CountryCode, ex.Message));
            }
            
            
            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            }

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-10 gdo-nav-mid-fs");

            //(this.Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);         
            //try
            //{              
            //    Response.Clear();
            //    Response.ClearHeaders();
            //    Response.Charset = "";
            //    Response.ContentType = "Application/pdf";
            //    Response.Expires = -1;
            //    Response.WriteFile(GetFAQFilePath());
            //    Response.Flush();
            //    Response.End();
            //}
            //catch (Exception ex)
            //{
            //    throw new ApplicationException("Unable to open FAQ pdf file");
            //}
		}
	}
}