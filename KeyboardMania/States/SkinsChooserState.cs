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
        private string _skinDirectory;
        private string _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        private List<Texture2D> _noteTextures = new List<Texture2D>();
        private List<Texture2D> _holdTextures = new List<Texture2D>();
        private List<Texture2D> _lengthTextures = new List<Texture2D>();
        private List<string> _currentTextures = new List<string>();
        private Dictionary<int, string> _allTextures = new Dictionary<int, string>();
        private int _selectedItem;
        private List<int> _currentTextureIDs = new List<int>();
        private string[] settingsFilePath;
        private SpriteFont _font;
        private List<Component> _components;
        public SkinsChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
        {
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;

            settingsFilePath = Directory.GetFiles(_rootDirectory, "Settings.txt");
            _skinDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Content", "Skins", "NoteTextures"));
            _font = _content.Load<SpriteFont>("Fonts/Font");
            LoadTextures();
            var parseSkinSettings = new ParseSkinSettings(_content);
            parseSkinSettings.ParseCurrentSettings(settingsFilePath[0], _rootDirectory, _content, _noteTextures, _holdTextures, _lengthTextures, _currentTextures);
            InstantiateTextureID();
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
            _components = new List<Component>()
            {
                saveButton,
                defaultResetButton,
                returnButton,
            };
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            var parseSkinSettings = new ParseSkinSettings(_content);
            parseSkinSettings.SaveNewSettings(settingsFilePath[0], _currentTextures);
        }
        private void DefaultResetButton_Click(object sender, EventArgs e)
        {
            _currentTextures.Clear();
            for (int i = 0; i < 4; i++)
            {
                _currentTextures.Add("WhiteNote");
            }
            _currentTextureIDs.Clear();
            for(int i = 0; i < _allTextures.Count; i++)
            {
                for (int j = 0; j < _currentTextures.Count; j++)
                {
                    if (_currentTextures[j] == _allTextures[i])
                    {
                        _currentTextureIDs.Add(i);
                    }
                }
            }
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new OptionsMenuState(_game, _graphicsDevice, _content));
        }

        private void InstantiateTextureID()
        {
            _currentTextureIDs.Clear();
            for (int i = 0; i < _allTextures.Count; i++)
            {
                for (int j = 0; j < _currentTextures.Count; j++)
                {
                    if (_currentTextures[j] == _allTextures[i])
                    {
                        _currentTextureIDs.Add(i);
                    }
                }
            }
        }
        private void LoadTextures()
        {
            _allTextures.Clear();
            int i = 0;
            var directories = Directory.GetDirectories(_skinDirectory);
            foreach (var directory in directories)
            {
                _allTextures.Add(i, Path.GetFileName(directory));
                i = i + 1;
            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            float scale = 1.5f; // Change this value to scale the text

            for (int i = 0; i < _currentTextures.Count; i++)
            {
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(_font, _currentTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.DrawString(_font, _currentTextures[i], new Vector2(100 + i * 100 * scale, 100), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                }
            }
            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }
            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime)
        {

        }
        bool firstPress = true;
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
            {
                _currentTextureIDs[_selectedItem]++;
                if (_currentTextureIDs[_selectedItem] >= _allTextures.Count)
                {
                    _currentTextureIDs[_selectedItem] = 0;
                }
                _currentTextures[_selectedItem] = _allTextures[_currentTextureIDs[_selectedItem]];
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && firstPress)
            {
                _currentTextureIDs[_selectedItem]--;
                if (_currentTextureIDs[_selectedItem] < 0)
                {
                    _currentTextureIDs[_selectedItem] = _allTextures.Count - 1;
                }
                _currentTextures[_selectedItem] = _allTextures[_currentTextureIDs[_selectedItem]];
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Left) && firstPress)
            {
                _selectedItem--;
                if (_selectedItem < 0)
                {
                    _selectedItem = _currentTextures.Count - 1;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Right) && firstPress)
            {
                _selectedItem++;
                if (_selectedItem >= _currentTextures.Count)
                {
                    _selectedItem = 0;
                }
                firstPress = false;
            }
            /*else if (keyboardState.IsKeyDown(Keys.Enter) && firstPress)
            {
                _game.ChangeState(new GameState(_game, _graphicsDevice, _content, _beatmaps[_selectedItem], LookForMp3File()));
            }*/
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
