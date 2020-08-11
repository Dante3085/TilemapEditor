using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TilemapEditor.src
{
    public class TileConverter : JsonConverter<Tile>
    {
        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.PropertyName &&
                reader.GetString() != "TILE")
            {
                throw new JsonException("Given json is not a Tile because it doesn't start with the key 'TILE'");
            }

            Tile newTile = new Tile();
            bool readName = false;
            bool readTextureBounds = false;
            bool readScreenBounds = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propName;
                    if ((propName = reader.GetString()) == "NAME")
                    {
                        reader.Read();
                        newTile.name = reader.GetString();

                        readName = true;
                    }
                    else if (propName == "TEXTURE_BOUNDS")
                    {
                        // Go to first number
                        reader.Read();
                        reader.Read();

                        newTile.textureBounds.X = reader.GetInt32();
                        reader.Read();
                        newTile.textureBounds.Y = reader.GetInt32();
                        reader.Read();
                        newTile.textureBounds.Width = reader.GetInt32();
                        reader.Read();
                        newTile.textureBounds.Height = reader.GetInt32();
                        reader.Read();

                        readTextureBounds = true;
                    }
                    else if (propName == "SCREEN_BOUNDS")
                    {
                        // Go to first number
                        reader.Read();
                        reader.Read();

                        newTile.screenBounds.X = reader.GetInt32();
                        reader.Read();
                        newTile.screenBounds.Y = reader.GetInt32();
                        reader.Read();
                        newTile.screenBounds.Width = reader.GetInt32();
                        reader.Read();
                        newTile.screenBounds.Height = reader.GetInt32();
                        reader.Read();

                        readScreenBounds = true;
                    }
                }

                if (readName && readTextureBounds && readScreenBounds)
                    break;
                else if (SequencePosition)
            }

            return newTile;
        }

        public override void Write(Utf8JsonWriter writer, Tile tile, JsonSerializerOptions options)
        {
            // Tile name
            writer.WritePropertyName("TILE");
            writer.WriteStartObject();

            // Tile name
            writer.WritePropertyName("NAME");
            writer.WriteStringValue(tile.name);

            // Texture bounds
            writer.WritePropertyName("TEXTURE_BOUNDS");
            writer.WriteStartArray();
            writer.WriteNumberValue(tile.textureBounds.X);
            writer.WriteNumberValue(tile.textureBounds.Y);
            writer.WriteNumberValue(tile.textureBounds.Width);
            writer.WriteNumberValue(tile.textureBounds.Height);
            writer.WriteEndArray();

            // Screen bounds
            writer.WritePropertyName("SCREEN_BOUNDS");
            writer.WriteStartArray();
            writer.WriteNumberValue(tile.screenBounds.X);
            writer.WriteNumberValue(tile.screenBounds.Y);
            writer.WriteNumberValue(tile.screenBounds.Width);
            writer.WriteNumberValue(tile.screenBounds.Height);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
