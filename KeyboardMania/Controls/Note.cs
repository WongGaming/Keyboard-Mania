using KeyboardMania.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KeyboardMania.Controls
{
    public class Input
    {
        public Keys D { get; set; }
        public Keys F { get; set; }
        public Keys J { get; set; }
        public Keys K { get; set; }
    }

    public class Note : Component
    {
        #region Fields

        private Texture2D _texture;

        #endregion

        #region Properties

        public Vector2 Position { get; set; }

        public Rectangle Key
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            }
        }

        #endregion

        #region Constructor

        public Note(Texture2D texture)
        {
            _texture = texture;
        }

        #endregion

        #region Methods

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Key, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
        }

        #endregion
    }
}
