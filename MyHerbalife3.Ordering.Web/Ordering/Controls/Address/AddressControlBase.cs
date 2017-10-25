using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public class RequireFieldDef
    {
        public object Field { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class AddressControlBase : UserControl, AddressBase
    {
        public ProductsBase ProductsBase
        {
            get { return (Page as ProductsBase); }
        }

        protected virtual string Recipient { get; set; }
        protected virtual string FirstName { get; set; }
        protected virtual string LastName { get; set; }
        protected virtual string MiddleInitial { get; set; }
        protected virtual string StreetAddress { get; set; }
        protected virtual string StreetAddress2 { get; set; }
        protected virtual string StreetAddress3 { get; set; }
        protected virtual string StreetAddress4 { get; set; }
        protected virtual string City { get; set; }
        protected virtual string County { get; set; }

        protected virtual string StateProvince { get; set; }
        protected virtual string ZipCode { get; set; }
        protected virtual string PhoneNumber { get; set; }
        protected virtual short TabIndex { get; set; }
        protected virtual string AreaCode { get; set; }
        protected virtual string Extension { get; set; }

        protected virtual RequireFieldDef[] RequiredFields { get; set; }

        public string XMLFile { get; set; }

        public List<string> ErrorList { get; set; }

        protected ShippingAddress_V02 _shippingAddr;

        public virtual object DataContext
        {
            get
            {
                if (ViewState["DataContext"] == null)
                {
                    return _shippingAddr;
                }
                return ViewState["DataContext"];
            }
            set
            {
                _shippingAddr = value as ShippingAddress_V02;
                ViewState["DataContext"] = value;
                LoadPage();
            }
        }

        public virtual void LoadPage()
        {
            ;
        }

        public virtual object CreateAddressFromControl(string typeName)
        {
            object dataContext = DataContext;
            var shipping = new ShippingAddress_V02();
            if (dataContext != null)
                shipping = (ShippingAddress_V02) dataContext;
            var shippingAddress = new Address_V01();

            shippingAddress.Line1 = StreetAddress ?? string.Empty;
            shippingAddress.Line2 = StreetAddress2 ?? string.Empty;
            shippingAddress.Line3 = StreetAddress3 ?? string.Empty;
            shippingAddress.Line4 = StreetAddress4 ?? string.Empty;

            shippingAddress.City = City;
            shippingAddress.CountyDistrict = County ?? string.Empty;
            shippingAddress.StateProvinceTerritory = StateProvince;
            shippingAddress.PostalCode = ZipCode;
            shipping.Address = shippingAddress;
            shipping.Phone = PhoneNumber;
            shipping.AltAreaCode = Extension ?? string.Empty;
            shipping.AreaCode = AreaCode ?? string.Empty;
            shipping.Recipient = Recipient;
            return _shippingAddr = shipping;
        }

        public virtual object CreateAddressFromControl()
        {
            return CreateAddressFromControl(string.Empty);
        }

        public virtual bool IsEditable()
        {
            return true;
        }

        public virtual bool Validate()
        {
            if (ErrorList == null)
                ErrorList = new List<string>();

            if (_shippingAddr == null)
            {
                return false;
            }

            RequireFieldDef[] requiredFields = RequiredFields;
            if (requiredFields != null)
            {
                foreach (RequireFieldDef rf in requiredFields)
                {
                    if (string.IsNullOrEmpty(rf.Field as string))
                        ErrorList.Add(rf.ErrorMsg);
                }
            }
            ValidationCheck(ErrorList);

            return ErrorList.Count == 0;
        }

        public virtual void ValidationCheck(List<string> _errors)
        {
        }

        public virtual void displayValidationMessages(List<string> errors)
        {
            ;
        }

        public virtual bool IsOkToSave()
        {
            return false;
        }


        public static bool IsNumeric(object Expression)
        {
            bool isNum;
            double retNum;
            isNum = Double.TryParse(Convert.ToString(Expression), NumberStyles.Any, NumberFormatInfo.InvariantInfo,
                                    out retNum);
            return isNum;
        }
    }

    ///<summary>
    /// inheriting the DropDownList to save attributes
    ///</summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DropDownListAttributes runat=server></{0}:DropDownListAttributes>")]
    public class DropDownListAttributes : DropDownList
    {
        ///<summary>
        /// By using this method save the attributes data in view state
        ///</summary>
        ///<returns></returns>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        protected override object SaveViewState()
        {
            object[] allStates = new object[this.Items.Count + 1];
            object baseState = base.SaveViewState();
            allStates[0] = baseState;
            int i = 1;
            foreach (ListItem li in this.Items)
            {

                int j = 0;
                string[][] attributes = new string[li.Attributes.Count][];
                foreach (string attribute in li.Attributes.Keys)
                    attributes[j++] = new string[] { attribute, li.Attributes[attribute] };

                allStates[i++] = attributes;

            }

            return allStates;
        }

        ///<summary>
        /// By using this method Load the attributes data from view state
        ///</summary>
        ///<param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] myState = (object[])savedState;
                if (myState[0] != null)
                    base.LoadViewState(myState[0]);

                int i = 1;
                foreach (ListItem li in this.Items)
                    foreach (string[] attribute in (string[][])myState[i++])
                        li.Attributes[attribute[0]] = attribute[1];
            }
        }

    }
}