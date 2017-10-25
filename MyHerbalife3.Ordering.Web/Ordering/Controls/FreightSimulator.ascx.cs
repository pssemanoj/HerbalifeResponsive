using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class FreightSimulator : UserControlBase
    {
        public class FreightInfo
        {
            public string Shippment { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string District { get; set; }
            public string Weight { get; set; }
            public string Freight { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LookupStates();
            }
        }

        private void LookupStates(bool autoDefault = false)
        {
            ddlState.Items.Clear();
            if (ShippingProvider != null)
            {
                var lookupResults = ShippingProvider.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        string[] item = province.Split('-');
                        ddlState.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlState.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlState.SelectedIndex = (autoDefault && (ddlState.Items.Count > 0)) ? 1 : 0;

                    if (ddlState.SelectedIndex > 0) LookupCities(ddlState.SelectedItem.Value, autoDefault);
                }
            }
        }

        private void LookupCities(string state, bool autoDefault = false)
        {
            if (ShippingProvider != null)
            {
                var lookupResults = ShippingProvider.GetCitiesForState(ProductsBase.CountryCode, state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        string[] item = city.Split('-');
                        ddlCity.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlCity.SelectedIndex = (autoDefault && (ddlCity.Items.Count > 0)) ? 1 : 0;

                    if (ddlCity.SelectedIndex > 0) LookupOther(ddlState.SelectedItem.Value, ddlCity.SelectedItem.Value, autoDefault);
                }
                else
                {
                    ddlState.Focus();
                }
            }
        }

        private void LookupOther(string state, string city, bool autoDefault = false)
        {
            if (ShippingProvider != null)
            {
                var lookupResults = ShippingProvider.GetStreetsForCity(ProductsBase.CountryCode, state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var district in lookupResults)
                    {
                        string[] item = district.Split('-');
                        ddlDistrict.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlDistrict.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlDistrict.SelectedIndex = (autoDefault && (ddlDistrict.Items.Count > 0)) ? 1 : 0;
                }
                else
                {
                    ddlCity.Focus();
                }
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrEmpty(ddlState.SelectedItem.Value) || string.IsNullOrEmpty(ddlCity.SelectedItem.Value) ||
                string.IsNullOrEmpty(ddlDistrict.SelectedItem.Value))
            {
                lblError.Text = GetLocalResourceObject("NoRegion") as string;
                return false;
            }

            if (string.IsNullOrEmpty(txtWeight.Text.Trim()))
            {
                lblError.Text = GetLocalResourceObject("NoWeight") as string;
                return false;
            }

            decimal weight = 0;
            if (!decimal.TryParse(txtWeight.Text.Trim(), out weight))
            {
                lblError.Text = GetLocalResourceObject("NoWeight") as string;
                return false;
            }

            return true;
        }

        private void Calculate()
        {
            pnlMessages.Visible = !Validate();
            divResult.Visible = !pnlMessages.Visible;

            if (!pnlMessages.Visible)
            {
                var shippingInfo = new ServiceProvider.OrderChinaSvc.ShippingInfo_V01
                {
                    Address = new ServiceProvider.OrderChinaSvc.Address_V01
                    {
                        StateProvinceTerritory = ddlState.SelectedItem.Text,
                        City = ddlCity.SelectedItem.Text,
                        CountyDistrict = ddlDistrict.SelectedItem.Text
                    }
                };

                var provider = this.ShippingProvider as ShippingProvider_CN;
                var freight = provider.CalculateFreight(shippingInfo, decimal.Parse(txtWeight.Text.Trim()));

                if (freight != null && !string.IsNullOrEmpty(freight.StoreName))
                {
                    var source = new List<FreightInfo>
                        {
                            new FreightInfo() 
                                {
                                    Shippment = freight.StoreName,
                                    State = shippingInfo.Address.StateProvinceTerritory,
                                    City = shippingInfo.Address.City,
                                    District = shippingInfo.Address.CountyDistrict,
                                    Weight = string.Format("{0}kg", txtWeight.Text.Trim()),
                                    Freight = getAmountString(freight.EstimatedFreight)
                                }
                        };
                    grvFreightInfo.DataSource = source;
                    grvFreightInfo.DataBind();
                }
                else
                {
                    lblError.Text = GetLocalResourceObject("NotSupported") as string;
                    pnlMessages.Visible = true;
                    divResult.Visible = !pnlMessages.Visible;
                }
            }
        }

        protected void ddlStateProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlState.SelectedItem.Value))
            {
                ddlDistrict.Items.Clear();
                ddlCity.Items.Clear();
                LookupCities(ddlState.SelectedItem.Value);
            }
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlState.SelectedItem.Value) && !string.IsNullOrEmpty(ddlCity.SelectedItem.Value))
            {
                ddlDistrict.Items.Clear();
                LookupOther(ddlState.SelectedItem.Value, ddlCity.SelectedItem.Value);
            }
        }

        protected void OnCalculate(object sender, EventArgs e)
        {
            Calculate();
        }
    }
}