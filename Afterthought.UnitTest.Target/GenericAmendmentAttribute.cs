using System;
using System.Collections.Generic;

namespace Afterthought.UnitTest.Target
{
    public class GenericAmendmentAttribute: AmendmentAttribute
    {
        private readonly Type amendmentType;

        public GenericAmendmentAttribute(Type amendmentType): base(amendmentType)
        {
            this.amendmentType = amendmentType;
        }

        public override IEnumerable<ITypeAmendment> GetAmendments(Type target)
        {
            yield return (ITypeAmendment) amendmentType.GetConstructor(new[] { typeof (Type) }).Invoke(new object[] { target });
        }
    }
}