using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapEditor.src
{
    public class TsData
    {
        public string TilesetPath { get; set; }
        public bool AutoTiles { get; set; }
        public List<int> TileSize { get; set; }
        public List<int> Region { get; set; }
    }
}
