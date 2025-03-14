using KeyboardMania.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



//WHEN DONE, USE _game.ChangeState(new GameState(_game, _graphicsDevice, _content)); AND DELETE THIS



namespace KeyboardMania.States
{
    internal class BeatmapChooserState : State
    {
        private List<Component> _components;
        private string _rootDirectory;
        private List<string> _folders;
        private int _selectedItem; //currently selected folder
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;
        private List<string> _beatmaps;
        public BeatmapChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _graphicsDevice = graphicsDevice;
            _font = _content.Load<SpriteFont>("Fonts/Font");
            //_rootDirectory = Path.Combine(Environment.CurrentDirectory, "Beatmaps"); (old, only works for the files in BIN folder)
            _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "Beatmaps")); //this parses the path, removes 3 locations, and then looks for Beatmaps folder .
            _folders = new List<string>();
            _selectedItem = 0;
            _beatmaps = new List<string>();
            LoadFolders();
            GetBeatmaps();
            var buttonTexture = _content.Load<Texture2D>("Controls/Button");
            int buttonSpacing = 50;
            var buttonFont = _content.Load<SpriteFont>("Fonts/Font");
            var playGameButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 1 * buttonSpacing),
                Text = "Play",
            };

            playGameButton.Click += PlayGameButton_Click;

            var returnButton = new Button(buttonTexture, buttonFont)
            {
                Position = new Vector2((_graphicsDevice.Viewport.Width - (buttonTexture.Width)) / 2, (_graphicsDevice.Viewport.Height - (buttonTexture.Height)) / 2 + 2 * buttonSpacing),
                Text = "Return",
            };
            returnButton.Click += ReturnButton_Click;
            _components = new List<Component>()
            {
                playGameButton,
                returnButton
            };
        }
        private void PlayGameButton_Click(object sender, EventArgs e)
        {
            _game.ChangeState(new GameState(_game, _graphicsDevice, _content, _beatmaps[_selectedItem], LookForMp3File()));
        }
        private void ReturnButton_Click(object sender, EventArgs e)
        {
           _game.ChangeState(new MenuState(_game, _graphicsDevice, _content));
        }
        private void LoadFolders()
        {
            _folders.Clear();
            var directories = Directory.GetDirectories(_rootDirectory);
            foreach (var directory in directories)
            {
                _folders.Add(Path.GetFileName(directory));
            }
        }


        private void GetBeatmaps() //loads all beatmaps from all folders in Beatmaps
        {
            for(int i = 0; i < _folders.Count; i++)
            {
                var beatmaps = Directory.GetFiles(Path.Combine(_rootDirectory, _folders[i]), "*.osu");
                foreach (var osu in beatmaps)
                {
                    Console.WriteLine(osu);
                    _beatmaps.Add(osu);
                }
            }
        }
        bool firstPress = true;
        bool holding = false; //attempt to implement holding to move up and down faster
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down) && firstPress)
            {
                _selectedItem++;
                if (_selectedItem >= _folders.Count)
                {
                    _selectedItem = 0;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Up) && firstPress)
            {
                _selectedItem--;
                if (_selectedItem < 0)
                {
                    _selectedItem = _folders.Count - 1;
                }
                firstPress = false;
            }
            else if (keyboardState.IsKeyDown(Keys.Enter) && firstPress)
            {
                _game.ChangeState(new GameState(_game, _graphicsDevice, _content, _beatmaps[_selectedItem], LookForMp3File()));
            }
            if(keyboardState.IsKeyUp(Keys.Down) && keyboardState.IsKeyUp(Keys.Up) && keyboardState.IsKeyUp(Keys.Enter))
            {
                firstPress = true;
            }
        }

        private string LookForMp3File()
        {
            var files = Directory.GetFiles(Path.Combine(_rootDirectory, _folders[_selectedItem]));
            foreach (var file in files)
            {
                if (file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }
            return null;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            foreach (var component in _components)
            {
                component.Draw(gameTime, spriteBatch);
            }
            for (int i = 0; i < _beatmaps.Count; i++)
            {
                if (i == _selectedItem)
                {
                    spriteBatch.DrawString(_font, _folders[i], new Vector2(100, 100 + i * 20), Color.Red);
                }
                else
                {
                    spriteBatch.DrawString(_font, _folders[i], new Vector2(100, 100 + i * 20), Color.White);
                }
            }

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput();
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }
        }
    }
}

