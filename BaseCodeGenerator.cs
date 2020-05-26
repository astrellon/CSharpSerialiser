using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;

namespace CSharpSerialiser
{
    public abstract class BaseCodeGenerator
    {
        #region Fields
        protected readonly Manager manager;
        protected IndentedTextWriter writer { get; private set; }

        public abstract string WriteObject { get; }
        public abstract string ReadObject { get; }
        public abstract string FileSuffix { get; }
        public abstract IEnumerable<string> UsingImports { get; }
        #endregion

        #region Constructor
        public BaseCodeGenerator(Manager manager)
        {
            this.manager = manager;
        }
        #endregion

        #region Methods
        public void SaveToFolder(string folder)
        {
            Directory.CreateDirectory(folder);

            foreach (var classBaseObject in this.manager.ClassBaseObjectMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classBaseObject.FullName)}JsonSerialiser.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.Open(outputFile, FileMode.Create))
                {
                    SaveToStream(classBaseObject, file);
                }
            }

            foreach (var classObject in manager.ClassMap.Values)
            {
                var filename = $"{CodeGeneratorUtils.GetPrimitiveName(classObject.FullName)}{this.FileSuffix}.cs";
                var outputFile = Path.Combine(folder, filename);

                using (var file = File.Open(outputFile, FileMode.Create))
                {
                    SaveToStream(classObject, file);
                }
            }
        }

        public void SaveToStream(ClassBaseObject classBaseObject, Stream output)
        {
            using (var streamWriter = new StreamWriter(output))
            using (this.writer = new IndentedTextWriter(streamWriter))
            {
                CodeGeneratorUtils.WriteOuterClass(this.manager, classBaseObject.FullName, writer, this.FileSuffix, this.UsingImports,
                    () =>
                    {
                        WriteBaseClass(classBaseObject);
                        ReadBaseClass(classBaseObject);
                    });
            }
        }

        public void SaveToStream(ClassObject classObject, Stream output)
        {
            using (var streamWriter = new StreamWriter(output))
            using (this.writer = new IndentedTextWriter(streamWriter))
            {
                CodeGeneratorUtils.WriteOuterClass(manager, classObject.FullName, writer, this.FileSuffix, this.UsingImports,
                    () =>
                    {
                        WriteClass(classObject);
                        ReadClass(classObject);
                    });
            }
        }

        protected virtual void WriteClass(ClassObject classObject)
        {
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classObject.Generics);

            this.WriteClassObjectMethod(generics, constraints, classObject);
            writer.WriteLine("{");

            writer.Indent++;
            WriteFields(classObject);
            writer.Indent--;

            writer.WriteLine("}\n");
        }

        protected virtual void WriteBaseClass(ClassBaseObject classBaseObject)
        {
            var generics = CodeGeneratorUtils.CreateGenericClassString(classBaseObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classBaseObject.Generics);

            if (!classBaseObject.Subclasses.Any())
            {
                writer.WriteLine($"// No derived classes found for base class: {classBaseObject.FullName.Value}");
                return;
            }

            this.WriterClassBaseObjectMethod(generics, constraints, classBaseObject);
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

                this.WriteBaseClassHandler(classBaseObject, subclass, castedName);

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

        protected abstract void WriteBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclass, string castedName);

        protected virtual void WriteClassObjectMethod(string generics, string constraints, ClassObject classObject)
        {
            writer.Write($"public static void Write{generics}({classObject.FullName.Value}{generics} input, {this.WriteObject} output)");
            writer.WriteLine(constraints);
        }

        protected virtual void WriterClassBaseObjectMethod(string generics, string constraints, ClassBaseObject classBaseObject)
        {
            writer.Write($"public static void Write{generics}({classBaseObject.FullName.Value}{generics} input, {this.WriteObject} output)");
            writer.WriteLine(constraints);
        }

        protected virtual void WriteFields(ClassObject classObject)
        {
            foreach (var field in classObject.Fields)
            {
                WriteField(field);
            }
        }

        protected abstract void WriteField(ClassField classField);

        protected virtual void ReadBaseClass(ClassBaseObject classBaseObject)
        {
            var readName = CodeGeneratorUtils.MakeReadMethodName(classBaseObject.FullName);
            var generics = CodeGeneratorUtils.CreateGenericClassString(classBaseObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classBaseObject.Generics);

            this.WriteReadClassBaseMethod(classBaseObject, readName, generics, constraints);
            writer.WriteLine("{");
            writer.Indent++;

            this.WriteReadBaseClassTypeHandler(classBaseObject);

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
                    this.WriteReadBaseClassHandler(classBaseObject, subclassPair);
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

        protected virtual void WriteReadClassBaseMethod(ClassBaseObject classBaseObject, string methodName, string generics, string constraints)
        {
            writer.Write($"public static {classBaseObject.FullName.Value}{generics} {methodName}{generics}({this.ReadObject} input)");
            writer.WriteLine(constraints);
        }

        // var inputField = $"input.GetProperty(\"{classBaseObject.TypeDiscriminator.CamelCaseName}\")";
        // writer.WriteLine($"var type = {ReadFieldType(manager, inputField, "type", classBaseObject.TypeDiscriminator.Type, 0, writer)};");
        protected abstract void WriteReadBaseClassTypeHandler(ClassBaseObject classBaseObject);

        // var shortName = CodeGeneratorUtils.GetPrimitiveName(subclassPair.Subclass.FullName);
        // writer.WriteLine($"return Read{shortName}(input);");
        protected abstract void WriteReadBaseClassHandler(ClassBaseObject classBaseObject, ClassBaseObject.SubclassPair subclassPair);

        protected virtual void ReadClass(ClassObject classObject)
        {
            var readName = CodeGeneratorUtils.MakeReadMethodName(classObject.FullName);
            var generics = CodeGeneratorUtils.CreateGenericClassString(classObject.Generics);
            var constraints = CodeGeneratorUtils.CreateGenericConstraintsString(classObject.Generics);

            writer.Write($"public static {this.TrimNameSpace(classObject.FullName)}{generics} {readName}{generics}({this.ReadObject} input)");
            writer.WriteLine(constraints);
            writer.WriteLine("{");

            writer.Indent++;
            this.ReadClassInner(classObject);
            writer.Indent--;

            writer.WriteLine("}");
        }

        protected abstract void ReadClassInner(ClassObject classObject);

        protected virtual string MakeReadValueMethod(ClassName className)
        {
            var shortName = className.TrimNameSpace(this.manager.NameSpace);
            return "Read" + shortName;
        }

        protected virtual string MakeGenericType(ClassType classType)
        {
            return CodeGeneratorUtils.MakeGenericType(classType, this.manager.NameSpace);
        }

        protected virtual string TrimNameSpace(ClassName className)
        {
            return className.TrimNameSpace(this.manager.NameSpace);
        }

        protected virtual void WriteReadFieldsToCtor(ClassObject classObject, CodeGeneratorUtils.ReadFieldHandler readFieldHandler)
        {
            CodeGeneratorUtils.ReadFieldsToCtor(this.manager, classObject, this.writer, readFieldHandler);
        }

        #endregion


    }
}