using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;

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

    class Program
    {
        static void Main(string[] args)
        {
            var manager = new Manager("Doggo.Serialiser");

            manager.AddClass(Manager.CreateObjectFromType(typeof(Vector2)));
            manager.AddClass(Manager.CreateObjectFromType(typeof(Definition)));

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
