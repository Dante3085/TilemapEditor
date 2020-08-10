
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace TilemapEditor
{
    // TODO: Resolve resolution problems.
    // TODO: Load TileSet and possibly other content in TilemapEditor class.
    // TODO: Properly implement Button.
    // TODO: Der Fakt, dass die Funktionen zum Laden/Speichern über mehrere Klassen verteilt sind ist komisch.

    // TODO: Tile being a class instead of a struct could create problems. 
    //       Struct value type might be smarter if we need to pass Tiles around.

    // TODO: Create a SaveAs Button that opens a FileDialog on being pressed.
    //       After choosing a name and location for the file that shall contain
    //       the current information of the TilemapEditor, the TilemapEditor creates
    //       that file in the previously mentioned FileFormat.

    // TODO: Create a LoadTileSelection Button that loads a TileSet with pre-defined Tiles into
    //       the TileSelection.

    // TODO: Create a LoadTilemap Button that loads a Tilemap that was previously created with
    //       TilemapEditor into the TilemapEditor to work on it further.

    // (Moritz): Ich habe den TilemapEditor extra nicht von scenes.Updateable erben und scenes.IDrawable implementieren lassen,
    //           weil er im Hauptmenü durch den TilemapEditorState erreichbar sein soll. Die Klassen im states Ordner haben nichts
    //           mit den Klassen in unserem scenes Ordner zu tun.

    public class TilemapEditor
    {
        #region Fields

        private TileSelection tileSelection;
        private DrawingArea drawingArea;

        private SpriteFont font;
        private String infoText = String.Empty;
        private Vector2 fontSize;
        private bool drawInfoText = false;

        private Viewport viewport;

        private ContentManager content;

        #endregion

        #region Properties

        public SpriteFont Font
        {
            get { return font; }
        }

        #endregion

        public TilemapEditor()
        {
            tileSelection = new TileSelection(new Vector2(0, 0), new Vector2(64, 64), 5, new Vector2(1, 1));
        }

        public void Update(GameTime gameTime)
        {
            tileSelection.Update(gameTime);
            drawingArea.Update(gameTime);

            CheckForSavingTilemapToFile();
            CheckForLoadingTilemapFromFile();
            CheckForLoadingTileSelectionFromFile();

            if (InputManager.OnKeyPressed(Microsoft.Xna.Framework.Input.Keys.I))
            {
                Console.WriteLine("TEeeeeeeeeest");
                drawInfoText = !drawInfoText;
            }
        }

        #region UpdateHelper
        private void CheckForSavingTilemapToFile()
        {
            // Namespace muss hier voll spezifiziert sein wegen WindowsForms ambiguity.
            // Vielleicht in ConfigFileUtility verschieben ?
            if (InputManager.OnKeyCombinationPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl,
                                                     Microsoft.Xna.Framework.Input.Keys.S))
            {
                String fileName = String.Empty;
                bool saveFileDialogCanceled = false;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save Tilemap as example.tm.txt";

                Thread dialogThread = new Thread(() =>
                {
                    DialogResult dialogResult = saveFileDialog.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        saveFileDialogCanceled = false;
                        fileName = saveFileDialog.FileName;
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        saveFileDialogCanceled = true;
                    }
                });
                dialogThread.SetApartmentState(ApartmentState.STA);
                dialogThread.Start();

                // Wait for the SaveFileDialog to close.
                dialogThread.Join();

                if (!saveFileDialogCanceled)
                {
                    drawingArea.SaveToFile(fileName);
                }
            }
        }

        private void CheckForLoadingTilemapFromFile()
        {
            if (InputManager.OnKeyCombinationPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl,
                                                     Microsoft.Xna.Framework.Input.Keys.L))
            {
                String fileName = String.Empty;
                bool openFileDialogCanceled = false;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select Tilemap file(example.tm.txt) to load...";

                Thread dialogThread = new Thread(() =>
                {
                    DialogResult dialogResult = openFileDialog.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        openFileDialogCanceled = false;
                        fileName = openFileDialog.FileName;
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        openFileDialogCanceled = true;
                    }
                });
                dialogThread.SetApartmentState(ApartmentState.STA);
                dialogThread.Start();

                // Wait for the SaveFileDialog to close.
                dialogThread.Join();

                if (!openFileDialogCanceled)
                {
                    ReadTilemapFile(fileName);
                }
            }
        }

        private void CheckForLoadingTileSelectionFromFile()
        {
            if (InputManager.OnKeyCombinationPressed(Microsoft.Xna.Framework.Input.Keys.LeftControl,
                                                     Microsoft.Xna.Framework.Input.Keys.T))
            {
                String fileName = String.Empty;
                bool openFileDialogCanceled = false;

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select TileSelection file(example.ts.txt) to load...";

                Thread dialogThread = new Thread(() =>
                {
                    DialogResult dialogResult = openFileDialog.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        openFileDialogCanceled = false;
                        fileName = openFileDialog.FileName;
                    }
                    else if (dialogResult == DialogResult.Cancel)
                    {
                        openFileDialogCanceled = true;
                    }
                });
                dialogThread.SetApartmentState(ApartmentState.STA);
                dialogThread.Start();

                // Wait for the SaveFileDialog to close.
                dialogThread.Join();

                if (!openFileDialogCanceled)
                {
                    tileSelection.ReadTilesFromFile(fileName, content);
                    drawingArea.GridCellSize = (int)tileSelection.TileSize.X;
                }
            }
        }

        #endregion

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            drawingArea.Draw(gameTime, spriteBatch);

            if (!tileSelection.Hidden)
            {
                tileSelection.Draw(gameTime, spriteBatch);
            }

            if (drawInfoText)
            {
                Vector2 textPosition = viewport.Bounds.Size.ToVector2() * new Vector2(0.5f, 0.5f) -
                                       (fontSize * new Vector2(0.5f, 0.5f));

                infoText = "General\n" +
                           "MousePos: " + drawingArea.ZoomedMousePosition + "\n" +
                           "Hold MiddleMouseButton: Drag screen\n" +
                           "Hold LeftMouseButton: Rectangle selection\n" +
                           "MouseWheel: Zoom in/out\n" +
                           "Hold Arrow Keys: Move selection slowly\n" +
                           "Hotkeys\n" +
                           "-------\n" +
                           "STRG+S: Save\n" +
                           "STRG+L: Load\n" +
                           "STRG+T: Load TileSelection\n" +
                           "STRG+C: Copy Selection\n" +
                           "STRG+X: Cut Selection\n" +
                           "STRG+V: Paste Copy/Cut to Mouse Position\n" +
                           "STRG+G: Toggle Grid\n" +
                           "STRG+H: Show/Hide TileSelection\n" +
                           "Delete/Entf: Delete selected Tiles\n" +
                           "DrawingArea\n" +
                           "-----------\n" +
                           "NumTiles: " + drawingArea.Tiles.Count + "\n" +
                           "NumTilesSelection: " + drawingArea.NumTilesSelection + "\n" +
                           "NumTilesCopyBuffer: " + drawingArea.NumTilesCopyBuffer + "\n" +
                           "CurrentTileInfo: \n" +
                           drawingArea.SelectionInfo;

                spriteBatch.Begin();

                spriteBatch.DrawString(font, infoText, textPosition, Color.Red);

                spriteBatch.End();
            }
        }

        public void LoadContent(ContentManager content, Viewport viewport)
        {
            this.content = content;

            this.viewport = viewport;

            infoText = "General\n" +
                           "MousePos: \n" +
                           "Hold MiddleMouseButton: Drag screen\n" +
                           "Hold LeftMouseButton: Rectangle selection\n" +
                           "MouseWheel: Zoom in/out\n" +
                           "Hold Arrow Keys: Move selection slowly\n" +
                           "Hotkeys\n" +
                           "-------\n" +
                           "STRG+S: Save\n" +
                           "STRG+L: Load\n" +
                           "STRG+T: Load TileSelection\n" +
                           "STRG+C: Copy Selection\n" +
                           "STRG+X: Cut Selection\n" +
                           "STRG+V: Paste Copy/Cut to Mouse Position\n" +
                           "STRG+G: Toggle Grid\n" +
                           "STRG+H: Show/Hide TileSelection\n" +
                           "Delete/Entf: Delete selected Tiles\n" +
                           "DrawingArea\n" +
                           "-----------\n" +
                           "NumTiles: \n" +
                           "NumTilesSelection: \n" +
                           "NumTilesCopyBuffer: \n" +
                           "CurrentTileInfo: \n";

            font = content.Load<SpriteFont>("fonts/font_default");
            fontSize = font.MeasureString(infoText);

            drawingArea = new DrawingArea(viewport.Bounds, tileSelection);
            tileSelection.ReadTilesFromFile("configFiles/tilesets/testAutoTilesFile.ts.json",
                                             content);

            tileSelection.LoadContent(content);
            drawingArea.LoadContent(content);
        }

        private void ReadTilemapFile(String path)
        {
            // Only allow new json format: example.tm.json
            if (!path.EndsWith(".tm.json"))
            {
                throw new ArgumentException("Given file '" + path + "' is not a tm(Tilemap)File.\n" +
                    "Provide a file that ends with '.tm.json'.");
            }

            // Variables for navigating Json file-structure.
            String jsonString = File.ReadAllText(path);
            JsonDocument jsonDoc = JsonDocument.Parse(jsonString);
            JsonElement tilesElement;

            // Variables for storing the information that will be read from the file.
            List<Tile> tiles = new List<Tile>();

            // TODO: Handle TILESET attribute so that correct Tileset for Tilemap
            //       is automatically loaded.

            // Read Tiles from TILES attribute.
            if (jsonDoc.RootElement.TryGetProperty("TILES", out tilesElement))
            {
                foreach (JsonProperty p in tilesElement.EnumerateObject())
                {
                    Tile newTile = new Tile();
                    newTile.name = p.Name;

                    var tileAttributes = p.Value.EnumerateObject().ToList();

                    var textureBoundsValues = tileAttributes[0].Value.EnumerateArray().ToList();
                    newTile.textureBounds.X = textureBoundsValues[0].GetInt32();
                    newTile.textureBounds.Y = textureBoundsValues[1].GetInt32();
                    newTile.textureBounds.Width = textureBoundsValues[2].GetInt32();
                    newTile.textureBounds.Height = textureBoundsValues[3].GetInt32();

                    var screenBoundsValues = tileAttributes[1].Value.EnumerateArray().ToList();
                    newTile.screenBounds.X = screenBoundsValues[0].GetInt32();
                    newTile.screenBounds.Y = screenBoundsValues[1].GetInt32();
                    newTile.screenBounds.Width = screenBoundsValues[2].GetInt32();
                    newTile.screenBounds.Height = screenBoundsValues[3].GetInt32();

                    tiles.Add(newTile);
                }
            }
            else
            {
                throw new FormatException("Given file '" + path + "' is missing a TILES attribute and is therefore " +
                    "not a valid tm(Tilemap)File.");
            }

            drawingArea.Tiles = tiles;
        }
    }
}
