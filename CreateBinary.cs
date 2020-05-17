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
                writer.WriteLine($"namespace {manager.NameSpace}");
                writer.WriteLine("{");
                writer.WriteLine($"public static partial class {manager.NameSpace.Replace(".", "")}BinarySerialiser");
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
            var fullName = classObject.FullName.Value.Replace(".", "");
            return $"Write{fullName}";
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
                if (manager.ClassMap.TryGetValue(field.TypeFullName, out var fieldClass))
                {
                    var writeName = MakeWriteMethodName(fieldClass);
                    writer.WriteLine($"{writeName}(input.{field.Name}, output);");
                }
                else
                {
                    writer.WriteLine($"output.Write(input.{field.Name});");
                }
            }
        }

        private static string MakeReadMethodName(ClassObject classObject)
        {
            var fullName = classObject.FullName.Value.Replace(".", "");
            return $"Read{fullName}";
        }
        private static string GetPrimitiveName(ClassName className)
        {
            var lastDot = className.Value.LastIndexOf('.');
            return className.Value.Substring(lastDot + 1);
        }

        private static void ReadClass(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            var readName = MakeReadMethodName(classObject);
            writer.WriteLine($"public static {classObject.FullName.Value} {readName}(BinaryReader input)");
            writer.WriteLine("{");

            ReadFields(manager, classObject, writer);

            writer.WriteLine("}");
        }

        private static void ReadFields(Manager manager, ClassObject classObject, StreamWriter writer)
        {
            foreach (var field in classObject.Fields)
            {
                if (manager.ClassMap.TryGetValue(field.TypeFullName, out var fieldClass))
                {
                    var readName = MakeReadMethodName(fieldClass);
                    writer.WriteLine($"var {field.Name} = {readName}(input);");
                }
                else
                {
                    var primitiveType = GetPrimitiveName(field.TypeFullName);
                    writer.WriteLine($"var {field.Name} = input.Read{primitiveType}();");
                }
            }

            var fieldOrder = classObject.CtorFields.Select(cf => classObject.Fields.First(f => f.Name.Equals(cf, StringComparison.OrdinalIgnoreCase)));
            var ctorArgs = string.Join(", ", fieldOrder.Select(f => f.Name));
            writer.WriteLine($"return new {classObject.FullName.Value}({ctorArgs});");
        }
        #endregion
    }
}