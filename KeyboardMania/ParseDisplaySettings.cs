using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public void ParseLogoScaling(string settingsFilePath, float logoScale)
        {
            bool parsed = false;
            string[] lines = File.ReadAllLines(settingsFilePath);
            foreach (string line in lines)
            {
                if (line.Contains("logoscaling"))
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
        public void ParseDisplayGameplayScaling(string settingsFilePath, float keyScaleFactor, float comboScaleFactor, float scoreScaleFactor, float hitScaleFactor)
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
                if(parsed.Count != 4)
                {
                    var instantiateSettings = new InstantiateSettings();
                    instantiateSettings.InitialiseDisplay(settingsFilePath);
                }
            }
        }
    }
}
