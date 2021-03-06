﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using C3.MonoGame;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace TilemapEditor
{
    // TODO: Make it impossible for the TileSelection to go outside of the screen.
    // TODO: Clear tileHoveredByMouseMarker if no Tile is marked(Like in DrawingArea).

    // TODO: When moving the TileSelection around, disable the tileHoveredByMouseMarker so that 
    //       that it doesn't flicker when the Mouse moves violently.

    // TODO: Make it possible to be able to resize the TileSelection during runtime by scaling the bounds rectangle.
    //       This would increase/decrease the number of Tiles on one row.

    public class TileSelection
    {
        #region Fields

        private Rectangle bounds;
        private Vector2 tileSize;
        private int numTilesPerRow;
        private Texture2D tileSet = null;
        private String tileSetName = String.Empty;
        private List<List<Tile>> tiles = new List<List<Tile>>();

        private Tile currentTile = null;
        private Tile tileHoveredByMouse = null;
        private Vector2 tileSpacing;

        private SpriteFont font;
        private Vector2 fontSize;
        private String text;

        private Rectangle tileHoveredByMouseMarker;
        private Rectangle currentTileMarker = Rectangle.Empty;

        private bool movementLocked = false;

        private bool hidden = false;

        #endregion

        #region Properties

        public bool Hidden
        {
            get { return hidden; }
            set { hidden = value; }
        }

        public Vector2 Position
        {
            get { return bounds.Location.ToVector2(); }
            set
            {
                bounds.Location = value.ToPoint();
                UpdateTileBounds();
            }
        }

        public Vector2 TileSize
        {
            get { return tileSize; }
            set { tileSize = value; }
        }

        public Rectangle Bounds
        {
            get { return bounds; }
        }

        public Tile CurrentTile
        {
            get { return currentTile; }
        }

        public Texture2D TileSet
        {
            get { return tileSet; }
            set { tileSet = value; }
        }

        public List<List<Tile>> Tiles
        {
            get { return tiles; }
            set { tiles = value; }
        }

        public bool IsHoveredByMouse
        {
            get; private set;
        }

        #endregion

        public TileSelection
        (
            Vector2 position,
            Vector2 tileSize,
            int numTilesPerRow,
            Vector2 tileSpacing
        )
        {
            this.tileSize = tileSize;
            this.numTilesPerRow = numTilesPerRow;
            this.tileSpacing = tileSpacing;

            bounds = new Rectangle(position.ToPoint(), Point.Zero);
        }

        #region PublicInterface

        public void Update(GameTime gameTime)
        {
            UpdateHiding();

            if (hidden)
                return;

            Vector2 currentMousePosition = InputManager.CurrentMousePosition();

            if (bounds.Contains(currentMousePosition))
            {
                IsHoveredByMouse = true;

                UpdateLockingMovement();
                UpdateDetectingHoveredTile(currentMousePosition);
                UpdateDetectingDiscardingCurrentTile();
            }
            else
            {
                IsHoveredByMouse = false;
                tileHoveredByMouse = null;
            }

            UpdateMovingTileSelection(currentMousePosition);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // TODO: Converting from Point to Vector2 and vice versa all the time is kinda stupid :|

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw all Tiles
            foreach (List<Tile> rows in tiles)
            {
                foreach (Tile tile in rows)
                {
                    spriteBatch.Draw(tileSet, tile.screenBounds.ToRectangle(),
                        tile.textureBounds.ToRectangle(), Color.White);
                }
            }

            // Draw TileSelection String, TileSelection's bounds, tileHoveredByMouseMarker and currentTileMarker.
            spriteBatch.DrawString(font, text, bounds.Location.ToVector2(), Color.White);
            Primitives2D.DrawRectangle(spriteBatch, bounds, Color.Green, 5);

            if (tileHoveredByMouse != null)
            {
                Primitives2D.DrawRectangle(spriteBatch, tileHoveredByMouseMarker, Color.AliceBlue, 5);
                spriteBatch.DrawString(font, tileHoveredByMouse.name,
                              InputManager.CurrentMousePosition() + new Vector2(0, -fontSize.Y), Color.White);
            }

            Primitives2D.DrawRectangle(spriteBatch, currentTileMarker, Color.DarkRed, 5);

            spriteBatch.End();
        }

        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("fonts/font_default");
            text = "Tile-Selection\nMovementLocked: false";
            fontSize = font.MeasureString(text);

            // We call these things here because they are dependant on fontSize.
            UpdateBounds();

            tileHoveredByMouse = tiles[0][0];
            UpdateTileBounds();
            tileHoveredByMouseMarker = tiles[0][0].screenBounds.ToRectangle();
        }

        public void ReadTilesFromFile(String file, ContentManager content)
        {
            TileSelectionData tileSelectionData = ConfigFileUtility.ReadTileSelectionFile(file, numTilesPerRow);
            tiles = tileSelectionData.tiles;
            tileSet = content.Load<Texture2D>(tileSelectionData.tileSetName);

            UpdateBounds();
        }

        #endregion

        #region PrivateUpdateHelper

        private void UpdateHiding()
        {
            if (InputManager.OnKeyPressed(Keys.H))
            {
                hidden = !hidden;
            }
        }

        private void UpdateLockingMovement()
        {
            if (InputManager.OnKeyPressed(Keys.M))
            {
                movementLocked = !movementLocked;
                text = "Tile-Selection\nMovementLocked: " + movementLocked.ToString();
            }
        }

        private void UpdateDetectingDiscardingCurrentTile()
        {
            // Detect currentTile
            if (InputManager.OnLeftMouseButtonDown())
            {
                currentTileMarker = tileHoveredByMouseMarker;
                currentTile = tileHoveredByMouse;
            }

            // Discard currentTile
            else if (InputManager.OnRightMouseButtonDown())
            {
                currentTileMarker = Rectangle.Empty;
                currentTile = null;
            }
        }

        private void UpdateDetectingHoveredTile(Vector2 currentMousePosition)
        {
            if (InputManager.HasMouseMoved)
            {
                bool breakOuterLoop = false;
                foreach (List<Tile> rows in tiles)
                {
                    foreach (Tile tile in rows)
                    {
                        if (tile.screenBounds.Contains(currentMousePosition))
                        {
                            tileHoveredByMouseMarker = tile.screenBounds.ToRectangle();
                            tileHoveredByMouse = tile;

                            // Break both loops if we found a Tile that the mouse currently hovers.
                            breakOuterLoop = true;
                            break;
                        }
                    }

                    if (breakOuterLoop)
                    {
                        break;
                    }
                }
            }
        }

        private void UpdateMovingTileSelection(Vector2 currentMousePosition)
        {
            if (!movementLocked && 
                IsHoveredByMouse &&
                InputManager.IsLeftMouseButtonDown())
            {
                Position += currentMousePosition - InputManager.PreviousMousePosition();
            }
        }

        private void UpdateBounds()
        {
            int widthFirstRow = (int)(tiles[0].Count * tileSize.X + (tiles[0].Count - 1) * tileSpacing.X);

            bounds.Width = fontSize.X > widthFirstRow ? (int)fontSize.X : widthFirstRow;
            bounds.Height = (int)(fontSize.Y + (tiles.Count * tileSize.Y + (tiles.Count - 1) * tileSpacing.Y));

            // width and height of bounds without text.
            //bounds.Width  = widthFirstRow;
            //bounds.Height = (int)(tiles.Count * tileSize.Y + (tiles.Count - 1) * tileSpacing.Y); 
        }

        private void UpdateTileBounds()
        {
            for (int i = 0; i < tiles.Count; ++i)
            {
                for (int j = 0; j < tiles[i].Count; ++j)
                {
                    tiles[i][j].screenBounds = new Rectangle((bounds.Location.ToVector2() + new Vector2(j * tileSize.X + j * tileSpacing.X,
                                                     i * tileSize.Y + i * tileSpacing.Y + fontSize.Y)).ToPoint(), tileSize.ToPoint());
                }
            }

            if (tileHoveredByMouse != null)
            {
                tileHoveredByMouseMarker = tileHoveredByMouse.screenBounds.ToRectangle();
            }

            if (currentTile != null)
            {
                currentTileMarker = currentTile.screenBounds.ToRectangle();
            }
        }

        #endregion
    }
}
