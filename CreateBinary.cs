using System.Linq;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public class CreateBinary : BaseCodeGenerator
    {
        #region Fields
        public override string WriteObject => "BinaryWriter";

        public override string ReadObject => "BinaryReader";

        public override string FileSuffix => "BinarySerialiser2";

        public override IEnumerable<string> UsingImports => new []
        { "System", "System.IO", "System.Collections.Generic" };
        #endregion

        #region Constructor
        public CreateBinary(Manager manager) : base(manager)
        {
        }
        #endregion

        #region Methods

        protected override void WriteBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclass, string castedName)
        {
            var paramName = $"{subclass.Subclass.FullName}.{classBaseObject.TypeDiscriminator.Name}";
            WriteFieldType(classBaseObject.TypeDiscriminator.Type, paramName, 0);
            writer.WriteLine($"Write({castedName}, output);");
        }

        protected override void WriteField(ClassField classField)
        {
            var inputFieldName = $"input.{classField.Name}";
            this.WriteFieldType(classField.Type, inputFieldName, 0);
        }

        protected override void WriteReadBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclassPair)
        {
            var shortName = CodeGeneratorUtils.GetPrimitiveName(subclassPair.Subclass.FullName);
            writer.WriteLine($"return Read{shortName}(input);");
        }

        protected override void WriteReadBaseClassTypeHandler(ClassBaseObject classBaseObject)
        {
            writer.WriteLine($"var type = {ReadFieldType("type", classBaseObject.TypeDiscriminator.Type, 0)};");
        }

        private void WriteFieldType(ClassType classType, string paramName, int depth)
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
                WriteFieldType(classType.GenericTypes.First(), itemName, depth + 1);
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
                WriteFieldType(classType.GenericTypes[0], keyName, depth + 1);
                WriteFieldType(classType.GenericTypes[1], valueName, depth + 1);
                writer.Indent--;
                writer.WriteLine("}\n");
            }
        }

        protected override void ReadClassInner(ClassObject classObject)
        {
            CodeGeneratorUtils.ReadFieldsToCtor(manager, classObject, writer, (manager, classField, writer) => this.ReadField(classField));
        }

        private void ReadField(ClassField classField)
        {
            var varString = classField.CamelCaseName;
            var valueString = ReadFieldType($"{classField.Name}", classField.Type, 0);

            if (varString != valueString)
            {
                writer.WriteLine($"var {varString} = {valueString};");
            }
        }

        private string ReadFieldType(string resultName, ClassType classType, int depth)
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
                writer.WriteLine($"{resultName}.Add({ReadFieldType(resultName + indexString.ToUpperInvariant(), genericType, depth + 1)});");
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
                writer.WriteLine($"var {keyName} = {ReadFieldType(keyName, keyType, depth + 1)};");
                writer.WriteLine($"var {valueName} = {ReadFieldType(valueName, valueType, depth + 1)};");
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