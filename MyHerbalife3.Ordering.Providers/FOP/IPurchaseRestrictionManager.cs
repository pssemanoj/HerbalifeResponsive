using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.FOP
{
    public interface IPurchaseRestrictionManager
    {
        /// <summary>
        /// current active limits - only one type at a time
        /// </summary>
        Dictionary<int, PurchasingLimits_V01> PurchasingLimits { get; set; }

        /// <summary>
        /// all applicable limits
        /// </summary>
        List<PurchasingLimits> ApplicableLimits { get; set; }

        /// <summary>
        /// can purchase P type
        /// </summary>
        bool CanPurchasePType { get; set; }

        /// <summary>
        /// can purchase any online
        /// </summary>
        bool CanPurchase { get; set; }

        bool ExtendRestrictionErrorMessage { get; set; }

        /// <summary>
        /// DS tins
        /// </summary>
        List<TaxIdentification> Tins { get; set; }

        /// <summary>
        /// from distributor profile
        /// </summary>
        string OrderSubType { get; set; }


        /// <summary>
        /// called on check out
        /// </summary>
        /// <param name="distributorId"></param>
        /// <returns></returns>
        bool PurchasingLimitsAreExceeded(string distributorId, MyHLShoppingCart cart);

        /// <summary>
        /// get purchasing limits passing order month
        /// </summary>
        /// <param name="orderMonth"></param>
        /// <returns></returns>
        PurchasingLimits_V01 GetPurchasingLimits(int orderMonth);

        /// <summary>
        /// update database
        /// </summary>
        /// <param name="limits"></param>
        void UpdatePurchasingLimits(PurchasingLimits_V01 limits);

        /// <summary>
        /// reload purchasing limits
        /// </summary>
        /// <param name="orderMonth"></param>
        void ReloadPurchasingLimits(int orderMonth);

        /// <summary>
        /// get purhcase Restriction
        /// </summary>
        void SetPurchaseRestriction();

        /// <summary>
        /// called after order placed
        /// </summary>
        /// <param name="shoppingCart"></param>
        /// <param name="distributorId"></param>
        /// <param name="countryCode"></param>
        void ReconcileAfterPurchase(MyHLShoppingCart shoppingCart, string distributorId, string countryCode);
    }
}
