using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ServiceProvider.OrderSvc
{
    public partial class Address_V01
    {
        public Address_V01()
        {
        }

        public Address_V01(string line1, string line2, string line3, string line4, string city, string countyDistrict, string stateProvinceTerritory, string country, string postalCode)
        {
            this.Line1 = line1;
            this.Line2 = line2;
            this.Line3 = line3;
            this.Line4 = line4;
            this.City = city;
            this.CountyDistrict = countyDistrict;
            this.StateProvinceTerritory = stateProvinceTerritory;
            this.Country = country;
            this.PostalCode = postalCode;
        }
    }

    public partial class Charge_V01
    {
        public Charge_V01()
        {
        }

        public Charge_V01(ChargeTypes chargeType, decimal amount)
            : base()
        {
            ChargeType = chargeType;
            Amount = amount;
        }
    }

    public partial class PurchasingLimits_V01 : ICloneable
    {
        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    public partial class CreditCard
    {
        /// <summary>Gets the IssuerAssociationType enum from the 2 character HPS card notation</summary>
        /// <param name="cardType">The HPS card notation</param>
        /// <returns>The appropriate IssuerAssociation</returns>
        public static IssuerAssociationType GetCardType(string cardType)
        {
            IssuerAssociationType CreditCardType;

            switch (cardType)
            {
                case "MC":
                    {
                        CreditCardType = IssuerAssociationType.MasterCard;
                        break;
                    }
                case "VI":
                    {
                        CreditCardType = IssuerAssociationType.Visa;
                        break;
                    }
                case "DI":
                    {
                        CreditCardType = IssuerAssociationType.Discover;
                        break;
                    }
                case "AX":
                    {
                        CreditCardType = IssuerAssociationType.AmericanExpress;
                        break;
                    }
                case "AL":
                    {
                        CreditCardType = IssuerAssociationType.APlus;
                        break;
                    }
                case "QH":
                    {
                        CreditCardType = IssuerAssociationType.MyKey;
                        break;
                    }
                case "DN":
                    {
                        CreditCardType = IssuerAssociationType.Diners;
                        break;
                    }
                case "JC":
                    {
                        CreditCardType = IssuerAssociationType.JCB;
                        break;
                    }
                case "MS":
                    {
                        CreditCardType = IssuerAssociationType.Maestro;
                        break;
                    }
                case "IO":
                    {
                        CreditCardType = IssuerAssociationType.PaymentGateway;
                        break;
                    }
                case "DB":
                    {
                        CreditCardType = IssuerAssociationType.GenericDebitCard;
                        break;
                    }
                case "HI":
                    {
                        CreditCardType = IssuerAssociationType.HiperCard;
                        break;
                    }
                case "NA":
                    {
                        CreditCardType = IssuerAssociationType.TarjetaNaranja;
                        break;
                    }
                case "OC":
                    {
                        CreditCardType = IssuerAssociationType.Oca;
                        break;
                    }
                case "CC":
                    {
                        CreditCardType = IssuerAssociationType.Cabal;
                        break;
                    }
                case "CP":
                    {
                        CreditCardType = IssuerAssociationType.Panal;
                        break;
                    }
                case "DM":
                    {
                        CreditCardType = IssuerAssociationType.VietnamLocalCard;
                        break;
                    }
                case "EL":
                    {
                        CreditCardType = IssuerAssociationType.ELO;
                        break;
                    }
                case "MR":
                    {
                        CreditCardType = IssuerAssociationType.MIR;
                        break;
                    }
                default:
                    {
                        CreditCardType = IssuerAssociationType.Visa;
                        break;
                    }

            }

            return CreditCardType;
        }

        /// <summary>Return the HPS notation for the Card Type</summary>
        /// <param name="cardType">The CardType enum</param>
        /// <returns>A 2 character string denoting the card type for HPS</returns>
        public static string CardTypeToHPSCardType(IssuerAssociationType cardType)
        {
            string creditCardType;

            switch (cardType)
            {
                case IssuerAssociationType.MasterCard:
                    {
                        creditCardType = "MC";
                        break;
                    }
                case IssuerAssociationType.Visa:
                    {
                        creditCardType = "VI";
                        break;
                    }
                case IssuerAssociationType.Discover:
                    {
                        creditCardType = "DI";
                        break;
                    }
                case IssuerAssociationType.AmericanExpress:
                    {
                        creditCardType = "AX";
                        break;
                    }
                case IssuerAssociationType.JCB:
                    {
                        creditCardType = "JC";
                        break;
                    }
                case IssuerAssociationType.APlus:
                    {
                        creditCardType = "AL";
                        break;
                    }
                case IssuerAssociationType.MyKey:
                    {
                        creditCardType = "QH";
                        break;
                    }
                case IssuerAssociationType.Diners:
                    {
                        creditCardType = "DN";
                        break;
                    }
                case IssuerAssociationType.Maestro:
                    {
                        creditCardType = "MS";
                        break;
                    }
                case IssuerAssociationType.PaymentGateway:
                    {
                        creditCardType = "IO";
                        break;
                    }
                case IssuerAssociationType.GenericDebitCard:
                    {
                        creditCardType = "DB";
                        break;
                    }
                case IssuerAssociationType.HiperCard:
                    {
                        creditCardType = "HI";
                        break;
                    }
                case IssuerAssociationType.TarjetaNaranja:
                    {
                        creditCardType = "NA";
                        break;
                    }
                case IssuerAssociationType.Oca:
                    {
                        creditCardType = "OC";
                        break;
                    }
                case IssuerAssociationType.Cabal:
                    {
                        creditCardType = "CC";
                        break;
                    }
                case IssuerAssociationType.Panal:
                    {
                        creditCardType = "CP";
                        break;
                    }
                case IssuerAssociationType.VietnamLocalCard:
                    {
                        creditCardType = "DM";
                        break;
                    }
                case IssuerAssociationType.ELO:
                    {
                        creditCardType = "EL";
                        break;
                    }
                case IssuerAssociationType.MIR:
                    {
                        creditCardType = "MR";
                        break;
                    }
                default:
                    {
                        creditCardType = "VI";
                        break;
                    }
            }

            return creditCardType;
        }
    }

    public partial class JapanPaymentOptions_V01
    {
        /// <summary>Convert a JapanPayment Option Type to a HPS notation</summary>
        /// <param name="optionType">The JapanPaymentOptionType to convert</param>
        /// <returns>The two character HPS notation</returns>
        public static string JapanPaymentOptionTypeToHPSOptionType(JapanPayOptionType optionType)
        {
            string option = string.Empty;
            switch (optionType)
            {
                case JapanPayOptionType.LumpSum:
                    {
                        option = "LS";
                        break;
                    }
                case JapanPayOptionType.Revolving:
                    {
                        option = "RV";
                        break;
                    }
                case JapanPayOptionType.Installments:
                    {
                        option = "IN";
                        break;
                    }
                case JapanPayOptionType.Bonus1Month:
                    {
                        option = "B1";
                        break;
                    }
                case JapanPayOptionType.Bonus2Month:
                    {
                        option = "B2";
                        break;
                    }
            }

            return option;
        }
    }

    public partial class OrderItem_V01
    {
        public OrderItem_V01()
        {
            this.ReferenceID = Guid.NewGuid();
        }

        public OrderItem_V01(string id, string SKU, int quantity)
        {
            //this.ID = id;
            this.SKU = SKU;
            this.Quantity = quantity;
        }
    }

    public partial class ItemTotal_V01
    {
        public ItemTotal_V01() { }

        public ItemTotal_V01(string sku, int quanitity, decimal totalPrice, decimal retailPrice, decimal discountedPrice, decimal totalTax, decimal earnBase, ChargeList chargeList)
        {
            this.SKU = sku;
            this.Quantity = quanitity;
            this.LinePrice = totalPrice;
            this.RetailPrice = retailPrice;
            this.DiscountedPrice = discountedPrice;
            this.LineTax = totalTax;
            this.EarnBase = earnBase;
            this.ChargeList = chargeList;
        }
    }

    public partial class ItemTotalsList 
    {
        /// <summary>
        /// Populates a item totals list with the items provided by an
        /// order request.
        /// </summary>
        public void InitializeTotals(OrderItems items)
        {
            foreach (OrderItem_V01 item in items)
            {
                item.ReferenceID = Guid.NewGuid();
                // Clean up SKUs.
                item.SKU = item.SKU.Trim();
                // Add a new Item total which corresponds to the OrderItem to the list.
                ItemTotal_V01 itemTotal
                    = new ItemTotal_V01(item.SKU, item.Quantity, 0, 0, 0, 0, 0, new ChargeList());
                // Associate item total to order item for later lookups.
                itemTotal.ReferenceID = item.ReferenceID;
                this.Add(itemTotal);
            }
        }
    }
}

namespace MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc
{
    public class PromoSKUInfo
    {
        /// <summary>
        /// promotion SKU
        /// </summary>
        public string SKU;

        /// <summary>
        /// promotion integer SKU
        /// </summary>
        public int IntSKU;

        /// <summary>
        /// Promotion type 
        /// </summary>
        public PromotionType Type;

        /// <summary>
        /// Promotion code.
        /// </summary>
        public string PromotionCode { get; set; }
    }

    public class PromotionCode
    {
        public const string NewSrPromotion = "NewSRPromo";
        public const string ChinaBadgePromo = "ChinaBadgePromo";
        public const string SRExcellentPromo = "SRExcellentPromo";
        public const string SRGrowingPromo = "SRGrowingPromo";
        public const string SRChinaPromo = "SRChinaPromo";
        public const string PCPromo = "PCPromo";
        public const string PCPromoSurvey = "PCPromoSurvey";
    }
}