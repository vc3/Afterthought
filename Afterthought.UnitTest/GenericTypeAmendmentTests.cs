using System;
using Afterthought.UnitTest.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Afterthought.UnitTest
{
    [TestClass]
    public class GenericTypeAmendmentTests
    {
        GenericCalculator<int> Calculator { get; set; }

        [TestInitialize]
        public void InitializeCalculator()
        {
            Calculator = new GenericCalculator<int>();
        }

        [TestMethod]
        public void AddAfterToMethod()
        {
            Assert.IsFalse(Calculator.MethodExecuted);
            Calculator.GetResult();
            Assert.IsTrue(Calculator.MethodExecuted);
        }

        [TestMethod]
        public void AddAttributeToField()
        {
            var attribute = Calculator.GetType().GetField("Value")
                .GetCustomAttributes(typeof(ObsoleteAttribute), true);
            Assert.IsNotNull(attribute);
        }
    }
}