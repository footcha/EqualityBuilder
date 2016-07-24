namespace EqualityBuilder.Core
{
    public interface IEqualityValue
    {
        int HashCode { get; }
        bool IsEqual(object obj);
    }
}