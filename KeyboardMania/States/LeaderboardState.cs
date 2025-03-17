using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardMania.States
{
    public class LeaderboardState : State
    {
        private Texture2D _textBoxTexture;
        private int _score;
        private string _beatmapName;
        private Rectangle _textBoxRectangle;
        private string _userInput;
        private bool _isTextBoxSelected;
        private string _message;
        SpriteFont font;
        private List<Component> _components;

        public LeaderboardState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, int score, string beatmapName)
        : base(game, graphicsDevice, content)
        {
            font = content.Load<SpriteFont>("Fonts/Font");
            _score = score;
            _beatmapName = beatmapName;
            _message = "Good job, you finished the song - Enter your Username to record your score, or leave it blank to not save it!";
            _textBoxTexture = new Texture2D(graphicsDevice, 1, 1);
            _textBoxTexture.SetData(new[] { Color.White });
            _textBoxRectangle = new Rectangle(100, 150, 200, 30);
            _userInput = string.Empty;
            _isTextBoxSelected = false;
            string saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Leaderboard");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            string leaderboardFilePath = Path.Combine(saveDirectory, $"{beatmapName}.txt");
            if (!File.Exists(leaderboardFilePath))
            {
                var leaderboardFile = File.Create(leaderboardFilePath);
                File.WriteAllLines(leaderboardFilePath, new[] { $"{beatmapName}:" });
                leaderboardFile.Close();
            }
            game.Window.TextInput += TextInputHandler;
        }

        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            if (_isTextBoxSelected)
            {
                if (args.Key == Keys.Back && _userInput.Length > 0)
                {
                    _userInput = _userInput[..^1];
                }
                else if (args.Key == Keys.Enter)
                {
                    SaveScoreToLeaderboard(_score, _beatmapName);
                }
                else if (!char.IsControl(args.Character))
                {
                    _userInput += args.Character;
                }
            }
        }
        private void SaveScoreToLeaderboard(int score, string beatmapName)
        {
            string saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Leaderboard");
            string leaderboardFilePath = Path.Combine(saveDirectory, $"{_beatmapName}.txt");

            List<string> leaderboardEntries = File.ReadAllLines(leaderboardFilePath).ToList();
            leaderboardEntries.Add($"{_userInput} - {DateTime.Now} - {score}");

            List<string> sortedEntries = new List<string>(leaderboardEntries);

            for (int i = 0; i < sortedEntries.Count - 1; i++)
            {
                for (int j = 0; j < sortedEntries.Count - i - 1; j++)
                {
                    int score1 = int.Parse(sortedEntries[j].Split('-').Last().Trim());
                    int score2 = int.Parse(sortedEntries[j + 1].Split('-').Last().Trim());

                    if (score1 < score2)
                    {
                        string temp = sortedEntries[j];
                        sortedEntries[j] = sortedEntries[j + 1];
                        sortedEntries[j + 1] = temp;
                    }
                }
            }

            File.WriteAllLines(leaderboardFilePath, sortedEntries);

            _game.ChangeState(new MenuState(_game, _graphicsDevice, _content));
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed && _textBoxRectangle.Contains(mouseState.Position))
            {
                _isTextBoxSelected = true;
            }
            else if (mouseState.LeftButton == ButtonState.Pressed)
            {
                _isTextBoxSelected = false;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, _message, new Vector2(100, 100), Color.White);
            spriteBatch.Draw(_textBoxTexture, _textBoxRectangle, Color.Gray);
            spriteBatch.DrawString(font, _userInput, new Vector2(105, 155), Color.Black);
            spriteBatch.End();
        }
    }
}
