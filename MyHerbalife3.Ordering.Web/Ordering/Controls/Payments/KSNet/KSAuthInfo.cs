using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public class KSAuthInfo
    {
        public const string Key = "KSAuthInfo";

        public KSAuthInfo(int amount, int cardCode, int installments, int BCPoints, string name)
        {
            this.Amount = amount;
            this.CardCode = cardCode;
            this.Installments = installments;
            this.BCPoints = BCPoints;
            this.CurrencyType = 410;
            this.Name = name;
        }

        public string OrdeNumber {get; set;}
        public string Name  {get; set;}
        public string IdNum  {get; set;}
        public string Email  {get; set;}
        public string PhoneNumber  {get; set;}
        public int Amount {get; set;}
        public int CurrencyType  {get; set;}
        public int CardCode  {get; set;}
        public int Installments  {get; set;}
        public int BCPoints { get; set; }
    }
 }           
             //ordernumber = Request["ordernumber"];
             //amount = "30122"; // Request["amount"];
             //goodname = "Gerry Hayes"; // Request["goodname"];
             //idnum = Request["idnum"];
             //email = Request["email"];                      
             //phoneno = Request["phoneno"];
             //currencytype = "410";
             //cardcode = Request["cardcode"];
             //installments = Request["installments"];


