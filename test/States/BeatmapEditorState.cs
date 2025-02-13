using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using KeyboardMania;
using KeyboardMania.Controls;

namespace KeyboardMania.States
{
    public class BeatmapEditorState : State
    {
        private List<Component> _components;
        public BeatmapEditorState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
          : base(game, graphicsDevice, content)
        {
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");

            var generateBeatmapButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2(300, 200),
                Text = "Play",
            };

            generateBeatmapButton.Click += GenerateBeatmapButton_Click;

            var manualEditButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2(300, 250),
                Text = "Beatmap Editor",
            };

            manualEditButton.Click += ManualEditButton_Click;

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2(300, 300),
                Text = "Return",
            };

            returnButton.Click += ReturnButton_Click;

            _components = new List<Component>()
            {
            generateBeatmapButton,
            manualEditButton,
            returnButton,
            };
        }
        private void GenerateBeatmapButton_Click(object sender, EventArgs e)
        {

        }
        private void ManualEditButton_Click(object sender, EventArgs e)
        {

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

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime)
        {

        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
