// Auto generated BinarySerialiser for CSharpSerialiser.SeaportComponent

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.SeaportComponent input, BinaryWriter output)
        {
            output.Write(input.Code);
            output.Write(input.Name);
        }

        public static CSharpSerialiser.SeaportComponent ReadSeaportComponent(BinaryReader input)
        {
            var code = input.ReadString();
            var name = input.ReadString();
            return new CSharpSerialiser.SeaportComponent(name, code);
        }
    }
}
