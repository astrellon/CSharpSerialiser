using System;
using System.Linq;
using System.IO;

namespace CSharpSerialiser
{
    public static class CreateBinary
    {
        #region Methods
        public static void SaveToStream(Manager manager, Stream output)
        {
            using (var writer = new StreamWriter(output))
            {
                writer.WriteLine("// Test output\n");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.IO;\n");
                writer.WriteLine("using System.Collections.Generic;\n");
                writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
                writer.WriteLine("{");
                writer.WriteLine($"public static partial class {manager.NameSpace.Last()}BinarySerialiser");
                writer.WriteLine("{");
                foreach (var kvp in manager.ClassMap)
                {
                    WriteClass(manager, kvp.Value, writer);
                    ReadClass(manager, kvp.Value, writer);
                }
                writer.WriteLine("}");
                writer.WriteLine("}");
            }
        }

        private static string MakeWriteMethodName(ClassObject classObject)
        {
            return $"Write";
        }

        private static void WriteClass(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            var writeName = MakeWriteMethodName(classObject);
            writer.WriteLine($"public static void {writeName}({classObject.FullName.Value} input, BinaryWriter output)");
            writer.WriteLine("{");

            WriteFields(manager, classObject, writer);

            writer.WriteLine("}");
        }

        private static void WriteFields(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            foreach (var field in classObject.Fields)
            {
                WriteField(manager, field, writer);
            }
        }

        private static void WriteField(Manager manager, ClassField classField, StreamWriter writer, string fieldNameOverride = null)
        {
            var inputFieldName = $"input.{classField.Name}";
            WriteFieldType(manager, classField.Type, inputFieldName, 0, writer);
        }

        public static void WriteFieldType(Manager manager, ClassType classType, string paramName, int depth, StreamWriter writer)
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
            else if (classType.CollectionType == CollectionType.Array || classType.CollectionType == CollectionType.List)
            {
                var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                writer.WriteLine($"output.Write({paramName}.Count);");
                writer.WriteLine($"foreach (var {itemName} in {paramName})");
                writer.WriteLine("{");
                WriteFieldType(manager, classType.GenericTypes.First(), itemName, depth + 1, writer);
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
                WriteFieldType(manager, classType.GenericTypes[0], keyName, depth + 1, writer);
                WriteFieldType(manager, classType.GenericTypes[1], valueName, depth + 1, writer);
                writer.WriteLine("}");
            }
        }

        private static string MakeReadMethodName(ClassName className)
        {
            var fullName = GetPrimitiveName(className);
            return $"Read{fullName}";
        }
        private static string GetPrimitiveName(ClassName className)
        {
            var lastDot = className.Value.LastIndexOf('.');
            return className.Value.Substring(lastDot + 1);
        }

        private static void ReadClass(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            var readName = MakeReadMethodName(classObject.FullName);
            writer.WriteLine($"public static {classObject.FullName.Value} {readName}(BinaryReader input)");
            writer.WriteLine("{");

            ReadFields(manager, classObject, writer);

            writer.WriteLine("}");
        }

        private static void ReadFields(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            foreach (var field in classObject.Fields)
            {
                ReadField(manager, field, writer);
            }

            var fieldOrder = classObject.CtorFields.Select(cf => classObject.Fields.First(f => f.Name.Equals(cf, StringComparison.OrdinalIgnoreCase)));
            var ctorArgs = string.Join(", ", fieldOrder.Select(f => f.Name));
            writer.WriteLine($"return new {classObject.FullName.Value}({ctorArgs});");
        }

        private static void ReadField(Manager manager, ClassField classField, StreamWriter writer)
        {
             writer.WriteLine($"var {classField.Name} = {ReadFieldType(manager, classField.Name + "Value", classField.Type, 0, writer)};");
        }

        private static string ReadFieldType(Manager manager, string resultName, ClassType classType, int depth, StreamWriter writer)
        {
            if (classType.CollectionType == CollectionType.NotACollection)
            {
                if (manager.ClassMap.TryGetValue(classType.Name, out var fieldClass))
                {
                    var readName = MakeReadMethodName(classType.Name);
                    return $"{readName}(input)";
                }
                else
                {
                    var primitiveType = GetPrimitiveName(classType.Name);
                    return $"input.Read{primitiveType}()";
                }
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
                writer.WriteLine($"{resultName}.Add({ReadFieldType(manager, resultName + "_", genericType, depth + 1, writer)});");
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
                writer.WriteLine($"var {keyName} = {ReadFieldType(manager, keyName, keyType, depth + 1, writer)};");
                writer.WriteLine($"var {valueName} = {ReadFieldType(manager, valueName, valueType, depth + 1, writer)};");
                writer.WriteLine($"{resultName}[{keyName}] = {valueName};");
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

            return type.Name.Value;
        }

        #endregion
    }
}