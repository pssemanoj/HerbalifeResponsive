using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using Resources;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class TodaysMagazine : UserControlBase
    {
        public bool UpdateCalled { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (APFDueProvider.containsOnlyAPFSku(ProductsBase.ShoppingCart.CartItems) || ProductsBase.ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
            {
                this.Visible = false;
                return;
            }

            if (!IsPostBack)
                DisplayTodaysMagazine(string.Empty);
        }

        private void DisplayTodaysMagazine(string message)
        {
            lblError.Visible = true;
            lblError.Text = message;

            if (HLConfigManager.Configurations.DOConfiguration.CheckTodaysMagazineAvailability)
            {
                if (CheckIfUserHasSubscribedToTodaysMagazine()) return;
            }

            if (CheckIfEventTicketOrdering()) return;

            if (!CheckIfProductPresentInCart()) return;

            if (CheckIfTodaysMagazinePresentInShoppingCart()) return;

            if (CheckIfOnlyAPF()) return;

            displayLocaleSpecificRadioButtons();

            if (!checkInventory()) return;
            MyHLShoppingCart cart = (ProductsBase).ShoppingCart;
            if (HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax > 1 ||
                (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity && cart.TodaysMagaZineQuantity > 1))
            {
                this.tbQuantity.ReadOnly = false;
                this.tbQuantity.Visible = true;
            }
            else
            {
                this.tbQuantity.ReadOnly = true;
                this.tbQuantity.Visible = true;
            }

            tbQuantity.Attributes["onkeypress"] = "Numeric(event, this)";
            if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity)
            {
                tbQuantity.Text = cart.TodaysMagaZineQuantity.ToString();
            }
            else
            {
                tbQuantity.Text = "1";
            }
        }

        private bool CheckIfEventTicketOrdering()
        {
            if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
            {
                this.divTodaysMagazine.Visible = false;
                displayTodaysMagazine(false);
                return true;
            }
            else
            {
                this.divTodaysMagazine.Visible = true;
                displayTodaysMagazine(true);
                return false;
            }
        }


        private bool CheckIfOnlyAPF()
        {
            if (APFDueProvider.containsOnlyAPFSku(ProductsBase.ShoppingCart.CartItems))
            {
                this.divTodaysMagazine.Visible = false;
                displayTodaysMagazine(false);
                return true;
            }
            else
            {
                this.divTodaysMagazine.Visible = true;
                displayTodaysMagazine(true);
                return false;
            }
        }


        private bool CheckIfTodaysMagazinePresentInShoppingCart()
        {
            MyHLShoppingCart cart = ShoppingCart;

            string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
            string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
            int todayMagazineMax = HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax;

            if (primarySku != String.Empty)
            {
                
                if (cart.CartItems.Exists(c  => c.SKU == primarySku || c.SKU == secondarySku))
                {
                    int selectedItems = 1;
                    var cartItems = cart.CartItems.Where(i => i.SKU == primarySku || i.SKU == secondarySku);
                    if (null != cartItems && cartItems.Count() > 0)
                    {
                        var cartItem = cartItems.First();
                        selectedItems = cartItem.Quantity;
                    }

                    // Ukraine and Hungary have their own max qty
                    if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity)
                    {
                        todayMagazineMax = cart.TodaysMagaZineQuantity;
                    }

                    //if user had added maximum quantity to the cart.
                    if (selectedItems >= todayMagazineMax)
                    {
                        displayTodaysMagazine(false);
                        this.lblError.Text = base.GetLocalResourceObject("MagazineAddedToCart") as string;
                        return true;
                    }    

                    //If user removes from cart and is less than the max quantity.
                    if (selectedItems < todayMagazineMax)
                    {
                        displayTodaysMagazine(true);
                        return false;
                    }
                }
            }
            return false;
        }

        private void displayLocaleSpecificRadioButtons()
        {
            string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
            string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;

            MyHLShoppingCart cart = (ProductsBase).ShoppingCart;

            //checks if primary sku is present in the catalog.
            CatalogItem_V01 catItem = CatalogProvider.GetCatalogItem(primarySku,
                                            (this.Page as ProductsBase).CountryCode);
            
            if (catItem != null && !IsBlocked(catItem))
            {
                this.divTodaysMagazine.Visible = true;
                displayTodaysMagazine(true);
                //SetlocaleDescription(Locale.Substring(3));
                //this.rbSecondaryLanguage.Visible = false;
            }
            else //Primary sku not present, do not display both primary and secondary.
            {
                this.rbPrimaryLanguage.Visible = false;
                this.rbSecondaryLanguage.Visible = false;
                this.divTodaysMagazine.Visible = false;
                this.divTodaysMagazine.Style.Add(System.Web.UI.HtmlTextWriterStyle.Display, "none");
                displayTodaysMagazine(false);
                return;
            }

            //if secondary sku is not present, then do not show the radio buttons.
            if (secondarySku.Equals(string.Empty))
            {
                this.rbPrimaryLanguage.Visible = false;
                this.rbSecondaryLanguage.Visible = false;
                return;
            }

            //if primary sku is present, check for secondary sku in the catalog.
            CatalogItem_V01 catSecItem = CatalogProvider.GetCatalogItem(secondarySku,(this.Page as ProductsBase).CountryCode);
            if (catSecItem != null && !IsBlocked(catSecItem))
            {
                //SetlocaleDescription(Locale.Substring(3));
                this.rbSecondaryLanguage.Visible = true;
            }
            else
            {
                this.rbPrimaryLanguage.Visible = 
                    this.rbSecondaryLanguage.Visible = false;
            }

            //Set the default selection to primary...
            if ((this.rbPrimaryLanguage != null) && (this.rbSecondaryLanguage != null))
            {
                this.rbPrimaryLanguage.Checked = true;
                this.rbSecondaryLanguage.Checked = false;
            }
        }

        //private string SetlocaleDescription(string locale)
        //{
        //    string description = string.Empty;
        //    switch (locale.ToLower())
        //    {
        //        case "us":
        //            this.rbPrimaryLanguage.Text = string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSkuName) ? "English" :
        //                HLConfigManager.Configurations.DOConfiguration.TodayMagazineSkuName;
        //            this.rbSecondaryLanguage.Text = string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySkuName) ? "Spanish" :
        //                HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySkuName;
        //            break;

        //        case "ca":
        //            this.rbPrimaryLanguage.Text = string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSkuName) ? "English" :
        //                HLConfigManager.Configurations.DOConfiguration.TodayMagazineSkuName;
        //            this.rbSecondaryLanguage.Text = string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySkuName) ? "French" :
        //                HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySkuName;
        //            break;

        //        default:
        //            break;
        //    }

        //    return description;
        //}

        private bool CheckIfProductPresentInCart()
        {
            MyHLShoppingCart cart = ShoppingCart;

            if ((cart.ShoppingCartItems != null) && (!cart.ShoppingCartItems.Count.Equals(0)))
            {
                List<CatalogItem_V01> catItems = cart.ShoppingCartItems.Select(s => s.CatalogItem).ToList<CatalogItem_V01>();
                //There should be at least one product in the cart if there is type restriction
                if (catItems.Any(c => c.ProductType == ProductType.Product) || 
                    !HLConfigManager.Configurations.DOConfiguration.TodayMagazineProdTypeRestricted)
                {
                    displayTodaysMagazine(true);
                    return true;
                }
            }
            //no items in the cart.
            this.divTodaysMagazine.Visible = false;
            displayTodaysMagazine(false);
            return false;
        }

        private bool CheckIfUserHasSubscribedToTodaysMagazine()
        {
            if ((this.Page as ProductsBase).TodaysMagazine)
            {
                this.divTodaysMagazine.Visible = false;
                displayTodaysMagazine(false);
                return true;
            }
            else
            {
                this.divTodaysMagazine.Visible = true;
                displayTodaysMagazine(true);
                return false;
            }
        }

        private void displayTodaysMagazine(bool value)
        {
            this.lblTodaysMagazine.Visible = value;
            this.tbQuantity.Visible = value;
            this.btnAddToCart.Visible = value;
            this.rbPrimaryLanguage.Visible = value ? !HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku.Equals(string.Empty) && this.rbPrimaryLanguage.Visible : value;
            this.rbSecondaryLanguage.Visible = value ? !HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku.Equals(string.Empty) && this.rbSecondaryLanguage.Visible : value;
        }

        private bool checkInventory()
        {
            bool hasInventory = true;

            int qty = 1;
            if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity)
            {
                MyHLShoppingCart cart = (ProductsBase).ShoppingCart;
                qty = cart.TodaysMagaZineQuantity;
            }

            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku))
                hasInventory = CheckInventory(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku, 1);
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku))
                hasInventory = hasInventory || CheckInventory(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku, 1);


            this.divTodaysMagazine.Visible = hasInventory;
            displayTodaysMagazine(hasInventory);
            return hasInventory;

        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            int quantity = 0;
            if (!string.IsNullOrEmpty(tbQuantity.Text))
            {
                int.TryParse(tbQuantity.Text.Trim(), out quantity);
            }

            if (CheckInventory(GetSelectedSKU(), quantity))
            {
                AddTodaysMagazineToCart();
                lblError.Visible = true;
               // lblError.Text = string.Empty;
            }
            else
            {
                lblError.Visible = true;
            }
        }

        private string GetSelectedSKU()
        {
            string selectedSku = string.Empty;
            if ((this.rbPrimaryLanguage != null) && (this.rbPrimaryLanguage.Checked))
                selectedSku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;

            if ((this.rbSecondaryLanguage != null) && (this.rbSecondaryLanguage.Checked))
                selectedSku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;

            if (((this.rbPrimaryLanguage == null) && (this.rbSecondaryLanguage == null)) || 
                ((!this.rbPrimaryLanguage.Checked && !this.rbSecondaryLanguage.Checked)))
                selectedSku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;

            return selectedSku;
        }
        private bool CheckInventory(string skuToCheck,int quantity)
        {
            int inventoryQty =0;

            if (!string.IsNullOrEmpty(skuToCheck))
            {
                CatalogItem_V01 catItem = CatalogProvider.GetCatalogItem(skuToCheck, this.ProductsBase.CountryCode);
                if (catItem != null)
                {
                    WarehouseInventory warehouseInventory;
                    if (catItem.InventoryList != null && catItem.InventoryList.TryGetValue(ProductsBase.CurrentWarehouse, out warehouseInventory))
                    {
                        var warehouseInventory01 = warehouseInventory as WarehouseInventory_V01;
                        if (warehouseInventory01 != null && !warehouseInventory01.IsBlocked)
                        {
                            inventoryQty = ShoppingCartProvider.CheckInventory(catItem, quantity,
                                                                                this.ProductsBase.CurrentWarehouse);
                        }
                    }
                }
            }

            lblError.Text  = inventoryQty >0 ? string.Empty :
                string.Format(MyHL_ErrorMessage.OutOfInventory, skuToCheck);

            return inventoryQty > 0;
        }

        
        [SubscribesTo(MyHLEventTypes.NotifyTodaysMagazineRecalculate)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            DisplayTodaysMagazine(string.Empty);
           //todayMagazinePanel.Update();
            UpdateCalled = true;     
        }


        [SubscribesTo(MyHLEventTypes.NotifyTodaysMagazineCancelOrder)]
        public void OnCancelOrder(object sender, EventArgs e)
        {
            DisplayTodaysMagazine(string.Empty);
            //todayMagazinePanel.Update();
            UpdateCalled = true;     
        }

        private bool IsBlocked(CatalogItem_V01 catItem)
        {
            MyHLShoppingCart cart = (ProductsBase).ShoppingCart;

            if (null != cart && null != cart.DeliveryInfo && catItem.InventoryList != null && catItem.InventoryList.ContainsKey(cart.DeliveryInfo.WarehouseCode))
            {
                WarehouseInventory warehouseInventory = catItem.InventoryList[cart.DeliveryInfo.WarehouseCode];
                if (null != warehouseInventory)
                {
                    WarehouseInventory_V01 inventory = warehouseInventory as WarehouseInventory_V01;
                    if(inventory != null)
                        return inventory.IsBlocked;
                }
            }
            return true;
        }

        private bool CheckInventory(CatalogItem_V01 catItem)
        {
            int inventoryQty = 0;
            int quantity = 0;
            if (null != catItem)
            {

                if (!string.IsNullOrEmpty(tbQuantity.Text))
                {
                    int.TryParse(tbQuantity.Text.Trim(), out quantity);
                }
                else
                {
                    quantity = 0;
                }

                MyHLShoppingCart cart = (ProductsBase).ShoppingCart;
                
                
                if (null != cart && null != cart.DeliveryInfo)
                {
                    inventoryQty = ShoppingCartProvider.CheckInventory(catItem, quantity, cart.DeliveryInfo.WarehouseCode);
                }
            }

            return inventoryQty > 0;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (UpdateCalled)
            {
                todayMagazinePanel.Update();
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
          
                DisplayTodaysMagazine(string.Empty);
                //todayMagazinePanel.Update();
                UpdateCalled = true;     
        }



        private void AddTodaysMagazineToCart()
        {
            int quantity;
            MyHLShoppingCart myShoppingCart = ShoppingCart;

            List<ShoppingCartItem_V01> products = new List<ShoppingCartItem_V01>();

           // if( this.ViewState["primaryCount"] == null )
                this.ViewState["primaryCount"] = int.Parse(this.tbQuantity.Text.Trim());

            //Add the donation amount to cart
            if (this.ViewState["primaryCount"] != null)
            {
                quantity = int.Parse(this.ViewState["primaryCount"].ToString());

                if (!HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity &&
                    (quantity <= 0 || quantity > (int)HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax))
                {
                    lblError.Visible = true;
                    lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazineMax + HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax.ToString() + ".");
                }
                else if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity &&
                (quantity <= 0 || quantity > myShoppingCart.TodaysMagaZineQuantity))
                {
                    lblError.Visible = true;
                    lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazineMax + myShoppingCart.TodaysMagaZineQuantity.ToString() + ".");
                }
                else
                {
                    if (HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku != null)
                    {
                        try
                        {
                            myShoppingCart.DeleteTodayMagazine(GetSelectedSKU());

                            ProductsBase.AddTodayMagazine(quantity, GetSelectedSKU());
                            IEnumerable<string> ruleResultMessages =
                                (from r in ShoppingCart.RuleResults
                                 where r.Result == RulesResult.Failure && r.RuleName == "SkuLimitation Rules"
                                 select r.Messages[0]);
                            if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                            {
                                lblError.Text = ruleResultMessages.Distinct().ToList().First();
                                lblError.Visible = true;
                                todayMagazinePanel.Update();
                                return;
                            }

                            DisplayTodaysMagazine(MyHL_ErrorMessage.AddTodayMagazine);
                            todayMagazinePanel.Update();
                           // lblError.Visible = true;
                           // lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazine);
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error("TodaysMagazine Can not be added to cart!\n" + ex.ToString());
                        }
                    }
                }
            }

            //Add the donation amount to cart
            if (this.ViewState["secondaryCount"]  != null)
            {
                quantity = int.Parse(this.ViewState["secondaryCount"].ToString());

                if (quantity <= 0 || (!HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity && quantity > (int)HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax) ||
                    (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity && quantity > myShoppingCart.TodaysMagaZineQuantity))
                {
                    lblError.Visible = true;
                    lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazineMax + HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax.ToString() + ".");
                }
                else
                {
                    if (HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku != null)
                    {
                        try
                        {
                            myShoppingCart.DeleteTodayMagazine(GetSelectedSKU());
                            ProductsBase.AddTodayMagazine(quantity, GetSelectedSKU());
                            DisplayTodaysMagazine(MyHL_ErrorMessage.AddTodayMagazine);
                            todayMagazinePanel.Update();
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error("TodaysMagazine Can not be added to cart!\n" + ex.ToString());
                        }
                    }
                }
            }
        }

        protected void rbPrimaryLanguage_OnCheckedChanged(Object sender, EventArgs e)
        {

        }

        protected void rbSecondaryLanguage_OnCheckedChanged(Object sender, EventArgs e)
        {
        }

        protected void tbQuantityTextChanged(Object sender, EventArgs e)
        {
            int quantity = int.Parse(this.tbQuantity.Text.Trim());
            //If user enters 0.
            if (quantity.Equals(0))
            {
                this.lblError.Text = MyHL_ErrorMessage.AddTodayMagazineEnterNumber;
                return;
            }

            MyHLShoppingCart cart = ShoppingCart;

            string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
            string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
            int todayMagazineMax = HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax;

            if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity)
            {
                todayMagazineMax = cart.TodaysMagaZineQuantity;
            }

            int primaryCountInCart = primarySku.Equals("") ? 0 : cart.CartItems.Count(c => c.SKU.Equals(primarySku));
            int secondaryCountInCart = secondarySku.Equals("") ? 0 : cart.CartItems.Count(c => c.SKU.Equals(secondarySku));
            
            //check for radio buttons
            if ((this.rbPrimaryLanguage != null) && (this.rbPrimaryLanguage.Checked))
                this.ViewState["primaryCount"] = quantity;

            if ((this.rbSecondaryLanguage != null) && (this.rbSecondaryLanguage.Checked))
                this.ViewState["secondaryCount"] = quantity;

            if ((this.rbPrimaryLanguage == null) && (this.rbSecondaryLanguage == null))
                this.ViewState["primaryCount"] = quantity;


            if (this.ViewState["primaryCount"] != null)
            {
                //if user had added maximum quantity to the cart.
                if ((primaryCountInCart + int.Parse(this.ViewState["primaryCount"].ToString())) > todayMagazineMax)
                {
                    this.lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazineMaXQuantity, todayMagazineMax);
                    return;
                }

                //If user enters 0.
                if (int.Parse(this.ViewState["primaryCount"].ToString()) .Equals(0))
                {
                    this.lblError.Text = MyHL_ErrorMessage.AddTodayMagazineEnterNumber;
                    return;
                }

            }

            if (this.ViewState["secondaryCount"] != null)
            {
                //if user had added maximum quantity to the cart.
                if ((secondaryCountInCart + int.Parse(this.ViewState["secondaryCount"].ToString())) > todayMagazineMax)
                {
                    this.lblError.Text = string.Format(MyHL_ErrorMessage.AddTodayMagazineMaXQuantity, todayMagazineMax );
                    return;
                }

                //If user enters 0.
                if (int.Parse(this.ViewState["secondaryCount"].ToString()).Equals(0))
                {
                    this.lblError.Text = MyHL_ErrorMessage.AddTodayMagazineEnterNumber;
                    return;
                }
            }
        }
    }
}
