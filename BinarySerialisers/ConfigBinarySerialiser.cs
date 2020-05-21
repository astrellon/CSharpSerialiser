// Auto generated BinarySerialiser for CSharpSerialiser.Config

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Config input, BinaryWriter output)
        {
            
            output.Write(input.NameSpace.Count);
            foreach (var item in input.NameSpace)
            {
                output.Write(item);
            }

            output.Write(input.BaseSerialiserClassName);
            
            output.Write(input.FindBaseClasses.Count);
            foreach (var item in input.FindBaseClasses)
            {
                Write(item, output);
            }

            
            output.Write(input.FindClasses.Count);
            foreach (var item in input.FindClasses)
            {
                Write(item, output);
            }

        }

        public static CSharpSerialiser.Config ReadConfig(BinaryReader input)
        {
            var countNameSpace = input.ReadInt32();
            var nameSpace = new List<System.String>(countNameSpace);
            for (var i = 0; i < countNameSpace; i++)
            {
                nameSpace.Add(input.ReadString());
            }

            var baseSerialiserClassName = input.ReadString();
            var countFindBaseClasses = input.ReadInt32();
            var findBaseClasses = new List<CSharpSerialiser.Config.FindBaseClass>(countFindBaseClasses);
            for (var i = 0; i < countFindBaseClasses; i++)
            {
                findBaseClasses.Add(ReadFindBaseClass(input));
            }

            var countFindClasses = input.ReadInt32();
            var findClasses = new List<CSharpSerialiser.Config.FindClass>(countFindClasses);
            for (var i = 0; i < countFindClasses; i++)
            {
                findClasses.Add(ReadFindClass(input));
            }

            return new CSharpSerialiser.Config(nameSpace, baseSerialiserClassName, findBaseClasses, findClasses);
        }
    }
}
