using Afterthought;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ExceptionHandler
{
    public class HandleExceptionAttribute : Attribute
    {
    }
}

namespace ExceptionHandler.Amender
{


	public class HandleExceptionAmdendment<T> : Afterthought.Amendment<T, T>
	{
		public HandleExceptionAmdendment()
		{

			var voidMethods = from m in Methods where m.ReturnType == typeof(void) select m;
			var retMethods = from m in Methods where m.ReturnType != typeof(void) select m;


			voidMethods.Before(HandleExceptionAmdendment<T>.BeforeAction);
			voidMethods.After(AfterAction);
			voidMethods.Catch<Exception>(
				new Amendment<T, T>.MethodEnumeration.CatchMethodAction<Exception>
				(HandleExceptionAmdendment<T>.CatchAction)
				);



			retMethods.Before(HandleExceptionAmdendment<T>.BeforeAction);
			retMethods.After(AfterFunc);
			retMethods.Catch<Exception>(
				new Amendment<T, T>.MethodEnumeration.CatchMethodFunc<Exception>
				(HandleExceptionAmdendment<T>.CatchFunc)
				);

			//this.Methods.Finally(HandleExceptionAmdendment<T>.FinallyAction);



		}


		public static object CatchFunc(T instance, string method, Exception exception, object[] parameters)
		{
			System.Console.WriteLine("Exception throuwn " + exception.GetType().FullName);


			//System.Console.ReadLine();
			return 10;
			//return true;
		}
		public static void CatchAction(T instance, string method, Exception exception, object[] parameters)
		{
			System.Console.WriteLine("Exception throuwn " + exception.GetType().FullName);


			//System.Console.ReadLine();
			//return 10;
			//return true;
		}

		public static void BeforeAction(T instance, string method, object[] parameters)
		{
			System.Console.WriteLine("Before method " + method);
			//System.Console.ReadLine();
		}
		

		public static void AfterAction(T instance, string method, object[] parameters)
		{
			System.Console.WriteLine("after method " + method);
			//System.Console.ReadLine();
			//return true;
		}

		public static object AfterFunc(T instance, string method ,object[] parameters, object result)
		{
			System.Console.WriteLine("after method " + method);
			//System.Console.ReadLine();
			return 8;
		}

		public static void FinallyAction(T instance, string method, object[] parameters)
		{
			System.Console.WriteLine("finally of " + method);
			//System.Console.ReadLine();
		}
	}

    [AttributeUsage(AttributeTargets.Assembly)]
    public class HandleExceptionAmender : Attribute, IAmendmentAttribute
    {
        public IEnumerable<ITypeAmendment> GetAmendments(Type target)
        {
            var hasHandleExceptionAttrib = 
                target.GetCustomAttributes(typeof(HandleExceptionAttribute), true).Count() > 0;
			if(hasHandleExceptionAttrib)
            yield return (ITypeAmendment)
                (  
                typeof(HandleExceptionAmdendment<>).MakeGenericType(target).
                GetConstructor(System.Type.EmptyTypes).Invoke(new object[0]) 
                );

        }
    }
}
