using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSharpSerialiser
{
    public static class CodeGeneratorUtils
    {
        #region Fields
        public static readonly IReadOnlyList<char> IndexIterators = new []{'i', 'j', 'k', 'l'};
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
        #endregion
    }
}