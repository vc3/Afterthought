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

            Fields
                .Named("Value")
                .AddAttribute<ObsoleteAttribute, string>("Don't use value directly");

            Properties
                .Named("ValueProperty")
                .AfterSet((instance, name, value, o, newValue) => instance.MethodExecuted = true);
        }
    }
}