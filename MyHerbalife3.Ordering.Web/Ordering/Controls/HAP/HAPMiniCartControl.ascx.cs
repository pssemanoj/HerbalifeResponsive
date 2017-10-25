using HL.Common.EventHandling;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.HAP
{
    public partial class HAPMiniCartControl : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ShoppingCart.DsType == null)
            {
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(DistributorID, CountryCode);
                ShoppingCart.DsType = DistributorType;
            }

            if (ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled && ShoppingCart.DsType == Scheme.Member)
            {
                ShoppingCart.HAPType = "01";
                SessionInfo.HAPOrderType = "Personal";
            }


            if (!IsPostBack)
            {
                bool isStatic = !string.IsNullOrEmpty(ShoppingCart.OrderNumber) && !string.IsNullOrEmpty(ShoppingCart.HAPType);

                pnlHAPOptions.Visible = !isStatic;
                pnlHAPOptionsStatic.Visible = isStatic;

                if (isStatic)
                {
                    switch (ShoppingCart.HAPType)
                    {
                        case "01": lblOrderTypeSelected.Text = GetLocalResourceObject("PersonalRBText.Text").ToString(); break;
                        case "02": lblOrderTypeSelected.Text = GetLocalResourceObject("ResaleRBText.Text").ToString(); break;
                    }

                    switch (ShoppingCart.HAPScheduleDay)
                    {
                        case 4: lblScheduleSelected.Text = GetLocalResourceObject("rb1Option.Text").ToString(); break;
                        case 11: lblScheduleSelected.Text = GetLocalResourceObject("rb2Option.Text").ToString(); break;
                        case 18: lblScheduleSelected.Text = GetLocalResourceObject("rb3Option.Text").ToString(); break;
                    }
                }
                else
                {
                    if (ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled)
                    {
                        tbHAPOrderType.Visible = false;
                    }
                    else
                    {
                        var oHAPOrders = (Page as ProductsBase).ActiveHAPOrders;
                        bool hasPersonalHAPOrder = false;
                        bool hasResaleHAPOrder = false;
                        if (oHAPOrders != null && oHAPOrders.Count > 0)
                        {
                            hasPersonalHAPOrder = oHAPOrders.Any(i => i.HapOrderProgramType == "01");
                            hasResaleHAPOrder = oHAPOrders.Any(i => i.HapOrderProgramType == "02");
                            rbPersonal.Visible = !hasPersonalHAPOrder;
                            rbResale.Visible = !hasResaleHAPOrder;
                        }

                        switch (ShoppingCart.HAPType)
                        {
                            case "01": rbPersonal.Checked = true; break;
                            case "02": rbResale.Checked = true; break;
                        }
                    }

                    if (ShoppingCart.HAPScheduleDay > 0)
                    {
                        switch (ShoppingCart.HAPScheduleDay)
                        {
                            case 4: rb1.Checked = true; break;
                            case 11: rb2.Checked = true; break;
                            case 18: rb3.Checked = true; break;
                        }
                    }
                }
            }

            //10.	HAP orders deadline dates are: 
            //o	US  11th and 18th
            //o	CA  4th, 11th and 18th
            if (CountryCode == "US")
            {
                rb1.Visible = false;
                rb1.Enabled = false;
            }
            else if (CountryCode == "CA")
            {
                rb1.Visible = true;
                rb1.Enabled = true;
            }
        }

        protected void Saletype_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPersonal.Checked)
            {
                ShoppingCart.HAPType = "01";
                SessionInfo.HAPOrderType = "Personal";
            }
            else if (rbResale.Checked)
            {
                ShoppingCart.HAPType = "02";
                SessionInfo.HAPOrderType = "RetailOrder";
            }
        }

        protected void hapSchedule_CheckedChanged(object sender, EventArgs e)
        {
            ShoppingCart.HAPScheduleDay = rb1.Checked ? 4 : rb2.Checked ? 11 : 18;
        }
    }
}