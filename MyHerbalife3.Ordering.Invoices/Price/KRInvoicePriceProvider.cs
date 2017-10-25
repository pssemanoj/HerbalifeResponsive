#region

using System;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class KRInvoicePriceProvider : InvoicePriceProvider
    {
        private readonly IInvoiceShippingDetails _invoiceShipping;

        public KRInvoicePriceProvider(IInvoiceShippingDetails invoiceShipping)
        {
            _invoiceShipping = invoiceShipping;
        }

        public override InvoiceModel CalculateBasicPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode, bool isReset)
        {
            var distributorOrderingProfile = GetDistributorOrderingProfile(memberId, countryCode);

            if (null == invoice.InvoicePrice)
            {
                invoice.InvoicePrice = new InvoicePriceModel();
            }

            var currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            invoice.InvoicePrice.DisplayCurrencySymbol = currencySymbol;
            foreach (var invoiceLine in invoice.InvoiceLines)
            {
                CalculateInvoiceLineTotal(invoiceLine, currencySymbol);
            }
            CalculateSubtotal(invoice);

            CalculateTotalEarnBase(invoice);
            var memberDiscount = invoice.InvoicePrice.MemberStaticDiscount > 0
                ? invoice.InvoicePrice.MemberStaticDiscount
                : distributorOrderingProfile.StaticDiscount;

            CalculateTotalVolumePoints(invoice);

            if (invoice.Type == "Distributor" && invoice.InvoicePrice.DiscountPercentage == 0)
            {
                invoice.InvoicePrice.DiscountPercentage = 25;
            }

            if (isReset)
            {
                invoice.InvoicePrice.CalcTaxAmount = 0;
                invoice.InvoicePrice.TaxAmount = invoice.InvoicePrice.CalcTaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();

                invoice.InvoicePrice.TotalDue = invoice.InvoicePrice.SubTotal;
                invoice.InvoicePrice.DisplayTotalDue = invoice.InvoicePrice.TotalDue.FormatPrice();

                invoice.InvoicePrice.DiscountPercentage = invoice.Type == "Distributor" ? 25 : 0;
                invoice.InvoicePrice.DiscountAmount = 0;
                invoice.InvoicePrice.CalcDiscountAmount = 0;
                invoice.InvoicePrice.DisplayDiscountedAmount = string.Format("-{0}",
                    invoice.InvoicePrice.CalcDiscountAmount.FormatPrice());

                invoice.InvoicePrice.ShippingPercentage = 0;
                invoice.InvoicePrice.ShippingAmount = 0;
                invoice.InvoicePrice.DisplayShipping = invoice.InvoicePrice.ShippingAmount.FormatPrice();

                invoice.InvoicePrice.MemberFreight = 0;
                invoice.InvoicePrice.DisplayMemberFreight = invoice.InvoicePrice.MemberFreight.FormatPrice();

                invoice.InvoicePrice.MemberTax = 0;
                invoice.InvoicePrice.DisplayMemberTax = invoice.InvoicePrice.MemberTax.FormatPrice();

                invoice.InvoicePrice.MemberTotal = 0;
                invoice.InvoicePrice.DisplayMemberTotal = invoice.InvoicePrice.MemberTotal.FormatPrice();

                invoice.InvoicePrice.Profit = 0;
                invoice.InvoicePrice.DisplayProfit = invoice.InvoicePrice.Profit.FormatPrice();

                invoice.InvoicePrice.ProfitPercentage = 0;
            }
            return invoice;
        }


        public override InvoiceModel CalculateCustomerPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode)
        {
            if (null == invoice || string.IsNullOrEmpty(memberId))
            {
                return invoice;
            }
            var address = !invoice.InvoiceShipToAddress
                ? (invoice.MemberAddress != null && !string.IsNullOrEmpty(invoice.MemberAddress.City) &&
                   !string.IsNullOrEmpty(invoice.MemberAddress.State))
                    ? GetAddressFromInvoice(invoice.MemberAddress)
                    : DistributorOrderingProfileProvider.GetAddress(AddressType.Mailing, memberId, countryCode)
                : GetAddressFromInvoice(invoice.Address);
            var warehouseCode = _invoiceShipping.GetWarehouseCode(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale);
            var shippingMethodId = string.IsNullOrEmpty(invoice.ShippingMethod)
                ? _invoiceShipping.GetShippingMethodId(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale)
                : invoice.ShippingMethod;

            CalculateDiscountedAmount(invoice);
            CalculateShippingAmount(invoice);
            foreach (var invoiceLine in invoice.InvoiceLines)
            {
                CalculateLineDiscountedAmmount(invoiceLine, invoice.InvoicePrice);
                CaculateFreightCharge(invoice, invoiceLine);
            }
            CalculateTaxAmount(invoice, memberId, countryCode, address, warehouseCode,
                shippingMethodId);
            CalculateTotalDue(invoice);

            CalculateProfit(invoice);
            return invoice;
        }

        public override InvoiceModel CalculateDistributorPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode)
        {
            var address = !invoice.InvoiceShipToAddress
                ? (invoice.MemberAddress != null && !string.IsNullOrEmpty(invoice.MemberAddress.City) &&
                   !string.IsNullOrEmpty(invoice.MemberAddress.State))
                    ? GetAddressFromInvoice(invoice.MemberAddress)
                    : DistributorOrderingProfileProvider.GetAddress(AddressType.Mailing, memberId, countryCode)
                : GetAddressFromInvoice(invoice.Address);
            var warehouseCode = _invoiceShipping.GetWarehouseCode(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale);
            var shippingMethodId = _invoiceShipping.GetShippingMethodId(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale);

            var order = ConvertToDistributorrOrder(invoice, memberId, countryCode, address, warehouseCode,
                shippingMethodId, invoice.InvoicePrice.DiscountPercentage);

            order.UseSlidingScale = true;
            var response = CallDistributorPricing(order, false);
            if (null == response)
            {
                LoggerHelper.Error("Invoice CalculateDistributorPriceOnCustomerSection Pricing error");
                return invoice;
            }
            CalculateDiscountedAmount(invoice, response);
            CalculateShippingAmount(invoice, response);
            CalculateTaxAmount(invoice, response);
            CalculateTotalDue(invoice);
            CalculateProfit(invoice);
            return invoice;
        }

        public override InvoiceModel CalculateModifiedPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode)
        {
            var address = !invoice.InvoiceShipToAddress
                ? DistributorOrderingProfileProvider.GetAddress(AddressType.Mailing, memberId, countryCode)
                : GetAddressFromInvoice(invoice.Address);
            var warehouseCode = _invoiceShipping.GetWarehouseCode(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale);
            var shippingMethodId = string.IsNullOrEmpty(invoice.ShippingMethod)
                ? _invoiceShipping.GetShippingMethodId(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale)
                : invoice.ShippingMethod;
            CalculateDiscountedAmount(invoice);
            CalculateShippingAmount(invoice);
            foreach (var invoiceLine in invoice.InvoiceLines)
            {
                CalculateLineDiscountedAmmount(invoiceLine, invoice.InvoicePrice);
                CaculateFreightCharge(invoice, invoiceLine);
            }
            CalculateTaxAmountForModifiedPricing(invoice, memberId, countryCode, address, warehouseCode,
                shippingMethodId);
            CalculateTotalDue(invoice);
            return invoice;
        }

        public override InvoiceModel CalculateDistributorPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode)
        {
            var distributorOrderingProfile = GetDistributorOrderingProfile(memberId, countryCode);
            var address = !invoice.InvoiceShipToAddress
                ? (invoice.MemberAddress != null && !string.IsNullOrEmpty(invoice.MemberAddress.City) &&
                   !string.IsNullOrEmpty(invoice.MemberAddress.State))
                    ? GetAddressFromInvoice(invoice.MemberAddress)
                    : DistributorOrderingProfileProvider.GetAddress(AddressType.Mailing, memberId, countryCode)
                : GetAddressFromInvoice(invoice.Address);
            var warehouseCode = _invoiceShipping.GetWarehouseCode(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale);
            var shippingMethodId = string.IsNullOrEmpty(invoice.ShippingMethod)
                ? _invoiceShipping.GetShippingMethodId(ObjectMappingHelper.Instance.GetToShipping(address as Address_V01), locale)
                : invoice.ShippingMethod;
            invoice.InvoicePrice.MemberStaticDiscount = invoice.InvoicePrice.MemberStaticDiscount > 0
                ? invoice.InvoicePrice.MemberStaticDiscount
                : distributorOrderingProfile.StaticDiscount;
            var order = ConvertToDistributorrOrder(invoice, memberId, countryCode, address, warehouseCode,
                shippingMethodId, invoice.InvoicePrice.MemberStaticDiscount);

            var response = CallDistributorPricing(order, false);
            if (null == response)
            {
                LoggerHelper.Error("Invoice CalculateDistributorPrice Pricing error");
                return invoice;
            }

            invoice.InvoicePrice.MemberDiscount = response.DiscountPercentage;

            foreach (var invoiceLine in invoice.InvoiceLines)
            {
                CalculateYourPrice(invoiceLine, invoice.InvoicePrice, response,
                    response.DiscountPercentage);
            }

            CalculateTotalYourPrice(invoice);
            CalculateMemberTax(response, invoice);
            CalculateMemberFreight(response, invoice);
            CalculateTotalMemberTax(invoice);


            return invoice;
        }

        #region private Methods

        private static void CalculateTaxAmount(InvoiceModel invoice, OrderTotals_V01 response)
        {
            if (invoice.InvoicePrice.TaxPercentage > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount =
                    decimal.Round(
                        ((invoice.InvoicePrice.SubTotal - invoice.InvoicePrice.CalcDiscountAmount)*
                         (invoice.InvoicePrice.TaxPercentage/100)), 2);
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else if (invoice.InvoicePrice.TaxAmount > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount = invoice.InvoicePrice.TaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else if (invoice.InvoicePrice.TaxAmount == 0 && invoice.InvoicePrice.TaxPercentage == 0 && (invoice.IsCustomerTaxEdited && !invoice.ResetCustomerTaxValue))
            {
                invoice.InvoicePrice.CalcTaxAmount = invoice.InvoicePrice.TaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else
            {
                invoice.InvoicePrice.CalcTaxAmount = response.TaxAmount;
                invoice.InvoicePrice.TaxAmount = invoice.InvoicePrice.CalcTaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
        }


        private static void CaculateFreightCharge(InvoiceModel invoice, InvoiceLineModel invoiceLine)
        {
            invoiceLine.FreightCharge =
                decimal.Round(
                    invoice.InvoicePrice.CalcShippingAmount*invoiceLine.Quantity*invoiceLine.RetailPrice/
                    invoice.InvoicePrice.SubTotal, 2);
        }

        private static void CalculateTotalYourPrice(InvoiceModel invoice)
        {
            invoice.InvoicePrice.TotalYourPrice = invoice.InvoiceLines.AsParallel().Sum(x => x.YourPrice);
            invoice.InvoicePrice.DisplayTotalYourPrice = invoice.InvoicePrice.TotalYourPrice.FormatPrice();
        }

        private static void CalculateTotalVolumePoints(InvoiceModel invoice)
        {
            invoice.InvoicePrice.TotalVolumePoints = invoice.InvoiceLines.AsParallel().Sum(x => x.TotalVolumePoint);
        }


        private static void CalculateMemberFreight(OrderTotals_V01 response, InvoiceModel invoice)
        {

            if (invoice.MakeMemberHandlingFeeZero)
            {
                invoice.InvoicePrice.MemberFreight = 0;
                invoice.InvoicePrice.DisplayMemberFreight = invoice.InvoicePrice.MemberFreight.FormatPrice();
                return;
            }

            if (!invoice.IsMemberHandlingFeeEdited && invoice.InvoicePrice.MemberFreight == 0)
            {
                var chargeListV01S =
                (from item in response.ChargeList
                 where item as Charge_V01 != null
                 select item as Charge_V01);

                if (!chargeListV01S.Any()) return;
                var distributorCharges = chargeListV01S.Where(p => (p.ChargeType == ChargeTypes.FREIGHT));
                if (!distributorCharges.Any()) return;
                var distributorCharge = distributorCharges.Single();
                invoice.InvoicePrice.MemberFreight = distributorCharge.Amount;
                invoice.InvoicePrice.DisplayMemberFreight = invoice.InvoicePrice.MemberFreight.FormatPrice();

                if (invoice.InvoicePrice.ShippingAmount == 0 && invoice.InvoicePrice.ShippingPercentage == 0)
                {
                    invoice.InvoicePrice.ShippingAmount = invoice.InvoicePrice.MemberFreight;
                }
                return;
            }
            if (invoice.InvoicePrice.ShippingAmount == 0 && invoice.InvoicePrice.ShippingPercentage == 0 && invoice.ResetCustomerTaxValue)
            {
                invoice.InvoicePrice.ShippingAmount = invoice.InvoicePrice.MemberFreight;
            }

            invoice.InvoicePrice.DisplayMemberFreight = invoice.InvoicePrice.MemberFreight.FormatPrice();
            
        }

        public override InvoiceModel CalculateTotalDue(InvoiceModel invoice, string memberId, string locale, string countryCode, bool isReset)
        {
            CalculateDiscountedAmount(invoice);
            CalculateTotalDue(invoice);
            return invoice;
        }
        private static void CalculateMemberTax(OrderTotals_V01 response, InvoiceModel invoice)
        {
            if (invoice.MakeMemberTaxZero)
            {
                invoice.InvoicePrice.MemberTax = 0;
                invoice.InvoicePrice.DisplayMemberTax = invoice.InvoicePrice.MemberTax.FormatPrice();
                return;
            }

            if (!invoice.IsMemberTaxEdited && invoice.InvoicePrice.MemberTax == 0)
            {
                invoice.InvoicePrice.MemberTax = response.TaxAmount;
                invoice.InvoicePrice.DisplayMemberTax = invoice.InvoicePrice.MemberTax.FormatPrice();
                return;
            }
            invoice.InvoicePrice.DisplayMemberTax = invoice.InvoicePrice.MemberTax.FormatPrice();
        }


        private static void CalculateProfit(InvoiceModel invoice)
        {
            invoice.InvoicePrice.Profit = invoice.InvoicePrice.TotalDue - invoice.InvoicePrice.MemberTotal;
            invoice.InvoicePrice.DisplayProfit = invoice.InvoicePrice.Profit.FormatPrice();

            invoice.InvoicePrice.ProfitPercentage =
                decimal.Round(((invoice.InvoicePrice.Profit/invoice.InvoicePrice.TotalDue)*100), 2);
        }

        private static void CalculateTotalMemberTax(InvoiceModel invoice)
        {
            invoice.InvoicePrice.MemberTotal = invoice.InvoicePrice.TotalYourPrice + invoice.InvoicePrice.MemberTax +
                                               invoice.InvoicePrice.MemberFreight;
            invoice.InvoicePrice.DisplayMemberTotal = invoice.InvoicePrice.MemberTotal.FormatPrice();
        }

        private static void CalculateYourPrice(InvoiceLineModel invoiceLine, InvoicePriceModel invoicePrice,
            OrderTotals_V01 response,
            decimal staticDiscount)
        {
            if (null != response.ItemTotalsList && response.ItemTotalsList.Any())
            {
                var itemTotal =
                    (ItemTotal_V01) response.ItemTotalsList.Find(p => ((ItemTotal_V01) p).SKU == invoiceLine.Sku);
                if (null != itemTotal)
                {
                    invoiceLine.YourPrice = itemTotal.DiscountedPrice;
                    invoiceLine.DisplayYourPrice = invoiceLine.YourPrice.FormatPrice();
                }
                else
                {
                    LoggerHelper.Error("CalculateYourPrice itemTotal is null");
                    invoiceLine.YourPrice = 0m;
                    invoiceLine.DisplayYourPrice = invoiceLine.YourPrice.FormatPrice();
                }
            }
            else
            {
                LoggerHelper.Error("CalculateYourPrice ItemTotalsList is null");
                invoiceLine.YourPrice = 0m;
                invoiceLine.DisplayYourPrice = invoiceLine.YourPrice.FormatPrice();
            }
        }

        private static void CalculateLineDiscountedAmmount(InvoiceLineModel invoiceLine, InvoicePriceModel invoicePrice)
        {
            var deltaPerItem = decimal.Round(
                (-invoicePrice.CalcDiscountAmount*invoiceLine.RetailPrice/invoicePrice.SubTotal), 2);
            invoiceLine.CalcDiscountedAmount = invoiceLine.RetailPrice + deltaPerItem;
        }

        private static void CalculateTotalEarnBase(InvoiceModel invoice)
        {
            invoice.InvoicePrice.TotalEarnBase = invoice.InvoiceLines.AsParallel().Sum(s => s.TotalEarnBase);
        }

        private static void CalculateTotalDue(InvoiceModel invoice)
        {
            invoice.InvoicePrice.TotalDue = invoice.InvoicePrice.TotalDiscountedAmount +
                                            invoice.InvoicePrice.CalcShippingAmount + invoice.InvoicePrice.CalcTaxAmount;

            invoice.InvoicePrice.DisplayTotalDue = invoice.InvoicePrice.TotalDue.FormatPrice();
        }

        private void CalculateTaxAmount(InvoiceModel invoice, string memberId, string countryCode, Address address,
            string warehouseCode, string shippingMethodId)
        {
            if (invoice.InvoicePrice.TaxPercentage > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount =
                    decimal.Round(
                        ((invoice.InvoicePrice.SubTotal - invoice.InvoicePrice.CalcDiscountAmount)*
                         (invoice.InvoicePrice.TaxPercentage/100)), 2);
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else if (invoice.InvoicePrice.TaxAmount > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount = invoice.InvoicePrice.TaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else if (invoice.InvoicePrice.TaxAmount == 0 && invoice.InvoicePrice.TaxPercentage == 0 && (invoice.IsCustomerTaxEdited && !invoice.ResetCustomerTaxValue))
            {
                invoice.InvoicePrice.CalcTaxAmount = invoice.InvoicePrice.TaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else
            {
                var customerOrder = ConvertToCustomerOrder(invoice, memberId, countryCode, address, warehouseCode,
                    shippingMethodId);

                var productTax = 0M;
                var freightTax = 0M;

                var customerOrderItems = new OrderItems();

                foreach (CustomerOrderItem_V01 item in customerOrder.OrderItems)
                {
                   customerOrderItems.Add(item);
                   
                }


                if (customerOrderItems.Any())
                {
                    customerOrder.OrderItems = null;
                    customerOrder.OrderItems = customerOrderItems;

                    var response = CallDwsPricing(customerOrder);
                    if (null != response && response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                    {
                        var responseV01 = response as GetTaxDataForDwsFromVertexResponse_V01;
                        if (responseV01 == null)
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "CalculateDistributorPrice - InvoicePriceProvider: An error occured while invoking CalculateDistributorPrice"
                                    ));
                        }
                        else
                        {
                            productTax += responseV01.ProductTaxData.AsParallel().Sum(x => x.Value);
                            freightTax += responseV01.FreightTaxData.AsParallel().Sum(x => x.Value);
                        }
                    }
                    else
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "CalculateDistributorPrice - InvoicePriceProvider: An error occured while invoking CalculateDistributorPrice {0}",
                                memberId));
                    }
                }

                invoice.InvoicePrice.CalcTaxAmount = productTax + freightTax;
                invoice.InvoicePrice.TaxAmount = invoice.InvoicePrice.CalcTaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
        }

        private void CalculateTaxAmountForModifiedPricing(InvoiceModel invoice, string memberId, string countryCode,
            Address address,
            string warehouseCode, string shippingMethodId)
        {
            if (invoice.InvoicePrice.TaxPercentage > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount =
                    decimal.Round((invoice.InvoicePrice.CalcDiscountAmount*(invoice.InvoicePrice.TaxPercentage/100)), 2);
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else if (invoice.InvoicePrice.TaxAmount > 0)
            {
                invoice.InvoicePrice.CalcTaxAmount = invoice.InvoicePrice.TaxAmount;
                invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
            }
            else
            {
                var order = ConvertToOrderForModifiedPrice(invoice, memberId, countryCode, address, warehouseCode,
                    shippingMethodId);
                var response = CallModifiedPricing(order);
                if (null != response && response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as CalculateTaxResponse_V01;
                    if (responseV01 == null)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "CalculateDistributorPrice - InvoicePriceProvider: An error occured while invoking CalculateTaxAmountForModifiedPricing"
                                ));
                    }
                    else
                    {
                        invoice.InvoicePrice.CalcTaxAmount = responseV01.ProductTax.AsParallel().Sum(x => x.Value) +
                                                             responseV01.FreightTax.AsParallel().Sum(x => x.Value) +
                                                             responseV01.DiscountTax.AsParallel().Sum(x => x.Value);
                        invoice.InvoicePrice.DisplayCalculatedTax = invoice.InvoicePrice.CalcTaxAmount.FormatPrice();
                    }
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format(
                            "CalculateDistributorPrice - InvoicePriceProvider: An error occured while invoking CalculateDistributorPrice {0}",
                            memberId));
                }
            }
        }

        private static void CalculateShippingAmount(InvoiceModel invoice, OrderTotals_V01 response)
        {
            if (invoice.InvoicePrice.ShippingPercentage > 0)
            {
                invoice.InvoicePrice.CalcShippingAmount =
                    decimal.Round(
                        ((invoice.InvoicePrice.SubTotal - invoice.InvoicePrice.CalcDiscountAmount)*
                         (invoice.InvoicePrice.ShippingPercentage/100)), 2);
            }
            else if (invoice.InvoicePrice.ShippingAmount > 0)
            {
                invoice.InvoicePrice.CalcShippingAmount = invoice.InvoicePrice.ShippingAmount;
            }
            else if (invoice.InvoicePrice.ShippingAmount == 0 && invoice.InvoicePrice.ShippingPercentage == 0 && (invoice.IsCustomerShippingHandlingEdited && !invoice.ResetCustomerTaxValue))
            {
                invoice.InvoicePrice.CalcShippingAmount = 0;
            }
            else
            {
                var chargeListV01S =
                    (from item in response.ChargeList
                        where item as Charge_V01 != null
                        select item as Charge_V01);

                if (!chargeListV01S.Any()) return;
                var distributorCharges = chargeListV01S.Where(p => (p.ChargeType == ChargeTypes.FREIGHT));
                if (!distributorCharges.Any()) return;
                var distributorCharge = distributorCharges.Single();
                invoice.InvoicePrice.CalcShippingAmount = distributorCharge.Amount;
                invoice.InvoicePrice.ShippingAmount = invoice.InvoicePrice.CalcShippingAmount;
            }
            invoice.InvoicePrice.DisplayShipping = invoice.InvoicePrice.CalcShippingAmount.FormatPrice();
        }

        private static void CalculateShippingAmount(InvoiceModel invoice)
        {
            if (invoice.InvoicePrice.ShippingPercentage > 0)
            {
                invoice.InvoicePrice.CalcShippingAmount =
                    decimal.Round(
                        ((invoice.InvoicePrice.SubTotal - invoice.InvoicePrice.CalcDiscountAmount)*
                         (invoice.InvoicePrice.ShippingPercentage/100)), 2);
            }
            else if (invoice.InvoicePrice.ShippingAmount > 0)
            {
                invoice.InvoicePrice.CalcShippingAmount = invoice.InvoicePrice.ShippingAmount;
            }
            else if (invoice.InvoicePrice.ShippingAmount == 0 && invoice.InvoicePrice.ShippingPercentage == 0 && (invoice.IsCustomerShippingHandlingEdited && !invoice.ResetCustomerTaxValue))
            {
                invoice.InvoicePrice.CalcShippingAmount = 0;
            }
            else
            {
                invoice.InvoicePrice.CalcShippingAmount = 0;
            }
            //  invoice.InvoicePrice.ShippingAmount = invoice.InvoicePrice.CalcShippingAmount;
            invoice.InvoicePrice.DisplayShipping = invoice.InvoicePrice.CalcShippingAmount.FormatPrice();
        }

        private static void CalculateDiscountedAmount(InvoiceModel invoice, OrderTotals_V01 response)
        {
            var totalAmount = invoice.Type == "Distributor"
                ? invoice.InvoicePrice.TotalEarnBase
                : invoice.InvoicePrice.SubTotal;

            if (invoice.InvoicePrice.DiscountPercentage > 0 &&
                invoice.InvoicePrice.DiscountPercentage == response.DiscountPercentage)
            {
                invoice.InvoicePrice.CalcDiscountAmount = response.ItemsTotal - response.DiscountedItemsTotal;
            }
            else if (invoice.InvoicePrice.DiscountPercentage > 0)
            {
                invoice.InvoicePrice.CalcDiscountAmount =
                    decimal.Round((totalAmount*(invoice.InvoicePrice.DiscountPercentage/100)), 2);
            }
            else if (invoice.InvoicePrice.DiscountAmount > 0)
            {
                invoice.InvoicePrice.CalcDiscountAmount = invoice.InvoicePrice.DiscountAmount;
            }
            else
            {
                invoice.InvoicePrice.CalcDiscountAmount = invoice.InvoicePrice.DiscountAmount;
            }
            invoice.InvoicePrice.DisplayDiscountedAmount = string.Format("-{0}",
                invoice.InvoicePrice.CalcDiscountAmount.FormatPrice());
            invoice.InvoicePrice.TotalDiscountedAmount = invoice.InvoicePrice.SubTotal -
                                                         invoice.InvoicePrice.CalcDiscountAmount;
        }

        private static void CalculateDiscountedAmount(InvoiceModel invoice)
        {
            var totalAmount = invoice.Type == "Distributor"
                ? invoice.InvoicePrice.TotalEarnBase
                : invoice.InvoicePrice.SubTotal;


            if (invoice.InvoicePrice.DiscountPercentage > 0)
            {
                invoice.InvoicePrice.CalcDiscountAmount =
                    decimal.Round((totalAmount*(invoice.InvoicePrice.DiscountPercentage/100)), 2);
            }
            else if (invoice.InvoicePrice.DiscountAmount > 0)
            {
                invoice.InvoicePrice.CalcDiscountAmount = invoice.InvoicePrice.DiscountAmount;
            }
            else
            {
                invoice.InvoicePrice.CalcDiscountAmount = invoice.InvoicePrice.DiscountAmount;
            }
            invoice.InvoicePrice.DisplayDiscountedAmount = string.Format("-{0}",
                invoice.InvoicePrice.CalcDiscountAmount.FormatPrice());
            invoice.InvoicePrice.TotalDiscountedAmount = invoice.InvoicePrice.SubTotal -
                                                         invoice.InvoicePrice.CalcDiscountAmount;
        }

        private static void CalculateSubtotal(InvoiceModel invoice)
        {
            invoice.InvoicePrice.SubTotal = invoice.InvoiceLines.AsParallel().Sum(s => s.TotalRetailPrice);
            invoice.InvoicePrice.DisplaySubtotal = invoice.InvoicePrice.SubTotal.FormatPrice();
        }

        private static void CalculateInvoiceLineTotal(InvoiceLineModel invoiceLine, string currencySymbol)
        {
            invoiceLine.TotalRetailPrice = invoiceLine.Quantity*invoiceLine.RetailPrice;
            invoiceLine.TotalVolumePoint = invoiceLine.Quantity*invoiceLine.VolumePoint;
            invoiceLine.TotalEarnBase = invoiceLine.Quantity*invoiceLine.EarnBase;

            invoiceLine.DisplayTotalRetailPrice = invoiceLine.TotalRetailPrice.FormatPrice();
            invoiceLine.DisplayTotalVp = invoiceLine.TotalVolumePoint.FormatPrice();
        }

        #endregion
    }
}