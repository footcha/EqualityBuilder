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
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            return new PropertiesEqualityGenerator(properties);
        }
    }
}