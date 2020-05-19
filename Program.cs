﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.IO;

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

    public enum ComponentType
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
        public readonly IReadOnlyDictionary<int, Vector2> BonusPositions;
        public readonly IReadOnlyList<Component> MapComponents;

        public Definition(string name, int age, bool isBool, DefinitionType definitionType, IReadOnlyList<IReadOnlyList<Vector2>> positions, IReadOnlyDictionary<int, Vector2> bonusPositions, IReadOnlyList<Component> mapComponents)
        {
            this.Name = name;
            this.Age = age;
            this.IsBool = isBool;
            this.DefinitionType = definitionType;
            this.Positions = positions;
            this.BonusPositions = bonusPositions;
            this.MapComponents = mapComponents;
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

            return new Definition(name, Rand.Next(), Rand.NextDouble() > 0.5, DefinitionType.Engine, positions, bonusPositions, null);
        }

        static void Main(string[] args)
        {
            var manager = new Manager(new string[]{"Doggo", "Serialiser"}, "Doggo");

            manager.AddBaseObjectFromType(typeof(Component), "CompType");
            manager.AddClass(manager.CreateObjectFromType(typeof(Vector2)));
            manager.AddClass(manager.CreateObjectFromType(typeof(Definition)));
            manager.AddClass(manager.CreateObjectFromType(typeof(DefinitionStore)));

            CreateBinary.SaveToFolder(manager, "Serialisers");
        }

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

        //     CreateBinary.SaveToFolder(manager, "Serialisers");
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
