using System;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public static class CreateBinary
    {
        #region Methods
        public static void SaveToFolder(Manager manager, string folder)
        {
            Directory.CreateDirectory(folder);

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

        private static string MakeWriteMethodName(ClassObject classObject)
        {
            return "Write";
        }

        private static void WriteClass(Manager manager, ClassObject classObject, IndentedTextWriter writer)
        {
            var writeName = MakeWriteMethodName(classObject);
            var generics = CreateGenericClassString(classObject);
            var constraints = CreateGenericConstraintsString(classObject);

            writer.Write($"public static void {writeName}{generics}({classObject.FullName.Value}{generics} input, BinaryWriter output)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            WriteFields(manager, classObject, writer);
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
                if (manager.ClassMap.TryGetValue(classType.Name, out var fieldClass))
                {
                    var writeName = MakeWriteMethodName(fieldClass);
                    writer.WriteLine($"{writeName}({paramName}, output);");
                }
                else
                {
                    writer.WriteLine($"output.Write({paramName});");
                }
            }
            else if (classType.CollectionType == CollectionType.Enum)
            {
                writer.WriteLine($"output.Write((int){paramName});");
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

        private static string MakeReadMethodName(ClassObject classObject)
        {
            var fullName = GetPrimitiveName(classObject.FullName);
            return $"Read{fullName}";
        }
        private static string GetPrimitiveName(ClassName className)
        {
            var lastDot = className.Value.LastIndexOf('.');
            return className.Value.Substring(lastDot + 1);
        }

        private static string CreateGenericClassString(ClassObject classObject)
        {
            var generics = "";
            if (classObject.Generics.Any())
            {
                generics = "<";
                foreach (var generic in classObject.Generics)
                {
                    if (generics.Length > 1)
                    {
                        generics += ", ";
                    }
                    generics += generic.Name;
                }
                generics += ">";
            }

            return generics;
        }

        private static string CreateGenericConstraintsString(ClassObject classObject)
        {
            var constraints = "";
            foreach (var generic in classObject.Generics)
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
            var readName = MakeReadMethodName(classObject);
            var generics = CreateGenericClassString(classObject);
            var constraints = CreateGenericConstraintsString(classObject);

            writer.Write($"public static {classObject.FullName.Value}{generics} {readName}{generics}(BinaryReader input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            ReadFields(manager, classObject, writer);
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
            var generics = CreateGenericClassString(classObject);
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
                if (manager.ClassMap.TryGetValue(classType.Name, out var fieldClass))
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
                return $"({classType.Name.Value})input.ReadInt32();";
            }
            else if (classType.CollectionType == CollectionType.List || classType.CollectionType == CollectionType.Array)
            {
                var countName = $"count{resultName}";
                var genericType = classType.GenericTypes.First();
                var genericTypeName = MakeGenericType(classType);
                var depthStr = depth == 0 ? "" : depth.ToString();
                var iterator = $"iter{depthStr}";

                writer.WriteLine($"var {countName} = input.ReadInt32();");
                writer.WriteLine($"var {resultName} = new {genericTypeName}({countName});");
                writer.WriteLine($"for (var {iterator} = 0; {iterator} < {countName}; {iterator}++)");
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
                var indexName = $"i{depthStr}";
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

        #endregion
    }
}