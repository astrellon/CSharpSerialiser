// Auto generated JsonSerialiser for CSharpSerialiser.Config.FindBaseClass

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config.FindBaseClass input, Utf8JsonWriter output, bool skipStartObject)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            Write(input, output);
        }
        
        public static void Write(Config.FindBaseClass input, Utf8JsonWriter output)
        {
            output.WriteString("typeNameRegex", input.TypeNameRegex);
            output.WriteString("typeField", input.TypeField);
            output.WriteEndObject();
        }
        
        public static Config.FindBaseClass ReadConfigFindBaseClass(JsonElement input)
        {
            var typeNameRegex = input.GetProperty("typeNameRegex").GetString();
            var typeField = input.GetProperty("typeField").GetString();
            
            return new Config.FindBaseClass(typeNameRegex, typeField);
        }
    }
}
