using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataErrorInfo
{
    [CLSCompliant(false)]
    public static class DataErrorInfoAmender<T> where T : class
    {
        public static string ValidateProperty(IDataErrorInfo instance, string propertyName)
        {
            var prop = instance.GetType().GetProperty(propertyName);
            var validators = prop.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
            return (from v in validators where !v.IsValid(prop.GetValue(instance, null)) select v.FormatErrorMessage(propertyName)).FirstOrDefault();
        }

        public static string GetError(IDataErrorInfo instance, string propertyname)
        {
            return null;
        }
    }
}
