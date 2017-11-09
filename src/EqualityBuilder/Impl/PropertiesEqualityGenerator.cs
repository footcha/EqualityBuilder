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
            var declaringType = properties[0].DeclaringType;

            var equalsExprs = new List<Expression>();

            var p1 = Expression.Parameter(typeof(object), "obj1");
            var p2 = Expression.Parameter(typeof(object), "obj2");
            var p1Casted = Expression.Convert(p1, declaringType);
            var var1 = Expression.Variable(declaringType, "obj1Casted");
            var assign1 = Expression.Assign(var1, p1Casted);
            var p2Casted = Expression.Convert(p2, declaringType);
            var var2 = Expression.Variable(declaringType, "obj2Casted");
            var assign2 = Expression.Assign(var2, p2Casted);

            foreach (var propertyInfo in properties)
            {
                var propertyExp1 = Expression.Property(var1, propertyInfo.Name);
                var propertyExp2 = Expression.Property(var2, propertyInfo.Name);
                var propertyExp2casted = Expression.Convert(propertyExp2, typeof(object));

                var equalsPropertyExp = Expression.Call(propertyExp1, propertyInfo.PropertyType.GetMethod("Equals", new[] { typeof(object) }), propertyExp2casted);
                if (propertyInfo.PropertyType.IsValueType)
                {
                    equalsExprs.Add(equalsPropertyExp);
                }
                else
                {
                    var @null = Expression.Constant(null);
                    var isNull = Expression.ReferenceEqual(propertyExp1, @null);
                    var g = Expression.Condition(isNull,
                        Expression.ReferenceEqual(propertyExp2, @null),
                        equalsPropertyExp);
                    equalsExprs.Add(g);
                }
            }

            var equals = equalsExprs[0];
            for (var i = 1; i < equalsExprs.Count; ++i)
            {
                var e2 = equalsExprs[i];
                var and = Expression.AndAlso(equals, e2);
                equals = and;
            }

            var final = Expression.Block(
                new[] { var1, var2 },
                assign1,
                assign2,
                equals
            );

            var compile = Expression.Lambda<Func<object, object, bool>>(final, p1, p2).Compile();

            return (a, b) => b != null && declaringType.IsInstanceOfType(b) && compile(a, b);
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

            var zero = Expression.Constant(0);
            var @null = Expression.Constant(null);

            var callExpressions = new List<Expression>();

            var p1 = Expression.Parameter(typeof(object), "obj1");
            var p1Casted = Expression.Convert(p1, prop1.DeclaringType);
            var var1 = Expression.Variable(prop1.DeclaringType, "obj1Casted");
            var assign = Expression.Assign(var1, p1Casted);
            foreach (var propertyInfo in properties)
            {
                //var propertyExp1 = Expression.Property(p1Casted, propertyInfo.Name);
                var propertyExp1 = Expression.Property(var1, propertyInfo.Name);

                var getHashCodePropertyExp = Expression.Call(propertyExp1, propertyInfo.PropertyType.GetMethod("GetHashCode"));
                if (propertyInfo.PropertyType.IsValueType)
                {
                    callExpressions.Add(getHashCodePropertyExp);
                }
                else
                {
                    var isNull = Expression.ReferenceEqual(propertyExp1, @null);
                    var g = Expression.Condition(isNull, zero, getHashCodePropertyExp);

                    callExpressions.Add(g);
                }
            }

            var constNumber = Expression.Constant(397);
            var hash = callExpressions[0];
            for (var i = 1; i < callExpressions.Count; ++i)
            {
                var multiply = Expression.Multiply(hash, constNumber);
                var e2 = callExpressions[i];
                var newHash = Expression.ExclusiveOr(multiply, e2);

                hash = newHash;
            }

            var final = Expression.Block(
                new[] { var1 },
                assign,
                hash);
            return Expression.Lambda<Func<object, int>>(final, p1).Compile();
        }
    }
}