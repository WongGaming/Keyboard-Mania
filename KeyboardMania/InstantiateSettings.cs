using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace KeyboardMania
{
    internal class InstantiateSettings
    {
        public void InitialiseSettings(string settingsFilePath)
        {
            if (!File.Exists(settingsFilePath))
            {
                string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania");
                Directory.CreateDirectory(settingsDirectory);
                settingsFilePath = Path.Combine(settingsDirectory, "Settings.txt");
                File.Create(settingsFilePath).Dispose();
            }

            #region SkinSettingsInstantiation
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";

            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            #endregion

            #region DisplaySettingsInstantiation
            string displaySettingsContent;
            displaySettingsContent = "[Display Settings]\n";
            displaySettingsContent += "logoScale = {0}\n";
            //add when I decide what to put here
            #endregion

            #region GameplaySettingsInstantiation
            string gameplaySettingsContent;
            gameplaySettingsContent = "[Gameplay Settings]\n";
            //add when I decide what to put here
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

            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            skinSettingsContent += "Circle\n";
            string CurrentFileContent = File.ReadAllText(settingsFilePath);
            CurrentFileContent = Regex.Replace(CurrentFileContent, @"\[Skin Settings\][\s\S]*?\[Display Settings\]", skinSettingsContent + "[Display Settings]");
            File.WriteAllText(settingsFilePath, CurrentFileContent);
        }
    }
}
