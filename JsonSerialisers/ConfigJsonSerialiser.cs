// Auto generated JsonSerialiser for CSharpSerialiser.Config

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(CSharpSerialiser.Config input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
                output.WriteStartObject();
            
            output.WriteStartArray("nameSpace");
            foreach (var item in input.NameSpace)
            {
                output.WriteStringValue(item);
            }
            output.WriteEndArray();
            output.WriteString("baseSerialiserClassName", input.BaseSerialiserClassName);
            
            output.WriteStartArray("findBaseClasses");
            foreach (var item in input.FindBaseClasses)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            
            output.WriteStartArray("findClasses");
            foreach (var item in input.FindClasses)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            
            output.WriteStartArray("findClassStubs");
            foreach (var item in input.FindClassStubs)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            
            output.WriteStartArray("formatConfigs");
            foreach (var item in input.FormatConfigs)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            output.WriteEndObject();
        }

        public static CSharpSerialiser.Config ReadConfig(JsonElement input)
        {
            var nameSpace = new List<System.String>();
            foreach (var value in input.GetProperty("nameSpace").EnumerateArray())
            {
                nameSpace.Add(value.GetString());
            }

            var baseSerialiserClassName = input.GetProperty("baseSerialiserClassName").GetString();
            var findBaseClasses = new List<CSharpSerialiser.Config.FindBaseClass>();
            foreach (var value in input.GetProperty("findBaseClasses").EnumerateArray())
            {
                findBaseClasses.Add(ReadFindBaseClass(value));
            }

            var findClasses = new List<CSharpSerialiser.Config.FindClass>();
            foreach (var value in input.GetProperty("findClasses").EnumerateArray())
            {
                findClasses.Add(ReadFindClass(value));
            }

            var findClassStubs = new List<CSharpSerialiser.Config.FindClass>();
            foreach (var value in input.GetProperty("findClassStubs").EnumerateArray())
            {
                findClassStubs.Add(ReadFindClass(value));
            }

            var formatConfigs = new List<CSharpSerialiser.Config.FormatConfig>();
            foreach (var value in input.GetProperty("formatConfigs").EnumerateArray())
            {
                formatConfigs.Add(ReadFormatConfig(value));
            }

            return new CSharpSerialiser.Config(nameSpace, baseSerialiserClassName, findBaseClasses, findClasses, findClassStubs, formatConfigs);
        }
    }
}
