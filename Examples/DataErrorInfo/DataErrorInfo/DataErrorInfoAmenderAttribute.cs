using System;
using System.Collections.Generic;
using Afterthought;

namespace DataErrorInfo
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DataErrorInfoAmenderAttribute : Attribute, IAmendmentAttribute
    {
        public IEnumerable<ITypeAmendment> GetAmendments(Type target)
        {
            if (target.GetCustomAttributes(typeof(DataErrorInfoAttribute), true).Length > 0 && target.GetInterface("System.ComponentModel.IDataErrorInfo") == null)
                yield return (ITypeAmendment)typeof(DataErrorInfoAmendment<>).MakeGenericType(target).GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        }
    }
}
