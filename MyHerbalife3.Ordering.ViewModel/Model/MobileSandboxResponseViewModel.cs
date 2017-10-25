using System;
using System.Runtime.Serialization;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    [DataContract(Name="ActivityLog")]
    public class MobileSandboxResponseViewModel
    {
        [DataMember(Name ="activityTypeId")]
        public int ActivityTypeID { get; set; }

        [DataMember(Name="activityLogId")]
        public int ActivityLogID { get; set; }

        [DataMember]
        public int PlatformTypeID { get; set; }

        [DataMember]
        public int EventTypeID { get; set; }

        [DataMember]
        public string ActivityTypeDesc { get; set; }

        [DataMember(Name ="Success")]
        public byte Success { get; set; }

        [DataMember(Name="OrderNumber")]
        public string OrderNumber { get; set; }

        [DataMember(Name = "AppName")]
        public string AppName { get; set; }

        [DataMember(Name = "RawUrl")]
        public string RawUrl { get; set; }

        [DataMember(Name = "OrderRequestXml")]
        public string OrderRequestXml { get; set; }

        [DataMember(Name = "DistributorId")]
        public string DistributorId { get; set; }

        [DataMember(Name = "OrderResponseXml")]
        public string OrderResponseXml { get; set; }

        [DataMember(Name = "RawHeaders")]
        public string RawHeaders { get; set; }

        [DataMember(Name = "RawResponse")]
        public string RawResponse { get; set; }

        [DataMember(Name = "UserAgent")]
        public string UserAgent { get; set; }

        [DataMember(Name = "Locale")]
        public string Locale { get; set; }
        
        
        [DataMember(Name = "CreatedDate")]
        public DateTime CreatedDate { get; set; }

        
        [DataMember(Name = "OrderRequest")]
        public string OrderRequest { get; set; }
    }
}
