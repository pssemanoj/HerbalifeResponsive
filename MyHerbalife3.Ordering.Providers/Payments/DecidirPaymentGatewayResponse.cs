using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class DecidirPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string Order = "noperacion";
        private const string AuthResult = "resultado";
        private const string GateWay = "Agency";
        private const string AuthorizationNumber = "codautorizacion";
        private const string Returning = "Returning";
        private const string PaymentGateWayName = "Decidir";
        private const string TheCardType = "tarjeta";
        private const string TheCardNumber = "nrotarjetavisible";
        private const string Visa = "Visa";
        private const string MasterCard = "MasterCard";
        private const string AmericanExpress = "Amex";
        private const string Naranja = "Tarjeta Naranja";

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                this.ReloadShoppingCart = true;
                if (QueryValues[GateWay] == PaymentGateWayName)
                {
                    canProcess = true;
                    OrderNumber = QueryValues[Order];
                    string decidirOrderNumber = PostedValues[Order];
                    if (!string.IsNullOrEmpty(OrderNumber) && !string.IsNullOrEmpty(decidirOrderNumber) && OrderNumber != decidirOrderNumber)
                    {
                        LogSecurityWarning(PaymentGateWayName);
                        return canProcess;
                    }
                    if (!string.IsNullOrEmpty(QueryValues[Returning]))
                    {
                        if (!string.IsNullOrEmpty(OrderNumber))
                        {
                            this.IsReturning = true;
                            this.CanSubmitIfApproved = false;
                            this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                            this.IsApproved = (this.Status == PaymentGatewayRecordStatusType.Approved || this.Status == PaymentGatewayRecordStatusType.OrderSubmitted);
                            return canProcess;
                        }
                    }
                    else
                    {
                        OrderNumber = decidirOrderNumber;
                        if (string.IsNullOrEmpty(PostedValues[AuthResult]) || PostedValues[AuthResult] == "ERROR")
                        {
                            base.AuthResultMissing = true;
                        }
                        else
                        {
                            this.CanSubmitIfApproved = true;
                            this.Status = OrderProvider.GetPaymentGatewayRecordStatus(OrderNumber);
                            IsApproved = PostedValues[AuthResult] == "APROBADA";
                            //Get cardtpye and cardnumber from the POST call
                            CardNumber = PostedValues[TheCardNumber];
                            AuthorizationCode = PostedValues[AuthorizationNumber];
                        }
                    }                    
                }

                return canProcess;
            }
        }

        public DecidirPaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
        }

        protected override bool DetermineSubmitStatus()
        {
            //POST and Redirect are supposedly ALWAYS POST first, then client Redirect
            if (IsReturning)
            {
                //This is a Client Redirect
                return false;
            }
            else
            {
                //This is a Server Post
                return true;
            }
        }

        public override void GetPaymentInfo(SerializedOrderHolder holder)
        {
            ServiceProvider.SubmitOrderBTSvc.Payment payment = holder.BTOrder.Payments[0];
            var orderPayment = (holder.Order as Order_V01).Payments[0] as CreditPayment_V01;
            string theCardType = PostedValues[TheCardType];

            switch (theCardType)
            {
                case Visa:
                    {
                        CardType = IssuerAssociationType.Visa;
                        break;
                    }
                case MasterCard:
                    {
                        CardType = IssuerAssociationType.MasterCard;
                        break;
                    }
                case AmericanExpress:
                    {
                        CardType = IssuerAssociationType.AmericanExpress;
                        break;
                    }
                case Naranja:
                    {
                        CardType = IssuerAssociationType.TarjetaNaranja;
                        break;
                    }
                default:
                    {
                        if (theCardType.StartsWith(MasterCard, StringComparison.CurrentCultureIgnoreCase))
                            CardType = IssuerAssociationType.MasterCard;
                        break;
                    }
            }
            payment.PaymentCode = CreditCard.CardTypeToHPSCardType(CardType);
            orderPayment.Card.IssuerAssociation = CreditCard.GetCardType(payment.PaymentCode);
        }
    }
}
