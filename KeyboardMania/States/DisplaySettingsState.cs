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
using static System.Formats.Asn1.AsnWriter;
using NAudio.Wave;

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
        private List<string> _settingName = new List<string>()
        {
            "LogoScale",
            "KeyScaleFactor",
            "ComboScaleFactor",
            "ScoreScaleFactor",
            "HitScaleFactor"
        };
        private List<bool> _settingSelected;
        private int _selectedItem;
        private List<string> _settingValues;
        public DisplaySettingsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _font = content.Load<SpriteFont>("Fonts/Font");
            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            var instantiateSettings = new InstantiateSettings();
            if ((_settingsFilePath.Count() == 0))
            {
                string _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
                instantiateSettings.InitialiseSettings(_rootDirectory);
                _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            }
            instantiateSettings.CheckForIncomplete(_settingsFilePath);
            var parseDisplaySettings = new ParseDisplaySettings(content);
            parseDisplaySettings.ParseLogoScaling(_settingsFilePath,ref _logoScale);
            parseDisplaySettings.ParseDisplayGameplayScaling(_settingsFilePath, ref _keyScaleFactor, ref _comboScaleFactor, ref _scoreScaleFactor, ref _hitScaleFactor);
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            int buttonSpacing = 50;
            var saveButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Save",
            };
            saveButton.Click += SaveButton_Click;
            var defaultResetButton = new Button(buttonTexture, _font)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Reset to Default",
            };
            defaultResetButton.Click += DefaultResetButton_Click;
            var resetButton = new Button(buttonTexture, _font)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 3 * buttonSpacing),
                Text = "Reset to Last Save",
            };
            resetButton.Click += ResetButton_Click;
            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 4 * buttonSpacing),
                Text = "Return (SAVE FIRST)",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                saveButton,
                defaultResetButton,
                resetButton,
                returnButton
            };
            _settingValues = new List<string>();
            _settingSelected = new List<bool>();
            for (int i = 0; i < 5; i++)
            {
                _settingSelected.Add(false);
            }
            InstantiateInitialSettings(_logoScale, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor, _settingValues);
            game.Window.TextInput += TextInputHandler;
        }
        private void InstantiateInitialSettings(float LogoScale, float KeyScaleFactor, float ComboScaleFactor, float ScoreScaleFactor, float HitScaleFactor, List<string> InitialSettings)
        {
            InitialSettings.Add(LogoScale.ToString());
            InitialSettings.Add(KeyScaleFactor.ToString());
            InitialSettings.Add(ComboScaleFactor.ToString());
            InitialSettings.Add(ScoreScaleFactor.ToString());
            InitialSettings.Add(HitScaleFactor.ToString());
        }

        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_settingSelected[i])
                {
                    if (args.Key == Keys.Back && _settingValues[i].Length > 0)
                    {
                        _settingValues[i] = _settingValues[i][..^1];
                    }
                    else if (!char.IsControl(args.Character))
                    {
                        if (char.IsDigit(args.Character) || args.Character == '.')
                        {
                            _settingValues[i] += args.Character;
                        }
                    }
                }
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            var parseDisplaySettings = new ParseDisplaySettings(_content);
            parseDisplaySettings.SaveNewSettings(_settingsFilePath, _logoScale, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor);
        }
        private void DefaultResetButton_Click(object sender, EventArgs e)
        {
            var instantiateSettings = new InstantiateSettings();
            instantiateSettings.InitialiseDisplay(_settingsFilePath);
            _game.ChangeState(new DisplaySettingsState(_game, _graphicsDevice, _content));
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            InstantiateInitialSettings(_logoScale, _keyScaleFactor, _comboScaleFactor, _scoreScaleFactor, _hitScaleFactor, _settingValues);
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
            for (int i = 0; i < _settingName.Count; i++)
            {
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(_font, _settingName[i], new Vector2(100, 100 + i * 50), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(_font, _settingName[i], new Vector2(100, 100 + i * 50), Color.White);
                }
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(_font, _settingValues[i], new Vector2(300, 100 + i * 50), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(_font, _settingValues[i], new Vector2(300, 100 + i * 50), Color.White);
                }
            }
            for (int i = 0; i < _settingSelected.Count; i++)
            {
                if (_settingSelected[i])
                {
                    spriteBatch.DrawString(_font, $"{_settingName[i]} is currently selected", new Vector2((_graphicsDevice.Viewport.Width) / 2, (_graphicsDevice.Viewport.Height) / 2 - 200), Color.White);
                }
            }
            spriteBatch.End();
        }
        bool firstPress = true;
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down) && firstPress && CheckAllFalse(_settingSelected))
            {
                _selectedItem++;
                if (_selectedItem >= _settingName.Count)
                {
                    _selectedItem = 0;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && firstPress && CheckAllFalse(_settingSelected))
            {
                _selectedItem--;
                if (_selectedItem < 0)
                {
                    _selectedItem = _settingName.Count - 1;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Enter) && firstPress && !_settingSelected[_selectedItem])
            {

                _settingSelected[_selectedItem] = true;
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Enter) && firstPress && _settingSelected[_selectedItem])
            {
                _settingSelected[_selectedItem] = false;

                if (_selectedItem == 0)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = "0";
                    }
                    if (int.TryParse(_settingValues[_selectedItem], out int intValue))
                    {
                        _logoScale = (float)intValue;
                    }
                }
                else if (_selectedItem == 1)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = "0";
                    }
                    if (int.TryParse(_settingValues[_selectedItem], out int intValue))
                    {
                        _keyScaleFactor = (float)intValue;
                    }
                }
                else if (_selectedItem == 2)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = "0";
                    }
                    if (int.TryParse(_settingValues[_selectedItem], out int intValue))
                    {
                        _comboScaleFactor = (float)intValue;
                    }
                }
                else if (_selectedItem == 3)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = "0";
                    }
                    if (int.TryParse(_settingValues[_selectedItem], out int intValue))
                    {
                        _scoreScaleFactor = (float)intValue;
                    }
                }
                else if (_selectedItem == 4)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = "0";
                    }
                    if (int.TryParse(_settingValues[_selectedItem], out int intValue))
                    {
                        _hitScaleFactor = (float)intValue;
                    }
                }
                firstPress = false;
            }
            if (keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Enter))
            {
                firstPress = true;
            }
        }
        private bool CheckAllFalse(List<bool> settingsSelected)
        {
            bool allFalse = true;
            for (int i = 0; i < settingsSelected.Count; i++)
            {
                if (settingsSelected[i])
                {
                    allFalse = false;
                }
            }
            return allFalse;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
            HandleInput();
        }
    }
}
