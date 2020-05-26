// Auto generated JsonSerialiser for CSharpSerialiser.Config.JsonFormatConfig

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config.JsonFormatConfig input, Utf8JsonWriter output, bool skipStartObject)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            Write(input, output);
        }
        
        public static void Write(Config.JsonFormatConfig input, Utf8JsonWriter output)
        {
            output.WriteString("outputFolder", input.OutputFolder);
            output.WriteEndObject();
        }

        public static Config.JsonFormatConfig ReadConfigJsonFormatConfig(JsonElement input)
        {
            var outputFolder = input.GetProperty("outputFolder").GetString();
            return new Config.JsonFormatConfig(outputFolder);
        }
    }
}
