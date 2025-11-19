namespace GateEntryExit.Caching
{
    public class CachedDataProvider : ICachedDataProvider
    {
        private readonly CachedData _cachedData;

        public CachedDataProvider(CachedData cachedData)
        {
            _cachedData = cachedData;
        }

        public CachedData GetCachedData() => _cachedData;
    }
}
