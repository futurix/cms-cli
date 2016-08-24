using System.Data.Linq;

namespace Wave.Services
{
    public class CacheDataContext : DataContext
    {
        public CacheDataContext(string connectionString)
            : base(connectionString)
        {
        }

        public Table<DBCacheRecord> ServerCache
        {
            get { return GetTable<DBCacheRecord>(); }
        }
    }
}
