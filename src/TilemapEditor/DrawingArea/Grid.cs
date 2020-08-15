using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3.MonoGame;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Helps other components of the DrawingArea to precisely position Tiles.
    /// </summary>
    public class Grid
    {
        private int gridCellSize = 100;
        private bool gridActivated = false;

        public bool GridActivated { get => gridActivated; }

        public int GridCellSize { get => gridCellSize; set => gridCellSize = value; }

        public Grid(int gridCellSize)
        {
            this.gridCellSize = gridCellSize;
        }

        #region PublicInterface

        public void Update()
        {
            ToggleGrid();
        }

        public Vector2 GetSnappingVectorForGivenPosition(Vector2 position)
        {
            Vector2 insideCellPosition = new Vector2((int)(position.X) % gridCellSize, (int)(position.Y) % gridCellSize);

            if (insideCellPosition == Vector2.Zero)
                return Vector2.Zero;

            Vector2 snappingVector = Vector2.Zero;
            if ((insideCellPosition.X) < gridCellSize / 2) // näher an linker Kante 
            {
                snappingVector.X -= insideCellPosition.X;
            }
            else // näher an Rechter Kante
            {
                snappingVector.X += (gridCellSize - insideCellPosition.X);
            }
            if ((insideCellPosition.Y) < gridCellSize / 2) // näher an oberer Kante 
            {
                snappingVector.Y -= insideCellPosition.Y;
            }
            else // näher an unterer Kante
            {
                snappingVector.Y += (gridCellSize - insideCellPosition.Y);
            }

            return snappingVector;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle bounds, float cameraZoom, Vector2 cameraPosition)
        {
            if (!gridActivated)
                return;

            float currentDisplayingWidth = bounds.Width / cameraZoom;
            float currentDisplayingHeight = bounds.Height / cameraZoom;
            for (float i = 0; i < -cameraPosition.X / cameraZoom + currentDisplayingWidth + gridCellSize; i += gridCellSize)
            {
                float y = -cameraPosition.Y / cameraZoom + currentDisplayingHeight + gridCellSize;
                Vector2 start = new Vector2(i, 0);
                Vector2 end = new Vector2(i, y);
                Primitives2D.DrawLine(spriteBatch, start, end, Color.DarkCyan, 2 / cameraZoom);
            }
            for (float i = 0; i < -cameraPosition.Y / cameraZoom + currentDisplayingHeight + gridCellSize; i += gridCellSize)
            {
                float x = -cameraPosition.X / cameraZoom + currentDisplayingWidth + gridCellSize;
                Vector2 start = new Vector2(0, i);
                Vector2 end = new Vector2(x, i);
                Primitives2D.DrawLine(spriteBatch, start, end, Color.DarkCyan, 2 / cameraZoom);
            }
        }

        #endregion

        #region PrivateHelperMethods

        private void ToggleGrid()
        {
            if (InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.G))
            {
                gridActivated = !gridActivated;
            }
        }

        #endregion
    }
}
