// Auto generated BinarySerialiser for CSharpSerialiser.Component

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Component input, BinaryWriter output)
        {
            if (input is CSharpSerialiser.AirportComponent inputCSharpSerialiserAirportComponent)
            {
                output.Write((System.Byte)CSharpSerialiser.AirportComponent.CompType);
                Write(inputCSharpSerialiserAirportComponent, output);
            }
            else if (input is CSharpSerialiser.SeaportComponent inputCSharpSerialiserSeaportComponent)
            {
                output.Write((System.Byte)CSharpSerialiser.SeaportComponent.CompType);
                Write(inputCSharpSerialiserSeaportComponent, output);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
        public static CSharpSerialiser.Component ReadComponent(NopBinaryReader input)
        {
            var type = (CSharpSerialiser.ComponentType)input.ReadByte();
            if (type == CSharpSerialiser.AirportComponent.CompType)
            {
                return ReadAirportComponent(input);
            }
            else if (type == CSharpSerialiser.SeaportComponent.CompType)
            {
                return ReadSeaportComponent(input);
            }
            else
            {
                throw new Exception("Unknown base class type");
            }
        }
    }
}
