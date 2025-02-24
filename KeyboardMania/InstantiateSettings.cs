﻿using System;
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
            #region SkinSettingsInstantiation
            string skinSettingsContent;
            skinSettingsContent = "[Skin Settings]\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            skinSettingsContent += "WhiteNote\n";
            #endregion

            #region DisplaySettingsInstantiation
            string displaySettingsContent;
            displaySettingsContent = "[Display Settings]\n";
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

            string CurrentFileContent = File.ReadAllText(settingsFilePath);
            CurrentFileContent = Regex.Replace(CurrentFileContent, @"\[Skin Settings\][\s\S]*?\[Display Settings\]", skinSettingsContent + "[Display Settings]");
            File.WriteAllText(settingsFilePath, CurrentFileContent);
        }
    }
}
