using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using TilemapEditor.src;

namespace TilemapEditor
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //using (var game = new Game1())
            //    game.Run();

            // JSON example
            //SomeData data = new SomeData();
            //data.age = 30;
            //data.height = 1.83f;
            //data.weight = 84.4f;

            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.WriteIndented = true;
            //string jsonString = JsonSerializer.Serialize(data, options);
            //File.WriteAllText("Test.json", jsonString);

            // JSON deserialization example with user defined type "TsData"
            //string jsonString = File.ReadAllText("configFiles/tilesets/jsonTilesetTest.ts.json");
            //TsData t = JsonSerializer.Deserialize<TsData>(jsonString);

            // JSON serialization example with user defined type "TsData"
            TsData t2 = new TsData();
            t2.AutoTiles = true;
            t2.Region = new List<int>() { 10, 22, 300, 400 };
            t2.TilesetPath = "tilesets/someTileset";
            t2.TileSize = new List<int>() { 16, 16 };

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string jsonString2 = JsonSerializer.Serialize(t2, options);
            File.WriteAllText("configFiles/tilesets/jsonTilesetTest2.ts.json", jsonString2);

        }
    }
#endif
}
