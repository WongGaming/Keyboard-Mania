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
        private Mp3Player _mp3Player;
        private Dictionary<int, List<HitObject>> _hitObjectsByLane;
        private Dictionary<int, List<Note>> _activeNotesByLane;
        private List<HitFeedback> _hitFeedbacks; // List to track active hit feedbacks
        private Texture2D _keyTexture;
        private Texture2D _noteTexture;
        //prepare the textures for loading, CHANGE THIS INTO SETTINGS LATER
        private Texture2D maniaHit0;
        private Texture2D maniaHit50;
        private Texture2D maniaHit100;
        private Texture2D maniaHit200;
        private Texture2D maniaHit300;
        private Texture2D maniaHitMax;
        private Texture2D _holdHeadTexture;
        //private Texture2D _holdLengthTexture; possibly obsolete
        private Texture2D _hitFeedbackTexture;
        private double _currentTime;
        private int _screenWidth;
        private int _screenHeight;
        private float _noteScaleFactor;

        //work out an algorithm to calculate the scale
        private float _keyScaleFactor = 1f; //2.5 home pc 1f laptop 
        private List<Vector2> _keyPositions;
        private const int NumberOfKeys = 4;
        private float _keyWidth;

        private float _missMargin;
        //The range that the player can hit the notes, if the player hits within this range but not the other range, the note is counted as a miss (allows ghost tapping)
        private Dictionary<String,float> _scoreMargins = new Dictionary<String, float>();

        //REPLACE EVERYTHING ABOUT HIT MARGIN WITH _SCORE MARGIN
        private float _noteVelocity = 2000f; // pixels per second 2000f home pc 1000f laptop
        private float _hitPointY; // Y position of the hit point
        private bool[] _keysPressed; // To track if a key was already pressed
        private int _comboCount = 0;
        private Dictionary<int, Keys> _keyMapping; // Map lanes to keys
        private float _overallDifficulty;
        //adjusts the actual difficulty of hitmargins
        private List<double> _hitTimings = new List<double>();
        // Track hit timings to adjust input lag
        private double _hitTimingsSum = 0;
        private double _hitTimingsAverage = 0;
        private double _latencyRemover = 222.92825; // Enter the average latency experienced
        private float _audioLatency = 0;
        private bool _mp3Played = false;
        private int _previousScrollValue = 0; // Store the initial scroll value
        private bool firstNotePress = false;
        //possible fix to hold notes?
        public GameState(Game1 game, GraphicsDevice graphicsDevice, ContentManager content, string _osuFilePath, string _mp3FilePath)
            : base(game, graphicsDevice, content)
        {
            //don't delete these like before!!
            _noteTexture = _content.Load<Texture2D>("Controls/mania-note1");
            _keyTexture = _content.Load<Texture2D>("Controls/mania-key1");
            _holdHeadTexture = _content.Load<Texture2D>("Controls/mania-note1H");
            //_holdLengthTexture = _content.Load<Texture2D>("Controls/mania-note1L");
            _hitFeedbackTexture = _content.Load<Texture2D>("Controls/mania-stage-light");
            _screenWidth = graphicsDevice.Viewport.Width;
            _screenHeight = graphicsDevice.Viewport.Height;

            //below filepaths are for bugtesting, uncomment them to test the game with specific files
            //_osuFilePath = @"C:\Users\kong3\source\repos\Keyboard-Mania\test\Beatmaps\M2U - Mare Maris\M2U - Mare Maris (Raveille) [EXPERT].osu";
            //_mp3FilePath = @"C:\Users\kong3\source\repos\Keyboard-Mania\test\Beatmaps\M2U - Mare Maris\maremaris.mp3";
            _mp3Player = new Mp3Player(_mp3FilePath);

            _noteScaleFactor = 100f * _keyScaleFactor / 256f;
            _keyWidth = _keyTexture.Width * _keyScaleFactor;
            float keyHeight = _keyTexture.Height * _keyScaleFactor;
            float bottomPositionY = _screenHeight - keyHeight - 10;

            _hitPointY = _screenHeight - keyHeight; //1410 on my home PC

            _keyPositions = CalculateKeyPositions(NumberOfKeys, _keyWidth, bottomPositionY);

            _hitObjectsByLane = new Dictionary<int, List<HitObject>>();
            _activeNotesByLane = new Dictionary<int, List<Note>>();
            _hitFeedbacks = new List<HitFeedback>(); // Initialize hit feedbacks list
            _keysPressed = new bool[NumberOfKeys]; // Initialize key pressed states

            // Initialize key mapping
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

            _missMargin = 2000f; // Example hit margin, adjust as needed, SHIFT TOWARDS THE SCORE MARGINS

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
                if(overallDifficultySection && line.StartsWith("OverallDifficulty: "))
                {
                    string[] difficultyLine = line.Split(' ');
                    _overallDifficulty = Convert.ToSingle(difficultyLine[1]);
                 ParseScoreMargins(_scoreMargins, _overallDifficulty);
                    overallDifficultySection = false;
                }
                else if (hitObjectSection && !string.IsNullOrWhiteSpace(line))
                {
                    var hitObject = ParseHitObject(line);
                    _hitObjectsByLane[hitObject.Lane].Add(hitObject);
                }
            }
        }
        private void ParseScoreMargins(Dictionary<string, float> scoreMargins, float overallDifficulty)
        {
            scoreMargins.Add("MAX", 16);
            scoreMargins.Add("300", 64 -  3*overallDifficulty);
            scoreMargins.Add("200", 97 - 3 * overallDifficulty);
            scoreMargins.Add("100", 127 - 3 * overallDifficulty);
            scoreMargins.Add("50", 151 - 3 * overallDifficulty);
        }
        private HitObject ParseHitObject(string line)
        {
            var parts = line.Split(',');
            int x = int.Parse(parts[0]);
            double startTime = double.Parse(parts[2]);
            int endTime = int.Parse(parts[5].Split(':')[0]);

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
        private void ParseHoldNoteSegments(HitObject hitObject, int lane)
        {
            double segmentDuration = 100; // Milliseconds between each segment
            double currentTime = hitObject.StartTime;

            while (currentTime < hitObject.EndTime)
            {
                var segment = new HitObject
                {
                    Lane = lane,
                    StartTime = currentTime,
                    EndTime = currentTime,
                    IsHeldNote = true
                };
                _hitObjectsByLane[lane].Add(segment);
                currentTime += segmentDuration;
            }
        }
        public override void Update(GameTime gameTime)
        {
            // Start the song if it's the first update frame
            if (_currentTime == 0)
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
                        if (hitObject.IsHeldNote)
                        {
                            noteTexture = _holdHeadTexture;
                        }
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
                    else if (note.HitObject.IsHeldNote && _keysPressed[lane] && !note._firstPressed && keyboardState.IsKeyUp(_keyMapping[lane]) && IsLowestNoteOnScreen(note, lane) && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount += 1;
                        currentNote = currentNote + 1;
                        firstNotePress = false;
                    }
                    else if (note.HitObject.IsHeldNote && note.IsHoldOffScreen(_screenHeight, note) && firstNotePress == true)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF HOLD NOTE IS MISSED (OFFSCREEN) (THIS ONLY CHECKS IF THE END PASSES THE FRONT (CURRENTLY IGNORES THE FRONT)
                        currentNote = currentNote + 1;
                        firstNotePress = false;
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

                // Check if within hit margin based only on time
                if (Math.Abs(timeDifference) <= _missMargin)
                {
                    // Hit registered (mainly for debugging)
                    _hitTimings.Add(timeDifference);
                    _hitTimingsSum += timeDifference;
                    _hitTimingsAverage = _hitTimingsSum / _hitTimings.Count;

                   /* if (note.HitObject.IsHeldNote)
                    {
                        note.StartHolding(_currentTime);
                    }*/
                    return true;
                }
            }
            if (note.HitObject.IsHeldNote && _keysPressed[lane] && note._firstPressed == true)
            {
                _comboCount += 1;
                note._firstPressed = false;
                //make a new boolean - initial that checks if this is the note's first time being hit
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

                    if (Math.Abs(holdDuration - note.HitObject.HoldDuration) <= _missMargin && Math.Abs(endTimeDifference) <= _missMargin)
                    {
                        note.CompleteHold();
                        _comboCount++; // Increment the combo count by one for hitting the note
                        return true;
                    }
                    else
                    {
                        note.FailHold();
                        return false;
                    }
                }
                /*else
                {
                    // Increment combo count for every 30 refreshes the note is held (this value may need to be changed if the screensize is different (velocity calculator is different)
                    if (Convert.ToInt32(_currentTime) % 30 == 0)
                    {
                        _comboCount++;
                    }

                }*/
                
                //the above code was to count the combo per every tick. this will change to be counting one hit + one release due to player feedback
            }

            return false;
        }
        private bool IsHoldComplete(double currentTime, int holdEndTime, float hitMargin)
        {
            if (currentTime == holdEndTime)
            {
                return true;
            }
            else if (currentTime > holdEndTime)
            {
                if ((currentTime - hitMargin) <= holdEndTime)
                {
                    return true;
                }
            }
            else if (currentTime < holdEndTime)
            {
                if ((currentTime + hitMargin) >= holdEndTime)
                {
                    return true;
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
    }

    public class HitObject
    {
        public int Lane { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public bool IsHeldNote { get; set; }
        public double HoldDuration { get; set; }
    }

    public class Note : Component
    {
        public HitObject HitObject { get; set; }
        public Texture2D Texture => _texture; // Expose texture to access height for hit calculation
        private Texture2D _texture;
        //private Texture2D 
        private Texture2D _holdLengthTexture;
        public Vector2 Position;
        public Vector2 Velocity;
        private bool _isHeld;
        public bool _currentlyHeld = false; //track if note is being hit at the moment
        public double _holdStartTime;
        public bool _firstPressed = true;
        public float Scale { get; set; } = 1f;

        public Note(ContentManager content, Texture2D texture, bool isHeld, string holdTexturePath = "Controls/mania-note1L") //debug sets it to default white hold length
        {
            _texture = texture;
            _isHeld = isHeld;
            if (isHeld)
            {
                _holdLengthTexture = content.Load<Texture2D>(holdTexturePath);
            }
        }
        public void StartHolding(double currentTime)
        {
            _isHeld = true;
            _holdStartTime = currentTime;
        }

        public void FailHold()
        {
            _isHeld = false;
            Console.WriteLine("Hold note failed!");
        }

        public void CompleteHold()
        {
            _isHeld = false;
            Console.WriteLine("Hold note successful!");
        }

        public override void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture != null)
            {
                if (!_isHeld)
                {
                    spriteBatch.Draw(_texture, Position, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
                else
                {
                    DrawHoldNoteSegments(spriteBatch);
                }
            }
            else
            {
                throw new InvalidOperationException("Note has null texture");
            }
        }
        private void DrawHoldNoteSegments(SpriteBatch spriteBatch)
        {
            int segments;
            Vector2 segmentPosition;
            float totalHeight = Convert.ToSingle((HitObject.EndTime - HitObject.StartTime) / 1000.0) * Velocity.Y; //using speed = distance / time, distance - time * speed
            //segments = Convert.ToInt32(Math.Ceiling((totalHeight - (2 * _texture.Height) )/ (_holdLengthTexture.Height * Scale))); //2.0 version, caused error when segments < 1 [GIVES TOO MANY]
            segments = Convert.ToInt32(totalHeight / (_texture.Height * Scale)); //1.0 version, used for old segment position tests (remove l8r) [SEEMS TO WORK NOW]
            if (segments < 1)
            {
                segments = 1;
            }
            //Position = new Vector2(xPosition, -noteTexture.Height * _noteScaleFactor), // Start position above the screen

            Vector2 finalPosition = new Vector2(Position.X, Position.Y - (2 * segments * _holdLengthTexture.Height * Scale) - 2 * _texture.Height * Scale); //FIX THIS ASAP IAM GOING INSANE
            //Vector2 finalPosition = new Vector2(Position.X, Position.Y - (segments * _texture.Height * Scale)); //old finalPosition calculator, finds the final value by considering _texture.Height
            Vector2 headPosition = new Vector2(Position.X, Position.Y - (_texture.Height * Scale) + 2 * _holdLengthTexture.Height * Scale);

            spriteBatch.Draw(_texture, headPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            for (int i = 2; i <= (segments + 1) * 2 + 2; i++) //segments on my laptop seems to be (segments+1) * 2 + 2, on home computer = 
            {
                //Vector2 segmentPosition = new Vector2(Position.X, (Position.Y - (_texture.Height + (i * _holdLengthTexture.Height * Scale))) / 2);

                segmentPosition = new Vector2(Position.X, Position.Y - ((_texture.Height * Scale) + ((i - 1) * (_holdLengthTexture.Height) * Scale)) + 3 * _holdLengthTexture.Height * Scale); //works
                spriteBatch.Draw(_holdLengthTexture, segmentPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

                //spriteBatch.Draw(_texture, segmentPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f); //old, causes strange tearing effect on the note
            }
            //segmentPosition = new Vector2(Position.X, Position.Y - ((_texture.Height * Scale) + ((segments + 2) * (_holdLengthTexture.Height) * Scale)) + 3 * _holdLengthTexture.Height * Scale);
            //spriteBatch.Draw(_holdLengthTexture, segmentPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            DrawEndTexture(spriteBatch, finalPosition);
        }
        private void DrawEndTexture(SpriteBatch spriteBatch, Vector2 finalPositon)
        {
            Vector2 originOfTexture = new Vector2(_texture.Width, _texture.Height);
            spriteBatch.Draw(_texture, finalPositon, null, Color.White, MathHelper.Pi, originOfTexture, Scale, SpriteEffects.None, 0f);
        }
        public bool IsOffScreen(int screenHeight)
        {
            float noteHeight = _texture.Height * Scale;
            return Position.Y > screenHeight + noteHeight;
        }
        public bool IsHoldOffScreen(int screenHeight, Note note)
        {
            //USES THE SAME ALGORITHMS AS THE DRAW HOLD NOTES - TRY TO REMOVE REDUNDANCY!!
            float totalHeight = Convert.ToSingle((HitObject.EndTime - HitObject.StartTime) / 1000.0) * Velocity.Y;
            //Vector2 finalPosition = new Vector2(Position.X, Position.Y - (2 * segments * _holdLengthTexture.Height * Scale) - 2 * _texture.Height * Scale);
            return Position.Y > screenHeight + totalHeight;
        }
    }

    public class HitFeedback
    {
        private Texture2D _texture;
        public Vector2 Position;
        private float _velocity;

        public HitFeedback(Texture2D texture, Vector2 startPosition, float velocity)
        {
            _texture = texture;
            Position = startPosition;
            _velocity = velocity;
        }

        public void Update(GameTime gameTime)
        {
            Position.Y += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds; //change this to instead change the vertical width to shrink + fadeout (saves more processor + how osu mania does it)
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float keyScaleFactor)
        { 
            spriteBatch.Draw(_texture, Position, null, Color.White, 0f, Vector2.Zero, keyScaleFactor, SpriteEffects.None, 0f);
        }

        public bool IsOffScreen(int screenHeight)
        {
            return Position.Y > screenHeight;
        }
    }
}
