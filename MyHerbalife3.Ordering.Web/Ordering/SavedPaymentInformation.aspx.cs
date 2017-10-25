using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

//using System.Data;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class SavedPaymentInformation : ProductsBase
    {
        private int endRecordIndex;
        private ModalPopupExtender mpPaymentInformation;

        private PaymentInformation paymentInformation;
        private List<PaymentInformation> paymentInformations;
        private String sortDirection;
        private String sortExpression;

        private int startRecordIndex;
        private UserControlBase ucPaymentInfoControl;

        [Publishes(MyHLEventTypes.CreditCardProcessing)]
        public event EventHandler onCreditCardProcessing;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            }
            if (GlobalContext.CurrentExperience.ExperienceType == Shared.ViewModel.ValueObjects.ExperienceType.Green
                && HLConfigManager.Configurations.DOConfiguration.ChangeOrderingLeftMenuMyHL3)
            {
                (Master as OrderingMaster).IsleftMenuVisible = true;
                (Master as OrderingMaster).IsleftOrderingMenuVisible = false;
            }
            var paymentsControl =
                LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentInfoControl);
            ucPaymentInfoControl = paymentsControl as UserControlBase;
            phPaymentInfoControl.Controls.Add(paymentsControl);

            if (HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry)
            {
                if (gvSavedPaymentInformation.Columns.Count > 0)
                {
                    gvSavedPaymentInformation.Columns[gvSavedPaymentInformation.Columns.Count - 1].Visible = false;
                    btnAddPaymentInfo.Visible = false;
                }
            }
            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;
            mpPaymentInformation = (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

            loadPaymentInformation();

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-no-right-nav");
        }

        /// <summary>Mask the card number for display</summary>
        /// <param name="cardNum">The card number</param>
        /// <returns>The masked value</returns>
        protected string getCardNumber(string cardNum, string cardType)
        {
            if (string.IsNullOrEmpty(cardNum))
            {
                return string.Empty;
            }
            else
            {
                cardNum = cardNum.Trim();
                return "- " + (cardNum.Length > 4 ? cardNum.Substring(cardNum.Length - 4) : "");
            }
        }

        protected string GetAlias(PaymentInformation Pi)
        {
            string alias = Pi.Alias;
            if (string.IsNullOrEmpty(alias))
            {
                string name = string.Concat(Pi.CardHolder.First + Pi.CardHolder.Last);
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }
                alias = string.Concat(name, " - ", Pi.CardType, " - ", getCardNumber(Pi.CardNumber, Pi.CardType));
            }

            return alias;
        }

        //string getPaymentInfoControlPath(ref bool isXML)
        //{
        //    //if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PaymentInfoingConfiguration.GridviewCellPaymentInfo))
        //    //{
        //    //    isXML = false;
        //    //    return HLConfigManager.Configurations.PaymentInfoingConfiguration.PaymentInfo;
        //    //}
        //    isXML = true;
        //    return HLConfigManager.Configurations.PaymentInfoingConfiguration.GridviewCellPaymentInfo;
        //}

        //protected PaymentInfoBase createPaymentInfo(string controlPath, bool isXML)
        //{
        //    try
        //    {
        //        if (!isXML)
        //        {
        //            PaymentInformationBase = LoadControl(controlPath) as PaymentInfoBase;
        //        }
        //        else
        //        {
        //            PaymentInformationBase = new PaymentInfoControl();
        //            PaymentInformationBase.XMLFile = controlPath;
        //        }
        //    }
        //    catch (HttpException ex)
        //    {
        //        ExceptionPolicy.HandleException(ex, MyHerbalife3.Shared.Providers.ProviderPolicies.SYSTEM_EXCEPTION);
        //    }
        //    return PaymentInformationBase;
        //}

        //protected void gvSavedPaymentInformation_DataBinding(object sender, EventArgs e)
        //{

        //}

        protected void gvSavedPaymentInformation_DataBound(object sender, EventArgs e)
        {
            if (gvSavedPaymentInformation == null) return;

            endRecordIndex = (gvSavedPaymentInformation.PageIndex + 1)*gvSavedPaymentInformation.PageSize;

            int totalRecords = 0;

            if (paymentInformations != null)
                totalRecords = paymentInformations.Count();

            if (endRecordIndex > totalRecords)
                endRecordIndex = totalRecords;

            if (totalRecords == 0)
                startRecordIndex = 0;
            else
                startRecordIndex = gvSavedPaymentInformation.PageIndex*gvSavedPaymentInformation.PageSize + 1;

            String resourceShowing = GetLocalResourceObject("Showing.Text") as string;
            String resourceOf = GetLocalResourceObject("Of.Text") as string;
            String resourceResults = GetLocalResourceObject("Results.Text") as string;

            var sb = new StringBuilder(resourceShowing + " ");

            sb.Append(startRecordIndex.ToString());
            if (startRecordIndex != endRecordIndex)
            {
                sb.Append(" - ");
                sb.Append(endRecordIndex.ToString());
            }
            sb.Append(" " + resourceOf + " ");
            sb.Append(totalRecords + " ");
            sb.Append(resourceResults);
            string recordCountHeaderRow = sb.ToString();
            // Get the header row.

            var lbl = new Label();

            lbl.Text = recordCountHeaderRow;
            var headerRow = gvSavedPaymentInformation.HeaderRow;
            if (headerRow != null)
            {
                headerRow.Cells[6].Controls.Add(lbl);
            }
        }

        protected void gvSavedPaymentInformation_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var gridView = (GridView) sender;
            if (e.Row.RowType == DataControlRowType.Header)
            {
                foreach (DataControlField field in gridView.Columns)
                {
                    if (HLConfigManager.Configurations.DOConfiguration.DisableAddressSortingInOrderPreferences)
                    {
                        field.SortExpression = String.Empty;
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int paymentId = Convert.ToInt32(gvSavedPaymentInformation.DataKeys[e.Row.RowIndex]["ID"]);

                var deleteButton = (LinkButton) e.Row.Cells[6].Controls[5];

                if (getSelectedPaymentInfo(paymentId).IsPrimary)
                {
                    //e.Row.Style.Add("background-color", "#ffff66");
                    e.Row.CssClass = "gdo-row-selected gdo-body-text";
                    e.Row.FindControl("Primary").Visible = true;

                    // defect 23978. Should disable primary
                    //deleteButton.Enabled = false;
                }

                var cardNumber = (Label) e.Row.FindControl("CardNumber");
                if (cardNumber != null && !String.IsNullOrEmpty(cardNumber.Text))                
                {
                    cardNumber.Text = "-" + cardNumber.Text.Substring(cardNumber.Text.Trim().Length - 4);                   
                }

                // Retrieve the LinkButton control from the last column.
                var editButton = (LinkButton) e.Row.Cells[6].Controls[3];

                // Set the LinkButton's CommandArgument property with the
                // row's index.
                editButton.CommandArgument = e.Row.RowIndex.ToString();
                deleteButton.CommandArgument = e.Row.RowIndex.ToString();

                var trigger1 = new AsyncPostBackTrigger();
                trigger1.ControlID = editButton.UniqueID;
                UpdatePanel2.Triggers.Add(trigger1);

                var trigger2 = new AsyncPostBackTrigger();
                trigger2.ControlID = deleteButton.UniqueID;
                UpdatePanel2.Triggers.Add(trigger2);
            }
        }

        private static void gvSavedPaymentInformation_GridView(object sender, GridViewRowEventArgs e)
        {
            var dGgridView = (GridView) sender;
            if (dGgridView.SortExpression.Length > 0)
            {
                int cellIndex = -1;
                foreach (DataControlField field in dGgridView.Columns)
                {
                    if (field.SortExpression == dGgridView.SortExpression)
                    {
                        cellIndex = dGgridView.Columns.IndexOf(field);
                        break;
                    }
                }
                if (cellIndex > -1)
                {
                    if (e.Row.RowType == DataControlRowType.Header)
                    {
                        //  this is a header row,set the sort style
                        e.Row.Cells[cellIndex].CssClass +=
                            (dGgridView.SortDirection == SortDirection.Ascending ? " sortasc" : " sortdesc");
                    }
                }
            }
        }

        protected void btnAddPaymentInfo_Click(object sender, EventArgs e)
        {
            onCreditCardProcessing(this,
                                   new PaymentInfoEventArgs(PaymentInfoCommandType.Add, new PaymentInformation(), false));
            var mpPaymentInformation =
                (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");
            mpPaymentInformation.Show();
        }

        protected void gvSavedPaymentInformation_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditCommand" && e.CommandName != "DeleteCommand") return;

            int index = Convert.ToInt32(e.CommandArgument);
            int rowIndex;
            rowIndex = index;

            // Retrieve the row that contains the button clicked 
            // by the user from the Rows collection.
            var row = gvSavedPaymentInformation.Rows[rowIndex];
            int paymentId = Convert.ToInt32(gvSavedPaymentInformation.DataKeys[row.RowIndex]["ID"]);

            var PaymentInformation = getSelectedPaymentInfo(paymentId);

            string distributorId = (Page as ProductsBase).DistributorID;
            if (PaymentInformation != null)
            {
                switch (e.CommandName)
                {
                    case "EditCommand":
                        {
                            paymentInformation = PaymentInfoProvider.GetPaymentInfo(distributorId,
                                                                                    (Page as ProductsBase).Locale,
                                                                                    paymentId);

                            onCreditCardProcessing(this,
                                                   new PaymentInfoEventArgs(PaymentInfoCommandType.Edit,
                                                                            paymentInformation, false));
                            mpPaymentInformation.Show();
                        }

                        return;
                    case "DeleteCommand":
                        {
                            paymentInformation = PaymentInfoProvider.GetPaymentInfo(distributorId,
                                                                                    (Page as ProductsBase).Locale,
                                                                                    paymentId);
                            onCreditCardProcessing(this,
                                                   new PaymentInfoEventArgs(PaymentInfoCommandType.Delete,
                                                                            paymentInformation, false));
                            mpPaymentInformation.Show();
                        }
                        return;

                    default:
                        return;
                }
            }
        }

        private PaymentInformation getSelectedPaymentInfo(int paymentId)
        {
            try
            {
                var paymentInformationList = paymentInformations.Where(s => s.ID == paymentId);
                if (paymentInformationList.Count() == 0)
                {
                    paymentInformationList = paymentInformations.Where(s => s.IsPrimary);
                }
                return paymentInformationList.Count() == 0 ? null : paymentInformationList.First();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void gvSavedPaymentInformation_RowEditing(object sender, GridViewEditEventArgs e)
        {
        }

        protected void gvSavedPaymentInformation_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void gvSavedPaymentInformation_RowCreated(object sender, GridViewRowEventArgs e)
        {
        }

        protected void gvSavedPaymentInformation_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSavedPaymentInformation.PageIndex = e.NewPageIndex;
            gvSavedPaymentInformation.DataBind();
        }

        protected void gvSavedPaymentInformation_Sorting(object sender, GridViewSortEventArgs e)
        {
            // get values from viewstate   
            sortExpression = ViewState["_GridView1LastSortExpression_"] as string;
            sortDirection = ViewState["_GridView1LastSortDirection_"] as string;
            // on first time header clicked ( including different header), sort ASCENDING    
            if (e.SortExpression != sortExpression)
            {
                sortExpression = e.SortExpression;
                sortDirection = "Ascending";
            }
                // on second time header clicked, toggle sort    
            else
            {
                if (sortDirection == "Ascending")
                {
                    sortExpression = e.SortExpression;
                    sortDirection = "Descending";
                }
                    // Descending        
                else
                {
                    sortExpression = e.SortExpression;
                    sortDirection = "Ascending";
                }
            }
            gvSavedPaymentInformation.PageIndex = 0;
            gvSavedPaymentInformation_DataBind(sortExpression, sortDirection);
        }

        private void gvSavedPaymentInformation_DataBind(String sortExpression, String sortDirection)
        {
            // save state for next time   
            ViewState["_GridView1LastSortDirection_"] = sortDirection;
            ViewState["_GridView1LastSortExpression_"] = sortExpression;

            IEnumerable<PaymentInformation> q;
            List<PaymentInformation> list = null;

            try
            {
                if (paymentInformations != null)
                {
                    if (sortDirection == "Descending")
                    {
                        q = from m in paymentInformations
                            orderby m.IsPrimary descending, OrderBy(sortExpression, m) descending
                            select m;
                    }
                    else
                    {
                        if (sortExpression == "Alias")
                        {
                            q = from m in paymentInformations
                                orderby m.IsPrimary descending,
                                    (string.IsNullOrEmpty(m.Alias) ? "zzzzz" : m.Alias) ascending,
                                    string.Concat(m.CardHolder.First, m.CardHolder.Last,
                                                  m.CardType,
                                                  (string.IsNullOrEmpty(m.CardNumber)) ? "" : m.CardNumber.Substring(m.CardNumber.Length - 4, 4)) ascending,
                                    OrderBy(sortExpression, m)
                                select m;
                        }
                        else
                        {
                            q = from m in paymentInformations
                                orderby m.IsPrimary descending, OrderBy(sortExpression, m)
                                select m;
                        }
                    }
                    list = q.ToList();
                }
            }
            catch (Exception)
            {
                list = null;
            }

            gvSavedPaymentInformation.DataSource = list;
            gvSavedPaymentInformation.DataBind();
        }

        private object OrderBy(String sortKey, PaymentInformation PaymentInformation)
        {
            switch (sortKey)
            {
                case "Alias":
                    return PaymentInformation.Alias;
                case "CardHolder":
                    return PaymentInformation.CardHolder.First;
                case "CardType":
                    return PaymentInformation.CardType;
                default:
                    break;
            }

            //return PaymentInformation.Alias;
            return null;
        }

        [SubscribesTo(MyHLEventTypes.CreditCardProcessed)]
        public void OnCreditCardProcessed(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
        }

        private void loadPaymentInformation()
        {
            var savedPaymentInfos = new List<PaymentInformation>();
            try
            {
                paymentInformations =
                    PaymentInfoProvider.GetPaymentInfo((Page as ProductsBase).DistributorID,
                                                       (Page as ProductsBase).Locale)
                                       .Where(s => s.ID < 99999991)
                                       .ToList();
            }
            catch (Exception)
            {
                return;
            }

            savedPaymentInfos = (from p in paymentInformations where !p.IsTemporary select p).ToList();
            if (null == savedPaymentInfos)
            {
                savedPaymentInfos = new List<PaymentInformation>();
            }
            paymentInformations = savedPaymentInfos;

            gvSavedPaymentInformation.DataSource = paymentInformations;
            gvSavedPaymentInformation.DataBind();

            if (sortExpression == null)
            {
                sortExpression = "Alias";
            }

            if (sortDirection == null)
            {
                sortDirection = "Ascending";
            }
            gvSavedPaymentInformation_DataBind(sortExpression, sortDirection);

            if (savedPaymentInfos != null && savedPaymentInfos.Count > 0)
            {
                tblPaymentInfo.Visible = true;
            }
            else
            {
                tblPaymentInfo.Visible = false;
            }
        }

        /// <summary>The standin date for MyKey card</summary>
        /// <returns>The date</returns>
        protected DateTime getMyKeyExpirationDate()
        {
            return new DateTime(2049, 12, 31);
        }

        /// <summary>Mask the card number for display</summary>
        /// <param name="cardType">The card number</param>
        /// <returns>The masked value</returns>
        protected string getCardName(string cardType)
        {
            string cardName = cardType;
            if (!string.IsNullOrEmpty(cardType))
            {
                cardName =
                    GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                            string.Format("CardType_{0}_Description", cardType)) as string;
            }

            return cardName;
        }
    }
}