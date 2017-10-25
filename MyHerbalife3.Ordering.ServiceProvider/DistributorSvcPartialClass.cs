using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ServiceProvider.DistributorSvc
{
    //public partial class TaxIdentification
    //{
    //    private static readonly Type[] knownTypes = new Type[]
    //    {
    //        typeof (TaxIdentification),
    //        typeof (TaxIdentification_V01),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentification),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentification_V01)
    //    };

    //    public static DistributorTinList ToDistributorSvc(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorTinList tinList)
    //    {
    //        if (tinList == null) return null;

    //        StringWriter objectString = new StringWriter();
    //        XmlTextWriter xmlTextWriter = new XmlTextWriter(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(DistributorTinList), knownTypes);
    //        serializer.WriteObject(xmlTextWriter, tinList);
    //        DistributorTinList distributorTinList = null;
    //        StringReader objectStringReader = new StringReader(objectString.ToString());
    //        XmlTextReader xmlReader = new XmlTextReader(objectStringReader);
    //        serializer = new DataContractSerializer(typeof(DistributorTinList), knownTypes);
    //        distributorTinList = serializer.ReadObject(xmlReader) as DistributorTinList;
    //        return distributorTinList;
    //    }
    //}

    //public partial class DistributorVolumes
    //{
    //    private static readonly Type[] knownTypes = new Type[]
    //    {
    //        typeof (DistributorVolume),
    //        typeof (DistributorVolume_V01),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume_V01)
    //    };

    //    public static List<DistributorVolume_V01> ToDistributorSvc(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume_V01[] distributorVolumes)
    //    {
    //        if (distributorVolumes == null) return null;

    //        StringWriter objectString = new StringWriter();
    //        XmlTextWriter xmlTextWriter = new XmlTextWriter(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(List<DistributorVolume_V01>), knownTypes);
    //        serializer.WriteObject(xmlTextWriter, distributorVolumes.ToList());
    //        List<DistributorVolume_V01> distributorVolumeList = null;
    //        StringReader objectStringReader = new StringReader(objectString.ToString());
    //        XmlTextReader xmlReader = new XmlTextReader(objectStringReader);
    //        serializer = new DataContractSerializer(typeof(List<DistributorVolume_V01>), knownTypes);
    //        distributorVolumeList = serializer.ReadObject(xmlReader) as List<DistributorVolume_V01>;
    //        return distributorVolumeList;
    //    }
    //}

    //public partial class Address
    //{
    //    private static readonly Type[] knownTypes = new Type[]
    //    {
    //        typeof (AddressesCollection),
    //        typeof (Address),
    //        typeof (Address_V01),
    //        typeof (Address_V02),
    //        typeof (Address_V03),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.AddressesCollection),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V01),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V03)
    //    };

    //    public static HL.Common.ValueObjects.AddressCollection ToCommonVO(MyHerbalife3.Core.DistributorProvider.DistributorSvc.AddressesCollection addressCollection)
    //    {
    //        if (addressCollection == null) return null;

    //        StringWriter objectString = new StringWriter();
    //        XmlTextWriter xmlTextWriter = new XmlTextWriter(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(AddressesCollection), knownTypes);
    //        serializer.WriteObject(xmlTextWriter, addressCollection);
    //        HL.Common.ValueObjects.AddressCollection dsAddressCollection = null;
    //        StringReader objectStringReader = new StringReader(objectString.ToString());
    //        XmlTextReader xmlReader = new XmlTextReader(objectStringReader);
    //        serializer = new DataContractSerializer(typeof(AddressesCollection), knownTypes);
    //        dsAddressCollection = serializer.ReadObject(xmlReader) as HL.Common.ValueObjects.AddressCollection;
    //        return dsAddressCollection;
    //    }
    //}

    //public partial class PhoneNumber
    //{
    //    private static readonly Type[] knownTypes = new Type[]
    //    {
    //        typeof (PhoneNumberBase),
    //        typeof (PhoneNumber_V03),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumberBase),
    //        typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumber_V03)
    //    };

    //    public static List<PhoneNumber_V03> ToDistributorSvc(MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumber_V03[] phoneNumbers)
    //    {
    //        if (phoneNumbers == null) return null;

    //        StringWriter objectString = new StringWriter();
    //        XmlTextWriter xmlTextWriter = new XmlTextWriter(objectString);
    //        DataContractSerializer serializer = new DataContractSerializer(typeof(List<PhoneNumber_V03>), knownTypes);
    //        serializer.WriteObject(xmlTextWriter, phoneNumbers.ToList());
    //        List<PhoneNumber_V03> phoneNumbersList = null;
    //        StringReader objectStringReader = new StringReader(objectString.ToString());
    //        XmlTextReader xmlReader = new XmlTextReader(objectStringReader);
    //        serializer = new DataContractSerializer(typeof(List<PhoneNumber_V03>), knownTypes);
    //        phoneNumbersList = serializer.ReadObject(xmlReader) as List<PhoneNumber_V03>;
    //        return phoneNumbersList;
    //    }
    //}

}
