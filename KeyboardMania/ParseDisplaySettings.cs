using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeyboardMania
{
    internal class ParseDisplaySettings
    {
        private ContentManager _content;

        public ParseDisplaySettings(ContentManager content)
        {
            _content = content;
        }
        public void ParseLogoScaling(string settingsFilePath, ref float logoScale)
        {
            bool parsed = false;
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                if (line.Contains("logoscale"))
                {
                    string[] splitLine = line.Split('=');
                    string logoScalingValue = splitLine[1].Trim();
                    logoScale = float.Parse(logoScalingValue);
                    parsed = true;
                }
            }
            if (parsed == false)
            {
                var instantiateSettings = new InstantiateSettings();
                instantiateSettings.InitialiseDisplay(settingsFilePath);
            }
        }
        public void ParseDisplayGameplayScaling(string settingsFilePath, ref float keyScaleFactor,ref float comboScaleFactor, ref float scoreScaleFactor, ref float hitScaleFactor)
        {
            List<bool> parsed = new List<bool>();
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                if (line.Contains("keyscale"))
                {
                    string[] splitLine = line.Split('=');
                    string keyScaleFactorValue = splitLine[1].Trim();
                    keyScaleFactor = float.Parse(keyScaleFactorValue);
                    parsed.Add(true);
                }
                else if (line.Contains("comboscale"))
                {
                    string[] splitLine = line.Split('=');
                    string comboScaleFactorValue = splitLine[1].Trim();
                    comboScaleFactor = float.Parse(comboScaleFactorValue);
                    parsed.Add(true);
                }
                else if (line.Contains("scorescale"))
                {
                    string[] splitLine = line.Split('=');
                    string scoreScaleFactorValue = splitLine[1].Trim();
                    scoreScaleFactor = float.Parse(scoreScaleFactorValue);
                    parsed.Add(true);
                }
                else if (line.Contains("hitscale"))
                {
                    string[] splitLine = line.Split('=');
                    string hitScaleFactorValue = splitLine[1].Trim();
                    hitScaleFactor = float.Parse(hitScaleFactorValue);
                    parsed.Add(true);
                }
            }
                if(parsed.Count != 4)
                {
                    var instantiateSettings = new InstantiateSettings();
                    instantiateSettings.InitialiseDisplay(settingsFilePath);
                }
        }
        public void SaveNewSettings(string settingsFilePath, float logoScale, float keyScaleFactor, float comboScaleFactor, float scoreScaleFactor, float hitScaleFactor)
        {
            string file = File.ReadAllText(settingsFilePath);
            List<string> displaySettingsContentList = new List<string>();
            displaySettingsContentList.Add("[Display Settings]");
            displaySettingsContentList.Add($"logoscale = {logoScale}");
            displaySettingsContentList.Add($"keyscale = {keyScaleFactor}");
            displaySettingsContentList.Add($"comboscale = {comboScaleFactor}");
            displaySettingsContentList.Add($"scorescale = {scoreScaleFactor}");
            displaySettingsContentList.Add($"hitscale = {hitScaleFactor}\n");
            string displaySettingsContent = string.Join("\n", displaySettingsContentList);
            file = Regex.Replace(file, @"\[Display Settings\][\s\S]*?\[Gameplay Settings\]", displaySettingsContent + "[Gameplay Settings]");
            File.WriteAllText(settingsFilePath, file);

        }
    }
}
