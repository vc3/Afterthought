using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterthought.UnitTest.Target
{
    public class TestNestedAmendment<T> : Amendment<T,T>
        where T : Nested.Example
    {
        public TestNestedAmendment()
        {
            Methods
                .Named("Add")
                .WithParams<int, int>()
                .Implement((T instance, int a, int b) =>
                               {
                                   instance.Result = a + b;
                                   return a + b;
                               });
        }
    }
}
