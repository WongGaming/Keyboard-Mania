using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using System.Text.RegularExpressions;
namespace KeyboardMania
{
    internal class ParseGameplaySettings
    {
        private ContentManager _content;

        public ParseGameplaySettings(ContentManager content)
        {
            _content = content;
        }
        public void ParseGameplayValues(string settingsFilePath, ref float noteVelocity, ref Dictionary<int, Keys> keyMapping,ref double latencyRemover,ref int fadeInTiming,ref float audioLatency)
        {
            List<bool> parsed = new List<bool>();
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                if (line.Contains("notevelocity"))
                {
                    string[] splitLine = line.Split('=');
                    string noteVelocityValue = splitLine[1].Trim();
                    noteVelocity = float.Parse(noteVelocityValue);
                    parsed.Add(true);
                }
                else if (line.Contains("keymapping"))
                {
                    string[] splitLine = line.Split('=');
                    string keyMappingValue = splitLine[1].Trim();
                    string[] keyMappingValues = keyMappingValue.Split(',');
                    for (int i = 0; i < keyMappingValues.Length; i++)
                    {
                        keyMapping[i] = (Keys)Enum.Parse(typeof(Keys), keyMappingValues[i]);
                    }
                    parsed.Add(true);
                }
                else if (line.Contains("latencyremover"))
                {
                    string[] splitLine = line.Split('=');
                    string latencyRemoverValue = splitLine[1].Trim();
                    latencyRemover = double.Parse(latencyRemoverValue);
                    parsed.Add(true);
                }
                else if (line.Contains("fadeintiming"))
                {
                    string[] splitLine = line.Split('=');
                    string fadeInTimingValue = splitLine[1].Trim();
                    fadeInTiming = int.Parse(fadeInTimingValue);
                    parsed.Add(true);
                }
                else if (line.Contains("audiolatency"))
                {
                    string[] splitLine = line.Split('=');
                    string audioLatencyValue = splitLine[1].Trim();
                    audioLatency = float.Parse(audioLatencyValue);
                    parsed.Add(true);
                }

            }
        }
        public void SaveNewSettings(string settingsFilePath, float noteVelocity, Dictionary<int, Keys> keyMapping, double latencyRemover, int fadeInTiming, float audioLatency)
        {
            string file = File.ReadAllText(settingsFilePath);
            List<string> gameplaySettingsContentList = new List<string>();
            gameplaySettingsContentList.Add("[Gameplay Settings]");
            gameplaySettingsContentList.Add($"notevelocity = {noteVelocity}");
            gameplaySettingsContentList.Add($"keymapping = {keyMapping[0]},{keyMapping[1]},{keyMapping[2]},{keyMapping[3]}");
            gameplaySettingsContentList.Add($"latencyremover = {latencyRemover}");
            gameplaySettingsContentList.Add($"fadeintiming = {fadeInTiming}");
            gameplaySettingsContentList.Add($"audiolatency = {audioLatency}\n");
            string gameplaySettingsContent = string.Join("\n", gameplaySettingsContentList);
            file = Regex.Replace(file, @"\[Gameplay Settings\][\s\S]*?", gameplaySettingsContent);
            File.WriteAllText(settingsFilePath, file);
        }
    }
}
