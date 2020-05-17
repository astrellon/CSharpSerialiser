using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace CSharpSerialiser
{
    [Serializable]
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

    [Serializable]
    public class Definition
    {
        public readonly string Name;
        public readonly int Age;
        public readonly bool IsBool;
        public readonly IReadOnlyList<IReadOnlyList<Vector2>> Positions;
        public readonly IReadOnlyDictionary<int, Vector2> BonusPositions;

        public Definition(string name, int age, bool isBool, IReadOnlyList<IReadOnlyList<Vector2>> positions, IReadOnlyDictionary<int, Vector2> bonusPositions)
        {
            this.Name = name;
            this.Age = age;
            this.IsBool = isBool;
            this.Positions = positions;
            this.BonusPositions = bonusPositions;
        }
    }

    [Serializable]
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
            for (var i = 0; i < 100; i++)
            {
                var positions_ = new List<Vector2>();
                for (var j = 0; j < 100; j++)
                {
                    positions_.Add(RandomVec2());
                }
            }

            var bonusPositions = new Dictionary<int, Vector2>();
            for (var i = 0; i < 100; i++)
            {
                bonusPositions[i] = RandomVec2();
            }
            return new Definition(name, Rand.Next(), Rand.NextDouble() > 0.5, positions, bonusPositions);
        }

        static void Main(string[] args)
        {
            var manager = new Manager(new string[]{"Doggo", "Serialiser"});

            manager.AddClass(Manager.CreateObjectFromType(typeof(Vector2)));
            manager.AddClass(Manager.CreateObjectFromType(typeof(Definition)));
            manager.AddClass(Manager.CreateObjectFromType(typeof(DefinitionStore)));

            using (var file = File.OpenWrite("./TestBinSerialiser.cs"))
            {
                CreateBinary.SaveToStream(manager, file);
            }
        }

        /*
        static void Main(string[] args)
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/mnt/Velma/Unity/2019.3.12f1/Editor/Data/Managed/UnityEngine/UnityEngine.dll");
            AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/mnt/Velma/Unity/2019.3.12f1/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll");
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(@"/home/alan/git/space-doggo-3/Library/ScriptAssemblies/Assembly-CSharp.dll");

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    Console.WriteLine(type.FullName);
                    foreach (var field in type.GetFields())
                    {
                        var fieldType = (Type)null;
                        try
                        {
                            fieldType = field.FieldType;
                        }
                        catch (IOException)
                        {
                            fieldType = null;
                        }

                        Console.WriteLine(" - " + field.Name + ": " + fieldType);
                        //Console.WriteLine("- " + field.Name + ": " + field.FieldType.FullName);
                    }
                }
            }
        }
        */
    }
}
