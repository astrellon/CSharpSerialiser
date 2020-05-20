// Auto generated JsonSerialiser for CSharpSerialiser.Config.FindClass

using System;
using System.IO;

using System.Text.Json;

using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config.FindClass input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
                output.WriteStartObject();
            output.WriteString("TypeNameRegex", input.TypeNameRegex);
            output.WriteEndObject();
        }
        public static CSharpSerialiser.Config.FindClass ReadFindClass(JsonElement input)
        {
            var TypeNameRegex = input.GetProperty("TypeNameRegex").GetString();
            return new CSharpSerialiser.Config.FindClass(TypeNameRegex);
        }
    }
}
