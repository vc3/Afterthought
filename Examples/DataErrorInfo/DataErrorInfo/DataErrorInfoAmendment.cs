using System;
using System.ComponentModel;
using Afterthought;

namespace DataErrorInfo
{
    public class DataErrorInfoAmendment<T> : Amendment<T, T> where T : class
    {
		public DataErrorInfoAmendment()
		{
		    Implement<IDataErrorInfo>(
                Properties.Add<string>("Error", (instance, name) => null),
                Methods.AddPropertyIndexerGetter<string, string>(DataErrorInfoAmender<T>.ValidateProperty)
		    );
		}
    }
}

