using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

//using HL.Common.ValueObjects;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.GlobalAddress
{
    /// <summary>
    ///     Collection of UI controls
    /// </summary>
    [Serializable]
    public class ControlCollection : List<Control>, IXmlSerializable, IEnumerable
    {
        private readonly object[] supportedTypes =
            {
                "Label", typeof (Label), "TextBox", typeof (TextBox), "ComboBox",
                typeof (ComboBox)
            };

        public void WriteXml(XmlWriter writer)
        {
            foreach (Control ctrl in this)
            {
                ctrl.WriteXml(writer);
            }
        }

        private Type getType(string name)
        {
            return
                supportedTypes.Where((n, index) => index%2 != 0 && n.ToString().Contains(name)).Select(x => x).First()
                as Type;
        }

        private bool isTypeSupported(string typeName)
        {
            return
                supportedTypes.Where((n, index) => index%2 == 0 && n.ToString().Contains(typeName))
                              .Select(x => x)
                              .Count() > 0;
        }

        private void createChildControl(XmlReader reader)
        {
            if (!string.IsNullOrEmpty(reader.Name) && isTypeSupported(reader.Name))
            {
                var types = new Type[1];
                types[0] = typeof (XmlReader);

                Type objectType = getType(reader.Name);
                ConstructorInfo constructorInfoObj =
                    objectType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null,
                                              CallingConventions.HasThis, types, null);

                var parameters = new object[1];
                parameters[0] = reader;
                Add(constructorInfoObj.Invoke(parameters) as Control);
            }
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            while (!reader.EOF)
            {
                createChildControl(reader);
                reader.Read();
            }
        }

        public XmlSchema GetSchema()
        {
            return (null);
        }

        public void SortControls()
        {
            List<Control> sorted = this.AsQueryable().OrderBy(s => s.Row).ThenBy(s => s.Column).ToList();
            Clear();
            AddRange(sorted);
        }
    }

    /// <summary>
    ///     Base class for all controls
    /// </summary>
    [Serializable]
    //[XmlInclude(typeof(Label))] -- this attribute does not work
    [XmlRoot(ElementName = "Control")]
    public class Control
    {
        public Control()
        {
        }

        public Control(string name)
        {
            Name = name;
        }

        public Control(string name, int row, int column)
        {
            Name = name;
            Row = row;
            Column = column;
        }

        public Control(XmlReader reader)
        {
        }

        public virtual void CreateAttributes(XmlReader reader)
        {
            string attr;
            Name = reader.GetAttribute("Name");
            Row = string.IsNullOrEmpty(attr = reader.GetAttribute("Grid.Row")) ? 0 : int.Parse(attr);
            Column = string.IsNullOrEmpty(attr = reader.GetAttribute("Grid.Column")) ? 0 : int.Parse(attr);
            Colspan = string.IsNullOrEmpty(attr = reader.GetAttribute("Grid.Colspan")) ? 1 : int.Parse(attr);
            Style = reader.GetAttribute("cssStyle");
            BindingText = reader.GetAttribute("Text");
            Width = reader.GetAttribute("Width");
        }

        public virtual System.Web.UI.Control CreateUIControl(HtmlGenericControl parent, object dataContext,
                                                             AddressResx addressResx)
        {
            return null;
        }

        public virtual void PopulateValueField()
        {
        }

        [DataMember]
        [XmlAttribute(AttributeName = "Width")]
        public string Width { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Height")]
        public string Height { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Style")]
        public string Style { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "BindingText")]
        public string BindingText { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Grid.Row")]
        public int Row { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Grid.Column")]
        public int Column { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Grid.Colspan")]
        public int Colspan { get; set; }

        [DataMember]
        [XmlIgnore]
        public virtual bool Bindable
        {
            get { return false; }
        }

        [DataMember]
        [XmlIgnore]
        public virtual bool Editable
        {
            get { return false; }
        }

        [DataMember]
        [XmlIgnore]
        public System.Web.UI.Control uiControl { get; set; }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", string.IsNullOrEmpty(Name) ? string.Empty : Name);
            writer.WriteAttributeString("Grid.Row", Row.ToString());
            writer.WriteAttributeString("Grid.Column", Column.ToString());
            writer.WriteAttributeString("BindingText", BindingText);
        }

        public virtual void ReadXml(XmlReader reader)
        {
            string attr;
            Name = reader.GetAttribute("Name");
            Row = string.IsNullOrEmpty(attr = reader.GetAttribute("Grid.Row")) ? 0 : int.Parse(attr);
            Column = string.IsNullOrEmpty(attr = reader.GetAttribute("Grid.Column")) ? 0 : int.Parse(attr);
            Style = reader.GetAttribute("cssStyle");
            BindingText = reader.GetAttribute("Text");
            Width = reader.GetAttribute("Width");
            Height = reader.GetAttribute("Height");
        }

        public virtual string ResolveBinding(ControlCollection coll, object DataContext)
        {
            return string.Empty;
        }

        public virtual object GetValue()
        {
            return null;
        }

        public virtual object SetValue(object dataContext)
        {
            return null;
        }

        public virtual object SetValue(object dataContext, string bindingText, string value)
        {
            return null;
        }

        public virtual bool Validate(ref List<string> errors)
        {
            return true;
        }

        public virtual string ValidationError()
        {
            return string.Empty;
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "Label")]
    public class Label : Control
    {
        [DataMember]
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }

        [DataMember]
        [XmlIgnore]
        public override bool Bindable
        {
            get { return true; }
        }

        public Label()
        {
        }

        public Label(string name)
            : base(name)
        {
        }

        public Label(string name, int row, int column)
            : base(name, row, column)
        {
        }

        public Label(string name, int row, int column, string text)
            : base(name, row, column)
        {
            Text = text;
        }

        public Label(XmlReader reader)
        {
            CreateAttributes(reader);
            base.CreateAttributes(reader);
        }

        public override void CreateAttributes(XmlReader reader)
        {
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Label");
            base.WriteXml(writer);
            writer.WriteAttributeString("Text", string.IsNullOrEmpty(Text) ? string.Empty : Text);
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
        }

        public override string ResolveBinding(ControlCollection coll, object dataContext)
        {
            string text = null;
            if (dataContext != null)
            {
                string[] bindFields = Grid.ResolveBinding(BindingText);

                if (bindFields != null)
                {
                    // popupate data -- reflection
                    Type objectType = dataContext.GetType();
                    object value = dataContext;
                    for (int i = 0; i < bindFields.Length; i++)
                    {
                        PropertyInfo pInfo = objectType.GetProperty(bindFields[i]);
                        value = pInfo.GetValue(value, null);
                        if (value == null)
                        {
                            continue;
                        }
                        objectType = value.GetType();
                        text = value == null ? string.Empty : value as string;
                    }
                }
                else
                {
                    text = BindingText;
                }
                if (uiControl != null && string.IsNullOrEmpty(((HtmlGenericControl)uiControl).InnerText))
                {
                    if ( string.IsNullOrWhiteSpace(text) )
                        uiControl.Visible = false;
                    else
                        ((HtmlGenericControl)uiControl).InnerText = text;
                }
                return text;
            }
            else
            {
                return string.Empty;
            }
        }

        public override System.Web.UI.Control CreateUIControl(HtmlGenericControl parent, object dataContext,
                                                              AddressResx addressResx)
        {
            var label = new HtmlGenericControl("label");
            label.Attributes.Add("Id", Name);
            //label.ID = Name;
            //label.CssClass = Style;
            
            if (Name.Contains("Format"))
            {
                label.Attributes.Add("class", string.Format("{0} {1}", Style, "gdo-form-format"));
            }
            else
            {
                label.Attributes.Add("class", Style);
            }
            //label.Text = ResolveBinding(dataContext);
            var localizedText = addressResx.KeyValue.data.Where(d => d.name == Name);
            if (localizedText.Count() > 0)
            {
                label.InnerText = string.IsNullOrEmpty(localizedText.First().value)
                                 ? label.InnerText
                                 : localizedText.First().value;
            }
            parent.Controls.Add(label);
            return uiControl = label;
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "TextBox")]
    public class TextBox : Control
    {
        public TextBox()
        {
        }

        public TextBox(string name, int row, int column)
            : base(name, row, column)
        {
        }

        public TextBox(XmlReader reader)
            : base(reader)
        {
            CreateAttributes(reader);
            base.CreateAttributes(reader);
        }

        [DataMember]
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "RegExp")]
        public string RegExp { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValErrorMsg")]
        public string ValErrorMsg { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValReqMsg")]
        public string ValReqMsg { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Required")]
        public string Required { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "KeyPressHandler")]
        public string KeyPressHandler { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "KeyPressAndBlurHandler")]
        public string KeyPressAndBlurHandler { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ChangeHandler")]
        public string ChangeHandler { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Dependent")]
        public string Dependent { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "MaximumLength")]
        public string MaximumLength { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "IndexForTab")]
        public string IndexForTab { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Exceptions")]
        public string Exceptions { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValExceptionZipMsg")]
        public string ValExceptionZipMsg { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ReadOnly")]
        public string ReadOnly { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "HasFocus")]
        public string HasFocus { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "MaxBytes")]
        public string MaxBytes { get; set; }

        [DataMember]
        [XmlIgnore]
        public string DependentOf { get; set; }

        [DataMember]
        [XmlIgnore]
        public Control DependentCntrl { get; set; }

        [DataMember]
        [XmlIgnore]
        private object dataContext { get; set; }

        [DataMember]
        [XmlIgnore]
        public override bool Bindable
        {
            get { return true; }
        }

        [DataMember]
        [XmlIgnore]
        public override bool Editable
        {
            get { return true; }
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("TextBox");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }

        public override string ResolveBinding(ControlCollection coll, object dataContext)
        {
            string text = null;
            if (dataContext != null)
            {
                // it has dependent
                if (!string.IsNullOrEmpty(Dependent))
                {
                    var ctrls = coll.Where(p => p.Name == Dependent).Select(s => s);
                    if (ctrls.Count() > 0)
                    {
                        // must be combobox
                        var textBox = ctrls.First() as TextBox;
                        if (textBox != null)
                        {
                            DependentOf = textBox.Name;
                            DependentCntrl = textBox;
                        }
                    }
                }

                string[] bindFields = Grid.ResolveBinding(BindingText);
                if (bindFields != null)
                {
                    // popupate data -- reflection
                    Type objectType = dataContext.GetType();
                    object value = dataContext;
                    for (int i = 0; i < bindFields.Length; i++)
                    {
                        PropertyInfo pInfo = objectType.GetProperty(bindFields[i]);
                        value = pInfo.GetValue(value, null);
                        if (value != null)
                        {
                            objectType = value.GetType();
                        }
                        text = value == null ? string.Empty : value as string;
                    }
                }
                else
                {
                    text = BindingText;
                }
                if (uiControl != null)
                {
                    ((System.Web.UI.WebControls.TextBox) uiControl).Text = text;
                }
                return text;
            }

            else
            {
                return string.Empty;
            }
        }

        public override object SetValue(object dataContext)
        {
            if (dataContext != null)
            {
                string text = uiControl != null ? ((System.Web.UI.WebControls.TextBox) uiControl).Text : BindingText;
                PropertyInfo pInfo;
                object obj;
                Grid.GetPropertyInfo(out pInfo, out obj, dataContext, BindingText);
                if (pInfo != null)
                {
                    pInfo.SetValue(obj, text, null);
                }
            }
            return dataContext;
        }

        public override void CreateAttributes(XmlReader reader)
        {
            base.CreateAttributes(reader);
            RegExp = reader.GetAttribute("RegExp");
            ValErrorMsg = reader.GetAttribute("ValErrorMsg");
            ValReqMsg = reader.GetAttribute("ValReqMsg");
            Required = reader.GetAttribute("Required");
            KeyPressHandler = reader.GetAttribute("KeyPressHandler");
            KeyPressAndBlurHandler = reader.GetAttribute("KeyPressAndBlurHandler");
            ChangeHandler = reader.GetAttribute("ChangeHandler");
            Dependent = reader.GetAttribute("Dependent");
            MaximumLength = reader.GetAttribute("MaximumLength");
            IndexForTab = reader.GetAttribute("IndexForTab");
            ReadOnly = reader.GetAttribute("ReadOnly");
            Exceptions = reader.GetAttribute("Exceptions");
            ValExceptionZipMsg = reader.GetAttribute("ValExceptionZipMsg");
            HasFocus = reader.GetAttribute("HasFocus");
            MaxBytes = reader.GetAttribute("MaxBytes");
        }

        public override System.Web.UI.Control CreateUIControl(HtmlGenericControl parent, object dataContext,
                                                              AddressResx addressResx)
        {
            this.dataContext = dataContext;
            var tb = new System.Web.UI.WebControls.TextBox();
            tb.ID = Name;
            tb.CssClass = Style;
            if (!string.IsNullOrEmpty(MaximumLength))
            {
                tb.MaxLength = int.Parse(MaximumLength);
            }
            tb.TabIndex = short.Parse(IndexForTab);
            //tb.Text = ResolveBinding(dataContext);
            tb.EnableViewState = true;
            parent.Controls.Add(tb);
            if (!string.IsNullOrEmpty(ChangeHandler))
            {
                tb.Attributes.Add("onChange", ChangeHandler);
            }

            if (!string.IsNullOrEmpty(KeyPressHandler))
            {
                tb.Attributes.Add("onkeypress", KeyPressHandler);
            }

            if (!string.IsNullOrEmpty(KeyPressAndBlurHandler))
            {
                tb.Attributes.Add("onkeypress", KeyPressAndBlurHandler);
                tb.Attributes.Add("onblur", KeyPressAndBlurHandler);
            }

            if (!string.IsNullOrEmpty(ReadOnly))
            {
                tb.ReadOnly = ReadOnly.ToLower().Equals("true");
            }

            if (!string.IsNullOrEmpty(HasFocus) && HasFocus.ToLower().Equals("true"))
            {
                tb.Focus();
            }
            return uiControl = tb;
        }

        public bool checkField(string stringToValidate, ref List<string> errors)
        {
            bool result = true;

            //disallow user to enter only white spaces.
            if (stringToValidate.Trim().Equals(string.Empty))
                stringToValidate = stringToValidate.Trim();

            if (stringToValidate != string.Empty)
            {
                // if there is regular expression
                if (!string.IsNullOrEmpty(RegExp))
                {
                    var regex = new Regex(RegExp);
                    // not match
                    if (!regex.Match(stringToValidate).Success)
                    {
                        // error msg specified
                        if (!string.IsNullOrEmpty(ValErrorMsg))
                        {
                            string resx = PlatformResources.GetGlobalResourceString("ErrorMessage", ValErrorMsg);
                            if (!string.IsNullOrEmpty(resx))
                            {
                                errors.Add(resx);
                            }
                            else
                            {
                                errors.Add(ValErrorMsg);
                            }
                        }
                        else
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError"));
                        }
                        result = false;
                    }
                    
                }
                // if there is max in bytes for field
                if (!string.IsNullOrEmpty(MaxBytes))
                {
                    Encoding enc_utf8 = new UTF8Encoding(false, true);
                    if (enc_utf8.GetByteCount(stringToValidate) > 60)
                       {
                        // error msg specified
                        if (!string.IsNullOrEmpty(ValErrorMsg))
                        {
                            string resx = PlatformResources.GetGlobalResourceString("ErrorMessage", ValErrorMsg);
                            if (!string.IsNullOrEmpty(resx))
                            {
                                errors.Add(resx);
                            }
                            else
                            {
                                errors.Add(ValErrorMsg);
                            }
                        }
                        else
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError"));
                        }
                        result = false;
                    }

                }

                if (!String.IsNullOrEmpty(Exceptions))
                {
                    // not match
                    if (Exceptions.Contains(stringToValidate))
                    {
                        // error msg specified
                        if (!string.IsNullOrEmpty(ValExceptionZipMsg))
                        {
                            string resx = PlatformResources.GetGlobalResourceString("ErrorMessage", ValExceptionZipMsg);
                            if (!string.IsNullOrEmpty(resx))
                            {
                                errors.Add(resx);
                            }
                            else
                            {
                                errors.Add(ValErrorMsg);
                            }
                        }
                        else
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError"));
                        }
                        result = false;
                    }
                }
            }
            else
            {
                // is required
                if (!string.IsNullOrEmpty(Required))
                {
                    string resx;
                    // error msg specified
                    if (!string.IsNullOrEmpty(ValReqMsg))
                    {
                        // ValReqMsg is a key to resx
                        resx = PlatformResources.GetGlobalResourceString("ErrorMessage", ValReqMsg);
                        if (!string.IsNullOrEmpty(resx))
                        {
                            errors.Add(resx);
                        }
                            // if not, use it as it is
                        else
                        {
                            errors.Add(ValReqMsg);
                        }
                    }
                    else
                    {
                        resx = PlatformResources.GetGlobalResourceString("ErrorMessage", "RequiredField");
                        if (!string.IsNullOrEmpty(resx))
                        {
                            if (!errors.Exists(p => p.Equals(resx)))
                            {
                                errors.Add(resx);
                            }
                        }
                        else
                        {
                            string error = "Requried Field";
                            if (!errors.Exists(p => p.Equals(error)))
                            {
                                errors.Add(error);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public override bool Validate(ref List<string> errors)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }
            base.Validate(ref errors);
            if (uiControl != null)
            {
                return checkField(((System.Web.UI.WebControls.TextBox) uiControl).Text, ref errors);
            }
            else
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "SysError"));
            }
            return false;
        }

        public override object GetValue()
        {
            if (dataContext != null)
            {
                return null;
            }
            return null;
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "ComboBox")]
    public class ComboBox : Control
    {
        //string[] attributes = { "DisplayMemberPath", "ItemsSource", "XPath" };

        [DataMember]
        [XmlAttribute(AttributeName = "Text")]
        public string Text { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "RegExp")]
        public string RegExp { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValErrorMsg")]
        public string ValErrorMsg { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValReqMsg")]
        public string ValReqMsg { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "DisplayMemberPath")]
        public string DisplayMemberPath { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "DisplayIDPath")]
        public string DisplayIDPath { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ItemsSource")]
        public string ItemsSource { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "XPath")]
        public string XPath { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Dependent")]
        public string Dependent { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "ValueField")]
        public string ValueField { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "IndexForTab")]
        public string IndexForTab { get; set; }

        [DataMember]
        [XmlIgnore]
        public string DependentOf { get; set; }

        [DataMember]
        [XmlIgnore]
        public Control DependentCntrl { get; set; }

        public ComboBox()
        {
        }

        public ComboBox(string name, int row, int column)
            : base(name, row, column)
        {
        }

        [DataMember]
        [XmlIgnore]
        public override bool Bindable
        {
            get { return true; }
        }

        [DataMember]
        [XmlIgnore]
        public override bool Editable
        {
            get { return true; }
        }

        [DataMember]
        [XmlIgnore]
        private object dataContext { get; set; }


        public ComboBox(XmlReader reader)
            : base(reader)
        {
            CreateAttributes(reader);
            base.CreateAttributes(reader);
        }

        public override void CreateAttributes(XmlReader reader)
        {
            base.CreateAttributes(reader);
            RegExp = reader.GetAttribute("RegExp");
            ValErrorMsg = reader.GetAttribute("ValErrorMsg");
            ValReqMsg = reader.GetAttribute("ValReqMsg");
            DisplayMemberPath = reader.GetAttribute("DisplayMemberPath");
            DisplayIDPath = reader.GetAttribute("DisplayIDPath");
            ValueField = reader.GetAttribute("ValueField");
            ItemsSource = reader.GetAttribute("ItemsSource");
            XPath = reader.GetAttribute("XPath");
            Dependent = reader.GetAttribute("Dependent");
            IndexForTab = reader.GetAttribute("IndexForTab");
           // MaxBytes = reader.GetAttribute("MaxBytes");
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ComboBox");
            base.WriteXml(writer);
            writer.WriteAttributeString("DisplayMemberPath",
                                        string.IsNullOrEmpty(DisplayMemberPath) ? string.Empty : DisplayMemberPath);
            writer.WriteEndElement();
        }

        private bool isXML(string str)
        {
            return str.ToLower().EndsWith(".xml");
        }

        public override object SetValue(object dataContext)
        {
            var ddl = uiControl as DropDownList;
            if (ddl.SelectedItem != null)
            {
                PropertyInfo pInfo;
                object obj;
                Grid.GetPropertyInfo(out pInfo, out obj, dataContext, BindingText);
                if (pInfo != null)
                {
                    pInfo.SetValue(obj, ddl.SelectedItem.Text, null);
                }
            }
            return dataContext;
        }

        public override object SetValue(object dataContext, string bindingText, string value)
        {
            if (bindingText != null)
            {
                PropertyInfo pInfo;
                object obj;
                Grid.GetPropertyInfo(out pInfo, out obj, dataContext, bindingText);
                if (pInfo != null)
                {
                    pInfo.SetValue(obj, value, null);
                }
            }

            return dataContext;
        }

        private string getStringValue(object dataContext)
        {
            var ddl = uiControl as DropDownList;
            if (ddl != null && dataContext != null)
            {
                PropertyInfo pInfo;
                object obj;
                Grid.GetPropertyInfo(out pInfo, out obj, dataContext, BindingText);
                if (pInfo != null && obj != null)
                {
                    object value;
                    value = pInfo.GetValue(obj, null);
                    return value as string;
                }
            }
            return string.Empty;
        }

        public override void PopulateValueField()
        {
            if (!string.IsNullOrEmpty(ValueField))
            {
                var ddl = uiControl as DropDownList;
                string strValue = ddl.SelectedValue;
                if (!string.IsNullOrEmpty(strValue))
                {
                    SetValue(dataContext, ValueField, strValue);
                }
            }
        }

        public override string ResolveBinding(ControlCollection coll, object DataContext)
        {
            try
            {
                ListItem selectedItem;
                var ddl = uiControl as DropDownList;
                ddl.DataBind();
                setSelected(DataContext, ddl);

                //Customozation to set the ddlCity drop down list
                GetCityValue(DataContext);

                // it has dependent
                if (!string.IsNullOrEmpty(Dependent))
                {
                    var ctrls = coll.Where(p => p.Name == Dependent).Select(s => s);
                    if (ctrls.Count() > 0)
                    {
                        // must be combobox
                        var cb = ctrls.First() as ComboBox;
                        if (cb != null)
                        {
                            DependentOf = cb.Name;
                            DependentCntrl = cb;
                            if ((selectedItem = ddl.SelectedItem) == null)
                            {
                                string strValue = getStringValue(DataContext);
                                selectedItem = string.IsNullOrEmpty(strValue)
                                                   ? ddl.Items[0]
                                                   : ddl.Items.FindByText(strValue);
                            }
                            var depddl = DependentCntrl.uiControl as DropDownList;
                            (depddl.DataSource as XmlDataSource).XPath = string.Format(cb.XPath, selectedItem.Value);
                            depddl.DataBind();
                            if (!string.IsNullOrEmpty(GetCity.CityValue))
                            {
                                //depddl.SelectedItem.Text = GetCity.CityValue;
                                ListItem item = depddl.Items.FindByText(GetCity.CityValue);
                                if (item != null)
                                {
                                    item.Selected = true;
                                }
                                else
                                {
                                    item = depddl.Items.FindByValue(GetCity.CityValue);
                                    if (item != null)
                                        item.Selected = true;
                                }
                            }
                            else
                            {
                                setSelected(DataContext, ddl);
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return null;
        }

        private void GetCityValue(object DataContext)
        {
            // The dependent city value can only be get one time
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID)
            {
                if (BindingText == "{Binding Address.City}")
                {
                    PropertyInfo pInfo;
                    object obj;
                    Grid.GetPropertyInfo(out pInfo, out obj, dataContext, BindingText);
                    object value = null;
                    if (pInfo != null && obj != null)
                    {
                        value = pInfo.GetValue(obj, null);
                    }
                    GetCity.CityValue = value as string;
                }
            }
        }

        private void setSelected(object DataContext, DropDownList ddl)
        {
            string strValue = getStringValue(DataContext);
            if (!string.IsNullOrEmpty(strValue))
            {
                ListItem item = ddl.Items.FindByText(strValue);
                if (item != null)
                {
                    item.Selected = true;
                }
                else
                {
                    item = ddl.Items.FindByValue(strValue);
                    if (item != null)
                        item.Selected = true;
                }
            }
        }

        public void XMLFileBinding(DropDownList ddl, HtmlGenericControl parent)
        {
            if (ItemsSource != string.Empty)
            {
                if (isXML(ItemsSource))
                {
                    var ds = new XmlDataSource();
                    ds.DataFile = ItemsSource;
                    ds.XPath = XPath;
                    parent.Controls.Add(ds);
                    ddl.DataSource = ds;
                    //dl.DataBind();
                }
            }
        }

        /// <summary>
        ///     when selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl.SelectedValue != null)
            {
                if (!string.IsNullOrEmpty(Dependent))
                {
                    ((DependentCntrl.uiControl as DropDownList).DataSource as XmlDataSource).XPath =
                        string.Format((DependentCntrl as ComboBox).XPath, ddl.SelectedValue);
                    DependentCntrl.uiControl.DataBind();
                    var cbbDependent = DependentCntrl as ComboBox;
                    if (!string.IsNullOrEmpty(cbbDependent.Dependent))
                    {
                        var ddld = DependentCntrl.uiControl as DropDownList;
                        ((cbbDependent.DependentCntrl.uiControl as DropDownList).DataSource as XmlDataSource).XPath =
                        string.Format((cbbDependent.DependentCntrl as ComboBox).XPath, ddld.SelectedValue);
                        cbbDependent.DependentCntrl.uiControl.DataBind();
                    
                    }

                }
                if (!string.IsNullOrEmpty(ValueField))
                {
                    SetValue(dataContext, ValueField, ddl.SelectedValue);
                }
                ddl.Focus();
            }
        }

        public override System.Web.UI.Control CreateUIControl(HtmlGenericControl parent, object dataContext,
                                                              AddressResx addressResx)
        {
            var ddl = new DropDownList();
            ddl.ID = Name;
            ddl.DataTextField = DisplayMemberPath;
            ddl.DataValueField = DisplayIDPath;
            ddl.TabIndex = short.Parse(IndexForTab);
            ddl.SelectedIndexChanged += OnSelectedIndexChanged;
            //ddl.DataBound += new EventHandler(OnDataBound);
            ddl.AutoPostBack = true;
            XMLFileBinding(ddl, parent);
            parent.Controls.Add(ddl);
            this.dataContext = dataContext;
            return uiControl = ddl;
        }

        public override bool Validate(ref List<string> errors)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }
            base.Validate(ref errors);
            if (uiControl != null)
            {
                return checkField(((DropDownList) uiControl).SelectedItem.Value, ref errors);
            }
            else
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "SysError"));
            }
            return false;
        }

        public bool checkField(string stringToValidate, ref List<string> errors)
        {
            bool result = true;
            if (stringToValidate != string.Empty)
            {
                // if there is regular expression
                if (!string.IsNullOrEmpty(RegExp))
                {
                    var regex = new Regex(RegExp);
                    // not match
                    if (!regex.Match(stringToValidate).Success)
                    {
                        // error msg specified
                        if (!string.IsNullOrEmpty(ValErrorMsg))
                        {
                            string resx = PlatformResources.GetGlobalResourceString("ErrorMessage", ValErrorMsg);
                            if (!string.IsNullOrEmpty(resx))
                            {
                                errors.Add(resx);
                            }
                            else
                            {
                                errors.Add(ValErrorMsg);
                            }
                        }
                        else
                        {
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError"));
                        }
                        result = false;
                    }
                }
            }
            return result;
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "ColumnDefinition")]
    public class ColumnDefinition
    {
        [DataMember]
        [XmlAttribute(AttributeName = "Width")]
        public string Width { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Height")]
        public string Height { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "RowDefinition")]
    public class RowDefinition
    {
        [DataMember]
        [XmlAttribute(AttributeName = "Width")]
        public string Width { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "Height")]
        public string Height { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "Grid")]
    public class Grid : Control
    {
        /// <summary>
        ///     RowDefinitions
        /// </summary>
        [DataMember]
        [XmlArray("RowDefinitions", ElementName = "Grid.RowDefinitions", Form = XmlSchemaForm.None)]
        [XmlArrayItem("RowDefinition", typeof (RowDefinition), Form = XmlSchemaForm.None)]
        public RowDefinition[] RowDefinitions { get; set; }

        /// <summary>
        ///     ColumnDefinitions
        /// </summary>
        [DataMember]
        [XmlArray("ColumnDefinitions", ElementName = "Grid.ColumnDefinitions", Form = XmlSchemaForm.None)]
        [XmlArrayItem("ColumnDefinition", typeof (ColumnDefinition), Form = XmlSchemaForm.None)]
        public ColumnDefinition[] ColumnDefinitions { get; set; }

        /// <summary>
        ///     Collection of controls
        /// </summary>
        [DataMember]
        public ControlCollection Controls { get; set; }

        public void ResovleBinding(object dataContext)
        {
            foreach (Control c in Controls)
            {
                c.ResolveBinding(Controls, dataContext);
            }
        }

        public void BindResx(AddressResx addressResx)
        {
            foreach (Control c in Controls)
            {
                if ((c as Label) != null)
                {
                    var resxVar = addressResx.KeyValue.data.Where(d => d.name == c.Name);
                    if (resxVar.Count() > 0)
                    {
                        ((System.Web.UI.WebControls.Label) c.uiControl).Text = resxVar.First().value;
                    }
                }
            }
        }

        public static string[] ResolveBinding(string element)
        {
            if (element != null)
            {
                //Regex regex = new Regex("^{Binding *[a-zA-Z0-9]}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var regex = new Regex("^{Binding .*}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (regex.Match(element).Success)
                {
                    element = element.Replace("{Binding ", "");
                    element = element.Replace("}", "");
                    return element.Trim().Split('.');
                }
            }
            return null;
        }

        public static void GetPropertyInfo(out PropertyInfo pInfo, out object obj, object dataContext,
                                           string bindingText)
        {
            pInfo = null;
            obj = null;

            if (bindingText != null)
            {
                Type objectType = dataContext.GetType();
                obj = dataContext;

                string[] bindFields = ResolveBinding(bindingText);
                for (int i = 0; i < bindFields.Length; i++)
                {
                    pInfo = objectType.GetProperty(bindFields[i]);
                    if ((i + 1) != bindFields.Length)
                    {
                        obj = pInfo.GetValue(obj, null);
                        objectType = obj.GetType();
                    }
                }
            }
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "AddressWindow", Namespace = "http://herbalife.com/GlobaleAddress")]
    public class AddressWindow
    {
        [DataMember]
        [XmlElement(ElementName = "Grid")]
        public Grid AddressGrid { get; set; }
    }

    public static class GetCity
    {
        public static string CityValue { get; set; }
    }
}