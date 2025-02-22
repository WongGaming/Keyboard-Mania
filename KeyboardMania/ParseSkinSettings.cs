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
        private List<Texture2D> _noteTextures = new List<Texture2D>();
        private List<Texture2D> _holdNoteTextures = new List<Texture2D>();
        private List<Texture2D> _lengthNoteTextures = new List<Texture2D>();
        private ContentManager _content;

        public ParseSkinSettings(ContentManager content)
        {
            _content = content;
        }

        public void ParseCurrentSettings(string settingsFilePath, string rootDirectory, ContentManager content, List<Texture2D> NoteTextures)
        {
            string skinFoldersLocation = Path.GetFullPath(Path.Combine(rootDirectory, "Content", "Skins", "NoteTextures"));
            string[] lines = File.ReadAllLines(settingsFilePath);
            bool skinSettingsSection = false;
            bool fullyParsed = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("[Skin Settings]"))
                {
                    skinSettingsSection = true;
                    continue;
                }

                if (skinSettingsSection)
                {
                    do
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                string directoryPath = Path.Combine(skinFoldersLocation, line);
                                if (Directory.Exists(directoryPath))
                                {
                                    string[] files = Directory.GetFiles(directoryPath, "*.png");
                                    foreach (string file in files)
                                    {
                                        string relativePath = Path.GetRelativePath(rootDirectory, file);
                                        relativePath = relativePath.Replace("\\", "/");
                                        relativePath = relativePath.Remove(relativePath.Length - 4);
                                        relativePath = Path.Combine("..", relativePath);

                                        if (Regex.IsMatch(file, @"^.*L\.png", RegexOptions.IgnoreCase))
                                        {
                                            _lengthNoteTextures.Add(_content.Load<Texture2D>(relativePath));
                                        }
                                        else if (Regex.IsMatch(file, @"^.*H\.png", RegexOptions.IgnoreCase))
                                        {
                                            _holdNoteTextures.Add(_content.Load<Texture2D>(relativePath));
                                        }
                                        else if (Regex.IsMatch(file, @"^.*\.png", RegexOptions.IgnoreCase))
                                        {
                                            _noteTextures.Add(_content.Load<Texture2D>(relativePath));
                                        }

                                    }
                                }
                                if (i == 3)
                                {
                                    fullyParsed = true;
                                }
                            }
                            else
                            {
                                InstantiateSettings instantiateSettings = new InstantiateSettings();
                                instantiateSettings.InitialiseSettings(settingsFilePath);
                                i = 4;
                            }
                        }
                    }
                    while (!fullyParsed);
                    skinSettingsSection = false;
                }
            }
        }
    }
}   