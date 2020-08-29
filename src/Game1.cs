using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

namespace TilemapEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphicsDeviceManager;
        SpriteBatch spriteBatch;

        private TilemapEditor tilemapEditor;
        private FpsCounter fpsCounter;

        private SpriteFont font;

        public static bool MouseVisible
        {
            get;
            set;
        }

        public Game1()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;
            MouseVisible = true;

            graphicsDeviceManager.PreferredBackBufferWidth = 1920;
            graphicsDeviceManager.PreferredBackBufferHeight = 1080;

            graphicsDeviceManager.IsFullScreen = false;
            graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;

            tilemapEditor = new TilemapEditor();
            fpsCounter = new FpsCounter(Vector2.One);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.ScissorTestEnable = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tilemapEditor.LoadContent(Content, GraphicsDevice.Viewport);
            fpsCounter.LoadContent(Content);

            font = Content.Load<SpriteFont>("fonts/font_default");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputManager.Update(gameTime, GraphicsDevice.Viewport);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            IsMouseVisible = MouseVisible;
            tilemapEditor.Update(gameTime);
            fpsCounter.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //tilemapEditor.Draw(gameTime, spriteBatch);

            //spriteBatch.Begin();
            //fpsCounter.Draw(gameTime, spriteBatch);
            //spriteBatch.End();

            // Seperate rendering area stuff - FUNKTIONERT Yes Boi - Text wird abgeschnitten ist der Beweis
            Rectangle rectangle = new Rectangle(100, 100, 400, 400);

            spriteBatch.Begin();
            spriteBatch.DrawRectangle(rectangle, Color.Red);
            spriteBatch.End();

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.ScissorTestEnable = true;
            spriteBatch.GraphicsDevice.RasterizerState = rasterizerState;

            float scrollWheel = InputManager.CurrentScrollWheel();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, rasterizerState, transformMatrix: Matrix.CreateTranslation(0, scrollWheel/8, 0));
            spriteBatch.GraphicsDevice.ScissorRectangle = rectangle;

            for (int i = 0; i < 1; i++)
            {
                Vector2 newsItems = new Vector2(200 + i * 80, 300);
                spriteBatch.DrawString(font, "scrollWheel: "  + scrollWheel, newsItems, Color.White);
            }

            spriteBatch.End();

            RasterizerState rasterizerState2 = new RasterizerState();
            rasterizerState2.ScissorTestEnable = false;
            spriteBatch.GraphicsDevice.RasterizerState = rasterizerState2;

            base.Draw(gameTime);
        }
    }
}
