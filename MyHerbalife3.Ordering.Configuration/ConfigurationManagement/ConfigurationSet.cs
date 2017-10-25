using System.Collections.Generic;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
	public class ConfigurationSet
    {

#region Fields
        private const string SectionHasErrors = "The {0} Section is invalid.\r\n";
        private const string SectionsHaveErrors = "The Sections: {0} are invalid.\r\n";
#endregion

#region Fields
        private string _locale = string.Empty;
		private AddressingConfiguration _addressingConfiguration = null;
		private APFConfiguration _APFConfiguration = null;
		private CheckoutConfiguration _checkoutConfiguration = null;
		private DOConfiguration _DOConfiguration = null;
		private PaymentsConfiguration _paymentsConfiguration = null;
		private ShoppingCartConfiguration _shoppingCartConfiguration = null;
		private RulesConfiguration _rulesConfiguration = null;
        private PickupOrDeliveryConfiguration _pickupOrDeliveryConfiguration = null;
        private List<string> _badSections = null;
#endregion

#region Construction
		public ConfigurationSet()
		{

		}

        public ConfigurationSet(System.Configuration.Configuration config)
		{
			_locale = config.AppSettings.Settings["Locale"].Value;
			_paymentsConfiguration = PaymentsConfiguration.GetConfiguration(config);
			_addressingConfiguration = AddressingConfiguration.GetConfiguration(config);
			_APFConfiguration = APFConfiguration.GetConfiguration(config);
			_checkoutConfiguration = CheckoutConfiguration.GetConfiguration(config);
			_DOConfiguration = DOConfiguration.GetConfiguration(config);
			_paymentsConfiguration = PaymentsConfiguration.GetConfiguration(config);
			_shoppingCartConfiguration = ShoppingCartConfiguration.GetConfiguration(config);
			_rulesConfiguration = RulesConfiguration.GetConfiguration(config);
            _pickupOrDeliveryConfiguration = PickupOrDeliveryConfiguration.GetConfiguration(config);
            ValidateState();
		}
#endregion

#region Properties
		/// <summary>The Locale of this Configuration set</summary>
		public string Locale
		{
			get { return _locale; }
		}

		/// <summary>Locale specific Addressing configurations for this Configuration set</summary>
		public AddressingConfiguration AddressingConfiguration
		{
			get { return _addressingConfiguration; }
            set { _addressingConfiguration = value; }
		}

		/// <summary>Locale specific APF configurations for this Configuration set</summary>
		public APFConfiguration APFConfiguration
		{
			get { return _APFConfiguration; }
            set { _APFConfiguration = value; }
		}

		/// <summary>Locale specific Checkout configurations for this Configuration set</summary>
		public CheckoutConfiguration CheckoutConfiguration
		{
			get { return _checkoutConfiguration; }
            set { _checkoutConfiguration = value; }
        }

		/// <summary>Locale specific DO configurations for this Configuration set</summary>
		public DOConfiguration DOConfiguration
		{
			get { return _DOConfiguration; }
            set { _DOConfiguration = value; }
        }

		/// <summary>Locale specific Payments configurations for this Configuration set</summary>
		public PaymentsConfiguration PaymentsConfiguration
		{
			get { return _paymentsConfiguration; }
            set { _paymentsConfiguration = value; }
		}

		/// <summary>Locale specific Shopping Cart configurations for this Configuration set</summary>
		public ShoppingCartConfiguration ShoppingCartConfiguration
		{
			get { return _shoppingCartConfiguration; }
            set { _shoppingCartConfiguration = value; }
        }

		/// <summary>Locale specific Rules configurations for this Configuration set</summary>
		public RulesConfiguration RulesConfiguration
		{
			get { return _rulesConfiguration; }
		}

        /// <summary>Locale specific Pickup Or Delivery configurations for this Configuration set</summary>
        public PickupOrDeliveryConfiguration PickupOrDeliveryConfiguration
        {
            get { return _pickupOrDeliveryConfiguration; }
            set { _pickupOrDeliveryConfiguration = value; }
        }

        /// <summary>Informs whether any sections had errors loading</summary>
        public bool HasErrors
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get;
            set;
        }

#endregion

#region Private methods
        private void ValidateState()
        {
            if(string.IsNullOrEmpty(_locale)) HasErrors = true;
            CheckSection(_paymentsConfiguration, "PaymentsConfiguration");
            CheckSection(_addressingConfiguration, "AddressingConfiguration");
            CheckSection(_APFConfiguration, "APFConfiguration");
            CheckSection(_checkoutConfiguration, "CheckoutConfiguration");
            CheckSection(_DOConfiguration, "DOConfiguration");
            CheckSection(_paymentsConfiguration, "PaymentsConfiguration");
            CheckSection(_shoppingCartConfiguration, "ShoppingCartConfiguration");
            CheckSection(_rulesConfiguration, "RulesConfiguration");
            CheckSection(_pickupOrDeliveryConfiguration, "PickupOrDeliveryConfiguration");
            //korea PIPA phase 2 - added MyHerbalife config section
            if (null != _badSections)
            {
                ErrorMessage = (_badSections.Count == 1) ? string.Format(SectionHasErrors, string.Join(", ", _badSections.ToArray())) : string.Format(SectionsHaveErrors, string.Join(", ", _badSections.ToArray()));
            }
        }

        private void CheckSection(HLConfiguration config, string sectionName)
        {

            if (null == config)
            {
                if(null == _badSections)
                {
                    _badSections = new List<string>();
                }

                HasErrors = true;
                _badSections.Add(sectionName);
            }
        }
#endregion

    }
}
