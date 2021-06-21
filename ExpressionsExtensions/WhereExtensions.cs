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
            if (term is null)
                return Enumerable.Empty<T>();

            source = source.ToList();
            var sourceParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var termConstant = System.Linq.Expressions.Expression.Constant(term);
            var propertiesList =
                MapKeyPropertiesToMemberExpressions(keyProperties, sourceParameterExpression);

            var containsMethod = GetContainsMethod();


            System.Linq.Expressions.Expression orExpression = null;
            for (var index = 0; index < propertiesList.Count; index += 2)
            {
                var shouldAddExpression = index > 0 || index < propertiesList.Count - 1;
                if (!shouldAddExpression) continue;

                var property = propertiesList[index];
                var property1 = index + 1 >= propertiesList.Count ? null : propertiesList[index + 1];

                var leftExpression =
                    GetProperExpression<T>(property, termConstant, containsMethod);
                var rightExpression =
                    GetProperExpression<T>(property1, termConstant, containsMethod);

                orExpression = AddOrCreateOrExpression(leftExpression, rightExpression, orExpression);
            }

            if (orExpression == null)
                return Enumerable.Empty<T>();

            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(orExpression, sourceParameterExpression);
            var res = source.Where(lambda.Compile());

            return res;
        }

        private static List<MemberExpression> MapKeyPropertiesToMemberExpressions(IEnumerable<string> keyProperties, ParameterExpression sourceParameterExpression)
        {
            return keyProperties
                .Where(a => !string.IsNullOrEmpty(a))
                .Select(property =>
                    System.Linq.Expressions.Expression.PropertyOrField(sourceParameterExpression, property))
                .ToList();
        }

        private static MethodInfo GetContainsMethod()
        {
            var containsMethod = typeof(string).GetMethods()!
                .FirstOrDefault(a => a.Name == "Contains" && a.GetParameters().Length == 1)!;
            return containsMethod;
        }

        private static System.Linq.Expressions.Expression AddOrCreateOrExpression(
            System.Linq.Expressions.Expression leftExpression,
            System.Linq.Expressions.Expression rightExpression,
            System.Linq.Expressions.Expression orExpression
        )
        {
            if (leftExpression == null) return orExpression;

            if (rightExpression != null)
            {
                if (orExpression == null)
                {
                    orExpression = System.Linq.Expressions.Expression.Or(leftExpression, rightExpression);
                    return orExpression;
                }

                var addedOrExpression =
                    System.Linq.Expressions.Expression.Or(leftExpression, rightExpression);
                orExpression = System.Linq.Expressions.Expression.Or(orExpression, addedOrExpression);
                return orExpression;
            }

            orExpression ??= leftExpression;
            return orExpression;
        }

        private static System.Linq.Expressions.Expression GetProperExpression<T>(
            System.Linq.Expressions.Expression property,
            System.Linq.Expressions.Expression termConstant,
            MethodInfo containsMethod)
        {
            if (property == null)
                return null;

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
                    searchExpression =
                        System.Linq.Expressions.Expression.Call(property, containsMethod!, termConstant);
                }
                //and for another reference type equals
                else if (property.Type == termConstant.Type)
                {
                    var equalsMethod = property.Type.GetMethods()
                        .FirstOrDefault(a => a.GetParameters().Length == 1 && a.Name == "Equals");
                    var equalsMethodExpression =
                        System.Linq.Expressions.Expression.Call(property, equalsMethod!, termConstant);

                    var notNullExpression = System.Linq.Expressions.Expression.NotEqual(property,
                        System.Linq.Expressions.Expression.Constant(null));
                    searchExpression =
                        System.Linq.Expressions.Expression.AndAlso(notNullExpression, equalsMethodExpression);
                }
            }

            return searchExpression;
        }
    }
}