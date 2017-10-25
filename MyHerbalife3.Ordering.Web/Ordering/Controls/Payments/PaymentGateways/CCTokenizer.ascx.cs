using MyHerbalife3.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
	public partial class CCTokenizer : System.Web.UI.UserControl
	{

#region Constants
		protected const string ScriptBlock = "<script type='text/javascript'>{0}</script>";
        private const string ValidationScriptMessages = "<script type='text/javascript'>    	var CardNumberRequired = '{0}';    	var CardNumberInvalid = '{1}';    	var CardNameRequired = '{2}';    	var ExpDateRequired = '{3}';    	var AddCardQuestion = '{4}';    	var CardHasExpired = '{5}';     var StreetAddressRequired = '{6}';     var CityRequired = '{7}';     var StateRequired = '{8}';     var ValidZipRequired = '{9}';     var CardTypeRequired = '{10}';     var TokenizationFailed = '{11}';     var CVVRequired = '{12}';</script>";
        private const string ControlName = "CCTokenizer";
#endregion

#region client code
        //protected const string ControlBindingScriptBlock = "<script type='text/javascript'>{0}</script>";

		//string x = "var txtCardName = document.getElementById(prefix + #CardNameControlId#);";
		//x.Append("s");
		// s.Append("var txtCardNumber = document.getElementById(prefix + #CardNumberControlId#);");
		/*
		ScriptBehaviorDescriptor.a
        var txtCardNumber = document.getElementById(prefix + #CardNumberControlId#);
        var txtStreetAddress = document.getElementById(prefix + #StreetAddressControlId#);
        var txtCity = document.getElementById(prefix + #CityControlId#);
        var txtState = document.getElementById(prefix + #StateControlId#);
        var txtZip = document.getElementById(prefix + #ZipControlId#);
        var ddlCardtype = document.getElementById(prefix + #CardTypeControlId#);
        var ddlExpMonth = document.getElementById(prefix + #ExpirationMonthControlId #);
        var ddlExpYear = document.getElementById(prefix + #ExpirationYearControlId#);
		*/

		// ========================================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		// ========================================================================================
		private void BindControlsWithClientCode()
		{
			StringBuilder sbCode = new StringBuilder();

			sbCode.Append("$(document).ready(function(){ ");

			sbCode.Append(" var prefixX = $$(ddlCardtypeControlName).attr('id').replace( $$(ddlCardtypeControlName).attr('id'), ''); ");

			sbCode.Append(" var ddlCardType = $$(ddlCardtypeControlName); ");
			sbCode.Append(" var $submitButton = $$(btnSubmitControlName); ");
			sbCode.Append(" if (typeof ddlCardType == 'undefined')");
			sbCode.Append(" { ");
			sbCode.Append("  alert('errr'); ");
			sbCode.Append(" } ");

			/// sbCode.Append("  alert($ddlCardType.attr('id')); ");

			sbCode.Append(" function $$(id, context) { ");
			sbCode.Append("    var el = $(\"#\" + id, context); ");
			sbCode.Append("    if (el.length < 1) ");
			sbCode.Append("        el = $(\"[id$=_\" + id + \"]\", context); ");
			sbCode.Append("    return el; ");
			sbCode.Append("} ");

			// Body

			sbCode.Append("$('#btnSubmit').click(function(){ ");
            sbCode.Append("var isValid = try {ValidateNewCardClient(event, this, prefixX);} catch { isValid = false; }");
            sbCode.Append(" return isValid; ");
			sbCode.Append("});");

			// Card Type DDL
			sbCode.Append(" var myOptions = [ ");
			sbCode.Append(" { text: 'Select', value: '0'} ");


			sbCode.Append(" ]; ");

			sbCode.Append(" $.each(myOptions, function(i, el) ");
			sbCode.Append(" {  ");
			sbCode.Append(" $('#ddlCardType').append( new Option(el.text,el.value) ); ");
			sbCode.Append(" }); ");

			// End tag
			sbCode.Append("});");

			this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "AttachScript", string.Format(ScriptBlock, sbCode.ToString()));

		}
#endregion

#region Properties
        //Control Names
		public string CardNameControlId { get; set; }
		public string CardNumberControlId { get; set; }
		public string CardHolderControlId { get; set; }
        public string CVVControlId { get; set; }
        public string StreetAddressControlId { get; set; }
		public string CityControlId { get; set; }
		public string StateControlId { get; set; }
		public string ZipControlId { get; set; }
		public string CardTypeControlId { get; set; }
		public string ExpirationMonthControlId { get; set; }
		public string ExpirationYearControlId { get; set; }
		public string SubmitButtonControlId { get; set; }
		public string MessageLabelControlId { get; set; }

        //Resx Names
        public string ResxSourceName { get; set; }
        public string ValidateAddCardMissingCard { get; set; }
        public string ValidateAddCardBadCard { get; set; }
        public string ValidateAddCardMissingName { get; set; }
        public string ValidateAddCardMissingExpDate { get; set; }
        public string ValidateAddCard { get; set; }
        public string ValidateAddCardExpired { get; set; }
        public string ValidateStreetAddressRequired { get; set; }
        public string ValidateCityRequired { get; set; }
        public string ValidateStateRequired { get; set; }
        public string ValidateZipRequired { get; set; }
        public string ValidateSelectCardType { get; set; }
        public string ValidateTokenizationFailed { get; set; }
        public string ValidateCVVRequired { get; set; }
        
        //Behaviour
        public bool DontTokenizeJustMaskAndUseSessionStorage { get; set; }
#endregion

#region Control Code
        // ========================================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		// ========================================================================================
		protected void Page_Load(object sender, EventArgs e)
		{
			setCardValidationClientScripts();
		}


		// ========================================================================================
		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		// ========================================================================================
		protected virtual void setCardValidationClientScripts()
		{
            string onClientClick = string.Empty;
			string disableTokens = System.Configuration.ConfigurationManager.AppSettings["TokenizationDisabled"];
			this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "TokenizationDisabled", string.Format(ScriptBlock, string.Concat("var tokenizationDisabled = ", string.IsNullOrEmpty(disableTokens) ? false.ToString().ToLower() : disableTokens.ToString().ToLower()), ";"));
            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "AuthToken", string.Format(ScriptBlock, string.Concat("var authToken = '", CookieHandler.GetSeamlessCredentials().AuthenticationToken.ToString(), "';")));
            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "ValidationMessages", string.Format(ValidationScriptMessages,
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCardMissingCard) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCardBadCard) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCardMissingName) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCardMissingExpDate) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCard) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateAddCardExpired) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateStreetAddressRequired) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateCityRequired) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateStateRequired) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateZipRequired) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateSelectCardType) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateTokenizationFailed) ),
																													EscapeJavascriptQuotes(GetResourceString(ValidateCVVRequired))
                                                                                                                    ));

			// Control binding
            var controls = GetControls(Page); // TODO, filter only significant controls to avoid overhead .Where(c => c is TextBox); ; Why not use FindControl?
            var textboxes = controls.OfType<TextBox>();
            LinkButton btnSubmit = (LinkButton)controls.Where(c => c is LinkButton && c.ID == SubmitButtonControlId).FirstOrDefault();

            if (btnSubmit == null)
                return;          
            
            //btnSubmit.OnClientClick = "return ValidateNewCard(event, this);";

            TextBox txtCardNumber = textboxes.Where(c => c.ID == CardNumberControlId).FirstOrDefault();
            TextBox txtCardName = textboxes.Where(c => c.ID == CardNameControlId).FirstOrDefault();
            TextBox txtCVV = textboxes.Where(c => c.ID == CVVControlId).FirstOrDefault();
            TextBox txtCardHolderName = textboxes.Where(c => c.ID == CardHolderControlId).FirstOrDefault();
            TextBox txtStreetAddress = textboxes.Where(c => c.ID == StreetAddressControlId).FirstOrDefault();
            TextBox txtCity = textboxes.Where(c => c.ID == CityControlId).FirstOrDefault();
            TextBox txtState = textboxes.Where(c => c.ID == StateControlId).FirstOrDefault();
            TextBox txtZip = textboxes.Where(c => c.ID == ZipControlId).FirstOrDefault();
            Control lblMessage = this.Parent.FindControl(MessageLabelControlId);

            DropDownList ddlCardType = (DropDownList)controls.Where(c => c is DropDownList && c.ID == CardTypeControlId).FirstOrDefault();
            DropDownList ddlExpMonth = (DropDownList)controls.Where(c => c is DropDownList && c.ID == ExpirationMonthControlId).FirstOrDefault();
            DropDownList ddlExpYear = (DropDownList)controls.Where(c => c is DropDownList && c.ID == ExpirationYearControlId).FirstOrDefault();
       
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("var txtCardNameControlName = '{0}';", GetControlName(txtCardName)));
            sb.Append(string.Format("var txtCardHolderNameControlName = '{0}';", GetControlName(txtCardHolderName)));
            sb.Append(string.Format("var txtCVVControlName = '{0}';", GetControlName(txtCVV)));
            sb.Append(string.Format("var txtCardNumberControlName = '{0}';", GetControlName(txtCardNumber)));
            sb.Append(string.Format("var txtStreetAddressControlName = '{0}';", GetControlName(txtStreetAddress)));
			sb.Append(string.Format("var txtCityControlName = '{0}';", GetControlName(txtCity)));
			sb.Append(string.Format("var txtStateControlName = '{0}';", GetControlName(txtState)));
			sb.Append(string.Format("var txtZipControlName = '{0}';", GetControlName(txtZip)));
			sb.Append(string.Format("var ddlCardtypeControlName = '{0}';", GetControlName(ddlCardType)));
			sb.Append(string.Format("var ddlExpMonthControlName = '{0}';", GetControlName(ddlExpMonth)));
			sb.Append(string.Format("var ddlExpYearControlName = '{0}';", GetControlName(ddlExpYear)));
			sb.Append(string.Format("var btnSubmitControlName = '{0}';", GetControlName(btnSubmit)));
			sb.Append(string.Format("var lblMessageControlName = '{0}';", GetControlName(lblMessage)));
            sb.Append(string.Format("var hdnValidationsControlName = '{0}';", GetControlName(hdnValidations)));

			string script = sb.ToString();

			this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "ClientControlsRegistration", string.Format(ScriptBlock, script));
            this.Page.ClientScript.RegisterClientScriptBlock(typeof(string), "DontTokenizeJustMaskAndUseSessionStorage", string.Format(ScriptBlock, string.Concat("var DontTokenizeJustMaskAndUseSessionStorage = ", DontTokenizeJustMaskAndUseSessionStorage.ToString().ToLower()), ";"));

            List<HPSCreditCardType> cardValidations = GetCardValidations();
            cardValidations.ForEach(cv => hdnValidations.Value += string.Concat(cv.Code, "=", cv.CardNumberValidationRegexText, ";"));
		}

        private string GetControlName(Control ctl)
        {
            string name = string.Empty;
            if (null != ctl)
            {
                name = ctl.ClientID;
            }

            return name;
        }

        // ========================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        // ========================================================================================
        IList<Control> GetControls(Control parentControl)
        {

            List<Control> controls = new List<Control>();

            foreach (Control c in parentControl.Controls)
            {
                controls.Add(c);

                if (c.HasControls())
                {
                    controls.AddRange(GetControls(c));
                }

            }

            return controls;
        }

        // ========================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        // ========================================================================================
        protected string EscapeJavascriptQuotes(string message)
        {
            string result = string.Empty;
            if (null != message)
            {
                result = message.Replace("'", @"\'");
            }

            return result;
        }

        //Hardcode validation strings - eliminate Order service call
        private List<HPSCreditCardType> GetCardValidations()
        {
            return new List<HPSCreditCardType>() 
            {
                new HPSCreditCardType(){ Code="VI", CardNumberValidationRegexText="^4\\d{15}$"}, 
                new HPSCreditCardType(){ Code="MC", CardNumberValidationRegexText="^5\\d{15}$|^36\\d{12}$"},
                new HPSCreditCardType(){ Code="AX", CardNumberValidationRegexText="^34\\d{14}$|^37\\d{14}$|^34\\d{13}$|^37\\d{13}$"},      
                new HPSCreditCardType(){ Code="DI", CardNumberValidationRegexText="^6011|622|644|65$"},
                new HPSCreditCardType(){ Code="JC", CardNumberValidationRegexText="^35"},
                new HPSCreditCardType(){ Code="DN", CardNumberValidationRegexText="^3[0,6,8]\\d{12}$"}, 
                new HPSCreditCardType(){ Code="MS", CardNumberValidationRegexText=""}, 
                new HPSCreditCardType(){ Code="HI", CardNumberValidationRegexText=""}
            };
        }

        private class HPSCreditCardType
        {
            public string Code { get; set; }
            public string CardNumberValidationRegexText { get; set; }
        }

        private string GetResourceString(string key)
        {
            string result = string.Empty;
            //Using this for null protection
            if (!string.IsNullOrEmpty(key))
            {
                //This was in the hope we could load existing resources from - say the parent form's resources
                //But can't be done.
                string source = ResxSourceName;
                if (string.IsNullOrEmpty(source))
                {
                    source = ControlName;
                }

                string resource = GetLocalResourceObject(key) as string;
                if (!string.IsNullOrEmpty(resource))
                {
                    result = resource;
                }
            }

            return result;
        }
#endregion

    }
}