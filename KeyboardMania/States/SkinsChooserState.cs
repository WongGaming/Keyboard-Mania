using KeyboardMania;
using KeyboardMania.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using KeyboardMania.Controls;

namespace KeyboardMania.States
{
    internal class SkinsChooserState : State
    {
        private string _noteSkinDirectory;
        private string _hitSkinDirectory;
        private string _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        
        private List<Texture2D> _noteTextures = new List<Texture2D>();
        private List<Texture2D> _holdTextures = new List<Texture2D>();
        private List<Texture2D> _lengthTextures = new List<Texture2D>();
        
        private List<Texture2D> _hitTextures = new List<Texture2D>();

        private List<string> _currentNoteTextures = new List<string>();
        private List<int> _currentNoteTextureIDs = new List<int>();
        
        private List<string> _currentHitTextures = new List<string>();
        private List<int> _currentHitTextureIDs = new List<int>();

        private Dictionary<int, string> _allNoteTextures = new Dictionary<int, string>();
        private Dictionary<int,string> _allHitTextures = new Dictionary<int, string>();
        private int _selectedItem;
        private string _settingsFilePath;
        private SpriteFont _font;
        private List<Component> _components;
        private bool _noteSkins = true;
        public SkinsChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
        {
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;

            _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            _noteSkinDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Content", "Skins", "NoteTextures"));
            _hitSkinDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Content", "Skins", "HitTextures"));
            _font = _content.Load<SpriteFont>("Fonts/Font");

            LoadNoteTextures();
            LoadHitTextures();
            if ((_settingsFilePath.Count() == 0))
            {
                var instantiateSettings = new InstantiateSettings();
                instantiateSettings.InitialiseSettings(_rootDirectory);
                _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            }
            var parseSkinSettings = new ParseSkinSettings(_content);
            parseSkinSettings.ParseNoteCurrentSettings(_settingsFilePath, _rootDirectory, _content, _noteTextures, _holdTextures, _lengthTextures, _currentNoteTextures);
            parseSkinSettings.ParseHitCurrentSettings(_settingsFilePath, _rootDirectory, _content,_hitTextures, _currentHitTextures);
            InstantiateNoteTextureID();
            InstantiateHitTextureID();

            var saveButton = new Button(buttonTexture, _font)
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
            var returnButton = new Button(buttonTexture, _font)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 3 * buttonSpacing),
                Text = "Return (SAVE FIRST)",
            };
            returnButton.Click += ReturnButton_Click;
            var switchButton = new Button(buttonTexture, _font)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 4 * buttonSpacing),
                Text = _noteSkins ? "Switch to Hit Skins" : "Switch to Note Skins"
            };
            switchButton.Click += SwitchButton_Click;
            switchButton.Click += (sender, e) => switchButton.Text = _noteSkins ? "Switch to Hit Skins" : "Switch to Note Skins";
            _components = new List<Component>()
            {
                saveButton,
                defaultResetButton,
                switchButton,
                returnButton
            };
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            var parseSkinSettings = new ParseSkinSettings(_content);
            parseSkinSettings.SaveNewSettings(_settingsFilePath, _currentNoteTextures,_currentHitTextures);
        }
        private void DefaultResetButton_Click(object sender, EventArgs e)
        {
            _currentNoteTextures.Clear();
            for (int i = 0; i < 4; i++)
            {
                _currentNoteTextures.Add("WhiteNote");
            }
            _currentNoteTextureIDs.Clear();

            for (int i = 0; i < _allNoteTextures.Count; i++)
            {
                for (int j = 0; j < _currentNoteTextures.Count; j++)
                {
                    if (_currentNoteTextures[j] == _allNoteTextures[i])
                    {
                        _currentNoteTextureIDs.Add(i);
                    }
                }
            }
            _currentHitTextures.Clear();
            for (int i = 0; i < 6; i++)
            {
                _currentHitTextures.Add("Circle");
            }
            _currentHitTextureIDs.Clear();
            for (int i = 0; i < _allHitTextures.Count; i++)
            {
                for (int j = 0; j < _currentHitTextures.Count; j++)
                {
                    if (_currentHitTextures[j] == _allHitTextures[i])
                    {
                        _currentHitTextureIDs.Add(i);
                    }
                }
            }
        }
        private void SwitchButton_Click(object sender, EventArgs e)
        {
            _selectedItem = 0;
            if (_noteSkins)
            {
                _noteSkins = false;
            }
            else
            {
                _noteSkins = true;
            }
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new OptionsMenuState(_game, _graphicsDevice, _content, _settingsFilePath));
        }

        private void InstantiateNoteTextureID()
        {
            _currentNoteTextureIDs.Clear();
            for (int i = 0; i < _allNoteTextures.Count; i++)
            {
                for (int j = 0; j < _currentNoteTextures.Count; j++)
                {
                    if (_currentNoteTextures[j] == _allNoteTextures[i])
                    {
                        _currentNoteTextureIDs.Add(i);
                    }
                }
            }
        }
        private void InstantiateHitTextureID()
        {
            _currentHitTextureIDs.Clear();
            for (int i = 0; i < _allHitTextures.Count; i++)
            {
                for (int j = 0; j < _currentHitTextures.Count; j++)
                {
                    if (_currentHitTextures[j] == _allHitTextures[i])
                    {
                        _currentHitTextureIDs.Add(i);
                    }
                }
            }
        }
        private void LoadNoteTextures()
        {
            _allNoteTextures.Clear();
            int i = 0;
            var directories = Directory.GetDirectories(_noteSkinDirectory);
            foreach (var directory in directories)
            {
                _allNoteTextures.Add(i, Path.GetFileName(directory));
                i = i + 1;
            }
        }
        private void LoadHitTextures()
        {
            _allHitTextures.Clear();
            int i = 0;
            var directories = Directory.GetDirectories(Path.Combine(_hitSkinDirectory, "0")); //change this to be individual to each different hit texture l8r
            foreach (var directory in directories)
            {
                _allHitTextures.Add(i, Path.GetFileName(directory));
                i = i + 1;
            }

        }   
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            float scale = 1.5f; // Change this value to scale the text
            if (_noteSkins)
            {
                for (int i = 0; i < _currentNoteTextures.Count; i++)
                {
                    if (i == _selectedItem)
                    {
                        spriteBatch.DrawString(_font, _currentNoteTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.DrawString(_font, _currentNoteTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _currentHitTextures.Count; i++)
                {
                    if (i == _selectedItem)
                    {
                        spriteBatch.DrawString(_font, _currentHitTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.DrawString(_font, _currentHitTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                }
               
            }
            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }
            spriteBatch.End();
        }

        bool firstPress = true;
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (_noteSkins)
            {
                if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
                {
                    _currentNoteTextureIDs[_selectedItem]++;
                    if (_currentNoteTextureIDs[_selectedItem] >= _allNoteTextures.Count)
                    {
                        _currentNoteTextureIDs[_selectedItem] = 0;
                    }
                    _currentNoteTextures[_selectedItem] = _allNoteTextures[_currentNoteTextureIDs[_selectedItem]];
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Up) && firstPress)
                {
                    _currentNoteTextureIDs[_selectedItem]--;
                    if (_currentNoteTextureIDs[_selectedItem] < 0)
                    {
                        _currentNoteTextureIDs[_selectedItem] = _allNoteTextures.Count - 1;
                    }
                    _currentNoteTextures[_selectedItem] = _allNoteTextures[_currentNoteTextureIDs[_selectedItem]];
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Left) && firstPress)
                {
                    _selectedItem--;
                    if (_selectedItem < 0)
                    {
                        _selectedItem = _currentNoteTextures.Count - 1;
                    }
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Right) && firstPress)
                {
                    _selectedItem++;
                    if (_selectedItem >= _currentNoteTextures.Count)
                    {
                        _selectedItem = 0;
                    }
                    firstPress = false;
                }
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
                {
                    _currentHitTextureIDs[_selectedItem]++;
                    if (_currentHitTextureIDs[_selectedItem] >= _allHitTextures.Count)
                    {
                        _currentHitTextureIDs[_selectedItem] = 0;
                    }
                    _currentHitTextures[_selectedItem] = _allHitTextures[_currentHitTextureIDs[_selectedItem]];
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Up) && firstPress)
                {
                    _currentHitTextureIDs[_selectedItem]--;
                    if (_currentHitTextureIDs[_selectedItem] < 0)
                    {
                        _currentHitTextureIDs[_selectedItem] = _allHitTextures.Count - 1;
                    }
                    _currentHitTextures[_selectedItem] = _allHitTextures[_currentHitTextureIDs[_selectedItem]];
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Left) && firstPress)
                {
                    _selectedItem--;
                    if (_selectedItem < 0)
                    {
                        _selectedItem = _currentHitTextures.Count - 1;
                    }
                    firstPress = false;
                }
                else if (keyboardState.IsKeyDown(Keys.Right) && firstPress)
                {
                    _selectedItem++;
                    if (_selectedItem >= _currentHitTextures.Count)
                    {
                        _selectedItem = 0;
                    }
                    firstPress = false;
                }
            }
            if (keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Left) && keyboardState.IsKeyUp(Keys.Right) && keyboardState.IsKeyUp(Keys.Enter))
            {
                firstPress = true;
            }
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
