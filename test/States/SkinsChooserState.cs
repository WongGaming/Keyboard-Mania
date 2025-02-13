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

namespace test.States
{
    internal class SkinsChooserState : State
    {
        private string _rootDirectory;
        private List<Texture2D> _noteTexture = new List<Texture2D>();

        public SkinsChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
        {
            _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
            string[] settingsFilePath = Directory.GetFiles(_rootDirectory, "Settings.txt");
            ParseCurrentSettings(settingsFilePath[0]);
            /* _lnoteTexture = _content.Load<Texture2D>("Textures/lnote");
             _mlnoteTexture = _content.Load<Texture2D>("Textures/mlnote");
             _mrnoteTexture = _content.Load<Texture2D>("Textures/mrnote");
             _rnoteTexture = _content.Load<Texture2D>("Textures/rnote");*/
        }
        private void ParseCurrentSettings(string _settingsFilePath)
        {
            string[] lines = File.ReadAllLines(_settingsFilePath);
            bool skinSettingsSection = false;
            List<string> noteTexture = new List<string>();
            foreach (string line in lines)
            {
                if (line.StartsWith("[Skin Settings]"))
                {
                    skinSettingsSection = true;
                    continue;
                }

                if (skinSettingsSection)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (line.StartsWith(""))
                        {
                            _noteTexture.Add(_content.Load<Texture2D>(line));
                        }
                    }

                }

            }
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

        }

        public override void PostUpdate(GameTime gameTime)
        {

        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
