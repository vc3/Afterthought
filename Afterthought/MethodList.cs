using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Reflection;

namespace Afterthought
{
	public partial class Amendment<TType, TAmended> : Amendment
	{
		#region MethodList

		public partial class MethodList : MethodEnumeration
		{
			IList<Amendment.Method> methods;

			internal MethodList()
				: base(new List<Amendment.Method>())
			{
				this.methods = (IList<Amendment.Method>)base.methods;
			}

			internal TMethod Add<TMethod>(TMethod method)
				where TMethod : Amendment.Method
			{
				methods.Add(method);
				return method;
			}

			static MethodInfo GetOverrideMethod(string name, params Type[] parameterTypes)
			{
				return typeof(TType).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, parameterTypes, null);
			}

			public Method Raise(Event @event, string name)
			{
				return Add(new Method(name) { RaisesEvent = @event });
			}
		}

		#endregion



		#region MethodEnumeration

		public partial class MethodEnumeration : MemberEnumeration<MethodEnumeration>, IEnumerable
		{
			internal IEnumerable<Amendment.Method> methods;

			internal MethodEnumeration(IEnumerable<Amendment.Method> methods)
			{
				this.methods = methods;
			}

			#region Delegates

			public delegate void BeforeMethod(TAmended instance, string method, object[] parameters);

			public delegate void AfterMethodAction(TAmended instance, string method, object[] parameters);
			
			public delegate object AfterMethodFunc(TAmended instance, string method, object[] parameters, object result);
			
			public delegate void CatchMethodAction<TException>(TAmended instance, string method, TException exception, object[] parameters);

			public delegate object CatchMethodFunc<TException>(TAmended instance, string method, TException exception, object[] parameters);

			#endregion

			#region Methods

			/// <summary>
			/// Gets all methods in the set with the specified name.
			/// </summary>
			/// <param name="methods"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public MethodEnumeration Named(string name)
			{
				return new MethodEnumeration(methods.Where(m => m.Name == name));
			}

			/// <summary>
			/// Gets the set of methods that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public MethodEnumeration Where(Func<Amendment.Method, bool> predicate)
			{
				return new MethodEnumeration(methods.Where(predicate));
			}

			public MethodEnumeration Before(BeforeMethod before)
			{
				foreach (Amendment.Method method in this)
					method.BeforeMethod = before.Method;
				return this;
			}

			public Method.Context<TContext>.Enumeration Before<TContext>(Method.Context<TContext>.BeforeMethod before)
			{
				foreach (Amendment.Method method in this)
					method.BeforeMethod = before.Method;
				return new Method.Context<TContext>.Enumeration(methods.Select(m => new Method.Context<TContext>(m)));
			}

			public MethodEnumeration After(AfterMethodAction after)
			{
				foreach (Amendment.Method method in this)
					method.AfterMethod = after.Method;
				return this;
			}

			public MethodEnumeration After(AfterMethodFunc after)
			{
				foreach (Amendment.Method method in this)
					method.AfterMethod = after.Method;
				return this;
			}

			public MethodEnumeration Catch<TException>(CatchMethodAction<TException> @catch)
			{
				foreach (Amendment.Method method in this)
					method.CatchMethod = @catch.Method;
				return this;
			}

			public MethodEnumeration Catch<TException>(CatchMethodFunc<TException> @catch)
			{
				foreach (Amendment.Method method in this)
					method.CatchMethod = @catch.Method;
				return this;
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return methods.GetEnumerator();
			}

			#endregion
		}	

		#endregion

		#region MethodEnumeration<TMethod, TEnumeration>

		public partial class MethodEnumeration<TMethod, TEnumeration> : MemberEnumeration<TEnumeration>, IEnumerable
			where TMethod : Amendment.Method
			where TEnumeration : MethodEnumeration<TMethod, TEnumeration>, new()
		{
			protected IEnumerable<TMethod> methods;

			/// <summary>
			/// Gets all methods in the set with the specified name.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="methods"></param>
			/// <param name="name"></param>
			/// <returns></returns>
			public TEnumeration Named(string name)
			{
				return new TEnumeration() { methods = methods.Where(m => m.Name == name) };
			}

			/// <summary>
			/// Gets the set of methods that match the specified criteria.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public TEnumeration Where(Func<TMethod, bool> predicate)
			{
				return new TEnumeration() { methods = methods.Where(predicate) };
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return methods.GetEnumerator();
			}
		}

		#endregion
	}
}
