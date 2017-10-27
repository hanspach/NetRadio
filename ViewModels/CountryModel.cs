using log4net;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml;

namespace NetRadio.ViewModels
{
    class Item : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty<string>(ref name, value); }
        }

        private string url;
        public string Url
        {
            get { return url; }
            set { SetProperty<string>(ref url, value); }
        }

        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set { SetProperty<string>(ref imagePath, value); }
        }

        // Needed because the IsSelected property of the TreeView-Control is readonly
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty<bool>(ref isSelected, value);}
        }

        private bool isNodeExpanded;
        public bool IsNodeExpanded
        {
            get { return isNodeExpanded; }
            set { SetProperty<bool>(ref isNodeExpanded, value); }
        }


        public Item Parent { get; set; }

        public Item(string name, string url, string imgPath)
        {
            Name = name;
            Url = url;
            int idx = imgPath.LastIndexOf('\\');
            if (idx != -1)
                ImagePath = imgPath;
            else
                ImagePath = Settings.ResourcePath + imgPath;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class Program : Item
    {
        public Program(string name, string url, string img) : base(name, url, img)
        {
        }
    }

    class Station : Item
    {
        public Station(string name, string url, string img) : base(name, url, img)
        {
            Programs = new ObservableCollection<Program>();
        }

        public ObservableCollection<Program> Programs { get; private set; }
    }

    class Country : Item
    {
        public Country(string name, string url, string img) : base(name,url, img)
        {
            Stations = new ObservableCollection<Station>();
        }

        public ObservableCollection<Station> Stations { get; private set; }
    }

    class RootItem : Item
    {
        public ObservableCollection<Country> Countries { get; set; }

        public RootItem() : base(string.Empty, string.Empty,string.Empty)
        {    
        }
    }

    sealed class CountryModel : ObservableCollection<Country> 
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        public RootItem RootItem { get; private set; }

        static CountryModel instance;
        public static CountryModel Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new CountryModel();
                }
                return instance;
            }
        }

        private CountryModel() 
        { 
            RootItem = new RootItem();
            try
            {
                foreach (Country country in ReadBookmarks(Settings.ResourcePath + "bookmarks.xml"))
                {
                    country.Parent = RootItem;          // needs a root
                    Add(country);
                }
                RootItem.Countries = this;
            }
            catch(Exception e)
            {
                log.ErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
            }
        }

        private IList<Country> ReadBookmarks(string path)
        {
            IList<Country> countries = new List<Country>();
            string name = string.Empty;
            string url = string.Empty;
            string img = string.Empty;

            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        url = string.Empty;
                        if (reader.Name.Equals("country"))
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "name")
                                    name = reader.Value;
                                else if (reader.Name == "image")
                                    img = reader.Value;
                            }
                            countries.Add(new Country(name, url, img));
                        }
                        else if (reader.Name.Equals("station"))
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "name")
                                    name = reader.Value;
                                else if (reader.Name == "url")
                                    url = reader.Value;
                                else if (reader.Name == "image")
                                    img = reader.Value;
                            }
                            Station station = new Station(name, url, img);
                            int pos = countries.Count > 0 ? countries.Count - 1 : 0;
                            station.Parent = countries[pos];
                            countries[pos].Stations.Add(station);
                        }
                        else if (reader.Name.Equals("bookmark"))
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "name")
                                    name = reader.Value;
                                else if (reader.Name == "url")
                                    url = reader.Value;
                                else if (reader.Name == "image")
                                    img = reader.Value;
                            }
                            int cidx = countries.Count > 0 ? countries.Count - 1 : 0;
                            int sidx = countries[cidx].Stations.Count > 0 ? countries[cidx].Stations.Count - 1 : 0;
                            Program program = new Program(name, url, img);
                            program.Parent = countries[cidx].Stations[sidx];
                            countries[cidx].Stations[sidx].Programs.Add(program);
                        }
                    }
                }
            }
            return countries;
        }

        public void WriteBookmarks(string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("bookmarks");
                foreach (Country country in this)
                {
                    writer.WriteStartElement("country");
                    writeAttributes(writer, country);
                    foreach (Station station in country.Stations)
                    {
                        writer.WriteStartElement("station");
                        writeAttributes(writer, station);
                        foreach (Program program in station.Programs)
                        {
                            writer.WriteStartElement("bookmark");
                            writeAttributes(writer, program);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void writeAttributes(XmlWriter writer, Item item)
        {
            writer.WriteAttributeString("name", item.Name);
            if (item.Url.Length > 0)
                writer.WriteAttributeString("url", item.Url);
            string img = item.ImagePath;
            int idx = img.LastIndexOf('\\');        // remove path 
            if (idx != -1)
                img = img.Substring(idx + 1);
            writer.WriteAttributeString("image", img);
        }
    }
}
