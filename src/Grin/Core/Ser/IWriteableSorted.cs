namespace Grin.Core.Ser
{
    public interface IWriteableSorted
    {
        void write_sorted(IWriter writer);
    }
}