// Auto generated JsonSerialiser for List

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace CSharpSerialiser
{
    public static partial class CSharpSerialiserJsonSerialiser
    {
        public delegate void WriteJsonArrayItemHandler<T>(T item, Utf8JsonWriter writer);
        public delegate T ReadJsonArrayItemHandler<T>(JsonElement input);

        public static void Write<T>(IReadOnlyList<T> input, Utf8JsonWriter writer, WriteJsonArrayItemHandler<T> itemHandler)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                itemHandler(item, writer);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<int> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<uint> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<long> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<ulong> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<float> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<double> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<decimal> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteNumberValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<string> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteStringValue(item);
            }
            writer.WriteEndArray();
        }

        public static void Write(IReadOnlyList<bool> input, Utf8JsonWriter writer)
        {
            writer.WriteStartArray();
            foreach (var item in input)
            {
                writer.WriteBooleanValue(item);
            }
            writer.WriteEndArray();
        }

        public static IEnumerable<T> ReadList<T>(JsonElement input, ReadJsonArrayItemHandler<T> itemHandler)
        {
            foreach (var item in input.EnumerateArray())
            {
                yield return itemHandler(item);
            }
        }

        public static IEnumerable<string> ReadListString(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetString());
        }
        public static IEnumerable<int> ReadListInt32(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetInt32());
        }
        public static IEnumerable<long> ReadListInt64(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetInt64());
        }
        public static IEnumerable<float> ReadListSingle(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetSingle());
        }
        public static IEnumerable<double> ReadListDouble(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetDouble());
        }
        public static IEnumerable<decimal> ReadListDecimal(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetDecimal());
        }
        public static IEnumerable<bool> ReadListBoolean(JsonElement input)
        {
            return input.EnumerateArray().Select(v => v.GetBoolean());
        }
}
}
