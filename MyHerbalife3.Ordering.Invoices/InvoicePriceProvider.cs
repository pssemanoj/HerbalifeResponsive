#region

using System;
using System.Collections.Generic;
using System.Linq;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.DataContract.Interfaces;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public abstract class InvoicePriceProvider
    {
        public abstract InvoiceModel CalculateBasicPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode, bool isReset);

        public abstract InvoiceModel CalculateTotalDue(InvoiceModel invoice, string memberId, string locale,
            string countryCode, bool isReset);

        public virtual InvoiceModel CalculateMemberTotal(InvoiceModel invoice, string memberId, string locale,
            string countryCode)
        {
            if (null != invoice && null != invoice.InvoicePrice)
            {
                invoice.InvoicePrice.MemberTotal = invoice.InvoicePrice.TotalYourPrice +
                                                   invoice.InvoicePrice.MemberFreight + invoice.InvoicePrice.MemberTax;
                invoice.InvoicePrice.DisplayMemberTotal = invoice.InvoicePrice.MemberTotal.FormatPrice();

                invoice.InvoicePrice.Profit = invoice.InvoicePrice.TotalDue - invoice.InvoicePrice.MemberTotal;
                invoice.InvoicePrice.DisplayProfit = invoice.InvoicePrice.Profit.FormatPrice();

                invoice.InvoicePrice.ProfitPercentage =
                    decimal.Round(((invoice.InvoicePrice.Profit / invoice.InvoicePrice.TotalDue) * 100), 2);
            }
            return invoice;
        }

        public abstract InvoiceModel CalculateCustomerPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode);

        public abstract InvoiceModel CalculateDistributorPrice(InvoiceModel invoice, string memberId, string locale,
            string countryCode);

        public abstract InvoiceModel CalculateDistributorPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode);

        public abstract InvoiceModel CalculateModifiedPriceOnCustomerSection(InvoiceModel invoice, string memberId,
            string locale,
            string countryCode);


        private readonly ISimpleCache _cache = CacheFactory.Create();
        public virtual ServiceProvider.OrderSvc.Address GetAddressFromInvoice(InvoiceAddressModel address)
        {
            return new ServiceProvider.OrderSvc.Address_V01
            {
                City = address.City,
                CountyDistrict = address.County,
                PostalCode = address.PostalCode,
                Line1 = address.Address1,
                Line2 = address.Address2,
                Country = address.Country,
                StateProvinceTerritory = address.State
            };
        }

        public virtual ServiceProvider.OrderSvc.CustomerOrder_V01 ConvertToCustomerOrder(InvoiceModel invoice, string memberId,
            string countryCode, ServiceProvider.OrderSvc.Address address, string warehouseCode, string shippingMethodId)
        {
            var customerOrder = new ServiceProvider.OrderSvc.CustomerOrder_V01
            {
                DistributorID = memberId,
                ProcessingCountry = countryCode,
                OrderItems = new ServiceProvider.OrderSvc.OrderItems()
            };
            foreach (var customerOrderItem in invoice.InvoiceLines.Select(invoiceLine => new ServiceProvider.OrderSvc.CustomerOrderItem_V01
            {
                Quantity = invoiceLine.Quantity,
                SKU = invoiceLine.Sku,
                RetailPrice = invoiceLine.CalcDiscountedAmount,
                StockingSKU = invoiceLine.StockingSku,
                TaxCategory = invoiceLine.ProductCategory,
                FreightCharge = invoiceLine.FreightCharge
            }))
            {
                customerOrder.OrderItems.Add(customerOrderItem);
            }

            customerOrder.Shipping = GetCustomerShippingInfo(address, warehouseCode, invoice, shippingMethodId);
            return customerOrder;
        }


        public virtual ServiceProvider.OrderSvc.Order_V01 ConvertToOrderForModifiedPrice(InvoiceModel invoice, string memberId,
            string countryCode, ServiceProvider.OrderSvc.Address address, string warehouseCode, string shippingMethodId)
        {
            var distributorOrder = new ServiceProvider.OrderSvc.Order_V01
            {
                DistributorID = memberId,
                CountryOfProcessing = countryCode,
                OrderMonth = DateUtils.GetCurrentLocalTime(countryCode).ToString("yyMM"),
                InputMethod = ServiceProvider.OrderSvc.InputMethodType.Internet,
                ReceivedDate = DateUtils.GetCurrentLocalTime(countryCode),
                OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO,
                UseSlidingScale = true,
                OrderItems = new ServiceProvider.OrderSvc.OrderItems()
            };
            foreach (var distributorOrderItem in invoice.InvoiceLines.Select(invoiceLine => new ServiceProvider.OrderSvc.OrderItem_V03
            {
                RetailPrice = invoice.Type == "Distributor" ? invoiceLine.RetailPrice : invoiceLine.EarnBase,
                Quantity = invoiceLine.Quantity,
                SKU = invoiceLine.Sku,
                DiscountAmount = (invoiceLine.RetailPrice * invoiceLine.Quantity) - (invoiceLine.CalcDiscountedAmount * invoiceLine.Quantity),
                FreightCharge = invoiceLine.FreightCharge,
                TaxCategory = invoiceLine.ProductCategory,
                StockingSKU = invoiceLine.StockingSku,
                PackageHandlingCharge = 0,
                Description = string.Empty
            }))
            {
                distributorOrder.OrderItems.Add(distributorOrderItem);
            }

            distributorOrder.Shipment = GetDistributorShippingInfo(address, warehouseCode, invoice, shippingMethodId);
            return distributorOrder;
        }

        public virtual ServiceProvider.OrderSvc.Order_V01 ConvertToDistributorrOrder(InvoiceModel invoice, string memberId,
            string countryCode, ServiceProvider.OrderSvc.Address address, string warehouseCode, string shippingMethodId, decimal discoount)
        {
            var distributorOrder = new ServiceProvider.OrderSvc.Order_V01
            {
                DistributorID = memberId,
                CountryOfProcessing = countryCode,
                OrderMonth = DateUtils.GetCurrentLocalTime(countryCode).ToString("yyMM"),
                InputMethod = ServiceProvider.OrderSvc.InputMethodType.Internet,
                ReceivedDate = DateUtils.GetCurrentLocalTime(countryCode),
                OrderCategory = ServiceProvider.OrderSvc.OrderCategoryType.RSO,
                UseSlidingScale = true,
                DiscountPercentage = discoount,
                OrderItems = new ServiceProvider.OrderSvc.OrderItems()
            };
            foreach (var distributorOrderItem in invoice.InvoiceLines.OrderByDescending(i => i.Quantity).Select(invoiceLine => new ServiceProvider.OrderSvc.OrderItem_V01
            {
                Quantity = invoiceLine.Quantity,
                SKU = invoiceLine.Sku
            }))
            {
                distributorOrder.OrderItems.Add(distributorOrderItem);
            }

            distributorOrder.Shipment = GetDistributorShippingInfo(address, warehouseCode, invoice, shippingMethodId);
            return distributorOrder;
        }

        private static ServiceProvider.OrderSvc.ShippingInfo GetDistributorShippingInfo(ServiceProvider.OrderSvc.Address address, string warehouseCode,
            InvoiceModel invoice,
            string shippingMethodId)
        {
            var shipping = new ServiceProvider.OrderSvc.ShippingInfo_V01
            {
                FreightVariant = string.Empty,
                ShippingMethodID = shippingMethodId,
                WarehouseCode = warehouseCode,
                Address = address,
                Recipient = invoice.FirstName,
                Phone = invoice.Phone
            };
            return shipping;
        }

        private static ServiceProvider.OrderSvc.ShippingInfo GetCustomerShippingInfo(ServiceProvider.OrderSvc.Address address, string warehouseCode, InvoiceModel invoice, string shippingMethodId)
        {
            var customerShippingInfo = new ServiceProvider.OrderSvc.CustomerShippingInfo_V01
            {
                Address = address,
                ShippingSpeed = ServiceProvider.OrderSvc.CustomerShippingMethod.Regular,
                WarehouseCode = warehouseCode,
                Recipient = new ServiceProvider.OrderSvc.Name_V01 { First = invoice.FirstName, Last = invoice.LastName },
                Carrier = ServiceProvider.OrderSvc.CustomerShippingCarrier.None,
                Phone = invoice.Phone,
                ShippingMethodId = shippingMethodId
            };
            return customerShippingInfo;
        }

        public virtual GetTaxDataForDwsFromVertexResponse CallDwsPricing(ServiceProvider.OrderSvc.CustomerOrder_V01 customerOrder)
        {
            var request = new GetTaxDataForDwsFromVertexRequest_V01() { Order = customerOrder };
            try
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                var response = proxy.GetTaxDataForDwsFromVertex(new GetTaxDataForDwsFromVertexRequest1(request)).GetTaxDataForDwsFromVertexResult;
                return response;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling GetTaxDataForDwsFromVertex service method {0}",
                        ex.Message));
            }
            return null;
        }

        public List<TaxRate> GetTaxRatesFromTableStorage(string locale)
        {
            var cacheKey = string.Format("Inv_Customer_TaxRates_{0}", locale);
            var result = _cache.Retrieve(_ => GetTaxRatesFromCache(locale), cacheKey,
                        TimeSpan.FromMinutes(60));
            return result;
        }

        private List<TaxRate> GetTaxRatesFromCache(string locale)
        {
            var request = new ServiceProvider.CustomerOrderSvc.GetTaxRateRequestV01 { Locale = locale };
            try
            {
                var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy();
                var response = proxy.GetTaxRate(new GetTaxRateRequest1(request)).GetTaxRateResult;
                if (null != response && response.Status == ServiceProvider.CustomerOrderSvc.ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as GetTaxRateResponse_V01;
                    if (null != responseV01 && responseV01.TaxRates.Any())
                    {
                        return responseV01.TaxRates;
                    }
                }
                LoggerHelper.Error("Invoice Customer pricing GetTaxRatesFromTableStorage return null");
                return new List<TaxRate>();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling GetTaxDataForDwsFromVertex service method {0}",
                        ex.Message));
            }
            return new List<TaxRate>();
        }

        public List<ItemTaxRate> GetItemTaxRatesFromTableStorage(string countryCode)
        {
            var cacheKey = string.Format("Inv_Customer_ItemTaxRates_{0}", countryCode);
            var result = _cache.Retrieve(_ => GetItemTaxRatesFromCache(countryCode), cacheKey,
                        TimeSpan.FromMinutes(60));
            return result;
        }

        private List<ItemTaxRate> GetItemTaxRatesFromCache(string countryCode)
        {
            var request = new GetTaxItemRateRequestV01 { CountryCode = countryCode };
            try
            {
                var proxy = ServiceClientProvider.GetCustomerOrderServiceProxy();
                var response = proxy.GetTaxItemRate(new GetTaxItemRateRequest1(request)).GetTaxItemRateResult;
                if (null != response && response.Status == ServiceProvider.CustomerOrderSvc.ServiceResponseStatusType.Success)
                {
                    var responseV01 = response as GetTaxItemRateResponse_V01;
                    if (null != responseV01 && responseV01.ItemTaxRates.Any())
                    {
                        return responseV01.ItemTaxRates;
                    }
                }
                LoggerHelper.Error("Invoice Customer pricing GetItemTaxRatesFromTableStorage return null");
                return new List<ItemTaxRate>();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling GetTaxDataForDwsFromVertex service method {0}",
                        ex.Message));
            }
            return new List<ItemTaxRate>();
        }

        public virtual ServiceProvider.OrderSvc.OrderTotals_V01 CallDistributorPricing(ServiceProvider.OrderSvc.Order_V01 order, bool useHmsCalc)
        {
            try
            {
                string errorCode;
                return ShoppingCartProvider.GetQuote(order, QuotePartType.Freight, useHmsCalc, out errorCode);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling CallDistributorPricing service method {0}",
                        ex.Message));
            }
            return null;
        }

        public virtual CalculateTaxResponse CallModifiedPricing(ServiceProvider.OrderSvc.Order_V01 order)
        {
            var request = new ServiceProvider.OrderSvc.CalculateTaxRequest_V01() { Order = order };
            try
            {
                var proxy = ServiceClientProvider.GetOrderServiceProxy();
                var response = proxy.CalculateTax(new CalculateTaxRequest1(request)).CalculateTaxResult;
                return response;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "OrderService - InvoiceProvider: An error occured while Calling CalculateTax service method {0}",
                        ex.Message));
            }
            return null;
        }


        public static DistributorOrderingProfile GetDistributorOrderingProfile(string memberId, string countryCode)
        {
            var distributorOrderingProfileFactory = new DistributorOrderingProfileFactory();
            return distributorOrderingProfileFactory.GetDistributorOrderingProfile(memberId, countryCode);
        }

    }
}