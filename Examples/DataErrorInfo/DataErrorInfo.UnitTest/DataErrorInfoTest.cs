using System.ComponentModel;
using DataErrorInfo.UnitTest.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataErrorInfo.UnitTest
{
    [TestClass]
    public class DataErrorInfoTest
    {
        [TestMethod]
        public void TestErrorMessage()
        {
            var wine = new Wine();

            var errorInfo = wine as IDataErrorInfo;
            Assert.IsNotNull(errorInfo);

            Assert.AreEqual(Wine.ShouldHaveACru, errorInfo["Cru"]);
            Assert.IsNull(errorInfo["Millesime"]);
        }
    }
}
