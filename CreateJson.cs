using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpSerialiser
{
    public class CreateJson : BaseCodeGenerator
    {
        #region Fields
        public override string WriteObject => "Utf8JsonWriter";

        public override string ReadObject => "JsonElement";

        public override string FileSuffix => "JsonSerialiser2";

        public override IEnumerable<string> UsingImports => new []
        { "System", "System.IO", "System.Collections.Generic", "System.Text.Json" };

        private enum ContainerType
        {
            Object, Array
        }
        #endregion

        #region Constructor
        public CreateJson(Manager manager) : base(manager)
        {

        }
        #endregion

        #region Methods
        protected override void WriteBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclass, string castedName)
        {
            var paramName = $"{subclass.Subclass.FullName}.{classBaseObject.TypeDiscriminator.Name}";
            writer.WriteLine("output.WriteStartObject();");
            WriteFieldType(classBaseObject.TypeDiscriminator.Type, classBaseObject.TypeDiscriminator.CamelCaseName, paramName, 0);
            writer.WriteLine($"Write({castedName}, output, true);");
        }

        protected override void WriteClassObjectMethod(string generics, string constraints, ClassObject classObject)
        {
            writer.Write($"public static void Write{generics}({classObject.FullName.Value}{generics} input, {this.WriteObject} output, bool skipStartObject = false)");
            writer.WriteLine(constraints);
        }

        protected override void WriteFields(ClassObject classObject)
        {
            writer.WriteLine("if (!skipStartObject)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("output.WriteStartObject();");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine();

            foreach (var field in classObject.Fields)
            {
                WriteField(field);
            }
            writer.WriteLine("output.WriteEndObject();");
        }

        protected override void WriteField(ClassField classField)
        {
            var inputFieldName = $"input.{classField.Name}";
            WriteFieldType(classField.Type, classField.CamelCaseName, inputFieldName, 0);
        }

        private void WriteStart(ContainerType containerType, string propertyName)
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

        private void WriteFieldType(ClassType classType, string propertyName, string paramName, int depth)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                WriteBasicField(classType.Name, $"\"{propertyName}\"", paramName);
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                writer.WriteLine($"output.WriteNumber(\"{propertyName}\", ({classType.EnumUnderlayingType.Name}){paramName});");
            }
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine();

                WriteStart(ContainerType.Array, propertyName);
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(classType.GenericTypes.First(), "", itemName, depth + 1);
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
                    WriteStart(ContainerType.Object, propertyName);
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (jsonType != "String")
                    {
                        keyName += ".ToString()";
                    }
                    WriteBasicField(classType.GenericTypes[1].Name, keyName, valueName);

                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine("output.WriteEndObject();\n");
                }
                // Complex key in dictionary
                else
                {
                    writer.WriteLine();
                    WriteStart(ContainerType.Array, propertyName);
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    writer.WriteLine("output.WriteStartObject();");
                    writer.WriteLine($"output.WritePropertyName(\"key\");");
                    WriteFieldType(classType.GenericTypes[0], "key", keyName, depth + 1);

                    writer.WriteLine($"output.WritePropertyName(\"value\");");
                    WriteFieldType(classType.GenericTypes[1], "value", valueName, depth + 1);
                    writer.WriteLine("output.WriteEndObject();");
                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine("output.WriteEndArray();\n");
                }

            }
        }

        private void WriteBasicField(ClassName className, string propertyName, string paramName)
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

        protected override void WriteReadBaseClassTypeHandler(ClassBaseObject classBaseObject)
        {
            var inputField = $"input.GetProperty(\"{classBaseObject.TypeDiscriminator.CamelCaseName}\")";
            writer.WriteLine($"var type = {ReadFieldType(inputField, "type", classBaseObject.TypeDiscriminator.Type, 0)};");
        }

        protected override void WriteReadBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclassPair)
        {
            var shortName = CodeGeneratorUtils.GetPrimitiveName(subclassPair.Subclass.FullName);
            writer.WriteLine($"return Read{shortName}(input);");
        }

        protected override void ReadClassInner(ClassObject classObject)
        {
            CodeGeneratorUtils.ReadFieldsToCtor(manager, classObject, writer, (manager, classField, writer) => this.ReadField(classField));
        }

        private void ReadField(ClassField classField)
        {
            var inputField = $"input.GetProperty(\"{classField.CamelCaseName}\")";
            var varString = classField.CamelCaseName;
            var valueString = ReadFieldType(inputField, $"{classField.Name}", classField.Type, 0);

            if (varString != valueString)
            {
                writer.WriteLine($"var {varString} = {valueString};");
            }
        }

        private string ReadFieldType(string input, string resultName, ClassType classType, int depth)
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
                writer.WriteLine($"{resultName}.Add({ReadFieldType(indexName, resultName + (depth + 1), genericType, depth + 1)});");
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

                    writer.WriteLine($"var {keyName} = {ReadFieldType(indexKeyName, keyName, keyType, depth + 1)};");
                    writer.WriteLine($"var {valueName} = {ReadFieldType(indexValueName, valueName, valueType, depth + 1)};");

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
                    writer.WriteLine($"var {valueName} = {ReadFieldType($"{indexName}.Value", valueName, valueType, depth + 1)};");

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