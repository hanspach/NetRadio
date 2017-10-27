using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetRadio.ViewModels
{
    static class Settings
    {
        public readonly static string ResourcePath = Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + @"\Resources\";

        private readonly static string settingsPath = Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + @"\app.ini";

        private static Dictionary<string, string> properties =
            new Dictionary<string, string>();

        public static bool IsDirty { get; set; }

        public static string LastVisitedUrl { get; set; }
        public static string Volume { get; set; }
        public static string Favorite1 { get; set; }
        public static string Favorite2 { get; set; }
        public static string Favorite3 { get; set; }

        static Settings()
        {
            if (File.Exists(settingsPath))
            {
                foreach (string line in File.ReadAllLines(settingsPath))
                {
                    string[] tokens = line.Split('=');
                    if (tokens.Length > 1)
                    {
                        properties.Add(tokens[0].TrimEnd(), tokens[1].TrimStart());
                    }
                }
                if (properties.ContainsKey("LastVisitedUrl"))
                    LastVisitedUrl = properties["LastVisitedUrl"];
                if (properties.ContainsKey("Volume"))
                    Volume = properties["Volume"];
                if (properties.ContainsKey("Favorite1"))
                    Favorite1 = properties["Favorite1"];
                if (properties.ContainsKey("Favorite2"))
                    Favorite2 = properties["Favorite2"];
                if (properties.ContainsKey("Favorite3"))
                    Favorite3 = properties["Favorite3"];
            }
        }

        public static void SaveSettings()
        {
            using (StreamWriter sw = File.CreateText(settingsPath))
            {
                if (!string.IsNullOrEmpty(LastVisitedUrl))
                    sw.WriteLine(string.Format("LastVisitedUrl={0}", LastVisitedUrl));
                if (!string.IsNullOrEmpty(Volume))
                    sw.WriteLine(string.Format("Volume={0}", Volume));
                if (!string.IsNullOrEmpty(Favorite1))
                    sw.WriteLine(string.Format("Favorite1={0}", Favorite1));
                if (!string.IsNullOrEmpty(Favorite2))
                    sw.WriteLine(string.Format("Favorite2={0}", Favorite2));
                if (!string.IsNullOrEmpty(Favorite3))
                    sw.WriteLine(string.Format("Favorite3={0}", Favorite3));
            }
        }
    }
}
