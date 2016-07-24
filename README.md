# EqualityBuilder
Generator for methods `Equal` and `GetHashCode`.

## Examples
1. **Value object with public properties**

   All properties are considered when generating methods `Equal` and `GetHashCode`:
   ```csharp
   public class ValueObject
   {
       private static readonly IEqualityGenerator EqualityGenerator = EqualityPattern.FromPublicProperties(MethodBase.GetCurrentMethod().DeclaringType);
   
       private readonly IEqualityValue equalityValue;
   
       public ValueObject(bool property1, int property2, decimal property3, float property4)
       {
           Property1 = property1;
           Property2 = property2;
           Property3 = property3;
           Property4 = property4;
           equalityValue = EqualityGenerator.CreateValue(this);
       }
   
       public bool Property1 { get; }
   
       public int Property2 { get; }
   
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
   }
   ```
