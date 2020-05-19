// Auto generated BinarySerialiser for CSharpSerialiser.AirportComponent

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
    {
        public static void Write(CSharpSerialiser.AirportComponent input, BinaryWriter output)
        {
            output.Write(input.IATA);
            output.Write(input.Name);
        }
        public static CSharpSerialiser.AirportComponent ReadAirportComponent(NopBinaryReader input)
        {
            var IATA = input.ReadString();
            var Name = input.ReadString();
            return new CSharpSerialiser.AirportComponent(Name, IATA);
        }
    }
}
