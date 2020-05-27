using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Runtime.Loader;

namespace CSharpSerialiser
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var file = File.OpenRead("selfConfig.json"))
            {
                var json = JsonDocument.Parse(file);
                var config = CSharpSerialiserJsonSerialiser.ReadConfig(json.RootElement);

                var manager = new Manager(config.NameSpace, config.BaseSerialiserClassName);

                LoadTargetProject(config.TargetProject);

                foreach (var findBaseClass in config.FindBaseClasses)
                {
                    AddBaseTypes(manager, findBaseClass.TypeNameRegex, findBaseClass.TypeField, null);
                }

                foreach (var findClass in config.FindClasses)
                {
                    AddTypes(manager, findClass.TypeNameRegex);
                }

                foreach (var formatConfig in config.FormatConfigs)
                {
                    if (formatConfig is Config.BinaryFormatConfig binaryFormat)
                    {
                        var createBinary = new CreateBinary(manager);
                        createBinary.SaveToFolder(binaryFormat.OutputFolder);
                    }
                    else if (formatConfig is Config.JsonFormatConfig jsonFormat)
                    {
                        var createJson = new CreateJson(manager);
                        createJson.SaveToFolder(jsonFormat.OutputFolder);
                    }
                    else if (formatConfig is Config.SimpleJsonFormatConfig simpleJsonFormat)
                    {
                        var createJsonSimple = new CreateSimpleJson(manager);
                        createJsonSimple.SaveToFolder(simpleJsonFormat.OutputFolder);
                    }
                }
            }
        }

        private static void LoadTargetProject(string targetFile)
        {
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(targetFile);

            if (xmlDoc.DocumentElement.Name != "Project")
            {
                throw new Exception("Unknown target project");
            }

            if (xmlDoc.DocumentElement.GetAttribute("Sdk") == "Microsoft.NET.Sdk")
            {
                // dotnet core project
                var targetFramework = xmlDoc.SelectSingleNode("//PropertyGroup/TargetFramework").InnerText;

                var filename = Path.GetFileNameWithoutExtension(targetFile) + ".dll";

                var filepath = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(targetFile)), "bin", "Debug", targetFramework, filename);

                AssemblyLoadContext.Default.LoadFromAssemblyPath(filepath);
            }
            else
            {
                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("m", "http://schemas.microsoft.com/developer/msbuild/2003");
                var unityReference = xmlDoc.SelectNodes("//m:ItemGroup/m:Reference", nsmgr);

                if (unityReference.Count == 0)
                {
                    throw new Exception("Unknown target project type");
                }

                //Console.WriteLine(unityReference);
                foreach (XmlNode reference in unityReference)
                {
                    var includeText = reference.Attributes.Item(0).InnerText;
                    if (includeText == "UnityEngine" ||
                        includeText == "UnityEngine.CoreModule" ||
                        includeText == "UnityEngine.SharedInternalsModule")
                    {
                        var path = reference.SelectSingleNode("./m:HintPath", nsmgr).InnerText;
                        Console.WriteLine("Loading Unity DLL: " + path);
                        AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                    }
                }

                var unityAssemblyName = xmlDoc.SelectSingleNode("//m:PropertyGroup/m:AssemblyName", nsmgr).InnerText;

                var filename = unityAssemblyName + ".dll";

                var filepath = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(targetFile)), "Library", "ScriptAssemblies", filename);
                Console.WriteLine("Loading game DLL: " + filepath);
                AssemblyLoadContext.Default.LoadFromAssemblyPath(filepath);
            }
        }

        private static bool TryFindType(string typeName, out Type result)
        {
            foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
            {
                result = assembly.GetType(typeName);
                if (result != null)
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static IEnumerable<Type> FindTypes(Regex typeName)
        {
            foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
            {
                var types = Utils.GetLoadedTypes(assembly);
                foreach (var type in types)
                {
                    if (string.IsNullOrWhiteSpace(type.FullName))
                    {
                        continue;
                    }

                    if (typeName.Match(type.FullName).Success)
                    {
                        yield return type;
                    }
                }
            }
        }

        private static void AddTypes(Manager manager, string typeName)
        {
            var regex = new Regex(typeName);

            foreach (var type in FindTypes(regex))
            {
                manager.AddClass(manager.CreateObjectFromType(type));
            }
        }
        private static void AddBaseTypes(Manager manager, string typeName, string typeDiscriminatorName, string interfaceBase)
        {
            var regex = new Regex(typeName);

            foreach (var type in FindTypes(regex))
            {
                var interfaceBaseType = (Type)null;
                if (!string.IsNullOrWhiteSpace(interfaceBase))
                {
                    interfaceBaseType = Type.GetType(interfaceBase);
                    if (interfaceBaseType == null)
                    {
                        throw new Exception($"Unable to find interface base: {interfaceBase}");
                    }
                }

                manager.AddBaseObjectFromType(type, typeDiscriminatorName, interfaceBaseType);
            }
        }
    }
}
