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
	/// <summary>
	/// Tests all supported modes of amending properties for a type.
	/// </summary>
	[TestClass]
	public class PropertyTests
	{
		Calculator Calculator { get; set; }

		[TestInitialize]
		public void InitializeCalculator()
		{
			Calculator = new Calculator();
		}

		/// <summary>
		/// Tests the addition of an automatic property: public t p { get; set; }
		/// </summary>
		[TestMethod]
		public void AddAuto()
		{
			// Set auto property Count to 5
			Calculator.Count = 5;

			// Assert that the value of Count is now 5
			Assert.AreEqual(5, Calculator.Count);
		}


		/// <summary>
		/// Tests adding a property that is lazily initialized.
		/// </summary>
		[TestMethod]
		public void AddAutoWithInitializer()
		{
			int expected = 12;

			Assert.AreEqual(expected, Calculator.InitiallyTwelve);
		}

		/// <summary>
		/// Tests adding a property with a getter implementation.
		/// </summary>
		[TestMethod]
		public void AddGetter()
		{
			// Assert that the calculated property Three equals 3
			Assert.AreEqual(3, Calculator.Three);
		}

		/// <summary>
		/// Tests adding a property with a setter implementation.
		/// </summary>
		[TestMethod]
		public void AddSetter()
		{
			// Set the result via the SetResult setter-only property
			Calculator.SetResult = 9;

			// Assert that the Result property now equals 9
			Assert.AreEqual(9, Calculator.Result);
		}

		/// <summary>
		/// Tests adding a property that is lazily initialized.
		/// </summary>
		[TestMethod]
		public void AddLazy()
		{
			string expected = Calculator.LazyRandomName;

			Assert.IsNotNull(expected);
			Assert.AreEqual(expected, Calculator.LazyRandomName);
		}

		/// <summary>
		/// Tests replacing the getter and setter for an existing property.
		/// </summary>
		[TestMethod]
		public void ReplaceGetterAndSetter()
		{
			int expected = 5;

			// Set Random5 equal to 5
			Calculator.Random5 = expected;

			// Verify that the value is still 5 instead of a randomly generated number
			Assert.AreEqual<int>(expected, Calculator.Random5);
		}

		/// <summary>
		/// Tests initializing an existing property.
		/// </summary>
		[TestMethod]
		public void Initialize()
		{
			int expected = 13;

			// Verify that InitiallyThirteen was successfully initialized to 13
			Assert.AreEqual(expected, Calculator.InitiallyThirteen);
		}

		/// <summary>
		/// Tests lazily initializing and existing property.
		/// </summary>
		[TestMethod]
		public void LazyInitialize()
		{
			string expected = Calculator.ExistingLazyRandomName;

			Assert.IsNotNull(expected);
			Assert.AreEqual(expected, Calculator.ExistingLazyRandomName);
		}

		/// <summary>
		/// Tests amending code before an existing property getter.
		/// </summary>
		[TestMethod]
		public void BeforeGet()
		{
			// Get the expected value of Random1
			int expected = Calculator.GetRandom(1);

			// Assert that the expected value is not 1
			Assert.AreNotEqual(expected, 1);

			// Set Random1 equal to 1
			Calculator.Random1 = 1;

			// Assert that the property returns the expected value, not 1
			Assert.AreEqual(expected, Calculator.Random1);
		}

		/// <summary>
		/// Tests amending code after an existing property getter.
		/// </summary>
		[TestMethod]
		public void AfterGet()
		{
			// Get the expected value of Random1
			int expected = Calculator.GetRandom(2);

			// Assert that the expected value is not 1
			Assert.AreNotEqual(expected, 2);

			// Set Random2 equal to 2
			Calculator.Random2 = 2;

			// Assert that the property returns the expected value, not 1
			Assert.AreEqual(expected, Calculator.Random2);

		}

		/// <summary>
		/// Tests amending code before an existing property setter and
		/// without affecting or being effected by conditional logic.
		/// </summary>
		[TestMethod]
		public void BeforeSet_SetMatchesInitalValue()
		{
			int initialValue = Calculator.Random6;
			
			Assert.AreEqual(initialValue, 1);
			Assert.AreNotEqual(int.MaxValue, Calculator.Result);
			
			Calculator.Random6 = initialValue;

			Assert.AreEqual(int.MaxValue, Calculator.Result);
			Assert.AreNotEqual(initialValue, Calculator.Random6);
		}

		/// <summary>
		/// Tests amending code before an existing property setter and
		/// without affecting or being effected by conditional logic.
		/// </summary>
		[TestMethod]
		public void BeforeSet_SetDoesNotMatchInitalValue()
		{
			int defaultValue = Calculator.Random6;

			Assert.AreEqual(defaultValue, 1);
			Assert.AreNotEqual(int.MaxValue, Calculator.Result);

			int expectedValue;
			Calculator.Random6 = expectedValue = 2;

			Assert.AreEqual(int.MaxValue, Calculator.Result);
			Assert.AreEqual(expectedValue, Calculator.Random6);
		}

		/// <summary>
		/// Tests amending code after an existing property setter and
		/// without affecting or being effected by conditional logic.
		/// </summary>
		[TestMethod]
		public void AfterSet_SetMatchesInitalValue()
		{
			int defaultValue = Calculator.Random7;
			Assert.AreEqual(defaultValue, 1);

			Assert.AreNotEqual(int.MaxValue, Calculator.Result);
			Calculator.Random7 = defaultValue;

			Assert.AreEqual(int.MaxValue, Calculator.Result);
			Assert.AreNotEqual(defaultValue, Calculator.Random7);
		}

		/// <summary>
		/// Tests amending code after an existing property setter and
		/// without affecting or being effected by conditional logic.
		/// </summary>
		[TestMethod]
		public void AfterSet_SetDoesNotMatchInitalValue()
		{
			int defaultValue = Calculator.Random7;

			Assert.AreEqual(defaultValue, 1);
			Assert.AreNotEqual(int.MaxValue, Calculator.Result);

			int expectedValue;
			Calculator.Random7 = expectedValue = 2;

			Assert.AreEqual(int.MaxValue, Calculator.Result);
			Assert.AreEqual(expectedValue, Calculator.Random7);
		}

		/// <summary>
		/// Tests amending code before an existing property setter without
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithoutOriginalValue()
		{
			int expected = Calculator.CopyToResult = 13;

			// Assert that both CopyToResult and Result equal 13
			Assert.AreEqual(expected, Calculator.CopyToResult);
			Assert.AreEqual(expected, Calculator.Result);
		}

		/// <summary>
		/// Tests amending code before an existing property setter and
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithOriginalValue()
		{
			// Demonstrates that setting the Add property causes the property to 
			// have the sum of the old and new values.
			Assert.AreEqual(0, Calculator.Add);
			Calculator.Add = 2;
			Assert.AreEqual(2, Calculator.Add);
			Calculator.Add = 2;
			Assert.AreEqual(4, Calculator.Add);
		}
		
		/// <summary>
		/// Tests amending code before an existing property setter without
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithoutOriginalOrNewValue()
		{
		}

		/// <summary>
		/// Tests amending code before an existing property setter without
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithoutOriginalAndWithNewValue()
		{
		}

		/// <summary>
		/// Tests amending code before an existing property setter without
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithOriginalAndWithoutNewValue()
		{
		}

		/// <summary>
		/// Tests amending code before an existing property setter without
		/// being passed the original value of the property.
		/// </summary>
		[TestMethod]
		public void BeforeSetWithOriginalAndNewValue()
		{
		}

	    /// <summary>
	    /// Tests amending code to pass ValueType property value to the nongeneric
	    /// AfterGet, BeforeSet and AfterSet delegates with object parameters (with boxing)
	    /// </summary>
	    [TestMethod]
	    public void AfterAndBeforeValueTypePropertyGetAndSetAmendedThroughNotTypedDelegate()
	    {
	        Calculator._valueTypeProperty = 5;

	        Calculator.ValueTypeProperty.ToString();

            Assert.AreEqual(Calculator.ValueTypePropertyAfterGetValue, 5);

	        Calculator.ValueTypeProperty = 10;

	        Assert.AreEqual(Calculator.ValueTypePropertyBeforeSetOldValue, 5);
	        Assert.AreEqual(Calculator.ValueTypePropertyBeforeSetValue, 10);

	        Assert.AreEqual(Calculator.ValueTypePropertyAfterSetOldValue, 5);
	        Assert.AreEqual(Calculator.ValueTypePropertyAfterSetValue, 10);
	        Assert.AreEqual(Calculator.ValueTypePropertyAfterSetNewValue, 11);
	    }
	}
}
