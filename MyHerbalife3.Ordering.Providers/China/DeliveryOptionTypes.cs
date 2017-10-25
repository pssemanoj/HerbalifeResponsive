using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.China
{
    /// <summary>
    /// China DO DeliveryOptionTypes
    /// </summary>
    public class DeliveryOptionTypes
    {
        // R:\China DO\ChinaDO Delivery Type Information Sharing.msg

        // select distinct [OrderDeliveryType]
        // from [CNSMS].[dbo].[OrderDelivery] (nolock)

        /// <summary>
        /// Shipping
        /// </summary>
        public const string EXP = "EXP";

        /// <summary>
        /// Pickup from Store
        /// </summary>
        public const string SD = "SD";

        /// <summary>
        /// Pickup Order A
        /// </summary>
        public const string PUCA = "PUCA";

        /// <summary>
        /// Pickup Order A+
        /// </summary>
        public const string PUCAplus = "PUCA+";

        /// <summary>
        /// Pickup Order B
        /// </summary>
        public const string PUCB = "PUCB";

        /// <summary>
        /// Pickup Order B+
        /// </summary>
        public const string PUCBplus = "PUCB+";

        private DeliveryOptionTypes() { }

        #region DeliveryTypeList

        /// <summary>
        /// Mapping to <see cref="DeliveryOptionType"/> .
        /// </summary>
        public static readonly Dictionary<string, DeliveryOptionType> DeliveryTypeMapping = new Dictionary<string, DeliveryOptionType>{
            { EXP       , DeliveryOptionType.Shipping },
            { SD        , DeliveryOptionType.Pickup },
            { PUCA      , DeliveryOptionType.PickupFromCourier },
            { PUCAplus  , DeliveryOptionType.PickupFromCourier },
            { PUCB      , DeliveryOptionType.PickupFromCourier },
            { PUCBplus  , DeliveryOptionType.PickupFromCourier },
        };

        #endregion

        /// <summary>
        /// Convert to <see cref="DeliveryOptionType"/> .
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <remarks>
        /// MyHerbalife3.Ordering.Providers\Shipping\ShippingProvider_CN.cs look for coding "shippingInfo.AddressType ="
        /// </remarks>
        public static DeliveryOptionType Convert(string code)
        {
            if (DeliveryTypeMapping.ContainsKey(code)) return DeliveryTypeMapping[code];

            return DeliveryOptionType.Unknown;
        }
    }
}
