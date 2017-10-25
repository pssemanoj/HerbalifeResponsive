using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class PickupOrDeliveryConfiguration : HLConfiguration
    {
        #region Construction

        public PickupOrDeliveryConfiguration()
        {
        }

        public static PickupOrDeliveryConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return HLConfiguration.GetConfiguration(config, "PickupOrDelivery") as PickupOrDeliveryConfiguration;
        }

        #endregion Construction

        #region Config Properties

        /// <summary>
        /// this attribute specify if certain locale is allowed for showing PickupOrDelivery Page
        /// </summary>
        //[ConfigurationProperty("pickupOrDeliveryControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string PickupOrDeliveryControl
        //{
        //    get
        //    {
        //        return (string)this["pickupOrDeliveryControl"];
        //    }
        //    set
        //    {
        //        this["pickupOrDeliveryControl"] = value;
        //    }
        //}

        //[ConfigurationProperty("pickupQueryInput", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string PickupQueryInput
        //{
        //    get
        //    {
        //        return (string)this["pickupQueryInput"];
        //    }
        //    set
        //    {
        //        this["pickupQueryInput"] = value;
        //    }
        //}

        //[ConfigurationProperty("shippingInstructionPage", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string ShippingInstructionPage
        //{
        //    get
        //    {
        //        return (string)this["shippingInstructionPage"];
        //    }
        //    set
        //    {
        //        this["shippingInstructionPage"] = value;
        //    }
        //}

        //[ConfigurationProperty("pickupInstructionPage", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string PickupInstructionPage
        //{
        //    get
        //    {
        //        return (string)this["pickupInstructionPage"];
        //    }
        //    set
        //    {
        //        this["pickupInstructionPage"] = value;
        //    }
        //}

        [ConfigurationProperty("allowPickup", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowPickup
        {
            get
            {
                return (bool)this["allowPickup"];
            }
            set
            {
                this["allowPickup"] = value;
            }
        }
        [ConfigurationProperty("PickupHaveMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupHaveMessage
        {
            get
            {
                return (bool)this["PickupHaveMessage"];
            }
            set
            {
                this["PickupHaveMessage"] = value;
            }
        }

        [ConfigurationProperty("DeliveryOptionHaveDropDown", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DeliveryOptionHaveDropDown
        {
            get
            {
                return (bool)this["DeliveryOptionHaveDropDown"];
            }
            set
            {
                this["DeliveryOptionHaveDropDown"] = value;
            }
        }


        [ConfigurationProperty("allowShipToCourier", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowShipToCourier
        {
            get
            {
                return (bool)this["allowShipToCourier"];
            }
            set
            {
                this["allowShipToCourier"] = value;
            }
        }

        [ConfigurationProperty("allowPickUpFromCourier", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowPickUpFromCourier
        {
            get
            {
                return (bool)this["allowPickUpFromCourier"];
            }
            set
            {
                this["allowPickUpFromCourier"] = value;
            }
        }

        [ConfigurationProperty("autoDisplayPickUpFromCourierPopUp", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AutoDisplayPickUpFromCourierPopUp
        {
            get
            {
                return (bool)this["autoDisplayPickUpFromCourierPopUp"];
            }
            set
            {
                this["autoDisplayPickUpFromCourierPopUp"] = value;
            }
        }

        /// <summary>
        /// Indicates the courier type to be disable when more than one option is confgured.
        /// </summary>
        [ConfigurationProperty("disabledCourierType", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DisabledCourierType
        {
            get
            {
                return this["disabledCourierType"] as string;
            }
            set
            {
                this["disabledCourierType"] = value;
            }
        }

        [ConfigurationProperty("blockedZipCodesForCourier", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string BlockedZipCodesForCourier
        {
            get
            {
                return this["blockedZipCodesForCourier"] as string;
            }
            set
            {
                this["blockedZipCodesForCourier"] = value;
            }
        }


        [ConfigurationProperty("shippingInstructionsHaveTime", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShippingInstructionsHaveTime
        {
            get
            {
                return (bool)this["shippingInstructionsHaveTime"];
            }
            set
            {
                this["shippingInstructionsHaveTime"] = value;
            }
        }

        [ConfigurationProperty("shippingInstructionsTimeMandatory", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool ShippingInstructionsTimeMandatory
        {
            get
            {
                return (bool)this["shippingInstructionsTimeMandatory"];
            }
            set
            {
                this["shippingInstructionsTimeMandatory"] = value;
            }
        }

        [ConfigurationProperty("shippingMethodsHaveDropDown", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShippingMethodsHaveDropDown
        {
            get
            {
                return (bool)this["shippingMethodsHaveDropDown"];
            }
            set
            {
                this["shippingMethodsHaveDropDown"] = value;
            }
        }

        [ConfigurationProperty("shippingMethodsVPLimit", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int ShippingMethodsVPLimit
        {
            get
            {
                return (int)this["shippingMethodsVPLimit"];
            }
            set
            {
                this["shippingMethodsVPLimit"] = value;
            }
        }

        [ConfigurationProperty("pickupInstructionsHaveName", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupInstructionsHaveName
        {
            get
            {
                return (bool)this["pickupInstructionsHaveName"];
            }
            set
            {
                this["pickupInstructionsHaveName"] = value;
            }
        }

        /// <summary>
        /// Regular expression to validate the pickup name.
        /// </summary>
        [ConfigurationProperty("pickupNameRegExp", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupNameRegExp
        {
            get { return this["pickupNameRegExp"] as string; }
            set { this["pickupNameRegExp"] = value; }
        }

        [ConfigurationProperty("pickupInstructionsHaveTinID", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupInstructionsHaveTinID
        {
            get
            {
                return (bool)this["pickupInstructionsHaveTinID"];
            }
            set
            {
                this["pickupInstructionsHaveTinID"] = value;
            }
        }

		[ConfigurationProperty("pickupInstructionsHaveRGNumber", DefaultValue = false, IsRequired = false, IsKey = false)]
		public bool PickupInstructionsHaveRGNumber
		{
			get
			{
				return (bool)this["pickupInstructionsHaveRGNumber"];
			}
			set
			{
				this["pickupInstructionsHaveRGNumber"] = value;
			}
		}

        [ConfigurationProperty("pickupInstructionsHavePhone", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupInstructionsHavePhone
        {
            get
            {
                return (bool)this["pickupInstructionsHavePhone"];
            }
            set
            {
                this["pickupInstructionsHavePhone"] = value;
            }
        }

        [ConfigurationProperty("pickupInstructionsHaveTime", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupInstructionsHaveTime
        {
            get
            {
                return (bool)this["pickupInstructionsHaveTime"];
            }
            set
            {
                this["pickupInstructionsHaveTime"] = value;
            }
        }

        [ConfigurationProperty("pickupInstructionsHaveDate", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupInstructionsHaveDate
        {
            get
            {
                return (bool)this["pickupInstructionsHaveDate"];
            }
            set
            {
                this["pickupInstructionsHaveDate"] = value;
            }
        }

        [ConfigurationProperty("shippingInstructionsHaveDate", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShippingInstructionsHaveDate
        {
            get
            {
                return (bool)this["shippingInstructionsHaveDate"];
            }
            set
            {
                this["shippingInstructionsHaveDate"] = value;
            }
        }
        [ConfigurationProperty("CO2DOSMSNotification", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CO2DOSMSNotification
        {
            get
            {
                return (bool)this["CO2DOSMSNotification"];
            }
            set
            {
                this["CO2DOSMSNotification"] = value;
            }
        }

        [ConfigurationProperty("shippingMethodNeedsDisplay", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShippingMethodNeedsDisplay
        {
            get
            {
                return (bool)this["shippingMethodNeedsDisplay"];
            }
            set
            {
                this["shippingMethodNeedsDisplay"] = value;
            }
        }
        [ConfigurationProperty("autoSelectShippingaddres", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool autoSelectShippingaddres
        {
            get
            {
                return (bool)this["autoSelectShippingaddres"];
            }
            set
            {
                this["autoSelectShippingaddres"] = value;
            }
        }

        [ConfigurationProperty("pickUpPhoneHasPhoneMask", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickUpPhoneHasPhoneMask
        {
            get
            {
                return (bool)this["pickUpPhoneHasPhoneMask"];
            }
            set
            {
                this["pickUpPhoneHasPhoneMask"] = value;
            }
        }

        [ConfigurationProperty("pickUpPhoneMask", DefaultValue = "999-9999999", IsRequired = false, IsKey = false)]
        public string PickUpPhoneMask
        {
            get
            {
                return this["pickUpPhoneMask"] as string;
            }
            set
            {
                this["pickUpPhoneMask"] = value;
            }
        }

        [ConfigurationProperty("pickUpPhoneRegExp", DefaultValue = @"^(\d+)$", IsRequired = false, IsKey = false)]
        public string PickUpPhoneRegExp
        {
            get
            {
                return this["pickUpPhoneRegExp"] as string;
            }
            set
            {
                this["pickUpPhoneRegExp"] = value;
            }
        }

        [ConfigurationProperty("pickUpFromCourierPhoneRegExp", DefaultValue = @"^(\d+)$", IsRequired = false, IsKey = false)]
        public string PickUpFromcourierPhoneRegExp
        {
            get
            {
                return this["pickUpFromCourierPhoneRegExp"] as string;
            }
            set
            {
                this["pickUpFromCourierPhoneRegExp"] = value;
            }
        }

        [ConfigurationProperty("pickUpPhoneMaxLen", DefaultValue = 15, IsRequired = false, IsKey = false)]
        public int PickUpPhoneMaxLen
        {
            get
            {
                return (int)this["pickUpPhoneMaxLen"];
            }
            set
            {
                this["pickUpPhoneMaxLen"] = value;
            }
        }

        [ConfigurationProperty("hasAdditonalNumber", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasAdditonalNumber
        {
            get
            {
                return (bool)this["hasAdditonalNumber"];
            }
            set
            {
                this["hasAdditonalNumber"] = value;
            }
        }

        [ConfigurationProperty("displaySingleTextBoxMobileNo", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplaySingleTextBoxMobileNo
        {
            get
            {
                return (bool)this["displaySingleTextBoxMobileNo"];
            }
            set
            {
                this["displaySingleTextBoxMobileNo"] = value;
            }
        }

        [ConfigurationProperty("hasFreeFormShippingInstruction", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasFreeFormShippingInstruction
        {
            get
            {
                return (bool)this["hasFreeFormShippingInstruction"];
            }
            set
            {
                this["hasFreeFormShippingInstruction"] = value;
            }
        }

        [ConfigurationProperty("freeFormShippingInstructionMaxLength", DefaultValue = 90, IsRequired = false, IsKey = false)]
        public int FreeFormShippingInstructionMaxLength
        {
            get
            {
                return (int)this["freeFormShippingInstructionMaxLength"];
            }
            set
            {
                this["freeFormShippingInstructionMaxLength"] = value;
            }
        }

        [ConfigurationProperty("isPickupPhoneRequired", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool IsPickupPhoneRequired
        {
            get
            {
                return (bool)this["isPickupPhoneRequired"];
            }
            set
            {
                this["isPickupPhoneRequired"] = value;
            }
        }

        [ConfigurationProperty("isPickupFromCourierPhoneRequired", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsPickupFromCourierPhoneRequired
        {
            get
            {
                return (bool)this["isPickupFromCourierPhoneRequired"];
            }
            set
            {
                this["isPickupFromCourierPhoneRequired"] = value;
            }
        }


        [ConfigurationProperty("isPickupInstructionsRequired", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsPickupInstructionsRequired
        {
            get
            {
                return (bool)this["isPickupInstructionsRequired"];
            }
            set
            {
                this["isPickupInstructionsRequired"] = value;
            }
        }

        [ConfigurationProperty("shippingCodesAreConsolidated", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShippingCodesAreConsolidated
        {
            get
            {
                return (bool)this["shippingCodesAreConsolidated"];
            }
            set
            {
                this["shippingCodesAreConsolidated"] = value;
            }
        }

        /// <summary>
        /// This allows to show the pickup option module in checkout options.
        /// </summary>
        [ConfigurationProperty("pickupAllowForEventTicket", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool PickupAllowForEventTicket
        {
            get
            {
                return (bool)this["pickupAllowForEventTicket"];
            }
            set
            {
                this["pickupAllowForEventTicket"] = value;
            }
        }
        /// <summary>
        /// This allows to show the instruction for event ticket
        /// </summary>
        [ConfigurationProperty("shippingInstructionForEventTicket", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShippingInstructionForEventTicket
        {
            get
            {
                return (bool)this["shippingInstructionForEventTicket"];
            }
            set
            {
                this["shippingInstructionForEventTicket"] = value;
            }
        }

        /// <summary>
        /// Display shipping method for APF and ETO
        /// </summary>
        [ConfigurationProperty("showShippingMethodForAPFETO", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowShippingMethodForAPFETO
        {
            get
            {
                return (bool)this["showShippingMethodForAPFETO"];
            }
            set
            {
                this["showShippingMethodForAPFETO"] = value;
            }
        }

        /// <summary>
        /// The only allowed delivery option for ETO.
        /// If this parameter has a value, it will be the only option displayed in delivery option.
        /// </summary>
        [ConfigurationProperty("deliveryAllowedETO", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DeliveryAllowedETO
        {
            get { return (string)this["deliveryAllowedETO"]; }
            set { this["deliveryAllowedETO"] = value; }
        }

        /// <summary>
        /// The only allowed delivery option for HAP.
        /// If this parameter has a value, it will be the only option displayed in delivery option.
        /// </summary>
        [ConfigurationProperty("deliveryAllowedHAP", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DeliveryAllowedHAP
        {
            get { return (string)this["deliveryAllowedHAP"]; }
            set { this["deliveryAllowedHAP"] = value; }
        }

        /// <summary>
        /// This allows to show the pickup option module in checkout options.
        /// </summary>
        //[ConfigurationProperty("pickupOptionsForETO", DefaultValue = "false", IsRequired = false, IsKey = false)]
        //public bool PickupOptionsForETO
        //{
        //    get
        //    {
        //        return (bool)this["pickupOptionsForETO"];
        //    }
        //    set
        //    {
        //        this["pickupOptionsForETO"] = value;
        //    }
        //}        

        /// <summary>
        /// this attribute specify if certain locale is allowed for showing PickupOrDelivery Page
        /// </summary>
        [ConfigurationProperty("deliveryControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DeliveryControl
        {
            get
            {
                return (string)this["deliveryControl"];
            }
            set
            {
                this["deliveryControl"] = value;
            }
        }

        /// <summary>
        /// this attribute specify if certain locale is allowed for showing PickupOrDelivery Page
        /// </summary>
        [ConfigurationProperty("pickupControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupControl
        {
            get
            {
                return (string)this["pickupControl"];
            }
            set
            {
                this["pickupControl"] = value;
            }
        }

        /// <summary>
        /// To specify the delivery time estimated
        /// </summary>
        [ConfigurationProperty("showDeliveryTimeEstimated", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowDeliveryTimeEstimated
        {
            get
            {
                return (bool)this["showDeliveryTimeEstimated"];
            }
            set
            {
                this["showDeliveryTimeEstimated"] = value;
            }
        }

        /// <summary>
        /// To show the delivery time estimated when editing shipping information
        /// </summary>
        [ConfigurationProperty("showDeliveryTimeOnShoppingCart", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowDeliveryTimeOnShoppingCart
        {
            get
            {
                return (bool)this["showDeliveryTimeOnShoppingCart"];
            }
            set
            {
                this["showDeliveryTimeOnShoppingCart"] = value;
            }
        }
        /// <summary>
        /// To define when pickup from courier option have a date for instructions.
        /// </summary>
        [ConfigurationProperty("pickupFromCourierHaveDate", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupFromCourierHaveDate
        {
            get
            {
                return (bool)this["pickupFromCourierHaveDate"];
            }
            set
            {
                this["pickupFromCourierHaveDate"] = value;
            }
        }

        /// <summary>
        /// The URL for pickup from courier help
        /// </summary>
        [ConfigurationProperty("courierInfoUrl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string CourierInfoUrl
        {
            get
            {
                return (string)this["courierInfoUrl"];
            }
            set
            {
                this["courierInfoUrl"] = value;
            }
        }

        
        /// <summary>
        /// created to validate HKID to allow alphanumeric characters
        /// </summary>
        [ConfigurationProperty("tinCodeIDRegExp", DefaultValue = @"^[0-9a-zA-Z]*$", IsRequired = false, IsKey = false)]
        public string TinCodeIDRegExp
        {
            get
            {
                return this["tinCodeIDRegExp"] as string;
            }
            set
            {
                this["tinCodeIDRegExp"] = value;
            }
        }

        /// <summary>
        /// To indicate when there is a message to show in shipping address control.
        /// </summary>
        [ConfigurationProperty("showShippingAddressMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowShippingAddressMessage
        {
            get
            {
                return (bool)this["showShippingAddressMessage"];
            }
            set
            {
                this["showShippingAddressMessage"] = value;
            }
        }

        /// <summary>
        /// To indicate when there is a message to show in shipping address control.
        /// </summary>
        [ConfigurationProperty("populatePickupDate", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool PopulatePickupDate
        {
            get
            {
                return (bool)this["populatePickupDate"];
            }
            set
            {
                this["populatePickupDate"] = value;
            }
        }

        /// <summary>
        /// To hide wire transfer option for special warehouse location
        /// </summary>
        [ConfigurationProperty("hideWireForSpecialWhLocations", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HideWireForSpecialWhLocations
        {
            get
            {
                return (bool)this["hideWireForSpecialWhLocations"];
            }
            set
            {
                this["hideWireForSpecialWhLocations"] = value;
            }
            }

        /// <summary>
        /// Regular expression to validate the pickup name.
        /// </summary>
        [ConfigurationProperty("specialWhlocations", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SpecialWhlocations
        {
            get { return this["specialWhlocations"] as string; }
            set { this["specialWhlocations"] = value; }
        }

        /// <summary>
        /// To indicate the number of days for the start date of the calendar in pickup options control.
        /// </summary>
        [ConfigurationProperty("pickupStartDate", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int PickupInstructionsStartDate
        {
            get
            {
                return (int)this["pickupStartDate"];
            }
            set
            {
                this["pickupStartDate"] = value;
            }
        }

        /// <summary>
        /// To indicate the number of working days allow in the calendar for pick up.
        /// For working days, if working are saturday and sunday, you have to add 1 days to the count, e.g. for 5 working days, the value of the property has to be 6
        /// For not workings days (normal days), you have to substract 1 days to the count, e.g for 5 normal days, the value of the property has to be 4
        /// </summary>
        [ConfigurationProperty("allowDaysPickUp", DefaultValue = 14, IsRequired = false, IsKey = false)]
        public int AllowDaysPickUp
        {
            get
            {
                return (int)this["allowDaysPickUp"];
            }
            set
            {
                this["allowDaysPickUp"] = value;
            }
        }

        /// <summary>
        /// To indicate the type of sorting por pickup dropdown.
        /// </summary>
        [ConfigurationProperty("pickupLocationsOrderedList", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupLocationsOrderedList
        {
            get
            {
                return (bool)this["pickupLocationsOrderedList"];
            }
            set
            {
                this["pickupLocationsOrderedList"] = value;
            }
        }
        /// <summary>
        /// To show the HTML fragments on the basis of shipping method
        /// </summary>
        [ConfigurationProperty("differentfragmentforshippingmethod", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DifferentFragmentForShippingMethod
        {
            get { return (bool)this["differentfragmentforshippingmethod"]; }
            set { this["differentfragmentforshippingmethod"] = value; }
        }
        /// <summary>
        /// To show the HTML fragments on the basis of pickup method
        /// </summary>
        [ConfigurationProperty("differentFragmentForCOP1", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DifferentFragmentForCOP1
        {
            get { return (bool)this["differentFragmentForCOP1"]; }
            set { this["differentFragmentForCOP1"] = value; }
        }
        /// <summary>
        /// To Hide the primary address checkbox of AddressPopUp
        /// </summary>
        [ConfigurationProperty("hideprimaryaddresscheckbox", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HidePrimaryAddressCheckBox
        {
            get { return (bool)this["hideprimaryaddresscheckbox"]; }
            set { this["hideprimaryaddresscheckbox"] = value; }
        }
        /// <summary>
        /// To Set Day Interval for DeliveryTime before 12pm
        /// </summary>
        [ConfigurationProperty("deliverytimespanbefore12pm", DefaultValue = 14, IsRequired = false, IsKey = false)]
        public int DeliveryTimeSpanBefore12Pm
        {
            get { return (int)this["deliverytimespanbefore12pm"]; }
            set { this["deliverytimespanbefore12pm"] = value; }
        }
        /// <summary>
        /// To Set Day Interval for DeliveryTime on and after 12pm
        /// </summary>
        [ConfigurationProperty("deliverytimespanafter12pm", DefaultValue = 14, IsRequired = false, IsKey = false)]
        public int DeliveryTimeSpanAfter12Pm
        {
            get { return (int)this["deliverytimespanafter12pm"]; }
            set { this["deliverytimespanafter12pm"] = value; }
        }
        /// <summary>
        /// To check DeliveryTime Interval
        /// </summary>
        [ConfigurationProperty("shippingtimeoptioncheck", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShippingTimeOptionCheck
        {
            get { return (bool)this["shippingtimeoptioncheck"]; }
            set { this["shippingtimeoptioncheck"] = value; }
        }

        ///<summary>
        /// To Check Baidu Map is applicable or not
        /// </summary>
        [ConfigurationProperty("hasBaiduMap", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasBaiduMap
        {
            get { return (bool)this["hasBaiduMap"]; }
            set { this["hasBaiduMap"] = value; }
        }

        ///<summary>
        /// To Check Google Map is applicable or not
        /// </summary>
        [ConfigurationProperty("hasGoogleMap", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasGoogleMap
        {
            get { return (bool)this["hasGoogleMap"]; }
            set { this["hasGoogleMap"] = value; }
        }
        ///<summary>
        /// Get URL of Map
        /// </summary>
        [ConfigurationProperty("mapURL", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string MapURL
        {
            get { return this["mapURL"] as string;}
            set { this["mapURL"] = value; }
        }

        /// <summary>
        /// Display shipping method for pickup option.
        /// </summary>
        [ConfigurationProperty("pickupMethodHaveDropDown", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupMethodHaveDropDown
        {
            get
            {
                return (bool)this["pickupMethodHaveDropDown"];
            }
            set
            {
                this["pickupMethodHaveDropDown"] = value;
            }
        }

        /// <summary>
        /// Regular expression to validate the zip code for lookup.
        /// </summary>
        [ConfigurationProperty("zipCodeLookupRegExp", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ZipCodeLookupRegExp
        {
            get { return this["zipCodeLookupRegExp"] as string; }
            set { this["zipCodeLookupRegExp"] = value; }
        }

        /// <summary>
        /// Do not check Address Line 1 for Norway while placing orders
        /// </summary>
        [ConfigurationProperty("doNotCheckAddressLine1", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DoNotCheckAddressLine1
        {
            get
            {
                return (bool)this["doNotCheckAddressLine1"];
            }
            set
            {
                this["doNotCheckAddressLine1"] = value;
            }
        }


        /// <summary>
        /// Indicates to take the zip code also to define the warehouse
        /// </summary>
        [ConfigurationProperty("shipToZipCode", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShipToZipCode
        {
            get
            {
                return (bool)this["shipToZipCode"];
            }
            set
            {
                this["shipToZipCode"] = value;
            }
        }

        /// <summary>
        /// Indicates whether the delivery drop down should show a predefined pick up
        /// </summary>
        [ConfigurationProperty("hasPredefinedPickUp", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasPredefinedPickUp
        {
            get
            {
                return (bool)this["hasPredefinedPickUp"];
            }
            set
            {
                this["hasPredefinedPickUp"] = value;
            }
        }

        /// <summary>
        /// Indicates the predefined pick up location zip code
        /// </summary>
        [ConfigurationProperty("predefinedPickUpLocationZipCode", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int PredefinedPickUpLocationZipCode
        {
            get
            {
                return (int)this["predefinedPickUpLocationZipCode"];
            }
            set
            {
                this["predefinedPickUpLocationZipCode"] = value;
            }
        }

        /// <summary>
        /// Indicates the predefined pick up locationd name and nick name
        /// </summary>
        [ConfigurationProperty("predefinedPickUpLocationName", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PredefinedPickUpLocationName
        {
            get
            {
                return (string)this["predefinedPickUpLocationName"];
            }
            set
            {
                this["predefinedPickUpLocationName"] = value;
            }
        }

        /// <summary>
        /// Indicates if there is warehouse for special events
        /// </summary>
        [ConfigurationProperty("hasSpecialEventWareHouse", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasSpecialEventWareHouse
        {
            get
            {
                return (bool)this["hasSpecialEventWareHouse"];
            }
            set
            {
                this["hasSpecialEventWareHouse"] = value;
            }
        }

        /// <summary>
        /// Indicates the warehouse for special events where the items can only be pick it up from this warehouse
        /// </summary>
        [ConfigurationProperty("specialEventWareHouse", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SpecialEventWareHouse
        {
            get
            {
                return (string)this["specialEventWareHouse"];
            }
            set
            {
                this["specialEventWareHouse"] = value;
            }
        }

        /// <summary>
        /// Indicates whether a different label needs to display when Shipping is free (invoices)
        /// </summary>
        [ConfigurationProperty("hasFreeShippingLabel", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasFreeShippingLabel
        {
            get
            {
                return (bool)this["hasFreeShippingLabel"];
            }
            set
            {
                this["hasFreeShippingLabel"] = value;
            }
        }

        /// <summary>
        /// Display the Name field for PickupFromCourier
        /// </summary>
        [ConfigurationProperty("pickupFromCourierHaveName", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupFromCourierHaveName
        {
            get
            {
                return (bool)this["pickupFromCourierHaveName"];
            }
            set
            {
                this["pickupFromCourierHaveName"] = value;
            }
        }

        /// <summary>
        /// Display the Phone field for PickupFromCourier
        /// </summary>
        [ConfigurationProperty("pickupFromCourierHavePhone", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupFromCourierHavePhone
        {
            get
            {
                return (bool)this["pickupFromCourierHavePhone"];
            }
            set
            {
                this["pickupFromCourierHavePhone"] = value;
            }
        }

        [ConfigurationProperty("displayPickupFromCourierInstructions", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayPickupFromCourierInstructions
        {
            get
            {
                return (bool)this["displayPickupFromCourierInstructions"];
            }
            set
            {
                this["displayPickupFromCourierInstructions"] = value;
            }
        }

        [ConfigurationProperty("updatePrimaryPickupLocationOnCheckout", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool UpdatePrimaryPickupLocationOnCheckout
        {
            get { return (bool)this["updatePrimaryPickupLocationOnCheckout"]; }
            set { this["updatePrimaryPickupLocationOnCheckout"] = value; }
        }

        [ConfigurationProperty("pickupFromCourierMapURL", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupFromCourierMapURL
        {
            get
            {
                return (string)this["pickupFromCourierMapURL"];
            }
            set
            {
                this["pickupFromCourierMapURL"] = value;
            }
        }

        [ConfigurationProperty("setDSAddressforCashOnDelivery", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SetDSAddressforCashOnDelivery
        {
            get
            {
                return (bool)this["setDSAddressforCashOnDelivery"];
            }
            set
            {
                this["setDSAddressforCashOnDelivery"] = value;
            }
        }
        [ConfigurationProperty("validateSMSNotificationNumber", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool validateSMSNotificationNumber
        {
            get
            {
                return (bool)this["validateSMSNotificationNumber"];
            }
            set
            {
                this["validateSMSNotificationNumber"] = value;
            }
        }
        [ConfigurationProperty("MessageNotify", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool MessageNotify
        {
            get
            {
                return (bool)this["MessageNotify"];
            }
            set
            {
                this["MessageNotify"] = value;
            }
        }
        //show notification for 7-11
        [ConfigurationProperty("ShowNotification", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowNotification
        {
            get
            {
                return (bool)this["ShowNotification"];
            }
            set
            {
                this["ShowNotification"] = value;
            }
        }
        [ConfigurationProperty("SevenElevenCountry", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SevenElevenCountry
        {
            get
            {
                return (bool)this["SevenElevenCountry"];
            }
            set
            {
                this["SevenElevenCountry"] = value;
            }
        }

        [ConfigurationProperty("useXHLTables", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool UseXHLTables
        {
            get
            {
                return (bool)this["useXHLTables"];
            }
            set
            {
                this["useXHLTables"] = value;
            }
        }
        [ConfigurationProperty("PickupPhoneFormat", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PickupPhoneFormat
        {
            get
            {
                return (bool)this["PickupPhoneFormat"];
            }
            set
            {
                this["PickupPhoneFormat"] = value;
            }
        }
        #endregion Config Properties
    }
}