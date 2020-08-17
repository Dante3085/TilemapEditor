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

        private List<TileAction> tileActionHistory = new List<TileAction>();

        private List<List<int>> addHistory = new List<List<int>>();

        private List<List<Tile>> deleteHistory = new List<List<Tile>>();

        private List<List<Tuple<Tile, Vector2>>> positionHistory = new List<List<Tuple<Tile, Vector2>>>();

        public TileHistory(int maxHistoryDepth)
        {
            this.maxHistoryDepth = maxHistoryDepth;
        }

        #region publicIntereface

        public void Update(List<Tile> drawingAreaTiles, TileSelector tileSelector)
        {
            if (tileActionHistory.Count > 0 &&
                InputManager.OnKeyCombinationPressed(Keys.LeftControl, Keys.Z))
            {
                switch(tileActionHistory[tileActionHistory.Count - 1])
                {
                    case TileAction.ADD_TILES:
                        {
                            for (int i = addHistory[addHistory.Count-1].Count-1; i >= 0; --i)
                            {
                                drawingAreaTiles.RemoveAt(addHistory[addHistory.Count-1][i]);
                            }

                            addHistory.RemoveAt(addHistory.Count - 1);
                            tileActionHistory.RemoveAt(tileActionHistory.Count - 1);
                            break;
                        }

                    case TileAction.DELETE_TILES:
                        {
                            drawingAreaTiles.AddRange(deleteHistory[deleteHistory.Count - 1]);
                            deleteHistory.RemoveAt(deleteHistory.Count - 1);
                            tileActionHistory.RemoveAt(tileActionHistory.Count - 1);
                            break;
                        }

                    case TileAction.MOVE_TILES:
                        {
                            foreach (Tuple<Tile, Vector2> oldPosition in positionHistory[positionHistory.Count - 1])
                            {
                                oldPosition.Item1.screenBounds.Position = oldPosition.Item2;
                            }

                            positionHistory.RemoveAt(positionHistory.Count - 1);
                            tileActionHistory.RemoveAt(tileActionHistory.Count - 1);
                            break;
                        }
                }
            }
        }
        
        public void AppendAddAction(List<int> addedTileIndices)
        {
            CheckMaxHistoryDepth();

            addHistory.Add(addedTileIndices);
            tileActionHistory.Add(TileAction.ADD_TILES);
        }

        public void AppendDeleteAction(List<Tile> deletedTiles)
        {
            CheckMaxHistoryDepth();

            deleteHistory.Add(deletedTiles);
            tileActionHistory.Add(TileAction.DELETE_TILES);
        }

        public void AppendMoveAction(List<Tuple<Tile, Vector2>> oldPositions)
        {
            CheckMaxHistoryDepth();

            positionHistory.Add(oldPositions);
            tileActionHistory.Add(TileAction.MOVE_TILES);
        }

        #endregion

        #region PrivateHelperMethods

        private void CheckMaxHistoryDepth()
        {
            if (tileActionHistory.Count == maxHistoryDepth)
            {
                // Remove oldest TileAction and it's associated history data.
                switch (tileActionHistory[0])
                {
                    case TileAction.ADD_TILES:
                        {
                            addHistory.RemoveAt(0);
                            break;
                        }

                    case TileAction.DELETE_TILES:
                        {
                            deleteHistory.RemoveAt(0);
                            break;
                        }

                    case TileAction.MOVE_TILES:
                        {
                            positionHistory.RemoveAt(0);
                            break;
                        }
                }
                tileActionHistory.RemoveAt(0);
            }
        }

        #endregion
    }
}
