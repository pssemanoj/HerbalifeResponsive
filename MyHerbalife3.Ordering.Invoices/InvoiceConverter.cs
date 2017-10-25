#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Invoices.Helper;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Invoices
{
    public class InvoiceConverter
    {
        public MemberInvoice ConvertToMemberInvoice(InvoiceModel invoiceModel)
        {
            MemberInvoiceStatus status;
            if (!Enum.TryParse(invoiceModel.Status, true, out status))
            {
                LoggerHelper.Error("ConvertToMemberInvoice - InvoiceProvider: InvoiceStatus parse error");
            }

            MemberInvoiceType type;
            if (!Enum.TryParse(invoiceModel.Type, true, out type))
            {
                LoggerHelper.Error("ConvertToMemberInvoice - InvoiceProvider: InvoiceType parse error");
            }

            var memberInvoice = new MemberInvoice
            {
                Address = null != invoiceModel.Address
                    ? ConvertToMemberInvoiceAddress(invoiceModel.Address)
                    : null,
                CreatedDate = invoiceModel.CreatedDate,
                CustomerId = invoiceModel.CustomerId,
                Email = invoiceModel.Email,
                FirstName = invoiceModel.FirstName,
                LastName = invoiceModel.LastName,
                Id = invoiceModel.Id,
                InvoiceDate = invoiceModel.InvoiceDate,
                InvoiceType = type,
                MemberId = invoiceModel.MemberId.ToUpper(),
                MemberInvoiceNumber = invoiceModel.MemberInvoiceNumber,
                Notes = invoiceModel.Notes,
                OrderId = invoiceModel.OrderId,
                Phone = invoiceModel.Phone,
                Price =
                    invoiceModel.InvoicePrice != null ? ConvertToMemberInvoicePrice(invoiceModel.InvoicePrice) : null,
                Items =
                    invoiceModel.InvoiceLines != null && invoiceModel.InvoiceLines.Any()
                        ? ConvertToMemberInvoiceItems(invoiceModel.InvoiceLines, invoiceModel.Id)
                        : null,
                Status = status,
                ShippingMethod = invoiceModel.ShippingMethod,
                OrderSource = invoiceModel.OrderSource,
                PriceType = invoiceModel.PricingType,
                Source = invoiceModel.Source,
                InvoiceShipToAddress = invoiceModel.InvoiceShipToAddress,
                IsFaceToFace = invoiceModel.IsFaceToFace,
                ApplicationCountryCode = invoiceModel.ApplicationCountryCode,
                IsPurchased = invoiceModel.isPurchased,
                IsProfitable = invoiceModel.isProfitable,
                ReceiptChannel = convertReceiptChannel(invoiceModel.ReceiptChannel),
                PaymentType =invoiceModel.PaymentType


            };
            return memberInvoice;
        }
        public string convertReceiptChannel(string ReceiptChannel)
        {
            if (ReceiptChannel == "ClubSaleReceipt")
            {
                return "Club Visit/Sale";
            }
            else if (ReceiptChannel == "ProductSaleReceipt")
            {
                return "Retail Sale";
            }
            else
            {
                return ReceiptChannel;
            }
        }
        public MemberInvoiceAddress ConvertToMemberInvoiceAddress(InvoiceAddressModel address)
        {
            return new MemberInvoiceAddress
            {
                Address01 = address.Address1,
                Address02 = address.Address2,
                City = address.City,
                County = address.County,
                CountryCode = address.Country,
                PostalCode = address.PostalCode,
                State = address.State
            };
        }

        public MemberInvoicePrice ConvertToMemberInvoicePrice(InvoicePriceModel price)
        {
            return new MemberInvoicePrice
            {
                CalcTaxAmount = price.CalcTaxAmount,
                CalcDiscountAmount = price.CalcDiscountAmount,
                CalcShippingAmount = price.CalcShippingAmount,
                DiscountAmount = price.DiscountAmount,
                DiscountPercentage = price.DiscountPercentage,
                MemberDiscount = price.MemberDiscount,
                MemberFreight = price.MemberFreight,
                MemberTax = price.MemberTax,
                ShippingAmount = price.ShippingAmount,
                ShippingPercentage = price.ShippingPercentage,
                SubTotal = price.SubTotal,
                TaxAmount = price.TaxAmount,
                TaxPercentage = price.TaxPercentage,
                TotalDiscountedAmount = price.TotalDiscountedAmount,
                TotalDue = price.TotalDue,
                TotalVolumePoints = price.TotalVolumePoints,
                MemberStaticDiscount = price.MemberStaticDiscount,
                TotalEarnBase = price.TotalEarnBase,
                TotalYourPrice = price.TotalYourPrice,
                MemberTotal = price.MemberTotal,
                Profit = price.Profit,
                ProfitPercentage = price.ProfitPercentage
            };
        }

        public List<MemberInvoiceItem> ConvertToMemberInvoiceItems(IEnumerable<InvoiceLineModel> invoiceLines,
            int invoiceId)
        {
            return invoiceLines.Select(invoiceLine => new MemberInvoiceItem
            {
                MemberInvoiceId = invoiceId,
                CalcDiscountedAmount = invoiceLine.CalcDiscountedAmount,
                UnitEarnBase = invoiceLine.EarnBase,
                FreightCharge = invoiceLine.FreightCharge,
                ProductCategory = invoiceLine.ProductCategory,
                ProductName = invoiceLine.ProductName,
                ProductType = invoiceLine.ProductType,
                Quantity = invoiceLine.Quantity,
                UnitRetailPrice = invoiceLine.RetailPrice,
                Sku = invoiceLine.Sku,
                StockingSku = invoiceLine.StockingSku,
                TotalEarnBase = invoiceLine.TotalEarnBase,
                TotalRetailPrice = invoiceLine.TotalRetailPrice,
                TotalVolumePoint = invoiceLine.TotalVolumePoint,
                UnitVolumePoint = invoiceLine.VolumePoint,
                YourPrice = invoiceLine.YourPrice
            }).ToList();
        }

        public Address_V01 ConvertToAddress(InvoiceAddressModel invoiceAddress, string countryCode)
        {
            return new Address_V01
            {
                City = invoiceAddress.City,
                Country = countryCode,
                CountyDistrict = invoiceAddress.County,
                Line1 = invoiceAddress.Address1,
                Line2 = invoiceAddress.Address2,
                PostalCode = invoiceAddress.PostalCode,
                StateProvinceTerritory = invoiceAddress.State
            };
        }


        public IEnumerable<InvoiceModel> ConvertToInvoiceModels(IEnumerable<MemberInvoice> memberInvoices,
            string locale)
        {
            return memberInvoices.Select(memberInvoice => ConvertToInvoiceModel(memberInvoice, locale)).ToList();
        }

        public InvoiceModel ConvertToInvoiceModel(MemberInvoice memberInvoice, string locale)
        {
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);

            return new InvoiceModel
            {
                Address = memberInvoice.Address != null ? ConvertToInvoiceAddress(memberInvoice.Address) : null,
                CreatedDate = memberInvoice.CreatedDate,
                CustomerId = memberInvoice.CustomerId,
                Email = memberInvoice.Email,
                MemberInvoiceNumber = memberInvoice.MemberInvoiceNumber,
                FirstName = memberInvoice.FirstName,
                LastName = memberInvoice.LastName,
                Id = memberInvoice.Id,
                InvoiceDate = memberInvoice.InvoiceDate,
                Phone = memberInvoice.Phone,
                OrderId = memberInvoice.OrderId,
                Status = memberInvoice.Status.ToString(),
                Total = memberInvoice.Price != null ? memberInvoice.Price.TotalDue : 0,
                DisplayTotal = memberInvoice.Price != null ? memberInvoice.Price.TotalDue.FormatPrice() : "0.00",
                Notes = memberInvoice.Notes,
                MemberId = memberInvoice.MemberId.ToUpper(),
                TotalVolumePoints = memberInvoice.Price != null ? memberInvoice.Price.TotalVolumePoints : 0,
                Type = memberInvoice.InvoiceType.ToString(),
                ShippingMethod = memberInvoice.ShippingMethod,
                InvoiceShipToAddress = memberInvoice.InvoiceShipToAddress,
                PricingType = memberInvoice.PriceType,
                OrderSource = memberInvoice.OrderSource,
                Source = memberInvoice.Source,
                ReceiptChannel = memberInvoice.ReceiptChannel,
                DisplayReceiptChannel = invoiceProvider.GetDisplayReceiptChannel(locale, memberInvoice.ReceiptChannel.ToString()),
                IsFaceToFace = memberInvoice.IsFaceToFace,
                InvoicePrice = memberInvoice.Price != null ? ConvertToInvoicePrice(memberInvoice.Price) : null,
                IsGdoMemberPricing = memberInvoice.InvoiceType.ToString() == "Distributor",
                Vat = (string.IsNullOrEmpty(memberInvoice.Vat) && locale.Substring(3) == "GB")
                    ? (string)
                        HttpContext.GetLocalResourceObject("/Views/Invoice/Display.cshtml",
                            "VatEmptyValue", CultureInfo.CurrentCulture)
                    : memberInvoice.Vat,
                ApplicationCountryCode = memberInvoice.ApplicationCountryCode,
                DisplayInvoiceStatus = invoiceProvider.GetDisplayInvoiceStatus(locale, memberInvoice.Status.ToString()),
                DisplayPaymentType= invoiceProvider.GetDisplayPaymentType(locale,memberInvoice.PaymentType.ToString()),
                DisplayInvoiceType = invoiceProvider.GetDisplayInvoiceType(locale, memberInvoice.InvoiceType.ToString()),
                isProfitable = memberInvoice.IsProfitable,
                isPurchased=memberInvoice.IsPurchased,
                PaymentType=memberInvoice.PaymentType,
                



            };
        }

        public InvoicePriceModel ConvertToInvoicePrice(MemberInvoicePrice price)
        {
            return new InvoicePriceModel
            {
                CalcDiscountAmount = price.CalcDiscountAmount,
                CalcShippingAmount = price.CalcShippingAmount,
                CalcTaxAmount = price.CalcTaxAmount,
                DiscountAmount = price.DiscountAmount,
                DiscountPercentage = price.DiscountPercentage,
                MemberStaticDiscount = price.MemberStaticDiscount,
                MemberFreight = price.MemberFreight,
                MemberTax = price.MemberTax,
                TaxAmount = price.TaxAmount,
                TaxPercentage = price.TaxPercentage,
                SubTotal = price.SubTotal,
                ShippingAmount = price.ShippingAmount,
                ShippingPercentage = price.ShippingPercentage,
                TotalDiscountedAmount = price.TotalDiscountedAmount,
                TotalDue = price.TotalDue,
                DisplayCurrencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                TotalVolumePoints = price.TotalVolumePoints,
                MemberTotal = price.MemberTotal,
                Profit = price.Profit,
                ProfitPercentage = price.ProfitPercentage,
                TotalEarnBase = price.TotalEarnBase,
                TotalYourPrice = price.TotalYourPrice,
                MemberDiscount = price.MemberDiscount,
                DisplayCalculatedTax = price.CalcTaxAmount.FormatPrice(),
                DisplayDiscountedAmount = string.Format("-{0}", price.CalcDiscountAmount.FormatPrice()),
                DisplayMemberFreight = price.MemberFreight.FormatPrice(),
                DisplayMemberTax = price.MemberTax.FormatPrice(),
                DisplayMemberTotal = price.MemberTotal.FormatPrice(),
                DisplayProfit = price.Profit.FormatPrice(),
                DisplayShipping = price.ShippingAmount.FormatPrice(),
                DisplaySubtotal = price.SubTotal.FormatPrice(),
                DisplayTax = price.TaxAmount.FormatPrice(),
                DisplayTotalDue = price.TotalDue.FormatPrice(),
                DisplayTotalYourPrice = price.TotalYourPrice.FormatPrice()
            };
        }

        public InvoiceAddressModel ConvertToInvoiceAddress(MemberInvoiceAddress address)
        {
            return new InvoiceAddressModel
            {
                Address1 = address.Address01,
                Address2 = address.Address02,
                City = address.City,
                Country = address.CountryCode,
                County = address.County,
                PostalCode = address.PostalCode,
                State = address.State
            };
        }
        public ClubInvoiceModel ConvertToClubInvoiceLines(IEnumerable<MemberInvoiceItem> items,InvoiceModel InvoiceModel)
        {
            ClubInvoiceModel cm = new ClubInvoiceModel();
            if (InvoiceModel.ReceiptChannel == "ClubSaleReceipt" || InvoiceModel.ReceiptChannel == "Club Visit/Sale")
            {
                cm.ClubRecieptDisplayTotalDue = InvoiceModel.InvoicePrice.TotalDue.ToString("0.##");
                cm.ClubRecieptQuantity = items.FirstOrDefault().Quantity.ToString();
                cm.ClubRecieptTotalVolumePoints = items.FirstOrDefault().TotalVolumePoint.ToString("0.##");
                cm.ClubRecieptProductName = items.FirstOrDefault().ProductName;
            }
            return cm;
               
        }
        public List<InvoiceLineModel> ConvertToInvoiceLines(IEnumerable<MemberInvoiceItem> items,
            int memberInvoiceId)
        {
            return items.Select(invoiceItem => new InvoiceLineModel
            {
                InvoiceId = memberInvoiceId,
                CalcDiscountedAmount = invoiceItem.CalcDiscountedAmount,
                DisplayCurrencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                EarnBase = invoiceItem.UnitEarnBase,
                FreightCharge = invoiceItem.FreightCharge,
                ProductCategory = invoiceItem.ProductCategory,
                ProductName = invoiceItem.ProductName,
                ProductType = invoiceItem.ProductType,
                Quantity = invoiceItem.Quantity,
                RetailPrice = invoiceItem.UnitRetailPrice,
                Sku = invoiceItem.Sku,
                StockingSku = invoiceItem.StockingSku,
                TotalEarnBase = invoiceItem.TotalEarnBase,
                TotalRetailPrice = invoiceItem.TotalRetailPrice,
                TotalVolumePoint = invoiceItem.TotalVolumePoint,
                VolumePoint = invoiceItem.UnitVolumePoint,
                YourPrice = invoiceItem.YourPrice,
                DisplayYourPrice = invoiceItem.YourPrice.FormatPrice(),
                DisplayRetailPrice = invoiceItem.UnitRetailPrice.FormatPrice(),
                DisplayTotalRetailPrice = invoiceItem.TotalRetailPrice.FormatPrice(),
                DisplayTotalVp = invoiceItem.TotalVolumePoint.FormatPrice(),
            }).ToList();
        }
    }
}