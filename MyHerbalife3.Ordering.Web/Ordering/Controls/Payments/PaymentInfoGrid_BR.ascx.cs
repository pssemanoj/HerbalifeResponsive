using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoGrid_BR : PaymentInfoGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (ForFControlValidation())
            {
                var sessionInfo = SessionInfo.GetSessionInfo(ShoppingCart.DistributorID, ShoppingCart.Locale);
                if (!string.IsNullOrEmpty(sessionInfo.BRPF))
                {
                    var digits = sessionInfo.BRPF.Split('-');
                    txtCPF.Text = digits[0];
                    txtVCode.Text = digits[1];
                }

                divCPF.Visible = true;
                divCPFStatic.Visible = false;
            }
            else
            {
                pnlCPF.Visible = false;
            }

            if (string.IsNullOrEmpty(ShoppingCart.TaxPersonalId))
            {
                // Getting the CPF from the distributor properties
                ShoppingCart.TaxPersonalId = DistributorOrderingProfileProvider.GetTaxIdentificationId(ShoppingCart.DistributorID, "BRPF");
            }
        }

        public override bool ValidateAndGetPayments(Address_V01 shippingAddress,
                                                    out List<Payment> payments,
                                                    bool showErrors)
        {
            bool isValid = base.ValidateAndGetPayments(shippingAddress, out payments, showErrors);

            var currentPayments = GetPayments(shippingAddress);
            var currentChoice = GetCurrentPaymentOption();
            if (currentChoice == PaymentOptionChoice.CreditCard && ForFControlValidation())
            {
                if (currentPayments.Count != 0)
                {
                    foreach (var payment in currentPayments)
                    {
                        if (payment.Address == null)
                        {
                            isValid = false;
                            lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                              "NoBillingAddress");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(payment.Address.Line1) ||
                                string.IsNullOrEmpty(payment.Address.Line2) ||
                                string.IsNullOrEmpty(payment.Address.City) ||
                                string.IsNullOrEmpty(payment.Address.StateProvinceTerritory) ||
                                string.IsNullOrEmpty(payment.Address.PostalCode))
                            {
                                isValid = false;
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                  "NoBillingAddress");
                            }
                        }
                        if (!isValid)
                        {
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(txtCPF.Text) || string.IsNullOrEmpty(txtVCode.Text))
                {
                    isValid = false;
                    lblErrorMessages.Text = GetLocalResourceObject("NotCPF") as string;
                }
                else
                {
                    var validNumber = IsValidCPF(txtCPF.Text, txtVCode.Text);
                    if (!validNumber)
                    {
                        isValid = validNumber;
                        lblErrorMessages.Text = GetLocalResourceObject("NotValidCPF") as string;
                    }
                    else
                    {
                        SessionInfo.BRPF = string.Format("{0}-{1}", txtCPF.Text, txtVCode.Text);
                        SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                    }
                }
            }

            _paymentError = !isValid;

            if (_paymentError)
            {
                OnCardAddOrEdit(null, null);
            }
            UpdatePanel1.Update();
            return isValid;
        }

        /// <summary>
        ///     Validate the CPF number
        /// </summary>
        /// <param name="number">First 9 digist</param>
        /// <param name="validationDigist">Last two digits to validate</param>
        /// <returns>Validation flag</returns>
        private bool IsValidCPF(string number, string validationDigits)
        {
            int sum1 = 0;
            int sum2 = 0;
            int const1 = 10;
            int const2 = 11;
            string fDig = string.Empty;
            string sDig = string.Empty;

            if (number.Length != 9 || validationDigits.Length != 2 || sameDigitsInCPF(number)) return false;

            foreach (var dig in number)
            {
                sum1 += int.Parse(dig.ToString())*const1;
                sum2 += int.Parse(dig.ToString())*const2;
                const1--;
                const2--;
            }

            var rest1 = sum1%11;
            fDig = (rest1 <= 1) ? "0" : (11 - rest1).ToString();
            sum2 += int.Parse(fDig)*const2;

            var rest2 = sum2%11;
            sDig = (rest2 <= 1) ? "0" : (11 - rest2).ToString();
            return string.Format("{0}{1}", fDig, sDig).Equals(validationDigits);
        }

        private bool sameDigitsInCPF(string numbers)
        {
            var spl = numbers.Split(numbers[0]);
            if (spl.Count() == 10) return true;
            return false;
        }

        /// <summary>
        ///     Defines if the data will send to FControl validation
        /// </summary>
        /// <returns></returns>
        private bool ForFControlValidation()
        {
            if (!HLConfigManager.Configurations.PaymentsConfiguration.FControlValidation)
                return false;

            // Validating against the level list from value object
            var levelToValidate = Enum.GetNames(typeof (DistributorLevelType)).ToList();
            return levelToValidate.Contains(string.Format("_{0}", DistributorProfileModel.TypeCode));
        }
    }
}