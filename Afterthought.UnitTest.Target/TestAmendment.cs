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
	public class TestAmendment<T> : Amendment<T,T>
		where T : Calculator
	{
		public TestAmendment()
		{
			#region Methods

			// Modify Multiply to also set the Result property to the resulting value
			Methods
				.Named("Multiply")
				.WithParams<int, int>()
				.Before((T instance, ref int x, ref int y) => instance.Result = x * y);

			// Modify SlowSum to measure the method execution time
			Methods
				.Named("SlowSum")
				.WithParams<int[]>()
				.Before((T instance, ref int[] values)
					=> { var s = new Stopwatch(); s.Start(); return s; })
				.Catch<OverflowException, int>((instance, stopwatch, exception, values)
					=> { instance.Result = (int)stopwatch.ElapsedMilliseconds; return Int32.MaxValue; })
				.Finally((instance, stopwatch, values)
					=> instance.Result = (int)stopwatch.ElapsedMilliseconds);

			// Modify Sum5 to swallow overflow errors
			Methods
				.Named("Sum5")
				.WithParams<int[]>()
				.Catch<OverflowException, int>((instance, exception, values)
					=> Int32.MaxValue);

			// Modify Multiply to also set the Result property to the resulting value
			Methods
				.Named("Add")
				.WithParams<int, int>()
				.Implement((instance, x, y) => x + y);

			// Modify Multiply2 to also set the Result property to the resulting value
			// Demonstrates use of array syntax
			Methods
				.Named("Multiply2")
				.Before((instance, methodName, parameters)
					=> instance.Result = (int)parameters[0] * (int)parameters[1]);

			// Modify Divide to change the second parameter value to 1 every time
			Methods
				.Named("Divide")
				.WithParams<int, int>()
				.Before((T instance, ref int x, ref int y)
					=> y = 1);


			// Modify Divide2 to change the second parameter value to 1 every time
			Methods
				.Named("Divide2")
				.Before((instance, methodName, parameters)
					=> parameters[1] = 1);

			// Replace implementation of Square to correct coding error
			Methods
				.Named("Square")
				.WithParams<int>()
				.Implement((instance, x) 
					=> x * x);

			// Modify Double to double each of the input values
			Methods
				.Named("Double")
				.WithParams<int[]>()
				.After((instance, set) =>
					{
						for (int i = 0; i < set.Length; i++)
							set[i] = set[i] * 2;
					});

				// Modify Double to double each of the input values
			Methods
				.Named("Double2")
				.After((instance, methodName, parameters) =>
					{
						for (int i = 0; i < ((int[])parameters[0]).Length; i++)
							((int[])parameters[0])[i] = ((int[])parameters[0])[i] * 2;
					});

			// Modify Sum to return the sum of the input values
			Methods
				.Named("Sum")
				.WithParams<int[]>()
				.After<long>((instance, set, result) 
					=> set.Sum());

			// Modify Sum2 to return the sum of the input values
			Methods
				.Named("Sum2")
				.After((instance, methodName, parameters, result) 
					=> (long)((int[])parameters[0]).Sum());
	

			// Modify the input values but ignore the return value
			Methods
				.Named("Sum3")
				.After((instance, methodName, parameters) =>
					{
						for (int i = 1; i < ((int[])parameters[0]).Length; i++)
							((int[])parameters[0])[i] = ((int[])parameters[0])[i - 1] + ((int[])parameters[0])[i];
					});

			#endregion

			#region Properties

			// public int Count { get; set; }
			Properties
				.Add<int>("Count");

			// public int Three { get { return One + Two; } }
			Properties
				.Add<int>("Three")
				.Get((instance, property) => instance.One + instance.Two);
		
			// public string LazyRandomName { get { if (lazyRandomName == null) lazyRandomName = "r" + new Random().Next(); return lazyRandomName; } }
			Properties
				.Add<string>("LazyRandomName", (instance, property) => "r" + new Random().Next());

			// public int SetResult { set { Result = value; } }
			Properties
				.Add<int>("SetResult")
				.Set((instance, property, value) => instance.Result = value);

			// public int InitiallyTwelve { get; set; } = 12
			Properties
				.Add<int>("InitiallyTwelve")
				.Initialize((instance, property) => 12);

			// Modify Random1 getter set value of Random1 to GetRandom(1) before returning the underlying property value
			Properties
				.Named("Random1")
				.BeforeGet((instance, propertyName) => instance.Random1 = instance.GetRandom(1));

			// Modify Random2 to calculate a random number based on an assigned seed value
			Properties
				.Named("Random2")
			    .OfType<int>()
				.AfterGet((instance, propertyName, result) => instance.GetRandom(result));

			// Modify ILog.e property
			Properties
				.Named("Afterthought.UnitTest.Target.ILog.e")
				.OfType<decimal>()
				.Get((instance, propertyName) => 2.71828m);

			// Modify Random5 to be just a simple getter/setter using Result property as the backing store
			Properties
				.Named("Random5")
				.OfType<int>()
				.Get((instance, propertyName) => instance.Result)
			    .Set((instance, propertyName, value) => instance.Result = value);

			// Update Result to equal the value assigned to CopyToResult
			Properties
				.Named("CopyToResult")
				.OfType<int>()
				.BeforeSet((instance, propertyName, oldValue, value) => { instance.Result = value; return value; });

			// Modify Add to add the old value to the value being assigned
			Properties
				.Named("Add")
				.OfType<int>()
				.BeforeSet((instance, propertyName, oldValue, value) => oldValue + value);

			// Initialize InitiallyThirteen to 13
			Properties
				.Named("InitiallyThirteen")
				.OfType<int>()
				.Initialize((instance, propertyName) => 13);

			// Initialize ExistingLazyRandomName
			Properties
				.Named("ExistingLazyRandomName")
				.OfType<string>()
				.LazyInitialize((instance, propertyName) => "r" + new Random().Next());

			#endregion

			#region Interfaces

			// Implement IMath interface
			Implement<IMath>(

				// Pi
				Properties.Add<decimal>("Pi").Get((instance, property) => 3.14159m),

				// Subtract(x, y)
				Methods.Add("Subtract", (T instance, decimal x, decimal y) => x - y)
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

		internal static Stopwatch BeforeSlowSum2(T instance, ref int[] values)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			return stopwatch;
		}

		internal static int CatchSlowSum2(T instance, Stopwatch stopwatch, OverflowException e, int[] values)
		{
			return Int32.MaxValue;
		}

		internal static void FinallySlowSum2(T instance, Stopwatch stopwatch, int[] values)
		{
			instance.Result = (int)stopwatch.ElapsedMilliseconds;
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
