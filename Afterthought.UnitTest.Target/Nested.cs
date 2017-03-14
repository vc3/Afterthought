using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Afterthought.UnitTest.Target
{
	public class Nested
	{
		[Amendment(typeof(TestNestedAmendment<>))]
		public class Example
		{
			public Example()
			{
				
			}

			public int Result { get; set; }

			public int Add(int x, int y)
			{
				return 0;
			}
		}
	}
}
