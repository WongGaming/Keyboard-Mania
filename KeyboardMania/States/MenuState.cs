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
    public class MenuState : State
    {
        private List<Component> _components;
        private Texture2D _logo;
        float logoScale = 0.35f; // .75f = home pc, 0.35f = laptop
        private string settingsFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
        public MenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            var parseDisplaySettings = new ParseDisplaySettings(content);
            parseDisplaySettings.ParseLogoScaling(settingsFileLocation, ref logoScale);

            _logo = _content.Load<Texture2D>("Textures/blacklogo");
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            var playGameButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Play",
            };
            playGameButton.Click += PlayGameButton_Click;

            var LeaderboardButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Leaderboard",
            };
            LeaderboardButton.Click += LeaderboardButton_Click;

            var optionsButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) /2+ 3 * buttonSpacing),
                Text = "Options",
            };
            optionsButton.Click += OptionsButton_Click;

            var quitGameButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 4 * buttonSpacing),
                Text = "Exit",
            };

            quitGameButton.Click += QuitGameButton_Click;

            _components = new List<Component>()
            {
                playGameButton,
                LeaderboardButton,
                optionsButton,
                quitGameButton,
            };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            
            Vector2 position = new Vector2((_graphicsDevice.Viewport.Width - (_logo.Width * logoScale)) / 2,(_graphicsDevice.Viewport.Height - (_logo.Height * logoScale)) / 2 - (_graphicsDevice.Viewport.Height / 4));


            spriteBatch.Draw(_logo, position, null, Color.White, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);

            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End();
        }

        private void LeaderboardButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new ChooseLeaderboardState(_game, _graphicsDevice, _content));
        }

        private void PlayGameButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new BeatmapChooserState(_game, _graphicsDevice, _content));
        }

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new OptionsMenuState(_game, _graphicsDevice, _content, settingsFileLocation));
        }
        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
        }

        private void QuitGameButton_Click(object sender, EventArgs e)
        {
            _game.Exit();
        }
    }
}
