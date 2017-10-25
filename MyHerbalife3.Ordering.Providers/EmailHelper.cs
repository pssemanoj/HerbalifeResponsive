using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using HL.Common.Logging;
using HL.Common.Utilities;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.LegacyProviders;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using System.Text;
using System.IO;
using EmailAddress = HL.Common.ValueObjects.EmailAddress;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using Distributor = MyHerbalife3.Shared.LegacyProviders.Distributor;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using Address_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Providers
{
    public static class EmailHelper
    {
        public static void SendEmail(MyHLShoppingCart shoppingCart, Order_V01 order)
        {
            if (shoppingCart == null)
            {
                return;
            }
            // when this method is called asynchronously, can't get correct locale from thread
            // need to retrieve current locale from shopping cart
            var doConfig = HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].DOConfiguration;
            if (!doConfig.SendEmailUsingSubmitOrder)
            {
                var profile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                                                                            shoppingCart.CountryCode);
                if (profile != null)
                {
                    //EmailPublisherProvider.SendEmail(
                    //    GetEmailFromOrder(
                    //        order,
                    //        shoppingCart,
                    //        shoppingCart.Locale,
                    //        shoppingCart.DeliveryInfo.Address.Recipient,
                    //        shoppingCart.DeliveryInfo));
                    EmailLegacyProvider.SendEmail(
                        GetEmailFromOrder(
                            order,
                            shoppingCart,
                            shoppingCart.Locale,
                            shoppingCart.DeliveryInfo.Address.Recipient,
                            shoppingCart.DeliveryInfo));
                }
                else
                {
                    if (!string.IsNullOrEmpty(shoppingCart.DistributorID))
                    {
                        //EmailPublisherProvider.SendEmail(
                        //    GetEmailFromOrder(
                        //        order,
                        //        shoppingCart,
                        //        shoppingCart.Locale,
                        //        shoppingCart.DeliveryInfo.Address.Recipient,
                        //        shoppingCart.DeliveryInfo));
                        EmailLegacyProvider.SendEmail(
                            GetEmailFromOrder(
                                order,
                                shoppingCart,
                                shoppingCart.Locale,
                                shoppingCart.DeliveryInfo.Address.Recipient,
                                shoppingCart.DeliveryInfo));
                    }
                }
            }
        }

        public static void SendEmailForMobile(MyHLShoppingCart shoppingCart, Order_V01 order)
        {
            if (shoppingCart == null)
            {
                return;
            }
            var doConfig = HLConfigManager.CurrentPlatformConfigs[shoppingCart.Locale].DOConfiguration;
            if (!doConfig.SendEmailUsingSubmitOrder)
            {
                var profile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                                                                            shoppingCart.CountryCode);
                if (profile != null)
                {
                    EmailLegacyProvider.SendEmail(
                        GetEmailFromOrderForMobile(
                            order,
                            shoppingCart,
                            shoppingCart.Locale,
                            shoppingCart.DeliveryInfo.Address.Recipient,
                            shoppingCart.DeliveryInfo));
                }
                else
                {
                    if (!string.IsNullOrEmpty(shoppingCart.DistributorID))
                    {
                        EmailLegacyProvider.SendEmail(
                            GetEmailFromOrderForMobile(
                                order,
                                shoppingCart,
                                shoppingCart.Locale,
                                shoppingCart.DeliveryInfo.Address.Recipient,
                                shoppingCart.DeliveryInfo));
                    }
                }
            }
        }

        private static string getEmail(List<EmailAddress> addresses)
        {
            List<string> emailList;
            if ((emailList = addresses.Where(p => p.IsPrimary).Select(p => p.Address).ToList()).Count >
                0)
            {
                return emailList.First();
            }
            else if ((emailList = addresses.Select(p => p.Address).ToList()).Count > 0)
            {
                return emailList.First();
            }
            return string.Empty;
        }

        public static DateTime GetCountryTimeNow()
        {
            try
            {
                string locale = Thread.CurrentThread.CurrentCulture.ToString();
                string CCode;
                int idx = locale.LastIndexOf("-");
                if (idx > -1)
                {
                    CCode = locale.Substring(idx + 1);
                }
                else
                {
                    CCode = "US"; // default
                }

                // for iceland no timezone tbd
                if (CCode == "IS")
                    CCode = "US";

                return DateTimeUtils.GetHmsFormattedTime(CCode);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        private static OnlineDistributor LoadDistributorInfoFromService(string distributorID,
                                                    Guid authenticationToken,
                                                    string loggedInCountry)
        {
            try
            {
                var loader = new DistributorLoader();
                var distributor = loader.Load(distributorID, loggedInCountry);

                if (distributor != null)
                {
                    bool isLocked = distributor.DistributorStatus != MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorStatusType.Complete;
                    bool isApproved = distributor.DistributorStatus == MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorStatusType.Complete;

                    var result = new OnlineDistributor(distributor, authenticationToken, isLocked, isApproved)
                    {
                        CurrentLoggedInCountry = loggedInCountry
                    };

                    return result;
                }

                LoggerHelper.Error("Email Helper LoadDistributorInfoFromService Error. Unsuccessful result from loader. ");
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Email Helper LoadDistributorInfoFromService encountered an exception. ");
            }

            return null;
        }

        public static DistributorOrderConfirmation GetEmailFromOrder(Order_V01 order,
                                                                     MyHLShoppingCart cartInfo,
                                                                     string locale,
                                                                     string pickupLocation,
                                                                     ShippingInfo deliveryInfo)
        {
            // Change the CurrentCulture of the current thread according to the locale.
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale, false);

            List<DistributorShoppingCartItem> cartItems;
            if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
            {
                cartItems =
                    cartInfo.ShoppingCartItems.Where(
                        c =>
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                            .ToList();
            }
            else
            {
                cartItems = cartInfo.ShoppingCartItems;
            }
            var doconf = new DistributorOrderConfirmation();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            //string distributorName = DistributorProfileModelHelper.FullLocalName(member.Value);
            //if (string.IsNullOrEmpty(distributorName))
            //    distributorName = DistributorProfileModelHelper.FullEnglishName(member.Value);

            // TODO : figreout name
            //var name = (distributor.Localname != null && !(string.IsNullOrEmpty(distributor.Localname.First)))
            //               ? distributor.Localname
            //               : distributor.EnglishName;
            if (member != null)
            {
                doconf.Distributor = new Distributor(null,
                                                 new ContactInfo(
                                                     member.Value.PrimaryEmail,
                                                     (order.Shipment as ShippingInfo_V01)
                                                         .Phone), cartInfo.DistributorID, "",
                                                 member.Value.FirstName, member.Value.LastName,
                                                 Thread.CurrentThread.CurrentUICulture.Name,
                                                 member.Value.MiddleName);
            }
            else
            {
                var memberProfile = LoadDistributorInfoFromService(order.DistributorID, new Guid(),
                                                    Thread.CurrentThread.CurrentUICulture.Name.Substring(3));
                if (null != memberProfile)
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), cartInfo.DistributorID, string.Empty, memberProfile.Value.Localname.First, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
                else
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), cartInfo.DistributorID, string.Empty, string.Empty, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
            }

            if (cartInfo.DsType == null)
            {
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(order.DistributorID, order.CountryOfProcessing);
                cartInfo.DsType = DistributorType;
            }
            doconf.Scheme = cartInfo.DsType.ToString();

            if (!String.IsNullOrEmpty(cartInfo.EmailAddress))
            {
                doconf.Distributor.Contact.Email = cartInfo.EmailAddress;
            }

            Address_V01 address = null;
            if (order.Payments != null && order.Payments.Count > 0)
            {
                address = order.Payments[0].Address as Address_V01;
                if (address != null)
                {
                    doconf.BillingAddress = new CAddress(address.City, address.Country, address.Line1, "",
                                                 address.StateProvinceTerritory, address.PostalCode,
                                                 address.Line3, address.Line4, address.CountyDistrict);
                }
            }
            
            doconf.OrderSubmittedDate = GetCountryTimeNow();
            address = (order.Shipment as ShippingInfo_V01).Address as Address_V01;

            if (deliveryInfo != null)
            {
                doconf.ShippingAddress = new CAddress(ObjectMappingHelper.Instance.GetToShared(deliveryInfo.Address.Address));
                if (cartInfo.CountryCode == "BR")
                {
                    switch (deliveryInfo.Option)
                    {
                        case DeliveryOptionType.Shipping:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource1.Text", CultureInfo.CurrentCulture) as string;
                            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeEstimated)
                            {
                                var providerBR =
                                    ShippingProvider.GetShippingProvider(locale.Substring(3));
                                var estimated = providerBR.GetDeliveryEstimate(deliveryInfo, locale);
                                if (estimated != null)
                                {
                                    doconf.DeliveryTimeEstimated =
                                        string.Format(
                                            HttpContext.GetLocalResourceObject(
                                                HLConfigManager.Configurations.CheckoutConfiguration
                                                               .CheckoutOptionsControl, "hlDeliveryTimeResource.Text",
                                                CultureInfo.CurrentCulture) as string, estimated.Value);
                                }
                            }
                            break;
                        case DeliveryOptionType.ShipToCourier:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource5.Text", CultureInfo.CurrentCulture) as string;
                            break;
                        case DeliveryOptionType.Pickup:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource2.Text", CultureInfo.CurrentCulture) as string;
                            break;
                    }
                }
                else if (cartInfo.CountryCode == "CN")
                {
                    doconf.DeliveryTimeEstimated = deliveryInfo.Option == DeliveryOptionType.Shipping ? "7" :  deliveryInfo.ShippingIntervalDays.ToString();
                }
            }
            else
            {
                doconf.ShippingAddress = new CAddress(ObjectMappingHelper.Instance.GetToShared(address));
            }

            string country = locale.Substring(3);

            Func<string, string, decimal> getListPrice = delegate(string sku, string countryCode)
                {
                    var catalogItemV01 = CatalogProvider.GetCatalogItem(sku, countryCode);
                    if (null != catalogItemV01)
                        return catalogItemV01.ListPrice;
                    return 0;
                };

            //Retrieving a list of child skus in the order
            List<string> childSKUsInOrder = new List<string>();
            var sKUsInOrder = from o in order.OrderItems
                              select o.SKU;
            childSKUsInOrder = OrderProvider.retrieveChildSKUsInOrder(sKUsInOrder.ToList<string>());

            doconf.Items = (from oi in order.OrderItems
                            from ci in cartItems
                            from ot in (cartInfo.Totals as OrderTotals_V01).ItemTotalsList
                            where oi.SKU == ci.SKU
                                  && (ot as ItemTotal_V01).SKU == oi.SKU
                            select
                                new OrderItemEmail(ci.EarnBase,
                                                    getDescription(ci, country),
                                                    getListPrice(oi.SKU, country),
                                                    oi.Quantity,
                                                    oi.SKU,
                                                    ci.SKU,
                                                    ci.RetailPrice,
                                                    (ot as ItemTotal_V01).VolumePoints,
                                                    (ot as ItemTotal_V01).DiscountedPrice,
                                         ci.Flavor,
                                                    OrderProvider.getPriceWithAllCharges(cartInfo.Totals as OrderTotals_V01, oi.SKU, oi.Quantity),
                                                    childSKUsInOrder.Contains(oi.SKU).ToString())
                                                    ).ToArray<OrderItemEmail>();


            if (null != cartInfo.Totals)
            {
                var orderTotals_V01 = cartInfo.Totals as OrderTotals_V01;
                if (cartInfo.CountryCode == "IN" && orderTotals_V01 != null)
                {
                    doconf.Tax = orderTotals_V01.VatTax != null ?  (decimal)orderTotals_V01.VatTax : decimal.Zero;
                }
                else
                {
                doconf.Tax = orderTotals_V01.TaxAmount;
                }

                
                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                {
                    decimal AmountDue = 0;
                    if (order.Payments != null)
                    {
                        AmountDue = order.Payments.Sum(i => i.Amount);
                    }
                    doconf.GrandTotal = AmountDue;
                    doconf.SubTotal = AmountDue;
                }
                else
                {
                    doconf.GrandTotal = orderTotals_V01.AmountDue;
                    doconf.SubTotal = orderTotals_V01.AmountDue;
                }

                doconf.TotalVolumePoints = orderTotals_V01.VolumePoints;
                doconf.TotalRetail = orderTotals_V01.ItemsTotal;
     
                DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cartInfo.DistributorID, cartInfo.CountryCode);
                doconf.VolumePointsRate = HLRulesManager.Manager.PerformDiscountRangeRules(cartInfo, locale, distributorOrderingProfile.StaticDiscount);

                doconf.TotalDiscountAmount = orderTotals_V01.ItemsTotal - orderTotals_V01.DiscountedItemsTotal;
                doconf.TotalDiscountPercentage = order.DiscountPercentage;

                decimal dCollateral = 0;
                foreach (var item in cartItems)
                {
                    try
                    {
                        if (item.ProdInfo != null)
                        {
                            if (item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.Literature ||
                                item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                            {
                                dCollateral += item.RetailPrice;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                doconf.TotalCollateralRetail = dCollateral;

                // Adding two new sub-calcutation.
                decimal pEarnBase = 0;
                decimal totalEarnBase = 0;
                decimal dProd = 0;
                foreach (var item in cartItems)
                {
                    try
                    {
                        // Total earn base.
                        totalEarnBase += item.EarnBase;
                        if (item.ProdInfo!=null)
                        {
                            if (item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.Product)
                            {
                                // Total product retail price.
                                dProd += item.RetailPrice;

                                // Total product earn base.
                                pEarnBase += item.EarnBase;
                            }    
                        }
                    }
                    catch
                    {
                    }
                }
                doconf.TotalProductRetail = dProd;

                // Sertting earn base properties.
                doconf.TotalEarnBase = totalEarnBase;
                doconf.TotalProductEarnBase = pEarnBase;

                decimal dPromo = 0;
                foreach (var ck in cartItems)
                {
                    try
                    {
                        if (ck.ProdInfo != null && ck.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.PromoAccessory)
                        {
                            dPromo += ck.DiscountPrice;
                        }
                    }
                    catch
                    {
                    }
                }

                doconf.TotalPromotionalRetail = dPromo;

                if (cartInfo.CountryCode == "BR")
                {
                    doconf.ICMS = orderTotals_V01.IcmsTax;
                    doconf.IPI = orderTotals_V01.IpiTax;
                    doconf.TotalCollateralRetail = orderTotals_V01.LiteratureRetailAmount;
                    doconf.TotalProductRetail = orderTotals_V01.ProductRetailAmount;
                    doconf.TotalPromotionalRetail = orderTotals_V01.PromotionRetailAmount;
                }

                if (cartInfo.InvoiceOption != null)
                {
                    try
                    {
                        // why some countries RESX is WithPackage_Y why the _Y?
                        var val = HttpContext.GetGlobalResourceObject("InvoiceOptions",
                                                                      cartInfo.InvoiceOption + "_Y",
                                                                      CultureInfo.CurrentCulture);
                        if (val != null)
                        {
                            doconf.InvoiceOption =
                                val.ToString().Replace("<br />", "").Replace("<br/>", "").Replace("<br>", "");
                        }
                        else
                        {
                            val = HttpContext.GetGlobalResourceObject("InvoiceOptions",
                                                                      cartInfo.InvoiceOption,
                                                                      CultureInfo.CurrentCulture);
                            if (val != null)
                            {
                                doconf.InvoiceOption =
                                    val.ToString().Replace("<br />", "").Replace("<br/>", "").Replace("<br>", "");
                            }
                        }
                    }
                    catch
                    {
                        doconf.InvoiceOption = (cartInfo.InvoiceOption);
                    }
                }

                //doconf.TotalDiscountRetail = orderTotals_V01.DiscountedItemsTotal;
                doconf.TotalDiscountRetail = cartInfo.EmailValues.DistributorSubTotal == 0 ? orderTotals_V01.DiscountedItemsTotal : cartInfo.EmailValues.DistributorSubTotal;

                var chargeList_V01s = orderTotals_V01.ChargeList !=null ?
                    (from item in orderTotals_V01.ChargeList
                     where item as Charge_V01 != null
                     select item as Charge_V01):null;

                if (null != chargeList_V01s && chargeList_V01s.Count() > 0)
                {
                    var phCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.PH));
                    if (null != phCharges && phCharges.Count() > 0)
                    {
                        var phCharge = phCharges.Single();
                        doconf.TotalPackagingHandling = phCharge.Amount;
                    }

                    var distributorCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.FREIGHT));
                    if (null != distributorCharges && distributorCharges.Count() > 0)
                    {
                        var distributorCharge = distributorCharges.Single();
                        doconf.ShippingHandling = distributorCharge.Amount;
                    }

                    var MarketingFundCharges =
                        chargeList_V01s.Where(
                            p =>
                            (p.ChargeType == ChargeTypes.MARKETING_FUND || p.ChargeType == ChargeTypes.LOGISTICS_CHARGE
                             || p.ChargeType == ChargeTypes.OTHER));
                    if (null != MarketingFundCharges && MarketingFundCharges.Count() > 0)
                    {
                        foreach (Charge_V01 MarketingFundCharge in MarketingFundCharges)
                            doconf.MarketingFund += MarketingFundCharge.Amount;
                    }
                }
            }

            int paymentCount = 0;
            if (order.Payments != null)
            {
                var paymentInfos = new PaymentInfo[order.Payments.Count];

                string ulocal = locale.ToUpper();

                // Japan has special format. Malaysia is old code 
                bool bShortDate = ulocal.Contains("JP") || ulocal.Contains("MY");
                string expires = string.Empty;
                string payoption = String.Empty;
                foreach (Payment orderPayment in order.Payments)
                {
                    var creditPayment = orderPayment as CreditPayment;
                    if (null != creditPayment)
                    {
                        if (null != creditPayment.Card)
                        {
                            string creditCardType = (creditPayment.Card.IssuerAssociation == IssuerAssociationType.None)
                                                        ? string.Empty
                                                        : creditPayment.Card.IssuerAssociation.ToString();

                            if (bShortDate)
                            {
                                expires = creditPayment.Card.Expiration.ToShortDateString();
                            }
                            else
                            {
                                try
                                {
                                    // . replacement for nn-NO, de-De.
                                    expires = String.Format("{0:MM/yyyy}", creditPayment.Card.Expiration)
                                                    .Replace(".", "/");
                                }
                                catch
                                {
                                    expires = creditPayment.Card.Expiration.ToShortDateString();
                                }
                            }

                            // payment options for Japan
                            if (ulocal.Contains("JP"))
                            {
                                try
                                {
                                    //payoption = GetJPPaymentOptions(orderPayment as CreditPayment_V01);
                                }
                                catch
                                {
                                    // no payment options
                                }
                            }
                            string installments = string.Empty;
                            if ((creditPayment as CreditPayment_V01) != null &&
                                ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01) != null)
                            {
                                installments =
                                    ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01)
                                        .NumberOfInstallments.ToString();
                            }

                            var maskedCC = creditPayment.Card.AccountNumber;
                            if (!string.IsNullOrEmpty(maskedCC) && maskedCC.Trim().Length >= 5)
                            {
                                var ccNumber = maskedCC.Trim();
                                maskedCC = string.Format("{0}{1}{2}", ccNumber.Substring(0, 1),
                                                         new string('X', ccNumber.Length - 5),
                                                         ccNumber.Substring(ccNumber.Length - 4));
                            }

                            var paymentInfo = new PaymentInfo(creditPayment.AuthorizationCode,
                                                              maskedCC,
                                                              creditCardType,
                                                              expires,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Amount,
                                                              creditPayment.TransactionType,
                                                              creditPayment.ReferenceID,
                                                              String.Empty,
                                                              installments
                                );

                            //HL.Common.Logging.LoggerHelper.Error(
                            //    string.Format("creditCardType {0} creditPayment{1} creditPayment.ReferenceID {2}",
                            //                  creditCardType, creditPayment.TransactionType, creditPayment.ReferenceID));

                            paymentInfos[paymentCount] = paymentInfo;
                        }
                    }
                    else
                    {
                        string transactionType = string.Empty;
                        string cardType = string.Empty;
                        string payCode = string.Empty;
                        string bankName = string.Empty;
                        if (orderPayment is WirePayment_V01)
                        {
                            cardType = "WIRE";
                            payCode = (orderPayment as WirePayment_V01).PaymentCode;
                            transactionType = orderPayment.TransactionType;
                        }
                        else if (orderPayment is DirectDepositPayment_V01)
                        {
                            cardType = "DEMANDDRAFT";
                            payCode = (orderPayment as DirectDepositPayment_V01).PaymentCode;
                            bankName = (orderPayment as DirectDepositPayment_V01).BankName;
                        }
                        if (cartInfo.CountryCode == "BR")
                        {
                            var strSplits = transactionType.Split('-');
                            if (strSplits.Length == 2)
                            {
                                transactionType = strSplits[0];
                            }
                        }

                        var paymentInfo = new PaymentInfo(string.Empty, string.Empty, cardType, string.Empty,
                                                          string.Empty, string.Empty, 0, transactionType,
                                                          String.Empty, payCode, string.Empty);
                        if (!string.IsNullOrEmpty(bankName))
                        {
                            paymentInfo.BankName = bankName.Trim();
                        }
                        paymentInfos[paymentCount] = paymentInfo;
                    }
                    paymentCount++;
                }

                if (String.IsNullOrEmpty(doconf.paymentOption))
                {
                    doconf.paymentOption = paymentCount.ToString();
                }

                if (deliveryInfo != null)
                {
                    doconf.PickupLocation = deliveryInfo.Description;

                    if (cartInfo.CountryCode == "UA")
                    {
                        doconf.SpecialInstructions = deliveryInfo.AdditionalInformation ?? string.Empty;
                    }

                    // use in case of pickup so never a specialinstruction conflict
                    //doconf.SpecialInstructions = deliveryInfo.Address.Address.Line1 + "<br />" +
                    //    deliveryInfo.Address.Address.Line2 + "<br />" + deliveryInfo.Address.Address.Line3 + "<br />" + deliveryInfo.Address.Address.Line4 + "<br />" +
                    //    deliveryInfo.Address.Address.City + "<br />" + deliveryInfo.Address.Address.StateProvinceTerritory + " " + deliveryInfo.Address.Address.PostalCode + "<br />" + deliveryInfo.Name;

                    // doconf.SpecialInstructions = deliveryInfo.Name;
                    //doconf.pickupTime = deliveryInfo.PickupDate.ToString();
                }
                else // else should never execute
                    doconf.PickupLocation = pickupLocation;

                if (order.Handling != null)
                {
                    var hinfo = (HandlingInfo_V01) order.Handling;

                    doconf.pickupTime = hinfo.ShippingInstructions;
                }

                doconf.Payments = paymentInfos;
            }

            var shippingInfo = order.Shipment as ShippingInfo_V01;

            string freightDescription = deliveryInfo.FreightCode;
            var provider =
                ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (provider != null)
            {
                var lstDeliveryOption = provider.GetDeliveryOptionsListForShipping(
                    locale.Substring(3), locale, deliveryInfo.Address);
                if (lstDeliveryOption != null)
                {
                    foreach (DeliveryOption delivOption in lstDeliveryOption)
                    {
                        if (delivOption.FreightCode == deliveryInfo.FreightCode)
                        {
                            freightDescription = string.IsNullOrEmpty(delivOption.Description)
                                                     ? string.Empty
                                                     : delivOption.Description.Trim();
                            break;
                        }
                    }
                }
            }

            var shipment = new Shipment(shippingInfo.Recipient, "", "", order.ReceivedDate, freightDescription,
                                        (cartInfo.DeliveryInfo != null && cartInfo.DeliveryInfo.Address != null)
                                            ? cartInfo.DeliveryInfo.Address.Recipient
                                            : string.Empty);
            doconf.Shipment = shipment;
            doconf.OrderId = order.OrderID;
            if (order.PurchaseCategory != OrderPurchaseType.None)
            {
                doconf.PurchaseType =
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                        order.PurchaseCategory.ToString(), CultureInfo.CurrentCulture)
                    as string;
            }

            // Month should have the same format as in the screen
            var currentSessionInfo = SessionInfo.GetSessionInfo(order.DistributorID, locale);
            doconf.OrderMonth = new OrderMonth(country).GetOrderMonthString(currentSessionInfo);

            if (string.IsNullOrEmpty(doconf.Language))
                doconf.Language = GetLanguage(locale);

            if (string.IsNullOrEmpty(doconf.Locale))
                doconf.Locale = GetLocale(order.CountryOfProcessing);
            OrderTotals_V01 totals;// = cartInfo.Totals as OrderTotals_V01;
            if (order.CountryOfProcessing == "CN")
            {
                totals = cartInfo.Totals as OrderTotals_V02;
            }
            else
            {
                totals = cartInfo.Totals as OrderTotals_V01;
            }


            var localTaxCharge = totals.ChargeList != null ?
                totals.ChargeList.Find(
                    delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOCALTAX; }) as Charge_V01 ??
                new Charge_V01() { ChargeType = ChargeTypes.LOCALTAX, Amount = (decimal)0.0 }
                : null;
            if (localTaxCharge != null)
            {
                doconf.LocalTaxCharge = localTaxCharge.Amount;
            }

            var taxedNet = totals.ChargeList != null ?
                totals.ChargeList.Find(
                    delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.TAXEDNET; }) as Charge_V01 ??
                new Charge_V01() { ChargeType = ChargeTypes.TAXEDNET, Amount = (decimal)0.0 }
                : null;
            if (taxedNet != null)
            {
                doconf.TaxedNet = taxedNet.Amount;
            }

            var logisticCharge = totals.ChargeList != null ?
                (totals as OrderTotals_V01).ChargeList.Find(
                    delegate (Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOGISTICS_CHARGE; }) as
                Charge_V01 ?? new Charge_V01() { ChargeType = ChargeTypes.LOGISTICS_CHARGE, Amount = (decimal)0.0 }
                : null;
            if (logisticCharge != null)
            {
                doconf.Logistics = logisticCharge.Amount;
            }

            // Indicate if the HFF disclaimer will be shown.
            doconf.HFFMessage = "N";
            if (HLConfigManager.Configurations.DOConfiguration.AllowHFF &&
                HLConfigManager.Configurations.DOConfiguration.ShowHFFMessage &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
            {
                if (cartItems.Any(i => i.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    doconf.HFFMessage = "Y";
                }
            }

            // add the remaining value and order month volume information
            doconf.RemainingValue = cartInfo.EmailValues.RemainingVolume ?? string.Empty;
            doconf.OrderMonthVolume = cartInfo.EmailValues.CurrentMonthVolume ?? string.Empty;

            return doconf;
        }


        public static DistributorOrderConfirmation GetEmailFromOrderForMobile(Order_V01 order,
                                                                     MyHLShoppingCart cartInfo,
                                                                     string locale,
                                                                     string pickupLocation,
                                                                     ShippingInfo deliveryInfo)
        {
            // Change the CurrentCulture of the current thread according to the locale.
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale, false);

            List<DistributorShoppingCartItem> cartItems;
            if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
            {
                cartItems =
                    cartInfo.ShoppingCartItems.Where(
                        c =>
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                            .ToList();
            }
            else
            {
                cartItems = cartInfo.ShoppingCartItems;
            }
            var doconf = new DistributorOrderConfirmation();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            //string distributorName = DistributorProfileModelHelper.FullLocalName(member.Value);
            //if (string.IsNullOrEmpty(distributorName))
            //    distributorName = DistributorProfileModelHelper.FullEnglishName(member.Value);

            // TODO : figreout name
            //var name = (distributor.Localname != null && !(string.IsNullOrEmpty(distributor.Localname.First)))
            //               ? distributor.Localname
            //               : distributor.EnglishName;
            if (member != null)
            {
                doconf.Distributor = new Distributor(null,
                                                 new ContactInfo(
                                                     member.Value.PrimaryEmail,
                                                     (order.Shipment as ShippingInfo_V01)
                                                         .Phone), cartInfo.DistributorID, "",
                                                 member.Value.FirstName, member.Value.LastName,
                                                 Thread.CurrentThread.CurrentUICulture.Name,
                                                 member.Value.MiddleName);
            }
            else
            {
                var memberProfile = LoadDistributorInfoFromService(order.DistributorID, new Guid(),
                                                    Thread.CurrentThread.CurrentUICulture.Name.Substring(3));
                if (null != memberProfile)
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), cartInfo.DistributorID, string.Empty, memberProfile.Value.Localname.First, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
                else
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), cartInfo.DistributorID, string.Empty, string.Empty, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
            }

            if (!String.IsNullOrEmpty(cartInfo.EmailAddress))
            {
                doconf.Distributor.Contact.Email = cartInfo.EmailAddress;
            }

            Address_V01 address = null;
            if (order.Payments != null && order.Payments.Count > 0)
            {
                address = order.Payments[0].Address as Address_V01;
                if (address != null)
                {
                    doconf.BillingAddress = new CAddress(address.City, address.Country, address.Line1, "",
                                                 address.StateProvinceTerritory, address.PostalCode,
                                                 address.Line3, address.Line4, address.CountyDistrict);
                }
            }

            doconf.OrderSubmittedDate = GetCountryTimeNow();
            address = (order.Shipment as ShippingInfo_V01).Address as Address_V01;

            if (deliveryInfo != null)
            {
                doconf.ShippingAddress = new CAddress(ObjectMappingHelper.Instance.GetToShared(deliveryInfo.Address.Address));
                if (cartInfo.CountryCode == "BR")
                {
                    switch (deliveryInfo.Option)
                    {
                        case DeliveryOptionType.Shipping:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource1.Text", CultureInfo.CurrentCulture) as string;
                            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowDeliveryTimeEstimated)
                            {
                                var providerBR =
                                    ShippingProvider.GetShippingProvider(locale.Substring(3));
                                var estimated = providerBR.GetDeliveryEstimate(deliveryInfo, locale);
                                if (estimated != null)
                                {
                                    doconf.DeliveryTimeEstimated =
                                        string.Format(
                                            HttpContext.GetLocalResourceObject(
                                                HLConfigManager.Configurations.CheckoutConfiguration
                                                               .CheckoutOptionsControl, "hlDeliveryTimeResource.Text",
                                                CultureInfo.CurrentCulture) as string, estimated.Value);
                                }
                            }
                            break;
                        case DeliveryOptionType.ShipToCourier:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource5.Text", CultureInfo.CurrentCulture) as string;
                            break;
                        case DeliveryOptionType.Pickup:
                            doconf.ShippingMethod =
                                HttpContext.GetLocalResourceObject(
                                    HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl,
                                    "ListItemResource2.Text", CultureInfo.CurrentCulture) as string;
                            break;
                    }
                }
                else if (cartInfo.CountryCode == "CN")
                {
                    doconf.DeliveryTimeEstimated = deliveryInfo.Option == DeliveryOptionType.Shipping ? "7" : deliveryInfo.ShippingIntervalDays.ToString();
                }
            }
            else
            {
                doconf.ShippingAddress = new CAddress(ObjectMappingHelper.Instance.GetToShared(address));
            }

            string country = locale.Substring(3);

            Func<string, string, decimal> getListPrice = delegate(string sku, string countryCode)
            {
                var catalogItemV01 = CatalogProvider.GetCatalogItem(sku, countryCode);
                if (null != catalogItemV01)
                    return catalogItemV01.ListPrice;
                return 0;
            };

            //Retrieving a list of child skus in the order
            List<string> childSKUsInOrder = new List<string>();
            var sKUsInOrder = from o in order.OrderItems
                              select o.SKU;
            childSKUsInOrder = OrderProvider.retrieveChildSKUsInOrder(sKUsInOrder.ToList<string>());

            doconf.Items = (from oi in order.OrderItems
                            from ci in cartItems
                            from ot in (cartInfo.Totals as OrderTotals_V01).ItemTotalsList
                            where oi.SKU == ci.SKU
                                  && (ot as ItemTotal_V01).SKU == oi.SKU
                            select
                                new OrderItemEmail(ci.EarnBase,
                                                    getDescription(ci, country),
                                                    getListPrice(oi.SKU, country),
                                                    oi.Quantity,
                                                    oi.SKU,
                                                    ci.SKU,
                                                    ci.RetailPrice,
                                                    (ot as ItemTotal_V01).VolumePoints,
                                                    (ot as ItemTotal_V01).DiscountedPrice,
                                         ci.Flavor,
                                                    OrderProvider.getPriceWithAllCharges(cartInfo.Totals as OrderTotals_V01, oi.SKU, oi.Quantity),
                                                    childSKUsInOrder.Contains(oi.SKU).ToString())
                                                    ).ToArray<OrderItemEmail>();


            #region Promo and Linked SKU Addition


            List<OrderItemEmail> itemList = doconf.Items.ToList<OrderItemEmail>();

            foreach (var ci in order.OrderItems)
            {
                if (HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalSku.Contains(ci.SKU))
                {
                    //Add Promo SKU
                    var catalogItemV01 = CatalogProvider.GetCatalogItem(ci.SKU, country);
                    OrderItemEmail itemToAdd = new OrderItemEmail();
                    itemToAdd.EarnBase = catalogItemV01.EarnBase;
                    itemToAdd.ItemDescription = catalogItemV01.Description;
                    itemToAdd.LineTotal = catalogItemV01.ListPrice;
                    itemToAdd.Quantity = ci.Quantity;
                    itemToAdd.SKU = ci.SKU;
                    itemToAdd.SkuId = ci.SKU;
                    itemToAdd.UnitPrice = catalogItemV01.ListPrice;
                    itemToAdd.VolumePoints = catalogItemV01.VolumePoints;
                    itemToAdd.PriceWithCharges = catalogItemV01.ListPrice;
                    itemToAdd.Flavor = catalogItemV01.ProductType.ToString();
                    itemToAdd.DistributorCost = catalogItemV01.ListPrice;
                    itemToAdd.IsLinkedSKU = childSKUsInOrder.Contains(ci.SKU).ToString();
                    itemList.Add(itemToAdd);

                }

                if (childSKUsInOrder.Contains(ci.SKU))
                {
                    var catalogItemV01 = CatalogProvider.GetCatalogItem(ci.SKU, country);
                    OrderItemEmail linkedSKUAdd = new OrderItemEmail(catalogItemV01.EarnBase,
                                                    catalogItemV01.Description,
                                                    getListPrice(ci.SKU, country),
                                                    ci.Quantity,
                                                    ci.SKU,
                                                    ci.SKU,
                                                    catalogItemV01.ListPrice,
                                                    catalogItemV01.VolumePoints,
                                                    catalogItemV01.ListPrice,
                                                    catalogItemV01.ProductCategory,
                                                    OrderProvider.getPriceWithAllCharges(cartInfo.Totals as OrderTotals_V01, ci.SKU, ci.Quantity),
                                                    "true");
                    itemList.Add(linkedSKUAdd);

                }
            }


            doconf.Items = null;
            doconf.Items = itemList.ToArray();

            #endregion

            if (null != cartInfo.Totals)
            {
                var orderTotals_V01 = cartInfo.Totals as OrderTotals_V01;
                if (cartInfo.CountryCode == "IN" && orderTotals_V01 != null)
                {
                    doconf.Tax = orderTotals_V01.VatTax != null ? (decimal)orderTotals_V01.VatTax : decimal.Zero;
                }
                else
                {
                    doconf.Tax = orderTotals_V01.TaxAmount;
                }


                if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                {
                    decimal AmountDue = 0;
                    if (order.Payments != null)
                    {
                        AmountDue = order.Payments.Sum(i => i.Amount);
                    }
                    doconf.GrandTotal = AmountDue;
                    doconf.SubTotal = AmountDue;
                }
                else
                {
                    doconf.GrandTotal = orderTotals_V01.AmountDue;
                    doconf.SubTotal = orderTotals_V01.AmountDue;
                }

                doconf.TotalVolumePoints = orderTotals_V01.VolumePoints;
                doconf.TotalRetail = orderTotals_V01.ItemsTotal;

                DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cartInfo.DistributorID, cartInfo.CountryCode);
                doconf.VolumePointsRate = HLRulesManager.Manager.PerformDiscountRangeRules(cartInfo, locale, distributorOrderingProfile.StaticDiscount);

                doconf.TotalDiscountAmount = orderTotals_V01.ItemsTotal - orderTotals_V01.DiscountedItemsTotal;
                doconf.TotalDiscountPercentage = order.DiscountPercentage;

                decimal dCollateral = 0;
                foreach (var item in cartItems)
                {
                    try
                    {
                        if (item.ProdInfo != null)
                        {
                            if (item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.Literature ||
                                item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                            {
                                dCollateral += item.RetailPrice;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                doconf.TotalCollateralRetail = dCollateral;

                // Adding two new sub-calcutation.
                decimal pEarnBase = 0;
                decimal totalEarnBase = 0;
                decimal dProd = 0;
                foreach (var item in cartItems)
                {
                    try
                    {
                        // Total earn base.
                        totalEarnBase += item.EarnBase;
                        if (item.ProdInfo != null)
                        {
                            if (item.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.Product)
                            {
                                // Total product retail price.
                                dProd += item.RetailPrice;

                                // Total product earn base.
                                pEarnBase += item.EarnBase;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                doconf.TotalProductRetail = dProd;

                // Sertting earn base properties.
                doconf.TotalEarnBase = totalEarnBase;
                doconf.TotalProductEarnBase = pEarnBase;

                decimal dPromo = 0;
                foreach (var ck in cartItems)
                {
                    try
                    {
                        if (ck.ProdInfo != null && ck.ProdInfo.TypeOfProduct == ServiceProvider.CatalogSvc.ProductType.PromoAccessory)
                        {
                            dPromo += ck.DiscountPrice;
                        }
                    }
                    catch
                    {
                    }
                }

                doconf.TotalPromotionalRetail = dPromo;

                if (cartInfo.CountryCode == "BR")
                {
                    doconf.ICMS = orderTotals_V01.IcmsTax;
                    doconf.IPI = orderTotals_V01.IpiTax;
                    doconf.TotalCollateralRetail = orderTotals_V01.LiteratureRetailAmount;
                    doconf.TotalProductRetail = orderTotals_V01.ProductRetailAmount;
                    doconf.TotalPromotionalRetail = orderTotals_V01.PromotionRetailAmount;
                }

                if (cartInfo.InvoiceOption != null)
                {
                    try
                    {
                        // why some countries RESX is WithPackage_Y why the _Y?
                        var val = HttpContext.GetGlobalResourceObject("InvoiceOptions",
                                                                      cartInfo.InvoiceOption + "_Y",
                                                                      CultureInfo.CurrentCulture);
                        if (val != null)
                        {
                            doconf.InvoiceOption =
                                val.ToString().Replace("<br />", "").Replace("<br/>", "").Replace("<br>", "");
                        }
                        else
                        {
                            val = HttpContext.GetGlobalResourceObject("InvoiceOptions",
                                                                      cartInfo.InvoiceOption,
                                                                      CultureInfo.CurrentCulture);
                            if (val != null)
                            {
                                doconf.InvoiceOption =
                                    val.ToString().Replace("<br />", "").Replace("<br/>", "").Replace("<br>", "");
                            }
                        }
                    }
                    catch
                    {
                        doconf.InvoiceOption = (cartInfo.InvoiceOption);
                    }
                }

                //doconf.TotalDiscountRetail = orderTotals_V01.DiscountedItemsTotal;
                doconf.TotalDiscountRetail = cartInfo.EmailValues.DistributorSubTotal == 0 ? orderTotals_V01.DiscountedItemsTotal : cartInfo.EmailValues.DistributorSubTotal;

                var chargeList_V01s = orderTotals_V01.ChargeList != null ?
                    (from item in orderTotals_V01.ChargeList
                     where item as Charge_V01 != null
                     select item as Charge_V01) : null;

                if (null != chargeList_V01s && chargeList_V01s.Count() > 0)
                {
                    var phCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.PH));
                    if (null != phCharges && phCharges.Count() > 0)
                    {
                        var phCharge = phCharges.Single();
                        doconf.TotalPackagingHandling = phCharge.Amount;
                    }

                    var distributorCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.FREIGHT));
                    if (null != distributorCharges && distributorCharges.Count() > 0)
                    {
                        var distributorCharge = distributorCharges.Single();
                        doconf.ShippingHandling = distributorCharge.Amount;
                    }

                    var MarketingFundCharges =
                        chargeList_V01s.Where(
                            p =>
                            (p.ChargeType == ChargeTypes.MARKETING_FUND || p.ChargeType == ChargeTypes.LOGISTICS_CHARGE
                             || p.ChargeType == ChargeTypes.OTHER));
                    if (null != MarketingFundCharges && MarketingFundCharges.Count() > 0)
                    {
                        foreach (Charge_V01 MarketingFundCharge in MarketingFundCharges)
                            doconf.MarketingFund += MarketingFundCharge.Amount;
                    }
                }
            }

            int paymentCount = 0;
            if (order.Payments != null)
            {
                var paymentInfos = new PaymentInfo[order.Payments.Count];

                string ulocal = locale.ToUpper();

                // Japan has special format. Malaysia is old code 
                bool bShortDate = ulocal.Contains("JP") || ulocal.Contains("MY");
                string expires = string.Empty;
                string payoption = String.Empty;
                foreach (Payment orderPayment in order.Payments)
                {
                    var creditPayment = orderPayment as CreditPayment;
                    if (null != creditPayment)
                    {
                        if (null != creditPayment.Card)
                        {
                            string creditCardType = (creditPayment.Card.IssuerAssociation == IssuerAssociationType.None)
                                                        ? string.Empty
                                                        : creditPayment.Card.IssuerAssociation.ToString();

                            if (bShortDate)
                            {
                                expires = creditPayment.Card.Expiration.ToShortDateString();
                            }
                            else
                            {
                                try
                                {
                                    // . replacement for nn-NO, de-De.
                                    expires = String.Format("{0:MM/yyyy}", creditPayment.Card.Expiration)
                                                    .Replace(".", "/");
                                }
                                catch
                                {
                                    expires = creditPayment.Card.Expiration.ToShortDateString();
                                }
                            }

                            // payment options for Japan
                            if (ulocal.Contains("JP"))
                            {
                                try
                                {
                                    //payoption = GetJPPaymentOptions(orderPayment as CreditPayment_V01);
                                }
                                catch
                                {
                                    // no payment options
                                }
                            }
                            string installments = string.Empty;
                            if ((creditPayment as CreditPayment_V01) != null &&
                                ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01) != null)
                            {
                                installments =
                                    ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01)
                                        .NumberOfInstallments.ToString();
                            }

                            var maskedCC = creditPayment.Card.AccountNumber;
                            if (!string.IsNullOrEmpty(maskedCC) && maskedCC.Trim().Length >= 5)
                            {
                                var ccNumber = maskedCC.Trim();
                                maskedCC = string.Format("{0}{1}{2}", ccNumber.Substring(0, 1),
                                                         new string('X', ccNumber.Length - 5),
                                                         ccNumber.Substring(ccNumber.Length - 4));
                            }

                            var paymentInfo = new PaymentInfo(creditPayment.AuthorizationCode,
                                                              maskedCC,
                                                              creditCardType,
                                                              expires,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Amount,
                                                              creditPayment.TransactionType,
                                                              creditPayment.ReferenceID,
                                                              String.Empty,
                                                              installments
                                );

                            //HL.Common.Logging.LoggerHelper.Error(
                            //    string.Format("creditCardType {0} creditPayment{1} creditPayment.ReferenceID {2}",
                            //                  creditCardType, creditPayment.TransactionType, creditPayment.ReferenceID));

                            paymentInfos[paymentCount] = paymentInfo;
                        }
                    }
                    else
                    {
                        string transactionType = string.Empty;
                        string cardType = string.Empty;
                        string payCode = string.Empty;
                        string bankName = string.Empty;
                        if (orderPayment is WirePayment_V01)
                        {
                            cardType = "WIRE";
                            payCode = (orderPayment as WirePayment_V01).PaymentCode;
                            transactionType = orderPayment.TransactionType;
                        }
                        else if (orderPayment is DirectDepositPayment_V01)
                        {
                            cardType = "DEMANDDRAFT";
                            payCode = (orderPayment as DirectDepositPayment_V01).PaymentCode;
                            bankName = (orderPayment as DirectDepositPayment_V01).BankName;
                        }
                        if (cartInfo.CountryCode == "BR")
                        {
                            var strSplits = transactionType.Split('-');
                            if (strSplits.Length == 2)
                            {
                                transactionType = strSplits[0];
                            }
                        }

                        var paymentInfo = new PaymentInfo(string.Empty, string.Empty, cardType, string.Empty,
                                                          string.Empty, string.Empty, 0, transactionType,
                                                          String.Empty, payCode, string.Empty);
                        if (!string.IsNullOrEmpty(bankName))
                        {
                            paymentInfo.BankName = bankName.Trim();
                        }
                        paymentInfos[paymentCount] = paymentInfo;
                    }
                    paymentCount++;
                }

                if (String.IsNullOrEmpty(doconf.paymentOption))
                {
                    doconf.paymentOption = paymentCount.ToString();
                }

                if (deliveryInfo != null)
                {
                    doconf.PickupLocation = deliveryInfo.Description;

                    // use in case of pickup so never a specialinstruction conflict
                    //doconf.SpecialInstructions = deliveryInfo.Address.Address.Line1 + "<br />" +
                    //    deliveryInfo.Address.Address.Line2 + "<br />" + deliveryInfo.Address.Address.Line3 + "<br />" + deliveryInfo.Address.Address.Line4 + "<br />" +
                    //    deliveryInfo.Address.Address.City + "<br />" + deliveryInfo.Address.Address.StateProvinceTerritory + " " + deliveryInfo.Address.Address.PostalCode + "<br />" + deliveryInfo.Name;

                    // doconf.SpecialInstructions = deliveryInfo.Name;
                    //doconf.pickupTime = deliveryInfo.PickupDate.ToString();
                }
                else // else should never execute
                    doconf.PickupLocation = pickupLocation;

                if (order.Handling != null)
                {
                    var hinfo = (HandlingInfo_V01)order.Handling;

                    doconf.pickupTime = hinfo.ShippingInstructions;
                }

                doconf.Payments = paymentInfos;
            }

            var shippingInfo = order.Shipment as ShippingInfo_V01;

            string freightDescription = deliveryInfo.FreightCode;
            var provider =
                ShippingProvider.GetShippingProvider(locale.Substring(3));
            if (provider != null)
            {
                var lstDeliveryOption = provider.GetDeliveryOptionsListForShipping(
                    locale.Substring(3), locale, deliveryInfo.Address);
                if (lstDeliveryOption != null)
                {
                    foreach (DeliveryOption delivOption in lstDeliveryOption)
                    {
                        if (delivOption.FreightCode == deliveryInfo.FreightCode)
                        {
                            freightDescription = string.IsNullOrEmpty(delivOption.Description)
                                                     ? string.Empty
                                                     : delivOption.Description.Trim();
                            break;
                        }
                    }
                }
            }

            var shipment = new Shipment(shippingInfo.Recipient, "", "", order.ReceivedDate, freightDescription,
                                        (cartInfo.DeliveryInfo != null && cartInfo.DeliveryInfo.Address != null)
                                            ? cartInfo.DeliveryInfo.Address.Recipient
                                            : string.Empty);
            doconf.Shipment = shipment;
            doconf.OrderId = order.OrderID;
            if (order.PurchaseCategory != OrderPurchaseType.None)
            {
                doconf.PurchaseType =
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                        order.PurchaseCategory.ToString(), CultureInfo.CurrentCulture)
                    as string;
            }

            // Month should have the same format as in the screen
            var currentSessionInfo = SessionInfo.GetSessionInfo(order.DistributorID, locale);
            doconf.OrderMonth = new OrderMonth(country).GetOrderMonthString(currentSessionInfo);

            if (string.IsNullOrEmpty(doconf.Language))
                doconf.Language = GetLanguage(locale);

            if (string.IsNullOrEmpty(doconf.Locale))
                doconf.Locale = GetLocale(order.CountryOfProcessing);
            OrderTotals_V01 totals;// = cartInfo.Totals as OrderTotals_V01;
            if (order.CountryOfProcessing == "CN")
            {
                totals = cartInfo.Totals as OrderTotals_V02;
            }
            else
            {
                totals = cartInfo.Totals as OrderTotals_V01;
            }


            var localTaxCharge = totals.ChargeList != null ?
                totals.ChargeList.Find(
                    delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOCALTAX; }) as Charge_V01 ??
                new Charge_V01(ChargeTypes.LOCALTAX, (decimal)0.0)
                : null;
            if (localTaxCharge != null)
            {
                doconf.LocalTaxCharge = localTaxCharge.Amount;
            }

            var taxedNet = totals.ChargeList != null ?
                totals.ChargeList.Find(
                    delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.TAXEDNET; }) as Charge_V01 ??
                new Charge_V01(ChargeTypes.TAXEDNET, (decimal)0.0)
                : null;
            if (taxedNet != null)
            {
                doconf.TaxedNet = taxedNet.Amount;
            }

            var logisticCharge = totals.ChargeList != null ?
                (totals as OrderTotals_V01).ChargeList.Find(
                    delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.LOGISTICS_CHARGE; }) as
                Charge_V01 ?? new Charge_V01(ChargeTypes.LOGISTICS_CHARGE, (decimal)0.0)
                : null;
            if (logisticCharge != null)
            {
                doconf.Logistics = logisticCharge.Amount;
            }

            // Indicate if the HFF disclaimer will be shown.
            doconf.HFFMessage = "N";
            if (HLConfigManager.Configurations.DOConfiguration.AllowHFF &&
                HLConfigManager.Configurations.DOConfiguration.ShowHFFMessage &&
                !string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
            {
                if (cartItems.Any(i => i.SKU == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    doconf.HFFMessage = "Y";
                }
            }

            // add the remaining value and order month volume information
            doconf.RemainingValue = cartInfo.EmailValues.RemainingVolume ?? string.Empty;
            doconf.OrderMonthVolume = cartInfo.EmailValues.CurrentMonthVolume ?? string.Empty;

            return doconf;
        }

        public static DistributorOrderConfirmation GetEmailFromOrder_V02(Order_V02 order, MyHLShoppingCart cartInfo, string locale)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale, false);

            var doconf = new DistributorOrderConfirmation();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            List<DistributorShoppingCartItem> cartItems;
            if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
            {
                cartItems =
                    cartInfo.ShoppingCartItems.Where(
                        c =>
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                        c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                            .ToList();
            }
            else
            {
                cartItems = cartInfo.ShoppingCartItems;
            }
            if (member != null)
            {
                doconf.Distributor = new Distributor(null,
                                                 new ContactInfo(
                                                     member.Value.PrimaryEmail,
                                                     (order.Shipment as ShippingInfo_V01)
                                                         .Phone), order.DistributorID, "",
                                                 member.Value.FirstName, member.Value.LastName,
                                                 Thread.CurrentThread.CurrentUICulture.Name,
                                                 member.Value.MiddleName);
            }
            else
            {
                var memberProfile = LoadDistributorInfoFromService(order.DistributorID, new Guid(),
                                                    Thread.CurrentThread.CurrentUICulture.Name.Substring(3));
                if (null != memberProfile)
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), order.DistributorID, string.Empty, memberProfile.Value.Localname.First, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
                else
                {
                    doconf.Distributor = new Distributor(null,
                    new ContactInfo(), order.DistributorID, string.Empty, string.Empty, string.Empty, Thread.CurrentThread.CurrentUICulture.Name, string.Empty);
                }
            }

            //if (!String.IsNullOrEmpty(order.EmailAddress))
            //{
            //    doconf.Distributor.Contact.Email = order.EmailAddress;
            //}

            if (cartInfo.DsType == null)
            {
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(order.DistributorID, order.CountryOfProcessing);
                cartInfo.DsType = DistributorType;
            }
            doconf.Scheme = cartInfo.DsType.ToString();

            var address = order.Payments[0].Address as Address_V01;
            if (address != null)
                doconf.BillingAddress = new CAddress(address.City, address.Country, address.Line1, "",
                                                     address.StateProvinceTerritory, address.PostalCode,
                                                     address.Line3, address.Line4, address.CountyDistrict);
            doconf.OrderSubmittedDate = GetCountryTimeNow();
            address = (order.Shipment as ShippingInfo_V01).Address as Address_V01;
            doconf.ShippingAddress = new CAddress(ObjectMappingHelper.Instance.GetToShared(address));
            string country = locale.Substring(3);

            Func<string, string, decimal> getListPrice = delegate(string sku, string countryCode)
            {
                var catalogItemV01 = CatalogProvider.GetCatalogItem(sku, countryCode);
                if (null != catalogItemV01)
                    return catalogItemV01.ListPrice;
                return 0;
            };

            //TODO: Get items for new OrderItemEmail array. Integration update
            //doconf.Items = (from oi in order.OrderItems
            //                select
            //                    new Item((oi as OrderItem_V02).EarnBase, (oi as OrderItem_V02).Description, (oi as OrderItem_V02).RetailPrice,
            //                             oi.Quantity, oi.SKU, oi.SKU, (oi as OrderItem_V02).RetailPrice,
            //                             0, (oi as OrderItem_V02).RetailPrice,
            //                             (oi as OrderItem_V02).Description, (oi as OrderItem_V02).RetailPrice)).ToArray<Item>();

            var pricing = order.Pricing as OrderTotals_V01;
            doconf.Tax = pricing.TaxAmount;
            doconf.GrandTotal = pricing.AmountDue;
            doconf.SubTotal = pricing.ItemsTotal;
            doconf.TotalVolumePoints = pricing.VolumePoints;
            doconf.TotalRetail = pricing.ItemsTotal;
            doconf.TotalDiscountAmount = pricing.DiscountedItemsTotal;
            doconf.TotalDiscountPercentage = order.DiscountPercentage;
            doconf.TotalCollateralRetail = pricing.LiteratureRetailAmount;
            doconf.TotalProductRetail = pricing.ProductRetailAmount;
            doconf.TotalEarnBase = pricing.TotalEarnBase;
            doconf.TotalProductEarnBase = pricing.TotalEarnBase;
            doconf.TotalPromotionalRetail = pricing.PromotionRetailAmount;
            doconf.TotalDiscountRetail = pricing.DiscountedItemsTotal;

            var chargeList_V01s = pricing.ChargeList != null ?
                    (from item in pricing.ChargeList
                     where item as Charge_V01 != null
                     select item as Charge_V01) : null;

            if (null != chargeList_V01s && chargeList_V01s.Count() > 0)
            {
                var phCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.PH));
                if (null != phCharges && phCharges.Count() > 0)
                {
                    var phCharge = phCharges.Single();
                    doconf.TotalPackagingHandling = phCharge.Amount;
                }

                var distributorCharges = chargeList_V01s.Where(p => (p.ChargeType == ChargeTypes.FREIGHT));
                if (null != distributorCharges && distributorCharges.Count() > 0)
                {
                    var distributorCharge = distributorCharges.Single();
                    doconf.ShippingHandling = distributorCharge.Amount;
                }

                var MarketingFundCharges =
                    chargeList_V01s.Where(
                        p =>
                        (p.ChargeType == ChargeTypes.MARKETING_FUND || p.ChargeType == ChargeTypes.LOGISTICS_CHARGE
                         || p.ChargeType == ChargeTypes.OTHER));
                if (null != MarketingFundCharges && MarketingFundCharges.Count() > 0)
                {
                    foreach (Charge_V01 MarketingFundCharge in MarketingFundCharges)
                        doconf.MarketingFund += MarketingFundCharge.Amount;
                }
            }

            int paymentCount = 0;
            if (order.Payments != null)
            {
                var paymentInfos = new PaymentInfo[order.Payments.Count];

                string ulocal = locale.ToUpper();

                bool bShortDate = ulocal.Contains("JP") || ulocal.Contains("MY");
                string expires = string.Empty;
                string payoption = String.Empty;
                foreach (Payment orderPayment in order.Payments)
                {
                    var creditPayment = orderPayment as CreditPayment;
                    if (null != creditPayment)
                    {
                        if (null != creditPayment.Card)
                        {
                            string creditCardType = (creditPayment.Card.IssuerAssociation == IssuerAssociationType.None)
                                                        ? string.Empty
                                                        : creditPayment.Card.IssuerAssociation.ToString();

                            if (bShortDate)
                            {
                                expires = creditPayment.Card.Expiration.ToShortDateString();
                            }
                            else
                            {
                                try
                                {
                                    // . replacement for nn-NO, de-De.
                                    expires = String.Format("{0:MM/yyyy}", creditPayment.Card.Expiration)
                                                    .Replace(".", "/");
                                }
                                catch
                                {
                                    expires = creditPayment.Card.Expiration.ToShortDateString();
                                }
                            }

                            string installments = string.Empty;
                            if ((creditPayment as CreditPayment_V01) != null &&
                                ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01) != null)
                            {
                                installments =
                                    ((creditPayment as CreditPayment_V01).PaymentOptions as PaymentOptions_V01)
                                        .NumberOfInstallments.ToString();
                            }

                            var maskedCC = creditPayment.Card.AccountNumber;
                            if (!string.IsNullOrEmpty(maskedCC) && maskedCC.Trim().Length >= 5)
                            {
                                var ccNumber = maskedCC.Trim();
                                maskedCC = string.Format("{0}{1}{2}", ccNumber.Substring(0, 1),
                                                         new string('X', ccNumber.Length - 5),
                                                         ccNumber.Substring(ccNumber.Length - 4));
                            }

                            var paymentInfo = new PaymentInfo(creditPayment.AuthorizationCode,
                                                              maskedCC,
                                                              creditCardType,
                                                              expires,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Card.NameOnCard,
                                                              creditPayment.Amount,
                                                              creditPayment.TransactionType,
                                                              creditPayment.ReferenceID,
                                                              String.Empty,
                                                              installments
                                );

                            paymentInfos[paymentCount] = paymentInfo;
                        }
                    }
                    else
                    {
                        string transactionType = string.Empty;
                        string cardType = string.Empty;
                        string payCode = string.Empty;
                        string bankName = string.Empty;
                        if (orderPayment is WirePayment_V01)
                        {
                            cardType = "WIRE";
                            payCode = (orderPayment as WirePayment_V01).PaymentCode;
                            transactionType = orderPayment.TransactionType;
                        }
                        else if (orderPayment is DirectDepositPayment_V01)
                        {
                            cardType = "DEMANDDRAFT";
                            payCode = (orderPayment as DirectDepositPayment_V01).PaymentCode;
                            bankName = (orderPayment as DirectDepositPayment_V01).BankName;
                        }
                        var paymentInfo = new PaymentInfo(string.Empty, string.Empty, cardType, string.Empty,
                                                          string.Empty, string.Empty, 0, transactionType,
                                                          String.Empty, payCode, string.Empty);
                        if (!string.IsNullOrEmpty(bankName))
                        {
                            paymentInfo.BankName = bankName.Trim();
                        }
                        paymentInfos[paymentCount] = paymentInfo;
                    }
                    paymentCount++;
                }

                if (String.IsNullOrEmpty(doconf.paymentOption))
                {
                    doconf.paymentOption = paymentCount.ToString();
                }

                if (order.Handling != null)
                {
                    var hinfo = (HandlingInfo_V01)order.Handling;
                    doconf.pickupTime = hinfo.ShippingInstructions;
                    doconf.InvoiceOption = hinfo.IncludeInvoice.ToString();
                }

                doconf.Payments = paymentInfos;
            }

            var shippingInfo = order.Shipment as ShippingInfo_V01;
            var shipment = new Shipment(shippingInfo.Recipient, "", "", order.ReceivedDate, shippingInfo.ShippingMethodID, shippingInfo.Recipient);
            doconf.Shipment = shipment;
            doconf.OrderId = order.OrderID;
            if (order.PurchaseCategory != OrderPurchaseType.None)
            {
                doconf.PurchaseType =
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                        order.PurchaseCategory.ToString(), CultureInfo.CurrentCulture)
                    as string;
            }

            // Month should have the same format as in the screen
            var currentSessionInfo = SessionInfo.GetSessionInfo(order.DistributorID, locale);
            doconf.OrderMonth = new OrderMonth(country).GetOrderMonthString(currentSessionInfo);

            if (string.IsNullOrEmpty(doconf.Language))
                doconf.Language = GetLanguage(locale);

            doconf.HFFMessage = "N";
            doconf.RemainingValue = string.Empty;
            doconf.OrderMonthVolume = string.Empty;

            return doconf;
        }

        private static string GetJPPaymentOptions(CreditPayment_V01 orderPayment)
        {
            string paymentOption = String.Empty;
            try
            {
                var orderPayment_VO1 = orderPayment;
                var options = orderPayment_VO1.PaymentOptions as PaymentOptions_V01;
                if (null != options)
                {
                    if (options.NumberOfInstallments > 0)
                    {
                        var val = HttpContext.GetLocalResourceObject("PaymentInfoGrid", "lbInstallments.Text",
                                                                     CultureInfo.CurrentCulture);
                        if (val != null)
                        {
                            paymentOption += val + " ";
                        }

                        paymentOption += options.NumberOfInstallments + " ";
                    }
                }
                var JPOptions = orderPayment_VO1.PaymentOptions as JapanPaymentOptions_V01;
                if (null != JPOptions)
                {
                    paymentOption +=
                        JapanPaymentOptions_V01.JapanPaymentOptionTypeToHPSOptionType(JPOptions.ChargeMode) + " ";
                    paymentOption += ((JPOptions.FirstInstallmentMonth > 0)
                                          ? JPOptions.FirstInstallmentMonth.ToString()
                                          : string.Empty) + " ";
                    paymentOption += ((JPOptions.FirstBonusMonth > 0)
                                          ? JPOptions.FirstBonusMonth.ToString()
                                          : string.Empty) + " ";
                    paymentOption += ((JPOptions.SecondBonusMonth > 0)
                                          ? JPOptions.SecondBonusMonth.ToString()
                                          : string.Empty) + " ";
                    if (orderPayment.Card.IssuerAssociation == IssuerAssociationType.AmericanExpress)
                        paymentOption +=
                            JapanPaymentOptions_V01.JapanPaymentOptionTypeToHPSOptionType(JapanPayOptionType.LumpSum);
                }
            }
            catch
            {
            }

            return paymentOption;
        }

        private static string getDescription(DistributorShoppingCartItem ci, string countryCode)
        {
            // The commented cod results in defect 25569 where description/flavor is repeated.
            //if (!string.IsNullOrEmpty(countryCode))
            //{
            //    if (countryCode.ToUpper() == "MY")
            //    {
            //        return ci.Description + " " + ci.Flavor;
            //    }
            //}

            // rare occasions there are still html tags in the description
            // need to cut this from the root cause. but emails never know what part of the system is broken
            // so will strip out here as well defect 24320
            try
            {
                // Taiwan has flavor as part of description. So, don't double add flavor. Inconsistent across countries.
                string description = string.Empty;
                if (countryCode == "TW")
                {
                    
                   description = Regex.Replace(Regex.Replace(ci.Description, "<p>", "", RegexOptions.IgnoreCase),
                                                       "</p>", "",
                                                       RegexOptions.IgnoreCase) +
                                         (ci.Description.IndexOf(ci.Flavor.Trim()) != -1 ? "" : (" " + ci.Flavor));

                    description = HTMLHelper.ToText(description);
                }
                else
                {
                    description = ci.Description;
                }
                return description;
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        ///     Method used to get the locale based on the country of processing
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>string</returns>
        private static string GetLocale(string locale)
        {
            if (!string.IsNullOrEmpty(locale))
            {
                return locale.Replace('-', '_');
            }
            return locale;
        }

        /// <summary>
        ///     Method used to get the language based on the country of processing
        /// </summary>
        /// <returns>string</returns>
        private static string GetLanguage(string locale)
        {
            if (!string.IsNullOrEmpty(locale))
            {
                var culture = new CultureInfo(locale);
                return culture.TwoLetterISOLanguageName;
            }

            return "en";
        }
    }
}