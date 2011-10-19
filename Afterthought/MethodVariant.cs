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
	#region Amendment<TType, TAmended>.Method

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance);

			public delegate TResult ImplementMethod<TResult>(TAmended instance);

			public delegate void BeforeMethod(TAmended instance);

			public delegate void AfterMethod(TAmended instance);

			public delegate TResult AfterMethod<TResult>(TAmended instance, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance);

				public new delegate void AfterMethod(TAmended instance, TContext context);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method.Enumeration WithParams()
			{
				return new Method.Enumeration(methods.OfType<Method>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method Add(string name, Method.ImplementMethod implementation)
			{
				return Add(new Method(name) { ImplementationMethod = implementation.Method });
			}

			public Method Add<TResult>(string name, Method.ImplementMethod<TResult> implementation)
			{
				return Add(new Method(name) { ImplementationMethod = implementation.Method });
			}

			public Method Override(string name)
			{
				return Add(new Method(name) { OverrideMethod = GetOverrideMethod(name) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1);

			public delegate void AfterMethod(TAmended instance, P1 param1);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1>.Enumeration WithParams<P1>()
			{
				return new Method<P1>.Enumeration(methods.OfType<Method<P1>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1> Add<P1>(string name, Method<P1>.ImplementMethod implementation)
			{
				return Add(new Method<P1>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1> Add<P1, TResult>(string name, Method<P1>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1> Override<P1>(string name)
			{
				return Add(new Method<P1>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2>.Enumeration WithParams<P1, P2>()
			{
				return new Method<P1, P2>.Enumeration(methods.OfType<Method<P1, P2>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2> Add<P1, P2>(string name, Method<P1, P2>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2> Add<P1, P2, TResult>(string name, Method<P1, P2>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2> Override<P1, P2>(string name)
			{
				return Add(new Method<P1, P2>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3>.Enumeration WithParams<P1, P2, P3>()
			{
				return new Method<P1, P2, P3>.Enumeration(methods.OfType<Method<P1, P2, P3>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3> Add<P1, P2, P3>(string name, Method<P1, P2, P3>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3> Add<P1, P2, P3, TResult>(string name, Method<P1, P2, P3>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3> Override<P1, P2, P3>(string name)
			{
				return Add(new Method<P1, P2, P3>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4>.Enumeration WithParams<P1, P2, P3, P4>()
			{
				return new Method<P1, P2, P3, P4>.Enumeration(methods.OfType<Method<P1, P2, P3, P4>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4> Add<P1, P2, P3, P4>(string name, Method<P1, P2, P3, P4>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4> Add<P1, P2, P3, P4, TResult>(string name, Method<P1, P2, P3, P4>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4> Override<P1, P2, P3, P4>(string name)
			{
				return Add(new Method<P1, P2, P3, P4>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5>.Enumeration WithParams<P1, P2, P3, P4, P5>()
			{
				return new Method<P1, P2, P3, P4, P5>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5> Add<P1, P2, P3, P4, P5>(string name, Method<P1, P2, P3, P4, P5>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5> Add<P1, P2, P3, P4, P5, TResult>(string name, Method<P1, P2, P3, P4, P5>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5> Override<P1, P2, P3, P4, P5>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5, P6>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5, P6> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6>.Enumeration WithParams<P1, P2, P3, P4, P5, P6>()
			{
				return new Method<P1, P2, P3, P4, P5, P6>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5, P6>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5, P6> Add<P1, P2, P3, P4, P5, P6>(string name, Method<P1, P2, P3, P4, P5, P6>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6> Add<P1, P2, P3, P4, P5, P6, TResult>(string name, Method<P1, P2, P3, P4, P5, P6>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6> Override<P1, P2, P3, P4, P5, P6>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5), typeof(P6)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5, P6, P7>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5, P6, P7> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7>()
			{
				return new Method<P1, P2, P3, P4, P5, P6, P7>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5, P6, P7>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5, P6, P7> Add<P1, P2, P3, P4, P5, P6, P7>(string name, Method<P1, P2, P3, P4, P5, P6, P7>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7> Add<P1, P2, P3, P4, P5, P6, P7, TResult>(string name, Method<P1, P2, P3, P4, P5, P6, P7>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7> Override<P1, P2, P3, P4, P5, P6, P7>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5), typeof(P6), typeof(P7)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5, P6, P7, P8>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5, P6, P7, P8> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8>()
			{
				return new Method<P1, P2, P3, P4, P5, P6, P7, P8>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5, P6, P7, P8>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Add<P1, P2, P3, P4, P5, P6, P7, P8>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Add<P1, P2, P3, P4, P5, P6, P7, P8, TResult>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8> Override<P1, P2, P3, P4, P5, P6, P7, P8>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5), typeof(P6), typeof(P7), typeof(P8)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8, P9>()
			{
				return new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9, TResult>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9> Override<P1, P2, P3, P4, P5, P6, P7, P8, P9>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5), typeof(P6), typeof(P7), typeof(P8), typeof(P9)) });
			}
		}
	}

	#endregion

	#region Amendment<TType, TAmended>.Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>

	public partial class Amendment<TType, TAmended> : Amendment
	{
		/// <summary>
		/// Represents an amendment for a new or existing method on a type.
		/// </summary>
		public partial class Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : Amendment.Method
		{
			#region Constructors

			internal Method(string name)
				: base(name)
			{ }

			internal Method(MethodInfo method)
				: base(method)
			{ }

			#endregion

			#region Delegates

			public delegate void ImplementMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			public delegate TResult ImplementMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			public delegate void BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9, ref P10 param10);

			public delegate void AfterMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			public delegate TResult AfterMethod<TResult>(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, TResult result);

			public delegate void CatchMethod<TException>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
				where TException : Exception;

			public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
				where TException : Exception;

			public delegate void FinallyMethod(TAmended instance, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

			#endregion

			#region Methods
			
			/// <summary>
			/// Specifies the implementation for a new or existing void method.
			/// </summary>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Implement(ImplementMethod implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the implementation for a new or existing method returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="implementation"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Implement<TResult>(ImplementMethod<TResult> implementation)
			{
				base.ImplementationMethod = implementation.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation.
			/// </summary>
			/// <param name="before"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Before(BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call before the original method implementation, which returns an
			/// instance of <see cref="TContext"/> that is passed to <see cref="After"/>, <see cref="Catch"/>
			/// and <see cref="Finally"/> delegates.
			/// </summary>
			/// <typeparam name="TContext"></typeparam>
			/// <param name="before"></param>
			/// <returns></returns>
			public Context<TContext> Before<TContext>(Context<TContext>.BeforeMethod before)
			{
				base.BeforeMethod = before.Method;
				return new Context<TContext>(this);
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for void methods.
			/// </summary>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> After(AfterMethod after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call after the original method implementation for methods
			/// returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="after"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> After<TResult>(AfterMethod<TResult> after)
			{
				base.AfterMethod = after.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original void
			/// method implementation.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Catch<TException>(CatchMethod<TException> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call when an exception occurs in the original method
			/// for methods returning <see cref="TResult"/>.
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <typeparam name="TResult"></typeparam>
			/// <param name="catch"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
				where TException : Exception
			{
				base.CatchMethod = @catch.Method;
				return this;
			}

			/// <summary>
			/// Specifies the delegate to call inside a finally block when the original method execution
			/// exits normally or due to an exception.
			/// </summary>
			/// <param name="finally"></param>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Finally(FinallyMethod @finally)
			{
				base.FinallyMethod = @finally.Method;
				return this;
			}

			#endregion

			#region Context

			public class Context<TContext> : Amendment.Method
			{
				Amendment.Method method;

				internal Context(Amendment.Method method)
					: base(method)
				{
					this.method = method;
				}

				#region Delegates

				public new delegate TContext BeforeMethod(TAmended instance, ref P1 param1, ref P2 param2, ref P3 param3, ref P4 param4, ref P5 param5, ref P6 param6, ref P7 param7, ref P8 param8, ref P9 param9, ref P10 param10);

				public new delegate void AfterMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

				public delegate TResult AfterMethod<TResult>(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10, TResult result);

				public delegate void CatchMethod<TException>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
					where TException : Exception;

				public delegate TResult CatchMethod<TException, TResult>(TAmended instance, TContext context, TException exception, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10)
					where TException : Exception;

				public new delegate void FinallyMethod(TAmended instance, TContext context, P1 param1, P2 param2, P3 param3, P4 param4, P5 param5, P6 param6, P7 param7, P8 param8, P9 param9, P10 param10);

				#endregion

				#region Methods

				public Context<TContext> Before(BeforeMethod before)
				{
					method.BeforeMethod = before.Method;
					return this;
				}

				public Context<TContext> After(AfterMethod after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> After<TResult>(AfterMethod<TResult> after)
				{
					method.AfterMethod = after.Method;
					return this;
				}

				public Context<TContext> Catch<TException>(CatchMethod<TException> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Catch<TException, TResult>(CatchMethod<TException, TResult> implementation)
					where TException : Exception
				{
					method.CatchMethod = implementation.Method;
					return this;
				}

				public Context<TContext> Finally(FinallyMethod implementation)
				{
					method.FinallyMethod = implementation.Method;
					return this;
				}

				#endregion

				#region Enumeration

				public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Context<TContext>, Enumeration>
				{
					public Enumeration()
					{ }

					internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Context<TContext>> methods)
					{
						this.methods = methods; 
					}

					public Enumeration After(AfterMethod after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration After<TResult>(AfterMethod<TResult> after)
					{
						foreach (var method in methods)
							method.After(after);
						return this;
					}

					public Enumeration Catch<TException>(CatchMethod<TException> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException>(@catch);
						return this;
					}

					public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
						where TException : Exception
					{
						foreach (var method in methods)
							method.Catch<TException, TResult>(@catch);
						return this;
					}

					public Enumeration Finally(FinallyMethod @finally)
					{
						foreach (var method in methods)
							method.Finally(@finally);
						return this;
					}
				}

				#endregion
			}

			#endregion

			#region Enumeration

			public class Enumeration : MethodEnumeration<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>, Enumeration>
			{
				public Enumeration()
				{ }

				internal Enumeration(IEnumerable<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>> methods)
				{
					this.methods = methods;
				}

				public Enumeration Implement(ImplementMethod implementation)
				{
					foreach (var method in methods)
						method.Implement(implementation);
					return this;
				}

				public Enumeration Implement<TResult>(ImplementMethod<TResult> implementation)
				{
					foreach (var method in methods)
						method.Implement<TResult>(implementation);
					return this;
				}

				public Enumeration Before(BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return this;
				}

				public Context<TContext>.Enumeration Before<TContext>(Context<TContext>.BeforeMethod before)
				{
					foreach (var method in methods)
						method.Before(before);
					return new Context<TContext>.Enumeration(methods.Select(m => new Context<TContext>(m)));
				}

				public Enumeration After(AfterMethod after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration After<TResult>(AfterMethod<TResult> after)
				{
					foreach (var method in methods)
						method.After(after);
					return this;
				}

				public Enumeration Catch<TException>(CatchMethod<TException> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException>(@catch);
					return this;
				}

				public Enumeration Catch<TException, TResult>(CatchMethod<TException, TResult> @catch)
					where TException : Exception
				{
					foreach (var method in methods)
						method.Catch<TException, TResult>(@catch);
					return this;
				}

				public Enumeration Finally(FinallyMethod @finally)
				{
					foreach (var method in methods)
						method.Finally(@finally);
					return this;
				}
			}

			#endregion
		}

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			/// <summary>
			/// Gets all methods in the set with the specified parameters.
			/// </summary>
			/// <typeparam name="P1"></typeparam>
			/// <typeparam name="P2"></typeparam>
			/// <returns></returns>
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Enumeration WithParams<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>()
			{
				return new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.Enumeration(methods.OfType<Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>>());
			}
		}

		public partial class MethodList : MethodEnumeration
		{
			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.ImplementMethod implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Add<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, TResult>(string name, Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>.ImplementMethod<TResult> implementation)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(name) { ImplementationMethod = implementation.Method });
			}

			public Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Override<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(string name)
			{
				return Add(new Method<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(name) { OverrideMethod = GetOverrideMethod(name, typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5), typeof(P6), typeof(P7), typeof(P8), typeof(P9), typeof(P10)) });
			}
		}
	}

	#endregion

}