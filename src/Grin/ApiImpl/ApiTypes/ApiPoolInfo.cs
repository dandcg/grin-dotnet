namespace Grin.Api.ApiTypes
{
    public class ApiPoolInfo
    {
        /// Size of the pool
        public uint pool_size { get; set; }

        /// Size of orphans
        public uint orphans_size { get; set; }

        /// Total size of pool + orphans
        public uint total_size { get; set; }
    }
}