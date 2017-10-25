// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OnlineDistributorHelper.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Helper class for online distributor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using MyHerbalife3.Shared.Providers;
using System;
using System.Linq;
using System.Collections.Generic;
using HL.Common.ValueObjects;
using HL.Distributor.ValueObjects;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Test.Helpers
{
    /// <summary>
    /// Helper class for shopping cart items.
    /// </summary>
    internal static class OnlineDistributorHelper
    {
        /// <summary>
        /// Gets a cart item.
        /// </summary>
        /// <param name="distributor">The distributor.</param>
        /// <returns>
        /// The online distributor object.
        /// </returns>
        public static DistributorOrderingProfile GetOnlineDistributor(string distributor)
        {
            return GetOnlineDistributor(distributor, null, null);
        }

        /// <summary>
        /// Gets a cart item.
        /// </summary>
        /// <param name="distributor">The distributor.</param>
        /// <param name="country">The country.</param>
        /// <param name="tins">The tin codes.</param>
        /// <returns>
        /// The online distributor object.
        /// </returns>
        public static DistributorOrderingProfile GetOnlineDistributor(string distributor, string country, IEnumerable<string> tins)
        {
            if (string.IsNullOrEmpty(country))
            {
                country = "US";
            }
            
            // Getting tin list.
            var tinList = new DistributorTinList();
            if (tins != null)
            {
                tinList.AddRange(tins.Select(
                    tin =>
                    new TaxIdentification
                        {
                            ID = tin,
                            IDType = new TaxIdentificationType(tin)
                                {
                                    EffectiveDate = DateTime.Now.AddDays(-1),
                                    ExpirationDate = DateTime.Now.AddDays(1),
                                },
                            CountryCode = country
                        }).ToList());
            }

            return new DistributorOrderingProfile
            {
                Id = distributor,
                CurrentLoggedInCountry = country,
                TinList = tinList,
                //Level = LevelType.SC
            };
        }
    }
}
