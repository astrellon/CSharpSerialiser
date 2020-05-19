// Auto generated BinarySerialiser for CSharpSerialiser.DefinitionStore

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
    {
        public static void Write(CSharpSerialiser.DefinitionStore input, BinaryWriter output)
        {
            output.Write(input.Definitions.Count);
            foreach (var item in input.Definitions)
            {
                Write(item, output);
            }
        }
        public static CSharpSerialiser.DefinitionStore ReadDefinitionStore(NopBinaryReader input)
        {
            var countDefinitionsValue = input.ReadInt32();
            var DefinitionsValue = new List<CSharpSerialiser.Definition>(countDefinitionsValue);
            for (var i = 0; i < countDefinitionsValue; i++)
            {
                DefinitionsValue.Add(ReadDefinition(input));
            }
            var Definitions = DefinitionsValue;
            return new CSharpSerialiser.DefinitionStore(Definitions);
        }
    }
}
