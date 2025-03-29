using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KeyboardMania.States;

namespace KeyboardMania
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private State _currentState; 
        private State _nextState;
        public void ChangeState(State state)
        {
            _nextState = state;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            IsMouseVisible = true; 

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _currentState = new MenuState(this, graphics.GraphicsDevice, Content);
        }
        bool firstPress = true;
        protected override void Update(GameTime gameTime)
        {
            if (_nextState != null)
            {
                _currentState = _nextState;
                _nextState = null;
            }


            _currentState.Update(gameTime);

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.F11) && firstPress && graphics.IsFullScreen)
            {
                graphics.IsFullScreen = false;
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.F11) && firstPress && !graphics.IsFullScreen)
            {
                graphics.IsFullScreen = true;
                firstPress = false;
            }
            else if (keyboardState.IsKeyUp(Keys.F11) && !firstPress)
            {
                firstPress = true;
            }
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                graphics.IsFullScreen = false;
            }
            graphics.HardwareModeSwitch = !graphics.IsFullScreen;
            graphics.ApplyChanges();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _currentState.Draw(gameTime, spriteBatch);

            base.Draw(gameTime);
        }
    }
}
