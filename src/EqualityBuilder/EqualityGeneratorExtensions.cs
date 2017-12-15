using EqualityBuilder.Core;
using EqualityBuilder.Impl;

namespace EqualityBuilder
{
    public static class EqualityGeneratorExtensions
    {
        public static EqualityComparer<T> ToComparer<T>(this IEqualityGenerator<T> generator)
        {
            return new EqualityComparer<T>(generator);
        }
    }
}