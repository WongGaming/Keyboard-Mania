using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KeyboardMania.States;
using KeyboardMania.Controls;
using System.IO;

namespace KeyboardMania.States
{
    public class ViewLeaderboardState : State
    {
        private List<Component> _components;
        private SpriteFont _font;
        private string _leaderboardDirectory;
        private List<string> _leaderboardLines = new List<string>();
        private string _leaderboard;
        public ViewLeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, string saveDirectory, string leaderboard)
            : base(game, graphicsDevice, content)
        {
            _leaderboard = leaderboard;
            _leaderboardDirectory = saveDirectory + "\\" + leaderboard;
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            int buttonSpacing = 50;
            var editButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Edit",
            };
            editButton.Click += EditButton_Click;
            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;

            _components = new List<Component>()
            {
                editButton,
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
        private void EditButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new EditLeaderboardState(_game, _graphicsDevice, _content, _leaderboardDirectory, _leaderboard));
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
                spriteBatch.DrawString(_font, line, new Vector2(100, y), Color.White);
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
        }
    }
}
