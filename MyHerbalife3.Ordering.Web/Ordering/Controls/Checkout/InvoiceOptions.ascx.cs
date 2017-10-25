using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class InvoiceOptions : UserControlBase
    {
        private RadioButtonList _invoiceOptionRBL = null;
        private MyHLShoppingCart _shoppingCart = null;

        public string SelectedInvoiceOption
        {
            get
            {
                if (_invoiceOptionRBL != null && null != _invoiceOptionRBL.SelectedItem)
                {
                    return _invoiceOptionRBL.SelectedItem.Value;
                }
                else
                    return null;
            }
            set
            {
                if (_invoiceOptionRBL != null)
                {
                    if (_invoiceOptionRBL.Items.Count > 0)
                    {
                        try
                        {
                            _invoiceOptionRBL.SelectedItem.Value = value;
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public bool IsReadOnly { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!IsPostBack)
            //{
            _shoppingCart = ShoppingCart;
            if (IsReadOnly)
            {
                trReadOnly.Visible = true;
                trEditInvoice.Visible = false;
            }
            else
            {
                trReadOnly.Visible = false;
                trEditInvoice.Visible = true;
            }
            populateInvoiceOptions();
            if (_invoiceOptionRBL != null)
            {
                if (_invoiceOptionRBL.Items.Count == 1)
                {
                    trReadOnly.Visible = true;
                    trEditInvoice.Visible = false;
                    ShoppingCart.InvoiceOption = _invoiceOptionRBL.Items[0].Value;
                }

                if (_invoiceOptionRBL.Items.Count > 0)
                {
                    ListItem itemSelected = _invoiceOptionRBL.Items.FindByValue(ShoppingCart.InvoiceOption);
                    if (itemSelected != null)
                    {
                        _invoiceOptionRBL.ClearSelection();
                        _invoiceOptionRBL.Items.FindByValue(ShoppingCart.InvoiceOption).Selected = true;
                        lblInvoiceOptions.Text = itemSelected.Text;
                    }
                }
            }
            //}
        }

        private void populateInvoiceOptions()
        {
            if (_shoppingCart != null && _shoppingCart.DeliveryInfo != null)
            {
                //divInvoiceOptions
                if (_invoiceOptionRBL != null)
                {
                    divInvoiceOptions.Controls.Remove(_invoiceOptionRBL);
                }
                _invoiceOptionRBL = new RadioButtonList();
                _invoiceOptionRBL.EnableViewState = true;
                ListItemCollection result = new ListItemCollection();

                CatalogItemList catItems = CatalogProvider.GetCatalogItems((from c in _shoppingCart.CartItems
                                                                            select c.SKU.Trim()).ToList<string>(),
                                                                           (this.Page as ProductsBase).CountryCode);
                var list = new List<CatalogItem_V01>();
                list.AddRange(catItems.Select(c => c.Value as CatalogItem_V01).ToList<CatalogItem_V01>());
                var invoiceOptions = new List<InvoiceHandlingType>();
                invoiceOptions = ShippingProvider.GetInvoiceOptions(_shoppingCart.DeliveryInfo.Address, list,
                                                                    this.ShoppingCart);
                //CR for Argies and MX
                if (!HLConfigManager.Configurations.CheckoutConfiguration.AlwaysDisplayInvoiceOption &&
                    (null == invoiceOptions || invoiceOptions.Count == 0))
                {
                    this.Visible = false;
                    _invoiceOptionRBL = null;
                    return;
                }

                var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("InvoiceOptions");

                foreach (var entry in entries)
                {
                    var key = entry.Key;
                    var parts = key.Split('_');
                    if (parts.Length > 1)
                    {
                        key = parts[0];
                    }
                    var value = entry.Value;
                    if (result.FindByValue(key) == null)
                    {
                        if (invoiceOptions != null)
                        {
                            if (
                                invoiceOptions.Exists(
                                    c => c == (InvoiceHandlingType) Enum.Parse(typeof (InvoiceHandlingType), key)))
                            {
                                var newItem = new ListItem(value, key);
                                if (_invoiceOptionRBL.Items.FindByValue(key) == null)
                                {
                                    _invoiceOptionRBL.Items.Add(newItem);
                                }
                            }
                        }
                        else
                        {
                            _invoiceOptionRBL.Items.Add(new ListItem(value, key));
                        }
                    }
                }
                if (_invoiceOptionRBL.Items.Count > 0)
                {
                    if (!HLConfigManager.Configurations.CheckoutConfiguration.DisableDefaultInvoiceOption)
                    {
                        ListItem defaultOption = null;
                        if (!string.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.DefaultInvoiceOption))
                        {
                            defaultOption =
                                _invoiceOptionRBL.Items.FindByValue(
                                    HLConfigManager.Configurations.CheckoutConfiguration.DefaultInvoiceOption);
                        }
                        if (defaultOption == null && _shoppingCart.Locale != "zh-TW")
                        {
                            // Selecting the first from Shipping rules.
                            defaultOption = _invoiceOptionRBL.Items.FindByValue(invoiceOptions[0].ToString());
                        }
                        if (defaultOption != null)
                        {
                            defaultOption.Selected = true;
                        }
                        else
                        {
                            if(_shoppingCart.Locale == "zh-TW" && _invoiceOptionRBL.Items.Count >1)
                                _invoiceOptionRBL.Items[1].Selected = true;
                            else
                            _invoiceOptionRBL.Items[0].Selected = true;
                        }
                    }
                }

                if (_invoiceOptionRBL.Items.Count > 0)
                {
                    _invoiceOptionRBL.AutoPostBack = true;
                    _invoiceOptionRBL.SelectedIndexChanged += new EventHandler(_invoiceOptionRBL_SelectedIndexChanged);
                }

                divInvoiceOptions.Controls.Add(_invoiceOptionRBL);
            }
        }

        private void _invoiceOptionRBL_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShoppingCart.InvoiceOption = _invoiceOptionRBL.SelectedValue;
        }
    }
}