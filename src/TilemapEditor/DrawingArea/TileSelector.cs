using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3.MonoGame;
using MonoGame.Extended;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Handles selection of Tiles in the DrawingArea.
    /// </summary>
    public class TileSelector
    {
        private SelectionRectangle selectionRectangle = new SelectionRectangle();
        private Tile drawingAreaHoveredTile = null;
        private RectangleF selectedTilesMinimalBoundingBox = RectangleF.Empty;
        private List<Tile> selectedTiles = new List<Tile>();

        public List<Tile> SelectedTiles { get => selectedTiles; }

        public ref RectangleF SelectedTilesMinimalBoundingBox { get => ref selectedTilesMinimalBoundingBox; }

        public bool SelectionBoxHasStartPoint { get => selectionRectangle.SelectionBoxHasStartPoint; }

        public TileSelector()
        {
        }

        #region PublicInterface

        public void Update
            (
            bool tileSelectionIsVisible, 
            bool tileSelectionIsHoveredByMouse, 
            bool tileSelectionCurrentTileIsDrawnOnMouse,
            bool movingSelectedTilesWithMouse,
            List<Tile> drawingAreaTiles,
            Vector2 currentMousePosition,
            GameTime gameTime
            )
        {
            UpdateDetectingDrawingAreaHoveredTile(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, tileSelectionCurrentTileIsDrawnOnMouse,
                                                  drawingAreaTiles, currentMousePosition);

            UpdateDetectingSelection(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, tileSelectionCurrentTileIsDrawnOnMouse,
                                     movingSelectedTilesWithMouse, currentMousePosition, drawingAreaTiles, gameTime);
        }

        public void Draw
            (
            SpriteBatch spriteBatch, 
            bool notMovingSelectedTilesWithMouse,
            bool notMovingSelectedTilesWithKeys,
            bool notInCollisionBoxMode
            )
        {
            selectionRectangle.Draw(spriteBatch);

            // Mark hovered Tile.
            if (drawingAreaHoveredTile != null &&
                notMovingSelectedTilesWithMouse &&
                notMovingSelectedTilesWithKeys &&
                notInCollisionBoxMode)
            {
                Primitives2D.DrawRectangle(spriteBatch, drawingAreaHoveredTile.screenBounds.ToRectangle(), 
                    Color.AliceBlue, 5);
            }

            // Mark selection.
            if (selectedTiles.Count != 0 /* &&
                !movingSelectionWithMouse &&
                !movingSelectionWithKeys*/)
            {
                Color boxColor = Color.DarkRed;
                boxColor.A = 50;

                // Mark selection with one Tile.
                if (selectedTiles.Count == 1)
                {
                    Primitives2D.FillRectangle(spriteBatch, selectedTiles[0].screenBounds.ToRectangle(), 
                        boxColor);
                }

                // Mark selection with multiple Tiles.
                else
                {
                    Primitives2D.FillRectangle(spriteBatch, selectedTilesMinimalBoundingBox.ToRectangle(), 
                        boxColor);
                }
            }
        }

        public void ClearSelection()
        {
            selectedTiles.Clear();
            selectedTilesMinimalBoundingBox = RectangleF.Empty;
        }

        public void UpdatetSelectedTilesMinimalBoundingBox()
        {
            CalcSelectionMinimalBoundingBox();
        }

        #endregion

        #region PrivateHelperMethods

        private void UpdateDetectingDrawingAreaHoveredTile
            (
            bool tileSelectionIsVisible, 
            bool tileSelectionIsHoveredByMouse,
            bool tileSelectionCurrentTileIsDrawnOnMouse,
            List<Tile> drawingAreaTiles,
            Vector2 currentMousePosition
            )
        {
            if (CantDetectDrawingAreaHoveredTile(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, tileSelectionCurrentTileIsDrawnOnMouse))
            {
                drawingAreaHoveredTile = null;
                return;
            }

            // If there is no hovered Tile we don't want to keep marking the previously hovered Tile,
            // so we set it to null here and if there actually is a hovered Tile this will be overriden
            // by the actual hovered Tile.
            drawingAreaHoveredTile = null;
            for (int i = drawingAreaTiles.Count - 1; i >= 0; --i)
            {
                Tile tile = drawingAreaTiles[i];
                if (tile.screenBounds.Contains(currentMousePosition))
                {
                    drawingAreaHoveredTile = tile;
                    return;
                }
            }
        }

        private bool CantDetectDrawingAreaHoveredTile
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool tileSelectionCurrentTileIsDrawnOnMouse
            )
        {
            return tileSelectionIsVisible &&
                   (
                   tileSelectionIsHoveredByMouse ||
                   tileSelectionCurrentTileIsDrawnOnMouse ||
                   selectionRectangle.SelectionBoxHasStartPoint
                   );
        }

        private void UpdateDetectingSelection
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool tileSelectionCurrentTileIsDrawnOnMouse,
            bool movingSelectedTilesWithMouse,
            Vector2 currentMousePosition,
            List<Tile> drawingAreaTiles,
            GameTime gameTime
            )
        {
            if (CantDetectSelection(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, tileSelectionCurrentTileIsDrawnOnMouse, movingSelectedTilesWithMouse))
                return;

            selectionRectangle.Update(gameTime, drawingAreaTiles, selectedTiles, currentMousePosition, 
                ref selectedTilesMinimalBoundingBox);
            UpdateSelectingAllTiles(drawingAreaTiles);
            UpdateSelectingIndividualTile(currentMousePosition);
        } 

        private bool CantDetectSelection
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool tileSelectionCurrentTileIsDrawnOnMouse,
            bool movingSelectedTilesWithMouse
            )
        {
            return tileSelectionIsVisible &&
                   (
                   tileSelectionIsHoveredByMouse ||
                   tileSelectionCurrentTileIsDrawnOnMouse ||
                   movingSelectedTilesWithMouse
                   );
        }

        private void UpdateSelectingIndividualTile(Vector2 currentMousePosition)
        {
            // One Tile selected.
            if (drawingAreaHoveredTile != null &&
                !selectedTilesMinimalBoundingBox.Contains(currentMousePosition) &&
                InputManager.OnLeftMouseButtonDown())
            {
                // If we are clicking the already selected Tile again, just throw it out
                // of the selection.
                if (selectedTiles.Count == 1 &&
                    selectedTiles[0] == drawingAreaHoveredTile)
                {
                    selectedTiles.Clear();

                    selectedTilesMinimalBoundingBox = RectangleF.Empty;
                }
                else
                {
                    selectedTiles.Clear();
                    selectedTiles.Add(drawingAreaHoveredTile);

                    selectedTilesMinimalBoundingBox = drawingAreaHoveredTile.screenBounds;
                }
            }
        }

        private void UpdateSelectingAllTiles(List<Tile> drawingAreaTiles)
        {
            // Select all Tiles with STRG+A
            if (InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.A))
            {
                if (selectedTiles.Count == drawingAreaTiles.Count)
                {
                    selectedTiles.Clear();
                    selectedTilesMinimalBoundingBox = RectangleF.Empty;
                }
                else
                {
                    selectedTiles.Clear();
                    selectedTiles.AddRange(drawingAreaTiles);
                    CalcSelectionMinimalBoundingBox();
                }
            }
        }

        private void CalcSelectionMinimalBoundingBox()
        {
            Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (Tile tile in selectedTiles)
            {
                // Find out topLeft Point of minimumBoundingBox.
                if (tile.screenBounds.Left < topLeft.X)
                    topLeft.X = tile.screenBounds.Left;
                if (tile.screenBounds.Top < topLeft.Y)
                    topLeft.Y = tile.screenBounds.Top;

                // Find bottomRight Point of minimumBoundingBox.
                if (tile.screenBounds.Right > bottomRight.X)
                    bottomRight.X = tile.screenBounds.Right;
                if (tile.screenBounds.Bottom > bottomRight.Y)
                    bottomRight.Y = tile.screenBounds.Bottom;
            }
            selectedTilesMinimalBoundingBox = new RectangleF(topLeft, (bottomRight - topLeft));
        }

        #endregion
    }
}
