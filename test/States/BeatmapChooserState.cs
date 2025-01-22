using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;



//WHEN DONE, USE _game.ChangeState(new GameState(_game, _graphicsDevice, _content)); AND DELETE THIS



namespace KeyboardMania.States
{
    internal class BeatmapChooserState : State
    {
        private string _rootDirectory; //Beatmaps folder, stored in the Content folder.
        private List<string> _folders;
        private int _selectedItem; //currently selected folder
        private SpriteFont _font;
        private GraphicsDevice _graphicsDevice;

        public BeatmapChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
            : base(game, graphicsDevice, content)
        {
            _graphicsDevice = graphicsDevice;
            _font = _content.Load<SpriteFont>("Fonts/Font");
            _rootDirectory = Path.Combine(Environment.CurrentDirectory, "Content", "Beatmaps");
            _folders = new List<string>();
            _selectedItem = 0;
            LoadFolders();
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


        private void GetBeatmaps()
        {
            var beatmaps = Directory.GetFiles(Path.Combine(_rootDirectory, _folders[_selectedItem]), "*.osu");
            foreach (var beatmap in beatmaps)
            {
                Console.WriteLine(beatmap);
            }
        }
        private void HandleInput()
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                _selectedItem++;
                if (_selectedItem >= _folders.Count)
                {
                    _selectedItem = 0;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Up))
            {
                _selectedItem--;
                if (_selectedItem < 0)
                {
                    _selectedItem = _folders.Count - 1;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Enter))
            {
                _game.ChangeState(new GameState(_game, _graphicsDevice, _content, Path.Combine(_rootDirectory, _folders[_selectedItem]), LookForMp3File()));
            }
        }

        private string LookForMp3File()
        {
            var files = Directory.GetFiles(Path.Combine(_rootDirectory, _folders[_selectedItem]), "*.mp3");
            if (files.Length > 0)
            {
                return files[0];
            }
            return null;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (string directory in _folders)
            {
                spriteBatch.DrawString(_font, directory, new Vector2(100, 100), Color.White);
            }
        }

        public override void PostUpdate(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}

