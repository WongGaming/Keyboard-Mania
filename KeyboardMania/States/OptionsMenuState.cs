﻿using System;
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

namespace KeyboardMania.States
{
  public class OptionsMenuState : State
  {
        private List<Component> _components;
        private Texture2D _logo; 
        float logoScale = 0.35f; // .75f = home pc, 0.35f = laptop
        public OptionsMenuState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, string settingsFileLocation) : base(game, graphicsDevice, content)
    {
            var parseDisplaySettings = new ParseDisplaySettings(content);
            parseDisplaySettings.ParseLogoScaling(settingsFileLocation, ref logoScale);

            _logo = _content.Load<Texture2D>("Textures/blacklogo");
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            // Setup components

            var skinsChooserButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Skins Chooser",
            };
            skinsChooserButton.Click += SkinsChooserButton_Click;

            var displaySettingsButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Display Settings",
            };
            displaySettingsButton.Click += DisplaySettingsButton_Click;

            var gameplaySettingsButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 3 * buttonSpacing),
                Text = "Gameplay Settings",
            };

            gameplaySettingsButton.Click += GameplaySettingsButton_Click;

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 4 * buttonSpacing),
                Text = "Return",
            };

            returnButton.Click += ReturnButton_Click;

            _components = new List<Component>()
            {
                    skinsChooserButton,
                    displaySettingsButton,
                    gameplaySettingsButton,
                    returnButton
            };
        }
        private void SkinsChooserButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new SkinsChooserState(_game, _graphicsDevice, _content));
        }
        private void DisplaySettingsButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new DisplaySettingsState(_game, _graphicsDevice, _content));
        }
        private void GameplaySettingsButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new GameplaySettingsState(_game, _graphicsDevice, _content));
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new MenuState(_game, _graphicsDevice, _content));
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
            spriteBatch.Begin();

            Vector2 position = new Vector2((_graphicsDevice.Viewport.Width - (_logo.Width * logoScale)) / 2, (_graphicsDevice.Viewport.Height - (_logo.Height * logoScale)) / 2 - (_graphicsDevice.Viewport.Height / 4));            spriteBatch.Draw(_logo, position, null, Color.White, 0f, Vector2.Zero, logoScale, SpriteEffects.None, 0f);
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
