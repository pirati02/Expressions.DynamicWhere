using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expression.Extensions
{
    public static class WhereExtensions
    {
        public static IEnumerable<T> DynamicWhere<T>(this IEnumerable<T> source, object term,
            IEnumerable<string> keyProperties)
        {
            return new EnumerableWhere<T>(source, term, keyProperties).Where();
        }

        public static IEnumerable<T> DynamicWhere<T, TResult>(this IEnumerable<T> source, object term,
            params Expression<Func<T, TResult>>[] keyProperties)
        {
            var propertiesList = keyProperties.Select(GetLambdaReturnPropertyName).ToList();

            return new EnumerableWhere<T>(source, term, propertiesList).Where();
        }

        private static string GetLambdaReturnPropertyName<T, TResult>(Expression<Func<T, TResult>> property)
        {
            var type = typeof(T);

            if (property.Body is not MemberExpression member)
                throw new ArgumentException(
                    $"Expression '{property}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(
                    $"Expression '{property}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(
                    $"Expression '{property}' refers to a property that is not from type {type}.");

            return propInfo.Name;
        }
    }
}