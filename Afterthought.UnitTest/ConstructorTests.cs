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
	public class ConstructorTests
	{
		Calculator Calculator { get; set; }

		[TestInitialize]
		public void InitializeCalculator()
		{
			Calculator = new Calculator();
		}

		/// <summary>
		/// Tests adding a new public constructor to a type.
		/// </summary>
		[TestMethod]
		public void AddConstructor()
		{
			//Add constructor not implemented
			//int expectedValue = 5;
			//Assert.AreNotEqual(expectedValue, Calculator.Result);

			//Calculator = new Calculator(expectedValue);
			//Assert.AreEqual(expectedValue, Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeConstructorWithoutChangesGeneric()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeConstructorWithoutChangesArray()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeConstructorWithChangesGeneric()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original method
		/// implementation without affecting the values of the specified parameters.
		/// </summary>
		[TestMethod]
		public void BeforeConstructorWithChangesArray()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code after the original method
		/// implementation that does not return a value.
		/// </summary>
		[TestMethod]
		public void AfterConstructorGeneric()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code after the original method
		/// implementation that does not return a value.
		/// </summary>
		[TestMethod]
		public void AfterConstructorArray()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to completely replace the implementation.
		/// </summary>
		[TestMethod]
		public void ImplementConstructor()
		{
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original
		/// implementation when conditional logic exists. 
		/// </summary>
		[TestMethod]
		public void Before_IfStatementFalse()
		{
			int expectedResult = int.MaxValue;
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreNotEqual(expectedResult, Calculator.Result);

			Calculator = new Calculator(false);
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreEqual(expectedResult, Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code before the original
		/// implementation when conditional logic exists. 
		/// </summary>
		[TestMethod]
		public void Before_IfStatementTrue()
		{
			const int expectedResult = int.MaxValue;
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreNotEqual(expectedResult, Calculator.Result);

			Calculator = new Calculator(true);
			Assert.AreEqual(1, Calculator.ConstructorSetInt);
			Assert.AreEqual(expectedResult, Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code after the original
		/// implementation when conditional logic exists. 
		/// </summary>
		[TestMethod]
		public void After_IfStatementFalse()
		{
			const int expectedResult = int.MaxValue;
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreNotEqual(expectedResult, Calculator.Result);

			Calculator = new Calculator(false, 12);
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreEqual(expectedResult, Calculator.Result);
		}

		/// <summary>
		/// Tests modifying an existing constructor to run code after the original
		/// implementation when conditional logic exists. 
		/// </summary>
		[TestMethod]
		public void After_IfStatementTrue()
		{
			const int expectedResult = int.MaxValue;
			Assert.AreEqual(0, Calculator.ConstructorSetInt);
			Assert.AreNotEqual(expectedResult, Calculator.Result);

			const int expectedConstructorSetInt = 12;
			Calculator = new Calculator(true, expectedConstructorSetInt);
			Assert.AreEqual(expectedConstructorSetInt, Calculator.ConstructorSetInt);
			Assert.AreEqual(expectedResult, Calculator.Result);
		}
	}
}
