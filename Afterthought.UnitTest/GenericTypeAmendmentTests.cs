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

        [TestMethod]
        public void AddAfterSetToProperty()
        {
            Assert.IsFalse(Calculator.MethodExecuted);
            Calculator.ValueProperty = 3;
            Assert.IsTrue(Calculator.MethodExecuted);
        }

        [TestMethod]
        public void AddBeforeToMethodWithParameter()
        {
            Assert.IsFalse(Calculator.MethodExecuted);
            Calculator.SetResult(12);
            Assert.IsTrue(Calculator.MethodExecuted);
        }
    }
}