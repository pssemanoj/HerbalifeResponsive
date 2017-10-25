using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class RuleRegulation : UserControlBase
    {
        public LinkButton _lbConfirm;

        public string _url;
        public string url
        {
            set { _url = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            _lbConfirm = this.OK;
            //if (IsChinaApp)
            //{
            //    if (IsPostBack)
            //    {
            //        InvoiceRulePopupExtender.Show();
            //    }
            //}

            lblChkout24hTitle.Text = "康宝来 规条";
        }


        public void Show()
        {
            InvoiceRulePopupExtender.Show();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

        }

        protected bool IsChinaApp
        {
            get
            {
                return HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
        }

        public List<Control> AddToCartControlList
        {
            get;
            set;
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            Response.Redirect("~\\Ordering\\pricelist.aspx");
        }
    }
}