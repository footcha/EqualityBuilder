namespace EqualityBuilder.Core
{
    public interface IEqualityGenerator
    {
        int GenerateHashCode(object @object);
        bool AreEqual(object obj1, object obj2);
        IEqualityValue CreateValue(object obj);
    }
}