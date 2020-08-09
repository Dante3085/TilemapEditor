﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TilemapEditor
{
    public class Tile
    {
        public String name;
        public Rectangle textureBounds;
        public Rectangle screenBounds;

        public Tile()
        {
            name = "NOT_INITIALIZED";
            textureBounds = Rectangle.Empty;
            screenBounds = Rectangle.Empty;
        }

        public Tile(String name, Rectangle textureBounds, Rectangle screenBounds)
        {
            this.name = name;
            this.textureBounds = textureBounds;
            this.screenBounds = screenBounds;
        }

        public Tile(Tile otherTile)
        {
            this.name = otherTile.name;
            this.textureBounds = otherTile.textureBounds;
            this.screenBounds = otherTile.screenBounds;
        }

        public override string ToString()
        {
            return "TILE{" + name + ", " + Utility.RectangleToString(textureBounds) + ", " +
                   Utility.RectangleToString(screenBounds) + "}";
        }
    }
}
