using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetRadio.ViewModels
{
    class FavoriteItem
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public FavoriteItem(string name="", string url ="")
        {
            Name = name;
            Url = url;
        }
    }

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
        public static FavoriteItem Favorite1 { get; set; }
        public static FavoriteItem Favorite2 { get; set; }
        public static FavoriteItem Favorite3 { get; set; }

        static Settings()
        {
            string[] tokens;

            if (File.Exists(settingsPath))
            {
                foreach (string line in File.ReadAllLines(settingsPath))
                {
                    tokens = line.Split('=');
                    if (tokens.Length > 1)
                    {
                        properties.Add(tokens[0].TrimEnd(), tokens[1].TrimStart());
                    }
                }
                if (properties.ContainsKey("LastVisitedUrl"))
                    LastVisitedUrl = properties["LastVisitedUrl"];
                if (properties.ContainsKey("Volume"))
                    Volume = properties["Volume"];

                Favorite1 = new FavoriteItem();
                Favorite2 = new FavoriteItem();
                Favorite3 = new FavoriteItem();
                if (properties.ContainsKey("Favorite1"))
                {
                    tokens = properties["Favorite1"].Split(';');
                    if (tokens.Length > 1)
                    {
                        Favorite1.Name = tokens[0];
                        Favorite1.Url = tokens[1];
                    }
                    else
                        Favorite1.Url = properties["Favorite1"];
                }
                if (properties.ContainsKey("Favorite2"))
                {
                    tokens = properties["Favorite2"].Split(';');
                    if (tokens.Length > 1)
                    {
                        Favorite2.Name = tokens[0];
                        Favorite2.Url = tokens[1];
                    }
                    else
                        Favorite2.Url = properties["Favorite2"];
                }
                if (properties.ContainsKey("Favorite3"))
                {
                    tokens = properties["Favorite3"].Split(';');
                    if (tokens.Length > 1)
                    {
                        Favorite3.Name = tokens[0];
                        Favorite3.Url = tokens[1];
                    }
                    else
                        Favorite3.Url = properties["Favorite3"];
                }
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
                if (Favorite1 != null)
                    sw.WriteLine(string.Format("Favorite1={0};{1}", Favorite1.Name,Favorite1.Url));
                if (Favorite2 != null)
                    sw.WriteLine(string.Format("Favorite2={0};{1}", Favorite2.Name,Favorite2.Url));
                if (Favorite3 != null)
                    sw.WriteLine(string.Format("Favorite3={0};{1}", Favorite3.Name,Favorite3.Url));
            }
        }
    }
}
