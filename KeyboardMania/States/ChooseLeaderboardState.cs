using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KeyboardMania.States;
using KeyboardMania.Controls;
using System.Net;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace KeyboardMania.States
{
    public class ChooseLeaderboardState : State
    {
        private List<Component> _components;
        private string _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Leaderboard");
        private List<string> _leaderboards = new List<string>();
        float logoScale = 0.35f;
        private int _selectedItem; //currently selected folder
        private SpriteFont _font;
        public ChooseLeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            var  viewButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "View",
            };
            viewButton.Click += ViewButton_Click;

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                viewButton,
                returnButton
            };
            LoadLeaderboards();
        }
        private void LoadLeaderboards()
        {
            _leaderboards.Clear();
            var files = Directory.GetFiles(_saveDirectory);
            foreach (var file in files)
            {
                _leaderboards.Add(Path.GetFileName(file));
            }
        }
        bool firstPress = true;
        bool holding = false; //attempt to implement holding to move up and down faster
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
            {
                _selectedItem++;
                if (_selectedItem >= _leaderboards.Count)
                {
                    _selectedItem = 0;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && firstPress)
            {
                _selectedItem--;
                if (_selectedItem < 0)
                {
                    _selectedItem = _leaderboards.Count - 1;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Enter) && firstPress)
            {
                ViewButton_Click(this, EventArgs.Empty);
            }
            if (keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Enter))
            {
                firstPress = true;
            }
        }
        private void ViewButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new ViewLeaderboardState(_game, _graphicsDevice, _content, _saveDirectory, _leaderboards[_selectedItem]));
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new MenuState(_game, _graphicsDevice, _content));
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }
            for (int i = 0; i < _leaderboards.Count; i++)
            {
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(_font, _leaderboards[i], new Vector2(100, 100 + i * 20), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(_font, _leaderboards[i], new Vector2(100, 100 + i * 20), Color.White);
                }
            }

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
            HandleInput();
        }
    }
}
