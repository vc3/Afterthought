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
        public void AddAfter()
        {
            Assert.IsFalse(Calculator.MethodExecuted);
            Calculator.GetResult();
            Assert.IsTrue(Calculator.MethodExecuted);
        }
    }
}