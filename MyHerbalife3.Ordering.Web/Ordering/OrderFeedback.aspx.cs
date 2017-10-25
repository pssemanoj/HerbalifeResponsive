using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.Web.MasterPages;
using Telerik.Web.UI;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OrderFeedback : ProductsBase
    {
        private GetFeedbackResponse_V01 _feedBack;
        private const int Id_CellIdx = 1;
        private const int ItemName_CellIdx = 2;
        private const int ItemType_CellIdx = 3;

        protected void Page_Load(object sender, EventArgs e)
        {

            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            ViewState["OrderHeaderId"] = Request["OrderHeaderId"] ?? string.Empty;
            BindGrid();
        }

        private void BindGrid()
        {
            _feedBack = OrderProvider.GetFeedBack(DistributorID);
            if (_feedBack != null && _feedBack.GetFeedbackResult != null && _feedBack.GetFeedbackResult.FeedBackItems!= null)
            {
                var dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[4]
                    {
                        new DataColumn("Id", typeof (string)),
                        new DataColumn("ItemName", typeof (string)),
                        new DataColumn("ItemType", typeof (string)),
                        new DataColumn("ItemTextInfo", typeof (string))
                    });
                foreach (var fb in _feedBack.GetFeedbackResult.FeedBackItems.FeedBackItem)
                {
                    dt.Rows.Add(fb.Id, fb.ItemName, fb.ItemType);
                }
                FeedBackGridView.DataSource = dt;
                FeedBackGridView.DataBind();
            }
            
        }

        protected void FeedBackGridView_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var lblItemName = new Label {ID = "lblItemName"};
                var dataRowView = e.Row.DataItem as DataRowView;
                if (dataRowView != null)
                {
                    var id = 0;
                    int.TryParse(dataRowView.Row["Id"].ToString(), out id);
                    lblItemName.Text = dataRowView.Row["ItemName"].ToString();
                    var itemType = dataRowView.Row["ItemType"].ToString();
                    e.Row.Cells[ItemName_CellIdx].Controls.Add(lblItemName);
                    //var itemText = dataRowView.Row["ItemText"].ToString();
                    switch (itemType)
                    {
                        case "RB":
                            var feedBackItem =
                                (_feedBack.GetFeedbackResult.FeedBackItems.FeedBackItem.Where(c => c.Id == id)
                                         .Select(d => d.ItemText)).FirstOrDefault();
                            if (feedBackItem != null)
                            {
                                var rblist = new RadioButtonList {ID = "rblItemText", RepeatDirection = RepeatDirection.Horizontal};
                                foreach (var text in feedBackItem)
                                {
                                    rblist.Items.Add(new ListItem(text));
                                }
                                rblist.Items[0].Selected = true;
                                var lblRemark = new Label { ID = "lblRemark", Text = GetLocalResourceString("Remark") };
                                var txtRemark = new TextBox {ID = "txtRemark"};
                                e.Row.Cells[ItemName_CellIdx].Controls.Add(rblist);
                                e.Row.Cells[ItemName_CellIdx].Controls.Add(lblRemark);
                                e.Row.Cells[ItemName_CellIdx].Controls.Add(txtRemark);
                            }
                            break;
                        case "IT":
                            var tbFeedBack = new TextBox() { ID = "tbFeedBack", MaxLength = 50};
                            e.Row.Cells[ItemName_CellIdx].Controls.Add(tbFeedBack);
                            break;
                        case "MS":
                            var feedBackCbItem =
                                (_feedBack.GetFeedbackResult.FeedBackItems.FeedBackItem.Where(c => c.Id == id)
                                         .Select(d => d.ItemText)).FirstOrDefault();
                            var cblist = new CheckBoxList() { ID = "cbItemText", RepeatDirection = RepeatDirection.Horizontal };
                            if (feedBackCbItem != null)
                            {
                                foreach (var text in feedBackCbItem)
                                {
                                    cblist.Items.Add(new ListItem(text));
                                }
                                e.Row.Cells[ItemName_CellIdx].Controls.Add(cblist);
                            }
                            break;
                        default:
                            LoggerHelper.Error("Item Type not supported");
                            break;

                    }

                }
                
            }
        }

        protected void btnSaveFeedBack_OnClick(object sender, EventArgs e)
        {
            foreach (GridViewRow row in FeedBackGridView.Rows)
            {
                var dataRowView = row;
                if (dataRowView != null)
                {
                    var lblId = row.Cells[Id_CellIdx].Text;// dataRowView// Row["Id"].ToString();
                    var id = 0;
                    int.TryParse(lblId, out id);
                    var itemType = row.Cells[ItemType_CellIdx].Text;
                    var feedBackItem =
                                (_feedBack.GetFeedbackResult.FeedBackItems.FeedBackItem.Where(c => c.Id == id))
                                    .FirstOrDefault();
                    switch (itemType)
                    {
                        case "RB":
                            var itemText = (RadioButtonList) row.FindControl("rblItemText");
                            var txtRemark = (TextBox) row.FindControl("txtRemark");
                            if (feedBackItem != null)
                            {
                                feedBackItem.Value = itemText.SelectedValue;
                                feedBackItem.Remark = txtRemark.Text;
                            }
                            break;
                        case "IT":
                            var tbItemText = (TextBox)row.FindControl("tbFeedBack");
                            if (feedBackItem != null)
                            {
                                feedBackItem.Value = tbItemText.Text;
                            }
                            break;
                        case "MS":
                            var cbItemText = (CheckBoxList)row.FindControl("cbItemText");
                            if (feedBackItem != null)
                            {
                                feedBackItem.Value = cbItemText.SelectedValue;
                            }
                            break;
                        default:
                            LoggerHelper.Error("Item Type not supported");
                            break;
                    }

                }
            }
            if (_feedBack != null && _feedBack.GetFeedbackResult != null)
            {
                var orderHeaderId = 0;
                int.TryParse(ViewState["OrderHeaderId"].ToString(), out orderHeaderId);
                _feedBack.GetFeedbackResult.OrderHeaderId = orderHeaderId;
                OrderProvider.UpdateFeedBack(DistributorID, "", _feedBack.GetFeedbackResult);
                Session["FeedbackSaved"] = true;
                Response.Redirect("~/Ordering/OrderListView.aspx");
            }
            
        }

        protected void btnCancelFeedBack_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/OrderListView.aspx");
        }
    }
}