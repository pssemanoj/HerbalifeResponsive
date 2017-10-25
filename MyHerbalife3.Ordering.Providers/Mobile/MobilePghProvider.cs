#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HL.Common.Utilities;
using HL.Mobile.ValueObjects;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.ViewModel.Models;
using OrderItem = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OnlineOrderItem;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public static class MobilePghProvider
    {
        public static MobilePghOrderResponseViewModel InsertPghOrder(MobilePghOrderRequestViewModel request,
            out List<ValidationErrorViewModel> validationErrors)
        {
            Thread.CurrentThread.Name = request.Client.Name == "ikiosk" ? "iKiosk" : request.Client.Name;
            var memberId = string.Empty;
            validationErrors = null;
            var response = new MobilePghOrderResponseViewModel();
            if (null == request || null == request.Data || null == request.InvoiceData)
            {
                return response;
            }
            try
            {
                var order = new Order_V01();
                var shoppingCart = new MyHLShoppingCart();
                var countryCode = request.Data.Locale.Substring(3, 2);
                memberId = request.Data.DistributorId;
                var totals = ConvertToOrderTotals(request.InvoiceData, countryCode);
                if (totals == null)
                {
                    validationErrors.Add(new ValidationErrorViewModel
                    {
                        Code = 0,
                        Reason = "Error on Totals is null value."
                    });
                    return response;
                }
                GenerateObject(request, out order, out shoppingCart, totals);
                var btOrder = OrderProvider.CreateOrder(order, shoppingCart, countryCode);
                if (null != btOrder && request.Client.Name == "ikiosk")
                {
                    ((ServiceProvider.SubmitOrderBTSvc.Order) btOrder).InputMethod = "IK";
                }
                var amount = GetAmountdue(request.InvoiceData);
                var orderNumber = GetOrderNumber(amount, shoppingCart.CountryCode, shoppingCart.DistributorID);
                var holder = OrderProvider.GetSerializedOrderHolder(btOrder, order, shoppingCart, new Guid());
                var orderData = OrderSerializer.SerializeOrder(holder);

                var gatewayName = HLConfigManager.Configurations.PaymentsConfiguration.GatewayName;
                var netbankingName = "TechProcessPaymentGateway";
                var paymentTypeDefault = "CreditCard";
                var selectedGateway = request.Data.PaymentInfo.IssuerAssociation.PaymentType.ToUpper() == paymentTypeDefault.ToUpper() ? gatewayName : netbankingName;

                var paymentId = OrderProvider.InsertPaymentGatewayRecord(orderNumber, memberId, selectedGateway,
                    orderData, request.Data.Locale);
                if (paymentId > 0)
                {
                    response.OrderNumber = orderNumber;
                    response.Status = true;
                    return response;
                }
            }
            catch (Exception ex)
            {
                validationErrors = new List<ValidationErrorViewModel>
                {
                    new ValidationErrorViewModel {Code = 0, Reason = ex.Message}
                };
            }
            response.OrderNumber = string.Empty;
            response.Status = false;
            return response;
        }

        public static Address_V01 ConvertAddressViewModelToAddress(ServiceProvider.OrderSvc.Address address)
        {
            if (null == address)
            {
                return null;
            }
            return new Address_V01
            {
                City = address.City,
                StateProvinceTerritory = address.StateProvinceTerritory,
                Country = address.Country,
                CountyDistrict = address.CountyDistrict,
                Line1 = address.Line1,
                PostalCode = address.PostalCode,
                Line2 = address.Line2,
                Line3 = address.Line3,
                Line4 = address.Line4
            };
        }

        #region Private Methods

        private static decimal GetAmountdue(OrderInvoice invoiceData)
        {
            var amountdue = 0m;
            var currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            decimal.TryParse(invoiceData.DiscountedGrandTotal.Replace(currencySymbol, string.Empty), out amountdue);
            return amountdue;
        }

        private static OrderTotals ConvertToOrderTotals(OrderInvoice invoiceData, string counrty)
        {
            if (string.IsNullOrEmpty(invoiceData.DiscountedGrandTotal))
            {
                return null;
            }
            var amountdue = GetAmountdue(invoiceData);
            if (amountdue == 0)
            {
                return null;
            }
            return counrty == "CN"
                ? new OrderTotals_V02 {AmountDue = amountdue}
                : new OrderTotals_V01 {AmountDue = amountdue};
        }

        private static string GetOrderNumber(decimal amount, string country, string memberId)
        {
            var OrderNumberResponse = OrderProvider.GenerateOrderNumber(
                new GenerateOrderNumberRequest_V01
                {
                    Amount = amount,
                    Country = country,
                    DistributorID = memberId
                }
                );
            return OrderNumberResponse.OrderID;
        }

        private static void GenerateObject(MobilePghOrderRequestViewModel request, out Order_V01 Order,
            out MyHLShoppingCart ShoppingCart, OrderTotals Totals)
        {
            var orderTotalsV01 = Totals as OrderTotals_V01;
            var orderTotalsV02 = Totals as OrderTotals_V02;


            decimal amount = 0;

            if (request.Data.OrderItems.Count > 0)
            {
                amount = request.Data.Locale.Substring(3, 2) == "CN"
                    ? orderTotalsV02.AmountDue
                    : orderTotalsV01.AmountDue;
            }
            else
            {
                amount = request.Data.Locale.Substring(3, 2) == "CN"
                    ? orderTotalsV02.Donation
                    : orderTotalsV01.AmountDue;
            }

            #region Fill Order
            var order = new Order_V01
            {
                DistributorID = request.Data.DistributorId,
                CountryOfProcessing = request.Data.Locale.Substring(3, 2),
                OrderCategory =
                    string.IsNullOrEmpty(request.Data.OrderType)
                        ? OrderCategoryType.RSO
                        : (OrderCategoryType)
                            Enum.Parse(typeof (OrderCategoryType), request.Data.OrderType),
                OrderMonth = GetOrderMonth(request.Data.OrderYear, request.Data.OrderMonth),
                Pricing = Totals,
                OrderItems = PopulateOrderItems(request.Data.OrderItems),
                ReceivedDate = DateUtils.GetCurrentLocalTime(request.Data.Locale.Substring(3, 2)),
                Shipment = PopulateOrderShipment(request),
                Payments = PopulateOrderPayments(request, amount),
                Handling = PopulateOrderHandlingInfo(request),
                DiscountPercentage = request.InvoiceData.Discount
            };

            #endregion

            #region ShoppingCart

            var shoppingCart = new MyHLShoppingCart
            {
                Locale = request.Data.Locale,
                CountryCode = request.Data.Locale.Substring(3, 2),
                DistributorID = request.Data.DistributorId,
                Totals = Totals,
                ShoppingCartID = 0,
                OrderCategory = ServiceProvider.CatalogSvc.OrderCategoryType.RSO,
                EmailAddress = request.Data.Confirmation.Email,
                EmailValues = new HLShoppingCartEmailValues(),
                DeliveryInfo = new ShippingInfo
                {
                    WarehouseCode = request.Data.WarehouseCode,
                    AdditionalInformation = string.Empty,
                    Option =
                        !string.IsNullOrEmpty(request.Data.DeliveryType) &&
                        request.Data.DeliveryType.ToUpper() == "PICKUP"
                            ? MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup
                            : MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping,
                    Address = new MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01
                    {
                        Recipient = (request.Data.ShippingInfo as ShippingInfo_V01).Recipient,
                        Address = ObjectMappingHelper.Instance.GetToShipping(request.Data.ShippingInfo.Address as Address_V01)
                    }
                }
            };

            shoppingCart.ShoppingCartItems = new List<DistributorShoppingCartItem>();
            foreach (var item in request.Data.OrderItems)
            {
                shoppingCart.ShoppingCartItems.Add(new DistributorShoppingCartItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    SKU = item.Sku
                });
            }

            #endregion

            Order = order;
            ShoppingCart = shoppingCart;
        }

        private static OrderItems PopulateOrderItems(IEnumerable<OrderInvoiceItem> orderItems)
        {
            var items = new OrderItems();
            items.AddRange(orderItems.Select(item => new ServiceProvider.OrderSvc.OnlineOrderItem
            {
                SKU = item.Sku,
                Quantity = item.Quantity,
                Description = item.Description
            }));

            return items;
        }

        #region Populate Methods

        private static HandlingInfo PopulateOrderHandlingInfo(MobilePghOrderRequestViewModel request)
        {
            return new HandlingInfo_V01
            {
                PickupName = request.Data.ShippingInfo.Recipient,
                IncludeInvoice = (InvoiceHandlingType)
                    Enum.Parse(typeof(InvoiceHandlingType), request.Data.PackWithInvoice), //TODO: Confirm
                ShippingInstructions =
                    string.IsNullOrEmpty(request.Data.ShippingInstruction)
                        ? string.Format("{0},{1}", request.Data.ShippingInfo.Recipient,
                            string.IsNullOrEmpty(request.Data.ShippingInfo.Phone)
                                ? "11111111111"
                                : request.Data.ShippingInfo.Phone) : request.Data.ShippingInstruction
            };
        }

        private static PaymentCollection PopulateOrderPayments(MobilePghOrderRequestViewModel request, decimal amount)
        {
            if (request == null)
            {
                return null;
            }
            var payment = OrderProvider.GetBasePayment("CreditCard");
            payment.Amount = amount;
            return new PaymentCollection
            {
                payment
            };
        }

        private static string GetOrderMonth(int year, int month)
        {
            var yearFormat = year.ToString().Substring(2, 2);
            var monthFormat = month.ToString().Length == 1 ? "0" + month : month.ToString();
            return
                yearFormat + monthFormat;
        }

        #endregion

        private static ShippingInfo_V01 PopulateOrderShipment(MobilePghOrderRequestViewModel request)
        {
            if (request == null)
            {
                return null;
            }
            var shippingInfo = request.Data.ShippingInfo;
            return new ShippingInfo_V01
            {
                Address = ConvertAddressViewModelToAddress(shippingInfo.Address),
                WarehouseCode = request.Data.WarehouseCode,
                ShippingMethodID = !string.IsNullOrEmpty(shippingInfo.ShippingMethodID) &&
                                   shippingInfo.ShippingMethodID != "0"
                    ? shippingInfo.ShippingMethodID
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                Recipient = shippingInfo.Recipient,
                Phone =
                    shippingInfo.Phone == string.Empty ? "11111111111" : shippingInfo.Phone
            };
        }

        #endregion
    }
}