#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class CreditCardViewModel
    {
        /// <summary>
        ///     Gets or sets the account number (credit card number).
        /// </summary>
        /// <value>The account number.</value>
        public string AccountNumber { get; set; }

        /// <summary>
        ///     Gets or sets the expiration date of the card.
        /// </summary>
        /// <value>The expiration.</value>
        /// <remarks>
        ///     Note that the year and month portions will be considered and the
        ///     rest of the date /time might not for comparison purposes
        /// </remarks>
        public DateTime Expiration { get; set; }

        /// <summary>
        ///     Gets or sets the CVV.
        /// </summary>
        /// <value>The CVV.</value>
        /// <remarks>
        ///     The special printed extra digits on the back of some credit cards. (3 digits on the back of Visa and MC, other
        ///     on other cards).
        /// </remarks>
        public string Cvv { get; set; }

        /// <summary>
        ///     Gets or sets the billing address associated with this card.
        /// </summary>
        /// <value>The billing address.</value>
        public AddressViewModel BillingAddress { get; set; }

        /// <summary>
        ///     Gets or sets the name on the card.
        /// </summary>
        /// <value>The name of the card holder as it appears on the card.</value>
        public string NameOnCard { get; set; }

        /// <summary>
        ///     Gets or sets the issuer association (Visa, MC , Amex etc.)
        /// </summary>
        /// <value>The issuer association.</value>
        public string IssuerAssociation { get; set; }


        /// <summary>
        ///     Gets or sets the issuing bank ID.
        /// </summary>
        /// <value>The issuing bank ID.</value>
        /// <remarks>
        ///     In countries such as Japan, Korea etc, a local bank may issue a card that is not Visa etc.
        ///     In these cases, a 2 letter "pay code" using HP3k as basis for the codes will be passed here.
        /// </remarks>
        public string IssuingBankId { get; set; }
    }
}