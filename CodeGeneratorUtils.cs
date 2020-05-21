using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public static class CodeGeneratorUtils
    {
        #region Fields
        public static readonly IReadOnlyList<char> IndexIterators = new []{'i', 'j', 'k', 'l'};

        public delegate void ReadFieldHandler(Manager manager, ClassField field, IndentedTextWriter writer);
        #endregion

        #region Methods
        public static string ToCamelCase(string input)
        {
            if (input.Length == 0)
            {
                return "null";
            }

            input = Regex.Replace((string)input, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLowerInvariant() + m.Groups[3].Value);

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
        public static string ToUpperCamelCase(string input)
        {
            if (input.Length == 0)
            {
                return "Null";
            }

            input = Regex.Replace((string)input, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLowerInvariant() + m.Groups[3].Value);
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }
        public static string ToCamelCase(ClassName input)
        {
            return ToCamelCase(input.Value);
        }

        public static string ToTitleCase(string input)
        {
            var first = input[0];
            if (Char.IsUpper(first))
            {
                return input;
            }

            return Char.ToUpperInvariant(first) + input.Substring(1);
        }

        public static string MakeGenericType(ClassType type)
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

        public static string MakeIndexIterator(int depth)
        {
            var multiply = (int)Math.Floor((float)depth / (float)IndexIterators.Count) + 1;
            var index = depth % IndexIterators.Count;

            var token = IndexIterators[index];
            return new string(token, multiply);
        }

        public static string MakeReadMethodName(ClassName className)
        {
            var shortName = GetPrimitiveName(className);
            return $"Read{shortName}";
        }
        public static string GetPrimitiveName(ClassName className)
        {
            var lastDot = className.Value.LastIndexOf('.');
            return className.Value.Substring(lastDot + 1);
        }

        public static string CreateGenericClassString(IReadOnlyList<ClassGeneric> generics)
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

        public static string CreateGenericConstraintsString(IReadOnlyList<ClassGeneric> generics)
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

        public static void ReadFieldsToCtor(Manager manager, ClassObject classObject, IndentedTextWriter writer, ReadFieldHandler readFieldHandler)
        {
            foreach (var field in classObject.Fields)
            {
                readFieldHandler(manager, field, writer);
            }

            var finalFieldOrder = (ClassField[])null;
            foreach (var ctorFields in classObject.CtorFields)
            {
                var fieldOrder = new ClassField[ctorFields.Count];
                for (var i = 0; i < ctorFields.Count; i++)
                {
                    var ctorField = ctorFields[i];
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
                    for (var i = 0; i < ctorFields.Count; i++)
                    {
                        if (fieldOrder[i] != null)
                        {
                            continue;
                        }

                        var ctorField = ctorFields[i];
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

                if (fieldOrder.All(fo => fo != null))
                {
                    finalFieldOrder = fieldOrder;
                    break;
                }
            }

            if (finalFieldOrder == null)
            {
                throw new Exception($"Unable to determin ctor parameters for: {classObject.FullName}");
            }

            var ctorArgs = string.Join(", ", finalFieldOrder.Select(f => f.Name).Select(CodeGeneratorUtils.ToCamelCase));
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            writer.WriteLine($"return new {classObject.FullName.Value}{generics}({ctorArgs});");
        }

        public static void WriteOuterClass(Manager manager, ClassName className, IndentedTextWriter writer, string classSuffix, IReadOnlyList<string> usingImports, Action writeInner)
        {
            writer.WriteLine($"// Auto generated {classSuffix} for {className}\n");

            foreach (var usingImport in usingImports)
            {
                writer.WriteLine($"using {usingImport};");
            }
            writer.WriteLine();

            writer.WriteLine($"namespace {string.Join('.', manager.NameSpace)}");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"public static partial class {manager.BaseSerialiserClassName}{classSuffix}");
            writer.WriteLine("{");
            writer.Indent++;

            writeInner();

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
        }

        #endregion
    }
}