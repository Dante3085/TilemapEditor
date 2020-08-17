using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Handles drawing Tiles copying, cutting, pasting and deleting them in the DrawingArea
    /// </summary>
    public class TileDrawer
    {
        private bool drawTileSelectionCurrentTileOnMouse = false;
        private bool drawMultipleTilesAtOnce = false;
        private List<Tile> copyBuffer = new List<Tile>();

        public List<Tile> CopyBuffer { get => copyBuffer; }

        public bool DrawTileSelectionCurrentTileOnMouse { get => drawTileSelectionCurrentTileOnMouse; }

        public TileDrawer()
        {

        }

        #region PublicInterface

        public void Update
            (
            Tile tileSelectionCurrentTile,
            bool tileSelectionIsNotHoveredByMouse,
            bool tileSelectionIsHidden,
            List<Tile> tiles,
            List<Tile> selectedTiles,
            Vector2 currentMousePosition,
            ref RectangleF selectedTilesMinimalBoundingBox,
            Grid grid,
            TileHistory tileHistory
            )
        {
            UpdateTileDrawing(tileSelectionCurrentTile, tileSelectionIsNotHoveredByMouse, tileSelectionIsHidden, tiles, currentMousePosition, grid,
                              tileHistory);

            UpdateCopyingCuttingDeletingPastingTileSelection(!tileSelectionIsHidden, !tileSelectionIsNotHoveredByMouse, selectedTiles, tiles,
                                                             ref selectedTilesMinimalBoundingBox, currentMousePosition, tileHistory);
        }

        public void Draw
            (
            SpriteBatch spriteBatch,
            List<Tile> tiles, 
            Texture2D tileset, 
            bool collisionBoxMode,
            Vector2 currentMousePosition,
            Tile tileSelectionCurrentTile
            )
        {
            // Draw all Tiles on DrawingArea.
            foreach (Tile tile in tiles)
            {
                spriteBatch.Draw(tileset, tile.screenBounds.ToRectangle(), tile.textureBounds.ToRectangle(), 
                    collisionBoxMode ? Color.LightGray : Color.White);
            }

            // Draw tileSeleciton's currentTile.
            if (drawTileSelectionCurrentTileOnMouse)
            {
                Game1.MouseVisible = false;
                spriteBatch.Draw(tileset, new Rectangle(currentMousePosition.ToPoint(), 
                    tileSelectionCurrentTile.screenBounds.ToRectangle().Size), 
                    tileSelectionCurrentTile.textureBounds.ToRectangle(), Color.White);
            }
            else
            {
                Game1.MouseVisible = true;
            }
        }

        #endregion

        #region PrivateHelperMethods

        private void UpdateTileDrawing
            (
            Tile tileSelectionCurrentTile,
            bool tileSelectionIsNotHoveredByMouse,
            bool tileSelectionIsHidden,
            List<Tile> tiles,
            Vector2 currentMousePosition,
            Grid grid,
            TileHistory tileHistory
            )
        {
            
            // Draw one Tile or multiple Tiles in quick succession.
            if ((tileSelectionIsNotHoveredByMouse ||
                  tileSelectionIsHidden) &&
                tileSelectionCurrentTile != null)
            {
                drawTileSelectionCurrentTileOnMouse = true;

                if (InputManager.OnLeftMouseButtonClicked())
                {
                    DrawTileSelectionCurrentTile(tileSelectionCurrentTile, currentMousePosition, tiles, grid, tileHistory);
                    drawMultipleTilesAtOnce = true;
                }
                else if (InputManager.OnLeftMouseButtonReleased())
                {
                    drawMultipleTilesAtOnce = false;
                }

                if (drawMultipleTilesAtOnce)
                {
                    // If last added Tile and tileSelection's currentTile at currentMousePosition overlap, 
                    // draw tileSelection's currentTile again.
                    if (!tiles[tiles.Count - 1].screenBounds.Intersects(new RectangleF(currentMousePosition, 
                                                                            tileSelectionCurrentTile.screenBounds.Size)))
                    {
                        DrawTileSelectionCurrentTile(tileSelectionCurrentTile, currentMousePosition, tiles, grid, tileHistory);
                    }
                }
            }
            else
            {
                drawTileSelectionCurrentTileOnMouse = false;
            }
        } 

        private void DrawTileSelectionCurrentTile
            (
            Tile tileSelectionCurrentTile, 
            Vector2 currentMousePosition, 
            List<Tile> tiles, 
            Grid grid,
            TileHistory tileHistory
            )
        {
            Tile newTile = new Tile(tileSelectionCurrentTile.name, 
                tileSelectionCurrentTile.textureBounds, new RectangleF(currentMousePosition.ToPoint(), 
                tileSelectionCurrentTile.screenBounds.Size));

            if (grid.GridActivated)
                newTile.screenBounds.Position += grid.GetSnappingVectorForGivenPosition(newTile.screenBounds.Position);
            
            tiles.Add(newTile);
            tileHistory.AppendAddAction(new List<int>() { tiles.Count - 1 });
        }

        private void UpdateCopyingCuttingDeletingPastingTileSelection
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            List<Tile> selectedTiles,
            List<Tile> tiles,
            ref RectangleF selectedTilesMinimalBoundingBox,
            Vector2 currentMousePosition,
            TileHistory tileHistory
            )
        {
            if (tileSelectionIsVisible &&
                tileSelectionIsHoveredByMouse)
            {
                return;
            }

            UpdateCopyingSelectedTiles(selectedTiles);
            UpdateCuttingSelectedTiles(tiles, selectedTiles, ref selectedTilesMinimalBoundingBox, tileHistory);
            UpdateDeletingSelectedTiles(tiles, selectedTiles, ref selectedTilesMinimalBoundingBox, tileHistory);
            UpdatePastingCopiedOrCuttedTiles(currentMousePosition, tiles, tileHistory);
        }

        private void UpdateCopyingSelectedTiles(List<Tile> selectedTiles)
        {
            if (selectedTiles.Count != 0 && InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.C))
            {
                copyBuffer.Clear();
                copyBuffer.AddRange(selectedTiles);
            }
        }

        private void UpdateCuttingSelectedTiles
            (
            List<Tile> tiles,
            List<Tile> selectedTiles,
            ref RectangleF selectedTilesMinimalBoundingBox,
            TileHistory tileHistory
            )
        {
            if (selectedTiles.Count != 0 && InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.X))
            {
                copyBuffer.Clear();
                copyBuffer.AddRange(selectedTiles);

                tileHistory.AppendDeleteAction(new List<Tile>(selectedTiles));

                // Remove all Tiles that have been cut from DrawingArea.
                tiles.RemoveAll((tile) =>
                {
                    return selectedTiles.Contains(tile);
                });

                selectedTilesMinimalBoundingBox = RectangleF.Empty;
            }
        }

        private void UpdateDeletingSelectedTiles
            (
            List<Tile> tiles,
            List<Tile> selectedTiles,
            ref RectangleF selectedTilesMinimalBoundingBox,
            TileHistory tileHistory
            )
        {
            if (selectedTiles.Count != 0 && InputManager.OnKeyPressed(Keys.Delete))
            {
                tileHistory.AppendDeleteAction(new List<Tile>(selectedTiles));

                tiles.RemoveAll((tile) =>
                {
                    return selectedTiles.Contains(tile);
                });
                selectedTiles.Clear();
                selectedTilesMinimalBoundingBox = RectangleF.Empty;
            }
        }

        private void UpdatePastingCopiedOrCuttedTiles(Vector2 currentMousePosition, List<Tile> tiles, TileHistory tileHistory)
        {
            if (copyBuffer.Count != 0 && InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.V))
            {
                Vector2 shiftVector = currentMousePosition - new Vector2(copyBuffer[0].screenBounds.Position.X,
                                                                         copyBuffer[0].screenBounds.Position.Y);

                List<int> addedIndices = new List<int>();
                foreach (Tile tile in copyBuffer)
                {
                    Vector2 newTilePosition = tile.screenBounds.Position + shiftVector;

                    Tile newTile = new Tile(tile.name, tile.textureBounds, 
                        new RectangleF(newTilePosition, tile.screenBounds.Size));

                    tiles.Add(newTile);
                    addedIndices.Add(tiles.Count - 1);
                }
                tileHistory.AppendAddAction(addedIndices);
            }
        }

        #endregion
    }
}
