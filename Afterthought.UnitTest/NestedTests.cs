using System;
using Afterthought.UnitTest.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Afterthought.UnitTest
{
    [TestClass]
    public class NestedTests
    {
        /// <summary>
        /// Tests the nested class's add method.
        /// </summary>
        [TestMethod]
        public void NestedAddMethod()
        {
            var a = new Nested.Example();
            var result = a.Add(4, 7);

            Assert.AreEqual(11, result);
            Assert.AreEqual(11, a.Result);
        }
    }
}
