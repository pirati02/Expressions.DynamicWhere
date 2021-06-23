using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expression.Extensions
{
    public static class WhereExtensions
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, object term,
            IEnumerable<string> keyProperties)
        {
            return new EnumerableWhere<T>(source, term, keyProperties).Where();
        }

        public static IEnumerable<T> Filter<T, TResult>(this IEnumerable<T> source, object term,
            params Expression<Func<T, TResult>>[] keyProperties)
        {
            var propertiesList = keyProperties.Select(GetLambdaReturnPropertyName).ToList();

            return new EnumerableWhere<T>(source, term, propertiesList).Where();
        }

        private static string GetLambdaReturnPropertyName<T, TResult>(Expression<Func<T, TResult>> property)
        {
            var member = property.Body as MemberExpression;
            var propInfo = member!.Member as PropertyInfo;
            return propInfo?.Name;
        }
    }
}