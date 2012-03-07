using System;
using System.Collections.Generic;
using System.Reflection;

namespace Afterthought.UnitTest.Target
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class CustomAmendAttribute : Attribute, IAmendmentAttribute
    {
        IEnumerable<ITypeAmendment> IAmendmentAttribute.GetAmendments(Type target)
        {
            if (target.GetCustomAttributes(typeof(ExplicitImplementationAttribute), true).Length > 0)
            {
                ConstructorInfo constructorInfo = typeof(ExplicitInterfaceAmendment<>).MakeGenericType(target).GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                    yield return (ITypeAmendment)constructorInfo.Invoke(new object[0]);
            }
            if (target.GetCustomAttributes(typeof(ImplicitImplementationAttribute), true).Length > 0)
            {
                ConstructorInfo constructorInfo = typeof(ImplicitInterfaceAmendment<>).MakeGenericType(target).GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                    yield return (ITypeAmendment)constructorInfo.Invoke(new object[0]);
            }
        }
    }
}
