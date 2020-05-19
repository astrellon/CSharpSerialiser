using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public static class CreateBinary
    {
        private static readonly IReadOnlyList<char> IndexIterators = new []{'i', 'j', 'k', 'l'};

        #region Methods
        public static void SaveToFolder(Manager manager, string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (var classBaseObject in manager.ClassBaseObjectMap.Values)
            {
                var filename = $"{GetPrimitiveName(classBaseObject.FullName)}BinarySerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.OpenWrite(outputFile))
                {
                    SaveToStream(manager, classBaseObject, file);
                }
            }

            foreach (var classObject in manager.ClassMap.Values)
            {
                var filename = $"{GetPrimitiveName(classObject.FullName)}BinarySerialiser.cs";
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
                writer.WriteLine($"// Auto generated BinarySerialiser for {classBaseObject.FullName}\n");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.IO;\n");
                writer.WriteLine("using System.Collections.Generic;\n");
                writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"public static partial class {manager.BaseSerialiserClassName}BinarySerialiser");
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
                writer.WriteLine($"// Auto generated BinarySerialiser for {classObject.FullName}\n");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.IO;\n");
                writer.WriteLine("using System.Collections.Generic;\n");
                writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"public static partial class {manager.BaseSerialiserClassName}BinarySerialiser");
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
            var generics = CreateGenericClassString(classObject.Generics);
            var constraints = CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static void Write{generics}({classObject.FullName.Value}{generics} input, BinaryWriter output)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            WriteFields(manager, classObject, writer);
            writer.Indent--;

            writer.WriteLine("}");
        }

        private static void WriteBaseClass(Manager manager, ClassBaseObject classBaseObject, IndentedTextWriter writer)
        {
            var generics = CreateGenericClassString(classBaseObject.Generics);
            var constraints = CreateGenericConstraintsString(classBaseObject.Generics);

            if (!classBaseObject.Subclasses.Any())
            {
                writer.WriteLine($"// No derived classes found for base class: {classBaseObject.FullName.Value}");
                return;
            }

            writer.Write($"public static void Write{generics}({classBaseObject.FullName.Value}{generics} input, BinaryWriter output)");
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

            writer.WriteLine("}");
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
                writer.WriteLine($"output.Write({paramName}.Count);");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(manager, classType.GenericTypes.First(), itemName, depth + 1, writer);
                writer.Indent--;
                writer.WriteLine("}");
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var itemName = $"kvp{(depth == 0 ? "" : depth.ToString())}";
                var keyName = $"{itemName}.Key";
                var valueName = $"{itemName}.Value";

                writer.WriteLine($"output.Write({paramName}.Count);");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                writer.Indent++;
                WriteFieldType(manager, classType.GenericTypes[0], keyName, depth + 1, writer);
                WriteFieldType(manager, classType.GenericTypes[1], valueName, depth + 1, writer);
                writer.Indent--;
                writer.WriteLine("}");
            }
        }

        private static string MakeReadMethodName(ClassName className)
        {
            var shortName = GetPrimitiveName(className);
            return $"Read{shortName}";
        }
        private static string GetPrimitiveName(ClassName className)
        {
            var lastDot = className.Value.LastIndexOf('.');
            return className.Value.Substring(lastDot + 1);
        }

        private static string CreateGenericClassString(IReadOnlyList<ClassGeneric> generics)
        {
            var genericsString = "";
            if (generics.Any())
            {
                genericsString = "<";
                foreach (var generic in generics)
                {
                    if (genericsString.Length > 1)
                    {
                        genericsString += ", ";
                    }
                    genericsString += generic.Name;
                }
                genericsString += ">";
            }

            return genericsString;
        }

        private static string CreateGenericConstraintsString(IReadOnlyList<ClassGeneric> generics)
        {
            var constraints = "";
            foreach (var generic in generics)
            {
                if (!generic.Constraints.Any())
                {
                    continue;
                }

                var constraintTypes = string.Join(", ", generic.Constraints.Select(c => c.Name.Value));
                constraints += $" where {generic.Name} : {constraintTypes}\n";
            }
            return constraints;
        }

        private static void ReadClass(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            var readName = MakeReadMethodName(classObject.FullName);
            var generics = CreateGenericClassString(classObject.Generics);
            var constraints = CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static {classObject.FullName.Value}{generics} {readName}{generics}(BinaryReader input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            ReadFields(manager, classObject, writer);
            writer.Indent--;

            writer.WriteLine("}");
        }

        private static void ReadBaseClass(Manager manager, ClassBaseObject classBaseObject, IndentedTextWriter writer)
        {
            var readName = MakeReadMethodName(classBaseObject.FullName);
            var generics = CreateGenericClassString(classBaseObject.Generics);
            var constraints = CreateGenericConstraintsString(classBaseObject.Generics);

            writer.Write($"public static {classBaseObject.FullName.Value}{generics} {readName}{generics}(BinaryReader input)");
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
                    var shortName = GetPrimitiveName(subclassPair.Subclass.FullName);
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
            var generics = CreateGenericClassString(classObject.Generics);
            writer.WriteLine($"return new {classObject.FullName.Value}{generics}({ctorArgs});");
        }

        private static void ReadField(Manager manager, ClassField classField, IndentedTextWriter writer)
        {
             writer.WriteLine($"var {classField.Name} = {ReadFieldType(manager, classField.Name + "Value", classField.Type, 0, writer)};");
        }

        private static string ReadFieldType(Manager manager, string resultName, ClassType classType, int depth, IndentedTextWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                if (manager.IsKnownClassOrBase(classType.Name))
                {
                    var fullName = GetPrimitiveName(classType.Name);
                    var readName = $"Read{fullName}";
                    if (classType.GenericTypes.Any())
                    {
                        var generics = MakeGenericType(classType);
                        return $"{readName}<{generics}>(input)";
                    }
                    return $"{readName}(input)";
                }
                else
                {
                    var primitiveType = GetPrimitiveName(classType.Name);
                    return $"input.Read{primitiveType}()";
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                var primitiveType = GetPrimitiveName(classType.EnumUnderlayingType.Name);
                return $"({classType.Name.Value})input.Read{primitiveType}()";
            }
            else if (classType.CollectionType == CollectionType.List || classType.CollectionType == CollectionType.Array)
            {
                var countName = $"count{resultName}";
                var genericType = classType.GenericTypes.First();
                var genericTypeName = MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                var indexString = MakeIndexIterator(depth);

                writer.WriteLine($"var {countName} = input.ReadInt32();");
                writer.WriteLine($"var {resultName} = new {genericTypeName}({countName});");
                writer.WriteLine($"for (var {indexString} = 0; {indexString} < {countName}; {indexString}++)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"{resultName}.Add({ReadFieldType(manager, resultName + "_", genericType, depth + 1, writer)});");
                writer.Indent--;
                writer.WriteLine("}");

                return resultName;
            }
            else if (classType.CollectionType == CollectionType.Dictionary)
            {
                var countName = $"count{resultName}";
                var keyType = classType.GenericTypes[0];
                var valueType = classType.GenericTypes[1];
                var depthStr = depth == 0 ? "" : depth.ToString();
                var keyName = $"key{depthStr}";
                var valueName = $"value{depthStr}";
                var indexName = MakeIndexIterator(depth);
                var genericName = MakeGenericType(classType);

                writer.WriteLine($"var {countName} = input.ReadInt32();");
                writer.WriteLine($"var {resultName} = new {genericName}();");
                writer.WriteLine($"for (var {indexName} = 0; {indexName} < {countName}; {indexName}++)");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"var {keyName} = {ReadFieldType(manager, keyName, keyType, depth + 1, writer)};");
                writer.WriteLine($"var {valueName} = {ReadFieldType(manager, valueName, valueType, depth + 1, writer)};");
                writer.WriteLine($"{resultName}[{keyName}] = {valueName};");
                writer.Indent--;
                writer.WriteLine("}");

                return resultName;
            }

            return "OH NO";
        }

        private static string MakeGenericType(ClassType type)
        {
            if (type.CollectionType == CollectionType.List || type.CollectionType == CollectionType.Array)
            {
                return $"List<{MakeGenericType(type.GenericTypes[0])}>";
            }
            else if (type.CollectionType == CollectionType.Dictionary)
            {
                return $"Dictionary<{MakeGenericType(type.GenericTypes[0])}, {MakeGenericType(type.GenericTypes[1])}>";
            }

            if (type.GenericTypes.Any())
            {
                return string.Join(", ", type.GenericTypes.Select(MakeGenericType));
            }

            return type.Name.Value;
        }

        private static string MakeIndexIterator(int depth)
        {
            var multiply = (int)Math.Floor((float)depth / (float)IndexIterators.Count) + 1;
            var index = depth % IndexIterators.Count;

            var token = IndexIterators[index];
            return new string(token, multiply);
        }

        #endregion
    }
}