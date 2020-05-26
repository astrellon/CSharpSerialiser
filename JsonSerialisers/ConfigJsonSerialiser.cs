// Auto generated JsonSerialiser for CSharpSerialiser.Config

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config input, Utf8JsonWriter output, bool skipStartObject = false)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            
            
            output.WriteStartArray("nameSpace");
            foreach (var item in input.NameSpace)
            {
                output.WriteStringValue(item);
            }
            output.WriteEndArray();
            output.WriteString("baseSerialiserClassName", input.BaseSerialiserClassName);
            output.WriteString("targetProject", input.TargetProject);
            
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

        public static Config ReadConfig(JsonElement input)
        {
            var nameSpace = new List<System.String>();
            foreach (var value in input.GetProperty("nameSpace").EnumerateArray())
            {
                nameSpace.Add(value.GetString());
            }

            var baseSerialiserClassName = input.GetProperty("baseSerialiserClassName").GetString();
            var targetProject = input.GetProperty("targetProject").GetString();
            var findBaseClasses = new List<Config.FindBaseClass>();
            foreach (var value in input.GetProperty("findBaseClasses").EnumerateArray())
            {
                findBaseClasses.Add(ReadConfigFindBaseClass(value));
            }

            var findClasses = new List<Config.FindClass>();
            foreach (var value in input.GetProperty("findClasses").EnumerateArray())
            {
                findClasses.Add(ReadConfigFindClass(value));
            }

            var findClassStubs = new List<Config.FindClass>();
            foreach (var value in input.GetProperty("findClassStubs").EnumerateArray())
            {
                findClassStubs.Add(ReadConfigFindClass(value));
            }

            var formatConfigs = new List<Config.FormatConfig>();
            foreach (var value in input.GetProperty("formatConfigs").EnumerateArray())
            {
                formatConfigs.Add(ReadConfigFormatConfig(value));
            }

            return new Config(nameSpace, baseSerialiserClassName, targetProject, findBaseClasses, findClasses, findClassStubs, formatConfigs);
        }
    }
}
