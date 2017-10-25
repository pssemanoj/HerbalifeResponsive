namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class CardPaymentViewModel : PaymentViewModel
    {
        public CreditCardViewModel Card { get; set; }

        /// <summary>
        ///     Gets or sets the transaction ID.
        /// </summary>
        /// <value>The transaction ID.</value>
        /// <remarks>
        ///     The transaction ID on the system that preformed the card authorization / capture.(reference = foreign system
        ///     transaction ID)
        /// </remarks>
        public string TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the authorization code.
        /// </summary>
        /// <value>The authorization code given by the merchant payment gateway.</value>
        public string AuthorizationCode { get; set; }

        /// <summary>
        ///     Gets or sets the authorization method.
        /// </summary>
        /// <value>The authorization method.</value>
        public string AuthorizationMethod { get; set; }


        /// <summary>
        ///     Gets or sets the authorization merchant account.
        /// </summary>
        /// <value>The merchant account to be used for processing this card.</value>
        public string AuthorizationMerchantAccount { get; set; }

        /// <summary>
        /// </summary>
        public string ClientReferenceNumber { get; set; }

        /// <summary>
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// </summary>
        public string SettlementMerchantNumber { get; set; }

        public int NumberOfInstallments { get; set; }
    }
}