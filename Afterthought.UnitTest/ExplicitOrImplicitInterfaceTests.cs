using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Afterthought.UnitTest.Target;

namespace Afterthought.UnitTest
{
    [TestClass]
    public class ExplicitOrImplicitInterfaceTests
    {

        [TestInitialize]
        public void InitializeCalculator()
        { }

        /// <summary>
        /// Tests implicitly implemented interface's property.
        /// </summary>
        [TestMethod]
        public void ImplicitPropertyImplementation()
        {
            // Assert that ImplicitProperty property exists.
            Assert.IsNotNull(typeof(Bar).GetProperty("ImplicitProperty"));
            // Assert that ImplicitProperty property is implemented implicitly.
            Assert.IsNull(typeof(Bar).GetExplicitlyImplementedProperty(typeof(IImplicitInterface), "ImplicitProperty"));
        }

        /// <summary>
        /// Tests explicitly implemented interface's property.
        /// </summary>
        [TestMethod]
        public void ExplicitPropertyImplementation()
        {
            PropertyInfo explicitPropertyInfo = typeof(IExplicitInterface).GetProperties().FirstOrDefault();
            Assert.IsNotNull(explicitPropertyInfo);

            // Assert that ExplicitProperty property is implemented explicitly.
            Assert.IsNotNull(typeof (Bar).GetExplicitlyImplementedProperty(typeof (IExplicitInterface),
                                                                           string.Format("{0}.{1}", typeof (IExplicitInterface).FullName, explicitPropertyInfo.Name)));
        }
    }
}
