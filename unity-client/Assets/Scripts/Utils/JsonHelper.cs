using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PvpGame.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings defaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None
        };

        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, defaultSettings);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to serialize object: {ex.Message}");
                return null;
            }
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, defaultSettings);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to deserialize JSON: {ex.Message}");
                return default;
            }
        }

        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json, defaultSettings);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
