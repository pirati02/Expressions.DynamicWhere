using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Extensions
{
    public static class WhereExtensions
    {
        
        public static IEnumerable<T> DynamicWhere<T>(this IEnumerable<T> source, object term,
            IEnumerable<string> keyProperties)
        {
            if (term is null)
                return Enumerable.Empty<T>();

            source = source.ToList();
            var sourceParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var termConstant = System.Linq.Expressions.Expression.Constant(term);
            var propertiesList =
                keyProperties
                    .Where(a => !string.IsNullOrEmpty(a))
                    .Select(property => System.Linq.Expressions.Expression.PropertyOrField(sourceParameterExpression, property))
                    .ToList();

            var containsMethod = typeof(string).GetMethods()!
                .FirstOrDefault(a => a.Name == "Contains" && a.GetParameters().Length == 1)!;

            var backingResult = Enumerable.Empty<T>().ToList();

            foreach (var property in propertiesList)
            {
                System.Linq.Expressions.Expression searchExpression = null;
                if (
                    property.Type.IsValueType && termConstant.Type.IsValueType
                                              && property.Type == termConstant.Type
                )
                {
                    searchExpression = System.Linq.Expressions.Expression.Equal(property, termConstant);
                }
                else
                {
                    //for string contains
                    if (property.Type == typeof(string) && termConstant.Type == typeof(string))
                    {
                        searchExpression = System.Linq.Expressions.Expression.Call(property, containsMethod!, termConstant);
                    }
                    //and for another reference type equals
                    else if (property.Type == termConstant.Type)
                    {
                        var equalsMethod = property.Type.GetMethods()
                            .FirstOrDefault(a => a.GetParameters().Length == 1 && a.Name == "Equals");
                        var equalsMethodExpression = System.Linq.Expressions.Expression.Call(property, equalsMethod!, termConstant);

                        var notNullExpression = System.Linq.Expressions.Expression.NotEqual(property, System.Linq.Expressions.Expression.Constant(null));
                        searchExpression = System.Linq.Expressions.Expression.AndAlso(notNullExpression, equalsMethodExpression);
                    }
                }

                if (searchExpression == null) continue;

                var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(searchExpression!, sourceParameterExpression);
                var res = source.Where(lambda.Compile());
                var except = res.Where(a => !backingResult.Contains(a));
                backingResult.AddRange(except!);
            }

            return backingResult;
        }
    }
}