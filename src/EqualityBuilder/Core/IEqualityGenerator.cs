namespace EqualityBuilder.Core
{
    public interface IEqualityGenerator
    {
        int GenerateHashCode(object @object);
        bool AreEqual(object obj1, object obj2);
        IEqualityValue CreateValue(object @object);
    }

    public interface IEqualityGenerator<in T>
    {
        int GenerateHashCode(T @object);
        bool AreEqual(T obj1, object obj2);
        IEqualityValue CreateValue(T @object);
    }
}