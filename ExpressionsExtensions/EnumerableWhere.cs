using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Expression.Extensions
{
    public class EnumerableWhere<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly object _term;
        private readonly IEnumerable<string> _keyProperties;

        public EnumerableWhere(IEnumerable<T> source, object term,
            IEnumerable<string> keyProperties)
        {
            _source = source;
            _term = term;
            _keyProperties = keyProperties;
        }

        public IEnumerable<T> Where()
        {
            switch (_term)
            {
                case null:
                    return Enumerable.Empty<T>();
                case string termString when string.IsNullOrEmpty(termString):
                    return Enumerable.Empty<T>();
            }

            var source = _source.ToList();
            var sourceParameterExpression = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var termConstant = System.Linq.Expressions.Expression.Constant(_term);
            var propertiesList =
                MapKeyPropertiesToMemberExpressions<T>(_keyProperties, sourceParameterExpression);

            var containsMethod = GetContainsMethod();


            System.Linq.Expressions.Expression orExpression = null;
            for (var index = 0; index < propertiesList.Count; index += 2)
            {
                var addOrExpression = index < propertiesList.Count - 1;
                if (!addOrExpression)
                {
                    var property = propertiesList[index];
                    if (orExpression != null)
                    {
                        var rightExpression = GetProperExpression(property, termConstant, containsMethod);
                        if (rightExpression != null)
                        {
                            orExpression = System.Linq.Expressions.Expression.Or(orExpression, rightExpression);
                        }

                        continue;
                    }

                    orExpression = GetProperExpression(property, termConstant, containsMethod);
                }
                else
                {
                    var property = propertiesList[index];
                    var property1 = index + 1 >= propertiesList.Count ? null : propertiesList[index + 1];

                    var leftExpression =
                        GetProperExpression(property, termConstant, containsMethod);
                    var rightExpression =
                        GetProperExpression(property1, termConstant, containsMethod);
                    if (leftExpression == null || rightExpression == null)
                    {
                        orExpression = leftExpression ?? rightExpression;
                        continue;
                    }

                    orExpression = AddOrCreateOrExpression(leftExpression, rightExpression, orExpression);
                }
            }

            if (orExpression == null)
                return Enumerable.Empty<T>();

            var lambda =
                System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(orExpression, sourceParameterExpression);
            var res = source.Where(lambda.Compile());

            return res;
        }

        private static List<MemberExpression> MapKeyPropertiesToMemberExpressions<T>(IEnumerable<string> keyProperties,
            System.Linq.Expressions.Expression sourceParameterExpression)
        {
            var reflectedType = typeof(T);
            return keyProperties
                .Where(a => !string.IsNullOrEmpty(a) &&
                            reflectedType.GetProperties().Any(p => p.Name == a))
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
            // if (leftExpression == null) return orExpression;
            // if (rightExpression == null) return orExpression;
            
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

        private static System.Linq.Expressions.Expression GetProperExpression(
            System.Linq.Expressions.Expression property,
            System.Linq.Expressions.Expression termConstant,
            MethodInfo containsMethod)
        {
            if (property == null)
                return null;

            if (
                property.Type.IsValueType && termConstant.Type.IsValueType
                                          && property.Type == termConstant.Type
            )
            {
                return System.Linq.Expressions.Expression.Equal(property, termConstant);
            }

            //for string contains
            if (property.Type == typeof(string) && termConstant.Type == typeof(string))
            {
                return
                    System.Linq.Expressions.Expression.Call(property, containsMethod!, termConstant);
            }
            //and for another reference type equals

            if (property.Type != termConstant.Type) return null;
            var equalsMethod = property.Type.GetMethods()
                .FirstOrDefault(a => a.GetParameters().Length == 1 && a.Name == "Equals");
            var equalsMethodExpression =
                System.Linq.Expressions.Expression.Call(property, equalsMethod!, termConstant);

            var notNullExpression = System.Linq.Expressions.Expression.NotEqual(property,
                System.Linq.Expressions.Expression.Constant(null));
            return
                System.Linq.Expressions.Expression.AndAlso(notNullExpression, equalsMethodExpression);
        }
    }
}