using System;
using System.Collections.Generic;
using System.Web.Security;
//using HL.Distributor.ValueObjects;
using MyHerbalife3.Core.DistributorProvider.DistributorSvc;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public class OnlineDistributor : MembershipUser<Distributor_V01>
    {
        public OnlineDistributor(Distributor_V01 distributor,
                                 Guid authenticationToken,
                                 bool isLockedOut,
                                 bool isApproved)
            : base(
                distributor.ID, distributor, authenticationToken.ToString(), Membership.Provider.Name, isLockedOut,
                isApproved)
        {
            base.Value = distributor;
            AuthenticationToken = authenticationToken;
            Roles = new List<string>(6); // to 6 roles defined in HLRoleProvider
            //ShowAllInventory = HLConfigManager.Configurations.DOConfiguration.InventoryViewDefault == 0;
        }

        /// <summary>
        ///     The authentication token associated with this instance
        /// </summary>
        public Guid AuthenticationToken { get; set; }

        // [Obsolete("Roles not stored with online distributor under split verticals providers")]
        public List<string> Roles { get; internal set; }

        /// <summary>
        ///     Indicates whether the underlying Distributor object is populated or not
        /// </summary>
        public bool IsPopulated
        {
            get { return Value != null; }
        }

        /// <summary>
        ///     Indicates whether the roles were assigned or not
        /// </summary>
        public DateTime? RolesAssignedTime { get; set; }

        /// <summary>
        ///     Gets whether the distributor has a sub
        /// </summary>
        public bool HasSubscription
        {
            get
            {
                return (Value != null
                        && Value.Subscription != null
                        && Value.Subscription.IsInEffect);
            }
        }

        /// <summary>
        ///     The distributor already has this months Magazine
        /// </summary>
        public bool HasCurrentTodayMagazine
        {
            get { return (null != Value) ? Value.TodaysMagazine : false; }
        }

        /// <summary>
        ///     The distributor's last inventory view
        /// </summary>
        public bool ShowAllInventory { get; set; }

        /// <summary>
        ///     Gets whether the subscription expires within <see cref="SpecialMessagingConfiguration" /> days
        /// </summary>
        public bool SubscriptionIsExpiring
        {
            get
            {
                if (!HasSubscription)
                {
                    return true;
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets whether the card expires within <see cref="SpecialMessagingConfiguration" /> days
        /// </summary>
        public bool CardIsExpiring
        {
            get
            {
                var result = false;
                if (HasSubscription)
                {
                    return result;
                }
                return result;
            }
        }

        /// <summary>
        ///     Gets the status of bill to
        /// </summary>
        public bool BillSuccess
        {
            get
            {
                var result = false;
                if (HasSubscription)
                {
                    return Value.Subscription.SuccessFlag;
                }
                return result;
            }
        }

        /// <summary>
        ///     Gets the bizworks turned on
        /// </summary>
        public bool IsInEffect
        {
            get
            {
                var result = false;
                if (HasSubscription)
                {
                    return Value.Subscription.IsInEffect;
                }
                return result;
            }
        }

        public string DistributorName
        {
            get
            {
                var name = string.Empty;
                var distributor = Value;
                if (null != distributor)
                {
                    if (distributor.Localname != null && !string.IsNullOrEmpty(distributor.Localname.First) &&
                        !string.IsNullOrEmpty(distributor.Localname.Last))
                    {
                        name = string.Format("{0} {1}", distributor.Localname.First, distributor.Localname.Last);
                    }
                    if (distributor.EnglishName != null && !string.IsNullOrEmpty(distributor.EnglishName.First) &&
                        !string.IsNullOrEmpty(distributor.EnglishName.Last))
                    {
                        name = string.Format("{0} {1}", distributor.EnglishName.First, distributor.EnglishName.Last);
                    }
                }

                return name;
            }
        }

        public string DistributorLocaleName
        {
            get
            {
                var name = string.Empty;
                var distributor = Value;
                if (null != distributor)
                {
                    if (distributor.Localname != null && !string.IsNullOrEmpty(distributor.Localname.First) &&
                        !string.IsNullOrEmpty(distributor.Localname.Last))
                    {
                        name = string.Format("{0} {1}", distributor.Localname.First, distributor.Localname.Last);
                    }
                }

                return name;
            }
        }

        public string CurrentLoggedInCountry { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1} ", Value.ID, AuthenticationToken);
        }
    }
}