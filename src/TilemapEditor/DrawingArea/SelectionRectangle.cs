using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// Handles selecting multiple Tiles with RectangleSelection like on Windows Desktop.
    /// </summary>
    public class SelectionRectangle
    {
        private Vector2 selectionBoxStartPoint = Vector2.Zero;
        private RectangleF selectionBox = RectangleF.Empty;

        public bool SelectionBoxHasStartPoint
        {
            get;
            private set;
        }

        public SelectionRectangle()
        {
            
        }

        public void Update(GameTime gameTime, List<Tile> tiles, List<Tile> selectedTiles, Vector2 currentMousePosition, 
                           ref RectangleF minimalBoundingBox)
        {
            if (InputManager.OnRightMouseButtonClicked())
            {
                selectionBoxStartPoint = currentMousePosition;
                SelectionBoxHasStartPoint = true;
                minimalBoundingBox = RectangleF.Empty;
            }
            else if (InputManager.OnRightMouseButtonReleased())
            {
                SelectionBoxHasStartPoint = false;

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
                    if (Utility.Contains(selectionBox, tile.screenBounds))
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
            if (SelectionBoxHasStartPoint && InputManager.IsRightMouseButtonDown())
            {
                selectionBox.Width = (int)Math.Abs(currentMousePosition.X - selectionBoxStartPoint.X);
                selectionBox.Height = (int)Math.Abs(currentMousePosition.Y - selectionBoxStartPoint.Y);
                selectionBox.X = (int)Math.Min(selectionBoxStartPoint.X, currentMousePosition.X);
                selectionBox.Y = (int)Math.Min(selectionBoxStartPoint.Y, currentMousePosition.Y);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (SelectionBoxHasStartPoint)
            {
                Color color = Color.Blue;
                color.A = 15;

                Primitives2D.FillRectangle(spriteBatch, selectionBox.ToRectangle(), color);
            }
        }
    }
}
