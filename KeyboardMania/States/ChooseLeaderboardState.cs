using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KeyboardMania.States;
using KeyboardMania.Controls;

namespace KeyboardMania.States
{
    public class ChooseLeaderboardState : State
    {
        private List<Component> _components;
        public ChooseLeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            var  viewButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "View",
            };
            viewButton.Click += ViewButton_Click;

            var editButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Edit",
            };
            editButton.Click += EditButton_Click;

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 3 * buttonSpacing),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                viewButton,
                editButton,
                returnButton
            };
        }
        private void ViewButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new ViewLeaderboardState(_game, _graphicsDevice, _content));
        }
        private void EditButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new EditLeaderboardState(_game, _graphicsDevice, _content));
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new MenuState(_game, _graphicsDevice, _content));
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Implement drawing logic here
        }

        public override void Update(GameTime gameTime)
        {
            // Implement update logic here
        }
    }
}
