using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.GlobalAddress;
using Control = MyHerbalife3.Ordering.Web.Ordering.Controls.GlobalAddress.Control;
using ControlCollection = MyHerbalife3.Ordering.Web.Ordering.Controls.GlobalAddress.ControlCollection;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    [ToolboxData("<{0}:AddressControl runat=server></{0}:AddressControl>")]
    [ParseChildren(true)]
    [Designer("HL.MyHerbalife.GlobalAddress.AddressControlDesigner, AddressControl", typeof (IDesigner))]
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class AddressControl : CompositeControl, AddressBase
    {
        public enum Mode
        {
            Edit = 0,
            Display = 1
        };

        //   private ITemplate _containerTemplate;
        private HtmlGenericControl _containerTable;
        private object _dataContext;
        private Mode _editMode = Mode.Display;

        public AddressControl()
        {
            deferPopulateFields = controlCreated = false;
        }

        public ShippingAddress_V01 ShippingAddr { get; set; }
        public List<string> ErrorList { set; get; }

        public void LoadPage()
        {
        }

        public bool IsOkToSave()
        {
            return false;
        }

        public object CreateAddressFromControl(string typeName)
        {
            object dataContext = DataContext;
            if (dataContext != null)
            {
                if (dataContext.GetType().ToString().Contains(typeName))
                {
                    return dataContext;
                }
                else
                {
                    Type objectType = dataContext.GetType();
                    PropertyInfo pInfo = objectType.GetProperty(typeName);
                    if (pInfo != null)
                    {
                        return pInfo.GetValue(dataContext, null);
                    }
                }
            }
            return null;
        }

        public object CreateAddressFromControl()
        {
            object dataContext = DataContext;
            if (dataContext != null && AddressDefinition != null)
            {
                ControlCollection coll =
                    AddressDefinition.AddressGrid.Controls;
                foreach (Control r in coll)
                {
                    if (r.Editable)
                    {
                        dataContext = r.SetValue(dataContext);
                    }
                    r.PopulateValueField();
                }
            }
            return dataContext;
        }

        public void displayValidationMessages(List<string> errors)
        {
        }

        public bool Validate()
        {
            if (AddressDefinition != null && controlCreated)
            {
                List<string> errors = null;
                ControlCollection coll = AddressDefinition.AddressGrid.Controls;
                Array.ForEach(coll.ToArray(), a => a.Validate(ref errors));
                
                //TODO: this actually does nothing!
                displayValidationMessages(errors);
                
                if (errors.Count > 0)
                {
                    ErrorList = errors;
                    return false;
                }
                return !(errors != null && errors.Count > 0);
            }
            return true;
        }

        #region "Properties"

        [Bindable(true)]
        [DefaultValue("")]
        [Description("Set the definition of the window")]
        public AddressWindow AddressDefinition { get; set; }

        [Browsable(false), PersistenceMode(PersistenceMode.InnerProperty)]
        public virtual ITemplate ItemTemplate { get; set; }

        [Category("Appearance"), Description("Title text")]
        public string Text
        {
            get
            {
                if (ViewState["Text"] == null)
                {
                    return ID;
                }
                return (string) ViewState["Text"];
            }
            set
            {
                ViewState["Text"] = value;
                Text = value;
            }
        }

        private bool deferPopulateFields { get; set; }
        private bool controlCreated { get; set; }

        [Bindable(true)]
        [DefaultValue("")]
        [Description("Set xml file path")]
        public string XMLFile
        {
            get { return string.Empty; }
            set { AddressDefinition = XMLHelper.LoadAddressFormatFromFile(value); }
        }

        [Bindable(true)]
        [DefaultValue("")]
        [Description("Set DataContext of address")]
        public object DataContext
        {
            get
            {
                if (ViewState["DataContext"] == null)
                {
                    return _dataContext;
                }
                return ViewState["DataContext"];
            }
            set
            {
                _dataContext = value;
                ViewState["DataContext"] = value;
                Populate();
            }
        }

        public bool IsEditable()
        {
            return _editMode == Mode.Edit;
        }

        #endregion

        private HtmlGenericControl createContainerTable()
        {
            HtmlGenericControl containerTable = null;
            //Create the HtmlTable that will hold the content area of the control
            containerTable = new HtmlGenericControl("div");
            //containerTable.CellPadding = 0;
            //containerTable.CellSpacing = 0;
            //containerTable.Border = 0;
            Controls.Add(containerTable);
            return containerTable;
        }

        private void createControls()
        {
            if (AddressDefinition != null)
            {
                HtmlGenericControl row = null;
                ControlCollection coll = AddressDefinition.AddressGrid.Controls;
                coll.SortControls();

                // Reading the resx to create the control
                var reader = new AddressResxReader();
                AddressResx addressResx = reader.GetAddressResx(Thread.CurrentThread.CurrentCulture.Name);
                var rowNumber = -1;
                foreach (int r in coll.Select(s => s.Row).Distinct())
                {
                    if (rowNumber != r)
                    {
                        var colNum = coll.Where(c => c.Row == r).Select(a => a.Column).Distinct().ToList();
                        _containerTable.Controls.Add(row = new HtmlGenericControl("div"));
                        rowNumber = r + 1;
                        var colNumRow = coll.Where(c => c.Row == rowNumber).Select(a => a.Column).Distinct().ToList();
                        if (colNumRow.Count > colNum.Count)
                            colNum = colNumRow;

                        var childNumber = 1;
                        foreach (int column in colNum)
                        {
                            if (colNum.Count() > 1)
                            {

                                var rowChild = new HtmlGenericControl("div");
                                row.Attributes.Add("class", "row-group " + "cols-" + colNum.Count().ToString());
                                rowChild.Attributes.Add("class", "inline");

                                if (childNumber == colNum.Count())
                                {
                                    rowChild.Attributes.Add("class", "inline last-child");
                                }
                                var lstControls = coll.Where(p => (p.Row == r || p.Row == rowNumber) && p.Column == column).Select(a => a).ToList();

                                rowChild.Attributes.Add("name", lstControls[0].Name.Contains("Spacer") ? "" : lstControls[0].Name.ToString());
                                foreach (Control c in lstControls)
                                {
                                    c.CreateUIControl(rowChild, DataContext, addressResx);
                                }
                                row.Controls.Add(rowChild);
                                childNumber++;
                            }
                            else
                            {
                                var lstControls = coll.Where(p => (p.Row == r || p.Row == rowNumber) && p.Column == column).Select(a => a).ToList();
                                row.Attributes.Add("name", lstControls[0].Name.Contains("Spacer") ? "" : lstControls[0].Name.ToString());
                                foreach (Control c in lstControls)
                                {
                                    c.CreateUIControl(row, DataContext, addressResx);
                                }
                            }
                        }
                    }
                    else
                    {
                        rowNumber = -1;
                    }
                }
                if (coll.Exists(p => p.Editable == true))
                {
                    _editMode = Mode.Edit;
                }
                AddressDefinition.AddressGrid.ResovleBinding(DataContext);
                controlCreated = true;

            }
        }

        public void Populate()
        {
            if (AddressDefinition != null && controlCreated)
            {
                ControlCollection coll = AddressDefinition.AddressGrid.Controls;
                Array.ForEach(coll.ToArray(), a => a.ResolveBinding(coll, DataContext));
            }
            else
            {
                deferPopulateFields = true;
            }
        }

        protected override void CreateChildControls()
        {
            _containerTable = createContainerTable();
            createControls();
        }

        //This method is present just to be able to force correct rendering during design-time
        internal void GetDesignTimeHtml()
        {
            EnsureChildControls();
        }

        
    }
}