using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExceptionHandler.UnitTest.Target;

namespace ExceptionHandler.UnitTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void CatchBlockForVoidMethod()
		{
			var instance = new TestExceptionHandler();
			
			try
			{
				instance.ThrowExceptionVoidMethod();
			}
			catch (Exception ex)
			{
				Assert.Fail("Exception thrown");
			}			
		}

		[TestMethod]
		public void CatchBlockForRetMethod()
		{
			var instance = new TestExceptionHandler();

			try
			{
				int i=instance.ThrowExceptionRetMethod();
			}
			catch (Exception ex)
			{
				Assert.Fail("Exception thrown");
			}
		}
	}
}
