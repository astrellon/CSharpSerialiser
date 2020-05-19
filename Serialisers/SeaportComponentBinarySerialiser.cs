// Auto generated BinarySerialiser for CSharpSerialiser.SeaportComponent

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
    {
        public static void Write(CSharpSerialiser.SeaportComponent input, BinaryWriter output)
        {
            output.Write(input.Code);
            output.Write(input.Name);
        }
        public static CSharpSerialiser.SeaportComponent ReadSeaportComponent(NopBinaryReader input)
        {
            var Code = input.ReadString();
            var Name = input.ReadString();
            return new CSharpSerialiser.SeaportComponent(Name, Code);
        }
    }
}
