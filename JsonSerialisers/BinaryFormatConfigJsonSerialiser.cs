// Auto generated JsonSerialiser for CSharpSerialiser.Config.BinaryFormatConfig

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config.BinaryFormatConfig input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
                output.WriteStartObject();
            output.WriteString("outputFolder", input.OutputFolder);
            output.WriteEndObject();
        }

        public static CSharpSerialiser.Config.BinaryFormatConfig ReadBinaryFormatConfig(JsonElement input)
        {
            var outputFolder = input.GetProperty("outputFolder").GetString();
            return new CSharpSerialiser.Config.BinaryFormatConfig(outputFolder);
        }
    }
}
