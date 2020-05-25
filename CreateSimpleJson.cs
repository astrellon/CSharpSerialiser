using System;
using System.Linq;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class CreateSimpleJson : BaseCodeGenerator
    {
        #region Fields
        public override string WriteObject => "";

        public override string ReadObject => "JSONObject";

        public override string FileSuffix => "SimpleJsonSerialiser";

        public override IEnumerable<string> UsingImports => new []
        {
            "System", "System.Collections.Generic", "SimpleJSON"
        };

        #endregion

        #region Constructor
        public CreateSimpleJson(Manager manager) : base(manager)
        {

        }
        #endregion

        #region Methods
        protected override void WriteBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclass, string castedName)
        {
            var paramName = $"{subclass.Subclass.FullName}.{classBaseObject.TypeDiscriminator.Name}";
            writer.WriteLine($"var result = Write({castedName});");
            writer.WriteLine($"result[\"{classBaseObject.TypeDiscriminator.CamelCaseName}\"] = {paramName};");
            writer.WriteLine("return result;");
        }

        protected override void WriteClassObjectMethod(string generics, string constraints, ClassObject classObject)
        {
            writer.Write($"public static JSONObject Write{generics}({classObject.FullName.Value}{generics} input)");
            writer.WriteLine(constraints);
        }

        protected override void WriterClassBaseObjectMethod(string generics, string constraints, ClassBaseObject classBaseObject)
        {
            writer.Write($"public static JSONObject Write{generics}({classBaseObject.FullName.Value}{generics} input)");
            writer.WriteLine(constraints);
        }

        protected override void WriteFields(ClassObject classObject)
        {
            writer.WriteLine("var result = new JSONObject();");
            foreach (var field in classObject.Fields)
            {
                WriteField(field);
            }
            writer.WriteLine("return result;");
        }

        protected override void WriteField(ClassField classField)
        {
            var inputFieldName = $"input.{classField.Name}";
            var valueString = WriteFieldType(classField.Type, inputFieldName, 0);

            writer.WriteLine($"result[\"{classField.CamelCaseName}\"] = {valueString};");
        }

        private string WriteFieldType(ClassType classType, string paramName, int depth)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                return WriteBasicField(classType.Name, paramName);
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                return $"{classType.EnumUnderlayingType.Name}){paramName}";
            }
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine();
                var arrayName = $"array{CodeGeneratorUtils.ToTitleCase(paramName.Replace(".", ""))}";

                writer.WriteLine($"var {arrayName} = new JSONArray();");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;

                var arrayValue = WriteFieldType(classType.GenericTypes.First(), itemName, depth + 1);
                writer.WriteLine($"{arrayName}.Add({arrayValue});");

                writer.Indent--;
                writer.WriteLine("}");

                return arrayName;
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var itemName = $"kvp{(depth == 0 ? "" : depth.ToString())}";
                var keyName = $"{itemName}.Key";
                var valueName = $"{itemName}.Value";
                var objectName = $"object{CodeGeneratorUtils.ToTitleCase(paramName.Replace(".", ""))}";

                writer.WriteLine();

                // Simple key in dictionary
                if (TryGetBasicJsonType(classType.GenericTypes[0].Name, out var jsonType))
                {
                    writer.WriteLine($"var {objectName} = new JSONObject();");
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    var keyString = WriteBasicField(classType.GenericTypes[0].Name, keyName);
                    var valueString = WriteBasicField(classType.GenericTypes[1].Name, valueName);

                    if (jsonType != "String")
                    {
                        keyString += ".ToString()";
                    }

                    writer.WriteLine($"{objectName}[{keyString}] = {valueString};");

                    writer.Indent--;
                    writer.WriteLine("}");
                }
                // Complex key in dictionary
                else
                {
                    writer.WriteLine($"var {objectName} = new JSONArray();");
                    writer.WriteLine($"foreach (var {itemName} in {paramName})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    var keyPairName = $"keypair{(depth == 0 ? "" : depth.ToString())}";

                    writer.WriteLine($"var {keyPairName} = new JSONObject();");
                    writer.WriteLine($"{keyPairName}[\"key\"] = {WriteFieldType(classType.GenericTypes[0], keyName, depth + 1)};");
                    writer.WriteLine($"{keyPairName}[\"value\"] = {WriteFieldType(classType.GenericTypes[1], valueName, depth + 1)};");

                    writer.WriteLine($"{objectName}.Add({keyPairName});");

                    writer.Indent--;
                    writer.WriteLine("}");
                }

                return objectName;
            }

            return "OH NO";
        }

        private string WriteBasicField(ClassName className, string paramName)
        {
            if (!TryGetBasicJsonType(className, out var jsonType) || manager.IsKnownClassOrBase(className))
            {
                return $"Write({paramName})";
            }
            else
            {
                return paramName;
            }
        }

        protected override void WriteReadBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclassPair)
        {
            var shortName = CodeGeneratorUtils.GetPrimitiveName(subclassPair.Subclass.FullName);
            writer.WriteLine($"return Read{shortName}(input);");
        }

        protected override void WriteReadBaseClassTypeHandler(ClassBaseObject classBaseObject)
        {
            var inputField = $"input[\"{classBaseObject.TypeDiscriminator.CamelCaseName}\"]";
            writer.WriteLine($"var type = {ReadFieldType(inputField, "type", classBaseObject.TypeDiscriminator.Type, 0)};");
        }

        protected override void ReadClassInner(ClassObject classObject)
        {
            CodeGeneratorUtils.ReadFieldsToCtor(manager, classObject, writer, (manager, classField, writer) => this.ReadField(classField));
        }

        private void ReadField(ClassField classField)
        {
            var inputField = $"input[\"{classField.CamelCaseName}\"]";
            var varString = classField.SafeCamelCaseName;
            var valueString = ReadFieldType(inputField, classField.Name, classField.Type, 0);

            if (varString != valueString)
            {
                writer.WriteLine($"var {varString} = {valueString};");
            }
        }

        private string ReadFieldType(string input, string resultName, ClassType classType, int depth)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                string jsonType;
                if (!TryGetBasicJsonType(classType.Name, out jsonType) && manager.IsKnownClassOrBase(classType.Name))
                {
                    var fullName = CodeGeneratorUtils.GetPrimitiveName(classType.Name);
                    var readName = $"Read{fullName}";
                    if (classType.GenericTypes.Any())
                    {
                        var generics = CodeGeneratorUtils.MakeGenericType(classType);
                        return $"{readName}<{generics}>({input}.AsObject)";
                    }
                    return $"{readName}({input}.AsObject)";
                }
                else
                {
                    if (jsonType == "String")
                    {
                        return $"{input}.Value";
                    }

                    return $"{input}.As{jsonType}";
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                var primitiveType = CodeGeneratorUtils.GetPrimitiveName(classType.EnumUnderlayingType.Name);
                return $"({classType.Name.Value}){input}.As{primitiveType}";
            }
            else if (classType.CollectionType == CollectionType.List || classType.CollectionType == CollectionType.Array)
            {
                var genericType = classType.GenericTypes.First();
                var genericTypeName = CodeGeneratorUtils.MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);
                var indexName = $"value{depthStr}";

                writer.WriteLine($"var {resultName} = new {genericTypeName}();");
                writer.WriteLine($"foreach (var {indexName} in {input}.Children)");
                writer.WriteLine("{");
                writer.Indent++;

                var readFromIter = indexName;

                if (TryGetBasicJsonType(genericType.Name, out var jsonType))
                {
                    if (jsonType != "String")
                    {
                        readFromIter += $".As{jsonType}";
                    }
                }
                else
                {
                    readFromIter += ".AsObject";
                }

                writer.WriteLine($"{resultName}.Add({ReadFieldType(readFromIter, resultName + (depth + 1), genericType, depth + 1)});");
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
                    var indexKeyName = $"{indexName}[\"key\"]";
                    var indexValueName = $"{indexName}[\"value\"]";

                    writer.WriteLine($"foreach (var {indexName} in {input}.Children)");
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
                    writer.WriteLine($"foreach (var {indexName} in {input})");
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (jsonType == "String")
                    {
                        writer.WriteLine($"var {keyName} = {indexName}.Key;");
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
                    primitiveType.Contains("uint", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("short", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("ushort", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("byte", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("sbyte", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Int";
                    return true;
                }

                if (primitiveType.Contains("long", StringComparison.OrdinalIgnoreCase) ||
                    primitiveType.Contains("ulong", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Long";
                    return true;
                }

                if (primitiveType.Contains("single", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Float";
                    return true;
                }

                if (primitiveType.Contains("double", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Double";
                    return true;
                }

                if (primitiveType.EndsWith("String", StringComparison.OrdinalIgnoreCase))
                {
                    result = "String";
                    return true;
                }

                if (primitiveType.EndsWith("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    result = "Bool";
                    return true;
                }
            }

            result = "";
            return false;
        }
        #endregion
    }
}