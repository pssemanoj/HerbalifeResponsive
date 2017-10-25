using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public partial class NorthAmericaAddressControl : AddressControlBase
    {
        //List<string> level0ErrorCodes = new List<string> { "E503", "E504" };
        private readonly List<string> level1ErrorCodes = new List<string> {"E101", "E212", "E213", "E502"};

        private readonly List<string> level2ErrorCodes = new List<string>
            {
                "FOUND",
                "E421",
                "E422",
                "E427",
                "E412",
                "E413",
                "E423",
                "E425",
                "E430",
                "E420",
                "E600"
            };

        private readonly List<string> level3ErrorCodes = new List<string>
            {
                "E214",
                "E216",
                "E428",
                "E429",
                "E503",
                "E504"
            };

        private readonly List<string> level4ErrorCodes = new List<string> {"E501", "E500", "E431", "E302" };
        private static readonly string ADDRESS_CONTROL_AVS_PREV_ERROR_LEVEL = "ADDRESS_CONTROL_AVS_PREV_ERROR_LEVEL";

        private short _tabIndex;

        protected override short TabIndex
        {
            get { return _tabIndex; }
            set
            {
                _tabIndex = value;
                txtFirstName.TabIndex = (value++);
                txtMiddleName.TabIndex = (value++);
                txtLastName.TabIndex = (value++);
                txtCareOfName.TabIndex = (value++);
                txtStreet.TabIndex = (value++);
                txtCity.TabIndex = (value++);
                dnlState.TabIndex = (value++);
                txtPostCode.TabIndex = (value++);
                txtAreaCode.TabIndex = (value++);
                txtNumber.TabIndex = (value++);
                txtExtension.TabIndex = (value);
            }
        }

        protected override string FirstName
        {
            set { txtFirstName.Text = value; }
            get { return (txtFirstName.Text.Trim()); }
        }

        protected override string MiddleInitial
        {
            get { return (txtMiddleName.Text.Trim()); }
            set { txtMiddleName.Text = value; }
        }

        protected override string LastName
        {
            set { txtLastName.Text = value; }
            get { return (txtLastName.Text.Trim()); }
        }

        protected override string StreetAddress
        {
            set { txtStreet.Text = value; }
            get { return (txtStreet.Text.Trim()); }
        }

        protected override string City
        {
            set { txtCity.Text = value; }
            get { return (txtCity.Text.Trim()); }
        }

        protected override string StateProvince
        {
            set
            {
                dnlState.SelectedItem.Value = value;
            }
            get { return dnlState.SelectedItem == null ? string.Empty : dnlState.SelectedItem.Value; }
        }

        protected override string ZipCode
        {
            set { txtPostCode.Text = value; }
            get { return txtPostCode.Text.Trim(); }
        }

        protected override string PhoneNumber
        {
            set { txtNumber.Text = value; }
            get { return txtNumber.Text.Trim(); }
        }

        protected override string AreaCode
        {
            set { txtAreaCode.Text = value; }
            get { return txtAreaCode.Text.Trim(); }
        }

        protected override string Extension
        {
            set { txtExtension.Text = value; }
            get { return txtExtension.Text.Trim(); }
        }

        protected override string Recipient
        {
            set { txtCareOfName.Text = value; }
            get { return txtCareOfName.Text.Trim(); }
        }

        public int AVSErrorLevel { set; get; }

        protected override RequireFieldDef[] RequiredFields
        {
            set { ; }
            get
            {
                return new[]
                    {
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Address.Line1,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoStreet1")
                            },
                        new RequireFieldDef
                            {
                                Field = _shippingAddr.Phone,
                                ErrorMsg = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone")
                            }
                    };
            }
        }

        public int PrevAVSErrorLevel
        {
            set { ViewState[ADDRESS_CONTROL_AVS_PREV_ERROR_LEVEL] = value; }
            get
            {
                object viewStateVal = ViewState[ADDRESS_CONTROL_AVS_PREV_ERROR_LEVEL];
                if (viewStateVal == null) return -1;
                return int.Parse(viewStateVal.ToString());
            }
        }


        public bool IsLevel2ErrorCode
        {
            get { return (bool) Session["IsLevel2ErrorCode"]; }
            set { Session["IsLevel2ErrorCode"] = value; }
        }

        public override void LoadPage()
        {
            displayPostalCodeFormatTextBasedOnCountryCode();
            if (_shippingAddr != null)
            {
                StreetAddress = _shippingAddr.Address.Line1;
                ListItem itemState = dnlState.Items.FindByValue(_shippingAddr.Address.StateProvinceTerritory);
                dnlState.SelectedIndex = -1;
                if (null != itemState) itemState.Selected = true;

                City = _shippingAddr.Address.City;
                setPostalcodeBycountry();
                if (!string.IsNullOrEmpty(_shippingAddr.Phone) && _shippingAddr.Phone.Contains('-'))
                {
                    var numbers = _shippingAddr.Phone.Split(new[] {'-'});
                    if (numbers.Length == 2)
                    {
                        AreaCode = numbers[0];
                        PhoneNumber = numbers[1];
                    }
                    else if (numbers.Length == 3)
                    {
                        AreaCode = numbers[0];
                        PhoneNumber = numbers[1];
                        Extension = numbers[2];
                    }
                }
                else if (!string.IsNullOrEmpty(_shippingAddr.Phone) && _shippingAddr.Phone.Length > 7)
                {
                    AreaCode = _shippingAddr.Phone.Substring(0, 3);
                    var number = _shippingAddr.Phone.Substring(3);
                    if (number.Length > 7)
                    {
                        PhoneNumber = number.Substring(0, 7);
                        Extension = number.Substring(7);
                    }
                    else
                    {
                        PhoneNumber = number;
                    }
                }
                else
                {
                    PhoneNumber = _shippingAddr.Phone;
                    AreaCode = _shippingAddr.AreaCode;
                    Extension = _shippingAddr.AltAreaCode;
                }

                FirstName = _shippingAddr.FirstName;
                LastName = _shippingAddr.LastName;
                MiddleInitial = _shippingAddr.MiddleName;
                Recipient = _shippingAddr.Recipient;
            }
            else
            {
                LookupStates();
            }
        }

        private void displayPostalCodeFormatTextBasedOnCountryCode()
        {
            string countryCode = (Page as ProductsBase).CountryCode;
            if (countryCode.Equals("US") || countryCode.Equals("PR"))
            {
                lbPostal.Text = GetLocalResourceObject("lbUSPostal.Text") as string;
                caFormat.Visible = false;
                usFormat.Visible = !countryCode.Equals("PR");
                prFormat.Visible = countryCode.Equals("PR");
            }

            if (countryCode.Equals("CA"))
            {
                caFormat.Visible = true;
                usFormat.Visible = false;
                prFormat.Visible = false;
            }
        }

        private void setPostalcodeBycountry()
        {
            if (null != _shippingAddr.Address.PostalCode)
            {
                string countryCode = (Page as ProductsBase).CountryCode;
                //Prevent to display the zipcode with -
                if (_shippingAddr.Address.PostalCode.Contains('-') && countryCode.Equals("US")
                    && _shippingAddr.Address.PostalCode.IndexOf("-", System.StringComparison.Ordinal)+1>=_shippingAddr.Address.PostalCode.Length )
                {
                    ZipCode = _shippingAddr.Address.PostalCode.Replace("-","");
                }
                else
                {
                    ZipCode = _shippingAddr.Address.PostalCode;
                }
            }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            TabIndex = 1;
            blErrors.Items.Clear();
            //trLabeName.Visible = !ProductsBase.CountryCode.Equals("PR");
            //trName.Visible = !ProductsBase.CountryCode.Equals("PR");
            dvName.Visible = !ProductsBase.CountryCode.Equals("PR");
            //trLabelCareOfName.Visible = ProductsBase.CountryCode.Equals("PR");
            //trCareOfName.Visible = ProductsBase.CountryCode.Equals("PR");
            dvCareOfName.Visible = ProductsBase.CountryCode.Equals("PR");
            lblExt.Visible = !ProductsBase.CountryCode.Equals("PR");
            txtExtension.Visible = !ProductsBase.CountryCode.Equals("PR");
            txtStreet.MaxLength = 40;
            dnlState.Visible = !ProductsBase.CountryCode.Equals("PR");
            txtState.Visible = ProductsBase.CountryCode.Equals("PR");
            txtState.ReadOnly = ProductsBase.CountryCode.Equals("PR");
        }

        public override object CreateAddressFromControl(string typeName)
        {
            object dataContext = DataContext;
            var shipping = new ShippingAddress_V02();
            if (dataContext != null)
                shipping = (ShippingAddress_V02) dataContext;
            var shippingAddress = new Address_V01();

            string countryCode = ProductsBase.CountryCode;

            // On delete of primary shipping address crashing here since shipping.Address is null. So do null check first
            shippingAddress.Country = shipping.Address == null ? countryCode : shipping.Address.Country;
            shippingAddress.Line2 = string.Empty;
            shippingAddress.Line3 = string.Empty;
            shippingAddress.Line4 = string.Empty;

            shippingAddress.Line1 = StreetAddress;
            shippingAddress.City = City;

            if (countryCode.Equals("US") || countryCode.Equals("PR"))
            {
                shippingAddress.StateProvinceTerritory = dnlState.SelectedItem.Value;
                txtState.Text = dnlState.SelectedItem.Value;
            }
            else
            {
                if (!dnlState.SelectedItem.Text.ToLower().Equals(GetLocalResourceObject("Select").ToString().ToLower()))
                    shippingAddress.StateProvinceTerritory = dnlState.SelectedItem.Text.Substring(0, 2);
                else
                    shippingAddress.StateProvinceTerritory = dnlState.SelectedItem.Text;
            }

            shippingAddress.PostalCode = ZipCode;
            shipping.Address = shippingAddress;
            shipping.Phone = PhoneNumber;
            shipping.AreaCode = AreaCode;
            shipping.AltAreaCode = Extension;
            shipping.FirstName = FirstName;
            shipping.LastName = LastName;
            shipping.MiddleName = MiddleInitial;
            if (countryCode.Equals("PR"))
            {
                shipping.Recipient = Recipient;
            }
            else
            {
                shipping.Recipient = FirstName + " " + MiddleInitial + " " + LastName;
            }
            shipping.Address.CountyDistrict = County;

            if (countryCode.Equals("PR"))
            {
                shipping.Address.Line3 = ZipCode;
                shipping.Address.Line4 = Recipient;
            }

            return _shippingAddr = shipping;
        }

        public override bool Validate()
        {
            base.Validate(); // call base to validate required
            string countryCode = ProductsBase.CountryCode;

            if (countryCode.Equals("US") || countryCode.Equals("CA"))
            {
                if (string.IsNullOrEmpty(_shippingAddr.FirstName))
                {
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoFirstName"));
                }
                if (string.IsNullOrEmpty(_shippingAddr.LastName))
                {
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoLastName"));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_shippingAddr.Recipient))
                {
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCareOfName"));
                }                
            }

            if (countryCode.Equals("CA"))
            {
                if (string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory) ||
                    (_shippingAddr.Address.StateProvinceTerritory.ToLower()
                                  .Equals(GetLocalResourceObject("Select").ToString().ToLower())))
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoState"));
                if (string.IsNullOrEmpty(_shippingAddr.Address.City))
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoCity"));
                if (string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoZipCode"));
            }

            if (countryCode.Equals("US"))
            {
                if (string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory) ||
                    (_shippingAddr.Address.StateProvinceTerritory.ToLower()
                                  .Equals(GetLocalResourceObject("Select").ToString().ToLower())))
                    ErrorList.Add(GetLocalResourceObject("NoState") as string);
                if (string.IsNullOrEmpty(_shippingAddr.Address.City))
                    ErrorList.Add(GetLocalResourceObject("NoCity") as string);
                if (string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    ErrorList.Add(GetLocalResourceObject("NoZip") as string);
            }

            if (countryCode.Equals("PR"))
            {
                if (string.IsNullOrEmpty(_shippingAddr.Address.StateProvinceTerritory) ||
                    (_shippingAddr.Address.StateProvinceTerritory.ToLower()
                                  .Equals(GetLocalResourceObject("Select").ToString().ToLower())))
                    ErrorList.Add(GetLocalResourceObject("NoState") as string);
                if (string.IsNullOrEmpty(_shippingAddr.Address.City))
                    ErrorList.Add(GetLocalResourceObject("NoCity") as string);
                if (string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                    ErrorList.Add(GetLocalResourceObject("NoZip") as string);
            }

            NameStreetAndPhoneValidation(ErrorList);

            if (countryCode.Equals("US") || countryCode.Equals("PR"))
            {
                USFieldValidation(ErrorList);

                //AVS address validation check. If false, avs address validation failed.
                string errorCode = string.Empty;
                ServiceProvider.AddressValidationSvc.Address avsAddress = null;
                _shippingAddr.Address.StateProvinceTerritory = _shippingAddr.Address.StateProvinceTerritory;
                bool avsSvcStatus = AVSAddressValidationCheck(out errorCode, out avsAddress);
                setAvsErrorLevel(avsSvcStatus, errorCode, ErrorList, avsAddress);

                //Update VIEW with AVS recommendations
                if (!this.AVSErrorLevel.Equals(4))
                {
                    updateViewWithAVSRecommendation(avsAddress);
                }
                if (ErrorList.Count == 0)
                    return true;

                bool bSave = IsOkToSave();
                if (bSave)
                {
                    if (this.ErrorList.Contains(GetLocalResourceObject("Level1ErrorMessage") as string))
                        this.ErrorList.Remove(GetLocalResourceObject("Level1ErrorMessage") as string);
                    else if (this.ErrorList.Contains(GetLocalResourceObject("Level2ErrorMessage") as string))
                        this.ErrorList.Remove(GetLocalResourceObject("Level2ErrorMessage") as string);
                    else if (this.ErrorList.Contains(GetLocalResourceObject("Level3ErrorMessage") as string))
                        this.ErrorList.Remove(GetLocalResourceObject("Level3ErrorMessage") as string);
                    else if (this.ErrorList.Contains(GetLocalResourceObject("Level4ErrorMessage") as string))
                        this.ErrorList.Remove(GetLocalResourceObject("Level4ErrorMessage") as string);
                }

                return bSave;
            }

            //CANADA
            if (countryCode.Equals("CA"))
            {
                //31554: All textbox allow these special characters (,) (-) (‘) (.) (/) and (\) 
                if (!string.IsNullOrEmpty(_shippingAddr.Address.City))
                {
                    if (!Regex.IsMatch(_shippingAddr.Address.City,
                                       @"(^[\w\s\/,-.'\\]+$)"))
                        ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCity"));
                }

                if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
                {
                    //if (!Regex.IsMatch(_shippingAddr.Address.PostalCode,
                    //                   @"(^[a-zA-Z0-9]{3}\s[a-zA-Z0-9]{3}$)"))
                    if(!Regex.IsMatch(_shippingAddr.Address.PostalCode, @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy]{1}\d{1}[a-zA-Z]{1} *\d{1}[a-zA-Z]{1}\d{1}$"))
                        ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
                }
            }

            return ErrorList.Count == 0;
        }

        private void NameStreetAndPhoneValidation(List<string> _errors)
        {
            //31554: All textbox allow these special characters (,) (-) (‘) (.) (/) and (\) 
            if (ProductsBase.CountryCode.Equals("PR"))
            {
                if (!string.IsNullOrEmpty(_shippingAddr.Recipient))
                {
                    if (!Regex.IsMatch(_shippingAddr.Recipient, @"(^[\w\s\/,-.'\\ ]+$)"))
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidRecipentName"));
                }
                if (!string.IsNullOrEmpty(_shippingAddr.Address.Line1))
                {
                    var line1 = _shippingAddr.Address.Line1.Replace(" ", string.Empty).ToUpper().Replace(".", string.Empty);
                    if (Regex.IsMatch(line1, @"POBOX\S*$"))
                    {
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError"));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_shippingAddr.FirstName))
                {
                    if (!Regex.IsMatch(_shippingAddr.FirstName,
                                       @"(^[\w\/,-.'\\ ]+$)"))
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidFirstName"));
                }

                if (!string.IsNullOrEmpty(_shippingAddr.LastName))
                {
                    if (!Regex.IsMatch(_shippingAddr.LastName,
                                       @"(^[\w\/,-.'\\ ]+$)"))
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidLastName"));
                }

                if (!string.IsNullOrEmpty(_shippingAddr.MiddleName))
                {
                    if (!Regex.IsMatch(_shippingAddr.MiddleName,
                                       @"(^[\w\/,-.'\\]+$)"))
                        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidMiddleName"));
                }
            }
            //if ((_shippingAddr.Address.Line1 != null) && (!_shippingAddr.Address.Line1.Equals(string.Empty)))
            //{
            //    if (!StreetAddressRegularExpressionCheck(_shippingAddr.Address.Line1))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress);
            //}

            if (!string.IsNullOrEmpty(_shippingAddr.Phone))
            {
                string[] numbers = _shippingAddr.Phone.Split('-');

                if (!Regex.IsMatch(numbers[0],
                                   @"^(\d{3})$"))
                    _errors.Add(GetLocalResourceObject("InvalidAreaCode") as string);

                if (!Regex.IsMatch(numbers[1],
                                   @"^(\d{7})$"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidPhone"));


                if (numbers.Length.Equals(3))
                {
                    if (!string.IsNullOrEmpty(numbers[2]))
                    {
                        if (!Regex.IsMatch(numbers[2],
                                           @"^(\d{0,5})$"))
                        {
                            if (
                                _errors.Contains(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                           "InvalidExtension")))
                            {
                                _errors.Remove(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                         "InvalidExtension"));
                                _errors.Add(GetLocalResourceObject("InvalidExtension") as string);
                            }
                        }
                    }
                }


                if (_errors.Contains(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidExtension")))
                {
                    _errors.Remove(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidExtension"));
                    _errors.Add(GetLocalResourceObject("InvalidExtension") as string);
                }
            }
        }

        private void USFieldValidation(List<string> _errors)
        {
            //if ((_shippingAddr.Address.Line1 != null) && (!_shippingAddr.Address.Line1.Equals(string.Empty)))
            //{
            //    if (!StreetAddressRegularExpressionCheck(_shippingAddr.Address.Line1))
            //        _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidStreetAddress"));
            //}

            //31554: All textbox allow these special characters (,) (-) (‘) (.) (/) and (\) 
            if (!string.IsNullOrEmpty(_shippingAddr.Address.City))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.City,
                                   @"(^[\w\s\/,-.'\\]+$)"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidCity"));
            }

            if (!string.IsNullOrEmpty(_shippingAddr.Address.PostalCode))
            {
                if (!Regex.IsMatch(_shippingAddr.Address.PostalCode.Trim(),
                                   @"(^\d{4}$)|(^\d{4}-\d{4}$)|(^\d{4}\s\d{4}$)|(^\d{5}$)|(^\d{5}-\d{4}$)|(^\d{5}\s\d{4}$)"))
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode"));
            }
        }

        private void updateViewWithAVSRecommendation(ServiceProvider.AddressValidationSvc.Address avsAddress)
        {
            txtStreet.Text = avsAddress.Line1;
            txtCity.Text = avsAddress.City;

            dnlState.SelectedIndex = -1;
            ListItem itemState = dnlState.Items.FindByValue(avsAddress.StateProvinceTerritory);
            if (itemState != null) itemState.Selected = true;

            txtPostCode.Text = avsAddress.PostalCode;
            County = avsAddress.CountyDistrict;
            string phone = _shippingAddr.Phone;
            CreateAddressFromControl();
            _shippingAddr.Phone = phone;
        }

        private void setAvsErrorLevel(bool svcCallStatus, string errorCode, List<string> errorList,
                                      ServiceProvider.AddressValidationSvc.Address avsAddress)
        {
            //there are field validation errors, 
            if (errorList.Count > 0)
            {
                AVSErrorLevel = 4;
                return;
            }

            if (level1ErrorCodes.Exists(l => l == errorCode))
                AVSErrorLevel = 1;
            if (level2ErrorCodes.Exists(l => l == errorCode))
                AVSErrorLevel = 2;
            if (level3ErrorCodes.Exists(l => l == errorCode))
                AVSErrorLevel = 3;
            if (level4ErrorCodes.Exists(l => l == errorCode))
                AVSErrorLevel = 4;

            //if State Code has changed, set the AVSErrorLevel to 2.
            if (AVSErrorLevel.Equals(0))
            {
                if (!_shippingAddr.Address.StateProvinceTerritory.Equals(avsAddress.StateProvinceTerritory))
                    AVSErrorLevel = 2;
            }


            //set Error messages for UI/View rendering
            if (AVSErrorLevel == 1)
                errorList.Add(GetLocalResourceObject("Level1ErrorMessage") as string);
            if (AVSErrorLevel == 2)
                errorList.Add(GetLocalResourceObject("Level2ErrorMessage") as string);
            if (AVSErrorLevel == 3)
                errorList.Add(GetLocalResourceObject("Level3ErrorMessage") as string);
            if (AVSErrorLevel == 4)
                errorList.Add(GetLocalResourceObject("Level4ErrorMessage") as string);
        }


        public override bool IsOkToSave()
        {
            bool returnValue = true;
            int prevAvsLevel = PrevAVSErrorLevel;
            int curAvsLevel = AVSErrorLevel;

            //SAVE OK, if CURRENT is 0
            if (curAvsLevel.Equals(0)) returnValue = true;

            //DO NOT OK, if CURRENT is 1
            if (curAvsLevel.Equals(1)) returnValue = false;

            //SAVE OK, if PREV & CURRENT is 2
            if (prevAvsLevel.Equals(2) && curAvsLevel.Equals(2))
                returnValue = true;

            //CASE1: DO NOT SAVE if CURRENT level is 3,4
            if (curAvsLevel.Equals(3) || curAvsLevel.Equals(4))
                returnValue = false;

            //CASE2: if PREV 3,4 AND CURRENT level is 2
            if (prevAvsLevel.Equals(3) && curAvsLevel.Equals(2))
                returnValue = false;
            if (prevAvsLevel.Equals(4) && curAvsLevel.Equals(2))
                returnValue = false;

            //CASE3: if PREV -1 AND CURRENT level is 2
            if (prevAvsLevel.Equals(-1) && curAvsLevel.Equals(2))
                returnValue = false;

            //CASE3: if PREV 1 AND CURRENT level is 2
            if (prevAvsLevel.Equals(1) && curAvsLevel.Equals(2))
                returnValue = false;

            PrevAVSErrorLevel = AVSErrorLevel;
            return returnValue;
        }

        private bool AVSAddressValidationCheck(out string errorCode,
                                               out ServiceProvider.AddressValidationSvc.Address avsAddress)
        {
            errorCode = string.Empty;
            avsAddress = null;
            
            if (ProductsBase.CountryCode.Equals("US"))
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider("US");
                if (provider != null)
                {
                    return provider.ValidateAddress(_shippingAddr, out errorCode,
                                                out avsAddress);
                }
            }
            else if (ProductsBase.CountryCode.Equals("PR"))
            {
                IShippingProvider provider = ShippingProvider.GetShippingProvider("PR");
                if (provider != null)
                {
                    return provider.ValidateAddress(_shippingAddr, out errorCode,
                                                out avsAddress);
                }
            }

            
            
            return true;
        }

        private bool StreetAddressRegularExpressionCheck(String streetAddress)
        {
            string alphaPattern = @"[a-zA-Z]+";
            string numberPattern = @"[\d]+";
            string spacePattern = @"[\s]+";

            bool alphaMatch = Regex.IsMatch(streetAddress,
                                            alphaPattern);
            bool numberMatch = Regex.IsMatch(streetAddress,
                                             numberPattern);
            bool spaceMatch = Regex.IsMatch(streetAddress,
                                            spacePattern);
            //if (!System.Text.RegularExpressions.Regex.IsMatch(streetAddress,
            //       @"([a-zA-Z0-9]+$)"))
            //    return false;

            if (alphaMatch && numberMatch && spaceMatch)
                return true;
            else
                return false;
        }


        protected string getSimplifiedText(string Text)
        {
            Text = Text.Replace("à", "a");
            Text = Text.Replace("è", "e");
            Text = Text.Replace("é", "e");
            Text = Text.Replace("ì", "i");
            Text = Text.Replace("ò", "o");
            Text = Text.Replace("ù", "u");
            Text = Text.Replace("°", " ");
            Text = Text.Replace("'", " ");
            Text = Text.Replace("§", " ");
            Text = Text.Replace("^", " ");
            Text = Text.Replace("*", " ");
            return Text;
        }

        private bool LookupStates()
        {
            bool lookedUp = false;
            dnlState.Items.Clear();
            var lookupResults = new List<string>();

            if (ProductsBase.CountryCode.Equals("CA"))
            {
                IShippingProvider providerCA = ShippingProvider.GetShippingProvider(
                    (Page as ProductsBase).CountryCode);
                if (providerCA != null)
                {
                    lookupResults = providerCA.GetStatesForCountry(ProductsBase.CountryCode);
                    if (lookupResults != null && lookupResults.Count > 0)
                    {
                        var items = (from s in lookupResults
                                     select new ListItem {Text = s, Value = s.Substring(0, 2)}).ToArray();
                        dnlState.Items.AddRange(items);
                        dnlState.Items.Insert(0,
                                              new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                        dnlState.SelectedIndex = 0;
                        lookedUp = true;
                    }
                }
            }
            else if (ProductsBase.CountryCode.Equals("PR"))
            {
                var provider_PR = new ShippingProvider_PR();
                lookupResults = provider_PR.GetStatesForCountry(ProductsBase.CountryCode);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults
                                 select new ListItem { Text = s, Value = s.Substring(0, 2) }).ToArray();
                    dnlState.Items.AddRange(items);
                    dnlState.SelectedIndex = 0;
                    txtState.Text = items[0].ToString();
                    lookedUp = true;
                }
            }
            else
            {
                var providerUS = new ShippingProvider_US();
                lookupResults = providerUS.GetStatesForCountryToDisplay(ProductsBase.CountryCode);
                if (null != lookupResults && lookupResults.Count > 0)
                {
                    var items = (from s in lookupResults
                                 select new ListItem {Text = s, Value = s.Substring(0, 2)}).ToArray();
                    dnlState.Items.AddRange(items);
                    dnlState.Items.Insert(0, new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                    dnlState.SelectedIndex = 0;
                    lookedUp = true;
                }
            }

            return lookedUp;
        }

        //private string FormatNumberToDB(string phoneNumber)
        //{
        //    if ((phoneNumber == null) || (phoneNumber.Equals(string.Empty))) return string.Empty;
        //    if (phoneNumber.Equals("(___) ___-____")) return string.Empty;
        //    return phoneNumber.Remove(0, 1).Remove(3, 1).Replace(' ', '-');
        //}

        //private string FormatNumberForUser(string phoneNumber)
        //{
        //    if ((phoneNumber == null) || (phoneNumber.Equals(string.Empty))) return string.Empty;
        //    return phoneNumber.Insert(0, "(").Insert(4, ")").Insert(5, " ").Remove(6, 1); 
        //}

        private string FormatNumberToDB(string areacode, string phoneNumber, string extension)
        {
            if (areacode.Equals(string.Empty) && phoneNumber.Equals(string.Empty) && extension.Equals(string.Empty))
                return string.Empty;

            if (extension.Equals(string.Empty))
                return areacode + "-" + phoneNumber;
            else
                return areacode + "-" + phoneNumber + "-" + extension;
        }

        private string[] FormatNumberForUser(string phoneNumber)
        {
            string[] numbers = phoneNumber.Split(new[] {'-'});

            if (numbers.Length.Equals(2))
            {
                numbers[0] = numbers[0];
                numbers[1] = numbers[1];
            }
            if (numbers.Length.Equals(3))
            {
                numbers[0] = numbers[0];
                numbers[1] = numbers[1];
                numbers[2] = numbers[2];
            }
            return numbers;
        }
    }
}