using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Providers.China;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;


namespace MyHerbalife3.Ordering.Providers
{
    public class SessionLessInfoManager
    {
        const string ThisClassName = "SessionLessInfoManager";

        #region singleton

        private SessionLessInfoManager() { }

        static SessionLessInfoManager _Instance = null;
        public static SessionLessInfoManager Instance
        {
            get
            {
                if (_Instance == null) _Instance = new SessionLessInfoManager();

                return _Instance;
            }
        }

        #endregion

        public void LoadOrderAcknowledgement(SessionInfo sessionInfo, string orderNumber, string locale, string distributorID)
        {
            DelayedOrderAcknowledgementManager mgr = new DelayedOrderAcknowledgementManager();
            var cart = mgr.LoadOrderAcknowledgement(orderNumber, locale, distributorID);

            var sessLessInfo = new SessionInfo
            {
                OrderNumber = orderNumber,
                ShoppingCart = cart,
            };

            sessionInfo.SessionLessInfo = sessLessInfo;
            sessionInfo.UseSessionLessInfo = true;
        }
    }

    public class DelayedOrderAcknowledgementManager
    {
        const string ThisClassName = "DelayedOrderAcknowledgementManager";

        public MyHLShoppingCart LoadOrderAcknowledgement(string orderNumber, string locale, string distributorID)
        {
            var req = new GetPendingOrdersRequest_V03();
            req.CountryCode = locale;
            req.DistributorId = distributorID;
            req.IncludeXmlOrderData = true;
            req.OrderNumberList = new List<string> { orderNumber };

            var proxy = ServiceClientProvider.GetOrderServiceProxy();

            var rsp = proxy.GetOrders(new GetOrdersRequest1(req)).GetOrdersResult as GetPendingOrdersResponse_V01;
            if (!Helper.Instance.ValidateResponse(rsp))
            {
                Helper.Instance.LogError(rsp.Message, string.Format("{0}.LoadOrderAcknowledgement()", ThisClassName));
                return null;
            }

            var cart = new MyHLShoppingCart();

            var pendingOdr = rsp.PendingOrders.FirstOrDefault();

            XElement xRoot = XElement.Parse(pendingOdr.XmlOrderData.OuterXml);

            var xBTOrder = GetChild(xRoot, "BTOrder");
            var xOrder = GetChild(xRoot, "Order");

            #region ShoppingCart

            cart.CountryCode = GetChildValue(xBTOrder, "countryOfProcessingField");
            cart.DistributorID = GetChildValue(xBTOrder, "distributorIDField");
            cart.Locale = GetChildValue(xBTOrder, "localeField");
            cart.OrderMonth = int.Parse(GetChildValue(xBTOrder, "orderMonthField"));
            cart.OrderSubType = GetChildValue(xBTOrder, "orderSubTypeField");
            cart.SMSNotification = GetChildValue(xBTOrder, "sMSNumberField");

            cart.EmailAddress = GetChildValue(xRoot, "Email");

            #region DeliveryInfo

            var xShipment = GetChild(xOrder, "Shipment");

            ShippingInfo_V01 shpInfoV01 = new ShippingInfo_V01
            {
                Carrier = GetChildValue(xShipment, "Carrier"),
                FreightVariant = GetChildValue(xShipment, "FreightVariant"),
                Phone = GetChildValue(xShipment, "Phone"),
                Recipient = GetChildValue(xShipment, "Recipient"),
                ShippingMethodID = GetChildValue(xShipment, "ShippingMethodID"),
                Version = GetChildValue(xShipment, "Version"),
                WarehouseCode = GetChildValue(xShipment, "WarehouseCode"),
                DeliveryNickName = GetChildValue(xShipment, "DeliveryNickName"),
            };

            var phone = shpInfoV01.Phone;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                // trim the start and end with -
                if (phone.StartsWith("-") && phone.EndsWith("-") && (phone.Length >= 2)) shpInfoV01.Phone = phone.Substring(1, phone.Length - 2);
            }

            var deliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo
            {
                //Description
                DeliveryNickName = shpInfoV01.DeliveryNickName,
                FreightCode = shpInfoV01.ShippingMethodID,
                FreightVariant = shpInfoV01.FreightVariant, // somehow FreightVariant will be null, due to the prop get implementation
                //Id
                WarehouseCode = shpInfoV01.WarehouseCode,
            };

            deliveryInfo.Option = DeliveryOptionTypes.Convert(shpInfoV01.FreightVariant);

            cart.FreightCode = deliveryInfo.FreightCode;

            #region ShippingAddress_V01

            var shippingAddr = new ServiceProvider.ShippingSvc.ShippingAddress_V01
            {
                Alias = shpInfoV01.DeliveryNickName,
                //AreaCode
                //DisplayName
                Phone = shpInfoV01.Phone,
                Recipient = shpInfoV01.Recipient,
            };

            #region Address_V01

            var xAddress = GetChild(xShipment, "Address");
            var addr = new ServiceProvider.ShippingSvc.Address_V01
            {
                City = GetChildValue(xAddress, "City"),
                Country = GetChildValue(xAddress, "Country"),
                CountyDistrict = GetChildValue(xAddress, "CountyDistrict"),
                Line1 = GetChildValue(xAddress, "Line1"),
                Line2 = GetChildValue(xAddress, "Line2"),
                Line3 = GetChildValue(xAddress, "Line3"),
                Line4 = GetChildValue(xAddress, "Line4"),
                PostalCode = GetChildValue(xAddress, "PostalCode"),
                StateProvinceTerritory = GetChildValue(xAddress, "StateProvinceTerritory"),
            };

            shippingAddr.Address = addr;

            #endregion

            deliveryInfo.Address = shippingAddr;

            #endregion

            #region HandlingInfo_V01

            var xHandling = GetChild(xOrder, "Handling");
            HandlingInfo_V01 handlingInfoV01 = new HandlingInfo_V01
            {
                PickupName = GetChildValue(xHandling, "PickupName"),
                ShippingInstructions = GetChildValue(xHandling, "ShippingInstructions"),
                Version = GetChildValue(xHandling, "Version"),
            };

            deliveryInfo.Instruction = handlingInfoV01.ShippingInstructions;

            #endregion

            cart.DeliveryInfo = deliveryInfo;

            #endregion

            #region ShoppingCartItems, CartItems

            cart.ShoppingCartItems = new List<DistributorShoppingCartItem>();
            cart.CartItems = new ServiceProvider.CatalogSvc.ShoppingCartItemList();

            var itemList = new List<DistributorShoppingCartItem>();
            var cartItemList = new List<ServiceProvider.CatalogSvc.ShoppingCartItem_V01>();

            var xOrderItemsField = GetChild(xBTOrder, "orderItemsField");
            var xBTItemList = GetChildList(xOrderItemsField, "Item");

            var xOrderItems = GetChild(xOrder, "OrderItems");
            var xItemList = GetChildList(xOrderItems, "Item");

            foreach (var xItem in xItemList)
            {
                var sku = GetChildValue(xItem, "SKU");

                #region Shared.Providers.DistributorShoppingCartItem

                DistributorShoppingCartItem item = new DistributorShoppingCartItem
                {
                    Description = GetChildValue(xItem, "Description"),
                    IsPromo = GetChildValue<bool>(xItem, "IsPromo"),
                    RetailPrice = GetChildValue<decimal>(xItem, "RetailPrice"),
                    Quantity = GetChildValue<int>(xItem, "Quantity"),
                    SKU = sku,
                    VolumePoints = GetChildValue<decimal>(xItem, "VolumePoint"),
                };

                var xBTItem = LookupBTOrderItem(xBTItemList, sku);
                if (xBTItem != null)
                {
                    item.DiscountPrice = GetChildValue<decimal>(xBTItem, "discountAmountField");
                    item.EarnBase = GetChildValue<decimal>(xBTItem, "earnBaseField");
                    item.Flavor = GetChildValue(xBTItem, "flavorField");
                }

                #endregion

                #region ShoppingCartItem_V01

                var cartItem = new ServiceProvider.CatalogSvc.ShoppingCartItem_V01
                {
                    IsPromo = GetChildValue<bool>(xItem, "IsPromo"),
                    Quantity = GetChildValue<int>(xItem, "Quantity"),
                    SKU = sku,
                };

                #endregion

                itemList.Add(item);
                cartItemList.Add(cartItem);
            }

            var itemListSorted = itemList.OrderBy(x => x.SKU).ToList();
            var cartItemListSorted = cartItemList.OrderBy(x => x.SKU).ToList();

            cart.ShoppingCartItems.AddRange(itemListSorted);
            cart.CartItems.AddRange(cartItemListSorted);

            #endregion

            #region Totals

            var xPricing = GetChild(xOrder, "Pricing");

            #region ChargeList

            var xChargeList = GetChild(xPricing, "ChargeList");
            ChargeList chargeList = new ChargeList();

            var xChargeList_List = GetChildList(xChargeList, "Charge");
            foreach (var xCharge in xChargeList_List)
            {
                #region Charge_V01

                if (GetAttributeValue(xCharge, "type") == "Charge_V01")
                {
                    Charge_V01 chargeV01 = new Charge_V01(ChargeTypes.None, 0)
                    {
                        Amount = GetChildValue<decimal>(xCharge, "Amount"),
                        TaxAmount = GetChildValue<decimal>(xCharge, "TaxAmount"),
                        Type = GetChildValue(xCharge, "Type"),
                        Version = GetChildValue(xCharge, "Version"),
                    };

                    chargeList.Add(chargeV01);
                }

                #endregion
            }

            #endregion

            #region ItemTotalsList

            var xItemTotalsList = GetChild(xPricing, "ItemTotalsList");
            var itemTotalsList = new ServiceProvider.OrderSvc.ItemTotalsList();

            var xItemTotalsList_List = GetChildList(xItemTotalsList, "ItemTotal");
            foreach (var xItemTotal in xItemTotalsList_List)
            {
                #region ItemTotal_V01

                ItemTotal_V01 itemTotalV01 = new ItemTotal_V01
                {
                    AfterDiscountTax = GetChildValue<decimal>(xItemTotal, "AfterDiscountTax"),
                    ChargeList = new ChargeList(), // just empty list
                    Discount = GetChildValue<decimal>(xItemTotal, "Discount"),
                    DiscountedPrice = GetChildValue<decimal>(xItemTotal, "DiscountedPrice"),
                    EarnBase = GetChildValue<decimal>(xItemTotal, "EarnBase"),
                    LinePrice = GetChildValue<decimal>(xItemTotal, "LinePrice"),
                    LineTax = GetChildValue<decimal>(xItemTotal, "LineTax"),
                    //ProductType = GetChildValue(xItemTotal, "ProductType"),
                    Quantity = GetChildValue<int>(xItemTotal, "Quantity"),
                    RetailPrice = GetChildValue<decimal>(xItemTotal, "RetailPrice"),
                    SKU = GetChildValue(xItemTotal, "SKU"),
                    TaxableAmount = GetChildValue<decimal>(xItemTotal, "TaxableAmount"),
                    VolumePoints = GetChildValue<decimal>(xItemTotal, "VolumePoints"),
                    Version = GetChildValue(xItemTotal, "Version"),
                };

                itemTotalsList.Add(itemTotalV01);

                var itemMatch = cart.ShoppingCartItems.FirstOrDefault(x => x.SKU == itemTotalV01.SKU);
                if (itemMatch != null)
                {
                    itemMatch.DiscountPrice = itemTotalV01.DiscountedPrice;
                }

                #endregion
            }

            #endregion

            #region OrderFreight

            var xOrderFreight = GetChild(xPricing, "OrderFreight");

            #region Packages

            var xPackages = GetChild(xOrderFreight, "Packages");
            List<Package> packageList = new List<Package>();

            var xPackageList = GetChildList(xPackages, "Package");
            foreach (var xPck in xPackageList)
            {
                #region Package

                var pck = new Package
                {
                    Packagetype = GetChildValue(xPck, "Packagetype"),
                    Unit = GetChildValue<int>(xPck, "Unit"),
                    Volume = GetChildValue<decimal>(xPck, "Volume"),
                };

                #endregion

                packageList.Add(pck);
            }

            #endregion

            var ordFreight = new OrderFreight
            {
                ActualFreight = GetChildValue<decimal>(xOrderFreight, "ActualFreight"),
                BeforeDiscountFreight = GetChildValue<decimal>(xOrderFreight, "BeforeDiscountFreight"),
                BeforeWeight = GetChildValue<decimal>(xOrderFreight, "BeforeWeight"),
                CaseRate = GetChildValue<decimal>(xOrderFreight, "CaseRate"),
                FreightCharge = GetChildValue<decimal>(xOrderFreight, "FreightCharge"),
                Insurance = GetChildValue<decimal>(xOrderFreight, "Insurance"),
                InsuranceRate = GetChildValue<decimal>(xOrderFreight, "InsuranceRate"),
                MaterialFee = GetChildValue<decimal>(xOrderFreight, "MaterialFee"),
                Packages = packageList,
                PackageType = GetChildValue(xOrderFreight, "PackageType"),
                PackageWeight = GetChildValue<decimal>(xOrderFreight, "PackageWeight"),
                PhysicalWeight = GetChildValue<decimal>(xOrderFreight, "PhysicalWeight"),
                Unit = GetChildValue<int>(xOrderFreight, "Unit"),
                VolumeWeight = GetChildValue<decimal>(xOrderFreight, "VolumeWeight"),
                Weight = GetChildValue<decimal>(xOrderFreight, "Weight"),
            };

            #endregion

            #region OrderTotals_V02

            if (GetAttributeValue(xPricing, "type") == "OrderTotals_V02")
            {
                var total = new OrderTotals_V02
                {
                    AmountDue = GetChildValue<decimal>(xPricing, "AmountDue"),
                    BalanceAmount = GetChildValue<decimal>(xPricing, "BalanceAmount"),
                    ChargeList = chargeList,
                    DiscountedItemsTotal = GetChildValue<decimal>(xPricing, "DiscountedItemsTotal"),
                    DiscountPercentage = GetChildValue<decimal>(xPricing, "DiscountPercentage"),
                    DiscountType = GetChildValue(xPricing, "DiscountType"),
                    Donation = GetChildValue<decimal>(xPricing, "Donation"),
                    Donation2 = GetChildValue<decimal>(xPricing, "Donation2"),
                    ExciseTax = GetChildValue<decimal>(xPricing, "ExciseTax"),
                    IcmsTax = GetChildValue<decimal>(xPricing, "IcmsTax"),
                    IpiTax = GetChildValue<decimal>(xPricing, "IpiTax"),
                    ItemsTotal = GetChildValue<decimal>(xPricing, "ItemsTotal"),
                    ItemTotalsList = itemTotalsList,
                    LiteratureRetailAmount = GetChildValue<decimal>(xPricing, "LiteratureRetailAmount"),
                    MiscAmount = GetChildValue<decimal>(xPricing, "MiscAmount"),
                    OrderFreight = ordFreight,
                    PricingServerName = GetChildValue(xPricing, "PricingServerName"),
                    ProductRetailAmount = GetChildValue<decimal>(xPricing, "ProductRetailAmount"),
                    ProductTaxTotal = GetChildValue<decimal>(xPricing, "ProductTaxTotal"),
                    PromotionRetailAmount = GetChildValue<decimal>(xPricing, "PromotionRetailAmount"),
                    TaxableAmountTotal = GetChildValue<decimal>(xPricing, "TaxableAmountTotal"),
                    TaxAfterDiscountAmount = GetChildValue<decimal>(xPricing, "TaxAfterDiscountAmount"),
                    TaxAmount = GetChildValue<decimal>(xPricing, "TaxAmount"),
                    TaxBeforeDiscountAmount = GetChildValue<decimal>(xPricing, "TaxBeforeDiscountAmount"),
                    TotalEarnBase = GetChildValue<decimal>(xPricing, "TotalEarnBase"),
                    TotalItemDiscount = GetChildValue<decimal>(xPricing, "TotalItemDiscount"),
                    Version = GetChildValue(xPricing, "Version"),
                    VolumePoints = GetChildValue<decimal>(xPricing, "VolumePoints"),
                };

                cart.Totals = total;
            }

            #endregion

            #endregion

            #endregion

            var xEmailInfoField = GetChild(xBTOrder, "emailInfoField");

            cart.InvoiceOption = GetChildValue(xEmailInfoField, "invoiceOptionField");

            return cart;
        }

        XElement GetChild(XElement node, string name)
        {
            return node.Elements().FirstOrDefault(x => x.Name.LocalName == name);
        }

        List<XElement> GetChildList(XElement node, string name)
        {
             // bug fix for 225890
            if(node!=null)
                return node.Elements().Where(x => x.Name.LocalName == name).ToList();
            return new List<XElement>();
        }

        XElement LookupBTOrderItem(List<XElement> xBTItemList, string sku)
        {
            foreach (var xItem in xBTItemList)
            {
                // something wrong the BT xml, as at Jan/2015, flavorField looks like the SKU data.
                var xMatch = xItem.Elements().FirstOrDefault(x => (x.Name.LocalName == "flavorField") && (x.Value == sku));
                if (xMatch != null) return xItem;
            }

            return null;
        }

        string GetAttributeValue(XElement node, string name)
        {
            var attb = node.Attributes().FirstOrDefault(x => x.Name.LocalName == name);
            if (attb == null) return null;

            string ret = attb.Value;
            var nmspcIdx = ret.LastIndexOf(':');
            if (nmspcIdx >= 0) ret = ret.Substring(nmspcIdx + 1);

            return ret;
        }

        string GetChildValue(XElement node, string name)
        {
            var elem = GetChild(node, name);
            return (elem != null) ? elem.Value : null;
        }

        T GetChildValue<T>(XElement node, string name) where T : struct
        {
            var val = GetChildValue(node, name);
            if (string.IsNullOrWhiteSpace(val)) return default(T);

            var t = typeof(T);

            if (t == typeof(bool))
            {
                bool b;
                if (bool.TryParse(val, out b)) return (T)Convert.ChangeType(b, typeof(T));
            }
            else if (t == typeof(decimal))
            {
                decimal d;
                if (decimal.TryParse(val, out d)) return (T)Convert.ChangeType(d, typeof(T));
            }
            else if (t == typeof(int))
            {
                int i;
                if (int.TryParse(val, out i)) return (T)Convert.ChangeType(i, typeof(T));
            }

            return default(T);
        }
    }
}