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
        #endregion

        #region Methods
        public static void SaveToFolder(Manager manager, string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (var classBaseObject in manager.ClassBaseObjectMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classBaseObject.FullName)}JsonSerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.OpenWrite(outputFile))
                {
                    SaveToStream(manager, classBaseObject, file);
                }
            }

            foreach (var classObject in manager.ClassMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classObject.FullName)}JsonSerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.OpenWrite(outputFile))
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
                writer.WriteLine($"// Auto generated JsonSerialiser for {classBaseObject.FullName}\n");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.IO;\n");
                writer.WriteLine("using System.Text.Json;\n");
                writer.WriteLine("using System.Collections.Generic;\n");
                writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"public static partial class {manager.BaseSerialiserClassName}JsonSerialiser");
                writer.WriteLine("{");
                writer.Indent++;

                WriteBaseClass(manager, classBaseObject, writer);
                ReadBaseClass(manager, classBaseObject, writer);

                writer.Indent--;
                writer.WriteLine("}");
                writer.Indent--;
                writer.WriteLine("}");
            }
        }

        public static void SaveToStream(Manager manager, ClassObject classObject, Stream output)
        {
            using (var streamWriter = new StreamWriter(output))
            using (var writer = new IndentedTextWriter(streamWriter))
            {
                writer.WriteLine($"// Auto generated JsonSerialiser for {classObject.FullName}\n");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.IO;\n");
                writer.WriteLine("using System.Text.Json;\n");
                writer.WriteLine("using System.Collections.Generic;\n");
                writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"public static partial class {manager.BaseSerialiserClassName}JsonSerialiser");
                writer.WriteLine("{");
                writer.Indent++;

                WriteClass(manager, classObject, writer);
                ReadClass(manager, classObject, writer);

                writer.Indent--;
                writer.WriteLine("}");
                writer.Indent--;
                writer.WriteLine("}");
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

            writer.WriteLine("}");
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
                WriteFieldType(manager, classBaseObject.TypeDiscriminator.Type, classBaseObject.TypeDiscriminator.Name, paramName, 0, writer);
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

        private static void WriteField(Manager manager, ClassField classField, IndentedTextWriter writer, string fieldNameOverride = null)
        {
            var inputFieldName = $"input.{classField.Name}";
            WriteFieldType(manager, classField.Type, classField.Name, inputFieldName, 0, writer);
        }

        public static void WriteFieldType(Manager manager, ClassType classType, string propertyName, string paramName, int depth, IndentedTextWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                WriteBasicField(manager, classType.Name, propertyName, paramName, writer);
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                writer.WriteLine($"output.WriteNumber(\"{propertyName}\", ({classType.EnumUnderlayingType.Name}){paramName});");
            }
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine($"output.WriteStartArray(\"{propertyName}\");");
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

                writer.WriteLine($"output.WriteStartObject(\"{propertyName}\");");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;

                var jsonType = GetBasicJsonType(classType.GenericTypes[0].Name);
                writer.WriteLine($"output.WritePropertyName({keyName}.ToString());");
                WriteBasicField(manager, classType.GenericTypes[1].Name, keyName, valueName, writer);

                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLine("output.WriteEndObject();");
            }
        }

        private static void WriteBasicField(Manager manager, ClassName className, string propertyName, string paramName, IndentedTextWriter writer)
        {
            if (manager.IsKnownClassOrBase(className))
            {
                writer.WriteLine($"Write({paramName}, output);");
            }
            else
            {
                var jsonType = GetBasicJsonType(className);
                if (propertyName == "")
                {
                    writer.WriteLine($"output.Write{jsonType}Value({paramName});");
                }
                else
                {
                    writer.WriteLine($"output.Write{jsonType}(\"{propertyName}\", {paramName});");
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
            ReadFields(manager, classObject, writer);
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

            var inputField = $"input.GetProperty(\"{classBaseObject.TypeDiscriminator.Name}\")";
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

        private static void ReadFields(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            foreach (var field in classObject.Fields)
            {
                ReadField(manager, field, writer);
            }

            var fieldOrder = new ClassField[classObject.CtorFields.Count];

            for (var i = 0; i < classObject.CtorFields.Count; i++)
            {
                var ctorField = classObject.CtorFields[i];
                foreach (var field in classObject.Fields)
                {
                    if (field.Name.Equals(ctorField.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        fieldOrder[i] = field;
                        break;
                    }
                }
            }

            if (fieldOrder.Any(fo => fo == null))
            {
                for (var i = 0; i < classObject.CtorFields.Count; i++)
                {
                    if (fieldOrder[i] != null)
                    {
                        continue;
                    }

                    var ctorField = classObject.CtorFields[i];
                    foreach (var field in classObject.Fields)
                    {
                        if (field.Type.Name == ctorField.Type.Name)
                        {
                            fieldOrder[i] = field;
                            break;
                        }
                    }
                }
            }

            if (fieldOrder.Any(fo => fo == null))
            {
                throw new Exception($"Unable to determin ctor parameters for: {classObject.FullName}");
            }

            var ctorArgs = string.Join(", ", fieldOrder.Select(f => f.Name));
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            writer.WriteLine($"return new {classObject.FullName.Value}{generics}({ctorArgs});");
        }

        private static void ReadField(Manager manager, ClassField classField, IndentedTextWriter writer)
        {
            var inputField = $"input.GetProperty(\"{classField.Name}\")";
            writer.WriteLine($"var {classField.Name} = {ReadFieldType(manager, inputField, classField.Name + "Value", classField.Type, 0, writer)};");
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
                var countName = $"count{resultName}";
                var genericType = classType.GenericTypes.First();
                var genericTypeName = CodeGeneratorUtils.MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                var indexString = CodeGeneratorUtils.MakeIndexIterator(depth);
                var arrayName = $"{resultName}Json";

                writer.WriteLine($"var {arrayName} = {input};");
                writer.WriteLine($"var {countName} = {arrayName}.GetArrayLength();");
                writer.WriteLine($"var {resultName} = new {genericTypeName}({countName});");
                writer.WriteLine($"for (var {indexString} = 0; {indexString} < {countName}; {indexString}++)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"{resultName}.Add({ReadFieldType(manager, $"{arrayName}[{indexString}]", resultName + "_", genericType, depth + 1, writer)});");
                writer.Indent--;
                writer.WriteLine("}");

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
                var dictName = $"{resultName}Json";

                var basicKeyType = CodeGeneratorUtils.GetPrimitiveName(keyType.Name);

                writer.WriteLine($"var {dictName} = {input};");
                writer.WriteLine($"var {resultName} = new {genericName}();");
                writer.WriteLine($"foreach (var {indexName} in {dictName}.EnumerateObject())");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"var {valueName} = {ReadFieldType(manager, $"{indexName}.Value", valueName, valueType, depth + 1, writer)};");

                if (basicKeyType == "String")
                {
                    writer.WriteLine($"{resultName}[{indexName}.Name] = {valueName};");
                }
                else
                {
                    writer.WriteLine($"var {keyName} = Convert.To{basicKeyType}({indexName}.Name);");
                    writer.WriteLine($"{resultName}[{keyName}] = {valueName};");
                }
                writer.Indent--;
                writer.WriteLine("}");

                return resultName;
            }

            return "OH NO";
        }

        private static string GetBasicJsonType(ClassName className)
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
                    return "Number";
                }

                if (primitiveType.EndsWith("String", StringComparison.OrdinalIgnoreCase))
                {
                    return "String";
                }

                if (primitiveType.EndsWith("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return "Boolean";
                }
            }

            throw new Exception("No basic Json type available");
        }

        #endregion
    }
}