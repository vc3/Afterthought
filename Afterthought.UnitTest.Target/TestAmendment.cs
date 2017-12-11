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
using System.ComponentModel;
using System.Diagnostics;


namespace Afterthought.UnitTest.Target
{
	public class TestAmendment : Amendment<Calculator,Calculator>
		//where T : Calculator
	{
		public static class AmendmentsFunctions
		{
			public static bool BeforeConstructor(Calculator instance, ref bool enterIfStatement)
			{
				instance.Result = int.MaxValue;
				return enterIfStatement;
			}


			public static void AfterConstructor(Calculator instance, bool enterIfStatement, int constructorSetIntIfTrue)
			{
				instance.Result = int.MaxValue;
			}

			public static void BeforeMultiply(Calculator instance, ref int x, ref int y) { instance.Result = x * y; }

			public static Stopwatch BeforeSlowSum(Calculator instance, ref int[] values) { var s = new Stopwatch(); s.Start(); return s; }

			public static Int32 CatchSlowSumException(Calculator instance, Stopwatch stopwatch, OverflowException exception, int[] values)

			{ instance.Result = (int)stopwatch.ElapsedMilliseconds; return Int32.MaxValue; }

			public static void SlowSumFinallyBlock(Calculator instance, Stopwatch stopwatch, int[] values)
			{
				instance.Result = (int)stopwatch.ElapsedMilliseconds;
			}

			public static Int32 GetThree(Calculator instance, string property) { return instance.One + instance.Two; }

			internal static string GetLazyRandomName(Calculator instance, string propertyName)
			{
				return "r" + new Random().Next();
			}

			internal static void SetReslt(Calculator instance, string propertyName, int value)
			{
				 instance.Result = value;
			}

			internal static int InitiallyTwelve(Calculator instance, string propertyName)
			{
				return 12;
			}

			internal static void BeforeGetRandome1(Calculator instance, string propertyName)
			{
				instance.Random1 = instance.GetRandom(1);
			}

			internal static int AfterGetRandom2(Calculator instance, string propertyName, int returnValue)
			{
				return instance.GetRandom(returnValue);
			}

			internal static decimal GetIlogE(Calculator instance, string propertyName)
			{
				return 2.71828m;
			}

			internal static int GetRandom5(Calculator instance, string propertyName)
			{
				return instance.Result;
			}

			internal static void SetRandom5(Calculator instance, string propertyName, int value)
			{
				instance.Result = value;
			}

			internal static int BeforeSetRandom6(Calculator instance, string propertyName, int oldValue, int value)
			{
				instance.Result = int.MaxValue;
				return value;
			}

			internal static void AftrSetRandom7(Calculator instance, string propertyName, int oldValue, int value, int newValue)
			{
				instance.Result = int.MaxValue;
			}

			internal static int BeforeSetCopyToResult(Calculator instance, string propertyName, int oldValue, int value)
			{
				instance.Result = value; return value;
			}

			internal static int BeforeSetAdd(Calculator instance, string propertyName, int oldValue, int value)
			{
				return oldValue + value;
			}

			internal static int InitializeThirteen(Calculator instance, string propertyName)
			{
				return 13;
			}

			internal static string ExistingLazyRandomNameLazyInitialize(Calculator instance, string propertyName)
			{
				return  "r" + new Random().Next();
			}

			internal static Stopwatch BeforeSlowSum3(Calculator instance,ref int[] parameters)
			{
				var s = new Stopwatch(); s.Start(); return s;
			}

			internal static int  CatchSlowSum3(Calculator instance,  Stopwatch context, OverflowException exception, int[] parameters)
			{
				instance.Result = (int)context.ElapsedMilliseconds; return Int32.MaxValue;
			}

			internal static void FinallySlowSum3(Calculator instance,  Stopwatch context, int[] parameters)
			{
				instance.Result = (int)context.ElapsedMilliseconds;
			}

			internal static int CatchSum5(Calculator instance, OverflowException exception, int[] param1)
			{
				return Int32.MaxValue;
			}

			internal static void AfterSlowSum4(Calculator instance, string method, object[] parameters)
			{
				//throw new NotImplementedException();
			}

			internal static int ImplementAdd(Calculator instance, int x, int y)
			{
				return x + y;
			}

			internal static void BeforeMultiply2(Calculator instance, string methodName, object[] parameters)
			{
				instance.Result = (int)parameters[0] * (int)parameters[1];
			}

			internal static void BeforeDivide(Calculator instance, ref int x, ref int y)
			{
				y = 1;
			}

			internal static Int32 ImpementSquare(Calculator instance, int x)
			{
				return x* x;
			}

			internal static void AfterDouble2(Calculator instance, string method, object[] parameters)
			{
				for (int i = 0; i < ((int[])parameters[0]).Length; i++)
					((int[])parameters[0])[i] = ((int[])parameters[0])[i] * 2;
			}

			static internal void AfterDouble(Calculator instance, int[] set)
			{
				for (int i = 0; i < set.Length; i++)
					set[i] = set[i] * 2;
			}

			internal static void BeforeDouble3(Calculator instance, ref int[] set, ref bool param2)
			{
				for (int i = 0; i < set.Length; i++)
					set[i] = set[i] * 2;
			}

			internal static void AfterDouble4(Calculator instance, int[] set, bool param2)
			{
				for (int i = 0; i < set.Length; i++)
					set[i] = set[i] * 2;
			}

			internal static long AfterSum(Calculator instance, int[] set, long result)
			{
				return set.Sum();
			}

			internal static object AfterSum2(Calculator instance, string method, object[] parameters,object result)
			{
				return (long)((int[])parameters[0]).Sum();
			}

			internal static void AfterSum3(Calculator instance, string method, object[] parameters)
			{
				for (int i = 1; i < ((int[])parameters[0]).Length; i++)
					((int[])parameters[0])[i] = ((int[])parameters[0])[i - 1] + ((int[])parameters[0])[i];
			}

			internal static decimal ImplementIMathSubtract(Calculator instance, decimal x, decimal y)
			{
				return x - y;
			}


			internal static decimal GetPI(Calculator instance,string propertyName)
			{
				return 3.14159m;
			}

			//(instance, result) => instance.Result = result
			internal static void Constructor(Calculator instance, int param1)
			{
				instance.Result = param1;
			}

			internal static bool BeforeConstructorEnterIf(Calculator instance, ref bool enterIfStatement)
			{
				//(Calculator instance, ref bool enterIfStatement) =>
				//{
				//	instance.Result = int.MaxValue;
				//	return enterIfStatement;
				//}

				instance.Result = int.MaxValue;
				return enterIfStatement;
			}

			internal static void BeforeDivide2(Calculator instance, string method, object[] parameters)
			{
				parameters[1] = 1;
			}
		}
		public TestAmendment()
		{
			#region Construtors

			//TODO: Solve
			////AddConstructor not implemented
			//Constructors
			//	.Add<int>("Calculator", (instance, result) => instance.Result = result);

			

			Constructors
				.WithParams<bool>()
				.Before<bool>(AmendmentsFunctions.BeforeConstructor);
			
			Constructors
				.WithParams<bool, int>()
				.After(AmendmentsFunctions.AfterConstructor);

			#endregion

			#region Methods

			// Modify Multiply to also set the Result property to the resulting value
			Methods
				.Named("Multiply")
				.WithParams<int, int>()
				.Before(AmendmentsFunctions.BeforeMultiply);


			// Modify SlowSum to measure the method execution time
			Methods
				.Named("SlowSum")
				.WithParams<int[]>()
				.Before(AmendmentsFunctions.BeforeSlowSum)
				.Catch<OverflowException, int>(AmendmentsFunctions.CatchSlowSumException)
				.Finally(AmendmentsFunctions.SlowSumFinallyBlock);


			// Modify SlowSum to measure the method execution time
			Methods
				.Named("SlowSum3")
				.WithParams<int[]>()
				.Before(AmendmentsFunctions.BeforeSlowSum3)
				.Catch<OverflowException,int>(AmendmentsFunctions.CatchSlowSum3)
				.Finally(AmendmentsFunctions.FinallySlowSum3);

			// Modify Sum5 to swallow overflow errors
			Methods
				.Named("Sum5")
				.WithParams<int[]>()
				.Catch<OverflowException, int>(AmendmentsFunctions.CatchSum5 );

			//TODO: Solve the non WithParam condition
			// Modify Sum5 to swallow overflow errors
			Methods
				.Named("SlowSum4")
				.After(AmendmentsFunctions.AfterSlowSum4);

			// Amend a generic method
			//Methods
			//    .Named("Sum")
			//    .Where(m => m.MethodInfo != null && m.MethodInfo.IsGenericMethodDefinition)
			//    .After((T instance, string method, object[] parameters, object result) 
			//        => { instance.Result = (int)result; return result; });

			// Modify Multiply to also set the Result property to the resulting value
			Methods
				.Named("Add")
				.WithParams<int, int>()
				.Implement(AmendmentsFunctions.ImplementAdd);

			//TODO: Solve the non WithParam condition
			//// Modify Multiply2 to also set the Result property to the resulting value
			//// Demonstrates use of array syntax
			Methods
				.Named("Multiply2")
				.Before(AmendmentsFunctions.BeforeMultiply2);

			// Modify Divide to change the second parameter value to 1 every time
			Methods
				.Named("Divide")
				.WithParams<int, int>()
				.Before(AmendmentsFunctions.BeforeDivide);


			//TODO: Solve the non WithParam condition
			//// Modify Divide2 to change the second parameter value to 1 every time
			Methods
				.Named("Divide2")
				.Before(AmendmentsFunctions.BeforeDivide2);


			// Replace implementation of Square to correct coding error
			Methods
				.Named("Square")
				.WithParams<int>()
				.Implement(AmendmentsFunctions.ImpementSquare);

			// Modify Double to double each of the input values
			Methods
				.Named("Double")
				.WithParams<int[]>()
				.After(AmendmentsFunctions.AfterDouble);

			// Modify Double to double each of the input values
			Methods
				.Named("Double2")
				.After(AmendmentsFunctions.AfterDouble2);

			// Modify Double to double each of the input values
			Methods
				.Named("Double3")
				.WithParams<int[], bool>()
				.Before(AmendmentsFunctions.BeforeDouble3);

			//TODO: Solve
			//// Modify Double to double each of the input values
			Methods
				.Named("Double4")
				.WithParams<int[], bool>()
				.After(AmendmentsFunctions.AfterDouble4);

			// Modify Sum to return the sum of the input values
			Methods
				.Named("Sum")
				.WithParams<int[]>()
				.After<long>(AmendmentsFunctions.AfterSum);

			// Modify Sum2 to return the sum of the input values
			Methods
				.Named("Sum2")
				.After(AmendmentsFunctions.AfterSum2);


			// Modify the input values but ignore the return value
			Methods
				.Named("Sum3")
				.After(AmendmentsFunctions.AfterSum3);

			#endregion

			#region Properties

			// public int Count { get; set; }
			Properties
				.Add<int>("Count");

			// public int Three { get { return One + Two; } }
			Properties
				.Add<int>("Three")
				.Get(AmendmentsFunctions.GetThree);

			//// public string LazyRandomName { get { if (lazyRandomName == null) lazyRandomName = "r" + new Random().Next(); return lazyRandomName; } }
			Properties
				.Add<string>("LazyRandomName", AmendmentsFunctions.GetLazyRandomName);

			// public int SetResult { set { Result = value; } }
			Properties
				.Add<int>("SetResult")
				.Set(AmendmentsFunctions.SetReslt);

			// public int InitiallyTwelve { get; set; } = 12
			Properties
				.Add<int>("InitiallyTwelve")
				.Initialize(AmendmentsFunctions.InitiallyTwelve);

			// Modify Random1 getter set value of Random1 to GetRandom(1) before returning the underlying property value
			Properties
				.Named("Random1")
				.BeforeGet(AmendmentsFunctions.BeforeGetRandome1);

			// Modify Random2 to calculate a random number based on an assigned seed value
			Properties
				.Named("Random2")
				.OfType<int>()
				.AfterGet(AmendmentsFunctions.AfterGetRandom2);

			// Modify ILog.e property
			Properties
				.Named("Afterthought.UnitTest.Target.ILog.e")
				.OfType<decimal>()
				.Get(AmendmentsFunctions.GetIlogE);

			// Modify Random5 to be just a simple getter/setter using Result property as the backing store
			Properties
				.Named("Random5")
				.OfType<int>()
				.Get(AmendmentsFunctions.GetRandom5)
				.Set(AmendmentsFunctions.SetRandom5);

			// Modify Random6 to always set the result to int.MaxValue before set
			Properties
				.Named("Random6")
				.OfType<int>()
				.BeforeSet(AmendmentsFunctions.BeforeSetRandom6);

			// Modify Random7 to always set the result to int.MaxValue after set
			Properties
				.Named("Random7")
				.OfType<int>()
				.AfterSet(AmendmentsFunctions.AftrSetRandom7);

			// Update Result to equal the value assigned to CopyToResult
			Properties
				.Named("CopyToResult")
				.OfType<int>()
				.BeforeSet(AmendmentsFunctions.BeforeSetCopyToResult);

			// Modify Add to add the old value to the value being assigned
			Properties
				.Named("Add")
				.OfType<int>()
				.BeforeSet(AmendmentsFunctions.BeforeSetAdd);

			// Initialize InitiallyThirteen to 13
			Properties
				.Named("InitiallyThirteen")
				.OfType<int>()
				.Initialize(AmendmentsFunctions.InitializeThirteen );

			// Initialize ExistingLazyRandomName
			Properties
				.Named("ExistingLazyRandomName")
				.OfType<string>()
				.LazyInitialize(AmendmentsFunctions.ExistingLazyRandomNameLazyInitialize);

			#endregion

			#region Interfaces

			////Implement IMath interface
			Implement<IMath>(
				// Subtract(x, y)
				Methods.Add("Subtract", (Method<decimal, decimal>.ImplementMethod<decimal>)AmendmentsFunctions.ImplementIMathSubtract)
				 // Pi
				 , Properties.Add<decimal>("Pi").Get(AmendmentsFunctions.GetPI)

			 //, Properties.Add<decimal>("SqRt2").Get((instance, property) => 1.5m)
			 //, Properties.Add<decimal>("Base")
			 //.Get((instance, property) => 10)
			 //.Set((instance, property, value) => { })
			 );

			#endregion

			#region Attributes

			// Type Attributes
			Attributes.Add<TestAttribute, Type>(typeof(string));
			Attributes.Add<TestAttribute>();
			Attributes.Add<TestAttribute, int>(5);
			Attributes.Add<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			// Field Attributes
			Fields
				.Named("holding1")
				.AddAttribute<TestAttribute, Type>(typeof(string))
				.AddAttribute<TestAttribute>()
				.AddAttribute<TestAttribute, int>(5)
				.AddAttribute<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			// Constructor Attributes
			Constructors
				.WithParams()
				.AddAttribute<TestAttribute, Type>(typeof(string))
				.AddAttribute<TestAttribute>()
				.AddAttribute<TestAttribute, int>(5)
				.AddAttribute<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			// Property Attributes
			Properties
				.Named("Random1")
				.AddAttribute<TestAttribute, Type>(typeof(string))
				.AddAttribute<TestAttribute>()
				.AddAttribute<TestAttribute, int>(5)
				.AddAttribute<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			// Method Attributes
			Methods
				.Named("Multiply")
				.AddAttribute<TestAttribute, Type>(typeof(string))
				.AddAttribute<TestAttribute>()
				.AddAttribute<TestAttribute, int>(5)
				.AddAttribute<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			// EventAttributes
			Events
				.Named("Calculate")
				.AddAttribute<TestAttribute, Type>(typeof(string))
				.AddAttribute<TestAttribute>()
				.AddAttribute<TestAttribute, int>(5)
				.AddAttribute<TestAttribute, string[]>(new string[] { "Testing", "Two" });

			#endregion
		}

		

		internal static Stopwatch BeforeSlowSum2(Calculator instance, ref int[] values)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		internal static object CatchSlowSum2(Calculator instance, Stopwatch stopwatch, OverflowException e, int[] values)
		{
			return Int32.MaxValue;
		}

		internal static void FinallySlowSum2(Calculator instance, Stopwatch stopwatch, int[] values)
		{
			instance.Result = (int)stopwatch.ElapsedMilliseconds;
		}

		internal static Stopwatch BeforeSlowSum3(Calculator instance, string method, object[] parameters)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		internal static int CatchSlowSum3(Calculator instance, string method, object[] parameters, Stopwatch stopwatch, OverflowException e)
		{
			return Int32.MaxValue;
		}

		internal static void FinallySlowSum3(Calculator instance, string method, object[] parameters, Stopwatch stopwatch)
		{
			instance.Result = (int)stopwatch.ElapsedMilliseconds;
		}

		internal static void AfterSlowSum3(Calculator instance, string method, object[] parameters)
		{
			
		}
	}

	public class TestAttribute : System.Attribute
	{
		public int IntValue { get; private set; }
		public Type TypeValue { get; private set; }
		public string[] StringArValue { get; private set; }
		public object ObjectValue { get; private set; }

		public TestAttribute()
		{
			IntValue = -1;
		}

		public TestAttribute(int number)
		{
			IntValue = number;
		}

		public TestAttribute(Type type)
		{
			TypeValue = type;
		}

		public TestAttribute(string[] values)
		{
			StringArValue = values;
		}

		public TestAttribute(object values)
		{
			ObjectValue = values;
		}
	}
}
