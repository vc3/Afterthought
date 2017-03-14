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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Afterthought
{
	#region Amendment<TAmended>.Constructor

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance);

			public delegate void BeforeConstructor(TAmended instance);

			public delegate void AfterConstructor(TAmended instance);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance);

				public new delegate void AfterConstructor(TAmended instance, TContext context);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor.Enumeration WithParams()
			{
				return new Constructor.Enumeration(constructors.OfType<Constructor>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor Add(string name, Constructor.ImplementConstructor implementation)
			{
				return Add(new Constructor(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1);

			public delegate void AfterConstructor(TAmended instance, P1 param1);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1>.Enumeration WithParams<P1>()
			{
				return new Constructor<P1>.Enumeration(constructors.OfType<Constructor<P1>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1> Add<P1>(string name, Constructor<P1>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2>.Enumeration WithParams<P1, P2>()
			{
				return new Constructor<P1, P2>.Enumeration(constructors.OfType<Constructor<P1, P2>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2> Add<P1, P2>(string name, Constructor<P1, P2>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3>.Enumeration WithParams<P1, P2, P3>()
			{
				return new Constructor<P1, P2, P3>.Enumeration(constructors.OfType<Constructor<P1, P2, P3>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3> Add<P1, P2, P3>(string name, Constructor<P1, P2, P3>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4>.Enumeration WithParams<P1, P2, P3, P4>()
			{
				return new Constructor<P1, P2, P3, P4>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4> Add<P1, P2, P3, P4>(string name, Constructor<P1, P2, P3, P4>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5>.Enumeration WithParams<P1, P2, P3, P4, P5>()
			{
				return new Constructor<P1, P2, P3, P4, P5>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5> Add<P1, P2, P3, P4, P5>(string name, Constructor<P1, P2, P3, P4, P5>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5, P6>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5, P6> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6>.Enumeration WithParams<P1, P2, P3, P4, P5, P6>()
			{
				return new Constructor<P1, P2, P3, P4, P5, P6>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5, P6>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5, P6> Add<P1, P2, P3, P4, P5, P6>(string name, Constructor<P1, P2, P3, P4, P5, P6>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5, P6>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5, P6, P7>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5, P6, P7> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7>()
			{
				return new Constructor<P1, P2, P3, P4, P5, P6, P7>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5, P6, P7>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5, P6, P7> Add<P1, P2, P3, P4, P5, P6, P7>(string name, Constructor<P1, P2, P3, P4, P5, P6, P7>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5, P6, P7>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5, P6, P7, P8>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5, P6, P7, P8> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8>()
			{
				return new Constructor<P1, P2, P3, P4, P5, P6, P7, P8>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5, P6, P7, P8>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8> Add<P1, P2, P3, P4, P5, P6, P7, P8>(string name, Constructor<P1, P2, P3, P4, P5, P6, P7, P8>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5, P6, P7, P8>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8, P9>()
			{
				return new Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9>(string name, Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

	#region Amendment<TAmended>.Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>

	public partial class Amendment<TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing constructor on a type.
		/// </summary>
		public partial class Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : Amendment.Constructor
		{
			#region Constructors

			internal Constructor(string name)
				: base(name)
			{ }

			internal Constructor(ConstructorInfo constructor)
				: base(constructor)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			public delegate void BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9, ref P10 param10);

			public delegate void AfterConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			public delegate void CatchConstructor<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
				where TException : Exception;

			public delegate void FinallyConstructor(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			#endregion

			#region Constructors
			
			/// <summary>
			/// Specifies the implementation for a new or existing void constructor.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Implement(ImplementConstructor implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Before(BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original constructor implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeConstructor before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original constructor implementation for void constructors.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> After(AfterConstructor after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// constructor implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Catch<TException>(CatchConstructor<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original constructor execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Finally(FinallyConstructor @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Constructor
			{
				Amendment.Constructor constructor;

				internal Context(Amendment.Constructor constructor)
					: base(constructor)
				{
					this.constructor = constructor;
				}

				#region Delegates

				public new delegate TContext BeforeConstructor(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9, ref P10 param10);

				public new delegate void AfterConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

				public delegate TResult AfterConstructor<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, TResult result);

				public delegate void CatchConstructor<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
					where TException : Exception;

				public delegate TResult CatchConstructor<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, TResult result)
					where TException : Exception;

				public new delegate void FinallyConstructor(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

				#endregion

				#region Constructors

				public Context<TContext> Before(BeforeConstructor before)
				{
					constructor.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterConstructor after)
				{
					constructor.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchConstructor<TException> implementation)
					where TException : Exception
				{
					constructor.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyConstructor implementation)
				{
					constructor.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Context<TContext>> constructors)
					{
						this.constructors = constructors; 
					}

					public Enumeration After(AfterConstructor after)
					{
						foreach (var constructor in constructors)
							constructor.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
						where TException : Exception
					{
						foreach (var constructor in constructors)
							constructor.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyConstructor @finally)
					{
						foreach (var constructor in constructors)
							constructor.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : ConstructorEnumeration<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>> constructors)
				{
					this.constructors = constructors;
				}

				public Enumeration Implement(ImplementConstructor implementation)
				{
					foreach (var constructor in constructors)
						constructor.Implement(implementation);
					return this;
				}

				public Enumeration Before(BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeConstructor before)
				{
					foreach (var constructor in constructors)
						constructor.Before(before);
					return new Context<TContext>.Enumeration(constructors.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterConstructor after)
				{
					foreach (var constructor in constructors)
						constructor.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchConstructor<TException> @catch)
					where TException : Exception
				{
					foreach (var constructor in constructors)
						constructor.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyConstructor @finally)
				{
					foreach (var constructor in constructors)
						constructor.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class ConstructorEnumeration : MemberEnumeration<ConstructorEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all constructors in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>()
			{
				return new Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Enumeration(constructors.OfType<Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>>());
			}
		}

		public partial class ConstructorList : ConstructorEnumeration
		{
			public Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(string name, Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.ImplementConstructor implementation)
			{
				return Add(new Constructor<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(name) { ImplementationMethod = implementation.Method });
			}
		}
	}

	#endregion

}