// Auto generated BinarySerialiser for CSharpSerialiser.DefinitionStore

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.DefinitionStore input, BinaryWriter output)
        {
            
            output.Write(input.Definitions.Count);
            foreach (var item in input.Definitions)
            {
                Write(item, output);
            }

        }

        public static CSharpSerialiser.DefinitionStore ReadDefinitionStore(BinaryReader input)
        {
            var countDefinitions = input.ReadInt32();
            var definitions = new List<CSharpSerialiser.Definition>(countDefinitions);
            for (var i = 0; i < countDefinitions; i++)
            {
                definitions.Add(ReadDefinition(input));
            }

            return new CSharpSerialiser.DefinitionStore(definitions);
        }
    }
}
