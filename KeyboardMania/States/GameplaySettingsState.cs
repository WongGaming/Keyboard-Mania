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
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Text.RegularExpressions;

namespace KeyboardMania.States
{
    public class GameplaySettingsState : State
    {
        private List<Component> _components;
        private string _settingsFilePath;
        private float _noteVelocity;
        private List<Keys> _keyMapping = new List<Keys>();
        private double _latencyRemover;
        private int _fadeInTiming;
        private float _audioLatency;
        SpriteFont _font;
        private string _lastWorkingKeyMap;
        private List<string> _settingName = new List<string>()
        {
            "Note Velocity",
            "Key Mapping",
            "Latency Remover",
            "Fade In Timing",
            "Audio Latency"
        };
        private List<bool> _settingSelected;
        private int _selectedItem;
        private List<string> _settingValues;
        public GameplaySettingsState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
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
            var parseGameplaySettings = new ParseGameplaySettings(_content);
            parseGameplaySettings.ParseGameplayValues(_settingsFilePath, ref _noteVelocity, ref _keyMapping, ref _latencyRemover, ref _fadeInTiming, ref _audioLatency);
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
            var averageButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 4 * buttonSpacing),
                Text = "Latency = Average",
            };
            averageButton.Click += AverageButton_Click;
            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 5 * buttonSpacing),
                Text = "Return (SAVE FIRST)",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                saveButton,
                defaultResetButton,
                resetButton,
                averageButton,
                returnButton
            };
            _settingValues = new List<string>();
            _settingSelected = new List<bool>();
            for (int i = 0; i < 5; i++)
            {
                _settingSelected.Add(false);
            }
            InstantiateInitialSettings(_noteVelocity, _keyMapping, _latencyRemover, _fadeInTiming, _audioLatency, _settingValues);
            game.Window.TextInput += TextInputHandler;
        }
        private void InstantiateInitialSettings(float NoteVelocity, List<Keys> KeyMapping, double LatencyRemover, float FadeInTiming, float AudioLatency, List<string> InitialSettings)
        {
            InitialSettings.Add(NoteVelocity.ToString());
            InitialSettings.Add(KeyMapping.ToString());
            InitialSettings.Add(LatencyRemover.ToString());
            InitialSettings.Add(FadeInTiming.ToString());
            InitialSettings.Add(AudioLatency.ToString());
            string keys = "";
            for (int i = 0; i < _keyMapping.Count; i++)
            {
                keys += _keyMapping[i].ToString();
                if (!(i == _keyMapping.Count - 1))
                {
                    keys += ",";
                }
            }
            _settingValues[1] = keys;
            _lastWorkingKeyMap = keys; //FOR SAVING JUST IN CASE AN ERROR OCCURS
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
                        /*if (_settingValues[i] == "" || _settingValues[i] == "" )
                        {
                            _settingValues[i] = "";
                        }*/
                    }
                    else if (!char.IsControl(args.Character) && _selectedItem != 1)
                    {
                        if (char.IsDigit(args.Character) || args.Character == '.')
                        {
                            _settingValues[i] += args.Character;
                        }
                    }
                    else if (_selectedItem == 1 && !char.IsControl(args.Character))
                    {
                        _settingValues[i] += args.Character;
                    }
                }
            }
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            var parseGameplaySettings = new ParseGameplaySettings(_content);
            parseGameplaySettings.SaveNewSettings(_settingsFilePath, _noteVelocity, _keyMapping, _latencyRemover, _fadeInTiming, _audioLatency);
        }
        private void DefaultResetButton_Click(object sender, EventArgs e)
        {
            var instantiateSettings = new InstantiateSettings();
            instantiateSettings.InitialiseGameplay(_settingsFilePath);
            _game.ChangeState(new GameplaySettingsState(_game, _graphicsDevice, _content));
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            InstantiateInitialSettings(_noteVelocity, _keyMapping, _latencyRemover, _fadeInTiming, _audioLatency, _settingValues);
        }
        private void AverageButton_Click(Object sender, EventArgs e)
        {
            var averageHitTiming = new AverageHitTiming(_content);
            averageHitTiming.LoadTiming(ref _latencyRemover);
            _settingValues[2] = _latencyRemover.ToString();
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
                    if (i == 1)
                    {
                        spriteBatch.DrawString(_font, "SAVE THIS IN THE FORMAT [0],[1],[2],[3] with NO spaces in between, and each bracket represents a key on the keyboard (integer or character) [THERE IS A COMMA INBETWEEN THE NUMBERS]", new Vector2((_graphicsDevice.Viewport.Width) / 2, (_graphicsDevice.Viewport.Height) / 2 - 150), Color.White);
                    }
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
                        _noteVelocity = (float)intValue;
                    }
                }

                else if (_selectedItem == 1)
                {
                    if (_settingValues[_selectedItem] == "")
                    {
                        _settingValues[_selectedItem] = _lastWorkingKeyMap;
                    }
                    if (CheckKeyMapFormat(_settingValues[_selectedItem]))
                    {
                        KeyMappingParse(_settingValues[_selectedItem]);
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
                        _latencyRemover = (float)intValue;
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
                        _fadeInTiming = intValue;
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
                        _audioLatency = (float)intValue;
                    }
                }
                firstPress = false;
            }
            if (keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Enter))
            {
                firstPress = true;
            }
        }
        private void KeyMappingParse(string keyMappingValue)
        {
            string[] keyMappingValues = keyMappingValue.Split(',');
            for (int i = 0; i < keyMappingValues.Length; i++)
            {
                _keyMapping[i] = (Keys)Enum.Parse(typeof(Keys), keyMappingValues[i].ToUpper());
            }
        }
        private bool CheckKeyMapFormat(string input)
        {
            input = input.ToUpper();
            if (Regex.IsMatch(input, @"^(.,.,.,.)$"))
            {
                _settingValues[1] = input;
                _lastWorkingKeyMap = input;
                return true;
            }
            return false;
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
