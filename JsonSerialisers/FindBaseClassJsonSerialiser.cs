// Auto generated JsonSerialiser for CSharpSerialiser.Config.FindBaseClass

using System;
using System.IO;

using System.Text.Json;

using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config.FindBaseClass input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
                output.WriteStartObject();
            output.WriteString("TypeNameRegex", input.TypeNameRegex);
            output.WriteString("TypeField", input.TypeField);
            output.WriteEndObject();
        }
        public static CSharpSerialiser.Config.FindBaseClass ReadFindBaseClass(JsonElement input)
        {
            var TypeNameRegex = input.GetProperty("TypeNameRegex").GetString();
            var TypeField = input.GetProperty("TypeField").GetString();
            return new CSharpSerialiser.Config.FindBaseClass(TypeNameRegex, TypeField);
        }
    }
}
