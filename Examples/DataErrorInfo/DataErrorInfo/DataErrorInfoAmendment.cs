using System;
using System.ComponentModel;
using Afterthought;

namespace DataErrorInfo
{
    [CLSCompliant(false)]
    public class DataErrorInfoAmendment<T> : Amendment<T, IDataErrorInfo> where T : class
    {
		public DataErrorInfoAmendment()
		{
		    Implement<IDataErrorInfo>(
                Properties.Add<string>("Error", DataErrorInfoAmender<T>.GetError),
                Methods.AddPropertyIndexerGetter<string, string>(DataErrorInfoAmender<T>.ValidateProperty)
		    );
		}
    }
}

