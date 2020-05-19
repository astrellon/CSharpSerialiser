// Auto generated BinarySerialiser for CSharpSerialiser.Definition

using System;
using System.IO;

using System.Collections.Generic;

namespace Doggo.Serialiser
{
    public static partial class DoggoBinarySerialiser
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
                output.Write(kvp.Key);
                Write(kvp.Value, output);
            }
            output.Write(input.MapComponents.Count);
            foreach (var item in input.MapComponents)
            {
                Write(item, output);
            }
        }
        public static CSharpSerialiser.Definition ReadDefinition(NopBinaryReader input)
        {
            var Name = input.ReadString();
            var Age = input.ReadInt32();
            var IsBool = input.ReadBoolean();
            var DefinitionType = (CSharpSerialiser.DefinitionType)input.ReadInt32();
            var countPositionsValue = input.ReadInt32();
            var PositionsValue = new List<List<CSharpSerialiser.Vector2>>(countPositionsValue);
            for (var i = 0; i < countPositionsValue; i++)
            {
                var countPositionsValue_ = input.ReadInt32();
                var PositionsValue_ = new List<CSharpSerialiser.Vector2>(countPositionsValue_);
                for (var j = 0; j < countPositionsValue_; j++)
                {
                    PositionsValue_.Add(ReadVector2(input));
                }
                PositionsValue.Add(PositionsValue_);
            }
            var Positions = PositionsValue;
            var countBonusPositionsValue = input.ReadInt32();
            var BonusPositionsValue = new Dictionary<System.Int32, CSharpSerialiser.Vector2>();
            for (var i = 0; i < countBonusPositionsValue; i++)
            {
                var key = input.ReadInt32();
                var value = ReadVector2(input);
                BonusPositionsValue[key] = value;
            }
            var BonusPositions = BonusPositionsValue;
            var countMapComponentsValue = input.ReadInt32();
            var MapComponentsValue = new List<CSharpSerialiser.Component>(countMapComponentsValue);
            for (var i = 0; i < countMapComponentsValue; i++)
            {
                MapComponentsValue.Add(ReadComponent(input));
            }
            var MapComponents = MapComponentsValue;
            return new CSharpSerialiser.Definition(Name, Age, IsBool, DefinitionType, Positions, BonusPositions, MapComponents);
        }
    }
}
