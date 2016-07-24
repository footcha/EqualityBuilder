using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using EqualityBuilder.Core;

namespace EqualityBuilder.Impl
{
    public class PropertiesEqualityGenerator : IEqualityGenerator
    {
        private readonly Func<object, object, bool> compiledEquality;
        private readonly Func<object, int> hashCodeGenerator;

        public PropertiesEqualityGenerator(IReadOnlyList<PropertyInfo> properties)
        {
            compiledEquality = CreateEqualityComparer(properties);
            hashCodeGenerator = CreateHashCodeGenerator(properties);
        }

        public int GenerateHashCode(object @object)
        {
            return hashCodeGenerator(@object);
        }

        public bool AreEqual(object obj1, object obj2)
        {
            return compiledEquality(obj1, obj2);

        }

        public IEqualityValue CreateValue(object obj)
        {
            return new EqualityValue(obj, GenerateHashCode(obj), AreEqual);
        }

        //protected bool Equals(TestValue other)
        //{
        //    return Property1 == other.Property1 && Property2 == other.Property2 && Property3 == other.Property3 && Property4 == other.Property4;
        //}
        private static Func<object, object, bool> CreateEqualityComparer(IReadOnlyList<PropertyInfo> properties)
        {
            var prop1 = properties[0];

            var equalsExprs = new List<MethodCallExpression>();

            var p1 = Expression.Parameter(typeof(object), "obj1");
            var p2 = Expression.Parameter(typeof(object), "obj2");
            foreach (var propertyInfo in properties)
            {
                var parameterExp1 = Expression.Convert(p1, prop1.DeclaringType);
                var parameterExp2 = Expression.Convert(p2, prop1.DeclaringType);
                var propertyExp1 = Expression.Property(parameterExp1, propertyInfo.Name);
                var propertyExp2 = Expression.Property(parameterExp2, propertyInfo.Name);
                var propertyExp2casted = Expression.Convert(propertyExp2, typeof(object));
                // TODO check null
                var equalsPropertyExp = Expression.Call(propertyExp1, propertyInfo.PropertyType.GetMethod("Equals", new[] { typeof(object) }), propertyExp2casted);

                equalsExprs.Add(equalsPropertyExp);
            }

            Expression e1 = equalsExprs[0];
            for (var i = 1; i < equalsExprs.Count; ++i)
            {
                var e2 = equalsExprs[i];
                var and = Expression.AndAlso(e1, e2);
                e1 = and;
            }

            return Expression.Lambda<Func<object, object, bool>>(e1, p1, p2).Compile();
        }

        // unchecked
        // {
        //     var hashCode = Property1.GetHashCode();
        //     hashCode = (hashCode * 397) ^ Property2.GetHashCode();
        //     hashCode = (hashCode * 397) ^ Property3.GetHashCode();
        //     hashCode = (hashCode * 397) ^ Property4.GetHashCode();
        //     return hashCode;
        // }
        private static Func<object, int> CreateHashCodeGenerator(IReadOnlyList<PropertyInfo> properties)
        {
            var prop1 = properties[0];

            var callExpressions = new List<MethodCallExpression>();

            var p1 = Expression.Parameter(typeof(object), "obj1");
            foreach (var propertyInfo in properties)
            {
                var parameterExp1 = Expression.Convert(p1, prop1.DeclaringType);
                var propertyExp1 = Expression.Property(parameterExp1, propertyInfo.Name);
                // TODO check null
                var getHashCodePropertyExp = Expression.Call(propertyExp1, propertyInfo.PropertyType.GetMethod("GetHashCode"));

                callExpressions.Add(getHashCodePropertyExp);
            }

            var constNumber = Expression.Constant(397);
            Expression e1 = callExpressions[0];
            for (var i = 1; i < callExpressions.Count; ++i)
            {
                var e2 = callExpressions[i];
                var multiply = Expression.Multiply(e1, constNumber);
                var newHash = Expression.ExclusiveOr(multiply, e2);

                e1 = newHash;
            }

            return Expression.Lambda<Func<object, int>>(e1, p1).Compile();
        }
    }
}