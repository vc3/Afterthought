using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHandler.UnitTest.Target
{
	[HandleException]
	public class TestExceptionHandler
	{
		public void ThrowExceptionVoidMethod()
		{
			throw new ArgumentException();
		}

		public void ThrowExceptionVoidMethod(int a, int b, int c)
		{
			throw new ArgumentException();
		}

		bool bTry = false;
		public int ThrowExceptionRetMethod()
		{
			if (!bTry)
			{
				try
				{

					throw new ArgumentException();
				}
				catch (Exception ex)
				{
					return 10;
				}
			}
			else
			return 10;
		}

		public int ThrowExceptionRetMethodWithArgs(int a, int b, int c)
		{
			if (!bTry)
			{
				throw new ArgumentException();

			}
			else
				return 10;
		}

		public int ThrowExceptionRetMethodWithArgsGeneric<T>(T a, int b, int c)
		{
			if (!bTry)
			{
				throw new ArgumentException();
			}
			else
			{
				return 10;
			}
		}

	}
	
    
}
