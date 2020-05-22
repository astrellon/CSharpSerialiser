// Auto generated BinarySerialiser for CSharpSerialiser.AirportComponent

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.AirportComponent input, BinaryWriter output)
        {
            output.Write(input.IATA);
            output.Write(input.Name);
        }

        public static CSharpSerialiser.AirportComponent ReadAirportComponent(BinaryReader input)
        {
            var iata = input.ReadString();
            var name = input.ReadString();
            return new CSharpSerialiser.AirportComponent(name, iata);
        }
    }
}
