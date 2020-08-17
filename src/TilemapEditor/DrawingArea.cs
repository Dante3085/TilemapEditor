using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using C3.MonoGame;
using Microsoft.Xna.Framework.Input;
using System.Text.Json;
using System.IO;

using TilemapEditor.DrawingAreaComponents;

namespace TilemapEditor
{
    // TODO: Vernünftig kommentieren...
    // TODO: Maybe make DrawingArea Mouse Tile hovering more performant by using Spacial-Partitioning(Grid, BSPTree).
    // TODO: Besides hoveredTile, create the notion of currentTile in DrawingArea(Red Marker).
    // TODO: Display number of Tiles in DrawingArea.
    // TODO: Rectangle selection of many Tiles and move them around as a unit.
    // TODO: Copy content of Rectangle selection with STRG+C and place it at mouse position with STRG+V.
    // TODO: Make lining up Tiles when moving a Tile easy. Select a Tile and move 1 unit with arrow keys.
    // TODO: Make deleting Tiles in DrawingArea possible.

    // TODO: Make it possible to scale the currentTile of DrawingArea by placing Mouse at bottom right corner, 
    //       pressing LeftMouseButton and moving the Mouse around.

    // TODO: Make moving of DrawingArea possible so that a DrawingArea bigger than the Screen is still accessible.
    //       Make it impossible that the whole DrawingArea goes outside the screen.
    //       An alternative is using the Camera class, but that maybe presents other problems.

    // TODO: Make it possible to draw multiple Tiles in quick succession.

    // TODO: InfoText ergänzen. Grid und TileSelection Hide show.

    public class DrawingArea
    {
        #region Fields

        private Rectangle bounds;
        private List<Tile> tiles = new List<Tile>();

        private TileSelection tileSelection;
        private Camera camera = new Camera();
        private TileSelector tileSelector = new TileSelector();
        private TileMover tileMover = new TileMover();
        private TileDrawer tileDrawer = new TileDrawer();
        private Grid grid;
        private TileHistory tileHistory = new TileHistory(50);

        private Vector2 currentMousePosition = Vector2.Zero;
        private Vector2 mouseTravel = Vector2.Zero;

        #endregion

        #region Properties

        public List<Tile> Tiles
        {
            get { return tiles; }
            set { tiles = value; }
        }

        public int NumTilesSelection
        {
            get { return tileSelector.SelectedTiles.Count; }
        }

        public int NumTilesCopyBuffer
        {
            get { return tileDrawer.CopyBuffer.Count; }
        }

        public String SelectionInfo
        {
            get
            {
                if (tileSelector.SelectedTiles.Count == 1)
                {
                    return tileSelector.SelectedTiles[0].ToString();
                }
                else if (tileSelector.SelectedTiles.Count > 1)
                {
                    return "SelectedTilesMinimalBoundingBox: " + tileSelector.SelectedTilesMinimalBoundingBox.ToString();
                }
                else
                {
                    return "No Tile selected";
                }
            }
        }
        public int GridCellSize
        {
            get { return grid.GridCellSize; }
           set { grid.GridCellSize = value; }
        }

        public Vector2 ZoomedMousePosition
        {
            get { return currentMousePosition; }
        }

        #endregion

        public DrawingArea(Rectangle bounds, TileSelection tileSelection)
        {
            this.bounds = bounds;
            this.tileSelection = tileSelection;

            grid = new Grid((int)tileSelection.TileSize.X);
        }

        #region PublicInterface

        public void Update(GameTime gameTime)
        {
            currentMousePosition = camera.CurrentMousePosition;
            Vector2 previousMousePosition = camera.PreviousMousePosition;
            mouseTravel = currentMousePosition - previousMousePosition;

            // Update all DrawingArea components.
            camera.Update();

            tileSelector.Update(!tileSelection.Hidden, tileSelection.IsHoveredByMouse, tileDrawer.DrawTileSelectionCurrentTileOnMouse, tileMover.MovingSelectedTilesWithMouse,
                                tiles, currentMousePosition, gameTime);

            tileMover.Update(!tileSelection.Hidden, tileSelection.IsHoveredByMouse, tileSelector.SelectionBoxHasStartPoint, false, tileSelector.SelectedTiles, tiles, 
                             ref tileSelector.SelectedTilesMinimalBoundingBox, currentMousePosition, mouseTravel, gameTime, grid, tileHistory);

            tileDrawer.Update(tileSelection.CurrentTile, !tileSelection.IsHoveredByMouse, tileSelection.Hidden, tiles, tileSelector.SelectedTiles, currentMousePosition,
                              ref tileSelector.SelectedTilesMinimalBoundingBox, grid, tileHistory);

            grid.Update();
            tileHistory.Update(tiles, tileSelector);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.ZoomMatrix);

            // Draw all drawable DrawingArea components.
            tileDrawer.Draw(spriteBatch, tiles, tileSelection.TileSet, false, currentMousePosition, tileSelection.CurrentTile);
            grid.Draw(spriteBatch, bounds, camera.Zoom, camera.Position);
            tileSelector.Draw(spriteBatch, !tileMover.MovingSelectedTilesWithMouse, !tileMover.MovingSelectedTilesWithKeys, true);

            spriteBatch.End();
        }

        public void SaveToFile(String path)
        {
            if (!path.EndsWith(".tm.json"))
            {
                throw new FormatException("Given file '" + path + "' is not a tm(Tilemap)File.\n" +
                    "Provide a file that ends with '.tm.json'.");
            }

            FileStream fileStream = new FileStream(path, FileMode.Create);
            JsonWriterOptions writerOptions = new JsonWriterOptions();
            writerOptions.Indented = true;
            Utf8JsonWriter writer = new Utf8JsonWriter(fileStream, writerOptions);

            writer.WriteStartObject();

            writer.WritePropertyName("TILESET");
            writer.WriteStringValue("tilesets/HOW_TO_GET_TILESET_NAME_?");

            writer.WritePropertyName("TILES");
            writer.WriteStartObject();

            for (int i = 0; i < tiles.Count; ++i)
            {
                Tile tile = tiles[i];

                writer.WritePropertyName("TILE_" + (i + 1));
                writer.WriteStartObject();

                // Name
                writer.WritePropertyName("NAME");
                writer.WriteStringValue(tile.name);

                // TextureBounds
                writer.WritePropertyName("TEXTURE_BOUNDS");
                writer.WriteStartArray();
                writer.WriteNumberValue(tile.textureBounds.X);
                writer.WriteNumberValue(tile.textureBounds.Y);
                writer.WriteNumberValue(tile.textureBounds.Width);
                writer.WriteNumberValue(tile.textureBounds.Height);
                writer.WriteEndArray();

                // ScreenBounds
                writer.WritePropertyName("SCREEN_BOUNDS");
                writer.WriteStartArray();
                writer.WriteNumberValue(tile.screenBounds.X);
                writer.WriteNumberValue(tile.screenBounds.Y);
                writer.WriteNumberValue(tile.screenBounds.Width);
                writer.WriteNumberValue(tile.screenBounds.Height);
                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.Dispose();
            fileStream.Dispose();
        }

        #endregion
    }
}
