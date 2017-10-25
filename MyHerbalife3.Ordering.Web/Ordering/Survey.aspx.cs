using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Survey;
using MyHerbalife3.Ordering.Web.MasterPages;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class Survey : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string usercontrol = Request.QueryString["@ctrl"];
            GetSurveyControl(usercontrol);

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid gdo-nav-mid-survey");
        }

        private void GetSurveyControl(string usercontrol)
        {
            switch (usercontrol)
            {
                case "PCPromoSurvey":
                    PCPromoSurvey pccontrol = (PCPromoSurvey) LoadControl("Controls/Survey/PCPromoSurvey.ascx");
                    PlaceHolder1.Controls.Add(pccontrol);
                    break;
                case "CustomerSurvey":
                    CustomerSurvey cscontrol = (CustomerSurvey)LoadControl("Controls/Survey/CustomerSurvey.ascx");
                    PlaceHolder1.Controls.Add(cscontrol);
                    break;
                    
            }
        }
    }
}