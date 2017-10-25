using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Security;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.Bizworks;
using MyHerbalife3.Ordering.SharedProviders.Invoices;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.ViewModel.Models;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class CreateInvoice : Invoicebase
	{
    
		private static Invoice CreateInvoiceFromCustomerOrder(ServiceProvider.CustomerOrderSvc.CustomerOrder_V01 customerOrderV01)
		{
			var invoice = new Invoice();
			try
			{
				invoice.ID = 0;
				invoice.Type = InvoiceType.customer;
				invoice.CreatedDate = DateTime.Now;
				invoice.DistributorID = customerOrderV01.DistributorID;
				if (null != customerOrderV01.Shipping)
				{
					var shippingInfo = (ServiceProvider.CustomerOrderSvc.CustomerShippingInfo_V01)customerOrderV01.Shipping;
					if (null != shippingInfo)
					{
						if (null != shippingInfo.Address)
						{
							invoice.Address1 = shippingInfo.Address.Line1;
							invoice.Address2 = shippingInfo.Address.Line2;
							invoice.City = shippingInfo.Address.City;
							invoice.State = shippingInfo.Address.StateProvinceTerritory;
							invoice.PostalCode = shippingInfo.Address.PostalCode;
						}
						if (null != shippingInfo.Recipient)
						{
							invoice.FirstName = shippingInfo.Recipient.First;
							invoice.LastName = shippingInfo.Recipient.Last;
						}
						invoice.PhoneNumber = shippingInfo.Phone;
					}

					if (null != customerOrderV01.OrderItems && customerOrderV01.OrderItems.Count > 0)
					{
						invoice.InvoiceSkus = new List<InvoiceSKU>();
						foreach (var orderItem in customerOrderV01.OrderItems)
						{
							var catalogItem = OrderProvider.GetInvoiceSkuDetails(orderItem.SKU);
							if (null != catalogItem)
							{
								var skuItem = new InvoiceSKU
								{
									SKU = orderItem.SKU,
									Quantity = orderItem.Quantity,
									Description = catalogItem.Description,
									UnitTotalPrice = catalogItem.ListPrice,
									UnitVolumePoints = catalogItem.VolumePoints,
									TotalVolumePoints = catalogItem.VolumePoints * orderItem.Quantity,
									TotalPrice = orderItem.Quantity * catalogItem.ListPrice
								};
								invoice.InvoiceSkus.Add(skuItem);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
                // TODO: Proper logging
				//MyHLWebUtil.LogExceptionWithContext(ex);
			}
			return invoice;
		}

		private static Invoice CreateInvoiceFromContact(Contact_V01 invoiceContact)
		{
			var invoice = new Invoice();
			try
			{
				if (null != invoiceContact)
				{
					invoice.ID = 0;
					invoice.DistributorID = invoiceContact.DistributorID;
					invoice.CreatedDate = DateTime.Now;
					invoice.InvoiceSkus = new List<InvoiceSKU>();
					if (null != invoiceContact.EnglishName)
					{
						invoice.FirstName = invoiceContact.EnglishName.First;
						invoice.LastName = invoiceContact.EnglishName.Last;
					}

					if (null != invoiceContact.PrimaryAddress)
					{
						invoice.Address1 = invoiceContact.PrimaryAddress.Line1;
                        invoice.Address2 = invoiceContact.PrimaryAddress.Line2;
						invoice.City = invoiceContact.PrimaryAddress.City;
						invoice.State = invoiceContact.PrimaryAddress.StateProvinceTerritory;
						invoice.PostalCode = invoiceContact.PrimaryAddress.PostalCode;
					}

					if (null != invoiceContact.Phones && invoiceContact.Phones.Count > 0)
					{
						var primaryPhones = invoiceContact.Phones.Where(p => p.IsPrimary);

						if (null != primaryPhones && primaryPhones.Count() > 0 && null != primaryPhones.First())
						{
							invoice.PhoneNumber = primaryPhones.First().Number;
						}
						//To retrieve secondary phone when Primary phone is null.
						if (string.IsNullOrEmpty(invoice.PhoneNumber))
						{
							var secondaryPhones = invoiceContact.Phones.Where(p => !p.IsPrimary);

							if (null != secondaryPhones && secondaryPhones.Count() > 0 && null != secondaryPhones.First())
							{
								invoice.PhoneNumber = secondaryPhones.First().Number;
							}
						}

					}

					if (null != invoiceContact.EmailAddresses && invoiceContact.EmailAddresses.Count > 0)
					{
						var primaryEmails = invoiceContact.EmailAddresses.Where(e => e.IsPrimary);

						if (null != primaryEmails && primaryEmails.Count() > 0 &&
							null != primaryEmails.First() && null != primaryEmails.First().Address)
						{
							invoice.Email = primaryEmails.First().Address;
						}
						//To retrieve secondary Email when Primary email is null.
						if (string.IsNullOrEmpty(invoice.Email))
						{
							var secondaryEmail = invoiceContact.EmailAddresses.Where(e => !e.IsPrimary);

							if (null != secondaryEmail && secondaryEmail.Count() > 0 &&
							null != secondaryEmail.First() && null != secondaryEmail.First().Address)
							{
								invoice.Email = secondaryEmail.First().Address;
							}
						}

					}
                    foreach (var phone in invoiceContact.Phones)
                    {
                        if (phone.IsPrimary)
                        {
                            invoice.PhoneNumber = phone.Number;
                        }
                        else if (phone.IsPrimary == false && !String.IsNullOrEmpty(phone.Number))
                        {
                            invoice.PhoneNumber = phone.Number;
                        }
                    }

                    foreach (var emailAddress in invoiceContact.EmailAddresses)
                    {
                        if (emailAddress.IsPrimary)
                        {
                            invoice.Email = emailAddress.Address;
                        }
                        else if (emailAddress.IsPrimary == false && !String.IsNullOrEmpty(emailAddress.Address))
                        {
                            invoice.Email = emailAddress.Address;
                        }
                    }

				}
			}
			catch (Exception ex)
			{
				//MyHLWebUtil.LogExceptionWithContext(ex);
			}
			return invoice;
		}

		private void DisplayEditMode()
		{
			var invoice = Session["Invoice"] as Invoice;
            invoice.InvoiceSkus = ValidSkus(invoice.InvoiceSkus);
			InvoiceInfo1.BindData(invoice, Session["ActionMode"] as string);
			InvoiceInfo1.BindInvoiceSkuGrid(invoice.InvoiceSkus);
			InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Edit);
			//begin - shan - mar 06, 2012 - show cancel button always
			//pnlStep2.Visible = true;
			pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = true;
			//end
			lblPanelIndex1.Visible = false;
			lnkStep1Edit.Visible = false;
			pnlStep1.CssClass = "step1-editing";

			var totalAmount = invoice.InvoiceSkus.Aggregate<InvoiceSKU, decimal>(0, (current, sku) => current + sku.TotalPrice);

		    InvoiceTotal1.TaxPercentage = invoice.TaxPercentage;			
		    InvoiceTotal1.TaxAmount = invoice.TaxAmount;
		    InvoiceTotal1.ShipAmount = invoice.ShippingAmount;
		    InvoiceTotal1.ShipPercentage = invoice.ShippingPercentage;
            InvoiceTotal1.TotalTaxAmount = invoice.TaxAmount;
			InvoiceTotal1.TotalDue = totalAmount - invoice.CustomerDiscount + invoice.TaxAmount + invoice.ShippingAmount;

			//begin - shan - mar 06, 2012 - show cancel button always
			//pnlStep2.Visible = false;
			pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = false;
			//end
			Session["ActionMode"] = null;
		}			

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			btnSave.Click += btnSave_Click;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
            Response.Redirect("~/Ordering/Invoice");
            Response.End();

            if (!Page.IsPostBack)
			{
                var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
                string distributorID = member.Value.Id;
				if (null != Session["ActionMode"] && Session["Invoice"] != null)
				{
					DisplayEditMode();
				}
				else if (null != Session["InvoiceContactID"])
				{
					var invoiceContactID = (int)Session["InvoiceContactID"];
					var assignedIds = new List<int>();
					var contact = ContactsDataProvider.GetContactDetail(invoiceContactID, out assignedIds);
					if (null != contact)
					{
						var invoice = CreateInvoiceFromContact(contact);


						lblPanelIndex1.Visible = true;
						lnkStep1Edit.Visible = false;
						pnlStep1.CssClass = "step1-editing";
						InvoiceInfo1.BindData(invoice, string.Empty);
						InvoiceInfo1.BindInvoiceSkuGrid();
                        InvoiceInfo1.SetInvoiceDistributorNumber(distributorID);
						InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Edit);
						pnlStep1.Visible = true;
						//begin - shan - mar 06, 2012 - show cancel button always
						//pnlStep2.Visible = false;
						pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = false;
						//end
						Session["InvoiceContactID"] = null;
					}
					else
					{

					}
				}
				//From Bizworks
				else if (Session["InvoiceStep2"] != null && Convert.ToBoolean(Session["InvoiceStep2"]) == true)
				{
					var invoice = Session["Invoice"] as Invoice;
					InvoiceInfo1.BindData(invoice, string.Empty);
					InvoiceInfo1.BindInvoiceSkuGrid();
                    InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Read);
					//begin - shan - mar 06, 2012 - show cancel button always
					//pnlStep2.Visible = true;
					pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = true;
					//end
					lblPanelIndex1.Visible = false;
					lnkStep1Edit.Visible = true;
					pnlStep1.CssClass = "step1-readonly";
					decimal totalAmount = 0;
					if (null != invoice.InvoiceSkus)
					{
					    foreach (var sku in invoice.InvoiceSkus)
					    {
					        totalAmount = totalAmount + sku.TotalPrice;
					    }

					    var taxAmount = 0M;

                        if(invoice.TaxAmount == 0)
                        {
					        taxAmount = OrderProvider.CalculateTaxForInvoice(invoice);
                        }
                        else
                        {
                            taxAmount = invoice.TaxAmount;
                        }

					    var taxRate = 0M;
					    if (taxAmount != 0)
					    {
					        taxRate = decimal.Divide(taxAmount, totalAmount);
					    }

					    var totalDue = totalAmount - invoice.CustomerDiscount;
					    var taxPercentage = 0M;


                        taxPercentage = (taxRate > 0) ? taxRate * 100 : invoice.TaxPercentage;
                        
					    InvoiceTotal1.RenderTaxErrorMessage(!(taxAmount > 0 || taxPercentage > 0));
					    InvoiceTotal1.TaxPercentage = taxPercentage;
                        

                        InvoiceTotal1.TotalTaxAmount = (invoice.TaxAmount > 0) ? invoice.TaxAmount : Math.Round((totalDue * invoice.TaxPercentage / 100), 2); ;

                        totalDue += invoice.TaxAmount > 0 ? invoice.TaxAmount : Math.Round((totalDue * invoice.TaxPercentage / 100), 2);

				        InvoiceTotal1.ShipAmount = invoice.ShippingAmount;
                        InvoiceTotal1.ShipPercentage = invoice.ShippingPercentage;
                        InvoiceTotal1.TotalShippingAmount = (invoice.ShippingAmount > 0) ? invoice.ShippingAmount : Math.Round((totalDue * invoice.ShippingPercentage / 100), 2);

                        totalDue += invoice.ShippingAmount > 0 ? invoice.ShippingAmount : Math.Round((totalDue * invoice.ShippingPercentage / 100), 2);
					    
                        InvoiceTotal1.TotalDue = totalDue;
                        this.InvoiceTotal1.SetInvoiceStatus(invoice.Status);

					}
					Session["InvoiceStep2"] = null;
				}
				//Form DWS.
				else if (!string.IsNullOrEmpty(Request.QueryString["cid"]))
				{
					var customerOrderID = Request.QueryString["cid"];
                    var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(customerOrderID);
					if (null != customerOrderV01)
					{
						var invoice = CreateInvoiceFromCustomerOrder(customerOrderV01);
						lblPanelIndex1.Visible = true;
						lnkStep1Edit.Visible = false;
						pnlStep1.CssClass = "step1-editing";
						InvoiceInfo1.BindData(invoice, string.Empty);
						InvoiceInfo1.BindInvoiceSkuGrid();
                        InvoiceInfo1.SetInvoiceDistributorNumber(distributorID);
                        InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Edit);
						pnlStep1.Visible = true;
						//begin - shan - mar 06, 2012 - show cancel button always
						//pnlStep2.Visible = false;
						pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = false;
						//end
					}
				}
                else if (!string.IsNullOrEmpty(Request.QueryString["c"]))
                {
                    var assignedIds = new List<int>();
                    var contactid = Request.QueryString["c"];
                    var contact = ContactsDataProvider.GetContactDetail(Convert.ToInt32(contactid), out assignedIds);
                    if (null != contact)
                    {
                        var invoice = CreateInvoiceFromContact(contact);
                        lblPanelIndex1.Visible = true;
                        lnkStep1Edit.Visible = false;
                        pnlStep1.CssClass = "step1-editing";
                        InvoiceInfo1.BindData(invoice, string.Empty);
                        InvoiceInfo1.BindInvoiceSkuGrid();
                        InvoiceInfo1.SetInvoiceDistributorNumber(distributorID);
                        InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Edit);
                        pnlStep1.Visible = true;
                        //begin - shan - mar 06, 2012 - show cancel button always
                        //pnlStep2.Visible = false;
                        pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = false;
                        //end
                    }
                }
				else
				{
					lblPanelIndex1.Visible = true;
					lnkStep1Edit.Visible = false;
					pnlStep1.CssClass = "step1-editing";
                    InvoiceInfo1.SetInvoiceDistributorNumber(distributorID);
                    InvoiceInfo1.SetRenderMode(Ordering.Controls.Invoices.InvoiceInfo.ControlState.Edit);
					pnlStep1.Visible = true;
					//begin - shan - mar 06, 2012 - show cancel button always
					//pnlStep2.Visible = false;
					pnlStep2.Visible = btnSave.Visible = litSaveInstr.Visible = false;
					//end
					//shan - mar 14, 2012 - format the sub total fields and set default values
					//shan - mar 12, 2012 - set invoice date to current date when creating for first time
					//this.InvoiceInfo1.InvoiceDatePicker.SelectedDate = DateTime.Today;
					this.InvoiceInfo1.SetDefaultValues();
					//coming to page for first time..clear invoice session, if exists..
					this.Session["Invoice"] = null;
				}
			}

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-nav-mid-invoices");
		}        

		protected void btnSave_Click(object sender, EventArgs e)
		{
			//begin - shan - mar 13, 2012 - before saving validate the fields in Invoice Total control
			InvoiceInfo1.BindInvoiceSkuGrid();

			InvoiceTotal1.CalculateTax(true);
			//check if err exists
			if (string.IsNullOrEmpty(InvoiceTotal1.ErrorMessage))
			{
				var invoice = Session["Invoice"] as Invoice;
				invoice.Notes = InvoiceTotal1.Note;
				invoice.PaymentAddress = InvoiceTotal1.PaymentAddress;
				
				InvoiceTotal1.CalculateTax(true);
                
				invoice.Status = (InvoiceStatus)InvoiceTotal1.InvoiceStatus;
				long invoiceId = InvoiceProvider.SetInvoice(Session["Invoice"] as Invoice);
				invoice.ID = invoiceId;
                Session["InvoiceSKUs"] = null;
				Session["Invoice"] = invoice;
				var invoiceDistributorSequence = InvoiceProvider.GetDistributorInvoiceNumber(invoice.DistributorID,
																							 invoiceId);
				Response.Redirect("~/Bizworks/MyInvoiceDetails.aspx?invoiceId=" + invoiceDistributorSequence.ToString());
			}
		}		

		protected void lnkStep1Edit_click(object sender, EventArgs e)
		{
			Session["ActionMode"] = "Edit";
			DisplayEditMode();
		}

		protected void btnConfirmYes_Click(object sender, EventArgs e)
		{
			try
			{
				//clear the session values as we are cancelling the changes now & also the cache for a cancel 
				
                var invoice = Session["Invoice"] as Invoice;
                if (invoice != null)
                {
                    InvoiceProvider.ExpireInvoicesCache(invoice.DistributorID); 
                }
                Session["Invoice"] = null;
                Response.Redirect("~/Bizworks/myinvoices.aspx", true);
			}
			catch (System.Threading.ThreadAbortException) { } //suppress the thread export exception
		}
        private List<InvoiceSKU> ValidSkus(List<InvoiceSKU> lstskus)
        {
            var ProductInfoCatalog = CatalogProvider.GetProductInfoCatalog(Thread.CurrentThread.CurrentCulture.Name);
            Dictionary<string, SKU_V01> allSKU;
            SKU_V01 sku_v01 = null;
            allSKU = ProductInfoCatalog.AllSKUs;
            InvoiceSKU validinvoicesku = null;
            List<InvoiceSKU> ValidInfoceSkus = new List<InvoiceSKU>();
            foreach (var invoiceSku in lstskus)
            {
                allSKU.TryGetValue(invoiceSku.SKU, out sku_v01);
                if (sku_v01 != null)
                {
                    validinvoicesku = new InvoiceSKU();
                    validinvoicesku.Quantity = invoiceSku.Quantity;
                    validinvoicesku.SKU = invoiceSku.SKU;
                    validinvoicesku.UnitVolumePoints = sku_v01.CatalogItem.VolumePoints;
                    validinvoicesku.UnitTotalPrice = sku_v01.CatalogItem.ListPrice;
                    validinvoicesku.TotalPrice = sku_v01.CatalogItem.ListPrice * invoiceSku.Quantity;
                    validinvoicesku.TotalVolumePoints = sku_v01.CatalogItem.VolumePoints * invoiceSku.Quantity;
                    validinvoicesku.ID = invoiceSku.ID;
                    validinvoicesku.InvoiceID = invoiceSku.InvoiceID;
                    validinvoicesku.Description = invoiceSku.Description;
                    ValidInfoceSkus.Add(validinvoicesku);
                }

            }
            if (ValidInfoceSkus.Count > 0)
                return ValidInfoceSkus;
            else
            {
                return new List<InvoiceSKU>();
            }
        }
	}
}
