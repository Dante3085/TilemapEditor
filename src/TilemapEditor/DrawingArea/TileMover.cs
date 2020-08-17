using Microsoft.Xna.Framework;
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
            ref RectangleF selectedTilesMinimalBoundingBox,
            Vector2 currentMousePosition,
            Vector2 mouseTravel,
            GameTime gameTime,
            Grid grid,
            TileHistory tileHistory
            )
        {
            UpdateMovingSelectedTilesWithMouse(tileSelectionIsVisible, tileSelectionIsHoveredByMouse, selectionBoxHasStartPoint, selectionIsBeingScaled,
                                               selectedTiles, ref selectedTilesMinimalBoundingBox, currentMousePosition, mouseTravel, grid, tileHistory);

            UpdateMovingSelectedTilesWithKeys(selectedTiles, ref selectedTilesMinimalBoundingBox, gameTime);
            UpdateSnappingAllTilesToGrid(tiles, grid, tileHistory);
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
            ref RectangleF selectedTilesMinimalBoundingBox,
            Vector2 currentMousePosition,
            Vector2 mouseTravel,
            Grid grid,
            TileHistory tileHistory
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

                List<Tuple<Tile, Vector2>> oldPositions = new List<Tuple<Tile, Vector2>>();
                foreach (Tile tile in selectedTiles)
                {
                    oldPositions.Add(new Tuple<Tile, Vector2>(tile, tile.screenBounds.Position));
                }
                tileHistory.AppendMoveAction(oldPositions);
            }
            else if (InputManager.OnLeftMouseButtonReleased())
            {
                movingSelectedTilesWithMouse = false;
            }

            if (movingSelectedTilesWithMouse /*&& mouseTravel != Vector2.Zero*/)
            {
                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Position += mouseTravel;
                }
                selectedTilesMinimalBoundingBox.Position += mouseTravel;
            }

            // Grid Snapping
            if (grid.GridActivated && InputManager.OnLeftMouseButtonReleased())
            {
                Vector2 snappingVector = grid.GetSnappingVectorForGivenPosition(selectedTilesMinimalBoundingBox.Position);
                Vector2 correctionVector = Vector2.Zero;

                selectedTilesMinimalBoundingBox.Position += snappingVector;

                // Correction if movement past DrawingArea bounds.
                if (selectedTilesMinimalBoundingBox.Position.X < 0)
                {
                    correctionVector.X -= selectedTilesMinimalBoundingBox.Position.X;
                }
                if (selectedTilesMinimalBoundingBox.Position.Y < 0)
                {
                    correctionVector.Y -= selectedTilesMinimalBoundingBox.Position.Y;
                }
                selectedTilesMinimalBoundingBox.Position += correctionVector;

                foreach (Tile tile in selectedTiles)
                {
                    tile.screenBounds.Position += snappingVector + correctionVector;
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
            ref RectangleF selectedTilesMinimalBoundingBox,
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
                    tile.screenBounds.Position += new Vector2(1, 0);
                }
                selectedTilesMinimalBoundingBox.Position += new Vector2(1, 0);
            }
            if (InputManager.IsKeyPressed(Keys.Right))
            {
                movingSelectedTilesWithKeys = true;

                if (rightHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the right.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Position += new Vector2(1, 0);
                    }
                    selectedTilesMinimalBoundingBox.Position += new Vector2(1, 0);

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
                    tile.screenBounds.Position += new Vector2(-1, 0);
                }
                selectedTilesMinimalBoundingBox.Position += new Vector2(-1, 0);
            }
            if (InputManager.IsKeyPressed(Keys.Left))
            {
                movingSelectedTilesWithKeys = true;

                if (leftHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the left.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Position += new Vector2(-1, 0);
                    }
                    selectedTilesMinimalBoundingBox.Position += new Vector2(-1, 0);

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
                    tile.screenBounds.Position += new Vector2(0, -1);
                }
                selectedTilesMinimalBoundingBox.Position += new Vector2(0, -1);
            }
            if (InputManager.IsKeyPressed(Keys.Up))
            {
                movingSelectedTilesWithKeys = true;

                if (upHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to the top.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Position += new Vector2(0, -1);
                    }
                    selectedTilesMinimalBoundingBox.Position += new Vector2(0, -1);

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
                    tile.screenBounds.Position += new Vector2(0, 1);
                }
                selectedTilesMinimalBoundingBox.Position += new Vector2(0, 1);
            }
            if (InputManager.IsKeyPressed(Keys.Down))
            {
                movingSelectedTilesWithKeys = true;

                if (downHoldElapsed >= holdDelay)
                {
                    // Move every Tile and the minimumBoundingBox one step to bottom.
                    foreach (Tile tile in selectedTiles)
                    {
                        tile.screenBounds.Position += new Vector2(0, 1);
                    }
                    selectedTilesMinimalBoundingBox.Position += new Vector2(0, 1);

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

        private void UpdateSnappingAllTilesToGrid(List<Tile> tiles, Grid grid, TileHistory tileHistory)
        {
            // Snap all Tiles
            if (grid.GridActivated && InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.LeftAlt, Keys.S))
            {
                SnapAllTilesToGrid(tiles, grid, tileHistory);
            }
        }

        private void SnapAllTilesToGrid(List<Tile> tiles, Grid grid, TileHistory tileHistory)
        {
            List<Tuple<Tile, Vector2>> oldPositions = new List<Tuple<Tile, Vector2>>();
            foreach (Tile tile in tiles)
            {
                oldPositions.Add(new Tuple<Tile, Vector2>(tile, tile.screenBounds.Position));
                tile.screenBounds.Position += grid.GetSnappingVectorForGivenPosition(tile.screenBounds.Position);
            }
            tileHistory.AppendMoveAction(oldPositions);
        }

        #endregion
    }
}
