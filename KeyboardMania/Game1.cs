using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KeyboardMania.States;

namespace KeyboardMania
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics; // Graphics device manager to handle screen settings
        private SpriteBatch spriteBatch; // Used for drawing textures

        private State _currentState; // The current state of the game (e.g., menu, game, etc.)
        private State _nextState;    // The next state to transition to

        // Method to change the game state
        public void ChangeState(State state)
        {
            _nextState = state;

        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content"; // Set the root directory for content loading

            // Initialize window size settings here if needed
            graphics.IsFullScreen = false; // Set full-screen to false by default
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width; // Set default width
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height; // Set default height
            graphics.ApplyChanges(); // Apply these settings
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true; // Make the mouse cursor visible

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Initialize the starting state of the game
            _currentState = new MenuState(this, graphics.GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here if needed
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Change the state if a new state is set
            if (_nextState != null)
            {
                _currentState = _nextState;
                _nextState = null;
            }

            // Update the current state
            _currentState.Update(gameTime);
            _currentState.PostUpdate(gameTime);

            // Handle input for full-screen toggle
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.F11))
            {
                graphics.IsFullScreen = !graphics.IsFullScreen; // Toggle full-screen mode
                graphics.HardwareModeSwitch = !graphics.IsFullScreen; // Switch between modes
                graphics.ApplyChanges(); // Apply the changes
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); // Clear the screen with a background color

            // Draw the current state
            _currentState.Draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
