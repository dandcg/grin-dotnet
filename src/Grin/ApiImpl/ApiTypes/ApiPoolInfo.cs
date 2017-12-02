namespace Grin.ApiImpl.ApiTypes
{
    public class ApiPoolInfo
    {
        /// Size of the pool
        public uint PoolSize { get; set; }

        /// Size of orphans
        public uint OrphansSize { get; set; }

        /// Total size of pool + orphans
        public uint TotalSize { get; set; }
    }
}