using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PvpGame.Utils
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions defaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize<T>(T obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, defaultOptions);
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
                return JsonSerializer.Deserialize<T>(json, defaultOptions);
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
                result = JsonSerializer.Deserialize<T>(json, defaultOptions);
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
