using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using System;
using System.Collections.Generic;

namespace NetRadio.ViewModels
{
    class Category : IEquatable<Category>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public override bool Equals(object o)
        {
            var c = o as Category;
            return c != null && Id == c.Id;
        }
        public bool Equals(Category c)
        {
            if (c == null) return false;
            return (this.Id.Equals(c.Id));
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public override string ToString()
        {
            return Title;
        }
    }

    class JsonStream
    {
        [JsonProperty("stream")]
        public string Url { get; set; }

        [JsonProperty("bitrate")]
        public int? Bitrate { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("listeners")]
        public int Listeners { get; set; }
    }

    class State
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("region")]
        public string Continent { get; set; }

        public override string ToString()
        {
            return string.Format("code:{0} country:{1}", CountryCode, Name);
        }
    }

    class ProgramProps : ViewModelBase, IComparable<ProgramProps>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country")]
        public string CountryCode { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("streams")]
        public List<JsonStream> Streams { get; set; }

        public int CompareTo(ProgramProps other)
        {
            return CountryCode.CompareTo(other.CountryCode);
        }

        private JsonStream currentStream;
        public JsonStream CurrentStream {
            get { return currentStream; }
            set { SetProperty<JsonStream>(ref currentStream, value); }
        }

        public override string ToString()
        {
            string cats = string.Empty;
            foreach (Category c in Categories)
                cats += c.ToString() + ", ";
            return string.Format("Name:{0} country_code:{1} categories:{2}", Name, CountryCode, cats);
        }
    }

    static class JsonHelper
    {
        static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.
            GetCurrentMethod().DeclaringType);

        public static int EntriesPerPage = 20;

        static string jsonPath = Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location) + @"\Resources\";

        public static List<Category> Categories { get; private set; }

        public static List<State> States { get; private set; }

        static JsonHelper()
        {
            Categories = new List<Category>();
            if (File.Exists(jsonPath + "categories.json"))
            {
                var categories = GetEntries<Category>("categories.json");
                if (categories != null)
                    Categories.AddRange(categories);
            }
            States = new List<State>();
            if (File.Exists(jsonPath + "countries.json"))
            {
                var countries = GetEntries<State>("countries.json");
                if (countries != null)
                    States.AddRange(countries);
            }
        }

        static string ReadJsonFile(string filename)
        {
            try
            {
                using (StreamReader r = new StreamReader(jsonPath + filename))
                {
                    return r.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("{0}\n{1}", e.Message, e.StackTrace);
                return null;
            }
        }

        public static IEnumerable<T> GetEntries<T>(string filename)
        {
            IEnumerable<T> res = null;
            string content = ReadJsonFile(filename);
            if (!string.IsNullOrEmpty(content))
            {
                res = JsonConvert.DeserializeObject<IEnumerable<T>>(content);
            }
            return res;
        }

        static string ReadDirbleData(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} - {1}", e.Message, e.StackTrace);
                return null;
            }
        }

        public static List<ProgramProps> GetDownloadedStations(string filename, string country = "", string category = "",string filter="")
        {
            var stations = GetEntries<ProgramProps>(filename);
            if (stations != null)
            {
                if (!string.IsNullOrEmpty(country))
                {
                    State state = States.Find(s => s.Name == country);
                    if (state != null)
                        stations = stations.Where(i => i.CountryCode == state.CountryCode);
                }
                if (!string.IsNullOrEmpty(category))
                {
                    stations = stations.Where(i => i.Categories.Exists(c => c.Title == category));
                }
                if (!string.IsNullOrEmpty(filter))
                {
                    stations = stations.Where(i => i.Name.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase) || i.Name.ToLower().Contains(filter.ToLower()));
                }
                foreach (var item in stations)
                {
                    item.CurrentStream = item.Streams[0];
                }
                return stations.ToList();

            }
            return new List<ProgramProps>();
        }

        public static List<string> GetExistingStates(string filename)
        {
            var states = new List<string>();
            var q = GetEntries<ProgramProps>(filename).GroupBy(i => i.CountryCode);
            if (q != null)
            {
                if (q.Count() > 1)
                    states.Add("All countries");
                foreach (var g in q)
                {
                    states.Add(States.Find(i => i.CountryCode == g.Key).Name);
                }
            }
            return states;
        }

        public static List<string> GetExistingCategories(string filename)
        {
            var cats = new Dictionary<string, string>();
            var res = new List<string>();
            res.Add("All categories");
            foreach (var p in GetEntries<ProgramProps>(filename))
            {
                foreach (Category c in p.Categories)
                {
                    if (!cats.ContainsKey(c.Id))
                        cats.Add(c.Id, c.Title);
                }
            }
            res.AddRange(cats.Values.ToList());
            return res;
        }

        static void AddStations(string key, string country = "", string category = "", string preurl = "http://api.dirble.com/v2/", string filename = "data.json")
        {
            string url = preurl;
            string countrycode = string.Empty;
            if (!string.IsNullOrEmpty(country))
            {
                var v = States.First(i => i.Name == country);
                if (v != null)
                {
                    countrycode = v.CountryCode;
                    url += "countries/" + countrycode + "/";
                }

            }
            else if (!string.IsNullOrEmpty(category))
            {
                var c = Categories.First(i => i.Title == category);
                if (c != null)
                {
                    string id = c.Id;
                    url += "category/" + id + "/";
                }
            }
            var stations = GetDownloadedStations(filename, countrycode);
            int page = stations.Count() / EntriesPerPage + 1;
            url += string.Format("stations?page={0}&token={1}", page, key);

            Console.WriteLine("Url:{0}", url);
            string content = ReadDirbleData(url);   // to errors 404 not found auswerten!
            if (!string.IsNullOrEmpty(content))
            {
                List<ProgramProps> list = JsonConvert.DeserializeObject<List<ProgramProps>>(content);
                stations.AddRange(list);
                string sum = JsonConvert.SerializeObject(stations);


                using (StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.Create)))
                {
                    writer.Write(sum);
                }
            }
        }

        static void LoadItem(string filename, string url)
        {
            string content = ReadDirbleData(url);
            if (!string.IsNullOrEmpty(content))
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(filename, FileMode.Create)))
                {
                    writer.Write(content);
                }
            }
        }
    }
}
