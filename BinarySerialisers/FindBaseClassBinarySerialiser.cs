// Auto generated BinarySerialiser for CSharpSerialiser.Config.FindBaseClass

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Config.FindBaseClass input, BinaryWriter output)
        {
            output.Write(input.TypeNameRegex);
            output.Write(input.TypeField);
        }

        public static CSharpSerialiser.Config.FindBaseClass ReadFindBaseClass(BinaryReader input)
        {
            var typeNameRegex = input.ReadString();
            var typeField = input.ReadString();
            return new CSharpSerialiser.Config.FindBaseClass(typeNameRegex, typeField);
        }
    }
}
