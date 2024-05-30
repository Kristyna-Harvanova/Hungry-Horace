using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HungryHorace
{
    class GameObjectConverter : JsonConverter<GameObject>
    {
        public override GameObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var element = doc.RootElement;

                if (!element.TryGetProperty("type", out var typeProperty))
                    throw new JsonException("Missing 'type' property for GameObject.");

                var objectType = Type.GetType(typeProperty.GetString());
                if (objectType == null || !typeof(GameObject).IsAssignableFrom(objectType))
                    throw new JsonException($"Invalid type '{typeProperty.GetString()}' for GameObject.");

                var gameObject = (GameObject)Activator.CreateInstance(objectType);

                // Deserialize common properties
                if (element.TryGetProperty("map", out var mapProperty))
                    gameObject.map = JsonSerializer.Deserialize<Map>(mapProperty.GetRawText(), options);

                if (element.TryGetProperty("position", out var positionProperty))
                    gameObject.position = JsonSerializer.Deserialize<VectorFloat>(positionProperty.GetRawText(), options);

                if (element.TryGetProperty("direction", out var directionProperty))
                    gameObject.direction = JsonSerializer.Deserialize<Direction>(directionProperty.GetRawText(), options);

                if (element.TryGetProperty("enabled", out var enabledProperty))
                    gameObject.enabled = enabledProperty.GetBoolean();

                if (element.TryGetProperty("speed", out var speedProperty))
                    gameObject.speed = speedProperty.GetInt32();

                // Deserialize specific properties based on derived class
                using (var stream = new MemoryStream())
                using (var writer = new Utf8JsonWriter(stream))
                {
                    element.WriteTo(writer);
                    writer.Flush();

                    gameObject = (GameObject)JsonSerializer.Deserialize(stream.ToArray(), objectType, options);
                }

                return gameObject;
            }
        }

        public override void Write(Utf8JsonWriter writer, GameObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("type", value.GetType().AssemblyQualifiedName);

            writer.WritePropertyName("map");
            JsonSerializer.Serialize(writer, value.map, options);

            writer.WritePropertyName("position");
            JsonSerializer.Serialize(writer, value.position, options);

            writer.WriteNumber("direction", (int)value.direction);

            writer.WriteBoolean("enabled", value.enabled);

            writer.WriteNumber("speed", value.speed);

            // Serialize specific properties based on derived class
            var derivedClassProperties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.DeclaringType != typeof(GameObject));

            foreach (var property in derivedClassProperties)
            {
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, property.GetValue(value), property.PropertyType, options);
            }

            writer.WriteEndObject();
        }
    }

    class MapConverter : JsonConverter<Map>
    {
        public override Map Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create a new Map object
            var map = new Map();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    if (propertyName == "width")
                    {
                        reader.Read();
                        map.width = reader.GetInt32();
                    }
                    else if (propertyName == "height")
                    {
                        reader.Read();
                        map.height = reader.GetInt32();
                    }
                    else if (propertyName == "map")
                    {
                        reader.Read();
                        map.map = JsonSerializer.Deserialize<Tile[,]>(ref reader, options);
                    }
                }
            }

            return map;
        }

        public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("width", value.width);
            writer.WriteNumber("height", value.height);

            writer.WritePropertyName("map");
            JsonSerializer.Serialize(writer, value.map, options);

            writer.WriteEndObject();
        }
    }

    class TileArrayConverter : JsonConverter<Tile[,]>
    {
        public override Tile[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    throw new JsonException("Invalid JSON format for Tile array.");

                var array = doc.RootElement.EnumerateArray().ToArray();

                int rows = array.Length;
                int cols = array[0].GetArrayLength();

                var tiles = new Tile[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    var row = array[i].EnumerateArray();

                    for (int j = 0; j < cols; j++)
                    {
                        var element = row.ElementAt(j);
                        tiles[i, j] = JsonSerializer.Deserialize<Tile>(element.GetRawText(), options);
                    }
                }

                return tiles;
            }
        }

        public override void Write(Utf8JsonWriter writer, Tile[,] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            int rows = value.GetLength(0);
            int cols = value.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                writer.WriteStartArray();
                for (int j = 0; j < cols; j++)
                {
                    JsonSerializer.Serialize(writer, value[i, j], options);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }
    }
}
