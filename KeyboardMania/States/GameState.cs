//leave commented when not testing
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KeyboardMania.Controls;
using Microsoft.Win32;
using System.Reflection.Metadata;
using System.Diagnostics.SymbolStore;

namespace KeyboardMania.States
{
    public class GameState : State
    {
        private int _comboCount = 0;
        private const int NumberOfKeys = 4;
        private int totalNotes = 0;
        private Mp3Player _mp3Player;

        private int _score = 0;
        private int fadeInTiming = 200;

        private Dictionary<int, List<HitObject>> _hitObjectsByLane;
        private Dictionary<int, List<Note>> _activeNotesByLane;
        private Dictionary<int, List<Texture2D>> _activeHitTexture;
        private List<Texture2D> _numberTextures;
        private List<Texture2D> _comboTexture;
        private List<Texture2D> _scoreTexture;

        private List<HitFeedback> _hitFeedbacks; // List to track active hit feedbacks
        private Texture2D _keyTexture;
        #region NoteTextures
        private List<Texture2D> _noteTexture = new List<Texture2D>();
        private List<Texture2D> _holdTexture = new List<Texture2D>();
        private List<Texture2D> _lengthTexture = new List<Texture2D>();
        private List<string> _currentNoteTextures = new List<string>();
        #endregion

        #region HitTextures
        private List<Texture2D> _allHitTextures = new List<Texture2D>();
        private List<string> _currentHitTextures = new List<string>();
        private Texture2D _hitTexture;
        private double _endHitTextureTime;
        #endregion

        #region GameplaySettings
        private float _noteVelocity = 2000f; // pixels per second 2000f home pc 1000f laptop
        private Dictionary<int, Keys> _keyMapping; // Map lanes to keys
        private double _latencyRemover = 222.92825; // Enter the average latency experienced

        #endregion

        #region DisplaySettings
        private float _noteScaleFactor;
        private float _keyScaleFactor = 2.5f; //2.5 home pc 1f laptop 
        #endregion

        private Texture2D _hitFeedbackTexture;
        private double _currentTime;
        private int _screenWidth;
        private int _screenHeight;
        
        private string _rootDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        //work out an algorithm to calculate the scale
        
        private List<Vector2> _keyPositions;
        private float _keyWidth;
        private Dictionary<String, float> _scoreMargins = new Dictionary<String, float>();
        private string settingsFilePath;
        //REPLACE EVERYTHING ABOUT HIT MARGIN WITH _SCORE MARGIN
        private float _hitPointY; // Y position of the hit point
        private bool[] _keysPressed; // To track if a key was already pressed
        private float _overallDifficulty;
        //adjusts the actual difficulty of hitmargins
        private List<double> _hitTimings = new List<double>();
        // Track hit timings to adjust input lag
        private double _hitTimingsSum = 0;
        private double _hitTimingsAverage = 0;
        private float _audioLatency = 0;
        private int _previousScrollValue = 0; // Store the initial scroll value
        private bool firstNotePress = false;
        public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, string _osuFilePath, string _mp3FilePath)
            : base(game, graphicsDevice, content)
        {
            _hitFeedbackTexture = _content.Load<Texture2D>("Controls/mania-stage-light");
            _keyTexture = _content.Load<Texture2D>("Controls/mania-key1");
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Settings.txt");
            var parseSkinSettings = new ParseSkinSettings(_content);
            if (!File.Exists(settingsFilePath))
            {
                var instantiateSettings = new InstantiateSettings();
                instantiateSettings.InitialiseSettings(settingsFilePath);
            }
            parseSkinSettings.ParseNoteCurrentSettings(settingsFilePath, _rootDirectory, _content, _noteTexture, _holdTexture, _lengthTexture, _currentNoteTextures);
            parseSkinSettings.ParseHitCurrentSettings(settingsFilePath, _rootDirectory, _content, _allHitTextures, _currentHitTextures);

            _mp3Player = new Mp3Player(_mp3FilePath);

            _noteScaleFactor = 100f * _keyScaleFactor / 256f;
            _keyWidth = _keyTexture.Width * _keyScaleFactor;
            float keyHeight = _keyTexture.Height * _keyScaleFactor;
            float bottomPositionY = _screenHeight - keyHeight - 10;

            _hitPointY = _screenHeight - keyHeight; //1410 on my home PC

            _keyPositions = CalculateKeyPositions(NumberOfKeys, _keyWidth, bottomPositionY);

            _hitObjectsByLane = new Dictionary<int, List<HitObject>>();
            _activeNotesByLane = new Dictionary<int, List<Note>>();
            _hitFeedbacks = new List<HitFeedback>();
            _keysPressed = new bool[NumberOfKeys]; 

            _keyMapping = new Dictionary<int, Keys>
                {
                    { 0, Keys.D },
                    { 1, Keys.F },
                    { 2, Keys.J },
                    { 3, Keys.K }
                };

            for (int i = 0; i < NumberOfKeys; i++)
            {
                _hitObjectsByLane[i] = new List<HitObject>();
                _activeNotesByLane[i] = new List<Note>();
            }

            _numberTextures = new List<Texture2D>();
            for(int i = 0; i <= 9; i++)
            {
                string currentTexture = "Controls/default-" + i;
                _numberTextures.Add(_content.Load<Texture2D>(currentTexture));
            }
            LoadBeatmap(_osuFilePath);
        }

        private void LoadBeatmap(string _osuFilePath)
        {
            string[] lines = File.ReadAllLines(_osuFilePath);
            bool overallDifficultySection = false;
            bool hitObjectSection = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("[Difficulty]"))
                {
                    overallDifficultySection = true;
                    continue;
                }
                if (line.StartsWith("[HitObjects]"))
                {
                    hitObjectSection = true;
                    overallDifficultySection = false;
                    continue;
                }
                if (overallDifficultySection && line.StartsWith("OverallDifficulty: "))
                {
                    string[] difficultyLine = line.Split(' ');
                    _overallDifficulty = Convert.ToSingle(difficultyLine[1]);
                    ParseScoreMargins(_scoreMargins, _overallDifficulty);
                    overallDifficultySection = false;
                }
                else if (hitObjectSection && !string.IsNullOrWhiteSpace(line))
                {
                    var hitObject = ParseHitObject(line);
                    totalNotes++;
                    _hitObjectsByLane[hitObject.Lane].Add(hitObject);
                }
            }
        }
        private void ParseScoreMargins(Dictionary<string, float> scoreMargins, float overallDifficulty)
        {
            scoreMargins.Add("300g", 16);
            scoreMargins.Add("300", 64 - 3 * overallDifficulty);
            scoreMargins.Add("200", 97 - 3 * overallDifficulty);
            scoreMargins.Add("100", 127 - 3 * overallDifficulty);
            scoreMargins.Add("50", 151 - 3 * overallDifficulty);
            scoreMargins.Add("0", 200 - 3 * overallDifficulty);
        }
        private HitObject ParseHitObject(string line)
        {
            var parts = line.Split(',');
            int x = int.Parse(parts[0]);
            double startTime = double.Parse(parts[2]) + fadeInTiming;
            int endTime = (int)Math.Floor(double.Parse(parts[5].Split(':')[0]) + fadeInTiming);

            int lane = x / 128;
            bool isHeldNote = endTime > startTime;

            var hitObject = new HitObject
            {
                Lane = lane,
                StartTime = startTime,
                EndTime = endTime,
                IsHeldNote = isHeldNote
            };

            if (isHeldNote)
            {
                hitObject.HoldDuration = endTime - startTime;
            }

            return hitObject;
        }
        public override void Update(GameTime gameTime)
        {
            // Start the song if it's the first update frame
            if (_currentTime > fadeInTiming)
            {
                _mp3Player.Play();
            }

            MouseState mouseState = Mouse.GetState();
            bool pause = false;
            int pausedGameTime = 0;
            KeyboardState keyboardState = Keyboard.GetState();
            if (!pause)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    pause = true;
                    _mp3Player.Stop();
                    pausedGameTime = gameTime.ElapsedGameTime.Milliseconds;
                }
            }
            else if (keyboardState.IsKeyDown(Keys.Space))
            {
                pause = false;
                _mp3Player.Play();
            }
            if (pause)
            {
                gameTime.Equals(pausedGameTime);
            }
            //// Track the scroll wheel value
            //int scrollValue = mouseState.ScrollWheelValue;

            //// Check if the scroll value has changed
            //if (scrollValue > _previousScrollValue)
            //{
            //    // Increase volume when scrolling up
            //    _mp3Player.Volume = Math.Min(_mp3Player.Volume + 0.05f, 1.0f); // Increment the volume, max 1.0f
            //}
            //else if (scrollValue < _previousScrollValue)
            //{
            //    // Decrease volume when scrolling down
            //    _mp3Player.Volume = Math.Max(_mp3Player.Volume - 0.05f, 0.0f); // Decrement the volume, min 0.0f
            //}

            //// Store the current scroll value for the next frame
            //_previousScrollValue = scrollValue;

            _mp3Player.Volume = 0.3f;
            _currentTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update hit feedbacks movement REPLACE THIS WITH SCALING + FADEOUT INSTEAD OF MOVEMENT (OSU MANIA STYLE)
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

                // sort active notes by their Y-position (by lower notes first)
                activeNotes.Sort((n1, n2) => n1.Position.Y.CompareTo(n2.Position.Y));

                for (int i = hitObjects.Count - 1; i >= 0; i--)
                {
                    var hitObject = hitObjects[i];
                    double spawnTime = hitObject.StartTime - (_hitPointY / _noteVelocity * 1000);
                    var noteTexture = _noteTexture;
                    if (!activeNotes.Exists(note => note.HitObject == hitObject) && _currentTime >= spawnTime)
                    {
                        if (hitObject.IsHeldNote)
                        {
                            noteTexture = _holdTexture;
                        }
                        float xPosition = (_screenWidth / 2) - (_keyWidth * NumberOfKeys / 2) + (hitObject.Lane * _keyWidth);
                        var note = new Note(_content, noteTexture, hitObject.IsHeldNote, _lengthTexture)
                        {
                            Position = new Vector2(xPosition, -noteTexture[hitObject.Lane].Height * _noteScaleFactor), //the start position above the screen
                            HitObject = hitObject,
                            Scale = _noteScaleFactor,
                            Velocity = new Vector2(0, _noteVelocity)
                        };
                        activeNotes.Add(note);
                        hitObjects.RemoveAt(i); //remove the note from the hitObjects list once it has spawned
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
                        _comboCount++; // Increase combo count, FOR SINGLE NOTES COMMENT TO CHECK IF HITS REGISTERED
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                        HandleScoreMargin(note, _hitTimings[_hitTimings.Count-1]);
                        activeNotes.RemoveAt(i);
                    }
                    else if (note.IsOffScreen(_screenHeight) && !note.HitObject.IsHeldNote && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF SINGLE NOTE IS MISSED (OFFSCREEN)
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                        _hitTexture = _allHitTextures[0];
                        _endHitTextureTime = _currentTime + 500;
                    }
                    else if (note.HitObject.IsHeldNote && _keysPressed[lane] && !note._firstPressed && keyboardState.IsKeyUp(_keyMapping[lane]) && IsLowestNoteOnScreen(note, lane) && firstNotePress == true)
                    {
                        if (Math.Abs(_hitTimings[_hitTimings.Count - 1]) < _scoreMargins["50"])
                        {
                        HandleScoreMargin(note, _hitTimings[_hitTimings.Count - 1]);
                        activeNotes.RemoveAt(i);
                        _comboCount += 1;
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                        }
                    }
                    else if (note.HitObject.IsHeldNote && note.IsHoldOffScreen(_screenHeight, note) && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF HOLD NOTE IS MISSED (OFFSCREEN) (THIS ONLY CHECKS IF THE END PASSES THE FRONT (CURRENTLY IGNORES THE FRONT)
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                        _hitTexture = _allHitTextures[0];
                        _endHitTextureTime = _currentTime + 500;
                    }
                }
            }
            HandleKeyReleases(); // Handle key releases for feedback
        }
        private bool CheckForHit(Note note, float hitPointY, int lane)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            // Check if the key was just pressed
            if (keyboardState.IsKeyDown(_keyMapping[lane]) && !_keysPressed[lane])
            {
                _keysPressed[lane] = true; // Mark key as pressed
                // Calculate the time difference
                double timeDifference = _currentTime - note.HitObject.StartTime - _latencyRemover; // Adjusted for latency

                if (Math.Abs(timeDifference) <= _scoreMargins["0"] && Math.Abs(timeDifference) > _scoreMargins["50"])
                {
                    _hitTimings.Add(timeDifference);
                    _hitTimingsSum += timeDifference;
                    _hitTimingsAverage = _hitTimingsSum / _hitTimings.Count;
                    _comboCount = -1;
                    return true;
                }
                if (Math.Abs(timeDifference) <= _scoreMargins["50"])
                {
                    _hitTimings.Add(timeDifference);
                    _hitTimingsSum += timeDifference;
                    _hitTimingsAverage = _hitTimingsSum / _hitTimings.Count;
                    return true;
                }
            }
            if (note.HitObject.IsHeldNote && _keysPressed[lane] && note._firstPressed == true)
            {
                _comboCount += 1;
                note._firstPressed = false;
                if (note._currentlyHeld == false)
                {
                    note._currentlyHeld = true;
                    note._holdStartTime = _currentTime;
                }
                if (keyboardState.IsKeyUp(_keyMapping[lane]))
                {
                    _keysPressed[lane] = false;
                    double holdDuration = note._holdStartTime - (note.HitObject.StartTime + note.HitObject.HoldDuration);
                    double endTimeDifference = _currentTime - (note._holdStartTime + note.HitObject.HoldDuration); // change the algorithm here, compare end time difference to the first initial input time

                    if (Math.Abs(holdDuration - note.HitObject.HoldDuration) <= _scoreMargins["50"] && Math.Abs(endTimeDifference) <= _scoreMargins["50"])
                    {
                        note.CompleteHold();
                        _comboCount++;
                        //HandleScoreMargin(note, endTimeDifference);
                        return true;
                    }
                    else
                    {
                        note.FailHold();
                        return false;
                    }
                }
            }

            return false;
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

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/Font"), Convert.ToString(_comboCount), new Vector2(100, 1000), Color.Red); //bugtesting combo counter Vector2(x, 2000) for home pc, Vector2(x,1000) for laptop
            spriteBatch.DrawString(_content.Load<SpriteFont>("Fonts/Font"), Convert.ToString(_score), new Vector2(100, 1200), Color.Red);
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
            if(_hitTexture != null && (_currentTime < _endHitTextureTime))
            {
                spriteBatch.Draw(_hitTexture, new Vector2(_keyPositions[2].X - _hitTexture.Width/3,(_screenHeight - (_keyTexture.Height * _keyScaleFactor))/2), null, Color.White, 0f, Vector2.Zero, new Vector2(0.5f), SpriteEffects.None, 0f);
            }

            spriteBatch.End();
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
        private bool IsLowestNoteOnScreen(Note note, int lane)
        {
            var activeNotes = _activeNotesByLane[lane];
            foreach (var activeNote in activeNotes)
            {
                if (!activeNote.HitObject.IsHeldNote && activeNote.Position.Y < note.Position.Y)
                {
                    return false;
                }
            }
            return true;
        }
        private void HandleScoreMargin(Note note, double timeDifference)
        {
            int hitValue = 0;
            int hitBonusValue = 0;
            int hitBonus = 0;
            int hitPunishment = 0;
            int maxScore = 1000000;

            if (Math.Abs(timeDifference) <= _scoreMargins["300g"])
            {
                hitValue = 320;
                hitBonusValue = 32;
                hitBonus = 2;
                _hitTexture = _allHitTextures[5];
                _endHitTextureTime = _currentTime + 500;
            }
            else if (Math.Abs(timeDifference) <= _scoreMargins["300"])
            {
                hitValue = 300;
                hitBonusValue = 32;
                hitBonus = 1;
                _hitTexture = _allHitTextures[4];
                _endHitTextureTime = _currentTime + 500;
            }
            else if (Math.Abs(timeDifference) <= _scoreMargins["200"])
            {
                hitValue = 200;
                hitBonusValue = 16;
                hitPunishment = 8;
                _hitTexture = _allHitTextures[3];
                _endHitTextureTime = _currentTime + 500;
            }
            else if (Math.Abs(timeDifference) <= _scoreMargins["100"])
            {
                hitValue = 100;
                hitBonusValue = 8;
                hitPunishment = 24;
                _hitTexture = _allHitTextures[2];
                _endHitTextureTime = _currentTime + 500;
            }
            else if (Math.Abs(timeDifference) <= _scoreMargins["50"])
            {
                hitValue = 50;
                hitBonusValue = 4;
                hitPunishment = 44;
                _hitTexture = _allHitTextures[1];
                _endHitTextureTime = _currentTime + 500;
            }
            else
            {
                hitValue = 0;
                hitBonusValue = 0;
                hitPunishment = int.MaxValue;
                _hitTexture = _allHitTextures[0];
                _endHitTextureTime = _currentTime + 500;
            }
            

            double baseScore = (maxScore * 0.5 / totalNotes) * (hitValue / 320.0);
            double bonus = Math.Clamp(hitBonus - hitPunishment, 0, 100); //clamp function is to prevent it from escaping the range 0-100
            double bonusScore = (maxScore * 0.5 / totalNotes) * (hitBonusValue * Math.Sqrt(bonus) / 320.0);

            _score += (int)(baseScore + bonusScore);
        }
    }
}
