using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace KeyboardMania
{
    public class Note : Component
    {
        public HitObject HitObject { get; set; }
        public List<Texture2D> Texture => _texture; // Expose texture to access height for hit calculation
        private List<Texture2D> _texture;
        private List<Texture2D> _holdLengthTexture;
        public Vector2 Position;
        public Vector2 Velocity;
        private bool _isHeld;
        public bool _currentlyHeld = false; //track if note is being hit at the moment
        public double _holdStartTime;
        public bool _firstPressed = true;
        public float Scale { get; set; } = 1f;

        public Note(ContentManager content, List<Texture2D> texture, bool isHeld, List<Texture2D> lengthTexture)
        {
            _texture = texture;
            _isHeld = isHeld;
            if (isHeld)
            {
                _holdLengthTexture = lengthTexture;
            }
        }

        public void StartHolding(double currentTime)
        {
            _isHeld = true;
            _holdStartTime = currentTime;
        }

        public void FailHold()
        {
            _isHeld = false;
        }

        public void CompleteHold()
        {
            _isHeld = false;
        }

        public override void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture != null)
            {
                if (!_isHeld)
                {
                    spriteBatch.Draw(_texture[HitObject.Lane], Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
                else
                {
                    DrawHoldNoteSegments(spriteBatch);
                }
            }
            else
            {
                throw new InvalidOperationException("Note has null texture");
            }
        }

        private void DrawHoldNoteSegments(SpriteBatch spriteBatch)
        {
            int segments;
            Vector2 segmentPosition;
            float totalHeight = Convert.ToSingle((HitObject.EndTime - HitObject.StartTime) / 1000.0) * Velocity.Y;
            segments = Convert.ToInt32(totalHeight / (_texture[HitObject.Lane].Height * Scale));
            if (segments < 1)
            {
                segments = 1;
            }

            Vector2 finalPosition = new Vector2(Position.X, Position.Y - (2 * segments * _holdLengthTexture[HitObject.Lane].Height * Scale) - 2 * _texture[HitObject.Lane].Height * Scale);
            Vector2 headPosition = new Vector2(Position.X, Position.Y - (_texture[HitObject.Lane].Height * Scale) + 2 * _holdLengthTexture[HitObject.Lane].Height * Scale);

            spriteBatch.Draw(_texture[HitObject.Lane], headPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            for (int i = 2; i <= (segments + 1) * 2 + 2; i++)
            {
                segmentPosition = new Vector2(Position.X, Position.Y - ((_texture[HitObject.Lane].Height * Scale) + ((i - 1) * (_holdLengthTexture[HitObject.Lane].Height) * Scale)) + 3 * _holdLengthTexture[HitObject.Lane].Height * Scale);
                spriteBatch.Draw(_holdLengthTexture[HitObject.Lane], segmentPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            }
            DrawEndTexture(spriteBatch, finalPosition);
        }

        private void DrawEndTexture(SpriteBatch spriteBatch, Vector2 finalPositon)
        {
            Vector2 originOfTexture = new Vector2(_texture[HitObject.Lane].Width, _texture[HitObject.Lane].Height);
            spriteBatch.Draw(_texture[HitObject.Lane], finalPositon, null, Color.White, MathHelper.Pi, originOfTexture, Scale, SpriteEffects.None, 0f);
        }

        public bool IsOffScreen(int screenHeight)
        {
            float noteHeight = _texture[HitObject.Lane].Height * Scale;
            return Position.Y > screenHeight + noteHeight;
        }

        public bool IsHoldOffScreen(int screenHeight, Note note)
        {
            float totalHeight = Convert.ToSingle((HitObject.EndTime - HitObject.StartTime) / 1000.0) * Velocity.Y;
            return Position.Y > screenHeight + totalHeight;
        }
    }
}
