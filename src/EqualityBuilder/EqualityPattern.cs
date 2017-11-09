using System;
using System.Reflection;
using EqualityBuilder.Core;
using EqualityBuilder.Impl;

namespace EqualityBuilder
{
    public class EqualityPattern
    {
        public static IEqualityGenerator FromPublicProperties(Type type)
        {
            return FromPublicPropertiesImpl(type);
        }

        public static IEqualityGenerator<T> FromPublicProperties<T>()
        {
            var generator = FromPublicPropertiesImpl(typeof(T));
            return new PropertiesEqualityGenerator<T>(generator);
        }

        private static PropertiesEqualityGenerator FromPublicPropertiesImpl(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return new PropertiesEqualityGenerator(properties);
        }
    }
}