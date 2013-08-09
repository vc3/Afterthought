using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace DataErrorInfo
{
    public static class DataErrorInfoAmender<T> where T : class
    {
        private static readonly IDictionary<string, ValidationAttribute[]> PropertyToValidatorsDictionary = new Dictionary<string, ValidationAttribute[]>();
        private static readonly IDictionary<string, Func<T, object>> PropertyToGetterDictionary = new Dictionary<string, Func<T, object>>();

        static DataErrorInfoAmender()
        {
            var amendedType = typeof(T);
            foreach (var propertyInfo in amendedType.GetProperties())
            {
                var validators = propertyInfo.GetCustomAttributes(typeof(ValidationAttribute), true) as ValidationAttribute[];
                if (validators == null || validators.Length <= 0) continue;

                PropertyToValidatorsDictionary[propertyInfo.Name] = validators;

                var indexParam = propertyInfo.GetIndexParameters();
                if (indexParam.Length != 0) continue;
                var expressionParameter = Expression.Parameter(amendedType, "t");
                var cast = Expression.TypeAs(Expression.Property(expressionParameter, propertyInfo), typeof(object));
                PropertyToGetterDictionary[propertyInfo.Name] = Expression.Lambda(cast, expressionParameter).Compile() as Func<T, object>;

                //indexer case
                //{ 
                //    var accessor = propertyInfo.GetGetMethod(); 
                //    var instance = Expression.Parameter(typeToProxy, "t"); 
                //    var index = Expression.Parameter(indexParam[0].ParameterType, "i"); 
                //    var exp = Expression.Call(instance, accessor, index); 
                //    var cast = Expression.TypeAs(exp, typeof (object)); 
                //    getter = Expression.Lambda(cast, instance, index).Compile(); 
                //}
            }
        }

        public static string ValidateProperty(T instance, string propertyName)
        {
            ValidationAttribute[] validators;
            if (PropertyToValidatorsDictionary.TryGetValue(propertyName, out validators))
            {
                var getter = PropertyToGetterDictionary[propertyName];
                var value = getter(instance);
                var errors = validators.Where(v => !v.IsValid(value)).Select(v => v.FormatErrorMessage(propertyName)).ToArray();
                return string.Join(Environment.NewLine, errors);
            }
            return null;
        }
    }
}
