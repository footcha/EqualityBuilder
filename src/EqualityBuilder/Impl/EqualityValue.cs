using System;
using EqualityBuilder.Core;

namespace EqualityBuilder.Impl
{
    public class EqualityValue : IEqualityValue
    {
        private readonly object thisObject;

        private readonly Func<object, object, bool> comparer;

        public EqualityValue(object thisObject, int hashCode, Func<object, object, bool> comparer)
        {
            this.thisObject = thisObject;
            this.comparer = comparer;
            HashCode = hashCode;
        }

        public int HashCode { get; }

        public bool IsEqual(object obj)
        {
            return comparer(thisObject, obj);
        }
    }
}