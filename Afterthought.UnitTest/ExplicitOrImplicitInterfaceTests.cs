using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Afterthought.UnitTest.Target;

namespace Afterthought.UnitTest
{
    [TestClass]
    public class ExplicitOrImplicitInterfaceTests
    {

        private Bar _bar;

        [TestInitialize]
        public void InitializeCalculator()
        {
            _bar = new Bar();
        }

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

        /// <summary>
        /// Tests that Explicit properties can be setted.
        /// </summary>
        [TestMethod]
        public void CanSetExplicitProperties()
        {
            Assert.IsInstanceOfType(_bar, typeof(IExplicitInterface));

            ((IExplicitInterface)_bar).ExplicitProperty = -1;

            // Assert that the value is now -1
            Assert.AreEqual(-1, ((IExplicitInterface)_bar).ExplicitProperty);
        }

        /// <summary>
        /// Tests that Implicit properties can be setted.
        /// </summary>
        [TestMethod]
        public void CanSetImplicitProperties()
        {
            Assert.IsInstanceOfType(_bar, typeof(IImplicitInterface));

            ((IImplicitInterface)_bar).ImplicitProperty = -2;

            // Assert that the value is now -2
            Assert.AreEqual(-2, ((IImplicitInterface)_bar).ImplicitProperty);
        }
    }
}
