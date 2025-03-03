using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace KeyboardMania
{
    internal class ParseSkinSettings
    {
        private ContentManager _content;

        public ParseSkinSettings(ContentManager content)
        {
            _content = content;
        }

        public void ParseNoteCurrentSettings(string settingsFilePath, string rootDirectory, ContentManager content, List<Texture2D> _noteTextures, List<Texture2D> _holdNoteTextures, List<Texture2D> _lengthNoteTextures, List<string> skinName)
        {
            string skinFoldersLocation = Path.GetFullPath(Path.Combine(rootDirectory, "Content", "Skins", "NoteTextures"));
            string[] lines = File.ReadAllLines(settingsFilePath);
            bool skinSettingsSection = false;
            bool fullyParsed = false;

            if (lines.Length == 0)
            {
                InstantiateSettings instantiateSettings = new InstantiateSettings();
                instantiateSettings.InitialiseSettings(settingsFilePath);
            }

            do
            {
                foreach (string line in lines)
                {
                    if (line.StartsWith("[Skin Settings]"))
                    {
                        skinSettingsSection = true;
                    }
                    if (skinSettingsSection)
                    {
                        string directoryPath = Path.Combine(skinFoldersLocation, line);
                        if (!string.IsNullOrWhiteSpace(line) && Directory.Exists(directoryPath))
                        {
                            string[] files = Directory.GetFiles(directoryPath, "*.png");
                            bool validFilesFound = false;

                            foreach (string file in files)
                            {
                                string relativePath = Path.GetRelativePath(rootDirectory, file);
                                relativePath = relativePath.Replace("\\", "/");
                                relativePath = relativePath.Remove(relativePath.Length - 4);
                                relativePath = Path.Combine("..", relativePath);

                                if (Regex.IsMatch(file, @"^.*L\.png", RegexOptions.IgnoreCase))
                                {
                                    _lengthNoteTextures.Add(_content.Load<Texture2D>(relativePath));
                                    validFilesFound = true;
                                }
                                else if (Regex.IsMatch(file, @"^.*H\.png", RegexOptions.IgnoreCase))
                                {
                                    _holdNoteTextures.Add(_content.Load<Texture2D>(relativePath));
                                    validFilesFound = true;
                                }
                                else if (Regex.IsMatch(file, @"^.*\.png", RegexOptions.IgnoreCase))
                                {
                                    _noteTextures.Add(_content.Load<Texture2D>(relativePath));
                                    validFilesFound = true;
                                    skinName.Add(line);
                                }
                                if (!validFilesFound)
                                {
                                    InstantiateSettings instantiateSettings = new InstantiateSettings();
                                    instantiateSettings.InitialiseSkin(settingsFilePath);
                                }
                            }
                            if (_noteTextures.Count == 4)
                            {
                                fullyParsed = true;
                                skinSettingsSection = false;
                            }
                        }
                        else if (!line.StartsWith("[Skin Settings]"))
                        {
                            InstantiateSettings instantiateSettings = new InstantiateSettings();
                            instantiateSettings.InitialiseSkin(settingsFilePath);
                            _lengthNoteTextures.Clear();
                            _holdNoteTextures.Clear();
                            _noteTextures.Clear();
                            skinName.Clear();
                            lines = File.ReadAllLines(settingsFilePath);
                        }
                    }
                }
            } while (!fullyParsed);
        }
        public void ParseHitCurrentSettings(string settingsFilePath, string rootDirectory, ContentManager content, List<Texture2D> _hitTextures, List<string> skinName)
        {
            string baseSkinFoldersLocation = Path.GetFullPath(Path.Combine(rootDirectory, "Content", "Skins", "HitTextures"));
            string[] lines = File.ReadAllLines(settingsFilePath);
            bool skinSettingsSection = false;
            bool hitTexturesSection = false;
            bool fullyParsed = false;
            int passes = 0;
            do 
            {
                foreach (string line in lines)
                {
                    if (line.StartsWith("[Skin Settings]"))
                    {
                        skinSettingsSection = true;
                    }
                    if (skinSettingsSection)
                    {
                        if (!Regex.IsMatch(line, @"^.*Note", RegexOptions.IgnoreCase) && !(line == "[Skin Settings]"))
                        {
                            hitTexturesSection = true;
                        }
                        if (hitTexturesSection)
                        {
                            string currentScore = "0";
                            if (passes == 1)
                            {
                                currentScore = "50";
                            }
                            else if (passes == 2)
                            {
                                currentScore = "100";
                            }
                            else if (passes == 3)
                            {
                                currentScore = "200";
                            }
                            else if (passes == 4)
                            {
                                currentScore = "300";
                            }
                            else if (passes == 5)
                            {
                                currentScore = "300g";
                            }
                            string skinFoldersLocation = Path.Combine(baseSkinFoldersLocation, currentScore, line);
                            if (!string.IsNullOrWhiteSpace(line) && Directory.Exists(skinFoldersLocation))
                            {
                                string[] files = Directory.GetFiles(skinFoldersLocation, "*.png");
                                bool validFilesFound = false;
                                foreach (string file in files)
                                {
                                    string relativePath = Path.GetRelativePath(rootDirectory, file);
                                    relativePath = relativePath.Replace("\\", "/");
                                    relativePath = relativePath.Remove(relativePath.Length - 4);
                                    relativePath = Path.Combine("..", relativePath);
                                    if (Regex.IsMatch(file, @"^.*\.png", RegexOptions.IgnoreCase))
                                    {
                                        _hitTextures.Add(_content.Load<Texture2D>(relativePath));
                                        validFilesFound = true;
                                        skinName.Add(line);
                                        passes = passes + 1;
                                    }
                                    if (!validFilesFound)
                                    {
                                        InstantiateSettings instantiateSettings = new InstantiateSettings();
                                        instantiateSettings.InitialiseSkin(settingsFilePath);
                                    }
                                }
                                if (_hitTextures.Count == 6)
                                {
                                    fullyParsed = true;
                                    skinSettingsSection = false;
                                    hitTexturesSection = false;
                                }
                            }
                            else if (!line.StartsWith("[Skin Settings]"))
                            {
                                InstantiateSettings instantiateSettings = new InstantiateSettings();
                                instantiateSettings.InitialiseSkin(settingsFilePath);
                                _hitTextures.Clear();
                                skinName.Clear();
                                 lines = File.ReadAllLines(settingsFilePath);
                            }
                        }
                        
                    }
                }
            } while (!fullyParsed);
        }
        public void SaveNewSettings(string settingsFilePath, List<string> _currentNoteTextures, List<string> _currentHitTextures)
        {
            string file = File.ReadAllText(settingsFilePath);
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            for (int i = 0; i < _currentNoteTextures.Count; i++)
            {
                skinSettingsContent += _currentNoteTextures[i] + "\n";
            }
            for (int i = 0; i < _currentHitTextures.Count; i++)
            {
                skinSettingsContent += _currentHitTextures[i] + "\n";
            }
            file = Regex.Replace(file, @"\[Skin Settings\][\s\S]*?\[Display Settings\]", skinSettingsContent + "[Display Settings]");
            File.WriteAllText(settingsFilePath, file);
        }
    }
}   