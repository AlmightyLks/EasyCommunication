using System;
using System.Text;
using Newtonsoft.Json;

namespace EasyCommunication.Helper
{
    public static class JsonExtension
    {
        public static bool TryDeserializeJson<T>(this string jsonStr, out T result)
        {
            Type type = typeof(T);
            bool success = jsonStr.TryDeserializeJson(out object obj, type);
            result = (T)obj;
            return success;
        }
        public static bool TryDeserializeJson(this string jsonStr, out object result, Type type)
        {
            result = null;

            try
            {
                result = JsonConvert.DeserializeObject(jsonStr, type);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
