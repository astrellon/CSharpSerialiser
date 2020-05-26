using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public class CreateBinary : BaseCodeGenerator
    {
        #region Fields
        public override string WriteObject => "BinaryWriter";

        public override string ReadObject => "BinaryReader";

        public override string FileSuffix => "BinarySerialiser";

        public override IEnumerable<string> UsingImports => new []
        { "System", "System.IO", "System.Collections.Generic" };
        #endregion

        #region Constructor
        public CreateBinary(Manager manager) : base(manager)
        {
        }
        #endregion

        #region Methods
        public override void SaveToFolder(string folder)
        {
            base.SaveToFolder(folder);

            SaveListHandlerToFolder(folder);
        }

        protected void SaveListHandlerToFolder(string folder)
        {
            var filename = $"List{this.FileSuffix}.cs";
            var outputFile = Path.Combine(folder, filename);

            using (var file = File.Open(outputFile, FileMode.Create))
            using (var streamWriter = new StreamWriter(file))
            using (this.writer = new IndentedTextWriter(streamWriter))
            {
                CodeGeneratorUtils.WriteOuterClass(this.manager, new ClassName("List"), this.writer, this.FileSuffix, this.UsingImports,
                    () =>
                    {
                        var template = File.ReadAllText("ListBinaryTemplate.txt");
                        this.writer.Write(template);
                    });
            }
        }

        protected override void WriteBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclass, string castedName)
        {
            var paramName = $"{this.TrimNameSpace(subclass.Subclass.FullName)}.{classBaseObject.TypeDiscriminator.Name}";
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
            writer.WriteLine($"return {this.MakeReadValueMethod(subclassPair.Subclass.FullName)}(input);");
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
                var genericType = classType.GenericTypes.First();
                if (this.manager.IsKnownClassOrBase(genericType.Name))
                {
                    writer.WriteLine($"Write({paramName}, output, Write);");
                }
                else
                {
                    if (TryGetReadListPrimitive(genericType.Name, out var listType))
                    {
                        writer.WriteLine($"Write({paramName}, output);");
                    }
                    else
                    {
                        var itemName = $"item{(depth == 0 ? "" : depth.ToString())}";
                        writer.WriteLine($"output.Write({paramName}.Count);");
                        writer.WriteLine($"foreach (var {itemName} in {paramName})");
                        writer.WriteLine("{");
                        writer.Indent++;
                        WriteFieldType(classType.GenericTypes.First(), itemName, depth + 1);
                        writer.Indent--;
                        writer.WriteLine("}");
                        writer.WriteLine();
                    }
                }
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
                WriteFieldType(classType.GenericTypes[0], keyName, depth + 1);
                WriteFieldType(classType.GenericTypes[1], valueName, depth + 1);
                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLine();
            }
        }

        protected override void ReadClassInner(ClassObject classObject)
        {
            this.WriteReadFieldsToCtor(classObject, this.ReadField);
        }

        private void ReadField(ClassField classField)
        {
            var varString = classField.SafeCamelCaseName;
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
                    var readName = this.MakeReadValueMethod(classType.Name);
                    if (classType.GenericTypes.Any())
                    {
                        var generics = this.MakeGenericType(classType);
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
                var genericType = classType.GenericTypes.First();
                var genericTypeName = this.MakeGenericType(classType);
                resultName = CodeGeneratorUtils.ToCamelCase(resultName);

                if (manager.IsKnownClassOrBase(genericType.Name))
                {
                    var readName = this.MakeReadValueMethod(genericType.Name);
                    if (genericType.GenericTypes.Any())
                    {
                        var generics = this.MakeGenericType(genericType);
                        readName += $"<{generics}>";
                    }

                    writer.WriteLine($"var {resultName} = new {genericTypeName}(ReadList(input, {readName}));");
                }
                else if (TryGetReadListPrimitive(genericType.Name, out var readListType))
                {
                    writer.WriteLine($"var {resultName} = new {genericTypeName}(ReadList{readListType}(input));");
                }
                else
                {
                    var countName = $"count{CodeGeneratorUtils.ToTitleCase(resultName)}";
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
                    writer.WriteLine("}");
                }
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
                var genericName = this.MakeGenericType(classType);
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
                writer.WriteLine("}");

                return resultName;
            }

            return "OH NO";
        }

        private static bool TryGetReadListPrimitive(ClassName className, out string result)
        {
            if (className.Value.StartsWith("System.") && className.Value.Count((char c) => c == '.') == 1)
            {
                var primitiveType = CodeGeneratorUtils.GetPrimitiveName(className);
                result = primitiveType;
                return true;
            }

            result = "";
            return false;
        }
        #endregion
    }
}