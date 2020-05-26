// Auto generated JsonSerialiser for CSharpSerialiser.Config

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public static void Write(Config input, Utf8JsonWriter output, bool skipStartObject)
        {
            if (!skipStartObject)
            {
                output.WriteStartObject();
            }
            Write(input, output);
        }
        
        public static void Write(Config input, Utf8JsonWriter output)
        {
            output.WritePropertyName("nameSpace");
            Write(input.NameSpace, output);
            
            output.WriteString("baseSerialiserClassName", input.BaseSerialiserClassName);
            output.WriteString("targetProject", input.TargetProject);
            output.WritePropertyName("findBaseClasses");
            Write(input.FindBaseClasses, output, Write);
            
            output.WritePropertyName("findClasses");
            Write(input.FindClasses, output, Write);
            
            output.WritePropertyName("findClassStubs");
            Write(input.FindClassStubs, output, Write);
            
            output.WritePropertyName("formatConfigs");
            Write(input.FormatConfigs, output, Write);
            
            output.WriteEndObject();
        }

        public static Config ReadConfig(JsonElement input)
        {
            var nameSpaceJson = input.GetProperty("nameSpace");
            var nameSpace = new List<System.String>(ReadListString(nameSpaceJson));

            var baseSerialiserClassName = input.GetProperty("baseSerialiserClassName").GetString();
            var targetProject = input.GetProperty("targetProject").GetString();
            var findBaseClassesJson = input.GetProperty("findBaseClasses");
            var findBaseClasses = new List<Config.FindBaseClass>(ReadList(findBaseClassesJson, ReadConfigFindBaseClass));

            var findClassesJson = input.GetProperty("findClasses");
            var findClasses = new List<Config.FindClass>(ReadList(findClassesJson, ReadConfigFindClass));

            var findClassStubsJson = input.GetProperty("findClassStubs");
            var findClassStubs = new List<Config.FindClass>(ReadList(findClassStubsJson, ReadConfigFindClass));

            var formatConfigsJson = input.GetProperty("formatConfigs");
            var formatConfigs = new List<Config.FormatConfig>(ReadList(formatConfigsJson, ReadConfigFormatConfig));

            return new Config(nameSpace, baseSerialiserClassName, targetProject, findBaseClasses, findClasses, findClassStubs, formatConfigs);
        }
    }
}
