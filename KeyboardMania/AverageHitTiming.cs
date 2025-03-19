//leave commented when not testing
using Microsoft.Xna.Framework.Content;
using System.IO;
using System;

namespace KeyboardMania
{
    internal class AverageHitTiming
    {
        private ContentManager _content;
        private string averageFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania","Averages", "AverageHitTiming.txt");
        public AverageHitTiming(ContentManager content)
        {
            _content = content;
        }
        public void SaveTiming(double timing)
        {
            if (!File.Exists(averageFileLocation))
            {
                string averageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyboardMania", "Averages");
                if (!Directory.Exists(averageDirectory))
                {
                    Directory.CreateDirectory(averageDirectory);
                }
                averageFileLocation = Path.Combine(averageDirectory, "Settings.txt");
                File.Create(averageFileLocation);
            }
            File.WriteAllText(averageFileLocation, timing.ToString());
        }
        public void LoadTiming(ref double timing)
        {
            if (File.Exists(averageFileLocation) && !string.IsNullOrEmpty(File.ReadAllText(averageFileLocation)))
            {
                double.TryParse(File.ReadAllText(averageFileLocation), out timing);
            }
            else
            {
                timing = 222.92825;
            }
        }
    }
}