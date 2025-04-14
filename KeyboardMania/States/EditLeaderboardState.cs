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
    public class EditLeaderboardState : State
    {
        string _leaderboardDirectory;
        List<string> _leaderboardLines = new List<string>();
        List<Component> _components;
        SpriteFont _font;
        private int _selectedItem;
        private string _leaderboard;
        public EditLeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content,string leaderboardDirectory, string leaderboard)
            : base(game, graphicsDevice, content)
        {
            _leaderboard = leaderboard;
            _font = _content.Load<SpriteFont>("Fonts/Font");
            _leaderboardDirectory = leaderboardDirectory;
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            int buttonSpacing = 50;
            var deleteButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Delete",
            };
            deleteButton.Click += DeleteButton_Click;
            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                deleteButton,
                returnButton
            };
            GetLeaderboardLines(_leaderboardDirectory);
        }
        private void GetLeaderboardLines(string leaderboardDirectory)
        {
            _font = _content.Load<SpriteFont>("Fonts/Font");
            var lines = File.ReadAllLines(leaderboardDirectory);
            foreach (var line in lines)
            {
                _leaderboardLines.Add(line);
            }
        }
        bool firstPress = true;
        bool holding = false;
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
            {
                _selectedItem++;
                if (_selectedItem >= _leaderboardLines.Count)
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
                    _selectedItem = _leaderboardLines.Count - 1;
                }
                firstPress = false;
            }
            if (keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up))
            {
                firstPress = true;
            }
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_leaderboardLines.Count > 0)
            {
            _leaderboardLines.RemoveAt(_selectedItem);
            File.WriteAllLines(_leaderboardDirectory, _leaderboardLines);
            }
            _selectedItem--;
            if (_selectedItem < 0)
            {
                _selectedItem = 0;
            }
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new ChooseLeaderboardState(_game, _graphicsDevice, _content));
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }
            spriteBatch.DrawString(_font, $"Leaderboard - {Path.GetFileNameWithoutExtension(_leaderboard)}", new Vector2(100, 50), Color.White);
            int y = 100;
            foreach (var line in _leaderboardLines)
            {
                if (_leaderboardLines.IndexOf(line) == _selectedItem)
                {
                    spriteBatch.DrawString(_font, line, new Vector2(100, y), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(_font, line, new Vector2(100, y), Color.White);
                }
                y += 50;
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
