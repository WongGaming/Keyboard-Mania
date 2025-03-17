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
using Microsoft.Xna.Framework.Input;

namespace KeyboardMania.States
{
    public class DisplaySettingsState : State
    {
        private List<Component> _components;
        private string _settingsFilePath;
        SpriteFont _font;
        private float _logoScale;
        private float _keyScaleFactor;
        private float _comboScaleFactor;
        private float _scoreScaleFactor;
        private float _hitScaleFactor;
        private List<string> DisplaySettings = new List<string>()
        {
            "LogoScale",
            "KeyScaleFactor",
            "ComboScaleFactor",
            "ScoreScaleFactor",
            "HitScaleFactor"
        };
        private List<Rectangle> _textBoxRectangle;
        private Texture2D _textBoxTexture;
        private Dictionary<int, bool> _textBoxSelected;
        private List<string> _textBoxText;
        public DisplaySettingsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            var parseDisplaySettings = new ParseDisplaySettings(content);
            parseDisplaySettings.ParseLogoScaling(_settingsFilePath, _logoScale);
            parseDisplaySettings.ParseDisplayGameplayScaling(_settingsFilePath, ref _keyScaleFactor,ref _comboScaleFactor,ref _scoreScaleFactor,ref _hitScaleFactor);
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            int buttonSpacing = 50;
            _textBoxTexture = new Texture2D(graphicsDevice, 1, 1);
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
            _textBoxRectangle = new List<Rectangle>
            {
                new Rectangle(100, 150, 200, 30),
                new Rectangle(100, 200, 200, 30),
                new Rectangle(100,250,200,30),
                new Rectangle(100, 300, 200, 30),
                new Rectangle(100, 350, 200, 30)
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                saveButton,
                returnButton
            };
            _textBoxText = new List<string>();
            _textBoxSelected = new Dictionary<int, bool>();
            for (int i = 0; i < 5; i++)
            {
                _textBoxSelected.Add(i, false);
            }
            InstantiateRectangleText(_logoScale, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor, _textBoxText);
            game.Window.TextInput += TextInputHandler;
        }
        private void InstantiateRectangleText(float LogoScale, float KeyScaleFactor, float ComboScaleFactor, float ScoreScaleFactor, float HitScaleFactor, List<string> TextBoxText)
        {
            TextBoxText.Add(LogoScale.ToString());
            TextBoxText.Add(KeyScaleFactor.ToString());
            TextBoxText.Add(ComboScaleFactor.ToString());
            TextBoxText.Add(ScoreScaleFactor.ToString());
            TextBoxText.Add(HitScaleFactor.ToString());
        }
        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_textBoxSelected[i])
                {
                    if (args.Key == Keys.Back && _textBoxText[i].Length > 0)
                    {
                        _textBoxText[i] = _textBoxText[i][..^1];
                    }
                    else if (args.Key == Keys.Enter)
                    {
                    }
                    else if (!char.IsControl(args.Character))
                    {
                        _textBoxText[i] += args.Character;
                    }
                }
            }
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
            int i = 0;
            foreach (var rect in _textBoxRectangle)
            {
                spriteBatch.Draw(_textBoxTexture, _textBoxRectangle[i], Color.Gray);
                spriteBatch.DrawString(_font, _textBoxText[i], new Vector2(300, 300 - i * 50), Color.White);
                spriteBatch.DrawString(_font, DisplaySettings[i], new Vector2(100, 300 - i * 50), Color.White);
                i++;
            }

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
            foreach (var rect in _textBoxRectangle)
            {

            }
            
        }
    }
}
