using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using TilemapEditor.src;
using Microsoft.Xna.Framework;

namespace TilemapEditor
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            //using (var game = new Game1())
            //    game.Run();

            //Tile tile = new Tile("SomeTile", new Rectangle(1, 2, 3, 4), new Rectangle(5, 6, 7, 8));
            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.WriteIndented = true;
            //options.Converters.Add(new TileConverter());
            //string jsonString = JsonSerializer.Serialize(tile, options);
            //jsonString = jsonString.Insert(0, "{");
            //jsonString = jsonString.Insert(jsonString.Length - 1, "}");
            //File.WriteAllText("converterTest.json", jsonString);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new TileConverter());
            Tile tile = JsonSerializer.Deserialize<Tile>(File.ReadAllText("converterTest.json"), options);
        }
    }
#endif
}
