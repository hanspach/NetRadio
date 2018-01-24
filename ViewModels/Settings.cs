using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NetRadio.ViewModels
{
    class FavoriteItem : System.IEquatable<FavoriteItem>
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public FavoriteItem(string name="", string url ="")
        {
            Name = name;
            Url = url;
        }

        public bool Equals(FavoriteItem other)
        {
            return Name.Equals(other.Name) && Url.Equals(other.Url);
        }
    }

    static class Settings
    {
        public readonly static string ResourcePath = Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + @"\Resources\";

        public readonly static string settingsPath = Path.GetDirectoryName(
                  Assembly.GetExecutingAssembly().Location) + @"\app.ini";

        private static IDictionary readProps = new Hashtable();

        public static IDictionary Properties { get; set; }

        static Settings()
        {
            string[] tokens;
            Properties = new Hashtable();
            if (File.Exists(settingsPath))
            {
                foreach (string line in File.ReadAllLines(settingsPath))
                {
                    tokens = line.Split('=');
                    if (tokens.Length > 1)
                    {
                        string key = tokens[0].TrimEnd();
                        string val = tokens[1].TrimStart();
                        if (key.StartsWith("Favorite"))
                        {
                            int idx = val.IndexOf(';');
                            if(idx != -1)
                            {
                                var fi = new FavoriteItem(val.Substring(0, idx), val.Substring(idx + 1));
                                readProps.Add(key, fi);        
                                Properties.Add(key, fi);
                            }
                        }
                        else
                        {
                            readProps.Add(key, val);         // to avoid unnecessary write operations
                            Properties.Add(key, val);
                        }
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            bool changed = false;
            foreach (DictionaryEntry item in Properties)
            {
                if(!readProps.Contains(item.Key) || !item.Value.Equals(readProps[item.Key]))
                {
                    changed = true;
                    break;
                }
            }
            if (changed)
            {
                using (StreamWriter sw = File.CreateText(settingsPath))
                {
                    foreach (DictionaryEntry item in Properties)
                    {
                        if(item.Value is FavoriteItem)
                        {
                            var val = item.Value as FavoriteItem;
                            sw.WriteLine(string.Format("{0}={1};{2}", item.Key, val.Name,val.Url));
                        }
                        else
                            sw.WriteLine(string.Format("{0}={1}", item.Key, item.Value));   
                    }
                }
            }
        }
    }
}
