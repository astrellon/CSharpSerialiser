// Auto generated JsonSerialiser for CSharpSerialiser.Config.FindClass

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config.FindClass input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            
            output.WriteString("typeNameRegex", input.TypeNameRegex);
            output.WriteEndObject();
        }

        public static Config.FindClass ReadConfigFindClass(JsonElement input)
        {
            var typeNameRegex = input.GetProperty("typeNameRegex").GetString();
            return new Config.FindClass(typeNameRegex);
        }
    }
}
