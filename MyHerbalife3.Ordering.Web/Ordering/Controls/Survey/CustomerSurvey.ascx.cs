using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using Telerik.Web.UI;
using OrderProvider = MyHerbalife3.Ordering.Providers.China.OrderProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Survey
{
    public partial class CustomerSurvey : UserControlBase
    {
        private const string surveyMessage = "您的调研问卷已成功提交，谢谢！";

        protected const string Id_NeedComment_Prefix = "uiNeedComment_";
        protected const string Attribute_ControlToAssociate = "ctlAssc";
        protected const string Attribute_ValidatorId = "vldtId";
        protected const string Css_NeedComment_Input = "cssNeedComment";
        protected const string Css_NeedComment_Section = "cssNeedCommentSect";
        
        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as OrderingMaster).SetPageHeader("客户调查");
            var test = OrderProvider.GetCustomerSurvey(DistributorID);
            if (test != null)
            {
                var tableOptions = new HtmlTable();
                foreach (var questionse in test)
                {
                    PopulateQuestion(questionse, tableOptions, 1);
                }

                plQuestion.Controls.Add(tableOptions);

            }



        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="question"></param>
        /// <param name="options"></param>
        /// <param name="answerList"></param>
        /// <param name="tableOptions"></param>
        /// <param name="questionIndex"></param>
        private void PopulateQuestion(SurveyQuestions question, HtmlTable tableOptions, int questionIndex)
        {
            var rowQuestion = new HtmlTableRow();
            var cellQuestion = new HtmlTableCell();
            var cellQuestionTitle = new HtmlTableCell();
            var strongQuestion = new HtmlGenericControl();

            rowQuestion.Controls.Add(cellQuestionTitle);
            rowQuestion.Controls.Add(cellQuestion);
            tableOptions.Controls.Add(rowQuestion);

            cellQuestionTitle.Controls.Add(strongQuestion);

            if (question == null || question.ListSurveyOptions.Count == 0)
            {
                cellQuestion.Controls.Add(new Label { Text = "QuestionsNotAvailableText" });
            }
            else
            {
                //cellQuestionTitle.Controls.Add(new HtmlGenericControl("BR"));
                cellQuestion.Controls.Add(new Label { Text = question.QuestionText });
                cellQuestion.Attributes.Add("class", "question-title");
                //cellQuestion.Controls.Add(new HtmlGenericControl("BR"));
                var strongOption = new HtmlGenericControl();

                var cellOptionTitle = new HtmlTableCell();

                cellOptionTitle.Controls.Add(strongOption);

                var isFirstOption = true;

                foreach (var option in question.ListSurveyOptions)
                {
                    var rowOption = new HtmlTableRow();
                    var cellOption = new HtmlTableCell();
                    var cellOptionLabel = new HtmlTableCell();

                    if (isFirstOption)
                    {
                        rowOption.Controls.Add(cellOptionTitle);
                        isFirstOption = false;
                    }
                    else
                    {
                        rowOption.Controls.Add(cellOptionLabel);
                    }
                    if (option.Type == QuestionType.Feedback)
                    {
                        cellOption.Controls.Add(
                       new TextBox
                       {

                           ID = string.Format("txtOption{0}", option.Id),
                       });
                    }
                    else if (option.Type == QuestionType.MultipleChoice)
                    {
                        var ui = new CheckBox
                        {
                            Text = option.OptionText,
                            ID = string.Format("chkOption{0}", option.Id),
                        };
                        ui.Attributes["onChange"] = "OnSurveyOptionChange($(this));";

                        cellOption.Controls.Add(ui);

                        cellOption.Attributes.Add("class", "check-option");
                        
                        if (option.NeedComment) AppendNeedCommentUI(cellOption, option, ui);
                    }
                    else
                    {
                        var ui = new RadioButton
                        {
                            Text = option.OptionText,
                            ID = string.Format("rdOption{0}", option.Id),
                            GroupName = string.Format("optionsQuestion{0}", question.Id),
                            Checked = false   
                        };
                        ui.Attributes["onChange"] = "OnSurveyOptionChange($(this));";

                        cellOption.Controls.Add(ui);

                        if (option.NeedComment) AppendNeedCommentUI(cellOption, option, ui);
                    }

                    rowOption.Controls.Add(cellOption);
                    
                    tableOptions.Controls.Add(rowOption);
                }

                var rowSpace = new HtmlTableRow();
                var cellSpace = new HtmlTableCell { ColSpan = 2 };

                cellSpace.Controls.Add(new HtmlGenericControl("HR"));

                rowSpace.Controls.Add(cellSpace);
                tableOptions.Controls.Add(rowSpace);
            }
        }

        /// <summary>
        /// Needed for workaround ClientId issue, which only can work properly in OnPreRender() event.
        /// </summary>
        class NeedCommentViewModel
        {
            public HtmlControl Container { get; set; }
            public CheckBox ControlToAssociat { get; set; }
            public RequiredFieldValidator ValidatorToAssociat { get; set; }
        }

        List<NeedCommentViewModel> NeedCommentVMList = new List<NeedCommentViewModel>();

        protected override void OnPreRender(EventArgs e)
        {
            #region workaround ClientId issue
            if ((NeedCommentVMList != null) && (NeedCommentVMList.Count > 0))
            {
                foreach (var vm in NeedCommentVMList)
                {
                    vm.Container.Attributes[Attribute_ControlToAssociate] = vm.ControlToAssociat.ClientID;
                    vm.Container.Attributes[Attribute_ValidatorId] = vm.ValidatorToAssociat.ClientID;
                }
            }
            #endregion

            base.OnPreRender(e);
        }

        void AppendNeedCommentUI(HtmlTableCell cell, SurveyOptions option, CheckBox uiAssociatedBy)
        {
            var uiSpan = new HtmlGenericControl("div");
            uiSpan.Attributes["class"] = Css_NeedComment_Section;

            var uiId = string.Format("{0}{1}", Id_NeedComment_Prefix, option.Id);

            var tb = new TextBox
            {
                ID = uiId,
                CssClass = Css_NeedComment_Input,
            };

            RequiredFieldValidator vldReq = new RequiredFieldValidator
            {
                ID = string.Format("req{0}", uiId),
                ControlToValidate = tb.ID,
                Display = ValidatorDisplay.Static,
                ErrorMessage = "必填选项",
                EnableClientScript = true,
            };

            uiSpan.Controls.Add(tb);
            uiSpan.Controls.Add(vldReq);

            // Need to paste the associating client controls' ClientId to the newly created control.
            // ClientId doesn't work properly at this stage, have to do this in OnPreRender event.
            NeedCommentVMList.Add(new NeedCommentViewModel { Container = uiSpan, ControlToAssociat = uiAssociatedBy, ValidatorToAssociat = vldReq });

            cell.Controls.Add(uiSpan);
        }

        protected void BtnSaveClick(object sender, EventArgs e)
        {
            var surveyQuestionswithOptions = OrderProvider.GetCustomerSurvey(DistributorID);
            SelectionDetail selection;
            var lstselection = new List<SelectionDetail>();
            int surveyID = SessionInfo.surveyDetails.SurveyId;
            int SkuQuentity = SessionInfo.surveyDetails.SurveySKUQuantity;
            string freeSKU = SessionInfo.surveyDetails.SurveySKU;
            if (surveyQuestionswithOptions != null)
            {
                foreach (var q in surveyQuestionswithOptions)
                {

                    selection = new SelectionDetail();
                    var answerChkList = (from option in q.ListSurveyOptions
                                         let optionControl =
                                             plQuestion.FindControl("chkOption" + option.Id) as CheckBox
                                         where optionControl != null
                                         where optionControl.Checked
                                         select option.Id).ToList();
                    var answerTxtList = (from option in q.ListSurveyOptions
                                         let optionControl =
                                             plQuestion.FindControl("txtOption" + option.Id) as TextBox
                                         where optionControl != null
                                         select optionControl.Text).ToList();
                    var answerRdList = (from option in q.ListSurveyOptions
                                        let optionControl =
                                            plQuestion.FindControl("rdOption" + option.Id) as RadioButton
                                        where optionControl != null
                                        where optionControl.Checked
                                        select option.Id).ToList();
                    if (answerChkList != null && answerChkList.Any())
                    {
                        foreach (var i in answerChkList)
                        {
                            selection = new SelectionDetail();

                            selection.QuestionId = q.Id;
                            selection.OptionSelectionID = i;
                            lstselection.Add(selection);

                            CaptureComment(selection, plQuestion, q, i);
                        }
                    }
                    else if (answerTxtList != null && answerTxtList.Any() && !string.IsNullOrEmpty(answerTxtList[0]))
                    {

                        selection.QuestionId = q.Id;
                        selection.Feedback = answerTxtList[0];
                        lstselection.Add(selection);

                    }
                    else if (answerRdList != null && answerRdList.Any())
                    {
                        foreach (var i in answerRdList)
                        {
                            selection.QuestionId = q.Id;
                            selection.OptionSelectionID = i;
                            lstselection.Add(selection);

                            CaptureComment(selection, plQuestion, q, i);
                        }
                    }

                }
                var noOfQuestions = lstselection.GroupBy(qst => qst.QuestionId)
                       .Select(grp => grp.First())
                       .ToList();
                if (noOfQuestions.Count() == surveyQuestionswithOptions.Count())
                {
                    var result = OrderProvider.SubmitCustomerSurvey(DistributorID, surveyID, lstselection);
                    if ((result == null) || (result.Status != ServiceResponseStatusType.Success))
                    {
                        lblErrorMessage.Text = "无法提交。请重试";
                        lblErrorMessage.Visible = true;
                    }
                    else
                    {
                        lblErrorMessage.Text = "";
                        lblErrorMessage.Visible = false;
                        OrderProvider.AddFreeGift(freeSKU, SkuQuentity, (ProductsBase).ProductInfoCatalog.AllSKUs, ProductsBase.CurrentWarehouse, ShoppingCart);
                        ScriptManager.RegisterStartupScript(this, GetType(), "popup",
                                                            "alert('"+surveyMessage+"');" +
                                                            "window.location='ShoppingCart.aspx';", true);
                       
                    }
                }
                else
                {
                    lblErrorMessage.Text = "请填写所有领域";
                    lblErrorMessage.Visible = true;
                }
            }
             


        }
     
        private bool NeedComment(SurveyQuestions q, int surveyOptionId)
        {
            var svOpList = q.ListSurveyOptions;
            if ((svOpList == null) || (svOpList.Count == 0)) return false;

            var m = svOpList.FirstOrDefault(x => x.Id == surveyOptionId);
            return (m != null) ? m.NeedComment : false;
        }

        private void CaptureComment(SelectionDetail dataObject, PlaceHolder uiObject, SurveyQuestions q, int surveyOptionId)
        {
            if (!NeedComment(q, surveyOptionId)) return;

            var uiComment = uiObject.FindControl(Id_NeedComment_Prefix + surveyOptionId) as TextBox;
            if (uiComment == null) return;

            var txt = uiComment.Text;
            if (string.IsNullOrWhiteSpace(txt)) return;

            dataObject.Feedback = txt.Trim();
        }

        protected void BtnCancelClick(object sender, EventArgs e)
        {
            Session["CustomerSurveyCancelled"] = true;
            Response.Redirect("Shoppingcart.aspx");
        }
        
       
    }
}