using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System.Text.Json;

namespace CSharpSerialiser
{
    public class Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public enum DefinitionType
    {
        Engine, PowerSource, Teleporter
    }

    public enum ComponentType : byte
    {
        Unknown, Airport, Seaport
    }

    public abstract class Component
    {
        public readonly string Name;

        public Component(string name)
        {
            this.Name = name;
        }
    }

    public class AirportComponent : Component
    {
        public static readonly ComponentType CompType = ComponentType.Airport;

        public readonly string IATA;

        public AirportComponent(string name, string iata) : base(name)
        {
            this.IATA = iata;
        }
    }

    public class SeaportComponent : Component
    {
        public static readonly ComponentType CompType = ComponentType.Seaport;

        public readonly string Code;

        public SeaportComponent(string name, string code) : base(name)
        {
            this.Code = code;
        }
    }

    public class Definition
    {
        public readonly string Name;
        public readonly int Age;
        public readonly bool IsBool;
        public readonly DefinitionType DefinitionType;
        public readonly IReadOnlyList<IReadOnlyList<Vector2>> Positions;
        public readonly IReadOnlyDictionary<Vector2, Vector2> BonusPositions;
        public readonly IReadOnlyDictionary<string, int> KeyValues;
        public readonly IReadOnlyDictionary<int, bool> KeyValues2;
        public readonly IReadOnlyList<Component> MapComponents;

        public Definition(string name, int age, bool isBool, DefinitionType definitionType, IReadOnlyList<IReadOnlyList<Vector2>> positions, IReadOnlyDictionary<Vector2, Vector2> bonusPositions, IReadOnlyList<Component> mapComponents, IReadOnlyDictionary<string, int> keyValues, IReadOnlyDictionary<int, bool> keyValues2)
        {
            this.Name = name;
            this.Age = age;
            this.IsBool = isBool;
            this.DefinitionType = definitionType;
            this.Positions = positions;
            this.BonusPositions = bonusPositions;
            this.MapComponents = mapComponents;
            this.KeyValues = keyValues;
            this.KeyValues2 = keyValues2;
        }
    }

    public class DefinitionStore
    {
        public readonly IReadOnlyList<Definition> Definitions;

        public DefinitionStore(IReadOnlyList<Definition> definitions)
        {
            this.Definitions = definitions;
        }
    }

    class Program
    {
        private static readonly Random Rand = new Random();

        static Vector2 RandomVec2()
        {
            return new Vector2((float)Rand.NextDouble(), (float)Rand.NextDouble());
        }

        static Definition RandomDef()
        {
            var name = "NAME_" + Rand.Next();
            var positions = new List<List<Vector2>>();
            for (var i = 0; i < 0; i++)
            {
                var positions_ = new List<Vector2>();
                for (var j = 0; j < 4; j++)
                {
                    positions_.Add(RandomVec2());
                }
                positions.Add(positions_);
            }

            var bonusPositions = new Dictionary<Vector2, Vector2>();
            for (var i = 0; i < 100; i++)
            {
                bonusPositions[RandomVec2()] = RandomVec2();
            }

            var keyValues = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
            {
                keyValues["KEY_" + Rand.Next()] = Rand.Next();
            }

            var keyValues2 = new Dictionary<int, bool>();
            for (var i = 0; i < 100; i++)
            {
                keyValues2[Rand.Next()] = Rand.NextDouble() > 0.5;
            }

            var comps = new List<Component>();
            for (var i = 0; i < 32; i++)
            {
                comps.Add(RandomComp());
            }

            return new Definition(name, Rand.Next(), Rand.NextDouble() > 0.5, DefinitionType.Engine, positions, bonusPositions, comps, keyValues, keyValues2);
        }

        static Component RandomComp()
        {
            if (Rand.NextDouble() > 0.5)
            {
                return new AirportComponent("AIRPORT_" + Rand.Next(), "IATA_" + Rand.Next());
            }
            else
            {
                return new SeaportComponent("SEAPORT_" + Rand.Next(), "CODE_" + Rand.Next());
            }
        }

        static void Main(string[] args)
        {
            var manager = new Manager(new string[]{"CSharpSerialiser"}, "CSharpSerialiser");

            // manager.AddBaseObjectFromType(typeof(Component), "CompType");
            // manager.AddClass(manager.CreateObjectFromType(typeof(Vector2)));
            // manager.AddClass(manager.CreateObjectFromType(typeof(Definition)));
            // manager.AddClass(manager.CreateObjectFromType(typeof(DefinitionStore)));
            manager.AddClass(manager.CreateObjectFromType(typeof(Config)));
            manager.AddClass(manager.CreateObjectFromType(typeof(Config.FindBaseClass)));
            manager.AddClass(manager.CreateObjectFromType(typeof(Config.FindClass)));

            // CreateBinary.SaveToFolder(manager, "BinarySerialisers");
            CreateJson.SaveToFolder(manager, "JsonSerialisers");
        }

        // static void Main(string[] args)
        // {
        //     var defs = new List<Definition>();
        //     for (var i = 0; i < 10000; i++)
        //     {
        //         defs.Add(RandomDef());
        //     }
        //     var store = new DefinitionStore(defs);

        //     var sw = new Stopwatch();
        //     using (var file = File.Open("test.bin", FileMode.Create))
        //     using (var writer = new BinaryWriter(file))
        //     {
        //         sw.Restart();
        //         CSharpSerialiserBinarySerialiser.Write(store, writer);
        //         sw.Stop();
        //     }

        //     Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to save");

        //     sw.Restart();
        //     using (var file = File.OpenRead("test.bin"))
        //     //using (var document = JsonDocument.Parse(file))
        //     using (var reader = new BinaryReader(file))
        //     {
        //         CSharpSerialiserBinarySerialiser.ReadDefinitionStore(reader);
        //         sw.Stop();
        //     }

        //     Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms to read");
        // }

        //     // var sw = new Stopwatch();

        //     // using (var file = File.OpenWrite("test.bin"))
        //     // using (var output = new BinaryWriter(file))
        //     // {
        //     //     sw.Start();
        //     //     Doggo.Serialiser.DoggoBinarySerialiser.Write(store, output);
        //     //     sw.Stop();
        //     // }

        //     // Console.WriteLine($"Saving took: {sw.ElapsedMilliseconds}ms");

        //     var sw = new Stopwatch();
        //     // var mem = new MemoryStream();
        //     // using (var file = File.OpenRead("test.bin"))
        //     // {
        //     //     file.CopyTo(mem);
        //     // }

        //     // mem.Seek(0, SeekOrigin.Begin);

        //     using (var file = File.OpenRead("test.bin"))
        //     using (var input = new BinaryReader(file))
        //     {
        //         sw.Restart();
        //         Doggo.Serialiser.DoggoBinarySerialiser.ReadDefinitionStore(input);
        //         sw.Stop();

        //         Console.WriteLine($"{input.boolSw.ElapsedMilliseconds}ms bools");
        //         Console.WriteLine($"{input.byteSw.ElapsedMilliseconds}ms bytes");
        //         Console.WriteLine($"{input.intSw.ElapsedMilliseconds}ms ints");
        //         Console.WriteLine($"{input.floatSw.ElapsedMilliseconds}ms floats");
        //         Console.WriteLine($"{input.stringSw.ElapsedMilliseconds}ms strings");
        //     }

        //     Console.WriteLine($"Reading took: {sw.ElapsedMilliseconds}ms");
        // }

        // static void Main(string[] args)
        // {
        //     AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/mnt/Velma/Unity/2019.3.12f1/Editor/Data/Managed/UnityEngine/UnityEngine.dll");
        //     var unityCoreAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/mnt/Velma/Unity/2019.3.12f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll");
        //     AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/mnt/Velma/Unity/2019.3.12f1/Editor/Data/Managed/UnityEngine/UnityEngine.SharedInternalsModule.dll");
        //     var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/home/alan/git/space-doggo-3/Library/ScriptAssemblies/Assembly-CSharp.dll");

        //     var manager = new Manager(new []{"Doggo.Serialisers"}, "Doggo");

        //     foreach (var module in unityCoreAssembly.Modules)
        //     {
        //         TryAddType(manager, module, "UnityEngine.Vector3");
        //         TryAddType(manager, module, "UnityEngine.Vector2");
        //     }

        //     foreach (var module in assembly.Modules)
        //     {
        //         foreach (var type in module.GetTypes())
        //         {
        //             var typeName = ClassName.ProcessTypeName(type.FullName);
        //             if ((typeName.EndsWith("Definition") || typeName.EndsWith("Id")) && typeName.StartsWith("Doggo"))
        //             {
        //                 Console.WriteLine(typeName);
        //                 manager.AddClass(manager.CreateObjectFromType(type));
        //             }
        //         }
        //     }

        //     CreateBinary.SaveToFolder(manager, "BinarySerialisers");
        //     CreateJson.SaveToFolder(manager, "JsonSerialisers");
        // }

        private static bool TryAddType(Manager manager, Module module, string typeName)
        {
            var vector3 = module.GetType(typeName);
            if (vector3 != null)
            {
                manager.AddClass(manager.CreateObjectFromType(vector3));
                return true;
            }

            return false;
        }
    }
}
