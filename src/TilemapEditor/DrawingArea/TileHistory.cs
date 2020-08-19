using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Xml.Schema;

namespace TilemapEditor.DrawingAreaComponents
{
    public enum TileAction
    {
        ADD_TILES,
        DELETE_TILES,
        MOVE_TILES
    }

    // TODO: Maybe use CSharp Events for this instead of passing it to all the 
    /// <summary>
    /// Handles undoing and redoing TileActions.
    /// </summary>
    public class TileHistory
    {
        private int maxHistoryDepth;

        private List<TileAction> undoTileActionHistory = new List<TileAction>();
        private List<TileAction> redoTileActionHistory = new List<TileAction>();

        private List<List<int>> undoAddHistory = new List<List<int>>();
        private List<List<Tile>> redoAddHistory = new List<List<Tile>>();

        private List<List<Tile>> undoDeleteHistory = new List<List<Tile>>();
        private List<List<int>> redoDeleteHistory = new List<List<int>>();

        private List<List<Tuple<Tile, Vector2>>> undoPositionHistory = new List<List<Tuple<Tile, Vector2>>>();
        private List<List<Tuple<Tile, Vector2>>> redoPositionHistory = new List<List<Tuple<Tile, Vector2>>>();


        public TileHistory(int maxHistoryDepth)
        {
            this.maxHistoryDepth = maxHistoryDepth;
        }

        #region publicIntereface

        public void Update(List<Tile> drawingAreaTiles)
        {
            UpdateUndoingLastTileAction(drawingAreaTiles);
            UpdateRedoingLastTileAction(drawingAreaTiles);

        }
        
        public void AppendAddAction(List<int> addedTileIndices)
        {
            CheckMaxHistoryDepth();

            undoAddHistory.Add(addedTileIndices);
            undoTileActionHistory.Add(TileAction.ADD_TILES);
        }

        public void AppendDeleteAction(List<Tile> deletedTiles)
        {
            CheckMaxHistoryDepth();

            undoDeleteHistory.Add(deletedTiles);
            undoTileActionHistory.Add(TileAction.DELETE_TILES);
        }

        public void AppendMoveAction(List<Tuple<Tile, Vector2>> oldPositions)
        {
            CheckMaxHistoryDepth();

            undoPositionHistory.Add(oldPositions);
            undoTileActionHistory.Add(TileAction.MOVE_TILES);
        }

        #endregion

        #region PrivateHelperMethods

        private void UpdateUndoingLastTileAction(List<Tile> drawingAreaTiles)
        {
            if (undoTileActionHistory.Count > 0 &&
                InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.Z))
            {
                switch (undoTileActionHistory.Last())
                {
                    case TileAction.ADD_TILES:
                        {
                            List<Tile> tilesForRedo = new List<Tile>();
                            for (int i = undoAddHistory.Last().Count - 1; i >= 0; --i)
                            {
                                tilesForRedo.Add(drawingAreaTiles[undoAddHistory.Last()[i]]);
                                drawingAreaTiles.RemoveAt(undoAddHistory.Last()[i]);
                            }
                            redoAddHistory.Add(tilesForRedo);
                            redoTileActionHistory.Add(TileAction.ADD_TILES);

                            undoAddHistory.RemoveAt(undoAddHistory.Count - 1);
                            undoTileActionHistory.RemoveAt(undoTileActionHistory.Count - 1);
                            break;
                        }

                    case TileAction.DELETE_TILES:
                        {
                            redoDeleteHistory.Add(new List<int>());
                            for (int i = 0; i < undoDeleteHistory.Last().Count; ++i)
                            {
                                redoDeleteHistory.Last().Add(i + drawingAreaTiles.Count);
                            }
                            redoTileActionHistory.Add(TileAction.DELETE_TILES);

                            drawingAreaTiles.AddRange(undoDeleteHistory.Last());

                            undoDeleteHistory.RemoveAt(undoDeleteHistory.Count - 1);
                            undoTileActionHistory.RemoveAt(undoTileActionHistory.Count - 1);
                            break;
                        }

                    case TileAction.MOVE_TILES:
                        {
                            redoPositionHistory.Add(new List<Tuple<Tile, Vector2>>());
                            foreach (Tuple<Tile, Vector2> oldPosition in undoPositionHistory.Last())
                            {
                                redoPositionHistory.Last().Add(new Tuple<Tile, Vector2>(oldPosition.Item1, oldPosition.Item1.screenBounds.Position));
                                oldPosition.Item1.screenBounds.Position = oldPosition.Item2;
                            }
                            redoTileActionHistory.Add(TileAction.MOVE_TILES);

                            undoPositionHistory.RemoveAt(undoPositionHistory.Count - 1);
                            undoTileActionHistory.RemoveAt(undoTileActionHistory.Count - 1);
                            break;
                        }
                }
            }
        }

        private void UpdateRedoingLastTileAction(List<Tile> drawingAreaTiles)
        {
            if (redoTileActionHistory.Count > 0 &&
                InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.Y))
            {
                switch (redoTileActionHistory[redoTileActionHistory.Count - 1])
                {
                    case TileAction.ADD_TILES:
                        {
                            undoAddHistory.Add(new List<int>());
                            for (int i = redoAddHistory.Last().Count - 1; i >= 0; --i)
                            {
                                drawingAreaTiles.Add(redoAddHistory.Last()[i]);
                                undoAddHistory.Last().Add(drawingAreaTiles.Count - 1);
                            }

                            undoTileActionHistory.Add(TileAction.ADD_TILES);

                            redoTileActionHistory.RemoveAt(redoTileActionHistory.Count - 1);
                            redoAddHistory.RemoveAt(redoAddHistory.Count - 1);

                            break;
                        }

                    case TileAction.DELETE_TILES:
                        {
                            undoDeleteHistory.Add(new List<Tile>());
                            for (int i = redoDeleteHistory.Last().Count-1; i >= 0; --i)
                            {
                                undoDeleteHistory.Last().Add(drawingAreaTiles[i]);
                                drawingAreaTiles.RemoveAt(i);
                            }

                            undoTileActionHistory.Add(TileAction.DELETE_TILES);

                            redoTileActionHistory.RemoveAt(redoTileActionHistory.Count - 1);
                            redoDeleteHistory.RemoveAt(redoDeleteHistory.Count - 1);

                            break;
                        }

                    case TileAction.MOVE_TILES:
                        {
                            // TODO: Set positions for all Tiles that have been moved by last undo
                            undoPositionHistory.Add(new List<Tuple<Tile, Vector2>>());
                            foreach (Tuple<Tile, Vector2> oldPosition in redoPositionHistory.Last())
                            {
                                undoPositionHistory.Last().Add(new Tuple<Tile, Vector2>(oldPosition.Item1, oldPosition.Item1.screenBounds.Position));
                                oldPosition.Item1.screenBounds.Position = oldPosition.Item2;
                            }

                            undoTileActionHistory.Add(TileAction.MOVE_TILES);

                            redoTileActionHistory.RemoveAt(redoTileActionHistory.Count - 1);
                            redoPositionHistory.RemoveAt(redoPositionHistory.Count - 1);

                            break;
                        }
                }
            }
        }

        private void CheckMaxHistoryDepth()
        {
            if (undoTileActionHistory.Count == maxHistoryDepth)
            {
                // Remove oldest TileAction and it's associated history data.
                switch (undoTileActionHistory[0])
                {
                    case TileAction.ADD_TILES:
                        {
                            undoAddHistory.RemoveAt(0);
                            break;
                        }

                    case TileAction.DELETE_TILES:
                        {
                            undoDeleteHistory.RemoveAt(0);
                            break;
                        }

                    case TileAction.MOVE_TILES:
                        {
                            undoPositionHistory.RemoveAt(0);
                            break;
                        }
                }
                undoTileActionHistory.RemoveAt(0);
            }
        }

        #endregion
    }
}
