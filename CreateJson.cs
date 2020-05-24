using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public static class CreateJson
    {
        #region Fields
        private const string Writer = "Utf8JsonWriter";
        private const string JsonElement = "JsonElement";

        private enum ContainerType
        {
            Object, Array
        }

        #endregion

        #region Methods
        public static void SaveToFolder(Manager manager, string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (var classBaseObject in manager.ClassBaseObjectMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classBaseObject.FullName)}JsonSerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.Open(outputFile, FileMode.Create))
                {
                    SaveToStream(manager, classBaseObject, file);
                }
            }

            foreach (var classObject in manager.ClassMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classObject.FullName)}JsonSerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.Open(outputFile, FileMode.Create))
                {
                    SaveToStream(manager, classObject, file);
                }
            }
        }

        public static void SaveToStream(Manager manager, ClassBaseObject classBaseObject, Stream output)
        {
            using (var streamWriter = new StreamWriter(output))
            using (var writer = new IndentedTextWriter(streamWriter))
            {
                CodeGeneratorUtils.WriteOuterClass(manager, classBaseObject.FullName, writer, "JsonSerialiser",
                    new []{"System", "System.IO", "System.Text.Json", "System.Collections.Generic"},
                    () =>
                    {
                        WriteBaseClass(manager, classBaseObject, writer);
                        ReadBaseClass(manager, classBaseObject, writer);
                    });
            }
        }

        public static void SaveToStream(Manager manager, ClassObject classObject, Stream output)
        {
            using (var streamWriter = new StreamWriter(output))
            using (var writer = new IndentedTextWriter(streamWriter))
            {
                CodeGeneratorUtils.WriteOuterClass(manager, classObject.FullName, writer, "JsonSerialiser",
                    new []{"System", "System.IO", "System.Text.Json", "System.Collections.Generic"},
                    () =>
                    {
                        WriteClass(manager, classObject, writer);
                        ReadClass(manager, classObject, writer);
                    });
            }
        }

        private static void WriteClass(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static void Write{generics}({classObject.FullName.Value}{generics} input, {Writer} output, bool skipStartObject = false)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            WriteFields(manager, classObject, writer);
            writer.Indent--;

            writer.WriteLine("}\n");
        }

        private static void WriteBaseClass(Manager manager, ClassBaseObject classBaseObject, IndentedTextWriter writer)
        {
            var generics = CodeGeneratorUtils.CreateGenericClassString(classBaseObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classBaseObject.Generics);

            if (!classBaseObject.Subclasses.Any())
            {
                writer.WriteLine($"// No derived classes found for base class: {classBaseObject.FullName.Value}");
                return;
            }

            writer.Write($"public static void Write{generics}({classBaseObject.FullName.Value}{generics} input, {Writer} output)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            var addElse = false;
            foreach (var subclass in classBaseObject.Subclasses)
            {
                if (addElse)
                {
                    writer.Write("else ");
                }

                var castedName = $"input{subclass.Subclass.FullName.Value.Replace(".", "")}";
                writer.WriteLine($"if (input is {subclass.Subclass.FullName} {castedName})");
                writer.WriteLine("{");
                writer.Indent++;
                var paramName = $"{subclass.Subclass.FullName}.{classBaseObject.TypeDiscriminator.Name}";
                writer.WriteLine("output.WriteStartObject();");
                WriteFieldType(manager, classBaseObject.TypeDiscriminator.Type, classBaseObject.TypeDiscriminator.CamelCaseName, paramName, 0, writer);
                writer.WriteLine($"Write({castedName}, output, true);");
                writer.Indent--;
                writer.WriteLine("}");

                addElse = true;
            }

            writer.WriteLine("else");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("throw new Exception(\"Unknown base class type\");");
            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;

            writer.WriteLine("}");
        }

        private static void WriteFields(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            writer.WriteLine("if (!skipStartObject)");
            writer.Indent++;
            writer.WriteLine("output.WriteStartObject();");
            writer.Indent--;

            foreach (var field in classObject.Fields)
            {
                WriteField(manager, field, writer);
            }
            writer.WriteLine("output.WriteEndObject();");
        }

        private static void WriteField(Manager manager, ClassField classField, IndentedTextWriter writer)
        {
            var inputFieldName = $"input.{classField.Name}";
            WriteFieldType(manager, classField.Type, classField.CamelCaseName, inputFieldName, 0, writer);
        }

        private static void WriteStart(IndentedTextWriter writer, ContainerType containerType, string propertyName)
        {
            var containerString = containerType.ToString();
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                writer.WriteLine($"output.WriteStart{containerString}();");
            }
            else
            {
                writer.WriteLine($"output.WriteStart{containerString}(\"{propertyName}\");");
            }
        }

        public static void WriteFieldType(Manager manager, ClassType classType, string propertyName, string paramName, int depth, IndentedTextWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                WriteBasicField(manager, classType.Name, $"\"{propertyName}\"", paramName, writer);
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                writer.WriteLine($"output.WriteNumber(\"{propertyName}\", ({classType.EnumUnderlayingType.Name}){paramName});");
            }
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine();

                WriteStart(writer, ContainerType.Array, propertyName);
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(manager, classType.GenericTypes.First(), "", itemName, depth + 1, writer);
                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLine("output.WriteEndArray();");
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var itemName = $"kvp{(depth == 0 ? "" : depth.ToString())}";
                var keyName = $"{itemName}.Key";
                var valueName = $"{itemName}.Value";

                // Simple key in dictionary
                if (TryGetBasicJsonType(classType.GenericTypes[0].Name, out var jsonType))
                {
                    writer.WriteLine();
                    WriteStart(writer, ContainerType.Object, propertyName);
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (jsonType != "String")
                    {
                        keyName += ".ToString()";
                    }
                    WriteBasicField(manager, classType.GenericTypes[1].Name, keyName, valueName, writer);

                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine("output.WriteEndObject();\n");
                }
                // Complex key in dictionary
                else
                {
                    writer.WriteLine();
                    WriteStart(writer, ContainerType.Array, propertyName);
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    writer.WriteLine("output.WriteStartObject();");
                    writer.WriteLine($"output.WritePropertyName(\"key\");");
                    WriteFieldType(manager, classType.GenericTypes[0], "key", keyName, depth + 1, writer);

                    writer.WriteLine($"output.WritePropertyName(\"value\");");
                    WriteFieldType(manager, classType.GenericTypes[1], "value", valueName, depth + 1, writer);
                    writer.WriteLine("output.WriteEndObject();");
                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine("output.WriteEndArray();\n");
                }

            }
        }

        private static void WriteBasicField(Manager manager, ClassName className, string propertyName, string paramName, IndentedTextWriter writer)
        {
            if (!TryGetBasicJsonType(className, out var jsonType) || manager.IsKnownClassOrBase(className))
            {
                writer.WriteLine($"Write({paramName}, output);");
            }
            else
            {
                if (propertyName == "" || propertyName == "\"\"")
                {
                    writer.WriteLine($"output.Write{jsonType}Value({paramName});");
                }
                else
                {
                    writer.WriteLine($"output.Write{jsonType}({propertyName}, {paramName});");
                }
            }
        }

        private static void ReadClass(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            var readName = CodeGeneratorUtils.MakeReadMethodName(classObject.FullName);
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static {classObject.FullName.Value}{generics} {readName}{generics}({JsonElement} input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            CodeGeneratorUtils.ReadFieldsToCtor(manager, classObject, writer, ReadField);
            writer.Indent--;

            writer.WriteLine("}");
        }

        private static void ReadBaseClass(Manager manager, ClassBaseObject classBaseObject, IndentedTextWriter writer)
        {
            var readName = CodeGeneratorUtils.MakeReadMethodName(classBaseObject.FullName);
            var generics = CodeGeneratorUtils.CreateGenericClassString(classBaseObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classBaseObject.Generics);

            writer.Write($"public static {classBaseObject.FullName.Value}{generics} {readName}{generics}({JsonElement} input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");
            writer.Indent++;

            var inputField = $"input.GetProperty(\"{classBaseObject.TypeDiscriminator.CamelCaseName}\")";
            writer.WriteLine($"var type = {ReadFieldType(manager, inputField, "type", classBaseObject.TypeDiscriminator.Type, 0, writer)};");

            var addElse = false;
            foreach (var subclassPair in classBaseObject.Subclasses)
            {
                if (addElse)
                {
                    writer.Write("else ");
                }

                var paramName = $"{subclassPair.Subclass.FullName}.{classBaseObject.TypeDiscriminator.Name}";
                writer.WriteLine($"if (type == {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                if (manager.ClassMap.TryGetValue(subclassPair.Subclass.FullName, out var fieldClass))
                {
                    var shortName = CodeGeneratorUtils.GetPrimitiveName(subclassPair.Subclass.FullName);
                    writer.WriteLine($"return Read{shortName}(input);");
                }
                else
                {
                    throw new Exception("Unknown subclass!");
                }
                writer.Indent--;
                writer.WriteLine("}");

                addElse = true;
            }

            writer.WriteLine("else");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("throw new Exception(\"Unknown base class type\");");
            writer.Indent--;
            writer.WriteLine("}");

            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void ReadField(Manager manager, ClassField classField, IndentedTextWriter writer)
        {
            var inputField = $"input.GetProperty(\"{classField.CamelCaseName}\")";
            var varString = classField.CamelCaseName;
            var valueString = ReadFieldType(manager, inputField, $"{classField.Name}", classField.Type, 0, writer);

            if (varString != valueString)
            {
                writer.WriteLine($"var {varString} = {valueString};");
            }
        }

        private static string ReadFieldType(Manager manager, string input, string resultName, ClassType classType, int depth, IndentedTextWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                if (manager.IsKnownClassOrBase(classType.Name))
                {
                    var fullName = CodeGeneratorUtils.GetPrimitiveName(classType.Name);
                    var readName = $"Read{fullName}";
                    if (classType.GenericTypes.Any())
                    {
                        var generics = CodeGeneratorUtils.MakeGenericType(classType);
                        return $"{readName}<{generics}>({input})";
                    }
                    return $"{readName}({input})";
                }
                else
                {
                    var primitiveType = CodeGeneratorUtils.GetPrimitiveName(classType.Name);
                    return $"{input}.Get{primitiveType}()";
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                var primitiveType = CodeGeneratorUtils.GetPrimitiveName(classType.EnumUnderlayingType.Name);
                return $"({classType.Name.Value}){input}.Get{primitiveType}()";
            }
            else if (classType.CollectionType == CollectionType.List || classType.CollectionType == CollectionType.Array)
            {
                var genericType = classType.GenericTypes.First();
                var genericTypeName = CodeGeneratorUtils.MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);
                var indexName = $"value{depthStr}";

                writer.WriteLine($"var {resultName} = new {genericTypeName}();");
                writer.WriteLine($"foreach (var {indexName} in {input}.EnumerateArray())");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"{resultName}.Add({ReadFieldType(manager, indexName, resultName + (depth + 1), genericType, depth + 1, writer)});");
                writer.Indent--;
                writer.WriteLine("}\n");

                return resultName;
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var keyType = classType.GenericTypes[0];
                var valueType = classType.GenericTypes[1];
                var depthStr = depth == 0 ? "" : depth.ToString();
                var indexName = $"prop{depthStr}";
                var keyName = $"key{depthStr}";
                var valueName = $"value{depthStr}";
                var genericName = CodeGeneratorUtils.MakeGenericType(classType);
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);


                writer.WriteLine($"var {resultName} = new {genericName}();");

                if (!TryGetBasicJsonType(classType.GenericTypes[0].Name, out var jsonType))
                {
                    var indexKeyName = $"{indexName}.GetProperty(\"key\")";
                    var indexValueName = $"{indexName}.GetProperty(\"value\")";

                    writer.WriteLine($"foreach (var {indexName} in {input}.EnumerateArray())");
                    writer.WriteLine("{");
                    writer.Indent++;

                    writer.WriteLine($"var {keyName} = {ReadFieldType(manager, indexKeyName, keyName, keyType, depth + 1, writer)};");
                    writer.WriteLine($"var {valueName} = {ReadFieldType(manager, indexValueName, valueName, valueType, depth + 1, writer)};");

                    writer.WriteLine($"{resultName}[{keyName}] = {valueName};");

                    writer.Indent--;
                    writer.WriteLine("}\n");
                }
                else
                {
                    writer.WriteLine($"foreach (var {indexName} in {input}.EnumerateObject())");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (jsonType == "String")
                    {
                        writer.WriteLine($"var {keyName} = {indexName}.Name;");
                    }
                    else
                    {
                        var primitiveType = CodeGeneratorUtils.GetPrimitiveName(keyType.Name);
                        writer.WriteLine($"var {keyName} = Convert.To{primitiveType}({indexName}.Name);");
                    }
                    //writer.WriteLine($"var {keyName} = {ReadFieldType(manager, $"{indexName}.Name", keyName, keyType, depth + 1, writer)};");
                    writer.WriteLine($"var {valueName} = {ReadFieldType(manager, $"{indexName}.Value", valueName, valueType, depth + 1, writer)};");

                    writer.WriteLine($"{resultName}[{keyName}] = {valueName};");

                    writer.Indent--;
                    writer.WriteLine("}\n");
                }

                return resultName;
            }

            return "OH NO";
        }

        private static bool TryGetBasicJsonType(ClassName className, out string result)
        {
            if (className.Value.StartsWith("System.") && className.Value.Count((char c) => c == '.') == 1)
            {
                var primitiveType = CodeGeneratorUtils.GetPrimitiveName(className);
                if (primitiveType.Contains("int", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("long", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("byte", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("short", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("double", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("single", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Number";
                    return true;
                }

                if (primitiveType.EndsWith("String", StringComparison.OrdinalIgnoreCase))
                {
                    result = "String";
                    return true;
                }

                if (primitiveType.EndsWith("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Boolean";
                    return true;
                }
            }

            result = "";
            return false;
        }

        #endregion
    }
}