using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapEditor.DrawingAreaComponents
{
    /// <summary>
    /// Handles moving Tiles in the DrawingArea.
    /// </summary>
    public class TileMover
    {
        private bool movingSelectedTilesWithMouse = false;
        private bool movingSelectedTilesWithKeys = false;

        private const float holdDelay = 500;
        private float rightHoldElapsed = 0;
        private float leftHoldElapsed = 0;
        private float upHoldElapsed = 0;
        private float downHoldElapsed = 0;

        public bool MovingSelectedTilesWithMouse { get => movingSelectedTilesWithMouse; }

        public bool MovingSelectedTilesWithKeys { get => movingSelectedTilesWithKeys; }

        public TileMover()
        {

        }

        #region PublicInterface

        public void Update
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool selectionBoxHasStartPoint,
            bool selectionIsBeingScaled,
            List<Tile> selectedTiles,
            List<Tile> tiles,
            ref Rectangle selectedTilesMinimalBoundingBox,
            Vector2 currentMousePosition,
            Vector2 mouseTravel,
            GameTime gameTime,
            Grid grid
            )
        {
            UpdateMovingSelectedTilesWithMouse(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, selectionBoxHasStartPoint, selectionIsBeingScaled,
                                               selectedTiles, ref selectedTilesMinimalBoundingBox, currentMousePosition, mouseTravel, grid);

            UpdateMovingSelectedTilesWithKeys(selectedTiles, ref selectedTilesMinimalBoundingBox, gameTime);
            UpdateSnappingAllTilesToGrid(tiles, grid);
        }

        #endregion

        #region PrivateHelperMethods

        private void UpdateMovingSelectedTilesWithMouse
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool selectionBoxHasStartPoint,
            bool selectionIsBeingScaled,
            List<Tile> selectedTiles,
            ref Rectangle selectedTilesMinimalBoundingBox,
            Vector2 currentMousePosition,
            Vector2 mouseTravel,
            Grid grid
            )
        {
            if (CantMoveSelectedTilesWithMouse(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, selectionBoxHasStartPoint, selectionIsBeingScaled,
                                               selectedTiles.Count == 0))
            {
                return;
            }

            // Move selection with Mouse.
            if (selectedTilesMinimalBoundingBox.Contains(currentMousePosition) &&
                InputManager.OnLeftMouseButtonClicked())
            {
                movingSelectedTilesWithMouse = true;
            }
            else if (InputManager.OnLeftMouseButtonReleased())
            {
                movingSelectedTilesWithMouse = false;
            }

            if (movingSelectedTilesWithMouse &&
                mouseTravel != Vector2.Zero)
            {
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += mouseTravel.ToPoint();
                }
                selectedTilesMinimalBoundingBox.Location += mouseTravel.ToPoint();
            }

            // Grid Snapping
            if (grid.GridActivated && InputManager.OnLeftMouseButtonReleased())
            {
                Vector2 snappingVector = grid.GetSnappingVectorForGivenPosition(selectedTilesMinimalBoundingBox.Location.ToVector2());
                Vector2 correctionVector = Vector2.Zero;

                selectedTilesMinimalBoundingBox.Location += snappingVector.ToPoint();

                // Correction if movement past DrawingArea bounds.
                if (selectedTilesMinimalBoundingBox.Location.X < 0)
                {
                    correctionVector.X -= selectedTilesMinimalBoundingBox.Location.X;
                }
                if (selectedTilesMinimalBoundingBox.Location.Y < 0)
                {
                    correctionVector.Y -= selectedTilesMinimalBoundingBox.Location.Y;
                }
                selectedTilesMinimalBoundingBox.Location += correctionVector.ToPoint();

                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += snappingVector.ToPoint() + correctionVector.ToPoint();
                }
            }
        }

        private bool CantMoveSelectedTilesWithMouse
            (
            bool tileSelectionIsVisible,
            bool tileSelectionIsHoveredByMouse,
            bool selectionBoxHasStartPoint,
            bool selectionIsBeingScaled,
            bool noTilesSelected
            )
        {
            return tileSelectionIsVisible &&
                   (
                   tileSelectionIsHoveredByMouse ||
                   noTilesSelected ||
                   selectionBoxHasStartPoint ||
                   selectionIsBeingScaled
                   );
        }

        private void UpdateMovingSelectedTilesWithKeys
            (
            List<Tile> selectedTiles,
            ref Rectangle selectedTilesMinimalBoundingBox,
            GameTime gameTime
            )
        {
            movingSelectedTilesWithKeys = false;

            // Move Right
            if (InputManager.OnKeyPressed(Keys.Right))
            {
                // Move every Tile and the minimumBoundingBox one step to the right.
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += new Point(1, 0);
                }
                selectedTilesMinimalBoundingBox.Location += new Point(1, 0);
            }
            if (InputManager.IsKeyPressed(Keys.Right))
            {
                movingSelectedTilesWithKeys = true;

                if (rightHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the right.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Location += new Point(1, 0);
                    }
                    selectedTilesMinimalBoundingBox.Location += new Point(1, 0);

                    rightHoldElapsed *= 0.90f;
                }
                else
                {
                    rightHoldElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else
            {
                rightHoldElapsed = 0;
            }

            // Move Left
            if (InputManager.OnKeyPressed(Keys.Left))
            {
                // Move every Tile and the minimumBoundingBox one step to the left.
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += new Point(-1, 0);
                }
                selectedTilesMinimalBoundingBox.Location += new Point(-1, 0);
            }
            if (InputManager.IsKeyPressed(Keys.Left))
            {
                movingSelectedTilesWithKeys = true;

                if (leftHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the left.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Location += new Point(-1, 0);
                    }
                    selectedTilesMinimalBoundingBox.Location += new Point(-1, 0);

                    leftHoldElapsed *= 0.90f;
                }
                else
                {
                    leftHoldElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else
            {
                leftHoldElapsed = 0;
            }

            // Move Up 
            if (InputManager.OnKeyPressed(Keys.Up))
            {
                // Move every Tile and the minimumBoundingBox one step to the top.
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += new Point(0, -1);
                }
                selectedTilesMinimalBoundingBox.Location += new Point(0, -1);
            }
            if (InputManager.IsKeyPressed(Keys.Up))
            {
                movingSelectedTilesWithKeys = true;

                if (upHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the top.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Location += new Point(0, -1);
                    }
                    selectedTilesMinimalBoundingBox.Location += new Point(0, -1);

                    upHoldElapsed *= 0.90f;
                }
                else
                {
                    upHoldElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else
            {
                upHoldElapsed = 0;
            }

            // Move Down.
            if (InputManager.OnKeyPressed(Keys.Down))
            {
                // Move every Tile and the minimumBoundingBox one step to bottom.
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Location += new Point(0, 1);
                }
                selectedTilesMinimalBoundingBox.Location += new Point(0, 1);
            }
            if (InputManager.IsKeyPressed(Keys.Down))
            {
                movingSelectedTilesWithKeys = true;

                if (downHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to bottom.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Location += new Point(0, 1);
                    }
                    selectedTilesMinimalBoundingBox.Location += new Point(0, 1);

                    downHoldElapsed *= 0.90f;
                }
                else
                {
                    downHoldElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
            else
            {
                downHoldElapsed = 0;
            }
        }

        private void UpdateSnappingAllTilesToGrid(List<Tile> tiles, Grid grid)
        {
            // Snap all Tiles
            if (grid.GridActivated && InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.LeftAlt, Keys.S))
            {
                SnapAllTilesToGrid(tiles, grid);
            }
        }

        private void SnapAllTilesToGrid(List<Tile> tiles, Grid grid)
        {
            foreach (Tile tile in tiles)
            {
                tile.screenBounds.Location += grid.GetSnappingVectorForGivenPosition(tile.screenBounds.Location.ToVector2()).ToPoint();
            }
        }

        #endregion
    }
}
