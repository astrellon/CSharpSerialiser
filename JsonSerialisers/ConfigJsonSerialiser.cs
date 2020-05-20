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
            output.WriteStartArray("NameSpace");
            foreach (var item in input.NameSpace)
            {
                output.WriteStringValue(item);
            }
            output.WriteEndArray();
            output.WriteString("BaseSerialiserClassName", input.BaseSerialiserClassName);
            output.WriteStartArray("FindBaseClasses");
            foreach (var item in input.FindBaseClasses)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            output.WriteStartArray("FindClasses");
            foreach (var item in input.FindClasses)
            {
                Write(item, output);
            }
            output.WriteEndArray();
            output.WriteEndObject();
        }
        public static CSharpSerialiser.Config ReadConfig(JsonElement input)
        {
            var NameSpaceValueJson = input.GetProperty("NameSpace");
            var countNameSpaceValue = NameSpaceValueJson.GetArrayLength();
            var NameSpaceValue = new List<System.String>(countNameSpaceValue);
            for (var i = 0; i < countNameSpaceValue; i++)
            {
                NameSpaceValue.Add(NameSpaceValueJson[i].GetString());
            }
            var NameSpace = NameSpaceValue;
            var BaseSerialiserClassName = input.GetProperty("BaseSerialiserClassName").GetString();
            var FindBaseClassesValueJson = input.GetProperty("FindBaseClasses");
            var countFindBaseClassesValue = FindBaseClassesValueJson.GetArrayLength();
            var FindBaseClassesValue = new List<CSharpSerialiser.Config.FindBaseClass>(countFindBaseClassesValue);
            for (var i = 0; i < countFindBaseClassesValue; i++)
            {
                FindBaseClassesValue.Add(ReadFindBaseClass(FindBaseClassesValueJson[i]));
            }
            var FindBaseClasses = FindBaseClassesValue;
            var FindClassesValueJson = input.GetProperty("FindClasses");
            var countFindClassesValue = FindClassesValueJson.GetArrayLength();
            var FindClassesValue = new List<CSharpSerialiser.Config.FindClass>(countFindClassesValue);
            for (var i = 0; i < countFindClassesValue; i++)
            {
                FindClassesValue.Add(ReadFindClass(FindClassesValueJson[i]));
            }
            var FindClasses = FindClassesValue;
            return new CSharpSerialiser.Config(NameSpace, BaseSerialiserClassName, FindBaseClasses, FindClasses);
        }
    }
}
