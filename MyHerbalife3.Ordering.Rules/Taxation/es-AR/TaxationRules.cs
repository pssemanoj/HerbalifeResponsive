using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.es_AR
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            bool IsNewArgentinaTaxStructure = Settings.GetRequiredAppSetting<bool>("EnableNewArgentinaTaxStructure", false);

            if (IsNewArgentinaTaxStructure)
            {
                NewTaxationRules(order, locale);
            }
            else
            {
                #region Old Tax Structure

                Message message = new Message();
                message.MessageType = "ContributorClass";
                string contributorClass = string.Empty;

                order.Messages = new MessageCollection();

                //Step 1
                //if DS is not AR COP then we're done!
                if (DistributorProfileModel.ProcessingCountryCode != "AR")
                {
                    message.MessageValue = "F";
                    order.Messages.Add(message);
                    return;
                }
                try
                {
                    //continue with calc            
                    //step 1.2
                    int legalProvince = 0;
                    int shippingProvince = 0;

                    var legalAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.PermanentLegal, order.DistributorID, Country);
                    var locallegalAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.PermanentLegalLocal, order.DistributorID, Country);

                    var shippingAddress = (order.Shipment as ShippingInfo_V01).Address;
                    if (null != legalAddress)
                    {
                        if (!Int32.TryParse(legalAddress.StateProvinceTerritory.Trim(), out legalProvince))
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "AR Taxation Rules failed for Distributor {0}. legalAddress.StateProvinceTerritory value is invalid: {1}",
                                    order.DistributorID, legalAddress.StateProvinceTerritory.Trim()));
                        }
                    }
                    if (null != shippingAddress)
                    {
                        if (!Int32.TryParse(shippingAddress.StateProvinceTerritory.Trim(), out shippingProvince))
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "AR Taxation Rules failed for Distributor {0}. shippingAddress.StateProvinceTerritory value is invalid: {1}",
                                    order.DistributorID, shippingAddress.StateProvinceTerritory.Trim()));
                        }
                    }

                    List<TaxIdentification> tinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
                    if (23 == legalProvince && 23 == shippingProvince) //TIERA DEL FUEGO
                    {
                        contributorClass = "X";
                    }
                    else
                    {
                        if (null != tinList && null != order)
                        {
                            bool hasARTX = (tinList.Find(t => t.IDType.Key == "ARTX") != null);
                            bool hasARVT = (tinList.Find(t => t.IDType.Key == "ARVT") != null);
                            bool hasCUIT = (tinList.Find(t => t.IDType.Key == "CUIT") != null);
                            //step 1.5
                            if (hasARTX && hasCUIT)
                            {
                                contributorClass = "M";
                            }
                            //step 1.7
                            else if (hasARVT && hasCUIT)
                            {
                                contributorClass = "I";
                            }
                            //step 1.8
                            else if (!hasARVT && !hasARTX)
                            {
                                contributorClass = "N";
                            }
                        }
                    }

                    //Step 2
                    //step 2.1 and 2.3
                    if (tinList.Find(t1 => t1.IDType.Key == "IBBA") != null ||
                        tinList.Find(t2 => t2.IDType.Key.Substring(0, 2) == "IB") == null || 2 == shippingProvince)
                    //2 == BEUNOS AIRES
                    {
                        contributorClass += "BA";
                    }
                    //step 2.4
                    if (tinList.Find(t => t.IDType.Key == "IBSF") != null || 20 == shippingProvince) //SANTA FE
                    {
                        contributorClass += "SF";
                    }
                    //step 2.5
                    if (tinList.Find(t => t.IDType.Key == "IBER") != null || 24 == shippingProvince) //ENTRE RIOS
                    {
                        contributorClass += "ER";
                    }
                    //step 2.6
                    if (tinList.Find(t => t.IDType.Key == "IBJJ") != null || 9 == shippingProvince) //JUJUY
                    {
                        contributorClass += "JJ";
                    }
                    //step 2.7
                    if (tinList.Find(t => t.IDType.Key == "IBSL") != null || 18 == shippingProvince) //SAN LUIS
                    {
                        contributorClass += "SL";
                    }
                    //step 2.8
                    if (tinList.Find(t => t.IDType.Key == "IBTU") != null || 22 == shippingProvince) //TUCUMAN
                    {
                        contributorClass += "TU";
                    }
                    //step 2.9
                    if (tinList.Find(t => t.IDType.Key == "IBCF") != null || 1 == shippingProvince) //CAPITAL FEDERAL
                    {
                        contributorClass += "CF";
                    }

                    message.MessageValue = contributorClass;
                    order.Messages.Add(message);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("AR Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                      order.DistributorID, ex.Message));
                }

                #endregion 
            }
        }

        private void NewTaxationRules(Order_V01 order, string locale)
        {
            Message message = new Message();
            message.MessageType = "ContributorClass";
            Message addionalMsg = new Message();
            addionalMsg.MessageType = "AdditionalClass";
            string contributorClass = string.Empty;
            string additionalClass = string.Empty;

            order.Messages = new MessageCollection();

            //Step 1
            //if DS is not AR COP then we're done!
            //Immaterial of Country of Processing we need to make DS flow thorugh step1 and step2,So commenting the return code.
            //if (DistributorProfileModel.ProcessingCountryCode != "AR")
            //{
            //    message.MessageValue = "F";
            //    order.Messages.Add(message);
            //    return;
            //}
            try
            {
                //continue with calc            
                //step 1.2
                int legalProvince = 0;
                int shippingProvince = 0;

                var legalAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.PermanentLegal, order.DistributorID, Country);
                var locallegalAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.PermanentLegalLocal, order.DistributorID, Country);

                var shippingAddress = (order.Shipment as ShippingInfo_V01).Address;
                if (null != legalAddress)
                {
                    if (!Int32.TryParse(legalAddress.StateProvinceTerritory.Trim(), out legalProvince))
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "AR Taxation Rules failed for Distributor {0}. legalAddress.StateProvinceTerritory value is invalid: {1}",
                                order.DistributorID, legalAddress.StateProvinceTerritory.Trim()));
                    }
                }
                if (null != shippingAddress)
                {
                    if (!Int32.TryParse(shippingAddress.StateProvinceTerritory.Trim(), out shippingProvince))
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "AR Taxation Rules failed for Distributor {0}. shippingAddress.StateProvinceTerritory value is invalid: {1}",
                                order.DistributorID, shippingAddress.StateProvinceTerritory.Trim()));
                    }
                }

                //This Fix is only For iKiosk - We do not receive statecode.GDO UI sends statecode so GDO works.
                if (HLConfigManager.Platform == "iKiosk" && shippingProvince == 0)
                {
                    IShippingProvider provider = ShippingProvider.GetShippingProvider(shippingAddress.Country);
                    if (provider != null)
                    {
                        var lookupResults = provider.GetStatesForCountry(shippingAddress.Country);
                        if (lookupResults != null && lookupResults.Count > 0)
                        {
                            foreach (var province in lookupResults)
                            {
                                string[] state = province.Split('-');
                                if (state[0] == shippingAddress.StateProvinceTerritory.Trim())
                                {
                                    shippingProvince = Convert.ToInt32(state[1]);
                                    break;
                                }
                            }
                        }
                    }
                }



                List<TaxIdentification> tinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
                bool hasARTX = (tinList.Find(t => t.IDType.Key == "ARTX") != null);
                bool hasARVT = (tinList.Find(t => t.IDType.Key == "ARVT") != null);
                bool hasCUIT = (tinList.Find(t => t.IDType.Key == "CUIT") != null);
                bool hasARCM = (tinList.Find(t => t.IDType.Key == "ARCM") != null);
                bool hasARRS = (tinList.Find(t => t.IDType.Key == "ARRS") != null);
                bool hasARID = (tinList.Find(t => t.IDType.Key == "ARID") != null);
                bool ISAR = DistributorProfileModel.ProcessingCountryCode == "AR";


                if (23 == shippingProvince) //TIERA DEL FUEGO
                {
                    if (null != tinList && null != order)
                    {
                        if (hasCUIT && hasARTX)
                        {
                            contributorClass = "Y";
                        }
                        if (hasARVT && hasCUIT)
                        {
                            contributorClass = "X";
                        }
                        if ((hasARID && !hasARTX && !hasARVT) || (!hasARID && !hasARTX && !hasARVT && !ISAR))
                        {
                            contributorClass = "Z";
                        }
                    }
                }
                else
                {
                    if (null != tinList && null != order)
                    {
                        //step 1.5
                        if (hasARTX && hasCUIT)
                        {
                            contributorClass = "M";
                        }
                        //step 1.7
                        else if (hasARVT && hasCUIT)
                        {
                            contributorClass = "I";
                        }
                        
                       //step 1.8
                        else if (!hasARVT && !hasARTX && !hasCUIT)
                        {
                            contributorClass = "N";
                        }
                    }
                }

                //Step-2 New Implementation

                if (hasARCM)
                {
                    contributorClass += "C";
                }
                else
                {
                    if (hasARRS)
                    {
                        contributorClass += "S";
                    }
                }

                //Call Create Contributor Class String
                string concatentionClass = string.Empty;
                CreateContributorClass(hasARCM, hasARVT, hasARTX, hasARRS, shippingProvince, tinList, out additionalClass, out concatentionClass);
                contributorClass += concatentionClass;

                message.MessageValue = contributorClass;
                order.Messages.Add(message);
                addionalMsg.MessageValue = additionalClass;
                order.Messages.Add(addionalMsg);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("AR Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }

        private void CreateContributorClass(bool hasARCM,bool hasARVT, bool hasARTX, bool hasARRS, int shippingProvince, List<TaxIdentification> tinList, out string additionalClass, out string contributorClass)
        {
            additionalClass = string.Empty;
            contributorClass = string.Empty;

            //Additional Class Extraction
            //ARTX/ARCM/ARVT
            if(hasARCM)
            {
                additionalClass = "ARCM";
            }
            if(hasARTX)
            {
                additionalClass += "/ARTX";
            }
            if(hasARVT)
            {
                additionalClass += "/ARVT";
            }

                //BEUNOS AIRES shippingProvince = 2;legalProvince = 2
                if (tinList.Find(t1 => t1.IDType.Key == "IBBA") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBBA" && t.IDType.ExemptFlag) != null);

                    if (!IsExempt)
                    {
                        if (shippingProvince == 2)
                        {
                            contributorClass += "EI/BA";
                        }
                        else
                        {
                            contributorClass += "BA";
                        }
                    }

                    
                }
                else
                {
                    if (shippingProvince == 2)
                    {
                        contributorClass += "E/BA";
                    }
                    else
                    {
                        contributorClass += "D/BA";
                    }
                    
                }

                //SANTA FE shippingProvince = 20;legalProvince = 20
                if (tinList.Find(t1 => t1.IDType.Key == "IBSF") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBSF" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 20)
                        {
                            contributorClass += "EI/SF";
                        }
                        else
                        {
                            contributorClass += "SF";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 20)
                    {
                        contributorClass += "E/SF";
                    }
                    
                }

                //ENTRE RIOS shippingProvince = 24;legalProvince = 24
                if (tinList.Find(t1 => t1.IDType.Key == "IBER") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBER" && t.IDType.ExemptFlag) != null);
                    if (IsExempt || !hasARRS)
                    {
                        if (shippingProvince == 24)
                        {
                            contributorClass += "EI/ER";
                        }
                        else
                        {
                            contributorClass += "ER";
                        }
                    }
                    
                }
                else
                {
                    if (shippingProvince == 24)
                    {
                        contributorClass += "E/ER";
                    }
                   
                }

                //JUJUY - shippingProvince = 9;legalProvince = 9
                if (tinList.Find(t1 => t1.IDType.Key == "IBJJ") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBJJ" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 9)
                        {
                            contributorClass += "EI/JJ";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 9)
                    {
                        contributorClass += "E/JJ";
                    }
                    
                }

                //SAN LUIS shippingProvince == 18;legalProvince = 18
                if (tinList.Find(t1 => t1.IDType.Key == "IBSL") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBSL" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 18)
                        {
                            contributorClass += "EI/SL";
                        }
                    }
                    
                }
                else
                {
                    if (shippingProvince == 18)
                    {
                        contributorClass += "E/SL";
                    }
                    
                }

                //TUCUMAN shippingProvince == 22;legalProvince = 22
                if (tinList.Find(t1 => t1.IDType.Key == "IBTU") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBTU" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 22)
                        {
                            contributorClass += "EI/TU";
                        }
                        else
                        {
                            contributorClass += "TU";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 22)
                    {
                        contributorClass += "E/TU";
                    }
                    
                }

                //CAPITAL FEDERAL shippingProvince == 1;legalProvince = 1
                if (tinList.Find(t1 => t1.IDType.Key == "IBCF") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBCF" && t.IDType.ExemptFlag) != null);
                    if (IsExempt || !hasARRS)
                    {
                        if (shippingProvince == 1)
                        {
                            contributorClass += "EI/CF";
                        }
                        else
                        {
                            contributorClass += "CF";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 1)
                    {
                        contributorClass += "E/CF";
                    }
                    
                }

                //MISIONES  shippingProvince == 13;legalProvince = 13
                if (tinList.Find(t1 => t1.IDType.Key == "IBMI") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBMI" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 13)
                        {
                            contributorClass += "EI/MI";
                        }
                        else
                        {
                            contributorClass += "MI";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 13)
                    {
                        contributorClass += "E/MI";
                    }
                   
                }

                //SAN JUAN  shippingProvince == 17;legalProvince = 17
                if (tinList.Find(t1 => t1.IDType.Key == "IBSJ") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBSJ" && t.IDType.ExemptFlag) != null);
                    if (IsExempt || !hasARRS)
                    {
                        if (shippingProvince == 17)
                        {
                            if (hasARTX)
                            {
                                contributorClass += "EI/SJ";
                                //additionalClass = "ARTX";
                            }
                            else
                            {
                                contributorClass += "EI/SJ";
                            }

                        }
                    }
                }
                else
                {
                    if (shippingProvince == 17)
                    {
                        if (hasARTX)
                        {
                            contributorClass += "E/SJ";
                            //additionalClass = "ARTX";
                        }
                        else
                        {
                            contributorClass += "E/SJ";
                        }
                    }
                    
                }

                //FORMOSA shippingProvince == 8;legalProvince = 8 
                if (tinList.Find(t1 => t1.IDType.Key == "IBFO") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBFO" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 8)
                        {
                            if(hasARCM)
                            {
                                if (hasARVT)
                                {
                                    contributorClass += "EI/FO";
                                    //additionalClass += "/ARCM/ARVT";
                                }
                                else
                                {
                                    contributorClass += "EI/FO";
                                    //additionalClass += "/ARCM";
                                }
                            }
                            else
                            {
                                if (hasARVT)
                                {
                                    contributorClass += "EI/FO";
                                    //additionalClass += "/ARVT";
                                }
                                else
                                {
                                    contributorClass += "EI/FO";
                                }
                            }
                        }
                        else
                        {
                            if (hasARCM)
                            {
                                if (hasARVT)
                                {
                                    contributorClass += "FO";
                                    //additionalClass += "/ARCM/ARVT";
                                }
                                else
                                {
                                    contributorClass += "FO";
                                    //additionalClass += "/ARCM";
                                }
                            }
                            else
                            {
                                if (hasARVT)
                                {
                                    contributorClass += "FO";
                                    //additionalClass += "/ARVT";
                                }
                                else
                                {
                                    contributorClass += "FO";
                                }
                            }
                        }
                    }
                    
                }
                else
                {
                    if (shippingProvince == 8)
                    {
                        if (hasARVT)
                        {
                            contributorClass += "E/FO";
                            //additionalClass += "/ARVT";
                        }
                        else
                        {
                            contributorClass += "E/FO";
                        }

                    }
                    
                }

                //CORDOBA  shippingProvince == 4;legalProvince = 4
                if (tinList.Find(t1 => t1.IDType.Key == "IBCO") != null)
                {
                    bool IsExempt = (tinList.Find(t => t.IDType.Key == "IBCO" && t.IDType.ExemptFlag) != null);
                    if (!IsExempt)
                    {
                        if (shippingProvince == 4)
                        {
                            contributorClass += "EI/CO";
                        }
                        else
                        {
                            contributorClass += "CO";
                        }
                    }
                   
                }
                else
                {
                    if (shippingProvince == 4)
                    {
                        contributorClass += "E/CO";
                    }
                   
                }
            
        }

    }
}