using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace KeyboardMania
{
    internal class InstantiateSettings
    {
        public void InitialiseSettings(string settingsFilePath)
        {
            if (!File.Exists(settingsFilePath))
            {
                string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania");
                if (!Directory.Exists(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);
                }
                settingsFilePath = Path.Combine(settingsDirectory, "Settings.txt");
                File.Create(settingsFilePath).Close();
            }

            #region SkinSettingsInstantiation
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";

            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            #endregion

            #region DisplaySettingsInstantiation
            double logoscale = GetMonitorWidth() / 5120.0;
            double keyScaleFactor = GetMonitorWidth() / 1400.0;
            double comboScaleFactor = GetMonitorWidth() / 3840.0;
            double scoreScaleFactor = 2.0 * comboScaleFactor;
            double hitScaleFactor = 1.5 * comboScaleFactor;
            string displaySettingsContent;

            displaySettingsContent = "[Display Settings]\n";
            displaySettingsContent += $"logoscale = {logoscale}\n";
            displaySettingsContent += $"keyscale = {keyScaleFactor}\n";
            displaySettingsContent += $"comboscale = {comboScaleFactor}\n";
            displaySettingsContent += $"scorescale = {scoreScaleFactor}\n";
            displaySettingsContent += $"hitscale = {hitScaleFactor}\n";
            #endregion

            #region GameplaySettingsInstantiation
            string gameplaySettingsContent;
            int noteVelocity = GetMonitorWidth() / 2;
            gameplaySettingsContent = "[Gameplay Settings]\n";
            gameplaySettingsContent += $"notevelocity = {noteVelocity}\n";
            gameplaySettingsContent += "keymapping = d,f,j,k\n";
            gameplaySettingsContent += "latencyremover = 222.92825\n";
            gameplaySettingsContent += "fadeintiming = 1000\n";
            gameplaySettingsContent += "audiolatency = 0\n";
            #endregion

            string fullSettingsContent = skinSettingsContent + displaySettingsContent + gameplaySettingsContent;
            File.WriteAllText(settingsFilePath, fullSettingsContent);
        }
        public void InitialiseSkin(string settingsFilePath)
        {
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";

            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            skinSettingsContent += "Playful\n";
            string CurrentFileContent = File.ReadAllText(settingsFilePath);
            CurrentFileContent = Regex.Replace(CurrentFileContent, @"\[Skin Settings\][\s\S]*?\[Display Settings\]", skinSettingsContent + "[Display Settings]");
            File.WriteAllText(settingsFilePath, CurrentFileContent);
        }
        public void InitialiseDisplay(string settingsFilePath)
        {
            double logoscale = (double)(GetMonitorWidth() / 5120.0);
            double keyScaleFactor = (double)(GetMonitorWidth() / 1400.0);
            double comboScaleFactor = (double)(GetMonitorWidth() / 3840.0);
            double scoreScaleFactor = (double)(2.0 * comboScaleFactor);
            double hitScaleFactor = (double)(1.5 * comboScaleFactor);
            string displaySettingsContent;
            displaySettingsContent = "[Display Settings]\n";
            displaySettingsContent += $"logoscale = {logoscale}\n";
            displaySettingsContent += $"keyscale = {keyScaleFactor}\n";
            displaySettingsContent += $"comboscale = {comboScaleFactor}\n";
            displaySettingsContent += $"scorescale = {scoreScaleFactor}\n";
            displaySettingsContent += $"hitscale = {hitScaleFactor}\n";
            string CurrentFileContent = File.ReadAllText(settingsFilePath);
            CurrentFileContent = Regex.Replace(CurrentFileContent, @"\[Display Settings\][\s\S]*?\[Gameplay Settings\]", displaySettingsContent + "[Gameplay Settings]");
            File.WriteAllText(settingsFilePath, CurrentFileContent);
        }
        public void InitialiseGameplay(string settingsFilePath)
        {
            string gameplaySettingsContent;
            int noteVelocity = GetMonitorWidth() / 2;
            gameplaySettingsContent = "[Gameplay Settings]\n";
            gameplaySettingsContent += $"notevelocity = {noteVelocity}\n";
            gameplaySettingsContent += "keymapping = d,f,j,k\n";
            gameplaySettingsContent += "latencyremover = 222.92825\n";
            gameplaySettingsContent += "fadeintiming = 1000\n";
            gameplaySettingsContent += "audiolatency = 0\n";
            string CurrentFileContent = File.ReadAllText(settingsFilePath);
            CurrentFileContent = Regex.Replace(CurrentFileContent, @"\[Gameplay Settings\][\s\S]*", gameplaySettingsContent);
            File.WriteAllText(settingsFilePath, CurrentFileContent);
        }
        public int GetMonitorWidth()
        {
            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        }
        public void CheckForIncomplete(string settingsFilePath)
        {
            string[] lines = File.ReadAllLines(settingsFilePath);
            List<bool> parsed = new List<bool>();
            foreach (string line in lines)
            {
                if (line.Contains("logoscale"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("keyscale"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("comboscale"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("scorescale"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("hitscale"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("notevelocity"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("keymapping"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("latencyremover"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("fadeintiming"))
                {
                    parsed.Add(true);
                }
                else if (line.Contains("audiolatency"))
                {
                    parsed.Add(true);
                }
            }
            if (parsed.Count != 10)
            {
                InitialiseSettings(settingsFilePath);
            }
        }
    }
}
