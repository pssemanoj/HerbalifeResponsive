using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace MyHerbalife3.Ordering.SharedProviders
{
    public static class ObjectProvider
    {
        public static void CopyProperties(object source, object destination)
        {
            var count = 1;
            GetPropertiesRecursive(source, destination, ref count);
        }

        private static void GetPropertiesRecursive(object source, object destination, ref int count)
        {
            var destinationType = destination.GetType();
            Type sourceType = null;
            if (source != null)
            {
                sourceType = source.GetType();
            }

            var destinationProperties = destinationType.GetProperties();
            foreach (var property in destinationProperties)
            {
                var propertyType = property.PropertyType;
                var sourceValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;

                PropertyInfo propertyInSource = null;
                var sourceHasDestinationProperty = false;

                if (source != null)
                {
                    propertyInSource = sourceType.GetProperty(property.Name);

                    if (propertyInSource != null)
                    {
                        sourceHasDestinationProperty = true;
                        sourceValue = propertyInSource.GetValue(source, null);
                    }
                }

                var isComplex = !propertyType.ToString().StartsWith("System");
                if (isComplex & !propertyType.IsArray)
                {
                    var ci = propertyType.GetConstructor(Type.EmptyTypes);
                    var newDestination = ci.Invoke(null);
                    property.SetValue(destination, newDestination, null);
                    GetPropertiesRecursive(sourceValue, newDestination, ref count);
                    continue;
                }

                var s = count + ". " + property.Name + (propertyType.IsArray ? "[]" : "") + " = ";

                if (!sourceHasDestinationProperty)
                {
                    s += "[default(" + propertyType + ")] = ";
                }

                if (propertyType.IsArray & propertyInSource != null)
                    sourceValue = DeepCopyArray(propertyInSource.PropertyType, propertyType, sourceValue, source, destination);

                property.SetValue(destination, sourceValue, null);
                var destinationValue = property.GetValue(destination, null);
            }
        }

        private static object DeepCopyArray(Type sourceType, Type destinationType, object sourceValue, object sourceParent, object destinationParent)
        {
            if (sourceValue == null || sourceType == null || sourceParent == null || destinationParent == null)
                return null;

            using (var stream = new MemoryStream(2 * 1024))
            {
                var serializer = new DataContractSerializer(sourceType);
                serializer.WriteObject(stream, sourceValue);
                serializer = new DataContractSerializer(destinationType);
                stream.Position = 0;
                return serializer.ReadObject(stream);
            }
        }
    }
}
