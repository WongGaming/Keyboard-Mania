using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KeyboardMania.Controls
{
    public class HitFeedback
    {
        private Texture2D _texture;
        public Vector2 Position;
        private float _velocity;

        public HitFeedback(Texture2D texture, Vector2 startPosition, float velocity)
        {
            _texture = texture;
            Position = startPosition;
            _velocity = velocity;
        }

        public void Update(GameTime gameTime)
        {
            Position.Y += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float keyScaleFactor)
        {
            spriteBatch.Draw(_texture, Position, null, Color.White, 0f, Vector2.Zero, keyScaleFactor, SpriteEffects.None, 0f);
        }

        public bool IsOffScreen(int screenHeight)
        {
            return Position.Y > screenHeight;
        }
    }
}
