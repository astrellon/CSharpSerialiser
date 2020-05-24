// Auto generated JsonSerialiser for CSharpSerialiser.Config.FormatConfig

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config.FormatConfig input, Utf8JsonWriter output)
        {
            if (input is CSharpSerialiser.Config.BinaryFormatConfig inputCSharpSerialiserConfigBinaryFormatConfig)
            {
                output.WriteStartObject();
                output.WriteString("type", CSharpSerialiser.Config.BinaryFormatConfig.Type);
                Write(inputCSharpSerialiserConfigBinaryFormatConfig, output, true);
            }
            else if (input is CSharpSerialiser.Config.JsonFormatConfig inputCSharpSerialiserConfigJsonFormatConfig)
            {
                output.WriteStartObject();
                output.WriteString("type", CSharpSerialiser.Config.JsonFormatConfig.Type);
                Write(inputCSharpSerialiserConfigJsonFormatConfig, output, true);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
        public static CSharpSerialiser.Config.FormatConfig ReadFormatConfig(JsonElement input)
        {
            var type = input.GetProperty("type").GetString();
            if (type == CSharpSerialiser.Config.BinaryFormatConfig.Type)
            {
                return ReadBinaryFormatConfig(input);
            }
            else if (type == CSharpSerialiser.Config.JsonFormatConfig.Type)
            {
                return ReadJsonFormatConfig(input);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
    }
}
