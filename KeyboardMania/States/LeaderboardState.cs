using KeyboardMania;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace KeyboardMania.States
private Texture2D _textBoxTexture;
private Rectangle _textBoxRectangle;
private string _userInput;
private bool _isTextBoxSelected;
namespace KeyboardMania.States
{
    public LeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
    : base(game, graphicsDevice, content)
    {
        _font = _content.Load<SpriteFont>("Fonts/Font");
        _message = "Good job, you finished the song - Enter your Username to record your score, or leave it blank to not save it!";
        _textBoxTexture = new Texture2D(graphicsDevice, 1, 1);
        _textBoxTexture.SetData(new[] { Color.White });
        _textBoxRectangle = new Rectangle(100, 150, 200, 30);
        _userInput = string.Empty;
        _isTextBoxSelected = false;
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();
        KeyboardState keyboardState = Keyboard.GetState();

        if (mouseState.LeftButton == ButtonState.Pressed && _textBoxRectangle.Contains(mouseState.Position))
        {
            _isTextBoxSelected = true;
        }
        else if (mouseState.LeftButton == ButtonState.Pressed)
        {
            _isTextBoxSelected = false;
        }

        if (_isTextBoxSelected)
        {
            foreach (var key in keyboardState.GetPressedKeys())
            {
                if (key == Keys.Back && _userInput.Length > 0)
                {
                    _userInput = _userInput[..^1];
                }
                else if (key == Keys.Enter)
                {
                    // Handle enter key if needed
                }
                else
                {
                    _userInput += key.ToString();
                }
            }
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _graphicsDevice.Clear(Color.Black);
        spriteBatch.Begin();
        spriteBatch.DrawString(_font, _message, new Vector2(100, 100), Color.White);
        spriteBatch.Draw(_textBoxTexture, _textBoxRectangle, Color.Gray);
        spriteBatch.DrawString(_font, _userInput, new Vector2(105, 155), Color.Black);
        spriteBatch.End();
    }
}
