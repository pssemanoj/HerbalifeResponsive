using System;
using System.Collections;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    /// <summary>
    ///     DataPagerGridView is a custom control that implements GrieView and IPageableItemContainer
    /// </summary>
    public class DataPagerGridView : GridView, IPageableItemContainer
    {
        public DataPagerGridView()
        {
            PagerSettings.Visible = false;
        }

        /// <summary>
        ///     TotalRowCountAvailable event key
        /// </summary>
        private static readonly object EventTotalRowCountAvailable = new object();

        /// <summary>
        ///     Call base control's CreateChildControls method and determine the number of rows in the source
        ///     then fire off the event with the derived data and then we return the original result.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="dataBinding"></param>
        /// <returns></returns>
        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            int rows = base.CreateChildControls(dataSource, dataBinding);

            //  if the paging feature is enabled, determine the total number of rows in the datasource
            if (AllowPaging)
            {
                //  if we are databinding, use the number of rows that were created, otherwise cast the datasource to an Collection and use that as the count
                int totalRowCount = dataBinding ? rows : ((ICollection) dataSource).Count;

                //  raise the row count available event
                IPageableItemContainer pageableItemContainer = this;
                OnTotalRowCountAvailable(new PageEventArgs(pageableItemContainer.StartRowIndex,
                                                           pageableItemContainer.MaximumRows, totalRowCount));

                //  make sure the top and bottom pager rows are not visible
                if (TopPagerRow != null)
                    TopPagerRow.Visible = false;

                if (BottomPagerRow != null)
                    BottomPagerRow.Visible = false;
            }
            return rows;
        }

        /// <summary>
        ///     Set the control with appropriate parameters and bind to right chunk of data.
        /// </summary>
        /// <param name="startRowIndex"></param>
        /// <param name="maximumRows"></param>
        /// <param name="databind"></param>
        void IPageableItemContainer.SetPageProperties(int startRowIndex, int maximumRows, bool databind)
        {
            int newPageIndex = (startRowIndex/maximumRows);
            PageSize = maximumRows;

            if (PageIndex != newPageIndex)
            {
                bool isCanceled = false;
                if (databind)
                {
                    //  create the event arguments and raise the event
                    var args = new GridViewPageEventArgs(newPageIndex);
                    OnPageIndexChanging(args);

                    isCanceled = args.Cancel;
                    newPageIndex = args.NewPageIndex;
                }

                //  if the event wasn't cancelled change the paging values
                if (!isCanceled)
                {
                    PageIndex = newPageIndex;

                    if (databind)
                        OnPageIndexChanged(EventArgs.Empty);
                }
                if (databind)
                    RequiresDataBinding = true;
            }
        }

        /// <summary>
        ///     IPageableItemContainer's StartRowIndex = PageSize * PageIndex properties
        /// </summary>
        int IPageableItemContainer.StartRowIndex
        {
            get { return PageSize*PageIndex; }
        }

        /// <summary>
        ///     IPageableItemContainer's MaximumRows  = PageSize property
        /// </summary>
        int IPageableItemContainer.MaximumRows
        {
            get { return PageSize; }
        }

        /// <summary>
        /// </summary>
        event EventHandler<PageEventArgs> IPageableItemContainer.TotalRowCountAvailable
        {
            add { base.Events.AddHandler(EventTotalRowCountAvailable, value); }
            remove { base.Events.RemoveHandler(EventTotalRowCountAvailable, value); }
        }

        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTotalRowCountAvailable(PageEventArgs e)
        {
            var handler = (EventHandler<PageEventArgs>) base.Events[EventTotalRowCountAvailable];
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}