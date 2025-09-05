using StackExchange.Redis;
using System.Text.Json;

namespace GateEntryExit.Service.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _cacheDb;
        private readonly IServer _server;

        public CacheService()
        {
            ConfigurationOptions option = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ConnectTimeout = 30000,
                EndPoints = { { "localhost", 6379 } }
            };
            var connectionMultiplexer = ConnectionMultiplexer.Connect(option);
            _cacheDb = connectionMultiplexer.GetDatabase();
            _server = connectionMultiplexer.GetServer("localhost:6379");
        }

        public T GetData<T>(string key)
        {
            var redisValue = _cacheDb.StringGet(key);

            if (!string.IsNullOrEmpty(redisValue))
                return JsonSerializer.Deserialize<T>(redisValue);

            return default;
        }

        public void RemoveData(string key)
        {
            if (_cacheDb.KeyExists(key))
                _cacheDb.KeyDelete(key);
        }

        public void RemoveDatas(string keyPattern)
        {
            if (_server != null)
            {
                foreach (var key in _server.Keys(pattern: keyPattern))
                {
                    _cacheDb.KeyDelete(key);
                }
            }
        }

        public void SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
    }
}
