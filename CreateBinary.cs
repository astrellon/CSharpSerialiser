using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public static class CreateBinary
    {
        #region Fields
        private const string Writer = "BinaryWriter";
        private const string Reader = "BinaryReader";
        #endregion

        #region Methods
        public static void SaveToFolder(Manager manager, string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (var classBaseObject in manager.ClassBaseObjectMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classBaseObject.FullName)}BinarySerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.Open(outputFile, FileMode.Create))
                {
                    SaveToStream(manager, classBaseObject, file);
                }
            }

            foreach (var classObject in manager.ClassMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classObject.FullName)}BinarySerialiser.cs";
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
                CodeGeneratorUtils.WriteOuterClass(manager, classBaseObject.FullName, writer, "BinarySerialiser",
                new []{"System", "System.IO", "System.Collections.Generic"},
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
                CodeGeneratorUtils.WriteOuterClass(manager, classObject.FullName, writer, "BinarySerialiser",
                new []{"System", "System.IO", "System.Collections.Generic"},
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

            writer.Write($"public static void Write{generics}({classObject.FullName.Value}{generics} input, {Writer} output)");
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
                WriteFieldType(manager, classBaseObject.TypeDiscriminator.Type, paramName, 0, writer);
                writer.WriteLine($"Write({castedName}, output);");
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

            writer.WriteLine("}\n");
        }

        private static void WriteFields(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            foreach (var field in classObject.Fields)
            {
                WriteField(manager, field, writer);
            }
        }

        private static void WriteField(Manager manager, ClassField classField, IndentedTextWriter writer, string fieldNameOverride = null)
        {
            var inputFieldName = $"input.{classField.Name}";
            WriteFieldType(manager, classField.Type, inputFieldName, 0, writer);
        }

        public static void WriteFieldType(Manager manager, ClassType classType, string paramName, int depth, IndentedTextWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                if (manager.IsKnownClassOrBase(classType.Name))
                {
                    writer.WriteLine($"Write({paramName}, output);");
                }
                else
                {
                    writer.WriteLine($"output.Write({paramName});");
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                writer.WriteLine($"output.Write(({classType.EnumUnderlayingType.Name}){paramName});");
            }
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine();
                writer.WriteLine($"output.Write({paramName}.Count);");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(manager, classType.GenericTypes.First(), itemName, depth + 1, writer);
                writer.Indent--;
                writer.WriteLine("}\n");
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var itemName = $"kvp{(depth == 0 ? "" : depth.ToString())}";
                var keyName = $"{itemName}.Key";
                var valueName = $"{itemName}.Value";

                writer.WriteLine();
                writer.WriteLine($"output.Write({paramName}.Count);");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(manager, classType.GenericTypes[0], keyName, depth + 1, writer);
                WriteFieldType(manager, classType.GenericTypes[1], valueName, depth + 1, writer);
                writer.Indent--;
                writer.WriteLine("}\n");
            }
        }

        private static void ReadClass(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            var readName = CodeGeneratorUtils.MakeReadMethodName(classObject.FullName);
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static {classObject.FullName.Value}{generics} {readName}{generics}({Reader} input)");
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

            writer.Write($"public static {classBaseObject.FullName.Value}{generics} {readName}{generics}({Reader} input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");
            writer.Indent++;

            writer.WriteLine($"var type = {ReadFieldType(manager, "type", classBaseObject.TypeDiscriminator.Type, 0, writer)};");

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
            var varString = CodeGeneratorUtils.ToCamelCase(classField.Name);
            var valueString = ReadFieldType(manager, $"{classField.Name}", classField.Type, 0, writer);

            if (varString != valueString)
            {
                writer.WriteLine($"var {varString} = {valueString};");
            }
        }

        private static string ReadFieldType(Manager manager, string resultName, ClassType classType, int depth, IndentedTextWriter writer)
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
                        return $"{readName}<{generics}>(input)";
                    }
                    return $"{readName}(input)";
                }
                else
                {
                    var primitiveType = CodeGeneratorUtils.GetPrimitiveName(classType.Name);
                    return $"input.Read{primitiveType}()";
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                var primitiveType = CodeGeneratorUtils.GetPrimitiveName(classType.EnumUnderlayingType.Name);
                return $"({classType.Name.Value})input.Read{primitiveType}()";
            }
            else if (classType.CollectionType == CollectionType.List || classType.CollectionType == CollectionType.Array)
            {
                var countName = $"count{CodeGeneratorUtils.ToTitleCase(resultName)}";
                var genericType = classType.GenericTypes.First();
                var genericTypeName = CodeGeneratorUtils.MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                var indexString = CodeGeneratorUtils.MakeIndexIterator(depth);
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);

                writer.WriteLine($"var {countName} = input.ReadInt32();");
                writer.WriteLine($"var {resultName} = new {genericTypeName}({countName});");
                writer.WriteLine($"for (var {indexString} = 0; {indexString} < {countName}; {indexString}++)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"{resultName}.Add({ReadFieldType(manager, resultName + indexString.ToUpperInvariant(), genericType, depth + 1, writer)});");
                writer.Indent--;
                writer.WriteLine("}\n");

                return resultName;
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var countName = $"count{CodeGeneratorUtils.ToTitleCase(resultName)}";
                var keyType = classType.GenericTypes[0];
                var valueType = classType.GenericTypes[1];
                var depthStr = depth == 0 ? "" : depth.ToString();
                var keyName = $"key{depthStr}";
                var valueName = $"value{depthStr}";
                var indexName = CodeGeneratorUtils.MakeIndexIterator(depth);
                var genericName = CodeGeneratorUtils.MakeGenericType(classType);
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);

                writer.WriteLine($"var {countName} = input.ReadInt32();");
                writer.WriteLine($"var {resultName} = new {genericName}();");
                writer.WriteLine($"for (var {indexName} = 0; {indexName} < {countName}; {indexName}++)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"var {keyName} = {ReadFieldType(manager, keyName, keyType, depth + 1, writer)};");
                writer.WriteLine($"var {valueName} = {ReadFieldType(manager, valueName, valueType, depth + 1, writer)};");
                writer.WriteLine($"{resultName}[{keyName}] = {valueName};");
                writer.Indent--;
                writer.WriteLine("}\n");

                return resultName;
            }

            return "OH NO";
        }


        #endregion
    }
}