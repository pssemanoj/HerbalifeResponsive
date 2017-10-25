using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.ViewModel;
using HL.Common.Logging;
using HL.Common.Utilities;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Survey
{
    public partial class PCPromoSurvey : UserControlBase
    {
        PromotionCollection pInfo = new PromotionCollection();
        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as OrderingMaster).SetPageHeader("贵宾客户满意度问卷调查");
            lblError.Text = string.Empty;
            pInfo = PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform, "zh-CN"); ;
            Session["AttainedSurvey"] = false;

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            Validation();
            if (lblError.Text == string.Empty)
            {
                string jsonFormatValue =
               "{" +
               lblQuestion1.Text + ":[" +

               "{" +
               lblOption1.Text + ":" +
               txtOption1.Text
               + "}" +
               "{" +
               lblOption2.Text + ":" +
               txtOption2.Text
               + "}" +
               "{" +
               lblOption3.Text + ":" +
               txtOption3.Text
               + "}" +
               "{" +
               lblOption4.Text + ":" +
               txtOption4.Text
               + "}]" +

               "}" +
               "{" +
               lblQuestion2.Text + "[" +
               "{" + lblOption5.Text + ":" + txtOption5.Text + "}" +
               "{" + lblOption6.Text + ":" + txtOption6.Text + "}" +
               "{" + lblOption7.Text + ":" + txtOption7.Text + "}" +
               "{" +
               lblOption8.Text + ":" +
               txtOption8.Text
               + "}" +
               "]}" +

               "}";

                ChinaPromotionProvider.SubmitPCPromoSurvey(ProductsBase.ShoppingCart.DistributorID, "CN", DateUtils.GetCurrentLocalTime("CN"),
                    convertDateTime(pInfo.FirstOrDefault(x => x.Code.Equals("PCPromoSurvey")).StartDate),
                    convertDateTime(pInfo.FirstOrDefault(x => x.Code.Equals("PCPromoSurvey")).EndDate),
                    "PCPromoSurvey", jsonFormatValue);
                Session["AttainedSurvey"] = true;
                Response.Redirect("Pricelist.aspx");

            }

        }

        private void Validation()
        {
            var txtbox = new List<TextBox>();
            txtbox.Add(txtOption1);
            txtbox.Add(txtOption2);
            txtbox.Add(txtOption3);
            txtbox.Add(txtOption4);
            txtbox.Add(txtOption5);
            txtbox.Add(txtOption6);
            txtbox.Add(txtOption7);
            txtbox.Add(txtOption8);
            var result = txtbox.Count(x => x.Text.Trim().Equals(string.Empty));
            if (result > 0)
            {
                lblError.Text = "请填写所有领域";

            }


        }

        private DateTime convertDateTime(string datetime)
        {
            const string format = "MM-dd-yyyy";
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime dt;
                if (DateTime.TryParseExact(datetime, format, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out dt))
                {
                    return dt;
                }
            }
            return DateTime.MaxValue;
        }

        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            Session["SurveyCancelled"] = true;
            Session["RecalledPromoRule"] = true;
            Response.Redirect("Pricelist.aspx");

        }
    }
}