using System;

namespace Afterthought.UnitTest.Target
{
    public class GenericTestAmendment: Amendment<IHaveExecutableMethod>
    {
        public GenericTestAmendment(Type amendingType): base(amendingType)
        {
            Methods
                .Named("GetResult")
                .After((instance, method, parameters) => instance.MethodExecuted = true);
        }
    }
}