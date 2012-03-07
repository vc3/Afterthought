using System;

namespace Afterthought.UnitTest.Target
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ImplicitImplementationAttribute : Attribute { }
}