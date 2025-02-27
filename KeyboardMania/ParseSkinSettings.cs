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

        public void ParseCurrentSettings(string settingsFilePath, string rootDirectory, ContentManager content, List<Texture2D> _noteTextures, List<Texture2D> _holdNoteTextures, List<Texture2D> _lengthNoteTextures, List<string> skinName)
        {
            string skinFoldersLocation = Path.GetFullPath(Path.Combine(rootDirectory, "Content", "Skins", "NoteTextures"));
            string[] lines = File.ReadAllLines(settingsFilePath);
            bool skinSettingsSection = false;
            bool fullyParsed = false;
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
                        else if(!line.StartsWith("[Skin Settings]"))
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
            } while (!fullyParsed) ;
        }
        public void SaveNewSettings(string settingsFilePath, List<string> _currentTextures)
        {
            string file = File.ReadAllText(settingsFilePath);
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            for (int i = 0; i < _currentTextures.Count; i++)
            {
                skinSettingsContent += _currentTextures[i] + "\n";
            }
            file = Regex.Replace(file, @"\[Skin Settings\][\s\S]*?\[Display Settings\]", skinSettingsContent + "[Display Settings]");
            File.WriteAllText(settingsFilePath, file);
        }
    }
}   