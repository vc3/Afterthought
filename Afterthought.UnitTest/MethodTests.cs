//-----------------------------------------------------------------------------
//
// Copyright (c) VC3, Inc. All rights reserved.
// This code is licensed under the Microsoft Public License.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//-----------------------------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Afterthought.UnitTest.Target;

namespace Afterthought.UnitTest
{
	[TestClass]
	public class MethodTests
	{
		Calculator Calculator { get; set; }

		[TestInitialize]
		public void InitializeCalculator()
		{
			Calculator = new Calculator();
		}

		/// <summary>
		/// Tests adding a new public method to a type.
		/// </summary>
		[TestMethod]
		public void AddMethod()
		{
			var expected = 16 - 5;

			Assert.AreEqual(expected, ((IMath)Calculator).Subtract(16, 5));
		}

		/// <summary>
		/// Tests modifying an existing method to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeMethodWithoutChangesGeneric()
		{
			// Verify that the Multiply method also copies the output to the Result property
			Assert.AreEqual(Calculator.Multiply(3, 4), Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing method to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeMethodWithoutChangesArray()
		{
			// Verify that the Multiply method also copies the output to the Result property
			Assert.AreEqual(Calculator.Multiply2(3, 4), Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing method to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeMethodWithChangesGeneric()
		{
			var expected = 18;

			// Verify that the second parameter is always converted to 1
			Assert.AreEqual(expected, Calculator.Divide(expected, 0));
		}

		/// <summary>
		/// Tests modifying an existing method to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeMethodWithChangesArray()
		{
			var expected = 18;

			// Verify that the second parameter is always converted to 1
			Assert.AreEqual(expected, Calculator.Divide2(expected, 0));
		}

		/// <summary>
		/// Tests modifying an existing method to run code after the original method
		/// implementation that does not return a value.
		/// </summary>
		[TestMethod]
		public void AfterMethodActionGeneric()
		{
			var expected = 18;
			var inputs = new int[] { 9 };
			Calculator.Double(inputs);
			Assert.AreEqual(expected, inputs[0]);
		}

		/// <summary>
		/// Tests modifying an existing method to run code after the original method
		/// implementation that does not return a value.
		/// </summary>
		[TestMethod]
		public void AfterMethodActionArray()
		{
			var expected = 18;
			var inputs = new int[] { 9 };
			Calculator.Double2(inputs);
			Assert.AreEqual(expected, inputs[0]);
		}

		/// <summary>
		/// Tests modifying an existing method to run code after the original method
		/// implementation that returns a value.
		/// </summary>
		[TestMethod]
		public void AfterMethodFuncGeneric()
		{
			var expected = 15L;
			Assert.AreEqual(expected, Calculator.Sum(new int[] { 1, 2, 3, 4, 5 }));
		}

		/// <summary>
		/// Tests modifying an existing method to run code after the original method
		/// implementation that returns a value.
		/// </summary>
		[TestMethod]
		public void AfterMethodFuncArray()
		{
			var expected = 15L;
			Assert.AreEqual(expected, Calculator.Sum2(new int[] { 1, 2, 3, 4, 5 }));
		}

		/// <summary>
		/// Tests modifying an existing method to run code after the original method
		/// implementation that returns a value.
		/// </summary>
		[TestMethod]
		public void AfterMethodFuncAsActionArray()
		{
			var expected = 15;
			var inputs = new int[] { 1, 2, 3, 4, 5 };
			Calculator.Sum3(inputs);
			Assert.AreEqual(expected, inputs[4]);
		}

		/// <summary>
		/// Tests modifying an existing method to completely replace the implementation.
		/// </summary>
		[TestMethod]
		public void ImplementMethod()
		{
			var expected = 4 * 4;

			// Verify that the implementation has been corrected by the amendment
			Assert.AreEqual(expected, Calculator.Square(4));
		}

		/// <summary>
		/// Tests modifying an existing method to completely replace the implementation.
		/// </summary>
		[TestMethod]
		public void BeforeAndAfterWithContext()
		{
			Calculator.Result = 0;

			// Perform a slow calculation
			Calculator.SlowSum(new int[] { 1, 2, 3, 4, 5 });

			// Verify that the execution time was measured and saved to result
			Assert.IsTrue(Calculator.Result >= 100);
		}

		[TestMethod]
		public void TestSomeOtherClass()
		{
			new SomeOtherClass().ResponseToSomeEvent();
		}
	}

	public class SomeOtherClass
	{
		public void ResponseToSomeEvent()
		{
			var proxy = new Proxy();

			// Assign the lambda to a local variable
			EventHandler doSomething = (sender, e) => Console.WriteLine("Just Once");

			proxy.SomeEvent += doSomething;

			proxy.Raise();

			proxy.SomeEvent -= doSomething;
			proxy.SomeEvent += doSomething;

			proxy.Raise();
		}
	}

	public class Proxy
	{
		public event EventHandler SomeEvent;

		public void Raise()
		{
			Console.WriteLine("Raise");
			SomeEvent(this, EventArgs.Empty);
		}
	}
}
