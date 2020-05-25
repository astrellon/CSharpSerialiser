// Auto generated JsonSerialiser for CSharpSerialiser.Config.SimpleJsonFormatConfig

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config.SimpleJsonFormatConfig input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            
            output.WriteString("outputFolder", input.OutputFolder);
            output.WriteEndObject();
        }

        public static CSharpSerialiser.Config.SimpleJsonFormatConfig ReadSimpleJsonFormatConfig(JsonElement input)
        {
            var outputFolder = input.GetProperty("outputFolder").GetString();
            return new CSharpSerialiser.Config.SimpleJsonFormatConfig(outputFolder);
        }
    }
}
