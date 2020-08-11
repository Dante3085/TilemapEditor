using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace TilemapEditor
{
#if WINDOWS || LINUX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
