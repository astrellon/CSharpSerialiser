// Auto generated JsonSerialiser for CSharpSerialiser.Config.FormatConfig

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config.FormatConfig input, Utf8JsonWriter output)
        {
            if (input is Config.BinaryFormatConfig inputCSharpSerialiserConfigBinaryFormatConfig)
            {
                output.WriteStartObject();
                output.WriteString("type", Config.BinaryFormatConfig.Type);
                Write(inputCSharpSerialiserConfigBinaryFormatConfig, output, true);
            }
            else if (input is Config.JsonFormatConfig inputCSharpSerialiserConfigJsonFormatConfig)
            {
                output.WriteStartObject();
                output.WriteString("type", Config.JsonFormatConfig.Type);
                Write(inputCSharpSerialiserConfigJsonFormatConfig, output, true);
            }
            else if (input is Config.SimpleJsonFormatConfig inputCSharpSerialiserConfigSimpleJsonFormatConfig)
            {
                output.WriteStartObject();
                output.WriteString("type", Config.SimpleJsonFormatConfig.Type);
                Write(inputCSharpSerialiserConfigSimpleJsonFormatConfig, output, true);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
        
        public static Config.FormatConfig ReadConfigFormatConfig(JsonElement input)
        {
            var type = input.GetProperty("type").GetString();
            if (type == CSharpSerialiser.Config.BinaryFormatConfig.Type)
            {
                return ReadConfigBinaryFormatConfig(input);
            }
            else if (type == CSharpSerialiser.Config.JsonFormatConfig.Type)
            {
                return ReadConfigJsonFormatConfig(input);
            }
            else if (type == CSharpSerialiser.Config.SimpleJsonFormatConfig.Type)
            {
                return ReadConfigSimpleJsonFormatConfig(input);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
    }
}
