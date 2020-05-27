using System.Text.Json;

namespace CSharpSerialiser
{
    public static class JsonExtensions
    {
        #region Methods
        public static string GetProperty(this JsonElement json, string propertyName, string defaultValue)
        {
            if (json.TryGetProperty(propertyName, out var element))
            {
                return element.GetString();
            }

            return defaultValue;
        }
        #endregion
    }
}