// Auto generated BinarySerialiser for CSharpSerialiser.Definition

using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserBinarySerialiser
    {
        public static void Write(CSharpSerialiser.Definition input, BinaryWriter output)
        {
            output.Write(input.Name);
            output.Write(input.Age);
            output.Write(input.IsBool);
            output.Write((System.Int32)input.DefinitionType);
            
            output.Write(input.Positions.Count);
            foreach (var item in input.Positions)
            {
                
                output.Write(item.Count);
                foreach (var item1 in item)
                {
                    Write(item1, output);
                }

            }

            
            output.Write(input.BonusPositions.Count);
            foreach (var kvp in input.BonusPositions)
            {
                Write(kvp.Key, output);
                Write(kvp.Value, output);
            }

            
            output.Write(input.KeyValues.Count);
            foreach (var kvp in input.KeyValues)
            {
                output.Write(kvp.Key);
                output.Write(kvp.Value);
            }

            
            output.Write(input.KeyValues2.Count);
            foreach (var kvp in input.KeyValues2)
            {
                output.Write(kvp.Key);
                output.Write(kvp.Value);
            }

            
            output.Write(input.MapComponents.Count);
            foreach (var item in input.MapComponents)
            {
                Write(item, output);
            }

        }

        public static CSharpSerialiser.Definition ReadDefinition(BinaryReader input)
        {
            var name = input.ReadString();
            var age = input.ReadInt32();
            var isBool = input.ReadBoolean();
            var definitionType = (CSharpSerialiser.DefinitionType)input.ReadInt32();
            var countPositions = input.ReadInt32();
            var positions = new List<List<CSharpSerialiser.Vector2>>(countPositions);
            for (var i = 0; i < countPositions; i++)
            {
                var countPositionsI = input.ReadInt32();
                var positionsI = new List<CSharpSerialiser.Vector2>(countPositionsI);
                for (var j = 0; j < countPositionsI; j++)
                {
                    positionsI.Add(ReadVector2(input));
                }

                positions.Add(positionsI);
            }

            var countBonusPositions = input.ReadInt32();
            var bonusPositions = new Dictionary<CSharpSerialiser.Vector2, CSharpSerialiser.Vector2>();
            for (var i = 0; i < countBonusPositions; i++)
            {
                var key = ReadVector2(input);
                var value = ReadVector2(input);
                bonusPositions[key] = value;
            }

            var countKeyValues = input.ReadInt32();
            var keyValues = new Dictionary<System.String, System.Int32>();
            for (var i = 0; i < countKeyValues; i++)
            {
                var key = input.ReadString();
                var value = input.ReadInt32();
                keyValues[key] = value;
            }

            var countKeyValues2 = input.ReadInt32();
            var keyValues2 = new Dictionary<System.Int32, System.Boolean>();
            for (var i = 0; i < countKeyValues2; i++)
            {
                var key = input.ReadInt32();
                var value = input.ReadBoolean();
                keyValues2[key] = value;
            }

            var countMapComponents = input.ReadInt32();
            var mapComponents = new List<CSharpSerialiser.Component>(countMapComponents);
            for (var i = 0; i < countMapComponents; i++)
            {
                mapComponents.Add(ReadComponent(input));
            }

            return new CSharpSerialiser.Definition(name, age, isBool, definitionType, positions, bonusPositions, mapComponents, keyValues, keyValues2);
        }
    }
}
