using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace KeyboardMania.States
{
    public class ViewLeaderboardState : State
    {
        private List<Component> _components;

        public ViewLeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            // Initialize components for the ViewLeaderboardState
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - buttonTexture.Width) / 2, (_graphicsDevice.Viewport.Height - buttonTexture.Height) / 2),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;

            _components = new List<Component>()
            {
                returnButton
            };
        }

        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new ChooseLeaderboardState(_game, _graphicsDevice, _content));
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            foreach (var component in _components)
                component.Draw(gameTime, spriteBatch);

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
                component.Update(gameTime);
        }
    }
}
