using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System.Text;

namespace GateEntryExit.Caching
{
    public class CachedDataHelper
    {
        private static readonly IDataProtector _protector;

        // static ctor to initialize data protection provider
        static CachedDataHelper()
        {
            var provider = DataProtectionProvider.Create("SecretApp");
            _protector = provider.CreateProtector("CachedSecretProtector");
        }

        private static byte[] Encrypt(string plainText)
        {
            return _protector.Protect(Encoding.UTF8.GetBytes(plainText));
        }

        private static string Decrypt(byte[] encrytedData)
        {
            var decryptedBytes = _protector.Unprotect(encrytedData);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public static void SaveSecretsToFile(CachedData cachedData, string filePath)
        {
            var json = JsonConvert.SerializeObject(cachedData);
            var encrypted = Encrypt(json);
            File.WriteAllBytes(filePath, encrypted);
        }

        public static CachedData LoadSecretFromFule(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var encrypted = File.ReadAllBytes(filePath);
            var json = Decrypt(encrypted);
            return JsonConvert.DeserializeObject<CachedData>(json);
        }
    }
}
