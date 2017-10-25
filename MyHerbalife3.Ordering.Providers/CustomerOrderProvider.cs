// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomerOrderProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The coupon status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.SharedProviders;

namespace MyHerbalife3.Ordering.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shipping;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
    using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;
    using System.Web.Security;
    using Configuration.ConfigurationManagement;
    using System.Web;
    using Shared.Infrastructure.Extensions;
    /// <summary>
                                           /// The customer order provider.
                                           /// </summary>
    public class CustomerOrderProvider
    {
        #region Public Methods
        /// <summary>
        /// The setup customer order data.
        /// </summary>
        /// <param name="customerOrderId">
        /// The customer order id.
        /// </param>
        /// <param name="distributorId"></param>
        /// <param name="countryCode"></param>
        /// <param name="locale"></param>
        /// <param name="isAddressValid"></param>
        /// <param name="customerAddressId"></param>
        public static void SetupCustomerOrderData(string customerOrderId, string distributorId, string countryCode, string locale, out bool isAddressValid, out int customerAddressId)
        {
            isAddressValid = false;
            var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(customerOrderId);

            // Close all carts for provided Customer Order ID
            if (customerOrderV01.OrderStatus == CustomerOrderStatusType.Cancelled)
            {
                ShoppingCartProvider.DeleteOldShoppingCartForCustomerOrder(
                    distributorId, customerOrderV01.OrderID);
            }

            var existingCart = ShoppingCartProvider.GetShoppingCart(distributorId, locale);
            bool useAddressValidation = ShippingProvider.GetShippingProvider(customerOrderV01.ProcessingCountry).AddressValidationRequired();
            bool useDsAddressAsShippingAddress = ShippingProvider.GetShippingProvider(customerOrderV01.ProcessingCountry).DSAddressAsShippingAddress();
            var DShippingAddressForCustomer = new List<DeliveryOption>();
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.SetDSAddressforCashOnDelivery && customerOrderV01.PaymentMethodChoice == CustomerPaymentMethodChoice.CashOnDelivery) // Mappinf object error CustomerPaymentMethodChoice.CashOnDelivery)
            {

                var dsProfile = DistributorOrderingProfileProvider.GetProfile(distributorId,
                                                                              customerOrderV01.ProcessingCountry);
                
               var  mailingAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, distributorId, customerOrderV01.ProcessingCountry);
               if (string.IsNullOrWhiteSpace(mailingAddress.PostalCode) || string.IsNullOrWhiteSpace(mailingAddress.City) || string.IsNullOrWhiteSpace(mailingAddress.Line1) || string.IsNullOrWhiteSpace(mailingAddress.StateProvinceTerritory) || dsProfile.PhoneNumbers == null)
               {
                   HttpContext.Current.Response.Redirect("~/dswebadmin/customerorders.aspx?error=2", false);
               }
                useDsAddressAsShippingAddress = true;
                DShippingAddressForCustomer.Add(

                    new DeliveryOption
                        {
                            Address = ObjectMappingHelper.Instance.GetToShipping(mailingAddress),
                            Id = -4,
                            AddressId = new Guid(),
                            AddressType = "Shipping",
                            Alias = "",
                            AltAreaCode = string.Empty,
                            AltPhone = string.Empty,
                            AreaCode = mailingAddress.PostalCode,
                            Recipient = string.Empty,
                            PostalCode = mailingAddress.PostalCode,
                        Phone = dsProfile.PhoneNumbers != null
                                    ? dsProfile.PhoneNumbers.Find(x => x.IsPrimary) != null
                                          ? dsProfile.PhoneNumbers.FirstOrDefault(x => x.IsPrimary).CountryPrefix + dsProfile.PhoneNumbers.FirstOrDefault(x => x.IsPrimary).AreaCode + dsProfile.PhoneNumbers.FirstOrDefault(x => x.IsPrimary).Number
                                          : dsProfile.PhoneNumbers.FirstOrDefault().CountryPrefix + dsProfile.PhoneNumbers.FirstOrDefault().AreaCode + dsProfile.PhoneNumbers.FirstOrDefault().Number
                                    : string.Empty,
                            State = mailingAddress.StateProvinceTerritory,
                            FreightCode = "NOF" ,// DEFAULT FREIGHTCODE TO CALL retrieveFreightCode,
                            Name = DistributorProfileModel.DistributorName(),
            
                            }
                    );

          
            }
            int temporaryEnteredAddressId = 0;
            DeliveryOption option = null;
            var custOrderAddress = GetCustomerAddress(customerOrderV01, useAddressValidation);
            if ((custOrderAddress != null) && (!useDsAddressAsShippingAddress))
            {
                ShippingProvider.GetShippingProvider(countryCode).GetShippingAddresses(
                    distributorId, customerOrderV01.ProcessingLocale);
                temporaryEnteredAddressId =
                    ShippingProvider.GetShippingProvider(countryCode).SaveShippingAddress(
                        distributorId, customerOrderV01.ProcessingLocale, custOrderAddress, true, true, false);

                isAddressValid = true;

                // Add address To Temporary Address and Insert customer order cart
                custOrderAddress.ID = temporaryEnteredAddressId;
                option = new DeliveryOption(custOrderAddress);
            }
            else
            {
                ShippingAddress_V02 shippingAddress = null;
                if (useDsAddressAsShippingAddress)
                {
                    
                    List<DeliveryOption> shippingAddresses = DShippingAddressForCustomer.Count > 0 ? DShippingAddressForCustomer:
                        ShippingProvider.GetShippingProvider(countryCode).GetShippingAddresses(
                            distributorId, customerOrderV01.ProcessingLocale);
                    if (shippingAddresses != null && shippingAddresses.Count > 0)
                    {
                        
                        if ((shippingAddress = shippingAddresses.Find(s => s.IsPrimary)) == null)
                        {
                            shippingAddress = shippingAddresses.First();
                        }
                    }

                }
                if (shippingAddress != null)
                {
                    ShippingAddress_V02 shippingAddressV2 = null;
                    if (DShippingAddressForCustomer.Count > 0)
                    {
                        shippingAddressV2 = DShippingAddressForCustomer[0];
                        shippingAddressV2.ID = -4;
                        shippingAddressV2.Recipient = DistributorProfileModel.DistributorName();
                    }

                    option = new DeliveryOption(shippingAddress);
                    temporaryEnteredAddressId =DShippingAddressForCustomer.Count > 0 ?  ShippingProvider.GetShippingProvider(countryCode).SaveShippingAddress(
                        distributorId, customerOrderV01.ProcessingLocale, shippingAddressV2, true, true, false) : shippingAddress.ID;
                    if (DShippingAddressForCustomer.Count>0)
                    DShippingAddressForCustomer[0].ID = temporaryEnteredAddressId;
                }
                else
                {
                    option = new DeliveryOption();
                }
            }

            customerAddressId = temporaryEnteredAddressId;

            if (existingCart != null)
            {
                if (existingCart.DeliveryInfo != null)
                {
                    existingCart.DeliveryInfo.Address.ID = temporaryEnteredAddressId;
                }
                ShoppingCartProvider.UpdateShoppingCart(existingCart);
            }
            else
            {
                var shippingInfo = new Shipping.ShippingInfo(ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping, option);

                MyHLShoppingCart customerCart = ShoppingCartProvider.InsertShoppingCart(
                    distributorId,
                    customerOrderV01.ProcessingLocale,
                    ServiceProvider.CatalogSvc.OrderCategoryType.RSO,
                    shippingInfo,
                    getCustomerOrderDetails(customerOrderV01), false, string.Empty);

                if (customerCart != null)
                {
                    foreach (ShoppingCartItem_V01 item in
                        GetCustomerCartItems(customerCart.ShoppingCartID, customerOrderV01.OrderItems))
                    {
                        ShoppingCartProvider.InsertShoppingCartItem(
                            distributorId, item, customerCart.ShoppingCartID);
                    }

                    // Update Status of Order To In Progress In Azure
                    CustomerOrderingProvider.UpdateCustomerOrderStatus(
                        customerOrderV01.OrderID, customerOrderV01.OrderStatus, CustomerOrderStatusType.InProgress);
                }
                else
                {
                    throw new Exception("Unable to Convert Customer Order To DO Order");
                }
            }
        }

        private static Shared.ViewModel.Models.DistributorProfileModel DistributorProfileModel
        {
            get
            {
                var membershipUser = (Shared.ViewModel.Models.MembershipUser<Shared.ViewModel.Models.DistributorProfileModel>)Membership.GetUser();
                return membershipUser != null ? membershipUser.Value : null;
            }
        }
        #endregion

        /// <summary>
        /// GetCustomerAddress
        /// </summary>
        /// <param name="custOrder"></param>
        /// <param name="useAddressValidation"></param>
        /// <returns></returns>
        private static ShippingAddress_V02 GetCustomerAddress(CustomerOrder_V01 custOrder, bool useAddressValidation)
        {
            var custAddress = new ShippingAddress_V02();

            custAddress.Phone = ((CustomerShippingInfo_V01)custOrder.Shipping).Phone;
            custAddress.FirstName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.First;
            custAddress.LastName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.Last;

            var address = new ServiceProvider.ShippingSvc.Address_V01();
            address.City = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.City;
            address.Country = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.Country;
            address.CountyDistrict = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.CountyDistrict;
            address.Line1 = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.Line1;
            address.Line2 = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.Line2;
            address.Line3 = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.Line3;
            address.PostalCode = ((CustomerShippingInfo_V01)custOrder.Shipping).Address.PostalCode;
            address.StateProvinceTerritory =
                ((CustomerShippingInfo_V01)custOrder.Shipping).Address.StateProvinceTerritory;
            custAddress.Address = address;

            custAddress.FirstName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.First;
            custAddress.LastName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.Last;
            custAddress.MiddleName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.Middle;
            custAddress.Recipient = custAddress.FirstName + " " + custAddress.MiddleName + " " + custAddress.LastName;

            if (useAddressValidation)
            {
                if (!ShippingProvider.GetShippingProvider(custOrder.ProcessingCountry).CustomerAddressIsValid(custOrder.ProcessingCountry, ref custAddress))
                {
                    return null;
                }
            }            

            return custAddress;
        }

        /// <summary>
        /// The get customer order details.
        /// </summary>
        /// <param name="custOrder">
        /// The cust order.
        /// </param>
        /// <returns>
        /// </returns>
        private static CustomerOrderDetail getCustomerOrderDetails(CustomerOrder_V01 custOrder)
        {
            var orderDetail = new CustomerOrderDetail();

            orderDetail.CustomerOrderID = custOrder.OrderID;
            orderDetail.EmailAddress = custOrder.EmailAddress;
            orderDetail.FirstName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.First;
            orderDetail.LastName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.Last;
            orderDetail.MiddleName = ((CustomerShippingInfo_V01)custOrder.Shipping).Recipient.Middle;
            orderDetail.OrderDate = custOrder.Created;
            orderDetail.CustomerPaymentPreference = custOrder.PaymentMethodChoice.ToString();
            orderDetail.ContactPreference = "HL";
            orderDetail.Telephone = ((CustomerShippingInfo_V01)custOrder.Shipping).Phone;
            orderDetail.CustomerComments = custOrder.CustomerNote;

            return orderDetail;
        }

        /// <summary>
        /// The get customer cart items.
        /// </summary>
        /// <param name="shoppingCartID">
        /// The shopping cart id.
        /// </param>
        /// <param name="items">
        /// The items.
        /// </param>
        /// <returns>
        /// List of shopping cart items
        /// </returns>
        private static List<ShoppingCartItem_V01> GetCustomerCartItems(int shoppingCartID, ServiceProvider.CustomerOrderSvc.OrderItems items)
        {
            var lstSkus = new List<ShoppingCartItem_V01>();

            for (int i = 0; i < items.Count; i++)
            {
                var cartItem = new ShoppingCartItem_V01()
                {
                    ID = shoppingCartID,
                    SKU = items[i].SKU,
                    Quantity = items[i].Quantity,
                    Updated = DateTime.Now,
                    MinQuantity = items[i].Quantity
                };
                lstSkus.Add(cartItem);
            }

            return lstSkus;
        }
    }
}