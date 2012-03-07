using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Afterthought.UnitTest
{
    public static class TypeExtensions
    {
        //Explicitly implemented interface methods in C# are private in the target class.
        public static IEnumerable<MethodInfo> GetExplicitlyImplementedMethods(this Type targetType, Type interfaceType)
        {
            return targetType.GetInterfaceMap(interfaceType).TargetMethods.Where(m => m.IsPrivate);
        }

        public static MethodInfo GetExplicitlyImplementedMethod(this Type targetType, Type interfaceType, string methodName)
        {
            return GetExplicitlyImplementedMethods(targetType, interfaceType).FirstOrDefault(m => m.Name == methodName);
        }

        public static IEnumerable<PropertyInfo> GetExplicitlyImplementedProperties(this Type targetType, Type interfaceType)
        {
            IEnumerable<MethodInfo> targetMethods = GetExplicitlyImplementedMethods(targetType, interfaceType);

            // avoid multiple enumeration.
            IEnumerable<MethodInfo> tm = targetMethods.ToList();

            IEnumerable<PropertyInfo> explicitProps =
                targetType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(prop => tm.Contains(prop.GetGetMethod(true)) ||
                                   tm.Contains(prop.GetSetMethod(true))
                    );

            return explicitProps;
        }

        public static PropertyInfo GetExplicitlyImplementedProperty(this Type targetType, Type interfaceType, string propertyName)
        {
            return GetExplicitlyImplementedProperties(targetType, interfaceType).FirstOrDefault(p => p.Name == propertyName);
        }
    }
}