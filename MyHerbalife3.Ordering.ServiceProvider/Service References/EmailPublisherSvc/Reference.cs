﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox", ConfigurationName="EmailPublisherSvc.Inbox_PortType")]
    public interface Inbox_PortType {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://herbalife.com/EmailPublisher/Service/Inbox/SubmitPredefined", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse1 SubmitPredefined(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest1 request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://herbalife.com/EmailPublisher/Service/Inbox/SubmitPredefined", ReplyAction="*")]
        System.Threading.Tasks.Task<MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse1> SubmitPredefinedAsync(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest1 request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://herbalife.com/EmailPublisher/Service/Inbox/SubmitCustom", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse1 SubmitCustom(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest1 request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://herbalife.com/EmailPublisher/Service/Inbox/SubmitCustom", ReplyAction="*")]
        System.Threading.Tasks.Task<MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse1> SubmitCustomAsync(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest1 request);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox")]
    public partial class SubmitPredefinedRequest : object, System.ComponentModel.INotifyPropertyChanged {
        
        private FormData formDataField;
        
        private string versionField;
        
        private string requestIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher", Order=0)]
        public FormData FormData {
            get {
                return this.formDataField;
            }
            set {
                this.formDataField = value;
                this.RaisePropertyChanged("FormData");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string RequestId {
            get {
                return this.requestIdField;
            }
            set {
                this.requestIdField = value;
                this.RaisePropertyChanged("RequestId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher")]
    public partial class FormData : object, System.ComponentModel.INotifyPropertyChanged {
        
        private System.Xml.XmlElement anyField;
        
        private string versionField;
        
        private string formIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
        public System.Xml.XmlElement Any {
            get {
                return this.anyField;
            }
            set {
                this.anyField = value;
                this.RaisePropertyChanged("Any");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FormId {
            get {
                return this.formIdField;
            }
            set {
                this.formIdField = value;
                this.RaisePropertyChanged("FormId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox")]
    public partial class SubmitPredefinedResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string trackingIdField;
        
        private string versionField;
        
        private string requestIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher", Order=0)]
        public string TrackingId {
            get {
                return this.trackingIdField;
            }
            set {
                this.trackingIdField = value;
                this.RaisePropertyChanged("TrackingId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string RequestId {
            get {
                return this.requestIdField;
            }
            set {
                this.requestIdField = value;
                this.RaisePropertyChanged("RequestId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SubmitPredefinedRequest1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox", Order=0)]
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest SubmitPredefinedRequest;
        
        public SubmitPredefinedRequest1() {
        }
        
        public SubmitPredefinedRequest1(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest SubmitPredefinedRequest) {
            this.SubmitPredefinedRequest = SubmitPredefinedRequest;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SubmitPredefinedResponse1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox", Order=0)]
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse SubmitPredefinedResponse;
        
        public SubmitPredefinedResponse1() {
        }
        
        public SubmitPredefinedResponse1(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse SubmitPredefinedResponse) {
            this.SubmitPredefinedResponse = SubmitPredefinedResponse;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox")]
    public partial class SubmitCustomRequest : object, System.ComponentModel.INotifyPropertyChanged {
        
        private Content contentField;
        
        private System.DateTime scheduledTimeField;
        
        private bool scheduledTimeFieldSpecified;
        
        private string versionField;
        
        private string requestIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher", Order=0)]
        public Content Content {
            get {
                return this.contentField;
            }
            set {
                this.contentField = value;
                this.RaisePropertyChanged("Content");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public System.DateTime ScheduledTime {
            get {
                return this.scheduledTimeField;
            }
            set {
                this.scheduledTimeField = value;
                this.RaisePropertyChanged("ScheduledTime");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ScheduledTimeSpecified {
            get {
                return this.scheduledTimeFieldSpecified;
            }
            set {
                this.scheduledTimeFieldSpecified = value;
                this.RaisePropertyChanged("ScheduledTimeSpecified");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string RequestId {
            get {
                return this.requestIdField;
            }
            set {
                this.requestIdField = value;
                this.RaisePropertyChanged("RequestId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher")]
    public partial class Content : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string fromAddressField;
        
        private string replyToAddressField;
        
        private string senderAddressField;
        
        private string toAddressField;
        
        private string subjectField;
        
        private ContentBody bodyField;
        
        private string versionField;
        
        private string languageCodeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string FromAddress {
            get {
                return this.fromAddressField;
            }
            set {
                this.fromAddressField = value;
                this.RaisePropertyChanged("FromAddress");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string ReplyToAddress {
            get {
                return this.replyToAddressField;
            }
            set {
                this.replyToAddressField = value;
                this.RaisePropertyChanged("ReplyToAddress");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string SenderAddress {
            get {
                return this.senderAddressField;
            }
            set {
                this.senderAddressField = value;
                this.RaisePropertyChanged("SenderAddress");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string ToAddress {
            get {
                return this.toAddressField;
            }
            set {
                this.toAddressField = value;
                this.RaisePropertyChanged("ToAddress");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public string Subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
                this.RaisePropertyChanged("Subject");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public ContentBody Body {
            get {
                return this.bodyField;
            }
            set {
                this.bodyField = value;
                this.RaisePropertyChanged("Body");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LanguageCode {
            get {
                return this.languageCodeField;
            }
            set {
                this.languageCodeField = value;
                this.RaisePropertyChanged("LanguageCode");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher")]
    public partial class ContentBody : object, System.ComponentModel.INotifyPropertyChanged {
        
        private System.Xml.XmlNode[] anyField;
        
        private bool isHtmlField;
        
        private bool isHtmlFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        [System.Xml.Serialization.XmlAnyElementAttribute(Order=0)]
        public System.Xml.XmlNode[] Any {
            get {
                return this.anyField;
            }
            set {
                this.anyField = value;
                this.RaisePropertyChanged("Any");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IsHtml {
            get {
                return this.isHtmlField;
            }
            set {
                this.isHtmlField = value;
                this.RaisePropertyChanged("IsHtml");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsHtmlSpecified {
            get {
                return this.isHtmlFieldSpecified;
            }
            set {
                this.isHtmlFieldSpecified = value;
                this.RaisePropertyChanged("IsHtmlSpecified");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1067.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox")]
    public partial class SubmitCustomResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string trackingIdField;
        
        private string versionField;
        
        private string requestIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/EmailPublisher", Order=0)]
        public string TrackingId {
            get {
                return this.trackingIdField;
            }
            set {
                this.trackingIdField = value;
                this.RaisePropertyChanged("TrackingId");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
                this.RaisePropertyChanged("Version");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string RequestId {
            get {
                return this.requestIdField;
            }
            set {
                this.requestIdField = value;
                this.RaisePropertyChanged("RequestId");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SubmitCustomRequest1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox", Order=0)]
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest SubmitCustomRequest;
        
        public SubmitCustomRequest1() {
        }
        
        public SubmitCustomRequest1(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest SubmitCustomRequest) {
            this.SubmitCustomRequest = SubmitCustomRequest;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class SubmitCustomResponse1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://schemas.herbalife.com/EmailPublisher/V03/Service/Inbox", Order=0)]
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse SubmitCustomResponse;
        
        public SubmitCustomResponse1() {
        }
        
        public SubmitCustomResponse1(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse SubmitCustomResponse) {
            this.SubmitCustomResponse = SubmitCustomResponse;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface Inbox_PortTypeChannel : MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.Inbox_PortType, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Inbox_PortTypeClient : System.ServiceModel.ClientBase<MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.Inbox_PortType>, MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.Inbox_PortType {
        
        public Inbox_PortTypeClient() {
        }
        
        public Inbox_PortTypeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Inbox_PortTypeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Inbox_PortTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Inbox_PortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse1 SubmitPredefined(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest1 request) {
            return base.Channel.SubmitPredefined(request);
        }
        
        public System.Threading.Tasks.Task<MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedResponse1> SubmitPredefinedAsync(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitPredefinedRequest1 request) {
            return base.Channel.SubmitPredefinedAsync(request);
        }
        
        public MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse1 SubmitCustom(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest1 request) {
            return base.Channel.SubmitCustom(request);
        }
        
        public System.Threading.Tasks.Task<MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomResponse1> SubmitCustomAsync(MyHerbalife3.Ordering.ServiceProvider.EmailPublisherSvc.SubmitCustomRequest1 request) {
            return base.Channel.SubmitCustomAsync(request);
        }
    }
}
