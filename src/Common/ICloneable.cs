namespace Common
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
