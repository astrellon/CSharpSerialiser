// Auto generated BinarySerialiser for CSharpSerialiser.Config.FindClass

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Config.FindClass input, BinaryWriter output)
        {
            output.Write(input.TypeNameRegex);
        }

        public static CSharpSerialiser.Config.FindClass ReadFindClass(BinaryReader input)
        {
            var typeNameRegex = input.ReadString();
            return new CSharpSerialiser.Config.FindClass(typeNameRegex);
        }
    }
}
