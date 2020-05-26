// Auto generated JsonSerialiser for CSharpSerialiser.Config.BinaryFormatConfig

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config.BinaryFormatConfig input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            
            output.WriteString("outputFolder", input.OutputFolder);
            output.WriteEndObject();
        }

        public static Config.BinaryFormatConfig ReadConfigBinaryFormatConfig(JsonElement input)
        {
            var outputFolder = input.GetProperty("outputFolder").GetString();
            return new Config.BinaryFormatConfig(outputFolder);
        }
    }
}
