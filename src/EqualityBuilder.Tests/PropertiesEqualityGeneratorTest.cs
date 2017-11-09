using System.Reflection;
using EqualityBuilder.Core;
using Xunit;

namespace EqualityBuilder.Tests
{
    public class PropertiesEqualityGeneratorTest
    {
        private class ValueObject
        {
            private static readonly IEqualityGenerator EqualityGenerator = EqualityPattern.FromPublicProperties(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IEqualityValue equalityValue;

            public ValueObject(bool property1, string property2, decimal property3, float property4)
            {
                Property1 = property1;
                Property2 = property2;
                Property3 = property3;
                Property4 = property4;
                equalityValue = EqualityGenerator.CreateValue(this);
            }

            public bool Property1 { get; }

            public string Property2 { get; }

            public decimal Property3 { get; }

            public float Property4 { get; }

            public override bool Equals(object obj)
            {
                return equalityValue.IsEqual(obj);
            }

            public override int GetHashCode()
            {
                return equalityValue.HashCode;
            }

            public int GetHashCodeManuallyImplemented()
            {
                unchecked
                {
                    var hashCode = Property1.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Property2?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 397) ^ Property3.GetHashCode();
                    hashCode = (hashCode * 397) ^ Property4.GetHashCode();
                    return hashCode;
                }
            }

            public bool EqualsManuallyImplemented(object obj)
            {
                var other = (ValueObject)obj;
                return Property1.Equals(other.Property1)
                    && (Property2 != null && Property2.Equals(other.Property2) || Property2 == null && other.Property2 == null)
                    && Property3.Equals(other.Property3)
                    && Property4.Equals(other.Property4);
            }
        }

        [Fact]
        public void Equality()
        {
            // arrange
            var x1 = new ValueObject(true, "a", 10, 2f);
            var x2 = new ValueObject(true, "a", 10, 2f);
            var y1 = new ValueObject(true, "b", 10, 12f);

            // act & assert
            Assert.Equal(x1.GetHashCodeManuallyImplemented(), x1.GetHashCode());
            Assert.Equal(x2.GetHashCodeManuallyImplemented(), x2.GetHashCode());
            Assert.Equal(y1.GetHashCodeManuallyImplemented(), y1.GetHashCode());

            Assert.True(x1.Equals(x2));
            Assert.False(x1.Equals(y1));
            Assert.False(x1.Equals(new object()));
            Assert.False(x1.Equals(null));
        }

        [Fact]
        public void EqualityWithNullProperties()
        {
            // arrange
            var x1 = new ValueObject(true, "a", 10, 12f);
            var x2 = new ValueObject(true, "a", 10, 12f);
            var y1 = new ValueObject(true, null, 10, 12f);
            var y2 = new ValueObject(true, null, 10, 12f);

            // act & assert
            Assert.Equal(x1, x2);
            Assert.Equal(x2, x1);
            Assert.NotEqual(x1, y1);
            Assert.NotEqual(y1, x1);
            Assert.Equal(y1, y2);
            Assert.Equal(y2, y1);
            Assert.NotEqual(y1, x1);
        }

        [Fact]
        public void Performance()
        {
            var x1 = new ValueObject(true, "a", 10, 2f);
            var x2 = new ValueObject(true, "a", 10, 2f);
            var y1 = new ValueObject(true, "b", 10, 12f);

            for (int i = 0; i < 1000000; i++)
            {
                var a = x1.Equals(x2);
                var b = x1.Equals(y1);
            }
        }


        // hash code from propertyInfo:
        // unchecked
        // {
        //     var hashCode = properties.First().GetValue(@object).GetHashCode();
        //     for (var i = 1; i < properties.Count(); ++i)
        //     {
        //         var propertyInfo = properties[i];
        //         hashCode = (hashCode * 397) ^ propertyInfo.GetValue(@object).GetHashCode(); // TODO check null
        //     }
        //     if (hashCode != hash1)
        //     {
        //         throw new Exception("Hash does not match.");
        //     }
        //     return hash1;
        // }

        // Are equal from propertyInfo
        // foreach (var propertyInfo in properties)
        // {
        //     var value1 = propertyInfo.GetValue(obj1);
        //     var value2 = propertyInfo.GetValue(obj2);

        //     // TODO check null
        //     if (false == value1.Equals(value2))
        //     {
        //         return false;
        //     }
        // }
        // return true;
    }
}