// Auto generated BinarySerialiser for CSharpSerialiser.Vector2

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Vector2 input, BinaryWriter output)
        {
            output.Write(input.X);
            output.Write(input.Y);
        }

        public static CSharpSerialiser.Vector2 ReadVector2(BinaryReader input)
        {
            var x = input.ReadSingle();
            var y = input.ReadSingle();
            return new CSharpSerialiser.Vector2(x, y);
        }
    }
}
