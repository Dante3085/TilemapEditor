using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using C3.MonoGame;
using MonoGame.Extended;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Handles selecting multiple Tiles with RectangleSelection like on Windows Desktop.
    /// </summary>
    public class SelectionRectangle
    {
        private Vector2 selectionBoxStartPoint = Vector2.Zero;
        private RectangleF selectionBox = RectangleF.Empty;
        private List<RectangleF> possibleSelectionMarkers = new List<RectangleF>();

        public bool SelectionBoxHasStartPoint
        {
            get;
            private set;
        }

        public SelectionRectangle()
        {
            
        }

        #region PublicInterface

        public void Update(GameTime gameTime, List<Tile> tiles, List<Tile> selectedTiles, Vector2 currentMousePosition, 
                           ref RectangleF minimalBoundingBox)
        {
            if (InputManager.OnRightMouseButtonClicked())
            {
                selectionBox = RectangleF.Empty;
                selectionBoxStartPoint = currentMousePosition;
                SelectionBoxHasStartPoint = true;
                minimalBoundingBox = RectangleF.Empty;
                
            }
            else if (InputManager.OnRightMouseButtonReleased())
            {
                SelectionBoxHasStartPoint = false;
                possibleSelectionMarkers.Clear();

                // Find out which Tiles were selected.
                // We want to know the minimumBoundingBox of all selected Tiles as well.
                Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 bottomRight = new Vector2(float.MinValue, float.MinValue);

                // We don't use DetectSelectionMinimalBoundingBox() because we already have to iterate
                // over all Tiles to figure out which actually are inside the selection.
                // So while we are iterating over all Tiles we can simultaneously figure out the dimensions
                // of the minimalBoundingBox.
                selectedTiles.Clear();
                foreach (Tile tile in tiles)
                {
                    if (selectionBox.Intersects(tile.screenBounds))
                    {
                        selectedTiles.Add(tile);

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
                }
                minimalBoundingBox = new RectangleF(topLeft, (bottomRight - topLeft));
            }
            if (InputManager.HasMouseMoved &&
                SelectionBoxHasStartPoint && InputManager.IsRightMouseButtonDown())
            {
                RecalculateSelectionBox(currentMousePosition);
                DeterminePossibleSelectionMarkers(tiles);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (SelectionBoxHasStartPoint)
            {
                DrawPossibleSelectionMarkers(spriteBatch);
                DrawSelectionBox(spriteBatch);
            }
        }

        #endregion

        #region PrivateHelperMethods

        private void DrawSelectionBox(SpriteBatch spriteBatch)
        {
            Color color = Color.Blue;
            color.A = 15;
            Primitives2D.FillRectangle(spriteBatch, selectionBox.ToRectangle(), color);
        }

        private void DrawPossibleSelectionMarkers(SpriteBatch spriteBatch)
        {
            Color color = Color.Red;
            color.A = 90;
            foreach (RectangleF marker in possibleSelectionMarkers)
            {
                spriteBatch.FillRectangle(marker.ToRectangle(), color);
            }
        }

        private void RecalculateSelectionBox(Vector2 currentMousePosition)
        {
            selectionBox.Width = (int)Math.Abs(currentMousePosition.X - selectionBoxStartPoint.X);
            selectionBox.Height = (int)Math.Abs(currentMousePosition.Y - selectionBoxStartPoint.Y);
            selectionBox.X = (int)Math.Min(selectionBoxStartPoint.X, currentMousePosition.X);
            selectionBox.Y = (int)Math.Min(selectionBoxStartPoint.Y, currentMousePosition.Y);
        }

        private void DeterminePossibleSelectionMarkers(List<Tile> tiles)
        {
            possibleSelectionMarkers.Clear();
            foreach (Tile tile in tiles)
            {
                if (selectionBox.Intersects(tile.screenBounds))
                {
                    RectangleF marker = tile.screenBounds;
                    marker.Inflate(-(tile.screenBounds.Width/4), -(tile.screenBounds.Height / 4));

                    possibleSelectionMarkers.Add(marker);
                }
            }
        }

        #endregion
    }
}
