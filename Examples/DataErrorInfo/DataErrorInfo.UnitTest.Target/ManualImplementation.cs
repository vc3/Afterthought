using System.ComponentModel;

namespace DataErrorInfo.UnitTest.Target
{
    public class ManualImplementation : IDataErrorInfo
    {
        string IDataErrorInfo.this[string columnName]
        {
            get { return columnName; }
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }
    }
}
