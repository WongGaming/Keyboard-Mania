/*using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using KeyboardMania.Controls;
using Microsoft.Win32;
using System.Reflection.Metadata;

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
        private Texture2D _holdHeadTexture;
        //private Texture2D _holdLengthTexture; possibly obsolete
        private Texture2D _hitFeedbackTexture;
        private double _currentTime;
        private int _screenWidth;
        private int _screenHeight;
        private float _noteScaleFactor;

        //work out an algorithm to calculate the scale
        private float _keyScaleFactor = 2.5f; //2.5 home pc 1f laptop 
        private List<Vector2> _keyPositions;
        private const int NumberOfKeys = 4;
        private float _keyWidth;
        private float _hitMargin;
        private float _noteVelocity = 2000f; // pixels per second 2000f home pc 1000f laptop
        private float _hitPointY; // Y position of the hit point
        private bool[] _keysPressed; // To track if a key was already pressed
        private int _comboCount = 0;
        private Dictionary<int, Keys> _keyMapping; // Map lanes to keys

        // Track hit timings to adjust input lag
        private List<double> _hitTimings = new List<double>();
        private double _hitTimingsSum = 0;
        private double _hitTimingsAverage = 0;
        private double _latencyRemover = 222.92825; // Enter the average latency experienced
        private float _audioLatency = 0;
        private bool _mp3Played = false;
        private int _previousScrollValue = 0; // Store the initial scroll value
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

            _hitMargin = 2000f; // Example hit margin, adjust as needed

            LoadBeatmap(_osuFilePath);
        }

        private void LoadBeatmap(string _osuFilePath)
        {
            string[] lines = File.ReadAllLines(_osuFilePath);
            bool hitObjectSection = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("[HitObjects]"))
                {
                    hitObjectSection = true;
                    continue;
                }

                if (hitObjectSection && !string.IsNullOrWhiteSpace(line))
                {
                    var hitObject = ParseHitObject(line);
                    _hitObjectsByLane[hitObject.Lane].Add(hitObject);
                }
            }
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
                //ParseHoldNoteSegments(hitObject,lane); 
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

                // Update active notes and check for hits (starting with the closest to the hit point)
                for (int i = activeNotes.Count - 1; i >= 0; i--)
                {
                    var note = activeNotes[i];
                    note.Update(gameTime);

                    // Check if note is hit
                    if (CheckForHit(note, _hitPointY, lane) && !note.HitObject.IsHeldNote)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount++; // Increase combo count, FOR SINGLE NOTES COMMENT TO CHECK IF HITS REGISTERED

                    }
                    else if (note.IsOffScreen(_screenHeight) && !note.HitObject.IsHeldNote)
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF SINGLE NOTE IS MISSED (OFFSCREEN)

                    }
                    else if (note.HitObject.IsHeldNote && note.IsHoldOffScreen(_screenHeight, note))
                    {
                        activeNotes.RemoveAt(i);
                        _comboCount = 0; // Reset combo count, IF HOLD NOTE IS MISSED (OFFSCREEN) (THIS ONLY CHECKS IF THE END PASSES THE FRONT (CURRENTLY IGNORES THE FRONT)
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

                // Debug: Output hit detection information
                //Console.WriteLine($"Checking hit: Lane={lane}, TimeDiff={timeDifference}");

                // Check if within hit margin based only on time
                if (Math.Abs(timeDifference) <= _hitMargin)
                {
                    // Hit registered
                    _hitTimings.Add(timeDifference);
                    _hitTimingsSum += timeDifference;
                    _hitTimingsAverage = _hitTimingsSum / _hitTimings.Count;

                    if (note.HitObject.IsHeldNote)
                    {
                        note.StartHolding(_currentTime);
                    }
                    return true;
                }
            }
            if (note.HitObject.IsHeldNote && _keysPressed[lane])
            {
                if (keyboardState.IsKeyUp(_keyMapping[lane]))
                {
                    _keysPressed[lane] = false;
                    //    if(!IsHoldComplete(_currentTime, note.HitObject.EndTime, _hitMargin) )
                    //    {
                    //           note.FailHold();
                    //            return false;
                    //    }
                    //    }
                    //    if (IsHoldComplete(_currentTime, note.HitObject.EndTime, _hitMargin))
                    //    {
                    //        note.CompleteHold();
                    //        return true;
                }
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
    }

    public class HitObject
    {
        public int Lane { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public bool IsHeldNote { get; set; }
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
        private bool _currentlyHeld = false; //track if note is being hit at the moment
        private double _holdStartTime;
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
            Console.WriteLine("Hold note completed!");
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

            //BELOW LIES TROUBLE NOT FIXED HELP
            Vector2 finalPosition = new Vector2(Position.X, Position.Y - (segments * _holdLengthTexture.Height * Scale) - _texture.Height * Scale); //FIX THIS ASAP IAM GOING INSANE
            //Vector2 finalPosition = new Vector2(Position.X, Position.Y - (segments * _texture.Height * Scale)); //old finalPosition calculator, finds the final value by considering _texture.Height
            Vector2 headPosition = new Vector2(Position.X, Position.Y - (_texture.Height * Scale) + 2 * _holdLengthTexture.Height);

            spriteBatch.Draw(_texture, headPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            for (int i = 2; i <= segments * 2 + 1; i++)
            {
                //Vector2 segmentPosition = new Vector2(Position.X, (Position.Y - (_texture.Height + (i * _holdLengthTexture.Height * Scale))) / 2);

                segmentPosition = new Vector2(Position.X, Position.Y - ((_texture.Height * Scale) + ((i - 1) * (_holdLengthTexture.Height) * Scale)) + 3 * _holdLengthTexture.Height * Scale); //works
                //spriteBatch.Draw(_holdLengthTexture, segmentPosition, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

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
            float noteHeight = _texture.Height * Scale;
            float totalHeight = Convert.ToSingle((HitObject.EndTime - HitObject.StartTime) / 1000.0) * Velocity.Y;
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
}*/