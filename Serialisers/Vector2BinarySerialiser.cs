// Auto generated BinarySerialiser for CSharpSerialiser.Vector2

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Vector2 input, BinaryWriter output)
        {
            output.Write(input.X);
            output.Write(input.Y);
        }
        public static CSharpSerialiser.Vector2 ReadVector2(NopBinaryReader input)
        {
            var X = input.ReadSingle();
            var Y = input.ReadSingle();
            return new CSharpSerialiser.Vector2(X, Y);
        }
    }
}
