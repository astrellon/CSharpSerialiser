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
            output.WriteString("typeNameRegex", input.TypeNameRegex);
            output.WriteString("typeField", input.TypeField);
            output.WriteEndObject();
        }

        public static CSharpSerialiser.Config.FindBaseClass ReadFindBaseClass(JsonElement input)
        {
            var typeNameRegex = input.GetProperty("typeNameRegex").GetString();
            var typeField = input.GetProperty("typeField").GetString();
            return new CSharpSerialiser.Config.FindBaseClass(typeNameRegex, typeField);
        }
    }
}
