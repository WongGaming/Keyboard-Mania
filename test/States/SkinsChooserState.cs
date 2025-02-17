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

namespace test.States
{
    internal class SkinsChooserState : State
    {
        private string _rootDirectory;
        private List<Texture2D> _startNoteTexture = new List<Texture2D>();
        private const int NumberOfKeys = 4;
        private Texture2D _hitFeedbackTexture;
        private int _screenWidth;
        private List<HitFeedback> _hitFeedbacks;
        private int _screenHeight;

        public SkinsChooserState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content)
        : base(game, graphicsDevice, content)
        {
            _hitFeedbackTexture = _content.Load<Texture2D>("Controls/mania-stage-light");
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;
            _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
            string[] settingsFilePath = Directory.GetFiles(_rootDirectory, "Settings.txt");
            ParseCurrentSettings(settingsFilePath[0],_rootDirectory);
            _hitFeedbacks = new List<HitFeedback>();
            /* _lnoteTexture = _content.Load<Texture2D>("Textures/lnote");
             _mlnoteTexture = _content.Load<Texture2D>("Textures/mlnote");
             _mrnoteTexture = _content.Load<Texture2D>("Textures/mrnote");
             _rnoteTexture = _content.Load<Texture2D>("Textures/rnote");*/
        }
        private void ParseCurrentSettings(string _settingsFilePath, string rootDirectory)
        {
            string defaultSkinLocation = Path.GetFullPath(Path.Combine(_rootDirectory, "Content", "Skins", "NoteTextures", "WhiteNote"));
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
                        if (!line.StartsWith(""))
                        {
                            _startNoteTexture.Add(_content.Load<Texture2D>(line));
                        }
                        else
                        {
                            _startNoteTexture.Add(_content.Load<Texture2D>("Skins/NoteTextures/WhiteNote/mania-note1"));
                        }
                    }
                    skinSettingsSection = false;
                }

            }
        }
        private void HandleKeyReleases()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            // Check each lane for key release
            for (int lane = 0; lane < NumberOfKeys; lane++)
            {
                if (keyboardState.IsKeyUp(_keyMapping[lane]) && _keysPressed[lane])
                {
                    // Key was released, add feedback and reset pressed state
                    _hitFeedbacks.Add(new HitFeedback(_hitFeedbackTexture, _keyPositions[lane], _noteVelocity));
                    _keysPressed[lane] = false;
                }
            }
        }
        private List<Vector2> CalculateKeyPositions(int numberOfKeys, float keyWidth, float bottomPositionY)
        {
            var keyPositions = new List<Vector2>();
            float totalWidth = numberOfKeys * keyWidth;
            float startX = (_screenWidth / 2) - (totalWidth / 2);

            for (int i = 0; i < NumberOfKeys; i++)
            {
                float xPosition = startX + (i * keyWidth);
                keyPositions.Add(new Vector2(xPosition, bottomPositionY));
            }

            return keyPositions;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/Font"), Convert.ToString(_comboCount), new Vector2(100, 1000), Color.Red); //bugtesting combo counter Vector2(x, 2000) for home pc, Vector2(x,1000) for laptop

            foreach (var laneNotes in _activeNotesByLane.Values)
            {
                foreach (var note in laneNotes)
                {
                    note.Draw(gameTime, spriteBatch);
                }
            }

            var keyStates = new[] { Keys.D, Keys.F, Keys.J, Keys.K };
            for (int i = 0; i < keyStates.Length; i++)
            {
                if (Keyboard.GetState().IsKeyDown(keyStates[i]))
                {
                    spriteBatch.Draw(_hitFeedbackTexture, _keyPositions[i], null, Color.White, 0f, Vector2.Zero, new Vector2(_keyScaleFactor), SpriteEffects.None, 0f);
                }
            }

            // Draw hit feedbacks first so that keys are rendered above them
            foreach (var feedback in _hitFeedbacks)
            {
                feedback.Draw(gameTime, spriteBatch, _keyScaleFactor);
            }

            foreach (var keyPosition in _keyPositions)
            {
                spriteBatch.Draw(_keyTexture, keyPosition, null, Color.White, 0f, Vector2.Zero, new Vector2(_keyScaleFactor), SpriteEffects.None, 0f);
            }

            spriteBatch.End();
        }

        public override void PostUpdate(GameTime gameTime)
        {

        }

        public override void Update(GameTime gameTime)
        {
            for (int i = _hitFeedbacks.Count - 1; i >= 0; i--)
            {
                _hitFeedbacks[i].Update(gameTime);
                if (_hitFeedbacks[i].IsOffScreen(_screenHeight))
                {
                    _hitFeedbacks.RemoveAt(i);
                }
            }

            for (int lane = 0; lane < NumberOfKeys; lane++)
            {
                var hitObjects = _hitObjectsByLane[lane];
                var activeNotes = _activeNotesByLane[lane];

                // Sort active notes by their Y-position (lower notes first)
                activeNotes.Sort((n1, n2) => n1.Position.Y.CompareTo(n2.Position.Y));

                // Generate new notes
                for (int i = hitObjects.Count - 1; i >= 0; i--)
                {
                    var hitObject = hitObjects[i];
                    double spawnTime = hitObject.StartTime - (_hitPointY / _noteVelocity * 1000);
                    var noteTexture = _noteTexture;
                    if (!activeNotes.Exists(note => note.HitObject == hitObject) && _currentTime >= spawnTime)
                    {
                        float xPosition = (_screenWidth / 2) - (_keyWidth * NumberOfKeys / 2) + (hitObject.Lane * _keyWidth);
                        var note = new Note(_content, noteTexture, hitObject.IsHeldNote)
                        {
                            Position = new Vector2(xPosition, -noteTexture.Height * _noteScaleFactor), // Start position above the screen
                            HitObject = hitObject,
                            Scale = _noteScaleFactor,
                            Velocity = new Vector2(0, _noteVelocity)
                        };
                        activeNotes.Add(note);
                        hitObjects.RemoveAt(i); // Remove the note from the hitObjects list once it has spawned
                    }
                }
                int currentNote = 0;
                // Update active notes and check for hits (starting with the closest to the hit point)
                for (int i = activeNotes.Count - 1; i >= 0; i--)
                {
                    firstNotePress = true;
                    var note = activeNotes[i];
                    note.Update(gameTime);
                    // Check if note is hit
                    if (CheckForHit(note, _hitPointY, lane) && !note.HitObject.IsHeldNote && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount++; // Increase combo count, FOR SINGLE NOTES COMMENT TO CHECK IF HITS REGISTERED
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                    }
                    else if (note.IsOffScreen(_screenHeight) && !note.HitObject.IsHeldNote && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF SINGLE NOTE IS MISSED (OFFSCREEN)
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                    }
                }
            }
            HandleKeyReleases();
        }
    }
}
