namespace Grin.CoreImpl.Ser
{
    public interface IWriteableSorted
    {
        void write_sorted(IWriter writer);
    }
}