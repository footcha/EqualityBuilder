using System.Collections.Generic;
using EqualityBuilder.Core;

namespace EqualityBuilder.Impl
{
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly IEqualityGenerator<T> generator;

        public EqualityComparer(IEqualityGenerator<T> generator)
        {
            this.generator = generator;
        }

        public bool Equals(T x, T y)
        {
            return generator.AreEqual(x, y);
        }

        public int GetHashCode(T obj)
        {
            return generator.GenerateHashCode(obj);
        }
    }
}