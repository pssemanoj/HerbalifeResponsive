using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders.Invoices;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
	public partial class InvoiceInfo : MyHerbalife3.Shared.UI.UserControlBase
	{
		//shan - mar 12, 2012 - get the invoice date control
		public Telerik.Web.UI.RadDatePicker InvoiceDatePicker
		{
			get { return this.rdpInvoiceDate; }
		}

		#region Public Methods

		public enum ControlState
		{
			Edit,
			Read
		}

		public void SetRenderMode(ControlState controlState)
		{
			try
			{
				var isReadOnly = (controlState == ControlState.Read);
				//Read only mode
				rdpInvoiceDate.Visible = !isReadOnly;
				txtInvoiceNumber.Visible = false;
				ltInvoiceDate.Visible = isReadOnly;
				ltInvoiceNumber.Visible = true;
				trNameEdit.Visible = !isReadOnly;
				trName.Visible = isReadOnly;
				txtAddress.Visible = !isReadOnly;
				ltAddress.Visible = isReadOnly;
				txtCity.Visible = !isReadOnly;
				ltCityState.Visible = isReadOnly;
				ddlState.Visible = this.txtZip.Visible = this.ltZipDash.Visible = this.txtZip2.Visible = !isReadOnly;

				txtPhone.Visible = !isReadOnly;
				ltPhone.Visible = isReadOnly;
				txtEmail.Visible = !isReadOnly;
				ltEmail.Visible = isReadOnly;
				rbCustomer.Visible = this.rbDistributor.Visible = !isReadOnly;
				ltInvoiceType.Visible = isReadOnly;
				ltLoyaltyCurrency.Visible = this.txtLoyaltyAmount.Visible = this.ltLoyaltyOr.Visible = this.txtLoyaltyPercentage.Visible = this.ltLoyaltyPercentage.Visible = btnApplyLoyalty.Visible = !isReadOnly;
				trNewItem.Visible = !isReadOnly;
				btnClear.Visible = btnContinue.Visible = !isReadOnly;
                if (controlState == ControlState.Read)
                {
                    var invoice = Session["Invoice"] as Invoice;
                    var txtPostal = String.IsNullOrEmpty(txtZip.Text) ? "" : "/" + txtZip.Text;
                    var txtPostal2 = String.IsNullOrEmpty(txtZip2.Text) ? "" : "-" + txtZip2.Text;
                    var txtsate = String.IsNullOrEmpty(invoice.State) ? "" : "/" + invoice.State;
                    ltCityState.Visible = false;
                    ltCityEdit.Visible = true;                    
                    ltCityEdit.Text = invoice.City + txtsate + txtPostal + txtPostal2;
                    
                }
                else
                {
                    ltCityEdit.Visible = false;
                }
				foreach (RepeaterItem item in rptProductItems.Items)
				{
					var lnkDeleteTemplate = item.FindControl("lnkDeleteTemplate") as LinkButton;
					lnkDeleteTemplate.Visible = !isReadOnly;

					var txtQty = item.FindControl("txtQty") as TextBox;
					txtQty.Visible = !isReadOnly;

					var ltQty = item.FindControl("ltQty") as Literal;
					ltQty.Visible = isReadOnly;

					var txtTotalPriceTemplate = item.FindControl("txtTotalPriceTemplate") as TextBox;
					txtTotalPriceTemplate.Visible = !isReadOnly;

					var ltTotalPriceTemplate = item.FindControl("ltTotalPriceTemplate") as Literal;
					ltTotalPriceTemplate.Visible = isReadOnly;

				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		public void Save()
		{

		}

		public void SetInvoiceDistributorNumber(string distributorID)
		{
			//shan - mar 14, 2012 - to set invoice number formatted to six digits
			//ltInvoiceNumber.Text = InvoiceProvider.GetNextDistributorInvoiceNumber(distributorID).ToString();
			ltInvoiceNumber.Text = InvoiceProvider.GetNextDistributorInvoiceNumber(distributorID).ToString("000000");
		}

		public void BindData(Invoice invoice, string actionMode)
		{
			try
			{
				//Edit mode, load data from DB
				//begin - shan - mar 08, 2012 - check if the date exists before setting to date pciker control
				//rdpInvoiceDate.SelectedDate = invoice.CreatedDate;
                rdpInvoiceDate.SelectedDate =
                    DateTime.MinValue == invoice.InvoiceDate ? DateTime.Today : invoice.InvoiceDate;
                rdpInvoiceDate.SelectedDate = actionMode == "Copy" ? DateTime.Today : rdpInvoiceDate.SelectedDate;
				//end
				ltInvoiceDate.Text = rdpInvoiceDate.SelectedDate != null ? rdpInvoiceDate.SelectedDate.Value.ToString("MM/dd/yyyy") : string.Empty;
				txtInvoiceNumber.Text = (string.IsNullOrEmpty(actionMode) || actionMode == "Edit") ? invoice.DistributorInvoiceNumber.ToString() :
					InvoiceProvider.GetNextDistributorInvoiceNumber(invoice.DistributorID).ToString();
				//shan - mar 14, 2012 - to set invoice number formatted to six digits
				//ltInvoiceNumber.Text = (string.IsNullOrEmpty(actionMode) || actionMode == "Edit") ? invoice.DistributorInvoiceNumber.ToString() :
				//    InvoiceProvider.GetNextDistributorInvoiceNumber(invoice.DistributorID).ToString();
				ltInvoiceNumber.Text = (string.IsNullOrEmpty(actionMode) || actionMode == "Edit") ? invoice.DistributorInvoiceNumber.ToString("000000") :
					InvoiceProvider.GetNextDistributorInvoiceNumber(invoice.DistributorID).ToString("000000");
				txtFirstName.Text = invoice.FirstName;
				txtLastName.Text = invoice.LastName;
				ltName.Text = invoice.FirstName + " " + invoice.LastName;
				txtAddress.Text = invoice.Address1+" "+ invoice.Address2;
				ltAddress.Text = invoice.Address1 + " " + invoice.Address2;
				
				txtCity.Text = invoice.City;
				if (null != ddlState && null != ddlState.Items && ddlState.Items.Count == 0)
				{
					BindStatesReference();
				}
                ddlState.SelectedIndex = ddlState.Items.IndexOf(ddlState.Items.FindByValue(invoice.State));
				
                
                if (!String.IsNullOrEmpty(invoice.PostalCode))
                {    
                var arrPostalCodes = invoice.PostalCode.Split('-');
                txtZip.Text = arrPostalCodes[0];
				    if(arrPostalCodes.Length > 1)
					    txtZip2.Text = arrPostalCodes[1];
				    else
					    txtZip2.Text = string.Empty;                
                }
                else
                  {
                      txtZip.Text = string.Empty;
                      txtZip2.Text = string.Empty;
                }
				var txtPostal = String.IsNullOrEmpty(txtZip.Text) ? "" : " / " + txtZip.Text;
				ltCityState.Text = invoice.City;
				txtCity.Text = invoice.City;
				txtPhone.Text = invoice.PhoneNumber;
				ltPhone.Text = invoice.PhoneNumber;

				txtEmail.Text = invoice.Email;
				ltEmail.Text = invoice.Email;

				if (invoice.Type == InvoiceType.customer)
				{
					rbCustomer.Checked = true;
					rbDistributor.Checked = false;
				}
				else
				{
					rbCustomer.Checked = false;
					rbDistributor.Checked = true;
				}

				ltInvoiceType.Text = invoice.Type.ToString();

				ltSubtotalTotalVolumePoints.Text = invoice.TotalVolumePoints.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

				decimal totalAmount = 0;
				foreach (var sku in invoice.InvoiceSkus)
				{
					totalAmount = totalAmount + sku.TotalPrice;
				}

				ltSubtotalTotalPrice.Text = totalAmount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

				//shan - set value only if it is > 0
				//txtLoyaltyAmount.Text = invoice.DiscountedAmount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
				//txtLoyaltyPercentage.Text = invoice.DiscountPercentage.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
				txtLoyaltyAmount.Text = invoice.DiscountedAmount > 0 ?
					invoice.DiscountedAmount.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
				txtLoyaltyPercentage.Text = invoice.DiscountPercentage > 0 ? 
					invoice.DiscountPercentage.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;

				ltDiscount.Text =invoice.CustomerDiscount.ToString();
				ltNetSubtotal.Text = (totalAmount - invoice.CustomerDiscount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));

				//shan - mar 14, 2012 - prefix customer loyalty program with the percentage in view mode, if exists..
				ltLoyaltyCaption.Text =
					(string.IsNullOrEmpty(actionMode) &&
					null != invoice &&
					0 < invoice.DiscountPercentage) ?
					invoice.DiscountPercentage.ToString("N2", CultureInfo.GetCultureInfo("en-US")) + "%" +
					" " + GetLocalResourceString("ltLoyaltyCaption.Text", "Customer Loyalty Program:") :
					GetLocalResourceString("ltLoyaltyCaption.Text", "Customer Loyalty Program:");
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		//shan - mar 14, 2012 - to set default values on coming to the page for the first time
		public void SetDefaultValues()
		{
			//set date to current date
			rdpInvoiceDate.SelectedDate = DateTime.Today;
			//set 0 to vp and cost fields
			decimal defaultValue = 0;
			ltSubtotalTotalVolumePoints.Text = defaultValue.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
			ltSubtotalTotalPrice.Text =
				ltDiscount.Text =
				ltNetSubtotal.Text = defaultValue.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
		}

		public void BindInvoiceSkuGrid()
		{
			var skus = GetSkus();
			BindInvoiceSkuGrid(skus);
		}

		public void BindInvoiceSkuGrid(List<InvoiceSKU> skus)
		{
			try
			{
				decimal totalVolumePoints = 0;
				decimal totalPrice = 0;

				rptProductItems.DataSource = skus;
				rptProductItems.DataBind();

				foreach (var sku in skus)
				{
					totalVolumePoints = totalVolumePoints + sku.TotalVolumePoints;
					totalPrice = totalPrice + sku.TotalPrice;
				}

				ltSubtotalTotalVolumePoints.Text = totalVolumePoints.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
				ltSubtotalTotalPrice.Text = totalPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

				decimal loyaltyAmount = 0;
				decimal.TryParse(txtLoyaltyAmount.Text, out loyaltyAmount);
				decimal loyaltyPercentage = 0;
				decimal.TryParse(txtLoyaltyPercentage.Text, out loyaltyPercentage);
				decimal discount = 0;

				discount = loyaltyAmount == 0 ? Math.Round(totalPrice * (loyaltyPercentage / 100), 2) : loyaltyAmount;
				ltDiscount.Text = discount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
				ltNetSubtotal.Text = (totalPrice - discount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
				Session["InvoiceSKUs"] = skus;
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}		

		public bool AddSku(string sku, int quantity)
		{
			try
			{
				var catalogItem = OrderProvider.GetInvoiceSkuDetails(sku);
				if (catalogItem == null)
				{
					return false;
				}
				quantity = (quantity <= 0) ? 1 : quantity;
				var skus = GetSkus();
				var skuItem = new InvoiceSKU
				 {
					 //shan - mar 07, 2012 - set temp id for sku as it will help in identifying which row
					 //has been modified to update the qty or total price in case of having multiple rows of same sku
					 ID = GetSkus().Count + 1,
					 SKU = sku,
					 Quantity = quantity,
					 //begin - shan - mar 14, 2012 - get the full description of the product
					 //Description = catalogItem.Description,
					 //the catalogItem object didnt have the full description
					 //get the ProductInfo object from allSKUs list and get the full name
					 //allSKUs should be available in the cache already..
					 Description = OrderProvider.GetProductDescription(sku),
					 //end
					 UnitTotalPrice = catalogItem.ListPrice,
					 UnitVolumePoints = catalogItem.VolumePoints,
					 TotalVolumePoints = catalogItem.VolumePoints * quantity,
					 TotalPrice = quantity * catalogItem.ListPrice
				 };
				//TODO: some process here to fill in the rest of data for SKU

				skus.Add(skuItem);
				SaveSkus(skus);
				return true;
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
				return false;
			}
		}

		public bool SaveSkus(List<InvoiceSKU> skus)
		{
			try
			{
				Session["InvoiceSKUs"] = skus;
				return true;
			}
			catch
			{
				return false;
			}

		}

		public Invoice GetInvoiceForm()
		{
			var invoice = new Invoice();
			try
			{
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                invoice.DistributorID = member.Value.Id;
				if (rdpInvoiceDate.SelectedDate != null) invoice.CreatedDate = rdpInvoiceDate.SelectedDate.Value;
				//begin - shan - mar 09, 2012 - set invoice date to current date if not available
				invoice.CreatedDate = DateTime.MinValue == invoice.CreatedDate ? DateTime.Today : invoice.CreatedDate;
				invoice.InvoiceDate = invoice.CreatedDate;
				//end
				long invoiceNumber = 0;
				long.TryParse(ltInvoiceNumber.Text, out invoiceNumber);
				invoice.DistributorInvoiceNumber = invoiceNumber;
				invoice.ID = InvoiceProvider.GetInvoiceIDFromDistributorInvoiceNumber(invoice.DistributorID, invoiceNumber);
				invoice.ContactInfoID = InvoiceProvider.GetInvoiceContactInfoID(invoice.DistributorID, invoice.ID);
				invoice.FirstName = txtFirstName.Text;
				invoice.LastName = txtLastName.Text;
				invoice.Address1 = txtAddress.Text;
				invoice.City = txtCity.Text;
				invoice.State = ddlState.SelectedValue;
				invoice.PostalCode = txtZip.Text + "-" + txtZip2.Text;
				invoice.Country = "US";
				invoice.PhoneNumber = txtPhone.Text;
				invoice.Email = txtEmail.Text;
				invoice.Type = rbCustomer.Checked ? InvoiceType.customer : InvoiceType.distributor;
				decimal subtotalVolumePoints = 0;
				decimal.TryParse(ltSubtotalTotalVolumePoints.Text.Trim(), out subtotalVolumePoints);
				invoice.TotalVolumePoints = subtotalVolumePoints;
				decimal discountAmount = 0;
				decimal.TryParse(txtLoyaltyAmount.Text, out discountAmount);
				invoice.DiscountedAmount = discountAmount;
				decimal discountPercentage = 0;
				decimal.TryParse(txtLoyaltyPercentage.Text, out discountPercentage);
				invoice.DiscountPercentage = discountPercentage;
				decimal customerDiscount = 0;
				decimal.TryParse(ltDiscount.Text, out customerDiscount);
				invoice.CustomerDiscount = Math.Abs(customerDiscount);
				invoice.InvoiceSkus = GetSkus();
				decimal totalPrice = 0;
				if (null != invoice.InvoiceSkus)
				{
					foreach (var sku in invoice.InvoiceSkus)
					{
						totalPrice = totalPrice + sku.TotalPrice;
					}
				}
				//shan - to retain the invoice values
				//if the invoice exists in session get the tax and shipping fields
				//while redirecting to the same page again those fields shouldn't get lost
				if (null != Session["Invoice"])
				{
					var invoiceExisting = Session["Invoice"] as Invoice;
					if (null != invoiceExisting)
					{
						//set the values to the new object
						invoice.TaxAmount = invoiceExisting.TaxAmount;
						invoice.TaxPercentage = invoiceExisting.TaxPercentage;
						invoice.ShippingAmount = invoiceExisting.ShippingAmount;
						invoice.ShippingPercentage = invoiceExisting.ShippingPercentage;
						invoice.Status = invoiceExisting.Status;
                        invoice.Notes = invoiceExisting.Notes;
                        invoice.PaymentAddress = invoiceExisting.PaymentAddress;
					}
				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
			return invoice;
		}

		public List<InvoiceSKU> UpdateSku(string sku, int quantity, decimal totalPrice, bool overrideTotalPrice)
		{
			var skus = GetSkus();
			//create data table
			if (quantity <= 0)
			{
				//remove if zero quantity in collection
				skus = skus.Where(p => p.SKU != sku).ToList();
			}
			else
			{
				try
				{
					//shan - mar 07, 2012 - get the item by matching with ID field
					//var invoiceSku = skus.SingleOrDefault(p => p.SKU == sku);
					var invoiceSku = skus.FirstOrDefault(p => p.ID.ToString() == sku);
					invoiceSku.Quantity = quantity;
					invoiceSku.TotalVolumePoints = invoiceSku.UnitVolumePoints * invoiceSku.Quantity;
					invoiceSku.TotalPrice = (overrideTotalPrice) ? totalPrice : (invoiceSku.UnitTotalPrice * invoiceSku.Quantity);
				}
				catch (Exception ex)
				{
					MyHLWebUtil.LogExceptionWithContext(ex);
				}
			}

			SaveSkus(skus);
			return skus;
		}

		public List<InvoiceSKU> DeleteSku(string sku)
		{
			var skus = GetSkus();
			//shan - mar 07, 2012 - delete sku based on the ID
			//skus = skus.Where(p => p.SKU != sku).ToList();
			skus = skus.Where(p => p.ID.ToString() != sku).ToList();
			SaveSkus(skus);
			return skus;
		}

		public List<InvoiceSKU> GetSkus()
		{
			List<InvoiceSKU> skus;
			try
			{
				if (Session["InvoiceSKUs"] == null)
				{
					skus = new List<InvoiceSKU>();
				}
				else
				{
					skus = Session["InvoiceSKUs"] as List<InvoiceSKU>;
				}
			}
			catch
			{
				skus = new List<InvoiceSKU>();
			}

			return skus;
		}	
		#endregion

		#region Control Events

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			btnApplyLoyalty.Click += new EventHandler(btnApplyLoyalty_Click);
			btnContinue.Click += new EventHandler(btnContinue_Click);
			rptProductItems.ItemCreated += new RepeaterItemEventHandler(rptProductItems_ItemCreated);
			rptProductItems.ItemDataBound += new RepeaterItemEventHandler(rptProductItems_ItemDataBound);
			rptProductItems.ItemCommand += new RepeaterCommandEventHandler(rptProductItems_ItemCommand);
			txtNewSku.TextChanged += new EventHandler(txtNewSku_TextChanged);

		}

		protected void Page_Load(object sender, EventArgs e)
		{
			invCatalog.onAddToInvoiceClick += invCatalog_onAddToInvoiceClick;
			invContact.onAddToContactClick += invContact_onAddToContactClick;
			if (!Page.IsPostBack)
			{
                if(ddlState.Items.Count == 0)
				    BindStatesReference();
			}
		}

		protected void invContact_onAddToContactClick(object sender, EventArgs e)
		{
			try
			{
				var command = e as CommandEventArgs;
				if (command != null)
				{
					switch (command.CommandName)
					{
						case "InvoiceContact":
							{
								var invoiceContact = command.CommandArgument as Contact_V01;
								BindContactValues(invoiceContact);
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}		

		protected void txtNewSku_TextChanged(object sender, EventArgs e)
		{
			AddSku(txtNewSku.Text.Trim(), 1);
			txtNewSku.Text = string.Empty;
			txtNewQty.Text = string.Empty;
			BindInvoiceSkuGrid();
		}

		protected void invCatalog_onAddToInvoiceClick(object sender, EventArgs e)
		{
			try
			{
				var command = e as CommandEventArgs;
				if (command != null)
				{
					switch (command.CommandName)
					{
						case "InvoiceSKUs":
							{
								var invoiceSkus = command.CommandArgument as List<InvoiceSKU>;
								foreach (var invoiceSku in invoiceSkus)
								{
									AddSku(invoiceSku.SKU, Convert.ToInt32(invoiceSku.Quantity));
									BindInvoiceSkuGrid();
									updRptProductItems.Update();
								}
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		protected void btnApplyLoyalty_Click(object sender, EventArgs e)
		{
			//begin - shan - mar 06, 2012 - validate fields on applying loyalty
			//BindInvoiceSkuGrid();
			//check if valid amount or percentage has been entered
			string errMsg = this.ValidateLoyaltyFields(true);
			//update err msg
			this.lblErrMsg.Text = errMsg;
			this.imgErrLoyalty.Visible = !string.IsNullOrEmpty(errMsg);
			//if no err msg continue..
			if (string.IsNullOrEmpty(errMsg))
			{
				BindInvoiceSkuGrid();
			}
			//end
		}			

		protected void btnContinue_Click(object sender, EventArgs e)
		{
			//shan - mar 13, 2012 - before continuing validate for error fields
			string errMsg = this.ValidateLoyaltyFields(false);
			this.lblErrMsg.Text = errMsg;
			this.imgErrLoyalty.Visible = !string.IsNullOrEmpty(errMsg);
			if (string.IsNullOrEmpty(errMsg))
			{
				//update the total based on the values entered
				BindInvoiceSkuGrid();
				var invoice = GetInvoiceForm();
				Session["Invoice"] = invoice;
				Session["InvoiceStep2"] = true;
				Response.Redirect(Request.RawUrl);
			}
		}

		protected void btnClear_Click(object sender, EventArgs e)
		{
			this.ClearFields();
		}		

		protected void rptProductItems_ItemCreated(object sender, RepeaterItemEventArgs e)
		{
			switch (e.Item.ItemType)
			{
				case ListItemType.Item:
				case ListItemType.AlternatingItem:
					var txtQty = e.Item.FindControl("txtQty") as TextBox;
					txtQty.TextChanged += txtQty_TextChanged;
					var txtTotalPriceTemplate = e.Item.FindControl("txtTotalPriceTemplate") as TextBox;
					txtTotalPriceTemplate.TextChanged += txtTotalPriceTemplate_TextChanged;
					break;
			}
		}

		protected void rptProductItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			try
			{
				switch (e.Item.ItemType)
				{
					case ListItemType.Item:
					case ListItemType.AlternatingItem:
						var itemSku = e.Item.DataItem as InvoiceSKU;

						var ltSkuTemplate = e.Item.FindControl("ltSkuTemplate") as Literal;
						ltSkuTemplate.Text = itemSku.SKU;

						//shan - mar 07, 2012 - set item id
						var hdnItemID = e.Item.FindControl("hdnItemID") as HiddenField;
						hdnItemID.Value = itemSku.ID.ToString();

						var ltProductionDescriptionTemplate = e.Item.FindControl("ltProductionDescriptionTemplate") as Literal;
						ltProductionDescriptionTemplate.Text = itemSku.Description;

						var lnkDeleteTemplate = e.Item.FindControl("lnkDeleteTemplate") as LinkButton;
						lnkDeleteTemplate.CommandName = "delete";
						//shan - mar 07, 2012 - set id as command argument,
						//as deleting by sku will delete all the other rows with the same sku
						//lnkDeleteTemplate.CommandArgument = itemSku.SKU;
						lnkDeleteTemplate.CommandArgument = itemSku.ID.ToString();

						var txtQty = e.Item.FindControl("txtQty") as TextBox;
						txtQty.Text = itemSku.Quantity.ToString();

						var ltQty = e.Item.FindControl("ltQty") as Literal;
						ltQty.Text = itemSku.Quantity.ToString();
						ltQty.Visible = false;

						var ltRetailPriceTemplate = e.Item.FindControl("ltRetailPriceTemplate") as Literal;
						ltRetailPriceTemplate.Text = itemSku.UnitTotalPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

						var ltTotalVolPointsTemplate = e.Item.FindControl("ltTotalVolPointsTemplate") as Literal;
						ltTotalVolPointsTemplate.Text = itemSku.TotalVolumePoints.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

						var txtTotalPriceTemplate = e.Item.FindControl("txtTotalPriceTemplate") as TextBox;
						txtTotalPriceTemplate.Text = itemSku.TotalPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

						var ltTotalPriceTemplate = e.Item.FindControl("ltTotalPriceTemplate") as Literal;
						ltTotalPriceTemplate.Text = itemSku.TotalPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
						ltTotalPriceTemplate.Visible = false;
						break;
				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		protected void txtQty_TextChanged(object sender, EventArgs e)
		{
			try
			{
				var txtQty = (TextBox)sender;
				var quantity = 0;
				int.TryParse(txtQty.Text, out quantity);
				quantity = (quantity <= 0) ? 1 : quantity;
				txtQty.Text = quantity.ToString();
				var repeaterItem = ((RepeaterItem)(txtQty.NamingContainer));
				var i = repeaterItem.ItemIndex;
				var ltSkuTemplate = (Literal)repeaterItem.FindControl("ltSkuTemplate");
				//begin - shan - mar 07, 2012 - get the item by matchin with the ID field
				//var itemSku = GetSkus().SingleOrDefault(p => p.SKU == ltSkuTemplate.Text);
				var hdnItemID = (HiddenField)repeaterItem.FindControl("hdnItemID");
				var itemSku = GetSkus().FirstOrDefault(p => p.ID.ToString() == hdnItemID.Value);
				//UpdateSku(itemSku.SKU, quantity, itemSku.UnitTotalPrice * quantity, false);
				UpdateSku(itemSku.ID.ToString(), quantity, itemSku.UnitTotalPrice * quantity, false);
				//end
				BindInvoiceSkuGrid();
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		protected void txtTotalPriceTemplate_TextChanged(object sender, EventArgs e)
		{
			try
			{
				var txtTotalPrice = (TextBox)sender;
				var repeaterItem = ((RepeaterItem)(txtTotalPrice.NamingContainer));
				var i = repeaterItem.ItemIndex;
				var txtQty = (TextBox)repeaterItem.FindControl("txtQty");
				var quantity = 0;
				int.TryParse(txtQty.Text, out quantity);
				quantity = (quantity <= 0) ? 1 : quantity;
				txtQty.Text = quantity.ToString();
				var ltSkuTemplate = (Literal)repeaterItem.FindControl("ltSkuTemplate");
				//begin - shan - mar 07, 2012 use firstOrdefault as
				//singleOrdefault will throw error when multiple rows with same sku available
				//var itemSku = GetSkus().SingleOrDefault(p => p.SKU == ltSkuTemplate.Text);
				var hdnItemID = (HiddenField)repeaterItem.FindControl("hdnItemID");
				var itemSku = GetSkus().FirstOrDefault(p => p.ID.ToString() == hdnItemID.Value);
				//end
				decimal totalPrice = 0;
				decimal.TryParse(txtTotalPrice.Text, out totalPrice);
				totalPrice = (totalPrice <= 0) ? 0 : totalPrice;
				//begin - shan - mar 06, 2012 - show err msg if total price exceeds qty * unit price
				//UpdateSku(itemSku.SKU, quantity, totalPrice, true);
				//BindInvoiceSkuGrid();
				decimal unitPrice = 0;
				unitPrice = decimal.TryParse(((Literal)repeaterItem.FindControl("ltRetailPriceTemplate")).Text, out unitPrice) ? unitPrice : 0;
				//if total price < 0, set it to unit price * qunatity
				totalPrice = (totalPrice <= 0) ?  unitPrice * quantity : totalPrice;
				//check if it is greater than unit price
				if (totalPrice > quantity * unitPrice)
				{
					lblErrMsg.Text = this.GetLocalResourceString("errMsg.PriceCannotExceedRetailPrice", "Your Price cannot exceed Retail Price");
					txtTotalPrice.Text = (quantity * unitPrice).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
					((System.Web.UI.HtmlControls.HtmlImage)repeaterItem.FindControl("imgErrPrice")).Visible = true;
				}
				else
				{
					lblErrMsg.Text = string.Empty;
					((System.Web.UI.HtmlControls.HtmlImage)repeaterItem.FindControl("imgErrPrice")).Visible = false;
					//shan - mar 07, 2012 - pass ID to get the sku item
					//UpdateSku(itemSku.SKU, quantity, totalPrice, true);
					UpdateSku(itemSku.ID.ToString(), quantity, totalPrice, true);
					BindInvoiceSkuGrid();
				}
				//end
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		protected void rptProductItems_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "delete":
					DeleteSku(e.CommandArgument.ToString());
					BindInvoiceSkuGrid();
					break;
			}
		}
		
		#endregion

		#region Private Methods

		private void BindStatesReference()
		{
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("UsaStates", new CultureInfo("en-US"));

			ddlState.Items.Insert(0, new ListItem(string.Empty, string.Empty));

            foreach (var item in entries)
		    {
		        ddlState.Items.Add(new ListItem {Text = item.Key, Value = item.Key});
		    }


			ddlState.SelectedIndex = 0;
		}

		private void BindContactValues(Contact_V01 invoiceContact)
		{
			try
			{
				if (null != invoiceContact)
				{
					if (null != invoiceContact.EnglishName)
					{
						txtFirstName.Text = invoiceContact.EnglishName.First;
						txtLastName.Text = invoiceContact.EnglishName.Last;
						ltName.Text = invoiceContact.EnglishName.First + " " + invoiceContact.EnglishName.Last;
					}

					if (null != invoiceContact.PrimaryAddress)
					{
						txtAddress.Text = invoiceContact.PrimaryAddress.Line1 + " " + invoiceContact.PrimaryAddress.Line2;
						ltAddress.Text = invoiceContact.PrimaryAddress.Line1;
						txtCity.Text = invoiceContact.PrimaryAddress.City;

						if (null != ddlState && null != ddlState.Items && ddlState.Items.Count == 0)
						{
							BindStatesReference();

						}
						ddlState.SelectedIndex =
							   ddlState.Items.IndexOf(
								   ddlState.Items.FindByValue(invoiceContact.PrimaryAddress.StateProvinceTerritory));
						ltCityState.Text = invoiceContact.PrimaryAddress.City + ", " + invoiceContact.PrimaryAddress.StateProvinceTerritory
							+ " " + invoiceContact.PrimaryAddress.PostalCode;
						var arrPostalCodes = invoiceContact.PrimaryAddress.PostalCode.Split('-');
						txtZip.Text = arrPostalCodes[0];
						//shan - mar 07, 2012 - reset zip2 field before getting the value
						txtZip2.Text = string.Empty;
						if (arrPostalCodes.Length == 2)
							txtZip2.Text = arrPostalCodes[1];
					}

					//shan - mar 07, 2012 - reset phone field before getting the value
					txtPhone.Text =
						ltPhone.Text = string.Empty;
					if (null != invoiceContact.Phones && invoiceContact.Phones.Count > 0)
					{
						var primaryPhones = invoiceContact.Phones.Where(p => p.IsPrimary);
						if (null != primaryPhones && primaryPhones.Count() > 0 && null != primaryPhones.First())
						{
							txtPhone.Text = primaryPhones.First().Number;
							ltPhone.Text = primaryPhones.First().Number;
						}
						//To retrieve secondary phone when Primary phone is null.
						if (string.IsNullOrEmpty(txtPhone.Text) && string.IsNullOrEmpty(ltPhone.Text))
						{
							var secondaryPhones = invoiceContact.Phones.Where(p => !p.IsPrimary);
							if (null != secondaryPhones && secondaryPhones.Count() > 0 && null != secondaryPhones.First())
							{
								txtPhone.Text = secondaryPhones.First().Number;
								ltPhone.Text = secondaryPhones.First().Number;
							}
						}

					}

					//shan - mar 07, 2012 - reset email field before getting the value
					txtEmail.Text =
						ltEmail.Text = string.Empty;
					if (null != invoiceContact.EmailAddresses && invoiceContact.EmailAddresses.Count > 0)
					{
						var primaryEmails = invoiceContact.EmailAddresses.Where(e => e.IsPrimary);
						if (null != primaryEmails && primaryEmails.Count() > 0 &&
							null != primaryEmails.First() && null != primaryEmails.First().Address)
						{
							txtEmail.Text = primaryEmails.First().Address;
							ltEmail.Text = primaryEmails.First().Address;
						}
						//To retrieve secondary email when Primary email is null.
						if (string.IsNullOrEmpty(txtEmail.Text) && string.IsNullOrEmpty(ltEmail.Text))
						{
							var secondaryEmails = invoiceContact.EmailAddresses.Where(e => !e.IsPrimary);
							if (null != secondaryEmails && secondaryEmails.Count() > 0 &&
						 null != secondaryEmails.First() && null != secondaryEmails.First().Address)
							{
								txtEmail.Text = secondaryEmails.First().Address;
								ltEmail.Text = secondaryEmails.First().Address;
							}
						}

					}
					if (invoiceContact.ContactType.ToString() == "DS")
					{
						rbDistributor.Checked = true;
						rbCustomer.Checked = false;
					}
					else
					{
						rbDistributor.Checked = false;
						rbCustomer.Checked = true;
					}
					updInvoiceInfo.Update();
				}
			}
			catch (Exception ex)
			{
				MyHLWebUtil.LogExceptionWithContext(ex);
			}
		}

		private void ClearFields()
		{
			//date - set to current date
			rdpInvoiceDate.SelectedDate = DateTime.Today;
			//name
			txtFirstName.Text =
				txtLastName.Text = string.Empty;
			//address
			txtAddress.Text =
				txtCity.Text =
				txtZip.Text =
				txtZip2.Text = string.Empty;
			ddlState.SelectedIndex = -1;
			//contact details
			txtPhone.Text =
				txtEmail.Text = string.Empty;
			//invoice type
			rbCustomer.Checked = true;
			rbDistributor.Checked = false;
			//skus
			rptProductItems.DataSource = null;
			rptProductItems.DataBind();
			//loyalty fields
			txtLoyaltyAmount.Text =
				txtLoyaltyPercentage.Text = string.Empty;
			//subtotals
			ltSubtotalTotalVolumePoints.Text =
				ltSubtotalTotalPrice.Text = "0.00";
			//discount and total
			ltDiscount.Text =
				ltNetSubtotal.Text = "0.00";
			//hide error labels
			lblErrMsg.Text = string.Empty;
			imgErrLoyalty.Visible = false;
			//clear session values
			Session["InvoiceSKUs"] = null;
		}

		private string ValidateLoyaltyFields(bool isApply)
		{
			string errMsg = string.Empty;
			//check if both loyalty and percentage are entered
			if (!string.IsNullOrEmpty(txtLoyaltyAmount.Text.Trim()) && !string.IsNullOrEmpty(txtLoyaltyPercentage.Text.Trim()))
			{
				errMsg = this.GetLocalResourceString("errMsg.EnterLoyaltyAmountOrPercentage", "Please enter a valid dollar amount OR percentage only");
			}
			//check if valid amount has been entered
			else if (!string.IsNullOrEmpty(txtLoyaltyAmount.Text.Trim()))
			{
				decimal loyaltyAmount = -999;
				loyaltyAmount = decimal.TryParse(txtLoyaltyAmount.Text.Trim(), out loyaltyAmount) ? loyaltyAmount : -999;
				//check if loyalty amount is a valid number
				if (loyaltyAmount < 0)
				{
					errMsg = this.GetLocalResourceString("errMsg.EnterValidLoyaltyAmount", "Please enter a valid discount amount in the following format: 12.95");
				}

				//check if loyalty amount is greater than total retail price
				if (string.IsNullOrEmpty(errMsg))
				{
					decimal totalPrice = 0;
					totalPrice = decimal.TryParse(ltSubtotalTotalPrice.Text.Trim(), out totalPrice) ? totalPrice : 0;
					if (loyaltyAmount > totalPrice)
					{
						errMsg = this.GetLocalResourceString("errMsg.EnterLoyaltyLessThanTotal", "Please enter a valid dollar amount equal to or less than the Total Retail Price");
					}
				}

				//valitate no more than 2 decimals.
				if (string.IsNullOrEmpty(errMsg))
				{
					if (!Regex.IsMatch(loyaltyAmount.ToString(), "^[+]?[0-9]+([.][0-9]{1,2})?$"))
					{
						errMsg = this.GetLocalResourceString("errMsg.EnterValidLoyaltyAmount", "Please enter a valid discount amount in the following format: 12.95");
					}
				}
			}

			//check if valid percentage has been entered
			else if (!string.IsNullOrEmpty(txtLoyaltyPercentage.Text.Trim()))
			{
				decimal loyaltyPercentage = -999;
				loyaltyPercentage = decimal.TryParse(txtLoyaltyPercentage.Text.Trim(), out loyaltyPercentage) ? loyaltyPercentage : -999;
				if (loyaltyPercentage < 0 ||
					loyaltyPercentage > 100)
				{
					errMsg = this.GetLocalResourceString("errMsg.EnterValidLoyaltyPercentage", "Please enter a valid percentage between 0-100");
				}

				//valitate no more than 2 decimals.
				if (string.IsNullOrEmpty(errMsg))
				{
					if (!Regex.IsMatch(loyaltyPercentage.ToString(), "^[+]?[0-9]+([.][0-9]{1,2})?$"))
					{
						errMsg = this.GetLocalResourceString("errMsg.EnterValidLoyaltyAmount", "Please enter a valid percentaje amount in the following format: 12.95");
					}
				}
			}

			if (isApply)
			{
				if (string.IsNullOrEmpty(txtLoyaltyAmount.Text.Trim()) && string.IsNullOrEmpty(txtLoyaltyPercentage.Text.Trim()))
				{
					errMsg = this.GetLocalResourceString("errMsg.EnterLoyaltyAtLeastOne", "Please enter a valid dollar amount or percentage at least");
				}
			}

			return errMsg;
		}

		#endregion
	}
}