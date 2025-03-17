using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using KeyboardMania.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KeyboardMania.States;
using System.IO;

namespace KeyboardMania.States
{
    public class DisplaySettingsState : State
    {
        private List<Component> _components;
        private string _settingsFilePath;
        private float _logoScale;
        private float _keyScaleFactor;
        private float _comboScaleFactor;
        private float _scoreScaleFactor;
        private float _hitScaleFactor;
        public DisplaySettingsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            var parseDisplaySettings = new ParseDisplaySettings(content);
            parseDisplaySettings.ParseLogoScaling(_settingsFilePath, _logoScale);
            parseDisplaySettings.ParseDisplayGameplayScaling(_settingsFilePath, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor);
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            int buttonSpacing = 50;
            var saveButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Save",
            };
            saveButton.Click += SaveButton_Click;
            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Return (SAVE FIRST)",
            };

            returnButton.Click += ReturnButton_Click;

            _components = new List<Component>()
            {
                saveButton,
                returnButton
            };
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            var parseDisplaySettings = new ParseDisplaySettings(_content);
            parseDisplaySettings.SaveNewSettings(_settingsFilePath, _logoScale, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor);
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new OptionsMenuState(_game, _graphicsDevice, _content, _settingsFilePath));
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
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
